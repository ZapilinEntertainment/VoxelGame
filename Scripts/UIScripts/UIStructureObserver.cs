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
        UIStructureObserver us = Instantiate(Resources.Load<GameObject>("UIPrefs/structureObserver"), mycanvas.rightPanel.transform).GetComponent<UIStructureObserver>();
        Structure.structureObserver = us;
        return us;
    }

	public void SetObservingStructure(Structure s) {
		if (s == null) {SelfShutOff();return;}
		else {
            if (observingStructure != null) observingStructure.DisabledOnGUI();
			observingStructure = s; isObserving = true;
            CheckName();
			demolishButton.gameObject.SetActive(!s.indestructible);
            sizeField.text = s.surfaceRect.size.ToString() + " x " + s.surfaceRect.size.ToString();
            if (s.isArtificial & s.ID != Structure.WIND_GENERATOR_1_ID)
            {
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX).gameObject.SetActive(true);
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX + 1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX).gameObject.SetActive(false);
                transform.GetChild(ROTATE_BUTTON_CHILDINDEX + 1).gameObject.SetActive(false);
            }
            if (s.ID == Structure.OBSERVATORY_ID | s.ID == Structure.SCIENCE_LAB_ID)
            {
                specialButton.transform.GetChild(0).GetComponent<Text>().text = (s.ID == Structure.OBSERVATORY_ID) ? Localization.GetPhrase(LocalizedPhrase.OpenMap) : Localization.GetPhrase(LocalizedPhrase.OpenResearchTab);
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
        if (observingStructure != null)
        {
            if (observingStructure is IPlanable && !(observingStructure is Hotel))
            {
                observingStructure.SetModelRotation(mycanvas.GetSelectedFaceIndex() + 11);
            }
            else
            {
                int r = observingStructure.modelRotation;
                if (observingStructure.rotate90only) r -= 2;
                else r--;
                observingStructure.SetModelRotation(r);
            }
        }
        else SelfShutOff();
    }
    public void RotateRight()
    {
        if (observingStructure != null)
        {
            if (observingStructure is IPlanable && !(observingStructure is Hotel))
            {
                observingStructure.SetModelRotation(mycanvas.GetSelectedFaceIndex() + 11);
            }
            else
            {
                int r = observingStructure.modelRotation;
                if (observingStructure.rotate90only) r += 2;
                else r++;
                observingStructure.SetModelRotation(r);
            }
        }
        else SelfShutOff();
    }
    public void SpecialButtonClick()
    {
        if (observingStructure == null) SelfShutOff();
        else
        {
            switch (observingStructure.ID)
            {
                case Structure.OBSERVATORY_ID: mycanvas.uicontroller.ChangeUIMode(UIMode.GlobalMap, true); break;
                case Structure.SCIENCE_LAB_ID: mycanvas.uicontroller.ChangeUIMode(UIMode.KnowledgeTab, true); break; 
                default: specialButton.SetActive(false);break;
            }
        }
    }

    override public void SelfShutOff() { // публичный, потому что на кнопках
        if (observingStructure != null) observingStructure.DisabledOnGUI();
        isObserving = false;
		mycanvas.SelectedObjectLost();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
        if (observingStructure != null)
        {
            observingStructure.DisabledOnGUI();
            observingStructure = null;
        }
		isObserving = false;
		gameObject.SetActive(false);
	}

	public void Demolish() {
		observingStructure.Annihilate(true, true, false);
        SelfShutOff();
	}
    public void CheckName() {
        if (observingStructure == null) SelfShutOff();
        else
        {
            if (observingStructure.ID == Structure.SETTLEMENT_CENTER_ID)
            {
                nameField.text = Localization.GetStructureName(observingStructure.ID) + " (" + (observingStructure as Settlement).level.ToString() + ')';
            }
            else nameField.text = Localization.GetStructureName(observingStructure.ID);
        }
    }
}
