using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChosenObjectType{None,Surface, Cube, Structure, Worksite}
public enum Icons {  GreenArrow, PowerOff, PowerOn, RedArrow, CrewBadIcon, CrewNormalIcon, CrewGoodIcon, ShuttleBadIcon, ShuttleNormalIcon, ShuttleGoodIcon  }
public enum ProgressPanelMode { Offline, Powerplant, Hangar}

sealed public class UIController : MonoBehaviour {
	public RectTransform questPanel; // fill in Inspector
	public RectTransform[] questButtons; // fill in Inspector
	public GameObject returnToQuestList_button; // fill in Inspector
	public GameObject rightPanel, upPanel, menuPanel, menuButton; // fill in the Inspector
	public Button touchZone, closePanelButton; // fill in the Inspector

    [SerializeField] GameObject colonyPanel, tradePanel, hospitalPanel, expeditionCorpusPanel, rollingShopPanel, progressPanel, storagePanel; // fiti
    [SerializeField] Text gearsText, happinessText, birthrateText, hospitalText, healthText, citizenString, energyString, energyCrystalsString;
    [SerializeField] Text[] announcementStrings;
    [SerializeField] Image colonyToggleButton, storageToggleButton;
    public Sprite overridingSprite;
    [SerializeField] Transform storagePanelContent;
    public Texture iconsTexture { get; private set; }
    public Texture resourcesTexture { get; private set; }
    float showingGearsCf, showingHappinessCf, showingBirthrate, showingHospitalCf, showingHealthCf;
    float updateTimer, announcementTimer;
    const float DATA_UPDATE_TIME = 2, ANNOUNCEMENT_TIME = 2;

	bool transformingRectInProgress = false, showMenuWindow = false, showColonyInfo = false, showStorageInfo = false;
	float rectTransformingSpeed = 0.8f, transformingProgress;
	RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
	int openedQuest = -1;

	float saved_energySurplus;
	int saved_citizenCount, saved_freeWorkersCount, saved_livespaceCount,saved_energyCount, saved_energyMax, saved_energyCrystalsCount,
        hospitalPanel_savedMode, exCorpus_savedCrewsCount, exCorpus_savedShuttlesCount, exCorpus_savedTransmittersCount, lastStorageOperationNumber;
    ProgressPanelMode progressPanelMode;

	public SurfaceBlock chosenSurface{get;private set;}
	CubeBlock chosenCube; byte faceIndex = 10;
	Structure chosenStructure; 
	UIObserver workingObserver;
	Worksite chosenWorksite;
	ChosenObjectType chosenObjectType;
	Transform selectionFrame; Material selectionFrameMaterial;

	public static UIController current;

	void Awake() {
		current = this;
        LocalizeButtonTitles();
		selectionFrame = Instantiate(Resources.Load<GameObject>("Prefs/structureFrame")).transform;
		selectionFrameMaterial = selectionFrame.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
		selectionFrame.gameObject.SetActive(false);
        iconsTexture = Resources.Load<Texture>("Textures/Icons");
        resourcesTexture = Resources.Load<Texture>("Textures/ResourcesIcons");
    }

