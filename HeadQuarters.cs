using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadQuarters : House {
	
	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		SetBuildingData(b,pos);
		GameMaster.colonyController.AddHousing(this);
		GameMaster.colonyController.SetHQ(this);
	}
}
