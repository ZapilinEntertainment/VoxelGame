using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum Difficulty{Utopia, Easy, Normal, Hard, Torture}
public enum GameStart {Nothing, Zeppelin, Headquarters}
public enum WorkType {Nothing, Digging, Pouring, Manufacturing, Clearing, Gathering, Mining}
public enum GUIStyles {RightOrientedLabel, BorderlessButton, BorderlessLabel, CenterOrientedLabel}

public class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}
	public float newGameSpeed = 1;

	public Transform camTransform, camBasis;
	Vector3 camLookPoint; bool moveCamToLookPoint = false;
	List<GameObject> cameraUpdateBroadcast;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cameraTimer =0, cameraUpdateTime = 0.04f;
	int camCullingMask = 1;
	public static Chunk mainChunk; 
	public static ColonyController colonyController; 
	public static GeologyModule geologyModule;
	public  LineRenderer systemDrawLR;
	static string path;

	public const int START_LIFEPOWER = 100000;
	public const int LIFEPOWER_SPREAD_SPEED = 10,  CRITICAL_DEPTH = - 200;
	public static float lifeGrowCoefficient {get;private set;}
	public static float demolitionLossesPercent {get;private set;}
	public const float START_HAPPINESS = 1, GEARS_ANNUAL_DEGRADE = 0.1f, LIFE_DECAY_SPEED = 0.1f, LABOUR_TICK = 1, DAY_LONG = 60, CAM_LOOK_SPEED = 10;

	public static Difficulty difficulty {get;private set;}
	public GameStart startGameWith = GameStart.Zeppelin;
	public static float LUCK_COEFFICIENT {get;private set;}

	public const int START_WORKERS_COUNT = 10;
	static float diggingSpeed = 0.1f, pouringSpeed = 0.1f, manufacturingSpeed = 0.1f, 
	clearingSpeed = 1, gatheringSpeed = 0.1f, miningSpeed = 0.05f;


	float t;
	uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;
	public List<Component> everydayUpdateList, everyYearUpdateList;

	public List <Component> windUpdateList;
	public Vector3 windVector {get; private set;}
	public float maxWindPower = 10, windTimer = 0, windChangeTime = 120;

	public static float sunlightIntensity {get; private set;}
	public Light sun;

	bool fontSize_set = false;
	public static float guiPiece {get;private set;}
	public static GUISkin mainGUISkin {get;private set;}

	public GameMaster () {
		if (realMaster != null) realMaster = null;
		realMaster = this;
	}

	void Awake() {
		gameSpeed = 1;
		cameraUpdateBroadcast = new List<GameObject>();
		path = Application.dataPath;

		everydayUpdateList = new List<Component>();
		everyYearUpdateList = new List<Component>();
		windUpdateList = new List<Component>();

		lifeGrowCoefficient = 1;
		//Localization.ChangeLanguage(Language.English);
		geologyModule = gameObject.AddComponent<GeologyModule>();
	}

	void Start() {
		if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);

		switch (difficulty) {
		case Difficulty.Utopia: LUCK_COEFFICIENT = 1;	demolitionLossesPercent = 0; break;
		case Difficulty.Easy: LUCK_COEFFICIENT = 0.7f; demolitionLossesPercent = 0.2f;  break;
		case Difficulty.Normal: LUCK_COEFFICIENT = 0.5f; demolitionLossesPercent = 0.4f;  break;
		case Difficulty.Hard: LUCK_COEFFICIENT = 0.1f; demolitionLossesPercent = 0.7f; break;
		case Difficulty.Torture: LUCK_COEFFICIENT = 0.01f; demolitionLossesPercent = 1; break;
		}

		switch (startGameWith) {
		case GameStart.Zeppelin :
			LandingUI lui = gameObject.AddComponent<LandingUI>();
			lui.lineDrawer = systemDrawLR;
			Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin")); 
			break;
		case GameStart.Headquarters : 
			int xpos = (int)(Random.value * (Chunk.CHUNK_SIZE - 1));
			int zpos = (int)(Random.value * (Chunk.CHUNK_SIZE - 1));

			colonyController = gameObject.AddComponent<ColonyController>();
			Structure s = Instantiate(Resources.Load<GameObject>("Structures/ZeppelinBasement")).GetComponent<Structure>();
			SurfaceBlock b = mainChunk.GetSurfaceBlock(xpos,zpos);
			s.SetBasement(b, new PixelPosByte(4,1));
			b.MakeIndestructible(true);
			b.basement.MakeIndestructible(true);
			mainChunk.SetAccessPoint(b.pos);

			colonyController.AddCitizens(START_WORKERS_COUNT);

			if (xpos > 0) xpos --; else xpos++;
			StorageHouse firstStorage = Instantiate(Resources.Load<GameObject>("Structures/Storage_level_0")).GetComponent<StorageHouse>();
			firstStorage.SetBasement(mainChunk.GetSurfaceBlock(xpos,zpos), PixelPosByte.one);

			UI ui = gameObject.AddComponent<UI>();
			ui.lineDrawer = systemDrawLR;
			//ui.lineDrawer.gameObject.layer = 5;
			break;
		}
	}

	void Update() {
		if (gameSpeed != newGameSpeed) gameSpeed = newGameSpeed;

		if (camTransform != null) {
			if (prevCamPos != camTransform.position || prevCamRot != Camera.main.transform.rotation) {
				cameraHasMoved = true;
				prevCamPos = camTransform.position;
				prevCamRot = camTransform.rotation;
			}
			if (cameraTimer > 0) cameraTimer-= Time.deltaTime;
			if (cameraTimer <= 0 && cameraHasMoved) { 
				int c = cameraUpdateBroadcast.Count - 1;
				while (c >= 0) {
					if (cameraUpdateBroadcast[c] == null) cameraUpdateBroadcast.RemoveAt(c);
					else cameraUpdateBroadcast[c].SendMessage("CameraUpdate", camTransform, SendMessageOptions.DontRequireReceiver);
					c--;
				}
				cameraHasMoved = false;
				cameraTimer = cameraUpdateTime;
			}
		}

		sunlightIntensity = 0.4f +Mathf.PerlinNoise(0.1f, Time.time * gameSpeed / 200f);
		sun.intensity = sunlightIntensity;

		if (gameSpeed == 0) return;
		t += Time.deltaTime * gameSpeed;

		if (t >= DAY_LONG) {
			uint daysDelta= (uint)(t / DAY_LONG);
			day += daysDelta;
			t = t % DAY_LONG;
			if (day >= DAYS_IN_WEEK) {
				week += day / DAYS_IN_WEEK;
				day = day % DAYS_IN_WEEK;
				if (week >= WEEKS_IN_MONTH) {
					month += week / WEEKS_IN_MONTH;
					week = week % WEEKS_IN_MONTH;
					if (month > MONTHS_IN_YEAR) {
						uint yearsDelta =(uint) month / MONTHS_IN_YEAR;
						year += yearsDelta;
						month = month % MONTHS_IN_YEAR;
						if (year > 1000) {
							millenium += year / 1000;
							year = year % 1000;
						}
						for (int c = 0; c < yearsDelta; c++) {
						if (everyYearUpdateList.Count > 0) {
							int i = 0;
							while (i < everyYearUpdateList.Count) {
								if (everyYearUpdateList[i] == null) {everyYearUpdateList.RemoveAt(i); continue;}
								else {
									everyYearUpdateList[i].SendMessage("EveryYearUpdate", SendMessageOptions.DontRequireReceiver);
									i++;
								}
							}
						}
					}
					}
				}
			}
			//day Update

			for (int c = 0; c < daysDelta; c++) {
			if (everydayUpdateList.Count > 0) {
				int i =0;
				while (i < everydayUpdateList.Count) {
					if (everydayUpdateList[i] == null) {everydayUpdateList.RemoveAt(i); continue;}
					else {
						everydayUpdateList[i].SendMessage("EverydayUpdate", SendMessageOptions.DontRequireReceiver);
						i++;
					}
				}
			}
		}
		//eo day update
		}

		windTimer -= Time.deltaTime * GameMaster.gameSpeed;
		if (windTimer <= 0) {
			windVector = Random.onUnitSphere * (maxWindPower * Random.value);
			windVector += Vector3.down * windVector.y;
			windTimer = windChangeTime + Random.value * windChangeTime;
			if (windUpdateList.Count != 0) {
				int i = 0;
				while (i < windUpdateList.Count) {
					Component c = windUpdateList[i];
					if (c == null) {windUpdateList.RemoveAt(i); continue;}
					else	{c.SendMessage("WindUpdate", windVector,SendMessageOptions.DontRequireReceiver); i++;}
				}
			}
		}
	}

	void LateUpdate() {
		if (moveCamToLookPoint) {
			float prevY = camBasis.transform.position.y, y = transform.position.y;
			camBasis.position = Vector3.MoveTowards(camBasis.position, camLookPoint, CAM_LOOK_SPEED * Time.deltaTime);
			if (Vector3.Distance(camBasis.position, camLookPoint) == 0) moveCamToLookPoint = false;
		}
	}

	public void AddToCameraUpdateBroadcast(GameObject g) {
		if (g != null) cameraUpdateBroadcast.Add(g);
	}


	public static void Save2DMatrix (float[,] arr, string fileName ) {
		// Не совсем правильный вывод, поправить
		using (StreamWriter sw = File.CreateText(path + '/' + fileName+".txt")) 
		{
			for (int i = arr.GetLength(0) - 1; i >= 0; i--) {
				string s = "";
				for (int j = 0; j< arr.GetLength(1); j++) {
					string ts = arr[i,j].ToString();
					if (ts.Length > 5) ts = ts.Substring(0,5); else {
						switch (ts.Length) {
						case 1: ts += ".000"; break;
						case 3: ts +="00";break;
						}
					}
					s += ts + ' ';
				}
				sw.WriteLine(s);
			}
		}	
	}
	public static void Save2DMatrix (bool[,] arr, string fileName ) {
		// Не совсем правильный вывод, поправить
		using (StreamWriter sw = File.CreateText(path + '/' + fileName+".txt")) 
		{
			for (int i = arr.GetLength(1) - 1; i >= 0; i--) {
				string s = "";
				for (int j = 0; j< arr.GetLength(0); j++) {
					if (arr[i,j] == true) s+=" 1"; else s+=" 0";
					}
				sw.WriteLine(s);
				}
			}
		}	

	public static float CalculateWorkflow(int workersCount, WorkType type) {
		if (colonyController == null) return 0;
		float workflow = workersCount * colonyController.labourEfficientcy_coefficient * colonyController.gears_coefficient - ( colonyController.health_coefficient + colonyController.happiness_coefficient - 2);
		switch (type) {
		case WorkType.Digging: workflow *= diggingSpeed;break;
		case WorkType.Manufacturing: workflow *= manufacturingSpeed;break;
		case WorkType.Nothing: workflow = 0; break;
		case WorkType.Pouring: workflow *= pouringSpeed;break;
		case WorkType.Clearing: workflow *= clearingSpeed;break;
		case WorkType.Gathering : workflow *= gatheringSpeed;break;
		case WorkType.Mining: workflow *= miningSpeed;break;
		}
		return workflow;
	}

	public void SetLookPoint(Vector3 point) {
		camLookPoint = point;
		// if doubletap moveCamToLookPoint = true;
	}

	void OnGUI() {
		if (!fontSize_set) {
			guiPiece = Screen.height / 24f;
			mainGUISkin = Resources.Load<GUISkin>("MainSkin");
			mainGUISkin.GetStyle("Label").fontSize = (int)(guiPiece/2f);
			mainGUISkin.GetStyle("Button").fontSize = (int)(guiPiece/2f);

			GUIStyle rightOrientedLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			rightOrientedLabel.alignment = TextAnchor.UpperRight;
			rightOrientedLabel.normal.textColor = Color.white;
			GUIStyle centerOrientedLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			centerOrientedLabel.alignment = TextAnchor.UpperCenter;
			centerOrientedLabel.normal.textColor = Color.white;

			GUIStyleState withoutImageStyle = new GUIStyleState();
			withoutImageStyle.background = null;
			GUIStyle borderlessButton = new GUIStyle(mainGUISkin.GetStyle("Button"));
			borderlessButton.normal = withoutImageStyle;
			borderlessButton.onHover = withoutImageStyle;
			GUIStyle borderlessLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			borderlessLabel.normal = withoutImageStyle;
			borderlessLabel.onHover = withoutImageStyle;
			mainGUISkin.customStyles = new GUIStyle[4] {rightOrientedLabel, borderlessButton, borderlessLabel, centerOrientedLabel};
			//testmode
			//
			GUI.skin = mainGUISkin;
			fontSize_set = true;
		}
		//GUI.Label(new Rect(Screen.width - 128, 0, 128,32), "day "+day.ToString());
		//GUI.Label(new Rect(Screen.width - 128, 32, 128,32), "week "+week.ToString());
		//GUI.Label(new Rect(Screen.width - 128, 64, 128,32), "month "+month.ToString());
		//GUI.Label(new Rect(Screen.width - 128, 96, 128,32), "year "+year.ToString());
	}
}
