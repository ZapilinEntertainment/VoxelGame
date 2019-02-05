using UnityEngine;
using System.Collections.Generic;
public class Structure : MonoBehaviour
{
    public SurfaceBlock basement { get; protected set; }
    public SurfaceRect innerPosition { get; protected set; }
    public bool isArtificial { get; protected set; } // fixed in ID - используется при проверках на снос
    public bool isBasement { get; private set; } // fixed in ID
    public bool placeInCenter { get; private set; } // fixed in ID 
    public bool indestructible { get; protected set; }  // fixed in ID
    public float hp { get; protected set; }
    public float maxHp { get; protected set; } // fixed in ID
    public bool rotate90only { get; private set; } // fixed in ID
    public bool showOnGUI { get; protected set; }
    public int id { get; protected set; }
    public bool visible { get; protected set; }
    public byte modelRotation { get; protected set; }

    protected bool destroyed = false, subscribedToUpdate = false, subscribedToChunkUpdate = false;
    protected uint skinIndex = 0;

    //проверь при добавлении
    //- ID
    // - get structure by id
    // -set model - загрузка модели
    // -prepare - установка inner position
    // - localization - name & description
    // - texture rect
    // -building - get applicable buildings list
    // -resource cost
    // score calculator
    public const int PLANT_ID = 1, DRYED_PLANT_ID = 2, RESOURCE_STICK_ID = 3, LANDED_ZEPPELIN_ID = 5,
    TREE_OF_LIFE_ID = 6, STORAGE_0_ID = 7, CONTAINER_ID = 8, MINE_ELEVATOR_ID = 9, LIFESTONE_ID = 10, TENT_ID = 11,
    DOCK_ID = 13, ENERGY_CAPACITOR_1_ID = 14, FARM_1_ID = 15, HQ_2_ID = 16, LUMBERMILL_1_ID = 17, MINE_ID = 18, SMELTERY_1_ID = 19,
    WIND_GENERATOR_1_ID = 20, BIOGENERATOR_2_ID = 22, HOSPITAL_2_ID = 21, MINERAL_POWERPLANT_2_ID = 23, ORE_ENRICHER_2_ID = 24,
    WORKSHOP_ID = 25, MINI_GRPH_REACTOR_3_ID = 26, FUEL_FACILITY_3_ID = 27, GRPH_REACTOR_4_ID = 28, PLASTICS_FACTORY_3_ID = 29,
    SUPPLIES_FACTORY_4_ID = 30, GRPH_ENRICHER_3_ID = 31, XSTATION_3_ID = 32, QUANTUM_ENERGY_TRANSMITTER_5_ID = 33, CHEMICAL_FACTORY_4_ID = 34,
    STORAGE_1_ID = 35, STORAGE_2_ID = 36, STORAGE_3_ID = 37, STORAGE_5_ID = 38, HOUSE_1_ID = 39, HOUSE_2_ID = 40, HOUSE_3_ID = 41,
    HOUSE_5_ID = 42, ENERGY_CAPACITOR_2_ID = 43, ENERGY_CAPACITOR_3_ID = 44, FARM_2_ID = 45, FARM_3_ID = 46, FARM_4_ID = 47, FARM_5_ID = 48,
    LUMBERMILL_2_ID = 49, LUMBERMILL_3_ID = 50, LUMBERMILL_4_ID = 51, LUMBERMILL_5_ID = 52, SUPPLIES_FACTORY_5_ID = 53, SMELTERY_2_ID = 54,
    SMELTERY_3_ID = 55, SMELTERY_5_ID = 57, HQ_3_ID = 58, HQ_4_ID = 59, QUANTUM_TRANSMITTER_4_ID = 60,
    COLUMN_ID = 61, SWITCH_TOWER_ID = 62, SHUTTLE_HANGAR_4_ID = 63,
    RECRUITING_CENTER_4_ID = 64, EXPEDITION_CORPUS_4_ID = 65, REACTOR_BLOCK_5_ID = 66, FOUNDATION_BLOCK_5_ID = 67, CONNECT_TOWER_6_ID = 68,
        CONTROL_CENTER_6_ID = 69, HOTEL_BLOCK_6_ID = 70, HOUSING_MAST_6_ID = 71, DOCK_ADDON_1_ID = 72, DOCK_ADDON_2_ID = 73, DOCK_2_ID = 74, DOCK_3_ID = 75,
        OBSERVATORY_ID = 76;
    public const int TOTAL_STRUCTURES_COUNT = 77, STRUCTURE_SERIALIZER_LENGTH = 16;
    public const string STRUCTURE_COLLIDER_TAG = "Structure";

    public static UIStructureObserver structureObserver;
    private static bool firstLaunch = true;


