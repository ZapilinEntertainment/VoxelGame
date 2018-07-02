using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStructureObserver : UIObserver {
	bool isObserving = false;
	Structure observingStructure;
	public Text nameField;
	public Button demolishButton;

	virtual public void SetObservingStructure(Structure s) {
		if (s == null) return;
		else {
			observingStructure = s; isObserving = true;
			nameField.text = Localization.GetName(s.id);
			demolishButton.gameObject.SetActive(!s.undestructible);
		}
	}

	public void Update() {
		if ( !isObserving ) return;
		if (observingStructure == null) {
			isObserving = false;
			UIController.current.SelectedObjectLost();
		}
		else {
			
		}
	}

	override public void ShutOff() {
		observingStructure = null;
		isObserving = false;
		gameObject.SetActive(false);
	}

	virtual public void Demolish() {
		observingStructure.Annihilate(false);
	}
}
