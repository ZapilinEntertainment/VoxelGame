using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanSite : Worksite {
	bool diggingMission = false;
	SurfaceBlock workObject;
	const int START_WORKERS_COUNT = 10;

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null) {
			Destroy(this);
			return;
		}
		if (workObject.surfaceObjects.Count == 0) {
			workObject.myChunk.DeleteBlock(workObject.pos);
			if (diggingMission) {
				DigSite ds =  workObject.basement.gameObject.AddComponent<DigSite>();
				ds.Set(workObject.basement, true);
			}
			Destroy(this);
			return;
		}
		if (workersCount  > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if ( labourTimer <= 0 ) {
				if (workflow >= 1) LabourResult();
				labourTimer = GameMaster.LABOUR_TICK;
			}
		}
	}

	void LabourResult() {
		Structure s = workObject.surfaceObjects[0].structure;
		bool alreadyDestroyed = false;
		if (s == null) {workObject.RequestAnnihilationAtIndex(0);return;}
			Plant p = s.GetComponent<Plant>();
			if (p != null) {
				Plant2D p2d = s.GetComponent<Plant2D>();
				if (p2d != null) { workflow --;}
				else {
					Tree t = s.GetComponent<Tree>();
					if (t != null) {
						float lumberDelta= t.CalculateLumberCount(); 
						GameMaster.colonyController.storage.AddResources(ResourceType.Lumber, lumberDelta * 0.9f);
						workflow -= lumberDelta;
					}
				}
			p.Annihilate();
			alreadyDestroyed = true;
			}
			else {
				HarvestableResource hr = s.GetComponent<HarvestableResource>();
				if (hr != null) {
					GameMaster.colonyController.storage.AddResources(hr.mainResource, hr.count1);
					Destroy(hr.gameObject);
				}
				else {
					Building b = s.GetComponent<Building>();
					if (b != null) {
						b.hp -= workflow;
					}
				}
			}
		if (!alreadyDestroyed) Destroy(workObject.surfaceObjects[0].structure.gameObject);
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Clearing);
	}

	public void Set(SurfaceBlock block, bool f_diggingMission) {
		workObject = block;
		if (block.grassland != null) {Destroy(block.grassland);}
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/ClearSign")) as GameObject;
		sign.transform.position = workObject.transform.position;
		sign.GetComponent<WorksiteSign>().Set(this);
		diggingMission = f_diggingMission;
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
	}
			
}
