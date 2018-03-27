using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageHouse : Building {
	public float volume = 1000;

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		GameMaster.colonyController.storage.AddWarehouse(this);
	}

	override public void SetActivationStatus(bool x) {
		isActive = x;
	}

	void OnDestroy() {
		PrepareBuildingForDestruction();
		if (basement != null) {
			GameMaster.colonyController.storage.RemoveWarehouse(this);
		}
	}
}
