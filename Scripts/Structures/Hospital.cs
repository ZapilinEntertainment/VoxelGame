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

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        if (!GameMaster.loading) colony.hospitalCoverageRecalculationNeeded = true;
    }
    public void RecalculateCoverage()
    {
        float prevCoverage = coverage;
        coverage = STANDART_COVERAGE * level * ((float)workersCount / (float)maxWorkers);
        if (prevCoverage != coverage) colony.hospitalCoverageRecalculationNeeded = true;
    }

    public override bool ShowUIInfo()
    {
        return true;
    }
    public override string UI_GetInfo()
    {
        return Localization.GetWord(LocalizedWord.HospitalsCoverage) + ": "+ ((int)(colony.hospitals_coefficient * 100f)).ToString() +'%';
    }

    override public int AddWorkers(int x)
    {
        var w = base.AddWorkers(x);
        RecalculateCoverage();
        return w;
    }
    override public int FreeWorkers(int x)
    {
        var n = base.FreeWorkers(x);
        RecalculateCoverage();
        return n;
    }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingPlace(this);
        colony.observer?.ActivateHospitalPanel();
        showOnGUI = true;
        return workbuildingObserver;
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(order);
        if (order.doSpecialChecks) colony.DeleteHospital(this);
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
