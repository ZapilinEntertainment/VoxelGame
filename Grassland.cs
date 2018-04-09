using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PixelPosByte {
	public byte x, y;
	public bool exists;
	public static PixelPosByte Empty, zero, one;
	public PixelPosByte (byte xpos, byte ypos) {x = xpos; y = ypos; exists = true;}
	public PixelPosByte (int xpos, int ypos) {
		if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0;
		x = (byte)xpos; y = (byte)ypos;
		exists = true;
	}
	static PixelPosByte() {
		Empty = new PixelPosByte(0,0); Empty.exists = false;
		zero = new PixelPosByte(0,0); // but exists
		one = new PixelPosByte(1,1);
	}

	public static bool operator ==(PixelPosByte lhs, PixelPosByte rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(PixelPosByte lhs, PixelPosByte rhs) {return !(lhs.Equals(rhs));}
}

public class Grassland : MonoBehaviour {
	public const int MAX_LIFEFORMS_COUNT= 8;
	public const float LIFEPOWER_TO_PREPARE = 16, LIFE_CREATION_TIMER = 30;

	public SurfaceBlock myBlock {get;private set;}
	float progress = 0, lifeTimer = 0;
	public float lifepower;
	List<Plant> plants;
	byte prevStage = 0;

	void Awake() {
		lifepower = 0; 
		plants = new List<Plant>();
	}

	void Update() {
		if (lifepower == 0) return;
		if (lifeTimer > 0) {
			lifeTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (lifeTimer <=0) {				
				progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
				byte stage = 0; 
				if (progress > 0.5f) {if (progress == 1) stage = 3; else stage = 2;}
				else { if (progress > 0.25f) stage = 1;}
				if (Mathf.Abs(stage - prevStage) > 1) {
					if (stage > prevStage) stage = (byte)(prevStage+1);
					else stage =  (byte)(prevStage - 1);
				}
				if (stage != prevStage) {
					switch (stage) {
					case 0: 						
						myBlock.surfaceRenderer.material = ResourceType.Dirt.material;
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
					if (stage > 2) {
						float lifepowerToSinglePlant = GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient;
						float lifepowerToShare = plants.Count * lifepowerToSinglePlant;
						if (lifepowerToShare > lifepower - 2 * LIFEPOWER_TO_PREPARE) {
							lifepowerToShare = lifepower - 2 * LIFEPOWER_TO_PREPARE;
							lifepowerToSinglePlant = lifepowerToShare / plants.Count;
						}
						bool allFull = true;
						for (int i = 0; i < plants.Count; i++) {
							if (plants[i] == null) {
								if (plants.Count > MAX_LIFEFORMS_COUNT) {
									plants.RemoveAt(i);
									continue;
								}
								else {
									PixelPosByte pos = myBlock.GetRandomCell();
									if (pos != PixelPosByte.Empty) {
										Plant p = PoolMaster.current.GetSapling();
										p.gameObject.SetActive(true);
										p.SetBasement(myBlock, pos);
										p.AddLifepower(lifepowerToSinglePlant);
										plants[i] = p;
										lifepower --;
									}
									else {
										plants.RemoveAt(i);
										continue;
									}
								}
							}
							if ( !plants[i].full ) {
								plants[i].AddLifepower( TakeLifepower(lifepowerToSinglePlant));
								allFull = false;
							}
							if (lifepower <= 0) break;
						}
						if (allFull && (plants.Count == MAX_LIFEFORMS_COUNT || myBlock.cellsStatus == 1)) {
							myBlock.myChunk.AddLifePower( TakeLifepower( GameMaster.MAX_LIFEPOWER_TRANSFER ) );
						}
					}
				}
				else { // lifepower falls down
					if (lifepower < 0 && plants.Count > 0) {
						float lifepowerNeeded = lifepower * -2;
						float lifepowerFromSinglePlant = lifepowerNeeded / plants.Count;
						for (int i = 0; i < plants.Count; i++) {
							if (plants[i] == null) {plants.RemoveAt(i); continue;}
							if (plants[i].lifepower <= 0) continue;
							lifepower += plants[i].TakeLifepower( lifepowerFromSinglePlant );
						}
					}
				}
				if (lifepower != 0) lifeTimer = GameMaster.LIFEPOWER_TICK;
			}
		}
	}

	public void AddLifepower(int count) {
		lifepower += count;
		if (lifeTimer == 0 ) lifeTimer = GameMaster.LIFEPOWER_TICK;
	}
	public int TakeLifepower(float count) {
		if (count < 0) return 0 ;
		float lifeTransfer = count;
		if (count > lifepower) {if (lifepower >= 0) lifeTransfer = lifepower; else lifeTransfer = 0;}
		lifepower -= lifeTransfer;
		return (int)lifeTransfer;
	}

	/// <summary>
	/// Use this only on pre-gen
	/// </summary>
	public void AddLifepowerAndCalculate(int count) {
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
				if (energy >  TreeSapling.MAXIMUM_LIFEPOWER) {
					Tree t = PoolMaster.current.GetTree();
					t.gameObject.SetActive(true);
					int pos = (int) (Random.value * (positions.Count - 1));
					t.SetBasement(myBlock, positions[pos]); // DETECTED FAILURE!
					t.SetLifepower(energy);
					positions.RemoveAt(pos);
					plants.Add(t);
				}
				else {
					TreeSapling pl = PoolMaster.current.GetSapling();
					int pos = (int) (Random.value * (positions.Count - 1));
					pl.SetBasement(myBlock, positions[pos]);
					pl.SetLifepower(energy);
					positions.RemoveAt(pos);
					plants.Add(pl);
				}
			}

			lifepower = 2 * LIFEPOWER_TO_PREPARE;
		}
		else 	lifepower = count;

			progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
			byte stage = 0; 
			if (progress > 0.5f) {if (progress == 1) stage = 3; else stage = 2;}
			else { if (progress > 0.25f) stage = 1;}
			switch (stage) {
				case 0: 						
					myBlock.surfaceRenderer.material = ResourceType.Dirt.material;
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
		if (lifepower != 0 && lifeTimer == 0 ) lifeTimer = GameMaster.LIFEPOWER_TICK;
	}

	public void SetBlock(SurfaceBlock b) {myBlock = b;}

	public void Annihilation() {
		for (int i =0; i< plants.Count; i++) {
			if (plants[i] != null) plants[i].Annihilate( false );
			else plants.RemoveAt(i);
		}
		myBlock.myChunk.AddLifePower((int)lifepower);
		myBlock.surfaceRenderer.material = ResourceType.Dirt.material;
		Destroy(this);
	}
}
