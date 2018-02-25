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
	bool cameraHasMoved = false; Vector3 prevCamPos = Vector3.zero; Quaternion prevCamRot = Quaternion.identity;
	float cullingTimer =0, cullingUpdateTime = 0.04f;
	public byte prevBitmask = 63;
	int[,] surfaceBlocks;
	List<GameObject> structures;
	public int lifePower = 0;
	public const float LIFEPOWER_TICK = 1;float lifepower_timer = 0;
	List<Block> dirt_for_grassland, grassland_blocks;
	public const int MAX_LIFEPOWER_TRANSFER = 16, CHUNK_SIZE = 16;

	void Awake() {
		dirt_for_grassland = new List<Block>();
		grassland_blocks = new List<Block>();
	}

	void Start() {
		CullingUpdate();
		prevCamPos = Camera.main.transform.position;
		prevCamRot = Camera.main.transform.rotation;
	}

	void Update() {
		if (prevCamPos != Camera.main.transform.position || prevCamRot != Camera.main.transform.rotation) {
			cameraHasMoved = true;
			prevCamPos = Camera.main.transform.position;
			prevCamRot = Camera.main.transform.rotation;
		}
		if (cullingTimer > 0) cullingTimer-= Time.deltaTime;
		if (cullingTimer <= 0 && cameraHasMoved) CullingUpdate(); 

		if (lifepower_timer > 0) {
		lifepower_timer -= Time.deltaTime;
		if (lifepower_timer <= 0) {
				if (lifePower > 0) {
					if (dirt_for_grassland.Count == 0 || lifePower == 0) {lifepower_timer = 0;}
					else {
						Block b = null;
						while (b == null && dirt_for_grassland.Count > 0) {
							int pos = (int)(Random.value * (dirt_for_grassland.Count - 1));
							b = dirt_for_grassland[pos];
							if (b.body.GetComponent<Grassland>() || !b.onSurface) {dirt_for_grassland.RemoveAt(pos);continue;}
							if (b != null) {
								Grassland gl = b.body.AddComponent<Grassland>();
								{
									int x = b.pos.x; int z = b.pos.z;
									if (x+1 < CHUNK_SIZE) {Block n = GetBlock(x+1, surfaceBlocks[x+1,z], z); if (n != null && n.f_id == Block.DIRT_ID && n.body.GetComponent<Grassland>() == null && !dirt_for_grassland.Contains(n)) dirt_for_grassland.Add(n);}
									if (x-1 >= 0) {Block n = GetBlock(x-1, surfaceBlocks[x-1,z], z); if (n != null && n.f_id == Block.DIRT_ID && n.body.GetComponent<Grassland>() == null && !dirt_for_grassland.Contains(n)) dirt_for_grassland.Add(n);}
									if (z+1 < CHUNK_SIZE) {Block n = GetBlock(x, surfaceBlocks[x,z+1], z+1); if (n != null && n.f_id == Block.DIRT_ID && n.body.GetComponent<Grassland>() == null && !dirt_for_grassland.Contains(n)) dirt_for_grassland.Add(n);}
									if (z-1 >= 0) {Block n = GetBlock(x, surfaceBlocks[x,z-1], z-1); if (n != null && n.f_id == Block.DIRT_ID && n.body.GetComponent<Grassland>() == null && !dirt_for_grassland.Contains(n)) dirt_for_grassland.Add(n);}
								}
								gl.SetBlock(b);
								if (lifePower > MAX_LIFEPOWER_TRANSFER) {gl.AddLifepower(MAX_LIFEPOWER_TRANSFER); lifePower -= MAX_LIFEPOWER_TRANSFER;}
								else {gl.AddLifepower(lifePower); lifePower = 0;}
								grassland_blocks.Add(b);
								lifepower_timer = LIFEPOWER_TICK;
							}
							dirt_for_grassland.RemoveAt(pos);
						}
					}
				}
				else { // LifePower decreases
					if (grassland_blocks.Count == 0) lifepower_timer = 0;
					else {
						Grassland gl = null;
						while (gl == null && grassland_blocks.Count > 0) {
							int pos = (int)(Random.value * (grassland_blocks.Count - 1));
							gl = grassland_blocks[pos].body.GetComponent<Grassland>();
							if (gl != null) {
								if (gl.lifepower > MAX_LIFEPOWER_TRANSFER) {lifePower += MAX_LIFEPOWER_TRANSFER; gl.TakeLifepower(MAX_LIFEPOWER_TRANSFER);}
								else {lifePower += (int)gl.lifepower; gl.TakeLifepower((int)gl.lifepower);}
							}
							grassland_blocks.RemoveAt(pos);
						}
						if (lifePower < 0) lifepower_timer = LIFEPOWER_TICK;
					}
				}
		}
		}
	}

	void RecalculateDirtForGrassland() {
		if (surfaceBlocks == null) return;
		dirt_for_grassland = new List<Block>();
		List<ChunkPos> markedBlocks = new List<ChunkPos>();
		for (int x = 0; x < CHUNK_SIZE; x++) {
			for (int z = 0; z < CHUNK_SIZE; z++) {
				int y = surfaceBlocks[x,z];
				if (blocks[x,y,z].f_id == Block.GRASS_ID || (blocks[x,y,z].f_id == Block.DIRT_ID && blocks[x,y,z].body.GetComponent<Grassland>())) {
					if (x+1 < CHUNK_SIZE) {Block n = GetBlock(x+1,surfaceBlocks[x+1,z],z); if (n != null && n.f_id==Block.DIRT_ID && !markedBlocks.Contains(n.pos) &&n.body.GetComponent<Grassland>() == null ) {dirt_for_grassland.Add(n); markedBlocks.Add(n.pos); continue;}}
					if (x-1 >= 0) {Block n = GetBlock(x-1,surfaceBlocks[x-1,z],z); if (n != null && n.f_id==Block.DIRT_ID &&!markedBlocks.Contains(n.pos) &&n.body.GetComponent<Grassland>() == null ) {dirt_for_grassland.Add(n); markedBlocks.Add(n.pos); continue;}}
					if (z+1 < CHUNK_SIZE) {Block n = GetBlock(x,surfaceBlocks[x,z+1],z+1); if (n != null && n.f_id==Block.DIRT_ID &&!markedBlocks.Contains(n.pos) &&n.body.GetComponent<Grassland>() == null ) {dirt_for_grassland.Add(n); markedBlocks.Add(n.pos); continue;}}
					if (z-1 >= 0) {Block n = GetBlock(x,surfaceBlocks[x,z-1],z - 1); if (n != null && n.f_id==Block.DIRT_ID &&!markedBlocks.Contains(n.pos) &&n.body.GetComponent<Grassland>() == null ) {dirt_for_grassland.Add(n); markedBlocks.Add(n.pos); continue;}}
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
						blocks[x,y,z].onSurface = true;
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
			if (blocks[x,y,z + i] != null)  {appliable = false;break;}
		}
		if (appliable == false) return false;
		for (int i =0; i< str.height; i++) {
			blocks[x,y,z+i].Destroy();
			blocks[x,y,z+i] = Block.BlockedByStructure;
		}
		return true;
	}

	public void ReplaceBlock(int x, int y, int z, int newId) {
		if (blocks[x,y,z] != null) {
			Block b = blocks[x,y,z].Replace(newId);
			blocks[x,y,z] = null;
			blocks[x,y,z] = b;
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
		

	void CullingUpdate() {
		byte camSector = 0;
		Vector3 cpos = transform.InverseTransformPoint(Camera.main.transform.position);
		Vector3 v = Vector3.one * (-1);
		int size = blocks.GetLength(0);
		if (cpos.x > 0) { if (cpos.x > size) v.x = 1; else v.x = 0;} 
		if (cpos.y > 0) {if (cpos.y > size) v.y = 1; else v.y = 0;}
		if (cpos.z > 0) {if (cpos.z > size) v.z = 1; else v.z = 0;}
		//print (v);
		if (v != Vector3.zero) {
			Vector3 cdir = transform.InverseTransformDirection(Camera.main.transform.forward);
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
			Transform camPoint = Camera.main.transform;
			foreach (Block b in blocks) {
				if (b == null || !b.IsVisible()) continue;
				Vector3 icpos = camPoint.InverseTransformPoint(b.body.transform.position);
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
		cullingTimer = cullingUpdateTime;
		cameraHasMoved = false;
	}

	public void AddLifePower (int count) {lifePower += count; if (lifepower_timer == 0) lifepower_timer = LIFEPOWER_TICK;}
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
