using UnityEngine;

public class ResourceType
{
    public readonly float mass, toughness;
    public readonly Texture icon;
    public readonly string name;
    public readonly string description;
    public readonly int ID = -1;
    public static readonly ResourceType Nothing, Lumber, Stone, Dirt, Food,
        metal_K_ore, metal_M_ore, metal_E_ore, metal_N_ore, metal_P_ore, metal_S_ore,
        metal_K, metal_M, metal_E, metal_N, metal_P, metal_S,
    mineral_F, mineral_L, Plastics, Concrete, FertileSoil, Fuel, Graphonium, Supplies, Snow;
    public const int STONE_ID = 1, DIRT_ID = 2, LUMBER_ID = 4, METAL_K_ID = 5, METAL_M_ID = 6, METAL_E_ID = 7,
    METAL_N_ID = 8, METAL_P_ID = 9, METAL_S_ID = 10, MINERAL_F_ID = 11, MINERAL_L_ID = 12, PLASTICS_ID = 13, FOOD_ID = 14,
    CONCRETE_ID = 15, METAL_K_ORE_ID = 16, METAL_M_ORE_ID = 17, METAL_E_ORE_ID = 18, METAL_N_ORE_ID = 19, METAL_P_ORE_ID = 20,
    METAL_S_ORE_ID = 21, FERTILE_SOIL_ID = 22, FUEL_ID = 23, GRAPHONIUM_ID = 24, SUPPLIES_ID = 25, SNOW_ID = 26;
    public static readonly ResourceType[] resourceTypesArray, materialsForCovering, blockMaterials;
    public static float[] prices, demand;
    public const byte TYPES_COUNT = 27;

    //проверь при добавлении
    //- ID
    // - localization - name & description
    // - texture rect
    // - score calculator

    public ResourceType(int f_id, float f_mass, float f_toughness)
    {
        ID = f_id;
        mass = f_mass;
        toughness = f_toughness;
        resourceTypesArray[ID] = this;
    }

