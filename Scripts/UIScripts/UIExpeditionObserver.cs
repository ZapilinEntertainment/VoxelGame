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
    private static bool waitForWorkRestoring = false;
    private bool? preparingMode = null;
    private byte lastChangesMarkerValue = 0;
    private int lastCrewListMarker = 0, lastShuttlesListMarker = 0;
    private ColonyController colony;
    private Expedition observingExpedition;
    private PointOfInterest selectedDestination;
    private Crew selectedCrew;
    private List<int> crewsIDs;

    private static UIExpeditionObserver _currentObserver;
    private readonly Color lightcyan = new Color(0.5f, 1f, 0.95f), halfred = new Color(1f,0f,0f,0.5f);
    private const int FUEL_BASE_COST = 5;

    private void Awake()
    {
        colony = GameMaster.realMaster?.colonyController;
        suppliesSlider.minValue = Expedition.MIN_SUPPLIES_COUNT;
        suppliesSlider.maxValue = Expedition.MAX_SUPPLIES_COUNT;
        crystalsSlider.maxValue = Expedition.MAX_START_CRYSTALS;
    }

    #region observer standart functions
    public static UIExpeditionObserver GetObserver()
    {
        if (_currentObserver == null)
        {
            _currentObserver = Instantiate(Resources.Load<GameObject>("UIPrefs/expeditionPanel"),
                MainCanvasController.current.mainCanvas).GetComponent<UIExpeditionObserver>();
        }
        return _currentObserver;
    }
    public static void Show(RectTransform parent, SpriteAlignment alignment, Expedition e, bool useCloseButton)
    {
        Show(parent, new Rect(Vector2.zero, parent.rect.size), alignment, e, useCloseButton);
    }
    public static void Show(RectTransform parent, SpriteAlignment alignment, PointOfInterest poi, bool useCloseButton)
    {
        Show(parent, parent.rect, alignment, poi, useCloseButton);
    }
    public static void Show(RectTransform parent, Rect r, SpriteAlignment alignment, Expedition e, bool useCloseButton)
    {
        var co = GetObserver();
        if (!co.gameObject.activeSelf) co.gameObject.SetActive(true);
        co.SetPosition(parent, r, alignment, useCloseButton);
        co.ShowExpedition(e, useCloseButton);
    }
    public static void Show(RectTransform parent, Rect r, SpriteAlignment alignment, PointOfInterest poi, bool useCloseButton)
    {
        var co = GetObserver();
        if (!co.gameObject.activeSelf) co.gameObject.SetActive(true);
        co.SetPosition(parent, r, alignment, useCloseButton);
        co.ShowExpedition(poi, useCloseButton);
    }
    public static void DisableObserver()
    {
        if (_currentObserver != null) _currentObserver.gameObject.SetActive(false);
    }
    public static void DestroyObserver()
    {
        if (_currentObserver != null) Destroy(_currentObserver.gameObject);
    }
    public static void Refresh()
    {
        if (_currentObserver != null) _currentObserver.RedrawWindow();
    }

    private void SetPosition(RectTransform parent, Rect r, SpriteAlignment alignment, bool useCloseButton)
    {
        closeButton.SetActive(useCloseButton);
        var rt = GetObserver().GetComponent<RectTransform>();
        MainCanvasController.PositionElement(rt, parent, alignment, r);
    }
    private void ShowExpedition(Expedition e, bool useCloseButton)
    {
        if (e == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            observingExpedition = e;
            RedrawWindow();
            closeButton.SetActive(useCloseButton);
        }
    }
    private void ShowExpedition(PointOfInterest poi, bool useCloseButton)
    {
        if (poi == null) gameObject.SetActive(false);
        else
        {
            if (poi.workingExpedition != null)
            {
                ShowExpedition(poi.workingExpedition, useCloseButton);
                return;
            }
            else
            {
                observingExpedition = null;
                selectedCrew = null;
                selectedDestination = poi;
            }
            RedrawWindow();
        }
    }
    public void ClearInfo(Expedition e)
    {
        if (_currentObserver != null && _currentObserver.observingExpedition == e)
        {
            _currentObserver.gameObject.SetActive(false);
        }
    }
    #endregion

    

    private void RedrawWindow()
    {
        int statedCrystalsCount = 0, statedSuppliesCount = Expedition.MIN_SUPPLIES_COUNT;
        Color redcolor = Color.red, whitecolor = Color.white;
        GameObject g;
        if (observingExpedition == null)
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
                transmitterMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskCompleted);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.FreeTransmitters) + c.ToString();
                transmitterLabel.color = whitecolor;
            }
            else
            {
                transmitterMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskFailed);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.NoTransmitters);
                transmitterLabel.color = redcolor;
                readyToStart = false;
            }
            //shuttles:
            c = Hangar.GetFreeShuttlesCount();
            if (c > 0)
            {
                shuttleMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskCompleted);
                shuttleLabel.text = Localization.GetPhrase(LocalizedPhrase.FreeShuttles) + c.ToString();
                shuttleLabel.color = whitecolor;
            }
            else
            {
                shuttleMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskFailed);
                shuttleLabel.text = Localization.GetPhrase(LocalizedPhrase.NoShuttles);
                shuttleLabel.color = redcolor;
                readyToStart = false;
            }
            lastShuttlesListMarker = Hangar.listChangesMarkerValue;
            //fuel:
            if (FuelCheck() == false) readyToStart = false;
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
            expLabel.text = Localization.GetExpeditionName(observingExpedition);
            statedCrystalsCount = observingExpedition.crystalsCollected;
            statedSuppliesCount = observingExpedition.suppliesCount;
            lastChangesMarkerValue = observingExpedition.changesMarkerValue;

            crewStableName.text = observingExpedition.crew?.name ?? Localization.GetPhrase(LocalizedPhrase.NoCrew);
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
            if (observingExpedition.hasConnection)
            {
                transmitterMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskCompleted);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.ConnectionOK);
                transmitterLabel.color = whitecolor;
            }
            else
            {
                transmitterMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskFailed);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.ConnectionLost);
                transmitterLabel.color = redcolor;
            }
            //shuttle & fuel
            if (preparingMode != false) {
                shuttleLine.gameObject.SetActive(false);
                fuelLine.gameObject.SetActive(false);                
            }            
            //launchbutton
            if (observingExpedition.stage == Expedition.ExpeditionStage.OnMission | observingExpedition.stage == Expedition.ExpeditionStage.WayIn)
            {
                launchButtonLabel.text = Localization.GetPhrase(LocalizedPhrase.StopMission);
                launchButton.GetComponent<Image>().color = halfred;
                launchButton.interactable = true;
                launchButton.gameObject.SetActive(true);
            }
            else launchButton.gameObject.SetActive(false);

            minigameButton.SetActive(observingExpedition.stage == Expedition.ExpeditionStage.OnMission);

            preparingMode = false;
        }       

        var edb = expDestinationButton;
        if (selectedDestination != null)
        {
            edb.transform.GetChild(0).GetComponent<RawImage>().uvRect = GlobalMapCanvasController.GetMarkerRect(selectedDestination.type);
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
        if (observingExpedition == null)
        {
            if (selectedCrew != null && !selectedCrew.atHome) selectedCrew = null;
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
                else selectedCrew = Crew.GetCrewByID(crewsIDs[1]);
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
            selectedCrew = observingExpedition.crew;
            opts.Add(new Dropdown.OptionData(quotes + selectedCrew.name + quotes));
            crewsIDs = new List<int>(selectedCrew.ID);
            crewDropdown.options = opts;
            crewDropdown.value = 0;
            crewDropdown.interactable = false;
        }        
        lastCrewListMarker = Crew.listChangesMarkerValue;
    }
    private bool FuelCheck()
    {
        int fuelNeeded = (int)(MapPoint.Distance(GameMaster.realMaster.globalMap.cityPoint, selectedDestination) * FUEL_BASE_COST);
        fuelLabel.text = Localization.GetPhrase(LocalizedPhrase.FuelNeeded) + fuelNeeded.ToString();
        int c = (int)colony.storage.standartResources[ResourceType.FUEL_ID];
        if (c > fuelNeeded)
        {
            fuelMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskCompleted);
            fuelLabel.color = Color.white;
            return true;
        }
        else
        {
            fuelMarker.uvRect = MainCanvasController.GetIconUVRect(Icons.TaskFailed);
            fuelLabel.color = Color.red;
            return false;
        }
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
            if (observingExpedition != null)
            {
                crystalsStableValue.text = observingExpedition.crystalsCollected.ToString();
                suppliesStableValue.text = observingExpedition.suppliesCount.ToString();                
            }
        }
    }
    private void Update()
    {
        if (fuelLabel.isActiveAndEnabled) FuelCheck();
    }

    public void OnCrewValueChanged(int i)
    {
        
        if (i == 0)
        {
            if (crewButton.activeSelf) crewButton.SetActive(false);
        }
        else
        {
            selectedCrew = Crew.GetCrewByID(crewsIDs[i]);
            selectedCrew.DrawCrewIcon(crewButton.transform.GetChild(0).GetComponent<RawImage>());
            RedrawWindow();
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
        if (observingExpedition == null)
        {
            if (selectedCrew != null && selectedCrew.atHome)
            {
                var storage = colony.storage;
                var res = storage.standartResources;
                bool TESTMODE = GameMaster.realMaster.weNeedNoResources;
                if (TESTMODE || suppliesSlider.value <= res[ResourceType.SUPPLIES_ID] &&
                    crystalsSlider.value <= colony.energyCrystalsCount &&
                    res[ResourceType.FUEL_ID] >= FUEL_BASE_COST                    
                    )
                {
                    int shID = Hangar.GetFreeShuttleID();
                    if (shID != Hangar.NO_SHUTTLE_VALUE)
                    {
                        var t = QuantumTransmitter.GetFreeTransmitter();
                        if (t != null)
                        {
                            if (storage.TryGetResources(ResourceType.Fuel, FUEL_BASE_COST)) {
                                var e = new Expedition(selectedDestination, selectedCrew, shID, t, storage.GetResources(ResourceType.Supplies, suppliesSlider.value), colony.GetEnergyCrystals(crystalsSlider.value));
                                if (workOnMainCanvas)
                                {
                                    observingExpedition = e;
                                }
                                else
                                {
                                    observingExpedition = null;
                                    selectedCrew = null;
                                    GameMaster.realMaster.globalMap.observer.GetComponent<GlobalMapCanvasController>().PreparePointDescription();
                                    gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                AnnouncementCanvasController.MakeAnnouncement(Localization.GetExpeditionErrorText(ExpeditionComposingErrors.NotEnoughFuel));
                            }
                            RedrawWindow();
                        }
                    }
                }
            }
        }
        else
        {
            observingExpedition.EndMission();
        }
    }
    public void MinigameButton()
    {
        if (observingExpedition == null || observingExpedition.stage != Expedition.ExpeditionStage.OnMission) RedrawWindow();
        else
        {
            if (MainCanvasController.isMainCanvasActive)
            { // main canvas
                ExplorationPanelUI.Deactivate();
                MainCanvasController.SetActivity(false);
                ExploringMinigameUI.ShowExpedition(observingExpedition, false);
            }
            else
            { // global map canvas
                GlobalMapCanvasController.GetObserver()?.CloseInfopanel();
                ExploringMinigameUI.ShowExpedition(observingExpedition, true);
            }
            
            gameObject.SetActive(false);
        }
    }
    public void CrewButton()
    {
        if (selectedCrew != null)
        {
            var rt = transform.parent.GetComponent<RectTransform>();
            var myRect = GetComponent<RectTransform>().rect;
            UICrewObserver.Show(rt, new Rect(myRect.x, myRect.y, myRect.width * rt.localScale.x, myRect.height * rt.localScale.y), SpriteAlignment.Center, selectedCrew, true);
            UICrewObserver.GetObserver().AddToClosingEvent(() => { RestoreActivity(); });
            gameObject.SetActive(false);
            waitForWorkRestoring = true;
        }
    }
    public static void RestoreActivity() {
        if (waitForWorkRestoring) { _currentObserver?.gameObject.SetActive(true); }
        waitForWorkRestoring = false;
    }

    private void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            MainCanvasController.current.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
    }
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            if (MainCanvasController.current != null)
            {
                MainCanvasController.current.statusUpdateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
        waitForWorkRestoring = false;
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            if (MainCanvasController.current != null)
            {
                MainCanvasController.current.statusUpdateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
        waitForWorkRestoring = false;
    }
}
