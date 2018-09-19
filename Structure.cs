using UnityEngine;

[System.Serializable]
public class StructureSerializer {
	public PixelPosByte pos;
	public bool undestructible;
	public float hp, maxHp;
    public int id;
	public byte[] specificData;
    public byte modelRotation;
}

public class Structure : MonoBehaviour  {
	public SurfaceBlock basement{get;protected set;}
	public SurfaceRect innerPosition{get;protected set;}
	public bool isArtificial {get;protected set;} // fixed in ID - используется при проверках на снос
	public bool isBasement{get;private set;} // fixed in ID
	public bool placeInCenter{get;private set;} // fixed in ID 
	public bool indestructible { get; protected set; }  // fixed in ID
    public float hp {get;protected set;}
	public float maxHp { get; protected set; } // fixed in ID
    public bool rotate90only { get; private set; } // fixed in ID
    public bool showOnGUI { get; protected set; }
	public int id {get; protected set;}
	public bool visible {get;protected set;}
    public byte modelRotation { get; protected set; }
    protected bool destroyed = false, subscribedToUpdate = false, subscribedToChunkUpdate = false;

	//проверь при добавлении
	//- ID
	// -set model - загрузка модели
	// -prepare - установка inner position
	// - localization - name & description
	// - texture rect
    // список построек
	public const int  PLANT_ID = 1, DRYED_PLANT_ID = 2, RESOURCE_STICK_ID = 3, LANDED_ZEPPELIN_ID = 5,
	TREE_OF_LIFE_ID = 6, STORAGE_0_ID = 7, CONTAINER_ID = 8, MINE_ELEVATOR_ID = 9, LIFESTONE_ID = 10, HOUSE_0_ID = 11, 
	DOCK_ID = 13, ENERGY_CAPACITOR_1_ID = 14, FARM_1_ID = 15, HQ_2_ID = 16, LUMBERMILL_1_ID = 17, MINE_ID = 18, SMELTERY_1_ID = 19, 
	WIND_GENERATOR_1_ID = 20, BIOGENERATOR_2_ID = 22, HOSPITAL_2_ID = 21, 	MINERAL_POWERPLANT_2_ID = 23, ORE_ENRICHER_2_ID = 24,
	ROLLING_SHOP_ID = 25, MINI_GRPH_REACTOR_3_ID = 26, FUEL_FACILITY_3_ID = 27, GRPH_REACTOR_4_ID = 28, PLASTICS_FACTORY_3_ID = 29,
	FOOD_FACTORY_4_ID = 30, GRPH_ENRICHER_3_ID = 31, XSTATION_3_ID = 32, QUANTUM_ENERGY_TRANSMITTER_5_ID = 33, CHEMICAL_FACTORY_4_ID = 34,
	STORAGE_1_ID = 35, STORAGE_2_ID = 36, STORAGE_3_ID = 37, STORAGE_5_ID = 38, HOUSE_1_ID = 39, HOUSE_2_ID = 40, HOUSE_3_ID = 41, 
	HOUSE_5_ID = 42, ENERGY_CAPACITOR_2_ID = 43, ENERGY_CAPACITOR_3_ID = 44, FARM_2_ID = 45, FARM_3_ID = 46, FARM_4_ID = 47, FARM_5_ID = 48,
	LUMBERMILL_2_ID = 49, LUMBERMILL_3_ID = 50, LUMBERMILL_4_ID = 51, LUMBERMILL_5_ID = 52, FOOD_FACTORY_5_ID = 53, SMELTERY_2_ID = 54, 
	SMELTERY_3_ID = 55,  SMELTERY_5_ID = 57, HQ_3_ID = 58, HQ_4_ID = 59, QUANTUM_TRANSMITTER_4_ID = 60,
    COLUMN_ID = 61, SWITCH_TOWER_ID = 62, SHUTTLE_HANGAR_4_ID = 63,
	RECRUITING_CENTER_4_ID = 64, EXPEDITION_CORPUS_4_ID = 65;
	public const int TOTAL_STRUCTURES_COUNT = 66;
	public static UIStructureObserver structureObserver;  


