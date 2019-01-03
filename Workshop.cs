using System.Collections.Generic;

public enum WorkshopMode : byte {NoActivity, GearsUpgrade}
//при добавлении вписать в UIController.LocalizeTitles

public sealed class Workshop : WorkBuilding {	
    public static Workshop current;

    public WorkshopMode mode { get; private set; }
    private const float GEARS_UPGRADE_SPEED = 0.0001f;

	override public void Prepare() {
		PrepareWorkbuilding();
		mode = WorkshopMode.NoActivity;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (current != null & current != this) current.Annihilate(false);
        current = this;
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

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(forced);
        if (current == this) current = null;
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }

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
    override public int Load(byte[] data, int startIndex, SurfaceBlock sblock)
    {
        startIndex = base.Load(data, startIndex, sblock);
        mode = (WorkshopMode)data[startIndex];
        return startIndex + 1;
    }

    #endregion
}
