[System.Serializable]
public class FoodFactorySerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public float food_inputBuffer, metalP_inputBuffer, supplies_outputBuffer; 
}

public class FoodFactory : WorkBuilding {
	const float food_input = 10, metalP_input = 1, supplies_output = 15;
	float food_inputBuffer = 0, metalP_inputBuffer = 0, supplies_outputBuffer = 0; 
	Storage storage;
	const float BUFFER_LIMIT = 10;

	override public void Prepare() {
		PrepareWorkbuilding();
		storage = colony.storage;
	}

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
    }

    override public void LabourUpdate()
    {
        if (supplies_outputBuffer > 0)
        {
            supplies_outputBuffer = storage.AddResource(ResourceType.Supplies, supplies_outputBuffer);
        }
        if (supplies_outputBuffer <= BUFFER_LIMIT)
        {
            if (isActive & energySupplied) workflow += workSpeed;
            if (workflow >= workflowToProcess) LabourResult();
        }
    }

    override protected void LabourResult()
    {
        int iterations = (int)(workflow / workflowToProcess);
        workflow = 0;
        if (storage.standartResources[ResourceType.FOOD_ID] + food_inputBuffer < food_input | storage.standartResources[ResourceType.METAL_P_ID] + metalP_inputBuffer < metalP_input)
        {            
            return;
        }
        else
        {
            food_inputBuffer += storage.GetResources(ResourceType.Food, food_input * iterations - food_inputBuffer);
            int it_a = (int)(food_inputBuffer / food_input);
            metalP_inputBuffer += storage.GetResources(ResourceType.metal_P, metalP_input * iterations - metalP_inputBuffer);
            int it_b = (int)(metalP_inputBuffer / metalP_input);
            if (it_a < it_b) iterations = it_a; else iterations = it_b;
            food_inputBuffer -= iterations * food_input;
            metalP_inputBuffer -= iterations * metalP_input;
            supplies_outputBuffer += iterations * supplies_output;
            supplies_outputBuffer = storage.AddResource(ResourceType.Supplies, supplies_outputBuffer);
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

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) {
            UnsetBasement();
        }
        else
        {
            if (food_inputBuffer > 0) storage.AddResource(ResourceType.Food, food_inputBuffer);
            if (metalP_inputBuffer > 0) storage.AddResource(ResourceType.metal_P, metalP_inputBuffer);
            if (supplies_outputBuffer > 0) storage.AddResource(ResourceType.Supplies, supplies_outputBuffer);
        }
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
