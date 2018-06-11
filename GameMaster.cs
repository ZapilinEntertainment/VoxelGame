using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum Difficulty{Utopia, Easy, Normal, Hard, Torture}
public enum GameStart {Nothing, Zeppelin, Headquarters}
public enum WorkType {Nothing, Digging, Pouring, Manufacturing, Clearing, Gathering, Mining, Farming}

public class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}

	public Constructor constructor;
	public Transform camTransform, camBasis;
	public static Vector3 camPos{get;private set;}
	Vector3 camLookPoint; 
	bool moveCamToLookPoint = false;
	const float CAM_STANDART_DISTANCE = 3;

	List<GameObject> cameraUpdateBroadcast;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cameraTimer =0, cameraUpdateTime = 0.04f;
	public static Chunk mainChunk; 
	public static ColonyController colonyController{get;private set;}
	public static GeologyModule geologyModule;
	public  LineRenderer systemDrawLR;
	static string path;

	public const int START_LIFEPOWER = 100000;
	public const int LIFEPOWER_SPREAD_SPEED = 10,  CRITICAL_DEPTH = - 200;
	public static float lifeGrowCoefficient {get;private set;}
	public static float demolitionLossesPercent {get;private set;}
	public static float lifepowerLossesPercent{get;private set;}
	public static float tradeVesselsTrafficCoefficient{get;private set;}
	public static float upgradeDiscount{get;private set;}
	public static float upgradeCostIncrease{get;private set;}
	public static float environmentalConditions{get; private set;} // 0 is hell, 1 is very favourable
	public static float warProximity{get;private set;} // 0 is far, 1 is nearby
	public const float START_HAPPINESS = 1, GEARS_ANNUAL_DEGRADE = 0.1f, LIFE_DECAY_SPEED = 0.1f, LABOUR_TICK = 1, DAY_LONG = 60, CAM_LOOK_SPEED = 10,
	START_BIRTHRATE_COEFFICIENT = 0.001f, LIFEPOWER_TICK = 1;

	public static Difficulty difficulty {get;private set;}
	public GameStart startGameWith = GameStart.Zeppelin;
	public static float LUCK_COEFFICIENT {get;private set;}
	public static float sellPriceCoefficient = 0.75f;
	public static int layerCutHeight = 16, prevCutHeight = 16;

	public const int START_WORKERS_COUNT = 70, MAX_LIFEPOWER_TRANSFER = 16;
	static float diggingSpeed = 1f, pouringSpeed = 1f, manufacturingSpeed = 0.3f, 
	clearingSpeed = 20, gatheringSpeed = 5f, miningSpeed = 0.5f;


	float t;
	uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	public const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;
	public List<Component> everydayUpdateList, everyYearUpdateList, everyMonthUpdateList;

	public List <Component> windUpdateList;
	public Vector3 windVector {get; private set;}
	public float maxWindPower = 10, windTimer = 0, windChangeTime = 120;

	public static float sunlightIntensity {get; private set;}
	public Light sun;

	bool fontSize_set = false;
	public static float guiPiece {get;private set;}
	public static GUISkin mainGUISkin {get;private set;}

	public string startResources_string;

	List<string> gameAnnouncements_string; 
	const byte ANNOUNCEMENT_LOG_LENGTH = 10;
	float announcementTimer; const float ANNOUNCEMENT_CLEAR_TIME = 10f;

	// FOR TESTING
	public float newGameSpeed = 1;
	public bool weNeedNoResources = false, treesOptimization = false;
	public bool generateChunk = true;
	//---------

	public GameMaster () {
		if (realMaster != null) realMaster = null;
		realMaster = this;
	}

	void Awake() {
		gameSpeed = 1;
		cameraUpdateBroadcast = new List<GameObject>();

		everydayUpdateList = new List<Component>();
		everyYearUpdateList = new List<Component>();
		everyMonthUpdateList = new List<Component>();
		windUpdateList = new List<Component>();
		gameAnnouncements_string = new List<string>();

		lifeGrowCoefficient = 1;
		//Localization.ChangeLanguage(Language.English);
		geologyModule = gameObject.AddComponent<GeologyModule>();
		difficulty = Difficulty.Normal;
		guiPiece = Screen.height / 24f;
		warProximity = 0.01f;
		layerCutHeight = Chunk.CHUNK_SIZE; prevCutHeight = layerCutHeight;
		colonyController = gameObject.AddComponent<ColonyController>();
		colonyController.CreateStorage();
		PoolMaster pm = gameObject.AddComponent<PoolMaster>();
		pm.Load();

		path = Application.dataPath + '/';
		string saveName = "default.sav";
		if (generateChunk || !LoadGame( path + saveName) ) {
			byte standartSize = 16; // cannot be bigger than 99, cause I say Limited
			Chunk.SetChunkSize( standartSize );
			constructor.ConstructChunk( standartSize );
		}
		else { // loading data
			
		}
	}

	void Start() {
		if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);

		switch (difficulty) {
		case Difficulty.Utopia: 
			LUCK_COEFFICIENT = 1;	
			demolitionLossesPercent = 0; 
			lifepowerLossesPercent = 0;
			sellPriceCoefficient = 1;
			tradeVesselsTrafficCoefficient = 0.2f;
			upgradeDiscount = 0.5f; upgradeCostIncrease = 1.1f;
			environmentalConditions = 1;
			Hospital.loweredCoefficient = 0;
			Hospital.improvedCoefficient = 2;
			break;
		case Difficulty.Easy: 
			LUCK_COEFFICIENT = 0.7f; 
			demolitionLossesPercent = 0.2f;  
			lifepowerLossesPercent = 0.1f;
			sellPriceCoefficient = 0.9f;
			tradeVesselsTrafficCoefficient = 0.4f;
			upgradeDiscount = 0.3f; upgradeCostIncrease = 1.3f;
			environmentalConditions = 1;
			Hospital.loweredCoefficient = 0.3f;
			Hospital.improvedCoefficient = 1.75f;
			break;
		case Difficulty.Normal: 
			LUCK_COEFFICIENT = 0.5f; 
			demolitionLossesPercent = 0.4f;  
			lifepowerLossesPercent = 0.3f;
			sellPriceCoefficient = 0.75f;
			tradeVesselsTrafficCoefficient = 0.5f;
			upgradeDiscount = 0.25f; upgradeCostIncrease = 1.5f;
			environmentalConditions = 0.95f;
			Hospital.loweredCoefficient = 0.5f;
			Hospital.improvedCoefficient = 1.5f;
			break;
		case Difficulty.Hard: 
			LUCK_COEFFICIENT = 0.1f; 
			demolitionLossesPercent = 0.7f; 
			lifepowerLossesPercent = 0.5f;
			sellPriceCoefficient = 0.5f;
			tradeVesselsTrafficCoefficient = 0.75f;
			upgradeDiscount = 0.2f; upgradeCostIncrease = 1.7f;
			environmentalConditions = 0.9f;
			Hospital.loweredCoefficient = 0.75f;
			Hospital.improvedCoefficient = 1.2f;
			break;
		case Difficulty.Torture: 
			LUCK_COEFFICIENT = 0.01f; 
			demolitionLossesPercent = 1; 
			lifepowerLossesPercent = 0.85f;
			sellPriceCoefficient = 0.33f;
			tradeVesselsTrafficCoefficient = 1;
			upgradeDiscount = 0.1f; upgradeCostIncrease = 2f;
			environmentalConditions = 0.8f;
			Hospital.loweredCoefficient = 0.9f;
			Hospital.improvedCoefficient = 1.1f;
			break;
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

			if (colonyController == null )colonyController = gameObject.AddComponent<ColonyController>();
			Structure s = Structure.GetNewStructure(Structure.LANDED_ZEPPELIN_ID);
			SurfaceBlock b = mainChunk.GetSurfaceBlock(xpos,zpos);
			s.SetBasement(b, PixelPosByte.zero);
			b.MakeIndestructible(true);
			b.myChunk.GetBlock(b.pos.x, b.pos.y -1, b.pos.z).MakeIndestructible(true);

			colonyController.AddCitizens(START_WORKERS_COUNT);
			colonyController.SetHQ(s.GetComponent<HeadQuarters>());

			if (xpos > 0) xpos --; else xpos++;
			StorageHouse firstStorage = Structure.GetNewStructure(Structure.STORAGE_0_ID) as StorageHouse;
			firstStorage.SetBasement(mainChunk.GetSurfaceBlock(xpos,zpos), PixelPosByte.zero);
			//start resources
			colonyController.storage.AddResource(ResourceType.metal_K,100);
			colonyController.storage.AddResource(ResourceType.metal_M,50);
			colonyController.storage.AddResource(ResourceType.metal_E,20);
			colonyController.storage.AddResource(ResourceType.Plastics,100);
			colonyController.storage.AddResource(ResourceType.Food, 200);

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
			camPos = camTransform.position;
		}

		sunlightIntensity = 0.7f +Mathf.PerlinNoise(0.1f, Time.time * gameSpeed / 200f) * 0.3f;
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
						//everymonth update
						if (everyMonthUpdateList.Count > 0) {
							int i = 0;
							while (i < everyMonthUpdateList.Count) {
								if (everyMonthUpdateList[i] == null) {everyMonthUpdateList.RemoveAt(i); continue;}
								else {
									everyMonthUpdateList[i].SendMessage("EveryMonthUpdate", SendMessageOptions.DontRequireReceiver);
									i++;
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
			if (windVector.magnitude == 0) AddAnnouncement( Localization.announcement_stillWind );
			if (windUpdateList.Count != 0) {
				int i = 0;
				while (i < windUpdateList.Count) {
					Component c = windUpdateList[i];
					if (c == null) {windUpdateList.RemoveAt(i); continue;}
					else	{c.SendMessage("WindUpdate", windVector,SendMessageOptions.DontRequireReceiver); i++;}
				}
			}
		}
		//   GAME   ANNOUNCEMENTS
		if (announcementTimer > 0 ) {
			announcementTimer -= Time.deltaTime * gameSpeed;
			if (announcementTimer <= 0) {
				if (gameAnnouncements_string.Count > 0) {
					gameAnnouncements_string.RemoveAt(0);
					if (gameAnnouncements_string.Count > 0) announcementTimer = ANNOUNCEMENT_CLEAR_TIME;
				}
			}
		}
	}


	void LateUpdate() {
		if (moveCamToLookPoint) {
			camBasis.position = Vector3.MoveTowards(camBasis.position, camLookPoint, CAM_LOOK_SPEED * Time.deltaTime);
			if (Vector3.Distance(camBasis.position, camLookPoint) == 0) moveCamToLookPoint = false;
		}
	}

	public void AddToCameraUpdateBroadcast(GameObject g) {
		if (cameraUpdateBroadcast == null) cameraUpdateBroadcast = new List<GameObject>();
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

	public static float CalculateWorkspeed(int workersCount, WorkType type) {
		if (colonyController == null) return 0;
		float workspeed = workersCount * colonyController.labourEfficientcy_coefficient * colonyController.gears_coefficient - ( colonyController.health_coefficient + colonyController.happiness_coefficient - 2);
		switch (type) {
		case WorkType.Digging: workspeed  *= diggingSpeed;break;
		case WorkType.Manufacturing: workspeed  *= manufacturingSpeed;break;
		case WorkType.Nothing: workspeed  = 0; break;
		case WorkType.Pouring: workspeed  *= pouringSpeed;break;
		case WorkType.Clearing: workspeed  *= clearingSpeed;break;
		case WorkType.Gathering : workspeed  *= gatheringSpeed;break;
		case WorkType.Mining: workspeed  *= miningSpeed;break;
		case WorkType.Farming : workspeed *= GameMaster.lifeGrowCoefficient * environmentalConditions;break;
		}
		return workspeed ;
	}

	public void SetLookPoint(Vector3 point) {
		// при двойном касании - перенос без условия
		camLookPoint = point;
		if (Vector3.Distance(point, camBasis.transform.position) > CAM_STANDART_DISTANCE) moveCamToLookPoint = true;
	}

	public void AddAnnouncement(string s) {
		if (gameAnnouncements_string.Count >= ANNOUNCEMENT_LOG_LENGTH) {
			gameAnnouncements_string.RemoveAt(0);
		}
		gameAnnouncements_string.Add(s);
		if (announcementTimer <= 0) announcementTimer = ANNOUNCEMENT_CLEAR_TIME;
	}

	public bool LoadGame( string name ) {  // отдельно функцию проверки и коррекции сейв-файла
		string fpath = path + "Saves/" + name + ".txt";
		if ( !File.Exists ( fpath ) ) {
			print ("file not exist");
			return false;
		}
		using (StreamReader sr = new StreamReader( fpath, System.Text.Encoding.Unicode))
		{
			string line;
			line = sr.ReadLine();
			int size = 0;
			if ( !int.TryParse(line, out size) ) {
				print ("length parsing failed");
				return false;
			}
			else { 
				string[] data = new string[ size * size];
				int k = 0; line = sr.ReadLine();
				while ( k < data.Length && line != null ) {
					data[k] = line;
					line = sr.ReadLine();
					k++;
				}
				if (k < data.Length) {
					print ("not all data here");
					return false;
				}

				Chunk nchunk = new GameObject("chunk").AddComponent<Chunk>();
				bool creatingChunkSuccess = nchunk.LoadChunk(data, size);
				if ( !creatingChunkSuccess ) {
					print ("chunk creating failed");
					return false;
				}
					Destroy(mainChunk.gameObject);
					mainChunk = nchunk;
					if (line == "$") {
						List<string> str_data = new List<string>();
						line = sr.ReadLine();
						while ( line != "$" & !sr.EndOfStream) {
							str_data.Add(line);
							line = sr.ReadLine();
						}
						mainChunk.LoadStructures(str_data);
					}
					//wind vector
				line = sr.ReadLine();
					Vector3 savedVector = windVector;
					savedVector = Quaternion.AngleAxis( int.Parse(line.Substring(0,3)) , Vector3.up) * Vector3.forward * int.Parse(line.Substring(3,2));
					windVector = savedVector;
					windTimer = windChangeTime * (0.5f + Random.value * 0.5f);
					foreach (Component c in windUpdateList) { c.BroadcastMessage("WindUpdate", windVector, SendMessageOptions.DontRequireReceiver);}
					// hospital birthrate
				 Hospital.SetBirthrateMode( int.Parse(sr.ReadLine()));
				// trade operations
				line = sr.ReadLine();
				int i = 0, p =0;
				while ( i < ResourceType.RTYPES_COUNT ) {
					if ( line[p] == '0' ) p++;
					else {
						if ( line[p] == '1' ) Dock.MakeLot(i, false, int.Parse( line.Substring(p+1,4) ));
						else Dock.MakeLot(i, true, int.Parse( line.Substring(p+1,4) ));
						p += 5;
					}
					i++;
				}
				//immigration
				line = sr.ReadLine();
				if ( line[1] == '0') Dock.SetImmigrationStatus( line[0] == '1', int.Parse( line.Substring(2,3)) * (-1) );
				else Dock.SetImmigrationStatus( line[0] == '1', int.Parse( line.Substring(2,3)));
				//colony controller
				colonyController.Load( sr.ReadLine() );
				// storage
				colonyController.storage.Load( sr.ReadLine() );
				//worksites
				line = sr.ReadLine();
				if (line != null) {
					if (line[0] == 'w') {
						if (line.Length > 1) {
							p = line.IndexOf(';', 1);
							int p2 = 1;
							while (p != -1 & p2 < line.Length ) {
								int x = int.Parse(line.Substring(p2 + 12, 2)), y = int.Parse(line.Substring(p2 + 14, 2)), z =int.Parse(line.Substring(p2 + 16, 2));
								switch ( line[p2] ) {
								case '1':
									GatherSite gs = mainChunk.GetBlock(x,y,z).gameObject.AddComponent<GatherSite>();
									gs.Load(line.Substring(p2, p - p2));
									break;
								case '2':
									DigSite ds = mainChunk.GetBlock(x,y,z).gameObject.AddComponent<DigSite>();
									ds.Load(line.Substring(p2, p - p2));
									break;
								case '3':
									CleanSite cs = mainChunk.GetBlock(x,y,z).gameObject.AddComponent<CleanSite>();
									cs.Load(line.Substring(p2, p - p2));
									break;
								case '4':
									TunnelBuildingSite tbs = mainChunk.GetBlock(x,y,z).gameObject.AddComponent<TunnelBuildingSite>();
									tbs.Load(line.Substring(p2, p - p2));
									break;
								}
								p2 = p + 1;
								p = line.IndexOf(';', p+1);
							}
						}
					}
				}
				//data
				line = sr.ReadLine();
				if (line != null && line[0] == 'd') {
					day = uint.Parse(line[1].ToString());
					week = uint.Parse(line[2].ToString());
					month = uint.Parse(line.Substring(3,2));
					year = uint.Parse(line.Substring(5,3));
					millenium = uint.Parse(line[7].ToString());
				}
				//end
				colonyController.RecalculatePowerGrid();
				colonyController.RecalculateHousing();
			}
		}
		return true;
	}

	public bool SaveGame( string name ) {
		string fpath = path + "Saves/"+ name + ".txt";
		using (StreamWriter sw = new StreamWriter(fpath,false, System.Text.Encoding.Unicode)) {
			string[] dataString = mainChunk.SaveChunkData();
			foreach (string ds in dataString) {
				sw.WriteLine(ds);
			}
			sw.WriteLine("$");
			dataString = mainChunk.SaveStructures();
			if (dataString != null && dataString.Length > 0) {
				foreach (string ds in dataString) {
					sw.WriteLine(ds);
				}
			}
			sw.WriteLine("$");
			sw.WriteLine( string.Format("{0:d3}", (int)(Vector3.Angle(Vector3.forward, windVector))) +  string.Format("{0:d2}", (int)windVector.magnitude));
			sw.WriteLine(Hospital.GetBirthrateModeIndex());
			//trading operations
			int i = 0;
			string s = "";
			while ( i < ResourceType.RTYPES_COUNT ) {
				if (Dock.isForSale[i] == null) {
					s += '0';
				}
				else {
					if ( Dock.isForSale[i] == true ) s += '1'; else s += '2';
					s += string.Format("{0:d4}", Dock.minValueForTrading[i]);
				}
				i++;
			}
			sw.WriteLine(s);
			//immigration
			s= "";
			if ( Dock.immigrationEnabled ) s+= '1'; else s+='0'; 
			if ( Dock.immigrationPlan < 0) s += '0'; else s+='1';
			s += string.Format("{0:d3}", (int)(Dock.immigrationPlan * Mathf.Sign(Dock.immigrationPlan)));
			sw.WriteLine(s);
			//colony controller
			sw.WriteLine(colonyController.Save());
			// storage
			sw.WriteLine(colonyController.storage.Save());
			//worksites
			s = "w";
			if ( colonyController.worksites.Count > 0) {
				foreach ( Worksite w in colonyController.worksites ) {
					if (w == null) continue;
					else s += w.Save() + ';';
				}
			}
			sw.WriteLine(s);
			// data
			sw.WriteLine( 'd' + day.ToString() + week.ToString() + string.Format("{0:d2}", month) + string.Format("{0:d3}", year) + millenium.ToString() );
		}
		return true;
	}

	void OnGUI() {
		if (!fontSize_set) {
			mainGUISkin = Resources.Load<GUISkin>("MainSkin");
			mainGUISkin.GetStyle("Label").fontSize = (int)(guiPiece/2f);
			mainGUISkin.GetStyle("Button").fontSize = (int)(guiPiece/2f);

			GUIStyle rightOrientedLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			rightOrientedLabel.alignment = TextAnchor.UpperRight;
			rightOrientedLabel.normal.textColor = Color.white;
			PoolMaster.GUIStyle_RightOrientedLabel = rightOrientedLabel;
			GUIStyle rightBottomLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			rightBottomLabel.alignment = TextAnchor.LowerRight;
			rightBottomLabel.normal.textColor = Color.white;
			PoolMaster.GUIStyle_RightBottomLabel = rightBottomLabel;
			GUIStyle centerOrientedLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			centerOrientedLabel.alignment = TextAnchor.MiddleCenter;
			centerOrientedLabel.normal.textColor = Color.white;
			PoolMaster.GUIStyle_CenterOrientedLabel = centerOrientedLabel;

			GUIStyleState withoutImageStyle = new GUIStyleState();
			withoutImageStyle.background = null;
			withoutImageStyle.textColor = Color.white;
			GUIStyle borderlessButton = new GUIStyle(mainGUISkin.GetStyle("Button"));
			borderlessButton.normal = withoutImageStyle;
			borderlessButton.onHover = withoutImageStyle;
			PoolMaster.GUIStyle_BorderlessButton= borderlessButton;
			GUIStyle borderlessLabel = new GUIStyle(mainGUISkin.GetStyle("Label"));
			borderlessLabel.normal = withoutImageStyle;
			borderlessLabel.onHover = withoutImageStyle;
			PoolMaster.GUIStyle_BorderlessLabel = borderlessLabel;
			GUIStyle systemAlert = new GUIStyle(mainGUISkin.GetStyle("Label"));
			systemAlert.normal = withoutImageStyle;
			systemAlert.normal.textColor = Color.red;
			systemAlert.fontSize = (int)guiPiece;
			systemAlert.alignment = TextAnchor.MiddleCenter;
			PoolMaster.GUIStyle_SystemAlert = systemAlert;

			fontSize_set = true;
		}
		GUI.skin = mainGUISkin;

		int sh = Screen.height;
		//if (GUI.Button(new Rect(0, sh - 3 *guiPiece, guiPiece, guiPiece), "x1")) newGameSpeed = 1;
		//if (GUI.Button(new Rect(0, sh - 2 *guiPiece, guiPiece, guiPiece), "x2")) newGameSpeed = 2;
		//if (GUI.Button(new Rect(0, sh - guiPiece, guiPiece, guiPiece), "x10")) newGameSpeed = 10;
		GUI.Label(new Rect(guiPiece, sh - guiPiece, 10 * guiPiece, guiPiece), "day : "+day.ToString() + " week: " + week.ToString() + ", month: " + month.ToString() + " year: " + year.ToString());

		if (gameAnnouncements_string.Count > 0) {
			Rect anr = new Rect(0, sh - 3 * guiPiece - gameAnnouncements_string.Count * guiPiece * 0.75f, 10 * guiPiece, 0.75f * guiPiece);
			GUI.color = Color.black;
			foreach (string announcement in gameAnnouncements_string) {
				GUI.Label(anr, announcement);
				anr.y += anr.height;
			}
			GUI.color = Color.white;
		}
	}
}
