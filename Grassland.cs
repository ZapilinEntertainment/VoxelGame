using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PixelPosByte {
	public byte x, y;
	public bool exists;
	public static readonly PixelPosByte Empty, zero, one;
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

[System.Serializable]
public class GrasslandSerializer {
	public float progress = 0, lifeTimer = 0, growTimer = 0, lifepower;
	public int prevStage = 0;
}

public class Grassland : MonoBehaviour {
	public const float LIFEPOWER_TO_PREPARE = 16, LIFE_CREATION_TIMER = 22;

	public SurfaceBlock myBlock {get;private set;}
	float progress = 0, lifeTimer = 0, growTimer = 0;
	public float lifepower;
	int prevStage = 0;

	void Awake() {
		lifepower = 0; 
	}

	void Update() {
		if (growTimer > 0) growTimer -=Time.deltaTime * GameMaster.gameSpeed;
		if (lifeTimer > 0) {
			lifeTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (lifeTimer <=0) {				
				List<Plant> plants = new List<Plant>();
				foreach (Structure s in myBlock.surfaceObjects) {
					if (s != null && s.id == Structure.PLANT_ID && s.gameObject.activeSelf) plants.Add(s as Plant);
				}
				Chunk c = myBlock.myChunk;
				if (lifepower > 2 * LIFEPOWER_TO_PREPARE) {
					int stage = CheckGrasslandStage();
					if (stage > 2) {						
						int i = 0;
						while (i < plants.Count & lifepower > 2 * LIFEPOWER_TO_PREPARE) {
							Plant p = plants[i];
							if (p.lifepower < p.lifepowerToGrow) p.AddLifepower(TakeLifepower(p.maxLifeTransfer));
							i++;
						}
						if (lifepower > 2 * LIFEPOWER_TO_PREPARE & myBlock.cellsStatus != 1) {
							PixelPosByte pos = myBlock.GetRandomCell();
							if (pos != PixelPosByte.Empty) {
								Plant p = Plant.GetNewPlant(Plant.TREE_OAK_ID);
								p.SetBasement(myBlock, pos);
								TakeLifepower(Plant.GetCreateCost(Plant.TREE_OAK_ID));
							}
						}
					}
				}
				else { // lifepower falls down
					if (lifepower < LIFEPOWER_TO_PREPARE & plants.Count > 0) {
					float lifepowerNeeded = Mathf.Abs(lifepower) + LIFEPOWER_TO_PREPARE + 2;
					int lifepowerFromSinglePlant = Mathf.RoundToInt(lifepowerNeeded / (float)plants.Count);
					while (lifepower <= LIFEPOWER_TO_PREPARE & plants.Count > 0) {
							int i = (int)(Random.value * (plants.Count - 1));
							lifepower += plants[i].TakeLifepower(lifepowerFromSinglePlant);
							plants.RemoveAt(i);
						}
					}
					CheckGrasslandStage();
				}
				lifeTimer = GameMaster.LIFEPOWER_TICK;
			}
		}
	}

	int CheckGrasslandStage() {
		progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
        int stage = Mathf.RoundToInt(progress / 0.2f);
		if (Mathf.Abs(stage - prevStage) > 1) {
			if (stage > prevStage) stage = prevStage+1;
			else stage =  prevStage - 1;
		}
		if (stage != prevStage) {
            SetGrassTexture(stage);
			prevStage = stage;
		}
		return stage;
	}

	public void AddLifepower(int count) {
		lifepower += count;
		if (lifeTimer == 0 ) lifeTimer = GameMaster.LIFEPOWER_TICK;
	}
	public int TakeLifepower(float count) {
		if (count < 0) return 0 ;
		if (lifepower < - 25) count = 0;
		else 	lifepower -= count;
		if (lifeTimer == 0 ) lifeTimer = GameMaster.LIFEPOWER_TICK;
		int lifeTransfer = (int) count;
		return lifeTransfer;
	}
	public void SetLifepower(float count) {
		lifepower = count;
		progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
		byte stage = 0; 
		if (progress > 0.5f) {if (progress == 1) stage = 3; else stage = 2;}
		else { if (progress > 0.25f) stage = 1;}
		if (stage != prevStage) {
            SetGrassTexture(stage);
			prevStage = stage;
		}
	}
    void SetGrassTexture(int stage)
    {
        switch (stage)
        {
            case 0:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.Dirt, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), true);
                break;
            case 1:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass20, myBlock.surfaceRenderer.GetComponent<MeshFilter>());
                break;
            case 2:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass40, myBlock.surfaceRenderer.GetComponent<MeshFilter>());
                break;
            case 3:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass60, myBlock.surfaceRenderer.GetComponent<MeshFilter>());
                break;
            case 4:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass80, myBlock.surfaceRenderer.GetComponent<MeshFilter>());
                break;
            case 5:
                myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetGreenMaterial(GreenMaterial.Grass100, myBlock.surfaceRenderer.GetComponent<MeshFilter>());
                break;
        }
    }

	/// <summary>
	/// Use this only on pre-gen
	/// </summary>
	public void AddLifepowerAndCalculate(int count) {
		if (count > 2 * LIFEPOWER_TO_PREPARE) {
			lifepower = 2 * LIFEPOWER_TO_PREPARE;
			float freeEnergy = count - lifepower; 
			int treesCount = (int)(Random.value * 10 + 4);
			int i = 0;
			List<PixelPosByte> positions = myBlock.GetRandomCells(treesCount);
			if (treesCount > positions.Count) treesCount = positions.Count;
			int lifepowerDosis = (int)(freeEnergy / treesCount);
			if (treesCount != 0) {
				while ( i < treesCount & freeEnergy > 0 & myBlock.cellsStatus != 1 ) {
					int plantID = Plant.TREE_OAK_ID;
					int ld  = (int)(lifepowerDosis * (0.3f + Random.value));
					if (ld > freeEnergy) {lifepower+=freeEnergy; break;}
					float maxEnergy = OakTree.GetLifepowerLevelForStage(OakTree.maxStage);
					byte getStage = (byte)(ld / maxEnergy * OakTree.maxStage);
					if (getStage > OakTree.maxStage) getStage = OakTree.maxStage;
					if (getStage == OakTree.maxStage & Random.value > 0.7f) getStage--;

					Plant p = Plant.GetNewPlant(plantID);
					p.SetBasement(myBlock, positions[i]);
					p.AddLifepower(ld);
					p.SetStage(getStage);
					freeEnergy -= (Plant.GetCreateCost(plantID) + ld);
					i++;
				}
			}
			lifepower = freeEnergy + 2 * LIFEPOWER_TO_PREPARE;
		}
		else 	lifepower = count;

			progress = Mathf.Clamp(lifepower / LIFEPOWER_TO_PREPARE, 0, 1);
			byte stage = 0; 
			if (progress > 0.5f) {if (progress == 1) stage = 3; else stage = 2;}
			else { if (progress > 0.25f) stage = 1;}
        SetGrassTexture(stage);
			prevStage = stage;
		if (lifepower != 0 && lifeTimer == 0 ) lifeTimer = GameMaster.LIFEPOWER_TICK;
	}

	public void SetBlock(SurfaceBlock b) {myBlock = b;}

	public void Annihilation() {
		myBlock.myChunk.AddLifePower((int)lifepower);
		myBlock.surfaceRenderer.sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.Dirt, myBlock.surfaceRenderer.GetComponent<MeshFilter>(), true);
		Destroy(this);
	}

	public GrasslandSerializer Save() {
		GrasslandSerializer gs = new GrasslandSerializer();
		gs.progress = progress; 
		gs.lifeTimer = lifeTimer;
		gs.growTimer = growTimer = 0;
		gs.lifepower = lifepower;
		gs.prevStage = prevStage;
		return gs;
	}

	public void Load( GrasslandSerializer gs) {
        prevStage = gs.prevStage;
        SetLifepower(gs.lifepower);
		progress = gs.progress;
		lifeTimer = gs.lifeTimer;
		growTimer = gs.growTimer;
		
	}
}
