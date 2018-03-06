using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	public static PoolMaster current;
	public Material[] grassland_ready_25, grassland_ready_50;
	public Material dryingLeaves_material, leaves_material;
	public GameObject tree_pref, grass_pref;
	public static GameObject quad_pref {get;private set;}
	public static Material dirt_material, grass_material, stone_material, default_material, lr_red_material, lr_green_material;
	public static Texture2D dirt_texture;
	public const int STONE_ID = 1, DIRT_ID = 2, GRASS_ID = 3;
	Human[] population; const int MAX_POPULATION = 256; int lastActiveHuman = -1;

	void Awake() {
		if (current != null && current != this) Destroy(current); 
		current = this;

		lr_red_material = Resources.Load<Material>("Materials/GUI_Red");
		lr_green_material = Resources.Load<Material>("Materials/GUI_Green");

		quad_pref = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad_pref.GetComponent<MeshRenderer>().enabled =false;
		dirt_material = Resources.Load<Material>("Materials/Dirt");
		grass_material = Resources.Load<Material>("Materials/Grass");
		stone_material = Resources.Load<Material>("Materials/Stone");
		default_material = Resources.Load<Material>("Materials/Default");
		dirt_texture = Resources.Load<Texture2D>("Textures/Dirt_tx");

		dryingLeaves_material = Resources.Load<Material>("Materials/DryingLeaves");
		leaves_material = Resources.Load<Material>("Materials/Leaves");

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

	public void StartPopulation(int x, Vector3 pos) {
		population = new Human[MAX_POPULATION];
		Human humanPref = Instantiate(Resources.Load<GameObject>("Prefs/Human")).GetComponent<Human>();
		int i = 0;
		population[i++] = humanPref;
		humanPref.transform.position = pos + Vector3.forward * Block.QUAD_SIZE / 2f * 0.9f;
		humanPref.FindHome();
		for (; i< x; i++) {
			population[i] = Instantiate(humanPref) as Human;
			population[i].transform.position = pos + Quaternion.AngleAxis(360f / (i + 1), Vector3.up) * Vector3.forward * Block.QUAD_SIZE / 2f * 0.9f;
			population[i].FindHome();
		}
		lastActiveHuman = x;
	}
	public Human GetNewHuman() {
		if (lastActiveHuman == MAX_POPULATION) return null;
		else {
			Human h = population[lastActiveHuman];
			if (h == null) {
				h = Instantiate(Resources.Load<GameObject>("Human")).GetComponent<Human>();
			}
			h.gameObject.SetActive(true);
			lastActiveHuman ++;
			return h;
		}
	}
	public void DisableHuman(Human h) {
		int i = 0;
		while (i < MAX_POPULATION) {
			if ( !population[i].Equals(h) )  continue;
				if (i != lastActiveHuman) {
					Human a = population[lastActiveHuman];
					population[lastActiveHuman] = h;
					population[i] = a;
				}
			h.gameObject.SetActive(false);
			lastActiveHuman--;
			break;
		}
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
