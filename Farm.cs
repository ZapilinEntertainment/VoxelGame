using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding {
	float farmFertility = 1;
	int lifepowerToEveryCrop = 2;
	Plant[] crops;
	[SerializeField]
	Plant crop_pref;
	int MAX_CROPS = 256;
	float growth = 0;


	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		b.ReplaceMaterial(ResourceType.FERTILE_SOIL_ID);
		MAX_CROPS = SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION - innerPosition.x_size * innerPosition.z_size;
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Farming);
		workflowToProcess = workflowToProcess_setValue *( 10 - (float)workersCount / (float)maxWorkers * 9);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 ) return;
		if ( isActive && energySupplied && workersCount > 0) {
			workflow += workSpeed * Time.deltaTime  * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
				workflow -= workflowToProcess;
			}
		}
	}

	override protected void LabourResult() {
		if (crops == null) {
			int count = 0;
			foreach (bool x in basement.map) {
				if (x) count ++;
				if (count >= MAX_CROPS) break;
			}
			crops = new Plant[count];
			for (int i =0; i< count; i++) {
				crops[i] = Instantiate(crop_pref); // заменить на пуллинг
				crops[i].gameObject.SetActive(true);
			}
			basement.AddMultipleCellObjects(crops);
		}
		else {
			if (crops[0].growth >=1) { // harvesting
				float harvest = 0;
				for (int i = 0; i < crops.Length; i++) {
					if (crops[i] == null) {
						PixelPosByte ppos = basement.GetRandomCell();
						if (ppos != PixelPosByte.Empty) {
							crops[i] = Instantiate(crop_pref);
							crops[i].SetBasement(basement, ppos);
						}
					}
					else {
						harvest += farmFertility;
						crops[i].SetLifepower(0);
					}
				}
				if (harvest != 0) GameMaster.colonyController.storage.AddResources(new ResourceContainer(ResourceType.Food, harvest));
			}
			else {
				foreach (Plant p in crops) {
					if ( p == null) continue;
					p.AddLifepower(lifepowerToEveryCrop);
				}
			}
		}
	}

}
