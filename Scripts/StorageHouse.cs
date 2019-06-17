
public sealed class StorageHouse : Building {
	public float volume { get; private set; }

    override public void Prepare() {
        PrepareBuilding();
        switch (id)
        {
            case STORAGE_0_ID: volume = GetMaxVolume(0);break;
            case STORAGE_1_ID: volume = GetMaxVolume(1); break;
            case STORAGE_2_ID: volume = GetMaxVolume(2); break;
            case STORAGE_5_ID: volume = GetMaxVolume(5); break;
        }
    }

    public static float GetMaxVolume (int level)
    {
        switch (level)
        {
            case 0: return 10000;
            case 1: return 15000;
            case 2: return 25000;
            case 3: return 50000;
            case 5: return 200000;
            default: return 1000;
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.storage.AddWarehouse(this);
	}

	override public void SetActivationStatus(bool x, bool recalculateAfter) {
		isActive = x;
        if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { UnsetBasement(); }
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        GameMaster.realMaster.colonyController.storage.RemoveWarehouse(this);
        Destroy(gameObject);
    }
}
