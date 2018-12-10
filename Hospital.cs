using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BirthrateMode {Normal, Improved, Lowered}
public class Hospital : WorkBuilding {
	public float coverage {get;private set;}
	public static float hospital_birthrate_coefficient = 1;
	public static  BirthrateMode birthrateMode{get; private set;}

	public const float loweredCoefficient = 0.5f, improvedCoefficient = 1.5f;
    const int STANDART_COVERAGE = 100;

	public static void ResetToDefaults_Static_Hospital() {
		SetBirthrateMode(0);
	}

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
        SetWorkbuildingData(b, pos);
        coverage = 0;
		colony.AddHospital(this);
	}

    override public void FreeWorkers(int x)
    {
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
        RecalculateWorkspeed();
        colony.RecalculateHospitals();
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter) {
		if (isActive == x) return;
		isActive = x;
		colony.RecalculateHospitals();
        if (connectedToPowerGrid & recalculateAfter) colony.RecalculatePowerGrid();
    }

    override protected void RecalculateWorkspeed()
    {
        float prevCoverage = coverage;
        coverage = STANDART_COVERAGE * ((float)workersCount / (float)maxWorkers);
        if (prevCoverage != coverage) colony.RecalculateHospitals();
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
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        UIController.current.ActivateHospitalPanel();
        showOnGUI = true;
        return workbuildingObserver;
    }
}
