using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour {
	public static Constructor main;
	public float TERRAIN_ROUGHNESS = 0.3f;
	Chunk c;
	public float seed = 1.1f;
	public int lifepowerToGeneration = 50000;

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
					dat[x,y + size/2,z] = ResourceType.STONE_ID;
				}
				if (Random.value > 0.8f) dat[x,height+size/2-2,z] = ResourceType.DIRT_ID; 
				if (Random.value < 0.95f) dat[x,height+size/2-1,z] = ResourceType.DIRT_ID; 
				//down part
				float pc = (1 - Mathf.Sqrt((x - size/2) * (x-size/2) + (z - size/2) * (z - size/2)) / radius);
				pc *= pc * pc;
				height = (int)(pc * size/2  + size/2 * perlin);
				if (height < 0) height = 1; else if (height > size/2) height = size/2;
				for (y = 0; y <= height; y++) {
					dat[x,size/2  - y,z] = ResourceType.STONE_ID;
				}
			}
		}
		GameObject g = new GameObject("chunk");
		c = g.AddComponent<Chunk>();
		GameMaster.mainChunk = c;
		c.SetChunk(dat);
		NatureCreation(c);
	}

	void NatureCreation(Chunk chunk) {
		SurfaceBlock[,] surface = chunk.GetSurface();
		byte x = (byte)(Random.value * (Chunk.CHUNK_SIZE-2) + 1);
		byte z = (byte)(Random.value * (Chunk.CHUNK_SIZE-2) + 1);
		x = Chunk.CHUNK_SIZE / 2;z=Chunk.CHUNK_SIZE/2;
		//surface[x,z].ReplaceMaterial(PoolMaster.grass_material);
		//chunk.SpreadBlocks(x,z, PoolMaster.GRASS_ID);
		chunk.GenerateNature(new PixelPosByte(x,z), lifepowerToGeneration);

		GameObject pref;
		if (Random.value > 0.5f) pref = Resources.Load<GameObject>("Structures/Tree of Life") ; else pref = Resources.Load<GameObject>("Structures/LifeStone");
		MultiblockStructure ms = Instantiate(pref).GetComponent<MultiblockStructure>();
		SurfaceBlock sb = chunk.GetSurfaceBlock(x,z);
		ms.SetBasement(sb, PixelPosByte.zero);
	}

}
