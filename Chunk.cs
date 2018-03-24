using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkPos {
	public byte x, y, z;
	public ChunkPos (byte xpos, byte ypos, byte zpos) {
		x = xpos; y = ypos; z= zpos;
	}
	public ChunkPos (int xpos,int ypos, int zpos) {
		if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0; if (zpos < 0) zpos = 0;
		x = (byte)xpos; y = (byte)ypos; z= (byte)zpos;
	}
}

public class Chunk : MonoBehaviour {
	readonly Vector3 CENTER_POS = new Vector3(8,8,8);
	Block[,,] blocks;
	public byte prevBitmask = 63;
	SurfaceBlock[,] surfaceBlocks;
	List<GameObject> structures;
	public float lifePower = 0, lifeSurplus = 0;
	public const float LIFEPOWER_TICK = 0.3f; float lifepower_timer = 0;
	List<SurfaceBlock> dirt_for_grassland;
	List<Grassland> grassland_blocks;
	public const int MAX_LIFEPOWER_TRANSFER = 16;
	public const byte CHUNK_SIZE = 16;
	public static int energy_take_speed = 10;
	public ChunkPos accessPoint{get;private set;}
	public bool[,,] accessibility {get;private set;}
	List<GameObject> borderLines; int linesIndex = 0; bool bordersEnabled = false; Transform borderlinesParent;

	void Awake() {
		dirt_for_grassland = new List<SurfaceBlock>();
		grassland_blocks = new List<Grassland>();

		borderLines = new List<GameObject>();
		borderLines.Add(Instantiate(Resources.Load<GameObject>("Prefs/BorderLine")));
		borderLines[0].SetActive(false);
		GameObject g = new GameObject("borderlinesParent"); g.transform.parent = transform; g.transform.localPosition = Vector3.zero;
		borderlinesParent = g.transform;

		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}

	void Start() {
		if (Camera.main != null) CullingUpdate(Camera.main.transform);
	}

	public void CameraUpdate(Transform t) {
		CullingUpdate(t);
	} 

