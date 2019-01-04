using System.Collections.Generic;

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
            if (isActive & isEnergySupplied) workflow += workSpeed;
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
	override public List<byte> Save() {
        var data = SerializeStructure();
        data.AddRange(SerializeBuilding());
        data.AddRange(SerializeWorkBuilding());
        data.AddRange(SerializeFoodFactory());
        return data;
	}
    public List<byte> SerializeFoodFactory()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(food_inputBuffer));
        data.AddRange(System.BitConverter.GetBytes(metalP_inputBuffer));
        data.AddRange(System.BitConverter.GetBytes(supplies_outputBuffer));
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock) {
        base.Load(fs, sblock);
        LoadFoodFactoryData(fs);
	}

    protected void LoadFoodFactoryData(System.IO.FileStream fs)
    {
        var data = new byte[12];
        fs.Read(data, 0, data.Length);
        LoadFoodFactoryData(data, 0);
    }
    protected void LoadFoodFactoryData(byte[] data, int startIndex)
    {
        food_inputBuffer = System.BitConverter.ToSingle(data, startIndex);
        metalP_inputBuffer = System.BitConverter.ToSingle(data, startIndex + 4);
        supplies_outputBuffer = System.BitConverter.ToSingle(data, startIndex + 8);
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
