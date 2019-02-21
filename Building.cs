using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure
{
    public int upgradedIndex { get; private set; } 
    public bool canBePowerSwitched { get; protected set; } 
    public bool isActive { get; protected set; }
    public bool isEnergySupplied { get; protected set; } //управляется только ColonyController'ом
    public bool connectedToPowerGrid { get; protected set; }// установлено ли подключение к электросети
    public float energySurplus = 0; 
    public float energyCapacity = 0;     
    public byte level { get; protected set; } 
    public bool specialBuildingConditions { get; protected set; }

    public static UIBuildingObserver buildingObserver;

    public static List<Building> GetApplicableBuildingsList(byte i_level)
    {
        //относительно ужасное решение
        // хотя создаются всего лишь экземпляры классов, без моделей и привязок
        List<Building> blist = new List<Building>();
        switch (i_level)
        {
            case 1:
                blist.Add(GetStructureByID(WIND_GENERATOR_1_ID) as Building);
                blist.Add(GetStructureByID(STORAGE_1_ID) as Building);
                blist.Add(GetStructureByID(HOUSE_1_ID) as Building);
                blist.Add(GetStructureByID(FARM_1_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_1_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_1_ID) as Building);
                blist.Add(GetStructureByID(ENERGY_CAPACITOR_1_ID) as Building);
                blist.Add(GetStructureByID(MINE_ID) as Building);
                blist.Add(GetStructureByID(DOCK_ID) as Building);
                break;
            case 2:
                blist.Add(GetStructureByID(STORAGE_2_ID) as Building);
                blist.Add(GetStructureByID(HOUSE_2_ID) as Building);
                blist.Add(GetStructureByID(FARM_2_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_2_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_2_ID) as Building);
                blist.Add(GetStructureByID(ENERGY_CAPACITOR_2_ID) as Building);
                blist.Add(GetStructureByID(ORE_ENRICHER_2_ID) as Building);
                blist.Add(GetStructureByID(BIOGENERATOR_2_ID) as Building);
                blist.Add(GetStructureByID(MINERAL_POWERPLANT_2_ID) as Building);
                blist.Add(GetStructureByID(HOSPITAL_2_ID) as Building);
                blist.Add(GetStructureByID(WORKSHOP_ID) as Building);
                break;
            case 3:
                blist.Add(GetStructureByID(STORAGE_3_ID) as Building);
                blist.Add(GetStructureByID(HOUSE_3_ID) as Building);
                blist.Add(GetStructureByID(FARM_3_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_3_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_3_ID) as Building);
                blist.Add(GetStructureByID(PLASTICS_FACTORY_3_ID) as Building);
                blist.Add(GetStructureByID(ENERGY_CAPACITOR_3_ID) as Building);
                blist.Add(GetStructureByID(MINI_GRPH_REACTOR_3_ID) as Building);
                blist.Add(GetStructureByID(FUEL_FACILITY_3_ID) as Building);
                blist.Add(GetStructureByID(XSTATION_3_ID) as Building);
                blist.Add(GetStructureByID(GRPH_ENRICHER_3_ID) as Building);
                break;
            case 4:
                blist.Add(GetStructureByID(FARM_4_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_4_ID) as Building);
                blist.Add(GetStructureByID(SUPPLIES_FACTORY_4_ID) as Building);
                blist.Add(GetStructureByID(GRPH_REACTOR_4_ID) as Building);
                blist.Add(GetStructureByID(SHUTTLE_HANGAR_4_ID) as Building);
                blist.Add(GetStructureByID(RECRUITING_CENTER_4_ID) as Building);
                blist.Add(GetStructureByID(EXPEDITION_CORPUS_4_ID) as Building);
                blist.Add(GetStructureByID(QUANTUM_TRANSMITTER_4_ID) as Building);
                blist.Add(GetStructureByID(CHEMICAL_FACTORY_4_ID) as Building);
                blist.Add(GetStructureByID(DOCK_ADDON_1_ID) as Building);
                break;
            case 5:
                blist.Add(GetStructureByID(STORAGE_5_ID) as Building);
                blist.Add(GetStructureByID(HOUSE_5_ID) as Building);
                blist.Add(GetStructureByID(FARM_5_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_5_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_5_ID) as Building);
                blist.Add(GetStructureByID(SUPPLIES_FACTORY_5_ID) as Building);
                blist.Add(GetStructureByID(QUANTUM_ENERGY_TRANSMITTER_5_ID) as Building);
                blist.Add(GetStructureByID(FOUNDATION_BLOCK_5_ID) as Building);
                blist.Add(GetStructureByID(REACTOR_BLOCK_5_ID) as Building);
                blist.Add(GetStructureByID(OBSERVATORY_ID) as Building);
                break;
            case 6:
                blist.Add(GetStructureByID(CONNECT_TOWER_6_ID) as Building);
                blist.Add(GetStructureByID(CONTROL_CENTER_6_ID) as Building);
                blist.Add(GetStructureByID(HOTEL_BLOCK_6_ID) as Building);
                blist.Add(GetStructureByID(HOUSING_MAST_6_ID) as Building);
                blist.Add(GetStructureByID(DOCK_ADDON_2_ID) as Building);
                blist.Add(GetStructureByID(SWITCH_TOWER_ID) as Building);
                break;
        }
        return blist;
    }

    public const int BUILDING_SERIALIZER_LENGTH = 5;


    public static float GetEnergyCapacity(int id)
    {
        switch (id)
        {
            case ENERGY_CAPACITOR_1_ID: return 200;
            case ENERGY_CAPACITOR_2_ID: return 800;
            case ENERGY_CAPACITOR_3_ID: return 2500;
            default: return 1;
        }
    }

    override public void Prepare() { PrepareBuilding(); }

    /// <summary>
    /// do not use directly, use Prepare() instead
    /// </summary>
    /// <param name="i_id"></param>
    /// <returns></returns>
	protected void PrepareBuilding()
    {
        PrepareStructure();
        isActive = false;
        isEnergySupplied = false;
        connectedToPowerGrid = false;
        specialBuildingConditions = false;
        switch (id)
        {
            case LANDED_ZEPPELIN_ID:
                {
                    upgradedIndex = HQ_2_ID;
                    canBePowerSwitched = false;
                    energySurplus = 1;
                    energyCapacity = 100;
                    level = 1;
                }
                break;
            case HQ_2_ID:
                {
                    upgradedIndex = HQ_3_ID;
                    canBePowerSwitched = false;
                    energySurplus = 3;
                    energyCapacity = 200;
                    level = 2;
                }
                break;
            case HQ_3_ID:
                {
                    upgradedIndex = HQ_4_ID;
                    canBePowerSwitched = false;
                    energySurplus = 5;
                    energyCapacity = 400;
                    level = 3;
                }
                break;
            case HQ_4_ID:
                {
                    upgradedIndex = 1;
                    canBePowerSwitched = false;
                    energySurplus = 10;
                    energyCapacity = 500;
                    level = 4;
                }
                break;
            case STORAGE_0_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    level = 0;
                }
                break;
            case STORAGE_1_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 1;
                }
                break;
            case STORAGE_2_ID:
                {
                    upgradedIndex = STORAGE_3_ID;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 2;
                }
                break;
            case STORAGE_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 3;
                }
                break;
            case STORAGE_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 5;
                }
                break;
            case TENT_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 0;
                }
                break;
            case HOUSE_1_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 1;
                    
                    level = 1;
                }
                break;
            case HOUSE_2_ID:
                {
                    upgradedIndex = HOUSE_3_ID;
                    canBePowerSwitched = false;
                    energySurplus = -5;
                    energyCapacity = 10;
                    
                    level = 2;
                }
                break;
            case HOUSE_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = -8;
                    energyCapacity = 20;
                    
                    level = 3;
                }
                break;
            case HOUSE_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = -50;
                    energyCapacity = 150;
                    
                    level = 5;
                }
                break;
            case DOCK_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -12;
                    energyCapacity = 24;                    
                    level = 1;
                }
                break;
            case DOCK_2_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 80;
                    level = 2;
                }
                break;
            case DOCK_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -30;
                    energyCapacity = 150;
                    level = 3;
                }
                break;
            case ENERGY_CAPACITOR_1_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = GetEnergyCapacity(id);
                    
                    level = 1;
                }
                break;
            case ENERGY_CAPACITOR_2_ID:
                {
                    upgradedIndex = ENERGY_CAPACITOR_3_ID;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = GetEnergyCapacity(id);
                    
                    level = 2;
                }
                break;
            case ENERGY_CAPACITOR_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = GetEnergyCapacity(id);
                    
                    level = 3;
                }
                break;
            case FARM_1_ID:
                {
                    upgradedIndex = FARM_2_ID;
                    canBePowerSwitched = true;
                    energySurplus = -1;
                    energyCapacity = 10;
                    specialBuildingConditions = true;
                    level = 1;
                }
                break;
            case FARM_2_ID:
                {
                    upgradedIndex = FARM_3_ID;
                    canBePowerSwitched = true;
                    energySurplus = -4;
                    energyCapacity = 20;
                    specialBuildingConditions = true;
                    level = 2;
                }
                break;
            case FARM_3_ID:
                {
                    upgradedIndex = FARM_4_ID;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 40;
                    specialBuildingConditions = true;
                    level = 3;
                }
                break;
            case FARM_4_ID:
                {
                    upgradedIndex = FARM_5_ID;
                    canBePowerSwitched = true;
                    energySurplus = -40;
                    energyCapacity = 120;
                    level = 4;
                }
                break;
            case FARM_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -100;
                    energyCapacity = 300;
                    level = 5;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    upgradedIndex = LUMBERMILL_2_ID;
                    canBePowerSwitched = true;
                    energySurplus = -2;
                    energyCapacity = 10;
                    specialBuildingConditions = true;
                    level = 1;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    upgradedIndex = LUMBERMILL_3_ID;
                    canBePowerSwitched = true;
                    energySurplus = -5;
                    energyCapacity = 40;
                    specialBuildingConditions = true;
                    level = 2;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    upgradedIndex = LUMBERMILL_4_ID;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 80;
                    specialBuildingConditions = true;
                    level = 3;
                }
                break;
            case LUMBERMILL_4_ID:
                {
                    upgradedIndex = LUMBERMILL_5_ID;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 80;
                    
                    level = 4;
                }
                break;
            case LUMBERMILL_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -80;
                    energyCapacity = 160;
                    
                    level = 5;
                }
                break;
            case MINE_ID:
                {
                    upgradedIndex = 0;
                    canBePowerSwitched = true;
                    energySurplus = -2;
                    energyCapacity = 20;
                    level = 1;
                }
                break;
            case SMELTERY_1_ID:
                {
                    upgradedIndex = SMELTERY_2_ID;
                    canBePowerSwitched = true;
                    energySurplus = -8;
                    energyCapacity = 40;
                    
                    level = 1;
                }
                break;
            case SMELTERY_2_ID:
                {
                    upgradedIndex = SMELTERY_3_ID;
                    canBePowerSwitched = true;
                    energySurplus = -15;
                    energyCapacity = 60;
                    
                    level = 2;
                }
                break;
            case SMELTERY_3_ID:
                {
                    upgradedIndex = SMELTERY_5_ID;
                    canBePowerSwitched = true;
                    energySurplus = -40;
                    energyCapacity = 100;
                    
                    level = 3;
                }
                break;
            case SMELTERY_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -80;
                    energyCapacity = 160;
                    
                    level = 5;
                }
                break;
            case WIND_GENERATOR_1_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 10;
                    energyCapacity = 10;
                    
                    level = 1;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = Powerplant.BIOGEN_OUTPUT;
                    energyCapacity = 20;
                    
                    level = 2;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -6;
                    energyCapacity = 50;
                    
                    level = 2;
                }
                break;
            case MINERAL_POWERPLANT_2_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = Powerplant.MINERAL_F_PP_OUTPUT;
                    energyCapacity = 20;
                    
                    level = 2;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -12;
                    energyCapacity = 40;
                    
                    level = 2;
                }
                break;
            case WORKSHOP_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -8;
                    energyCapacity = 20;
                    
                    level = 2;
                }
                break;
            case MINI_GRPH_REACTOR_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 50;
                    energyCapacity = 100;
                    
                    level = 3;
                }
                break;
            case FUEL_FACILITY_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 50;
                    
                    level = 3;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = Powerplant.GRPH_REACTOR_OUTPUT;
                    energyCapacity = 100;
                    
                    level = 4;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 40;
                    
                    level = 3;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    upgradedIndex = SUPPLIES_FACTORY_5_ID;
                    canBePowerSwitched = true;
                    energySurplus = -25;
                    energyCapacity = 20;
                    
                    level = 4;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -40;
                    energyCapacity = 80;
                    
                    level = 5;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -70;
                    energyCapacity = 20;
                    
                    level = 3;
                }
                break;
            case XSTATION_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -10;
                    energyCapacity = 12;
                    
                    level = 3;
                }
                break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 5;
                }
                break;
            case CHEMICAL_FACTORY_4_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -20;
                    energyCapacity = 40;
                    
                    level = 4;
                }
                break;
            case SWITCH_TOWER_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;
                    
                    level = 6;
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -10;
                    energyCapacity = 40;
                    
                    level = 4;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -12;
                    energyCapacity = 20;
                    
                    level = 4;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -12;
                    energyCapacity = 20;
                    
                    level = 4;
                }
                break;
            case QUANTUM_TRANSMITTER_4_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 0;                    
                    level = 4;
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = Powerplant.REACTOR_BLOCK_5_OUTPUT;
                    energyCapacity = 200;
                    level = 5;
                }
                break;
            case FOUNDATION_BLOCK_5_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 10;
                    energyCapacity = 10;
                    level = 5;
                }
                break;
            case CONNECT_TOWER_6_ID:
                {
                    specialBuildingConditions = true;
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -64;
                    energyCapacity = 192;
                    level = 6;
                }
                break;
            case CONTROL_CENTER_6_ID:
                {
                    specialBuildingConditions = true;
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -40;
                    energyCapacity = 240;
                    level = 6;
                }
                break;
            case HOTEL_BLOCK_6_ID:
                {
                    specialBuildingConditions = false;
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = -70;
                    energyCapacity = 300;
                    level = 6;
                }
                break;
            case HOUSING_MAST_6_ID:
                {
                    specialBuildingConditions = true;
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = -120;
                    energyCapacity = 1000;
                    level = 6;
                }
                break;
            case DOCK_ADDON_1_ID:
                {
                    specialBuildingConditions = false;
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 100;
                    level = 1;
                }
                break;
            case DOCK_ADDON_2_ID:
                {
                    specialBuildingConditions = false;
                    upgradedIndex = -1;
                    canBePowerSwitched = false;
                    energySurplus = 0;
                    energyCapacity = 100;
                    level = 2;
                }
                break;
            case OBSERVATORY_ID:
                {
                    specialBuildingConditions = true;
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    energySurplus = - 50;
                    energyCapacity = 200;
                    level = 5;
                }
                break;
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
    }

    protected void SetBuildingData(SurfaceBlock b, PixelPosByte pos)
    {
        SetStructureData(b, pos);
        isActive = true;
        if (energySurplus != 0 | energyCapacity > 0)
        {
            GameMaster.realMaster.colonyController.AddToPowerGrid(this);
            connectedToPowerGrid = true;
        }
    }
    virtual public void SetActivationStatus(bool x, bool recalculateAfter)
    {
        isActive = x;
        if (connectedToPowerGrid & recalculateAfter)
        {
            GameMaster.realMaster.colonyController.RecalculatePowerGrid();
        }
        ChangeRenderersView( x & isEnergySupplied );
    }

    public virtual void SetEnergySupply(bool x, bool recalculateAfter)
    {
        isEnergySupplied = x;
        if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
        ChangeRenderersView(x & isActive);
    }

    protected void ChangeRenderersView(bool setOnline)
    {
        if (transform.childCount == 0) return;
        Renderer[] myRenderers = transform.GetChild(0).GetComponentsInChildren<Renderer>();
        if (myRenderers == null | myRenderers.Length == 0) return;
        if (setOnline == false)
        {
            for (int i = 0; i < myRenderers.Length; i++)
            {
                if (myRenderers[i].sharedMaterials.Length > 1)
                {
                    bool replacing = false;
                    Material[] newMaterials = new Material[myRenderers[i].sharedMaterials.Length];
                    for (int j = 0; j < myRenderers[i].sharedMaterials.Length; j++)
                    {
                        Material m = myRenderers[i].sharedMaterials[j];
                        if (m == PoolMaster.glass_material) { m = PoolMaster.glass_offline_material; replacing = true; }
                        else
                        {
                            if (m == PoolMaster.energy_material) { m = PoolMaster.energy_offline_material; replacing = true; }
                        }
                        newMaterials[j] = m;
                    }
                    if (replacing) myRenderers[i].sharedMaterials = newMaterials;
                }
                else
                {
                    Material m = myRenderers[i].sharedMaterial;
                    bool replacing = false;
                    if (m == PoolMaster.glass_material) { m = PoolMaster.glass_offline_material; replacing = true; }
                    else
                    {
                        if (m == PoolMaster.energy_material) { m = PoolMaster.energy_offline_material; replacing = true; }
                    }
                    if (replacing) myRenderers[i].sharedMaterial = m;
                }
            }
        }
        else
        { // Включение
            for (int i = 0; i < myRenderers.Length; i++)
            {
                if (myRenderers[i].sharedMaterials.Length > 1)
                {
                    bool replacing = false;
                    Material[] newMaterials = new Material[myRenderers[i].sharedMaterials.Length];
                    for (int j = 0; j < myRenderers[i].sharedMaterials.Length; j++)
                    {
                        Material m = myRenderers[i].sharedMaterials[j];
                        if (m == PoolMaster.glass_offline_material) { m = PoolMaster.glass_material; replacing = true; }
                        else
                        {
                            if (m == PoolMaster.energy_offline_material) { m = PoolMaster.energy_material; replacing = true; }
                        }
                        newMaterials[j] = m;
                    }
                    if (replacing) myRenderers[i].sharedMaterials = newMaterials;
                }
                else
                {
                    Material m = myRenderers[i].sharedMaterial;
                    bool replacing = false;
                    if (m == PoolMaster.glass_offline_material) { m = PoolMaster.glass_material; replacing = true; }
                    else
                    {
                        if (m == PoolMaster.energy_offline_material) { m = PoolMaster.energy_material; replacing = true; }
                    }
                    if (replacing) myRenderers[i].sharedMaterial = m;
                }
            }
        }
    }

    override public void SetVisibility(bool x)
    {
        if (x == visible) return;
        else
        {
            visible = x;
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(visible);
            }
        }
    }  

    public override UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
        return buildingObserver;
    }


    public virtual bool IsLevelUpPossible(ref string refusalReason)
    {
        if (level < GameMaster.realMaster.colonyController.hq.level) return true;
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.Unavailable);
            return false;
        }
    }
    public virtual void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!GameMaster.realMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                GameLogUI.NotEnoughResourcesAnnounce();
                return;
            }
        }
        Building upgraded = GetStructureByID(upgradedIndex) as Building;
        PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
        if (upgraded.innerPosition.size == 16) setPos = new PixelPosByte(0, 0);
        if (upgraded.rotate90only & (modelRotation % 2 != 0))
        {
            upgraded.modelRotation = (byte)(modelRotation - 1);
        }
        else upgraded.modelRotation = modelRotation;
        upgraded.SetBasement(basement, setPos);
        if (returnToUI) upgraded.ShowOnGUI();
    }
    public virtual ResourceContainer[] GetUpgradeCost()
    {
        if (upgradedIndex == -1) return null;
        ResourceContainer[] cost = ResourcesCost.GetCost(upgradedIndex);
        float discount = GameMaster.realMaster.upgradeDiscount;
        for (int i = 0; i < cost.Length; i++)
        {
            cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * (1 - discount));
        }
        return cost;
    }

    protected bool PrepareBuildingForDestruction(bool forced)
    {
        if (connectedToPowerGrid) GameMaster.realMaster.colonyController.DisconnectFromPowerGrid(this);
        return PrepareStructureForDestruction(forced);
    }
    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(forced);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SerializeStructure();
        data.AddRange(SerializeBuilding());
        return data;
    }

    protected List<byte> SerializeBuilding()
    {
        var data = new List<byte>() { isActive ? (byte)1 : (byte)0};
        data.AddRange(System.BitConverter.GetBytes(energySurplus));
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
    }
    protected void LoadBuildingData(System.IO.FileStream fs)
    {
        var data = new byte[BUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadBuildingData(data, 0);
    }
    protected void LoadBuildingData(byte[] data, int startIndex)
    {
        energySurplus = System.BitConverter.ToSingle(data, startIndex + 1);
        SetActivationStatus(data[startIndex] == 1, true);
    }   
    #endregion
}