    static ResourceType()
    {
        resourceTypesArray = new ResourceType[TYPES_COUNT];
        prices = new float[TYPES_COUNT]; demand = new float[TYPES_COUNT];
        Nothing = new ResourceType(0, 0, 0);
        float p = 0.01f;

        Food = new ResourceType(FOOD_ID, 0.1f, 0.1f);
        prices[FOOD_ID] = p; demand[FOOD_ID] = 2;

        metal_K = new ResourceType(METAL_K_ID, 0.7f, 50);
        prices[METAL_K_ID] = 3 * p; demand[METAL_K_ID] = 5;
        metal_M = new ResourceType(METAL_M_ID, 0.6f, 35);
        prices[METAL_M_ID] = 5 * p; demand[METAL_M_ID] = 5;
        metal_E = new ResourceType(METAL_E_ID, 0.3f, 3);
        prices[METAL_E_ID] = 7 * p; demand[METAL_E_ID] = 5;
        metal_N = new ResourceType(METAL_N_ID, 2, 3);
        prices[METAL_N_ID] = 25 * p; demand[METAL_N_ID] = 10;
        metal_P = new ResourceType(METAL_P_ID, 0.63f, 32);
        prices[METAL_P_ID] = 5 * p; demand[METAL_P_ID] = 1;
        metal_S = new ResourceType(METAL_S_ID, 0.2f, 40);
        prices[METAL_S_ID] = 15 * p; demand[METAL_S_ID] = 4;

        float lowK = 0.7f, ore_lowK = 0.5f;
        metal_K_ore = new ResourceType(METAL_K_ORE_ID, 2.7f, 25);
        prices[METAL_K_ORE_ID] = prices[METAL_K_ID] * lowK; demand[METAL_K_ORE_ID] = demand[METAL_K_ID] * ore_lowK;
        metal_M_ore = new ResourceType(METAL_M_ORE_ID, 2.6f, 17.5f);
        prices[METAL_M_ORE_ID] = prices[METAL_M_ID] * lowK; demand[METAL_M_ORE_ID] = demand[METAL_M_ID] * ore_lowK;
        metal_E_ore = new ResourceType(METAL_E_ORE_ID, 2.3f, 1.5f);
        prices[METAL_E_ORE_ID] = prices[METAL_E_ID] * lowK; demand[METAL_E_ORE_ID] = demand[METAL_E_ID] * ore_lowK;
        metal_N_ore = new ResourceType(METAL_N_ORE_ID, 4, 1.5f);
        prices[METAL_N_ORE_ID] = prices[METAL_N_ID] * lowK; demand[METAL_N_ORE_ID] = demand[METAL_N_ID] * ore_lowK;
        metal_P_ore = new ResourceType(METAL_P_ORE_ID, 2.63f, 16);
        prices[METAL_P_ORE_ID] = prices[METAL_P_ID] * lowK; demand[METAL_P_ORE_ID] = demand[METAL_P_ID] * ore_lowK;
        metal_S_ore = new ResourceType(METAL_S_ORE_ID, 2.2f, 20);
        prices[METAL_S_ORE_ID] = prices[METAL_S_ID] * lowK; demand[METAL_S_ORE_ID] = demand[METAL_S_ID] * ore_lowK;

        mineral_F = new ResourceType(MINERAL_F_ID, 1.1f, 1);
        prices[MINERAL_F_ID] = 3 * p; demand[MINERAL_F_ID] = 10;
        Fuel = new ResourceType(FUEL_ID, 0.1f, 0);
        prices[FUEL_ID] = prices[MINERAL_F_ID] * 4; demand[FUEL_ID] = 45;

        Plastics = new ResourceType(PLASTICS_ID, 0.5f, 10);
        prices[PLASTICS_ID] = 1.5f * p; demand[PLASTICS_ID] = 2;
        Lumber = new ResourceType(LUMBER_ID, 0.5f, 5);
        prices[LUMBER_ID] = prices[PLASTICS_ID] / 10f; demand[LUMBER_ID] = demand[PLASTICS_ID] / 6f;
        mineral_L = new ResourceType(MINERAL_L_ID, 1, 2);
        prices[MINERAL_L_ID] = prices[PLASTICS_ID] / 2f; demand[MINERAL_L_ID] = demand[PLASTICS_ID] * ore_lowK;

        Stone = new ResourceType(STONE_ID, 2.5f, 30);
        prices[STONE_ID] = 0.2f * p; demand[STONE_ID] = 0.5f;
        Concrete = new ResourceType(CONCRETE_ID, 3, 38);
        prices[CONCRETE_ID] = prices[STONE_ID] * 1.5f; demand[CONCRETE_ID] = demand[STONE_ID] * 1.8f;

        Dirt = new ResourceType(DIRT_ID, 1, 1);
        prices[DIRT_ID] = 0.5f * p; demand[DIRT_ID] = 1;
        FertileSoil = new ResourceType(FERTILE_SOIL_ID, 1, 1);
        prices[FERTILE_SOIL_ID] = prices[DIRT_ID]; demand[FERTILE_SOIL_ID] = demand[DIRT_ID];

        Graphonium = new ResourceType(GRAPHONIUM_ID, 4, 2.5f);
        prices[GRAPHONIUM_ID] = prices[METAL_N_ID] * 3;
        demand[GRAPHONIUM_ID] = 1;

        Supplies = new ResourceType(SUPPLIES_ID, 0.1f, 0.1f);
        prices[SUPPLIES_ID] = prices[FOOD_ID] * 2;
        demand[SUPPLIES_ID] = demand[FOOD_ID] * 0.95f;

        Snow = new ResourceType(SNOW_ID, 0.5f, 0.03f);
        prices[SUPPLIES_ID] = prices[FOOD_ID] / 2f;
        demand[SUPPLIES_ID] = 0.01f;

        materialsForCovering = new ResourceType[] { Stone, Dirt, Lumber, metal_K, metal_M, metal_E, metal_N, metal_P, metal_S, mineral_F, mineral_L, Plastics, Concrete, FertileSoil, Graphonium, Snow };
        blockMaterials = new ResourceType[] {
           Stone, Dirt, Lumber, metal_K, metal_M, metal_E, metal_N, metal_P, metal_S, mineral_F, mineral_L, Plastics,  Concrete, FertileSoil, Graphonium, Snow
        };
    }

