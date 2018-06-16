using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodFactory : WorkBuilding {
	const float food_input = 10, metalP_input = 1, output = 15;
	float food_inputBuffer = 0, metalP_inputBuffer = 0, supplies_outputBuffer = 0; 
	Storage storage;
	const float BUFFER_LIMIT = 10;

	override public void Prepare() {
		PrepareWorkbuilding();
		storage = GameMaster.colonyController.storage;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (supplies_outputBuffer > 0) {
			supplies_outputBuffer = storage.AddResource(ResourceType.Supplies, supplies_outputBuffer);
			if (supplies_outputBuffer > BUFFER_LIMIT) return;
		}
		if (workersCount > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				if (food_inputBuffer < food_input) food_inputBuffer += storage.GetResources(ResourceType.Food, food_input - food_inputBuffer); 
				if ( metalP_inputBuffer < metalP_input ) metalP_inputBuffer += storage.GetResources( ResourceType.metal_P, metalP_input - metalP_inputBuffer);
				LabourResult();
			}
		}
	}

	override protected void LabourResult() {
		while ( food_inputBuffer >= food_input & metalP_inputBuffer >= metalP_input & workflow >= workflowToProcess)
		{
			food_inputBuffer -= food_input;
			metalP_inputBuffer -= metalP_input;
			supplies_outputBuffer += output;
			workflow -= workflowToProcess;
		}
	}

	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData() + SaveFoodFactoryData();
	}

	protected string SaveFoodFactoryData() {
		string s = "";
		s += string.Format("{0:d5}", (int)(food_inputBuffer * 1000f ));
		s += string.Format("{0:d5}", (int)(metalP_inputBuffer * 1000f ));
		s += string.Format("{0:d5}", (int)(supplies_outputBuffer * 1000f));
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		//workbuilding class part
		workflow = int.Parse(s_data.Substring(12,3)) / 100f;
		AddWorkers(int.Parse(s_data.Substring(15,3)));
		//food factory class part
		food_inputBuffer = int.Parse(s_data.Substring(18,5)) / 1000f;
		metalP_inputBuffer = int.Parse(s_data.Substring(23,5)) / 1000f;
		supplies_outputBuffer = int.Parse(s_data.Substring(28,5)) / 1000f;
		//building class part
		SetActivationStatus(s_data[11] == '1');     
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	

	void OnDestroy() {
		if (food_inputBuffer > 0) storage.AddResource(ResourceType.Food, food_inputBuffer);
		if (metalP_inputBuffer > 0) storage.AddResource(ResourceType.metal_P, metalP_inputBuffer);
		if (supplies_outputBuffer > 0) storage.AddResource(ResourceType.Supplies,supplies_outputBuffer);
		PrepareWorkbuildingForDestruction();
	}
}
