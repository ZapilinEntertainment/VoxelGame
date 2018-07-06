using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceType {
	public readonly float mass , toughness;
	public readonly Texture icon;
	public readonly string name;
	public readonly string description;
	public readonly int ID = -1;
	public Material material{get;private set;}
	public static readonly ResourceType Nothing, Lumber, Stone, Dirt,Food, 
		metal_K_ore, metal_M_ore, metal_E_ore, metal_N_ore, metal_P_ore, metal_S_ore, 
		metal_K, metal_M, metal_E, metal_N, metal_P, metal_S,
	mineral_F, mineral_L, Plastics, Concrete, FertileSoil, Fuel, Graphonium, Supplies;
	public const int STONE_ID = 1, DIRT_ID = 2, LUMBER_ID = 4, METAL_K_ID = 5, METAL_M_ID = 6, METAL_E_ID = 7,
	METAL_N_ID = 8, METAL_P_ID = 9,  METAL_S_ID = 10, MINERAL_F_ID = 11, MINERAL_L_ID = 12, PLASTICS_ID = 13, FOOD_ID = 14,
	CONCRETE_ID = 15, METAL_K_ORE_ID = 16, METAL_M_ORE_ID = 17, METAL_E_ORE_ID = 18, METAL_N_ORE_ID = 19, METAL_P_ORE_ID = 20,
	METAL_S_ORE_ID = 21, FERTILE_SOIL_ID = 22, FUEL_ID = 23, GRAPHONIUM_ID = 24, SUPPLIES_ID = 25;
	public static readonly ResourceType[] resourceTypesArray, materialsForCovering;
	public static float[] prices, demand;
	public const int RTYPES_COUNT = 26, RTYPE_ARRAY_ROWS = 5, RTYPE_ARRAY_COLUMNS = 6; 

	//проверь при добавлении
	//- ID
	// - localization - name & description
	// - texture rect

	public ResourceType(string f_name, int f_id, float f_mass, float f_toughness, Material f_material, Texture f_icon, string f_descr) {
		name = f_name;
		ID = f_id;
		mass = f_mass;
		toughness = f_toughness;
		material = f_material;
		icon = f_icon;
		description = f_descr;
		resourceTypesArray[ID] = this;
		}

		static ResourceType() {
		resourceTypesArray = new ResourceType[RTYPES_COUNT];
		prices = new float[RTYPES_COUNT]; demand = new float[RTYPES_COUNT];
		Nothing = new ResourceType(Localization.rtype_nothing_name, 0, 0, 0, Resources.Load<Material>("Materials/default"), Resources.Load<Texture>("Textures/resource_empty"), Localization.rtype_nothing_descr); 

		Food = new ResourceType(Localization.rtype_food_name, FOOD_ID, 0.1f, 0.1f, Resources.Load<Material>("Materials/default"), Resources.Load<Texture>("Textures/resource_food"), Localization.rtype_food_descr);
		prices[FOOD_ID] = 1; demand[FOOD_ID] = 2;

		metal_K = new ResourceType("Metal K", METAL_K_ID, 0.7f, 50, Resources.Load<Material>("Materials/MetalK") , Resources.Load<Texture>("Textures/resource_metalK"), Localization.rtype_metalK_descr);
		prices[METAL_K_ID] = 3; demand[METAL_K_ID] = 5;
		metal_M = new ResourceType("Metal M", METAL_M_ID, 0.6f, 35,  Resources.Load<Material>("Materials/MetalM"), Resources.Load<Texture>("Textures/resource_metalM"), Localization.rtype_metalM_descr);
		prices[METAL_M_ID] = 5; demand[METAL_M_ID] = 5;
		metal_E = new ResourceType("Metal E", METAL_E_ID, 0.3f, 3,  Resources.Load<Material>("Materials/MetalE"), Resources.Load<Texture>("Textures/resource_metalE"), Localization.rtype_metalE_descr);
		prices[METAL_E_ID] = 7; demand[METAL_E_ID] = 5;
		metal_N = new ResourceType("Metal N", METAL_N_ID, 2, 3,  Resources.Load<Material>("Materials/MetalN"), Resources.Load<Texture>("Textures/resource_metalN"), Localization.rtype_metalN_descr);
		prices[METAL_N_ID] = 25; demand[METAL_N_ID] = 10;
		metal_P = new ResourceType("Metal P", METAL_P_ID, 0.63f, 32,  Resources.Load<Material>("Materials/MetalP"), Resources.Load<Texture>("Textures/resource_metalP"), Localization.rtype_metalP_descr);
		prices[METAL_P_ID] = 5; demand[METAL_P_ID] = 1;
		metal_S= new ResourceType("Metal S", METAL_S_ID, 0.2f, 40,  Resources.Load<Material>("Materials/MetalS"), Resources.Load<Texture>("Textures/resource_metalS"), Localization.rtype_metalS_descr);
		prices[METAL_S_ID] = 15; demand[METAL_S_ID] = 4;

		float lowK = 0.7f, ore_lowK = 0.5f;
		metal_K_ore = new ResourceType(Localization.rtype_metalK_ore_name, METAL_K_ORE_ID, 2.7f, 25,  Resources.Load<Material>("Materials/MetalK"), Resources.Load<Texture>("Textures/resource_metalK_ore"), Localization.rtype_metalK_descr);
		prices[METAL_K_ORE_ID] = prices[METAL_K_ID] * lowK; demand[METAL_K_ORE_ID] = demand[METAL_K_ID] * ore_lowK;
		metal_M_ore = new ResourceType(Localization.rtype_metalM_ore_name, METAL_M_ORE_ID, 2.6f, 17.5f, Resources.Load<Material>("Materials/MetalM"), Resources.Load<Texture>("Textures/resource_metalM_ore"), Localization.rtype_metalM_descr);
		prices[METAL_M_ORE_ID] = prices[METAL_M_ID] * lowK; demand[METAL_M_ORE_ID] = demand[METAL_M_ID] * ore_lowK;
		metal_E_ore = new ResourceType(Localization.rtype_metalE_ore_name, METAL_E_ORE_ID, 2.3f, 1.5f, Resources.Load<Material>("Materials/MetalE"), Resources.Load<Texture>("Textures/resource_metalE_ore"), Localization.rtype_metalE_descr);
		prices[METAL_E_ORE_ID] = prices[METAL_E_ID] * lowK; demand[METAL_E_ORE_ID] = demand[METAL_E_ID] * ore_lowK;
		metal_N_ore = new ResourceType(Localization.rtype_metalN_ore_name, METAL_N_ORE_ID, 4, 1.5f, Resources.Load<Material>("Materials/MetalN"), Resources.Load<Texture>("Textures/resource_metalN_ore"), Localization.rtype_metalN_descr);
		prices[METAL_N_ORE_ID] = prices[METAL_N_ID] * lowK; demand[METAL_N_ORE_ID] = demand[METAL_N_ID] * ore_lowK;
		metal_P_ore = new ResourceType(Localization.rtype_metalP_ore_name, METAL_P_ORE_ID, 2.63f, 16, Resources.Load<Material>("Materials/MetalP"), Resources.Load<Texture>("Textures/resource_metalP_ore"), Localization.rtype_metalP_descr);
		prices[METAL_P_ORE_ID] = prices[METAL_P_ID] * lowK; demand[METAL_P_ORE_ID] = demand[METAL_P_ID] * ore_lowK;
		metal_S_ore = new ResourceType(Localization.rtype_metalS_ore_name, METAL_S_ORE_ID, 2.2f, 20, Resources.Load<Material>("Materials/MetalS"), Resources.Load<Texture>("Textures/resource_metalS_ore"), Localization.rtype_metalS_descr);
		prices[METAL_S_ORE_ID] = prices[METAL_S_ID] * lowK; demand[METAL_S_ORE_ID] = demand[METAL_S_ID] * ore_lowK;

		mineral_F = new ResourceType("Mineral F", MINERAL_F_ID,1.1f, 1, Resources.Load<Material>("Materials/MineralF"), Resources.Load<Texture>("Textures/resource_mineralF"), Localization.rtype_mineralF_descr);
		prices[MINERAL_F_ID] = 3; demand[MINERAL_F_ID] = 10;
		Fuel = new ResourceType(Localization.rtype_fuel_name, FUEL_ID, 0.1f, 0, PoolMaster.default_material, Resources.Load<Texture>("Textures/resource_fuel"), Localization.rtype_fuel_descr);
		prices[FUEL_ID] = prices[MINERAL_F_ID] * 4; demand[FUEL_ID] = 45;

		Plastics = new ResourceType("Plastics", PLASTICS_ID ,0.5f, 10, Resources.Load<Material>("Materials/Plastic"), Resources.Load<Texture>("Textures/resource_elasticMass"), Localization.rtype_plastics_descr);
		prices[PLASTICS_ID] = 1.5f; demand[PLASTICS_ID] = 2;
		Lumber = new ResourceType(Localization.rtype_lumber_name, LUMBER_ID, 0.5f, 5,  Resources.Load<Material>("Materials/Lumber"), Resources.Load<Texture>("Textures/resource_lumber"), Localization.rtype_lumber_descr); 
		prices[LUMBER_ID] = prices[PLASTICS_ID] / 10f; demand[LUMBER_ID] = demand[PLASTICS_ID] / 6f;
		mineral_L = new ResourceType("Mineral L",  MINERAL_L_ID ,1, 2, Resources.Load<Material>("Materials/MineralL"), Resources.Load<Texture>("Textures/resource_mineralL"), Localization.rtype_mineralL_descr);
		prices[MINERAL_L_ID] = prices[PLASTICS_ID] / 2f; demand [MINERAL_L_ID] = demand[PLASTICS_ID] *ore_lowK;

		Stone = new ResourceType(Localization.rtype_stone_name, STONE_ID, 2.5f, 30,  Resources.Load<Material>("Materials/Stone"), Resources.Load<Texture>("Textures/resource_stone"), Localization.rtype_stone_descr);
		prices[STONE_ID] = 0.2f; demand[STONE_ID] = 0.5f;
		Concrete = new ResourceType(Localization.rtype_concrete_name, CONCRETE_ID, 3, 38, Resources.Load<Material>("Materials/Concrete"), Resources.Load<Texture>("Textures/resource_concrete"), Localization.rtype_concrete_descr);
		prices[CONCRETE_ID] = prices[STONE_ID] * 1.5f; demand[CONCRETE_ID] = demand[STONE_ID] * 1.8f;

		Dirt = new ResourceType (Localization.rtype_dirt_name,DIRT_ID, 1, 1,   Resources.Load<Material>("Materials/Dirt"), Resources.Load<Texture>("Textures/resource_dirt"), Localization.rtype_dirt_descr);
		prices[DIRT_ID] = 0.5f; demand[DIRT_ID] = 1;
		Material fertileSoil_material = new Material(Dirt.material);	fertileSoil_material.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Farmland"));
		FertileSoil = new ResourceType(Localization.rtype_fertileSoil_name, FERTILE_SOIL_ID, 1, 1, fertileSoil_material, Resources.Load<Texture>("Textures/resource_fertileSoil"), Localization.rtype_fertileSoil_descr);
		prices[FERTILE_SOIL_ID] = prices[DIRT_ID]; demand[FERTILE_SOIL_ID] = demand[DIRT_ID];

		Graphonium = new ResourceType(Localization.rtype_graphonium_name, GRAPHONIUM_ID, 4, 2.5f, PoolMaster.energy_material, PoolMaster.energyCrystal_icon_tx, Localization.rtype_graphonium_descr);
		prices[GRAPHONIUM_ID] = prices[METAL_N_ID * 3];
		demand[GRAPHONIUM_ID] = 1;

		Supplies = new ResourceType(Localization.rtype_supplies_name, SUPPLIES_ID, 0.1f, 0.1f, PoolMaster.default_material, Resources.Load<Texture>("Textures/resource_supplies"), Localization.rtype_supplies_descr);
		prices[SUPPLIES_ID] = prices[FOOD_ID] * 2; demand[SUPPLIES_ID] = demand[FOOD_ID] * 0.95f;

		materialsForCovering = new ResourceType[] {ResourceType.Concrete, ResourceType.Dirt, ResourceType.metal_K, ResourceType.metal_S, ResourceType.Stone};
	}

	public static ResourceType GetResourceTypeById(int f_id) {
		if (f_id > resourceTypesArray.Length || f_id < 0) {
			// return custom id's
			return ResourceType.Nothing;
		}
		else return resourceTypesArray[f_id];
	}
	public static Material GetMaterialById(int f_id) {
		Material m = PoolMaster.default_material;
		if (f_id > resourceTypesArray.Length || f_id < 0) {
			// return custom resource material
			return m;
		}
		else {
			if (resourceTypesArray[f_id] != null) m= resourceTypesArray[f_id].material;
		}
		if (m != null) return m; else return PoolMaster.default_material;
	}

	public static Rect GetTextureRect (int f_id) {
		float p = 0.125f;
		switch (f_id) {
		default:	return new Rect(0,0,p,p);
		case STONE_ID : return new Rect(5 *p, 5*p, p,p);
		case  DIRT_ID  : return new Rect(p, 7 *p, p, p);
		case LUMBER_ID : return new Rect( 6 *p, 7 *p, p,p);
		case METAL_K_ID : return new Rect(p, 6 *p, p,p);
		case METAL_M_ID : return new Rect(3 *p, 6 *p,p,p);
		case METAL_E_ID : return new Rect( 7 *p, 7 *p, p,p);
		case METAL_N_ID : return new Rect( 5 * p, 6 *p, p, p);
		case METAL_P_ID : return new Rect( 7 *p, 6 *p, p, p);
		case METAL_S_ID : return new Rect(p, 5 *p, p, p);
		case MINERAL_F_ID : return new Rect( 3 *p, 5 *p, p,p);
		case MINERAL_L_ID : return new Rect( 4 *p, 5 *p, p,p);
		case PLASTICS_ID : return new Rect( 2 *p, 7 *p, p,p);
		case FOOD_ID : return new Rect( 4 *p, 7 *p, p,p);
		case CONCRETE_ID : return new Rect(0, 7 *p,p,p);
		case METAL_K_ORE_ID : return new Rect(2 *p, 6 *p, p,p);
		case METAL_M_ORE_ID : return new Rect( 4 *p, 6 *p, p,p);
		case METAL_E_ORE_ID : return new Rect(p, 6 *p, p,p);
		case METAL_N_ORE_ID : return new Rect( 6 *p, 6 *p,p,p);
		case METAL_P_ORE_ID : return new Rect(0, 5 *p,p,p);
		case METAL_S_ORE_ID : return new Rect( 2 *p, 5 *p, p,p);
		case FERTILE_SOIL_ID : return new Rect(3 *p, 7 *p,p,p);
		case FUEL_ID : return new Rect( 5 *p, 7 *p,p,p);
		case GRAPHONIUM_ID : return new Rect( 7 *p, 5 *p,p,p);
		case SUPPLIES_ID : return new Rect(6 *p, 5 *p,p,p);
		}
	}
}


public struct ResourceContainer {
	public readonly ResourceType type;
	public float volume{get; private set;}
	public static readonly ResourceContainer Empty;

	public ResourceContainer(ResourceType f_type, float f_volume) {
		type= f_type; 
		if (f_volume < 0) f_volume = 0;
		volume = f_volume;
	}
	static ResourceContainer() {
		Empty = new ResourceContainer (ResourceType.resourceTypesArray[0], 0);
	}
}



