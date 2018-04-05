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
		mineral_F, mineral_L, ElasticMass, Concrete, FertileSoil;
	public const int STONE_ID = 1, DIRT_ID = 2, LUMBER_ID = 4, METAL_K_ID = 5, METAL_M_ID = 6, METAL_E_ID = 7,
	METAL_N_ID = 8, METAL_P_ID = 9,  METAL_S_ID = 10, MINERAL_F_ID = 11, MINERAL_L_ID = 12, ELASTIC_MASS_ID = 13, FOOD_ID = 14,
	CONCRETE_ID = 15, METAL_K_ORE_ID = 16, METAL_M_ORE_ID = 17, METAL_E_ORE_ID = 18, METAL_N_ORE_ID = 19, METAL_P_ORE_ID = 20,
	METAL_S_ORE_ID = 21, FERTILE_SOIL_ID = 22;
	public static readonly ResourceType[] resourceTypesArray;

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
		resourceTypesArray = new ResourceType[23];
		Nothing = new ResourceType(Localization.rtype_nothing_name, 0, 0, 0, PoolMaster.default_material, Resources.Load<Texture>("Textures/resource_empty"), Localization.rtype_nothing_descr);
		Lumber = new ResourceType(Localization.rtype_lumber_name, LUMBER_ID, 0.5f, 5,  Resources.Load<Material>("Materials/Lumber"), Resources.Load<Texture>("Textures/resource_lumber"), Localization.rtype_lumber_descr);
		Stone = new ResourceType(Localization.rtype_stone_name, STONE_ID, 2.5f, 30,  Resources.Load<Material>("Materials/Stone"), Resources.Load<Texture>("Textures/resource_stone"), Localization.rtype_stone_descr);
		Dirt = new ResourceType (Localization.rtype_dirt_name,DIRT_ID, 1, 1,   Resources.Load<Material>("Materials/Dirt"), Resources.Load<Texture>("Textures/resource_dirt"), Localization.rtype_dirt_descr);
		Food = new ResourceType(Localization.rtype_food_name, FOOD_ID, 0.1f, 0.1f, PoolMaster.default_material, Resources.Load<Texture>("Textures/resource_supplies"), Localization.rtype_food_descr);

		metal_K_ore = new ResourceType(Localization.rtype_metalK_ore_name, METAL_K_ORE_ID, 2.7f, 25,  Resources.Load<Material>("Materials/MetalK"), Resources.Load<Texture>("Textures/resource_metalK_ore"), Localization.rtype_metalK_descr);
		metal_M_ore = new ResourceType(Localization.rtype_metalM_ore_name, METAL_M_ORE_ID, 2.6f, 17.5f, Resources.Load<Material>("Materials/MetalM"), Resources.Load<Texture>("Textures/resource_metalM_ore"), Localization.rtype_metalM_descr);
		metal_E_ore = new ResourceType(Localization.rtype_metalE_ore_name, METAL_E_ORE_ID, 2.3f, 1.5f, Resources.Load<Material>("Materials/MetalE"), Resources.Load<Texture>("Textures/resource_metalE_ore"), Localization.rtype_metalE_descr);
		metal_N_ore = new ResourceType(Localization.rtype_metalN_ore_name, METAL_N_ORE_ID, 4, 1.5f, Resources.Load<Material>("Materials/MetalN"), Resources.Load<Texture>("Textures/resource_metalN_ore"), Localization.rtype_metalN_descr);
		metal_P_ore = new ResourceType(Localization.rtype_metalP_ore_name, METAL_P_ORE_ID, 2.63f, 16, Resources.Load<Material>("Materials/MetalP"), Resources.Load<Texture>("Textures/resource_metalP_ore"), Localization.rtype_metalP_descr);
		metal_S_ore = new ResourceType(Localization.rtype_metalS_ore_name, METAL_S_ORE_ID, 2.2f, 20, Resources.Load<Material>("Materials/MetalS"), Resources.Load<Texture>("Textures/resource_metalS_ore"), Localization.rtype_metalS_descr);

		metal_K = new ResourceType("Metal K", METAL_K_ID, 0.7f, 50, Resources.Load<Material>("Materials/MetalK") , Resources.Load<Texture>("Textures/resource_metalK"), Localization.rtype_metalK_descr);
		metal_M = new ResourceType("Metal M", METAL_M_ID, 0.6f, 35,  Resources.Load<Material>("Materials/MetalM"), Resources.Load<Texture>("Textures/resource_metalM"), Localization.rtype_metalM_descr);
		metal_E = new ResourceType("Metal E", METAL_E_ID, 0.3f, 3,  Resources.Load<Material>("Materials/MetalE"), Resources.Load<Texture>("Textures/resource_metalE"), Localization.rtype_metalE_descr);
		metal_N = new ResourceType("Metal N", METAL_N_ID, 2, 3,  Resources.Load<Material>("Materials/MetalKN"), Resources.Load<Texture>("Textures/resource_metalN"), Localization.rtype_metalN_descr);
		metal_P = new ResourceType("Metal P", METAL_P_ID, 0.63f, 32,  Resources.Load<Material>("Materials/MetalP"), Resources.Load<Texture>("Textures/resource_metalP"), Localization.rtype_metalP_descr);
		metal_S= new ResourceType("Metal S", METAL_S_ID, 0.2f, 40,  Resources.Load<Material>("Materials/MetalS"), Resources.Load<Texture>("Textures/resource_metalS"), Localization.rtype_metalS_descr);

		mineral_F = new ResourceType("Mineral F", MINERAL_F_ID,1.1f, 1, Resources.Load<Material>("Materials/MineralF"), Resources.Load<Texture>("Textures/resource_mineralF"), Localization.rtype_mineralF_descr);
		mineral_L = new ResourceType("Mineral L",  MINERAL_L_ID ,1, 2, Resources.Load<Material>("Materials/MineralL"), Resources.Load<Texture>("Textures/resource_mineralL"), Localization.rtype_mineralL_descr);

		ElasticMass = new ResourceType("Elastic Mass", ELASTIC_MASS_ID ,0.5f, 10, Resources.Load<Material>("Materials/Plastic"), Resources.Load<Texture>("Textures/resource_elasticMass"), Localization.rtype_elasticMass_descr);
		Concrete = new ResourceType(Localization.rtype_concrete_name, CONCRETE_ID, 3, 38, Resources.Load<Material>("Materials/Concrete"), Resources.Load<Texture>("Textures/resource_concrete"), Localization.rtype_concrete_descr);
		Material fertileSoil_material = new Material(Dirt.material);	fertileSoil_material.SetTexture("_MainTex", Resources.Load<Texture>("Textures/Farmland"));
		FertileSoil = new ResourceType(Localization.rtype_fertileSoil_name, FERTILE_SOIL_ID, 1, 1, fertileSoil_material, Resources.Load<Texture>("Textures/resource_fertileSoil"), Localization.rtype_fertileSoil_descr);
		}

		public static bool operator ==(ResourceType lhs, ResourceType rhs) {return lhs.Equals(rhs);}
		public static bool operator !=(ResourceType lhs, ResourceType rhs) {return !(lhs.Equals(rhs));}

	public static ResourceType GetResourceTypeById(int f_id) {
		if (f_id > resourceTypesArray.Length || f_id < 0) {
			// return custom id's
			return ResourceType.Nothing;
		}
		else return resourceTypesArray[f_id];
	}
	public static Material GetMaterialById(int f_id) {
		if (f_id > resourceTypesArray.Length || f_id < 0) {
			// return custom resource material
			return PoolMaster.default_material;
		}
		else return resourceTypesArray[f_id].material;
	}
}