    void Update() {
        if (transformingRectInProgress) {
            transformingProgress = Mathf.MoveTowards(transformingProgress, 1, rectTransformingSpeed * Time.deltaTime);
            transformingRect.anchorMin = Vector2.Lerp(transformingRect.anchorMin, resultingAnchorMin, transformingProgress);
            transformingRect.anchorMax = Vector2.Lerp(transformingRect.anchorMax, resultingAnchorMax, transformingProgress);
            if (transformingProgress == 1) {
                transformingProgress = 0;
                transformingRectInProgress = false;
            }
        }
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0) {
            updateTimer = DATA_UPDATE_TIME;
            ColonyController colony = GameMaster.colonyController;
            if (showColonyInfo) {
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
                    if (showingBirthrate != colony.birthrateCoefficient)
                    {
                        showingBirthrate = colony.birthrateCoefficient;
                        birthrateText.text = showingBirthrate > 0 ? '+' + showingBirthrate.ToString() : showingBirthrate.ToString();
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

            if (progressPanel.activeSelf) {
                switch (progressPanelMode)
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

	#region up panel
    public void ColonyButton()
    {
        showColonyInfo = !showColonyInfo;
        if (showColonyInfo) {
            if (showStorageInfo) StorageButton();
            colonyPanel.SetActive(true);
            colonyToggleButton.overrideSprite = overridingSprite;
            ColonyController colony = GameMaster.colonyController;
            if (colony == null) return;
            showingGearsCf = colony.gears_coefficient;
            showingHappinessCf = colony.happiness_coefficient;
            showingBirthrate = colony.birthrateCoefficient;
            showingHospitalCf = colony.hospitals_coefficient;
            showingHealthCf = colony.health_coefficient;
            gearsText.text = string.Format("{0:0.###}", showingGearsCf);
            happinessText.text = string.Format("{0:0.##}", showingHappinessCf * 100) + '%';
            birthrateText.text = showingBirthrate > 0 ? '+' + showingBirthrate.ToString() : showingBirthrate.ToString();
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
            storageToggleButton.overrideSprite = overridingSprite;
            storagePanel.SetActive(true);
            RecalculateStoragePanel();
        }
        else
        {
            storageToggleButton.overrideSprite = null;
            storagePanel.SetActive(false);
        }
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
                t.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetTextureRect(i);
                t.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(i);
                t.GetChild(2).GetComponent<Text>().text = ((int)(resources[i] * 10) / 10f).ToString(); // why not format? I think no need
            }
            i++;
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
            for (; b< buttonsCount; b++)
            {
                storagePanelContent.GetChild(b).gameObject.SetActive(false);
            }
        }
    } 

    public void MenuButton() {
		showMenuWindow = !showMenuWindow;
		if (showMenuWindow) {
			if (rightPanel.activeSelf) rightPanel.SetActive(false);
            if (SurfaceBlock.surfaceObserver != null) SurfaceBlock.surfaceObserver.ShutOff();
            if (showColonyInfo) ColonyButton();
            if (showStorageInfo) StorageButton();
			menuPanel.SetActive(true);
            //menuButton.transform.SetAsLastSibling();
		}
		else {
			menuPanel.SetActive(false);
		}
	}
	public void SaveButton() {
         bool success = GameMaster.realMaster.SaveGame("newsave");
        MakeAnnouncement(Localization.GetAnnouncementString(success ? GameAnnouncements.GameSaved : GameAnnouncements.SavingFailed));
    }
	public void LoadButton(){
        bool success = GameMaster.realMaster.LoadGame("newsave");
        MakeAnnouncement(Localization.GetAnnouncementString(success ? GameAnnouncements.GameLoaded : GameAnnouncements.LoadingFailed));
    }
	#endregion

	public void Raycasting() {
		Vector2 mpos = Input.mousePosition;
		RaycastHit rh;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rh)) {
			GameObject collided = rh.collider.gameObject;
			switch (collided.tag) {
			case "Structure":
				chosenStructure = collided.GetComponent<Structure>();
				chosenCube = null;
				chosenSurface = null;
				chosenWorksite = null;
				if (chosenStructure != null) ChangeChosenObject( ChosenObjectType.Structure ); 
				else ChangeChosenObject( ChosenObjectType.None );
				break;
			case "BlockCollider":
				Block b = collided.transform.parent.gameObject.GetComponent<Block>();
				switch (b.type) {
				case BlockType.Cave:
				case BlockType.Surface:
					chosenSurface = b as SurfaceBlock;
					chosenCube = null;
					if (chosenSurface != null) ChangeChosenObject( ChosenObjectType.Surface); else ChangeChosenObject( ChosenObjectType.None);
					break;
				case BlockType.Cube:
					chosenCube = b as CubeBlock;
					chosenSurface = null;
					if (chosenCube != null) {
						faceIndex = 10;
						for (byte i =0; i< 6; i++) {
							if (chosenCube.faces[i] == null) continue;
							if (chosenCube.faces[i].GetComponent<Collider>() == rh.collider ) {faceIndex = i;break;}
						}
						if (faceIndex  < 6) ChangeChosenObject( ChosenObjectType.Cube );
					}
					else ChangeChosenObject(ChosenObjectType.None);
					break;
				}
				chosenStructure = null;
				chosenWorksite = null;;
				break;
			case "WorksiteSign":
				WorksiteSign ws = collided.GetComponent<WorksiteSign>();
				if (ws != null)	chosenWorksite =  ws.worksite; else chosenWorksite = null;
				chosenStructure = null;
				chosenSurface = null;
				chosenCube = null;
				if (chosenWorksite != null) ChangeChosenObject( ChosenObjectType.Worksite ); else ChangeChosenObject( ChosenObjectType.None );
				break;
			}
		}
		else SelectedObjectLost();
	}

