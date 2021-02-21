using System.Collections.Generic; // листы
using UnityEngine; // классы Юнити
using System.IO; // чтение-запись файлов
using UnityEngine.SceneManagement;

public sealed class GameStartSettings : MyObject  {
    private GameMode gameMode;
    private bool loadGame = false;
    private string savename;
    private byte subIndex0 = 255, subIndex1 = 255;
    private Difficulty difficulty = Difficulty.Normal;
    private ChunkGenerationSettings chunkGenerationSettings;

    private GameStartSettings() {  }
    public static GameStartSettings GetDefaultStartSettings() {
        var cgs = new GameStartSettings();
        cgs.gameMode = GameMode.Survival;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetDefaultSettings();
        return cgs;
    }
    public static GameStartSettings GetEditorStartSettings(byte i_size)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = GameMode.Editor;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetGenerationSettings(ChunkGenerationMode.EditorPreset, i_size);
        return cgs;
    }
    /// <summary>
    /// starts new editor session and loads terrain preset
    /// </summary>
    public static GameStartSettings GetEditorStartSettings(string i_savename)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = GameMode.Editor;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetLoadingSettings(i_savename);
        return cgs;
    }
    public static GameStartSettings GetStartSettings(GameMode i_mode, ChunkGenerationSettings i_chunkSettings, Difficulty i_difficulty, StartFoundingType i_start)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = i_mode;
        cgs.chunkGenerationSettings = i_chunkSettings;
        cgs.difficulty = i_difficulty;
        cgs.subIndex0 = (byte)i_start;
        return cgs;
    }
    public static GameStartSettings GetModeChangingSettings(GameMode newMode, Difficulty i_difficulty, StartFoundingType i_foundingType)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = newMode;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetNoActionSettings();
        cgs.difficulty = i_difficulty;
        cgs.subIndex0 = (byte)i_foundingType;
        return cgs;
    }
    public static GameStartSettings GetLoadingSettings(GameMode i_gameMode, string i_savename)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = i_gameMode;
        cgs.loadGame = true;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetNoActionSettings();
        cgs.savename = i_savename;
        return cgs;
    }

    public bool NeedLoading() { return loadGame; }
    public GameMode DefineGameMode()
    {
        return gameMode;
    }
    public Difficulty DefineDifficulty()
    {
        return difficulty;
    }
    public string GetSavename()
    {
        return savename ?? string.Empty;
    }
    public string GetSavenameFullpath()
    {
        if (savename != null)
        {
            switch (gameMode)
            {
                case GameMode.Survival:
                    if (chunkGenerationSettings.preparingActionMode != ChunkPreparingAction.Load)
                        return SaveSystemUI.GetGameSaveFullpath(savename);
                    else return SaveSystemUI.GetTerrainSaveFullpath(savename);
                case GameMode.Editor: return SaveSystemUI.GetTerrainSaveFullpath(savename);
            }
        }
        return string.Empty;
    }
    public StartFoundingType DefineFoundingType()
    {
        if (subIndex0 == 255) return StartFoundingType.Nothing;
        else
        {
            return (StartFoundingType)subIndex0;
        }
    }
    public ChunkGenerationSettings GetChunkGenerationSettings() { return chunkGenerationSettings ?? ChunkGenerationSettings.GetDefaultSettings(); }

    protected override bool IsEqualNoCheck(object obj)
    {
        var gss = (GameStartSettings)obj;
        if (gameMode == gss.gameMode )
        {
            if (gameMode == GameMode.MainMenu) return true;
            else {
                if (loadGame && gss.loadGame) return chunkGenerationSettings == gss.chunkGenerationSettings && savename == gss.savename;
                else {
                    if (gss.loadGame != loadGame) return false;
                    else {
                        switch (gameMode)
                        {
                            case GameMode.Survival:
                                return chunkGenerationSettings == gss.chunkGenerationSettings && difficulty == gss.difficulty && subIndex0 == gss.subIndex0;
                            case GameMode.Scenario:
                                return chunkGenerationSettings == gss.chunkGenerationSettings && subIndex0 == gss.subIndex0 && subIndex1 == gss.subIndex1 && difficulty == gss.difficulty && savename == gss.savename;
                            case GameMode.Editor:
                                return chunkGenerationSettings == gss.chunkGenerationSettings && savename == gss.savename;
                            default: return true;
                        }
                    }
                }
            }
        }
        else return false;
    }
}

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
    public ColonyController colonyController { get { return _colonyController; } }
    private ColonyController _colonyController;
    public EnvironmentMaster environmentMaster { get; private set; }
    public EventChecker eventTracker { get; private set; }
    public GameMode gameMode { get; private set; }
    public GlobalMap globalMap { get; private set; }
    private UIController uicontroller;
    private GameStartSettings startSettings;

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
    private GameStartSettings test_gameStartSettings = GameStartSettings.GetEditorStartSettings(16);
    //

    #region static functions
    //
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
    //
    public static void LoadingFail()
    {
        loadingFailed = true;
        SetPause(true);
        Debug.Log("loading failed");
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
    #endregion

    private void ClearPreviousSessionData()
    {
        // ОЧИСТКА
        StopAllCoroutines();
        if (Zeppelin.current != null)
        {
            Destroy(Zeppelin.current.gameObject);
        }
        mainChunk?.ClearChunk();
        // очистка подписчиков на ивенты невозможна, сами ивенты к этому моменту недоступны
        ResetComponentsStaticValues();
        colonyController?.ResetToDefaults(); // подчищает все списки
        SetDefaultValues();
    }
    public void ChangeModeToPlay()
    {
        if (gameMode != GameMode.Editor) return;
        startSettings = null;
        _applyingGameStartSettings = GameStartSettings.GetModeChangingSettings(GameMode.Survival, Difficulty.Normal, StartFoundingType.Zeppelin);
        uicontroller.ChangeUIMode(UIMode.Standart,true);
        sessionPrepared = false;
        
        Awake();
        PrepareSession();
    }

    private void Awake()
    {
        if (testMode && test_gameStartSettings != null) _applyingGameStartSettings = test_gameStartSettings;
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
            uicontroller.ChangeUIMode(UIMode.Standart, true);
            if (globalMap == null) globalMap = gameObject.AddComponent<GlobalMap>();
            globalMap.Prepare();
            eventTracker = new EventChecker();
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
        switch (gameMode)
        {
            case GameMode.Survival:
                {
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
                            mainChunk.GetNature().FirstLifeformGeneration(Chunk.chunkSize * Chunk.chunkSize * 500f);
                        }
                    }
                    //
                    FollowingCamera.main.ResetTouchRightBorder();
                    FollowingCamera.main.CameraRotationBlock(false);
                    warProximity = 0.01f;
                    layerCutHeight = Chunk.chunkSize; prevCutHeight = layerCutHeight;
                    //
                    switch (startSettings.DefineFoundingType())
                    {
                        case StartFoundingType.Zeppelin:
                            {
                                Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
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
                    FollowingCamera.main.WeNeedUpdate();
                    break;
                }
            case GameMode.Editor:
                {
                    uicontroller.ChangeUIMode(UIMode.Editor, true);
                    gameObject.AddComponent<PoolMaster>().Load();
                    mainChunk = new GameObject("chunk").AddComponent<Chunk>();
                    int size = Chunk.chunkSize;
                    int[,,] blocksArray = new int[size, size, size];
                    size /= 2;
                    blocksArray[size, size, size] = ResourceType.STONE_ID;
                    mainChunk.CreateNewChunk(blocksArray);
                    break;
                }
        }   

         // set look point
         FollowingCamera.camBasisTransform.position = sceneCenter;

        startSettings = null;
        sessionPrepared = true;
        if (testMode) AnnouncementCanvasController.MakeAnnouncement("game master loaded");
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
    public ColonyController PrepareColonyController(bool assignNewGameID)
    {
        if (_colonyController == null)
        {
            _colonyController = GetComponent<ColonyController>();
            if (_colonyController == null) _colonyController = gameObject.AddComponent<ColonyController>();
            _colonyController.Prepare();
        }
        environmentMaster.LinkColonyController(_colonyController);
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
        return _colonyController;
    }  

    #region updates
    private void Update()
    {
        if (loading) return;

        //if (Input.GetKeyDown("o")) mainChunk.GetNature()?.DEBUG_HaveGrasslandDublicates();
        //if (Input.GetKeyDown("n")) globalMap.ShowOnGUI();
        if (Input.GetKeyDown("x"))
        {
            //TEST_AddCitizens(1000);
            //TEST_PrepareForExpeditions();
            TEST_GiveRoutePoints(Knowledge.ResearchRoute.Foundation, 2);
        }
        if (Input.GetKeyDown("c"))
        {
            TEST_GivePuzzleParts(50);
        }
        if (Input.GetKeyDown("n"))
        {
            TEST_AddPopulation(5000);
        }
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
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * 1.5f * DAYS_IN_MONTH);
                break;
            case Difficulty.Easy:
                colonyController.AddCitizens(70);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 60);
                colonyController.storage.AddResource(ResourceType.metal_E, 30);
                colonyController.storage.AddResource(ResourceType.Plastics, 150);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH);
                break;
            case Difficulty.Normal:
                colonyController.AddCitizens(50);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 50);
                colonyController.storage.AddResource(ResourceType.metal_E, 20);
                colonyController.storage.AddResource(ResourceType.Plastics, 100);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH * 0.75f);
                break;
            case Difficulty.Hard:
                colonyController.AddCitizens(40);
                colonyController.storage.AddResource(ResourceType.metal_K, 50);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 2);
                colonyController.storage.AddResource(ResourceType.Plastics, 10);
                colonyController.storage.AddResource(ResourceType.Food, colonyController.citizenCount * DAYS_IN_MONTH * 0.55f);
                break;
            case Difficulty.Torture:
                colonyController.AddCitizens(30);
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
    #endregion
    //test
    public void OnGUI()
    {
        Rect r = new Rect(0, Screen.height - 16, 200, 16);
        GUI.Box(r, GUIContent.none);
        weNeedNoResources = GUI.Toggle(r, weNeedNoResources, "unlimited resources");
        //if (GUILayout.Button("testMethod")) TestMethod();
    }
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
        globalMap.Save(fs);
        environmentMaster.Save(fs);
        Artifact.SaveStaticData(fs);
        Crew.SaveStaticData(fs);
        mainChunk.SaveChunkData(fs);
        colonyController.Save(fs); // <------- COLONY CONTROLLER

        QuestUI.current.Save(fs);        
        Expedition.SaveStaticData(fs);
        Knowledge.GetCurrent().Save(fs);
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

            if (sessionPrepared) ClearPreviousSessionData();
            else SetDefaultValues();


            // НАЧАЛО ЗАГРУЗКИ   
            #region gms mainPartLoading            
            data = new byte[4];
            fs.Read(data, 0, 4);
            uint saveSystemVersion = System.BitConverter.ToUInt32(data, 0); // может пригодиться в дальнейшем
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
            int i = 40;
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
            globalMap.Load(fs);            
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


            Settlement.TotalRecalculation(); // Totaru Annihirationu no imoto-chan
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
            fs.Close();

            FollowingCamera.main.WeNeedUpdate();
            loading = false;
            currentSavename = fullname;

            if (afterloadRecalculationEvent != null)
            {
                afterloadRecalculationEvent();
                afterloadRecalculationEvent = null;
            }            
            SetPause(false);
            colonyController.FORCED_PowerGridRecalculation();
            colonyController.SYSTEM_DocksRecalculation();
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
        print(errorReason);
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
        mainChunk.LoadChunkData(fs);
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
    #endregion

    public static void TestMethod2(Vector3 pos, Material mat)
    {
        var m = new Mesh();
        m.vertices = new Vector3[] { Vector3.zero, Vector3.forward, new Vector3(1, 0, 1), Vector3.right, new Vector3(1, 1, 0), Vector3.one, new Vector3(0, 1, 1), Vector3.up };
        m.triangles = new int[] { 0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 7, 4, 0, 1, 6, 0, 6, 7, 1, 5, 6, 1, 2, 5, 2, 4, 5, 2, 3, 4, 7, 6, 5, 7, 5, 4 };
        var g = new GameObject();
        var mf = g.AddComponent<MeshFilter>();
        mf.sharedMesh = m;
        var mr = g.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        g.transform.position = pos;
    }
    public static void TestMethod3(Vector3 pos)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.transform.position = pos;
    }
    public static void TestMethod()
    {       
       
        return;
        /*
        Vector3Int ecpos = Vector3Int.zero;
        byte ir = PlaneExtension.INNER_RESOLUTION;
        var slist = mainChunk.surfaces;
        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.MINI_GRPH_REACTOR_3_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }
        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.MINI_GRPH_REACTOR_3_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }
        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.MINI_GRPH_REACTOR_3_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }

        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.OBSERVATORY_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
            (s as WorkBuilding).AddWorkers(50);
        }
       
        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.QUANTUM_TRANSMITTER_4_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }

        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.SCIENCE_LAB_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }

        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.EXPEDITION_CORPUS_4_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }
        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.RECRUITING_CENTER_4_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
        }
        if (mainChunk.TryGetPlace(ref ecpos, ir))
        {
            Structure s = Structure.GetStructureByID(Structure.SHUTTLE_HANGAR_4_ID);
            s.SetBasement(slist[ecpos.z], PixelPosByte.zero);
            (s as Hangar).FORCED_MakeShuttle();
        }

        Crew c = Crew.CreateNewCrew(colonyController, Crew.MAX_MEMBER_COUNT);
        colonyController.storage.AddResources(new ResourceContainer[] {
            new ResourceContainer(ResourceType.Fuel, 500f),
            new ResourceContainer(ResourceType.Supplies, 500f)
        });
        colonyController.AddEnergyCrystals(1000f);

        globalMap.FORCED_CreatePointOfInterest();
        */
    }
    public static void Test_RepairGameMaster()
    {
        if (realMaster == null)
        {
            var g = new GameObject("gameMaster");
            realMaster = g.AddComponent<GameMaster>();
        }
    }

    public void TEST_GivePuzzleParts(int count)
    {
        var k = Knowledge.GetCurrent();
        k.AddPuzzlePart(Knowledge.GREENCOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.BLUECOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.REDCOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.WHITECOLOR_CODE, count);
        k.AddPuzzlePart(Knowledge.CYANCOLOR_CODE, count);
    }
    public void TEST_AddCitizens(int count)
    {
        var cc = colonyController;
        if (cc != null)
        {
            cc.storage.AddResource(ResourceType.Food, 3000);
            cc.AddCitizens(count);
        }
    }
    public void TEST_PrepareForExpeditions()
    {
        if (colonyController == null) return;
        var planes = mainChunk.GetUnoccupiedSurfaces(8);
        planes[0]?.CreateStructure(Structure.RECRUITING_CENTER_4_ID);
        var c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Mony Mony");
        c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Black Rabbit");
        c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Bonemaker");
        c = Crew.CreateNewCrew(colonyController, Crew.MAX_CREWS_COUNT); c.Rename("Eiffiel Dungeon");
        planes[1]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[2]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[3]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[4]?.CreateStructure(Structure.QUANTUM_TRANSMITTER_4_ID);
        planes[5]?.CreateStructure(Structure.EXPEDITION_CORPUS_4_ID);
        planes[6]?.CreateStructure(Structure.MINI_GRPH_REACTOR_3_ID);
        planes[7]?.CreateStructure(Structure.STORAGE_BLOCK_ID);
        colonyController.storage.AddResource(ResourceType.Fuel, 25000);
        colonyController.storage.AddResource(ResourceType.Supplies, 2000);

        var pf = mainChunk.GetUnoccupiedEdgePosition(false,false);
        var p = pf.plane;
        Structure s;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        pf = mainChunk.GetUnoccupiedEdgePosition(false,false);
        p = pf.plane;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        pf = mainChunk.GetUnoccupiedEdgePosition(false, false);
        p = pf.plane;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        pf = mainChunk.GetUnoccupiedEdgePosition(false, false);
        p = pf.plane;
        if (p != null)
        {
            s = p.CreateStructure(Structure.SHUTTLE_HANGAR_4_ID);
            if (s != null)
            {
                s.SetModelRotation(pf.faceIndex * 2);
                (s as Hangar).FORCED_MakeShuttle();
            }
        }
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
        globalMap.FORCED_CreatePointOfInterest();
    }
    public void TEST_GiveRoutePoints(Knowledge.ResearchRoute rr, int count)
    {
        Knowledge.GetCurrent()?.AddResearchPoints(rr, count);
    }
    public void TEST_AddPopulation(int count)
    {
        colonyController?.AddCitizens(count);
    }
}
  