    public static ResourceType GetResourceTypeById(int f_id)
    {
        if (f_id > resourceTypesArray.Length) return Nothing;
        else
        {
            if (f_id > 0) return resourceTypesArray[f_id];
            else {
                switch (f_id)
                {
                    case PoolMaster.MATERIAL_ADVANCED_COVERING_ID: return metal_K;
                    case PoolMaster.MATERIAL_GRASS_100_ID:
                    case PoolMaster.MATERIAL_GRASS_80_ID:
                    case PoolMaster.MATERIAL_GRASS_60_ID:
                    case PoolMaster.MATERIAL_GRASS_40_ID:
                    case PoolMaster.MATERIAL_GRASS_20_ID: return Dirt;
                    case PoolMaster.MATERIAL_WHITE_METAL_ID: return metal_K;
                    case PoolMaster.MATERIAL_DEAD_LUMBER_ID: return Lumber;
                    case PoolMaster.MATERIAL_WHITEWALL_ID: return Concrete;
                    default: return Nothing;
                }
            }
        }
    }

    /// <summary>
    /// icons
    /// </summary>
    /// <param name="f_id"></param>
    /// <returns></returns>
	public static Rect GetResourceIconRect(int f_id)
    {
        float p = 0.125f;
        switch (f_id)
        {
            case PoolMaster.MATERIAL_ADVANCED_COVERING_ID: return new Rect(p, 4 * p, p, p);
            case STONE_ID: return new Rect(5 * p, 5 * p, p, p);
            case DIRT_ID: return new Rect(p, 7 * p, p, p);
            case LUMBER_ID: return new Rect(6 * p, 7 * p, p, p);
            case METAL_K_ID: return new Rect(p, 6 * p, p, p);
            case METAL_M_ID: return new Rect(3 * p, 6 * p, p, p);
            case METAL_E_ID: return new Rect(7 * p, 7 * p, p, p);
            case METAL_N_ID: return new Rect(5 * p, 6 * p, p, p);
            case METAL_P_ID: return new Rect(7 * p, 6 * p, p, p);
            case METAL_S_ID: return new Rect(p, 5 * p, p, p);
            case MINERAL_F_ID: return new Rect(3 * p, 5 * p, p, p);
            case MINERAL_L_ID: return new Rect(4 * p, 5 * p, p, p);
            case PLASTICS_ID: return new Rect(2 * p, 7 * p, p, p);
            case FOOD_ID: return new Rect(4 * p, 7 * p, p, p);
            case CONCRETE_ID: return new Rect(0, 7 * p, p, p);
            case METAL_K_ORE_ID: return new Rect(2 * p, 6 * p, p, p);
            case METAL_M_ORE_ID: return new Rect(4 * p, 6 * p, p, p);
            case METAL_E_ORE_ID: return new Rect(0, 6 * p, p, p);
            case METAL_N_ORE_ID: return new Rect(6 * p, 6 * p, p, p);
            case METAL_P_ORE_ID: return new Rect(0, 5 * p, p, p);
            case METAL_S_ORE_ID: return new Rect(2 * p, 5 * p, p, p);
            case FERTILE_SOIL_ID: return new Rect(3 * p, 7 * p, p, p);
            case FUEL_ID: return new Rect(5 * p, 7 * p, p, p);
            case SUPPLIES_ID: return new Rect(6 * p, 5 * p, p, p);
            case GRAPHONIUM_ID: return new Rect(7 * p, 5 * p, p, p);
            case SNOW_ID: return new Rect(0, 4 * p, p, p);
            default: return new Rect(0, 0, p, p);
        }
    }
}


public struct ResourceContainer
{
    public readonly ResourceType type;
    public readonly float volume;
    public static readonly ResourceContainer Empty;

    public ResourceContainer(ResourceType f_type, float f_volume)
    {
        type = f_type;
        if (f_volume < 0) f_volume = 0;
        volume = f_volume;
    }
    public ResourceContainer(int i_id, float i_volume)
    {
        type = ResourceType.GetResourceTypeById(i_id);
        if (i_volume < 0) i_volume = 0;
        volume = i_volume;
    }

    public ResourceContainer ChangeVolumeToPercent(float pc)
    {
        if (pc <= 0f) return Empty;
        else return new ResourceContainer(type, volume * pc);
    }

    static ResourceContainer()
    {
        Empty = new ResourceContainer(ResourceType.resourceTypesArray[0], 0);
    }
}



