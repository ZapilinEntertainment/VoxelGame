using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hospital : House {
	[SerializeField]
	int _coverage = 100;
	public int coverage {get;private set;}

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		coverage = _coverage;
		GameMaster.colonyController.AddHospital(this);
	}

	override public void SetActivationStatus(bool x) {
		if (isActive == x) return;
		isActive = x;
		GameMaster.colonyController.RecalculateHousing();
		GameMaster.colonyController.RecalculateHospitals();
	}
}
