using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingShop : Building {

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		GameMaster.colonyController.AddRollingShop(this);
	}

	void OnDestroy() {
		GameMaster.colonyController.RemoveRollingShop(this);
		PrepareBuildingForDestruction();
	}
}
