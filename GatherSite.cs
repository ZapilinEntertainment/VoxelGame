using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherSite : Worksite {
	float destructionTimer;
	SurfaceBlock workObject;
	const int START_WORKERS_COUNT = 5;

	void Awake() {
		workersCount = 0;
		destructionTimer = GameMaster.LABOUR_TICK * 10;
	}

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || workObject.surfaceObjects.Count == 0) {
			Destroy(this);
		}
		if (workersCount  > 0) {
			workflow += GameMaster.CalculateWorkflow(workersCount, WorkType.Gathering);
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if ( labourTimer <= 0 ) {
				if (workflow >= 1) LabourResult();
				labourTimer = GameMaster.LABOUR_TICK;
			}
		}
			
		destructionTimer -= Time.deltaTime * GameMaster.gameSpeed; 
		if (destructionTimer <=0) Destroy(this);
	}

	void LabourResult() {
			int i = 0;
			bool resourcesFound = false;
			while (i < workObject.surfaceObjects.Count) {
				if (workObject.surfaceObjects[i].structure == null) { workObject.RequestAnnihilationAtIndex(i); continue;}
				Tree t = workObject.surfaceObjects[i].structure.GetComponent<Tree>();
				if ( t != null) {
					resourcesFound = true;
					if (t.hp < workflow) {
						workflow -= t.hp;
						GameMaster.colonyController.storage.AddResources(ResourceType.Lumber, t.CalculateLumberCount());
						t.Chop();
						i++;
						break;
					}
					else {i++; continue;}
				}
				else {
					HarvestableResource hr = workObject.surfaceObjects[i].structure.GetComponent<HarvestableResource>();
					if (hr == null) {i++; continue;}
					else {
						resourcesFound = true;
						if (workflow > hr.count1) {
							GameMaster.colonyController.storage.AddResources(hr.mainResource, hr.count1);
							workflow -= hr.count1;
							Destroy(hr.gameObject);
							break;
						}
						else {
							GameMaster.colonyController.storage.AddResources(hr.mainResource, hr.count1);
							hr.count1 -= Mathf.FloorToInt(workflow); workflow = 0;
							break;
						}
					}
				}
			}
			if (resourcesFound) destructionTimer = GameMaster.LABOUR_TICK * 10;
	}


	public void Set(SurfaceBlock block) {
		workObject = block;
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/GatherSign")) as GameObject;
		sign.transform.position = workObject.transform.position + Vector3.down /2f * Block.QUAD_SIZE;
		sign.GetComponent<WorksiteSign>().Set(this);
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
	}
}
