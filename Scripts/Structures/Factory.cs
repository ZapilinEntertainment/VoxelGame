using UnityEngine;
using System.Collections.Generic;

public enum FactorySpecialization : byte { Unspecialized, Smeltery, OreRefiner, FuelFacility, PlasticsFactory, GraphoniumEnricher, SuppliesFactory, Composter }
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
    virtual protected int FACTORY_SERIALIZER_LENGTH { get{return 17;} }//dependence: AdvancedFactory

    private static UIFactoryObserver _factoryObserver;
    public static UIFactoryObserver GetFactoryObserver()
    {
        if (_factoryObserver == null) _factoryObserver = UIFactoryObserver.InitializeFactoryObserverScript();
        return _factoryObserver;
    }
    public static UIFactoryObserver TryGetFactoryObserver()
    {
        return _factoryObserver;
    }

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
            case COMPOSTER_ID:
                specialization = FactorySpecialization.Composter;
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
        // #copy to advanced factory
        Storage storage = colony.storage;
        if (outputResourcesBuffer > 0)
        {
            outputResourcesBuffer = storage.AddResource(recipe.output, outputResourcesBuffer);
        }
        if (outputResourcesBuffer <= BUFFER_LIMIT)
        {
            if (isActive && isEnergySupplied)
            {
                float work = GetLabourCoefficient();
                if (productionMode == FactoryProductionMode.Limit)
                {
                    if (!workPaused) INLINE_WorkCalculation();
                    else
                    {
                        if (storage.GetResourceCount(recipe.output) < productionModeValue)
                        {
                            workPaused = false;
                            INLINE_WorkCalculation();
                        }
                    }
                }
                else INLINE_WorkCalculation();
            }
        }
        // eo copy
    }

    override protected void LabourResult(int iterations)
    {
        if ( GameMaster.loading | iterations < 1) return;
        workflow -= iterations;
        Storage storage = colony.storage;
        if (storage.GetResourceCount(recipe.input) + inputResourcesBuffer >= recipe.inputValue)
        {
            inputResourcesBuffer += storage.GetResources(recipe.input, recipe.inputValue * iterations - inputResourcesBuffer);
            iterations = Mathf.FloorToInt(inputResourcesBuffer / recipe.inputValue);
            switch (productionMode)
            {
                case FactoryProductionMode.Limit:
                    {
                        float stVal = storage.GetResourceCount(recipe.output);
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
                workPaused = (storage.GetResourceCount(recipe.output) >= productionModeValue);
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
    virtual protected void SetRecipe(Recipe r)
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
        workComplexityCoefficient = recipe.workComplexity;
        workPaused = (productionMode == FactoryProductionMode.Limit) & colony.storage.GetResourceCount(recipe.output) >= productionModeValue;
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
                AnnouncementCanvasController.NotEnoughResourcesAnnounce();
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
            case FactorySpecialization.Composter: return Recipe.composterRecipes;
            default: return new Recipe[1] { Recipe.NoRecipe};
        }
    }

    public override UIObserver ShowOnGUI()
    {
        var fo = GetFactoryObserver();
        if (!fo.gameObject.activeSelf) fo.gameObject.SetActive(true);
        fo.SetObservingFactory(this);
        showOnGUI = true;
        return fo;
    }
    override public string UI_GetInfo()
    {
        if (recipe != null) return string.Format("{0:0.##}", GetLabourCoefficient() * recipe.outputValue / GameMaster.LABOUR_TICK) + ' ' + Localization.GetPhrase(LocalizedPhrase.PerSecond);
        else return "No recipe";
    }


    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        if (order.returnResources) SetRecipe(Recipe.NoRecipe);
        PrepareWorkbuildingForDestruction(order);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }

    #region save-load system
    public override List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(SerializeFactory());
        return data;
    }
    virtual protected List<byte> SerializeFactory()
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

    override public void Load(System.IO.Stream fs, Plane sblock)
    {
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
        var data = new byte[WORKBUILDING_SERIALIZER_LENGTH + FACTORY_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadFactoryData(data, WORKBUILDING_SERIALIZER_LENGTH);
        LoadWorkBuildingData(data, 0);
        //changed copy to AdvancedFactory
    }

    virtual protected int LoadFactoryData(byte[] data, int startIndex)
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
}