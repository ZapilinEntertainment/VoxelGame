using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkBuilding : Building {
	public float workflow {get;protected set;} 
	protected float workSpeed = 0;
	public float workflowToProcess{get; protected set;}
	public int maxWorkers = 8;
	public int workersCount {get; protected set;}
	const float WORKFLOW_GAIN = 1;
	public float workflowToProcess_setValue = 1;

	void Awake() {
		PrepareWorkbuilding();
	}
	protected void PrepareWorkbuilding() {
		PrepareBuilding();
		workersCount = 0;
		workflow = 0;
		workflowToProcess = workflowToProcess_setValue;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (workersCount > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
			}
		}
	}

	protected virtual void LabourResult() {
		workflow = 0;
	}

	virtual public int AddWorkers (int x) {
		if (workersCount == maxWorkers) return 0;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
			}
			RecalculateWorkspeed();
			return x;
		}
	}

	virtual public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	virtual protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Manufacturing);
	}

	protected void PrepareWorkbuildingForDestruction() {
		PrepareBuildingForDestruction();
		if (workersCount != 0) GameMaster.colonyController.AddWorkers(workersCount);
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
	}
}
