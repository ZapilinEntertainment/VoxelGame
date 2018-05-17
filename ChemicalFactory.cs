using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChemicalFactory : WorkBuilding {
	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		GameMaster.colonyController.AddChemicalFactory(this);
	}

	void OnDestroy() {
		GameMaster.colonyController.RemoveChemicalFactory(this);
		PrepareBuildingForDestruction();
	}
}
