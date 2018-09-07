using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

	static SurfaceRect() {
		one = new SurfaceRect(0,0,1,1);
		full = new SurfaceRect(0,0, SurfaceBlock.INNER_RESOLUTION, SurfaceBlock.INNER_RESOLUTION);
	}

	public static bool operator ==(SurfaceRect lhs, SurfaceRect rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(SurfaceRect lhs, SurfaceRect rhs) {return !(lhs.Equals(rhs));}
	public override bool Equals(object obj) 
	{
		// Check for null values and compare run-time types.
		if (obj == null || GetType() != obj.GetType()) 
			return false;

		SurfaceRect p = (SurfaceRect)obj;
		return (x == p.x) && (z == p.z) && (x_size == p.x_size) && (z_size == p.z_size);
	}

	public override int GetHashCode()
	{ 
		return x + z + x_size + z_size;
	}
	public static readonly SurfaceRect one;
	public static readonly SurfaceRect full;
}

public class SurfaceBlock : Block {
	public const byte INNER_RESOLUTION = 16;
	public MeshRenderer surfaceRenderer {get;protected set;}
	public Grassland grassland{get;protected set;}
	public List<Structure> surfaceObjects{get;protected set;}
	public sbyte cellsStatus {get;protected set;}// -1 is not stated, 1 is full, 0 is empty;
	public int artificialStructures = 0;
	public bool[,] map { get; protected set; }
	public BlockRendererController structureBlock;
	public int freeCells = 0;
    public Texture saveTex;

	public static UISurfacePanelController surfaceObserver;

