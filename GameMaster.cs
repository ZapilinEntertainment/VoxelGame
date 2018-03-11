using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum Difficulty{Utopia, Easy, Normal, Hard, Torture}

public class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}
	public float newGameSpeed = 1;

	public Transform camTransform;
	List<GameObject> cameraUpdateBroadcast;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cameraTimer =0, cameraUpdateTime = 0.04f;
	bool fontSize_set = false;
	int camCullingMask = 1;
	public Chunk mainChunk; public static ColonyController colonyController;
	static string path;

	public const int LIFEPOWER_SPREAD_SPEED = 10, START_LIFEPOWER = 100000, CRITICAL_DEPTH = - 200;
	public static float lifeGrowCoefficient {get;private set;}
	public static float labourEfficiency {get;private set;}
	public const float START_HAPPINESS = 1, GEARS_ANNUAL_DEGRADE = 0.1f, LIFE_DECAY_SPEED = 0.1f;

	public static Difficulty difficulty {get;private set;}
	public static float LUCK_COEFFICIENT {get;private set;}

	public static float metalC_abundance = 0.01f, metalM_abundance = 0.005f, metalE_abundance = 0.003f, 
	metalN_abundance = 0.0001f, metalP_abundance = 0.02f, metalS_abundance = 0.0045f,
	mineralF_abundance = 0.02f, mineralL_abundance = 0.02f; // sum must be less than one!

	float t;
	uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;
	const float DAY_LONG = 60;
	public List<Component> everydayUpdateList, everyYearUpdateList;

	public List <Component> windUpdateList;
	public Vector3 windVector {get; private set;}
	public float maxWindPower = 10, windTimer = 0, windChangeTime = 120;

	public static float sunlightIntensity {get; private set;}
	public Light sun;

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

		labourEfficiency =1;
		lifeGrowCoefficient = 1;

		//Localization.ChangeLanguage(Language.English);
	}

	void Start() {
		if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);

		switch (difficulty) {
		case Difficulty.Utopia: LUCK_COEFFICIENT = 1;	break;
		case Difficulty.Easy: LUCK_COEFFICIENT = 0.7f;break;
		case Difficulty.Normal: LUCK_COEFFICIENT = 0.5f; break;
		case Difficulty.Hard: LUCK_COEFFICIENT = 0.1f;break;
		case Difficulty.Torture: LUCK_COEFFICIENT = 0.01f;break;
		}

		Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
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

		windTimer -= t;
		if (windTimer <= 0) {
			windVector = Random.onUnitSphere * (maxWindPower * Random.value);
			windTimer = windChangeTime * (1 + Random.value -0.5f);
			if (windUpdateList.Count != 0) {
				foreach (Component c in windUpdateList) {
					c.SendMessage("WindUpdate",SendMessageOptions.DontRequireReceiver);
				}
			}
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

	public static float CalculateWorkflow(int workersCount) {
		if (colonyController == null) return 0;
		return (workersCount * labourEfficiency * colonyController.gears_coefficient - ( colonyController.health_coefficient + colonyController.happiness_coefficient - 2));
	}

	void OnGUI() {
		if (!fontSize_set) {
			GUI.skin.GetStyle("Label").fontSize = 27;
			fontSize_set = true;
		}
		GUI.Label(new Rect(Screen.width - 128, 0, 128,32), "day "+day.ToString());
		GUI.Label(new Rect(Screen.width - 128, 32, 128,32), "week "+week.ToString());
		GUI.Label(new Rect(Screen.width - 128, 64, 128,32), "month "+month.ToString());
		GUI.Label(new Rect(Screen.width - 128, 96, 128,32), "year "+year.ToString());
	}
}
