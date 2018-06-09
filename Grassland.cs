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

	public override bool Equals(object obj) 
	{
		// Check for null values and compare run-time types.
		if (obj == null || GetType() != obj.GetType()) 
			return false;

		PixelPosByte p = (PixelPosByte)obj;
		return (x == p.x) && (y == p.y) && (exists == p.exists);
	}

	public override int GetHashCode()
	{ 
		if (exists) return x * y;
		else return x* y * (-1);
	}
}

public class Grassland : MonoBehaviour {
	public const int MAX_LIFEFORMS_COUNT= 8;
	public const float LIFEPOWER_TO_PREPARE = 16, LIFE_CREATION_TIMER = 15;

	public SurfaceBlock myBlock {get;private set;}
	float progress = 0, lifeTimer = 0;
	public float lifepower;
	byte prevStage = 0;

	void Awake() {
		lifepower = 0; 
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

				if (lifepower != 0) {
					bool noActivity = true;
						List<Plant> plants = new List<Plant>();
						foreach (Structure s in myBlock.surfaceObjects) {
						if (s != null && s.type == StructureType.Plant && s.gameObject.activeSelf) plants.Add(s as Plant);
						}
						if (lifepower > 2 * LIFEPOWER_TO_PREPARE) {
							if (stage > 2) {
								float lifepowerToSinglePlant = GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient;
								if (plants.Count > 0) {
									float lifepowerToShare = plants.Count * lifepowerToSinglePlant;
									if (lifepowerToShare > lifepower - 2 * LIFEPOWER_TO_PREPARE) {
										lifepowerToShare = lifepower - 2 * LIFEPOWER_TO_PREPARE;
										lifepowerToSinglePlant = lifepowerToShare / plants.Count;
									}
									for (int i = 0; i < plants.Count; i++) {	
										if ( !plants[i].full ) 	plants[i].AddLifepower( TakeLifepower(lifepowerToSinglePlant));
										if (lifepower <= 0) break;
									}
								}
								if ( lifepower > 1 && plants.Count < MAX_LIFEFORMS_COUNT) {
									PixelPosByte pos = myBlock.GetRandomCell();
									if (pos != PixelPosByte.Empty) {
										Plant p = PoolMaster.current.GetSapling();
										p.gameObject.SetActive(true);
										p.SetBasement(myBlock, pos);
										p.AddLifepower(lifepowerToSinglePlant);
										lifepower --;	
										noActivity = false;
									}
								}
							}
						if ( noActivity ) myBlock.myChunk.AddLifePower( TakeLifepower( GameMaster.MAX_LIFEPOWER_TRANSFER ) );
							}
							else { // lifepower falls down
								if (lifepower < 0 && plants.Count > 0) {
									float lifepowerNeeded = lifepower * -2;
									float lifepowerFromSinglePlant = lifepowerNeeded / plants.Count;
									for (int i = 0; i < plants.Count; i++) {
										if (plants[i].lifepower <= 0) continue;
										lifepower += plants[i].TakeLifepower( lifepowerFromSinglePlant );
										if (lifepower >= 0) break;
									}
								}
							}
					lifeTimer = LIFE_CREATION_TIMER;
				}
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
			if (freeEnergy < MAX_LIFEFORMS_COUNT)  plants_count = (int)(MAX_LIFEFORMS_COUNT - freeEnergy); 
			List<PixelPosByte> positions = myBlock.GetRandomCells(plants_count);
			float[] lifepowers = new float[plants_count];
			float total = 0;
			for (int i =0; i< plants_count; i++) {
				lifepowers[i] = Random.value;
				total += lifepowers[i];
			}
			float lifePiece = count; lifePiece /= total * 2;

			for (int i =0; i< plants_count; i++) {
				float energy = lifepowers[i] * lifePiece;
				if (energy >  TreeSapling.MAXIMUM_LIFEPOWER) {
					Tree t = PoolMaster.current.GetTree();
					t.gameObject.SetActive(true);
					int pos = (int) (Random.value * (positions.Count - 1));
					t.SetBasement(myBlock, positions[pos]);
					t.SetLifepower(energy);
					positions.RemoveAt(pos);
				}
				else {
					TreeSapling pl = PoolMaster.current.GetSapling();
					pl.gameObject.SetActive(true);
					int pos = (int) (Random.value * (positions.Count - 1));
					pl.SetBasement(myBlock, positions[pos]);
					pl.SetLifepower(energy);
					positions.RemoveAt(pos);
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
		myBlock.myChunk.AddLifePower((int)lifepower);
		myBlock.surfaceRenderer.material = ResourceType.Dirt.material;
		Destroy(this);
	}
}
