using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChosenObjectType : byte { None, Surface, Cube, Structure, Worksite }
public enum Icons : byte { GreenArrow, GuidingStar, PowerOff, PowerPlus, PowerMinus, Citizen, RedArrow, CrewBadIcon, CrewNormalIcon, CrewGoodIcon, ShuttleBadIcon, ShuttleNormalIcon, ShuttleGoodIcon, TaskFrame, TaskCompleted, DisabledBuilding }
public enum ProgressPanelMode : byte { Offline, Powerplant, Hangar, RecruitingCenter }
public enum ActiveWindowMode : byte { NoWindow, TradePanel, StoragePanel, BuildPanel, SpecificBuildPanel, QuestPanel, GameMenu, ExpeditionPanel}


sealed public class UIController : MonoBehaviour
{
    public GameObject rightPanel, upPanel, menuPanel, menuButton; // fill in the Inspector
    public Button closePanelButton; // fill in the Inspector

#pragma warning disable 0649
    [SerializeField] GameObject colonyPanel, tradePanel, hospitalPanel, expeditionPanel, rollingShopPanel, progressPanel, storagePanel, optionsPanel, leftPanel; // fiti
    [SerializeField] Text gearsText, happinessText, birthrateText, hospitalText, housingText, healthText, citizenString, energyString, energyCrystalsString, moneyFlyingText, progressPanelText, dataString;
    [SerializeField] Text[] announcementStrings;
    [SerializeField] Image colonyToggleButton, storageToggleButton, layerCutToggleButton, storageOccupancyFullfill, progressPanelFullfill, foodIconFullfill;
    [SerializeField] Transform storagePanelContent;
    [SerializeField] RawImage progressPanelIcon;
    [SerializeField] QuestUI _questUI;
#pragma warning restore 0649

    public bool showLayerCut { get; private set; }
    public delegate void StatusUpdateHandler();
    public event StatusUpdateHandler statusUpdateEvent;
    public ActiveWindowMode currentActiveWindowMode { get; private set; }
    public ProgressPanelMode progressPanelMode { get; private set; }
    public Texture iconsTexture { get; private set; }
    public Texture resourcesTexture { get; private set; }
    public Texture buildingsTexture { get; private set; }
    public Transform mainCanvas { get; private set; }
    public SurfaceBlock chosenSurface { get; private set; }
    public QuestUI questUI { get; private set; }

    Vector3 flyingMoneyOriginalPoint = Vector3.zero;

    private enum MenuSection { NoSelection, Save, Load, Options }
    MenuSection selectedMenuSection = MenuSection.NoSelection;

    const float DATA_UPDATE_TIME = 1, DISSAPPEAR_SPEED = 0.3f, STATUS_UPDATE_TIME = 1;    
    public int interceptingConstructPlaneID = -1;

    private float showingGearsCf, showingHappinessCf, showingBirthrate, showingHospitalCf, showingHealthCf,
    updateTimer, moneyFlySpeed = 0, showingHousingCf;
    private byte showingStorageOccupancy, faceIndex = 10;
    private float saved_energySurplus, statusUpdateTimer = 0;
    private int saved_citizenCount, saved_freeWorkersCount, saved_livespaceCount, saved_energyCount, saved_energyMax, saved_energyCrystalsCount,
        hospitalPanel_savedMode, exCorpus_savedCrewsCount, exCorpus_savedShuttlesCount, exCorpus_savedTransmittersCount, lastStorageOperationNumber,
        saved_citizenCountBeforeStarvation
        ;
    private bool showMenuWindow = false, showColonyInfo = false, showStorageInfo = false, activeAnnouncements = false, 
        localized = false, storagePositionsPrepared = false, linksReady = false, starvationSignal = false;
    

    private CubeBlock chosenCube;
    private ColonyController colony;
    private Structure chosenStructure;
    private Storage storage;
    UIObserver workingObserver;
    Worksite chosenWorksite;
    ChosenObjectType chosenObjectType;
    Transform selectionFrame; Material selectionFrameMaterial;

    public static UIController current;

    const int MENUPANEL_SAVE_BUTTON_INDEX = 0, MENUPANEL_LOAD_BUTTON_INDEX = 1, MENUPANEL_OPTIONS_BUTTON_INDEX = 2, RPANEL_CUBE_DIG_BUTTON_INDEX = 4;

    public void Awake()
    {
        current = this;
        leftPanel.SetActive(false);
        upPanel.SetActive(false);
        LocalizeButtonTitles();
        selectionFrame = Instantiate(Resources.Load<GameObject>("Prefs/structureFrame")).transform;
        selectionFrameMaterial = selectionFrame.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        selectionFrame.gameObject.SetActive(false);
        iconsTexture = Resources.Load<Texture>("Textures/Icons");
        resourcesTexture = Resources.Load<Texture>("Textures/resourcesIcons");
        buildingsTexture = Resources.Load<Texture>("Textures/buildingIcons");
        questUI = _questUI;
        if (flyingMoneyOriginalPoint == Vector3.zero) flyingMoneyOriginalPoint = moneyFlyingText.rectTransform.position;
        mainCanvas = transform.GetChild(1);

        SaveSystemUI.Check(mainCanvas);

        if (!localized) LocalizeButtonTitles();        
    }

    public void Prepare()
    {
        colony = GameMaster.realMaster.colonyController;
        storage = colony.storage;
        linksReady = true;
        leftPanel.SetActive(true);
        upPanel.SetActive(true);
    }

