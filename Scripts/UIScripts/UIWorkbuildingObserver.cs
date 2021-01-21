﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIWorkbuildingObserver : UIObserver { // работает и на workbuilding, и на worksite
	 
	int showingWorkersCount, showingWorkersMaxCount;
	float showingWorkspeed;
    private bool workbuildingMode = true, workspeedStringEnabled = true, ignoreWorkersSlider = false;

	public WorkBuilding observingWorkbuilding { get; private set; }
    public Worksite observingWorksite { get; private set; }
#pragma warning disable 0649
    [SerializeField] private GameObject stopButton;
    [SerializeField] private Button minusAllButton, minusButton, plusButton, plusAllButton;
    [SerializeField] private Slider slider; // fiti
    [SerializeField] private Text workersCountField, workSpeedField, actionLabel;
#pragma warning restore 0649

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
		showingWorkspeed = wb.GetWorkSpeed();

        ignoreWorkersSlider = true;// иначе будет вызывать ивент
        slider.minValue = 0; 
		slider.maxValue = showingWorkersMaxCount;
        slider.value = showingWorkersCount;
        ignoreWorkersSlider = false;

        workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
		workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);

		workspeedStringEnabled = (showingWorkspeed > 0 & observingWorkbuilding.ShowWorkspeed());
        workSpeedField.enabled = workspeedStringEnabled;
        actionLabel.enabled = false;        
        stopButton.SetActive(false);
	}

    public void SetObservingWorksite(Worksite ws)
    {
        workbuildingMode = false;
        if (ws == null)
        {
            SelfShutOff();
            return;
        }
        observingWorksite = ws; isObserving = true;

        showingWorkersCount = ws.workersCount;
        showingWorkersMaxCount = ws.GetMaxWorkers();
        showingWorkspeed = ws.GetWorkSpeed();

        ignoreWorkersSlider = true;// иначе будет вызывать ивент
        slider.value = showingWorkersCount; 
        slider.minValue = 0;
        slider.maxValue = showingWorkersMaxCount;
        ignoreWorkersSlider = false;

        workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
        workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);
        workSpeedField.enabled = (showingWorkspeed > 0 );
        actionLabel.enabled = true;
        actionLabel.text = ws.actionLabel;        
        stopButton.SetActive(true);
        StatusUpdate();
    }

	override public void StatusUpdate() {
		if ( !isObserving ) return;
        if (workbuildingMode)
        { // WORKBUILDING
            if (observingWorkbuilding == null)
            {
                SelfShutOff();                
            }
            else
            {
                if (showingWorkersCount != observingWorkbuilding.workersCount)
                {
                    showingWorkersCount = observingWorkbuilding.workersCount;
                    workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
                    ignoreWorkersSlider = true;
                    slider.value = showingWorkersCount;
                    ignoreWorkersSlider = false;
                }
                if (showingWorkersMaxCount != observingWorkbuilding.maxWorkers)
                {
                    showingWorkersMaxCount = observingWorkbuilding.maxWorkers;
                    ignoreWorkersSlider = true;
                    slider.maxValue = showingWorkersMaxCount;
                    ignoreWorkersSlider = false;
                }
                if (workspeedStringEnabled)
                {
                        showingWorkspeed = observingWorkbuilding.GetWorkSpeed();
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
            if (observingWorksite == null)
            {
                SelfShutOff();                
            }
            else
            {
                if (showingWorkersCount != observingWorksite.workersCount)
                {
                    showingWorkersCount = observingWorksite.workersCount;
                    workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
                    ignoreWorkersSlider = true;
                    slider.value = showingWorkersCount;
                    ignoreWorkersSlider = false;
                }
                int maxWorkers = observingWorksite.GetMaxWorkers();
                if (showingWorkersMaxCount != maxWorkers)
                {
                    showingWorkersMaxCount = maxWorkers;
                    ignoreWorkersSlider = true;
                    slider.maxValue = showingWorkersMaxCount;
                    ignoreWorkersSlider = false;
                }
                if (workspeedStringEnabled )
                {
                    showingWorkspeed = observingWorksite.GetWorkSpeed();
                    if (showingWorkspeed == 0) workSpeedField.enabled = false;
                    else
                    {
                        workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetPhrase(LocalizedPhrase.PointsSec);
                        workSpeedField.enabled = true;
                    }
                }
                actionLabel.text = observingWorksite.actionLabel;
            }
        }
	}

    override public void SelfShutOff()
    {
        isObserving = false;
        if (workbuildingMode)
        {
            Building.buildingObserver.SelfShutOff();
            if (UIController.current.progressPanelMode != ProgressPanelMode.Offline) UIController.current.DeactivateProgressPanel(ProgressPanelMode.Powerplant);
        }
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        if (observingWorksite != null)
        {
            observingWorksite.showOnGUI = false;
            observingWorkbuilding = null;
        }
        if (workbuildingMode)
        {
            Building.buildingObserver.ShutOff();
            observingWorkbuilding = null;
            if (UIController.current.progressPanelMode != ProgressPanelMode.Offline) UIController.current.DeactivateProgressPanel(ProgressPanelMode.Powerplant);
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
                    GameMaster.realMaster.colonyController.SendWorkers(1, observingWorkbuilding);
                }
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
                if (observingWorksite.workersCount < observingWorksite.GetMaxWorkers())
                {
                    GameMaster.realMaster.colonyController.SendWorkers(1, observingWorksite);
                }
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
                int wcount = observingWorkbuilding.workersCount, max = observingWorkbuilding.maxWorkers;
                if (wcount == max) return;
                else
                {
                    if (wcount + 10 < max)
                    {
                        GameMaster.realMaster.colonyController.SendWorkers(10, observingWorkbuilding);
                    }
                    else
                    {
                        GameMaster.realMaster.colonyController.SendWorkers(max - wcount, observingWorkbuilding);
                    }
                    StatusUpdate();
                }
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
                int wcount = observingWorksite.workersCount;
                int max = observingWorksite.GetMaxWorkers();
                if (wcount == max) return;
                else
                {
                    if (wcount + 10 < max)
                    {
                        GameMaster.realMaster.colonyController.SendWorkers(10, observingWorksite);
                    }
                    else GameMaster.realMaster.colonyController.SendWorkers(max - wcount, observingWorksite);
                    StatusUpdate();
                }
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
                int wcount = observingWorkbuilding.workersCount;
                if (wcount == 0) return;
                if (wcount > 10) observingWorkbuilding.FreeWorkers(10);
                else observingWorkbuilding.FreeWorkers();
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
                int wcount = observingWorksite.workersCount;
                if (wcount == 0) return;
                else
                {
                    if (wcount > 10) observingWorksite.FreeWorkers(10);
                    else observingWorkbuilding.FreeWorkers();
                    StatusUpdate();
                }
            }
        }
	}
    public void StopButton()
    {
        if (observingWorksite != null)
        {
            observingWorksite.StopWork(true);
            observingWorksite = null;
            SelfShutOff();            
        }
    }
    public void Slider_SetWorkersCount() {
        if (ignoreWorkersSlider) return;
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
                if (x > observingWorkbuilding.workersCount) GameMaster.realMaster.colonyController.SendWorkers(x - observingWorkbuilding.workersCount, observingWorkbuilding);
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
                int maxWorkers = observingWorksite.GetMaxWorkers();
                if (x > maxWorkers) x = maxWorkers;
                else
                {
                    if (x < 0) x = 0;
                }
                if (x > observingWorksite.workersCount) GameMaster.realMaster.colonyController.SendWorkers(x - observingWorksite.workersCount, observingWorksite);
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

    public override void LocalizeTitles()
    {
        stopButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Stop);
    }
}