    public static void ResetToDefaults_Static()
    {
        if (firstLaunch)
        {
            firstLaunch = false;
            return;
        }
        OakTree.ResetToDefaults_Static_OakTree();
        Corn.ResetToDefaults_Static_Corn();
        Hospital.ResetToDefaults_Static_Hospital();
        Dock.ResetToDefaults_Static_Dock();
        RecruitingCenter.ResetToDefaults_Static_RecruitingCenter();
        QuantumTransmitter.ResetToDefaults_Static_QuantumTransmitter();
        Hangar.ResetToDefaults_Static_Hangar();
        ExpeditionCorpus.ResetToDefaults_Static_ExpeditionCorpus();
        Observatory.ResetBuiltMarker();
    }

    virtual protected void SetModel()
    {
        //switch skin index
        GameObject model;
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        switch (id)
        {
            default: model = GameObject.CreatePrimitive(PrimitiveType.Cube); break;
            case DRYED_PLANT_ID:
                model = Instantiate(Resources.Load<GameObject>("Structures/dryedPlant"));
                break;
            case RESOURCE_STICK_ID: model = Instantiate(Resources.Load<GameObject>("Structures/resourcesStick")); break;
            case LANDED_ZEPPELIN_ID: model = Instantiate(Resources.Load<GameObject>("Structures/ZeppelinBasement")); break;
            case TREE_OF_LIFE_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Tree of Life")); break;
            case STORAGE_0_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Storage_level_0")); break;
            case STORAGE_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_1")); break;
            case STORAGE_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_2")); break;
            case STORAGE_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_3")); break;
            case STORAGE_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/storageBlock_level_5")); break;
            case MINE_ELEVATOR_ID: model = Instantiate(Resources.Load<GameObject>("Structures/MineElevator")); break;
            case LIFESTONE_ID: model = Instantiate(Resources.Load<GameObject>("Structures/LifeStone")); break;
            case TENT_ID: model = Instantiate(Resources.Load<GameObject>("Structures/House_level_0")); break;
            case HOUSE_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/House_level_1")); break;
            case HOUSE_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/House_level_2")); break;
            case HOUSE_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/House_level_3")); break;
            case HOUSE_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/houseBlock_level_5")); break;
            case DOCK_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_level_1")); break;
            case DOCK_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_level_2")); break;
            case DOCK_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_level_3")); break;
            case ENERGY_CAPACITOR_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/EnergyCapacitor_level_1")); break;
            case ENERGY_CAPACITOR_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/EnergyCapacitor_level_2")); break;
            case ENERGY_CAPACITOR_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/EnergyCapacitor_level_3")); break;
            case FARM_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_1")); break;
            case FARM_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_2")); break;
            case FARM_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_3")); break;
            case FARM_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_4")); break;
            case FARM_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/farmBlock_level_5")); break;
            case HQ_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/HQ_level_2")); break;
            case HQ_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/HQ_level_3")); break;
            case HQ_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/HQ_level_4")); break;
            case LUMBERMILL_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_1")); break;
            case LUMBERMILL_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_2")); break;
            case LUMBERMILL_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_3")); break;
            case LUMBERMILL_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_4")); break;
            case LUMBERMILL_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/lumbermillBlock_level_5")); break;
            case MINE_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Mine_level_1")); break;
            case SMELTERY_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Smeltery_level_1")); break;
            case SMELTERY_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Smeltery_level_2")); break;
            case SMELTERY_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Smeltery_level_3")); break;
            case SMELTERY_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/smelteryBlock_level_5")); break;
            case WIND_GENERATOR_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/windGenerator_level_1")); break;
            case BIOGENERATOR_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Biogenerator_level_2")); break;
            case HOSPITAL_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Hospital_level_2")); break;
            case MINERAL_POWERPLANT_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/mineralPP_level_2")); break;
            case ORE_ENRICHER_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/oreEnricher_level_2")); break;
            case WORKSHOP_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/rollingShop_level_2")); break;
            case MINI_GRPH_REACTOR_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/miniReactor_level_3")); break;
            case FUEL_FACILITY_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/fuelFacility_level_3")); break;
            case GRPH_REACTOR_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/graphoniumReactor_level_4")); break;
            case PLASTICS_FACTORY_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/plasticsFactory_level_3")); break;
            case SUPPLIES_FACTORY_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/foodFactory_level_4")); break;
            case SUPPLIES_FACTORY_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/foodFactoryBlock_level_5")); break;
            case GRPH_ENRICHER_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/graphoniumEnricher_level_3")); break;
            case XSTATION_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/XStation_level_3")); break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/quantumEnergyTransmitter_level_4")); break;
            case CHEMICAL_FACTORY_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/chemicalFactory_level_4")); break;
            case COLUMN_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Column")); break;
            case SWITCH_TOWER_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/switchTower")); break;
            case SHUTTLE_HANGAR_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/shuttleHangar")); break;
            case RECRUITING_CENTER_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/recruitingCenter")); break;
            case EXPEDITION_CORPUS_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/expeditionCorpus")); break;
            case QUANTUM_TRANSMITTER_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/quantumTransmitter")); break;
            case REACTOR_BLOCK_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/reactorBlock")); break;
            case FOUNDATION_BLOCK_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/foundationBlock")); break;
            case CONNECT_TOWER_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/connectTower")); break;
            case CONTROL_CENTER_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/controlCenter")); break;
            case HOTEL_BLOCK_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/hotelBlock")); break;
            case HOUSING_MAST_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/housingMast")); break;
            case DOCK_ADDON_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_addon1")); break;
            case DOCK_ADDON_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_addon2")); break;
            case OBSERVATORY_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/observatory")); break;
        }
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
    }

    virtual public void SetModelRotation(int r)
    {
        if (r > 7) r %= 8;
        else
        {
            if (r < 0) r += 8;
        }
        modelRotation = (byte)r;

        if (basement != null)
        {
            transform.localRotation = Quaternion.AngleAxis(modelRotation * 45, Vector3.up);
        }
    }
    public void SetHP(float t)
    {
        hp = t;
        if (hp == 0) Annihilate(false);
    }

    public static Structure GetStructureByID(int i_id)
    {
        Structure s;
        switch (i_id)
        {
            case PLANT_ID: print("error : cannot create plant via GetStructureById"); return null;
            case DRYED_PLANT_ID:
                s = new GameObject("dryed plant").AddComponent<Structure>(); break;
            case COLUMN_ID:
                s = new GameObject("Column").AddComponent<Structure>(); break;
            case RESOURCE_STICK_ID:
                s = new GameObject("Scalable harvestable resource").AddComponent<ScalableHarvestableResource>(); break;
            case MINE_ELEVATOR_ID:
                s = new GameObject("Mine elevator").AddComponent<MineElevator>(); break;
            case LANDED_ZEPPELIN_ID:
            case HQ_2_ID:
            case HQ_3_ID:
            case HQ_4_ID:
                s = new GameObject("HQ").AddComponent<HeadQuarters>(); break;
            case TREE_OF_LIFE_ID:
            case LIFESTONE_ID:
                s = new GameObject("Lifestone").AddComponent<LifeSource>(); break;
            case STORAGE_0_ID:
            case STORAGE_1_ID:
            case STORAGE_2_ID:
            case STORAGE_3_ID:
            case STORAGE_5_ID:
                s = new GameObject("Storage").AddComponent<StorageHouse>(); break;
            case CONTAINER_ID:
                //use HarvestableResource.ConstructContainer instead
                s = new GameObject("Container").AddComponent<HarvestableResource>(); break;
            case TENT_ID:
            case HOUSE_1_ID:
            case HOUSE_2_ID:
            case HOUSE_3_ID:
            case HOUSE_5_ID:
                s = new GameObject("House").AddComponent<House>(); break;
            case DOCK_ID:
            case DOCK_2_ID:
            case DOCK_3_ID:
                s = new GameObject("Dock").AddComponent<Dock>(); break;
            case FARM_1_ID:
            case FARM_2_ID:
            case FARM_3_ID:
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
                s = new GameObject("Farm").AddComponent<Farm>(); break;
            case LUMBERMILL_4_ID:
            case LUMBERMILL_5_ID:
            case FARM_4_ID:
            case FARM_5_ID:
                s = new GameObject("CoveredFarm").AddComponent<CoveredFarm>(); break;
            case MINE_ID:
                s = new GameObject("Mine").AddComponent<Mine>(); break;
            case SMELTERY_1_ID:
            case SMELTERY_2_ID:
            case SMELTERY_3_ID:
            case SMELTERY_5_ID:
            case ORE_ENRICHER_2_ID:
            case PLASTICS_FACTORY_3_ID:
            case FUEL_FACILITY_3_ID:
                s = new GameObject("Factory").AddComponent<Factory>(); break;
            case SUPPLIES_FACTORY_4_ID:
            case SUPPLIES_FACTORY_5_ID:
                s = new GameObject().AddComponent<FoodFactory>(); break;
            case WIND_GENERATOR_1_ID:
                s = new GameObject().AddComponent<WindGenerator>(); break;
            case BIOGENERATOR_2_ID:
            case MINERAL_POWERPLANT_2_ID:
            case GRPH_REACTOR_4_ID:
                s = new GameObject("Powerplant").AddComponent<Powerplant>(); break;
            case HOSPITAL_2_ID:
                s = new GameObject("Hospital").AddComponent<Hospital>(); break;
            case WORKSHOP_ID:
                s = new GameObject("Rolling Shop").AddComponent<Workshop>(); break;
            case MINI_GRPH_REACTOR_3_ID:
            case ENERGY_CAPACITOR_1_ID:
            case ENERGY_CAPACITOR_2_ID:
            case ENERGY_CAPACITOR_3_ID:
                s = new GameObject("Energy capacitor").AddComponent<Building>(); break;
            case GRPH_ENRICHER_3_ID:
                s = new GameObject("Graphonium Enricher").AddComponent<Factory>(); break;
            case XSTATION_3_ID:
                s = new GameObject("XStation").AddComponent<XStation>(); break; // AWAITING
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: s = new GameObject("Quantum energy transmitter").AddComponent<QuantumEnergyTransmitter>(); break;
            case CHEMICAL_FACTORY_4_ID:
                s = new GameObject("Chemical factory").AddComponent<ChemicalFactory>(); break; // AWAITING
            case QUANTUM_TRANSMITTER_4_ID:
                s = new GameObject("Quantum transmitter").AddComponent<QuantumTransmitter>(); break;
            case SWITCH_TOWER_ID:
                s = new GameObject("Switch tower").AddComponent<SwitchTower>(); break;
            case SHUTTLE_HANGAR_4_ID:
                s = new GameObject("Shuttle hangar").AddComponent<Hangar>(); break;
            case RECRUITING_CENTER_4_ID:
                s = new GameObject("Recruiting center").AddComponent<RecruitingCenter>(); break;
            case EXPEDITION_CORPUS_4_ID:
                s = new GameObject("Expedition corpus").AddComponent<ExpeditionCorpus>(); break;
            case REACTOR_BLOCK_5_ID:
                s = new GameObject("Reactor block").AddComponent<Powerplant>(); break;
            case FOUNDATION_BLOCK_5_ID:
                s = new GameObject("Foundation block").AddComponent<Building>(); break;
            case CONNECT_TOWER_6_ID:
                s = new GameObject("Connect tower").AddComponent<ConnectTower>(); break;
            case CONTROL_CENTER_6_ID:
                s = new GameObject("Command center").AddComponent<ControlCenter>(); break;
            case HOTEL_BLOCK_6_ID:
                s = new GameObject("Hotel block").AddComponent<House>(); break; // AWAITING
            case HOUSING_MAST_6_ID:
                s = new GameObject("Housing mast").AddComponent<House>(); break;
            case DOCK_ADDON_1_ID:
                s = new GameObject("Dock Addon 1").AddComponent<DockAddon>(); break;
            case DOCK_ADDON_2_ID:
                s = new GameObject("Dock Addon 2").AddComponent<DockAddon>(); break;
            case OBSERVATORY_ID:
                s = new GameObject("Observatory").AddComponent<Observatory>();break;
            default: return null;
        }
        s.id = i_id;
        s.Prepare();
        return s;
    }

    virtual public void Prepare()
    {
        PrepareStructure();
    }
    /// <summary>
    /// do not use directly, use Prepare() instead
    /// </summary>
    /// <param name="i_id"></param>
    /// <returns></returns>
    // в финальном виде копипастить в потомков
    protected void PrepareStructure()
    {
        indestructible = false;
        switch (id)
        {
            case PLANT_ID:
                {
                    maxHp = 1;
                    innerPosition = SurfaceRect.one;
                    isArtificial = false;
                    isBasement = false;
                    placeInCenter = false;
                    rotate90only = false;
                }
                break;
            case DRYED_PLANT_ID:
                {
                    maxHp = 1;
                    innerPosition = SurfaceRect.one;
                    isArtificial = false;
                    isBasement = false;
                    placeInCenter = false;
                    rotate90only = false;
                }
                break;
            case RESOURCE_STICK_ID:
                {
                    maxHp = SurfaceBlock.INNER_RESOLUTION;
                    innerPosition = new SurfaceRect(0, 0, 2);
                    isArtificial = false;
                    isBasement = false;
                    placeInCenter = false;
                    rotate90only = true;
                }
                break;
            case LANDED_ZEPPELIN_ID:
                {
                    maxHp = 1000;
                    innerPosition = SurfaceRect.full;
                    isArtificial = true;
                    isBasement = false;
                    placeInCenter = true;
                    rotate90only = true;
                    indestructible = true;
                }
                break;
            case TREE_OF_LIFE_ID:
                {
                    maxHp = LifeSource.MAX_HP;
                    innerPosition = SurfaceRect.full;
                    isArtificial = false;
                    isBasement = false;
                    placeInCenter = true;
                    rotate90only = false;
                    indestructible = true;
                }
                break;
            case STORAGE_0_ID:
                {
                    maxHp = 750;
                    innerPosition = SurfaceRect.full;
                    isArtificial = true;
                    isBasement = false;
                    placeInCenter = true;
                    rotate90only = true;
                    indestructible = true;
                }
                break;
            case STORAGE_1_ID:
                {
                    maxHp = 500;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    isArtificial = true;
                    isBasement = false;
                    placeInCenter = false;
                    rotate90only = false;
                }
                break;
            case STORAGE_2_ID:
                {
                    maxHp = 700;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    isArtificial = true;
                    isBasement = false;
                    placeInCenter = false;
                    rotate90only = false;
                }
                break;
            case STORAGE_3_ID:
                {
                    maxHp = 1000;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case STORAGE_5_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case CONTAINER_ID:
                {
                    maxHp = 10;
                    innerPosition = SurfaceRect.one;
                    placeInCenter = false;
                    rotate90only = false;
                    isArtificial = false;
                    isBasement = false;
                }
                break;
            case MINE_ELEVATOR_ID:
                {
                    maxHp = 100;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case LIFESTONE_ID:
                {
                    maxHp = LifeSource.MAX_HP;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = false;
                    isBasement = false;
                    indestructible = true;
                }
                break;
            case TENT_ID:
                {
                    maxHp = 5;
                    innerPosition = SurfaceRect.one;
                    placeInCenter = false;
                    rotate90only = false;
                    isArtificial = false;
                    isBasement = false;
                }
                break;
            case HOUSE_1_ID:
                {
                    maxHp = 100;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case HOUSE_2_ID:
                {
                    maxHp = 500;
                    innerPosition = new SurfaceRect(0, 0, 6);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case HOUSE_3_ID:
                {
                    maxHp = 1000;
                    innerPosition = new SurfaceRect(0, 0, 6);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case HOUSE_5_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case DOCK_ID:
                {
                    maxHp = 1200;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case ENERGY_CAPACITOR_1_ID:
                {
                    maxHp = 200;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case ENERGY_CAPACITOR_2_ID:
                {
                    maxHp = 400;
                    innerPosition = new SurfaceRect(0, 0, 7);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case ENERGY_CAPACITOR_3_ID:
                {
                    maxHp = 1000;
                    innerPosition = new SurfaceRect(0, 0, 7);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FARM_1_ID:
                {
                    maxHp = 500;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FARM_2_ID:
                {
                    maxHp = 800;
                    innerPosition = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FARM_3_ID:
                {
                    maxHp = 1500;
                    innerPosition = new SurfaceRect(0, 0, 8);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FARM_4_ID:
                {
                    maxHp = 2000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FARM_5_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case HQ_2_ID:
                {
                    maxHp = 2000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                    indestructible = true;
                }
                break;
            case HQ_3_ID:
                {
                    maxHp = 3000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                    indestructible = true;
                }
                break;
            case HQ_4_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                    indestructible = true;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    maxHp = 500;
                    innerPosition = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    maxHp = 750;
                    innerPosition = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    maxHp = 1000;
                    innerPosition = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case LUMBERMILL_4_ID:
                {
                    maxHp = 1200;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case LUMBERMILL_5_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case MINE_ID:
                {
                    maxHp = 500;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SMELTERY_1_ID:
                {
                    maxHp = 800;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SMELTERY_2_ID:
                {
                    maxHp = 1200;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SMELTERY_3_ID:
                {
                    maxHp = 1800;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SMELTERY_5_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case WIND_GENERATOR_1_ID:
                {
                    maxHp = 700;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    maxHp = 1200;
                    innerPosition = new SurfaceRect(0, 0, 8);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    maxHp = 1500;
                    innerPosition = new SurfaceRect(0, 0, 8);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case MINERAL_POWERPLANT_2_ID:
                {
                    maxHp = 1100;
                    innerPosition = new SurfaceRect(0, 0, 8);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    maxHp = 1500;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case WORKSHOP_ID:
                {
                    maxHp = 1200;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case MINI_GRPH_REACTOR_3_ID:
                {
                    maxHp = 800;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FUEL_FACILITY_3_ID:
                {
                    maxHp = 1250;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    maxHp = 2700;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    maxHp = 1600;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    maxHp = 1500;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    maxHp = 4000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    maxHp = 1400;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case XSTATION_3_ID:
                {
                    maxHp = 1700;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID:
                {
                    maxHp = 2100;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case CHEMICAL_FACTORY_4_ID:
                {
                    maxHp = 2000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case COLUMN_ID:
                {
                    maxHp = 2200;
                    innerPosition = new SurfaceRect(0, 0, 2);
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case SWITCH_TOWER_ID:
                {
                    maxHp = 250;
                    innerPosition = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    maxHp = 1200;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    maxHp = 1600;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    maxHp = 1800;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case QUANTUM_TRANSMITTER_4_ID:
                {
                    maxHp = 1100;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    maxHp = 3700;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case FOUNDATION_BLOCK_5_ID:
                {
                    maxHp = 8000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case CONNECT_TOWER_6_ID:
                {
                    maxHp = 1700;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case CONTROL_CENTER_6_ID:
                {
                    maxHp = 5000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case HOTEL_BLOCK_6_ID:
                {
                    maxHp = 2700;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = true;
                }
                break;
            case HOUSING_MAST_6_ID:
                {
                    maxHp = 5500;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case DOCK_ADDON_1_ID:
                {
                    maxHp = 2000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case DOCK_ADDON_2_ID:
                {
                    maxHp = 2200;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case DOCK_2_ID:
                {
                    maxHp = 2400;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case DOCK_3_ID:
                {
                    maxHp = 5000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case OBSERVATORY_ID:
                {
                    maxHp = 5000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    isBasement = false;                    
                }
                break;
        }
        hp = maxHp;
    }

    public static Rect GetTextureRect(int f_id)
    {
        float p = 0.125f;
        switch (f_id)
        {
            default: return Rect.zero;
            case DRYED_PLANT_ID:
            case PLANT_ID: return new Rect(p, 7 * p, p, p);
            case HQ_4_ID:
            case HQ_3_ID:
            case HQ_2_ID:
            case LANDED_ZEPPELIN_ID: return new Rect(2 * p, 7 * p, p, p);
            case LIFESTONE_ID:
            case TREE_OF_LIFE_ID: return new Rect(3 * p, 7 * p, p, p);
            case STORAGE_1_ID:
            case STORAGE_2_ID:
            case STORAGE_3_ID:
            case STORAGE_5_ID:
            case STORAGE_0_ID: return new Rect(5 * p, 7 * p, p, p);
            case CONTAINER_ID: return new Rect(4 * p, 7 * p, p, p);
            case MINE_ID:
            case MINE_ELEVATOR_ID: return new Rect(6 * p, 7 * p, p, p);
            case HOUSE_1_ID:
            case HOUSE_2_ID:
            case HOUSE_3_ID:
            case HOUSE_5_ID:
            case TENT_ID: return new Rect(7 * p, 7 * p, p, p);
            case DOCK_ID: return new Rect(0, 6 * p, p, p);
            case ENERGY_CAPACITOR_3_ID:
            case ENERGY_CAPACITOR_2_ID:
            case ENERGY_CAPACITOR_1_ID: return new Rect(p, 6 * p, p, p);
            case FARM_2_ID:
            case FARM_3_ID:
            case FARM_4_ID:
            case FARM_5_ID:
            case FARM_1_ID: return new Rect(2 * p, 6 * p, p, p);
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case LUMBERMILL_4_ID:
            case LUMBERMILL_5_ID:
            case LUMBERMILL_1_ID: return new Rect(3 * p, 6 * p, p, p);
            case SMELTERY_2_ID:
            case SMELTERY_3_ID:
            case SMELTERY_5_ID:
            case SMELTERY_1_ID: return new Rect(4 * p, 6 * p, p, p);
            case WIND_GENERATOR_1_ID: return new Rect(5 * p, 6 * p, p, p);
            case BIOGENERATOR_2_ID: return new Rect(6 * p, 6 * p, p, p);
            case HOSPITAL_2_ID: return new Rect(7 * p, 6 * p, p, p);
            case MINERAL_POWERPLANT_2_ID: return new Rect(0, 5 * p, p, p);
            case ORE_ENRICHER_2_ID: return new Rect(p, 5 * p, p, p);
            case WORKSHOP_ID: return new Rect(2 * p, 5 * p, p, p);
            case MINI_GRPH_REACTOR_3_ID: return new Rect(3 * p, 5 * p, p, p);
            case FUEL_FACILITY_3_ID: return new Rect(4 * p, 5 * p, p, p);
            case GRPH_REACTOR_4_ID: return new Rect(5 * p, 5 * p, p, p);
            case PLASTICS_FACTORY_3_ID: return new Rect(6 * p, 5 * p, p, p);
            case SUPPLIES_FACTORY_5_ID:
            case SUPPLIES_FACTORY_4_ID: return new Rect(7 * p, 5 * p, p, p);
            case GRPH_ENRICHER_3_ID: return new Rect(0, 4 * p, p, p);
            case XSTATION_3_ID: return new Rect(p, 4 * p, p, p);
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: return new Rect(2 * p, 4 * p, p, p);
            case CHEMICAL_FACTORY_4_ID: return new Rect(3 * p, 4 * p, p, p);
            case RESOURCE_STICK_ID: return new Rect(4 * p, 4 * p, p, p);
            case COLUMN_ID: return new Rect(5 * p, 4 * p, p, p);
            case SWITCH_TOWER_ID: return new Rect(6 * p, 4 * p, p, p);
            case SHUTTLE_HANGAR_4_ID: return new Rect(7 * p, 4 * p, p, p);
            case RECRUITING_CENTER_4_ID: return new Rect(0, 3 * p, p, p);
            case EXPEDITION_CORPUS_4_ID: return new Rect(p, 3 * p, p, p);
            case QUANTUM_TRANSMITTER_4_ID: return new Rect(2 * p, 3 * p, p, p);
            case REACTOR_BLOCK_5_ID: return new Rect(3 * p, 3 * p, p, p);
            case FOUNDATION_BLOCK_5_ID: return new Rect(4 * p, 3 * p, p, p);
            case CONNECT_TOWER_6_ID: return new Rect(5 * p, 3 * p, p, p);
            case CONTROL_CENTER_6_ID: return new Rect(6 * p, 3 * p, p, p);
            case HOTEL_BLOCK_6_ID: return new Rect(7 * p, 3 * p, p, p);
            case HOUSING_MAST_6_ID: return new Rect(0, 2 * p, p, p);
            case DOCK_ADDON_1_ID:
            case DOCK_ADDON_2_ID:
            case DOCK_2_ID:
            case DOCK_3_ID:
                return new Rect(p, 2 * p, p, p);
            case OBSERVATORY_ID: return new Rect(2 * p, 2*p,p,p);
        }
    }

    virtual public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetStructureData(b, pos);
    }

    /// <summary>
    /// do not use directly, use basement.TransferStructures() instead
    /// </summary>
    /// <param name="sb"></param>
    public void ChangeBasement(SurfaceBlock sb)
    {
        basement = sb;
        if (transform.childCount == 0) SetModel();
        basement.AddStructure(this);
        if (isBasement)
        {
            BlockRendererController brc = transform.GetChild(0).GetComponent<BlockRendererController>();
            if (brc != null)
            {
                brc.SetStructure(this);
                basement.SetStructureBlock(brc);
            }
        }
    }
    // в финальном виде копипастить в потомков
    protected void SetStructureData(SurfaceBlock b, PixelPosByte pos)
    {
        //#setStructureData
        if (b.type == BlockType.Cave & isBasement)
        {
            basement = b.myChunk.ReplaceBlock(b.pos, BlockType.Surface, b.material_id, false) as SurfaceBlock;
        }
        else basement = b;
        innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.size);
        if (transform.childCount == 0) SetModel();
        basement.AddStructure(this);
        if (isBasement)
        {
            if (!subscribedToChunkUpdate)
            {
                basement.myChunk.ChunkUpdateEvent += ChunkUpdated;
                subscribedToChunkUpdate = true;
            }
            if (!GameMaster.loading)
            {
                if (basement.pos.y + 1 < Chunk.CHUNK_SIZE)
                {
                    ChunkPos npos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
                    Block upperBlock = basement.myChunk.GetBlock(npos);
                    if (upperBlock == null)
                    {
                        int replacingMaterialID = ResourceType.ADVANCED_COVERING_ID;
                        if (id == COLUMN_ID) replacingMaterialID = ResourceType.CONCRETE_ID;
                        basement.myChunk.AddBlock(npos, BlockType.Surface, replacingMaterialID, false);
                    }
                }
                else
                {
                    basement.myChunk.SetRoof(basement.pos.x, basement.pos.z, isArtificial);
                }
            }
            BlockRendererController brc = transform.GetChild(0).GetComponent<BlockRendererController>();
            if (brc != null)
            {
                brc.SetStructure(this);
                basement.SetStructureBlock(brc);
            }
        }
    }

    public void ApplyDamage(float d)
    {
        hp -= d;
        if (hp <= 0) Annihilate(false);
    }

    virtual public void ChunkUpdated()
    {
        if (basement == null) return;
        Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y + 1, basement.pos.z);
        if (upperBlock == null)
        {
            basement.myChunk.AddBlock(new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z), BlockType.Surface, ResourceType.CONCRETE_ID, false);
        }
    }

    virtual public void SetVisibility(bool x)
    {
        if (x == visible) return;
        else
        {
            visible = x;
            if (transform.childCount != 0) transform.GetChild(0).gameObject.SetActive(visible);
        }
    }

    virtual public UIObserver ShowOnGUI()
    {
        if (structureObserver == null) structureObserver = UIStructureObserver.InitializeStructureObserverScript();
        else structureObserver.gameObject.SetActive(true);
        structureObserver.SetObservingStructure(this);
        return structureObserver;
    }
    virtual public void DisableGUI()
    {
        showOnGUI = false;
    }

    public void ChangeInnerPosition(SurfaceRect sr)
    {
        if (basement != null) return;
        else innerPosition = sr;
    }

    public void UnsetBasement()
    {
        if (basement != null)
        {
            if (subscribedToChunkUpdate)
            {
                basement.myChunk.ChunkUpdateEvent -= ChunkUpdated;
                subscribedToChunkUpdate = false;
                BlockRendererController brc = transform.GetChild(0).GetComponent<BlockRendererController>();
                if (brc != null) basement.ClearStructureBlock(brc);
            }
            if (isBasement)
            {
                if (basement.pos.y == Chunk.CHUNK_SIZE - 1) basement.myChunk.DeleteRoof(basement.pos.x, basement.pos.z);
            }
        }
        basement = null;
        innerPosition = new SurfaceRect(0, 0, innerPosition.size);
    }

    virtual public void SectionDeleted(ChunkPos pos) { } // для структур, имеющих влияние на другие блоки; сообщает, что одна секция отвалилась

    // в финальном виде копипастить в потомков
    protected bool PrepareStructureForDestruction(bool forced)
    {
        if (forced) { UnsetBasement(); }
        else
        {
            ResourceContainer[] resourcesLeft = ResourcesCost.GetCost(id);
            if (resourcesLeft.Length > 0 & GameMaster.realMaster.demolitionLossesPercent != 1)
            {
                for (int i = 0; i < resourcesLeft.Length; i++)
                {
                    resourcesLeft[i] = new ResourceContainer(resourcesLeft[i].type, resourcesLeft[i].volume * (1 - GameMaster.realMaster.demolitionLossesPercent));
                }
                GameMaster.realMaster.colonyController.storage.AddResources(resourcesLeft);
            }
        }
        bool haveBasement = basement != null;
        if (subscribedToChunkUpdate & haveBasement)
        {
            basement.myChunk.ChunkUpdateEvent -= ChunkUpdated;
            subscribedToChunkUpdate = false;
        }
        if (!forced & haveBasement)
        {
            basement.RemoveStructure(this);
            SurfaceBlock lastBasement = basement;
            if (isBasement)
            {
                BlockRendererController brc = transform.GetChild(0).GetComponent<BlockRendererController>();
                if (brc != null) basement.ClearStructureBlock(brc);
                Block upperBlock = lastBasement.myChunk.GetBlock(lastBasement.pos.x, lastBasement.pos.y + 1, lastBasement.pos.z);
                if (upperBlock != null)
                {
                    if (upperBlock is SurfaceBlock)
                    {
                        if (upperBlock.type == BlockType.Surface)
                        {
                            if (lastBasement.myChunk.CalculateSupportPoints(lastBasement.pos.x, lastBasement.pos.y, lastBasement.pos.z) >= Chunk.SUPPORT_POINTS_ENOUGH_FOR_HANGING)
                            {
                                lastBasement.myChunk.ReplaceBlock(lastBasement.pos, BlockType.Cave, lastBasement.material_id, upperBlock.material_id, false);
                            }
                            else lastBasement.myChunk.DeleteBlock(upperBlock.pos);
                        }
                        else // cave
                        {
                            (upperBlock as CaveBlock).DestroySurface();
                        }
                    }
                    lastBasement.myChunk.DeleteBlock(upperBlock.pos);
                }
                else
                {
                    if (basement.pos.y == Chunk.CHUNK_SIZE - 1) basement.myChunk.DeleteRoof(basement.pos.x, basement.pos.z);
                }
            }
            return true;
        }
        else return false;
    }
    /// <summary>
    /// forced means that this object will be deleted without basement-linked actions
    /// </summary>
    /// <param name="forced">If set to <c>true</c> forced.</param>
    /// 
    virtual public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(forced);
        basement = null;
        Destroy(gameObject);
    }

    #region save-load system
    public static void LoadStructures(int count, System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[4];
        for (int i = 0; i < count; i++)
        {
            fs.Read(data, 0, 4);
            int id = System.BitConverter.ToInt32(data, 0);
            if (id != PLANT_ID)
            {
                if (id != CONTAINER_ID)
                {
                    GetStructureByID(id).Load(fs, sblock);
                }
                else HarvestableResource.LoadContainer(fs, sblock);
            }
            else Plant.LoadPlant(fs, sblock);
        }
    }

    public virtual List<byte> Save()
    {
        return SerializeStructure();
    }

    public virtual void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        LoadStructureData(fs, sblock);
    }
    // в финальном виде копипастить в потомков
    protected void LoadStructureData(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        //copy in harvestable resource.load - changed
        Prepare();
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, sblock);
    }
    protected void LoadStructureData(byte[] data, SurfaceBlock sblock)
    {
        //copy in harvestable resource.load - changed
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        SetBasement(sblock, new PixelPosByte(data[0], data[1]));
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 12);

    }

    // в финальном виде копипастить в потомков
    protected List<byte> SerializeStructure()
    {
        byte one = 1, zero = 0;
        List<byte> data = new List<byte> {
            innerPosition.x, innerPosition.z, // 0 , 1
            modelRotation,                      // 2
            indestructible ? one : zero     //3
        };
        data.InsertRange(0, System.BitConverter.GetBytes(id)); // считывается до load(), поэтому не учитываем в индексах
        // little endian check ignoring
        data.AddRange(System.BitConverter.GetBytes(skinIndex));  // 4 - 7
        data.AddRange(System.BitConverter.GetBytes(maxHp)); // 8 - 11
        data.AddRange(System.BitConverter.GetBytes(hp)); // 12 - 15
        return data;
        //SERIALIZER_LENGTH = 16
    }
    #endregion
}