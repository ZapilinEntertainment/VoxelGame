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

    public static UIStructureObserver InitializeStructureObserverScript()
    {
        UIStructureObserver us = Instantiate(Resources.Load<GameObject>("UIPrefs/structureObserver"), UIController.current.rightPanel.transform).GetComponent<UIStructureObserver>();
        Structure.structureObserver = us;
        return us;
    }

	public void SetObservingStructure(Structure s) {
		if (s == null) {SelfShutOff();return;}
		else {
            if (observingStructure != null) observingStructure.showOnGUI = false;
			observingStructure = s; isObserving = true; observingStructure.showOnGUI = true;
			nameField.text = Localization.GetStructureName(s.id);
			demolishButton.gameObject.SetActive(!s.indestructible);
            sizeField.text = s.innerPosition.size.ToString() + " x " + s.innerPosition.size.ToString();
		}
	}

	override protected void StatusUpdate() {
		if ( !isObserving ) return;
		if (observingStructure == null) SelfShutOff();
	}

	override public void SelfShutOff() {
        if (observingStructure != null) observingStructure.showOnGUI = false;
        isObserving = false;
		UIController.current.SelectedObjectLost();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
        if (observingStructure != null)
        {
            observingStructure.showOnGUI = false;
            observingStructure = null;
        }
		isObserving = false;
		gameObject.SetActive(false);
	}

	public void Demolish() {
		observingStructure.Annihilate(false);
	}
    public void CheckName() {
        nameField.text = Localization.GetStructureName(observingStructure.id);
    }
}
