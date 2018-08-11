using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public enum Difficulty{Utopia, Easy, Normal, Hard, Torture}
public enum GameStart {Nothing, Zeppelin, Headquarters}
public enum WorkType {Nothing, Digging, Pouring, Manufacturing, Clearing, Gathering, Mining, Farming, MachineConstructing}

public sealed class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}

	public Constructor constructor;
	public Transform camTransform, camBasis;
	public static Vector3 camPos{get;private set;}
    public static bool applicationStopWorking { get; protected set; }

	public List<GameObject> cameraUpdateBroadcast;
	public List<GameObject> standartSpritesList, mastSpritesList;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cameraTimer =0, cameraUpdateTime = 0.04f;
	public static Chunk mainChunk; 
	public static ColonyController colonyController{get;private set;}
	public static GeologyModule geologyModule;
	public  LineRenderer systemDrawLR;

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
    START_BIRTHRATE_COEFFICIENT = 0.001f, LIFEPOWER_TICK = 1, HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000;
    public const int START_WORKERS_COUNT = 70, MAX_LIFEPOWER_TRANSFER = 16, SURFACE_MATERIAL_REPLACE_COUNT = 256;

    public static Difficulty difficulty {get;private set;}
	public GameStart startGameWith = GameStart.Zeppelin;
	public static float LUCK_COEFFICIENT {get;private set;}
	public static float sellPriceCoefficient = 0.75f;
	public static int layerCutHeight = 16, prevCutHeight = 16;

	
	static float diggingSpeed = 1f, pouringSpeed = 1f, manufacturingSpeed = 0.3f, 
	clearingSpeed = 20, gatheringSpeed = 0.1f, miningSpeed = 0.5f, machineConstructingSpeed = 1;


	float t;
	uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	public const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;
	public List<Component> everydayUpdateList, everyYearUpdateList, everyMonthUpdateList;

	public List <Component> windUpdateList;
	public Vector3 windVector {get; private set;}
	public float windTimer = 0, windChangeTime = 120;

	// FOR TESTING
	public float newGameSpeed = 1;
	public bool weNeedNoResources = false, treesOptimization = false;
	public bool generateChunk = true;
    [SerializeField] byte chunkSize = 16;// cannot be bigger than 99, cause I say Limited
                                         

    public GameMaster () {
        if (realMaster != null)
        {
            Destroy(realMaster);
        }
        realMaster = this;
	}

	void Start() {
        gameSpeed = 1;
        cameraUpdateBroadcast = new List<GameObject>();
        standartSpritesList = new List<GameObject>();

        mastSpritesList = new List<GameObject>();
        GameObject[] msprites = GameObject.FindGameObjectsWithTag("AddToMastSpritesList");
        if (msprites != null) foreach (GameObject g in msprites) mastSpritesList.Add(g);
        msprites = null;

        everydayUpdateList = new List<Component>();
        everyYearUpdateList = new List<Component>();
        everyMonthUpdateList = new List<Component>();
        windUpdateList = new List<Component>();

        lifeGrowCoefficient = 1;
        //Localization.ChangeLanguage(Language.English);
        geologyModule = gameObject.AddComponent<GeologyModule>();
        difficulty = Difficulty.Normal;
        warProximity = 0.01f;
        layerCutHeight = Chunk.CHUNK_SIZE; prevCutHeight = layerCutHeight;
        colonyController = gameObject.AddComponent<ColonyController>();
        colonyController.CreateStorage();
        PoolMaster pm = gameObject.AddComponent<PoolMaster>();
        pm.Load();

        string saveName = "default.sav";
        if (generateChunk)
        {
            Chunk.SetChunkSize(chunkSize);
            constructor.ConstructChunk(chunkSize);
        }
        else
        { // loading data

        }
        camBasis.transform.position = Vector3.one * chunkSize / 2f;

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
			break;
		case Difficulty.Easy: 
			LUCK_COEFFICIENT = 0.7f; 
			demolitionLossesPercent = 0.2f;  
			lifepowerLossesPercent = 0.1f;
			sellPriceCoefficient = 0.9f;
			tradeVesselsTrafficCoefficient = 0.4f;
			upgradeDiscount = 0.3f; upgradeCostIncrease = 1.3f;
			environmentalConditions = 1;
			break;
		case Difficulty.Normal: 
			LUCK_COEFFICIENT = 0.5f; 
			demolitionLossesPercent = 0.4f;  
			lifepowerLossesPercent = 0.3f;
			sellPriceCoefficient = 0.75f;
			tradeVesselsTrafficCoefficient = 0.5f;
			upgradeDiscount = 0.25f; upgradeCostIncrease = 1.5f;
			environmentalConditions = 0.95f;
			break;
		case Difficulty.Hard: 
			LUCK_COEFFICIENT = 0.1f; 
			demolitionLossesPercent = 0.7f; 
			lifepowerLossesPercent = 0.5f;
			sellPriceCoefficient = 0.5f;
			tradeVesselsTrafficCoefficient = 0.75f;
			upgradeDiscount = 0.2f; upgradeCostIncrease = 1.7f;
			environmentalConditions = 0.9f;
			break;
		case Difficulty.Torture: 
			LUCK_COEFFICIENT = 0.01f; 
			demolitionLossesPercent = 1; 
			lifepowerLossesPercent = 0.85f;
			sellPriceCoefficient = 0.33f;
			tradeVesselsTrafficCoefficient = 1;
			upgradeDiscount = 0.1f; upgradeCostIncrease = 2f;
			environmentalConditions = 0.8f;
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

			//UI ui = gameObject.AddComponent<UI>();
			//ui.lineDrawer = systemDrawLR;
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
                AllCameraFollowersUpdate();
			}
			camPos = camTransform.position;
		}

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
			windVector = Random.onUnitSphere;
			windVector += Vector3.down * windVector.y;
			windTimer = windChangeTime + Random.value * windChangeTime;
			if (windVector.magnitude == 0) UIController.current.MakeAnnouncement( Localization.announcement_stillWind );
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

    public void AddToCameraUpdateBroadcast(GameObject g)
    {
        if (cameraUpdateBroadcast == null) cameraUpdateBroadcast = new List<GameObject>();
        if (g != null) cameraUpdateBroadcast.Add(g);
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
		case WorkType.MachineConstructing: workspeed *= machineConstructingSpeed;break;
		}
		return workspeed ;
	}

    void AllCameraFollowersUpdate()
    {
        GameObject receiver = null;
        int c = 0;
        while (c < cameraUpdateBroadcast.Count)
        {
            receiver = cameraUpdateBroadcast[c];
            if (receiver == null)
            {
                cameraUpdateBroadcast.RemoveAt(c);
                continue;
            }
            else
            {
                if (receiver.activeSelf) receiver.SendMessage("CameraUpdate", camTransform, SendMessageOptions.DontRequireReceiver);
                c++;
            }
        }
        if (standartSpritesList.Count > 0)
        {
            int i = 0;
            while (i < standartSpritesList.Count)
            {
                receiver = standartSpritesList[i];
                if (receiver == null)
                {
                    standartSpritesList.RemoveAt(i);
                    continue;
                }
                else
                {
                    if (receiver.activeSelf) receiver.transform.LookAt(camPos);
                    i++;
                }
            }
        }
        if (mastSpritesList.Count > 0)
        {
            int i = 0;
            while (i < mastSpritesList.Count)
            {
                receiver = mastSpritesList[i];
                if (receiver == null)
                {
                    mastSpritesList.RemoveAt(i);
                    continue;
                }
                else
                {
                    if (receiver.activeSelf)
                    {
                        Transform obj = receiver.transform;
                        Vector3 dir = camPos - obj.position;
                        dir = Vector3.ProjectOnPlane(dir, obj.TransformDirection(Vector3.up));
                        obj.rotation = Quaternion.LookRotation(dir.normalized, obj.TransformDirection(Vector3.up));
                    }
                    i++;
                }
            }
        }
        cameraHasMoved = false;
        cameraTimer = cameraUpdateTime;
    }

	public bool SaveGame( string name ) { // заменить потом на persistent -  постоянный путь
		Time.timeScale = 0;
		GameMasterSerializer gms = new GameMasterSerializer();
		#region gms mainPartFilling
		gms.gameSpeed = gameSpeed;
		gms.lifeGrowCoefficient = lifeGrowCoefficient;
		gms.demolitionLossesPercent = demolitionLossesPercent;
		gms.lifepowerLossesPercent = lifepowerLossesPercent;
		gms.luckCoefficient = LUCK_COEFFICIENT;
		gms.sellPriceCoefficient = sellPriceCoefficient;
		gms.tradeVesselsTrafficCoefficient = tradeVesselsTrafficCoefficient;
		gms.upgradeDiscount = upgradeDiscount;
		gms.upgradeCostIncrease = upgradeCostIncrease;
		gms.environmentalConditions = environmentalConditions;
		gms.warProximity = warProximity;
		gms.difficulty = difficulty;
		gms.startGameWith = startGameWith;
		gms.prevCutHeight = prevCutHeight;
		gms.diggingSpeed = diggingSpeed;
		gms.pouringSpeed = pouringSpeed;
		gms.manufacturingSpeed = manufacturingSpeed;
		gms.clearingSpeed = clearingSpeed;
		gms.gatheringSpeed = gatheringSpeed;
		gms.miningSpeed = miningSpeed;
		gms.machineConstructingSpeed = machineConstructingSpeed;
		gms.day = day; gms.week = week; gms.month = month; gms.year = year; gms.millenium = millenium; gms.t = t;
		gms.windVector_x = windVector.x; gms.windVector_y = windVector.y; gms.windVector_z = windVector.z; 
		gms.windTimer = windTimer;gms.windChangeTime = windChangeTime;
		gms.recruiting_hireCost = RecruitingCenter.GetHireCost();
		#endregion
		gms.chunkSerializer = mainChunk.SaveChunkData();
		gms.colonyControllerSerializer = colonyController.Save();
		gms.dockStaticSerializer = Dock.SaveStaticDockData();
		gms.shuttleStaticSerializer = Shuttle.SaveStaticData();
		gms.crewStaticSerializer = Crew.SaveStaticData();
		gms.questStaticSerializer = Quest.SaveStaticData();
		gms.expeditionCorpusStaticSerializer = ExpeditionCorpus.SaveStaticData();
		FileStream fs = File.Create(Application.persistentDataPath + "/save.txt");
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(fs, gms);
		fs.Close();
		Time.timeScale = 1;
		return true;
	}


	public bool LoadGame( string name ) {  // отдельно функцию проверки и коррекции сейв-файла
		if(File.Exists(Application.dataPath + "/Saves/save.txt")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/save.txt", FileMode.Open);
            Time.timeScale = 0; GameMaster.gameSpeed = 0;
			GameMasterSerializer gms = (GameMasterSerializer) bf.Deserialize(file);
			#region gms mainPartLoading
			gameSpeed =  gms.gameSpeed;
			lifeGrowCoefficient = gms.lifeGrowCoefficient;
			demolitionLossesPercent = gms.demolitionLossesPercent;
			lifepowerLossesPercent = gms.lifepowerLossesPercent;
			LUCK_COEFFICIENT = gms.luckCoefficient;
			sellPriceCoefficient = gms.sellPriceCoefficient;
			tradeVesselsTrafficCoefficient = gms.tradeVesselsTrafficCoefficient ;
			upgradeDiscount = gms.upgradeDiscount;
			upgradeCostIncrease = gms.upgradeCostIncrease;
			environmentalConditions = gms.environmentalConditions ;
			warProximity = gms.warProximity;
			difficulty = gms.difficulty ;
			startGameWith = gms.startGameWith ;
			prevCutHeight = gms.prevCutHeight;
			diggingSpeed = gms.diggingSpeed ;
			pouringSpeed = gms.pouringSpeed ;
			manufacturingSpeed =gms.manufacturingSpeed ;
			clearingSpeed = gms.clearingSpeed;
			gatheringSpeed = gms.gatheringSpeed ;
			miningSpeed = gms.miningSpeed ;
			machineConstructingSpeed = gms.machineConstructingSpeed ;
			day = gms.day; week = gms.week;month =  gms.month ; year = gms.year ; millenium = gms.millenium ; t = gms.t ;
			windVector = new Vector3(gms.windVector_x, gms.windVector_y, gms.windVector_z);
			windTimer= gms.windTimer ;windChangeTime = gms.windChangeTime ;
			RecruitingCenter.SetHireCost( gms.recruiting_hireCost);
            #endregion      
			Destroy (mainChunk.gameObject);            

            Crew.Reset(); Shuttle.Reset(); Hospital.Reset();Dock.Reset(); RecruitingCenter.Reset();
			QuantumTransmitter.Reset();Hangar.Reset();
            Grassland.ScriptReset(); 
			//UI.current.Reset();

			Crew.LoadStaticData(gms.crewStaticSerializer);
			Shuttle.LoadStaticData(gms.shuttleStaticSerializer); // because of hangars

			GameObject g = new GameObject("chunk");
			mainChunk = g.AddComponent<Chunk>();
			mainChunk.LoadChunkData(gms.chunkSerializer);
			colonyController.Load(gms.colonyControllerSerializer);

			Dock.LoadStaticData(gms.dockStaticSerializer);
			Quest.LoadStaticData(gms.questStaticSerializer);
			ExpeditionCorpus.LoadStaticData(gms.expeditionCorpusStaticSerializer);

			file.Close();
			Time.timeScale = 1; GameMaster.gameSpeed = 1;
            AllCameraFollowersUpdate();
            prevCamPos = camTransform.position;
            prevCamRot = camTransform.rotation;
            return true;
		}
		else return false;
	}

	public static void DeserializeByteArray<T>( byte[] data, ref T output ) {
		using (MemoryStream stream = new MemoryStream(data))
		{
			output = (T)System.Convert.ChangeType(new BinaryFormatter().Deserialize(stream), typeof(T));
		}
	}

    private void OnApplicationQuit()
    {
        applicationStopWorking = true;
    }
}

