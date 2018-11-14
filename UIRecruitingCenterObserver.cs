using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIRecruitingCenterObserver : UIObserver
{
    public enum RecruitingCenterObserverMode : byte { NoShowingCrew, HiringCrew, ShowingCrewInfo }
    public RecruitingCenter observingRCenter { get; private set; }
#pragma warning disable 0649
    [SerializeField] InputField crewNameTextField; // fiti
    [SerializeField] Button hireButton, dismissButton; // fiti
    [SerializeField] Text  crewSlotsText; // fiti
    [SerializeField] Dropdown crewListDropdown; // fiti
    [SerializeField] GameObject dropdownPseudoButton;
#pragma warning restore 0649
    public Crew showingCrew { get; private set; }
    private int showingCrewSlots, showingTotalCrewSlots, lastCrewsUpdate = -1;
    public RecruitingCenterObserverMode mode { get; private set; }

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

        PrepareCrewsWindow();

        STATUS_UPDATE_TIME = 1f; timer = STATUS_UPDATE_TIME;
    }

    public void PrepareCrewsWindow()
    {
        switch (mode)
        {
            case RecruitingCenterObserverMode.NoShowingCrew:
                if (UIController.current.progressPanelMode != ProgressPanelMode.Offline) UIController.current.DeactivateProgressPanel();
                if (Crew.crewSlotsFree == 0)
                {
                    hireButton.interactable = false;
                    hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoFreeSlots);
                }
                else
                {
                    hireButton.interactable = true;
                    hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost().ToString() + ')';
                }
                hireButton.gameObject.SetActive(true);
                dismissButton.gameObject.SetActive(false);
                crewNameTextField.gameObject.SetActive(false);
                dropdownPseudoButton.SetActive(Crew.freeCrewsList.Count > 0);
                break;
            case RecruitingCenterObserverMode.HiringCrew:
                UIController.current.ActivateProgressPanel(ProgressPanelMode.RecruitingCenter);
                hireButton.gameObject.SetActive(false);
                dismissButton.gameObject.SetActive(false);
                crewNameTextField.gameObject.SetActive(false);
                dropdownPseudoButton.SetActive(Crew.freeCrewsList.Count > 0);
                break;
            case RecruitingCenterObserverMode.ShowingCrewInfo:
                UIController.current.ActivateProgressPanel(ProgressPanelMode.RecruitingCenter);
                hireButton.gameObject.SetActive(false);
                dismissButton.gameObject.SetActive(true);
                crewNameTextField.text = showingCrew.name;
                crewNameTextField.gameObject.SetActive(true);
                dropdownPseudoButton.SetActive(true);
                break;
        }
        showingCrewSlots = Crew.crewSlotsFree;
        showingTotalCrewSlots = Crew.crewSlotsTotal;
        crewListDropdown.interactable = (Crew.freeCrewsList.Count > 0);
        crewSlotsText.text = Localization.GetPhrase(LocalizedPhrase.CrewSlots) + " : " + showingCrewSlots.ToString() + " / " + showingTotalCrewSlots.ToString();

        if (lastCrewsUpdate != Crew.totalOperations) PrepareCrewsDropdown();
    }

    override protected void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingRCenter == null) SelfShutOff();
        else
        {
            bool changeSlotsInfo = false;
            if (showingCrewSlots != Crew.crewSlotsFree) { showingCrewSlots = Crew.crewSlotsFree; changeSlotsInfo = true; }
            if (showingTotalCrewSlots != Crew.crewSlotsTotal) { showingTotalCrewSlots = Crew.crewSlotsTotal; changeSlotsInfo = true; }
            if (changeSlotsInfo)
            {
                crewSlotsText.text = Localization.GetPhrase(LocalizedPhrase.CrewSlots) + " : " + showingCrewSlots.ToString() + " / " + showingTotalCrewSlots.ToString();
            }
            crewListDropdown.interactable = (Crew.freeCrewsList.Count > 0);

            RecruitingCenterObserverMode newMode = RecruitingCenterObserverMode.NoShowingCrew;
            if (showingCrew != null)    newMode = RecruitingCenterObserverMode.ShowingCrewInfo;
            else
            {
                if (observingRCenter.finding) newMode = RecruitingCenterObserverMode.HiringCrew;
            }
            if (newMode != mode)
            {
                mode = newMode;
                PrepareCrewsWindow();
            }

            if (lastCrewsUpdate != Crew.totalOperations)
            {
                PrepareCrewsDropdown();
                dropdownPseudoButton.SetActive(Crew.freeCrewsList.Count > 0);
            }
        }
    }    

    public void PrepareCrewsDropdown()
    {
        List<Dropdown.OptionData> crewButtons = new List<Dropdown.OptionData>();
        crewButtons.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost() + ')'));
        var crews = Crew.freeCrewsList;
        if (crews.Count > 0)
        {
            for (int i = 0; i < crews.Count; i++)
            {
                crewButtons.Add(new Dropdown.OptionData('\"' + crews[i].name + '\"'));
            }
        }
        crewListDropdown.options = crewButtons;
        lastCrewsUpdate = Crew.totalOperations;
    }

    public void SelectCrew(int i)
    {
        if (i == 0) // hire button
        {
            if (observingRCenter.StartHiring())
            {
                showingCrew = null;
                mode = RecruitingCenterObserverMode.HiringCrew;
                PrepareCrewsWindow();
                
            }
        }
        else
        {
            i--;
            SelectCrew(Crew.freeCrewsList[i]);
        }
    }
    public void SelectCrew(Crew c)
    {
        showingCrew = c;
        mode = RecruitingCenterObserverMode.ShowingCrewInfo;
        PrepareCrewsWindow();
    }
    public void StartHiring()
    {
        if (observingRCenter != null) observingRCenter.StartHiring();
        mode = RecruitingCenterObserverMode.HiringCrew;
        PrepareCrewsWindow();
    }

    public void ChangeName()
    {
        showingCrew.name = crewNameTextField.text;
        PrepareCrewsDropdown();
    }
    public void Dismiss()
    {
        showingCrew.Dismiss();
        showingCrew = null;
        mode = RecruitingCenterObserverMode.NoShowingCrew;
        PrepareCrewsWindow();
    }


    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        if (mode != RecruitingCenterObserverMode.NoShowingCrew) UIController.current.DeactivateProgressPanel();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingRCenter = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        if (mode != RecruitingCenterObserverMode.NoShowingCrew) UIController.current.DeactivateProgressPanel();
        gameObject.SetActive(false);
    }

    public void LocalizeButtonTitles()
    {
        hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost() + ')';
        dismissButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);
    }
}
