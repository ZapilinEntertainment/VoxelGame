using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkBuilding : Building {
	public float workflow = 0, workflowToProcess = 1;
	public int maxWorkers = 8;
	public int workersCount {get; protected set;}
	const float WORKFLOW_GAIN = 1;

	void Awake() {
		PrepareWorkbuilding();
	}
	protected void PrepareWorkbuilding() {
		PrepareBuilding();
		workersCount = 0;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive) return;
		if (workersCount > 0) {
			workflow += GameMaster.CalculateWorkflow(workersCount, WorkType.Manufacturing);
			if (workflow >= workflowToProcess) {
				LabourResult();
				workflow -= workflowToProcess;
			}
		}
	}

	protected virtual void LabourResult() {

	}

	public void AddWorkers (int x) {
		if (x > 0) workersCount += x;
	}

	protected void PrepareWorkbuildingForDestruction() {
		PrepareBuildingForDestruction();
		if (workersCount != 0) GameMaster.colonyController.AddWorkers(workersCount);
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
	}
}
