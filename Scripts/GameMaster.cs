using System.Collections.Generic; // листы
using UnityEngine; // классы Юнити
using System.IO; // чтение-запись файлов
using UnityEngine.SceneManagement;

public enum Difficulty : byte {Utopia, Easy, Normal, Hard, Torture}
//dependencies:
// GameConstants.GetBlackoutStabilityTestHardness
// Lightning.CalculateDamage
// Monument.ArtifactStabilityTest
// RecruitingCenter.GetHireCost
//ScoreCalculator
// StabilityEnforcer - LabourUpdate

public enum StartFoundingType : byte { Nothing,Zeppelin, Headquarters}
public enum GameMode: byte { MainMenu, Survival, Scenario, Editor }
    //dependence - equality check
public enum GameEndingType : byte { Default, ColonyLost, ConsumedByReal, ConsumedByLastSector, FoundationRoute,
    CloudWhaleRoute, EngineRoute, PipesRoute, CrystalRoute, MonumentRoute, BlossomRoute, PollenRoute, HimitsuRoute}
//dependence - localization
/// -----------------------------------------------------------------------------

public sealed class GameMaster : MonoBehaviour
{
    private static GameStartSettings _applyingGameStartSettings; // нужна для change scene!
    public static GameMaster realMaster { get; private set; }
    public static float gameSpeed { get; private set; }
    public static bool sceneClearing { get; private set; }
    public static bool loading { get; private set; }
    public static bool loadingFailed; // hot
    public static bool soundEnabled { get; private set; }
    public static float LUCK_COEFFICIENT { get; private set; }
    public static float sellPriceCoefficient = 0.75f;
    public static byte layerCutHeight = 16, prevCutHeight = 16;

    public static float stability {
        get {
            if (realMaster != null)
            {
                var em = realMaster.environmentMaster;
                if (em != null) return em.islandStability;
            }
            return 0.5f;
        }
    }

    public static Vector3 sceneCenter { get { return Vector3.one * Chunk.chunkSize / 2f; } } // SCENE CENTER 
    public static GeologyModule geologyModule;
    public static Audiomaster audiomaster;

    private static byte pauseRequests = 0;

    public Chunk mainChunk { get; private set; }
    public ColonyController colonyController { get; private set; }
    public EnvironmentMaster environmentMaster { get; private set; }
    public EventChecker eventTracker { get; private set; }
    public GameMode gameMode { get; private set; }
    public GlobalMap globalMap { get; private set; }
    private UIController uicontroller;
    private GameStartSettings startSettings;
    private Scenario executingScenario;
    public GameRules gameRules { get; private set; }

    public event System.Action labourUpdateEvent, blockersRestoreEvent, everydayUpdate,
        afterloadRecalculationEvent;
    public static event System.Action staticResetFunctions;

    public float lifeGrowCoefficient { get; private set; }
    public float demolitionLossesPercent { get; private set; }
    public float lifepowerLossesPercent { get; private set; }
    public float tradeVesselsTrafficCoefficient { get; private set; }
    public float upgradeDiscount { get; private set; }
    public float upgradeCostIncrease { get; private set; }
    public float warProximity { get; private set; } // 0 is far, 1 is nearby  
    public float gearsDegradeSpeed { get; private set; }
    private string currentSavename = string.Empty;

    private enum GameStatus : byte { Playing, Cinematic, Ended}
    private GameStatus gameStatus;

    public Difficulty difficulty { get; private set; }
    //data
    private int gameID = -1;
    private float timeGone;
    public byte day { get; private set; }
    public byte month { get; private set; }
    public uint year { get; private set; }
    public const byte DAYS_IN_MONTH = 30, MONTHS_IN_YEAR = 12;
    private const byte PLAY_SCENE_INDEX = 1, MENU_SCENE_INDEX = 0;
    public const float DAY_LONG = 60;
    // updating
    public const float LIFEPOWER_TICK = 1, LABOUR_TICK = 0.5f; // cannot be zero
    private float labourTimer = 0;
    private bool sessionPrepared = false;
    // FOR TESTING
    [SerializeField] private bool testMode = false;
    public bool IsInTestMode { get { return testMode; } }
    [SerializeField] private float _gameSpeed = 1f;
    public bool weNeedNoResources { get; private set; }
    private static GameStartSettings test_gameStartSettings = null;
    // GameStartSettings.GetDefaultStartSettings();
    // GameStartSettings.GetLoadingSettings(GameMode.Survival,"saved3");
    //
    private static bool DEBUG_STOP = false;
  
