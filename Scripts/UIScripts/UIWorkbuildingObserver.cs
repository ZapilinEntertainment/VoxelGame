using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIWorkbuildingObserver : UIObserver { // работает и на workbuilding, и на worksite
	 
	int showingWorkersCount, showingWorkersMaxCount;
    private bool workspeedStringEnabled = true, ignoreWorkersSlider = false;

    public ILabourable observingPlace { get; private set; }
#pragma warning disable 0649
    [SerializeField] private GameObject stopButton;
    [SerializeField] private Button minusAllButton, minusButton, plusButton, plusAllButton;
    [SerializeField] private Slider slider; // fiti
    [SerializeField] private Text workersCountField, workSpeedField, actionLabel;
#pragma warning restore 0649

    public static UIWorkbuildingObserver InitializeWorkbuildingObserverScript()
    {
        UIWorkbuildingObserver uwb = Instantiate(Resources.Load<GameObject>("UIPrefs/workBuildingObserver"), mycanvas.rightPanel.transform).GetComponent<UIWorkbuildingObserver>();
        WorkBuilding.workbuildingObserver = uwb;
        Worksite.observer = uwb;
        return uwb;
    }

	public void SetObservingPlace( ILabourable wb ) {
        if (wb == null) {
			SelfShutOff();
			return;
		}
        if (!wb.IsWorksite())
        {
            UIBuildingObserver ub = Building.buildingObserver;
            if (ub == null) ub = UIBuildingObserver.InitializeBuildingObserverScript();
            else ub.gameObject.SetActive(true);            
            ub.SetObservingBuilding(wb as WorkBuilding);
        }
        observingPlace = wb; isObserving = true;
        showingWorkersCount = wb.GetWorkersCount();
		showingWorkersMaxCount = wb.GetMaxWorkersCount();

        ignoreWorkersSlider = true;// иначе будет вызывать ивент
        slider.minValue = 0; 
		slider.maxValue = showingWorkersMaxCount;
        slider.value = showingWorkersCount;
        ignoreWorkersSlider = false;

        workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
        workSpeedField.text = observingPlace.UI_GetInfo();

		workspeedStringEnabled = observingPlace.ShowUIInfo();
        workSpeedField.enabled = workspeedStringEnabled;
        if (observingPlace.IsWorksite())
        {
            actionLabel.enabled = true;
            actionLabel.text = (observingPlace as Worksite).actionLabel;
            stopButton.SetActive(true);           
        }
        else
        {
            actionLabel.enabled = false;
            stopButton.SetActive(false);
        }
        StatusUpdate();
    }

	override public void StatusUpdate() {
		if ( !isObserving ) return;
        if (observingPlace == null)
        {
            SelfShutOff();
        }
        else
        {
            int wcount = observingPlace.GetWorkersCount();
            if (showingWorkersCount != wcount)
            {
                showingWorkersCount = wcount;
                workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
                ignoreWorkersSlider = true;
                slider.value = showingWorkersCount;
                ignoreWorkersSlider = false;
            }
            wcount = observingPlace.GetMaxWorkersCount();
            if (showingWorkersMaxCount != wcount)
            {
                showingWorkersMaxCount = wcount;
                ignoreWorkersSlider = true;
                slider.maxValue = showingWorkersMaxCount;
                ignoreWorkersSlider = false;
            }
            if (workspeedStringEnabled)
            {
                workSpeedField.text = observingPlace.UI_GetInfo();
                if (!workSpeedField.enabled) workSpeedField.enabled = true;
            }
            if (observingPlace.IsWorksite())
            {
                actionLabel.text = (observingPlace as Worksite).actionLabel;
            }
        }
    }

    override public void SelfShutOff()
    {
        isObserving = false;
        Building.buildingObserver?.SelfShutOff();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        if (observingPlace != null)
        {
            observingPlace.DisabledOnGUI();
            observingPlace = null;
        }
        Building.buildingObserver?.ShutOff();
        gameObject.SetActive(false);
    }

    public void PlusButton() {
        if (observingPlace == null)
        {
            SelfShutOff(); return;
        }
        else
        {
            if (!observingPlace.MaximumWorkersReached())
            {
                GameMaster.realMaster.colonyController.SendWorkers(1, observingPlace);
            }
            StatusUpdate();
        }
    }
	public void PlusAllButton() {
            if (observingPlace == null)
            {
                SelfShutOff(); return;
            }
            else
            {
                if (observingPlace.MaximumWorkersReached()) return;
                else
                {
                    GameMaster.realMaster.colonyController.SendWorkers(10, observingPlace);
                    StatusUpdate();
                }
            }
	}
	public void MinusButton() {
        if (observingPlace == null)
        {
            SelfShutOff(); return;
        }
        else
        {
            if (observingPlace.GetWorkersCount() != 0)
            {
                observingPlace.FreeWorkers(1);
                StatusUpdate();
            }
        }
    }
	public void MinusAllButton() {
        if (observingPlace == null)
        {
            SelfShutOff(); return;
        }
        else
        {
            if (observingPlace.GetWorkersCount() != 0)
            {
                observingPlace.FreeWorkers(10);
                StatusUpdate();
            }
        }
    }
    public void StopButton()
    {
        if (observingPlace != null && observingPlace.IsWorksite())
        {
            var ow = observingPlace as Worksite;
            ow.StopWork(true);
            observingPlace = null;
            SelfShutOff();            
        }
    }
    public void Slider_SetWorkersCount() {
        if (ignoreWorkersSlider) return;        
        int wcount = observingPlace.GetWorkersCount(), mwcount = observingPlace.GetMaxWorkersCount();
        int x = (int)slider.value;
        if (wcount == x) return;
        else
        {
            if (x > mwcount) x = mwcount;
            else
            {
                if (x < 0) x = 0;
            }
            if (x > wcount) GameMaster.realMaster.colonyController.SendWorkers(x - wcount, observingPlace);
            else observingPlace.FreeWorkers(wcount - x);
            StatusUpdate();
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
