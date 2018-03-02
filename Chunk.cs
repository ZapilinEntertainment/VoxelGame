using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkPos {
	public int x, y, z;
	public ChunkPos (int xpos, int ypos, int zpos) {
		x = xpos; y = ypos; z= zpos;
	}
}

public class Chunk : MonoBehaviour {
	readonly Vector3 CENTER_POS = new Vector3(8,8,8);
	Block[,,] blocks;
	public byte prevBitmask = 63;
	int[,] surfaceBlocks;
	List<GameObject> structures;
	public int lifePower = 0;
	public const float LIFEPOWER_TICK = 0.3f; float lifepower_timer = 0;
	List<Block> dirt_for_grassland;
	List<Grassland> grassland_blocks;
	public const int MAX_LIFEPOWER_TRANSFER = 4;
	public const byte CHUNK_SIZE = 16;
	public static int energy_take_speed = 10;

	void Awake() {
		dirt_for_grassland = new List<Block>();
		grassland_blocks = new List<Grassland>();
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
	}

	void Start() {
		if (Camera.main != null) CullingUpdate(Camera.main.transform);
	}

	public void CameraUpdate(Transform t) {
		CullingUpdate(t);
	} 

	void Update() {
		if (lifepower_timer > 0) {
			lifepower_timer -= Time.deltaTime  * GameMaster.gameSpeed;
		if (lifepower_timer <= 0) {
				if (lifePower > 0) {
					if (Random.value > 0.5f || grassland_blocks.Count == 0)
					{ // creating new grassland
					if (dirt_for_grassland.Count != 0) {
							int spos = 0; 
							while (spos < dirt_for_grassland.Count) {
								if (dirt_for_grassland[spos].grassland != null || !dirt_for_grassland[spos].onSurface) dirt_for_grassland.RemoveAt(spos);
								else spos++;
							}

						Block b = null;
						while (b == null && dirt_for_grassland.Count > 0) {
							int pos = (int)(Random.value * (dirt_for_grassland.Count - 1));
							b = dirt_for_grassland[pos];
							if (b != null) {
								{
									int x = b.pos.x; int z = b.pos.z;
									List<ChunkPos> candidats = new List<ChunkPos>();
									bool rightSide = false, leftSide = false;
									if (x + 1 < CHUNK_SIZE) {candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1,z] ,z));rightSide = true;}
									if (x - 1 >= 0) {candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1,z], z));leftSide = true;}
									if (z + 1 < CHUNK_SIZE) {
										candidats.Add(new ChunkPos(x, surfaceBlocks[x, z+1], z+1));
										if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z+1], z+1));
										if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z+1], z+1));
									}
									if (z - 1 >= 0) {
										candidats.Add(new ChunkPos(x, surfaceBlocks[x, z-1], z-1));
										if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z-1], z-1));
										if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z-1], z-1));
									}
									foreach (ChunkPos p in candidats) {
										Block n = GetBlock(p.x, p.y, p.z);
										if (n == null) continue;
											if (n.f_id == Block.DIRT_ID && !dirt_for_grassland.Contains(n) &&n.grassland == null && Mathf.Abs(b.pos.y - p.y) < 2) dirt_for_grassland.Add(n);
									}
								}
									b.AddGrassland();
									int lifeTransfer = (int)(MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
									if (lifePower > lifeTransfer) {b.grassland.AddLifepower(lifeTransfer); lifePower -= lifeTransfer;}
									else {b.grassland.AddLifepower(lifePower); lifePower = 0;}
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
									if (lifePower < count)  count = lifePower;
									gl.AddLifepower(count);
									lifePower -= count;
								}
								else {grassland_blocks.RemoveAt(pos);continue;}
						}
					}
					}
					if (dirt_for_grassland.Count != 0 || grassland_blocks.Count != 0) lifepower_timer = LIFEPOWER_TICK;
				}
				else { // LifePower decreases
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

	public void GenerateNature (PixelPosByte lifeSourcePos, int lifeVolume) {
		byte px = lifeSourcePos.x, py = lifeSourcePos.y;
		float [,] lifepowers = new float[CHUNK_SIZE, CHUNK_SIZE];
		lifepowers[px, py] = 1;
		float power = 1;
		bool leftSide = false, rightSide = false, upSide = false, downSide = false;
		if (px > 0) {
			leftSide = true;
			for (int i = px - 1; i >= 0; i--) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[i + 1,py] - surfaceBlocks[i, py]);
				lifepowers[i,py] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[i, py] * 0.9f;
			}
		}
		power = 1;
		if (px < CHUNK_SIZE - 1) {
			rightSide = true;
			for (int i = px + 1; i < CHUNK_SIZE; i++) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[i - 1,py] - surfaceBlocks[i, py]);
				lifepowers[i,py] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[i, py] * 0.9f;
			}
		}
		power = 1;
		if (py > 0) {
			downSide = true;
			for (int i = py - 1; i >= 0; i--) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[px, i+1] - surfaceBlocks[px,i]);
				lifepowers[px,i] = power * (1 - (delta / 8f) * (delta / 8f));
				power = lifepowers[px, i] * 0.9f;
			}
		}
		power = 1;
		if (px < CHUNK_SIZE - 1) {
			upSide= true;
			for (int i = py + 1; i < CHUNK_SIZE; i++) {
				byte delta = (byte)Mathf.Abs(surfaceBlocks[px, i -1] - surfaceBlocks[px, i]);
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
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j+1] - surfaceBlocks[i,j]);
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
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j] - surfaceBlocks[i,j-1]);
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
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j] - surfaceBlocks[i,j-1]);
					lifepowers[i,j] = (power  * (1 - (delta / 8f) * (delta / 8f)) + lifepowers[i,j] ) / 2f;
					power = lifepowers[i,j] * 0.9f;
				}
			}
			if (downSide) {
				power = lifepowers [i,py];
				for (int j = py - 1; j >=0; j--) {
					byte delta = (byte)Mathf.Abs(surfaceBlocks[i,j] - surfaceBlocks[i,j+1]);
					lifepowers[i,j] = (power  * (1 - (delta / 8f) * (delta / 8f)) + lifepowers[i,j]) / 2f;
					power = lifepowers[i,j] * 0.9f;
				}
			}
		}

		float total = 0;
		List<Block> afl = new List<Block>();
		for (int i =0; i< CHUNK_SIZE; i++) {
			for (int j = 0; j< CHUNK_SIZE; j++) {
				Block b = blocks[i, surfaceBlocks[i,j],j];
				if (b == null) continue;
				if (b.f_id == Block.DIRT_ID || b.f_id == Block.GRASS_ID) { // Acceptable for life
					total += lifepowers[i,j];
					afl.Add(b);
				}
			}
		}
		float lifePiece = lifeVolume / total;
		foreach (Block b in afl) {
			Grassland gl = null;
			if (b.grassland == null) {gl = b.AddGrassland();}
			else gl = b.grassland;
			b.grassland.AddLifepowerAndCalculate((int)(lifepowers[b.pos.x, b.pos.z] * lifePiece));
			grassland_blocks.Add(b.grassland);
		}
	}

	void RecalculateDirtForGrassland() {
		if (surfaceBlocks == null) return;
		dirt_for_grassland = new List<Block>();
		List<ChunkPos> markedBlocks = new List<ChunkPos>();
		for (int x = 0; x < CHUNK_SIZE; x++) {
			for (int z = 0; z < CHUNK_SIZE; z++) {
				int y = surfaceBlocks[x,z];
				if ((blocks[x,y,z].f_id == Block.GRASS_ID || (blocks[x,y,z].f_id == Block.DIRT_ID) && blocks[x,y,z].grassland == null)) {					
					List<ChunkPos> candidats = new List<ChunkPos>();
					bool rightSide = false, leftSide = false;
					if (x + 1 < CHUNK_SIZE) {rightSide = true;candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1,z] ,z));	}
					if (x - 1 >= 0) {candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1,z], z));leftSide = true;}
					if (z + 1 < CHUNK_SIZE) {
						candidats.Add(new ChunkPos(x, surfaceBlocks[x, z+1], z+1));
						if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z+1], z+1));
						if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z+1], z+1));
					}
					if (z - 1 >= 0) {
						candidats.Add(new ChunkPos(x, surfaceBlocks[x, z-1], z-1));
						if (rightSide) candidats.Add(new ChunkPos(x+1, surfaceBlocks[x+1, z-1], z-1));
						if (leftSide) candidats.Add(new ChunkPos(x-1, surfaceBlocks[x-1, z-1], z-1));
					}
					foreach (ChunkPos p in candidats) {
							Block b = GetBlock(p.x, p.y, p.z);
						if (b == null) continue;
						if (b.f_id == Block.DIRT_ID && !markedBlocks.Contains(p) &&b.grassland == null && Mathf.Abs(y - p.y) < 2) {dirt_for_grassland.Add(b); markedBlocks.Add(p);}
					}
				}
			}
		}
	}

	void RecalculateSurface() {
		int size = blocks.GetLength(0);
		surfaceBlocks = new int[size,size];
		for (int x = 0; x < size; x++) {
			for (int z = 0; z < size; z++) {
				for (int y = size-1; y >=0; y--) {
					if (blocks[x,y,z] == null || blocks[x,y,z] == Block.BlockedByStructure) continue;
					else {
						surfaceBlocks[x,z] = y;		
						blocks[x,y,z].SetSurfaceStatus(true);
						break;
					}
				}
			}
		}
		RecalculateDirtForGrassland();
	}
		
	public bool AddStructure(GameObject g, int x, int y, int z) {
		if (g == null) return false;
		if (structures == null) structures = new List<GameObject>();
		structures.Add(g);
		Structure str = g.GetComponent<Structure>();
		if (str == null) return false;
		bool appliable = true;
		for (int i =0; i< str.height; i++) {
			if (blocks[x,y + i,z] != null)  {appliable = false;break;}
		}
		if (appliable == false) return false;
		for (int i =0; i< str.height; i++) {
			if (blocks[x, y+i,z] == null) 	blocks[x,y + i,z] = Block.BlockedByStructure;
			else ReplaceBlock(x, y+i, z, Block.BLOCKED_BY_STRUCTURE_ID);
		}
		return true;
	}

	public void ReplaceBlock(int x, int y, int z, int newId) {
		if (blocks[x,y,z] != null) {
			blocks[x,y,z].Replace(newId);
		}
		else {
			blocks[x,y,z] = new Block(newId);
			blocks[x,y,z].body.transform.parent = transform;
			blocks[x,y,z].SetPos(x,y,z);
		}
	}

	public byte GetVisibilityMask(int i, int j, int k) {
		byte vmask = 63;
		Block bx = GetBlock(i+1,j,k); if (bx != null && !bx.IsTransparent()) vmask &= 61;
		bx = GetBlock(i-1,j,k); if (bx != null && !bx.IsTransparent()) vmask &= 55;
		bx = GetBlock(i,j+1,k); if (bx != null && !bx.IsTransparent()) vmask &= 47;
		bx = GetBlock(i,j - 1,k); if (bx != null && !bx.IsTransparent()) vmask &= 31;
		bx = GetBlock(i,j,k+1); if (bx != null && !bx.IsTransparent()) vmask &= 62;
		bx = GetBlock(i,j,k-1); if (bx != null && !bx.IsTransparent()) vmask &= 59;
		return vmask;
	}
	public void ChangeBlockVisibilityOnReplacement (int x, int y, int z, bool value) {
		Block b = GetBlock(x+1,y,z); if (b != null) {b.ChangeVisibilityMask(3,value);}
		b = GetBlock(x-1,y,z); if (b != null) {b.ChangeVisibilityMask(1,value);}
		b = GetBlock(x,y + 1,z); if (b != null) {b.ChangeVisibilityMask(5,value);}
		b = GetBlock(x,y-1,z); if (b != null) {b.ChangeVisibilityMask(4,value);}
		b = GetBlock(x,y,z+1); if (b != null) {b.ChangeVisibilityMask(2,value);}
		b = GetBlock(x,y,z-1); if (b != null) {b.ChangeVisibilityMask(0,value);}
	}

	public void SetChunk(int[,,] newData) {
		if (blocks != null) ClearChunk();
		int size = newData.GetLength(0);

		blocks = new Block[size,size,size];
		surfaceBlocks = new int[size, size];

		for (int x = 0; x< size; x++) {
			for (int z =0; z< size; z++) {
				for (int y = size - 1; y >= 0; y--) {
					if (newData[x,y,z] != 0) {
						blocks[x,y,z] = new Block(newData[x,y,z]);
						blocks[x,y,z].body.transform.parent = transform;
						blocks[x,y,z].SetPos(x,y,z);
						blocks[x,y,z].SetChunk(this);
					}
				}
			}
		}
		RecalculateSurface();
		for (int i = 0; i< size; i++) {
			for (int j =0; j< size; j++) {
				for (int k = 0; k< size; k++) {
					if (blocks[i,j,k] == null) continue;
					blocks[i,j,k].SetVisibilityMask(GetVisibilityMask(i,j,k));
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
					blocks[i,j,k].Destroy();
					blocks[i,j,k] = null;
				}
			}
		}
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
					if (b!=null && b.IsVisible()) b.SetRenderBitmask(renderBitmask);
				}
					prevBitmask = renderBitmask;
				}
		}
		else {
			//camera in chunk
			foreach (Block b in blocks) {
				if (b == null || !b.IsVisible()) continue;
				Vector3 icpos = campoint.InverseTransformPoint(b.body.transform.position);
				Vector3 vn = Vector3.one * (-1);
				if (icpos.x > 0) { if (icpos.x > size) vn.x = 1; else vn.x = 0;} 
				if (icpos.y > 0) {if (icpos.y > size) vn.y = 1; else vn.y = 0;}
				if (icpos.z > 0) {if (icpos.z > size) vn.z = 1; else vn.z = 0;}
				byte renderBitmask = 63;
				if (v.x ==1) renderBitmask &= 55; else if (v.x == -1) renderBitmask &= 61;
				if (v.y == 1) renderBitmask &= 31; else if (v.y == -1) renderBitmask &= 47;
				if (v.z == 1) renderBitmask &= 59; else if (v.z == -1) renderBitmask &= 62;
				b.SetRenderBitmask(renderBitmask);
			}
		}
	}

	public void AddLifePower (int count) {lifePower += count; if (lifepower_timer == 0) lifepower_timer = LIFEPOWER_TICK;}
	public int TakeLifePower (int count) {
		if (count < 0) return 0;
		int lifeTransfer = count;
		if (lifeTransfer > lifePower) {if (lifePower >= 0) lifeTransfer = lifePower; else lifeTransfer = 0;}
		lifePower -= lifeTransfer;
		if (lifepower_timer == 0) lifepower_timer = LIFEPOWER_TICK;
		return lifeTransfer;
	}
	public int[,] GetSurface() {return surfaceBlocks;}

	public void SpreadBlocks (int xpos, int zpos, int newId) {
		int size = blocks.GetLength(0);
		bool haveRightLine = false, haveLeftLine =false;
		if (zpos+1 < size) {
			if (Mathf.Abs(surfaceBlocks[xpos, zpos+1] - surfaceBlocks[xpos,zpos]) < 2 ) 
			{
				ReplaceBlock(xpos, surfaceBlocks[xpos,zpos+1], zpos+1, newId);
				if (zpos + 2 < size) {
					if (Mathf.Abs(surfaceBlocks[xpos, zpos+1] - surfaceBlocks[xpos,zpos+ 2]) < 2) 
						ReplaceBlock(xpos, surfaceBlocks[xpos,zpos+2], zpos + 2, newId);
				}
			}
			if (xpos+1 < size) {haveRightLine = true; if (Mathf.Abs(surfaceBlocks[xpos+1, zpos+1] - surfaceBlocks[xpos,zpos]) < 2 )  ReplaceBlock(xpos+1, surfaceBlocks[xpos+1,zpos+1], zpos + 1, newId);}
			if (xpos -1 > 0) {haveLeftLine = true; if (Mathf.Abs(surfaceBlocks[xpos-1, zpos+1] - surfaceBlocks[xpos,zpos]) < 2 )  ReplaceBlock(xpos-1, surfaceBlocks[xpos-1,zpos+1], zpos + 1, newId);}
		}
		if (haveRightLine) {
			if (Mathf.Abs(surfaceBlocks[xpos+1, zpos] - surfaceBlocks[xpos,zpos]) < 2 ) 
			{
				ReplaceBlock(xpos+1, surfaceBlocks[xpos+1,zpos], zpos, newId);
				if (xpos + 2 < size) {
					if (Mathf.Abs(surfaceBlocks[xpos + 2, zpos] - surfaceBlocks[xpos + 2,zpos]) < 2) 
						ReplaceBlock(xpos + 2, surfaceBlocks[xpos + 2,zpos], zpos, newId);
				}
			}
		}
		if (haveLeftLine) {
			if (Mathf.Abs(surfaceBlocks[xpos - 1, zpos] - surfaceBlocks[xpos,zpos]) < 2 ) 
			{
				ReplaceBlock(xpos - 1, surfaceBlocks[xpos - 1,zpos], zpos, newId);
				if (xpos - 2 > 0) {
					if (Mathf.Abs(surfaceBlocks[xpos - 2, zpos] - surfaceBlocks[xpos - 2,zpos]) < 2 ) 
						ReplaceBlock(xpos - 2, surfaceBlocks[xpos - 2, zpos], zpos, newId);
				}
			}
		}
		if (zpos - 1 > 0) {
			if (Mathf.Abs(surfaceBlocks[xpos, zpos-1] - surfaceBlocks[xpos,zpos]) < 2 ) 
			{
				ReplaceBlock(xpos, surfaceBlocks[xpos,zpos-1], zpos - 1,newId);
				if (zpos - 2 > 0) {
					if (Mathf.Abs(surfaceBlocks[xpos, zpos-1] - surfaceBlocks[xpos, zpos- 2]) < 2 ) 
						ReplaceBlock(xpos, surfaceBlocks[xpos, zpos-2], zpos - 2, newId);
				}
			}
			if (haveRightLine) {if (Mathf.Abs(surfaceBlocks[xpos+1, zpos-1] - surfaceBlocks[xpos,zpos]) < 2 )  ReplaceBlock(xpos+1, surfaceBlocks[xpos+1, zpos-1], zpos - 1,newId);}
			if (haveLeftLine) {if (Mathf.Abs(surfaceBlocks[xpos-1, zpos-1] - surfaceBlocks[xpos,zpos]) < 2 )  ReplaceBlock(xpos-1, surfaceBlocks[xpos-1,xpos-1], zpos - 1, newId);}
		}
		RecalculateSurface();
	}


}
