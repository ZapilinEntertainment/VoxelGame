using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodFactory : WorkBuilding {
	[SerializeField]
	float food_input = 10, metalP_input = 1, output = 5;
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
			food_outputBuffer = storage.AddResource(ResourceType.Food, food_outputBuffer);
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

	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData() + SaveFoodFactoryData();
	}

	protected string SaveFoodFactoryData() {
		string s = "";
		s += string.Format("{0:d5}", (int)(food_inputBuffer * 1000f ));
		s += string.Format("{0:d5}", (int)(metalP_inputBuffer * 1000f ));
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
		food_input = int.Parse(s_data.Substring(18,5)) / 1000f;
		metalP_input = int.Parse(s_data.Substring(23,5)) / 1000f;
		//building class part
		SetActivationStatus(s_data[11] == '1');     
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	

	void OnDestroy() {
		if (food_inputBuffer > 0) {
			if (food_outputBuffer > 0) storage.AddResource(ResourceType.Food, food_inputBuffer + food_outputBuffer);
			else storage.AddResource(ResourceType.Food, food_inputBuffer);
		}
		if (food_outputBuffer > 0) storage.AddResource(ResourceType.metal_P, metalP_inputBuffer);
		PrepareWorkbuildingForDestruction();
	}
}
