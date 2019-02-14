using System.Collections.Generic; // листы
using UnityEngine; // классы Юнити
using System.IO; // чтение-запись файлов
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
public enum GameEndingType : byte { Default, ColonyLost, TransportHubVictory, ConsumedByReal, ConsumedByLastSector}

/// -----------------------------------------------------------------------------

public sealed class GameMaster : MonoBehaviour
{
    public static GameMaster realMaster;
    public static float gameSpeed { get; private set; }
    public static bool sceneClearing { get; private set; }
    public static bool editMode = false;
    public static bool loading { get; private set; }
    public static bool soundEnabled { get; private set; }
    public static string savename { get; private set; }
    public static float LUCK_COEFFICIENT { get; private set; }
    public static float sellPriceCoefficient = 0.75f;
    public static byte layerCutHeight = 16, prevCutHeight = 16;

    public static Vector3 sceneCenter { get { return Vector3.one * Chunk.CHUNK_SIZE / 2f; } } // SCENE CENTER
    public static GameStartSettings gameStartSettings = GameStartSettings.Empty;
    public static Difficulty difficulty { get; private set; }
    public static GeologyModule geologyModule;
    public static Audiomaster audiomaster;

    private static byte pauseRequests = 0;

#pragma warning disable 0649
    [SerializeField]private MeshRenderer upperHemisphere, lowerHemisphere;
#pragma warning restore 0649

    public Chunk mainChunk { get; private set; }
    public ColonyController colonyController { get; private set; }
    public EnvironmentMaster environmentMaster { get; private set; }
    public GlobalMap globalMap { get; private set; }
    public Constructor constructor;
    public delegate void StructureUpdateHandler();
    public event StructureUpdateHandler labourUpdateEvent, lifepowerUpdateEvent;
    public GameStart startGameWith = GameStart.Zeppelin;
    public float lifeGrowCoefficient { get; private set; }
    public float demolitionLossesPercent { get; private set; }
    public float lifepowerLossesPercent { get; private set; }
    public float tradeVesselsTrafficCoefficient { get; private set; }
    public float upgradeDiscount { get; private set; }
    public float upgradeCostIncrease { get; private set; }
    public float warProximity { get; private set; } // 0 is far, 1 is nearby  
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
    private bool firstSet = true;
    // FOR TESTING
    public bool weNeedNoResources { get; private set; }
    public bool generateChunk = true;
    public byte test_size = 100;
    public bool _editMode = false;

    private bool hotStart = true;
    private GameStartSettings hotStartSettings = new GameStartSettings(ChunkGenerationMode.GameLoading);
    private string hotStart_savename = "alpha9.3.1";
    //
    private byte upSkyStatus = 0, lowSkyStatus = 0;
    private float worldConsumingTimer = 0;

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
    public static void ChangeScene(GameLevel level)
    {
        sceneClearing = true;
        SceneManager.LoadScene((int)level);
        sceneClearing = false;
        Structure.ResetToDefaults_Static();
    }
    #endregion

    public void ChangeModeToPlay()
    {
        if (!editMode) return;
        _editMode = false;
        Instantiate(Resources.Load<GameObject>("UIPrefs/UIController")).GetComponent<UIController>();
        firstSet = true;
        gameStartSettings.generationMode = ChunkGenerationMode.DontGenerate;
        startGameWith = GameStart.Zeppelin;
        Awake();
        Start();
    }

    private void Awake()
    {
        if (realMaster != null & realMaster != this)
        {
            Destroy(this);
            return;
        }
        realMaster = this;
        sceneClearing = false;
        if (environmentMaster == null) environmentMaster = gameObject.AddComponent<EnvironmentMaster>();
        environmentMaster.Prepare();
        if (!editMode)
        {
            if (globalMap == null) globalMap = gameObject.AddComponent<GlobalMap>();
            globalMap.Prepare();
        }
    }

