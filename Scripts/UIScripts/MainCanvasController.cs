using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChosenObjectType : byte { None, Plane, Structure, Worksite }

public enum ProgressPanelMode : byte { Offline, Powerplant, Hangar, RecruitingCenter, Settlement }
public enum ActiveWindowMode : byte { NoWindow, TradePanel, StoragePanel, BuildPanel, SpecificBuildPanel, QuestPanel, GameMenu, ExpeditionPanel, LogWindow }


sealed public class MainCanvasController : MonoBehaviour,IObserverController, ILocalizable
{
    //prev UIController
    public GameObject rightPanel, upPanel, menuPanel, menuButton; // fill in the Inspector
    public Button closePanelButton; // fill in the Inspector

#pragma warning disable 0649
    [SerializeField] private GameObject colonyPanel, tradePanel, hospitalPanel, progressPanel, storagePanel, leftPanel, 
        colonyRenameButton, rightFastPanel, housingNotEnoughMarker, gearsProblemMarker; // fiti
    [SerializeField] private Text gearsText, happinessText, birthrateText, hospitalText, housingText, citizenString, energyString, energyCrystalsString, moneyFlyingText, progressPanelText, dataString;
    [SerializeField] private Image colonyToggleButton, storageToggleButton, layerCutToggleButton, storageOccupancyFullfill, progressPanelFullfill, foodIconFullfill;
    [SerializeField] private Transform storagePanelContent, gameCanvas;
    [SerializeField] private Button touchzone;
    [SerializeField] private RawImage progressPanelIcon;
    [SerializeField] private QuestUI _questUI;
    [SerializeField] private InputField colonyNameField;
#pragma warning restore 0649

    public bool showLayerCut { get; private set; }
    public delegate void StatusUpdateHandler();
    public event StatusUpdateHandler statusUpdateEvent;
    public ActiveWindowMode currentActiveWindowMode { get; private set; }
    public ProgressPanelMode progressPanelMode { get; private set; }    
    public Texture resourcesIcons { get; private set; }
    public Texture buildingsIcons { get; private set; }
    public Plane selectedPlane { get; private set; }
    public QuestUI questUI { get; private set; }
    public UIController uicontroller { get; private set; }

    private Vector3 flyingMoneyOriginalPoint = Vector3.zero;

  

    private const float DATA_UPDATE_TIME = 1, STATUS_UPDATE_TIME = 1;
    public int interceptingConstructPlaneID = -1;

    private float showingGearsCf, showingHappinessCf, showingBirthrate, showingHospitalCf,
    updateTimer, moneyFlySpeed = 0;
    private byte showingStorageOccupancy, selectedFaceIndex = 10;
    private float saved_energySurplus, statusUpdateTimer = 0, powerFailureTimer = 0;
    private int saved_citizenCount, saved_freeWorkersCount, saved_livespaceCount, saved_energyCount, saved_energyMax, saved_energyCrystalsCount,
        hospitalPanel_savedMode, exCorpus_savedCrewsCount, exCorpus_savedShuttlesCount, exCorpus_savedTransmittersCount, lastStorageOperationNumber,
        saved_citizenCountBeforeStarvation, rpanel_textfield_userStructureID = -1;
        
    private bool showMenuWindow = false, showColonyInfo = false, showStorageInfo = false,
        localized = false, storagePositionsPrepared = false, linksReady = false, starvationSignal = false;
    public List<int> activeFastButtons { get; private set; }

    private ColonyController colony;
    private Structure chosenStructure;
    private Storage storage;
    public UIObserver workingObserver { get; private set; }
    Worksite chosenWorksite;
    ChosenObjectType selectedObjectType;
    Transform selectionFrame; Material selectionFrameMaterial;

    private const int RPANEL_CUBE_DIG_BUTTON_INDEX = 3, RPANEL_TEXTFIELD_INDEX = 7;
    private const float HAPPINESS_LOW_VALUE = 0.3f, HAPPINESS_HIGH_VALUE = 0.8f, GEARS_LOW_VALUE = 1f;

    public void Initialize( UIController uic)
    {
        uicontroller = uic;
        UIObserver.LinkToMainCanvasController(this);
        leftPanel.SetActive(false);
        upPanel.SetActive(false);
        selectionFrame = Instantiate(Resources.Load<GameObject>("Prefs/structureFrame")).transform;
        selectionFrameMaterial = selectionFrame.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        selectionFrame.gameObject.SetActive(false);        
        resourcesIcons = Resources.Load<Texture>("Textures/resourcesIcons");
        buildingsIcons = Resources.Load<Texture>("Textures/buildingIcons");
        questUI = _questUI;
        questUI.enabled = colony != null;
        if (flyingMoneyOriginalPoint == Vector3.zero) flyingMoneyOriginalPoint = moneyFlyingText.rectTransform.position;
        activeFastButtons = new List<int>();
        if (!localized) LocalizeTitles();
        if (menuPanel.activeSelf) menuPanel.SetActive(false);
    }
    public Transform GetMainCanvasTransform() 
    {
        return gameCanvas;
    }

