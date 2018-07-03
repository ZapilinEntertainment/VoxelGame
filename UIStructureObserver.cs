using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStructureObserver : UIObserver {
	Structure observingStructure;
	public Text nameField;
	public Button demolishButton;

	public void SetObservingStructure(Structure s) {
		if (s == null) {SelfShutOff();return;}
		else {
			observingStructure = s; isObserving = true;
			nameField.text = s.name;
			demolishButton.gameObject.SetActive(!s.undestructible);
		}
	}

	override protected void StatusUpdate() {
		if ( !isObserving ) return;
		if (observingStructure == null) SelfShutOff();
	}

	override public void SelfShutOff() {
		isObserving = false;
		UIController.current.SelectedObjectLost();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
		observingStructure = null;
		isObserving = false;
		gameObject.SetActive(false);
	}

	public void Demolish() {
		observingStructure.Annihilate(false);
	}
}
