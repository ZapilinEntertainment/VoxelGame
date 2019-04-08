﻿using System.Collections;
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
    [SerializeField] private Text crewSlotsInfo;
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
    }
    private void PrepareButtons()
    {
        if (showingCrew == null)
        {
            hiremode = true;
            UIController.current.ActivateProgressPanel(ProgressPanelMode.RecruitingCenter);
            if (observingRCenter.finding)
            {
                hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);
            }
            else
            {
                hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost().ToString() + ')';
            }
            hireButton.gameObject.SetActive(true);
            infoButton.SetActive(false);
            replenishButton.SetActive(false);
        }
        else
        {
            hiremode = false;
            UIController.current.DeactivateProgressPanel();
            hireButton.gameObject.SetActive(false);
            infoButton.SetActive(true);

            replenishButton.transform.GetChild(1).GetComponent<Text>().text = RecruitingCenter.GetHireCost().ToString();
            replenishButton.GetComponent<Button>().interactable= showingCrew.membersCount != Crew.MAX_MEMBER_COUNT;
            replenishButton.SetActive(true);
        }
    }

    override protected void StatusUpdate()
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
        crewsIDsList.Add(-1);
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
        if (crewsIDsList[i] == -1)  showingCrew = null;
        else   showingCrew = Crew.GetCrewByID(crewsIDsList[i]);
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
    }
    public void InfoButton()
    {
        if (showingCrew == null)
        {
            infoButton.SetActive(false);
        }
        else
        {
            showingCrew.ShowOnGUI();
            //коррекция под окно?
        }
    }
    public void ReplenishButton()
    {
        if (showingCrew == null) replenishButton.SetActive(false);
        else
        {
            if (showingCrew.membersCount == Crew.MAX_MEMBER_COUNT) replenishButton.GetComponent<Button>().interactable = false;
            else
            {
                var colony = GameMaster.realMaster.colonyController;
                float hireCost = RecruitingCenter.REPLENISH_COST;
                if (colony.energyCrystalsCount >= hireCost)
                {
                    colony.GetEnergyCrystals(hireCost);
                    showingCrew.AddMember();
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
}
