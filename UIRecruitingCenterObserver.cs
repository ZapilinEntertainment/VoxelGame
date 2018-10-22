using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRecruitingCenterObserver : UIObserver {
    RecruitingCenter observingRCenter;
    [SerializeField] RawImage mainCrewIcon; // fiti
    [SerializeField] InputField crewNameTextField; // fiti
    [SerializeField] Button hireButton, dismissButton; // fiti
    [SerializeField] Text crewStatusText, crewSlotsText; // fiti
    [SerializeField] RectTransform progressBar;//fiti
    [SerializeField] Dropdown crewListDropdown; // fiti
    Crew showingCrew;
    int savedProgressBarValue = 100;
    float fullProgressBarLength = -1, startOffset = 0;

    public static UIRecruitingCenterObserver InitializeRCenterObserverScript()
    {
        UIRecruitingCenterObserver urco = Instantiate(Resources.Load<GameObject>("UIPrefs/recruitingCenterObserver"), UIController.current.rightPanel.transform).GetComponent<UIRecruitingCenterObserver>();
        RecruitingCenter.rcenterObserver = urco;
        return urco;
    }

    public void SetObservingRCenter(RecruitingCenter rc)
    {
        if (fullProgressBarLength == -1)
        {
            fullProgressBarLength = progressBar.rect.width;
            startOffset = progressBar.offsetMin.x;
        }
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

    override protected void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingRCenter == null) SelfShutOff();
        else
        {
            if (showingCrew == null)
            {
                if (savedProgressBarValue != (int)(observingRCenter.progress * 100))
                {
                    savedProgressBarValue = (int)(observingRCenter.progress* 100);
                    crewStatusText.text = savedProgressBarValue.ToString() + '%';
                    progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset,  savedProgressBarValue / 100f * fullProgressBarLength);
                }
            }
            else
            {
                if (savedProgressBarValue != (int)(showingCrew.stamina * 100))
                {
                    savedProgressBarValue = (int)(showingCrew.stamina * 100);
                    crewStatusText.text = savedProgressBarValue.ToString() + '%';
                    progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset,  savedProgressBarValue / 100f * fullProgressBarLength);
                }
            }
        }
    }

    public void PrepareCrewsWindow()
    {
        showingCrew = null;
        crewNameTextField.gameObject.SetActive(false);
        hireButton.gameObject.SetActive(true);
        hireButton.GetComponent<Image>().overrideSprite = (observingRCenter.finding) ? PoolMaster.gui_overridingSprite : null;
        mainCrewIcon.enabled = false;
        if ( Crew.crewsList.Count == 0 )
        {
            dismissButton.gameObject.SetActive(false);              
            crewListDropdown.interactable = false;
        }
        else
        {
            dismissButton.gameObject.SetActive(true);                         
            crewListDropdown.interactable = true;
        }
        crewSlotsText.text = Localization.GetPhrase(LocalizedPhrase.CrewSlots) + " : " + Crew.crewSlots.ToString();
        savedProgressBarValue = (int)(observingRCenter.progress * 100);
        progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset,  savedProgressBarValue / 100f * fullProgressBarLength);
        crewStatusText.text = savedProgressBarValue.ToString() + '%';
    }

    public void PrepareCrewsDropdown()
    {
        List<Dropdown.OptionData> crewButtons = new List<Dropdown.OptionData>();
        crewButtons.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost() + ')'));
        var crews = Crew.crewsList;
        if ( crews.Count > 0)
        {
            for (int i = 0; i < crews.Count; i++)
            {
                crewButtons.Add(new Dropdown.OptionData('\"' + crews[i].name + '\"'));
            }
        }
        crewListDropdown.options = crewButtons;
    }

    public void SelectCrew(int i)
    {
        if (i == 0) // hire button
        {
            PrepareCrewsWindow();
        }
        else
        {            
            i--;
            SelectCrew(Crew.crewsList[i]);            
        }
    }

    public void SelectCrew(Crew c)
    {
        showingCrew = c;        
        hireButton.gameObject.SetActive(false);
        crewListDropdown.interactable = true;
        crewSlotsText.text = Localization.GetPhrase(LocalizedPhrase.CrewSlots) + " : " + Crew.crewSlots.ToString();
        crewNameTextField.gameObject.SetActive(true);
        crewNameTextField.text = '\"' + showingCrew.name + '\"';

        mainCrewIcon.enabled = true;
        showingCrew.DrawCrewIcon(mainCrewIcon);

        savedProgressBarValue = (int)(showingCrew.stamina * 100);
        progressBar.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset, savedProgressBarValue / 100f * fullProgressBarLength);
        crewStatusText.text = savedProgressBarValue.ToString() + '%';
    }

    public void StartHiring()
    {
        if (observingRCenter.finding)
        {
            observingRCenter.finding = false;
            hireButton.GetComponent<Image>().overrideSprite = null;
            return;
        }
        else
        {
            if (Crew.crewSlots > 0)
            {
                if (GameMaster.colonyController.energyCrystalsCount >= RecruitingCenter.GetHireCost())
                {
                    GameMaster.colonyController.GetEnergyCrystals(RecruitingCenter.GetHireCost());
                    observingRCenter.finding = true;
                    hireButton.gameObject.SetActive(true);
                    hireButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                }
                else
                {
                    UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughEnergyCrystals));
                    hireButton.GetComponent<Image>().overrideSprite = null;
                }
            }
            else
            {
                UIController.current.MakeAnnouncement(Localization.GetRefusalReason(RefusalReason.NotEnoughSlots));
            }
        }
    }

    public void ChangeName()
    {
        showingCrew.name = crewNameTextField.text;
    }
    public void Dismiss()
    {
        showingCrew.Dismiss();
        PrepareCrewsWindow();
    }


    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingRCenter = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        gameObject.SetActive(false);
    }

    public void LocalizeButtonTitles()
    {
        hireButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.HireNewCrew) + " (" + RecruitingCenter.GetHireCost() + ')';
        dismissButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);
    }
}
