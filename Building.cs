using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public float workflow_to_build = 60,  currentWorkflow = 0, workflow_to_result = 30;
	public bool completed {get; protected set;}
	public int maxWorkers = 8;
	public byte level {get;protected set;}
	ResourceContainer[] containingResources;
	int[] resourcesCost, resourcesContain;
	public int workersCount {get; protected set;}
	const float WORKFLOW_GAIN = 1;
	ColonyController colonyController;

	void Awake() {
		hp = maxHp;
		innerPosition = SurfaceRect.Empty;
		workersCount = 0;
		completed = false;
		if (containingResources.Length != 0) {
			resourcesCost = new int[containingResources.Length];
			resourcesContain = new int[containingResources.Length];
			float pc = currentWorkflow / workflow_to_build;
			if (pc == 1) completed = true;
			for (int i =0; i < resourcesContain.Length; i++) {
				resourcesContain[i] = (int)( resourcesCost[i] * pc);
			}
		}
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || colonyController == null) return;
		if (workersCount > 0) {
			float workflow = GameMaster.CalculateWorkflow(workersCount);
			if (GameMaster.LUCK_COEFFICIENT > 0) {if (Random.value > 0.5f) workflow += workflow * GameMaster.LUCK_COEFFICIENT;}
			else {if (Random.value < 0.1f) {workflow += workflow * GameMaster.LUCK_COEFFICIENT;}}
			currentWorkflow += workflow;
			if (completed) {
				if (currentWorkflow > workflow_to_result) {
					Result();
					currentWorkflow -= workflow_to_result;
				}
			}
			else {
				float pc = currentWorkflow / workflow_to_build;
				if (pc == 1) {completed = true; currentWorkflow = 0;}
				if (pc < 0) pc = 0;
				for (int i =0; i < resourcesContain.Length; i++) {
					resourcesContain[i] = (int)( resourcesCost[i] * pc);
				}
			}
		}
	}

	void Result() {
		
	}

	void StartBuilding() {
		currentWorkflow = 0;
	}

	protected override void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(innerPosition);
		}
		if (workersCount != 0) GameMaster.colonyController.AddWorkers(workersCount);
	}
}