    void Update()
    {
        float tm = Time.deltaTime * GameMaster.gameSpeed;
        statusUpdateTimer -= tm;
        if (statusUpdateTimer <= 0)
        {
            if (statusUpdateEvent != null) statusUpdateEvent();
            statusUpdateTimer = STATUS_UPDATE_TIME;
        }
        updateTimer -= tm;
       
        if (linksReady & colony != null)
        {
            if (updateTimer <= 0)
            {
                updateTimer = DATA_UPDATE_TIME;

                byte so = (byte)(storage.totalVolume / storage.maxVolume * 100);
                if (showingStorageOccupancy != so)
                {
                    showingStorageOccupancy = so;
                    float f = so / 100f;
                    storageOccupancyFullfill.fillAmount = f;
                    storageOccupancyFullfill.color = Color.Lerp(PoolMaster.gameOrangeColor, Color.red, f * f);
                }

                if (showColonyInfo)
                {
                    if (showingGearsCf != colony.gears_coefficient)
                    {
                        showingGearsCf = colony.gears_coefficient;
                        gearsText.text = string.Format("{0:0.###}", showingGearsCf);
                    }
                    if (showingHappinessCf != colony.happiness_coefficient)
                    {
                        showingHappinessCf = colony.happiness_coefficient;
                        happinessText.text = string.Format("{0:0.##}", showingHappinessCf * 100) + '%';
                    }
                    if (showingBirthrate != colony.realBirthrate)
                    {
                        showingBirthrate = colony.realBirthrate;
                        birthrateText.text = showingBirthrate > 0 ? '+' + string.Format("{0:0.#####}", showingBirthrate) : string.Format("{0:0.#####}", showingBirthrate);
                    }
                    if (showingHousingCf != colony.housingLevel)
                    {
                        showingHousingCf = colony.housingLevel;
                        housingText.text = string.Format("{0:0.##}", showingHousingCf);
                    }
                    if (showingHospitalCf != colony.hospitals_coefficient)
                    {
                        showingHospitalCf = colony.hospitals_coefficient;
                        hospitalText.text = string.Format("{0:0.##}", showingHospitalCf * 100) + '%';
                    }
                    if (showingHealthCf != colony.health_coefficient)
                    {
                        showingHealthCf = colony.health_coefficient;
                        healthText.text = string.Format("{0:0.##}", showingHealthCf * 100) + '%';
                    }
                }
                else
                {
                    if (showStorageInfo)
                    {

                        if (lastStorageOperationNumber != storage.operationsDone)
                        {
                            RecalculateStoragePanel();
                        }
                    }
                }

                if (progressPanel.activeSelf)
                {
                    switch (progressPanelMode)
                    {
                        case ProgressPanelMode.Powerplant:
                            {
                                UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
                                if (uwb == null || (!uwb.gameObject.activeSelf))
                                {
                                    DeactivateProgressPanel();
                                    return;
                                }
                                else
                                {
                                    Powerplant pp = uwb.observingWorkbuilding as Powerplant;
                                    RawImage ri = progressPanel.transform.GetChild(0).GetComponent<RawImage>();
                                    ri.texture = resourcesTexture;
                                    ri.uvRect = ResourceType.GetTextureRect(pp.GetFuelResourseID());
                                    progressPanelFullfill.fillAmount = pp.fuelLeft;
                                    progressPanelText.text = string.Format("{0:0.###}", pp.fuelLeft * 100) + '%';
                                }
                                break;
                            }
                        case ProgressPanelMode.Hangar:
                            {
                                UIHangarObserver uho = Hangar.hangarObserver;
                                if (uho == null)
                                {
                                    DeactivateProgressPanel();
                                    return;
                                }
                                else
                                {
                                    switch (uho.mode)
                                    {
                                        case HangarObserverMode.BuildingShuttle:
                                            {
                                                float x = uho.observingHangar.workflow / uho.observingHangar.workflowToProcess;
                                                progressPanelFullfill.fillAmount = x;
                                                progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                                break;
                                            }
                                        case HangarObserverMode.ShuttleInside:
                                            {
                                                float x = uho.observingHangar.shuttle.condition;
                                                progressPanelFullfill.fillAmount = x;
                                                progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                                break;
                                            }
                                        case HangarObserverMode.NoShuttle:
                                            DeactivateProgressPanel();
                                            break;
                                    }
                                }
                                break;
                            }
                        case ProgressPanelMode.RecruitingCenter:
                            {
                                UIRecruitingCenterObserver urc = RecruitingCenter.rcenterObserver;
                                if (urc == null | !urc.isObserving) return;
                                else
                                {
                                    switch (urc.mode)
                                    {
                                        case UIRecruitingCenterObserver.RecruitingCenterObserverMode.NoShowingCrew:
                                            DeactivateProgressPanel();
                                            return;
                                        case UIRecruitingCenterObserver.RecruitingCenterObserverMode.HiringCrew:
                                            {
                                                float x = urc.observingRCenter.workflow / urc.observingRCenter.workflowToProcess;
                                                progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                                progressPanelFullfill.fillAmount = x;
                                                break;
                                            }
                                        case UIRecruitingCenterObserver.RecruitingCenterObserverMode.ShowingCrewInfo:
                                            {
                                                float x = urc.showingCrew.stamina;
                                                progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                                progressPanelFullfill.fillAmount = x;
                                                break;
                                            }
                                    }
                                }
                                break;
                            }
                    }
                }
                else
                {
                    if (hospitalPanel.activeSelf)
                    {
                        int nhm = Hospital.GetBirthrateModeIndex();
                        if (nhm != hospitalPanel_savedMode)
                        {
                            switch (nhm)
                            {
                                case 0: hospitalPanel.transform.GetChild(1).GetComponent<Toggle>().isOn = true; break; // normal
                                case 1: hospitalPanel.transform.GetChild(2).GetComponent<Toggle>().isOn = true; break; // improved
                                case 2: hospitalPanel.transform.GetChild(3).GetComponent<Toggle>().isOn = true; break; // lowered
                            }
                            hospitalPanel_savedMode = nhm;
                        }
                    }
                }

                //up panel values:
                {
                    bool valuesChanged = false;
                    if (saved_freeWorkersCount != colony.freeWorkers)
                    {
                        saved_freeWorkersCount = colony.freeWorkers;
                        valuesChanged = true;
                    }
                    if (saved_citizenCount != colony.citizenCount)
                    {
                        saved_citizenCount = colony.citizenCount;
                        valuesChanged = true;
                    }
                    if (saved_livespaceCount != colony.totalLivespace)
                    {
                        saved_livespaceCount = colony.totalLivespace;
                        valuesChanged = true;
                    }
                    if (valuesChanged)
                    {
                        citizenString.text = saved_freeWorkersCount.ToString() + " / " + saved_citizenCount.ToString() + " / " + saved_livespaceCount.ToString();
                    }

                    valuesChanged = false;
                    if (saved_energyCount != colony.energyStored)
                    {
                        saved_energyCount = (int)colony.energyStored;
                        valuesChanged = true;
                    }
                    if (saved_energyMax != colony.totalEnergyCapacity)
                    {
                        saved_energyMax = (int)colony.totalEnergyCapacity;
                        valuesChanged = true;
                    }
                    float es = (int)(colony.energySurplus * 10) / 10;
                    if (saved_energySurplus != es)
                    {
                        saved_energySurplus = es;
                        valuesChanged = true;
                    }
                    if (valuesChanged)
                    {
                        string surplus = es.ToString();
                        if (es > 0) surplus = '+' + surplus;
                        energyString.text = saved_energyCount.ToString() + " / " + saved_energyMax.ToString() + " (" + surplus + ')';
                    }

                    if (saved_energyCrystalsCount != (int)colony.energyCrystalsCount)
                    {
                        saved_energyCrystalsCount = (int)colony.energyCrystalsCount;
                        energyCrystalsString.text = saved_energyCrystalsCount.ToString();
                    }
                    //date
                    GameMaster gm = GameMaster.realMaster;
                    if (gm.year > 0)
                    {
                        dataString.text = Localization.GetWord(LocalizedWord.Year_short) + ' ' + gm.year.ToString() + ' ' 
                            + Localization.GetWord(LocalizedWord.Month_short) + ' ' + gm.month.ToString() + ' ' 
                            + Localization.GetWord(LocalizedWord.Day_short) + ' ' + gm.day.ToString();
                    }
                    else
                    {
                        if (gm.month > 0)
                        {
                            dataString.text = Localization.GetWord(LocalizedWord.Month_short) + ' ' + gm.month.ToString() + ' '
                            + Localization.GetWord(LocalizedWord.Day_short) +' ' + gm.day.ToString();
                        }
                        else
                        {
                            dataString.text = Localization.GetWord(LocalizedWord.Day) + ' ' + gm.day.ToString();
                        }
                    }
                    //food val
                    float foodValue = storage.standartResources[ResourceType.FOOD_ID] + storage.standartResources[ResourceType.SUPPLIES_ID];
                    if (foodValue == 0)
                    {
                        if (!starvationSignal) {
                            saved_citizenCountBeforeStarvation = colony.citizenCount;
                            foodIconFullfill.color = Color.red;                            
                            starvationSignal = true;
                        }
                        if (saved_citizenCountBeforeStarvation != 0)
                        {
                            foodIconFullfill.fillAmount = colony.citizenCount / saved_citizenCountBeforeStarvation;
                        }
                    }
                    else
                    {
                        if (starvationSignal)
                        {
                            starvationSignal = false;
                            foodIconFullfill.color = Color.white;
                        }
                        if (colony.citizenCount != 0)
                        {
                            foodIconFullfill.fillAmount = foodValue / (colony.citizenCount * GameConstants.FOOD_CONSUMPTION * GameMaster.DAYS_IN_MONTH);
                        }
                    }
                }
            }
            if (moneyFlySpeed != 0)
            {
                Vector3 pos = moneyFlyingText.rectTransform.position;
                if (moneyFlySpeed > 0)
                {
                    moneyFlySpeed -= Time.deltaTime / 5f;
                    if (moneyFlySpeed < 0)
                    {
                        moneyFlySpeed = 0;
                        moneyFlyingText.enabled = false;
                    }
                    else
                    {
                        moneyFlyingText.rectTransform.position = Vector3.Lerp(flyingMoneyOriginalPoint + 2 * Vector3.up, flyingMoneyOriginalPoint, moneyFlySpeed);
                        moneyFlyingText.color = Color.Lerp(Color.green, new Color(0, 1, 0, 0), moneyFlySpeed);
                    }
                }
                else
                {
                    moneyFlySpeed += Time.deltaTime / 5f;
                    if (moneyFlySpeed < 1)
                    {
                        moneyFlyingText.rectTransform.position = Vector3.Lerp(flyingMoneyOriginalPoint, flyingMoneyOriginalPoint + 2 * Vector3.up, moneyFlySpeed);
                        moneyFlyingText.color = Color.Lerp(new Color(1, 0, 0, 0), Color.red, moneyFlySpeed);
                    }
                    else
                    {
                        moneyFlySpeed = 0;
                        moneyFlyingText.enabled = false;
                    }
                }
            }
        }
        if (activeAnnouncements)
        {
            Text t = announcementStrings[0];
            Color c = t.color;
            c.a = Mathf.Lerp(c.a, 0, DISSAPPEAR_SPEED * Time.deltaTime * GameMaster.gameSpeed * (2 - c.a) * (2 - c.a));
            if (c.a > 0.05f) t.color = c;
            else
            {
                if (announcementStrings[1].enabled)
                {
                    int lastIndex = 1;
                    t.color = Color.black;
                    int i = 1;
                    while (i < announcementStrings.Length)
                    {
                        if (!announcementStrings[i].enabled) break;
                        else lastIndex = i;
                        announcementStrings[i - 1].text = announcementStrings[i].text;
                        i++;
                    }
                    announcementStrings[lastIndex].enabled = false;
                }
                else
                {
                    t.enabled = false;
                    activeAnnouncements = false;
                }
            }
        }
    }

