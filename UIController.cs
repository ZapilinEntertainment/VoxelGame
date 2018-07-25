using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChosenObjectType{None,Surface, Cube, Structure, Worksite}

sealed public class UIController : MonoBehaviour {
	public RectTransform questPanel; // fill in Inspector
	public RectTransform[] questButtons; // fill in Inspector
	public GameObject returnToQuestList_button; // fill in Inspector
	public GameObject rightPanel, upPanel, menuPanel, menuButton; // fill in the Inspector
	public Button touchZone, closePanelButton; // fill in the Inspector

    [SerializeField] GameObject colonyPanel, tradePanel, hospitalPanel, expeditionCorpusPanel; // fiti
    [SerializeField] Text gearsText, happinessText, birthrateText, hospitalText, healthText;
    float showingGearsCf, showingHappinessCf, showingBirthrate, showingHospitalCf, showingHealthCf;
    float updateTimer;
    const float DATA_UPDATE_TIME = 2;


	byte submode = 0;
	bool transformingRectInProgress = false, showMenuWindow = false, showColonyInfo = false;
	float rectTransformingSpeed = 0.8f, transformingProgress;
	RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
	int openedQuest = -1;

	float coinsCount, energyCount, energyMax;
	int citizenCount, freeWorkersCount, livespaceCount;
    int hospitalPanel_savedMode, exCorpus_savedCrewsCount, exCorpus_savedShuttlesCount, exCorpus_savedTransmittersCount;

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
            if (showColonyInfo) {
                ColonyController colony = GameMaster.colonyController;
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

	#region up panel
    public void ColonyButton()
    {
        showColonyInfo = !showColonyInfo;
        if (showColonyInfo) {
            if (showMenuWindow) MenuButton();
            colonyPanel.SetActive(true);
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
        }
    }

	public void MenuButton() {
		showMenuWindow = !showMenuWindow;
		if (showMenuWindow) {
			if (rightPanel.activeSelf) rightPanel.SetActive(false);            
            if (showColonyInfo) ColonyButton();
			menuPanel.SetActive(true);
		}
		else {
			if (chosenObjectType != ChosenObjectType.None) {
				rightPanel.SetActive(true);
			}
			menuPanel.SetActive(false);
		}
	}
	public void SaveButton() {
         bool success = GameMaster.realMaster.SaveGame("newsave");
        GameMaster.realMaster.AddAnnouncement(Localization.GetAnnouncementString(success ? GameAnnouncements.GameSaved : GameAnnouncements.SavingFailed));
    }
	public void LoadButton(){
        bool success = GameMaster.realMaster.LoadGame("newsave");
        GameMaster.realMaster.AddAnnouncement(Localization.GetAnnouncementString(success ? GameAnnouncements.GameLoaded : GameAnnouncements.LoadingFailed));
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

	public void ChangeChosenObject(ChosenObjectType newChosenType ) {
		//отключение предыдущего observer
		if (workingObserver != null) workingObserver.ShutOff();

		if (newChosenType == ChosenObjectType.None) {
            if (hospitalPanel.activeSelf) DeactivateHospitalPanel();
            else {
                if (expeditionCorpusPanel.activeSelf) DeactivateExpeditionCorpusPanel();
            }
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
                if (hospitalPanel.activeSelf) DeactivateHospitalPanel();
                else
                {
                    if (expeditionCorpusPanel.activeSelf) DeactivateExpeditionCorpusPanel();
                }
                workingObserver = chosenStructure.ShowOnGUI();
			FollowingCamera.main.SetLookPoint(chosenStructure.transform.position);
			break;
		}

		selectionFrameMaterial.SetColor("_TintColor", Color.HSVToRGB(sframeColor.x, sframeColor.y, sframeColor.z));
	}

    #region auxiliary panels
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

    public void LocalizeButtonTitles()
    {
        hospitalPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase (LocalizedPhrase.BirthrateMode ) + " :";
        hospitalPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Normal);
        hospitalPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Improved) + " (" + string.Format("{0:0.##}", Hospital.improvedCoefficient) + "%)";
        hospitalPanel.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Lowered) + " (" + string.Format("{0:0.##}", Hospital.loweredCoefficient) + "%)";
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
