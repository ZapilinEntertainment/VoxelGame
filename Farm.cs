using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding {
	protected float farmFertility = 1;
	public int crop_id = -1;

	override public void Prepare() {
		PrepareWorkbuilding();
		if (crop_id == -1) crop_id = Plant.CROP_CORN_ID;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		b.ClearSurface();
		SetBuildingData(b, pos);
		b.ReplaceMaterial(ResourceType.FERTILE_SOIL_ID);
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
		int i = 0, totalCost = 0;
		float lifepowerCost = 0;
		List<Structure> structures = basement.surfaceObjects;
		Chunk c = basement.myChunk;
		if (basement.cellsStatus != 0) {	
			while (i < structures.Count) {
				if (structures[i] == null ) {
					basement.RequestAnnihilationAtIndex(i);
					continue;
				}
				else {
					if (structures[i].id == Structure.PLANT_ID) {
						Plant p = structures[i] as Plant;
						if (p.plant_ID == crop_id) {
							if (p.stage >= p.harvestableStage & p.growth >= 1) 	p.Harvest();
							else {
								if (p.lifepower < p.lifepowerToGrow) 	{
									p.AddLifepower(p.maxLifeTransfer);
									totalCost += p.maxLifeTransfer;
								}
							}
						}
					}
					i++;
				}
			}
		}
		if (basement.cellsStatus != 1) {
			PixelPosByte pos = basement.GetRandomCell();
			int cost = Plant.GetCreateCost(crop_id);
			if (pos != PixelPosByte.Empty & c.lifePower > cost ) {
				Plant p = Plant.GetNewPlant(crop_id);
				p.Prepare();
				p.SetBasement(basement, pos);
				c.TakeLifePowerWithForce(cost);
				totalCost += cost;
			}
		}
		if (totalCost > 0) c.TakeLifePowerWithForce(totalCost);
	}
	void OnDestroy() {
		if (basement != null) {
			if (basement.material_id == ResourceType.FERTILE_SOIL_ID) basement.ReplaceMaterial(ResourceType.DIRT_ID);
		}
		PrepareWorkbuildingForDestruction();
	}
}
