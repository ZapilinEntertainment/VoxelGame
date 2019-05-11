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
        UIRecruitingCenterObserver urco = Instantiate(Resources.Load<GameObject>("UIPrefs/recruitingCenterObserver"), UIController.current.rightPanel.transform).GetComponent<UIRecruitingCenterObserver>();
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
        uwb.SetObservingWorkBuilding(rc);        
        PrepareWindow();
    }

    public void PrepareWindow()
    {
        PrepareCrewsDropdown();
        PrepareButtons();
        crewSlotsInfo.text = Crew.crewsList.Count.ToString() + " / " + RecruitingCenter.GetCrewsSlotsCount().ToString();
    }
    private void PrepareButtons()
    {
        if (showingCrew == null)
        {
            hiremode = true;            
            if (observingRCenter.finding)
            {
                hireButton.gameObject.SetActive(false);
                UIController.current.ActivateProgressPanel(ProgressPanelMode.RecruitingCenter);
                crewsDropdown.gameObject.SetActive(false);
            }
            else
            {
                hireButton.gameObject.SetActive(true);
                UIController.current.DeactivateProgressPanel();
                crewsDropdown.gameObject.SetActive(true);
            }            
            infoButton.SetActive(false);
            replenishButton.SetActive(false);
            membersText.enabled = false;
            Crew.DisableObserver();
        }
        else
        {
            crewsDropdown.gameObject.SetActive(true);
            hiremode = false;
            UIController.current.DeactivateProgressPanel();
            hireButton.gameObject.SetActive(false);
            infoButton.SetActive(true);

            replenishButton.transform.GetChild(1).GetComponent<Text>().text = RecruitingCenter.REPLENISH_COST.ToString();
            replenishButton.SetActive(showingCrew.membersCount != Crew.MAX_MEMBER_COUNT);

            membersText.text = Localization.GetPhrase(LocalizedPhrase.MembersCount) + ": " + showingCrew.membersCount.ToString() + '/' + Crew.MAX_MEMBER_COUNT.ToString();
            membersText.enabled = true;

            if (Crew.crewObserver != null && Crew.crewObserver.isActiveAndEnabled) Crew.crewObserver.RedrawWindow();
        }
    }

    override public void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingRCenter == null) SelfShutOff();
        else
        {
            if (lastCrewActionHash != Crew.actionsHash)
            {
                PrepareWindow();
                lastCrewActionHash = Crew.actionsHash;
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
        lastCrewActionHash = Crew.actionsHash;

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
            if (Crew.crewObserver != null && Crew.crewObserver.isActiveAndEnabled) InfoButton();
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
            var rt = infoButton.GetComponent<RectTransform>();
            float f = Screen.height / 3f;
            if (f < 500) f = 500;
            if (f > Screen.height * 0.8f) f = Screen.height * 0.8f;
            var r = new Rect(new Vector2(rt.position.x - rt.rect.width / 2f, 50f), new Vector2(f,f));
            showingCrew.ShowOnGUI(r, SpriteAlignment.BottomRight, true );
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
                    GameLogUI.NotEnoughMoneyAnnounce();
                }
            }
        }
    }
    //

    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        if (hiremode) UIController.current.DeactivateProgressPanel();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingRCenter = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        if (hiremode) UIController.current.DeactivateProgressPanel();
        gameObject.SetActive(false);
    }

    public override void LocalizeTitles()
    {
        hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost().ToString() + ')';
    }
}