    public void ShowWorksite(Worksite ws)
    {
        chosenWorksite = ws;
        ChangeChosenObject(ChosenObjectType.Worksite);
    }

	public void ChangeChosenObject(ChosenObjectType newChosenType ) {
		//отключение предыдущего observer
		if (workingObserver != null) workingObserver.ShutOff();

        if (hospitalPanel.activeSelf) DeactivateHospitalPanel();
        else
        {
            if (expeditionCorpusPanel.activeSelf) DeactivateExpeditionCorpusPanel();
            else
            {
                if (rollingShopPanel.activeSelf) DeactivateRollingShopPanel();
            }
        }
        if (newChosenType == ChosenObjectType.None) {          
            rightPanel.SetActive(false);
			selectionFrame.gameObject.SetActive(false);
			chosenObjectType = ChosenObjectType.None;
		}
		else {
			chosenObjectType = newChosenType;
			rightPanel.transform.SetAsLastSibling();
			rightPanel.SetActive(true);

			selectionFrame.gameObject.SetActive(true);
			if (showMenuWindow) {
				menuPanel.SetActive(false);
				showMenuWindow = false;
			}
		}

		Vector3 sframeColor = Vector3.one;
		switch (chosenObjectType) {
		case ChosenObjectType.None:
			faceIndex = 10;
			break;
		case ChosenObjectType.Surface:
			faceIndex = 10;
			selectionFrame.position = chosenSurface.transform.position + Vector3.down * Block.QUAD_SIZE/2f;
			selectionFrame.rotation = Quaternion.identity;
			selectionFrame.localScale = new Vector3(SurfaceBlock.INNER_RESOLUTION, 1, SurfaceBlock.INNER_RESOLUTION);
			sframeColor = new Vector3(140f/255f, 1,1);
			selectionFrame.gameObject.SetActive(true);
			workingObserver = chosenSurface.ShowOnGUI();
			FollowingCamera.main.SetLookPoint(chosenSurface.transform.position);
			break;

		case ChosenObjectType.Cube:
			selectionFrame.position = chosenCube.faces[faceIndex].transform.position;
			switch (faceIndex) {
				case 0: selectionFrame.transform.rotation = Quaternion.Euler(90,0,0);break;
				case 1: selectionFrame.transform.rotation = Quaternion.Euler(0,0,-90);break;
				case 2: selectionFrame.transform.rotation = Quaternion.Euler(-90,0,0);break;
				case 3: selectionFrame.transform.rotation = Quaternion.Euler(0,0,90);break;
				case 4: selectionFrame.transform.rotation = Quaternion.identity;break;
				case 5: selectionFrame.transform.rotation = Quaternion.Euler(-180,0,0);break;
			}
			selectionFrame.localScale = new Vector3(SurfaceBlock.INNER_RESOLUTION, 1, SurfaceBlock.INNER_RESOLUTION);
			sframeColor = new Vector3(140f/255f, 1,0.9f);
			FollowingCamera.main.SetLookPoint(chosenCube.transform.position);
			break;

		case ChosenObjectType.Structure:
			faceIndex = 10;
			selectionFrame.position = chosenStructure.transform.position;
			selectionFrame.rotation = chosenStructure.transform.rotation;
			selectionFrame.localScale = new Vector3(chosenStructure.innerPosition.x_size, 1, chosenStructure.innerPosition.z_size);
			sframeColor = new Vector3(1,0,1);
                workingObserver = chosenStructure.ShowOnGUI();
			FollowingCamera.main.SetLookPoint(chosenStructure.transform.position);
			break;

            case ChosenObjectType.Worksite:
                faceIndex = 10;
                selectionFrame.gameObject.SetActive(false);
                workingObserver = chosenWorksite.ShowOnGUI();
                FollowingCamera.main.SetLookPoint(chosenWorksite.transform.position);
                break;
		}

		selectionFrameMaterial.SetColor("_TintColor", Color.HSVToRGB(sframeColor.x, sframeColor.y, sframeColor.z));
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
            RollingShop rs = wbo.observingWorkbuilding.GetComponent<RollingShop>();
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
            RollingShop rs = wbo.observingWorkbuilding.GetComponent<RollingShop>();
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
            case Icons.PowerOff: return new Rect(2 * p, 7 *p, p,p);
            case Icons.PowerOn: return new Rect(3 * p, 7 * p, p, p);
            case Icons.RedArrow: return new Rect(2 * p, 6 *p,p,p);
            case Icons.CrewBadIcon: return new Rect(p, 5 *p,p,p);
            case Icons.CrewNormalIcon: return new Rect(2 * p, 5 * p, p, p);
            case Icons.CrewGoodIcon: return new Rect(3 * p, 5 *p, p,p);
            case Icons.ShuttleBadIcon: return new Rect(4 *p, 5 *p,p,p);
            case Icons.ShuttleNormalIcon: return new Rect(5 * p, 5 * p, p, p);
            case Icons.ShuttleGoodIcon: return new Rect(6 * p, 5 * p, p, p);
        }
    }

    public void MakeAnnouncement(string s)
    {

    }

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
    }

    #region right panel
    public void SelectedObjectLost() {
		if (chosenObjectType == ChosenObjectType.None) return;
		ChangeChosenObject(ChosenObjectType.None);
	}
	#endregion

	#region quest window
	public void OpenQuestWindow() {
		questPanel.gameObject.SetActive(true);
	}
    public void CloseQuestWindow()
    {
        questPanel.gameObject.SetActive(false);
    }

	public void QuestButton_OpenQuest(int index) {
		transformingRect = questButtons[index];
		transformingRectInProgress = true;
		transformingProgress = 0;
		resultingAnchorMin = Vector2.zero;
		resultingAnchorMax= Vector2.one;
		for (int i = 0; i < questButtons.Length; i++) {
			if (i == index) continue;
			else {
				questButtons[i].gameObject.SetActive(false);
			}
		}
		openedQuest = index;
		returnToQuestList_button.SetActive(true);
	}

	public void QuestButton_ReturnToQuestList() {
		if (openedQuest == - 1) return;
		QuestButton_RestoreButtonRect(openedQuest);
		transformingRectInProgress = true;
		transformingProgress = 0;
		for (int i = 0; i < questButtons.Length; i++) {
			if (i == openedQuest) continue;
			else {
				questButtons[i].gameObject.SetActive(true);
			}
		}
	}

	void QuestButton_RestoreButtonRect(int i) {
		switch (i) {
		case 0:
			resultingAnchorMin = new Vector2(0, 0.66f);
			resultingAnchorMax = new Vector2(0.33f, 1);
			break;
		case 1:
			resultingAnchorMin = new Vector2(0.33f, 0.66f);
			resultingAnchorMax = new Vector2(0.66f, 1);
			break;
		case 2:
			resultingAnchorMin = new Vector2(0.66f, 0.66f);
			resultingAnchorMax = new Vector2(1, 1);
			break;
		case 3:
			resultingAnchorMin = new Vector2(0, 0.33f);
			resultingAnchorMax = new Vector2(0.33f, 0.66f);
			break;
		case 4:
			resultingAnchorMin = new Vector2(0.33f, 0.33f);
			resultingAnchorMax= new Vector2(0.66f, 0.66f);
			break;
		case 5:
			resultingAnchorMin = new Vector2(0.66f, 0.33f);
			resultingAnchorMax = new Vector2(1, 0.66f);
			break;
		case 6:
			resultingAnchorMin = new Vector2(0, 0);
			resultingAnchorMax = new Vector2(0.33f, 0.33f);
			break;
		case 7:
			resultingAnchorMin = new Vector2(0.33f, 0);
			resultingAnchorMax = new Vector2(0.66f, 0.33f);
			break;
		case 8:
			resultingAnchorMin = new Vector2(0.66f, 0);
			resultingAnchorMax = new Vector2(1, 0.33f);
			break;
		}
	}
	#endregion
}
