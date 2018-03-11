using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanSite : MonoBehaviour {

	public int workersCount {get;private set;}
	float workflow;
	SurfaceBlock workObject;
	GameObject sign;


	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null) {
			Destroy(this);
		}
		if (workObject.surfaceObjects.Count == 0) {
			Destroy(workObject);
			Destroy(this);
		}
		workflow += GameMaster.CalculateWorkflow(workersCount);
		if (workflow > 0) {
			Structure s = workObject.surfaceObjects[0].myGameObject.GetComponent<Structure>();
			Plant p = s.GetComponent<Plant>();
			if (p != null) {
				Plant2D p2d = s.GetComponent<Plant2D>();
				if (p2d != null) {workflow --;}
				else {
					Tree t = s.GetComponent<Tree>();
					if (t != null) {
						float lumberDelta= 500 * t.transform.localScale.y; 
						GameMaster.colonyController.storage.AddResources(ResourceType.Lumber, lumberDelta);
						workflow -= lumberDelta;
					}
				}
			}
			else {
				HarvestableResource hr = s.GetComponent<HarvestableResource>();
				GameMaster.colonyController.storage.AddResources(hr.mainResource, hr.count1);
				Destroy(hr.gameObject);
			}
			Destroy(workObject.surfaceObjects[0].myGameObject);
		}
	}

	public void Set(SurfaceBlock block) {
		workObject = block;
		workObject.cleanWorks = true;
		if (block.grassland != null) {Destroy(block.grassland);}
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/ClearSign")) as GameObject;
		sign.transform.parent = workObject.transform;
		sign.transform.localPosition = Vector3.up * 0.5f;
	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign);
		if (workObject != null) workObject.cleanWorks = false;
	}
		
}
