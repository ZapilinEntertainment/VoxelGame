using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChosenObjectType{None,Surface, Cube, Structure, Worksite}
public enum Icons {  GreenArrow, GuidingStar, PowerOff, PowerOn, Citizen, RedArrow, CrewBadIcon, CrewNormalIcon, CrewGoodIcon, ShuttleBadIcon, ShuttleNormalIcon, ShuttleGoodIcon, TaskFrame, TaskCompleted  }
public enum ProgressPanelMode { Offline, Powerplant, Hangar}


sealed public class UIController : MonoBehaviour {	
	public GameObject rightPanel, upPanel, menuPanel, menuButton; // fill in the Inspector
	public Button closePanelButton; // fill in the Inspector

    [SerializeField] GameObject colonyPanel, tradePanel, hospitalPanel, expeditionCorpusPanel, rollingShopPanel, progressPanel, storagePanel, optionsPanel, leftPanel; // fiti
    [SerializeField] Text gearsText, happinessText, birthrateText, hospitalText, healthText, citizenString, energyString, energyCrystalsString, moneyFlyingText;
    [SerializeField] Text[] announcementStrings;
    [SerializeField] Image colonyToggleButton, storageToggleButton, layerCutToggleButton;
    [SerializeField] Transform storagePanelContent;
    public Texture iconsTexture { get; private set; }
    public Texture resourcesTexture { get; private set; }
    public Texture buildingsTexture { get; private set; }
    float showingGearsCf, showingHappinessCf, showingBirthrate, showingHospitalCf, showingHealthCf;
    float updateTimer, moneyFlySpeed = 0;
    Vector3 flyingMoneyOriginalPoint = Vector3.zero;

    enum MenuSection { NoSelection, Save, Load, Options }
    MenuSection selectedMenuSection = MenuSection.NoSelection;

    const float DATA_UPDATE_TIME = 1, DISSAPPEAR_SPEED = 0.3f;

    bool showMenuWindow = false, showColonyInfo = false, showStorageInfo = false, showLayerCut = false, activeAnnouncements = false, localized = false;
    public int interceptingConstructPlaneID = -1;

	float saved_energySurplus;
    int saved_citizenCount, saved_freeWorkersCount, saved_livespaceCount, saved_energyCount, saved_energyMax, saved_energyCrystalsCount,
        hospitalPanel_savedMode, exCorpus_savedCrewsCount, exCorpus_savedShuttlesCount, exCorpus_savedTransmittersCount, lastStorageOperationNumber;
    ProgressPanelMode progressPanelMode;

	public SurfaceBlock chosenSurface{get;private set;}
    [SerializeField] QuestUI _questUI;
    public QuestUI questUI { get; private set; }

	CubeBlock chosenCube; byte faceIndex = 10;
	Structure chosenStructure; 
	UIObserver workingObserver;
	Worksite chosenWorksite;
	ChosenObjectType chosenObjectType;
	Transform selectionFrame; Material selectionFrameMaterial;

	public static UIController current;

    const int MENUPANEL_SAVE_BUTTON_INDEX = 0, MENUPANEL_LOAD_BUTTON_INDEX = 1, MENUPANEL_OPTIONS_BUTTON_INDEX = 2, RPANEL_CUBE_DIG_BUTTON_INDEX = 5;

	void Awake() {
		current = this;
        LocalizeButtonTitles();
		selectionFrame = Instantiate(Resources.Load<GameObject>("Prefs/structureFrame")).transform;
		selectionFrameMaterial = selectionFrame.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
		selectionFrame.gameObject.SetActive(false);
        iconsTexture = Resources.Load<Texture>("Textures/Icons");
        resourcesTexture = Resources.Load<Texture>("Textures/resourcesIcons");
        buildingsTexture = Resources.Load<Texture>("Textures/buildingIcons");
        questUI = _questUI;
        if (flyingMoneyOriginalPoint == Vector3.zero) flyingMoneyOriginalPoint = moneyFlyingText.rectTransform.position;

        SaveSystemUI.Check(transform.root);

        if (!localized) LocalizeButtonTitles();
    }

