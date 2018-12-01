
public sealed class StorageHouse : Building {
	public float volume { get; private set; }

    override public void Prepare() {
        PrepareBuilding();
        switch (id)
        {
            case STORAGE_0_ID: volume = 10000;break;
            case STORAGE_1_ID: volume = 5000; break;
            case STORAGE_2_ID: volume = 20000; break;
            case STORAGE_3_ID: volume = 50000; break;
            case STORAGE_5_ID: volume = 20000;break;
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		GameMaster.colonyController.storage.AddWarehouse(this);
	}

	override public void SetActivationStatus(bool x) {
		isActive = x;
	}

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        if (PrepareBuildingForDestruction(forced))
        {
            GameMaster.colonyController.storage.RemoveWarehouse(this);
        }
        Destroy(gameObject);
    }
}
