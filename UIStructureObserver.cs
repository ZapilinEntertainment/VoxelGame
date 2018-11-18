using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStructureObserver : UIObserver {
    Structure observingStructure;
#pragma warning disable 0649
    [SerializeField] Text nameField, sizeField;
    [SerializeField]  Button demolishButton;
#pragma warning restore 0649
    const int ROTATE_BUTTON_CHILDINDEX = 3;

    public static UIStructureObserver InitializeStructureObserverScript()
    {
        UIStructureObserver us = Instantiate(Resources.Load<GameObject>("UIPrefs/structureObserver"), UIController.current.rightPanel.transform).GetComponent<UIStructureObserver>();
        Structure.structureObserver = us;
        return us;
    }

	public void SetObservingStructure(Structure s) {
		if (s == null) {SelfShutOff();return;}
		else {
            if (observingStructure != null) observingStructure.DisableGUI();
			observingStructure = s; isObserving = true;
			nameField.text = Localization.GetStructureName(s.id);
			demolishButton.gameObject.SetActive(!s.indestructible);
            sizeField.text = s.innerPosition.size.ToString() + " x " + s.innerPosition.size.ToString();
            if (s.isArtificial)
            {
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX).gameObject.SetActive(true);
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX + 1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX).gameObject.SetActive(false);
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX + 1).gameObject.SetActive(false);
            }
		}
	}

	override protected void StatusUpdate() {
		if ( !isObserving ) return;
		if (observingStructure == null) SelfShutOff();
	}

    public void RotateLeft()
    {
        int r = observingStructure.modelRotation;
        if (observingStructure.rotate90only) r -= 2;
        else r--;
        observingStructure.SetModelRotation(r);
    }
    public void RotateRight()
    {
        int r = observingStructure.modelRotation;
        if (observingStructure.rotate90only) r += 2;
        else r++;
        observingStructure.SetModelRotation(r);
    }

    override public void SelfShutOff() {
        if (observingStructure != null) observingStructure.DisableGUI();
        isObserving = false;
		UIController.current.SelectedObjectLost();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
        if (observingStructure != null)
        {
            observingStructure.DisableGUI();
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
