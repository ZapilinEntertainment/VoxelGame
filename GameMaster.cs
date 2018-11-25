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

public enum Difficulty : byte {Utopia, Easy, Normal, Hard, Torture}
public enum GameStart : byte {Nothing, Zeppelin, Headquarters}
public enum WorkType : byte {Nothing, Digging, Pouring, Manufacturing, Clearing, Gathering, Mining, Farming, MachineConstructing}
public enum GameLevel : byte { Menu, Playable, Editor}

/// -----------------------------------------------------------------------------

public sealed class GameMaster : MonoBehaviour {
	public static  GameMaster realMaster;
	public static float gameSpeed  {get; private set;}
    public static bool sceneClearing { get; private set; }
    public static bool editMode = false;
    public static bool loading { get; private set; }
    public static bool soundEnabled { get; private set; }
    public static string savename { get; private set; }
    public static float LUCK_COEFFICIENT { get; private set; }
    public static float sellPriceCoefficient = 0.75f;
    public static int layerCutHeight = 16, prevCutHeight = 16;

    public static Vector3 sceneCenter { get { return Vector3.one * Chunk.CHUNK_SIZE / 2f; } } // SCENE CENTER
    public static GameStartSettings gameStartSettings = GameStartSettings.Empty;
    public static Difficulty difficulty { get; private set; }
    public static Chunk mainChunk { get; private set; } 
	public static ColonyController colonyController{get;private set;}
	public static GeologyModule geologyModule;
    public static Audiomaster audiomaster;

    private static byte pauseRequests = 0;

    public Constructor constructor;
    public delegate void StructureUpdateHandler();
    public event StructureUpdateHandler labourUpdateEvent, lifepowerUpdateEvent;
    public delegate void WindChangeHandler(Vector2 newVector);
    public event WindChangeHandler WindUpdateEvent;
    public Vector2 windVector { get; private set; }      

    public GameStart startGameWith = GameStart.Zeppelin;

    public float lifeGrowCoefficient {get;private set;}
	public float demolitionLossesPercent {get;private set;}
	public float lifepowerLossesPercent{get;private set;}
	public float tradeVesselsTrafficCoefficient{get;private set;}
	public float upgradeDiscount{get;private set;}
	public float upgradeCostIncrease{get;private set;}
	public float environmentalConditions{get; private set;} // 0 is hell, 1 is very favourable
	public float warProximity{get;private set;} // 0 is far, 1 is nearby  
    public float gearsDegradeSpeed { get; private set; }
    
	private const float diggingSpeed = 0.5f, pouringSpeed = 0.5f, manufacturingSpeed = 0.3f, 
	clearingSpeed = 5, gatheringSpeed = 0.1f, miningSpeed = 1, machineConstructingSpeed = 1;
    //data
    private float timeGone;
    public byte day { get; private set; }
    public byte month { get; private set; }
    public uint year { get; private set; }
	public const byte DAYS_IN_MONTH = 30, MONTHS_IN_YEAR = 12;
    public const float DAY_LONG = 60;
    // updating
    public const float LIFEPOWER_TICK = 1, LABOUR_TICK = 0.25f; // cannot be zero
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

