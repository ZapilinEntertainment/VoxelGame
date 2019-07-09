using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure
{
    private int _upgradedIndex = -1;
    public int upgradedIndex { get { return _upgradedIndex; } private set { _upgradedIndex = value; } }
    public bool canBePowerSwitched { get; protected set; }
    public bool isActive { get; protected set; }
    public bool isEnergySupplied { get; protected set; } //управляется только ColonyController'ом
    public bool connectedToPowerGrid { get; protected set; }// установлено ли подключение к электросети
    public float energySurplus { get; protected set; }
    public float energyCapacity { get; protected set; }
    private byte _level = 1;
    public byte level { get { return _level; } protected set { _level = value; } }
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
                blist.Add(GetStructureByID(PSYCHOKINECTIC_GENERATOR) as Building);
                blist.Add(GetStructureByID(STORAGE_1_ID) as Building);
                blist.Add(GetStructureByID(SETTLEMENT_CENTER_ID) as Building);
                blist.Add(GetStructureByID(FARM_1_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_1_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_1_ID) as Building);
                blist.Add(GetStructureByID(ENERGY_CAPACITOR_1_ID) as Building);
                blist.Add(GetStructureByID(MINE_ID) as Building);
                blist.Add(GetStructureByID(DOCK_ID) as Building);
                break;
            case 2:
                blist.Add(GetStructureByID(STORAGE_2_ID) as Building);
                //blist.Add(GetStructureByID(HOUSE_2_ID) as Building);
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
                if (Settlement.maxAchievedLevel >= 3) blist.Add(GetStructureByID(SETTLEMENT_CENTER_ID) as Building);
                blist.Add(GetStructureByID(FARM_3_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_3_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_3_ID) as Building);
                blist.Add(GetStructureByID(PLASTICS_FACTORY_3_ID) as Building);
                blist.Add(GetStructureByID(MINI_GRPH_REACTOR_3_ID) as Building);
                blist.Add(GetStructureByID(FUEL_FACILITY_3_ID) as Building);
                //blist.Add(GetStructureByID(XSTATION_3_ID) as Building);
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
                blist.Add(GetStructureByID(DOCK_ADDON_1_ID) as Building);
                blist.Add(GetStructureByID(ARTIFACTS_REPOSITORY_ID) as Building);
                blist.Add(GetStructureByID(CHEMICAL_FACTORY_4_ID) as Building);
                break;
            case 5:
                blist.Add(GetStructureByID(STORAGE_5_ID) as Building);                
                blist.Add(GetStructureByID(FARM_5_ID) as Building);
                blist.Add(GetStructureByID(LUMBERMILL_5_ID) as Building);
                blist.Add(GetStructureByID(SMELTERY_5_ID) as Building);
                blist.Add(GetStructureByID(SUPPLIES_FACTORY_5_ID) as Building);
                blist.Add(GetStructureByID(QUANTUM_ENERGY_TRANSMITTER_5_ID) as Building);
                blist.Add(GetStructureByID(FOUNDATION_BLOCK_5_ID) as Building);
                blist.Add(GetStructureByID(REACTOR_BLOCK_5_ID) as Building);
                blist.Add(GetStructureByID(OBSERVATORY_ID) as Building);
                blist.Add(GetStructureByID(MONUMENT_ID) as Building);
                break;
            case 6:
                if (Settlement.maxAchievedLevel >= 6)
                {
                    blist.Add(GetStructureByID(SETTLEMENT_CENTER_ID) as Building);
                    if (Settlement.maxAchievedLevel == Settlement.MAX_HOUSING_LEVEL)
                        blist.Add(GetStructureByID(HOUSE_BLOCK_ID) as Building);
                }
                blist.Add(GetStructureByID(CONNECT_TOWER_6_ID) as Building);
                blist.Add(GetStructureByID(CONTROL_CENTER_6_ID) as Building);
                //blist.Add(GetStructureByID(HOTEL_BLOCK_6_ID) as Building);
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
            case ENERGY_CAPACITOR_1_ID: return 1000f;
            case ENERGY_CAPACITOR_2_ID: return 4000f;

            case SETTLEMENT_CENTER_ID: return 100f;
            case HOUSE_BLOCK_ID: return 150f;
            case HOUSING_MAST_6_ID: return 1000f;

            case DOCK_ID: return 24f;
            case DOCK_2_ID: return 80f;
            case DOCK_3_ID: return 150f;

            case FARM_1_ID: return 10f;
            case FARM_2_ID: return 20f;
            case FARM_3_ID: return 40f;
            case FARM_4_ID: return 120f;
            case FARM_5_ID: return 300f;

            case LUMBERMILL_1_ID: return 10f;
            case LUMBERMILL_2_ID: return 40f;
            case LUMBERMILL_3_ID: return 80f;
            case LUMBERMILL_4_ID: return 80f;
            case LUMBERMILL_5_ID: return 160f;

            case SMELTERY_1_ID: return 40f;
            case SMELTERY_2_ID: return 60f;
            case SMELTERY_3_ID: return 100f;
            case SMELTERY_5_ID: return 160f;

            case SUPPLIES_FACTORY_4_ID: return 20f;
            case SUPPLIES_FACTORY_5_ID: return 80f;

            case FOUNDATION_BLOCK_5_ID:
            case WIND_GENERATOR_1_ID:            
                return 10f;
            case XSTATION_3_ID: return 12f;

            case EXPEDITION_CORPUS_4_ID:
            case RECRUITING_CENTER_4_ID:
            case GRPH_ENRICHER_3_ID:
            case MINE_ID:
            case MINERAL_POWERPLANT_2_ID:
            case WORKSHOP_ID:
            case BIOGENERATOR_2_ID: return 20f;

            case SHUTTLE_HANGAR_4_ID:
            case CHEMICAL_FACTORY_4_ID:
            case PLASTICS_FACTORY_3_ID:
            case ORE_ENRICHER_2_ID:
            case PSYCHOKINECTIC_GENERATOR:
                return 40f;

            case FUEL_FACILITY_3_ID:
            case HOSPITAL_2_ID: return 50f;

            case DOCK_ADDON_1_ID:
            case DOCK_ADDON_2_ID:
            case MINI_GRPH_REACTOR_3_ID: return 100f;

            case ARTIFACTS_REPOSITORY_ID: return 160f;

            case CONNECT_TOWER_6_ID: return 192f;

            case OBSERVATORY_ID:
            case REACTOR_BLOCK_5_ID: return 200f;

            case CONTROL_CENTER_6_ID: return 240f;

            case HOTEL_BLOCK_6_ID: return 300f;

            case MONUMENT_ID: return 800f;
            default: return 0;
        }
    }
    public static float GetEnergySurplus(int id)
    {
        switch (id)
        {
            case SETTLEMENT_CENTER_ID: return -2f;
            case SETTLEMENT_STRUCTURE_ID: return -0.5f;
            case HOUSE_BLOCK_ID: return -200f;
            case HOUSING_MAST_6_ID: return -120f;

            case DOCK_ID: return -10f;
            case DOCK_2_ID: return -20f;
            case DOCK_3_ID: return -30f;

            case FARM_1_ID: return -1f;
            case FARM_2_ID: return -4f;
            case FARM_3_ID: return -20f;
            case FARM_4_ID: return -40f;
            case FARM_5_ID: return -100f;

            case LUMBERMILL_1_ID: return -2f;
            case LUMBERMILL_2_ID: return -4f;
            case LUMBERMILL_3_ID: return -20f;
            case LUMBERMILL_4_ID: return -20f;
            case LUMBERMILL_5_ID: return -80f;

            case SUPPLIES_FACTORY_4_ID: return -25f;
            case SUPPLIES_FACTORY_5_ID: return -40f;

            case MINE_ID: return -2f;

            case SMELTERY_1_ID: return -8f;
            case SMELTERY_2_ID: return -15f;
            case SMELTERY_3_ID: return -40f;
            case SMELTERY_5_ID: return -80f;

            case HOSPITAL_2_ID: return -6f;
            case SHUTTLE_HANGAR_4_ID:
            case MONUMENT_ID:
            case XSTATION_3_ID: return -10f;
            case WORKSHOP_ID: return -8f;
            case EXPEDITION_CORPUS_4_ID:
            case RECRUITING_CENTER_4_ID:
            case ORE_ENRICHER_2_ID: return -12f;
            case ARTIFACTS_REPOSITORY_ID: return -16f;
            case CHEMICAL_FACTORY_4_ID:
            case PLASTICS_FACTORY_3_ID:
            case FUEL_FACILITY_3_ID: return -20f;
            case CONTROL_CENTER_6_ID: return -40f;
            case OBSERVATORY_ID: return -50f;
            case CONNECT_TOWER_6_ID: return -64f;
            case HOTEL_BLOCK_6_ID:
            case GRPH_ENRICHER_3_ID: return -70f;


            case MINERAL_POWERPLANT_2_ID: return Powerplant.MINERAL_F_PP_OUTPUT;
            case FOUNDATION_BLOCK_5_ID:
            case WIND_GENERATOR_1_ID:
                return 10f;
            case BIOGENERATOR_2_ID: return Powerplant.BIOGEN_OUTPUT;
            case MINI_GRPH_REACTOR_3_ID: return 100f;
            case GRPH_REACTOR_4_ID: return Powerplant.GRPH_REACTOR_OUTPUT;
            case REACTOR_BLOCK_5_ID: return Powerplant.REACTOR_BLOCK_5_OUTPUT;

            default: return 0;
        }
    }

    override public void Prepare() { PrepareBuilding(); }

    protected void ChangeUpgradedIndex(int x)
    {
        upgradedIndex = x;
    }

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

        upgradedIndex = -1;
        canBePowerSwitched = false;
        canBePowerSwitched = false;
        energySurplus = GetEnergySurplus(ID);
        energyCapacity = GetEnergyCapacity(ID);
        level = 1;

        switch (ID)
        {
            case HEADQUARTERS_ID:
                {
                    upgradedIndex = 0;
                    break;
                }
            case STORAGE_0_ID:
                {
                    level = 0;
                    upgradedIndex = STORAGE_1_ID;
                }
                break;
            case STORAGE_1_ID:
                {
                    level = 1;
                    upgradedIndex = STORAGE_2_ID;
                }
                break;
            case STORAGE_2_ID:
                {
                   // upgradedIndex = STORAGE_3_ID;
                    level = 2;
                }
                break;
            case STORAGE_5_ID:
                {
                    level = 5;
                }
                break;
            case TENT_ID:
                {
                    level = 0;
                }
                break;
            case HOUSE_BLOCK_ID:
                {
                    level = 5;
                }
                break;
            case DOCK_ID:
                {
                    level = 1;
                    canBePowerSwitched = true;
                }
                break;
            case DOCK_2_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case DOCK_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case ENERGY_CAPACITOR_1_ID:
                {
                    upgradedIndex = ENERGY_CAPACITOR_2_ID;
                    level = 1;
                }
                break;
            case ENERGY_CAPACITOR_2_ID:
                {
                    level = 2;
                }
                break;
            case FARM_1_ID:
                {
                    upgradedIndex = FARM_2_ID;
                    canBePowerSwitched = true;
                    specialBuildingConditions = true;
                }
                break;
            case FARM_2_ID:
                {
                    upgradedIndex = FARM_3_ID;
                    canBePowerSwitched = true;
                    specialBuildingConditions = true;
                    level = 2;
                }
                break;
            case FARM_3_ID:
                {
                    upgradedIndex = FARM_4_ID;
                    canBePowerSwitched = true;
                    specialBuildingConditions = true;
                    level = 3;
                }
                break;
            case FARM_4_ID:
                {
                    upgradedIndex = FARM_5_ID;
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case FARM_5_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    upgradedIndex = LUMBERMILL_2_ID;
                    canBePowerSwitched = true;
                    specialBuildingConditions = true;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    upgradedIndex = LUMBERMILL_3_ID;
                    canBePowerSwitched = true;
                    specialBuildingConditions = true;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    upgradedIndex = LUMBERMILL_4_ID;
                    canBePowerSwitched = true;
                    specialBuildingConditions = true;
                    level = 3;
                }
                break;
            case LUMBERMILL_4_ID:
                {
                    upgradedIndex = LUMBERMILL_5_ID;
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case LUMBERMILL_5_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case MINE_ID:
                {
                    upgradedIndex = 0;
                    canBePowerSwitched = true;
                    level = 1;
                }
                break;
            case SMELTERY_1_ID:
                {
                    upgradedIndex = SMELTERY_2_ID;
                    canBePowerSwitched = true;
                }
                break;
            case SMELTERY_2_ID:
                {
                    upgradedIndex = SMELTERY_3_ID;
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case SMELTERY_3_ID:
                {
                    upgradedIndex = SMELTERY_5_ID;
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case SMELTERY_5_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    level = 2;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case MINERAL_POWERPLANT_2_ID:
                {
                    level = 2;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case WORKSHOP_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case MINI_GRPH_REACTOR_3_ID:
                {
                    level = 3;
                }
                break;
            case FUEL_FACILITY_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    level = 4;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    upgradedIndex = SUPPLIES_FACTORY_5_ID;
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case XSTATION_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case CHEMICAL_FACTORY_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case SWITCH_TOWER_ID:
                {
                    level = 6;
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case QUANTUM_TRANSMITTER_4_ID:
                {
                    level = 4;
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    level = 5;
                }
                break;
            case FOUNDATION_BLOCK_5_ID:
                {
                    level = 5;
                }
                break;
            case CONNECT_TOWER_6_ID:
                {
                    specialBuildingConditions = true;
                    canBePowerSwitched = true;
                    level = 6;
                }
                break;
            case CONTROL_CENTER_6_ID:
                {
                    specialBuildingConditions = true;
                    canBePowerSwitched = true;
                    level = 6;
                }
                break;
            case HOTEL_BLOCK_6_ID:
                {
                    canBePowerSwitched = true;
                    level = 6;
                }
                break;
            case HOUSING_MAST_6_ID:
                {
                    specialBuildingConditions = true;
                    level = 6;
                }
                break;
            case DOCK_ADDON_2_ID:
                {
                    level = 2;
                }
                break;
            case OBSERVATORY_ID:
                {
                    specialBuildingConditions = true;
                    canBePowerSwitched = true;
                    level = 5;
                    break;
                }
            case ARTIFACTS_REPOSITORY_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                    break;
                }
            case MONUMENT_ID:
                {
                    level = 5;
                    break;
                }
            case SETTLEMENT_CENTER_ID:
                {
                    level = 1;
                    upgradedIndex = 0;
                    break;
                }
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
        ChangeRenderersView(x & isEnergySupplied);
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
                        if (m == PoolMaster.glassMaterial) { m = PoolMaster.glassMaterial_disabled; replacing = true; }
                        else
                        {
                            if (m == PoolMaster.energyMaterial) { m = PoolMaster.energyMaterial_disabled; replacing = true; }
                        }
                        newMaterials[j] = m;
                    }
                    if (replacing) myRenderers[i].sharedMaterials = newMaterials;
                }
                else
                {
                    Material m = myRenderers[i].sharedMaterial;
                    bool replacing = false;
                    if (m == PoolMaster.glassMaterial) { m = PoolMaster.glassMaterial_disabled; replacing = true; }
                    else
                    {
                        if (m == PoolMaster.energyMaterial) { m = PoolMaster.energyMaterial_disabled; replacing = true; }
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
                        if (m == PoolMaster.glassMaterial_disabled) { m = PoolMaster.glassMaterial; replacing = true; }
                        else
                        {
                            if (m == PoolMaster.energyMaterial_disabled) { m = PoolMaster.energyMaterial; replacing = true; }
                        }
                        newMaterials[j] = m;
                    }
                    if (replacing) myRenderers[i].sharedMaterials = newMaterials;
                }
                else
                {
                    Material m = myRenderers[i].sharedMaterial;
                    bool replacing = false;
                    if (m == PoolMaster.glassMaterial_disabled) { m = PoolMaster.glassMaterial; replacing = true; }
                    else
                    {
                        if (m == PoolMaster.energyMaterial_disabled) { m = PoolMaster.energyMaterial; replacing = true; }
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
        PixelPosByte setPos = new PixelPosByte(surfaceRect.x, surfaceRect.z);
        if (upgraded.surfaceRect.size == 16) setPos = new PixelPosByte(0, 0);
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

    protected void PrepareBuildingForDestruction(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (connectedToPowerGrid) GameMaster.realMaster.colonyController.DisconnectFromPowerGrid(this);
        PrepareStructureForDestruction(clearFromSurface, returnResources, leaveRuins);
    }
    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {        
        var data = SaveStructureData();
        data.AddRange(SaveBuildingData());
        return data;
        // copied to Settlement.Save()
        // copy to PsychokineticGenerator.Save()
    }

    protected List<byte> SaveBuildingData()
    {
        // copied to Settlement.Save()        
        var data = new List<byte>() { isActive ? (byte)1 : (byte)0 };
        data.AddRange(System.BitConverter.GetBytes(energySurplus));
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {        
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
        // changed in Settlement.Load()
        // copy to PsychokineticGenerator.Load()        
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
        //copy to HeadQuarters
    }
    #endregion
}