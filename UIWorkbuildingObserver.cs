using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIWorkbuildingObserver : UIObserver {
	public Button minusAllButton, minusButton, plusButton, plusAllButton; // fiti
	public Slider slider; // fiti
    public Text workersCountField, workSpeedField, actionLabel; // fiti
	int showingWorkersCount, showingWorkersMaxCount;
	float showingWorkspeed;
    bool workbuildingMode = true;

	public WorkBuilding observingWorkbuilding { get; private set; }
    public Worksite observingWorksite { get; private set; }
    [SerializeField] GameObject stopButton;

    public static UIWorkbuildingObserver InitializeWorkbuildingObserverScript()
    {
        UIWorkbuildingObserver uwb = Instantiate(Resources.Load<GameObject>("UIPrefs/workBuildingObserver"), UIController.current.rightPanel.transform).GetComponent<UIWorkbuildingObserver>();
        WorkBuilding.workbuildingObserver = uwb;
        Worksite.observer = uwb;
        return uwb;
    }

	public void SetObservingWorkBuilding( WorkBuilding wb ) {
        workbuildingMode = true;
        if (wb == null) {
			SelfShutOff();
			return;
		}
		UIBuildingObserver ub = Building.buildingObserver;
        if (ub == null) ub = UIBuildingObserver.InitializeBuildingObserverScript();
        else ub.gameObject.SetActive(true);
		observingWorkbuilding = wb; isObserving = true;
		ub.SetObservingBuilding(observingWorkbuilding);

		showingWorkersCount = wb.workersCount;
		showingWorkersMaxCount = wb.maxWorkers;
		showingWorkspeed = wb.workSpeed;
		
		slider.minValue = 0; 
		slider.maxValue = showingWorkersMaxCount;
        slider.value = showingWorkersCount;
        workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
		workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);

		if (showingWorkersCount > 0) {
			minusAllButton.enabled = true;
			minusButton.enabled = true;
		}
		else {
			minusAllButton.enabled = false;
			minusButton.enabled = false;
		}
		workSpeedField.enabled = (showingWorkspeed > 0);
        actionLabel.enabled = false;        
        stopButton.SetActive(false);

		STATUS_UPDATE_TIME = 0.5f; timer = STATUS_UPDATE_TIME;
	}

    public void SetWorksiteObserver(Worksite ws)
    {
        workbuildingMode = false;
        if (ws == null)
        {
            SelfShutOff();
            return;
        }
        print("here");
        observingWorksite = ws; isObserving = true;

        showingWorkersCount = ws.workersCount;
        showingWorkersMaxCount = ws.maxWorkers;
        showingWorkspeed = ws.workSpeed;

        slider.minValue = 0;
        slider.maxValue = showingWorkersMaxCount;
        slider.value = showingWorkersCount;
        workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
        workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);

        if (showingWorkersCount > 0)
        {
            minusAllButton.enabled = true;
            minusButton.enabled = true;
        }
        else
        {
            minusAllButton.enabled = false;
            minusButton.enabled = false;
        }
        workSpeedField.enabled = (showingWorkspeed > 0);
        actionLabel.enabled = true;
        actionLabel.text = ws.actionLabel;        
        stopButton.SetActive(true);

        STATUS_UPDATE_TIME = 0.5f; timer = STATUS_UPDATE_TIME;
    }

	override protected void StatusUpdate() {
		if ( !isObserving ) return;
        if (workbuildingMode)
        { // WORKBUILDING
            if (observingWorkbuilding == null) SelfShutOff();
            else
            {
                bool changes = false;
                if (showingWorkersCount != observingWorkbuilding.workersCount)
                {
                    showingWorkersCount = observingWorkbuilding.workersCount;
                    workersCountField.text = showingWorkersCount.ToString();
                    slider.value = showingWorkersCount;
                    changes = true;
                }
                if (showingWorkersMaxCount != observingWorkbuilding.maxWorkers)
                {
                    showingWorkersMaxCount = observingWorkbuilding.maxWorkers;
                    slider.maxValue = showingWorkersMaxCount;
                    changes = true;
                }
                if (changes)
                {
                    if (showingWorkersCount == showingWorkersMaxCount)
                    {
                        plusAllButton.enabled = false;
                        plusButton.enabled = false;
                    }
                    else
                    {
                        plusAllButton.enabled = true;
                        plusButton.enabled = true;
                    }
                    if (showingWorkersCount == 0)
                    {
                        minusAllButton.enabled = false;
                        minusAllButton.enabled = false;
                    }
                    else
                    {
                        minusAllButton.enabled = true;
                        minusButton.enabled = true;
                    }
                }
                if (showingWorkspeed != observingWorkbuilding.workSpeed)
                {
                    showingWorkspeed = observingWorkbuilding.workSpeed;
                    if (showingWorkspeed == 0) workSpeedField.enabled = false;
                    else
                    {
                        workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);
                        workSpeedField.enabled = true;
                    }
                }
            }
        }
        else
        {// WORKSITE
            if (observingWorksite == null) SelfShutOff();
            else
            {
                bool changes = false;
                if (showingWorkersCount != observingWorksite.workersCount)
                {
                    showingWorkersCount = observingWorksite.workersCount;
                    workersCountField.text = showingWorkersCount.ToString();
                    slider.value = showingWorkersCount;
                    changes = true;
                }
                if (showingWorkersMaxCount != observingWorksite.maxWorkers)
                {
                    showingWorkersMaxCount = observingWorksite.maxWorkers;
                    slider.maxValue = showingWorkersMaxCount;
                    changes = true;
                }
                if (changes)
                {
                    if (showingWorkersCount == showingWorkersMaxCount)
                    {
                        plusAllButton.enabled = false;
                        plusButton.enabled = false;
                    }
                    else
                    {
                        plusAllButton.enabled = true;
                        plusButton.enabled = true;
                    }
                    if (showingWorkersCount == 0)
                    {
                        minusAllButton.enabled = false;
                        minusAllButton.enabled = false;
                    }
                    else
                    {
                        minusAllButton.enabled = true;
                        minusButton.enabled = true;
                    }
                }
                if (showingWorkspeed != observingWorksite.workSpeed)
                {
                    showingWorkspeed = observingWorksite.workSpeed;
                    if (showingWorkspeed == 0) workSpeedField.enabled = false;
                    else
                    {
                        workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);
                        workSpeedField.enabled = true;
                    }
                }
            }
        }
	}

    override public void SelfShutOff()
    {
        isObserving = false;
        if (workbuildingMode) Building.buildingObserver.SelfShutOff();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingWorkbuilding = null;
        if (workbuildingMode)
        {
            Building.buildingObserver.ShutOff();
            observingWorkbuilding = null;
        }
        else observingWorksite = null;
        gameObject.SetActive(false);
    }

    public void PlusButton() {
        if (workbuildingMode)
        {   // WORKBUILDING
            if (observingWorkbuilding == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorkbuilding.workersCount < observingWorkbuilding.maxWorkers)
                {
                    GameMaster.colonyController.SendWorkers(1, observingWorkbuilding, WorkersDestination.ForWorkBuilding);
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
        else
        {   //WORKSITE
            if (observingWorksite == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorksite.workersCount < observingWorksite.maxWorkers)
                {
                    GameMaster.colonyController.SendWorkers(1, observingWorksite, WorkersDestination.ForWorksite);
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
	}
	public void PlusAllButton() {
        if (workbuildingMode)
        { // WORKBUILDING
            if (observingWorkbuilding == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorkbuilding.workersCount < observingWorkbuilding.maxWorkers)
                {
                    GameMaster.colonyController.SendWorkers(observingWorkbuilding.maxWorkers - observingWorkbuilding.workersCount, observingWorkbuilding, WorkersDestination.ForWorkBuilding);
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
        else
        { //WORKSITE
            if (observingWorksite == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorksite.workersCount < observingWorksite.maxWorkers)
                {
                    GameMaster.colonyController.SendWorkers(observingWorksite.maxWorkers - observingWorksite.workersCount, observingWorksite, WorkersDestination.ForWorksite);
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
	}
	public void MinusButton() {
        if (workbuildingMode)
        { // WORKBUILDING
            if (observingWorkbuilding == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorkbuilding.workersCount > 0)
                {
                    observingWorkbuilding.FreeWorkers(1);
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
        else
        {// WORKSITE
            if (observingWorksite == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorksite.workersCount > 0)
                {
                    observingWorksite.FreeWorkers(1);
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
	}
	public void MinusAllButton() {
        if (workbuildingMode)
        { //WORKBUILDING
            if (observingWorkbuilding == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorkbuilding.workersCount > 0)
                {
                    observingWorkbuilding.FreeWorkers();
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
        else
        { //WORKSITE
            if (observingWorksite == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingWorksite.workersCount > 0)
                {
                    observingWorksite.FreeWorkers();
                }
                timer = STATUS_UPDATE_TIME;
                StatusUpdate();
            }
        }
	}
    public void StopButton()
    {
        if (observingWorksite != null)
        {
            observingWorksite.StopWork();
            observingWorksite = null;
            SelfShutOff();
        }
    }
    public void Slider_SetWorkersCount() {        
        int x = (int)slider.value;
        if (workbuildingMode)
        {
            if (observingWorkbuilding.workersCount == x) return;
            else
            {
                if (x > observingWorkbuilding.maxWorkers) x = observingWorkbuilding.maxWorkers;
                else
                {
                    if (x < 0) x = 0;
                }
                if (x > observingWorkbuilding.workersCount) GameMaster.colonyController.SendWorkers(x - observingWorkbuilding.workersCount, observingWorkbuilding, WorkersDestination.ForWorkBuilding);
                else observingWorkbuilding.FreeWorkers(observingWorkbuilding.workersCount - x);
                showingWorkersCount = observingWorkbuilding.workersCount;
                workersCountField.text = showingWorkersCount.ToString();
            }
        }
        else
        {
            if (observingWorksite.workersCount == x) return;
            else
            {
                if (x > observingWorksite.maxWorkers) x = observingWorksite.maxWorkers;
                else
                {
                    if (x < 0) x = 0;
                }
                if (x > observingWorksite.workersCount) GameMaster.colonyController.SendWorkers(x - observingWorksite.workersCount, observingWorksite, WorkersDestination.ForWorksite);
                else observingWorksite.FreeWorkers(observingWorksite.workersCount - x);
                showingWorkersCount = observingWorksite.workersCount;
                workersCountField.text = showingWorkersCount.ToString();
            }
        }
    }
    public void SetActionLabel(string s)
    {
        actionLabel.text = s;
        actionLabel.enabled = true;
    }
}