    #region menu
    public void MenuButton()
    {
        showMenuWindow = !showMenuWindow;
        if (showMenuWindow)
        { // on
            GameMaster.SetPause(true);
            if (rightPanel.activeSelf) { rightPanel.SetActive(false); FollowingCamera.main.ResetTouchRightBorder(); }
            if (leftPanel.activeSelf) leftPanel.SetActive(false);
            if (SurfaceBlock.surfaceObserver != null) SurfaceBlock.surfaceObserver.ShutOff();
            if (showColonyInfo) ColonyButton();
            ChangeActiveWindow(ActiveWindowMode.GameMenu);
            menuPanel.transform.GetChild(MENUPANEL_SAVE_BUTTON_INDEX).GetComponent<Button>().interactable = (GameMaster.realMaster.colonyController != null);
            menuPanel.SetActive(true);
            //menuButton.transform.SetAsLastSibling();
            MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GamePaused));
            SetMenuPanelSelection(MenuSection.NoSelection);
        }
        else
        { //off
            GameMaster.SetPause(false);            
            FollowingCamera.main.ResetTouchRightBorder();
            menuPanel.SetActive(false);
            leftPanel.SetActive(true);
            SetMenuPanelSelection(MenuSection.NoSelection);
            menuButton.GetComponent<Image>().overrideSprite = null;
            MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GameUnpaused));
            DropActiveWindow(ActiveWindowMode.GameMenu);
        }
    }
    public void SaveButton()
    {
        SaveSystemUI.current.Activate(true, false);
        SetMenuPanelSelection(MenuSection.Save);
    }
    public void LoadButton()
    {
        SaveSystemUI.current.Activate(false, false);
        SetMenuPanelSelection(MenuSection.Load);
    }
    public void OptionsButton()
    {
        optionsPanel.SetActive(true);
        SetMenuPanelSelection(MenuSection.Options);
    }
    public void ToMainMenu()
    {
        SetMenuPanelSelection(MenuSection.NoSelection);
        GameMaster.ChangeScene(GameLevel.Menu);
    }
    public void ExitButton()
    {
        SetMenuPanelSelection(MenuSection.NoSelection);
        // запрос на сохранение?
        Application.Quit();
    }

    void SetMenuPanelSelection(MenuSection ms)
    {
        if (ms == selectedMenuSection) return;
        else
        {
            switch (selectedMenuSection)
            {
                case MenuSection.Save:
                    menuPanel.transform.GetChild(MENUPANEL_SAVE_BUTTON_INDEX).GetComponent<Image>().overrideSprite = null;
                    SaveSystemUI.current.CloseButton();
                    break;
                case MenuSection.Load:
                    menuPanel.transform.GetChild(MENUPANEL_LOAD_BUTTON_INDEX).GetComponent<Image>().overrideSprite = null;
                    SaveSystemUI.current.CloseButton();
                    break;
                case MenuSection.Options:
                    menuPanel.transform.GetChild(MENUPANEL_OPTIONS_BUTTON_INDEX).GetComponent<Image>().overrideSprite = null;
                    optionsPanel.SetActive(false);
                    break;
            }
            selectedMenuSection = ms;
            switch (selectedMenuSection)
            {
                case MenuSection.Save: menuPanel.transform.GetChild(MENUPANEL_SAVE_BUTTON_INDEX).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case MenuSection.Load: menuPanel.transform.GetChild(MENUPANEL_LOAD_BUTTON_INDEX).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case MenuSection.Options: menuPanel.transform.GetChild(MENUPANEL_OPTIONS_BUTTON_INDEX).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
            }
        }
    }

    #endregion

    public void Raycasting()
    {
        if (colony == null || colony.hq == null) return;
        // кастует луч, проверяет, выделен ли уже этот объект, если нет - меняет режим через ChangeChosenObject
        if (FollowingCamera.touchscreen )
        {
            if (Input.touchCount != 1 | FollowingCamera.camRotateTrace > 0) return;
        }
        Vector2 mpos = Input.mousePosition;
        RaycastHit rh;
        if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh))
        {
            GameObject collided = rh.collider.gameObject;
            switch (collided.tag)
            {
                case "Structure":
                    {
                        Structure s = collided.transform.parent.GetComponent<Structure>();
                        if (s == null) ChangeChosenObject(ChosenObjectType.None);
                        else
                        {
                            if (chosenStructure == s) return;
                            else
                            {
                                chosenStructure = s;
                                chosenSurface = s.basement;
                                chosenCube = null;
                                chosenWorksite = null;
                                ChangeChosenObject(ChosenObjectType.Structure);
                            }
                        }
                        break;
                    }
                case Block.BLOCK_COLLIDER_TAG:
                    {
                        Block b = collided.transform.parent.GetComponent<Block>();
                        if (b == null) b = collided.transform.parent.parent.GetComponent<Block>(); // cave block
                        if (b == null) ChangeChosenObject(ChosenObjectType.None);
                        switch (b.type)
                        {
                            case BlockType.Cave:
                            case BlockType.Surface:
                                SurfaceBlock sb = b as SurfaceBlock;
                                if (sb == chosenSurface & chosenObjectType == ChosenObjectType.Surface) return;
                                else
                                {
                                    chosenSurface = sb;
                                    chosenStructure = null;
                                    chosenCube = null;
                                    chosenWorksite = sb.worksite;
                                    ChangeChosenObject(ChosenObjectType.Surface);
                                }
                                break;
                            case BlockType.Cube:
                                CubeBlock cb = b as CubeBlock;
                                if (cb == chosenCube) return;
                                else
                                {
                                    chosenCube = cb;
                                    chosenSurface = null;
                                    chosenStructure = null;
                                    chosenWorksite = cb.worksite;
                                    faceIndex = 10;
                                    for (byte i = 0; i < 6; i++)
                                    {
                                        if (chosenCube.faces[i] == null) continue;
                                        if (chosenCube.faces[i].GetComponent<Collider>() == rh.collider) { faceIndex = i; break; }
                                    }
                                    if (faceIndex < 6) ChangeChosenObject(ChosenObjectType.Cube);
                                    else ChangeChosenObject(ChosenObjectType.None);
                                }
                                break;
                        }
                    }
                    break;
                case "WorksiteSign":
                    {
                        WorksiteSign ws = collided.GetComponent<WorksiteSign>();
                        if (ws != null)
                        {
                            if (ws.worksite == chosenWorksite) return;
                            else
                            {
                                chosenCube = null;
                                chosenSurface = null;
                                chosenStructure = null;
                                chosenWorksite = ws.worksite;
                                ChangeChosenObject(ChosenObjectType.Worksite);
                            }
                        }
                        else ChangeChosenObject(ChosenObjectType.None);
                    }
                    break;
                default:
                    if (collided.transform.parent != null)
                    {
                        if (collided.transform.parent.gameObject.GetInstanceID() == interceptingConstructPlaneID) UISurfacePanelController.current.ConstructingPlaneTouch(rh.point);
                    }
                    break;
            }
        }
        else SelectedObjectLost();
    }

    public void ChangeChosenObject(ChosenObjectType newChosenType)
    {

        if (hospitalPanel.activeSelf) DeactivateHospitalPanel();
        else
        {
           if (rollingShopPanel.activeSelf) DeactivateWorkshopPanel();
        }

        //отключение предыдущего observer
        if (workingObserver != null)
        {
            workingObserver.ShutOff();
            workingObserver = null;
        }
        bool disableCubeMenuButtons = true, changeFrameColor = true;
        if (newChosenType == ChosenObjectType.None)
        {
            rightPanel.SetActive(false);
            FollowingCamera.main.ResetTouchRightBorder();
            selectionFrame.gameObject.SetActive(false);
            chosenObjectType = ChosenObjectType.None;
            chosenWorksite = null;
            chosenStructure = null;
            chosenCube = null;
            chosenSurface = null;
            faceIndex = 10;
            changeFrameColor = false;
        }
        else
        {
            switch (newChosenType)
            {
                case ChosenObjectType.Cube: if (chosenCube == null) return; else break;
                case ChosenObjectType.Structure: if (chosenStructure == null) return; else break;
                case ChosenObjectType.Surface: if (chosenSurface == null) return; else break;
                case ChosenObjectType.Worksite: if (chosenWorksite == null) return; else break;
            }
            FollowingCamera.main.SetTouchRightBorder(rightPanel.GetComponent<RectTransform>().anchorMin.x * Screen.width);
            chosenObjectType = newChosenType;
            rightPanel.transform.SetAsLastSibling();
            rightPanel.SetActive(true);
            disableCubeMenuButtons = true;
            selectionFrame.gameObject.SetActive(true);
            if (showMenuWindow)
            {
                MenuButton();
            }
        }

        Vector3 sframeColor = Vector3.one;
        switch (chosenObjectType)
        {
            case ChosenObjectType.Surface:
                {
                    faceIndex = 10; // вспомогательная дата для chosenCube
                    selectionFrame.position = chosenSurface.transform.position + Vector3.down * Block.QUAD_SIZE / 2f;
                    selectionFrame.rotation = Quaternion.identity;
                    selectionFrame.localScale = new Vector3(SurfaceBlock.INNER_RESOLUTION, 1, SurfaceBlock.INNER_RESOLUTION);
                    sframeColor = new Vector3(140f / 255f, 1, 1);

                    workingObserver = chosenSurface.ShowOnGUI();
                    FollowingCamera.main.SetLookPoint(chosenSurface.transform.position);
                }
                break;

            case ChosenObjectType.Cube:
                {
                    if (chosenCube.faces[faceIndex] != null)
                    {
                        selectionFrame.position = chosenCube.faces[faceIndex].transform.position;
                        switch (faceIndex)
                        {
                            case 0: selectionFrame.transform.rotation = Quaternion.Euler(90, 0, 0); break;
                            case 1: selectionFrame.transform.rotation = Quaternion.Euler(0, 0, -90); break;
                            case 2: selectionFrame.transform.rotation = Quaternion.Euler(-90, 0, 0); break;
                            case 3: selectionFrame.transform.rotation = Quaternion.Euler(0, 0, 90); break;
                            case 4: selectionFrame.transform.rotation = Quaternion.identity; break;
                            case 5: selectionFrame.transform.rotation = Quaternion.Euler(-180, 0, 0); break;
                        }
                        selectionFrame.localScale = new Vector3(SurfaceBlock.INNER_RESOLUTION, 1, SurfaceBlock.INNER_RESOLUTION);
                        sframeColor = new Vector3(140f / 255f, 1, 0.9f);
                    }
                    else selectionFrame.gameObject.SetActive(false);

                    FollowingCamera.main.SetLookPoint(chosenCube.transform.position);

                    Transform t = rightPanel.transform;
                    t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX).gameObject.SetActive(true);
                    t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 1).gameObject.SetActive(chosenCube.excavatingStatus != 0); // pour in button
                    t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 2).gameObject.SetActive(faceIndex == 4); // make surface button
                    disableCubeMenuButtons = false;
                }
                break;

            case ChosenObjectType.Structure:
                faceIndex = 10; // вспомогательная дата для chosenCube
                selectionFrame.position = chosenStructure.transform.position;
                selectionFrame.rotation = chosenStructure.transform.rotation;
                selectionFrame.localScale = new Vector3(chosenStructure.innerPosition.size, 1, chosenStructure.innerPosition.size);
                sframeColor = new Vector3(1, 0, 1);
                workingObserver = chosenStructure.ShowOnGUI();
                FollowingCamera.main.SetLookPoint(chosenStructure.transform.position);
                break;

            case ChosenObjectType.Worksite:
                faceIndex = 10; // вспомогательная дата для chosenCube
                selectionFrame.gameObject.SetActive(false);
                changeFrameColor = false;
                workingObserver = chosenWorksite.ShowOnGUI();
                FollowingCamera.main.SetLookPoint(chosenWorksite.sign.transform.position);
                break;
        }
        if (disableCubeMenuButtons)
        {
            Transform t = rightPanel.transform;
            t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX).gameObject.SetActive(false);
            t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 1).gameObject.SetActive(false);
            t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 2).gameObject.SetActive(false);
        }
        if (changeFrameColor)
        {
            selectionFrameMaterial.SetColor("_TintColor", Color.HSVToRGB(sframeColor.x, sframeColor.y, sframeColor.z));
            selectionFrame.gameObject.SetActive(true);
        }
    }
    public void ChangeChosenObject(CubeBlock cb)
    {
        chosenCube = cb;
        faceIndex = 4;
        ChangeChosenObject(ChosenObjectType.Cube);
    }
    public void ShowWorksite(Worksite ws)
    {
        chosenWorksite = ws;
        ChangeChosenObject(ChosenObjectType.Worksite);
    }

    public void ChangeActiveWindow(ActiveWindowMode mode)
    {
        if (mode == currentActiveWindowMode) return;
        //deactivating previous panel
        if (currentActiveWindowMode != ActiveWindowMode.NoWindow)       
        {
            switch (currentActiveWindowMode)
            {
                case ActiveWindowMode.BuildPanel:
                    UISurfacePanelController.current.ChangeMode(SurfacePanelMode.SelectAction);
                    break;
                case ActiveWindowMode.QuestPanel:
                    questUI.Deactivate();
                    break;
                case ActiveWindowMode.SpecificBuildPanel:
                    UISurfacePanelController.current.SetCostPanelMode(CostPanelMode.Disabled);
                    break;
                case ActiveWindowMode.StoragePanel:
                    if (showStorageInfo) StorageButton();
                    break;
                case ActiveWindowMode.TradePanel:
                    if (tradePanel.activeSelf) CloseTradePanel();
                    break;
                case ActiveWindowMode.ExpeditionPanel:
                    if (expeditionPanel.activeSelf) expeditionPanel.SetActive(false);
                    break;
            }
        }
        currentActiveWindowMode = mode;
        if (currentActiveWindowMode == ActiveWindowMode.ExpeditionPanel)
        {
            expeditionPanel.GetComponent<ExpeditionPanelUI>().Activate();
        }
        bool deactivating = (mode == ActiveWindowMode.NoWindow);
        FollowingCamera fc = FollowingCamera.main;
        fc.ControllerStickActivity(deactivating);
        fc.CameraRotationBlock(!deactivating);
    }
    public void DropActiveWindow(ActiveWindowMode droppingMode)
    {
        if (droppingMode == currentActiveWindowMode)
        {
            currentActiveWindowMode = ActiveWindowMode.NoWindow;
        }
        FollowingCamera fc = FollowingCamera.main;
        fc.ControllerStickActivity(true);
        fc.CameraRotationBlock(false);
    }

    public void FullDeactivation()
    {
        ChangeChosenObject(ChosenObjectType.None);
        ChangeActiveWindow(ActiveWindowMode.NoWindow);
        leftPanel.SetActive(false);
        menuPanel.SetActive(false);
        upPanel.SetActive(false);
    }
    public void FullReactivation()
    {
        leftPanel.SetActive(true);
        menuPanel.SetActive(true);
        upPanel.SetActive(true);
    }

    #region auxiliary panels
    public void ActivateProgressPanel(ProgressPanelMode mode)
    {
        switch (mode)
        {
            case ProgressPanelMode.Powerplant:
                {
                    UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
                    if (uwb == null || !uwb.isObserving)
                    {
                        DeactivateProgressPanel();
                        return;
                    }
                    else
                    {
                        Powerplant pp = uwb.observingWorkbuilding as Powerplant;
                        progressPanelIcon.texture = resourcesTexture;
                        int resourceID = pp.GetFuelResourseID();
                        progressPanelIcon.uvRect = ResourceType.GetTextureRect(resourceID);
                        Text resourceNameText = progressPanelIcon.transform.GetChild(0).GetComponent<Text>();
                        resourceNameText.text = Localization.GetResourceName(resourceID);
                        resourceNameText.gameObject.SetActive(true);

                        progressPanelFullfill.color = PoolMaster.gameOrangeColor;
                        progressPanelFullfill.fillAmount = pp.fuelLeft;
                        progressPanelText.text = string.Format("{0:0.###}", pp.fuelLeft) + '%';

                        progressPanel.SetActive(true);
                        progressPanelMode = mode;
                    }
                    break;
                }
            case ProgressPanelMode.Hangar:
                {
                    UIHangarObserver uho = Hangar.hangarObserver;
                    if (uho == null || !uho.isObserving) { DeactivateProgressPanel(); return; }
                    else
                    {
                        progressPanelIcon.transform.GetChild(0).gameObject.SetActive(false);
                        progressPanelIcon.texture = iconsTexture;
                        Hangar h = uho.observingHangar;
                        Shuttle sh = h.shuttle;
                        bool haveShuttle = !(sh == null);
                        if (haveShuttle)
                        {
                            progressPanelIcon.uvRect = GetTextureUV(sh.condition > 0.85 ? Icons.ShuttleGoodIcon : (sh.condition < 0.5 ? Icons.ShuttleBadIcon : Icons.ShuttleNormalIcon));
                            if (uho.mode == HangarObserverMode.ShuttleOnMission)
                            {
                                progressPanelText.text = Localization.GetPhrase(LocalizedPhrase.ShuttleOnMission);
                                progressPanelFullfill.fillAmount = 1;
                                progressPanelFullfill.color = Color.white;
                            }
                            else
                            {
                                progressPanelText.text = ((int)(sh.condition * 100)).ToString() + '%';
                                progressPanelFullfill.fillAmount = sh.condition;
                                progressPanelFullfill.color = Color.green;
                            }
                        }
                        else
                        {
                            progressPanelIcon.uvRect = GetTextureUV(Icons.TaskFrame);
                            if (h.constructing)
                            {
                                float x = h.workflow / h.workflowToProcess;
                                progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                progressPanelFullfill.fillAmount = x;
                                progressPanelFullfill.color = PoolMaster.gameOrangeColor;
                            }
                            else
                            {
                                DeactivateProgressPanel();
                                return;
                            }
                        }
                    }
                    break;
                }
            case ProgressPanelMode.RecruitingCenter:
                {
                    UIRecruitingCenterObserver urc = RecruitingCenter.rcenterObserver;
                    if (urc == null || !urc.isObserving) { DeactivateProgressPanel(); return; }
                    else
                    {
                        progressPanelIcon.transform.GetChild(0).gameObject.SetActive(false);
                        switch (urc.mode)
                        {
                            case UIRecruitingCenterObserver.RecruitingCenterObserverMode.NoShowingCrew:
                                DeactivateProgressPanel();
                                return;
                            case UIRecruitingCenterObserver.RecruitingCenterObserverMode.HiringCrew:
                                {
                                    progressPanelIcon.texture = iconsTexture;
                                    progressPanelIcon.uvRect = GetTextureUV(Icons.TaskFrame);
                                    float x = urc.observingRCenter.workflow / urc.observingRCenter.workflowToProcess;
                                    progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                    progressPanelFullfill.fillAmount = x;
                                    progressPanelFullfill.color = PoolMaster.gameOrangeColor;
                                    break;
                                }
                            case UIRecruitingCenterObserver.RecruitingCenterObserverMode.ShowingCrewInfo:
                                {
                                    progressPanelIcon.texture = iconsTexture;
                                    float c = urc.showingCrew.stamina;
                                    progressPanelIcon.uvRect = GetTextureUV(c > Crew.HIGH_STAMINA_VALUE ? Icons.CrewGoodIcon : (c < Crew.LOW_STAMINA_VALUE ? Icons.CrewBadIcon : Icons.CrewNormalIcon));
                                    progressPanelText.text = ((int)(c * 100)).ToString() + '%';
                                    progressPanelFullfill.fillAmount = c;
                                    progressPanelFullfill.color = Color.blue;
                                    break;
                                }
                        }
                    }
                    break;
                }
        }
        progressPanel.SetActive(true);
        progressPanelMode = mode;
    }
    public void DeactivateProgressPanel()
    {
        //print("detector");
        progressPanel.SetActive(false);
        progressPanelMode = ProgressPanelMode.Offline;
    }

    public void ActivateWorkshopPanel()
    {
        UIWorkbuildingObserver wbo = WorkBuilding.workbuildingObserver;        
        if (wbo != null && wbo.gameObject.activeSelf)
        { // уу, костыли!
            // и вообще надо переделать на dropdown           
            Workshop rs = wbo.observingWorkbuilding as Workshop;
            if (rs != null)
            {
                Dropdown d = rollingShopPanel.transform.GetChild(0).GetComponent<Dropdown>();
                d.value = (int)rs.mode;             
                rollingShopPanel.SetActive(true);
            }
        }
        rollingShopPanel.SetActive(true);
    }
    public void DeactivateWorkshopPanel()
    {
        rollingShopPanel.SetActive(false);
    }
    public void Workshop_SetActivity(int input)
    {
        byte x = (byte)input;
        UIWorkbuildingObserver wbo = WorkBuilding.workbuildingObserver;
        if (wbo == null | !wbo.gameObject.activeSelf)
        {
            DeactivateWorkshopPanel();
            return;
        }
        else
        { 
            Workshop rs = wbo.observingWorkbuilding as Workshop;
            if (rs == null)
            {
                DeactivateWorkshopPanel();
                return;
            }
            else
            {
                rs.SetMode((byte)input);
            }
        }
    }

    public void ActivateHospitalPanel()
    {
        int hm = Hospital.GetBirthrateModeIndex();
        switch (hm)
        {
            case 0: hospitalPanel.transform.GetChild(1).GetComponent<Toggle>().isOn = true; break; // normal
            case 1: hospitalPanel.transform.GetChild(2).GetComponent<Toggle>().isOn = true; break; // improved
            case 2: hospitalPanel.transform.GetChild(3).GetComponent<Toggle>().isOn = true; break; // lowered
        }
        hospitalPanel_savedMode = hm;
        hospitalPanel.SetActive(true);
    }
    public void DeactivateHospitalPanel()
    {
        hospitalPanel.SetActive(false);
    }
    public void Hospital_SetBirthrateMode(int i)
    {
        if (i != Hospital.GetBirthrateModeIndex()) Hospital.SetBirthrateMode(i);
    }

    public void ActivateTradePanel()
    {
        tradePanel.SetActive(true);
    }
    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
    }
    #endregion

    public static Rect GetTextureUV(Icons i)
    {
        float p = 0.125f;
        switch (i)
        {
            default: return Rect.zero;
            case Icons.GreenArrow: return new Rect(6 * p, 7 * p, p, p);
            case Icons.GuidingStar: return new Rect(7 * p, 7 * p, p, p);
            case Icons.PowerOff: return new Rect(2 * p, 7 * p, p, p);
            case Icons.PowerPlus: return new Rect(3 * p, 7 * p, p, p);
            case Icons.PowerMinus: return new Rect(4 * p, 7 * p, p, p);
            case Icons.Citizen: return new Rect(p, 6 * p, p, p);
            case Icons.RedArrow: return new Rect(2 * p, 6 * p, p, p);
            case Icons.CrewBadIcon: return new Rect(p, 5 * p, p, p);
            case Icons.CrewNormalIcon: return new Rect(2 * p, 5 * p, p, p);
            case Icons.CrewGoodIcon: return new Rect(3 * p, 5 * p, p, p);
            case Icons.ShuttleBadIcon: return new Rect(4 * p, 5 * p, p, p);
            case Icons.ShuttleNormalIcon: return new Rect(5 * p, 5 * p, p, p);
            case Icons.ShuttleGoodIcon: return new Rect(6 * p, 5 * p, p, p);
            case Icons.TaskFrame: return new Rect(3 * p, 4 * p, p, p);
            case Icons.TaskCompleted: return new Rect(4 * p, 4 * p, p, p);
            case Icons.DisabledBuilding: return new Rect(p, 3*p, p,p);
        }
    }

    public void MakeAnnouncement(string s)
    {
        MakeAnnouncement(s, Color.black);
    }
    public void MakeAnnouncement(string s, Color col)
    {
        int lastIndex = 0, len = announcementStrings.Length;
        for (int i = 0; i < len; i++)
        {
            lastIndex = i;
            if (announcementStrings[i].enabled == false) break;
        }
        if (lastIndex == len - 1)
        { // сдвигаем все на одну позицию назад
            for (int i = 1; i < len; i++)
            {
                announcementStrings[i - 1].text = announcementStrings[i].text;
                announcementStrings[i - 1].color = announcementStrings[i].color;
            }
            announcementStrings[len - 1].text = s;
            announcementStrings[len - 1].color = col;
        }
        else
        {
            Text t = announcementStrings[lastIndex];
            t.enabled = true;
            t.text = s;
            t.color = col;
        }
        activeAnnouncements = true;
    }
    public void MoneyChanging(float f)
    {
        if (f > 0)
        {
            moneyFlyingText.color = Color.green;
            moneyFlyingText.text = '+' + string.Format("{0:0.##}", f);
            moneyFlySpeed = 1;
            moneyFlyingText.rectTransform.position = flyingMoneyOriginalPoint - Vector3.up * 2;
        }
        else
        {
            moneyFlyingText.color = new Color(1, 0, 0, 0);
            moneyFlyingText.text = string.Format("{0:0.##}", f);
            moneyFlySpeed = -1;
            moneyFlyingText.rectTransform.position = flyingMoneyOriginalPoint;
        }
        moneyFlyingText.enabled = true;
    }

    #region leftPanel
    public void ActivateQuestUI()
    {
        questUI.Activate();
        leftPanel.SetActive(false);
        storagePanel.SetActive(false);
    }
    public void ActivateLeftPanel()
    {
        leftPanel.SetActive(true);
    }
    public void ColonyButton()
    {
        showColonyInfo = !showColonyInfo;
        if (showColonyInfo)
        {
            if (showStorageInfo) StorageButton();
            else
            {
                if (showLayerCut) LayerCutButton();
            }
            colonyPanel.SetActive(true);
            colonyToggleButton.overrideSprite = PoolMaster.gui_overridingSprite;
            if (colony == null) return;
            showingGearsCf = colony.gears_coefficient;
            showingHappinessCf = colony.happiness_coefficient;
            showingBirthrate = colony.realBirthrate;
            showingHospitalCf = colony.hospitals_coefficient;
            showingHealthCf = colony.health_coefficient;
            showingHousingCf = colony.housingLevel;
            gearsText.text = string.Format("{0:0.###}", showingGearsCf);
            happinessText.text = string.Format("{0:0.##}", showingHappinessCf * 100) + '%';
            birthrateText.text = showingBirthrate > 0 ? '+' + string.Format("{0:0.#####}", showingBirthrate) : string.Format("{0:0.#####}", showingBirthrate);
            housingText.text = string.Format("{0:0.##}", showingHousingCf);
            hospitalText.text = string.Format("{0:0.##}", showingHospitalCf * 100) + '%';
            healthText.text = string.Format("{0:0.##}", showingHealthCf * 100) + '%';
        }
        else
        {
            colonyPanel.SetActive(false);
            colonyToggleButton.overrideSprite = null;
        }
    }
    public void StorageButton()
    {
        showStorageInfo = !showStorageInfo;
        if (showStorageInfo)
        {
            if (showColonyInfo) ColonyButton();
            else
            {
                if (showLayerCut) LayerCutButton();
            }
            ChangeActiveWindow(ActiveWindowMode.StoragePanel);
            storageToggleButton.overrideSprite = PoolMaster.gui_overridingSprite;
            storagePanel.SetActive(true);
            RecalculateStoragePanel();
        }
        else
        {
            DropActiveWindow(ActiveWindowMode.StoragePanel);
            storageToggleButton.overrideSprite = null;
            storagePanel.SetActive(false);            
        }
    }
    public void LayerCutButton()
    {
        showLayerCut = !showLayerCut;
        if (showLayerCut)
        { // layercut on
            if (showColonyInfo) ColonyButton();
            else
            {
                if (showStorageInfo) StorageButton();
            }
            layerCutToggleButton.overrideSprite = PoolMaster.gui_overridingSprite;
            int p = GameMaster.layerCutHeight;
            GameMaster.layerCutHeight = GameMaster.prevCutHeight;
            if (GameMaster.layerCutHeight != p) GameMaster.realMaster.mainChunk.LayersCut();
        }
        else // off
        {
            layerCutToggleButton.overrideSprite = null;
            GameMaster.prevCutHeight = GameMaster.layerCutHeight;
            GameMaster.layerCutHeight = Chunk.CHUNK_SIZE;
            GameMaster.realMaster.mainChunk.LayersCut();
        }

        Transform t = layerCutToggleButton.transform;
        t.GetChild(1).gameObject.SetActive(showLayerCut); // plus button
        t.GetChild(2).gameObject.SetActive(showLayerCut); // minus button
        t.GetChild(3).gameObject.SetActive(showLayerCut); // level cut value   
        layerCutToggleButton.transform.GetChild(3).GetComponent<Text>().text = GameMaster.layerCutHeight.ToString();
    }
    public void LayerCutPlus()
    {
        GameMaster.layerCutHeight++;
        if (GameMaster.layerCutHeight > Chunk.CHUNK_SIZE) GameMaster.layerCutHeight = Chunk.CHUNK_SIZE;
        else GameMaster.realMaster.mainChunk.LayersCut();
        layerCutToggleButton.transform.GetChild(3).GetComponent<Text>().text = GameMaster.layerCutHeight.ToString();
    }
    public void LayerCutMinus()
    {
        GameMaster.layerCutHeight--;
        if (GameMaster.layerCutHeight < 0) GameMaster.layerCutHeight = 0;
        else GameMaster.realMaster.mainChunk.LayersCut();
        layerCutToggleButton.transform.GetChild(3).GetComponent<Text>().text = GameMaster.layerCutHeight.ToString();
    }

    void RecalculateStoragePanel()
    {
        int i = 0;
        if (!storagePositionsPrepared)
        {
            storagePanelContent.gameObject.SetActive(false);
            RectTransform t = null;
            for (; i < ResourceType.resourceTypesArray.Length; i++)
            {
                t = Instantiate(storagePanelContent.GetChild(0), storagePanelContent) as RectTransform;
                t.localPosition += Vector3.down * (i + 1) * t.rect.height;
            }
            (storagePanelContent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, i * t.rect.height);
            storagePanelContent.gameObject.SetActive(true);
            storagePositionsPrepared = true;
        }
        Storage st = colony.storage;        
        if (st.totalVolume == 0)
        {
            //empty storage
            for (i = 0; i < storagePanelContent.childCount + 1; i++)
            {
                storagePanelContent.GetChild(i).gameObject.SetActive(false);
            }
            Transform t = storagePanelContent.GetChild(0);
            t.GetChild(0).GetComponent<RawImage>().enabled = false;
            t.GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Total) + ':';
            t.GetChild(2).GetComponent<Text>().text = (((int)(st.totalVolume * 100)) / 100f).ToString() + " / " + st.maxVolume.ToString();
        }
        else
        {
            List<int> resourceIDs = new List<int>();
            float[] resources = st.standartResources;
            for(i = 0; i < resources.Length; i++)
            {
                if (resources[i] > 0) resourceIDs.Add(i);
            }
            Transform t;
            int resourceID;
            // redraw 
            for (i = 0; i < resourceIDs.Count; i++)
            {
                resourceID = resourceIDs[i];
                t = storagePanelContent.GetChild(i);
                t.GetChild(0).GetComponent<RawImage>().enabled = true;
                t.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetTextureRect(resourceID);
                t.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(resourceID);
                t.GetChild(2).GetComponent<Text>().text = ((int)(resources[resourceID] * 10) / 10f).ToString();
                t.gameObject.SetActive(true);
            }
            t = storagePanelContent.GetChild(i);
            t.GetChild(0).GetComponent<RawImage>().enabled = false;
            t.GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Total) + ':';
            t.GetChild(2).GetComponent<Text>().text = (((int)(st.totalVolume * 100)) / 100f).ToString() + " / " + st.maxVolume.ToString();
            t.gameObject.SetActive(true);
            i++;
            if (i < storagePanelContent.childCount)
            {
                for (int j = i; j < storagePanelContent.childCount; j++)
                    storagePanelContent.GetChild(j).gameObject.SetActive(false);
            }
            //storagePanel.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>().size = 
        }
    }
    #endregion

    public void LocalizeButtonTitles()
    {
        Transform t = hospitalPanel.transform;
        t.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.BirthrateMode) + " :";
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Normal);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Improved) + " (" + string.Format("{0:0.##}", Hospital.improvedCoefficient) + "%)";
        t.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Lowered) + " (" + string.Format("{0:0.##}", Hospital.loweredCoefficient) + "%)";

        t = menuPanel.transform;
        t.GetChild(0).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Save);
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Load);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Options);
        t.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.MainMenu);
        t.GetChild(4).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Exit);

        optionsPanel.GetComponent<GameSettingsUI>().LocalizeTitles();

        t = rightPanel.transform;
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dig);
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.PourIn);
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 2).GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.MakeSurface);

        Dropdown d = rollingShopPanel.transform.GetChild(0).GetComponent<Dropdown>();
        d.options = new List<Dropdown.OptionData>();
        d.options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoActivity)));
        d.options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.ImproveGears)));
        localized = true;
    }

    #region right panel
    public void SelectedObjectLost()
    {
        if (chosenObjectType == ChosenObjectType.None) return;
        ChangeChosenObject(ChosenObjectType.None);
    }

    public void DigCube()
    {
        if (chosenCube == null) return;
        else
        {
            if (faceIndex == 4)
            {
                SurfaceBlock sb = chosenCube.myChunk.GetBlock(chosenCube.pos.x, chosenCube.pos.y + 1, chosenCube.pos.z) as SurfaceBlock;
                if (sb == null)
                {
                    DigSite ds = chosenCube.gameObject.AddComponent<DigSite>();
                    ds.Set(chosenCube, true);
                    workingObserver = ds.ShowOnGUI(); // вообще они должны сами в конце цепочки устанавливать здесь workingObserver, не?
                }
                else
                {
                    CleanSite cs = chosenCube.gameObject.AddComponent<CleanSite>();
                    cs.Set(sb, true);
                    workingObserver = cs.ShowOnGUI();
                }
            }
            else
            {
                if (faceIndex < 4)
                {
                    TunnelBuildingSite tbs = chosenCube.gameObject.AddComponent<TunnelBuildingSite>();
                    tbs.Set(chosenCube);
                    tbs.CreateSign(faceIndex);
                    workingObserver = tbs.ShowOnGUI();
                }
            }
        }
    }
    public void PourInCube()
    {
        if (chosenCube == null || chosenCube.excavatingStatus == 0) return;
        else
        {
            DigSite ds = chosenCube.gameObject.AddComponent<DigSite>();
            ds.Set(chosenCube, false);
            ds.ShowOnGUI();
        }
    }
    public void MakeSurfaceOnCube()
    {
        Block b = chosenCube.myChunk.AddBlock(new ChunkPos(chosenCube.pos.x, chosenCube.pos.y + 1, chosenCube.pos.z), BlockType.Surface, chosenCube.material_id, false);
        if (b != null)
        {
            chosenSurface = b as SurfaceBlock;
            ChangeChosenObject(ChosenObjectType.Surface);
        }
        else MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.ActionError));
    }
    #endregion
}
