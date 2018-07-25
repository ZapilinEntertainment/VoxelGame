using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BirthrateMode {Normal, Improved, Lowered}
public class Hospital : WorkBuilding {
	[SerializeField]
	int _coverage = 100;
	public int coverage {get;private set;}
	public static float hospital_birthrate_coefficient = 1;
	public static  BirthrateMode birthrateMode{get; private set;}
	public const float loweredCoefficient = 0.5f, improvedCoefficient = 1.5f;

	public static void Reset() {
		SetBirthrateMode(0);
	}

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
        SetBuildingData(b, pos);
        coverage = _coverage;
		GameMaster.colonyController.AddHospital(this);
	}

    void Update()
    {
        return;
    }

    override public int AddWorkers(int x)
    {
        if (workersCount == maxWorkers) return 0;
        else
        {
            if (x > maxWorkers - workersCount)
            {
                x -= (maxWorkers - workersCount);
                workersCount = maxWorkers;
            }
            else
            {
                workersCount += x;
            }
            coverage = (int)(_coverage * ((float)workersCount / (float)maxWorkers));
            GameMaster.colonyController.RecalculateHospitals();
            return x;
        }
    }
    override public void FreeWorkers(int x)
    {
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        GameMaster.colonyController.AddWorkers(x);
        coverage = (int)(_coverage * ((float)workersCount / (float)maxWorkers));
        GameMaster.colonyController.RecalculateHospitals();
    }

    override public void SetActivationStatus(bool x) {
		if (isActive == x) return;
		isActive = x;
		GameMaster.colonyController.RecalculateHospitals();
	}

	public static void SetBirthrateMode(int x) {
		switch (x) {
		case 0: birthrateMode = BirthrateMode.Normal; hospital_birthrate_coefficient = 1; break;
		case 1: birthrateMode = BirthrateMode.Improved; hospital_birthrate_coefficient = improvedCoefficient; break;
		case 2: birthrateMode = BirthrateMode.Lowered; hospital_birthrate_coefficient = loweredCoefficient; break;
		}
	}
	public static int GetBirthrateModeIndex() {
		switch (birthrateMode) {
		case BirthrateMode.Normal: return 0;
		case BirthrateMode.Improved: return 1;
		case BirthrateMode.Lowered: return 2;
		default: return 0;
		}
	}

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = Instantiate(Resources.Load<GameObject>("UIPrefs/workbuildingObserver"), UIController.current.rightPanel.transform).GetComponent<UIWorkbuildingObserver>();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        UIController.current.ActivateHospitalPanel();
        showOnGUI = true;
        return workbuildingObserver;
    }
}
