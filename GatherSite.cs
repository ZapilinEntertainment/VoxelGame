using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherSite : MonoBehaviour {

	public int workersCount {get;private set;}
	float workflow;
	SurfaceBlock workObject;
	GameObject sign;

	void Awake() {workersCount = 0;}

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || workObject.surfaceObjects.Count == 0) {
			Destroy(this);
		}
		if (workersCount  > 0) {
			workflow += GameMaster.CalculateWorkflow(workersCount);
			if (workflow > 0) {
				
			}
		}
	}

	public void Set(SurfaceBlock block) {
		workObject = block;


	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign);
		if (workObject != null) workObject.cleanWorks = false;
	}

}
