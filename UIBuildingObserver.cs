using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingObserver : UIObserver {
	Building observingBuilding;
	bool status_connectedToPowerGrid = false, status_energySupplied = false, isHouse = false;
	float showingEnergySurplus = 0;
	int showingHousing = 0;
	public RawImage energyImage, housingImage; //fiti
	public Text energyValue, housingValue; // fiti


	public void SetObservingBuilding(Building b) {
		if (b == null) {
			SelfShutOff();
			return;
		}
		UIStructureObserver us = Structure.structureObserver;
		if (us == null) {
			us = Instantiate(Resources.Load<GameObject>("UIPrefs/structureObserver"), UIController.current.rightPanel.transform).GetComponent<UIStructureObserver>();
			Structure.structureObserver = us;
		}
		else us.gameObject.SetActive(true);
		observingBuilding = b; isObserving = true;
		us.SetObservingStructure(observingBuilding);
		status_connectedToPowerGrid = b.connectedToPowerGrid;
		if (status_connectedToPowerGrid) {
			showingEnergySurplus = b.energySurplus;
			status_energySupplied = b.energySupplied;
			if (status_energySupplied)	{
				if (showingEnergySurplus <=0 ) energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
				else energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
			}
			else energyValue.text = Localization.GetWord(LocalizationKey.Offline);
			energyValue.enabled = true;
			energyImage.enabled = true;
		}
		else {
			energyValue.enabled = false;
			energyImage.enabled = false;
		}
		if (b is House) {
			isHouse = true;
			showingHousing = (b as House).housing;
			housingValue.text = showingHousing.ToString();
			housingValue.enabled = true;
			housingImage.enabled = true;
		}
		else {
			isHouse = false;
			housingValue.enabled = false;
			housingImage.enabled = false;
		}
		STATUS_UPDATE_TIME = 1.2f; timer = STATUS_UPDATE_TIME;
	}

	override protected void StatusUpdate() {
		if ( !isObserving ) return;
		if (observingBuilding == null) SelfShutOff();
		else {
			if (status_connectedToPowerGrid != observingBuilding.connectedToPowerGrid) {
				status_connectedToPowerGrid = observingBuilding.connectedToPowerGrid;
				if (status_connectedToPowerGrid) {
					showingEnergySurplus = observingBuilding.energySurplus;
					status_energySupplied = observingBuilding.energySupplied;
					if (status_energySupplied)	{
						if (showingEnergySurplus <=0 ) energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
						else energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
					}
					else energyValue.text = Localization.GetWord(LocalizationKey.Offline);
					energyValue.enabled = true;
					energyImage.enabled = true;
				}
				else {
					energyValue.enabled = false;
					energyImage.enabled = false;
				}
			}
			else {				
				if (status_energySupplied != observingBuilding.energySupplied) {
					status_energySupplied = observingBuilding.energySupplied;
					if (status_energySupplied)	{			
						showingEnergySurplus = observingBuilding.energySurplus;
						if (showingEnergySurplus <=0 ) energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
						else energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
					}
					else energyValue.text = Localization.GetWord(LocalizationKey.Offline);
				}
				else {
					if (showingEnergySurplus != observingBuilding.energySurplus) {
						showingEnergySurplus = observingBuilding.energySurplus;
						if (showingEnergySurplus <=0 ) energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
						else energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
					}
				}
			}
			if (isHouse) {
				int h = (observingBuilding as House).housing;
				if (showingHousing != h ){
					showingHousing = h;
					housingValue.text = showingHousing.ToString();
				}
			}
		}
	}

	override public void SelfShutOff() {
		isObserving = false;
		Structure.structureObserver.SelfShutOff();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
		isObserving = false;
		observingBuilding = null;
		Structure.structureObserver.ShutOff();
		gameObject.SetActive(false);
	}
}
