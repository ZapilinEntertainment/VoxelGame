using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour {
	public static Constructor main;
	public float TERRAIN_ROUGHNESS = 0.3f;
	Chunk c;
	public float seed = 1.1f;

	// Use this for initialization
	void Awake () {
		if (main != null) {Destroy(main); main = this;} // singleton pattern
		seed += System.DateTime.Now.Second;
		ConstructChunk();
	}

	void Update() {
		if (Input.GetKeyDown("g")) c.ClearChunk();
	}

	void ConstructChunk() {
		int size = Chunk.CHUNK_SIZE;
		int[,,] dat = new int[size, size ,size ];
		float radius = size * Mathf.Sqrt(2);
		for (int x =0; x< size ; x++) {
			for (int z= 0; z< size ; z++)
			{
				float cs = size ;
				float perlin = Mathf.PerlinNoise(x/cs * (10 * TERRAIN_ROUGHNESS), z/cs * (10 * TERRAIN_ROUGHNESS));
				//perlin += pc; if (perlin > 1) perlin = 1;
				int height = (int)(size/2 * perlin);
				if (height < 2) height = 2; else	if (height > size/2  ) height = size/2 ;
				int y = 0;
				for (; y < height; y++) {
					dat[x,y + size/2,z] = Block.STONE_ID;
				}
				if (Random.value > 0.8f) dat[x,height+size/2-2,z] = Block.DIRT_ID; 
				if (Random.value < 0.95f) dat[x,height+size/2-1,z] = Block.DIRT_ID; 
				//down part
				float pc = (1 - Mathf.Sqrt((x - size/2) * (x-size/2) + (z - size/2) * (z - size/2)) / radius);
				pc *= pc * pc;
				height = (int)(pc * size/2  + size/2 * perlin);
				if (height < 0) height = 1; else if (height > size/2) height = size/2;
				for (y = 0; y <= height; y++) {
					dat[x,size/2  - y,z] = Block.STONE_ID;
				}
			}
		}
		GameObject g = new GameObject("chunk");
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		NatureCreation(c);
	}

	void NatureCreation(Chunk chunk) {
		int[,] surface = chunk.GetSurface();
		int x = (int)(Random.value * (Chunk.CHUNK_SIZE-2)) + 1;
		int z = (int)(Random.value * (Chunk.CHUNK_SIZE-2)) + 1;
		Block b = chunk.GetBlock(x,surface[x,z],z);
		chunk.ReplaceBlock(x,surface[x,z], z, Block.GRASS_ID);
		GameObject pref;
		if (Random.value > 0.5f) pref = Resources.Load<GameObject>("Structures/Tree of Life") ; else pref = Resources.Load<GameObject>("Structures/LifeStone");
		GameObject g = Instantiate(pref) as GameObject;
		chunk.AddStructure(g,x,surface[x,z], z);
		g.transform.parent = chunk.transform;
		g.transform.localPosition = new Vector3(x,surface[x,z], z);
		g.transform.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
		Structure str = g.GetComponent<Structure>();
		str.SetBasement(b);
		chunk.SpreadBlocks(x,z, Block.GRASS_ID);
	}

}
