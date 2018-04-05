using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building {
	public int housing = 2;

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		SetBuildingData(b,pos);
		GameMaster.colonyController.AddHousing(this);
	}

	override public void SetActivationStatus(bool x) {
		if (isActive == x) return;
		isActive = x;
		GameMaster.colonyController.RecalculateHousing();
	}



	 void OnDestroy() {
		PrepareBuildingForDestruction();
		GameMaster.colonyController.DeleteHousing(this);
	}
}
