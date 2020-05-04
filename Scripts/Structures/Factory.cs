using UnityEngine;
using System.Collections.Generic;

public enum FactorySpecialization : byte { Unspecialized, Smeltery, OreRefiner, FuelFacility, PlasticsFactory, GraphoniumEnricher, SuppliesFactory }
public enum FactoryProductionMode : byte { NoLimit, Limit, Iterations } // if changing, change UIFactoryObserver prefab also

public class Factory : WorkBuilding
{
    public bool workPaused { get; protected set; }
    public int productionModeValue { get; protected set; } // limit or iterations
    public float inputResourcesBuffer { get; protected set; }
    public float outputResourcesBuffer { get; protected set; }
    public FactoryProductionMode productionMode { get; protected set; }
    public FactorySpecialization specialization { get; protected set; }
    protected Recipe recipe;

    public const float BUFFER_LIMIT = 10;
    public const int FACTORY_SERIALIZER_LENGTH = 17;

    public static UIFactoryObserver factoryObserver;


    override public void Prepare()
    {
        PrepareWorkbuilding();
        recipe = Recipe.NoRecipe;
        switch (ID)
        {
            case SMELTERY_1_ID:
            case SMELTERY_2_ID:
            case SMELTERY_3_ID:
            case SMELTERY_BLOCK_ID:
                specialization = FactorySpecialization.Smeltery;
                break;
            case ORE_ENRICHER_2_ID:
                specialization = FactorySpecialization.OreRefiner;
                break;
            case PLASTICS_FACTORY_3_ID:
                specialization = FactorySpecialization.PlasticsFactory;
                break;
            case FUEL_FACILITY_ID:
                specialization = FactorySpecialization.FuelFacility;
                break;
            case GRPH_ENRICHER_3_ID:
                specialization = FactorySpecialization.GraphoniumEnricher;
                break;
            case SUPPLIES_FACTORY_4_ID:
            case SUPPLIES_FACTORY_5_ID:
                specialization = FactorySpecialization.SuppliesFactory;
                break;
            default: specialization = FactorySpecialization.Unspecialized;
                break;
        }
        inputResourcesBuffer = 0;
        //changed copy to AdvancedFactory
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        SetActivationStatus(false, true);
        //copy to SmelteryBlock
    }

    override public void LabourUpdate()
    {
        if (recipe == Recipe.NoRecipe) return;
        Storage storage = colony.storage;
        if (outputResourcesBuffer > 0)
        {
            outputResourcesBuffer = storage.AddResource(recipe.output, outputResourcesBuffer);
        }
        if (outputResourcesBuffer <= BUFFER_LIMIT)
        {
            if (isActive)
            {
                if (isEnergySupplied)
                {
                    workSpeed = colony.workspeed * workersCount * GameConstants.FACTORY_SPEED * level * level;
                    if (productionMode == FactoryProductionMode.Limit)
                    {
                        if (!workPaused)
                        {
                            workflow += workSpeed;
                            colony.gears_coefficient -= gearsDamage * workSpeed;
                            if (workflow >= workflowToProcess) LabourResult();
                        }
                        else
                        {
                            if (storage.standartResources[recipe.output.ID] < productionModeValue)
                            {
                                workPaused = false;
                                workflow += workSpeed;
                                colony.gears_coefficient -= gearsDamage * workSpeed;
                                if (workflow >= workflowToProcess) LabourResult();
                            }
                        }
                    }
                    else
                    {
                        workflow += workSpeed;
                        colony.gears_coefficient -= gearsDamage * workSpeed;
                        if (workflow >= workflowToProcess) LabourResult();
                    }
                }
            }
        }
        //changed copy to AdvancedFactory
    }
    override protected void LabourResult()
    {
        int iterations = (int)(workflow / workflowToProcess);
        workflow = 0;
        Storage storage = colony.storage;
        if (storage.standartResources[recipe.input.ID] + inputResourcesBuffer >= recipe.inputValue)
        {
            inputResourcesBuffer += storage.GetResources(recipe.input, recipe.inputValue * iterations - inputResourcesBuffer);
            iterations = Mathf.FloorToInt(inputResourcesBuffer / recipe.inputValue);
            switch (productionMode)
            {
                case FactoryProductionMode.Limit:
                    {
                        float stVal = storage.standartResources[recipe.output.ID];
                        if (stVal + recipe.outputValue * iterations > productionModeValue)
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
                workPaused = (storage.standartResources[recipe.output.ID] >= productionModeValue);
                break;
            case FactoryProductionMode.Iterations:
                productionModeValue -= iterations;
                if (productionModeValue <= 0)
                {
                    productionModeValue = 0;
                    SetActivationStatus(false, true);
                }
                break;
        }
        //changed copy to AdvancedFactory
    }

    public Recipe GetRecipe()
    {
        return recipe;
    }
    public void SetRecipe(Recipe r)
    {
        if (r == recipe) return;
        if (recipe != Recipe.NoRecipe)
        {
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
        workPaused = (productionMode == FactoryProductionMode.Limit) & colony.storage.standartResources[r.output.ID] >= productionModeValue;
    }
    public void SetRecipe(int x)
    {
        Recipe[] allrecipes = GetFactoryRecipes();
        if (x > allrecipes.Length) return;
        else SetRecipe(allrecipes[x]);
    }

    public void SetProductionMode(FactoryProductionMode m)
    {
        if (productionMode == m) return;
        else
        {
            productionMode = m;
            productionModeValue = 0;
            workPaused = false;
        }
    }
    public void SetProductionValue(int x)
    {
        productionModeValue = x;
    }

    override public void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                GameLogUI.NotEnoughResourcesAnnounce();
                return;
            }
        }
        Factory upgraded = GetStructureByID(upgradedIndex) as Factory;
        upgraded.Prepare();
        PixelPosByte setPos = new PixelPosByte(surfaceRect.x, surfaceRect.z);
        if (upgraded.surfaceRect.size == 16) setPos = new PixelPosByte(0, 0);
        int workers = workersCount;
        workersCount = 0;
        if (upgraded.rotate90only & (modelRotation % 2 != 0))
        {
            upgraded.modelRotation = (byte)(modelRotation - 1);
        }
        else upgraded.modelRotation = modelRotation;        
        //
        upgraded.SetRecipe(recipe);
        upgraded.productionMode = productionMode;
        upgraded.productionModeValue = productionModeValue;
        upgraded.workPaused = workPaused;
        upgraded.workflow = workflow;
        upgraded.inputResourcesBuffer = inputResourcesBuffer; inputResourcesBuffer = 0;
        upgraded.outputResourcesBuffer = outputResourcesBuffer; outputResourcesBuffer = 0;        
        //
        upgraded.SetBasement(basement, setPos);
        upgraded.AddWorkers(workers);
        if (isActive) upgraded.SetActivationStatus(true, true);
        if (returnToUI) upgraded.ShowOnGUI();
        GameMaster.realMaster.eventTracker?.BuildingUpgraded(this);
    }