   // SCENERY CHANGING
    public static void StartNewGame(GameStartSettings gss)
    {
        if (realMaster == null)
        {
            _applyingGameStartSettings = gss;
            ChangeScene(PLAY_SCENE_INDEX);            
        }
        else
        {
            if (gss.NeedLoading())
            {
                realMaster.gameMode = gss.DefineGameMode();
                realMaster.LoadGame(gss.GetSavenameFullpath());
            }
            else
            {
                realMaster.ClearPreviousSessionData();
                _applyingGameStartSettings = gss;
                realMaster.Awake();
                realMaster.PrepareSession();
            }
        }
    }
    public static void ReturnToMainMenu()
    {
        ChangeScene(MENU_SCENE_INDEX);
    }
    private static void ChangeScene(byte index)
    {
        if (SceneManager.GetActiveScene().buildIndex == MENU_SCENE_INDEX)
        {
            var m = Component.FindObjectOfType<MenuUI>();
            if (m != null) m.ToLoadingView();
        }
        sceneClearing = true;        
        SceneManager.LoadScene(index);
        if (realMaster != null) ResetComponentsStaticValues();
        sceneClearing = false;
    }
    public void ChangePlayMode(GameStartSettings gss)
    {
        startSettings = null;
        _applyingGameStartSettings = gss;
        startSettings = null;
        sessionPrepared = false;

        Awake();
        PrepareSession();
    }
    //   

