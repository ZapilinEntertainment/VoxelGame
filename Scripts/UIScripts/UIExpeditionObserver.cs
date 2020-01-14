using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIExpeditionObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown crewDropdown;
    [SerializeField] private GameObject crewButton, closeButton, minigameButton;
    [SerializeField] private Slider suppliesSlider, crystalsSlider;
    [SerializeField] private Text suppliesStableValue, crystalsStableValue, crewStableName;
    [SerializeField] private Transform transmitterLine, shuttleLine, fuelLine;
    [SerializeField] private Button launchButton;
#pragma warning restore 0649
    private RawImage transmitterMarker { get { return transmitterLine.GetChild(1).GetComponent<RawImage>(); } }
    private Text transmitterLabel { get { return transmitterLine.GetChild(0).GetComponent<Text>(); } }
    private RawImage shuttleMarker { get { return shuttleLine.GetChild(1).GetComponent<RawImage>(); } }
    private Text shuttleLabel { get { return shuttleLine.GetChild(0).GetComponent<Text>(); } }
    private RawImage fuelMarker { get { return fuelLine.GetChild(1).GetComponent<RawImage>(); } }
    private Text fuelLabel { get { return fuelLine.GetChild(0).GetComponent<Text>(); } }
    private Text launchButtonLabel { get { return launchButton.transform.GetChild(0).GetComponent<Text>(); } }

    private Transform expNameField { get { return transform.GetChild(0); } }
    private Text expLabel { get { return expNameField.GetChild(0).GetComponent<Text>(); } }
    private GameObject expDestinationButton { get { return expNameField.GetChild(1).gameObject; } }

    private bool subscribedToUpdate = false, workOnMainCanvas = true;
    private bool? preparingMode = null;
    private byte lastChangesMarkerValue = 0;
    private int lastCrewListMarker = 0, lastShuttlesListMarker = 0;
    private ColonyController colony;
    private Expedition showingExpedition;
    private PointOfInterest selectedDestination;
    private Crew selectedCrew;
    private List<int> crewsIDs;

    private readonly Color lightcyan = new Color(0.5f, 1f, 0.95f), halfred = new Color(1f,0f,0f,0.5f);
    private const int FUEL_NEEDED = 200;

    private void Awake()
    {
        colony = GameMaster.realMaster.colonyController;
        suppliesSlider.minValue = Expedition.MIN_SUPPLIES_COUNT;
        suppliesSlider.maxValue = Expedition.MAX_SUPPLIES_COUNT;
        crystalsSlider.maxValue = Expedition.MAX_START_CRYSTALS;
    }
    public void SetPosition(Rect r, SpriteAlignment alignment, bool drawOnMainCanvas)
    {
        var rt = GetComponent<RectTransform>();
        if (workOnMainCanvas != drawOnMainCanvas)
        {
            rt.transform.parent = drawOnMainCanvas ? UIController.current.mainCanvas : GameMaster.realMaster.globalMap.observer.GetComponent<GlobalMapUI>().GetMapCanvas();
            workOnMainCanvas = drawOnMainCanvas;
            closeButton.SetActive(!workOnMainCanvas);
        }
        rt.position = r.position;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, r.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, r.height);
        Vector2 correctionVector = Vector2.zero;
        switch (alignment)
        {
            case SpriteAlignment.BottomRight: correctionVector = Vector2.left * rt.rect.width; break;
            case SpriteAlignment.RightCenter: correctionVector = new Vector2(-1f * rt.rect.width, -0.5f * rt.rect.height); break;
            case SpriteAlignment.TopRight: correctionVector = new Vector2(-1f * rt.rect.width, -1f * rt.rect.height); break;
            case SpriteAlignment.Center: correctionVector = new Vector2(-0.5f * rt.rect.width, -0.5f * rt.rect.height); break;
            case SpriteAlignment.TopCenter: correctionVector = new Vector2(-0.5f * rt.rect.width, -1f * rt.rect.height); break;
            case SpriteAlignment.BottomCenter: correctionVector = new Vector2(-0.5f * rt.rect.width, 0f); break;
            case SpriteAlignment.TopLeft: correctionVector = Vector2.down * rt.rect.height; break;
            case SpriteAlignment.LeftCenter: correctionVector = Vector2.down * rt.rect.height * 0.5f; break;
        }
        rt.anchoredPosition += correctionVector;
    }

    public void Show(Expedition e)
    {
        if (e == null) gameObject.SetActive(false);
        else
        {
            showingExpedition = e;
            RedrawWindow();
        }
    }
    public void Show(PointOfInterest poi)
    {
        if (poi == null) gameObject.SetActive(false);
        else
        {
            if (poi.workingExpedition != null)
            {
                Show(poi.workingExpedition);
                return;
            }
            else
            {
                showingExpedition = null;
                selectedCrew = null;
                selectedDestination = poi;
            }
            RedrawWindow();
        }
    }

    private void RedrawWindow()
    {
        int statedCrystalsCount = 0, statedSuppliesCount = Expedition.MIN_SUPPLIES_COUNT;
        Color redcolor = Color.red, whitecolor = Color.white;
        GameObject g;
        if (showingExpedition == null)
        {
            //подготовка новой экспедиции
            expLabel.text = Localization.GetExpeditionName(selectedDestination);         
            bool readyToStart = true;

            PrepareCrewDropdown();
            if (selectedCrew != null)
            {
                readyToStart = selectedCrew.atHome;
                crewButton.SetActive(true);
            }
            else
            {
                readyToStart = false;
                crewButton.SetActive(false);
            }     
            if (!crewDropdown.gameObject.activeSelf)
            {
                crewDropdown.gameObject.SetActive(true);
                crewStableName.enabled = false;
            }
            //supplies
            suppliesSlider.value = statedSuppliesCount;
            OnSuppliesSliderChanged(statedSuppliesCount);
            if (preparingMode != true)
            {
                suppliesSlider.gameObject.SetActive(true);
                suppliesStableValue.gameObject.SetActive(false);
            }
            //crystals
            crystalsSlider.value = statedCrystalsCount;
            OnCrystalsSliderChanged(statedCrystalsCount);
            if (preparingMode != true)
            {
                crystalsSlider.gameObject.SetActive(true);
                crystalsStableValue.gameObject.SetActive(false);
            }            
            //transmitters:
            int c = QuantumTransmitter.GetFreeTransmittersCount();
            if (c > 0)
            {
                transmitterMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.FreeTransmitters) + c.ToString();
                transmitterLabel.color = whitecolor;
            }
            else
            {
                transmitterMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.NoTransmitters);
                transmitterLabel.color = redcolor;
                readyToStart = false;
            }
            //shuttles:
            c = Hangar.GetFreeShuttlesCount();
            if (c > 0)
            {
                shuttleMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);
                shuttleLabel.text = Localization.GetPhrase(LocalizedPhrase.FreeShuttles) + c.ToString();
                shuttleLabel.color = whitecolor;
            }
            else
            {
                shuttleMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                shuttleLabel.text = Localization.GetPhrase(LocalizedPhrase.NoShuttles);
                shuttleLabel.color = redcolor;
                readyToStart = false;
            }
            lastShuttlesListMarker = Hangar.listChangesMarkerValue;
            //fuel:
            fuelLabel.text = Localization.GetPhrase(LocalizedPhrase.FuelNeeded) + FUEL_NEEDED.ToString();
            c = (int)colony.storage.standartResources[ResourceType.FUEL_ID];
            if (c > FUEL_NEEDED)
            {
                fuelMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);                
                fuelLabel.color = whitecolor;
            }
            else
            {
                fuelMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                fuelLabel.color = redcolor;
                readyToStart = false;
            }
            if (preparingMode != true)
            {
                shuttleLine.gameObject.SetActive(true);
                fuelLine.gameObject.SetActive(true);
                minigameButton.SetActive(false);
            }
            //launch button
            launchButtonLabel.text = Localization.GetWord(LocalizedWord.Launch);
            launchButton.interactable = readyToStart;
            launchButton.GetComponent<Image>().color = readyToStart ? lightcyan : Color.grey;
            launchButton.gameObject.SetActive(true);
            //            
            preparingMode = true;
        }
        else
        {
             // отрисовка существующей
            expLabel.text = Localization.GetExpeditionName(showingExpedition);
            statedCrystalsCount = showingExpedition.crystalsCollected;
            statedSuppliesCount = showingExpedition.suppliesCount;
            lastChangesMarkerValue = showingExpedition.changesMarkerValue;

            crewStableName.text = showingExpedition.crew.name;
            if (crewDropdown.gameObject.activeSelf)
            {
                crewDropdown.gameObject.SetActive(false);
                crewStableName.enabled = true;
            }
            //supplies
            suppliesStableValue.text = statedSuppliesCount.ToString();
            suppliesStableValue.color = statedSuppliesCount > 0 ? whitecolor : redcolor;
            if (preparingMode != false)
            {
                suppliesSlider.gameObject.SetActive(false);
                suppliesStableValue.gameObject.SetActive(true);
            }
            //crystals
            crystalsStableValue.text = statedCrystalsCount.ToString();
            if (preparingMode != false)
            {
                crystalsSlider.gameObject.SetActive(false);
                crystalsStableValue.gameObject.SetActive(true);
            }
            //transmitter:
            if (showingExpedition.hasConnection)
            {
                transmitterMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.ConnectionOK);
                transmitterLabel.color = whitecolor;
            }
            else
            {
                transmitterMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.ConnectionLost);
                transmitterLabel.color = redcolor;
            }
            //shuttle & fuel
            if (preparingMode != false) {
                shuttleLine.gameObject.SetActive(false);
                fuelLine.gameObject.SetActive(false);                
            }            
            //launchbutton
            if (showingExpedition.stage == Expedition.ExpeditionStage.OnMission | showingExpedition.stage == Expedition.ExpeditionStage.WayIn)
            {
                launchButtonLabel.text = Localization.GetPhrase(LocalizedPhrase.StopMission);
                launchButton.GetComponent<Image>().color = halfred;
                launchButton.interactable = true;
                launchButton.gameObject.SetActive(true);
            }
            else launchButton.gameObject.SetActive(false);

            minigameButton.SetActive(showingExpedition.stage == Expedition.ExpeditionStage.OnMission);

            preparingMode = false;
        }       

        var edb = expDestinationButton;
        if (selectedDestination != null)
        {
            edb.transform.GetChild(0).GetComponent<RawImage>().uvRect = GlobalMapUI.GetMarkerRect(selectedDestination.type);
            edb.SetActive(workOnMainCanvas);
        }
        else
        {
            if (edb.activeSelf) edb.SetActive(false);
        }        
    }
    private void PrepareCrewDropdown()
    {
        const char quotes = '"';
        var opts = new List<Dropdown.OptionData>() { };
        if (showingExpedition == null)
        {
            crewsIDs = new List<int>() { -1 };
            opts.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoCrew)));
            var clist = Crew.crewsList;
            if (clist != null && clist.Count > 0)
            {
                foreach (Crew c in Crew.crewsList)
                {
                    if (c.atHome)
                    {
                        opts.Add(new Dropdown.OptionData(quotes + c.name + quotes));
                        crewsIDs.Add(c.ID);
                    }
                }
            }
            crewDropdown.options = opts;
            if (crewsIDs.Count > 1)
            {
                crewDropdown.value = 1;
                if (selectedCrew != null)
                {
                    for (int i = 1; i < crewsIDs.Count; i++)
                    {
                        if (crewsIDs[i] == selectedCrew.ID)
                        {
                            crewDropdown.value = i;
                        }
                    }
                }
                else selectedCrew = clist[0];
                crewDropdown.interactable = true;
            }
            else
            {
                crewDropdown.value = 0;
                crewDropdown.interactable = false;
                selectedCrew = null;
            }
        }
        else
        {
            selectedCrew = showingExpedition.crew;
            opts.Add(new Dropdown.OptionData(quotes + selectedCrew.name + quotes));
            crewsIDs = new List<int>(selectedCrew.ID);
            crewDropdown.options = opts;
            crewDropdown.value = 0;
            crewDropdown.interactable = false;
        }        
        lastCrewListMarker = Crew.listChangesMarkerValue;
    }

    public void StatusUpdate()
    {
        bool redrawRequest = false;
        if (lastCrewListMarker != Crew.listChangesMarkerValue) redrawRequest = true;
        else {
            if (lastShuttlesListMarker != Hangar.listChangesMarkerValue) redrawRequest = true;
        }
        if (redrawRequest) RedrawWindow();
        else
        {
            if (showingExpedition != null)
            {
                crystalsStableValue.text = showingExpedition.crystalsCollected.ToString();
                suppliesStableValue.text = showingExpedition.suppliesCount.ToString();
            }
        }
    }

    public void OnCrewValueChanged(int i)
    {
        if (i == 0)
        {
            selectedCrew = null;
            if (crewButton.activeSelf) crewButton.SetActive(false);
        }
        else
        {
            selectedCrew = Crew.crewsList[i - 1];
            selectedCrew.DrawCrewIcon(crewButton.transform.GetChild(0).GetComponent<RawImage>());
            if (!crewButton.activeSelf) crewButton.SetActive(true);
        }
    }
    public void OnSuppliesSliderChanged(float f)
    {
        int x = (int)f;
        var t = suppliesSlider.transform.GetChild(3).GetComponent<Text>();
        t.text = x.ToString();
        if (x > colony.storage.standartResources[ResourceType.SUPPLIES_ID]) t.color = Color.red; else t.color = Color.white;
    }
    public void OnCrystalsSliderChanged(float f)
    {
        int x = (int)f;
        var t = crystalsSlider.transform.GetChild(3).GetComponent<Text>();
        t.text = x.ToString();
        if (x > colony.energyCrystalsCount) t.color = Color.red; else t.color = Color.white;
    }

    public void LaunchButton()
    {
        if (showingExpedition == null)
        {
            if (selectedCrew != null && selectedCrew.atHome)
            {
                var storage = colony.storage;
                var res = storage.standartResources;
                if (suppliesSlider.value <= res[ResourceType.SUPPLIES_ID] &&
                    crystalsSlider.value <= colony.energyCrystalsCount &&
                    res[ResourceType.FUEL_ID] >= FUEL_NEEDED)
                {
                    int shID = Hangar.GetFreeShuttleID();
                    if (shID != Hangar.NO_SHUTTLE_VALUE)
                    {
                        var t = QuantumTransmitter.GetFreeTransmitter();
                        if (t != null)
                        {
                            if (storage.TryGetResources(ResourceType.Fuel, FUEL_NEEDED)) {
                                var e = new Expedition(selectedDestination, selectedCrew, shID, t, storage.GetResources(ResourceType.Supplies, suppliesSlider.value), colony.GetEnergyCrystals(crystalsSlider.value));
                                if (workOnMainCanvas)
                                {
                                    showingExpedition = e;
                                    RedrawWindow();
                                    return;
                                }
                                else
                                {
                                    GameMaster.realMaster.globalMap.observer.GetComponent<GlobalMapUI>().PreparePointDescription();
                                    gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                GameLogUI.MakeAnnouncement(Localization.GetExpeditionErrorText(ExpeditionComposingErrors.NotEnoughFuel));
                                RedrawWindow();
                                return;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            showingExpedition.EndMission();
        }
    }
    public void MinigameButton()
    {
        if (showingExpedition == null || showingExpedition.stage != Expedition.ExpeditionStage.OnMission) RedrawWindow();
        else
        {            
            ExplorationPanelUI.Deactivate();
            UIController.SetActivity(false);
            ExploringMinigameUI.ShowExpedition(showingExpedition,false);
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
    }
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            if (UIController.current != null)
            {
                UIController.current.statusUpdateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            if (UIController.current != null)
            {
                UIController.current.statusUpdateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
    }
}
