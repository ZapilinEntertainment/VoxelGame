using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkBuilding : Building {
	public float workflow = 0, labourTimer = 0;
	public int maxWorkers = 8;
	public int workersCount {get; protected set;}
	const float WORKFLOW_GAIN = 1;

	void Awake() {
		hp = maxHp;
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
		workersCount = 0;
		if (energyCapacity != 0) GameMaster.colonyController.totalEnergyCapacity += energyCapacity;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		if (workersCount > 0) {
			workflow += GameMaster.CalculateWorkflow(workersCount, WorkType.Manufacturing);
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (labourTimer <= 0) {LabourResult(); labourTimer = GameMaster.LABOUR_TICK;}
		}
	}

	protected virtual void LabourResult() {

	}

	public void AddWorkers (int x) {
		if (x > 0) workersCount += x;
	}

	public override void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition,this));
			basement.artificialStructures --;
		}
		if (workersCount != 0) GameMaster.colonyController.AddWorkers(workersCount);
		if (energyCapacity != 0) GameMaster.colonyController.totalEnergyCapacity -= energyCapacity;
	}
}
