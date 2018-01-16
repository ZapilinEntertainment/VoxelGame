using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour {
	public static Constructor main;
	Chunk c;

	// Use this for initialization
	void Awake () {
		if (main != null) {Destroy(main); main = this;} // singleton pattern
		ConstructSurface();
	}

	void Update() {
		if (Input.GetKeyDown("g")) c.ClearChunk();
	}

	void ConstructSurface() {
		int[,,] dat = new int[16,16,16];
		for (int i =0; i< 16; i++) {
			for (int j= 0; j< 16; j++)
			{
				int height = (int)(16* Mathf.PerlinNoise(i/16.0f,j/16.0f));
				for (int k =0; k< 16; k++) {
					if (k > height) break;
					dat[i,k,j] = 1;
				}
			}
		}
		GameObject g = new GameObject("chunk");
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);


		g = new GameObject("chunk2"); g.transform.position = new Vector3(-16,0,0);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk3"); g.transform.position = new Vector3(16,0,0);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk4"); g.transform.position = new Vector3(0,0,16);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk5"); g.transform.position = new Vector3(0,0,-16);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk2"); g.transform.position = new Vector3(-32,0,0);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk3"); g.transform.position = new Vector3(32,0,0);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk4"); g.transform.position = new Vector3(0,0,32);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
		g = new GameObject("chunk5"); g.transform.position = new Vector3(0,0,-32);
		c = g.AddComponent<Chunk>();
		c.SetChunk(dat);
	}

}
