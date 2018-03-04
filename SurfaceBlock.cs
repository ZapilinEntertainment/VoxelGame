using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Content {Empty, Plant, HarvestableResources, Structure, MainStructure}
public struct SurfaceRect {
	public byte x, z, x_size, z_size;
	public Content content;
	public GameObject myGameObject;
	public SurfaceRect(byte x_pos, byte z_pos, byte size_x, byte size_z, Content f_content, GameObject theGameObject) {
		x = x_pos; z = z_pos; x_size = size_x; z_size = size_z; content = f_content; myGameObject = theGameObject;
	}
	public static SurfaceRect Empty;
	static SurfaceRect() {
		SurfaceRect.Empty = new SurfaceRect(0,0,0,0,Content.Empty, null);
	}
}

public class SurfaceBlock : Block {
	public const byte INNER_RESOLUTION = 16;
	public MeshRenderer surfaceRenderer {get;private set;}
	public Grassland grassland{get;private set;}
	List<SurfaceRect> surfaceObjects;
	public sbyte cellsStatus {get; private set;} // -1 is not stated, 1 is full, 0 is empty;

	void Awake() 
	{
		cellsStatus = 0;
		GameObject g = GameObject.Instantiate(PoolMaster.quad_pref) as GameObject;
		surfaceRenderer =g.GetComponent <MeshRenderer>();
		g.transform.parent = transform;
		g.transform.localPosition = new Vector3(0, -Block.QUAD_SIZE/2f, 0); 
		g.transform.localRotation = Quaternion.Euler(90, 0, 0);
		g.name = "upper_plane"; 
		material_id = 0;
		surfaceRenderer.enabled = true;
		surfaceObjects = new List<SurfaceRect>();
	}

	bool[,] GetBooleanMap() {
		bool[,] map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
		for (int i =0; i < map.GetLength(0); i++) {
			for (int j =0; j< map.GetLength(1); j++) map[i,j] = false;
		}
		if (surfaceObjects.Count != 0) {
			foreach (SurfaceRect sr in surfaceObjects) {
				for (int i =0; i< sr.x_size; i++) {
					for (int j =0; j<sr.z_size; j++) {
						map[sr.x + i, sr.z + j] = true;
					}
				}
			}
		}
		return map;
	}
	void CellsStatusUpdate() {
		bool[,] map = GetBooleanMap();
		bool empty = true, full = true; 
		bool emptyCheckFailed = false, fullCheckFailed = false;
		foreach (bool b in map) {
			if (b == true) {empty = false; emptyCheckFailed = true;}
			else {full = false; fullCheckFailed = true;}
			if (emptyCheckFailed && fullCheckFailed ) {cellsStatus = -1; break;}
			else {
				if (empty) cellsStatus = 0;
				else {if (full) cellsStatus = 1;}
			}
		}
	}

	public void AddStructure(SurfaceRect sr) { // with autoreplacing
		if (cellsStatus != 0) { 
			int i =0;
			while (i < surfaceObjects.Count) {
				SurfaceRect a = surfaceObjects[i];
				int leftX = -1, rightX = -1;
				if (a.x > sr.x) leftX = a.x; else leftX = sr.x;
				if (a.x + a.x_size > sr.x + sr.x_size) rightX = sr.x + sr.x_size; else rightX = a.x + a.x_size;
				if (leftX >= rightX) {i++;continue;}
				int topZ = -1, downZ = -1;
				if (a.z > sr.z) downZ = a.z; else downZ = sr.z;
				if (a.z + a.z_size > sr.z + sr.z_size) topZ = sr.z + sr.z_size; else topZ = a.z + a.z_size;
				if (topZ <= downZ) {i++;continue;}
				else {
					a.myGameObject.GetComponent<Structure>().UnsetBasement();
					Destroy(a.myGameObject);
					surfaceObjects.RemoveAt(i);
				}
			}
		}
		surfaceObjects.Add(sr);
		sr.myGameObject.transform.parent = transform;
		sr.myGameObject.transform.localPosition = GetLocalPosition(sr);
		CellsStatusUpdate();
	}
	public void RemoveStructure(SurfaceRect sr) {
		int count = surfaceObjects.Count;
		if (count == 0) return;
		for ( int i = 0; i < count; i++) {
			if (surfaceObjects[i].Equals(sr)) {
				surfaceObjects.RemoveAt(i);
				if (surfaceObjects.Count == 0) cellsStatus = 0;
				else CellsStatusUpdate();
				break;
			}
		}
	}


	public Grassland AddGrassland() {
		if (grassland == null)  {
			grassland = gameObject.AddComponent<Grassland>();
			grassland.SetBlock(this);
		}
		return grassland;
	}

	public override void Replace( int newId) {
		material_id = newId;
		surfaceRenderer.material =  PoolMaster.GetMaterialById(newId);
		if (grassland != null && !(newId == PoolMaster.DIRT_ID ||newId == PoolMaster.DIRT_ID) ) grassland.Annihilation();
	}