    public static void ResetToDefaults_Static()
    {
        OakTree.ResetToDefaults_Static_OakTree();
        Corn.ResetToDefaults_Static_Corn();
        Hospital.ResetToDefaults_Static_Hospital();
        Dock.ResetToDefaults_Static_Dock();
        RecruitingCenter.ResetToDefaults_Static_RecruitingCenter();
        QuantumTransmitter.ResetToDefaults_Static_QuantumTransmitter();
        Hangar.ResetToDefaults_Static_Hangar();
    }

    virtual protected void SetModel()
    {
        GameObject model;
        if (transform.childCount != 0 ) Destroy(transform.GetChild(0).gameObject);
        switch (id)
        {
            default: model = GameObject.CreatePrimitive(PrimitiveType.Cube); break;
            case DRYED_PLANT_ID: model = Instantiate(Resources.Load<GameObject>("Structures/dryedPlant"));break;
            case RESOURCE_STICK_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/resourcesStick"));break;
            case LANDED_ZEPPELIN_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/ZeppelinBasement"));break;
            case TREE_OF_LIFE_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Tree of Life"));break;
            case STORAGE_0_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Storage_level_0"));break;
            case STORAGE_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_1"));break;
            case STORAGE_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_2"));break;
            case STORAGE_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Storage_level_3"));break;
            case STORAGE_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/storageBlock_level_5"));break;
            case MINE_ELEVATOR_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/MineElevator"));break;
            case LIFESTONE_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/LifeStone"));break;
            case HOUSE_0_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/House_level_0"));break;
            case HOUSE_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/House_level_1"));break;
            case HOUSE_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/House_level_2"));break;
            case HOUSE_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/House_level_3"));break;
            case HOUSE_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/houseBlock_level_5"));break;
            case DOCK_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Dock_level_1"));break;
            case ENERGY_CAPACITOR_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/EnergyCapacitor_level_1"));break;
            case ENERGY_CAPACITOR_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/EnergyCapacitor_level_2"));break;
            case ENERGY_CAPACITOR_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/EnergyCapacitor_level_3"));break;
            case FARM_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_1"));break;
            case FARM_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_2"));break;
            case FARM_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_3"));break;
            case FARM_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Farm_level_4"));break;
            case FARM_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/farmBlock_level_5"));break;
            case HQ_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/HQ_level_2"));break;
            case HQ_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/HQ_level_3"));break;
            case HQ_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/HQ_level_4"));break;
            case LUMBERMILL_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_1"));break;
            case LUMBERMILL_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_2"));break;
            case LUMBERMILL_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_3"));break;
            case LUMBERMILL_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Lumbermill_level_4"));break;
            case LUMBERMILL_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/lumbermillBlock_level_5"));break;
            case MINE_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Mine_level_1"));break;
            case SMELTERY_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Smeltery_level_1"));break;
            case SMELTERY_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Smeltery_level_2"));break;
            case SMELTERY_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Smeltery_level_3"));break;
            case SMELTERY_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/smelteryBlock_level_5"));break;
            case WIND_GENERATOR_1_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/windGenerator_level_1"));break;
            case BIOGENERATOR_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Biogenerator_level_2"));break;
            case HOSPITAL_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/Hospital_level_2"));break;
            case MINERAL_POWERPLANT_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/mineralPP_level_2"));break;
            case ORE_ENRICHER_2_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/oreEnricher_level_2"));break;
            case ROLLING_SHOP_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/rollingShop_level_2"));break;
            case MINI_GRPH_REACTOR_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/miniReactor_level_3"));break;
            case FUEL_FACILITY_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/fuelFacility_level_3"));break;
            case GRPH_REACTOR_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/graphoniumReactor_level_4"));break;
            case PLASTICS_FACTORY_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/plasticsFactory_level_3"));break;
            case FOOD_FACTORY_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/foodFactory_level_4"));break;
            case FOOD_FACTORY_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Blocks/foodFactoryBlock_level_5"));break;
            case GRPH_ENRICHER_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/graphoniumEnricher_level_3"));break;
            case XSTATION_3_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/XStation_level_3"));break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/quantumEnergyTransmitter_level_4"));break;
            case CHEMICAL_FACTORY_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/chemicalFactory_level_4"));break;
            case COLUMN_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Column"));break;
            case SWITCH_TOWER_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/switchTower"));break;
            case SHUTTLE_HANGAR_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/shuttleHangar"));break;
            case RECRUITING_CENTER_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/recruitingCenter"));break;
            case EXPEDITION_CORPUS_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/expeditionCorpus"));break;
            case QUANTUM_TRANSMITTER_4_ID: model =  Instantiate(Resources.Load<GameObject>("Structures/Buildings/quantumTransmitter"));break;
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
        if (transform.childCount != 0 & basement != null)
        {
            transform.localRotation = Quaternion.Euler(0, modelRotation * 45, 0);
        }
    }

    public static Structure GetStructureByID(int i_id)
    {
        Structure s;
        switch (i_id) {
            case PLANT_ID: print("error : cannot create plant via GetStructureById"); return null;
            case DRYED_PLANT_ID:
                s = new GameObject("dryed plant").AddComponent<Structure>(); break;
            case COLUMN_ID:
                s = new GameObject("Column").AddComponent<Structure>();break;
            case RESOURCE_STICK_ID:
                s = new GameObject().AddComponent<ScalableHarvestableResource>();break;
            case MINE_ELEVATOR_ID:
                s = new GameObject().AddComponent<MineElevator>();break;
            case LANDED_ZEPPELIN_ID:
            case HQ_2_ID:
            case HQ_3_ID:
            case HQ_4_ID:
                s = new GameObject().AddComponent<HeadQuarters>();break;
            case TREE_OF_LIFE_ID:
            case LIFESTONE_ID:
                s = new GameObject().AddComponent<LifeSource>();break;
            case STORAGE_0_ID :
            case STORAGE_1_ID:
            case STORAGE_2_ID:
            case STORAGE_3_ID:
            case STORAGE_5_ID:
                s = new GameObject().AddComponent<StorageHouse>();break;
            case CONTAINER_ID:
                s = new GameObject().AddComponent<HarvestableResource>();break;
            case HOUSE_0_ID:
            case HOUSE_1_ID:
            case HOUSE_2_ID:
            case HOUSE_3_ID:
            case HOUSE_5_ID:
                s = new GameObject().AddComponent<House>();break;
            case DOCK_ID: s = new GameObject().AddComponent<Dock>();break;
            case FARM_1_ID:
            case FARM_2_ID:
            case FARM_3_ID:
            case FARM_4_ID:
            case FARM_5_ID:
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case LUMBERMILL_4_ID:
            case LUMBERMILL_5_ID:
                s = new GameObject().AddComponent<Farm>();break;
            case MINE_ID:
                s = new GameObject().AddComponent<Mine>();break;
            case SMELTERY_1_ID:
            case SMELTERY_2_ID:
            case SMELTERY_3_ID:
            case SMELTERY_5_ID:
            case ORE_ENRICHER_2_ID:
            case PLASTICS_FACTORY_3_ID:
            case FUEL_FACILITY_3_ID:
                s = new GameObject().AddComponent<Factory>();break;
            case FOOD_FACTORY_4_ID:
            case FOOD_FACTORY_5_ID:
                s = new GameObject().AddComponent<FoodFactory>();break;
            case WIND_GENERATOR_1_ID:
                s = new GameObject().AddComponent<WindGenerator>();break;
            case BIOGENERATOR_2_ID:
            case MINERAL_POWERPLANT_2_ID:
            case GRPH_REACTOR_4_ID:
                s = new GameObject().AddComponent<Powerplant>();break;
            case HOSPITAL_2_ID:
                s = new GameObject().AddComponent<Hospital>();break;
            case ROLLING_SHOP_ID:
                s = new GameObject().AddComponent<RollingShop>();break;
            case MINI_GRPH_REACTOR_3_ID:
            case ENERGY_CAPACITOR_1_ID:
            case ENERGY_CAPACITOR_2_ID:
            case ENERGY_CAPACITOR_3_ID:
                s = new GameObject().AddComponent<Building>();break;
            case GRPH_ENRICHER_3_ID: s = new GameObject().AddComponent<GraphoniumEnricher>();break;
            case XSTATION_3_ID: s = new GameObject().AddComponent<XStation>();break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID: s = new GameObject().AddComponent<QuantumEnergyTransmitter>();break;
            case CHEMICAL_FACTORY_4_ID: s = new GameObject().AddComponent<ChemicalFactory>();break;
            case QUANTUM_TRANSMITTER_4_ID: s = new GameObject().AddComponent<QuantumTransmitter>();break;
            case SWITCH_TOWER_ID: s = new GameObject().AddComponent<SwitchTower>();break;
            case SHUTTLE_HANGAR_4_ID: s = new GameObject().AddComponent<Hangar>();break;
            case RECRUITING_CENTER_4_ID: s = new GameObject().AddComponent<RecruitingCenter>();break;
            case EXPEDITION_CORPUS_4_ID: s = new GameObject().AddComponent<ExpeditionCorpus>();break;
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
                    maxHp = 25000;
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
                    innerPosition = new SurfaceRect(0, 0, 6);
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
                    maxHp = 45000;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = false; 
                    isArtificial = false;
                    isBasement = false;
                    indestructible = true;
                }
                break;
            case HOUSE_0_ID:
                {
                    maxHp = 5;
                    innerPosition = SurfaceRect.one;
                    placeInCenter = false;
                    rotate90only = false;
                    isArtificial = true;
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
            case ROLLING_SHOP_ID:
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
                    rotate90only = true;
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
            case FOOD_FACTORY_4_ID:
                {
                    maxHp = 1500;
                    innerPosition = SurfaceRect.full;
                    placeInCenter = true;
                    rotate90only = true;
                    isArtificial = true;
                    isBasement = false;
                }
                break;
            case FOOD_FACTORY_5_ID:
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
        }
        hp = maxHp;
    }

    public static Rect GetTextureRect(int f_id) {
		float p = 0.125f;
		switch (f_id) {
		default: return Rect.zero;
		case DRYED_PLANT_ID:
		case PLANT_ID : return new Rect(p, 7 * p,p,p);
		case HQ_4_ID:
		case HQ_3_ID:
		case HQ_2_ID:
		case LANDED_ZEPPELIN_ID : return new Rect(2 * p, 7*p, p, p);
            case LIFESTONE_ID :
		case TREE_OF_LIFE_ID : return new Rect(3 * p, 7 *p, p, p);
            case STORAGE_1_ID :
		case STORAGE_2_ID :
		case STORAGE_3_ID :
		case STORAGE_5_ID :
		case STORAGE_0_ID : return new Rect(5 * p, 7 *p, p, p);
            case CONTAINER_ID :return new Rect(4 * p, 7 *p, p, p);
            case MINE_ID:
		case MINE_ELEVATOR_ID : return new Rect(6 * p, 7 * p, p, p);
            case HOUSE_1_ID :
		case HOUSE_2_ID :
		case HOUSE_3_ID :
		case HOUSE_5_ID :
		case HOUSE_0_ID : return new Rect(7 * p, 7* p, p, p);
            case DOCK_ID : return new Rect(0, 6 *p,p,p);
            case ENERGY_CAPACITOR_3_ID:
		case ENERGY_CAPACITOR_2_ID:
		case ENERGY_CAPACITOR_1_ID : return new Rect(p, 6*p, p, p);
            case FARM_2_ID :
		case FARM_3_ID :
		case FARM_4_ID :
		case FARM_5_ID :
		case FARM_1_ID : return new Rect(2 * p, 6 * p, p, p);
            case LUMBERMILL_2_ID :
		case LUMBERMILL_3_ID :
		case LUMBERMILL_4_ID :
		case LUMBERMILL_5_ID :
		case LUMBERMILL_1_ID : return new Rect(3 *p, 6 *p, p, p);
            case SMELTERY_2_ID :
		case	 SMELTERY_3_ID :
		case SMELTERY_5_ID :
		case  SMELTERY_1_ID : return new Rect(4 *p, 6 *p, p, p);
            case WIND_GENERATOR_1_ID : return new Rect(5 * p, 6 *p, p, p);
            case BIOGENERATOR_2_ID : return new Rect(6 * p, 6 *p, p, p);
            case HOSPITAL_2_ID : return new Rect(7 *p, 6 *p, p, p);
            case MINERAL_POWERPLANT_2_ID :return new Rect(0, 5 *p, p, p);
            case ORE_ENRICHER_2_ID : return new Rect(p, 5 *p, p, p);
            case ROLLING_SHOP_ID : return new Rect(2 * p, 5 *p, p, p);
            case MINI_GRPH_REACTOR_3_ID : return new Rect(3 * p, 5 *p, p, p);
            case FUEL_FACILITY_3_ID : return new Rect(4 * p, 5 *p, p, p);
            case GRPH_REACTOR_4_ID : return new Rect(5 * p, 5 *p, p, p);
            case PLASTICS_FACTORY_3_ID : return new Rect(6 * p, 5 *p, p, p);
            case FOOD_FACTORY_5_ID:
		case FOOD_FACTORY_4_ID : return new Rect(7 *p, 5 *p, p, p);
            case GRPH_ENRICHER_3_ID : return new Rect(0, 4 *p, p, p);
            case XSTATION_3_ID : return new Rect(p, 4 *p, p, p);
            case QUANTUM_ENERGY_TRANSMITTER_5_ID : return new Rect(2 * p, 4 *p, p, p);
            case CHEMICAL_FACTORY_4_ID : return new Rect(3 *p, 4 *p, p, p);
            case RESOURCE_STICK_ID : return new Rect(4 * p, 4 *p, p, p);
            case COLUMN_ID : return new Rect(5 *p, 4 *p, p, p);
            case SWITCH_TOWER_ID : return new Rect(6 * p, 4 *p, p, p);
            case SHUTTLE_HANGAR_4_ID : return new Rect(7 *p, 4 *p, p, p);
            case RECRUITING_CENTER_4_ID : return new Rect(0, 3*p, p, p);
            case EXPEDITION_CORPUS_4_ID : return new Rect(p, 3 *p, p, p);
            case QUANTUM_TRANSMITTER_4_ID : return new Rect(2 *p, 3 *p, p, p);
        }
	}

	virtual public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetStructureData(b,pos);
	}
    // в финальном виде копипастить в потомков
    protected void SetStructureData(SurfaceBlock b, PixelPosByte pos) {
        //#setStructureData
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.size);
        if (transform.childCount == 0) SetModel();
		b.AddStructure(this);
		if (isBasement) {
            if (!subscribedToChunkUpdate)
            {
                basement.myChunk.ChunkUpdateEvent += ChunkUpdated;
                subscribedToChunkUpdate = true;
            }
            if (basement is CaveBlock) {
				basement.myChunk.ReplaceBlock(basement.pos, BlockType.Surface, basement.material_id, false);
			}
			if (basement.pos.y + 1 < Chunk.CHUNK_SIZE) {
				ChunkPos npos = new ChunkPos(basement.pos.x, basement.pos.y + 1, basement.pos.z);
				Block upperBlock = basement.myChunk.GetBlock(npos.x, npos.y, npos.z);
				if ( upperBlock == null ) basement.myChunk.AddBlock(npos, BlockType.Surface, ResourceType.CONCRETE_ID, false);
			}
			else {
				GameObject g = PoolMaster.GetRooftop(this);
				g.transform.parent = basement.transform;
				g.transform.localPosition = Vector3.up * Block.QUAD_SIZE/2f;
				g.name = "block ceiling";
			}
		}
	} 	

	public void ApplyDamage(float d) {
		hp -= d;
		if ( hp <= 0 ) Annihilate(false);
	}

	virtual public void ChunkUpdated( ChunkPos pos) { // проверка?
		if ( basement == null) return;
		Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y+1, basement.pos.z);
		if (upperBlock == null) {
			basement.myChunk.AddBlock( new ChunkPos(basement.pos.x, basement.pos.y+1, basement.pos.z), BlockType.Surface, ResourceType.CONCRETE_ID, false);
		}
	}

	virtual public void SetVisibility( bool x) {
		if (x == visible ) return;
		else {
			visible = x;
            if (transform.childCount != 0) transform.GetChild(0).gameObject.SetActive(visible);
		}
	}

	#region save-load system
	public virtual StructureSerializer Save() {
		return GetStructureSerializer();
	}

	public virtual void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss,sblock);
	}
    // в финальном виде копипастить в потомков
    protected void LoadStructureData(StructureSerializer ss, SurfaceBlock sblock) {
        modelRotation = ss.modelRotation;
        indestructible = ss.undestructible;
		SetBasement(sblock, ss.pos);
		maxHp = ss.maxHp; hp = ss.maxHp;       
	}

    // в финальном виде копипастить в потомков
    protected StructureSerializer GetStructureSerializer() {
		StructureSerializer ss = new StructureSerializer();
		ss.pos = new PixelPosByte(innerPosition.x, innerPosition.z);
		ss.undestructible = indestructible;
		ss.hp = hp;
		ss.maxHp = maxHp;
        ss.modelRotation = modelRotation;
		ss.id = id;
		return ss;
	}
	#endregion

	virtual public UIObserver ShowOnGUI() {
        if (structureObserver == null) structureObserver = UIStructureObserver.InitializeStructureObserverScript();
        else structureObserver.gameObject.SetActive(true);
		structureObserver.SetObservingStructure(this);
		return structureObserver;
	}
    virtual public void DisableGUI()
    {
        showOnGUI = false;
    }

	public void ChangeInnerPosition(SurfaceRect sr) { 
		if (basement != null) return;
		else innerPosition = sr;
	}

    public void UnsetBasement()
    {
        if (isBasement & basement != null & subscribedToChunkUpdate)
        {
            basement.myChunk.ChunkUpdateEvent -= ChunkUpdated;
            subscribedToChunkUpdate = false;
        }
        basement = null;
        innerPosition = new SurfaceRect(0, 0, innerPosition.size);
    }

    virtual public void SectionDeleted(ChunkPos pos)   {    } // для структур, имеющих влияние на другие блоки; сообщает, что одна секция отвалилась

    // в финальном виде копипастить в потомков
    protected bool PrepareStructureForDestruction( bool forced )
    {
        if (forced) { UnsetBasement(); }
        else
        {
            ResourceContainer[] resourcesLeft = ResourcesCost.GetCost(id);
            if (resourcesLeft.Length > 0 & GameMaster.demolitionLossesPercent != 1)
            {
                for (int i = 0; i < resourcesLeft.Length; i++)
                {
                    resourcesLeft[i] = new ResourceContainer(resourcesLeft[i].type, resourcesLeft[i].volume * (1 - GameMaster.demolitionLossesPercent));
                }
                GameMaster.colonyController.storage.AddResources(resourcesLeft);
            }
        }
        if (!forced & (basement != null))
        {
            basement.RemoveStructure(this);
            if (isArtificial) basement.artificialStructures--;
            if (subscribedToChunkUpdate)
            {
                basement.myChunk.ChunkUpdateEvent -= ChunkUpdated;
                subscribedToChunkUpdate = false;
            }
            SurfaceBlock lastBasement = basement;
            if (isBasement)
            {
                Block upperBlock = lastBasement.myChunk.GetBlock(lastBasement.pos.x, lastBasement.pos.y + 1, lastBasement.pos.z);
                if (upperBlock != null)
                {
                    lastBasement.myChunk.ReplaceBlock(lastBasement.pos, BlockType.Cave, lastBasement.material_id, upperBlock.material_id, false);
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
	virtual public void Annihilate( bool forced ) {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(forced);
        basement = null;
        Destroy(gameObject);
	}

    protected void OnDestroy()
    {
        if (!destroyed & !GameMaster.applicationStopWorking) Annihilate(true);
    }
}
