using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding {
	[SerializeField]
	float farmFertility = 1;
	int lifepowerToEveryCrop = 2;
	Plant[] crops;
	[SerializeField]
	Plant crop_pref;
	int MAX_CROPS = 256;


	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		b.ClearSurface();
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
			List<PixelPosByte> cropsPositions = basement.GetRandomCells(MAX_CROPS);
			if (cropsPositions.Count > 0) {
				crops = new Plant[cropsPositions.Count];
				for (int i =0; i< crops.Length; i++) {
					crops[i] = Instantiate(crop_pref); // заменить на пуллинг
					crops[i].gameObject.SetActive(true);
					crops[i].SetBasement(basement, cropsPositions[i]);
				}
			}
		}
		else {
			if (crops[0].growth >=1) { // harvesting
				float harvest = 0;
				for (int i = 0; i < crops.Length; i++) {
					if (crops[i] == null) {
						PixelPosByte ppos = basement.GetRandomCell();
						if (ppos != PixelPosByte.Empty) {
							crops[i] = Instantiate(crop_pref);
							crops[i].gameObject.SetActive(true);
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
