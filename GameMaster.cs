using System.Collections.Generic; // листы
using UnityEngine; // классы Юнити
using System.IO; // чтение-запись файлов
using System.Runtime.Serialization.Formatters.Binary; // конверсия в поток байтов и обратно
using UnityEngine.SceneManagement;

public struct GameStartSettings  {
    public byte chunkSize;
    public ChunkGenerationMode generationMode;
    public Difficulty difficulty;
    public float terrainRoughness;
    public static readonly GameStartSettings Empty;
    static GameStartSettings()
    {
        Empty = new GameStartSettings(ChunkGenerationMode.Standart, 16, Difficulty.Normal, 0.3f);
    }
    public GameStartSettings(ChunkGenerationMode i_genMode, byte i_chunkSize, Difficulty diff, float i_terrainRoughness)
    {
        generationMode = i_genMode;
        chunkSize = i_chunkSize;
        difficulty = diff;
        terrainRoughness = i_terrainRoughness;
    }
    public GameStartSettings(ChunkGenerationMode i_genMode)
    {
        generationMode = i_genMode;
        chunkSize = 8;
        difficulty = Difficulty.Normal;
        terrainRoughness = 0.3f;
    }
    }

public enum Difficulty{Utopia, Easy, Normal, Hard, Torture}
public enum GameStart {Nothing, Zeppelin, Headquarters}
public enum WorkType {Nothing, Digging, Pouring, Manufacturing, Clearing, Gathering, Mining, Farming, MachineConstructing}
public enum GameLevel { Menu, Playable, Editor}

/// -----------------------------------------------------------------------------

