using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	public static PoolMaster current;
	public Material[] grassland_ready_25, grassland_ready_50;

	void Awake() {
		if (current != null && current != this) Destroy(current); 
		current = this;
		grassland_ready_25 = new Material[4]; grassland_ready_50 = new Material[4];
		grassland_ready_25[0] = new Material(Block.dirt_material); grassland_ready_25[0].name ="grassland_25_0";
		grassland_ready_25[0].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_0"));
		grassland_ready_25[1] = new Material(Block.dirt_material); grassland_ready_25[1].name ="grassland_25_1";
		grassland_ready_25[1].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_1"));
		grassland_ready_25[2] = new Material(Block.dirt_material); grassland_ready_25[2].name ="grassland_25_2";
		grassland_ready_25[2].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_2"));
		grassland_ready_25[3] = new Material(Block.dirt_material); grassland_ready_25[3].name ="grassland_25_3";
		grassland_ready_25[3].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_25_3"));

		grassland_ready_50[0] = new Material(Block.dirt_material); grassland_ready_50[0].name ="grassland_50_0";
		grassland_ready_50[0].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_0"));
		grassland_ready_50[1] = new Material(Block.dirt_material); grassland_ready_50[1].name ="grassland_50_1";
		grassland_ready_50[1].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_1"));
		grassland_ready_50[2] = new Material(Block.dirt_material); grassland_ready_50[2].name ="grassland_50_2";
		grassland_ready_50[2].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_2"));
		grassland_ready_50[3] = new Material(Block.dirt_material); grassland_ready_50[3].name ="grassland_50_3";
		grassland_ready_50[3].SetTexture("_MainTex", Resources.Load<Texture>("Textures/grassland_ready_50_3"));
	}


}
