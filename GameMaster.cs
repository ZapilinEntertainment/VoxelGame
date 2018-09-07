using System.Collections.Generic; // листы
using UnityEngine; // классы Юнити
using System.IO; // чтение-запись файлов
using System.Runtime.Serialization.Formatters.Binary; // конверсия в поток байтов и обратно

public struct GameStartSettings  {
    public bool generateChunk;
    public byte chunkSize;
    public Difficulty difficulty;
    public float terrainRoughness;
    public static readonly GameStartSettings Empty;
    static GameStartSettings()
    {
        Empty = new GameStartSettings(true, 16, Difficulty.Normal, 0.3f);
    }
    public GameStartSettings(bool i_generateChunk, byte i_chunkSize, Difficulty diff, float i_terrainRoughness)
    {
        generateChunk = i_generateChunk;
        chunkSize = i_chunkSize;
        difficulty = diff;
        terrainRoughness = i_terrainRoughness;
    }
    public GameStartSettings(bool i_generateChunk)
    {
        generateChunk = i_generateChunk;
        chunkSize = 8;
        difficulty = Difficulty.Normal;
        terrainRoughness = 0.3f;
    }
    }

public enum Difficulty{Utopia, Easy, Normal, Hard, Torture}
public enum GameStart {Nothing, Zeppelin, Headquarters}
public enum WorkType {Nothing, Digging, Pouring, Manufacturing, Clearing, Gathering, Mining, Farming, MachineConstructing}

/// -----------------------------------------------------------------------------