public sealed class GameMaster : MonoBehaviour {
	public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}
    public static bool sceneClearing { get; private set; }
    public static bool editMode = false;
    public static string savename { get; protected set; }
    public static float LUCK_COEFFICIENT { get; private set; }
    public static float sellPriceCoefficient = 0.75f;
    public static int layerCutHeight = 16, prevCutHeight = 16;
    public static Vector3 sceneCenter { get { return Vector3.one * Chunk.CHUNK_SIZE / 2f; } } // SCENE CENTER
    public static GameStartSettings gameStartSettings = GameStartSettings.Empty;
    public static Difficulty difficulty { get; private set; }
    public static Chunk mainChunk { get; private set; } 
	public static ColonyController colonyController{get;private set;}
	public static GeologyModule geologyModule;

	public LineRenderer systemDrawLR;
    public Constructor constructor;
    public delegate void StructureUpdateHandler();
    public event StructureUpdateHandler labourUpdateEvent, lifepowerUpdateEvent;
    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;
    public Vector2 windVector { get; private set; }    

    public const int LIFEPOWER_PER_BLOCK = 130; // 200
	public const int LIFEPOWER_SPREAD_SPEED = 10,  CRITICAL_DEPTH = - 200;
    public const float START_HAPPINESS = 1, GEARS_ANNUAL_DEGRADE = 0.1f, LIFE_DECAY_SPEED = 0.1f, DAY_LONG = 60, CAM_LOOK_SPEED = 10,
    START_BIRTHRATE_COEFFICIENT = 0.001f, HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000;
    public const float LIFEPOWER_TICK = 1, LABOUR_TICK = 0.25f; // cannot be zero
    public const int START_WORKERS_COUNT = 70, MAX_LIFEPOWER_TRANSFER = 16, SURFACE_MATERIAL_REPLACE_COUNT = 256;

    public GameStart startGameWith = GameStart.Zeppelin;

    public float lifeGrowCoefficient {get;private set;}
	public float demolitionLossesPercent {get;private set;}
	public float lifepowerLossesPercent{get;private set;}
	public float tradeVesselsTrafficCoefficient{get;private set;}
	public float upgradeDiscount{get;private set;}
	public float upgradeCostIncrease{get;private set;}
	public float environmentalConditions{get; private set;} // 0 is hell, 1 is very favourable
	public float warProximity{get;private set;} // 0 is far, 1 is nearby  
    
	private float diggingSpeed = 0.5f, pouringSpeed = 0.5f, manufacturingSpeed = 0.3f, 
	clearingSpeed = 20, gatheringSpeed = 0.1f, miningSpeed = 1, machineConstructingSpeed = 1;    
	private float timeGone;
	private uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
	public const byte DAYS_IN_WEEK = 7, WEEKS_IN_MONTH = 4, MONTHS_IN_YEAR = 12;
    
    private float labourTimer = 0, lifepowerTimer = 0;    
    private float windTimer = 0, windChangeTime = 120;
    private bool firstSet = true;

	// FOR TESTING
	public float newGameSpeed = 1;
	public bool weNeedNoResources { get; private set; }
	public bool generateChunk = true;
    public byte test_size = 100;
    public bool _editMode = false;
         
    public static void SetSavename(string s)
    {
        savename = s;
    }
    public static void SetMainChunk(Chunk c) { mainChunk = c; }

    public void ChangeModeToPlay()
    {
        if (!editMode) return;
        _editMode = false;
        firstSet = true;
        gameStartSettings.generationMode = ChunkGenerationMode.DontGenerate;
        startGameWith = GameStart.Zeppelin;
        Awake();
        Start();
    }

    private void Awake() {
        if (realMaster != null & realMaster != this)
        {
            Destroy(this);
            return;
        }
        realMaster = this;
        sceneClearing = false;
	}

	void Start() {
        if (!firstSet) return;
        gameSpeed = 1;

        editMode = _editMode;
        if (!editMode)
        {            
            lifeGrowCoefficient = 1;
            //Localization.ChangeLanguage(Language.English);

            if (geologyModule == null) geologyModule = gameObject.AddComponent<GeologyModule>();
            difficulty = gameStartSettings.difficulty;
            if (colonyController == null)
            {
                colonyController = gameObject.AddComponent<ColonyController>();
                colonyController.CreateStorage();
            }
            if (PoolMaster.current == null)
            {
                PoolMaster pm = gameObject.AddComponent<PoolMaster>();
                pm.Load();
            }
            //byte chunksize = gss.chunkSize;
            byte chunksize;
            chunksize = gameStartSettings.chunkSize;
            if (gameStartSettings.generationMode != ChunkGenerationMode.GameLoading)
            {
                if (gameStartSettings.generationMode != ChunkGenerationMode.DontGenerate)
                {
                    if (gameStartSettings.generationMode != ChunkGenerationMode.TerrainLoading)
                    {
                        constructor.ConstructChunk(chunksize, gameStartSettings.generationMode);
                    }
                    else LoadTerrain(SaveSystemUI.GetTerrainsPath() + '/' + savename + '.' + SaveSystemUI.TERRAIN_FNAME_EXTENSION);
                }

                FollowingCamera.CenterCamera(sceneCenter);
                FollowingCamera.main.ResetTouchRightBorder();
                FollowingCamera.camRotationBlocked = false;

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
                        List<SurfaceBlock> sblocks = mainChunk.surfaceBlocks;
                        SurfaceBlock sb = sblocks[(int)(Random.value * (sblocks.Count - 1))];
                        int xpos = sb.pos.x;
                        int zpos = sb.pos.z;

                        if (colonyController == null) colonyController = gameObject.AddComponent<ColonyController>();

                        //Structure s = Structure.GetStructureByID(Structure.LANDED_ZEPPELIN_ID);
                        Structure s = Structure.GetStructureByID(Structure.HQ_4_ID);                        

                        SurfaceBlock b = mainChunk.GetSurfaceBlock(xpos, zpos);
                        s.SetBasement(b, PixelPosByte.zero);
                        b.MakeIndestructible(true);
                        b.myChunk.GetBlock(b.pos.x, b.pos.y - 1, b.pos.z).MakeIndestructible(true);
                        //test
                       HeadQuarters hq = s as HeadQuarters;
                        weNeedNoResources = true;
                        hq.LevelUp(false);
                        hq.LevelUp(false);
                        Crew ncrew = new Crew();
                        ncrew.SetCrew(colonyController, 100);
                        Crew.freeCrewsList.Add(ncrew);
                        ncrew = new Crew();
                        ncrew.SetCrew(colonyController, 100);
                        Crew.freeCrewsList.Add(ncrew);
                        ncrew = new Crew();
                        ncrew.SetCrew(colonyController, 100);
                        Crew.freeCrewsList.Add(ncrew);
                        //


                        sb = mainChunk.GetSurfaceBlock(xpos - 1, zpos + 1);
                        if (sb == null)
                        {
                            sb = mainChunk.GetSurfaceBlock(xpos, zpos + 1);
                            if (sb == null)
                            {
                                sb = mainChunk.GetSurfaceBlock(xpos + 1, zpos + 1);
                                if (sb == null)
                                {
                                    sb = mainChunk.GetSurfaceBlock(xpos - 1, zpos);
                                    if (sb == null)
                                    {
                                        sb = mainChunk.GetSurfaceBlock(xpos + 1, zpos);
                                        if (sb == null)
                                        {
                                            sb = mainChunk.GetSurfaceBlock(xpos - 1, zpos - 1);
                                            if (sb == null)
                                            {
                                                sb = mainChunk.GetSurfaceBlock(xpos, zpos - 1);
                                                if (sb == null)
                                                {
                                                    sb = mainChunk.GetSurfaceBlock(xpos + 1, zpos - 1);
                                                    if (sb == null)
                                                    {
                                                        print("bad generation, do something!");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        StorageHouse firstStorage = Structure.GetStructureByID(Structure.STORAGE_0_ID) as StorageHouse;
                        firstStorage.SetBasement(sb, PixelPosByte.zero);                        

                        //UI ui = gameObject.AddComponent<UI>();
                        //ui.lineDrawer = systemDrawLR;
                        break;
                }
                FollowingCamera.main.WeNeedUpdate();
                //start resources
                colonyController.AddCitizens(START_WORKERS_COUNT);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 50);
                colonyController.storage.AddResource(ResourceType.metal_E, 20);
                colonyController.storage.AddResource(ResourceType.Plastics, 100);
                colonyController.storage.AddResource(ResourceType.Food, 200);
            }
            else LoadGame(SaveSystemUI.GetSavesPath() + '/' + savename + ".sav");
            if (savename == null | savename == string.Empty) savename = "autosave";
        }
        else
        {
            gameObject.AddComponent<PoolMaster>().Load();           
            mainChunk = new GameObject("chunk").AddComponent<Chunk>();
            int size = 16;
            int[,,] blocksArray = new int[size,size,size];
            size /= 2;
            blocksArray[size,size,size] = ResourceType.STONE_ID;
            mainChunk.SetChunk(blocksArray); // временно, потом сделать возможность выбирать размер

            FollowingCamera.CenterCamera(sceneCenter);
        }
	}

    #region updates
    private void Update()
    {
        //testzone
        if (gameSpeed != newGameSpeed) gameSpeed = newGameSpeed;
        if (Input.GetKeyDown("x")) mainChunk.TakeLifePowerWithForce(1000);

        if (Input.GetKeyDown("m")) { layerCutHeight = 0; mainChunk.LayersCut(); }
        // eo testzone

        //float frameTime = Time.deltaTime * gameSpeed;
    }

    private void FixedUpdate()
    {
        if (gameSpeed != 0 & !editMode)
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
                if (lifepowerUpdateEvent != null) lifepowerUpdateEvent();
            }
        }
    }  
    #endregion

    public float CalculateWorkspeed(int workersCount, WorkType type) {
		if (colonyController == null) return 0;
		float workspeed = workersCount * colonyController.labourEfficientcy_coefficient * (colonyController.gears_coefficient + colonyController.health_coefficient + colonyController.happiness_coefficient - 2);
        if (workspeed < 0) workspeed = 0.01f;
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
        gms.windVector_x = windVector.x;
        gms.windVector_z = windVector.y;

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
        string path = SaveSystemUI.GetSavesPath() + '/';
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        FileStream fs = File.Create(path + name + '.' + SaveSystemUI.SAVE_FNAME_EXTENSION);
        savename = name;
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(fs, gms);
		fs.Close();
		Time.timeScale = 1;
		return true;
	}
    public bool LoadGame() { return LoadGame("autosave"); }
    public bool LoadGame(string fullname)
    {  // отдельно функцию проверки и коррекции сейв-файла
        if (true) // <- тут будет функция проверки
        {
            Time.timeScale = 0; gameSpeed = 0;
            // ОЧИСТКА
            StopAllCoroutines();
            if (mainChunk != null) mainChunk.ClearChunk();
            // очистка подписчиков на ивенты невозможна, сами ивенты к этому моменту недоступны
            Crew.Reset(); Shuttle.Reset();
            Grassland.ScriptReset();
            Expedition.GameReset();
            Structure.ResetToDefaults_Static(); // все наследуемые resetToDefaults внутри
            colonyController.ResetToDefaults(); // подчищает все списки
            FollowingCamera.main.ResetLists();
            //UI.current.Reset();


            // НАЧАЛО ЗАГРУЗКИ
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fullname, FileMode.Open);
            
            GameMasterSerializer gms = (GameMasterSerializer)bf.Deserialize(file);
            file.Close();
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

            windVector = new Vector2(gms.windVector_x, gms.windVector_z);
            windTimer = gms.windTimer; windChangeTime = gms.windChangeTime;
            lifepowerTimer = gms.lifepowerTimer;
            labourTimer = gms.labourTimer;
            #endregion
            RecruitingCenter.SetHireCost(gms.recruiting_hireCost);
            Crew.LoadStaticData(gms.crewStaticSerializer);
            Shuttle.LoadStaticData(gms.shuttleStaticSerializer); // because of hangars

            if (mainChunk == null)
            {
                GameObject g = new GameObject("chunk");
                mainChunk = g.AddComponent<Chunk>();
            }
            mainChunk.LoadChunkData(gms.chunkSerializer);
            colonyController.Load(gms.colonyControllerSerializer); // < --- COLONY CONTROLLER

            Dock.LoadStaticData(gms.dockStaticSerializer);
            QuestUI.current.Load(gms.questStaticSerializer);
            Expedition.LoadStaticData(gms.expeditionStaticSerializer);

            FollowingCamera.main.WeNeedUpdate();
            Time.timeScale = 1; gameSpeed = 1;

            savename = fullname;
            return true;
        }
        else
        {
            UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed));
            return false;
        }
    }

    public bool SaveTerrain(string name)
    {
        string path = SaveSystemUI.GetTerrainsPath() + '/';
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        FileStream fs = File.Create(path + name + '.' + SaveSystemUI.TERRAIN_FNAME_EXTENSION);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, mainChunk.SaveChunkData());
        fs.Close();
        return true;
    }
    public bool LoadTerrain(string fullname)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(fullname, FileMode.Open);
        ChunkSerializer cs = (ChunkSerializer)bf.Deserialize(file);
        file.Close();
        if (mainChunk == null)
        {
            GameObject g = new GameObject("chunk");
            mainChunk = g.AddComponent<Chunk>();
        }
        mainChunk.LoadChunkData(cs);
        FollowingCamera.main.WeNeedUpdate();
        return true;
    }

    

	public static void DeserializeByteArray<T>( byte[] data, ref T output ) {
		using (MemoryStream stream = new MemoryStream(data))
		{
			output = (T)System.Convert.ChangeType(new BinaryFormatter().Deserialize(stream), typeof(T));
		}
	}
    #endregion

    //test
    public void OnGUI()
    {
        Rect r = new Rect(0, Screen.height - 16, 200, 16);
        GUI.Box(r, GUIContent.none);
        weNeedNoResources = GUI.Toggle(r, weNeedNoResources, "unlimited resources");
    }

    public void OnApplicationQuit()
    {
        StopAllCoroutines();
        sceneClearing = true;
    }

    public static void ChangeScene(GameLevel level)
    {
        sceneClearing = true;
        SceneManager.LoadScene((int)level);
        sceneClearing = false;
        Structure.ResetToDefaults_Static();
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
	public uint day = 0, week = 0, month = 0, year = 0, millenium = 0;
    public float t;
    public float windVector_x, windVector_z; // cause serialization error
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