[System.Serializable]
class GameMasterSerializer {
	public float gameSpeed;
	public float lifeGrowCoefficient, demolitionLossesPercent, lifepowerLossesPercent, luckCoefficient, sellPriceCoefficient,
	tradeVesselsTrafficCoefficient, upgradeDiscount, upgradeCostIncrease, environmentalConditions, warProximity;
	public Difficulty difficulty;
	public GameStart startGameWith;
	public int prevCutHeight = 16;
	public float diggingSpeed = 1f, pouringSpeed = 1f, manufacturingSpeed = 0.3f, 
	clearingSpeed = 20, gatheringSpeed = 5f, miningSpeed = 0.5f, machineConstructingSpeed = 1;
	public uint day = 0, week = 0, month = 0, year = 0, millenium = 0; public float t;
	public float windVector_x,windVector_y,windVector_z;
	public float windTimer = 0, windChangeTime = 120;
	public float sunlightIntensity;

	public ChunkSerializer chunkSerializer;
	public ColonyControllerSerializer colonyControllerSerializer;
	public DockStaticSerializer dockStaticSerializer;
	public CrewStaticSerializer crewStaticSerializer;
	public ShuttleStaticSerializer shuttleStaticSerializer;
	public QuestStaticSerializer questStaticSerializer;
	public ExpeditionCorpusStaticSerializer expeditionCorpusStaticSerializer;
	public float recruiting_hireCost;

	// все, что можно - в классы - сериализаторы
}
