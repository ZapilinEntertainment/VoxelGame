using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building {
	public int housing = 2;
	public float birthrate = 0.01f;

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b,pos);
		GameMaster.colonyController.AddHousing(this);
	}

	override public void SetActivationStatus(bool x) {
		if (isActive == x) return;
		ChangeBuildingActivity(x);
		GameMaster.colonyController.RecalculateHousing();
	}

	 void OnDestroy() {
		PrepareBuildingForDestruction();
		GameMaster.colonyController.DeleteHousing(this);
	}
}
