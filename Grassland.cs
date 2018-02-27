using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PixelPosByte {
	public byte x, y;
	public bool exists;
	public static PixelPosByte Empty;
	public PixelPosByte (byte xpos, byte ypos) {x = xpos; y = ypos; exists = true;}
	static PixelPosByte() {Empty = new PixelPosByte(0,0); Empty.exists = false;}

	public static bool operator ==(PixelPosByte lhs, PixelPosByte rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(PixelPosByte lhs, PixelPosByte rhs) {return !(lhs.Equals(rhs));}
}

public class Grassland : MonoBehaviour {
	public const int MAX_LIFEFORMS_COUNT= 8;
	public const float LIFEPOWER_TO_PREPARE = 16;

	Block myBlock;
	BlockSurface surfacePlane;
	float fertility = 1, progress = 0, lifeTimer = 0;
	public float lifepower {get; private set;}
	Plant[] plants;

	void Awake() {
		lifepower = 0; 
		plants = new Plant[MAX_LIFEFORMS_COUNT];
	}

	void Update() {
		if (lifepower == 0) return;
		if (lifeTimer > 0) {
			lifeTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (lifeTimer <=0) {
				if (lifepower > 0) {
					float lifeDelta = Time.deltaTime * fertility * GameMaster.lifeGrowCoefficient;
					if (lifeDelta < lifepower) lifeDelta = lifepower;
					if (progress < LIFEPOWER_TO_PREPARE) {
						if (lifeDelta > LIFEPOWER_TO_PREPARE - progress) lifeDelta = LIFEPOWER_TO_PREPARE - progress;
						float prevProgress = progress;
						progress += lifeDelta; lifepower -= lifeDelta;
						if (progress > LIFEPOWER_TO_PREPARE/2f) {
							if (progress >= LIFEPOWER_TO_PREPARE) {
								surfacePlane.surfaceRenderer.material = Block.grass_material;
							}
							else {
								if (prevProgress < LIFEPOWER_TO_PREPARE/2f) {
									int index = (int)(Random.value * PoolMaster.current.grassland_ready_50.Length - 1);
									surfacePlane.surfaceRenderer.material = PoolMaster.current.grassland_ready_50[index];
								}
							}
						}
						else {
							if (progress > LIFEPOWER_TO_PREPARE/4f) {
									if (prevProgress < LIFEPOWER_TO_PREPARE/4f) {
									MeshRenderer mr = myBlock.GetSurfacePlane(); 
									int index = (int)(Random.value * PoolMaster.current.grassland_ready_25.Length - 1);
									surfacePlane.surfaceRenderer.material = PoolMaster.current.grassland_ready_25[index];
								}
							}
						}
					}
					else { // totally covered by grass
						int pos = (int)(Random.value * (plants.Length - 1));
						if (plants[pos] == null) {
							PixelPosByte ppos = surfacePlane.PutInCell(Content.Plant);
							if (ppos != PixelPosByte.Empty) {
								plants[pos] = (Instantiate(PoolMaster.current.grass_pref) as GameObject).GetComponent<Plant>(); lifepower-=1;
								plants[pos].gameObject.SetActive(true);
								plants[pos].GetComponent<Plant2D>().SetPosition(ppos, myBlock.body.transform);
							}
							else print("no empty cells!");
						}
						else {
							if ( plants[pos].full && plants[pos].GetComponent<Plant2D>() != null ) ReplaceGrassToTree(pos);
							else plants[pos].AddLifepower(lifeDelta * (1 + lifepower / 1000f));
						}
					}
				}
				lifeTimer = Chunk.LIFEPOWER_TICK;
			}
		}
	}

	void ReplaceGrassToTree(int pos) {
		Tree t = (Instantiate(PoolMaster.current.tree_pref) as GameObject).GetComponent<Tree>();
		t.gameObject.SetActive(true);
		t.SetPosition(plants[pos].cellPosition, myBlock.body.transform);
		t.AddLifepower( plants[pos].lifepower);
		Destroy(plants[pos].gameObject);
		plants[pos] = t;
	}

	public void AddLifepower(float count) {
		lifepower += count;
		if (lifeTimer == 0 ) lifeTimer = Chunk.LIFEPOWER_TICK;
	}
	public void TakeLifepower(int count) {lifepower -= count;}

	public void SetBlock(Block b) {
		myBlock = b;
		surfacePlane = b.GetSurfacePlane().GetComponent<BlockSurface>();
	}

	public void Annihilation() {
		
	}
}
