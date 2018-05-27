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
	Vector3 CENTER_POS = new Vector3(8,8,8);
	Block[,,] blocks;
	List<SurfaceBlock> surfaceBlocks;
	public byte prevBitmask = 63;
	List<GameObject> structures;
	public float lifePower = 0;
	 float lifepower_timer = 0;
	List<Grassland> grassland_blocks;
	public static byte CHUNK_SIZE  {get;private set;}
	public static int energy_take_speed = 10;
	GameObject cave_pref;
	public List <Component> chunkUpdateSubscribers;

	void Awake() {
		CENTER_POS = new Vector3(CHUNK_SIZE/2f, CHUNK_SIZE/2f, CHUNK_SIZE/2f);
		grassland_blocks = new List<Grassland>();
		surfaceBlocks = new List<SurfaceBlock>();
		cave_pref = Resources.Load<GameObject>("Prefs/CaveBlock_pref");
		chunkUpdateSubscribers = new List<Component>();

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
					if ((Random.value > 0.5f || grassland_blocks.Count == 0) && surfaceBlocks.Count > 0)
					{ // creating new grassland
						List<SurfaceBlock> dirt_for_grassland = new List<SurfaceBlock>();
						foreach ( SurfaceBlock sb in surfaceBlocks) {
							if ( sb.material_id == ResourceType.DIRT_ID && sb.grassland == null) dirt_for_grassland.Add(sb);
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
									if (x + 1 < CHUNK_SIZE) {candidats.Add(new ChunkPos(x + 1, GetSurfaceBlock(x,z).pos.y ,z)); rightSide = true;}
									if (x - 1 >= 0) {candidats.Add(new ChunkPos(x-1, GetSurfaceBlock(x-1,z).pos.y, z));leftSide = true;}
									if (z + 1 < CHUNK_SIZE) {
										candidats.Add(new ChunkPos(x, GetSurfaceBlock(x,z+1).pos.y, z+1));
										if (rightSide) candidats.Add(new ChunkPos(x+1, GetSurfaceBlock(x+1,z+1).pos.y, z+1));
										if (leftSide) candidats.Add(new ChunkPos(x-1, GetSurfaceBlock(x-1,z+1).pos.y, z+1));
									}
									if (z - 1 >= 0) {
										candidats.Add(new ChunkPos(x, GetSurfaceBlock(x,z-1).pos.y, z-1));
										if (rightSide) candidats.Add(new ChunkPos(x+1, GetSurfaceBlock(x+1,z-1).pos.y, z-1));
										if (leftSide) candidats.Add(new ChunkPos(x-1, GetSurfaceBlock(x-1,z-1).pos.y, z-1));
									}
									foreach (ChunkPos p in candidats) {
										SurfaceBlock n = GetSurfaceBlock(p.x, p.z);
											if (n == null ) continue;
											if (n.material_id == ResourceType.DIRT_ID && !dirt_for_grassland.Contains(n) &&n.grassland== null && Mathf.Abs(b.pos.y - p.y) < 2) dirt_for_grassland.Add(n);
									}
								}
									b.AddGrassland();
									int lifeTransfer = (int)(GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
									if (lifePower > lifeTransfer) {b.grassland.AddLifepower(lifeTransfer); lifePower -= lifeTransfer;}
									else {b.grassland.AddLifepower((int)lifePower); lifePower = 0;}
									grassland_blocks.Add(b.grassland);
							}
							else dirt_for_grassland.RemoveAt(pos);
						}
				}
					else {//adding energy to existing life tiles
						if (grassland_blocks.Count != 0) {
							Grassland gl = null;
							while (gl== null && grassland_blocks.Count >0) {
								int pos = (int)(Random.value * (grassland_blocks.Count - 1));
								gl = grassland_blocks[pos];
								if (gl != null) {
									int  count = (int)(GameMaster.MAX_LIFEPOWER_TRANSFER * GameMaster.lifeGrowCoefficient);
									if (lifePower < count)  count = (int)lifePower;
									gl.AddLifepower(count);
									lifePower -= count;
								}
								else {grassland_blocks.RemoveAt(pos);continue;}
						}
					}
					}
					lifepower_timer = GameMaster.LIFEPOWER_TICK;
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
								else lifePower += gl.TakeLifepower(energy_take_speed);
								pos++;
							}
							else {
								grassland_blocks.RemoveAt(pos); 
							}
						}
					}
				}
				lifepower_timer = GameMaster.LIFEPOWER_TICK;
		}
		}
	}

	public byte GetVisibilityMask(int i, int j, int k) {
		byte vmask = 63;
		if (j > GameMaster.layerCutHeight) vmask = 0;
		else {
			Block bx = GetBlock(i+1,j,k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube ) vmask &= 61;
			bx = GetBlock(i-1,j,k); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube ) vmask &= 55;
			bx = GetBlock(i,j+1,k); if (bx != null &&!bx.isTransparent && bx.type != BlockType.Shapeless && (bx.pos.y != GameMaster.layerCutHeight + 1)) vmask &= 47;
			bx = GetBlock(i,j - 1,k); if (bx != null && !bx.isTransparent && (bx.type == BlockType.Cube || bx.type == BlockType.Cave)) vmask &= 31;
			bx = GetBlock(i,j,k+1); if (bx != null &&!bx.isTransparent && bx.type == BlockType.Cube ) vmask &= 62;
			bx = GetBlock(i,j,k-1); if (bx != null && !bx.isTransparent && bx.type == BlockType.Cube ) vmask &= 59;
		}
		return vmask;
	}
	void ApplyVisibleInfluenceMask(int x, int y, int z, byte mask) {
		Block b = GetBlock(x,y,z+1); if ( b != null ) b.ChangeVisibilityMask(2, ((mask & 1) != 0 ));
		b = GetBlock(x + 1,y,z); if ( b != null ) b.ChangeVisibilityMask(3, ((mask & 2) != 0));
		b = GetBlock(x, y ,z-1); if ( b != null ) b.ChangeVisibilityMask(0, ((mask & 4) != 0));
		b = GetBlock(x - 1,y ,z); if ( b != null ) b.ChangeVisibilityMask(1, ((mask & 8) != 0));
		b = GetBlock(x ,y + 1, z ); if ( b != null ) b.ChangeVisibilityMask(5, ((mask & 16) != 0));
		b = GetBlock(x ,y - 1,z); if ( b != null ) b.ChangeVisibilityMask(4, ((mask & 32) != 0));
		BroadcastChunkUpdate( new ChunkPos(x,y,z) );
	}

	public void AddBlock (ChunkPos f_pos, BlockType f_type, int f_material_id) {
		int x = f_pos.x, y = f_pos.y, z = f_pos.z;
		if (blocks[x,y,z] != null) ReplaceBlock(f_pos, f_type, f_material_id, false);
		GameObject g = null;
		CubeBlock cb = null;
		Block b = null;
		byte visMask = GetVisibilityMask(x,y,z), influenceMask = 63; // видимость объекта, видимость стенок соседних объектов

		switch (f_type) {
		case BlockType.Cube:
			g = new GameObject();
			cb = g.AddComponent<CubeBlock>();
			cb.BlockSet(this, f_pos, f_material_id, false);
			blocks[x,y,z] = cb;
			if (cb.isTransparent == false) influenceMask = 0; else influenceMask = 1; // закрывает собой все соседние стенки
			break;
		case BlockType.Shapeless:
			g = new GameObject();
			blocks[x,y,z] = g.AddComponent<Block>();
			blocks[x,y,z].BlockSet(this, f_pos, f_material_id);
			break;
		case BlockType.Surface:
			b = blocks[x, y-1, z];
			if (b == null ) {
				return;
			}
			g = new GameObject();
			SurfaceBlock sb = g.AddComponent<SurfaceBlock>();
			sb.SurfaceBlockSet(this, f_pos, f_material_id);
			blocks[x,y,z] = sb;
			surfaceBlocks.Add(sb);
			influenceMask = 47;
			break;
		case BlockType.Cave:
			Block upperBlock = blocks[x, y-1, z]; 
			if (upperBlock == null) {
				AddBlock(f_pos, BlockType.Surface, f_material_id);
				return;
			}
			g = Instantiate(cave_pref);
			Block lowerBlock = blocks[x, y-1, z]; 
			if ( lowerBlock == null ) {
				Destroy(g);
				return;
			}
			CaveBlock caveb = g.GetComponent<CaveBlock>();
			caveb.CaveBlockSet(this, f_pos, f_material_id, upperBlock.material_id );
			blocks[x,y,z] = caveb;
			influenceMask = 15;
			break;
		}
		blocks[x,y,z].SetVisibilityMask(visMask);
		blocks[x,y,z].SetRenderBitmask(prevBitmask);
		ApplyVisibleInfluenceMask(x,y,z, influenceMask);
	}

	public void ReplaceBlock(ChunkPos f_pos, BlockType f_newType, int f_newMaterial_id, bool naturalGeneration) {
		int x = f_pos.x, y = f_pos.y, z= f_pos.z;
		Block originalBlock = GetBlock(x,y,z);
		if (originalBlock == null) {AddBlock(f_pos, f_newType, f_newMaterial_id); return;}
		if (originalBlock.type == f_newType) {
			originalBlock.ReplaceMaterial(f_newMaterial_id);
			return;
		}
		else {
			if (originalBlock.indestructible) {
				if ((originalBlock.type == BlockType.Surface || originalBlock.type == BlockType.Cave) && f_newType != BlockType.Surface && f_newType != BlockType.Cave) return;
			}
		}
		Block b = null;
		byte influenceMask = 63;
		switch (f_newType) {
		case BlockType.Shapeless:
			b = new GameObject().AddComponent<Block>();
			b.ShapelessBlockSet(this, f_pos, null);
			blocks[x,y,z] = b;
			break;
		case BlockType.Surface:
			SurfaceBlock sb= new GameObject().AddComponent<SurfaceBlock>();
			sb.SurfaceBlockSet(this, f_pos, f_newMaterial_id);
			b = sb;
			blocks[x,y,z] = sb;
			influenceMask = 31;
			if (originalBlock.type == BlockType.Cave) {
				CaveBlock originalSurface = originalBlock as CaveBlock;
				foreach (Structure s in originalSurface.surfaceObjects) {
					if (s == null) continue;
					s.SetBasement(sb, new PixelPosByte(s.innerPosition.x, s.innerPosition.z));
				}
			}
			break;
		case BlockType.Cube:
			CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
			cb.BlockSet(this, f_pos,f_newMaterial_id, naturalGeneration);
			b = cb;
			blocks[x,y,z] = cb;
			influenceMask = 0;
			break;
		case BlockType.Cave:
			CaveBlock cvb = Instantiate(cave_pref).GetComponent<CaveBlock>();
			int upMaterial_id = ResourceType.Stone.ID, downMaterial_id = upMaterial_id;
			b = GetBlock(x,y+1,z) ;
			if (b!= null && b.material_id != 0) upMaterial_id = b.material_id;
			b = GetBlock(x,y-1,z);
			if (b!= null && b.material_id != 0) downMaterial_id = b.material_id;
			cvb.CaveBlockSet(this, f_pos, upMaterial_id, downMaterial_id);
			blocks[x,y,z] = cvb;
			b = cvb;
			influenceMask = 15;
			if (originalBlock.type == BlockType.Surface) {
				SurfaceBlock originalSurface = originalBlock as SurfaceBlock;
				foreach (Structure s in originalSurface.surfaceObjects) {
					if (s == null) continue;
					s.SetBasement(cvb, new PixelPosByte(s.innerPosition.x, s.innerPosition.z));
				}
			}
			break;
		}
		b.SetVisibilityMask( originalBlock.visibilityMask );
		blocks[x,y,z].MakeIndestructible(originalBlock.indestructible);
		Destroy(originalBlock.gameObject);
		b.SetRenderBitmask(prevBitmask);
		ApplyVisibleInfluenceMask(x,y,z,influenceMask);
	}

	public void DeleteBlock(ChunkPos pos) {
		// в сиквеле стоит пересмотреть всю иерархию классов ><
		Block b = blocks[pos.x, pos.y,pos.z];
		if (b == null || b.indestructible == true) return;
		int x = pos.x, y = pos.y, z = pos.z;
		switch (b.type) {
		case BlockType.Cube : 
			CubeBlock cb = b.GetComponent<CubeBlock>();
			Block upperBlock = GetBlock(x, y+1, z);
			if ( upperBlock != null && upperBlock.type == BlockType.Surface ) DeleteBlock(new ChunkPos(x, y+1, z));
			break;
		case BlockType.Surface: 
		case BlockType.Cave:
			SurfaceBlock sb = b as SurfaceBlock;
			if (sb.grassland != null) sb.grassland.Annihilation();
			sb.ClearSurface();
			break;
		}
		Destroy(blocks[x,y,z].gameObject);
		blocks[x,y,z] = null;
		ApplyVisibleInfluenceMask(x,y,z, 63);
	}

	public void GenerateNature (PixelPosByte lifeSourcePos, int lifeVolume) {
		byte px = lifeSourcePos.x, py = lifeSourcePos.y;
		float [,] lifepowers = new float[CHUNK_SIZE, CHUNK_SIZE];
		lifepowers[px, py] = 1;
		float power = 1, half = (float)CHUNK_SIZE / 2f;
		bool leftSide = false, rightSide = false, upSide = false, downSide = false;
		SurfaceBlock[,] t_surfaceBlocks = new SurfaceBlock[CHUNK_SIZE, CHUNK_SIZE];
		RecalculateSurfaceBlocks();
		foreach (SurfaceBlock sb in surfaceBlocks) {
			if (t_surfaceBlocks[sb.pos.x, sb.pos.z] == null) t_surfaceBlocks[sb.pos.x, sb.pos.z] = sb;
			else {
				if (sb.pos.y > t_surfaceBlocks[sb.pos.x, sb.pos.z].pos.y) t_surfaceBlocks[sb.pos.x, sb.pos.z] = sb;
			}
		}
		if (px > 0) {
			leftSide = true;
			for (int i = px - 1; i >= 0; i--) {
				byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i + 1,py].pos.y - t_surfaceBlocks[i, py].pos.y);
				lifepowers[i,py] = power * (1 - (delta / half) * (delta / half));
				power = lifepowers[i, py] * 0.9f;
			}
		}
		power = 1;
		if (px < CHUNK_SIZE - 1) {
			rightSide = true;
			for (int i = px + 1; i < CHUNK_SIZE; i++) {
				byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i - 1,py].pos.y - t_surfaceBlocks[i, py].pos.y);
				lifepowers[i,py] = power * (1 - (delta / half) * (delta / half));
				power = lifepowers[i, py] * 0.9f;
			}
		}
		power = 1;
		if (py > 0) {
			downSide = true;
			for (int i = py - 1; i >= 0; i--) {
				byte delta = (byte)Mathf.Abs(t_surfaceBlocks[px, i+1].pos.y - t_surfaceBlocks[px,i].pos.y);
				lifepowers[px,i] = power * (1 - (delta / half) * (delta / half));
				power = lifepowers[px, i] * 0.9f;
			}
		}
		power = 1;
		if (px < CHUNK_SIZE - 1) {
			upSide= true;
			for (int i = py + 1; i < CHUNK_SIZE; i++) {
				byte delta = (byte)Mathf.Abs(t_surfaceBlocks[px, i -1].pos.y - t_surfaceBlocks[px, i].pos.y);
				lifepowers[px, i] = power * (1 - (delta / half) * (delta / half));
				power = lifepowers[px, i] * 0.9f;
			}
		}

		// горизонтальная обработка
		if (leftSide) {
			for (int i = 0; i< CHUNK_SIZE; i++) {
				if (i == py) continue;
				power= lifepowers[i, px];
				for (int j = px - 1; j >= 0; j--) {
					byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i,j+1].pos.y - t_surfaceBlocks[i,j].pos.y);
					lifepowers[i,j] = power  * (1 - (delta / half) * (delta / half));
					power = lifepowers[i,j] * 0.9f;
				}
			}
		}
		if (rightSide) {
			for (int i = 0; i< CHUNK_SIZE; i++) {
				if (i == py) continue;
				power= lifepowers[i, px];
				for (int j = px +1; j < CHUNK_SIZE; j++) {
					byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i,j].pos.y - t_surfaceBlocks[i,j-1].pos.y);
					lifepowers[i,j] = power  * (1 - (delta / half) * (delta / half));
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
					byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i,j].pos.y - t_surfaceBlocks[i,j-1].pos.y);
					lifepowers[i,j] = (power  * (1 - (delta / half) * (delta / half)) + lifepowers[i,j] ) / 2f;
					power = lifepowers[i,j] * 0.9f;
				}
			}
			if (downSide) {
				power = lifepowers [i,py];
				for (int j = py - 1; j >=0; j--) {
					byte delta = (byte)Mathf.Abs(t_surfaceBlocks[i,j].pos.y - t_surfaceBlocks[i,j+1].pos.y);
					lifepowers[i,j] = (power  * (1 - (delta / half) * (delta / half)) + lifepowers[i,j]) / 2f;
					power = lifepowers[i,j] * 0.9f;
				}
			}
		}

		float total = 0;
		List<SurfaceBlock> afl = new List<SurfaceBlock>();
		for (int i =0; i< CHUNK_SIZE; i++) {
			for (int j = 0; j< CHUNK_SIZE; j++) {
				SurfaceBlock b = t_surfaceBlocks[i,j];
				if (b == null) continue;
				if (b.material_id == ResourceType.DIRT_ID) { // Acceptable for life
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

	public static void SetChunkSize( byte x) {
		CHUNK_SIZE = x;
	}

	public void SetChunk(int[,,] newData) {
		if (blocks != null) ClearChunk();
		int size = newData.GetLength(0);
		CHUNK_SIZE = (byte) size;
		if (CHUNK_SIZE < 3) CHUNK_SIZE = 16;
		GameMaster.layerCutHeight = CHUNK_SIZE;

		blocks = new Block[size,size,size];
		surfaceBlocks = new List<SurfaceBlock>();

		for (int x = 0; x< size; x++) {
			for (int z =0; z< size; z++) {
				byte surfaceFound = 2;
				for (int y = size - 1; y >= 0; y--) {
					if (newData[x,y,z] != 0) {
						if ( surfaceFound == 2 ) {
							SurfaceBlock sb = new GameObject().AddComponent<SurfaceBlock>();
							sb.SurfaceBlockSet(this, new ChunkPos(x,y,z), newData[x,y,z]);
							surfaceBlocks.Add(sb);
							blocks[x,y,z] = sb;
							surfaceFound --;
						}
						else {
							CubeBlock cb = new GameObject().AddComponent<CubeBlock>();
							cb.BlockSet(this, new ChunkPos(x,y,z), newData[x,y,z], true);
							blocks[x,y,z] = cb;
							if (surfaceFound == 1) {
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
					if (blocks[x,y,z] == null ) continue;
					blocks[x,y,z].SetVisibilityMask(GetVisibilityMask(x,y,z));
				}
			}
		}
		if (surfaceBlocks.Count != 0) {
			foreach (SurfaceBlock sb in surfaceBlocks) {
				GameMaster.geologyModule.SpreadMinerals(sb);
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
		surfaceBlocks.Clear(); grassland_blocks.Clear();
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
					if ( b !=null ) {
						b.SetRenderBitmask(renderBitmask);
					}
				}
					prevBitmask = renderBitmask;
				}
		}
		else {
			//camera in chunk
			foreach (Block b in blocks) {
				if (b == null ) continue;
				Vector3 icpos = campoint.InverseTransformPoint(b.transform.position);
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

	public void AddLifePower (int count) {lifePower += count; if (lifepower_timer == 0) lifepower_timer = GameMaster.LIFEPOWER_TICK;}
	public int TakeLifePower (int count) {
		if (count < 0) return 0;
		float lifeTransfer = count;
		if (lifeTransfer > lifePower) {if (lifePower >= 0) lifeTransfer = lifePower; else lifeTransfer = 0;}
		lifePower -= lifeTransfer;
		if (lifepower_timer == 0) lifepower_timer = GameMaster.LIFEPOWER_TICK;
		return (int)lifeTransfer;
	}
	public int TakeLifePowerWithForce (int count) {
		if (count < 0) return 0;
		lifePower -= count;
		if (lifepower_timer == 0) lifepower_timer = GameMaster.LIFEPOWER_TICK;
		return count;
	}
		
	public SurfaceBlock GetSurfaceBlock(int x, int z) {
		if (x < 0 || z < 0 || x >= CHUNK_SIZE || z >= CHUNK_SIZE) return null;
		SurfaceBlock fsb = null;
		foreach (SurfaceBlock sb in surfaceBlocks) {
			if ( sb.pos.x == x && sb.pos.z == z) fsb = sb; // to find the highest
		}
		return fsb; // we are not watching you. Honestly.
	}

	public void BlockByStructure(byte x, byte y, byte z, Structure s) {
		if (x > CHUNK_SIZE || y > CHUNK_SIZE || z > CHUNK_SIZE || x < 0 || y < 0 || z < 0 || s == null) return;
		Block b = GetBlock(x,y,z);
		if (b != null) { ReplaceBlock( new ChunkPos(x,y,z), BlockType.Shapeless, 0, false); }
		else blocks[x,y,z] = new GameObject().AddComponent<Block>();
		blocks[x,y,z].ShapelessBlockSet(this, new ChunkPos(x,y,z), s);
	}

	public void RecalculateSurfaceBlocks() {
		surfaceBlocks = new List<SurfaceBlock>();
		foreach (Block b in blocks) {
			if (b == null) continue;
			if (b.type == BlockType.Surface || b.type == BlockType.Cave) surfaceBlocks.Add(b as SurfaceBlock);
		}
	}

	void BroadcastChunkUpdate(ChunkPos pos) {
		int i =0;
		while ( i < chunkUpdateSubscribers.Count ) {
			if (chunkUpdateSubscribers[i] == null) {
				chunkUpdateSubscribers.RemoveAt(i);
				continue;
			}
			chunkUpdateSubscribers[i].BroadcastMessage("ChunkUpdated", pos, SendMessageOptions.DontRequireReceiver);
			i++;
		}
	}

	public void LayersCut (  ) {
		for (int x = 0; x < CHUNK_SIZE; x++) {
				for (int z = 0; z < CHUNK_SIZE; z++) {
				int y = CHUNK_SIZE - 1;
				if (GameMaster.layerCutHeight != CHUNK_SIZE) {
					for (; y > GameMaster.layerCutHeight; y--) {
						if (blocks[x,y,z] != null) blocks[x,y,z].SetVisibilityMask(0);
					}
				}
				for (; y > -1; y--) {
					if (blocks[x,y,z] != null) blocks[x,y,z].SetVisibilityMask(GetVisibilityMask(x,y,z));
				}
			}			
		}
	}

	public void SaveChunk() {
		
	}

	void OnGUI () { //test
		GUI.Label(new Rect(0, 32, 64,32), lifePower.ToString());
	}
}
