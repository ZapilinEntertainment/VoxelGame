using System.Collections.Generic;

public enum WorkshopMode : byte {NoActivity, GearsUpgrade}
//при добавлении вписать в UIController.LocalizeTitles

public sealed class Workshop : WorkBuilding {	

    public WorkshopMode mode { get; private set; }
    private const float GEARS_UPGRADE_SPEED = 0.0001f;

	override public void Prepare() {
		PrepareWorkbuilding();
		mode = WorkshopMode.NoActivity;
	}

    override public void LabourUpdate()
    {
        if (isActive & isEnergySupplied)
        {
            switch (mode) {
                case WorkshopMode.GearsUpgrade:
                    if (colony.gears_coefficient < GameConstants.GEARS_UP_LIMIT)
                    {
                        colony.gears_coefficient += workSpeed * GEARS_UPGRADE_SPEED;
                    }
                    break;
        }
        }
    }

    override protected void LabourResult() {
	}
    public void SetMode(byte x)
    {
       mode = (WorkshopMode)x;
    }
    public void SetMode (WorkshopMode rsm) { mode = rsm; }	

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        UIController.current.ActivateWorkshopPanel();
        return workbuildingObserver;
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.Add((byte)mode);
        return data;
    }
    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        base.Load(fs, sblock);
        mode = (WorkshopMode)fs.ReadByte();
    }

    #endregion
}
