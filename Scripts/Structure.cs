using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
public class Structure : MonoBehaviour
{
    public Plane basement { get; protected set; }
    public SurfaceRect surfaceRect { get; protected set; }
    public bool isArtificial { get; protected set; } // fixed in ID - используется при проверках на снос
    public bool placeInCenter { get; private set; } // fixed in ID 
    public bool indestructible { get; protected set; }  // fixed in ID
    public float hp { get; protected set; }
    public float maxHp { get; protected set; } // fixed in ID
    public bool rotate90only { get; private set; } // fixed in ID
    public bool showOnGUI { get; protected set; }
    public int ID { get; protected set; }
    public bool visible { get; protected set; }
    public byte modelRotation { get; protected set; }

    protected bool destroyed = false, subscribedToUpdate = false, subscribedToChunkUpdate = false;
    protected uint skinIndex = 0;

    //проверь при добавлении
    //- ID
    // - get structure by id
    // -set model - загрузка модели, проверить разделение модели по материалам
    // -prepare - установка inner position - по всем классам
    // - localization - name & description
    // - texture rect
    // -building - get applicable buildings list
    // -resource cost
    // score calculator
    public const int PLANT_ID = 1, DRYED_PLANT_ID = 2, RESOURCE_STICK_ID = 3, HEADQUARTERS_ID = 4, SETTLEMENT_CENTER_ID = 5,
    TREE_OF_LIFE_ID = 6, STORAGE_0_ID = 7, CONTAINER_ID = 8, MINE_ELEVATOR_ID = 9, LIFESTONE_ID = 10, TENT_ID = 11,
    DOCK_ID = 13, ENERGY_CAPACITOR_1_ID = 14, ENERGY_CAPACITOR_2_ID = 43, FARM_1_ID = 15, SETTLEMENT_STRUCTURE_ID = 16, LUMBERMILL_1_ID = 17, MINE_ID = 18, SMELTERY_1_ID = 19,
    WIND_GENERATOR_1_ID = 20, BIOGENERATOR_2_ID = 22, HOSPITAL_2_ID = 21, MINERAL_POWERPLANT_2_ID = 23, ORE_ENRICHER_2_ID = 24,
    WORKSHOP_ID = 25, MINI_GRPH_REACTOR_3_ID = 26, FUEL_FACILITY_ID = 27, GRPH_REACTOR_4_ID = 28, PLASTICS_FACTORY_3_ID = 29,
    SUPPLIES_FACTORY_4_ID = 30, GRPH_ENRICHER_3_ID = 31, XSTATION_3_ID = 32, QUANTUM_ENERGY_TRANSMITTER_5_ID = 33,
        SCIENCE_LAB_ID = 34, STORAGE_1_ID = 35, STORAGE_2_ID = 36, STORAGE_5_ID = 38, PSYCHOKINECTIC_GEN_ID = 39,
    HOUSE_BLOCK_ID = 42,  FARM_2_ID = 45, FARM_3_ID = 46, FARM_4_ID = 47, FARM_5_ID = 48,
    LUMBERMILL_2_ID = 49, LUMBERMILL_3_ID = 50, LUMBERMILL_4_ID = 51, LUMBERMILL_5_ID = 52, SUPPLIES_FACTORY_5_ID = 53, SMELTERY_2_ID = 54,
    SMELTERY_3_ID = 55, SMELTERY_5_ID = 57, QUANTUM_TRANSMITTER_4_ID = 60,
    COLUMN_ID = 61, SWITCH_TOWER_ID = 62, SHUTTLE_HANGAR_4_ID = 63,
    RECRUITING_CENTER_4_ID = 64, EXPEDITION_CORPUS_4_ID = 65, REACTOR_BLOCK_5_ID = 66, FOUNDATION_BLOCK_5_ID = 67, CONNECT_TOWER_6_ID = 68,
         HOTEL_BLOCK_6_ID = 70, HOUSING_MAST_6_ID = 71, DOCK_ADDON_1_ID = 72, DOCK_ADDON_2_ID = 73, DOCK_2_ID = 74, DOCK_3_ID = 75,
        OBSERVATORY_ID = 76, ARTIFACTS_REPOSITORY_ID = 77, MONUMENT_ID = 78;
    //free ids 37,39,40,44, 58, 59, 69
    public const int TOTAL_STRUCTURES_COUNT = 79, STRUCTURE_SERIALIZER_LENGTH = 16;
    public const string STRUCTURE_COLLIDER_TAG = "Structure";