    public void SurfaceBlockSet(Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id)
    {
        if (firstSet)
        {
            cellsStatus = 0; map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++) map[i, j] = false;
            }
            material_id = 0;
            surfaceObjects = new List<Structure>();
            artificialStructures = 0;
            isTransparent = false;            
            type = BlockType.Surface;
            personalNumber = lastUsedNumber++;
            firstSet = false;
        }
        myChunk = f_chunk;

        if (model == null)
        {
            model = new GameObject();
            model.transform.parent = f_chunk.transform;
        }
        if (surfaceRenderer == null)
        {
            GameObject g = Object.Instantiate(PoolMaster.quad_pref);
            surfaceRenderer = g.GetComponent<MeshRenderer>();
            surfaceRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            g.transform.parent = model.transform;
            g.transform.localPosition = new Vector3(0, -QUAD_SIZE / 2f, 0);
            g.transform.localRotation = Quaternion.Euler(90, 0, 0);
            g.name = "upper_plane";
            g.tag = "BlockCollider";
        }
        material_id = f_material_id;
        surfaceRenderer.sharedMaterial = ResourceType.GetMaterialById(material_id, surfaceRenderer.GetComponent<MeshFilter>());
        
        if (visibilityMask != 0) surfaceRenderer.enabled = true;       
       
        pos = f_chunkPos;
        model.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
        model.transform.localRotation = Quaternion.Euler(Vector3.zero);
        model.name = "block " + pos.x.ToString() + ';' + pos.y.ToString() + ';' + pos.z.ToString();
    }

    public void SetGrassland(Grassland g) { grassland = g; }


	public bool[,] GetBooleanMap() {
			map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
			for (int i =0; i < INNER_RESOLUTION; i++) {
            for (int j = 0; j < INNER_RESOLUTION; j++)
            {
                map[i, j] = false;
            }
			}
			if (surfaceObjects.Count != 0) {
				int a = 0;
				while (a < surfaceObjects.Count) {
					SurfaceRect sr = surfaceObjects[a].innerPosition;	
				//if (sr.x_size != 1 && sr.z_size != 1) print (surfaceObjects[a].name+ ' '+ sr.x_size.ToString() + ' ' + sr.z_size.ToString());
					int i = 0, j=0;
					while ( j < sr.z_size ) {
						while (i < sr.x_size ) {
								map[ sr.x + i, sr.z + j ] = true;
								i++;
							}
						i = 0; // обнуляй переменные !
						j++;
					}
						a++;
				}
			}
			freeCells = 0;
			for (int i =0; i < INNER_RESOLUTION; i++) {
				for (int j =0; j< INNER_RESOLUTION; j++) {
					if (map[i,j] == false) freeCells ++;
				}
			}
		return map;
	}

    public Texture2D GetMapTexture()
    {
        byte[] buildmap = new byte[INNER_RESOLUTION * INNER_RESOLUTION * 4];
        for (int i = 0; i < buildmap.Length; i += 4)
        {
            buildmap[i] = 0;
            buildmap[i + 1] = 255;
            buildmap[i + 2] = 255;
            buildmap[i + 3] = 128; // cyan
        }
        GetBooleanMap(); // обновит данные и избавит от проверки на null
        if (cellsStatus != 0)
        {
            foreach (Structure s in surfaceObjects)
            {
                byte[] col;
                if (s is Plant) col = new byte[4] { 0, 255, 0, 255 };
                else
                {
                    if (s is HarvestableResource | s is ScalableHarvestableResource) col = new byte[4] { 255, 106, 0, 255 };
                    else
                    {
                        Building bd = s as Building;
                        if (bd != null)
                        {
                            if (bd.placeInCenter) col = new byte[4] { 255, 255, 255, 255 };
                            else col = new byte[4] { 64, 64, 64, 255 };
                        }
                        else col = new byte[4] { 128, 128, 128, 255 };
                    }
                }
                SurfaceRect sr = s.innerPosition;
                for (int i = sr.x; i < sr.x + sr.x_size; i++)
                {
                    for (int j = sr.z; j < sr.z + sr.z_size; j++)
                    {
                        int index = i * INNER_RESOLUTION * 4 + j * 4;
                        buildmap[index] = col[0];
                        buildmap[index + 1] = col[1];
                        buildmap[index + 2] = col[2];
                        buildmap[index + 3] = col[3];
                    }
                }
            }
        }
        Texture2D planeTex = new Texture2D(INNER_RESOLUTION, INNER_RESOLUTION, TextureFormat.RGBA32, false);
        planeTex.filterMode = FilterMode.Point;
        planeTex.LoadRawTextureData(buildmap);
        planeTex.Apply();
        saveTex = planeTex;
        return planeTex;
    }
    public Vector2 WorldToMapCoordinates(Vector3 point)
    {
        if (model == null) return Vector2.zero;
        point = model.transform.InverseTransformPoint(point);
        return new Vector2(point.x / QUAD_SIZE + 0.5f, 0.5f - point.z / QUAD_SIZE );
    }

	public void CellsStatusUpdate() {
		map = GetBooleanMap();
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
	/// <summary>
	/// Do not use directly, use "Set Basement" instead
	/// </summary>
	/// <param name="s">S.</param>
	public void AddStructure(Structure s) { // with autoreplacing
		if (s == null ) return;
		if (s.innerPosition.x > INNER_RESOLUTION | s.innerPosition.z > INNER_RESOLUTION  ) {
			MonoBehaviour.print ("error in structure size");
			return;
		}
		if (s.innerPosition.x_size == 1 && s.innerPosition.z_size == 1) {
			AddCellStructure(s, new PixelPosByte(s.innerPosition.x, s.innerPosition.z)); 
			return;
		}
		Structure savedBasementForNow = null;
		if (cellsStatus != 0) { 
			SurfaceRect sr = s.innerPosition;
			int i =0;
			if (sr == SurfaceRect.full) { // destroy everything there
                                          //print("fullscale");
                ClearSurface(false); // false так как не нужна лишняя установка коллайдера
				surfaceRenderer.GetComponent<Collider>().enabled = false;
			}
			else {
				while (i < surfaceObjects.Count) {
					if ( surfaceObjects[i] == null ) {surfaceObjects.RemoveAt(i); continue;}
					SurfaceRect a = surfaceObjects[i].innerPosition;
					int leftX = -1, rightX = -1;
					if (a.x > sr.x) leftX = a.x; else leftX = sr.x;
					if (a.x + a.x_size > sr.x + sr.x_size) rightX = sr.x + sr.x_size; else rightX = a.x + a.x_size;
					if (leftX >= rightX) {i++;continue;}
					int topZ = -1, downZ = -1;
					if (a.z > sr.z) downZ = a.z; else downZ = sr.z;
					if (a.z + a.z_size > sr.z + sr.z_size) topZ = sr.z + sr.z_size; else topZ = a.z + a.z_size;
					if (topZ <= downZ) {i++;continue;}
					else {
						if (surfaceObjects[i].isBasement) savedBasementForNow = surfaceObjects[i];
						else surfaceObjects[i].Annihilate( true );
						i++;
					}
				}
			}
		}
		surfaceObjects.Add(s);
		s.model.transform.parent = model.transform;
		s.model.transform.localPosition = GetLocalPosition(s.innerPosition);
		if (visibilityMask == 0) s.SetVisibility(false); else s.SetVisibility(true);
		BlockRendererController brc =  s.model.GetComponent<BlockRendererController>();
		if (brc != null) {
			structureBlock = brc;
			brc.SetRenderBitmask(renderMask);
			brc.SetVisibilityMask(visibilityMask);
		}
        else
        {
            s.model.transform.localRotation = Quaternion.Euler(0, s.modelRotation * 45, 0);
        }
		if (s.isArtificial) artificialStructures++;
		CellsStatusUpdate();
		if (savedBasementForNow != null) {
			savedBasementForNow.Annihilate(true);
		}
	}

    /// <summary>
    /// collider check - enables surface collider, if inactive
    /// </summary>
    /// <param name="colliderCheck"></param>
	public void ClearSurface(bool colliderCheck) {
		if (surfaceObjects == null) return;
        // is basement check and special conditions?
		int i =0;
		while ( i < surfaceObjects.Count) {
			if (surfaceObjects[i] != null) surfaceObjects[i].Annihilate(true);
            i++;
		}
        surfaceObjects.Clear();
		cellsStatus = 0; artificialStructures = 0;
		i = 0;
		map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
		for (; i < INNER_RESOLUTION; i++) {
			for (int j = 0; j < INNER_RESOLUTION; j++) {
				map[i,j] = false;
			}
		}
        if (colliderCheck) surfaceRenderer.GetComponent<MeshCollider>().enabled = true;        
		structureBlock = null;
	}

	/// <summary>
	/// Do not use directly, use "Set Basement" instead
	/// </summary>
	/// <param name="s">S.</param>
	/// <param name="pos">Position.</param>
	public void AddCellStructure(Structure s, PixelPosByte pos) { 
		if (s == null) return;
		if (map[pos.x, pos.y] == true) {
			int i = 0;
			while ( i < surfaceObjects.Count ) {
				if ( surfaceObjects[i] == null) {surfaceObjects.RemoveAt(i); continue;}
				SurfaceRect sr = surfaceObjects[i].innerPosition;
				if (sr.x <= pos.x && sr.z <= pos.y && sr.x + sr.x_size >= pos.x && sr.z+ sr.z_size >= pos.y) {
					if ( surfaceObjects[i].undestructible)
					{	
						s.Annihilate( true);
						return;
					}
					else {
						surfaceObjects[i].Annihilate( true );
						break; 
					}
				}
				i++;
			}
		}
		surfaceObjects.Add(s);
		s.model.transform.parent = model.transform;
		s.model.transform.localPosition = GetLocalPosition(new SurfaceRect(pos.x, pos.y, 1, 1));
        s.model.transform.localRotation = Quaternion.Euler(0, s.modelRotation * 45, 0);
        if ( visibilityMask == 0 ) s.SetVisibility(false); else s.SetVisibility(true);
		if (s.isArtificial) artificialStructures++;
		CellsStatusUpdate();
	}

	/// <summary>
	/// Remove structure data from this block structures map
	/// </summary>
	/// <param name="so">So.</param>
	public void RemoveStructure(Structure s) {
        int count = surfaceObjects.Count;
		if (count == 0) return;
        for ( int i = 0; i < count; i++) {
			if (surfaceObjects[i] == s) {
				if (s.isArtificial) artificialStructures--;	if (artificialStructures < 0) artificialStructures = 0;
				surfaceObjects.RemoveAt(i);
				if (surfaceObjects.Count == 0) {
					cellsStatus = 0;
					if (s.innerPosition == SurfaceRect.full) {
						surfaceRenderer.GetComponent<Collider>().enabled = true;
					}
				}
				else CellsStatusUpdate();
				break;
			}
		}
	}

	public override void ReplaceMaterial( int newId) {
		material_id = newId;
		if (grassland != null) {
			grassland.Annihilation();
			CellsStatusUpdate();
		}
		surfaceRenderer.sharedMaterial =  ResourceType.GetMaterialById(newId, surfaceRenderer.GetComponent<MeshFilter>());
	}


		
	public static Vector3 GetLocalPosition(SurfaceRect sr) {
		float res = INNER_RESOLUTION;
		float xpos = sr.x + sr.x_size/2f ;
		float zpos = sr.z + sr.z_size/2f;
		return( new Vector3((xpos / res - 0.5f) * QUAD_SIZE , -QUAD_SIZE/2f, ((1 -zpos / res) - 0.5f)* QUAD_SIZE));
	}

	public PixelPosByte GetRandomCell() {
		if (cellsStatus == 1) return PixelPosByte.Empty;
		else {
			if (cellsStatus == 0) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
			else {
				List<PixelPosByte> acceptableVariants = GetAcceptablePositions(10);
				int ppos = (int)(GameMaster.randomizer.NextDouble() * (acceptableVariants.Count - 1));
				return acceptableVariants[ppos];
			}
		}
	}
	public List<PixelPosByte> GetRandomCells (int count) {
		List<PixelPosByte> positions = new List<PixelPosByte>();
		if (cellsStatus != 1)  {
			List<PixelPosByte> acceptableVariants = GetAcceptablePositions(INNER_RESOLUTION * INNER_RESOLUTION);
			while (positions.Count < count && acceptableVariants.Count > 0) {
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
		if (acceptablePositions.Count > 0)	return acceptablePositions[(int)(Random.value * (acceptablePositions.Count - 1))];
		else return PixelPosByte.Empty;
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
		List<PixelPosByte> acceptableVariants = new List<PixelPosByte>();
		for (byte i = 0; i< INNER_RESOLUTION; i++) {
			for (byte j =0; j < INNER_RESOLUTION; j++) {
				if (map[i,j] == false) {acceptableVariants.Add(new PixelPosByte(i,j)); }
			}	
		}
		while (acceptableVariants.Count > count) {
			int i = (int)(GameMaster.randomizer.NextDouble() * (acceptableVariants.Count - 1));
			acceptableVariants.RemoveAt(i);
		}
		return acceptableVariants;
	}


	public bool IsAnyBuildingInArea(SurfaceRect sa) {
		if (surfaceObjects == null || surfaceObjects.Count == 0) return false;
		bool found = false;
		foreach (Structure suro in surfaceObjects) {
			if ( !suro.isArtificial ) continue;
			int minX = -1, maxX = -1, minZ = -1, maxZ = -1;
			if (sa.x > suro.innerPosition.x) minX = sa.x; else minX = suro.innerPosition.x;
			if (sa.x + sa.x_size < suro.innerPosition.x + suro.innerPosition.x_size) maxX = sa.x+sa.x_size; 
			else maxX = suro.innerPosition.x + suro.innerPosition.x_size;
			if (minX >= maxX) continue;
			if (sa.z > suro.innerPosition.z) minZ = sa.z; else minZ = suro.innerPosition.z;
			if (sa.z + sa.z_size < suro.innerPosition.z + suro.innerPosition.z_size ) maxZ = sa.z + sa.z_size; 
			else maxZ = suro.innerPosition.z + suro.innerPosition.z_size;
			if (minZ >= maxZ) continue;
			else {found = true; break;}
		}
		return found;
	}

	override public void SetRenderBitmask(byte x) {
		if (renderMask != x) {
			renderMask = x;
			if ( visibilityMask != 0 && structureBlock != null) structureBlock.SetRenderBitmask(x); 
			if ((renderMask & 16 & visibilityMask) == 0) surfaceRenderer.enabled = false;
			else surfaceRenderer.enabled = true;
		}
	}

	override public void SetVisibilityMask (byte x) {
		if (visibilityMask != x) {
			byte prevVisibility = visibilityMask;
			visibilityMask = x;
			if (visibilityMask == 0) {
				surfaceRenderer.GetComponent<Collider>().enabled = false;
				surfaceRenderer.enabled = false;
				int i = 0;
				while ( i < surfaceObjects.Count ) {
					surfaceObjects[i].SetVisibility(false);
					i++;
				} 
			}
			else {
				if ( renderMask != 0 && structureBlock != null) structureBlock.SetRenderBitmask(x); 
				if ((renderMask & 16 & visibilityMask) == 0) surfaceRenderer.enabled = false;
				else surfaceRenderer.enabled = true;
				if ( prevVisibility == 0) {
					surfaceRenderer.enabled = true;
					surfaceRenderer.GetComponent<Collider>().enabled = true;
					int i = 0; 
					while ( i < surfaceObjects.Count ) {
						surfaceObjects[i].SetVisibility(true);
						i++;
					} 
				}
			}
		}
	}

	public UIObserver ShowOnGUI() {
		if (surfaceObserver == null) {
			surfaceObserver = Object.Instantiate(Resources.Load<GameObject>("UIPrefs/surfaceObserver"), UIController.current.transform).GetComponent<UISurfacePanelController>();
		}
		else surfaceObserver.gameObject.SetActive(true);
		surfaceObserver.SetObservingSurface(this);
		return surfaceObserver;
	}

	#region save-load system
	override public BlockSerializer Save() {
		BlockSerializer bs = GetBlockSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetSurfaceBlockSerializer());
			bs.specificData =  stream.ToArray();
		}
		return bs;
	} 

	override public void Load(BlockSerializer bs) {
		LoadBlockData(bs);
		SurfaceBlockSerializer sbs = new SurfaceBlockSerializer();
		GameMaster.DeserializeByteArray<SurfaceBlockSerializer>(bs.specificData, ref sbs);
		LoadSurfaceBlockData(sbs);
		if (sbs.haveStructures) {
			foreach (StructureSerializer ss in sbs.structuresList) {
				Structure s = Structure.GetStructureByID(ss.id);
				if (s!=null) s.Load(ss,this);
			}
		}
	}

	protected void LoadSurfaceBlockData(SurfaceBlockSerializer sbs) {
		if (sbs.haveGrassland) {
            grassland = Grassland.Create(this);
			grassland.Load(sbs.grasslandSerializer);
		}
	}

	public SurfaceBlockSerializer GetSurfaceBlockSerializer() {
		SurfaceBlockSerializer sbs = new SurfaceBlockSerializer();
		if (grassland != null) {
			sbs.haveGrassland = true; 
			sbs.grasslandSerializer = grassland.Save();
		}
		else sbs.haveGrassland = false;
		if (surfaceObjects.Count != 0) {
			sbs.haveStructures = true;
			sbs.structuresList = new List<StructureSerializer>();
			int realCount = 0;
			foreach (Structure s in surfaceObjects) {
				if (s == null) continue;
				StructureSerializer ss = s.Save();
				if (ss == null) continue;
				sbs.structuresList.Add(ss);
				realCount++;
			}
			if (realCount == 0) sbs.haveStructures = false;
		}
		else sbs.haveStructures = false;
		return sbs;
	}
	#endregion
}

[System.Serializable]
public class SurfaceBlockSerializer {
	public bool haveGrassland;
	public GrasslandSerializer grasslandSerializer;
	public bool haveStructures;
	public List<StructureSerializer> structuresList;
}