    #region save-load system
    public override List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(SerializeFactory());
        return data;
    }
    protected List<byte> SerializeFactory()
    {
        var data = new List<byte>() { (byte)productionMode };
        data.AddRange(System.BitConverter.GetBytes(recipe.ID));
        data.AddRange(System.BitConverter.GetBytes(inputResourcesBuffer));
        data.AddRange(System.BitConverter.GetBytes(outputResourcesBuffer));
        data.AddRange(System.BitConverter.GetBytes(productionModeValue));
        //SERIALIZER_LENGTH = 17;
        return data;
        //changed copy to AdvancedFactory
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
        var data = new byte[WORKBUILDING_SERIALIZER_LENGTH + FACTORY_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadFactoryData(data, WORKBUILDING_SERIALIZER_LENGTH);
        LoadWorkBuildingData(data, 0);
    }

    protected int LoadFactoryData(byte[] data, int startIndex)
    {
        SetRecipe(Recipe.GetRecipeByNumber(System.BitConverter.ToInt32(data, startIndex + 1)));
        inputResourcesBuffer = System.BitConverter.ToSingle(data, startIndex + 5);
        outputResourcesBuffer = System.BitConverter.ToSingle(data, startIndex + 9);
        productionMode = (FactoryProductionMode)data[startIndex];
        productionModeValue = System.BitConverter.ToInt32(data, startIndex + 13);
        return startIndex + 17;
        //change copy to AdvancedFactory
    }
    #endregion

    public Recipe[] GetFactoryRecipes()
    {
        switch (specialization)
        {            
            case FactorySpecialization.Smeltery: return Recipe.smelteryRecipes;
            case FactorySpecialization.OreRefiner: return Recipe.oreRefiningRecipes;
            case FactorySpecialization.FuelFacility: return Recipe.fuelFacilityRecipes;
            case FactorySpecialization.PlasticsFactory: return Recipe.plasticFactoryRecipes;
            case FactorySpecialization.GraphoniumEnricher: return Recipe.graphoniumEnricherRecipes;
            case FactorySpecialization.SuppliesFactory: return AdvancedRecipe.supplyFactoryRecipes;
            default: return new Recipe[1] { Recipe.NoRecipe};
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        SetRecipe(Recipe.NoRecipe);
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        //throw new System.Exception("factory destroyed");
        Destroy(gameObject);
    }
}