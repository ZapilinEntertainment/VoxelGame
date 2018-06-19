using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding {
	protected float farmFertility = 1;
	int lifepowerToEveryCrop = 2;
	int MAX_CROPS = 256;
	List<Plant> unusedCrops;

	override public void Prepare() {
		PrepareWorkbuilding();
		unusedCrops = new List<Plant>();
	}

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
		int i = 0;
		if (basement.cellsStatus != 0) {			
			float harvest = 0;
			while ( i < basement.surfaceObjects.Count ) {
				if ( basement.surfaceObjects[i] == null ) {
					basement.RequestAnnihilationAtIndex(i);
					continue;
				}
				else {
					Plant p = basement.surfaceObjects[i] as Plant;
					if ( p != null && p.plantType == PlantType.Crop  ) {
						if ( !p.full ) 	p.AddLifepower( lifepowerToEveryCrop );
						else {
							if (p.growth >= 1) {
								harvest += farmFertility;
								p.Annihilate(false);
							}
						}
					}
				}
				i++;
			}
			if ( harvest > 0 ) {
				GameMaster.colonyController.storage.AddResource(ResourceType.Food, harvest);
			}
		}
		if (i < MAX_CROPS) {
			List<PixelPosByte> positions = basement.GetRandomCells(i);
			i = positions.Count - 1;
			while ( i >= 0 ) {
				Structure s = null;
				if ( unusedCrops.Count > 0 ) {
					s = unusedCrops[0];
					s.gameObject.SetActive(true);
					unusedCrops.RemoveAt(0);
				}
				else s = Structure.GetNewStructure(Structure.WHEAT_CROP_ID);
				s.SetBasement(basement, positions[i]);
				(s as Plant).AddLifepower(lifepowerToEveryCrop);
				i--;
			}
		}
	}

	public void ReturnCropToPool (Plant c) {
		if (c != null ) {
			c.SetLifepower(0);
			c.SetGrowth(0);
			c.gameObject.SetActive(false);
			unusedCrops.Add(c);
		}
	}

}