    public static void SetPause(bool pause)
    {
        if (pause)
        {
            pauseRequests++;
            Time.timeScale = 0;
            gameSpeed = 0;
        }
        else
        {
            if (pauseRequests > 0) pauseRequests--;
            if (pauseRequests == 0)
            {
                Time.timeScale = 1;
                gameSpeed = 1;
            }
        }
    }

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
        Time.timeScale = 1;
        gameSpeed = 1;
        pauseRequests = 0;
        audiomaster = gameObject.AddComponent<Audiomaster>();
        audiomaster.Prepare();

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
                        gearsDegradeSpeed = 0;
                        break;
                    case Difficulty.Easy:
                        LUCK_COEFFICIENT = 0.7f;
                        demolitionLossesPercent = 0.2f;
                        lifepowerLossesPercent = 0.1f;
                        sellPriceCoefficient = 0.9f;
                        tradeVesselsTrafficCoefficient = 0.4f;
                        upgradeDiscount = 0.3f; upgradeCostIncrease = 1.3f;
                        environmentalConditions = 1;
                        gearsDegradeSpeed = 0.00001f;
                        break;
                    case Difficulty.Normal:
                        LUCK_COEFFICIENT = 0.5f;
                        demolitionLossesPercent = 0.4f;
                        lifepowerLossesPercent = 0.3f;
                        sellPriceCoefficient = 0.75f;
                        tradeVesselsTrafficCoefficient = 0.5f;
                        upgradeDiscount = 0.25f; upgradeCostIncrease = 1.5f;
                        environmentalConditions = 0.95f;
                        gearsDegradeSpeed = 0.00002f;
                        break;
                    case Difficulty.Hard:
                        LUCK_COEFFICIENT = 0.1f;
                        demolitionLossesPercent = 0.7f;
                        lifepowerLossesPercent = 0.5f;
                        sellPriceCoefficient = 0.5f;
                        tradeVesselsTrafficCoefficient = 0.75f;
                        upgradeDiscount = 0.2f; upgradeCostIncrease = 1.7f;
                        environmentalConditions = 0.9f;
                        gearsDegradeSpeed = 0.00003f;
                        break;
                    case Difficulty.Torture:
                        LUCK_COEFFICIENT = 0.01f;
                        demolitionLossesPercent = 1;
                        lifepowerLossesPercent = 0.85f;
                        sellPriceCoefficient = 0.33f;
                        tradeVesselsTrafficCoefficient = 1;
                        upgradeDiscount = 0.1f; upgradeCostIncrease = 2f;
                        environmentalConditions = 0.8f;
                        gearsDegradeSpeed = 0.00005f;
                        break;
                }
                warProximity = 0.01f;
                layerCutHeight = Chunk.CHUNK_SIZE; prevCutHeight = layerCutHeight;
                if (colonyController == null) colonyController = gameObject.AddComponent<ColonyController>();
                switch (startGameWith)
                {
                    case GameStart.Zeppelin:
                        Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
                        break;

                    case GameStart.Headquarters:
                        List<SurfaceBlock> sblocks = mainChunk.surfaceBlocks;
                        SurfaceBlock sb = sblocks[(int)(Random.value * (sblocks.Count - 1))];
                        int xpos = sb.pos.x;
                        int zpos = sb.pos.z;                       

                        Structure s = Structure.GetStructureByID(Structure.LANDED_ZEPPELIN_ID);
                        //Structure s = Structure.GetStructureByID(Structure.HQ_4_ID);                        

                        SurfaceBlock b = mainChunk.GetSurfaceBlock(xpos, zpos);
                        s.SetBasement(b, PixelPosByte.zero);
                        b.MakeIndestructible(true);
                        b.myChunk.GetBlock(b.pos.x, b.pos.y - 1, b.pos.z).MakeIndestructible(true);
                        //test
                       //HeadQuarters hq = s as HeadQuarters;
                       //weNeedNoResources = true;
                       // hq.LevelUp(false);
                       // hq.LevelUp(false);
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
                        SetStartResources();
                        //UI ui = gameObject.AddComponent<UI>();
                        //ui.lineDrawer = systemDrawLR;
                        break;
                }
                FollowingCamera.main.WeNeedUpdate();                
            }
            else LoadGame(SaveSystemUI.GetSavesPath() + '/' + savename + ".sav");
            if (savename == null | savename == string.Empty) savename = "autosave";
        }
        else
        {
            gameObject.AddComponent<PoolMaster>().Load();           
            mainChunk = new GameObject("chunk").AddComponent<Chunk>();
            int size = Chunk.CHUNK_SIZE;
            int[,,] blocksArray = new int[size,size,size];
            size /= 2;
            blocksArray[size,size,size] = ResourceType.STONE_ID;
            mainChunk.CreateNewChunk(blocksArray); 
        }

        { // set look point
            FollowingCamera.camBasisTransform.position = sceneCenter;
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
            float fixedTime = Time.fixedDeltaTime * gameSpeed;
            timeGone += fixedTime;

            if (timeGone >= DAY_LONG)
            {
                uint daysDelta = (uint)(timeGone / DAY_LONG);
                if (daysDelta > 0 & colonyController != null)
                {
                    colonyController.EverydayUpdate(daysDelta);
                }
                uint sum = day + daysDelta;
                if (sum >=  DAYS_IN_MONTH)
                {
                    day = (byte)(sum % DAYS_IN_MONTH);
                    sum /= DAYS_IN_MONTH;
                    sum += month;
                    if (sum >= MONTHS_IN_YEAR)
                    {
                        month = (byte)(sum % MONTHS_IN_YEAR);
                        year = sum / MONTHS_IN_YEAR;
                    }
                    else month = (byte)sum;
                }
                else
                {
                    day = (byte)sum;
                }
                timeGone = timeGone % DAY_LONG;
            }
            
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

    public void SetStartResources()
    {
        //start resources
        switch (difficulty)
        {
            case Difficulty.Utopia:
                colonyController.AddCitizens(100);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 100);
                colonyController.storage.AddResource(ResourceType.metal_E, 50);
                colonyController.storage.AddResource(ResourceType.metal_N, 1);
                colonyController.storage.AddResource(ResourceType.Plastics, 200);
                colonyController.storage.AddResource(ResourceType.Food, 1000);
                break;
            case Difficulty.Easy:
                colonyController.AddCitizens(70);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 60);
                colonyController.storage.AddResource(ResourceType.metal_E, 30);
                colonyController.storage.AddResource(ResourceType.Plastics, 150);
                colonyController.storage.AddResource(ResourceType.Food, 500);
                break;
            case Difficulty.Normal:
                colonyController.AddCitizens(50);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 50);
                colonyController.storage.AddResource(ResourceType.metal_E, 20);
                colonyController.storage.AddResource(ResourceType.Plastics, 100);
                colonyController.storage.AddResource(ResourceType.Food, 250);
                break;
            case Difficulty.Hard:
                colonyController.AddCitizens(40);
                colonyController.storage.AddResource(ResourceType.metal_K, 50);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 2);
                colonyController.storage.AddResource(ResourceType.Plastics, 10);
                colonyController.storage.AddResource(ResourceType.Food, 200);
                break;
            case Difficulty.Torture:
                colonyController.AddCitizens(30);
                colonyController.storage.AddResource(ResourceType.metal_K, 40);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 10);
                colonyController.storage.AddResource(ResourceType.Food, 200);
                break;
        }
        
    }

    #region save-load system
    public bool SaveGame() { return SaveGame("autosave"); }
	public bool SaveGame( string name ) { // заменить потом на persistent -  постоянный путь
        SetPause(true);
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
		gms.day = day; gms.month = month; gms.year = year; gms.t = timeGone;
        gms.windVector_x = windVector.x;
        gms.windVector_z = windVector.y;
        gms.gearsDegradeSpeed = gearsDegradeSpeed;

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
        SetPause(false);
		return true;
	}
    public bool LoadGame() { return LoadGame("autosave"); }
    public bool LoadGame(string fullname)
    {  // отдельно функцию проверки и коррекции сейв-файла
        if (true) // <- тут будет функция проверки
        {
            SetPause(true); 
            loading = true;
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
            day = gms.day;  month = gms.month; year = gms.year; timeGone = gms.t;
            gearsDegradeSpeed = gms.gearsDegradeSpeed;

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
            loading = false;
            savename = fullname;
            SetPause(false);

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
        Time.timeScale = 1;
        gameSpeed = 1;
        pauseRequests = 0;
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
	tradeVesselsTrafficCoefficient, upgradeDiscount, upgradeCostIncrease, environmentalConditions, warProximity, gearsDegradeSpeed;
	public Difficulty difficulty;
	public GameStart startGameWith;
	public int prevCutHeight = 16;
    public byte day = 0, month = 0;
    public uint year = 0;
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