public sealed class GameMaster : MonoBehaviour {
	 public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}

	public Constructor constructor;
	public Transform camTransform, camBasis;
    public static System.Random randomizer;
	public static Vector3 camPos{get;private set;}
    public static bool applicationStopWorking { get; private set; }

	public List<GameObject> cameraUpdateBroadcast;
	public List<GameObject> standartSpritesList, mastSpritesList;
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cameraTimer =0, cameraUpdateTime = 0.04f;
	public static Chunk mainChunk; 
	public static ColonyController colonyController{get;private set;}
	public static GeologyModule geologyModule;
	public  LineRenderer systemDrawLR;

	public const int LIFEPOWER_PER_BLOCK = 200;
	public const int LIFEPOWER_SPREAD_SPEED = 10,  CRITICAL_DEPTH = - 200;
	public static float lifeGrowCoefficient {get;private set;}
	public static float demolitionLossesPercent {get;private set;}
	public static float lifepowerLossesPercent{get;private set;}
	public static float tradeVesselsTrafficCoefficient{get;private set;}
	public static float upgradeDiscount{get;private set;}
	public static float upgradeCostIncrease{get;private set;}
	public static float environmentalConditions{get; private set;} // 0 is hell, 1 is very favourable
	public static float warProximity{get;private set;} // 0 is far, 1 is nearby

    public const float START_HAPPINESS = 1, GEARS_ANNUAL_DEGRADE = 0.1f, LIFE_DECAY_SPEED = 0.1f, DAY_LONG = 60, CAM_LOOK_SPEED = 10,
    START_BIRTHRATE_COEFFICIENT = 0.001f, HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000;
    public const float LIFEPOWER_TICK = 1, LABOUR_TICK = 0.25f; // cannot be zero
    public const int START_WORKERS_COUNT = 70, MAX_LIFEPOWER_TRANSFER = 16, SURFACE_MATERIAL_REPLACE_COUNT = 256;

    public static string savename = "autosave";
    public static GameStartSettings gss = GameStartSettings.Empty;
    public static Difficulty difficulty {get;private set;}
	public GameStart startGameWith = GameStart.Zeppelin;
	public static float LUCK_COEFFICIENT {get;private set;}
	public static float sellPriceCoefficient = 0.75f;
	public static int layerCutHeight = 16, prevCutHeight = 16;

	
	static float diggingSpeed = 0.5f, pouringSpeed = 1f, manufacturingSpeed = 0.3f, 
	clearingSpeed = 20, gatheringSpeed = 0.1f, miningSpeed = 1, machineConstructingSpeed = 1;
    
	float timeGone;
	uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	public const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;

    public delegate void StructureUpdateHandler();
    public event StructureUpdateHandler labourUpdateEvent, visualUpdateEvent, lifepowerUpdateEvent;
    private float labourTimer = 0, lifepowerTimer = 0;

    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;
	public Vector2 windVector {get; private set;}
    private float windTimer = 0, windChangeTime = 120;

    bool firstSet = true;

	// FOR TESTING
	public float newGameSpeed = 1;
	public bool weNeedNoResources = false, treesOptimization = false;
	public bool generateChunk = true;
                                         

    private void Awake() {
        if (realMaster != null & realMaster != this)
        {
            Destroy(realMaster);
        }
        realMaster = this;
	}

	void Start() {
        if (!firstSet) return;
        gameSpeed = 1;
        cameraUpdateBroadcast = new List<GameObject>();
        standartSpritesList = new List<GameObject>();

        mastSpritesList = new List<GameObject>();
        GameObject[] msprites = GameObject.FindGameObjectsWithTag("AddToMastSpritesList");
        if (msprites != null) foreach (GameObject g in msprites) mastSpritesList.Add(g);

        lifeGrowCoefficient = 1;
        //Localization.ChangeLanguage(Language.English);
        randomizer = new System.Random();

            geologyModule = gameObject.AddComponent<GeologyModule>();
            difficulty = gss.difficulty;
            colonyController = gameObject.AddComponent<ColonyController>();
            colonyController.CreateStorage();
            PoolMaster pm = gameObject.AddComponent<PoolMaster>();
            pm.Load();
            if (gss.generateChunk)
            {
                Chunk.SetChunkSize(gss.chunkSize);
                constructor.ConstructChunk(gss.chunkSize);
                camBasis.transform.position = Vector3.one * gss.chunkSize / 2f;
                switch (difficulty)
                {
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
                warProximity = 0.01f;
                layerCutHeight = Chunk.CHUNK_SIZE; prevCutHeight = layerCutHeight;
                switch (startGameWith)
                {
                    case GameStart.Zeppelin:
                        LandingUI lui = gameObject.AddComponent<LandingUI>();
                        lui.lineDrawer = systemDrawLR;
                        Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
                        break;
                    case GameStart.Headquarters:
                        int xpos = (int)(Random.value * (Chunk.CHUNK_SIZE - 1));
                        int zpos = (int)(Random.value * (Chunk.CHUNK_SIZE - 1));

                        if (colonyController == null) colonyController = gameObject.AddComponent<ColonyController>();
                        Structure s = Structure.GetStructureByID(Structure.LANDED_ZEPPELIN_ID);
                        SurfaceBlock b = mainChunk.GetSurfaceBlock(xpos, zpos);
                        s.SetBasement(b, PixelPosByte.zero);
                        b.MakeIndestructible(true);
                        b.myChunk.GetBlock(b.pos.x, b.pos.y - 1, b.pos.z).MakeIndestructible(true);

                        colonyController.AddCitizens(START_WORKERS_COUNT);

                        if (xpos > 0) xpos--; else xpos++;
                        StorageHouse firstStorage = Structure.GetStructureByID(Structure.STORAGE_0_ID) as StorageHouse;
                        firstStorage.SetBasement(mainChunk.GetSurfaceBlock(xpos, zpos), PixelPosByte.zero);
                        //start resources
                        colonyController.storage.AddResource(ResourceType.metal_K, 100);
                        colonyController.storage.AddResource(ResourceType.metal_M, 50);
                        colonyController.storage.AddResource(ResourceType.metal_E, 20);
                        colonyController.storage.AddResource(ResourceType.Plastics, 100);
                        colonyController.storage.AddResource(ResourceType.Food, 200);

                        //UI ui = gameObject.AddComponent<UI>();
                        //ui.lineDrawer = systemDrawLR;
                        break;
                }
            }
            else LoadGame(Application.persistentDataPath + "/Saves/" + savename + ".sav");        

        if (camTransform == null) camTransform = Camera.main.transform;
		prevCamPos = camTransform.position * (-1);
	}

    #region updates
    private void Update()
    {
        if (gameSpeed != newGameSpeed) gameSpeed = newGameSpeed;

        if (camTransform != null)
        {
            if (prevCamPos != camTransform.position || prevCamRot != Camera.main.transform.rotation)
            {
                cameraHasMoved = true;
                prevCamPos = camTransform.position;
                prevCamRot = camTransform.rotation;
            }
            if (cameraTimer > 0) cameraTimer -= Time.deltaTime;
            if (cameraTimer <= 0 && cameraHasMoved)
            {
                AllCameraFollowersUpdate();
            }
            camPos = camTransform.position;
        }

        float frameTime = Time.deltaTime * gameSpeed;
        if (visualUpdateEvent != null)
        {
            visualUpdateEvent();
        }
    }

    private void FixedUpdate()
    {
        if (gameSpeed != 0)
        {
            timeGone += Time.deltaTime * gameSpeed;

            if (timeGone >= DAY_LONG)
            {
                uint daysDelta = (uint)(timeGone / DAY_LONG);
                day += daysDelta;
                timeGone = timeGone % DAY_LONG;
                if (day >= DAYS_IN_WEEK)
                {
                    week += day / DAYS_IN_WEEK;
                    day = day % DAYS_IN_WEEK;
                    if (week >= WEEKS_IN_MONTH)
                    {
                        month += week / WEEKS_IN_MONTH;
                        week = week % WEEKS_IN_MONTH;
                        if (month > MONTHS_IN_YEAR)
                        {
                            uint yearsDelta = (uint)month / MONTHS_IN_YEAR;
                            year += yearsDelta;
                            month = month % MONTHS_IN_YEAR;
                            if (year > 1000)
                            {
                                millenium += year / 1000;
                                year = year % 1000;
                            }
                            if (yearsDelta < 2) colonyController.EveryYearUpdate();
                            else print("unexpected - more than one year coming in one frame");
                        }
                    }
                }
                //day Update
                if (daysDelta < 2) colonyController.EverydayUpdate();
                else print("unexpected - more than one day coming in one frame");
                //eo day update
            }

            float fixedTime = Time.fixedDeltaTime * gameSpeed;
            windTimer -= fixedTime;
            labourTimer -= fixedTime;
            lifepowerTimer -= fixedTime;

            if (windTimer <= 0)
            {
                windTimer = windChangeTime * (0.7f + Random.value * 1.3f);
                windVector = Random.insideUnitCircle;
                windTimer = windChangeTime + Random.value * windChangeTime;
                if (WindUpdateEvent != null)
                {
                    WindUpdateEvent(windVector);
                }
            }
          if (labourTimer <= 0)
            {
                labourTimer = LABOUR_TICK;
                if (labourUpdateEvent != null) labourUpdateEvent();
            }
          if (lifepowerTimer <= 0)
            {
                lifepowerTimer = LIFEPOWER_TICK;
                Plant.PlantUpdate();
                if (mainChunk != null) mainChunk.LifepowerUpdate(); // внутри обновляет все grasslands  
            }
        }
    }

    public void AddToCameraUpdateBroadcast(GameObject g)
    {
        if (cameraUpdateBroadcast == null) cameraUpdateBroadcast = new List<GameObject>();
        if (g != null) cameraUpdateBroadcast.Add(g);
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
    #endregion

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
		case WorkType.Mining: workspeed  *= miningSpeed;break; // digging inside mine
		case WorkType.Farming : workspeed *= lifeGrowCoefficient * environmentalConditions;break;
		case WorkType.MachineConstructing: workspeed *= machineConstructingSpeed;break;
		}
		return workspeed ;
	}

    

    #region save-load system
    public bool SaveGame() { return SaveGame("autosave"); }
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
		gms.day = day; gms.week = week; gms.month = month; gms.year = year; gms.millenium = millenium; gms.t = timeGone;
        gms.windVector = windVector;

		gms.windTimer = windTimer;gms.windChangeTime = windChangeTime;
        gms.labourTimer = labourTimer;
        gms.lifepowerTimer = lifepowerTimer;

		gms.recruiting_hireCost = RecruitingCenter.GetHireCost();
		#endregion
		gms.chunkSerializer = mainChunk.SaveChunkData();
		gms.colonyControllerSerializer = colonyController.Save();
		gms.dockStaticSerializer = Dock.SaveStaticDockData();
		gms.shuttleStaticSerializer = Shuttle.SaveStaticData();
		gms.crewStaticSerializer = Crew.SaveStaticData();
		gms.questStaticSerializer = QuestUI.current.Save();
		gms.expeditionStaticSerializer = Expedition.SaveStaticData();
        string path = Application.persistentDataPath + "/Saves/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        FileStream fs = File.Create(path + name + ".sav");
        savename = name;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(fs, gms);
		fs.Close();
		Time.timeScale = 1;
		return true;
	}

    public bool LoadGame() { return LoadGame("autosave"); }
	public bool LoadGame( string fullname ) {  // отдельно функцию проверки и коррекции сейв-файла
        if (true) // <- тут будет функция проверки
        {
            StopAllCoroutines();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fullname, FileMode.Open);
            Time.timeScale = 0; gameSpeed = 0;
            GameMasterSerializer gms = (GameMasterSerializer)bf.Deserialize(file);
            #region gms mainPartLoading
            gameSpeed = gms.gameSpeed;
            lifeGrowCoefficient = gms.lifeGrowCoefficient;
            demolitionLossesPercent = gms.demolitionLossesPercent;
            lifepowerLossesPercent = gms.lifepowerLossesPercent;
            LUCK_COEFFICIENT = gms.luckCoefficient;
            sellPriceCoefficient = gms.sellPriceCoefficient;
            tradeVesselsTrafficCoefficient = gms.tradeVesselsTrafficCoefficient;
            upgradeDiscount = gms.upgradeDiscount;
            upgradeCostIncrease = gms.upgradeCostIncrease;
            environmentalConditions = gms.environmentalConditions;
            warProximity = gms.warProximity;
            difficulty = gms.difficulty;
            startGameWith = gms.startGameWith;
            prevCutHeight = gms.prevCutHeight;
            diggingSpeed = gms.diggingSpeed;
            pouringSpeed = gms.pouringSpeed;
            manufacturingSpeed = gms.manufacturingSpeed;
            clearingSpeed = gms.clearingSpeed;
            gatheringSpeed = gms.gatheringSpeed;
            miningSpeed = gms.miningSpeed;
            machineConstructingSpeed = gms.machineConstructingSpeed;
            day = gms.day; week = gms.week; month = gms.month; year = gms.year; millenium = gms.millenium; timeGone = gms.t;

            windVector = gms.windVector;
            windTimer = gms.windTimer; windChangeTime = gms.windChangeTime;
            lifepowerTimer = gms.lifepowerTimer;
            labourTimer = gms.labourTimer;
            RecruitingCenter.SetHireCost(gms.recruiting_hireCost);
            #endregion
            if (mainChunk != null)Destroy(mainChunk.gameObject);

            Crew.Reset(); Shuttle.Reset(); Hospital.Reset(); Dock.ResetToDefaults(); RecruitingCenter.Reset();
            QuantumTransmitter.Reset(); Hangar.Reset();
            Grassland.ScriptReset();
            Expedition.GameReset();
            Structure.ResetToDefaults_Static();
            //UI.current.Reset();

            Crew.LoadStaticData(gms.crewStaticSerializer);
            Shuttle.LoadStaticData(gms.shuttleStaticSerializer); // because of hangars

            GameObject g = new GameObject("chunk");
            mainChunk = g.AddComponent<Chunk>();
            mainChunk.LoadChunkData(gms.chunkSerializer);
            colonyController.Load(gms.colonyControllerSerializer);

            Dock.LoadStaticData(gms.dockStaticSerializer);
            QuestUI.current.Load(gms.questStaticSerializer);
            Expedition.LoadStaticData(gms.expeditionStaticSerializer);

            file.Close();
            Time.timeScale = 1; gameSpeed = 1;
            AllCameraFollowersUpdate();
            prevCamPos = camTransform.position;
            prevCamRot = camTransform.rotation;
            savename = fullname;
            return true;
        }
        else
        {
            UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed));
            return false;
        }
	}

	public static void DeserializeByteArray<T>( byte[] data, ref T output ) {
		using (MemoryStream stream = new MemoryStream(data))
		{
			output = (T)System.Convert.ChangeType(new BinaryFormatter().Deserialize(stream), typeof(T));
		}
	}
    #endregion
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
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
    public Vector2 windVector;
	public float windTimer = 0, windChangeTime = 120, labourTimer, lifepowerTimer;

	public ChunkSerializer chunkSerializer;
	public ColonyControllerSerializer colonyControllerSerializer;
	public DockStaticSerializer dockStaticSerializer;
	public CrewStaticSerializer crewStaticSerializer;
	public ShuttleStaticSerializer shuttleStaticSerializer;
	public QuestStaticSerializer questStaticSerializer;
	public ExpeditionStaticSerializer expeditionStaticSerializer;
	public float recruiting_hireCost;

	// все, что можно - в классы - сериализаторы
}
