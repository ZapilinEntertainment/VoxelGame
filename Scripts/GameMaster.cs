using System.Collections.Generic; // листы
using UnityEngine; // классы Юнити
using System.IO; // чтение-запись файлов
using UnityEngine.SceneManagement;

public struct GameStartSettings  {
    public byte chunkSize;
    public ChunkGenerationMode generationMode;
    public Difficulty difficulty;
    public static readonly GameStartSettings Empty;
    static GameStartSettings()
    {
        Empty = new GameStartSettings(ChunkGenerationMode.Standart, 16, Difficulty.Normal);
    }
    public GameStartSettings(ChunkGenerationMode i_genMode, byte i_chunkSize, Difficulty diff)
    {
        generationMode = i_genMode;
        chunkSize = i_chunkSize;
        difficulty = diff;
    }
    public GameStartSettings(ChunkGenerationMode i_genMode)
    {
        generationMode = i_genMode;
        chunkSize = 8;
        difficulty = Difficulty.Normal;
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

public enum GameStart : byte {Nothing, Zeppelin, Headquarters}
public enum GameMode: byte { Play, Editor, Menu, Cinematic }
public enum GameEndingType : byte { Default, ColonyLost, TransportHubVictory, ConsumedByReal, ConsumedByLastSector}

/// -----------------------------------------------------------------------------

public sealed class GameMaster : MonoBehaviour
{
    public static GameMaster realMaster;
    public static float gameSpeed { get; private set; }
    public static bool sceneClearing { get; private set; }
    public static bool needTutorial = false;
    public static bool loading { get; private set; }
    public static bool loadingFailed; // hot
    public static bool soundEnabled { get; private set; }
    public static string savename { get; private set; }
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
    public static GameStartSettings gameStartSettings = GameStartSettings.Empty;    
    public static GeologyModule geologyModule;
    public static Audiomaster audiomaster;

    private static byte pauseRequests = 0;

    public Chunk mainChunk { get; private set; }
    public ColonyController colonyController { get; private set; }
    public EnvironmentMaster environmentMaster { get; private set; }
    public EventChecker eventTracker { get; private set; }
    public GameMode gameMode { get; private set; }
    public GlobalMap globalMap { get; private set; }

    public event System.Action labourUpdateEvent, blockersRestoreEvent, everydayUpdate;
    public GameStart startGameWith = GameStart.Zeppelin;

    public float lifeGrowCoefficient { get; private set; }
    public float demolitionLossesPercent { get; private set; }
    public float lifepowerLossesPercent { get; private set; }
    public float tradeVesselsTrafficCoefficient { get; private set; }
    public float upgradeDiscount { get; private set; }
    public float upgradeCostIncrease { get; private set; }
    public float warProximity { get; private set; } // 0 is far, 1 is nearby  
    public float gearsDegradeSpeed { get; private set; }  

    public Difficulty difficulty { get; private set; }
    //data
    private float timeGone;
    public byte day { get; private set; }
    public byte month { get; private set; }
    public uint year { get; private set; }
    public const byte DAYS_IN_MONTH = 30, MONTHS_IN_YEAR = 12, PLAY_SCENE_INDEX = 1, EDITOR_SCENE_INDEX = 2, MENU_SCENE_INDEX = 0;
    public const float DAY_LONG = 60;
    // updating
    public const float LIFEPOWER_TICK = 1, LABOUR_TICK = 0.25f; // cannot be zero
    private float labourTimer = 0;
    private bool gameStarted = false;
    // FOR TESTING
    [SerializeField] private GameMode _gameMode;
    [SerializeField] public bool testMode = false;
    public bool weNeedNoResources { get; private set; }
    public bool generateChunk = true;
    public byte test_size = 100;
    //

    #region static functions
    public static void SetSavename(string s)
    {
        savename = s;
    }
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
    public static void ChangeScene(byte index)
    {
        if (SceneManager.GetActiveScene().buildIndex == MENU_SCENE_INDEX)
        {
            var m = Component.FindObjectOfType<MenuUI>();
            if (m != null) m.ToLoadingView();
        }
        sceneClearing = true;
        SceneManager.LoadScene(index);
        sceneClearing = false;
        Structure.ResetToDefaults_Static();
    }
    public static void LoadingFail()
    {
        loadingFailed = true;
        SetPause(true);
        Debug.Log("loading failed");
    }
    #endregion

    public void ChangeModeToPlay()
    {
        if (gameMode != GameMode.Editor) return;
        gameMode = GameMode.Play;
        _gameMode = gameMode;
        Instantiate(Resources.Load<GameObject>("UIPrefs/UIController")).GetComponent<UIController>();
        gameStarted = false;
        gameStartSettings.generationMode = ChunkGenerationMode.DontGenerate;
        startGameWith = GameStart.Zeppelin;
        Awake();
        Start();
    }

    private void Awake()
    {
        //testzone
        //gameStartSettings.generationMode = ChunkGenerationMode.GameLoading;
        //savename = "test";
        //

        if (realMaster != null & realMaster != this)
        {
            Destroy(this);
            return;
        }
        gameMode = _gameMode;
        realMaster = this;
        sceneClearing = false;
        if (PoolMaster.current == null)
        {
            PoolMaster pm = gameObject.AddComponent<PoolMaster>();
            pm.Load();
        }
        if (gameMode == GameMode.Play)
        {
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
    void Start()
    {
        if (gameStarted) return;
        
        if (gameMode != GameMode.Editor)
        {            
            difficulty = gameStartSettings.difficulty;
            SetDefaultValues();
            //byte chunksize = gss.chunkSize;
            byte chunksize;
            chunksize = gameStartSettings.chunkSize;
            if (gameStartSettings.generationMode != ChunkGenerationMode.GameLoading)
            {
                if (gameStartSettings.generationMode != ChunkGenerationMode.DontGenerate)
                {
                    if (gameStartSettings.generationMode != ChunkGenerationMode.TerrainLoading)
                    {
                        Constructor.ConstructChunk(chunksize, gameStartSettings.generationMode);
                        // Constructor.ConstructBlock(chunksize);
                        if (gameStartSettings.generationMode == ChunkGenerationMode.Peak)
                        {
                            environmentMaster.PrepareIslandBasis(ChunkGenerationMode.Peak);
                        }
                    }
                    else LoadTerrain(SaveSystemUI.GetTerrainsPath() + '/' + savename + '.' + SaveSystemUI.TERRAIN_FNAME_EXTENSION);
                }
                FollowingCamera.main.ResetTouchRightBorder();
                FollowingCamera.main.CameraRotationBlock(false);
                warProximity = 0.01f;
                layerCutHeight = Chunk.chunkSize; prevCutHeight = layerCutHeight;
                switch (startGameWith)
                {
                    case GameStart.Zeppelin:
                        Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
                        if (needTutorial)
                        {
                            GameLogUI.EnableDecisionWindow(null, Localization.GetTutorialHint(LocalizedTutorialHint.Landing));
                        }
                        else
                        {
                            GameLogUI.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.SetLandingPoint));
                        }
                        break;

                    case GameStart.Headquarters:
                        var sblocks = mainChunk.surfaces;
                        Plane sb = sblocks[Random.Range(0, sblocks.Length)];
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


                        sb = mainChunk.GetHighestSurfacePlane(xpos - 1, zpos + 1);
                        if (sb == null)
                        {
                            sb = mainChunk.GetHighestSurfacePlane(xpos, zpos + 1);
                            if (sb == null)
                            {
                                sb = mainChunk.GetHighestSurfacePlane(xpos + 1, zpos + 1);
                                if (sb == null)
                                {
                                    sb = mainChunk.GetHighestSurfacePlane(xpos - 1, zpos);
                                    if (sb == null)
                                    {
                                        sb = mainChunk.GetHighestSurfacePlane(xpos + 1, zpos);
                                        if (sb == null)
                                        {
                                            sb = mainChunk.GetHighestSurfacePlane(xpos - 1, zpos - 1);
                                            if (sb == null)
                                            {
                                                sb = mainChunk.GetHighestSurfacePlane(xpos, zpos - 1);
                                                if (sb == null)
                                                {
                                                    sb = mainChunk.GetHighestSurfacePlane(xpos + 1, zpos - 1);
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
                        break;
                }               
                FollowingCamera.main.WeNeedUpdate();
            }
            else
            {
                LoadGame(SaveSystemUI.GetSavesPath() + '/' + savename + ".sav");
            }
        }
        else
        {
            gameObject.AddComponent<PoolMaster>().Load();
            mainChunk = new GameObject("chunk").AddComponent<Chunk>();
            int size = Chunk.chunkSize;
            int[,,] blocksArray = new int[size, size, size];
            size /= 2;
            blocksArray[size, size, size] = ResourceType.STONE_ID;
            mainChunk.CreateNewChunk(blocksArray);            
        }       

        { // set look point
            FollowingCamera.camBasisTransform.position = sceneCenter;
        }
        gameStarted = true;
    }

    private void SetDefaultValues()
    {        
        Time.timeScale = 1;
        gameSpeed = 1;
        pauseRequests = 0;        
        if (savename == null || savename == string.Empty) savename = "autosave";
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
        
    }

    public void SetMainChunk(Chunk c) { mainChunk = c; }
    public void SetColonyController(ColonyController c)
    {
        colonyController = c;
        environmentMaster.LinkColonyController(c);
    }  

    #region updates
    private void Update()
    {
        if (loading) return;

        if (testMode)
        {
            {
                if (Input.GetKeyDown("n")) globalMap.ShowOnGUI();

                if (Input.GetKeyDown("o")) TestMethod();
            }
            if (Input.GetKeyDown("m"))
            {
                if (colonyController != null) colonyController.AddEnergyCrystals(1000f);
            }
            if (Input.GetKeyDown("p"))
            {
                Knowledge.GetCurrent().OpenResearchTab();
                UIController.current.gameObject.SetActive(false);
            }
        }   
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
    }
    //
    public void GameOver(GameEndingType endType)
    {
        SetPause(true);
        UIController.current.FullDeactivation();

        double score = new ScoreCalculator().GetScore(this);
        Highscore.AddHighscore(new Highscore(colonyController.cityName, score, endType));

        string reason = Localization.GetEndingTitle(endType);
        switch (endType)
        {
            case GameEndingType.TransportHubVictory:
                {
                    Transform endpanel = Instantiate(Resources.Load<GameObject>("UIPrefs/endPanel"), UIController.current.mainCanvas).transform;
                    endpanel.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = reason;
                    endpanel.GetChild(2).GetComponent<UnityEngine.UI.Text>().text = Localization.GetWord(LocalizedWord.Score) + ": " + ((int)score).ToString();
                    var b = endpanel.GetChild(3).GetComponent<UnityEngine.UI.Button>();
                    b.onClick.AddListener(ReturnToMenuAfterGameOver);
                    b.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = Localization.GetWord(LocalizedWord.MainMenu);
                    b = endpanel.GetChild(4).GetComponent<UnityEngine.UI.Button>();
                    b.onClick.AddListener(() => { ContinueGameAfterEnd(endpanel.gameObject); });
                    b.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = Localization.GetWord(LocalizedWord.Continue);
                    break;
                }
            case GameEndingType.ColonyLost:
            case GameEndingType.Default:
            case GameEndingType.ConsumedByReal:
            case GameEndingType.ConsumedByLastSector:
            default:
                {
                    Transform failpanel = Instantiate(Resources.Load<GameObject>("UIPrefs/failPanel"), UIController.current.mainCanvas).transform;
                    failpanel.GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = reason;
                    failpanel.GetChild(2).GetComponent<UnityEngine.UI.Text>().text = Localization.GetWord(LocalizedWord.Score) + ": " + ((int)score).ToString();
                    var b = failpanel.GetChild(3).GetComponent<UnityEngine.UI.Button>();
                    b.onClick.AddListener(ReturnToMenuAfterGameOver);
                    b.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = Localization.GetWord(LocalizedWord.MainMenu);
                    break;
                }
        }
    }
    public void ReturnToMenuAfterGameOver()
    {
        sceneClearing = true;
        ChangeScene(MENU_SCENE_INDEX);
        sceneClearing = false;
    }
    public void ContinueGameAfterEnd(GameObject panel)
    {
        Destroy(panel);
        UIController.current.FullReactivation();
        gameSpeed = 1;
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
        savename = name;
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
        fs.WriteByte((byte)startGameWith); // 42
        fs.WriteByte(prevCutHeight); //43
        fs.WriteByte(day); // 44
        fs.WriteByte(month); //45
        fs.Write(System.BitConverter.GetBytes(year), 0, 4);
        fs.Write(System.BitConverter.GetBytes(timeGone), 0, 4);
        fs.Write(System.BitConverter.GetBytes(gearsDegradeSpeed), 0, 4);
        fs.Write(System.BitConverter.GetBytes(labourTimer), 0, 4);
        fs.Write(System.BitConverter.GetBytes(RecruitingCenter.GetHireCost()), 0, 4);
        //65 (+4) end
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
            // ОЧИСТКА
            StopAllCoroutines();
            if (Zeppelin.current != null)
            {
                Destroy(Zeppelin.current);
            }
            if (mainChunk != null) mainChunk.ClearChunk();
            // очистка подписчиков на ивенты невозможна, сами ивенты к этому моменту недоступны
            Crew.Reset(); 
            Expedition.GameReset();
            Structure.ResetToDefaults_Static(); // все наследуемые resetToDefaults внутри
            if (colonyController != null) colonyController.ResetToDefaults(); // подчищает все списки
            else
            {
                colonyController = gameObject.AddComponent<ColonyController>();
                colonyController.Prepare();
            }
            //UI.current.Reset();


            // НАЧАЛО ЗАГРУЗКИ   
            if (gameStarted) SetDefaultValues();
            #region gms mainPartLoading            
            data = new byte[4];
            fs.Read(data, 0, 4);
            uint saveSystemVersion = System.BitConverter.ToUInt32(data, 0); // может пригодиться в дальнейшем
            //start writing
            data = new byte[65]; 
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
            startGameWith = (GameStart)data[41];
            prevCutHeight = data[42];
            day = data[43];
            month = data[44];

            year = System.BitConverter.ToUInt32(data, 45);
            timeGone = System.BitConverter.ToSingle(data, 49);
            gearsDegradeSpeed = System.BitConverter.ToSingle(data, 53);
            labourTimer = System.BitConverter.ToSingle(data, 57);
            RecruitingCenter.SetHireCost(System.BitConverter.ToSingle(data, 61));
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

            if (mainChunk == null)
            {
                GameObject g = new GameObject("chunk");
                mainChunk = g.AddComponent<Chunk>();
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
            fs.Close();

            FollowingCamera.main.WeNeedUpdate();
            loading = false;
            savename = fullname;
            SetPause(false);

            return true;
        }
        else
        {
            GameLogUI.MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed) + " : hashsum incorrect");
            if (soundEnabled) audiomaster.Notify(NotificationSound.SystemError);
            SetPause(true);
            fs.Close();
            return false;
        }
        FAIL:
        GameLogUI.MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed) + " : data corruption");
        if (soundEnabled) audiomaster.Notify(NotificationSound.SystemError);
        print(errorReason);
        SetPause(true);
        fs.Close();
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

    private void TestMethod()
    {
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
    }
}
  
