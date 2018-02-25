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
		for (int x =0; x< size ; x++) {
			for (int z= 0; z< size ; z++)
			{
				float cs = size ;
				int height = (int)(size * Mathf.PerlinNoise(x/cs + seed,z/cs + seed));
				if (height < 2) height = 2;
				if (Random.value < TERRAIN_ROUGHNESS) { height += (int)(Random.value * TERRAIN_ROUGHNESS * height);}
				if (height >= size  ) height = size  - 1;
				int y = height;
				if (Random.value < 0.95f) dat[x,y,z] = Block.DIRT_ID; else dat[x,y,z] = Block.STONE_ID; y--;
				if (Random.value > 0.8f) dat[x,y,z] = Block.DIRT_ID; else dat[ x,y,z] = Block.STONE_ID; y --;
				for (; y > 0; y--) {
					dat[x,y,z] = Block.STONE_ID;
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