    void Update() {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0)
        {
            updateTimer = DATA_UPDATE_TIME;
            ColonyController colony = GameMaster.colonyController;
            if (showColonyInfo)
            {
                if (colony != null)
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
                        showingBirthrate = (int)(colony.realBirthrate * 100) / 100f; ;
                        birthrateText.text = showingBirthrate > 0 ? '+' + string.Format("{0:0.#####}", showingBirthrate) : string.Format("{0:0.#####}", showingBirthrate);
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
            }
            else
            {
                if (showStorageInfo)
                {
                    if (lastStorageOperationNumber != colony.storage.operationsDone)
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
                        UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
                        if (uwb == null || (!uwb.gameObject.activeSelf | uwb.observingWorkbuilding.id != Structure.MINERAL_POWERPLANT_2_ID))
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
                            Transform t = progressPanel.transform.GetChild(1);
                            RectTransform rt = t.GetChild(0).GetComponent<RectTransform>();
                            rt.offsetMax = new Vector2((pp.fuelLeft - 1) * rt.rect.width, 0);
                            t.GetChild(1).GetComponent<Text>().text = string.Format("{0:0.###}", pp.fuelLeft * 100) + '%';
                        }
                        break;
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
                else
                {
                    if (expeditionCorpusPanel.activeSelf)
                    {
                        int x = Shuttle.shuttlesList.Count;
                        if (exCorpus_savedShuttlesCount != x)
                        {
                            exCorpus_savedShuttlesCount = x;
                            expeditionCorpusPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ShuttlesAvailable) + " : " + exCorpus_savedShuttlesCount.ToString();
                        }
                        x = Crew.crewsList.Count;
                        if (x != exCorpus_savedCrewsCount)
                        {
                            exCorpus_savedCrewsCount = x;
                            expeditionCorpusPanel.transform.GetChild(1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.CrewsAvailable) + " : " + exCorpus_savedCrewsCount.ToString();
                        }
                        x = QuantumTransmitter.transmittersList.Count;
                        if (x != exCorpus_savedTransmittersCount)
                        {
                            exCorpus_savedTransmittersCount = x;
                            expeditionCorpusPanel.transform.GetChild(2).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.TransmittersAvailable) + " : " + exCorpus_savedTransmittersCount.ToString();
                        }
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
            }
        }
        if (activeAnnouncements)
        {
            Text t = announcementStrings[0];
            Color c = t.color;
            c.a = Mathf.Lerp(c.a, 0, DISSAPPEAR_SPEED * Time.deltaTime * GameMaster.gameSpeed * (2 - c.a) * (2- c.a));
            if (c.a > 0.05f)  t.color = c;
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
                moneyFlySpeed += Time.deltaTime/5f;
                if (moneyFlySpeed < 1)
                {
                    moneyFlyingText.rectTransform.position = Vector3.Lerp(flyingMoneyOriginalPoint, flyingMoneyOriginalPoint + 2 * Vector3.up, moneyFlySpeed);
                    moneyFlyingText.color = Color.Lerp(new Color(1, 0, 0, 0), Color.red,  moneyFlySpeed);
                }
                else
                {
                   moneyFlySpeed = 0;
                    moneyFlyingText.enabled = false;
                }
            }
        }
    }

    #region menu
    public void MenuButton() {
		showMenuWindow = !showMenuWindow;
		if (showMenuWindow) { // on
			if (rightPanel.activeSelf) rightPanel.SetActive(false);
            if (SurfaceBlock.surfaceObserver != null) SurfaceBlock.surfaceObserver.ShutOff();
            if (showColonyInfo) ColonyButton();
            if (showStorageInfo) StorageButton();
			menuPanel.SetActive(true);
            //menuButton.transform.SetAsLastSibling();
            Time.timeScale = 0;
            MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GamePaused));
        }
		else { //off
			menuPanel.SetActive(false);
            optionsPanel.SetActive(false);
            SetMenuPanelSelection(MenuSection.NoSelection);
            menuButton.GetComponent<Image>().overrideSprite = null;
            Time.timeScale = 1;
            MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GameUnpaused));
        }        
	}
	public void SaveButton() {
        SaveSystemUI.current.Activate(true);
        SetMenuPanelSelection(MenuSection.Save);
    }
	public void LoadButton(){
        SaveSystemUI.current.Activate(false);
        SetMenuPanelSelection(MenuSection.Load);
    }
    public void OptionsButton() {
        optionsPanel.SetActive(true);
        SetMenuPanelSelection(MenuSection.Options);
    }
    public void ToMainMenu()
    {
        SetMenuPanelSelection(MenuSection.NoSelection);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);        
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
               // case MenuSection.Options:
                 //   menuPanel.transform.GetChild(MENUPANEL_OPTIONS_BUTTON_INDEX).GetComponent<Image>().overrideSprite = null;
                 //   optionsPanel.SetActive(false);
                 //   break;
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

	public void Raycasting() {
        // кастует луч, проверяет, выделен ли уже этот объект, если нет - меняет режим через ChabgeChosenObject
		Vector2 mpos = Input.mousePosition;
		RaycastHit rh;
		if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh)) {            
            GameObject collided = rh.collider.gameObject;   
            
            switch (collided.tag) {
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
			case "BlockCollider":
                    {
                        Block b = collided.transform.parent.GetComponent<Block>();
                        if (b == null) b = collided.transform.parent.parent.GetComponent<Block>(); // cave block
                        if (b == null) ChangeChosenObject(ChosenObjectType.None);
                        switch (b.type)
                        {
                            case BlockType.Cave:
                            case BlockType.Surface:
                                SurfaceBlock sb = b as SurfaceBlock;
                                if (sb == chosenSurface) return;
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
                                else {
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
            if (expeditionCorpusPanel.activeSelf) DeactivateExpeditionCorpusPanel();
            else
            {
                if (rollingShopPanel.activeSelf) DeactivateRollingShopPanel();
            }
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
                    FollowingCamera.main.SetLookPoint(chosenCube.transform.position);

                    Transform t = rightPanel.transform;
                    t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX).gameObject.SetActive(true);
                    if (chosenCube.excavatingStatus != 0) t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 1).gameObject.SetActive(true);
                    else t.GetChild(RPANEL_CUBE_DIG_BUTTON_INDEX + 1).gameObject.SetActive(false);
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
        }
        if (changeFrameColor)
        {
            selectionFrameMaterial.SetColor("_TintColor", Color.HSVToRGB(sframeColor.x, sframeColor.y, sframeColor.z));
            selectionFrame.gameObject.SetActive(true);
        }
    }

    public void ShowWorksite(Worksite ws)
    {
        chosenWorksite = ws;
        ChangeChosenObject(ChosenObjectType.Worksite);
    }

    

    #region auxiliary panels
    public void ActivateProgressPanel(ProgressPanelMode mode)
    {        
        switch (mode)
        {
            case ProgressPanelMode.Powerplant:
                UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
                if (uwb == null || !uwb.gameObject.activeSelf)
                {
                    DeactivateProgressPanel();
                    return;
                }
                else
                {
                    Powerplant pp = uwb.observingWorkbuilding as Powerplant;
                    Transform t = progressPanel.transform;
                    RawImage ri = t.GetChild(0).GetComponent<RawImage>();
                    ri.texture = resourcesTexture;
                    int resourceID = pp.GetFuelResourseID();
                    ri.uvRect = ResourceType.GetTextureRect(resourceID);
                    ri.transform.GetChild(0).GetComponent<Text>().text = Localization.GetResourceName(resourceID);
                    t = t.GetChild(1);
                    RectTransform rt = t.GetChild(0).GetComponent<RectTransform>();
                    rt.offsetMax = new Vector2((Mathf.Clamp(pp.fuelLeft,0,1) - 1) * rt.rect.width, 0);
                    t.GetChild(1).GetComponent<Text>().text = string.Format("{0:0.###}", pp.fuelLeft) + '%';

                    progressPanel.SetActive(true);
                    progressPanelMode = mode;
                }
                break;
        }
    }
    public void DeactivateProgressPanel()
    {
        progressPanel.SetActive(false);
        progressPanelMode = ProgressPanelMode.Offline;
    }

    public void ActivateRollingShopPanel()
    {
        UIWorkbuildingObserver wbo = WorkBuilding.workbuildingObserver;
        if (wbo != null && wbo.gameObject.activeSelf)
        { // уу, костыли!
            // и вообще надо переделать на dropdown
            RollingShop rs = wbo.observingWorkbuilding as RollingShop;
            if (rs != null)
            {
                rollingShopPanel.SetActive(true);
                rollingShopPanel.transform.GetChild(rs.GetModeIndex()).GetComponent<Toggle>().isOn = true;
            }
        }
        rollingShopPanel.SetActive(true);
    }
    public void DeactivateRollingShopPanel()
    {
        rollingShopPanel.SetActive(false);
    }
    public void RollingShop_SetActivity(int x)
    {
        UIWorkbuildingObserver wbo = WorkBuilding.workbuildingObserver;
        if (wbo == null | !wbo.gameObject.activeSelf)
        {
            DeactivateRollingShopPanel();
            return;
        }
        else { // уу, костыли!
            RollingShop rs = wbo.observingWorkbuilding as RollingShop;
            if (rs == null)
            {
                DeactivateRollingShopPanel();
                return;
            }
            else
            {
                if (rs.GetModeIndex() != x)
                {
                    rs.SetMode(x);
                    rollingShopPanel.transform.GetChild(x).GetComponent<Toggle>().isOn = true;
                }
            }
        }
    }

    public void ActivateExpeditionCorpusPanel()
    {
        expeditionCorpusPanel.SetActive(true);
        exCorpus_savedShuttlesCount = Shuttle.shuttlesList.Count;
        expeditionCorpusPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ShuttlesAvailable) + " : " + exCorpus_savedShuttlesCount.ToString();
        exCorpus_savedCrewsCount = Crew.crewsList.Count;
        expeditionCorpusPanel.transform.GetChild(1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.CrewsAvailable) + " : " + exCorpus_savedCrewsCount.ToString();
        exCorpus_savedTransmittersCount = QuantumTransmitter.transmittersList.Count;
        expeditionCorpusPanel.transform.GetChild(2).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.TransmittersAvailable) + " : " + exCorpus_savedTransmittersCount.ToString();
    }
    public void DeactivateExpeditionCorpusPanel()
    {
        expeditionCorpusPanel.SetActive(false);
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
        if (i != Hospital.GetBirthrateModeIndex())   Hospital.SetBirthrateMode(i);
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

    public static Rect GetTextureUV (Icons i)
    {
        float p = 0.125f;
        switch (i)
        {
            default: return Rect.zero;
            case Icons.GreenArrow: return new Rect(6 * p, 7 *p, p,p);
            case Icons.GuidingStar: return new Rect(7 * p, 7 * p, p, p);
            case Icons.PowerOff: return new Rect(2 * p, 7 *p, p,p);
            case Icons.PowerOn: return new Rect(3 * p, 7 * p, p, p);
            case Icons.Citizen: return new Rect(p, 6*p,p,p);
            case Icons.RedArrow: return new Rect(2 * p, 6 *p,p,p);
            case Icons.CrewBadIcon: return new Rect(p, 5 *p,p,p);
            case Icons.CrewNormalIcon: return new Rect(2 * p, 5 * p, p, p);
            case Icons.CrewGoodIcon: return new Rect(3 * p, 5 *p, p,p);
            case Icons.ShuttleBadIcon: return new Rect(4 *p, 5 *p,p,p);
            case Icons.ShuttleNormalIcon: return new Rect(5 * p, 5 * p, p, p);
            case Icons.ShuttleGoodIcon: return new Rect(6 * p, 5 * p, p, p);
            case Icons.TaskFrame: return new Rect(3 * p, 4 *p,p,p);
            case Icons.TaskCompleted: return new Rect(4 * p, 4 *p,p,p);
        }
    }

    public void MakeAnnouncement(string s)
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
            }
            announcementStrings[len - 1].text = s;
        }
        else
        {
            Text t = announcementStrings[lastIndex];
            t.enabled = true;
            t.text = s;
            t.color = Color.black;
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
            moneyFlyingText.rectTransform.position =flyingMoneyOriginalPoint - Vector3.up * 2; 
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
            ColonyController colony = GameMaster.colonyController;
            if (colony == null) return;
            showingGearsCf = colony.gears_coefficient;
            showingHappinessCf = colony.happiness_coefficient;
            showingBirthrate = colony.realBirthrate;
            showingHospitalCf = colony.hospitals_coefficient;
            showingHealthCf = colony.health_coefficient;
            gearsText.text = string.Format("{0:0.###}", showingGearsCf);
            happinessText.text = string.Format("{0:0.##}", showingHappinessCf * 100) + '%';
            birthrateText.text = showingBirthrate > 0 ? '+' + string.Format("{0:0.#####}", showingBirthrate) : string.Format("{0:0.#####}", showingBirthrate);
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
            storageToggleButton.overrideSprite = PoolMaster.gui_overridingSprite;
            storagePanel.SetActive(true);
            RecalculateStoragePanel();
        }
        else
        {
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
            if (GameMaster.layerCutHeight != p) GameMaster.mainChunk.LayersCut();
        }
        else // off
        {
            layerCutToggleButton.overrideSprite = null;
            GameMaster.prevCutHeight = GameMaster.layerCutHeight;
            GameMaster.layerCutHeight = Chunk.CHUNK_SIZE;
            GameMaster.mainChunk.LayersCut();
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
        else GameMaster.mainChunk.LayersCut();
        layerCutToggleButton.transform.GetChild(3).GetComponent<Text>().text = GameMaster.layerCutHeight.ToString();
    }
    public void LayerCutMinus()
    {
        GameMaster.layerCutHeight--;
        if (GameMaster.layerCutHeight < 0) GameMaster.layerCutHeight = 0;
        else GameMaster.mainChunk.LayersCut();
        layerCutToggleButton.transform.GetChild(3).GetComponent<Text>().text = GameMaster.layerCutHeight.ToString();
    }

    void RecalculateStoragePanel()
    {
        Storage st = GameMaster.colonyController.storage;
        float[] resources = st.standartResources;
        int i = 0, b = 0, buttonsCount = storagePanelContent.childCount;
        while (i < resources.Length)
        {
            if (resources[i] != 0)
            {
                Transform t;
                if (b < buttonsCount) t = storagePanelContent.GetChild(b);
                else
                {
                    t = Instantiate(storagePanelContent.GetChild(0), storagePanelContent);
                    RectTransform rt = (t as RectTransform);
                    t.localPosition += Vector3.down * b * rt.rect.height;
                }
                t.gameObject.SetActive(true);
                b++;
                t.GetChild(0).GetComponent<RawImage>().enabled = true;
                t.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetTextureRect(i);
                t.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(i);
                t.GetChild(2).GetComponent<Text>().text = ((int)(resources[i] * 10) / 10f).ToString(); // why not format? I think no need
            }
            i++;
        }
        // "Total" string
        {
            Transform t;
            if (b < buttonsCount) t = storagePanelContent.GetChild(b);
            else
            {
                t = Instantiate(storagePanelContent.GetChild(0), storagePanelContent);
                RectTransform rt = (t as RectTransform);
                t.localPosition += Vector3.down * b * rt.rect.height;
            }
            t.gameObject.SetActive(true);
            b++;
            t.GetChild(0).GetComponent<RawImage>().enabled = false;
            t.GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Total) + ':';
            t.GetChild(2).GetComponent<Text>().text = (((int)(st.totalVolume * 100)) / 100f).ToString() + " / " + st.maxVolume.ToString();
        }

        {
            RectTransform rt = storagePanel.transform.GetChild(0) as RectTransform;
            float listSize = b * (storagePanelContent.GetChild(0) as RectTransform).rect.height;
            float freeSpace = Screen.height - upPanel.GetComponent<RectTransform>().rect.height * 0.7f;
            if (listSize < freeSpace)
            {
                rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, listSize);
            }
            else
            {
                rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, freeSpace);
            }
            (storagePanelContent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, listSize);
            //rt.offsetMin = new Vector2(rt.offsetMin.x, rt.offsetMax.y -  b * (storagePanelContent.GetChild(0) as RectTransform).rect.height);
        }
        lastStorageOperationNumber = st.operationsDone;
        if (b < buttonsCount)
        {
            for (; b < buttonsCount; b++)
            {
                storagePanelContent.GetChild(b).gameObject.SetActive(false);
            }
        }
    }
    #endregion

    public void LocalizeButtonTitles()
    {
        Transform t = hospitalPanel.transform;
        t.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase (LocalizedPhrase.BirthrateMode ) + " :";
        t.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Normal);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Improved) + " (" + string.Format("{0:0.##}", Hospital.improvedCoefficient) + "%)";
        t.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Lowered) + " (" + string.Format("{0:0.##}", Hospital.loweredCoefficient) + "%)";

        t = rollingShopPanel.transform;
        t.GetChild(0).GetChild(1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoActivity);
        t.GetChild(1).GetChild(1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ImproveGears);

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
        localized = true;
    }

    #region right panel
    public void SelectedObjectLost() {
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
    #endregion
}
