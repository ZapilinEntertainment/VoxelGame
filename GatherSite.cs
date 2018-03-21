using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherSite : MonoBehaviour {

	public int workersCount {get;private set;}
	public float workflow, labourTimer = 0;
	SurfaceBlock workObject;
	GameObject sign;
	float destructionTimer;
	public const int MAX_WORKERS  = 32;

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
			if (labourTimer <= 0) {LabourResult();labourTimer = GameMaster.LABOUR_TICK;}
		}
			
		destructionTimer -= Time.deltaTime * GameMaster.gameSpeed; 
		if (destructionTimer <=0) Destroy(this);
	}

	void LabourResult() {
		if (workflow > 0) {
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
	}

	public void AddWorkers (int x) {
		if (x > 0) workersCount += x;
	}

	public void Set(SurfaceBlock block) {
		workObject = block;
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/GatherSign")) as GameObject;
		sign.transform.parent = workObject.transform;
		sign.transform.localPosition = Vector3.down * Block.QUAD_SIZE/2f;
		GameMaster.colonyController.gatherSites.Add(this);
	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign);
	}

}