    void Start()
    {
        if (!firstSet) return;
        Time.timeScale = 1;
        gameSpeed = 1;
        pauseRequests = 0;
        audiomaster = gameObject.AddComponent<Audiomaster>();
        audiomaster.Prepare();

        //testzone
        editMode = _editMode;
        if (hotStart)
        {
            gameStartSettings = hotStartSettings;
            savename = hotStart_savename;
            hotStart = false;
        }
        //end of test data

        if (geologyModule == null) geologyModule = gameObject.AddComponent<GeologyModule>();
        if (!editMode)
        {
            lifeGrowCoefficient = 1;
            difficulty = gameStartSettings.difficulty;
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
                FollowingCamera.main.CameraRotationBlock(false);

                switch (difficulty)
                {
                    case Difficulty.Utopia:
                        LUCK_COEFFICIENT = 1;
                        demolitionLossesPercent = 0;
                        lifepowerLossesPercent = 0;
                        sellPriceCoefficient = 1;
                        tradeVesselsTrafficCoefficient = 2;
                        upgradeDiscount = 0.5f; upgradeCostIncrease = 1.1f;
                        environmentMaster.SetEnvironmentalConditions(1);
                        gearsDegradeSpeed = 0;
                        break;
                    case Difficulty.Easy:
                        LUCK_COEFFICIENT = 0.7f;
                        demolitionLossesPercent = 0.2f;
                        lifepowerLossesPercent = 0.1f;
                        sellPriceCoefficient = 0.9f;
                        tradeVesselsTrafficCoefficient = 1.5f;
                        upgradeDiscount = 0.3f; upgradeCostIncrease = 1.3f;
                        environmentMaster.SetEnvironmentalConditions(1);
                        gearsDegradeSpeed = 0.00001f;
                        break;
                    case Difficulty.Normal:
                        LUCK_COEFFICIENT = 0.5f;
                        demolitionLossesPercent = 0.4f;
                        lifepowerLossesPercent = 0.3f;
                        sellPriceCoefficient = 0.75f;
                        tradeVesselsTrafficCoefficient = 1;
                        upgradeDiscount = 0.25f; upgradeCostIncrease = 1.5f;
                        environmentMaster.SetEnvironmentalConditions(0.95f);
                        gearsDegradeSpeed = 0.00002f;
                        break;
                    case Difficulty.Hard:
                        LUCK_COEFFICIENT = 0.1f;
                        demolitionLossesPercent = 0.7f;
                        lifepowerLossesPercent = 0.5f;
                        sellPriceCoefficient = 0.5f;
                        tradeVesselsTrafficCoefficient = 0.9f;
                        upgradeDiscount = 0.2f; upgradeCostIncrease = 1.7f;
                        environmentMaster.SetEnvironmentalConditions(0.9f);
                        gearsDegradeSpeed = 0.00003f;
                        break;
                    case Difficulty.Torture:
                        LUCK_COEFFICIENT = 0.01f;
                        demolitionLossesPercent = 1;
                        lifepowerLossesPercent = 0.85f;
                        sellPriceCoefficient = 0.33f;
                        tradeVesselsTrafficCoefficient = 0.75f;
                        upgradeDiscount = 0.1f; upgradeCostIncrease = 2f;
                        environmentMaster.SetEnvironmentalConditions(0.8f);
                        gearsDegradeSpeed = 0.00005f;
                        break;
                }
                warProximity = 0.01f;
                layerCutHeight = Chunk.CHUNK_SIZE; prevCutHeight = layerCutHeight;
                switch (startGameWith)
                {
                    case GameStart.Zeppelin:
                        Instantiate(Resources.Load<GameObject>("Prefs/Zeppelin"));
                        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.SetLandingPoint));
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
            int[,,] blocksArray = new int[size, size, size];
            size /= 2;
            blocksArray[size, size, size] = ResourceType.STONE_ID;
            mainChunk.CreateNewChunk(blocksArray);
        }