    public void LinkColonyController()
    {
        colony = GameMaster.realMaster.colonyController;
        colony.LinkObserver(this);
        storage = colony.storage;
        
        linksReady = true;
        leftPanel.SetActive(true);
        upPanel.SetActive(true);
        colonyRenameButton.SetActive(true);
        if (!questUI.enabled) questUI.enabled = true;
    }

    void Update()
    {
        float tm = Time.deltaTime * GameMaster.gameSpeed;
        statusUpdateTimer -= tm;
        if (statusUpdateTimer <= 0)
        {
            statusUpdateEvent?.Invoke();
            statusUpdateTimer = STATUS_UPDATE_TIME;

            if (rpanel_textfield_userStructureID != -1)
            {
                if (rpanel_textfield_userStructureID == Structure.XSTATION_3_ID)
                {
                    rightPanel.transform.GetChild(RPANEL_TEXTFIELD_INDEX).GetComponent<Text>().text = XStation.GetInfo();            
                }
                else
                {
                    rightPanel.transform.GetChild(RPANEL_TEXTFIELD_INDEX).GetComponent<Text>().text = Hotel.GetInfo();
                }
            }
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
                        gearsText.color = showingGearsCf > GEARS_LOW_VALUE ? Color.white : Color.red;
                    }
                    if (showingHappinessCf != colony.happinessCoefficient)
                    {
                        showingHappinessCf = colony.happinessCoefficient;
                        happinessText.text = string.Format("{0:0.##}", showingHappinessCf * 100) + '%';
                        happinessText.color = showingHappinessCf > HAPPINESS_LOW_VALUE ? (showingHappinessCf > HAPPINESS_HIGH_VALUE ? Color.green : Color.white) : Color.red;
                    }
                    if (showingBirthrate != colony.realBirthrate)
                    {
                        showingBirthrate = colony.realBirthrate;
                        birthrateText.text = showingBirthrate > 0 ? '+' + string.Format("{0:0.#####}", showingBirthrate) : string.Format("{0:0.#####}", showingBirthrate);
                    }

                    int housingPercent = 0;
                    float hpc = colony.totalLivespace;
                    if (colony.citizenCount != 0)
                    {
                        hpc /= colony.citizenCount; hpc *= 100f;
                    }
                    else hpc = 1f;
                    if (colony.totalLivespace > 0) housingPercent = (int)hpc;
                    housingText.text = string.Format("{0:0.##}", colony.housingLevel) + " (" +  housingPercent.ToString() + "%)";

                    if (showingHospitalCf != colony.hospitals_coefficient)
                    {
                        showingHospitalCf = colony.hospitals_coefficient;
                        hospitalText.text = string.Format("{0:0.##}", showingHospitalCf * 100) + '%';
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
                                    DeactivateProgressPanel(progressPanelMode);
                                    return;
                                }
                                else
                                {
                                    Powerplant pp = uwb.observingPlace as Powerplant;
                                    RawImage ri = progressPanel.transform.GetChild(0).GetComponent<RawImage>();
                                    ri.texture = resourcesIcons;
                                    ri.uvRect = ResourceType.GetResourceIconRect(pp.GetFuelResourceID());
                                    progressPanelFullfill.fillAmount = pp.fuelLeft;
                                    progressPanelText.text = string.Format("{0:0.###}", pp.fuelLeft * 100) + '%';
                                }
                                break;
                            }
                        case ProgressPanelMode.Hangar:
                            {
                                UIHangarObserver uho = Hangar.hangarObserver;
                                if (uho == null || uho.showingStatus != Hangar.HangarStatus.ConstructingShuttle)
                                {
                                    DeactivateProgressPanel(progressPanelMode);
                                    return;
                                }
                                else
                                {
                                    float x = uho.observingHangar.workflow;
                                    progressPanelFullfill.fillAmount = x;
                                    progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                }
                                break;
                            }
                        case ProgressPanelMode.RecruitingCenter:
                            {
                                UIRecruitingCenterObserver urc = RecruitingCenter.rcenterObserver;
                                if (urc == null | !urc.isObserving | !urc.hiremode)
                                {
                                    progressPanel.SetActive(false);
                                }
                                else
                                {
                                    if (urc.observingRCenter.finding)
                                    {
                                        float x = urc.observingRCenter.workflow;
                                        progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                                        progressPanelFullfill.fillAmount = x;
                                    }
                                    else
                                    {
                                        urc.PrepareWindow();
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
                        int nhm = 0;
                        if (colony.birthrateMode == BirthrateMode.Improved) nhm = 1;
                        else
                        {
                            if (colony.birthrateMode == BirthrateMode.Lowered) nhm = 2;
                        }
                        if (nhm != hospitalPanel_savedMode)
                        {
                            var t = hospitalPanel.transform.GetChild(hospitalPanel_savedMode);
                            if (t != null) t.GetComponent<Image>().overrideSprite = null;
                            t = hospitalPanel.transform.GetChild(nhm);
                            if (t != null) t.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                            hospitalPanel_savedMode = nhm;
                        }
                    }
                }

                #region up panel values
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
                        housingNotEnoughMarker.SetActive(saved_citizenCount > saved_livespaceCount);
                    }
                    //

