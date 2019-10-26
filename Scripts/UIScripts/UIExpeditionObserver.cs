using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIExpeditionObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown missionDropdown, crewDropdown, shuttleDropdown, artifactDropdown;
    [SerializeField] private GameObject crewButton, shuttleButton;
    [SerializeField] private Slider suppliesSlider, crystalsSlider;
    [SerializeField] private Transform transmitterPanel, fuelPanel;
#pragma warning restore 0649
    private RawImage transmitterMarker { get { return transmitterPanel.GetChild(1).GetComponent<RawImage>(); } }
    private Text fuelLabel { get { return fuelPanel.GetChild(0).GetComponent<Text>(); } }

    private Transform expNameField { get { return transform.GetChild(0); } }
    private Text expLabel { get { return expNameField.GetChild(0).GetComponent<Text>(); } }
    private GameObject expDestinationButton { get { return expNameField.GetChild(1).gameObject; } }

    private bool subscribedToUpdate = false;
    private byte lastChangesMarkerValue = 0;
    private int lastCrewListMarker = 0;
    private Expedition showingExpedition;

    private PointOfInterest selectedDestination;
    private Crew selectedCrew;
    private List<int> crewsIDs;

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
                showingExpedition = poe.workingExpedition;
                selectedCrew = showingExpedition.crew;
                selectedDestination = showingExpedition.destination;
            }
            else
            {
                showingExpedition = null;
                selectedCrew = null;
                selectedDestination = poe;
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
            suppliesSlider.value = 0f;
            crystalsSlider.value = 0f;
        }
        else
        {
             // отрисовка существующей
            //name
            expLabel.text = Localization.GetExpeditionName(showingExpedition);
            //supplies
            suppliesSlider.value = showingExpedition.suppliesCount / Expedition.MAX_SUPPLIES_COUNT;
            crystalsSlider.value = showingExpedition.collectedMoney / Expedition.MAX_START_CRYSTALS;

            lastChangesMarkerValue = showingExpedition.changesMarkerValue;            
        }

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
        if (showingExpedition == null) gameObject.SetActive(false);
        else {
            if (lastChangesMarkerValue != showingExpedition.changesMarkerValue) RedrawWindow();
            else
            {
                if (showingExpedition.hasConnection && (showingExpedition.stage == Expedition.ExpeditionStage.OnMission | showingExpedition.stage == Expedition.ExpeditionStage.LeavingMission))
                {
                    //fill stage text & stage bar
                    currentStepText.text = Localization.GetWord(LocalizedWord.Step) + ' ' + showingExpedition.currentStep.ToString() + " / " + showingExpedition.mission.stepsCount.ToString();
                    float f = showingExpedition.progress / Expedition.ONE_STEP_WORKFLOW;
                    stepProgressBar.fillAmount = f;
                    progressText.text = ((int)(f * 100)).ToString() + '%';
                }
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
        suppliesSlider.transform.GetChild(3).GetComponent<Text>().text = f *
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
