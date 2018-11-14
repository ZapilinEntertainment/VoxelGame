public enum FactorySpecialization {Unspecialized, Smeltery, OreRefiner, FuelFacility, PlasticsFactory}

[System.Serializable]
public class FactorySerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public int recipeID;
	public float inputResourcesBuffer,outputResourcesBuffer;
}

public class Factory : WorkBuilding {	
	public Recipe recipe { get; private set; }
	protected Storage storage;
	protected const float BUFFER_LIMIT = 10;
	public float inputResourcesBuffer  {get; protected set;}
	protected bool gui_showRecipesList = false;
	public FactorySpecialization specialization; // fixed by id
	protected float outputResourcesBuffer = 0;

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
		SetBuildingData(b, pos);
		storage = GameMaster.colonyController.storage;
        if ( !subscribedToUpdate )
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
	}

	override public void LabourUpdate() {
        if (recipe == Recipe.NoRecipe ) return;
        if(outputResourcesBuffer > 0) {
            outputResourcesBuffer = storage.AddResource(recipe.output, outputResourcesBuffer);
        }
        if (outputResourcesBuffer <= BUFFER_LIMIT & workSpeed != 0)
        {
            if (isActive & energySupplied) workflow += workSpeed;
            if (workflow >= workflowToProcess) LabourResult();
        }
	}

	override protected void LabourResult() {
        int iterations = (int)(workflow / workflowToProcess);
        workflow = 0;
        if (storage.standartResources[recipe.input.ID] + inputResourcesBuffer < recipe.inputValue)
        {            
            return;
        }
        else
        {
            inputResourcesBuffer += storage.GetResources(recipe.input, recipe.inputValue * iterations - inputResourcesBuffer);
            iterations = (int)(inputResourcesBuffer / recipe.inputValue);
            inputResourcesBuffer -= iterations * recipe.inputValue;
            outputResourcesBuffer += iterations * recipe.outputValue;
            outputResourcesBuffer = storage.AddResource(recipe.output, outputResourcesBuffer);
        }
	}

	public void SetRecipe( Recipe r ) {
		if (r == recipe) return;
		if (recipe != Recipe.NoRecipe) {
            if (inputResourcesBuffer > 0)
            {
                storage.AddResource(recipe.input, recipe.inputValue);
                inputResourcesBuffer = 0;
            }
            if (outputResourcesBuffer > 0)
            {
                storage.AddResource(recipe.output, recipe.outputValue);
                outputResourcesBuffer = 0;
            }
            }
		workflow = 0;		 
		recipe = r;
		workflowToProcess = r.workflowToResult;
	}
    public void SetRecipe(int x)
    {
        Recipe[] allrecipes = GetFactoryRecipes();
        if (x > allrecipes.Length) return;
        else  SetRecipe(allrecipes[x]);
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
        LoadWorkBuildingData(fs.workBuildingSerializer);		
	}

	protected FactorySerializer GetFactorySerializer() {
		FactorySerializer fs = new FactorySerializer();
		fs.workBuildingSerializer = GetWorkBuildingSerializer();
		fs.recipeID = recipe.ID;
		fs.inputResourcesBuffer = inputResourcesBuffer;
		fs.outputResourcesBuffer = outputResourcesBuffer;
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
