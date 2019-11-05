using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIExpeditionObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown crewDropdown;
    [SerializeField] private GameObject crewButton;
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

    private bool subscribedToUpdate = false;
    private byte lastChangesMarkerValue = 0;
    private float statedSuppliesCount = 0f, statedCrystalsCount = 0f;
    private int lastCrewListMarker = 0;
    private Expedition showingExpedition;

    private PointOfInterest selectedDestination;
    private Crew selectedCrew;
    private List<int> crewsIDs;

    private readonly Color lightcyan = new Color(0.5f, 1f, 0.95f);
    private const int FUEL_NEEDED = 200;


    public void SetPosition(Rect r, SpriteAlignment alignment)
    {
        var rt = GetComponent<RectTransform>();
        rt.position = r.position;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, r.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, r.y);
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
    public void Show(PointOfInterest poe)
    {
        if (poe == null) gameObject.SetActive(false);
        else
        {
            if (poe.workingExpedition != null)
            {
                Show(poe.workingExpedition);
                return;
            }
            else
            {
                showingExpedition = null;
                selectedCrew = null;
                selectedDestination = poe;
                statedSuppliesCount = 0;
                statedCrystalsCount = 0;
            }
            RedrawWindow();
        }
    }

    private void RedrawWindow()
    {
        if (showingExpedition == null)
        {
            //подготовка новой экспедиции
            expLabel.text = Localization.GetExpeditionName(selectedDestination);
            bool readyToStart = selectedCrew != null;
            if (readyToStart) readyToStart = selectedCrew.atHome;
            //transmitters:
            int c = QuantumTransmitter.GetFreeTransmittersCount();
            if (c > 0)
            {
                transmitterMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.FreeTransmitters) + c.ToString();
            }
            else
            {
                transmitterMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                transmitterLabel.text = Localization.GetPhrase(LocalizedPhrase.NoTransmitters);
                readyToStart = false;
            }
            //shuttles:
            c = Hangar.GetFreeShuttlesCount();
            if (c > 0)
            {
                shuttleMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);
                shuttleLabel.text = Localization.GetPhrase(LocalizedPhrase.FreeShuttles) + c.ToString();
            }
            else
            {
                shuttleMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                shuttleLabel.text = Localization.GetPhrase(LocalizedPhrase.NoShuttles);
                readyToStart = false;
            }
            //fuel:
            fuelLabel.text = Localization.GetPhrase(LocalizedPhrase.FuelNeeded) + FUEL_NEEDED.ToString();
            c = (int)GameMaster.realMaster.colonyController.storage.standartResources[ResourceType.FUEL_ID];
            if (c > FUEL_NEEDED)
            {
                fuelMarker.uvRect = UIController.GetIconUVRect(Icons.TaskCompleted);                
                fuelLabel.color = Color.white;
            }
            else
            {
                fuelMarker.uvRect = UIController.GetIconUVRect(Icons.TaskFailed);
                fuelLabel.color = Color.red;
                readyToStart = false;
            }
            //launch button
            launchButtonLabel.text = Localization.GetWord(LocalizedWord.Launch);
            launchButton.interactable = readyToStart;
            launchButton.GetComponent<Image>().color = readyToStart ? lightcyan : Color.grey;
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
        crystalsSlider.value = statedCrystalsCount;

        var edb = expDestinationButton;
        if (selectedDestination != null)
        {
            edb.transform.GetChild(0).GetComponent<RawImage>().uvRect = GlobalMapUI.GetMarkerRect(selectedDestination.type);
            if (!edb.activeSelf) edb.SetActive(true);
        }
        else
        {
            if (edb.activeSelf) edb.SetActive(false);
        }
        PrepareCrewDropdown();

    }
    private void PrepareCrewDropdown()
    {
        const char quotes = '"';
        var opts = new List<Dropdown.OptionData>() { };
        if (showingExpedition != null)
        {
            selectedCrew = showingExpedition.crew;
            opts.Add(new Dropdown.OptionData(quotes + selectedCrew.name + quotes));
            crewsIDs.Add(selectedCrew.ID);

            crewDropdown.value = crewsIDs.Count;
            crewDropdown.interactable = false;
        }
        else
        {
            opts.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoCrew)));
            crewsIDs = new List<int>() { -1 };
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
            if (crewsIDs.Count > 1)
            {
                crewDropdown.value = 1;
                crewDropdown.interactable = true;
            }
            else
            {
                crewDropdown.value = 0;
                crewDropdown.interactable = false;
            }
        }              
    }

    public void StatusUpdate()
    {
        
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
        suppliesSlider.transform.GetChild(3).GetComponent<Text>().text = ((int)f * Expedition.MAX_SUPPLIES_COUNT).ToString();
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
