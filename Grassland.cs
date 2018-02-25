using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PixelPosByte {
	public byte x, y;
	public PixelPosByte (byte xpos, byte ypos) {x = xpos; y = ypos;}
}

public class Grassland : MonoBehaviour {
	Block myBlock;
	public byte stage = 1, stagePart = 0;
	float progress = 0;
	float nextLimit = 0;
	public float lifepower {get; private set;}

	void Awake() {lifepower = 1000; nextLimit = Chunk.MAX_LIFEPOWER_TRANSFER; stage = 1;}

	void Update() {
		if (lifepower > 0) {
			float delta = Time.deltaTime * (GameMaster.realMaster.lifeGrowCoefficient + lifepower / 10000f);
			progress += delta;
			float pc = progress / nextLimit;
			byte stage_part = 0;
			if (pc > 0.5f ) {if (pc > 0.75f) stage_part = 3; else stage_part = 2;}
			else {if (pc > 0.25f) stage_part = 1;}
			if (stage_part != stagePart) {
				switch (stage) {
				case 1:
					MeshRenderer mr = myBlock.GetSurfacePlane();
					switch (stage_part) {
					case 1: mr.material = PoolMaster.current.grassland_ready_25[(int)(Random.value * (PoolMaster.current.grassland_ready_25.Length - 1))]; break;
					case 2: mr.material =  PoolMaster.current.grassland_ready_50[(int)(Random.value * (PoolMaster.current.grassland_ready_50.Length - 1))];	break;
					}
					break;
				}
				stagePart = stage_part;
			}
			lifepower -= delta;
			if (progress >= nextLimit) {
				stage++;
				switch (stage) {
				case 2: MeshRenderer mr = myBlock.GetSurfacePlane(); mr.material = Block.grass_material;break;
				}
				nextLimit = Mathf.Pow(Chunk.MAX_LIFEPOWER_TRANSFER, stage);
				progress = 0;
			}
		}
	}
		
	public void AddLifepower(int count) {
		lifepower += count; 
	}
	public void TakeLifepower(int count) {lifepower -= count;}

	public void SetBlock(Block b) {
		myBlock = b;
	}
}
