using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphoniumEnricher : WorkBuilding {

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		GameMaster.colonyController.AddGraphoniumEnricher(this);
	}

	void OnDestroy() {
		GameMaster.colonyController.RemoveGraphoniumEnricher(this);
		PrepareBuildingForDestruction();
	}
}
