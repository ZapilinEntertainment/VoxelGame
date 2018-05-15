using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum CoveredFarmOutput { Food, Lumber }
public class CoveredFarm : WorkBuilding {
	[SerializeField]
	CoveredFarmOutput outputResource_type;
	[SerializeField]
	float output_value = 1;
	ResourceType outputResource;
	const float MIN_SPEED = 0.2f;
	Storage s;

	void Awake() {
		PrepareWorkbuilding();
		if (outputResource_type == CoveredFarmOutput.Food) outputResource = ResourceType.Food;
		else outputResource = ResourceType.Lumber;
		s = GameMaster.colonyController.storage;
	}

	override protected void LabourResult() {
		s.AddResources( new ResourceContainer (outputResource, output_value * workflow / workflowToProcess) );
		workflow = 0;
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Farming);
		if (workSpeed < MIN_SPEED && workersCount > 0) workSpeed = MIN_SPEED;
	}
}
