using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIRecruitingCenterObserver : UIObserver
{
    public RecruitingCenter observingRCenter { get; private set; }
#pragma warning disable 0649
    [SerializeField] private Dropdown crewsDropdown;
    [SerializeField] private Button hireButton;
    [SerializeField] private GameObject infoButton, replenishButton;
    [SerializeField] private Text crewSlotsInfo, membersText;
#pragma warning restore 0649
    public Crew showingCrew { get; private set; }
    public bool hiremode { get; private set; }
    private int lastCrewActionHash = 0;
    private List<int> crewsIDsList;

    public static UIRecruitingCenterObserver InitializeRCenterObserverScript()
    {
        UIRecruitingCenterObserver urco = Instantiate(Resources.Load<GameObject>("UIPrefs/recruitingCenterObserver"), mycanvas.rightPanel.transform).GetComponent<UIRecruitingCenterObserver>();
        RecruitingCenter.rcenterObserver = urco;
        return urco;
    }

    public void SetObservingRCenter(RecruitingCenter rc)
    {
        if (rc == null)
        {
            SelfShutOff();
            return;
        }
        UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
        if (uwb == null) uwb = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else uwb.gameObject.SetActive(true);
        observingRCenter = rc; isObserving = true;
        uwb.SetObservingPlace(rc);        
        PrepareWindow();
    }

    public void PrepareWindow()
    {
        PrepareCrewsDropdown();
        PrepareButtons();
        crewSlotsInfo.text = Crew.crewsList.Count.ToString() + " / " + RecruitingCenter.GetCrewsSlotsCount().ToString();
        hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost().ToString() + ')';
    }
    private void PrepareButtons()
    {
        if (showingCrew == null)
        {
            hiremode = true;            
            if (observingRCenter.finding)
            {
                hireButton.gameObject.SetActive(false);
                mycanvas.ActivateProgressPanel(ProgressPanelMode.RecruitingCenter);
                crewsDropdown.gameObject.SetActive(false);
            }
            else
            {
                hireButton.gameObject.SetActive(true);
                mycanvas.DeactivateProgressPanel(ProgressPanelMode.RecruitingCenter);
                crewsDropdown.gameObject.SetActive(true);
            }            
            infoButton.SetActive(false);
            replenishButton.SetActive(false);
            membersText.enabled = false;
            UICrewObserver.DisableObserver();
        }
        else
        {
            crewsDropdown.gameObject.SetActive(true);
            hiremode = false;
            mycanvas.DeactivateProgressPanel(ProgressPanelMode.RecruitingCenter);
            hireButton.gameObject.SetActive(false);
            infoButton.SetActive(true);

            replenishButton.transform.GetChild(1).GetComponent<Text>().text = RecruitingCenter.REPLENISH_COST.ToString();
            replenishButton.SetActive(
                (showingCrew.membersCount != Crew.MAX_MEMBER_COUNT) & (showingCrew.atHome) );

            membersText.text = Localization.GetPhrase(LocalizedPhrase.MembersCount) + ": " + showingCrew.membersCount.ToString() + '/' + Crew.MAX_MEMBER_COUNT.ToString();
            membersText.enabled = true;

            UICrewObserver.Refresh();
        }
    }

    override public void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingRCenter == null) SelfShutOff();
        else
        {
            if (lastCrewActionHash != Crew.listChangesMarkerValue)
            {
                PrepareWindow();
                lastCrewActionHash = Crew.listChangesMarkerValue;
            }
            else
            {
                if ((showingCrew == null) != hiremode) PrepareButtons();
            }
        }
    }    

    public void PrepareCrewsDropdown()
    {
        List<Dropdown.OptionData> crewButtons = new List<Dropdown.OptionData>();
        crewButtons.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.HireNewCrew) ));
        crewsIDsList = new List<int> { -1 };
        var crews = Crew.crewsList;        
        if (crews.Count > 0)
        {
            foreach (Crew c in crews)
            {
                crewButtons.Add(new Dropdown.OptionData('\"' + c.name + '\"'));
                crewsIDsList.Add(c.ID);
            }
        }
        crewsDropdown.options = crewButtons;
        lastCrewActionHash = Crew.listChangesMarkerValue;

        if (showingCrew != null)
        {
            for(int i = 1; i < crewsIDsList.Count; i++)
            {
                if (crewsIDsList[i] == showingCrew.ID)
                {
                    crewsDropdown.value = i;
                    break;
                }
            }
        }
    }

    //buttons
    public void SelectCrew(int i)
    {
        if (crewsIDsList[i] == -1) showingCrew = null;
        else
        {
            showingCrew = Crew.GetCrewByID(crewsIDsList[i]);
            if (showingCrew != null)
            {
                InfoButton();
                if (!infoButton.activeSelf) infoButton.SetActive(true);
                if (showingCrew.membersCount < Crew.MAX_MEMBER_COUNT & !replenishButton.activeSelf) replenishButton.SetActive(true);
            }
            else PrepareCrewsDropdown();
        }
        PrepareButtons();
    }
    public void SelectCrew(Crew c)
    {
        showingCrew = c;
        PrepareButtons();
    }
    public void HireButton()
    {
        observingRCenter.StartHiring();
        PrepareWindow();
    }
    public void InfoButton()
    {
        if (showingCrew == null)
        {
            infoButton.SetActive(false);
        }
        else
        {
            RectTransform canvas = mycanvas.GetMainCanvasTransform() as RectTransform;
            float f = Screen.height / 2f;
            infoButton.SetActive(false);
            var r = new Rect(new Vector2(-GetComponent<RectTransform>().rect.width,0f), new Vector2(f,f));            
            UICrewObserver.Show(canvas,r, SpriteAlignment.BottomRight, showingCrew, true );
            UICrewObserver.GetObserver().AddToClosingEvent(() => { this.infoButton.SetActive(true); });
        }
    }
    public void ReplenishButton()
    {
        if (showingCrew == null) replenishButton.SetActive(false);
        else
        {
            if (showingCrew.membersCount == Crew.MAX_MEMBER_COUNT) replenishButton.SetActive(false);
            else
            {
                var colony = GameMaster.realMaster.colonyController;
                float hireCost = RecruitingCenter.REPLENISH_COST;
                if (colony.energyCrystalsCount >= hireCost)
                {
                    colony.GetEnergyCrystals(hireCost);
                    showingCrew.AddMember();
                    PrepareButtons();
                }
                else
                {
                    AnnouncementCanvasController.NotEnoughMoneyAnnounce();
                }
            }
        }
    }
    //

    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        if (hiremode) mycanvas.DeactivateProgressPanel(ProgressPanelMode.RecruitingCenter);
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingRCenter = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        if (hiremode) mycanvas.DeactivateProgressPanel(ProgressPanelMode.RecruitingCenter);
        gameObject.SetActive(false);
    }
}