	void Update() {
		lifePower += lifeSurplus * Time.deltaTime * GameMaster.gameSpeed;
		if (lifepower_timer > 0) {
			lifepower_timer -= Time.deltaTime  * GameMaster.gameSpeed;
		if (lifepower_timer <= 0) {
				if (lifePower > 0) {
					if (Random.value > 0.5f || grassland_blocks.Count == 0)
					{ // creating new grassland
					if (dirt_for_grassland.Count != 0) {
							int spos = 0; 
							while (spos < dirt_for_grassland.Count) {
								if (dirt_for_grassland[spos].grassland != null) dirt_for_grassland.RemoveAt(spos);
								else spos++;
							}

						SurfaceBlock b = null;
						while (b == null && dirt_for_grassland.Count > 0) {
							int pos = (int)(Random.value * (dirt_for_grassland.Count - 1));
							b = dirt_for_grassland[pos];
							if (b != null) {
								{
									int x = b.pos.x; int z = b.pos.z;
									List<ChunkPos> candidats = new List<ChunkPos>();
									bool rightSide = false, leftSide = false;
										if (x + 1 < CHUNK_SIZE) {candidats.Add(new ChunkPos(x + 1, surfaceBlocks[x,z].pos.y ,z)); rightSide = true;}
										if (x - 1 >= 0) {candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1,z].pos.y, z));leftSide = true;}
									if (z + 1 < CHUNK_SIZE) {
											candidats.Add(new ChunkPos(x, surfaceBlocks[x, z+1].pos.y, z+1));
											if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z+1].pos.y, z+1));
											if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z+1].pos.y, z+1));
									}
									if (z - 1 >= 0) {
											candidats.Add(new ChunkPos(x, surfaceBlocks[x, z-1].pos.y, z-1));
											if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z-1].pos.y, z-1));
											if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z-1].pos.y, z-1));
									}
									foreach (ChunkPos p in candidats) {
											SurfaceBlock n = GetBlock(p.x, p.y, p.z).GetComponent<SurfaceBlock>();
											if (n == null ) continue;
											if (n.material_id == PoolMaster.DIRT_ID && !dirt_for_grassland.Contains(n) &&n.grassland== null && Mathf.Abs(b.pos.y - p.y) < 2) dirt_for_grassland.Add(n);
									}
								}
									b.AddGrassland();
									int lifeTransfer = (int)(MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
									if (lifePower > lifeTransfer) {b.grassland.AddLifepower(lifeTransfer); lifePower -= lifeTransfer;}
									else {b.grassland.AddLifepower((int)lifePower); lifePower = 0;}
									grassland_blocks.Add(b.grassland);
							}
							dirt_for_grassland.RemoveAt(pos);
						}
					}
				}
					else {//adding energy to existing life tiles
						if (grassland_blocks.Count != 0) {
							Grassland gl = null;
							while (gl== null && grassland_blocks.Count >0) {
								int pos = (int)(Random.value * (grassland_blocks.Count - 1));
								gl = grassland_blocks[pos];
								if (gl != null) {
									int  count = (int)(Mathf.Pow(MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient, gl.level));
									if (lifePower < count)  count = (int)lifePower;
									gl.AddLifepower(count);
									lifePower -= count;
								}
								else {grassland_blocks.RemoveAt(pos);continue;}
						}
					}
					}
					if (dirt_for_grassland.Count != 0 || grassland_blocks.Count != 0) lifepower_timer = LIFEPOWER_TICK;
				}
				if (lifePower < -100) { // LifePower decreases
					//print (grassland_blocks.Count);
					if (grassland_blocks.Count == 0) lifepower_timer = 0;
					else {
						Grassland gl = null;
						int pos = 0;
						while (pos < grassland_blocks.Count && lifePower <= 0) {
							gl = grassland_blocks[pos];
							if (gl != null) {
								if (gl.lifepower <= 0 ) gl.Annihilation();
								else lifePower += gl.TakeLifepower(energy_take_speed * gl.level);
								pos++;
							}
							else {
								grassland_blocks.RemoveAt(pos); 
							}
						}
					}
				}
				lifepower_timer = LIFEPOWER_TICK;
		}
		}
	}

	public void AddBlock (ChunkPos f_pos, BlockType f_type, int f_material_id) {
		int x = f_pos.x, y = f_pos.y, z = f_pos.z;
		if (blocks[x,y,z] != null) ReplaceBlock(f_pos, f_type, f_material_id, false);
		GameObject g = new GameObject();
		switch (f_type) {
		case BlockType.Cube:
			CubeBlock cb = g.AddComponent<CubeBlock>();
			cb.BlockSet(this, f_pos, f_material_id, false);
			blocks[x,y,z] = cb;
			cb.SetVisibilityMask(GetVisibilityMask(f_pos.x, f_pos.y, f_pos.z));
			cb.SetRenderBitmask(prevBitmask);
			if (cb.isTransparent == false) {
				cb = GetBlock(x+1,y,z).GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(3,false);}
				cb = GetBlock(x-1,y,z).GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(1,false);}
				cb = GetBlock(x,y + 1,z).GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(5,false);}
				cb = GetBlock(x,y-1,z).GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(4,false);}
				cb = GetBlock(x,y,z+1).GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(2,false);}
				cb = GetBlock(x,y,z-1).GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(0,false);}
			}
			break;
		case BlockType.Shapeless:
			blocks[x,y,z] = g.AddComponent<Block>();
			blocks[x,y,z].BlockSet(this, f_pos, f_material_id);
			break;
		case BlockType.Surface:
			CubeBlock bas = blocks[x, y-1, z].GetComponent<CubeBlock>();
			if (bas == null) return;
			SurfaceBlock sb = g.AddComponent<SurfaceBlock>();
			sb.SurfaceBlockSet(this, f_pos, f_material_id, bas);
			blocks[x,y,z] = sb;
			if (sb.isTransparent == false) bas.ChangeVisibilityMask(4, false);
			bool clearSpace = true; f_pos.y++;
			while (f_pos.y < CHUNK_SIZE && clearSpace) {
				if (blocks[x,y,z] != null) {clearSpace = false; break;}
				else f_pos.y++;
			}
			if ( clearSpace ) surfaceBlocks[x,z] = sb;
			break;
		}
	}

	public void ReplaceBlock(ChunkPos f_pos, BlockType f_newType, int f_newMaterial_id, bool naturalGeneration) {
		int x = f_pos.x, y = f_pos.y, z= f_pos.z;
		Block originalBlock = GetBlock(x,y,z);
		if (originalBlock.type == f_newType) {originalBlock.ReplaceMaterial(f_newMaterial_id);return;}
		CubeBlock cb = null;
		switch (f_newType) {
		case BlockType.Shapeless:
			Block b = new GameObject().AddComponent<Block>();
			b.ShapelessBlockSet(this, f_pos, null);
			blocks[x,y,z] = b;

			if (originalBlock.type != BlockType.Shapeless) {
				if (GetBlock(x+1,y,z) != null) {cb = blocks[x+1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(3,true);}}
				if (GetBlock(x-1,y,z) != null) {cb = blocks[x-1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(1,true);}}
				if (GetBlock(x,y,z+1) != null) {cb = blocks[x,y,z+1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(2,true);}}
				if (GetBlock(x,y,z-1) != null) {cb = blocks[x,y,z-1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(0,true);}}
				if (GetBlock(x,y + 1,z) != null) {cb = blocks[x,y + 1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(5,true);}}
			}
			if (GetBlock(x,y-1,z) != null) {cb = blocks[x,y-1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(4,true);}}
			break;
		case BlockType.Surface:
			cb = blocks[x,y-1,z].gameObject.GetComponent<CubeBlock>();
			if (cb == null) return; // замена не удалась
			SurfaceBlock sb= new GameObject().AddComponent<SurfaceBlock>();
			sb.SurfaceBlockSet(this, f_pos, f_newMaterial_id, cb);
			blocks[x,y,z] = sb;
			if (originalBlock.type != BlockType.Shapeless) {
				if (GetBlock(x+1,y,z) != null) {cb = blocks[x+1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(3,true);}}
				if (GetBlock(x-1,y,z) != null) {cb = blocks[x-1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(1,true);}}
				if (GetBlock(x,y,z+1) != null) {cb = blocks[x,y,z+1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(2,true);}}
				if (GetBlock(x,y,z-1) != null) {cb = blocks[x,y,z-1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(0,true);}}
				if (GetBlock(x,y + 1,z) != null) {cb = blocks[x,y + 1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(5,true);}}
			}
			if (GetBlock(x,y-1,z) != null) {cb = blocks[x,y-1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(4,false);}}
			break;
		case BlockType.Cube:
			cb = new GameObject().AddComponent<CubeBlock>();
			cb.BlockSet(this, f_pos,f_newMaterial_id, naturalGeneration);
			blocks[x,y,z] = cb;
			if (GetBlock(x+1,y,z) != null) {cb = blocks[x+1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(3,false);}}
			if (GetBlock(x-1,y,z) != null) {cb = blocks[x-1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(1,false);}}
			if (GetBlock(x,y,z+1) != null) {cb = blocks[x,y,z+1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(2,false);}}
			if (GetBlock(x,y,z-1) != null) {cb = blocks[x,y,z-1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(0,false);}}
			if (GetBlock(x,y + 1,z) != null) {cb = blocks[x,y + 1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(5,false);}}
			if (originalBlock.type != BlockType.Surface) {if (GetBlock(x,y-1,z) != null) {cb = blocks[x,y-1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(4,false);}}}
			break;
		}
		Destroy(originalBlock.gameObject);
	}

	public void DeleteBlock(ChunkPos pos) {
		// в сиквеле стоит пересмотреть всю иерархию классов ><
		Block b = blocks[pos.x, pos.y,pos.z];
		if (b.indestructible == true) return;
		int x = pos.x, y = pos.y, z = pos.z;
		switch (b.type) {
		case BlockType.Cube : 
			CubeBlock cb = b.GetComponent<CubeBlock>();
			if (y < CHUNK_SIZE - 1) {
				if (blocks[x, y + 1, z] != null) {
					if (blocks[x, y + 1, z].type == BlockType.Surface) DeleteBlock(new ChunkPos(x, y + 1, z));
				}
			}
			Block b2 =  GetBlock(x,y-1,z);
			if (b2  == null || b2.type != BlockType.Cube ) {
				Destroy(cb.gameObject);
				blocks[pos.x, pos.y, pos.z] = null;
			}
			else {
				ReplaceBlock(pos, BlockType.Surface, b.material_id, false);
			}
			if (b.isTransparent == false) {
				if (GetBlock(x+1,y,z) != null) {cb = blocks[x+1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(3,true);}}
				if (GetBlock(x-1,y,z) != null) {cb = blocks[x-1,y,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(1,true);}}
				if (GetBlock(x,y + 1,z) != null) {cb = blocks[x,y + 1,z].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(5,true);}}
				if (GetBlock(x,y,z+1) != null) {cb = blocks[x,y,z+1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(2,true);}}
				if (GetBlock(x,y,z-1) != null) {cb = blocks[x,y,z-1].GetComponent<CubeBlock>(); if (cb != null) {cb.ChangeVisibilityMask(0,true);}}
			}
			break;
		case BlockType.Shapeless: 
			Destroy(blocks[pos.x,pos.y,pos.z].gameObject);
			blocks[pos.x, pos.y, pos.z] = null;
			break;
		case BlockType.Surface: 
			Destroy(blocks[pos.x,pos.y,pos.z].gameObject);
			blocks[pos.x, pos.y, pos.z] = null;
			CubeBlock cb2 = GetBlock(x,y-1,z).GetComponent<CubeBlock>(); if (cb2 != null) {cb2.ChangeVisibilityMask(4,true);}
			break;
		}
	}

	public void GenerateNature (PixelPosByte lifeSourcePos, int lifeVolume) {
		byte px = lifeSourcePos.x, py = lifeSourcePos.y;
		float [,] lifepowers = new float[CHUNK_SIZE, CHUNK_SIZE];
		lifepowers[px, py] = 1;
		float power = 1;
		bool leftSide = false, rightSide = false, upSide = false, downSide = false;
		if (px > 0) {
			leftSide = true;
			for (int i = px - 1; i >= 0; i--) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[i + 1,py].pos.y - surfaceBlocks[i, py].pos.y);
				lifepowers[i,py] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[i, py] * 0.9f;
			}
		}
		power = 1;
		if (px < CHUNK_SIZE - 1) {
			rightSide = true;
			for (int i = px + 1; i < CHUNK_SIZE; i++) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[i - 1,py].pos.y - surfaceBlocks[i, py].pos.y);
				lifepowers[i,py] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[i, py] * 0.9f;
			}
		}
		power = 1;
		if (py > 0) {
			downSide = true;
			for (int i = py - 1; i >= 0; i--) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[px, i+1].pos.y - surfaceBlocks[px,i].pos.y);
				lifepowers[px,i] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[px, i] * 0.9f;
			}
		}
		power = 1;
		if (px < CHUNK_SIZE - 1) {
			upSide= true;
			for (int i = py + 1; i < CHUNK_SIZE; i++) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[px, i -1].pos.y - surfaceBlocks[px, i].pos.y);
				lifepowers[px, i] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[px, i] * 0.9f;
			}
		}

		// горизонтальная обработка
		if (leftSide) {
			for (int i = 0; i< CHUNK_SIZE; i++) {
				if (i == py) continue;
				power= lifepowers[i, px];
				for (int j = px - 1; j >= 0; j--) {
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j+1].pos.y - surfaceBlocks[i,j].pos.y);
					lifepowers[i,j] = power  * (1 - (delta / 8f) * (delta / 8f));
					power = lifepowers[i,j] * 0.9f;
				}
			}
		}
		if (rightSide) {
			for (int i = 0; i< CHUNK_SIZE; i++) {
				if (i == py) continue;
				power= lifepowers[i, px];
				for (int j = px +1; j < CHUNK_SIZE; j++) {
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j].pos.y - surfaceBlocks[i,j-1].pos.y);
					lifepowers[i,j] = power  * (1 - (delta / 8f) * (delta / 8f));
					power = lifepowers[i,j] * 0.9f;
				}
			}
		}
		// вертикальная обработка + усреднение
		for (int i = 0; i < CHUNK_SIZE; i++) {
			if (i == px) continue;
			if (upSide) {
				power = lifepowers [i,py];
				for (int j = py + 1; j < CHUNK_SIZE; j++) {
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j].pos.y - surfaceBlocks[i,j-1].pos.y);
					lifepowers[i,j] = (power  * (1 - (delta / 8f) * (delta / 8f)) + lifepowers[i,j] ) / 2f;
					power = lifepowers[i,j] * 0.9f;
				}
			}
			if (downSide) {
				power = lifepowers [i,py];
				for (int j = py - 1; j >=0; j--) {
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j].pos.y - surfaceBlocks[i,j+1].pos.y);
					lifepowers[i,j] = (power  * (1 - (delta / 8f) * (delta / 8f)) + lifepowers[i,j]) / 2f;
					power = lifepowers[i,j] * 0.9f;
				}
			}
		}

		float total = 0;
		List<SurfaceBlock> afl = new List<SurfaceBlock>();
		for (int i =0; i< CHUNK_SIZE; i++) {
			for (int j = 0; j< CHUNK_SIZE; j++) {
				SurfaceBlock b = surfaceBlocks[i,j];
				if (b == null) continue;
				if (b.material_id == PoolMaster.DIRT_ID || b.material_id == PoolMaster.GRASS_ID) { // Acceptable for life
					total += lifepowers[i,j];
					afl.Add(b);
				}
			}
		}
		float lifePiece = lifeVolume / total;
		foreach (SurfaceBlock b in afl) {
			Grassland gl = null;
			if (b.grassland == null) {gl = b.AddGrassland();}
			else gl = b.grassland;
			b.grassland.AddLifepowerAndCalculate((int)(lifepowers[b.pos.x, b.pos.z] * lifePiece));
			grassland_blocks.Add(b.grassland);
		}
	}

	void RecalculateDirtForGrassland() {
		if (surfaceBlocks == null) return;
		dirt_for_grassland = new List<SurfaceBlock>();
		List<ChunkPos> markedBlocks = new List<ChunkPos>();
		for (int x = 0; x < CHUNK_SIZE; x++) {
			for (int z = 0; z < CHUNK_SIZE; z++) {
				SurfaceBlock sb = surfaceBlocks[x,z];
				if (sb == null) continue;
				if ((sb.material_id == PoolMaster.DIRT_ID || (sb.material_id == PoolMaster.GRASS_ID) && sb.grassland == null)) {					
					List<ChunkPos> candidats = new List<ChunkPos>();
					bool rightSide = false, leftSide = false;
					if (x + 1 < CHUNK_SIZE) {rightSide = true;candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1,z].pos.y ,z));	}
					if (x - 1 >= 0) {candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1,z].pos.y, z));leftSide = true;}
					if (z + 1 < CHUNK_SIZE) {
						candidats.Add(new ChunkPos(x, surfaceBlocks[x, z+1].pos.y, z+1));
						if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z+1].pos.y, z+1));
						if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z+1].pos.y, z+1));
					}
					if (z - 1 >= 0) {
						candidats.Add(new ChunkPos(x, surfaceBlocks[x, z-1].pos.y, z-1));
						if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z-1].pos.y, z-1));
						if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z-1].pos.y, z-1));
					}
					foreach (ChunkPos p in candidats) {
						SurfaceBlock b = GetBlock(p.x, p.y, p.z).GetComponent<SurfaceBlock>();
						if (b == null) continue;
						if (b.material_id == PoolMaster.DIRT_ID && !markedBlocks.Contains(p) &&b.grassland == null && Mathf.Abs(sb.pos.y - p.y) < 2) {dirt_for_grassland.Add(b); markedBlocks.Add(p);}
					}
				}
			}
		}
	}

	void RecalculateSurface() {
		int size = blocks.GetLength(0);
		surfaceBlocks = new SurfaceBlock[size,size];
		for (int x = 0; x < size; x++) {
			for (int z = 0; z < size; z++) {
				for (int y = size-1; y >=0; y--) {
					SurfaceBlock sb = blocks[x,y,z].gameObject.GetComponent<SurfaceBlock>();
					if (sb != null) {surfaceBlocks[x,z] = sb;break;}
				}
			}
		}
		RecalculateDirtForGrassland();
	}


	public byte GetVisibilityMask(int i, int j, int k) {
		byte vmask = 63;
		Block bx = GetBlock(i+1,j,k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 61;
		bx = GetBlock(i-1,j,k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 55;
		bx = GetBlock(i,j+1,k); if (bx != null &&!bx.isTransparent && bx.type != BlockType.Shapeless) vmask &= 47;
		bx = GetBlock(i,j - 1,k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 31;
		bx = GetBlock(i,j,k+1); if (bx != null &&!bx.isTransparent && bx.type == BlockType.Cube) vmask &= 62;
		bx = GetBlock(i,j,k-1); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube) vmask &= 59;
		return vmask;
	}

	public void SetChunk(int[,,] newData) {
		if (blocks != null) ClearChunk();
		int size = newData.GetLength(0);

		blocks = new Block[size,size,size];
		surfaceBlocks = new SurfaceBlock[size, size];

		for (int x = 0; x< size; x++) {
			for (int z =0; z< size; z++) {
				byte surfaceFound = 2;
				for (int y = size - 1; y >= 0; y--) {
					if (newData[x,y,z] != 0) {
						if ( surfaceFound == 2 ) {
							SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
							sb.SurfaceBlockSet(this, new ChunkPos(x,y,z), newData[x,y,z], null);
							surfaceBlocks[x,z] = sb;
							blocks[x,y,z] = sb;
							surfaceFound --;
						}
						else {
							CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
							cb.BlockSet(this, new ChunkPos(x,y,z), newData[x,y,z], true);
							blocks[x,y,z] = cb;
							if (surfaceFound == 1) {
								blocks[x,y+1,z].GetComponent<SurfaceBlock>().basement = cb;
								surfaceFound = 0; 
							}
						}
					}
				}
			}
		}
		for (int x = 0; x< size; x++) {
			for (int z =0; z< size; z++) {
				for (int y = 0; y< size; y++) {
					if (blocks[x,y,z] == null || blocks[x,y,z].type != BlockType.Cube) continue;
					blocks[x,y,z].GetComponent<CubeBlock>().SetVisibilityMask(GetVisibilityMask(x,y,z));
				}
				if (surfaceBlocks[x,z] != null) { // surface minerals generation
					GameMaster.geologyModule.SpreadMinerals(surfaceBlocks[x,z]);
				}
			}
		}
	}

	public Block GetBlock (int x, int y, int z) {
		int size = blocks.GetLength(0);
		if (x < 0 ||x > size - 1 || y < 0 || y > size - 1 || z < 0 || z > size - 1) return null;
		else {return blocks[x,y,z];}
	}

	public void ClearChunk() {
		int size = blocks.GetLength(0);
		for (int i = 0; i< size; i++) {
			for (int j =0; j< size; j++) {
				for (int k =0; k< size; k++) {
					if ( blocks[i,j,k] == null) continue;
					Destroy(blocks[i,j,k].gameObject);
				}
			}
		}
		blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
		dirt_for_grassland.Clear(); grassland_blocks.Clear();
	}
		

	void CullingUpdate(Transform campoint) {
		if (campoint == null) campoint = Camera.main.transform;
		byte camSector = 0;
		Vector3 cpos = transform.InverseTransformPoint(campoint.position);
		Vector3 v = Vector3.one * (-1);
		int size = blocks.GetLength(0);
		if (cpos.x > 0) { if (cpos.x > size) v.x = 1; else v.x = 0;} 
		if (cpos.y > 0) {if (cpos.y > size) v.y = 1; else v.y = 0;}
		if (cpos.z > 0) {if (cpos.z > size) v.z = 1; else v.z = 0;}
		//print (v);
		if (v != Vector3.zero) {
			Vector3 cdir = transform.InverseTransformDirection(campoint.forward);
			//easy-culling
			float av = Vector3.Angle(CENTER_POS - cpos, cdir);				
				byte renderBitmask = 63;
				if (v.x ==1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
				if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
				if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;
				if (renderBitmask != prevBitmask) {
					foreach(Block b in blocks) {
					if ( b !=null  && b.type == BlockType.Cube) {
						b.GetComponent<CubeBlock>().SetRenderBitmask(renderBitmask);
					}
				}
					prevBitmask = renderBitmask;
				}
		}
		else {
			//camera in chunk
			foreach (Block b in blocks) {
				if (b == null || b.type != BlockType.Cube) continue;
				CubeBlock cb = b.GetComponent<CubeBlock>();
				Vector3 icpos = campoint.InverseTransformPoint(b.transform.position);
				Vector3 vn = Vector3.one * (-1);
				if (icpos.x > 0) { if (icpos.x > size) vn.x = 1; else vn.x = 0;} 
				if (icpos.y > 0) {if (icpos.y > size) vn.y = 1; else vn.y = 0;}
				if (icpos.z > 0) {if (icpos.z > size) vn.z = 1; else vn.z = 0;}
				byte renderBitmask = 63;
				if (v.x ==1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
				if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
				if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;
				cb.SetRenderBitmask(renderBitmask);
			}
		}
	}

	public void AddLifePower (int count) {lifePower += count; if (lifepower_timer == 0) lifepower_timer = LIFEPOWER_TICK;}
	public int TakeLifePower (int count) {
		if (count < 0) return 0;
		float lifeTransfer = count;
		if (lifeTransfer > lifePower) {if (lifePower >= 0) lifeTransfer = lifePower; else lifeTransfer = 0;}
		lifePower -= lifeTransfer;
		if (lifepower_timer == 0) lifepower_timer = LIFEPOWER_TICK;
		return (int)lifeTransfer;
	}

	public SurfaceBlock[,] GetSurface() {return surfaceBlocks;}
	public SurfaceBlock GetSurfaceBlock(byte x, byte z) {
		if (x >= CHUNK_SIZE || z >= CHUNK_SIZE || surfaceBlocks == null) return null;
		return surfaceBlocks[x,z];
	}
	public SurfaceBlock GetSurfaceBlock(int x, int z) {
		if (x < 0 || z < 0 || x >= CHUNK_SIZE || z >= CHUNK_SIZE || surfaceBlocks == null) return null;
		return surfaceBlocks[x,z];
	}

	public void SpreadBlocks (int xpos, int zpos, int newId) {
		int size = blocks.GetLength(0);
		bool haveRightLine = false, haveLeftLine =false;
		if (zpos+1 < size) {
			if (Mathf.Abs(surfaceBlocks[xpos, zpos+1].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 ) 
			{
				blocks[xpos, surfaceBlocks[xpos,zpos+1].pos.y, zpos+1].ReplaceMaterial( newId);
				if (zpos + 2 < size) {
					if (Mathf.Abs(surfaceBlocks[xpos, zpos+1].pos.y - surfaceBlocks[xpos,zpos+ 2].pos.y) < 2) 
						blocks[xpos, surfaceBlocks[xpos,zpos+2].pos.y, zpos + 2].ReplaceMaterial(newId);
				}
			}
			if (xpos+1 < size) {haveRightLine = true; if (Mathf.Abs(surfaceBlocks[xpos+1, zpos+1].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 )  blocks[xpos+1, surfaceBlocks[xpos+1,zpos+1].pos.y, zpos + 1].ReplaceMaterial(newId);}
			if (xpos -1 > 0) {haveLeftLine = true; if (Mathf.Abs(surfaceBlocks[xpos-1, zpos+1].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 )  blocks[xpos-1, surfaceBlocks[xpos-1,zpos+1].pos.y, zpos + 1].ReplaceMaterial (newId);}
		}
		if (haveRightLine) {
			if (Mathf.Abs(surfaceBlocks[xpos+1, zpos].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 ) 
			{
				blocks[xpos+1, surfaceBlocks[xpos+1,zpos].pos.y, zpos].ReplaceMaterial (newId);
				if (xpos + 2 < size) {
					if (Mathf.Abs(surfaceBlocks[xpos + 2, zpos].pos.y - surfaceBlocks[xpos + 2,zpos].pos.y) < 2) 
						blocks[xpos + 2, surfaceBlocks[xpos + 2,zpos].pos.y, zpos].ReplaceMaterial(newId);
				}
			}
		}
		if (haveLeftLine) {
			if (Mathf.Abs(surfaceBlocks[xpos - 1, zpos].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 ) 
			{
				blocks[xpos - 1, surfaceBlocks[xpos - 1,zpos].pos.y, zpos].ReplaceMaterial(newId);
				if (xpos - 2 > 0) {
					if (Mathf.Abs(surfaceBlocks[xpos - 2, zpos].pos.y - surfaceBlocks[xpos - 2,zpos].pos.y) < 2 ) 
						blocks[xpos - 2, surfaceBlocks[xpos - 2, zpos].pos.y, zpos].ReplaceMaterial(newId);
				}
			}
		}
		if (zpos - 1 > 0) {
			if (Mathf.Abs(surfaceBlocks[xpos, zpos-1].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 ) 
			{
				blocks[xpos, surfaceBlocks[xpos,zpos-1].pos.y, zpos - 1].ReplaceMaterial(newId);
				if (zpos - 2 > 0) {
					if (Mathf.Abs(surfaceBlocks[xpos, zpos-1].pos.y - surfaceBlocks[xpos, zpos- 2].pos.y) < 2 ) 
						blocks[xpos, surfaceBlocks[xpos, zpos-2].pos.y, zpos - 2].ReplaceMaterial(newId);
				}
			}
			if (haveRightLine) {if (Mathf.Abs(surfaceBlocks[xpos+1, zpos-1].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 )  blocks[xpos+1, surfaceBlocks[xpos+1, zpos-1].pos.y, zpos - 1].ReplaceMaterial(newId);}
			if (haveLeftLine) {if (Mathf.Abs(surfaceBlocks[xpos-1, zpos-1].pos.y - surfaceBlocks[xpos,zpos].pos.y) < 2 )  blocks[xpos-1, surfaceBlocks[xpos-1,xpos-1].pos.y, zpos - 1].ReplaceMaterial(newId);}
		}
		RecalculateSurface();
	}

	public void BlockByStructure(byte x, byte y, byte z, Structure s) {
		if (x > CHUNK_SIZE || y > CHUNK_SIZE || z > CHUNK_SIZE || x < 0 || y < 0 || z < 0 || s == null) return;
		Block b = GetBlock(x,y,z);
		if (b != null) {Destroy(blocks[x,y,z].gameObject);}
		else blocks[x,y,z] = new GameObject().AddComponent<Block>();
		blocks[x,y,z].ShapelessBlockSet(this, new ChunkPos(x,y,z), s);
	}

	public void SetAccessPoint(ChunkPos pos) {
		accessPoint = pos;
	}


}
