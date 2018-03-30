using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	public static PoolMaster current;
	public Material[] grassland_ready_25, grassland_ready_50;
	public Material dryingLeaves_material, leaves_material;
	public GameObject tree_pref, grass_pref, deadTree_pref;
	public static GameObject quad_pref {get;private set;}
	public static Material dirt_material, grass_material, stone_material, lumber_material,
		default_material, lr_red_material, lr_green_material, 
		metalK_material, metalM_material, metalE_material, metalN_material, metalP_material, metalS_material, mineralF_material, mineralL_material,
	elasticMass_material, concrete_material;
	public static Mesh plane_excavated_025, plane_excavated_05,plane_excavated_075;

	public const int STONE_ID = 1, DIRT_ID = 2, GRASS_ID = 3, LUMBER_ID = 4, METAL_K_ID = 5, METAL_M_ID = 6, METAL_E_ID = 7,
	METAL_N_ID = 8, METAL_P_ID = 9,  METAL_S_ID = 10, MINERAL_F_ID = 11, MINERAL_L_ID = 12, ELASTIC_MASS_ID = 13;	

	const int MAX_POPULATION = 256; int lastActiveHuman = -1;

	void Awake() {
		if (current != null && current != this) Destroy(current); 
		current = this;

		metalK_material = Resources.Load<Material>("Materials/MetalK");
		metalM_material = Resources.Load<Material>("Materials/MetalM");
		metalE_material = Resources.Load<Material>("Materials/MetalE");
		metalN_material = Resources.Load<Material>("Materials/MetalN");
		metalP_material = Resources.Load<Material>("Materials/MetalP");
		metalS_material = Resources.Load<Material>("Materials/MetalS");
		mineralF_material = Resources.Load<Material>("Materials/MineralF");
		mineralL_material = Resources.Load<Material>("Materials/MineralL");
		elasticMass_material = Resources.Load<Material>("Materials/Plastic");
		concrete_material = Resources.Load<Material>("Materials/Concrete");

		plane_excavated_025 = Resources.Load<Mesh>("Meshes/Plane_excavated_025");
		plane_excavated_05 = Resources.Load<Mesh>("Meshes/Plane_excavated_05");
		plane_excavated_075 = Resources.Load<Mesh>("Meshes/Plane_excavated_075");

		lr_red_material = Resources.Load<Material>("Materials/GUI_Red");
		lr_green_material = Resources.Load<Material>("Materials/GUI_Green");

		quad_pref = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad_pref.GetComponent<MeshRenderer>().enabled =false;
		lumber_material = Resources.Load<Material>("Materials/Lumber");
		dirt_material = Resources.Load<Material>("Materials/Dirt");
		grass_material = Resources.Load<Material>("Materials/Grass");
		stone_material = Resources.Load<Material>("Materials/Stone");
		default_material = Resources.Load<Material>("Materials/Default");

		dryingLeaves_material = Resources.Load<Material>("Materials/DryingLeaves");
		leaves_material = Resources.Load<Material>("Materials/Leaves");

		deadTree_pref = Resources.Load<GameObject>("Lifeforms/DeadTree");deadTree_pref.SetActive(false);
		tree_pref = Resources.Load<GameObject>("Lifeforms/Tree");tree_pref.SetActive(false);
		grass_pref = Resources.Load<GameObject>("Lifeforms/Grass"); grass_pref.SetActive(false);

		grassland_ready_25 = new Material[4]; grassland_ready_50 = new Material[4];
		grassland_ready_25[0] = new Material(dirt_material); grassland_ready_25[0].name ="grassland_25_0";
		grassland_ready_25[0].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_0"));
		grassland_ready_25[1] = new Material(dirt_material); grassland_ready_25[1].name ="grassland_25_1";
		grassland_ready_25[1].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_1"));
		grassland_ready_25[2] = new Material(dirt_material); grassland_ready_25[2].name ="grassland_25_2";
		grassland_ready_25[2].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_2"));
		grassland_ready_25[3] = new Material(dirt_material); grassland_ready_25[3].name ="grassland_25_3";
		grassland_ready_25[3].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_3"));

		grassland_ready_50[0] = new Material(dirt_material); grassland_ready_50[0].name ="grassland_50_0";
		grassland_ready_50[0].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_0"));
		grassland_ready_50[1] = new Material(dirt_material); grassland_ready_50[1].name ="grassland_50_1";
		grassland_ready_50[1].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_1"));
		grassland_ready_50[2] = new Material(dirt_material); grassland_ready_50[2].name ="grassland_50_2";
		grassland_ready_50[2].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_2"));
		grassland_ready_50[3] = new Material(dirt_material); grassland_ready_50[3].name ="grassland_50_3";
		grassland_ready_50[3].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_3"));
	}


	public static Material GetMaterialById(int id) {
		switch (id) {
		case STONE_ID: return stone_material;  break;
		case DIRT_ID: return dirt_material; break;
		case GRASS_ID: return grass_material; break;
		default: return default_material; break;
		}
	}
}
