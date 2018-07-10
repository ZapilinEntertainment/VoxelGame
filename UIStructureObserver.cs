using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStructureObserver : UIObserver {
    Structure observingStructure;
    [SerializeField] 
	Text nameField, sizeField;
    [SerializeField]
    Button demolishButton;


	public void SetObservingStructure(Structure s) {
		if (s == null) {SelfShutOff();return;}
		else {
            if (observingStructure != null) observingStructure.showOnGUI = false;
			observingStructure = s; isObserving = true; observingStructure.showOnGUI = true;
			nameField.text = s.name;
			demolishButton.gameObject.SetActive(!s.undestructible);
            sizeField.text = s.innerPosition.x_size.ToString() + " x " + s.innerPosition.z_size.ToString();
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
        if (observingStructure != null) observingStructure.showOnGUI = false;
        observingStructure = null;
		isObserving = false;
		gameObject.SetActive(false);
	}

	public void Demolish() {
		observingStructure.Annihilate(false);
	}
    public void CheckName() {
        nameField.text = observingStructure.name;
    }
}
