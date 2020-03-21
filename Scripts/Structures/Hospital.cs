public class Hospital : WorkBuilding {
	public float coverage {get;private set;}	
    const int STANDART_COVERAGE = 1000;

	public override void SetBasement(Plane b, PixelPosByte pos) {		
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

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        colony.RecalculateHospitals();
    }

    override public void RecalculateWorkspeed()
    {
        float prevCoverage = coverage;
        coverage = STANDART_COVERAGE * ((float)workersCount / (float)maxWorkers);
        if (prevCoverage != coverage) colony.RecalculateHospitals();
        gearsDamage = GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT * workSpeed / 20f;
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface , returnResources, leaveRuins);
        colony.DeleteHospital(this);
        Destroy(gameObject);
    }
}
