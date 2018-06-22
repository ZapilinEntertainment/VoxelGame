using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodFactorySerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public float food_inputBuffer, metalP_inputBuffer, supplies_outputBuffer; 
}

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

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetFoodFactorySerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		FoodFactorySerializer ffs = new FoodFactorySerializer();
		GameMaster.DeserializeByteArray<FoodFactorySerializer>(ss.specificData, ref ffs);
		LoadFoodFactoryData(ffs);
	}

	protected void LoadFoodFactoryData(FoodFactorySerializer ffs) {
		LoadWorkBuildingData(ffs.workBuildingSerializer);
		food_inputBuffer = ffs.food_inputBuffer;
		metalP_inputBuffer = ffs.metalP_inputBuffer;
		supplies_outputBuffer = ffs.supplies_outputBuffer;
	}

	public FoodFactorySerializer GetFoodFactorySerializer() {
		FoodFactorySerializer ffs = new FoodFactorySerializer();
		ffs.workBuildingSerializer = GetWorkBuildingSerializer();
		ffs.food_inputBuffer = food_inputBuffer;
		ffs.metalP_inputBuffer = metalP_inputBuffer;
		ffs.supplies_outputBuffer = supplies_outputBuffer;
		return ffs;
	}
	#endregion

	void OnDestroy() {
		if (food_inputBuffer > 0) storage.AddResource(ResourceType.Food, food_inputBuffer);
		if (metalP_inputBuffer > 0) storage.AddResource(ResourceType.metal_P, metalP_inputBuffer);
		if (supplies_outputBuffer > 0) storage.AddResource(ResourceType.Supplies,supplies_outputBuffer);
		PrepareWorkbuildingForDestruction();
	}
}