    private void Awake()
    {
        gameRules = GameRules.defaultRules;
        if (testMode && test_gameStartSettings != null)
        {
            _applyingGameStartSettings = test_gameStartSettings;
            test_gameStartSettings = null;
        }
        if (_applyingGameStartSettings == null && startSettings == null) _applyingGameStartSettings = GameStartSettings.GetDefaultStartSettings();
        if (startSettings == null)
        {
            startSettings = _applyingGameStartSettings;
            _applyingGameStartSettings = null;
        }
        gameMode = startSettings.DefineGameMode();
        if (gameMode == GameMode.MainMenu)
        {
            Destroy(gameObject);
            return;
        }
        if (realMaster != null & realMaster != this)
        {
            Destroy(realMaster);
            realMaster = this;
            return;
        }
        realMaster = this;
        sceneClearing = false;
        //
        uicontroller = UIController.GetCurrent();        
        //
        if (PoolMaster.current == null)
        {
            PoolMaster pm = gameObject.AddComponent(typeof(PoolMaster)) as PoolMaster;
           pm.Load();
        }
        if (gameMode == GameMode.Survival)
        {            
            if (globalMap == null) globalMap = gameObject.AddComponent<GlobalMap>();
            globalMap.Prepare();            
        }
        if (environmentMaster == null) environmentMaster = new GameObject("Environment master").AddComponent<EnvironmentMaster>();
        environmentMaster.Prepare();
        
        if (audiomaster == null)
        {
            audiomaster = gameObject.AddComponent<Audiomaster>();
            audiomaster.Prepare();
        }
        if (geologyModule == null) geologyModule = gameObject.AddComponent<GeologyModule>();
    }
    private void Start()
    {
        if (startSettings != null && startSettings.NeedLoading()) {
            LoadGame(startSettings.GetSavenameFullpath());
        }
        else PrepareSession();
    }
    void PrepareSession()
    {        
        if (sessionPrepared) return;
        bool activateEventTracker = false;
        switch (gameMode)
        {
            case GameMode.Survival:
                {
                    uicontroller.ChangeUIMode(UIMode.Standart, true);
                    difficulty = startSettings.DefineDifficulty();
                    SetDefaultValues();
                    var cgs = startSettings.GetChunkGenerationSettings();
                    var chunkAction = cgs.preparingActionMode;
                    if (chunkAction != ChunkPreparingAction.NoAction)
                    {
                        if (chunkAction == ChunkPreparingAction.Load)
                        {
                            LoadTerrain(SaveSystemUI.GetTerrainsPath() + '/' + cgs.GetTerrainName() + '.' + SaveSystemUI.TERRAIN_FNAME_EXTENSION);
                        }
                        else
                        {
                            mainChunk = Constructor.ConstructChunk(cgs);
                            var slist = mainChunk.GetSurfaces();
                            if (slist != null) geologyModule.SpreadMinerals(slist);
                            mainChunk.GetNature().FirstLifeformGeneration(Chunk.chunkSize * Chunk.chunkSize * 500f);
                        }
                    }
                    //
                    var fcm = FollowingCamera.main;
                    fcm.CameraToStartPosition();
                    fcm.ResetTouchRightBorder();
                    fcm.CameraRotationBlock(false);
                    warProximity = 0.01f;
                    layerCutHeight = Chunk.chunkSize; prevCutHeight = layerCutHeight;
                    //
                    switch (startSettings.DefineFoundingType())
                    {
                        case StartFoundingType.Zeppelin:
                            {
                                Zeppelin.CreateNew();
                                AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.SetLandingPoint));
                                break;
                            }
                        case StartFoundingType.Headquarters:
                            {
                                Plane sb = mainChunk.GetRandomSurface();
                                int xpos = sb.pos.x;
                                int zpos = sb.pos.z;

                                Structure s;
                                if (testMode)
                                {
                                    s = HeadQuarters.GetHQ(6);
                                    weNeedNoResources = true;
                                }
                                else
                                {
                                    weNeedNoResources = false;
                                    s = HeadQuarters.GetHQ(1);
                                }

                                Plane b = mainChunk.GetHighestSurfacePlane(xpos, zpos);
                                s.SetBasement(b, PixelPosByte.zero);


                                sb = mainChunk.GetNearestUnoccupiedSurface(b.pos);
                                StorageHouse firstStorage = Structure.GetStructureByID(Structure.STORAGE_0_ID) as StorageHouse;
                                firstStorage.SetBasement(sb, PixelPosByte.zero);
                                SetStartResources();
                                break;
                            }
                    }
                    fcm.WeNeedUpdate();
                    activateEventTracker = true;
                    break;
                }
            case GameMode.Editor:
                {
                    uicontroller.ChangeUIMode(UIMode.Editor, true);
                    mainChunk = new GameObject("chunk").AddComponent<Chunk>();
                    int size = Chunk.chunkSize;
                    int[,,] blocksArray = new int[size, size, size];
                    size /= 2;
                    blocksArray[size, size, size] = ResourceType.STONE_ID;
                    mainChunk.Rebuild(blocksArray);
                    FollowingCamera.main.CameraToStartPosition();
                    break;
                }
            case GameMode.Scenario:
                {
                    uicontroller.ChangeUIMode(UIMode.Standart, true);
                    switch (startSettings.GetScenarioType())
                    {
                        case ScenarioType.Embedded:
                            {
                                switch ((EmbeddedScenarioType)startSettings.GetSecondSubIndex())
                                {
                                    case EmbeddedScenarioType.Tutorial:
                                        {
                                            gameRules = GameRules.GetTutorialRules();
                                            LoadTerrain(SaveSystemUI.GetTerrainSaveFullpath(startSettings.GetSavename()));
                                            TutorialUI.Initialize();
                                            break;
                                        }
                                }
                                break;
                            }
                    }                    
                    activateEventTracker = true;
                    FollowingCamera.main.CameraToStartPosition();
                    break;
                }
        }
        if (activateEventTracker) eventTracker = new EventChecker();

        startSettings = null;
        sessionPrepared = true;
        if (testMode) AnnouncementCanvasController.MakeAnnouncement("game master loaded");
    }
    public ColonyController PrepareColonyController(bool assignNewGameID)
    {
        if (colonyController == null)
        {
            colonyController = GetComponent<ColonyController>();
            if (colonyController == null) colonyController = gameObject.AddComponent<ColonyController>();
            colonyController.Prepare();
        }
        environmentMaster.LinkColonyController(colonyController);
        uicontroller.GetMainCanvasController()?.LinkColonyController();
        if (gameID == -1)
        {
            var keyname = GameConstants.PP_GAMEID_PROPERTY;
            if (assignNewGameID)
            {
                if (PlayerPrefs.HasKey(keyname)) gameID = PlayerPrefs.GetInt(keyname);
                else gameID = Random.Range(int.MinValue, int.MaxValue);
                int g2 = gameID;
                if (g2 == int.MaxValue) g2 = int.MinValue; else g2++;
                PlayerPrefs.SetInt(keyname, g2);
                PlayerPrefs.Save();
            }
        }
        return colonyController;
    }

    private void SetDefaultValues()
    {        
        Time.timeScale = 1;
        gameSpeed = 1;
        pauseRequests = 0;        
        if (currentSavename == null || currentSavename == string.Empty) currentSavename = "autosave";
        if (gameMode != GameMode.Editor)
        {
            lifeGrowCoefficient = 1;
            switch (difficulty)
            {
                case Difficulty.Utopia:
                    LUCK_COEFFICIENT = 1;
                    demolitionLossesPercent = 0;
                    lifepowerLossesPercent = 0;
                    sellPriceCoefficient = 1;
                    tradeVesselsTrafficCoefficient = 2;
                    upgradeDiscount = 0.5f; upgradeCostIncrease = 1.1f;
                    gearsDegradeSpeed = 0;
                    break;
                case Difficulty.Easy:
                    LUCK_COEFFICIENT = 0.7f;
                    demolitionLossesPercent = 0.2f;
                    lifepowerLossesPercent = 0.1f;
                    sellPriceCoefficient = 0.9f;
                    tradeVesselsTrafficCoefficient = 1.5f;
                    upgradeDiscount = 0.3f; upgradeCostIncrease = 1.3f;
                    gearsDegradeSpeed = 0.00001f;
                    break;
                case Difficulty.Normal:
                    LUCK_COEFFICIENT = 0.5f;
                    demolitionLossesPercent = 0.4f;
                    lifepowerLossesPercent = 0.3f;
                    sellPriceCoefficient = 0.75f;
                    tradeVesselsTrafficCoefficient = 1;
                    upgradeDiscount = 0.25f; upgradeCostIncrease = 1.5f;
                    gearsDegradeSpeed = 0.00002f;
                    break;
                case Difficulty.Hard:
                    LUCK_COEFFICIENT = 0.1f;
                    demolitionLossesPercent = 0.7f;
                    lifepowerLossesPercent = 0.5f;
                    sellPriceCoefficient = 0.5f;
                    tradeVesselsTrafficCoefficient = 0.9f;
                    upgradeDiscount = 0.2f; upgradeCostIncrease = 1.7f;
                    gearsDegradeSpeed = 0.00003f;
                    break;
                case Difficulty.Torture:
                    LUCK_COEFFICIENT = 0.01f;
                    demolitionLossesPercent = 1;
                    lifepowerLossesPercent = 0.85f;
                    sellPriceCoefficient = 0.33f;
                    tradeVesselsTrafficCoefficient = 0.75f;
                    upgradeDiscount = 0.1f; upgradeCostIncrease = 2f;
                    gearsDegradeSpeed = 0.00005f;
                    break;
            }
            RenderSettings.skybox.SetFloat("_Saturation", 0.75f + 0.25f * GameConstants.START_HAPPINESS);
        }
        else RenderSettings.skybox.SetFloat("_Saturation", 1f);
        DockSystem.ResetRequest();
    }
    private void ClearPreviousSessionData()
    {
        // ОЧИСТКА
        StopAllCoroutines();
        if (Zeppelin.current != null)
        {
            Destroy(Zeppelin.current.gameObject);
        }
        if (executingScenario != null)
        {
            executingScenario.ClearScenarioDecorations();
            executingScenario = null;
        }
        mainChunk?.ClearChunk();
        Ship.DeleteShips();
        // очистка подписчиков на ивенты невозможна, сами ивенты к этому моменту недоступны
        ResetComponentsStaticValues();
        colonyController?.ResetToDefaults(); // подчищает все списки
        SetDefaultValues();
    }
    private static void ResetComponentsStaticValues()
    {
        if (staticResetFunctions != null)
        {
            staticResetFunctions.Invoke();
            staticResetFunctions = null;
        }
        DockSystem.ResetRequest();
        Crew.Reset();
        Expedition.GameReset();
    }

    #region updates
    private void Update()
    {
        if (loading | gameSpeed == 0) return;

        //if (Input.GetKeyDown("o")) mainChunk.GetNature()?.DEBUG_HaveGrasslandDublicates();
        //if (Input.GetKeyDown("n")) globalMap.ShowOnGUI();       
        gameSpeed = _gameSpeed;
    }
    private void FixedUpdate()
    {
        if (gameSpeed != 0)
        {
            float fixedTime = Time.fixedDeltaTime * gameSpeed;
            if (gameMode != GameMode.Editor)
            {
                labourTimer -= fixedTime;
                if (labourTimer <= 0)
                {
                    labourTimer = LABOUR_TICK;
                    if (labourUpdateEvent != null) labourUpdateEvent();
                }
                timeGone += fixedTime;

                if (timeGone >= DAY_LONG)
                {
                    uint daysDelta = (uint)(timeGone / DAY_LONG);
                    if (daysDelta > 0 & colonyController != null)
                    {
                        // счет количества дней в ускорении отменен
                        everydayUpdate();
                    }
                    uint sum = day + daysDelta;
                    if (sum >= DAYS_IN_MONTH)
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
            }
        }
    }
    #endregion

    #region game parameters
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
    public void SetStartResources()
    {
        //start resources
        switch (difficulty)
        {
            case Difficulty.Utopia:
                colonyController.AddCitizens(100, true);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 100);
                colonyController.storage.AddResource(ResourceType.metal_E, 50);
                colonyController.storage.AddResource(ResourceType.metal_N, 1);
                colonyController.storage.AddResource(ResourceType.Plastics, 200);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * 1.5f * DAYS_IN_MONTH);
                break;
            case Difficulty.Easy:
                colonyController.AddCitizens(70, true);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 60);
                colonyController.storage.AddResource(ResourceType.metal_E, 30);
                colonyController.storage.AddResource(ResourceType.Plastics, 150);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH);
                break;
            case Difficulty.Normal:
                colonyController.AddCitizens(50, true);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 50);
                colonyController.storage.AddResource(ResourceType.metal_E, 20);
                colonyController.storage.AddResource(ResourceType.Plastics, 100);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH * 0.75f);
                break;
            case Difficulty.Hard:
                colonyController.AddCitizens(40, true);
                colonyController.storage.AddResource(ResourceType.metal_K, 50);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 2);
                colonyController.storage.AddResource(ResourceType.Plastics, 10);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH * 0.55f);
                break;
            case Difficulty.Torture:
                colonyController.AddCitizens(30, true);
                colonyController.storage.AddResource(ResourceType.metal_K, 40);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 10);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH * 0.45f);
                break;
        }
        colonyController.storage.AddResources(ResourcesCost.GetCost(Structure.SETTLEMENT_CENTER_ID));        
    }
    public float GetDifficultyCoefficient()
    {
        // 0 - 1 only!
        switch(difficulty)
        {
            case Difficulty.Utopia: return 0.1f;
            case Difficulty.Easy: return 0.25f;
            case Difficulty.Hard: return 0.7f;
            case Difficulty.Torture: return 1f;
            default: return 0.5f;
        }
    } 
    public bool CanWeSaveTheGame()
    {
        return gameRules.gameCanBeSaved;
    }
    public bool UseQuestAutoCreating()
    {
        return gameRules.createNewQuests;
    }
    public void BindScenario(Scenario s)
    {
        executingScenario = s;
    }
    public void UnbindScenario(Scenario s)
    {
        if (s == executingScenario) executingScenario = null;
    }
    #endregion
    //test
    public void OnGUI()
    {
        Rect r = new Rect(0, Screen.height - 16, 200, 16);
        GUI.Box(r, GUIContent.none);
        weNeedNoResources = GUI.Toggle(r, weNeedNoResources, "unlimited resources");
        //if (GUILayout.Button("testMethod")) TestMethod();
    }
    public void SYSTEM_SetNoResourcesCheat(bool x) { weNeedNoResources = x;}
    //
    public void GameOver(GameEndingType endType)
    {
        if (gameStatus == GameStatus.Ended) return;
        gameStatus = GameStatus.Ended;
        ulong score = (ulong)ScoreCalculator.GetScore(this);
        uicontroller?.GameOver(endType, score);
        if (gameID != -1) Highscore.AddHighscore(new Highscore(gameID, colonyController.cityName, score, endType));
        SetPause(true);                              
    }
    public void ContinueGameAfterEnd()
    {
        if (gameStatus != GameStatus.Ended) return;
        uicontroller.ChangeUIMode(UIMode.Standart, true);
        uicontroller.GetMainCanvasController().FullReactivation();
        SetPause(false);
        gameStatus = GameStatus.Playing;
    }

    public void OnApplicationQuit()
    {
        StopAllCoroutines();
        sceneClearing = true;
        Time.timeScale = 1;
        gameSpeed = 1;
        pauseRequests = 0;
    }

