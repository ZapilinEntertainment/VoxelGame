using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumbermill : WorkBuilding {
	int lifepowerForSingleTree = 4;
	const int MAX_TREES = 16;
	float chopLimit = 0.2f;

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
		Plant[] saplingsAndTrees = new Plant[MAX_TREES];
		int firstEmptySaplingIndex = 0;
		int i = 0;
		while ( i < MAX_TREES && i < basement.surfaceObjects.Count) {
			if (basement.surfaceObjects[i]== null || !basement.surfaceObjects[i].gameObject.activeSelf) {basement.RequestAnnihilationAtIndex(i); i ++ ;continue;}
			if (basement.surfaceObjects[i].type != StructureType.Plant) {i++;continue;}
			else {
				Plant p = basement.surfaceObjects[i] as Plant;
				if ((p.plantType == PlantType.Tree | p.plantType == PlantType.TreeSapling) && p.enabled) {
					saplingsAndTrees[firstEmptySaplingIndex++] = p;
					if (firstEmptySaplingIndex >= MAX_TREES) break;
				}
			}
			i++;
		}
		
		if (firstEmptySaplingIndex < MAX_TREES ) {
					PixelPosByte newSaplingPos = basement.GetRandomCell();
					if (newSaplingPos != PixelPosByte.Empty) { 
					saplingsAndTrees[firstEmptySaplingIndex] = PoolMaster.current.GetSapling();
					saplingsAndTrees[firstEmptySaplingIndex].gameObject.SetActive(true);
					saplingsAndTrees[firstEmptySaplingIndex].SetBasement(basement, newSaplingPos);		
						firstEmptySaplingIndex++;
					}
		}
			i =0;
		for (;i < saplingsAndTrees.Length; i++) {
			if (saplingsAndTrees[i] == null) continue;
			if (saplingsAndTrees[i] is Tree) {
				Tree t = saplingsAndTrees[i] as Tree;
					if (t.growth >= chopLimit) {
						GameMaster.colonyController.storage.AddResources(ResourceType.Lumber, t.CalculateLumberCount());
						t.Chop();
					saplingsAndTrees[i] = null;
						i++;
						continue;
					}
					else {
					if ( !saplingsAndTrees[i].full ) saplingsAndTrees[i].AddLifepower(lifepowerForSingleTree);
					}
				}
				else {
					if ( !saplingsAndTrees[i].full ) saplingsAndTrees[i].AddLifepower(lifepowerForSingleTree/4);
				}
			}
		}
}