    public static UIStructureObserver structureObserver;
    private static bool firstLaunch = true;
    private static List<System.Type> resetTypesList;

    public static void AddToResetList(System.Type t)
    {
        if (resetTypesList == null) resetTypesList = new List<System.Type>();
        if (!resetTypesList.Contains(t)) resetTypesList.Add(t);
    }
    public static void ResetToDefaults_Static()
    {
        if (firstLaunch)
        {
            firstLaunch = false;
            return;
        }

        if (resetTypesList != null)
        {
            foreach (var t in resetTypesList)
            {
                var func = t.GetMethod("ResetStaticData");
                if (func != null)
                {
                    func.Invoke(null, null);
                }
                else print(t.ToString());
            }
        }
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
            case HEADQUARTERS_ID:
                s = new GameObject("HQ").AddComponent<HeadQuarters>(); break;
            case TREE_OF_LIFE_ID:
            case LIFESTONE_ID:
                s = new GameObject("Lifesource").AddComponent<LifeSource>(); break;
            case STORAGE_0_ID:
            case STORAGE_1_ID:
            case STORAGE_2_ID:
            case STORAGE_5_ID:
                s = new GameObject("Storage").AddComponent<StorageHouse>(); break;
            case CONTAINER_ID:
                //use HarvestableResource.ConstructContainer instead
                s = new GameObject("Container").AddComponent<HarvestableResource>(); break;
            case TENT_ID:
            case HOUSE_BLOCK_ID:
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
            case FUEL_FACILITY_ID:
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
                s = new GameObject("Energy capacitor").AddComponent<Building>(); break;
            case GRPH_ENRICHER_3_ID:
                s = new GameObject("Graphonium Enricher").AddComponent<Factory>(); break;
            case XSTATION_3_ID:
                s = new GameObject("XStation").AddComponent<XStation>(); break; // AWAITING
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: s = new GameObject("Quantum energy transmitter").AddComponent<QuantumEnergyTransmitter>(); break;
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
            // case CONTROL_CENTER_6_ID:
            // s = new GameObject("Command center").AddComponent<ControlCenter>(); break;
            case HOTEL_BLOCK_6_ID:
                s = new GameObject("Hotel block").AddComponent<Hotel>(); break; // AWAITING
            case HOUSING_MAST_6_ID:
                s = new GameObject("Housing mast").AddComponent<House>(); break;
            case DOCK_ADDON_1_ID:
                s = new GameObject("Dock Addon 1").AddComponent<DockAddon>(); break;
            case DOCK_ADDON_2_ID:
                s = new GameObject("Dock Addon 2").AddComponent<DockAddon>(); break;
            case OBSERVATORY_ID:
                s = new GameObject("Observatory").AddComponent<Observatory>(); break;
            case ARTIFACTS_REPOSITORY_ID:
                s = new GameObject("Artifacts repository").AddComponent<ArtifactsRepository>(); break;
            case MONUMENT_ID:
                s = new GameObject("Monument").AddComponent<Monument>(); break;
            case SETTLEMENT_CENTER_ID:
                s = new GameObject("Settlement center").AddComponent<Settlement>(); break;
            case SETTLEMENT_STRUCTURE_ID:
                s = new GameObject("Settlement structure").AddComponent<SettlementStructure>(); break;
            case PSYCHOKINECTIC_GEN_ID:
                s = new GameObject("Psychocinetic gen").AddComponent<PsychokineticGenerator>(); break;
            case SCIENCE_LAB_ID:
                s = new GameObject("Science Lab").AddComponent<ScienceLab>(); break;
            default: return null;
        }
        s.ID = i_id;
        s.Prepare();
        return s;
    }
    public static Rect GetTextureRect(int f_id)
    {
        float p = 0.125f;
        switch (f_id)
        {
            default: return Rect.zero;
            case DRYED_PLANT_ID:
            case PLANT_ID: return new Rect(p, 7 * p, p, p);
            case HEADQUARTERS_ID: return new Rect(2 * p, 7 * p, p, p);
            case LIFESTONE_ID:
            case TREE_OF_LIFE_ID: return new Rect(3 * p, 7 * p, p, p);
            case STORAGE_1_ID:
            case STORAGE_2_ID:
            case STORAGE_5_ID:
            case STORAGE_0_ID: return new Rect(5 * p, 7 * p, p, p);
            case CONTAINER_ID: return new Rect(4 * p, 7 * p, p, p);
            case MINE_ID:
            case MINE_ELEVATOR_ID: return new Rect(6 * p, 7 * p, p, p);
            case SETTLEMENT_CENTER_ID:
            case SETTLEMENT_STRUCTURE_ID:
            case HOUSE_BLOCK_ID:
            case TENT_ID: return new Rect(7 * p, 7 * p, p, p);
            case DOCK_ID: return new Rect(0, 6 * p, p, p);
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
            case FUEL_FACILITY_ID: return new Rect(4 * p, 5 * p, p, p);
            case GRPH_REACTOR_4_ID: return new Rect(5 * p, 5 * p, p, p);
            case PLASTICS_FACTORY_3_ID: return new Rect(6 * p, 5 * p, p, p);
            case SUPPLIES_FACTORY_5_ID:
            case SUPPLIES_FACTORY_4_ID: return new Rect(7 * p, 5 * p, p, p);
            case GRPH_ENRICHER_3_ID: return new Rect(0, 4 * p, p, p);
            case XSTATION_3_ID: return new Rect(p, 4 * p, p, p);
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: return new Rect(2 * p, 4 * p, p, p);
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
            case SCIENCE_LAB_ID: return new Rect(6f * p, 3f * p, p, p);
            case HOTEL_BLOCK_6_ID: return new Rect(7 * p, 3 * p, p, p);
            case HOUSING_MAST_6_ID: return new Rect(0, 2 * p, p, p);
            case DOCK_ADDON_1_ID:
            case DOCK_ADDON_2_ID:
            case DOCK_2_ID:
            case DOCK_3_ID:
                return new Rect(p, 2 * p, p, p);
            case OBSERVATORY_ID: return new Rect(2 * p, 2 * p, p, p);
            case ARTIFACTS_REPOSITORY_ID: return new Rect(3 * p, 2 * p, p, p);
            case MONUMENT_ID: return new Rect(4 * p, 2 * p, p, p);
            case PSYCHOKINECTIC_GEN_ID: return new Rect(5 * p, 2 * p, p, p);
        }
    }

    virtual protected void SetModel()
    {
        //switch skin index
        GameObject model;
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        switch (ID)
        {
            default: model = GameObject.CreatePrimitive(PrimitiveType.Cube); break;
            case DRYED_PLANT_ID:
                model = Instantiate(Resources.Load<GameObject>("Structures/dryedPlant"));
                break;
            case TREE_OF_LIFE_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Tree of Life")); break;
            case STORAGE_0_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Storage_level_0")); break;
            case STORAGE_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_1")); break;
            case STORAGE_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_2")); break;           
            case STORAGE_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/storageBlock_level_5")); break;
            case MINE_ELEVATOR_ID: model = Instantiate(Resources.Load<GameObject>("Structures/MineElevator")); break;
            case LIFESTONE_ID: model = Instantiate(Resources.Load<GameObject>("Structures/LifeStone")); break;
            case TENT_ID: model = Instantiate(Resources.Load<GameObject>("Structures/House_level_0")); break;
            case HOUSE_BLOCK_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/houseBlock_level_5")); break;
            case DOCK_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_level_1")); break;
            case DOCK_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_level_2")); break;
            case DOCK_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_level_3")); break;
            case ENERGY_CAPACITOR_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Energy_capacitor_level_1")); break;
            case ENERGY_CAPACITOR_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Energy_capacitor_level_2")); break;           
            case FARM_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_1")); break;
            case FARM_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_2")); break;
            case FARM_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_3")); break;
            case FARM_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_4")); break;
            case FARM_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/farmBlock_level_5")); break;
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
            case FUEL_FACILITY_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/fuelFacility")); break;
            case GRPH_REACTOR_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/graphoniumReactor_level_4")); break;
            case PLASTICS_FACTORY_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/plasticsFactory_level_3")); break;
            case SUPPLIES_FACTORY_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/foodFactory_level_4")); break;
            case SUPPLIES_FACTORY_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/foodFactoryBlock_level_5")); break;
            case GRPH_ENRICHER_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/graphoniumEnricher_level_3")); break;
            case XSTATION_3_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/XStation_level_3")); break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/quantumEnergyTransmitter_level_4")); break;           
            case COLUMN_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Column")); break;
            case SWITCH_TOWER_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/switchTower")); break;
            case SHUTTLE_HANGAR_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/shuttleHangar")); break;
            case RECRUITING_CENTER_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/recruitingCenter")); break;
            case EXPEDITION_CORPUS_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/expeditionCorpus")); break;
            case QUANTUM_TRANSMITTER_4_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/quantumTransmitter")); break;
            case REACTOR_BLOCK_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/reactorBlock")); break;
            case FOUNDATION_BLOCK_5_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/foundationBlock")); break;
            case CONNECT_TOWER_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/connectTower")); break;           
            case HOTEL_BLOCK_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Blocks/hotelBlock")); break;
            case HOUSING_MAST_6_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/housingMast")); break;
            case DOCK_ADDON_1_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_addon1")); break;
            case DOCK_ADDON_2_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/dock_addon2")); break;
            case OBSERVATORY_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/observatory")); break;
            case ARTIFACTS_REPOSITORY_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/artifactsRepository")); break;
            case MONUMENT_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/monument")); break;
            case SETTLEMENT_CENTER_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_0")); break;
            case PSYCHOKINECTIC_GEN_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/psychokineticGenerator"));break;
            case SCIENCE_LAB_ID: model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/scienceLab")); break;
        }
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(model, true);
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
        if (hp == 0) Annihilate(true, false, true);
    }
    public bool IsDestroyed() { return destroyed; }
    virtual public bool IsCube() { return false; }
    virtual public bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
    {
        return true;
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
        switch (ID)
        {
            case PLANT_ID:
                {
                    maxHp = 1;
                    surfaceRect = SurfaceRect.one;
                    isArtificial = false;
                    placeInCenter = false;
                    rotate90only = false;
                }
                break;
            case DRYED_PLANT_ID:
                {
                    maxHp = 1;
                    surfaceRect = SurfaceRect.one;
                    isArtificial = false;
                    placeInCenter = false;
                    rotate90only = false;
                }
                break;
            case RESOURCE_STICK_ID:
                {
                    maxHp = PlaneExtension.INNER_RESOLUTION;
                    surfaceRect = new SurfaceRect(0, 0, ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE);
                    isArtificial = false;
                    placeInCenter = false;
                    rotate90only = true;
                }
                break;
            case HEADQUARTERS_ID:
                {
                    maxHp = 1000;
                    surfaceRect = SurfaceRect.full;
                    isArtificial = true;
                    placeInCenter = true;
                    rotate90only = true;
                    indestructible = true;
                }
                break;
            case SETTLEMENT_CENTER_ID:
                {
                    maxHp = 1000;
                    surfaceRect = new SurfaceRect(0, 0, 4);
                    isArtificial = true;
                    placeInCenter = true;
                    rotate90only = false;
                    indestructible = false;
                }
                break;
            case TREE_OF_LIFE_ID:
                {
                    maxHp = LifeSource.MAX_HP;
                    surfaceRect = SurfaceRect.full;
                    isArtificial = false;
                    placeInCenter = true;
                    rotate90only = false;
                    indestructible = true;
                }
                break;
            case STORAGE_0_ID:
                {
                    maxHp = 750;
                    surfaceRect = SurfaceRect.full;
                    isArtificial = true;
                    placeInCenter = true;
                    rotate90only = true;
                    indestructible = true;
                }
                break;
            case STORAGE_1_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    isArtificial = true;
                    placeInCenter = true;
                    rotate90only = true;
                }
                break;
            case STORAGE_2_ID:
                {
                    maxHp = 6000;
                    surfaceRect = SurfaceRect.full;
                    isArtificial = true;                    
                    placeInCenter = true;
                    rotate90only = true;
                }
                break;
            case STORAGE_5_ID:
                {
                    maxHp = 12000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case CONTAINER_ID:
                {
                    maxHp = 10;
                    surfaceRect = SurfaceRect.one;
                    placeInCenter = false;
                    rotate90only = false;
                    isArtificial = false;
                    
                }
                break;
            case MINE_ELEVATOR_ID:
                {
                    maxHp = 100;
                    surfaceRect = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case LIFESTONE_ID:
                {
                    maxHp = LifeSource.MAX_HP;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = false;
                    
                    indestructible = true;
                }
                break;
            case TENT_ID:
                {
                    maxHp = 5;
                    surfaceRect = SurfaceRect.one;
                    placeInCenter = false;
                    rotate90only = false;
                    isArtificial = false;                    
                }
                break;   
            case HOUSE_BLOCK_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case DOCK_ID:
                {
                    maxHp = 1200;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case ENERGY_CAPACITOR_1_ID:
                {
                    maxHp = 1000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case ENERGY_CAPACITOR_2_ID:
                {
                    maxHp = 2000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case FARM_1_ID:
                {
                    maxHp = 500;
                    surfaceRect = new SurfaceRect(0, 0, 4);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case FARM_2_ID:
                {
                    maxHp = 800;
                    surfaceRect = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case FARM_3_ID:
                {
                    maxHp = 1500;
                    surfaceRect = new SurfaceRect(0, 0, 8);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case FARM_4_ID:
                {
                    maxHp = 2000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case FARM_5_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    maxHp = 500;
                    surfaceRect = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    maxHp = 750;
                    surfaceRect = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    maxHp = 1000;
                    surfaceRect = new SurfaceRect(0, 0, 6);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case LUMBERMILL_4_ID:
                {
                    maxHp = 1200;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case LUMBERMILL_5_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case MINE_ID:
                {
                    maxHp = 500;
                    surfaceRect = new SurfaceRect(0, 0, 4);
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SMELTERY_1_ID:
                {
                    maxHp = 800;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SMELTERY_2_ID:
                {
                    maxHp = 1200;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SMELTERY_3_ID:
                {
                    maxHp = 1800;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SMELTERY_5_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case WIND_GENERATOR_1_ID:
                {
                    maxHp = 700;
                    surfaceRect =  new SurfaceRect(0,0,8);
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    maxHp = 1200;
                    surfaceRect = new SurfaceRect(0, 0, 10);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case HOSPITAL_2_ID:
                {
                    maxHp = 1500;
                    surfaceRect = new SurfaceRect(0, 0, 10);
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case MINERAL_POWERPLANT_2_ID:
                {
                    maxHp = 1100;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    maxHp = 1500;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case WORKSHOP_ID:
                {
                    maxHp = 1200;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case MINI_GRPH_REACTOR_3_ID:
                {
                    maxHp = 800;
                    surfaceRect = new SurfaceRect(0,0,8);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case FUEL_FACILITY_ID:
                {
                    maxHp = 1250;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    maxHp = 2700;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    maxHp = 1600;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    maxHp = 1500;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    maxHp = 1400;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case XSTATION_3_ID:
                {
                    maxHp = 1700;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID:
                {
                    maxHp = 2100;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case COLUMN_ID:
                {
                    maxHp = 2200;
                    surfaceRect = new SurfaceRect(0, 0, 2);
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SWITCH_TOWER_ID:
                {
                    maxHp = 250;
                    surfaceRect = new SurfaceRect(0, 0, 4);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    maxHp = 1200;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    maxHp = 1600;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    maxHp = 1800;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case QUANTUM_TRANSMITTER_4_ID:
                {
                    maxHp = 1100;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    maxHp = 3700;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case FOUNDATION_BLOCK_5_ID:
                {
                    maxHp = 8000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case CONNECT_TOWER_6_ID:
                {
                    maxHp = 1700;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;                    
                }
                break;
            case HOTEL_BLOCK_6_ID:
                {
                    maxHp = 2700;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case HOUSING_MAST_6_ID:
                {
                    maxHp = 5500;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case DOCK_ADDON_1_ID:
                {
                    maxHp = 2000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;                    
                }
                break;
            case DOCK_ADDON_2_ID:
                {
                    maxHp = 2200;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case DOCK_2_ID:
                {
                    maxHp = 2400;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case DOCK_3_ID:
                {
                    maxHp = 5000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case OBSERVATORY_ID:
                {
                    maxHp = 5000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case ARTIFACTS_REPOSITORY_ID:
                {
                    maxHp = 1500;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                }
                break;
            case MONUMENT_ID:
                {
                    maxHp = 4000;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    
                    break;
                }
            case SETTLEMENT_STRUCTURE_ID:
                {
                    maxHp = 100;
                    surfaceRect = new SurfaceRect(0,0,SettlementStructure.CELLSIZE);
                    placeInCenter = false;
                    rotate90only = true;
                    isArtificial = true;
                    
                    break;
                }
            case PSYCHOKINECTIC_GEN_ID:
                {
                    maxHp = 800;
                    surfaceRect = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    
                    break;
                }
            case SCIENCE_LAB_ID:
                {
                    maxHp = 1400;
                    surfaceRect = new SurfaceRect(0, 0, 12);
                    placeInCenter = true;
                    rotate90only = false;
                    isArtificial = true;
                    
                    break;
                }
        }
        hp = maxHp;
    }   

    virtual public void SetBasement(Plane p, PixelPosByte pos)
    {
        if (p == null) return;
        SetStructureData(p, pos);
    }
    protected void SetStructureData(Plane p, PixelPosByte pos)
    {
        // dependency - SettlementStructure.setBasement()
        if (!placeInCenter) surfaceRect = new SurfaceRect(pos.x, pos.y, surfaceRect.size);
        else surfaceRect = new SurfaceRect((byte)(PlaneExtension.INNER_RESOLUTION / 2 - surfaceRect.size / 2), (byte)(PlaneExtension.INNER_RESOLUTION / 2 - surfaceRect.size / 2), surfaceRect.size);
        if (transform.childCount == 0) SetModel();
        basement = p;
        basement.AddStructure(this);
    }

    /// <summary>
    /// do not use directly, use basement.TransferStructures() instead
    /// </summary>
    /// <param name="sb"></param>
    public void ChangeBasement(Plane p)
    {
        basement = p;
        if (transform.childCount == 0) SetModel();
        basement.AddStructure(this);
    }
    public void ChangeInnerPosition(SurfaceRect sr)
    {
        if (basement != null) return;
        else surfaceRect = sr;
    }
    public void ClearBasementLink(Plane p)
    {
        if (basement == p) basement = null;
    }
    virtual public void ApplyDamage(float d)
    {
        if (destroyed | indestructible) return;
        hp -= d;
        if (hp <= 0) Annihilate(true, false, true);
    }

    virtual public void ChunkUpdated()
    {
        if (basement == null) return;
        
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

    virtual public void SectionDeleted(ChunkPos pos) { } // для структур, имеющих влияние на другие блоки; сообщает, что одна секция отвалилась
    // в финальном виде копипастить в потомков
    protected void PrepareStructureForDestruction(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (returnResources)
        {
            ResourceContainer[] resourcesLeft = ResourcesCost.GetCost(ID);
            if (resourcesLeft.Length > 0 & GameMaster.realMaster.demolitionLossesPercent != 1)
            {
                for (int i = 0; i < resourcesLeft.Length; i++)
                {
                    resourcesLeft[i] = new ResourceContainer(resourcesLeft[i].type, resourcesLeft[i].volume * (1 - GameMaster.realMaster.demolitionLossesPercent));
                }
                GameMaster.realMaster.colonyController.storage.AddResources(resourcesLeft);
            }
        }
        if (clearFromSurface)
        {
            if (basement != null)
            {
                if (subscribedToChunkUpdate)
                {
                    basement.myBlockExtension.myBlock.myChunk.ChunkUpdateEvent -= ChunkUpdated;
                    subscribedToChunkUpdate = false;
                }
                basement.extension?.RemoveStructure(this);

                if (leaveRuins && !GameMaster.sceneClearing)
                {
                    basement.GetExtension().ScatterResources(surfaceRect, ResourceType.mineral_F, CalculateRuinsVolume());
                }
            }
        }
        basement = null;
    }
    protected int CalculateRuinsVolume()
    {
        var cost = ResourcesCost.GetCost(ID);
        if (cost != null && cost.Length > 0)
        {
            float x = 0;
            foreach (var c in cost)
            {
                x += c.volume;
            }
            float hp_k = 0f;
            if (hp > 0) hp_k = hp / maxHp;
            else
            {
                if (hp > -100f) hp_k = 1f - hp / 100f;
            }
            return (int)(x * GameConstants.RUINS_COEFFICIENT * hp_k * GameMaster.realMaster.environmentMaster.environmentalConditions);
        }
        else return 0;
    }

    virtual public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(clearFromSurface, returnResources, leaveRuins);
        basement = null;
        Destroy(gameObject);
    }

    #region save-load system
    public static void LoadStructures(int count, System.IO.FileStream fs, Plane p)
    {
        var data = new byte[4];
        for (int i = 0; i < count; i++)
        {
            fs.Read(data, 0, 4);
            int id = System.BitConverter.ToInt32(data, 0);
            int debug_prevID = -1;
            if (id != PLANT_ID)
            {
                if (id != CONTAINER_ID)
                {
                    var s = GetStructureByID(id);
                    if (s != null)
                    {
                        s.Load(fs, p);
                        debug_prevID = id;
                    }
                    else
                    {
                        print("error, desu: structure at position " + i.ToString() + ", id " + id.ToString() + " data corrupted");
                        GameMaster.LoadingFail();
                        return;
                    }
                }
                else
                {
                    HarvestableResource.LoadContainer(fs, p);
                    debug_prevID = CONTAINER_ID;
                }
            }
            else
            {
                Plant.LoadPlant(fs, p);
                debug_prevID = PLANT_ID;
            }
        }
    }

    public virtual List<byte> Save()
    {
        return SaveStructureData();
    }

    public virtual void Load(System.IO.FileStream fs, Plane p)
    {
        LoadStructureData(fs, p);
    }
    // в финальном виде копипастить в потомков
    protected void LoadStructureData(System.IO.FileStream fs, Plane p)
    {
        //copy in harvestable resource.load - changed
        Prepare();
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, p);
    }
    protected void LoadStructureData(byte[] data, Plane p)
    {        
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        SetBasement(p, new PixelPosByte(data[0], data[1]));
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 12);
        //copy to harvestable resource.load - changed
        // copy to scalable harvestable resource
        // copy to settlement
        //copy to settlement structure
    }

    // в финальном виде копипастить в потомков
    protected List<byte> SaveStructureData()
    {
        byte one = 1, zero = 0;
        List<byte> data = new List<byte> {
            surfaceRect.x, surfaceRect.z, // 0 , 1
            modelRotation,                      // 2
            indestructible ? one : zero     //3
        };
        data.InsertRange(0, System.BitConverter.GetBytes(ID)); // считывается до load(), поэтому не учитываем в индексах
        // little endian check ignoring
        data.AddRange(System.BitConverter.GetBytes(skinIndex));  // 4 - 7
        data.AddRange(System.BitConverter.GetBytes(maxHp)); // 8 - 11
        data.AddRange(System.BitConverter.GetBytes(hp)); // 12 - 15
        return data;
        //SERIALIZER_LENGTH = 16
    }
    #endregion
}