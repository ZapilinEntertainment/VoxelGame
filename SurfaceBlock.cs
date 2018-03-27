using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SurfaceRect {
	public byte x,z,x_size,z_size;
	public SurfaceRect(byte f_x, byte f_z, byte f_xsize, byte f_zsize) {
		if (f_x < 0) f_x = 0; if (f_x >= SurfaceBlock.INNER_RESOLUTION) f_x = SurfaceBlock.INNER_RESOLUTION - 1;
		if (f_z < 0) f_z = 0; if (f_z >= SurfaceBlock.INNER_RESOLUTION) f_z = SurfaceBlock.INNER_RESOLUTION - 1;
		if (f_xsize < 1) f_xsize = 1; if (f_xsize > SurfaceBlock.INNER_RESOLUTION) f_xsize = SurfaceBlock.INNER_RESOLUTION;
		if (f_zsize < 1) f_zsize = 1; if (f_zsize > SurfaceBlock.INNER_RESOLUTION) f_zsize = SurfaceBlock.INNER_RESOLUTION;
		x = f_x;
		z = f_z; 
		x_size = f_xsize; 
		z_size = f_zsize;
	}

	public static bool operator ==(SurfaceRect lhs, SurfaceRect rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(SurfaceRect lhs, SurfaceRect rhs) {return !(lhs.Equals(rhs));}
}
public struct SurfaceObject {
	public SurfaceRect rect;
	public Structure structure;

	public SurfaceObject(SurfaceRect f_rect, Structure f_structure) {
		rect = f_rect; structure = f_structure;
	}
}

public class SurfaceBlock : Block {
	public const byte INNER_RESOLUTION = 16;
	public MeshRenderer surfaceRenderer {get;private set;}
	public Grassland grassland{get;private set;}
	public List<SurfaceObject> surfaceObjects{get;private set;}
	public sbyte cellsStatus {get; private set;} // -1 is not stated, 1 is full, 0 is empty;
	public CubeBlock basement; 
	public int artificialStructures = 0;
	public float fertility = 1, habitability = 0;
	public bool[,] map {get; private set;}

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
		surfaceObjects = new List<SurfaceObject>();
		artificialStructures = 0;
		isTransparent = false;
	}


	public bool[,] GetBooleanMap() {
			map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
			for (int i =0; i < map.GetLength(0); i++) {
				for (int j =0; j< map.GetLength(1); j++) map[i,j] = false;
			}
			if (surfaceObjects.Count != 0) {
				int a = 0;
				while (a < surfaceObjects.Count) {
				if ( surfaceObjects[a].structure == null) {surfaceObjects.RemoveAt(a); continue;}
				SurfaceRect sr = surfaceObjects[a].rect;					
					for (int i =0; i< sr.x_size; i++) {
						for (int j =0; j < sr.z_size; j++) {
							map[sr.x + i, sr.z + j] = true;
						}
					}
					a++;
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

	public void AddStructure(SurfaceObject so) { // with autoreplacing
		if (so.structure == null) return;
		if (cellsStatus != 0) { 
			SurfaceRect sr = so.rect;
			int i =0;
			while (i < surfaceObjects.Count) {
				if ( !surfaceObjects[i].structure == null ) {surfaceObjects.RemoveAt(i); continue;}
				SurfaceRect a = surfaceObjects[i].rect;
				int leftX = -1, rightX = -1;
				if (a.x > sr.x) leftX = a.x; else leftX = sr.x;
				if (a.x + a.x_size > sr.x + sr.x_size) rightX = sr.x + sr.x_size; else rightX = a.x + a.x_size;
				if (leftX >= rightX) {i++;continue;}
				int topZ = -1, downZ = -1;
				if (a.z > sr.z) downZ = a.z; else downZ = sr.z;
				if (a.z + a.z_size > sr.z + sr.z_size) topZ = sr.z + sr.z_size; else topZ = a.z + a.z_size;
				if (topZ <= downZ) {i++;continue;}
				else {
					surfaceObjects[i].structure.UnsetBasement();
					Destroy(surfaceObjects[i].structure.gameObject);
					surfaceObjects.RemoveAt(i);
				}
			}
		}
		surfaceObjects.Add(so);
		so.structure.transform.parent = transform;
		so.structure.transform.localPosition = GetLocalPosition(so.rect);
		if (so.structure.isArtificial) artificialStructures++;
		CellsStatusUpdate();
	}

	/// <summary>
	/// For  1x1 objects only!
	/// </summary>
	/// <param name="cellObjects">Cell objects.</param>
	public void AddMultipleCellObjects(List<Structure> cellObjects) {
		List<PixelPosByte> pos = GetRandomCells(cellObjects.Count);
		for (int i = 0; i < cellObjects.Count; i++) {
			if (cellObjects[i] == null) continue;
			SurfaceObject so = new SurfaceObject(new SurfaceRect(pos[i].x, pos[i].y, 1,1), cellObjects[i]);
			cellObjects[i].SetBasement(this, pos[i]);
			surfaceObjects.Add(so);
		}
		CellsStatusUpdate();
	}
	public void RemoveStructure(SurfaceObject so) {
		int count = surfaceObjects.Count;
		if (count == 0) return;
		for ( int i = 0; i < count; i++) {
			if (surfaceObjects[i].Equals(so)) {
				if (so.structure.isArtificial) artificialStructures--;	if (artificialStructures < 0) artificialStructures = 0;
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

	public override void ReplaceMaterial( int newId) {
		material_id = newId;
		surfaceRenderer.material =  PoolMaster.GetMaterialById(newId);
		if (grassland != null && !(newId == PoolMaster.DIRT_ID ||newId == PoolMaster.GRASS_ID) ) grassland.Annihilation();
	}

	public void SurfaceBlockSet (Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id, CubeBlock f_basement) {
		// проверки при повторном использовании?
		isTransparent = false;
		myChunk = f_chunk; transform.parent = f_chunk.transform;
		pos = f_chunkPos; transform.localPosition = new Vector3(pos.x,pos.y,pos.z);
		transform.localRotation = Quaternion.Euler(Vector3.zero);
		material_id = f_material_id;
		surfaceRenderer.material = PoolMaster.GetMaterialById(material_id);
		type = BlockType.Surface; isTransparent = false;
		basement = f_basement;
		gameObject.name = "block "+ pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
	}
	public void SetBasement(CubeBlock cb) {if (cb != null) basement = cb;}
		
	public static Vector3 GetLocalPosition(SurfaceRect sr) {
		float res = INNER_RESOLUTION;
		float xpos = sr.x + sr.x_size/2f ;
		float zpos = sr.z + sr.z_size/2f;
		return( new Vector3((xpos / res - 0.5f) * Block.QUAD_SIZE , -Block.QUAD_SIZE/2f, (zpos / res - 0.5f)* Block.QUAD_SIZE));
	}
	public PixelPosByte WorldToLocalPosition(Vector3 pos) {
		float xdelta = pos.x - gameObject.transform.position.x; xdelta /= Block.QUAD_SIZE;
		float zdelta = pos.z - gameObject.transform.position.z; zdelta /= Block.QUAD_SIZE;
		if (Mathf.Abs(xdelta) > 0.5f|| Mathf.Abs(zdelta) > 0.5f) return PixelPosByte.Empty;
		else {
			return new PixelPosByte((byte)((xdelta + 0.5f) * INNER_RESOLUTION), (byte)(zdelta  + 0.5f) * INNER_RESOLUTION);
		}
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
		if (cellsStatus == 1 || xsize >= INNER_RESOLUTION || zsize >= INNER_RESOLUTION || xsize < 1 || zsize < 1) return PixelPosByte.Empty;
		if (cellsStatus == 0) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
		return GetAcceptablePosition(xsize, zsize);
	}
	public List<PixelPosByte> GetRandomPositions (byte xsize, byte zsize, int count) {
		if (cellsStatus == 1 || xsize >= INNER_RESOLUTION || zsize >= INNER_RESOLUTION || xsize < 1 || zsize < 1) return new List<PixelPosByte>();
		List<PixelPosByte> acceptablePositions = GetAcceptablePositions(xsize,zsize, count);
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

	PixelPosByte GetAcceptablePosition (byte xsize, byte zsize) {
		bool[,] map = GetBooleanMap();
		List<PixelPosByte> acceptablePositions = new List<PixelPosByte>();
		for (int xpos = 0; xpos <= INNER_RESOLUTION - xsize; xpos++) {
			int width = 0;
			for (int zpos = 0; zpos <= INNER_RESOLUTION - zsize; zpos++) {
				if (map[xpos, zpos] == true) width = 0; else width++;
				if (width >= zsize) {
					bool appliable = true;
					for (int xdelta = 1; xdelta < xsize; xdelta++) {
						for (int zdelta = 0; zdelta < zsize; zdelta++) {
							if (map[xpos + xdelta, zpos + zdelta] == true) {appliable = false; break;}
						}
						if (appliable == false) break;
					}
					if (appliable) {
						acceptablePositions.Add( new PixelPosByte(xpos, zpos)); width = 0;
						for (int xdelta = 1; xdelta < xsize; xdelta++) {
							for (int zdelta = 0; zdelta < zsize; zdelta++) {
								map[xpos + xdelta, zpos + zdelta] = true;
							}
						}
					}
				}
			}
		}
		return acceptablePositions[(int)(Random.value * (acceptablePositions.Count - 1))];
	}

	List<PixelPosByte> GetAcceptablePositions(byte xsize, byte zsize, int maxVariants) {
		if (maxVariants > INNER_RESOLUTION * INNER_RESOLUTION) maxVariants = INNER_RESOLUTION * INNER_RESOLUTION;
		if (xsize > INNER_RESOLUTION || zsize > INNER_RESOLUTION || xsize <=0 || zsize <= 0) return null;
		bool[,] map = GetBooleanMap();
		List<PixelPosByte> acceptablePositions = new List<PixelPosByte>();
		for (int xpos = 0; xpos <= INNER_RESOLUTION - xsize; xpos++) {
			int width = 0;
			for (int zpos = 0; zpos <= INNER_RESOLUTION - zsize; zpos++) {
				if (map[xpos, zpos] == true) width = 0; else width++;
				if (width >= zsize) {
					bool appliable = true;
					for (int xdelta = 1; xdelta < xsize; xdelta++) {
						for (int zdelta = 0; zdelta < zsize; zdelta++) {
							if (map[xpos + xdelta, zpos + zdelta] == true) {appliable = false; break;}
						}
						if (appliable == false) break;
					}
					if (appliable) {
						acceptablePositions.Add( new PixelPosByte(xpos, zpos)); width = 0;
						for (int xdelta = 1; xdelta < xsize; xdelta++) {
							for (int zdelta = 0; zdelta < zsize; zdelta++) {
								map[xpos + xdelta, zpos + zdelta] = true;
							}
						}
					}
				}
			}
		}
		while (acceptablePositions.Count > maxVariants) {
			int i = (int)(Random.value * (acceptablePositions.Count - 1));
			acceptablePositions.RemoveAt(i);
		}
		return acceptablePositions;
	}

	List<PixelPosByte> GetAcceptablePositions(int count) {
		bool[,] map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
		List<PixelPosByte> acceptableVariants = new List<PixelPosByte>();
		for (byte i = 0; i< INNER_RESOLUTION; i++) {
			for (byte j =0; j < INNER_RESOLUTION; j++) {
				if (map[i,j] == false) {acceptableVariants.Add(new PixelPosByte(i,j)); }
			}	
		}
		while (acceptableVariants.Count > count) {
			int i = (int)(Random.value * (acceptableVariants.Count - 1));
			acceptableVariants.RemoveAt(i);
		}
		return acceptableVariants;
	}

	public bool ReplaceStructure(SurfaceObject so) {
		if (cellsStatus == 0 || so.structure == null) return false;
		bool found = false;
		for (int i = 0; i< surfaceObjects.Count; i++) {
			if (surfaceObjects[i].structure == null) {RequestAnnihilationAtIndex(i); continue;}
			SurfaceRect sr = so.rect;
			if ( surfaceObjects[i].rect == sr ) {
				so.structure.transform.parent = transform;
				so.structure.transform.localPosition = surfaceObjects[i].structure.transform.localPosition;
				if ( surfaceObjects[i].structure.isArtificial ) artificialStructures --; if (artificialStructures < 0) artificialStructures = 0;
				Destroy(surfaceObjects[i].structure.gameObject);
				surfaceObjects[i] = so;
				if (so.structure.isArtificial) artificialStructures ++;
				surfaceObjects[i].structure.gameObject.SetActive(true);
				found = true;
			}
		}
		return found;
	}

	public bool IsAnyBuildingInArea(SurfaceRect sa) {
		if (surfaceObjects == null || surfaceObjects.Count == 0) return false;
		bool found = false;
		foreach (SurfaceObject suro in surfaceObjects) {
			if ( !suro.structure.isArtificial ) continue;
			int minX = -1, maxX = -1, minZ = -1, maxZ = -1;
			if (sa.x > suro.rect.x) minX = sa.x; else minX = suro.rect.x;
			if (sa.x + sa.x_size < suro.rect.x + suro.rect.x_size) maxX = sa.x+sa.x_size; else maxX = suro.rect.x + suro.rect.x_size;
			if (minX >= maxX) continue;
			if (sa.z > suro.rect.z) minZ = sa.z; else minZ = suro.rect.z;
			if (sa.z + sa.z_size < suro.rect.z + suro.rect.z_size ) maxZ = sa.z + sa.z_size; else maxZ = suro.rect.z + suro.rect.z_size;
			if (minZ >= maxZ) continue;
			else {found = true; break;}
		}
		return found;
	}

	public void RequestAnnihilationAtIndex(int index) {
		if (index < 0 || index >= surfaceObjects.Count) return;
		else {
			if (surfaceObjects[index].structure == null ) {
				surfaceObjects.RemoveAt(index);
				CellsStatusUpdate();
			}
		}
	}
}
