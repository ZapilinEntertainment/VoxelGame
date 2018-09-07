using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageHouse : Building {
	public float volume = 1000; // fixed by asset

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
    }
}
