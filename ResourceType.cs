using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceType {
	public readonly float mass , toughness;
	public readonly Material material; 
	public readonly string name;
	public readonly string description;
		public static readonly ResourceType Nothing, Lumber, Stone, Dirt,Food, 
		metal_K_ore, metal_M_ore, metal_E_ore, metal_N_ore, metal_P_ore, metal_S_ore, 
		metal_K, metal_M, metal_E, metal_N, metal_P, metal_S,
	mineral_F, mineral_L, ElasticMass;

	public ResourceType(string f_name, float f_mass, float f_toughness, Material m, string f_descr) {
		name = f_name;	
		mass = f_mass;
		toughness = f_toughness;
		material = m;
		description = f_descr;
		}

		static ResourceType() {
			Nothing = new ResourceType(Localization.rtype_nothing_name, 0, 0, PoolMaster.default_material, Localization.rtype_nothing_descr);
			Lumber = new ResourceType(Localization.rtype_lumber_name,0.5f, 5,  PoolMaster.lumber_material, Localization.rtype_lumber_descr);
			Stone = new ResourceType(Localization.rtype_stone_name, 2.5f, 30, PoolMaster.stone_material, Localization.rtype_stone_descr);
			Dirt = new ResourceType (Localization.rtype_dirt_name, 1, 1,  PoolMaster.dirt_material, Localization.rtype_dirt_descr);
			Food = new ResourceType(Localization.rtype_food_name, 0.1f, 0.1f, PoolMaster.default_material, Localization.rtype_food_descr);

		metal_K_ore = new ResourceType(Localization.rtype_metalK_ore_name, 2.7f, 25, PoolMaster.metalK_material, Localization.rtype_metalK_descr);
		metal_M_ore = new ResourceType(Localization.rtype_metalM_ore_name, 2.6f, 17.5f, PoolMaster.metalM_material, Localization.rtype_metalM_descr);
		metal_E_ore = new ResourceType(Localization.rtype_metalE_ore_name, 2.3f, 1.5f, PoolMaster.metalE_material, Localization.rtype_metalE_descr);
		metal_N_ore = new ResourceType(Localization.rtype_metalN_ore_name, 4, 1.5f, PoolMaster.metalN_material, Localization.rtype_metalN_descr);
		metal_P_ore = new ResourceType(Localization.rtype_metalP_ore_name, 2.63f, 16, PoolMaster.metalP_material, Localization.rtype_metalP_descr);
		metal_S_ore = new ResourceType(Localization.rtype_metalS_ore_name, 2.2f, 20, PoolMaster.metalS_material, Localization.rtype_metalS_descr);

		metal_K = new ResourceType("Metal K", 0.7f, 50, PoolMaster.metalK_material, Localization.rtype_metalK_descr);
		metal_M = new ResourceType("Metal M", 0.6f, 35, PoolMaster.metalM_material, Localization.rtype_metalM_descr);
		metal_E = new ResourceType("Metal E", 0.3f, 3, PoolMaster.metalE_material, Localization.rtype_metalE_descr);
		metal_N = new ResourceType("Metal N", 2, 3, PoolMaster.metalN_material, Localization.rtype_metalN_descr);
		metal_P = new ResourceType("Metal P", 0.63f, 32, PoolMaster.metalP_material, Localization.rtype_metalP_descr);
		metal_S= new ResourceType("Metal S", 0.2f, 40, PoolMaster.metalS_material, Localization.rtype_metalS_descr);

		mineral_F = new ResourceType("Mineral F",1.1f, 1, PoolMaster.mineralF_material, Localization.rtype_mineralF_descr);
		mineral_L = new ResourceType("Mineral L",1, 2, PoolMaster.mineralL_material, Localization.rtype_mineralL_descr);

		ElasticMass = new ResourceType("Elastic Mass", 0.5f, 10, PoolMaster.elasticMass_material, Localization.rtype_elasticMass_descr);
		}

		public static bool operator ==(ResourceType lhs, ResourceType rhs) {return lhs.Equals(rhs);}
		public static bool operator !=(ResourceType lhs, ResourceType rhs) {return !(lhs.Equals(rhs));}

	public static ResourceType GetResourceTypeByMaterialId(int mId) {
		switch (mId) {
		case PoolMaster.STONE_ID: return Stone; break;
		case PoolMaster.DIRT_ID: return Dirt;break;
		case  PoolMaster.GRASS_ID: return Dirt;break;
		case PoolMaster.LUMBER_ID: return Lumber;break; 
		case PoolMaster.METAL_K_ID : return metal_K;break;
		case PoolMaster.METAL_M_ID : return metal_M;break;
		case PoolMaster.METAL_E_ID : return metal_E;break;
		case PoolMaster.METAL_N_ID : return metal_N;break;
		case PoolMaster.METAL_P_ID : return metal_P;break;
		case PoolMaster.METAL_S_ID : return metal_S;break;
		case PoolMaster.MINERAL_F_ID: return mineral_F;break;
		case PoolMaster.MINERAL_L_ID : return mineral_L;break;
		case PoolMaster.ELASTIC_MASS_ID : return ElasticMass;break;
		default: return ResourceType.Nothing;
		}
	}

	public static float CalculateWorkflowToProcess(ResourceType Input, ResourceType Output) {
		float worktime = 1;
		if (Input == ResourceType.Lumber) {
			if (Output == ResourceType.ElasticMass) worktime = 30;
		}
		return worktime;
	}
	}



