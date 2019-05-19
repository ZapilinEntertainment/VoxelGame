using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStructureObserver : UIObserver {
    private Structure observingStructure;
#pragma warning disable 0649
    [SerializeField] private Text nameField, sizeField;
    [SerializeField] private Button demolishButton;
    [SerializeField] private GameObject specialButton;
#pragma warning restore 0649
    const int ROTATE_BUTTON_CHILDINDEX = 4;

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
            sizeField.text = s.surfaceRect.size.ToString() + " x " + s.surfaceRect.size.ToString();
            if (s.isArtificial & s.id != Structure.WIND_GENERATOR_1_ID)
            {
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX).gameObject.SetActive(true);
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX + 1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX).gameObject.SetActive(false);
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX + 1).gameObject.SetActive(false);
            }
            if (s.id == Structure.OBSERVATORY_ID)
            {
                specialButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.OpenMap);
                specialButton.SetActive(true);
            }
            else
            {
                if (specialButton.activeSelf) specialButton.SetActive(false);
            }
		}
	}

	override public void StatusUpdate() {
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
    public void SpecialButtonClick()
    {
        if (observingStructure == null) SelfShutOff();
        else
        {
            if (observingStructure.id == Structure.OBSERVATORY_ID)
            {
                GameMaster.realMaster.globalMap.ShowOnGUI();
            }
            else specialButton.SetActive(false);
        }
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
		observingStructure.Annihilate(true, true, false);
	}
    public void CheckName() {
        nameField.text = Localization.GetStructureName(observingStructure.id);
    }
}
