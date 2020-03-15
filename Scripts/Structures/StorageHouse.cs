
public class StorageHouse : Building {
	public float volume { get; private set; }

    override public void Prepare() {
        PrepareBuilding();
        switch (ID)
        {
            case STORAGE_0_ID: volume = GetMaxVolume(0);break;
            case STORAGE_1_ID: volume = GetMaxVolume(1); break;
            case STORAGE_2_ID: volume = GetMaxVolume(2); break;
            case STORAGE_BLOCK_ID: volume = GetMaxVolume(5); break;
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

    override public void SetBasement(Plane b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.storage.AddWarehouse(this);
        //copy to StorageBlock.cs
        isEnergySupplied = true;
	}


    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) basement = null;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        GameMaster.realMaster.colonyController.storage.RemoveWarehouse(this);
        Destroy(gameObject);
    }
}
