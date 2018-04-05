using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumbermill : WorkBuilding {
	int lifepowerForSingleTree = 16;
	const int MAX_TREES = 16;
	float chopLimit = 0.5f;

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (workersCount > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed * GameMaster.lifeGrowCoefficient;
			if (workflow >= workflowToProcess) {
				LabourResult();
				workflow -= workflowToProcess;
			}
		}
	}	


		override protected void LabourResult() {
		Plant[] saplings = new Plant[MAX_TREES];
		int firstEmptySaplingIndex = 0;
		int i = 0;
		for (; i < basement.surfaceObjects.Count; i ++) {
			if (basement.surfaceObjects[i].structure == null) continue;
			Plant p = basement.surfaceObjects[i].structure as Plant;
			if ( p == null || p.gameObject.activeSelf == false) continue;
			else {
				if (p.plantType == PlantType.Tree || p.plantType == PlantType.TreeSapling) {
					saplings[firstEmptySaplingIndex++] = p;
					if (firstEmptySaplingIndex >= MAX_TREES) break;
				}
			}
		}
		
		if (firstEmptySaplingIndex < MAX_TREES ) {
					PixelPosByte newSaplingPos = basement.GetRandomCell();
					if (newSaplingPos != PixelPosByte.Empty) { 
						saplings[firstEmptySaplingIndex] = PoolMaster.current.GetGrass().GetComponent<Plant2D>();
						saplings[firstEmptySaplingIndex].gameObject.SetActive(true);
						saplings[firstEmptySaplingIndex].SetBasement(basement, newSaplingPos);		
						firstEmptySaplingIndex++;
					}
		}
			i =0;
			while ( i < firstEmptySaplingIndex) {
				if (saplings[i] is Tree) {
					Tree t = saplings[i] as Tree;
					if (t.growth >= chopLimit) {
						GameMaster.colonyController.storage.AddResources(ResourceType.Lumber, t.CalculateLumberCount());
						t.Chop();
						saplings[i] = null;
						i++;
						continue;
					}
					else {
						if ( !saplings[i].full ) saplings[i].AddLifepower(lifepowerForSingleTree);
					}
				}
				else {
					if ( !saplings[i].full ) saplings[i].AddLifepower(lifepowerForSingleTree/4);
					else {
						if (saplings[i].growth >= 1) {
							Tree t = PoolMaster.current.GetTree().GetComponent<Tree>();
							if (basement.ReplaceStructure(new SurfaceObject(saplings[i].innerPosition, t))) {
								t.SetLifepower( saplings[i].lifepower);
								saplings[i]= t;
							}
							else {
								print ("replacing with tree failed : lumbermill");
								Destroy(t.gameObject);
							}
						}
					}
				}
				i++;
			}
		}
}
