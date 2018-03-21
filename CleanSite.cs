using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanSite : MonoBehaviour {
	public const int MAX_WORKERS = 32;
	public int workersCount {get;private set;}
	float workflow;
	SurfaceBlock workObject;
	GameObject sign;
	float labourTimer = 0;
	bool diggingMission = false;

	void Awake() {workersCount = 0;}

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
			workflow += GameMaster.CalculateWorkflow(workersCount, WorkType.Clearing);
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (labourTimer <= 0) {
				LabourResult();
				labourTimer = GameMaster.LABOUR_TICK;
			}
		}
	}

	void LabourResult() {
		if (workflow > 0) {
			Structure s = workObject.surfaceObjects[0].structure;
			Plant p = s.GetComponent<Plant>();
			if (p != null) {
				Plant2D p2d = s.GetComponent<Plant2D>();
				if (p2d != null) {workflow --;}
				else {
					Tree t = s.GetComponent<Tree>();
					if (t != null) {
						float lumberDelta= t.CalculateLumberCount(); 
						GameMaster.colonyController.storage.AddResources(ResourceType.Lumber, lumberDelta * 0.9f);
						workflow -= lumberDelta;
					}
				}
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
			Destroy(workObject.surfaceObjects[0].structure.gameObject);
		}
	}

	public void Set(SurfaceBlock block, bool f_diggingMission) {
		workObject = block;
		workObject.cleanWorks = true;
		if (block.grassland != null) {Destroy(block.grassland);}
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/ClearSign")) as GameObject;
		sign.transform.parent = workObject.transform;
		sign.transform.localPosition = Vector3.down * Block.QUAD_SIZE/2f;
		GameMaster.colonyController.cleanSites.Add(this);
		diggingMission = f_diggingMission;
	}

	public void AddWorkers (int x) {
		if (x > 0) workersCount += x;
	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign);
		if (workObject != null) workObject.cleanWorks = false;
	}
		
}