	public override void BlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id) {
		// проверки при повторном использовании?
		isTransparent = false;
		myChunk = f_chunk; transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = f_material_id;
		surfaceRenderer.material = PoolMaster.GetMaterialById(material_id);
		type = BlockType.Surface;
		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
	}
		
	public static Vector3 GetLocalPosition(SurfaceRect sr) {
		float res = INNER_RESOLUTION;
		float xpos = sr.x + sr.x_size/2f ;
		float zpos = sr.z + sr.z_size/2f;
		return( new Vector3((xpos / res - 0.5f) * Block.QUAD_SIZE * 0.9f, -Block.QUAD_SIZE/2f, (zpos / res - 0.5f)* Block.QUAD_SIZE * 0.9f));
	}


	public PixelPosByte GetRandomCell() {
		if (cellsStatus == 1) return PixelPosByte.Empty;
		else {
			if (cellsStatus == 0) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
			else {
				List<PixelPosByte> acceptableVariants = GetAcceptablePositions(10);
				int ppos = (int)(Random.value * (acceptableVariants.Count - 1));
				return acceptableVariants[ppos];
			}
		}
	}
	public List<PixelPosByte> GetRandomCells (int count) {
		List<PixelPosByte> positions = new List<PixelPosByte>();
		if (cellsStatus != 1)  {
			List<PixelPosByte> acceptableVariants = GetAcceptablePositions(INNER_RESOLUTION * INNER_RESOLUTION);
			while (positions.Count <= count && acceptableVariants.Count > 0) {
				int ppos = (int)(Random.value * (acceptableVariants.Count - 1));
				positions.Add(acceptableVariants[ppos]);
				acceptableVariants.RemoveAt(ppos);
			}
		}
		return positions;
	}

	public PixelPosByte GetRandomPosition(byte xsize, byte zsize) {
		if (cellsStatus == 1 || xsize >= INNER_RESOLUTION || zsize >= INNER_RESOLUTION) return PixelPosByte.Empty;
		int maxVariants= 10;
		if (cellsStatus == 0) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
		List<PixelPosByte> acceptablePositions = GetAcceptablePositions(xsize,zsize, maxVariants);
		if (acceptablePositions.Count > 0) {
			int ppos = (int)(Random.value * (acceptablePositions.Count - 1));
			return acceptablePositions[ppos];
		}
		else return PixelPosByte.Empty;
	}
	public List<PixelPosByte> GetRandomPositions (byte xsize, byte zsize, int count) {
		if (cellsStatus == 1) return new List<PixelPosByte>();
		List<PixelPosByte> acceptablePositions = GetAcceptablePositions(xsize,zsize, 2 * count);
		if (acceptablePositions.Count <= count) return acceptablePositions;
		else {
			List<PixelPosByte> positions = new List<PixelPosByte>();
			if (acceptablePositions.Count > count) {
				int i = 0;
				while ( i < count && acceptablePositions.Count > 0) {
					int ppos = (int)(Random.value * (acceptablePositions.Count - 1));
					positions.Add(acceptablePositions[ppos]);
					acceptablePositions.RemoveAt(ppos);
					i++;
				}
			}
			return positions;
		}
	}

	List<PixelPosByte> GetAcceptablePositions(byte xsize, byte zsize, int maxVariants) {
		if (maxVariants > INNER_RESOLUTION * INNER_RESOLUTION) maxVariants = INNER_RESOLUTION * INNER_RESOLUTION;
		bool[,] map = GetBooleanMap();
		List<PixelPosByte> acceptablePositions = new List<PixelPosByte>();
		if (Random.value > 0.5f) { // поиск снизу
			for (int i = INNER_RESOLUTION - xsize; i >= 0; i--) {
				byte width = 0;
				for (int j =  INNER_RESOLUTION; j >= 0; j--)
				{
					if (map[i,j] == false) width++; else width = 0;
					if (width >= xsize) {
						bool appliable = true;
						for (byte a = 1; a < xsize; a++) {
							for (byte b = 0; b < zsize; b++) {
								if (map[i + a, j - b] == true) {appliable = false; break;}
							}
							if ( !appliable) break;
						}
						if ( appliable ) {acceptablePositions.Add(new PixelPosByte(i, j)); width--;}
						if (acceptablePositions.Count >= maxVariants) break;
					}
				}
				if (acceptablePositions.Count >= maxVariants) break;
			}
		}
		else { // поиск сверху
			for (int i = xsize - 1; i < INNER_RESOLUTION; i++) {
				byte width = 0;
				for (int j = 0; j <INNER_RESOLUTION; j++) {
					if (map[i,j] == false) width++; else width = 0;
					if (width >= xsize) {
						bool appliable = true;
						for (byte a = 1; a < xsize; a++) {
							for (byte b = 0; b < zsize; b++) {
								if ( map[i - a, j - b] == true ) {appliable = false; break;}
							}
							if ( appliable == false) break;
						}
						if ( appliable ) {acceptablePositions.Add(new PixelPosByte(i - xsize + 1, j - zsize + 1)); width--;}
						if (acceptablePositions.Count >= maxVariants) break;
					}
				}
				if (acceptablePositions.Count >= maxVariants) break;
			}
		}
		return acceptablePositions;
	}

	List<PixelPosByte> GetAcceptablePositions(int count) {
		bool[,] map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
		List<PixelPosByte> acceptableVariants = new List<PixelPosByte>();
		for (byte i = 0; i< INNER_RESOLUTION; i++) {
			for (byte j =0; j < INNER_RESOLUTION; j++) {
				if (map[i,j] == false) {acceptableVariants.Add(new PixelPosByte(i,j)); if (acceptableVariants.Count >= count) break;}
			}
			if (acceptableVariants.Count >= count) break;
		}
		return acceptableVariants;
	}

	public void ReplaceStructure(SurfaceRect sr) {
		if (cellsStatus == 0) return;
		for (int i = 0; i< surfaceObjects.Count; i++) {
			if (surfaceObjects[i].x == sr.x && surfaceObjects[i].z == sr.z && surfaceObjects[i].x_size == sr.x_size && surfaceObjects[i].z_size == sr.z_size   ) {
				Destroy(surfaceObjects[i].myGameObject);
				surfaceObjects[i] = sr;
			}
		}
	}
}
