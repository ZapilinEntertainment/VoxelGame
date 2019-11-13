using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIExpeditionObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown crewDropdown;
    [SerializeField] private GameObject crewButton, closeButton;
    [SerializeField] private Slider suppliesSlider, crystalsSlider;
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
    private byte lastChangesMarkerValue = 0;
    private int lastCrewListMarker = 0, lastShuttlesListMarker = 0;
    private ColonyController colony;
    private Expedition showingExpedition;
    private PointOfInterest selectedDestination;
    private Crew selectedCrew;
    private List<int> crewsIDs;

    private readonly Color lightcyan = new Color(0.5f, 1f, 0.95f);
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
            Color redcolor = Color.red, whitecolor = Color.white;
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
            //launch button
            launchButtonLabel.text = Localization.GetWord(LocalizedWord.Launch);
            launchButton.interactable = readyToStart;
            launchButton.GetComponent<Image>().color = readyToStart ? lightcyan : Color.grey;
            //
        }
        else
        {
             // отрисовка существующей
            expLabel.text = Localization.GetExpeditionName(showingExpedition);
            statedCrystalsCount = showingExpedition.crystalsCollected;
            statedSuppliesCount = showingExpedition.suppliesCount;
            lastChangesMarkerValue = showingExpedition.changesMarkerValue;            
        }
        suppliesSlider.value = statedSuppliesCount;
        OnSuppliesSliderChanged(statedSuppliesCount);
        crystalsSlider.value = statedCrystalsCount;
        OnCrystalsSliderChanged(statedCrystalsCount);

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
                var res = colony.storage.standartResources;
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
                            var e = new Expedition(selectedDestination, selectedCrew, shID, t);
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
                    }
                }
            }
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