        { // set look point
            FollowingCamera.camBasisTransform.position = sceneCenter;
        }

        if (upperHemisphere != null & upSkyStatus == 0) upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky");
        if (lowerHemisphere != null & lowSkyStatus == 0 ) lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky");
    }

    public void SetMainChunk(Chunk c) { mainChunk = c; }
    public void SetColonyController(ColonyController c)
    {
        colonyController = c;
        worldConsumingTimer = 60;
    }

    #region updates
    private void Update()
    {
        if (loading) return;
        if (colonyController != null & upperHemisphere != null & lowerHemisphere != null)
        {
            worldConsumingTimer -= Time.deltaTime * gameSpeed;
            if (worldConsumingTimer <= 0)
            {
                float ch = colonyController.happiness_coefficient;
                byte newUpSkyStatus = upSkyStatus, newLowSkyStatus = lowSkyStatus;
                if (ch < GameConstants.RSPACE_CONSUMING_VAL)
                {
                    if (newUpSkyStatus > 0) newUpSkyStatus--;
                    if (newLowSkyStatus < 4) {
                        newLowSkyStatus++;
                        if (newLowSkyStatus == 1) UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.IslandCollapsing), Color.red);
                    }
                    worldConsumingTimer = GameConstants.WORLD_CONSUMING_TIMER * newLowSkyStatus;
                }
                else
                {
                    if (newLowSkyStatus > 0) newLowSkyStatus--;
                    if (ch > GameConstants.LSECTOR_CONSUMING_VAL)
                    {
                        if (newUpSkyStatus < 4)
                        {
                            newUpSkyStatus++;
                            if (newUpSkyStatus == 1) UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.IslandCollapsing), Color.red);
                        }
                        worldConsumingTimer = GameConstants.WORLD_CONSUMING_TIMER * newUpSkyStatus;
                    }
                    else
                    {
                        newUpSkyStatus = 0;
                        worldConsumingTimer = 60;
                    }
                }

                bool cceCheck = false;
                if (newUpSkyStatus != upSkyStatus)
                {
                    switch (newUpSkyStatus)
                    {
                        case 1:
                            upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky1");
                            break;
                        case 2:
                            upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky2");
                            break;
                        case 3:
                            upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky3");
                            break;
                        case 4: upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky");
                            break;
                        default:
                            upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky");
                            break;
                    }
                    upSkyStatus = newUpSkyStatus;
                    cceCheck = true;
                }
                if (newLowSkyStatus != lowSkyStatus)
                {
                    switch (newLowSkyStatus)
                    {
                        case 1:
                            lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky1");
                            break;
                        case 2:
                            lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky2");
                            break;
                        case 3:
                            lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky3");
                            break;
                        case 4:
                            lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky");
                            break;
                        default:
                            lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky");
                            break;
                    }
                    lowSkyStatus = newLowSkyStatus;
                    cceCheck = true;
                }
                if (cceCheck)
                {
                    ChunkConsumingEffect cce = mainChunk.GetComponent<ChunkConsumingEffect>();
                    if (cce == null) cce = mainChunk.gameObject.AddComponent<ChunkConsumingEffect>();
                    cce.SetSettings(upSkyStatus, lowSkyStatus);
                }
            }
        }

        //testzone
        if (Input.GetKeyDown("m") & colonyController != null) colonyController.AddEnergyCrystals(1000);
        //eo testzone
    }

    private void FixedUpdate()
    {
        if (gameSpeed != 0)
        {
            float fixedTime = Time.fixedDeltaTime * gameSpeed;
            if (!editMode)
            {
                labourTimer -= fixedTime;
                lifepowerTimer -= fixedTime;
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
                timeGone += fixedTime;

                if (timeGone >= DAY_LONG)
                {
                    uint daysDelta = (uint)(timeGone / DAY_LONG);
                    if (daysDelta > 0 & colonyController != null)
                    {
                        // счет количества дней в ускорении отменен
                        colonyController.EverydayUpdate();
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

    public float CalculateWorkspeed(int workersCount, WorkType type)
    {
        if (colonyController == null) return 0;
        float workspeed = workersCount * colonyController.labourEfficientcy_coefficient * (colonyController.gears_coefficient + colonyController.health_coefficient + colonyController.happiness_coefficient - 2);
        if (workspeed < 0) workspeed = 0.01f;
        switch (type)
        {
            case WorkType.Digging: workspeed *= diggingSpeed; break;
            case WorkType.Manufacturing: workspeed *= manufacturingSpeed; break;
            case WorkType.Nothing: workspeed = 0; break;
            case WorkType.Pouring: workspeed *= pouringSpeed; break;
            case WorkType.Clearing: workspeed *= clearingSpeed; break;
            case WorkType.Gathering: workspeed *= gatheringSpeed; break;
            case WorkType.Mining: workspeed *= miningSpeed; break; // digging inside mine
            case WorkType.Farming: workspeed *= lifeGrowCoefficient * environmentMaster.environmentalConditions; break;
            case WorkType.MachineConstructing: workspeed *= machineConstructingSpeed; break;
        }
        return workspeed;
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
                colonyController.storage.AddResource(ResourceType.Food, 700);
                break;
            case Difficulty.Normal:
                colonyController.AddCitizens(50);
                colonyController.storage.AddResource(ResourceType.metal_K, 100);
                colonyController.storage.AddResource(ResourceType.metal_M, 50);
                colonyController.storage.AddResource(ResourceType.metal_E, 20);
                colonyController.storage.AddResource(ResourceType.Plastics, 100);
                colonyController.storage.AddResource(ResourceType.Food, 500);
                break;
            case Difficulty.Hard:
                colonyController.AddCitizens(40);
                colonyController.storage.AddResource(ResourceType.metal_K, 50);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 2);
                colonyController.storage.AddResource(ResourceType.Plastics, 10);
                colonyController.storage.AddResource(ResourceType.Food, 700);
                break;
            case Difficulty.Torture:
                colonyController.AddCitizens(30);
                colonyController.storage.AddResource(ResourceType.metal_K, 40);
                colonyController.storage.AddResource(ResourceType.metal_M, 20);
                colonyController.storage.AddResource(ResourceType.metal_E, 10);
                colonyController.storage.AddResource(ResourceType.Food, 750);
                break;
        }

    }

    //test
    public void OnGUI()
    {
        Rect r = new Rect(0, Screen.height - 16, 200, 16);
        GUI.Box(r, GUIContent.none);
        weNeedNoResources = GUI.Toggle(r, weNeedNoResources, "unlimited resources");
    }

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
        ChangeScene(GameLevel.Menu);
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
        fs.Write(System.BitConverter.GetBytes(gameSpeed), 0, 4);
        fs.Write(System.BitConverter.GetBytes(lifeGrowCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(demolitionLossesPercent), 0, 4);
        fs.Write(System.BitConverter.GetBytes(lifepowerLossesPercent), 0, 4);
        fs.Write(System.BitConverter.GetBytes(LUCK_COEFFICIENT), 0, 4);
        fs.Write(System.BitConverter.GetBytes(sellPriceCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(tradeVesselsTrafficCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(upgradeDiscount), 0, 4);
        fs.Write(System.BitConverter.GetBytes(upgradeCostIncrease), 0, 4);
        fs.Write(System.BitConverter.GetBytes(warProximity), 0, 4); // end 40
        fs.WriteByte((byte)difficulty); // 41
        fs.WriteByte((byte)startGameWith); // 42
        fs.WriteByte(prevCutHeight); //43
        fs.WriteByte(day);
        fs.WriteByte(month); //45
        fs.Write(System.BitConverter.GetBytes(year), 0, 4);
        fs.Write(System.BitConverter.GetBytes(timeGone), 0, 4);
        fs.Write(System.BitConverter.GetBytes(gearsDegradeSpeed), 0, 4);
        // 57
        fs.Write(System.BitConverter.GetBytes(labourTimer), 0, 4);
        fs.Write(System.BitConverter.GetBytes(lifepowerTimer), 0, 4);
        fs.Write(System.BitConverter.GetBytes(RecruitingCenter.GetHireCost()), 0, 4);
        // 69
        fs.Write(System.BitConverter.GetBytes(worldConsumingTimer), 0, 4);
        fs.WriteByte(upSkyStatus);
        fs.WriteByte(lowSkyStatus);
        //75
        #endregion
        environmentMaster.Save(fs);
        Shuttle.SaveStaticData(fs);
        Crew.SaveStaticData(fs);
        mainChunk.SaveChunkData(fs);
        colonyController.Save(fs);
        Dock.SaveStaticDockData(fs);



        QuestUI.current.Save(fs);
        Expedition.SaveStaticData(fs);
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
            if (Zeppelin.current != null)
            {
                Destroy(Zeppelin.current);
            }
            if (mainChunk != null) mainChunk.ClearChunk();
            // очистка подписчиков на ивенты невозможна, сами ивенты к этому моменту недоступны
            Crew.Reset(); Shuttle.Reset();
            Grassland.ScriptReset();
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
            FileStream fs = File.Open(fullname, FileMode.Open);
            #region gms mainPartLoading
            var data = new byte[4];
            fs.Read(data, 0, 4);
            uint saveSystemVersion = System.BitConverter.ToUInt32(data, 0); // может пригодиться в дальнейшем
            data = new byte[75];
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
            lifepowerTimer = System.BitConverter.ToSingle(data, 61);
            RecruitingCenter.SetHireCost(System.BitConverter.ToSingle(data, 65));

            worldConsumingTimer = System.BitConverter.ToSingle(data, 69);
            upSkyStatus = data[73];
            if (upperHemisphere != null)
            {
                switch (upSkyStatus)
                {
                    case 1:
                        upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky1");
                        break;
                    case 2:
                        upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky2");
                        break;
                    case 3:
                        upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky3");
                        break;
                    case 4:
                        upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky");
                        break;
                    default:
                        upperHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky");
                        break;
                }                
            }
            lowSkyStatus = data[74];
            if (lowerHemisphere != null)
            {
                switch (lowSkyStatus)
                {
                    case 1:
                        lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky1");
                        break;
                    case 2:
                        lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky2");
                        break;
                    case 3:
                        lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky3");
                        break;
                    case 4:
                        lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/Sky");
                        break;
                    default:
                        lowerHemisphere.sharedMaterial = Resources.Load<Material>("Materials/LowSky");
                        break;
                }
            }
            else print("no hemisphere");
            #endregion
            if (environmentMaster == null) environmentMaster = gameObject.AddComponent<EnvironmentMaster>();
            environmentMaster.Load(fs);
            Shuttle.LoadStaticData(fs); // because of hangars
            Crew.LoadStaticData(fs);

            if (mainChunk == null)
            {
                GameObject g = new GameObject("chunk");
                mainChunk = g.AddComponent<Chunk>();
            }
            mainChunk.LoadChunkData(fs);
            ChunkConsumingEffect cce = mainChunk.GetComponent<ChunkConsumingEffect>();
            if (upSkyStatus != 0 | lowSkyStatus != 0)
            {
                if (cce == null) cce = mainChunk.gameObject.AddComponent<ChunkConsumingEffect>();
                cce.SetSettings(upSkyStatus, lowSkyStatus);
            }
            else
            {
                if (cce != null) cce.SetSettings(upSkyStatus, lowSkyStatus);
            }

            colonyController.Load(fs); // < --- COLONY CONTROLLER
            Dock.LoadStaticData(fs);
            QuestUI.current.Load(fs);
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
    #endregion

}
  
