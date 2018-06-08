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
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
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
				if (workObject.surfaceObjects[i]== null) { workObject.RequestAnnihilationAtIndex(i); continue;}
				Tree t = workObject.surfaceObjects[i].GetComponent<Tree>();
				if ( t != null && t.enabled) {
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
						HarvestableResource hr = workObject.surfaceObjects[i].GetComponent<HarvestableResource>();
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

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Gathering);
	}
	public void Set(SurfaceBlock block) {
		workObject = block;
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/GatherSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.transform.position + Vector3.down /2f * Block.QUAD_SIZE;
		actionLabel = Localization.ui_gather_in_progress;
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
	}
}
