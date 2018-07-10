using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkbuildingObserver : UIObserver {
	public Button minusAllButton, minusButton, plusButton, plusAllButton; // fiti
	public Slider slider; // fiti
	public Text workersCountField, workSpeedField; // fiti
	int showingWorkersCount, showingWorkersMaxCount;
	float showingWorkspeed;

	WorkBuilding observingWorkbuilding;

	public void SetObservingWorkBuilding( WorkBuilding wb ) {
		if (wb == null) {
			SelfShutOff();
			return;
		}
		UIBuildingObserver ub = Building.buildingObserver;
		if (ub== null) {
			ub = Instantiate(Resources.Load<GameObject>("UIPrefs/buildingObserver"), UIController.current.rightPanel.transform).GetComponent<UIBuildingObserver>();
			Building.buildingObserver = ub;
		}
		else ub.gameObject.SetActive(true);
		observingWorkbuilding = wb; isObserving = true;
		ub.SetObservingBuilding(observingWorkbuilding);

		showingWorkersCount = wb.workersCount;
		showingWorkersMaxCount = wb.maxWorkers;
		showingWorkspeed = wb.workSpeed;

		slider.value = showingWorkersCount;
		slider.minValue = 0; 
		slider.maxValue = showingWorkersMaxCount;
		workersCountField.text = showingWorkersCount.ToString() + '/' + showingWorkersMaxCount.ToString();
		workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetWord(LocalizationKey.PointsSec);

		if (showingWorkersCount > 0) {
			minusAllButton.enabled = true;
			minusButton.enabled = true;
		}
		else {
			minusAllButton.enabled = false;
			minusButton.enabled = false;
		}
		workSpeedField.enabled = (showingWorkspeed > 0);

		STATUS_UPDATE_TIME = 0.5f; timer = STATUS_UPDATE_TIME;
	}

	override protected void StatusUpdate() {
		if ( !isObserving ) return;
		if (observingWorkbuilding == null) SelfShutOff();
		else {
			bool changes = false;
			if (showingWorkersCount != observingWorkbuilding.workersCount) {
				showingWorkersCount = observingWorkbuilding.workersCount;
                workersCountField.text = showingWorkersCount.ToString();
                slider.value = showingWorkersCount;
				changes = true;
			}
			if (showingWorkersMaxCount != observingWorkbuilding.maxWorkers) {
				showingWorkersMaxCount = observingWorkbuilding.maxWorkers;
				slider.maxValue = showingWorkersMaxCount;
				changes = true;
			}
			if (changes) {
				if (showingWorkersCount == showingWorkersMaxCount) {
					plusAllButton.enabled = false;
					plusButton.enabled = false;
				}
				else {
					plusAllButton.enabled = true;
					plusButton.enabled = true;
				}
				if (showingWorkersCount == 0) {
					minusAllButton.enabled = false;
					minusAllButton.enabled = false;
				}
				else {
					minusAllButton.enabled = true;
					minusButton.enabled = true;
				}
			}
			if (showingWorkspeed != observingWorkbuilding.workSpeed) {
				showingWorkspeed = observingWorkbuilding.workSpeed;
				if (showingWorkspeed == 0) workSpeedField.enabled = false;
				else {
					workSpeedField.text = string.Format("{0:0.00}", showingWorkspeed) + ' ' + Localization.GetWord(LocalizationKey.PointsSec);
					workSpeedField.enabled = true;
				}
			}
		}
	}

	public void PlusButton() {
		if (observingWorkbuilding == null) {
			SelfShutOff(); return;
		}
		else {
			if (observingWorkbuilding.workersCount < observingWorkbuilding.maxWorkers) {
				GameMaster.colonyController.SendWorkers(1,observingWorkbuilding,WorkersDestination.ForWorkBuilding);
			}
			timer = STATUS_UPDATE_TIME;
			StatusUpdate();
		}
	}
	public void PlusAllButton() {
		if (observingWorkbuilding == null) {
			SelfShutOff(); return;
		}
		else {
			if (observingWorkbuilding.workersCount < observingWorkbuilding.maxWorkers) {
				GameMaster.colonyController.SendWorkers(observingWorkbuilding.maxWorkers - observingWorkbuilding.workersCount ,observingWorkbuilding,WorkersDestination.ForWorkBuilding);
			}
			timer = STATUS_UPDATE_TIME;
			StatusUpdate();
		}
	}
	public void MinusButton() {
		if (observingWorkbuilding == null) {
			SelfShutOff(); return;
		}
		else {
			if (observingWorkbuilding.workersCount > 0) {
				observingWorkbuilding.FreeWorkers(1);
			}
			timer = STATUS_UPDATE_TIME;
			StatusUpdate();
		}
	}
	public void MinusAllButton() {
		if (observingWorkbuilding == null) {
			SelfShutOff(); return;
		}
		else {
			if (observingWorkbuilding.workersCount > 0) {
				observingWorkbuilding.FreeWorkers();
			}
			timer = STATUS_UPDATE_TIME;
			StatusUpdate();
		}
	}
    public void Slider_SetWorkersCount() {        
        int x = (int)slider.value;
        if (observingWorkbuilding.workersCount == x) return;
        else {
            if (x > observingWorkbuilding.maxWorkers) x = observingWorkbuilding.maxWorkers;
            else {
                if (x < 0) x = 0;
            }
            if (x > observingWorkbuilding.workersCount) GameMaster.colonyController.SendWorkers(x - observingWorkbuilding.workersCount, observingWorkbuilding, WorkersDestination.ForWorkBuilding);
            else observingWorkbuilding.FreeWorkers(observingWorkbuilding.workersCount - x);
            showingWorkersCount = observingWorkbuilding.workersCount;
            workersCountField.text = showingWorkersCount.ToString();
        }
    }


	override public void SelfShutOff() {
		isObserving = false;
		Building.buildingObserver.SelfShutOff();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
		isObserving = false;
		observingWorkbuilding = null;
		Building.buildingObserver.ShutOff();
		gameObject.SetActive(false);
	}
}
