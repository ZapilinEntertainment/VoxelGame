using System.IO;

public class Hospital : WorkBuilding {
	public float coverage {get;private set;}	
    const int STANDART_COVERAGE = 500;

    override public bool IsLevelUpPossible(ref string refusalReason)
    {
        if (GameMaster.realMaster.colonyController.hq.level >= 5) return true;
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.Unavailable);
            return false;
        }
    }
    public override void SetBasement(Plane b, PixelPosByte pos) {		
		if (b == null) return;
        SetWorkbuildingData(b, pos);        
        if (!GameMaster.loading)
        {
            RecalculateCoverage();
            colony.AddHospital(this);
        }
	}
    public override float GetWorkSpeed()
    {
        return coverage;
    }

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        if (!GameMaster.loading) colony.RecalculateHospitals();
    }
    private void RecalculateCoverage()
    {
        float prevCoverage = coverage;
        coverage = STANDART_COVERAGE * level * ((float)workersCount / (float)maxWorkers);
        if (prevCoverage != coverage) colony.RecalculateHospitals();
    }
    override public int AddWorkers(int x)
    {
        var w = base.AddWorkers(x);
        RecalculateCoverage();
        return w;
    }
    override public void FreeWorkers(int x)
    {
        base.FreeWorkers(x);
        RecalculateCoverage();
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

    #region save-load
    public override void Load(FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        RecalculateCoverage();
        colony.AddHospital(this);
    }
    #endregion
}
