public enum FactorySpecialization {Unspecialized, Smeltery, OreRefiner, FuelFacility, PlasticsFactory}

[System.Serializable]
public class FactorySerializer {
    public FactoryProductionMode productionMode;
	public WorkBuildingSerializer workBuildingSerializer;
	public int recipeID, productionModeValue;
	public float inputResourcesBuffer,outputResourcesBuffer;
}

public enum FactoryProductionMode : byte { NoLimit, Limit, Iterations} // if changing, change UIFactoryObserver prefab also

public class Factory : WorkBuilding {
    public int productionModeValue { get; protected set; } // limit or iterations
    public float inputResourcesBuffer { get; protected set; }
    public float outputResourcesBuffer { get; protected set; }
    public FactoryProductionMode productionMode { get; protected set; }
    public FactorySpecialization specialization { get; protected set; }
    public Recipe recipe { get; private set; }

    public const float BUFFER_LIMIT = 10;
      
    protected bool gui_showRecipesList = false;    

    public static UIFactoryObserver factoryObserver;

	override public void Prepare() {
		PrepareWorkbuilding();
		recipe = Recipe.NoRecipe;
        switch (id)
        {
            case SMELTERY_1_ID:
            case SMELTERY_2_ID:
            case SMELTERY_3_ID:
            case SMELTERY_5_ID:
                specialization = FactorySpecialization.Smeltery;
                break;
            case ORE_ENRICHER_2_ID:
                specialization = FactorySpecialization.OreRefiner;
                break;
            case PLASTICS_FACTORY_3_ID:
                specialization = FactorySpecialization.PlasticsFactory;
                break;
            case FUEL_FACILITY_3_ID:
                specialization = FactorySpecialization.FuelFacility;
                break;
        }
		inputResourcesBuffer = 0;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetWorkbuildingData(b, pos);        
        if ( !subscribedToUpdate )
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        SetActivationStatus(false);
	}

	override public void LabourUpdate() {
        if (recipe == Recipe.NoRecipe ) return;
        Storage storage = colony.storage;
        if(outputResourcesBuffer > 0) {
            outputResourcesBuffer = storage.AddResource(recipe.output, outputResourcesBuffer);
        }
        if (outputResourcesBuffer <= BUFFER_LIMIT)
        {
            if (isActive)
            {
                if (energySupplied)
                {
                    workflow += workSpeed;
                    if (workflow >= workflowToProcess) LabourResult();
                }
            }
            else
            {
                if (productionMode == FactoryProductionMode.Limit)
                {
                    if (storage.standartResources[recipe.output.ID] < productionModeValue) SetActivationStatus(true);
                }
            }
        }
	}

	override protected void LabourResult() {
        int iterations = (int)(workflow / workflowToProcess);
        workflow = 0;
        Storage storage = colony.storage;
        if (storage.standartResources[recipe.input.ID] + inputResourcesBuffer >= recipe.inputValue)
        {
            inputResourcesBuffer += storage.GetResources(recipe.input, recipe.inputValue * iterations - inputResourcesBuffer);
            iterations = (int)(inputResourcesBuffer / recipe.inputValue);
            switch (productionMode)
            {
                case FactoryProductionMode.Limit:
                    {
                        float stVal = storage.standartResources[recipe.output.ID];
                        if (stVal+ recipe.outputValue * iterations > productionModeValue)
                        {
                            iterations = (int)((productionModeValue - stVal) / recipe.outputValue);
                        }
                        break;
                    }
                case FactoryProductionMode.Iterations:
                    {
                        if (productionModeValue < iterations) iterations = productionModeValue;
                        break;
                    }
            }
            if (iterations > 0)
            {
                inputResourcesBuffer -= iterations * recipe.inputValue;
                outputResourcesBuffer += iterations * recipe.outputValue;
                outputResourcesBuffer = storage.AddResource(recipe.output, outputResourcesBuffer);               
            }
        }
        switch (productionMode)
        {
            case FactoryProductionMode.Limit:
                if (storage.standartResources[recipe.output.ID] >= productionModeValue) SetActivationStatus(false);
                break;
            case FactoryProductionMode.Iterations:
                productionModeValue -= iterations;
                if (productionModeValue <= 0)
                {
                    productionModeValue = 0;
                    SetActivationStatus(false);
                }
                break;
        }
	}

	public void SetRecipe( Recipe r ) {
		if (r == recipe) return;
		if (recipe != Recipe.NoRecipe) {
            if (inputResourcesBuffer > 0)
            {
                colony.storage.AddResource(recipe.input, recipe.inputValue);
                inputResourcesBuffer = 0;
            }
            if (outputResourcesBuffer > 0)
            {
                colony.storage.AddResource(recipe.output, recipe.outputValue);
                outputResourcesBuffer = 0;
            }
            }
		workflow = 0;		 
		recipe = r;
        productionModeValue = 0;
		workflowToProcess = r.workflowToResult;
	}
    public void SetRecipe(int x)
    {
        Recipe[] allrecipes = GetFactoryRecipes();
        if (x > allrecipes.Length) return;
        else  SetRecipe(allrecipes[x]);
    }

    public void SetProductionMode(FactoryProductionMode m)
    {
        if (productionMode == m) return;
        else
        {
            productionMode = m;
            productionModeValue = 0;

        }
    }
    public void SetProductionValue(int x)
    {
        productionModeValue = x;
    }

    #region save-load system
    public override StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetFactorySerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load (StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		FactorySerializer fs = new FactorySerializer();
		GameMaster.DeserializeByteArray(ss.specificData, ref fs);
		LoadFactoryData(fs);
	}

	protected void LoadFactoryData(FactorySerializer fs) {
        SetRecipe(Recipe.GetRecipeByNumber(fs.recipeID));
        inputResourcesBuffer = fs.inputResourcesBuffer;
        outputResourcesBuffer = fs.outputResourcesBuffer;
        productionMode = fs.productionMode;
        productionModeValue = fs.productionModeValue;
        LoadWorkBuildingData(fs.workBuildingSerializer);		
	}

	protected FactorySerializer GetFactorySerializer() {
		FactorySerializer fs = new FactorySerializer();
		fs.workBuildingSerializer = GetWorkBuildingSerializer();
		fs.recipeID = recipe.ID;
		fs.inputResourcesBuffer = inputResourcesBuffer;
		fs.outputResourcesBuffer = outputResourcesBuffer;
        fs.productionMode = productionMode;
        fs.productionModeValue = productionModeValue;
		return fs;
	}
	#endregion

    public virtual Recipe[] GetFactoryRecipes()
    {
        switch (specialization)
        {
            default:
            case FactorySpecialization.Unspecialized: return new Recipe[0];
            case FactorySpecialization.Smeltery: return Recipe.smelteryRecipes;
            case FactorySpecialization.OreRefiner: return Recipe.oreRefiningRecipes;
            case FactorySpecialization.FuelFacility: return Recipe.fuelFacilityRecipes;
            case FactorySpecialization.PlasticsFactory: return Recipe.plasticFactoryRecipes;
        }
    }

    public override UIObserver ShowOnGUI()
    {
        if (factoryObserver == null) factoryObserver = UIFactoryObserver.InitializeFactoryObserverScript();
        else factoryObserver.gameObject.SetActive(true);
        factoryObserver.SetObservingFactory(this);
        showOnGUI = true;
        return factoryObserver;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
