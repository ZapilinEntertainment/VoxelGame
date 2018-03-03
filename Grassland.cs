using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PixelPosByte {
	public byte x, y;
	public bool exists;
	public static PixelPosByte Empty, zero;
	public PixelPosByte (byte xpos, byte ypos) {x = xpos; y = ypos; exists = true;}
	public PixelPosByte (int xpos, int ypos) {
		if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0;
		x = (byte)xpos; y = (byte)ypos;
		exists = true;
	}
	static PixelPosByte() {
		Empty = new PixelPosByte(0,0); Empty.exists = false;
		zero = new PixelPosByte(0,0); // but exists
	}

	public static bool operator ==(PixelPosByte lhs, PixelPosByte rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(PixelPosByte lhs, PixelPosByte rhs) {return !(lhs.Equals(rhs));}
}

public class Grassland : MonoBehaviour {
	public const int MAX_LIFEFORMS_COUNT= 8;
	public const float LIFEPOWER_TO_PREPARE = 16;

	public SurfaceBlock myBlock {get;private set;}
	float fertility = 1, progress = 0, lifeTimer = 0;
	public float lifepower;
	List<Plant> plants;
	byte prevStage = 0;
	public byte level{get;private set;}

	void Awake() {
		lifepower = 0; 
		plants = new List<Plant>();
		level = 1;
	}

	void Update() {
		if (lifepower == 0) return;
		if (lifeTimer > 0) {
			lifeTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (lifeTimer <=0) {
				
				progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
				byte stage = 0; level = 1;
				if (progress > 0.5f) {if (progress == 1) {stage = 3;level = 2;} else stage = 2;}
				else { if (progress > 0.25f) stage = 1;}
				if (stage != prevStage) {
					switch (stage) {
					case 0: 						
						myBlock.surfaceRenderer.material = PoolMaster.dirt_material;
						break;
					case 1:
						int index1 = (int)(Random.value * (PoolMaster.current.grassland_ready_25.Length - 1));
						myBlock.surfaceRenderer.material = PoolMaster.current.grassland_ready_25[index1];
						break;
					case 2:
						int index2 = (int)(Random.value * (PoolMaster.current.grassland_ready_50.Length - 1));
						myBlock.surfaceRenderer.material = PoolMaster.current.grassland_ready_50[index2];
						break;
					case 3:
						myBlock.surfaceRenderer.material = PoolMaster.grass_material;
						break;
					}
					prevStage = stage;
				}

				if (lifepower > 2 * LIFEPOWER_TO_PREPARE) {
					int i = 0; bool allFilled_check = true;
					while (i < plants.Count) {
						if (plants[i] == null) plants.RemoveAt(i);
						else {
							if (plants[i].full == false) allFilled_check = false;
							i++;
						}
					}
						if (plants.Count < MAX_LIFEFORMS_COUNT) {
							level = 3;
							PixelPosByte ppos = myBlock.GetRandomCell();
							if (ppos != PixelPosByte.Empty) {
								Plant p = (Instantiate(PoolMaster.current.grass_pref) as GameObject).GetComponent<Plant>(); lifepower-=1;
								p.gameObject.SetActive(true);
								p.SetBasement(myBlock, ppos);
								plants.Add(p);
							}
								//else print("no empty cells!");
						}
						else {
						if (allFilled_check == false) {
							level = 4;
							int lifeDelta = (int) (Mathf.Pow(Chunk.MAX_LIFEPOWER_TRANSFER * fertility * GameMaster.lifeGrowCoefficient * GameMaster.gameSpeed, level));
							if (lifeDelta > lifepower) { lifeDelta = (int)lifepower;}
							int pos = (int)(Random.value * (plants.Count - 1));
							if (lifeDelta > plants[pos].maxLifepower - plants[pos].lifepower) lifeDelta = (int)(plants[pos].maxLifepower - plants[pos].lifepower);
							if ( plants[pos].full ) {
								if (plants[pos].GetComponent<Plant2D>() != null ) ReplaceGrassToTree(pos);
							}
							else {
								plants[pos].AddLifepower(lifeDelta); lifepower -= lifeDelta;
							}			
						}
						}
				}
				else { // lifepower falls down
					if (plants != null && lifepower <= 0) {
						for (int i =0; i< plants.Count;i++) {
							if (plants[i] != null) {
								if (plants[i].lifepower <= 0) {plants[i].Annihilate();plants.RemoveAt(i);}
								else lifepower += plants[i].TakeLifepower(Chunk.energy_take_speed * 2);
							}
							else {plants.RemoveAt(i);}
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
		myBlock.ReplaceStructure( new SurfaceRect (plants[pos].innerPosition.x, plants[pos].innerPosition.z, plants[pos].innerPosition.x_size, plants[pos].innerPosition.z_size, Content.Plant, t.gameObject));
		t.AddLifepower( (int)plants[pos].lifepower);
		plants[pos] = t;
	}

	public void AddLifepower(int count) {
		lifepower += count;
		if (lifeTimer == 0 ) lifeTimer = Chunk.LIFEPOWER_TICK;
	}
	public int TakeLifepower(int count) {
		if (count < 0) return 0 ;
		int lifeTransfer = count;
		if (count > lifepower) {if (lifepower >= 0) lifeTransfer = (int)lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		return lifeTransfer;
	}

	/// <summary>
	/// Use this only on pre-gen
	/// </summary>
	public void AddLifepowerAndCalculate(int count) {
		if (plants.Count != 0) return;

		if (count > 2 * LIFEPOWER_TO_PREPARE) {
			float freeEnergy = count - 2 * LIFEPOWER_TO_PREPARE;
			int plants_count = MAX_LIFEFORMS_COUNT;
			if (freeEnergy < MAX_LIFEFORMS_COUNT)  plants_count = (int)(freeEnergy - MAX_LIFEFORMS_COUNT); 
			List<PixelPosByte> positions = myBlock.GetRandomCells(plants_count);
			float[] lifepowers = new float[plants_count];
			float total = 0;
			for (int i =0; i< plants_count; i++) {
				lifepowers[i] = Random.value;
				total += lifepowers[i];
			}
			float lifePiece = count; lifePiece /= total;

			for (int i =0; i< plants_count; i++) {
				float energy = lifepowers[i] * lifePiece;
				if (energy >  Plant2D.MAXIMUM_LIFEPOWER) {
					Tree t = Instantiate(PoolMaster.current.tree_pref).GetComponent<Tree>();	t.gameObject.SetActive(true);
					int pos = (int) (Random.value * (positions.Count - 1));
					t.SetBasement(myBlock, positions[pos]);
					t.AddLifepower((int)energy);
					positions.RemoveAt(pos);
					plants.Add(t);
				}
				else {
					Plant2D pl = Instantiate(PoolMaster.current.grass_pref).GetComponent<Plant2D>(); pl.gameObject.SetActive(true);
					int pos = (int) (Random.value * (positions.Count - 1));
					pl.SetBasement(myBlock, positions[pos]);
					pl.AddLifepower((int)energy);
					positions.RemoveAt(pos);
					plants.Add(pl);
				}
			}
			lifepower = 2 * LIFEPOWER_TO_PREPARE;
		}
		else {
			lifepower = count;
			progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
			byte stage = 0; level = 1;
			if (progress > 0.5f) {if (progress == 1) {stage = 3;level = 2;} else stage = 2;}
			else { if (progress > 0.25f) stage = 1;}
			if (stage != prevStage) {
				switch (stage) {
				case 0: 						
					myBlock.surfaceRenderer.material = PoolMaster.dirt_material;
					break;
				case 1:
					int index1 = (int)(Random.value * (PoolMaster.current.grassland_ready_25.Length - 1));
					myBlock.surfaceRenderer.material = PoolMaster.current.grassland_ready_25[index1];
					break;
				case 2:
					int index2 = (int)(Random.value * (PoolMaster.current.grassland_ready_50.Length - 1));
					myBlock.surfaceRenderer.material = PoolMaster.current.grassland_ready_50[index2];
					break;
				case 3:
					myBlock.surfaceRenderer.material = PoolMaster.grass_material;
					break;
				}
				prevStage = stage;
			}
		}
		if (lifepower != 0 && lifeTimer == 0 ) lifeTimer = Chunk.LIFEPOWER_TICK;
	}

	public void SetBlock(SurfaceBlock b) {myBlock = b;}

	public void Annihilation() {
		for (int i =0; i< plants.Count; i++) {
			if (plants[i] != null) plants[i].Annihilate();
			else plants.RemoveAt(i);
		}
		myBlock.myChunk.AddLifePower((int)lifepower);
		myBlock.surfaceRenderer.material = PoolMaster.dirt_material;
		Destroy(this);
	}
}