                    gearsProblemMarker.SetActive(colony.gears_coefficient < GEARS_LOW_VALUE && ((colony.hq?.level ?? 0) > 1 ));
                //
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
                    if (valuesChanged & powerFailureTimer == 0)
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
                    if (dataString.enabled)
                    {
                        GameMaster gm = GameMaster.realMaster;
                        string s;
                        if (gm.year > 0)
                        {
                            s = Localization.GetWord(LocalizedWord.Year_short) + ' ' + gm.year.ToString() + ' '
                                + Localization.GetWord(LocalizedWord.Month_short) + ' ' + gm.month.ToString() + ' '
                                + Localization.GetWord(LocalizedWord.Day_short) + ' ' + gm.day.ToString();
                        }
                        else
                        {
                            if (gm.month > 0)
                            {
                                s = Localization.GetWord(LocalizedWord.Month_short) + ' ' + gm.month.ToString() + ' '
                                + Localization.GetWord(LocalizedWord.Day_short) + ' ' + gm.day.ToString();
                            }
                            else
                            {
                                s = Localization.GetWord(LocalizedWord.Day) + ' ' + gm.day.ToString();
                            }
                        }
                        s = colony.cityName + ", " + s;
                        dataString.text = s;
                    }
                    //food val
                    float foodValue = storage.standartResources[ResourceType.FOOD_ID];
                    float foodMonth = colony.citizenCount * GameConstants.FOOD_CONSUMPTION * GameMaster.DAYS_IN_MONTH;
                    if (foodValue > foodMonth)
                    {
                        if (foodIconFullfill.transform.parent.gameObject.activeSelf) foodIconFullfill.transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (!foodIconFullfill.transform.parent.gameObject.activeSelf) foodIconFullfill.transform.parent.gameObject.SetActive(true);
                        if (foodValue == 0)
                        {
                            if (!starvationSignal)
                            {
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
                                foodIconFullfill.fillAmount = foodValue / foodMonth;
                            }
                        }
                    }
                    #endregion
                }
            if (moneyFlySpeed != 0) // Почему не работает?
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
        if (powerFailureTimer > 0)
        {
            powerFailureTimer -= tm;
            if (powerFailureTimer <= 0)
            {
                powerFailureTimer = 0;
                string surplus = saved_energySurplus.ToString();
                if (saved_energySurplus > 0) surplus = '+' + surplus;
                energyString.text = saved_energyCount.ToString() + " / " + saved_energyMax.ToString() + " (" + surplus + ')';
            }
        }
    }
    
    public void Raycasting()
    {
        if (GameMaster.gameSpeed == 0 | colony == null || colony.hq == null) return;
        // кастует луч, проверяет, выделен ли уже этот объект, если нет - меняет режим через ChangeChosenObject
        if (FollowingCamera.touchscreen)
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
                case Structure.BLOCKPART_COLLIDER_TAG:
                case Structure.STRUCTURE_COLLIDER_TAG:
                    {
                        Structure s;
                        if (collided.tag == Structure.BLOCKPART_COLLIDER_TAG)
                        {
                            var sp = collided.GetComponent<StructurePointer>();
                            s = sp.GetStructureLink();
                            selectedFaceIndex = sp.GetFaceIndex();
                        }
                        else s = collided.transform.parent.GetComponent<Structure>();
                        Select(s);
                        break;
                    }
                case Chunk.BLOCK_COLLIDER_TAG:
                    {
                        var crh = GameMaster.realMaster.mainChunk.GetBlock(rh.point, rh.normal);
                        Block b = crh.block;
                        selectedFaceIndex = crh.faceIndex;
                        if (b == null) ChangeChosenObject(ChosenObjectType.None);
                        else
                        {
                            Plane p;
                            if (b.TryGetPlane(selectedFaceIndex, out p))
                            {
                                Select(p);
                            }
                            else ChangeChosenObject(ChosenObjectType.None);
                        }
                        break;
                    }
                case "WorksiteSign":
                    {
                        WorksiteSign ws = collided.GetComponent<WorksiteSign>();
                        if (ws != null)
                        {
                            if (ws.worksite == chosenWorksite) return;
                            else
                            {
                                selectedPlane = null;
                                chosenStructure = null;
                                chosenWorksite = ws.worksite;
                                ChangeChosenObject(ChosenObjectType.Worksite);
                                selectedFaceIndex = Chunk.NO_FACE_VALUE;
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
        else ChangeChosenObject(ChosenObjectType.None);
    }

    public void Select(Structure s) // то же самое что рейкаст, только вызывается снаружи
    {
        if (s != null)
        {
            if (selectedObjectType == ChosenObjectType.Structure & chosenStructure == s) return;
            else
            {
                chosenStructure = s;
                selectedPlane = null;
                chosenWorksite = null;
                if (!(s is IPlanable)) selectedFaceIndex = Chunk.NO_FACE_VALUE;
                ChangeChosenObject(ChosenObjectType.Structure);
            }
        }
        else ChangeChosenObject(ChosenObjectType.None);
    }
    public void Select(Plane p)
    {
        selectedPlane = p;
        chosenStructure = null;
        chosenWorksite = null;
        ChangeChosenObject(ChosenObjectType.Plane);
    }
    public byte GetSelectedFaceIndex() //  для обсерверов структур - блоков
    {
        return selectedFaceIndex;
    }

    public void ChangeChosenObject(ChosenObjectType newChosenType)
    {
        if (hospitalPanel.activeSelf) DeactivateHospitalPanel();

        //отключение предыдущего observer
        if (workingObserver != null)
        {
            workingObserver.ShutOff();
            workingObserver = null;
        }
        bool changeFrameColor = true;
        if (newChosenType == ChosenObjectType.None)
        {
            rightPanel.SetActive(false);
            FollowingCamera.main.ResetTouchRightBorder();
            selectionFrame.gameObject.SetActive(false);
            selectedObjectType = ChosenObjectType.None;
            chosenWorksite = null;
            chosenStructure = null;
            selectedPlane = null;
            selectedFaceIndex = Chunk.NO_FACE_VALUE;
            changeFrameColor = false;
        }
        else
        {
            switch (newChosenType)
            {
                case ChosenObjectType.Structure: if (chosenStructure == null) return; else break;
                case ChosenObjectType.Plane: if (selectedPlane == null) return; else break;
                case ChosenObjectType.Worksite: if (chosenWorksite == null) return; else break;
            }
            FollowingCamera.main.SetTouchRightBorder(Screen.width - rightPanel.GetComponent<RectTransform>().rect.width);
            selectedObjectType = newChosenType;
            rightPanel.transform.SetAsLastSibling();
            rightPanel.SetActive(true);
            selectionFrame.gameObject.SetActive(true);
            if (showMenuWindow)
            {
                MenuButton();
            }
        }

        Vector3 sframeColor = Vector3.one;
        switch (selectedObjectType)
        {
            case ChosenObjectType.Plane:
                {
                    selectedFaceIndex = selectedPlane.faceIndex; 
                    selectionFrame.position = selectedPlane.GetCenterPosition();
                    selectionFrame.rotation = Quaternion.Euler(selectedPlane.GetEulerRotationForQuad());
                    selectionFrame.localScale = new Vector3(PlaneExtension.INNER_RESOLUTION, 1, PlaneExtension.INNER_RESOLUTION);
                    sframeColor = new Vector3(140f / 255f, 1, 1);

                    workingObserver = selectedPlane.ShowOnGUI();
                    FollowingCamera.main.SetLookPoint(selectedPlane.GetCenterPosition());                 
                }
                break;              

            case ChosenObjectType.Structure:
                if (!(chosenStructure is IPlanable))
                {
                    INLINE_STR_SELECT(ref sframeColor);
                }
                else
                {
                    var ip = chosenStructure as IPlanable;
                    var p = ip.FORCED_GetPlane(selectedFaceIndex);
                    if (p == null) INLINE_STR_SELECT(ref sframeColor);
                    else
                    {
                        selectionFrame.position = p.GetCenterPosition();
                        selectionFrame.rotation = Quaternion.Euler(p.GetEulerRotationForQuad());
                        selectionFrame.localScale = new Vector3(PlaneExtension.INNER_RESOLUTION, 1, PlaneExtension.INNER_RESOLUTION);
                        sframeColor = new Vector3(140f / 255f, 1f, 1f);
                        workingObserver = chosenStructure.ShowOnGUI();
                        FollowingCamera.main.SetLookPoint(selectionFrame.position);
                    }
                }
                break;

            case ChosenObjectType.Worksite:
                selectedFaceIndex = Chunk.NO_FACE_VALUE; // вспомогательная дата для chosenCube
                selectionFrame.gameObject.SetActive(false);
                changeFrameColor = false;
                workingObserver = chosenWorksite.ShowOnGUI();
                FollowingCamera.main.SetLookPoint(chosenWorksite.sign.transform.position);
                break;
        }
        if (changeFrameColor)
        {
            selectionFrameMaterial.SetColor("_TintColor", Color.HSVToRGB(sframeColor.x, sframeColor.y, sframeColor.z));
            selectionFrame.gameObject.SetActive(true);
        }
    }
    private void INLINE_STR_SELECT(ref Vector3 sframeColor)
    {
        selectedFaceIndex = Chunk.NO_FACE_VALUE;
        selectionFrame.position = chosenStructure.transform.position;
        selectionFrame.rotation = chosenStructure.transform.rotation;
        selectionFrame.localScale = new Vector3(chosenStructure.surfaceRect.size, 1, chosenStructure.surfaceRect.size);
        sframeColor = new Vector3(1f, 0f, 1f);
        workingObserver = chosenStructure.ShowOnGUI();
        FollowingCamera.main.SetLookPoint(chosenStructure.transform.position);
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
                    ExplorationPanelUI.Deactivate();
                    break;
                case ActiveWindowMode.LogWindow:
                    AnnouncementCanvasController.DeactivateLogWindow();
                    break;
                case ActiveWindowMode.GameMenu:                    
                    menuPanel.SetActive(false);
                    menuButton.GetComponent<Image>().overrideSprite = null;
                    menuButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Menu);

                    AnnouncementCanvasController.ChangeVisibility(true);
                    FollowingCamera.main.ResetTouchRightBorder();
                    if (colony != null) leftPanel.SetActive(true);
                    if (rightFastPanel.transform.childCount > 0) rightFastPanel.SetActive(true);
                    GameMaster.SetPause(false);
                    break;
            }
        }
        // activation new panel:
        currentActiveWindowMode = mode;
        if (currentActiveWindowMode == ActiveWindowMode.ExpeditionPanel)
        {
            ExplorationPanelUI.Initialize();
        }
        else
        {
            if (currentActiveWindowMode == ActiveWindowMode.GameMenu)
            {
                GameMaster.SetPause(true);
                UISurfacePanelController.current?.ShutOff();
                if (rightPanel.activeSelf) { rightPanel.SetActive(false); FollowingCamera.main.ResetTouchRightBorder(); }
                if (rightFastPanel.activeSelf) rightFastPanel.SetActive(false);
                if (leftPanel.activeSelf) leftPanel.SetActive(false);                
                //if (Plane.surfaceObserver != null) Plane.surfaceObserver.ShutOff();
                if (showColonyInfo) ColonyButton();
                AnnouncementCanvasController.ChangeVisibility(false);
                var ep = ExplorationPanelUI.current;
                if (ep != null && ep.isActiveAndEnabled) ep.gameObject.SetActive(false);
                
                menuPanel.SetActive(true);                                  
            }
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
        // заменить на отключение gameCanvas
        ChangeChosenObject(ChosenObjectType.None);
        ChangeActiveWindow(ActiveWindowMode.NoWindow);
        leftPanel.SetActive(false);
        menuPanel.SetActive(false);
        menuButton.SetActive(false);
        upPanel.SetActive(false);
        colonyRenameButton.SetActive(false);
        AnnouncementCanvasController.ChangeVisibility(false);
    }
    public void FullReactivation()
    {
        leftPanel.SetActive(true);
        menuPanel.SetActive(true);
        menuButton.SetActive(true);
        upPanel.SetActive(true);
        colonyRenameButton.SetActive(true);
        AnnouncementCanvasController.ChangeVisibility(true);
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
    public void StartPowerFailureTimer() {
        powerFailureTimer = 1;
        energyString.text = Localization.GetPhrase(LocalizedPhrase.PowerFailure);
        if (powerFailureTimer <= 0)
        {
            if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.PowerFailure);
        }
    }

    #region leftPanel
    public void ActivateQuestUI()
    {
        questUI.Activate();
        leftPanel.SetActive(false);
        storagePanel.SetActive(false);
    }
    public RectTransform SYSTEM_GetQuestButton()
    {
        return leftPanel.transform.GetChild(2).GetComponent<RectTransform>();
    }
    public RectTransform SYSTEM_GetLayerCutButton()
    {
        return layerCutToggleButton.rectTransform;
    }
    public RectTransform SYSTEM_GetCitizenString()
    {
        return citizenString.rectTransform;
    }
    public RectTransform SYSTEM_GetEnergyString()
    {
        return energyString.rectTransform;
    }
    public RectTransform SYSTEM_GetStorageButton()
    {
        return storageToggleButton.rectTransform;
    }
    public bool IsStorageUIActive()
    {
        return storagePanel.activeSelf;
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
            showingHappinessCf = colony.happinessCoefficient;
            showingBirthrate = colony.realBirthrate;
            showingHospitalCf = colony.hospitals_coefficient;
            gearsText.text = string.Format("{0:0.###}", showingGearsCf);
            gearsText.color = showingGearsCf > GEARS_LOW_VALUE ? Color.white : Color.red;
            happinessText.text = string.Format("{0:0.##}", showingHappinessCf * 100) + '%';
            birthrateText.text = showingBirthrate > 0 ? '+' + string.Format("{0:0.#####}", showingBirthrate) : string.Format("{0:0.#####}", showingBirthrate);
            happinessText.color = showingHappinessCf > HAPPINESS_LOW_VALUE ? (showingHappinessCf > HAPPINESS_HIGH_VALUE ? Color.green : Color.white) : Color.red;
            int housingPercent = 0;
            if (colony.totalLivespace > 0)
            {
                housingPercent = (int)(((float)colony.citizenCount / colony.totalLivespace) * 100f);                
            }
            housingText.text = string.Format("{0:0.##}", colony.housingLevel) + " (" + housingPercent.ToString() + "%)";
            hospitalText.text = string.Format("{0:0.##}", showingHospitalCf * 100) + '%';
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
            GameMaster.layerCutHeight = Chunk.chunkSize;
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
        if (GameMaster.layerCutHeight > Chunk.chunkSize) GameMaster.layerCutHeight = Chunk.chunkSize;
        else GameMaster.realMaster.mainChunk.LayersCut();
        layerCutToggleButton.transform.GetChild(3).GetComponent<Text>().text = GameMaster.layerCutHeight.ToString();
    }
    public void LayerCutMinus()
    {
        if (GameMaster.layerCutHeight > 0) GameMaster.layerCutHeight--;
        GameMaster.realMaster.mainChunk.LayersCut();
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
        if (st == null || st.totalVolume == 0)
        {
            //empty storage
            for (i = 0; i < storagePanelContent.childCount + 1; i++)
            {
                storagePanelContent.GetChild(i).gameObject.SetActive(false);
            }
            Transform t = storagePanelContent.GetChild(0);
            t.GetChild(0).GetComponent<RawImage>().enabled = false;
            t.GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Total) + ':';
            t.GetChild(2).GetComponent<Text>().text = "0 / " + st.maxVolume.ToString();
        }
        else
        {
            List<int> resourceIDs = new List<int>();
            float[] resources = st.standartResources;
            for (i = 0; i < resources.Length; i++)
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
                t.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(resourceID);
                t.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(resourceID);
                t.GetChild(2).GetComponent<Text>().text = ((int)(resources[resourceID] * 10) / 10f).ToString();
                t.gameObject.SetActive(true);
            }
            t = storagePanelContent.GetChild(i);
            t.GetChild(0).GetComponent<RawImage>().enabled = false;
            t.GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Total) + ':';
            t.GetChild(2).GetComponent<Text>().text = string.Format("{0:0.##}", st.totalVolume) + " / " + st.maxVolume.ToString();
            t.gameObject.SetActive(true);
            i++;
            if (i < storagePanelContent.childCount)
            {
                for (int j = i; j < storagePanelContent.childCount; j++)
                    storagePanelContent.GetChild(j).gameObject.SetActive(false);
            }
            //storagePanel.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>().size = 
        }
        lastStorageOperationNumber = st.operationsDone;
    }
    #endregion

    #region right panel
    public void CloseButton()
    {
        ChangeChosenObject(ChosenObjectType.None);
    }
    public void SelectedObjectLost(ChosenObjectType cot)
    {
        if (selectedObjectType == ChosenObjectType.None) return;
        if (cot == selectedObjectType) ChangeChosenObject(ChosenObjectType.None);
    }
    public void EnableTextfield(int id)
    {
        rpanel_textfield_userStructureID = id;
        rightPanel.transform.GetChild(RPANEL_TEXTFIELD_INDEX).gameObject.SetActive(true);
    }
    public void DisableTextfield(int id)
    {
        if (id == rpanel_textfield_userStructureID)
        {
            rightPanel.transform.GetChild(RPANEL_TEXTFIELD_INDEX).gameObject.SetActive(false);
            rpanel_textfield_userStructureID = -1;
        }
    }

    public void AddFastButton(Structure s)
    {        
        GameObject buttonGO = null;
        if (activeFastButtons.Count == 0)
        {
            buttonGO = rightFastPanel.transform.GetChild(0).gameObject;
            buttonGO.SetActive(true);
        }
        else
        {
            if (activeFastButtons.Contains(s.ID)) return;
            buttonGO = Instantiate(rightFastPanel.transform.GetChild(0).gameObject, rightFastPanel.transform);
            int i = activeFastButtons.Count;
            RectTransform rt = buttonGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.9f - 0.1f * i);
            rt.anchorMax = new Vector2(1f, 1f - 0.1f * i);
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
        }
        Button b = buttonGO.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        switch (s.ID)
        {
            case Structure.SCIENCE_LAB_ID:
                b.onClick.AddListener(() => { uicontroller.ChangeUIMode(UIMode.KnowledgeTab, true); });
                break;
            case Structure.OBSERVATORY_ID:
                b.onClick.AddListener(() => { uicontroller.ChangeUIMode(UIMode.GlobalMap,true); });
                break;
            case Structure.EXPEDITION_CORPUS_4_ID:
                b.onClick.AddListener( () => { ExplorationPanelUI.Initialize(); } );
                break;
            default:
                b.onClick.AddListener(() => { Select(s); });
                break;
        }
        buttonGO.transform.GetChild(0).GetComponent<RawImage>().uvRect = Structure.GetTextureRect(s.ID);
        activeFastButtons.Add(s.ID);
    }
    public void RemoveFastButton(Structure s)
    {
        int i = 0;
        bool found = false;
        Transform t = rightFastPanel.transform;
        while (i < activeFastButtons.Count)
        {
            if (activeFastButtons[i] == s.ID)
            {
                if (t.childCount > 1) Destroy(t.GetChild(i).gameObject);
                else t.GetChild(i).gameObject.SetActive(false);
                activeFastButtons.RemoveAt(i);
                found = true;
            }
            else
            {
                if (found)
                {
                    RectTransform rt = t.GetChild(i) as RectTransform;
                    rt.position += Vector3.up * rt.rect.height;
                    i++;
                }
            }
        }
    }
    #endregion

   public void MenuButton()
    {
        if (menuPanel.activeSelf) // SWITCH OFF
        {
            menuPanel.SetActive(false);
            ChangeActiveWindow(ActiveWindowMode.NoWindow);
            uicontroller.ReactivateSpecialCanvas();
            menuButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Menu);
        }
        else // SWITCH ON
        {
            if (GameMaster.realMaster == null)
            {
                var g = new GameObject();
                g.AddComponent<GameMaster>();
                AnnouncementCanvasController.MakeImportantAnnounce("GameMaster reloaded");
            }
            ChangeActiveWindow(ActiveWindowMode.GameMenu);
            uicontroller.DisableSpecialCanvas();
            menuButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetAnnouncementString(GameAnnouncements.GamePaused);
            menuPanel.SetActive(true);
        }
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
                        DeactivateProgressPanel(progressPanelMode);
                        return;
                    }
                    else
                    {
                        Powerplant pp = uwb.observingPlace as Powerplant;
                        progressPanelIcon.texture = resourcesIcons;
                        int resourceID = pp.GetFuelResourceID();
                        progressPanelIcon.uvRect = ResourceType.GetResourceIconRect(resourceID);
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
                    if (uho == null || !uho.isObserving || uho.showingStatus != Hangar.HangarStatus.ConstructingShuttle) { DeactivateProgressPanel(progressPanelMode); return; }
                    else
                    {
                        progressPanelIcon.transform.GetChild(0).gameObject.SetActive(false);
                        progressPanelIcon.texture = UIController.iconsTexture;

                        progressPanelIcon.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                        float x = uho.observingHangar.workflow;
                        progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                        progressPanelFullfill.fillAmount = x;
                        progressPanelFullfill.color = PoolMaster.gameOrangeColor;

                    }
                    break;
                }
            case ProgressPanelMode.RecruitingCenter:
                {
                    UIRecruitingCenterObserver urc = RecruitingCenter.rcenterObserver;
                    if (urc == null || ( !urc.isObserving  | !urc.hiremode)) { DeactivateProgressPanel(progressPanelMode); return; }
                    else
                    {
                        progressPanelIcon.transform.GetChild(0).gameObject.SetActive(false);
                        progressPanelIcon.texture = UIController.iconsTexture;
                        progressPanelIcon.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                        float x = urc.observingRCenter.workflow;
                        progressPanelText.text = ((int)(x * 100)).ToString() + '%';
                        progressPanelFullfill.fillAmount = x;
                        progressPanelFullfill.color = PoolMaster.gameOrangeColor;
                    }
                    break;
                }
        }
        progressPanel.SetActive(true);
        progressPanelMode = mode;
    }
    public void DeactivateProgressPanel(ProgressPanelMode mode)
    {
        if (mode == progressPanelMode)
        {
            progressPanel.SetActive(false);
            progressPanelMode = ProgressPanelMode.Offline;
        }
    }
    public void ProgressPanelStopButton()
    {
        if (progressPanelMode == ProgressPanelMode.RecruitingCenter)
        {
            UIRecruitingCenterObserver urc = RecruitingCenter.rcenterObserver;
            if (urc != null && urc.isObserving && urc.hiremode) urc.observingRCenter.StopHiring();
        }
        else
        {
            if (progressPanelMode == ProgressPanelMode.Hangar)
            {
                var uho = Hangar.hangarObserver;
                if (uho != null && uho.isObserving) uho.observingHangar.StopConstruction();
            }
        }
    }

    public void ActivateHospitalPanel()
    {
        Transform t = null;
        int x = 0;
        if (colony.birthrateMode == BirthrateMode.Improved) x = 1;
        else
        {
            if (colony.birthrateMode == BirthrateMode.Lowered) x = 2;
        }
        
        if (x != hospitalPanel_savedMode)
        {
            t = hospitalPanel.transform.GetChild(hospitalPanel_savedMode);
            if (t != null) t.GetComponent<Image>().overrideSprite = null;
        }
        hospitalPanel_savedMode = x;
        t = hospitalPanel.transform.GetChild(hospitalPanel_savedMode);
        if (t != null) t.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
        hospitalPanel.SetActive(true);
    }
    public void DeactivateHospitalPanel()
    {
        hospitalPanel.SetActive(false);
    }
    public void Hospital_SetBirthrateMode(int i)
    {
        switch (i)
        {
            case 0: 
                colony.SetBirthrateMode(BirthrateMode.Normal);
                break;
            case 1:
                colony.SetBirthrateMode(BirthrateMode.Improved);
                break;
            case 2:
                colony.SetBirthrateMode(BirthrateMode.Lowered);
                break;
        }
        Transform t = null;
        if (i != hospitalPanel_savedMode)
        {            
            t = hospitalPanel.transform.GetChild(hospitalPanel_savedMode);
            if (t != null) t.GetComponent<Image>().overrideSprite = null;                     
        }
        hospitalPanel_savedMode = i;
        t = hospitalPanel.transform.GetChild(hospitalPanel_savedMode);
        if (t != null) t.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
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

    #region up panel
    public void ActivateColonyNameField()
    {
        dataString.enabled = false;
        colonyRenameButton.SetActive(false);
        colonyNameField.text = colony.cityName;
        colonyNameField.Select();
        colonyNameField.gameObject.SetActive(true);
    }
    public void ColonyNameEditingEnd(string s)
    {
        colonyRenameButton.SetActive(true);
        colonyNameField.gameObject.SetActive(false);
        if (s != colony.cityName) colony.RenameColony(s);
        dataString.enabled = true;
    }
    #endregion

    public void LocalizeTitles()
    {
        Transform t = hospitalPanel.transform;
        t.GetChild(3).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.BirthrateMode) + " :";
        t.GetChild(0).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Normal);
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Improved) + " (" + string.Format("{0:0.##}", ColonyController.IMPROVED_BIRTHRATE_COEFFICIENT * 100f) + "%)";
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Lowered) + " (" + string.Format("{0:0.##}", ColonyController.LOWERED_BIRTHRATE_COEFFICIENT * 100f) + "%)";

        menuButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Menu);        

        t = rightPanel.transform;
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dig);
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.PourIn);
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 2).GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.MakeSurface);
        t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 3).GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.AddPlatform);

        localized = true;
        Localization.AddToLocalizeList(this);
    }

    private void OnDestroy()
    {
        Localization.RemoveFromLocalizeList(this);
    }

}
