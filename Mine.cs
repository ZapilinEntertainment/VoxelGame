using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : WorkBuilding {
	CubeBlock workObject;
	public float criticalVolume = 0.5f;
	public bool horizontal = true;
	bool workFinished = false;
	public byte oriented = 6; // 6 - all 

	void Awake() {
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null)  {
			Destroy(gameObject);
			return;
		}
		float pc = workObject.volume/CubeBlock.MAX_VOLUME;
		if (pc > criticalVolume) {
			if (workersCount > 0) {
				workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed;
			}
		}
		else {
			if (!workFinished) {
				workFinished = true;
			}
		}
	}

	override protected void LabourResult() {
		if (workflow > 1) {
			int x = (int) workflow;
			float production = x;
			production = workObject.Dig(x, true);
			GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
			workflow -= production;	
		}
	}
}
