using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodFactory : WorkBuilding {
	[SerializeField]
	float food_input = 1, metalP_input = 1, output = 5;
	float food_inputBuffer = 0, metalP_inputBuffer = 0, food_outputBuffer = 0; 
	Storage storage;
	const float BUFFER_LIMIT = 10;

	override public void Prepare() {
		PrepareWorkbuilding();
		storage = GameMaster.colonyController.storage;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (food_outputBuffer > 0) {
			food_outputBuffer = storage.AddResources(ResourceType.Food, food_outputBuffer);
			if (food_outputBuffer > BUFFER_LIMIT) return;
		}
		if (workersCount > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				if (food_inputBuffer < food_input) food_inputBuffer += storage.GetResources(ResourceType.Food, food_input - food_inputBuffer); 
				if ( metalP_inputBuffer < metalP_input ) metalP_inputBuffer += storage.GetResources( ResourceType.metal_P, metalP_input - metalP_inputBuffer);
				if (food_inputBuffer >= food_input && metalP_inputBuffer >= metalP_input) LabourResult();
			}
		}
	}

	override protected void LabourResult() {
		float val = workflow / workflowToProcess;
		food_outputBuffer += val  * output;
		metalP_inputBuffer -= val * metalP_input;
		food_inputBuffer -= val * food_input;
		workflow = 0;
	}

	void OnDestroy() {
		if (food_inputBuffer > 0) {
			if (food_outputBuffer > 0) storage.AddResources(ResourceType.Food, food_inputBuffer + food_outputBuffer);
			else storage.AddResources(ResourceType.Food, food_inputBuffer);
		}
		if (food_outputBuffer > 0) storage.AddResources(ResourceType.metal_P, metalP_inputBuffer);
		PrepareWorkbuildingForDestruction();
	}
}