#region save-load system
    public bool SaveGame() { return SaveGame("autosave"); }
    public bool SaveGame(string name)
    { // заменить потом на persistent -  постоянный путь
        SetPause(true);

        string path = SaveSystemUI.GetSavesPath() + '/';
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        FileStream fs = File.Create(path + name + '.' + SaveSystemUI.SAVE_FNAME_EXTENSION);
        currentSavename = name;
        //сразу передавать файловый поток для записи, чтобы не забивать озу
        #region gms mainPartFilling
        fs.Write(System.BitConverter.GetBytes(GameConstants.SAVE_SYSTEM_VERSION),0,4);
        fs.WriteByte((byte)gameMode);
        // start writing
        fs.Write(System.BitConverter.GetBytes(gameSpeed), 0, 4);
        fs.Write(System.BitConverter.GetBytes(lifeGrowCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(demolitionLossesPercent), 0, 4);
        fs.Write(System.BitConverter.GetBytes(lifepowerLossesPercent), 0, 4);
        fs.Write(System.BitConverter.GetBytes(LUCK_COEFFICIENT), 0, 4);
        fs.Write(System.BitConverter.GetBytes(sellPriceCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(tradeVesselsTrafficCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(upgradeDiscount), 0, 4);
        fs.Write(System.BitConverter.GetBytes(upgradeCostIncrease), 0, 4);
        fs.Write(System.BitConverter.GetBytes(warProximity), 0, 4); 
        //40
        fs.WriteByte((byte)difficulty);// 41
        fs.WriteByte(prevCutHeight); //42
        fs.WriteByte(day); // 43
        fs.WriteByte(month); //44
        fs.Write(System.BitConverter.GetBytes(year), 0, 4);
        fs.Write(System.BitConverter.GetBytes(timeGone), 0, 4);
        fs.Write(System.BitConverter.GetBytes(gearsDegradeSpeed), 0, 4);
        fs.Write(System.BitConverter.GetBytes(labourTimer), 0, 4);
        fs.Write(System.BitConverter.GetBytes(RecruitingCenter.GetHireCost()), 0, 4);
        fs.Write(System.BitConverter.GetBytes(gameID),0,4);
        //68 (+4) end
        #endregion

        DockSystem.SaveDockSystem(fs);
        if (globalMap != null)
        {
            fs.WriteByte(1);
            globalMap.Save(fs);
        }
        else fs.WriteByte(0);
        environmentMaster.Save(fs);
        Artifact.SaveStaticData(fs);
        Crew.SaveStaticData(fs);
        mainChunk.SaveChunkData(fs);
        colonyController.Save(fs); // <------- COLONY CONTROLLER

        QuestUI.current.Save(fs);        
        Expedition.SaveStaticData(fs);
        Knowledge.GetCurrent().Save(fs);

        if (executingScenario != null)
        {
            fs.WriteByte(1);
            executingScenario.Save(fs);
        }
        else fs.WriteByte(0);
        FollowingCamera.main.Save(fs);

        fs.Position = 0;
        double hashsum = GetHashSum(fs, false);
        fs.Write(System.BitConverter.GetBytes(hashsum),0,8);
        fs.Close();
        SetPause(false);
        return true;
    }
    public bool LoadGame() { return LoadGame("autosave"); }
    public bool LoadGame(string fullname)
    {
        bool debug_noresource = weNeedNoResources;
        FileStream fs = File.Open(fullname, FileMode.Open);        
        double realHashSum = GetHashSum(fs, true);
        var data = new byte[8];
        fs.Read(data, 0, 8);
        double readedHashSum = System.BitConverter.ToDouble(data, 0);
        string errorReason = "reason not stated";
        if (realHashSum == readedHashSum) 
        {
            fs.Position = 0;
            SetPause(true);
            loading = true;
           

            // НАЧАЛО ЗАГРУЗКИ   
            #region gms mainPartLoading            
            data = new byte[5];
            fs.Read(data, 0, data.Length);
            uint saveSystemVersion = System.BitConverter.ToUInt32(data, 0); // может пригодиться в дальнейшем
            gameMode = (GameMode)data[4];
            //
            if (sessionPrepared)
            {
                ClearPreviousSessionData();
            }
            else PrepareSession();
            //start reading
            data = new byte[68]; 
            fs.Read(data, 0, data.Length);
            gameSpeed = System.BitConverter.ToSingle(data, 0);
            lifeGrowCoefficient = System.BitConverter.ToSingle(data, 4);
            demolitionLossesPercent = System.BitConverter.ToSingle(data, 8);
            lifepowerLossesPercent = System.BitConverter.ToSingle(data, 12);
            LUCK_COEFFICIENT = System.BitConverter.ToSingle(data, 16);
            sellPriceCoefficient = System.BitConverter.ToSingle(data, 20);
            tradeVesselsTrafficCoefficient = System.BitConverter.ToSingle(data, 24);
            upgradeDiscount = System.BitConverter.ToSingle(data, 28);
            upgradeCostIncrease = System.BitConverter.ToSingle(data, 32);
            warProximity = System.BitConverter.ToSingle(data, 36);

            difficulty = (Difficulty)data[40];
            int i = 41;
            prevCutHeight = data[i++];
            day = data[i++];
            month = data[i++];

            year = System.BitConverter.ToUInt32(data, i); i += 4;            
            timeGone = System.BitConverter.ToSingle(data, i); i += 4;
            gearsDegradeSpeed = System.BitConverter.ToSingle(data, i); i += 4;
            labourTimer = System.BitConverter.ToSingle(data, i); i += 4;
            RecruitingCenter.SetHireCost(System.BitConverter.ToSingle(data, i)); i += 4;
            gameID = System.BitConverter.ToInt32(data, i); i += 4;
            #endregion

            DockSystem.LoadDockSystem(fs);
            var b = fs.ReadByte();
            if (b == 1) globalMap.Load(fs);            
            if (loadingFailed)
            {
                errorReason = "global map error";
                goto FAIL;
            }           

            environmentMaster.Load(fs);
            if (loadingFailed)
            {
                errorReason = "environment error";
                goto FAIL;
            }

            Artifact.LoadStaticData(fs); // crews & monuments
            if (loadingFailed)
            {
                errorReason = "artifacts load failure";
                goto FAIL;
            }
            
            Crew.LoadStaticData(fs);
            if (loadingFailed)
            {
                errorReason = "crews load failure";
                goto FAIL;
            }
            //
            if (colonyController == null) PrepareColonyController(false);            
            //
            if (mainChunk == null)
            {
                mainChunk = Chunk.InitializeChunk();
            }
            mainChunk.LoadChunkData(fs);
            if (loadingFailed)
            {
                errorReason = "chunk load failure";
                goto FAIL;
            }
            else
            {
                if (blockersRestoreEvent != null) blockersRestoreEvent();
            }
           
            Settlement.TotalRecalculation(); // Totaru Annihiration no imoto-chan
            if (loadingFailed)
            {
                errorReason = "settlements load failure";
                goto FAIL;
            }

            colonyController.Load(fs); // < --- COLONY CONTROLLER
            if (loadingFailed)
            {
                errorReason = "colony controller load failure";
                goto FAIL;
            }

            if (loadingFailed)
            {
                errorReason = "dock load failure";
                goto FAIL;
            }
            QuestUI.current.Load(fs);
            if (loadingFailed)
            {
                errorReason = "quest load failure";
                goto FAIL;
            }
            Expedition.LoadStaticData(fs);
            Knowledge.Load(fs);
            b = fs.ReadByte();
            if (b == 1) executingScenario = Scenario.StaticLoad(fs);
            FollowingCamera.main.Load(fs);
            fs.Close();

            FollowingCamera.main.WeNeedUpdate();
            loading = false;
            currentSavename = fullname;

            //Debug.Log("recalculation event");
            if (afterloadRecalculationEvent != null)
            {
                afterloadRecalculationEvent();
                afterloadRecalculationEvent = null;
            }            
            SetPause(false);
            //Debug.Log("power grid");
            colonyController.FORCED_PowerGridRecalculation();
            //Debug.Log("docks");
            colonyController.SYSTEM_DocksRecalculation();
            //Debug.Log("end");

            

            DEBUG_STOP = true;

            return true;
        }
        else
        {
            AnnouncementCanvasController.MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed) + " : hashsum incorrect");
            if (soundEnabled) audiomaster.Notify(NotificationSound.SystemError);
            SetPause(true);
            fs.Close();
            return false;
        }
        FAIL:
        AnnouncementCanvasController.MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed) + " : data corruption");
        if (soundEnabled) audiomaster.Notify(NotificationSound.SystemError);
        Debug.Log(errorReason);
        SetPause(true);
        fs.Close();
        if (debug_noresource) weNeedNoResources = true;
        return false;
    }

    public bool SaveTerrain(string name)
    {
        string path = SaveSystemUI.GetTerrainsPath() + '/';
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        FileStream fs = File.Create(path + name + '.' + SaveSystemUI.TERRAIN_FNAME_EXTENSION);
        fs.Write(System.BitConverter.GetBytes(GameConstants.SAVE_SYSTEM_VERSION), 0, 4);
        mainChunk.SaveChunkData(fs);
        fs.Close();
        return true;
    }
    public bool LoadTerrain(string fullname)
    {
        FileStream fs = File.Open(fullname, FileMode.Open);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        uint saveVersion = System.BitConverter.ToUInt32(data, 0);
        if (mainChunk == null)
        {
            GameObject g = new GameObject("chunk");
            mainChunk = g.AddComponent<Chunk>();
        }
        loading = true;
        mainChunk.LoadChunkData(fs);
        loading = false;
        fs.Close();
        FollowingCamera.main.WeNeedUpdate();
        return true;
    }

    public double GetHashSum(FileStream fs, bool ignoreLastEightBytes)
    {
        double hsum = 0d, full = 255d;
        int x = 0;
        long count = fs.Length;
        if (ignoreLastEightBytes) count -= 8; ;
        for (long i = 0; i < count; i++)
        {
            x = fs.ReadByte();
            hsum += x / full;            
        }
        return hsum;
    }

    public static void LoadingFail()
    {
        loadingFailed = true;
        SetPause(true);
        Debug.Log("loading failed");
    }
    #endregion
}

public sealed class GameRules
{
    public bool createNewQuests { get; private set; }
    public bool gameCanBeSaved { get; private set; }
    public float foodSpendRate { get; private set; }
    public static GameRules defaultRules { get { return new GameRules(); } }

    private GameRules()
    {
        createNewQuests = true;
        gameCanBeSaved = true;
        foodSpendRate = 1f;
    }   
    public static GameRules GetTutorialRules()
    {
        var gr = new GameRules();
        gr.createNewQuests = false;
        gr.gameCanBeSaved = false;
        gr.foodSpendRate = 0f;
        return gr;
    }
}  
