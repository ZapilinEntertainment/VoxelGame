using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SurfaceRect
{
    public byte x, z, size;
    public SurfaceRect(byte f_x, byte f_z, byte f_size)
    {
        if (f_x < 0) f_x = 0; if (f_x >= SurfaceBlock.INNER_RESOLUTION) f_x = SurfaceBlock.INNER_RESOLUTION - 1;
        if (f_z < 0) f_z = 0; if (f_z >= SurfaceBlock.INNER_RESOLUTION) f_z = SurfaceBlock.INNER_RESOLUTION - 1;
        if (f_size < 1) f_size = 1; if (f_size > SurfaceBlock.INNER_RESOLUTION) f_size = SurfaceBlock.INNER_RESOLUTION;
        x = f_x;
        z = f_z;
        size = f_size;
    }

    static SurfaceRect()
    {
        one = new SurfaceRect(0, 0, 1);
        full = new SurfaceRect(0, 0, SurfaceBlock.INNER_RESOLUTION);
    }

    public static bool operator ==(SurfaceRect lhs, SurfaceRect rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(SurfaceRect lhs, SurfaceRect rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        SurfaceRect p = (SurfaceRect)obj;
        return (x == p.x) & (z == p.z) & (size == p.size);
    }

    public override int GetHashCode()
    {
        return x + z + size;
    }
    public static readonly SurfaceRect one;
    public static readonly SurfaceRect full;
}

public class SurfaceBlock : Block
{
    public const byte INNER_RESOLUTION = 16;
    public Grassland grassland { get; protected set; }
    public List<Structure> surfaceObjects { get; protected set; }
    public sbyte cellsStatus { get; protected set; }// -1 is not stated, 1 is full, 0 is empty;
    public int artificialStructures { get; protected set; }
    public bool[,] map { get; protected set; }
    public BlockRendererController structureBlockRenderer { get; protected set; }
    public bool haveSupportingStructure { get; protected set; }

    public static UISurfacePanelController surfaceObserver;

    public SurfaceBlock (Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id) : base (f_chunk, f_chunkPos)
    {
        type = BlockType.Surface;
        material_id = f_material_id;

        cellsStatus = 0; map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++) map[i, j] = false;
        }
        surfaceObjects = new List<Structure>();
        artificialStructures = 0;      
    }

    public void SetGrassland(Grassland g) { grassland = g; }


    public bool[,] RecalculateSurface()
    {
        map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
        for (int i = 0; i < INNER_RESOLUTION; i++)
        {
            for (int j = 0; j < INNER_RESOLUTION; j++)
            {
                map[i, j] = false;
            }
        }

        haveSupportingStructure = false;
        mainStructure = null;
        artificialStructures = 0;
        cellsStatus = -1;

        bool allCellsEmpty = true;
        if (surfaceObjects.Count != 0)
        {
            int a = 0;
            while (a < surfaceObjects.Count)
            {
                Structure s = surfaceObjects[a];
                if (s == null)
                {
                    surfaceObjects.RemoveAt(a);
                    continue;
                }
                else allCellsEmpty = false;
                if (s.isArtificial) artificialStructures++;
                if (s.isBasement)
                {
                    haveSupportingStructure = true;
                    mainStructure = s;
                }
                SurfaceRect sr = s.innerPosition;
                if (sr.size != INNER_RESOLUTION)
                {
                    int i = 0, j = 0;
                    while (j < sr.size & sr.x + i < INNER_RESOLUTION)
                    {
                        while (i < sr.size & sr.z + j < INNER_RESOLUTION)
                        {
                            map[sr.x + i, sr.z + j] = true;
                            i++;
                        }
                        i = 0; // обнуляй переменные !
                        j++;
                    }
                }
                else
                {
                    for (int i = 0; i < map.Length; i++) map[i / INNER_RESOLUTION, i % INNER_RESOLUTION] = true;
                    cellsStatus = 1;
                }
                a++;
            }
        }
        if (cellsStatus == -1)
        {
            if (allCellsEmpty)
            {
                cellsStatus = 0;
            }
            else
            {
                bool allCellsFull = true;
                foreach (bool b in map)
                {
                    if (b == true)
                    {
                        allCellsFull = false;
                        break;
                    }
                }
                if (allCellsFull)
                {
                    cellsStatus = 1;
                }
                else
                {
                    cellsStatus = -1;
                }
            }
        }
        return map;
    }
    public Texture2D GetMapTexture()
    {
        int cellRes = 4;
        int realRes = INNER_RESOLUTION * cellRes;
        byte[] buildmap = new byte[realRes * realRes * 4];
        int index;
        for (int i = 0; i < buildmap.Length; i += 4)
        {
            buildmap[i] = 0;
            buildmap[i + 1] = 255;
            buildmap[i + 2] = 255;
            buildmap[i + 3] = 128;
        }
        // red axis
        buildmap[0] = 255;
        buildmap[1] = 0;
        buildmap[2] = 0;
        buildmap[3] = 255;
        for (int i = 1; i < realRes; i++)
        {
            index = i * 4;
            buildmap[index] = 255;
            buildmap[index + 1] = 0;
            buildmap[index + 2] = 0;
            buildmap[index + 3] = 255;
            if (i % cellRes == 0)
            {
                for (int j = 1; j < realRes; j++)
                {
                    buildmap[index + j * realRes * 4] = 0;
                    buildmap[index + 1 + j * realRes * 4] = 0;
                    buildmap[index + 2 + j * realRes * 4] = 0;
                    buildmap[index + 3 + j * realRes * 4] = 150;

                    buildmap[i * realRes * 4 + j * 4] = 0;
                    buildmap[i * realRes * 4 + j * 4 + 1] = 0;
                    buildmap[i * realRes * 4 + j * 4 + 2] = 0;
                    buildmap[i * realRes * 4 + j * 4 + 3] = 150;
                }
            }

            index = i * realRes * 4;
            buildmap[index] = 255;
            buildmap[index + 1] = 0;
            buildmap[index + 2] = 0;
            buildmap[index + 3] = 255;
        }
        // eo red axis

        RecalculateSurface(); // обновит данные и избавит от проверки на null
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
                for (int i = sr.x * cellRes; i < (sr.x + sr.size) * cellRes; i++)
                {
                    for (int j = sr.z * cellRes; j < (sr.z + sr.size) * cellRes; j++)
                    {
                        index = i * realRes * 4 + j * 4;
                        buildmap[index] = col[0];
                        buildmap[index + 1] = col[1];
                        buildmap[index + 2] = col[2];
                        buildmap[index + 3] = col[3];
                    }
                }
            }
        }
        Texture2D planeTex = new Texture2D(INNER_RESOLUTION * cellRes, INNER_RESOLUTION * cellRes, TextureFormat.RGBA32, false);
        planeTex.filterMode = FilterMode.Point;
        planeTex.LoadRawTextureData(buildmap);
        planeTex.Apply();
        return planeTex;
    }

    public Vector2 WorldToMapCoordinates(Vector3 point)
    {
        Vector3 leftDownCorner = GetLocalPosition(0,0);
        return new Vector2(point.x - leftDownCorner.x, point.z - leftDownCorner.z);
    }
    /// <summary>
    /// Do not use directly, use "Set Basement" instead
    /// </summary>
    /// <param name="s">S.</param>
    public void AddStructure(Structure s)
    { // with autoreplacing
        if (s == null) return;
        if (s.innerPosition.x > INNER_RESOLUTION | s.innerPosition.z > INNER_RESOLUTION)
        {
            return;
        }
        if (s.innerPosition.size == 1 && s.innerPosition.size == 1)
        {
            AddCellStructure(s, new PixelPosByte(s.innerPosition.x, s.innerPosition.z));
            return;
        }
        Structure savedBasementForNow = null;
        if (cellsStatus != 0)
        {
            SurfaceRect sr = s.innerPosition;
            int i = 0;
            if (sr == SurfaceRect.full)
            {// destroy everything there
                ClearSurface(false); // false так как не нужна лишняя проверка
            }
            else
            {
                while (i < surfaceObjects.Count)
                {
                    if (surfaceObjects[i] != null)
                    {
                        SurfaceRect a = surfaceObjects[i].innerPosition;
                        int leftX = -1, rightX = -1;
                        if (a.x > sr.x) leftX = a.x; else leftX = sr.x;
                        if (a.x + a.size > sr.x + sr.size) rightX = sr.x + sr.size; else rightX = a.x + a.size;
                        if (leftX < rightX)
                        {
                            int topZ = -1, downZ = -1;
                            if (a.z > sr.z) downZ = a.z; else downZ = sr.z;
                            if (a.z + a.size > sr.z + sr.size) topZ = sr.z + sr.size; else topZ = a.z + a.size;
                            if (topZ > downZ)
                            {
                                if (surfaceObjects[i].isBasement) savedBasementForNow = surfaceObjects[i];
                                else surfaceObjects[i].Annihilate(false);
                            }
                        }
                    }
                    i++;
                }
            }
        }
        surfaceObjects.Add(s);
        s.transform.position = GetLocalPosition(s.innerPosition);
        if (myChunk.GetVisibilityMask(pos) == 0) s.SetVisibility(false); else s.SetVisibility(true);
        s.transform.localRotation = Quaternion.Euler(0, s.modelRotation * 45, 0);
        if (savedBasementForNow != null)
        {
            savedBasementForNow.Annihilate(true);
        }
        RecalculateSurface();
    }

    /// <summary>
    /// collider check - enables surface collider, if inactive
    /// </summary>
    /// <param name="colliderCheck"></param>
	public void ClearSurface(bool check)
    {
        if (surfaceObjects.Count > 0)
        {
            for (int i = 0; i < surfaceObjects.Count; i++)
            {
                surfaceObjects[i].Annihilate(true); // чтобы не вызывали removeStructure здесь
            }
            surfaceObjects.Clear();
        }
        if (check) RecalculateSurface();
    }

    /// <summary>
    /// Do not use directly, use "Set Basement" instead
    /// </summary>
    public void AddCellStructure(Structure s, PixelPosByte ppos)
    {
        if (s == null) return;
        if (map[ppos.x, ppos.y] == true)
        {
            int i = 0;
            while (i < surfaceObjects.Count)
            {
                if (surfaceObjects[i] == null) { surfaceObjects.RemoveAt(i); continue; }
                SurfaceRect sr = surfaceObjects[i].innerPosition;
                if (sr.x <= ppos.x & sr.z <= ppos.y & sr.x + sr.size > ppos.x & sr.z + sr.size > ppos.y)
                {
                    if (surfaceObjects[i].indestructible)
                    {
                        s.Annihilate(true);
                        return;
                    }
                    else
                    {
                        surfaceObjects[i].Annihilate(true);
                        break;
                    }
                }
                i++;
            }
        }
        surfaceObjects.Add(s);
        s.transform.position = GetLocalPosition(new SurfaceRect(ppos.x, ppos.y, 1));
        s.transform.rotation = Quaternion.Euler(0, s.modelRotation * 45, 0);
        if (myChunk.GetVisibilityMask(pos) == 0) s.SetVisibility(false); else s.SetVisibility(true);
        RecalculateSurface();
    }

    /// <summary>
    /// Remove structure data from this block structures map
    /// </summary>
    public void RemoveStructure(Structure s)
    {
        int count = surfaceObjects.Count;
        if (count == 0) return;
        for (int i = 0; i < count; i++)
        {
            if (surfaceObjects[i] == s)
            {
                surfaceObjects.RemoveAt(i);
                break;
            }
        }
        RecalculateSurface();
    }

    public void TransferStructures(SurfaceBlock receiver)
    {
        if (cellsStatus == 0) return;
        else
        {
            foreach (Structure s in surfaceObjects)
            {
                if (s == null) return;
                else
                {
                    s.ChangeBasement(receiver);
                }
            }
            surfaceObjects.Clear();
            map = RecalculateSurface();
        }
    }

    public override void ReplaceMaterial(int newId)
    {
        material_id = newId;
        if (material_id != ResourceType.DIRT_ID & material_id != ResourceType.FERTILE_SOIL_ID & grassland != null)
        {
            grassland.Annihilation(false, true);
        }
        myChunk.RefreshBlockVisualising(this);
    }

    public void SetStructureBlock(BlockRendererController brc)
    {
        structureBlockRenderer = brc;
        brc.SetVisibilityMask(myChunk.GetVisibilityMask(pos));
    }
    public void ClearStructureBlock(BlockRendererController brc)
    {
        if (structureBlockRenderer == brc)
        {
            structureBlockRenderer = null;
        }
    }

    #region structures positioning   
    public Vector3 GetLocalPosition(SurfaceRect sr)
    {
        Vector3 leftBottomCorner = pos.ToWorldSpace() + new Vector3(-0.5f, 0.5f, -0.5f) * QUAD_SIZE;
        float res = INNER_RESOLUTION;
        float xpos = sr.x + sr.size / 2f;
        float zpos = sr.z + sr.size / 2f;
        return (leftBottomCorner + new Vector3((xpos / res - 0.5f) * QUAD_SIZE, -QUAD_SIZE / 2f, ((1 - zpos / res) - 0.5f) * QUAD_SIZE));
    }
    public Vector3 GetLocalPosition(byte x, byte z)
    {
        Vector3 leftBottomCorner = pos.ToWorldSpace() + new Vector3(-0.5f, 0.5f, -0.5f) * QUAD_SIZE;
        float ir = INNER_RESOLUTION, half = 1f / ir / 2f;
        return leftBottomCorner + new Vector3(x / ir + half, 0f, z / ir + half);
    }

    public PixelPosByte GetRandomCell()
    {
        if (cellsStatus == 1) return PixelPosByte.Empty;
        else
        {
            if (cellsStatus == 0) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
            else
            {
                List<PixelPosByte> acceptableVariants = GetAcceptablePositions(10);
                if (acceptableVariants.Count == 0) return PixelPosByte.Empty;
                else
                {
                    int ppos = Random.Range(0, acceptableVariants.Count);
                    return acceptableVariants[ppos];
                }
            }
        }
    }
    public List<PixelPosByte> GetRandomCells(int count)
    {
        List<PixelPosByte> positions = new List<PixelPosByte>();
        if (cellsStatus != 1)
        {
            List<PixelPosByte> acceptableVariants = GetAcceptablePositions(INNER_RESOLUTION * INNER_RESOLUTION);
            while (positions.Count < count && acceptableVariants.Count > 0)
            {
                int ppos = Random.Range(0, acceptableVariants.Count);
                positions.Add(acceptableVariants[ppos]);
                acceptableVariants.RemoveAt(ppos);
            }
        }
        return positions;
    }

    public PixelPosByte GetRandomPosition(byte xsize, byte zsize)
    {
        if (cellsStatus == 1 || xsize >= INNER_RESOLUTION || zsize >= INNER_RESOLUTION || xsize < 1 || zsize < 1) return PixelPosByte.Empty;
        if (cellsStatus == 0) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
        return GetAcceptablePosition(xsize, zsize);
    }

    PixelPosByte GetAcceptablePosition(byte xsize, byte zsize)
    {
        List<PixelPosByte> acceptablePositions = new List<PixelPosByte>();
        for (int xpos = 0; xpos <= INNER_RESOLUTION - xsize; xpos++)
        {
            int width = 0;
            for (int zpos = 0; zpos <= INNER_RESOLUTION - zsize; zpos++)
            {
                if (map[xpos, zpos] == true) width = 0; else width++;
                if (width >= zsize)
                {
                    bool appliable = true;
                    for (int xdelta = 1; xdelta < xsize; xdelta++)
                    {
                        for (int zdelta = 0; zdelta < zsize; zdelta++)
                        {
                            if (map[xpos + xdelta, zpos + zdelta] == true) { appliable = false; break; }
                        }
                        if (appliable == false) break;
                    }
                    if (appliable)
                    {
                        acceptablePositions.Add(new PixelPosByte(xpos, zpos)); width = 0;
                        for (int xdelta = 1; xdelta < xsize; xdelta++)
                        {
                            for (int zdelta = 0; zdelta < zsize; zdelta++)
                            {
                                map[xpos + xdelta, zpos + zdelta] = true;
                            }
                        }
                    }
                }
            }
        }
        if (acceptablePositions.Count > 0) return acceptablePositions[Random.Range(0, acceptablePositions.Count)];
        else return PixelPosByte.Empty;
    }

    List<PixelPosByte> GetAcceptablePositions(byte xsize, byte zsize, int maxVariants)
    {
        if (maxVariants > INNER_RESOLUTION * INNER_RESOLUTION) maxVariants = INNER_RESOLUTION * INNER_RESOLUTION;
        if (xsize > INNER_RESOLUTION | zsize > INNER_RESOLUTION | xsize <= 0 | zsize <= 0) return null;
        List<PixelPosByte> acceptablePositions = new List<PixelPosByte>();
        for (int xpos = 0; xpos <= INNER_RESOLUTION - xsize; xpos++)
        {
            int width = 0;
            for (int zpos = 0; zpos <= INNER_RESOLUTION - zsize; zpos++)
            {
                if (map[xpos, zpos] == true) width = 0; else width++;
                if (width >= zsize)
                {
                    bool appliable = true;
                    for (int xdelta = 1; xdelta < xsize; xdelta++)
                    {
                        for (int zdelta = 0; zdelta < zsize; zdelta++)
                        {
                            if (map[xpos + xdelta, zpos + zdelta] == true) { appliable = false; break; }
                        }
                        if (appliable == false) break;
                    }
                    if (appliable)
                    {
                        acceptablePositions.Add(new PixelPosByte(xpos, zpos)); width = 0;
                        for (int xdelta = 1; xdelta < xsize; xdelta++)
                        {
                            for (int zdelta = 0; zdelta < zsize; zdelta++)
                            {
                                map[xpos + xdelta, zpos + zdelta] = true;
                            }
                        }
                    }
                }
            }
        }
        while (acceptablePositions.Count > maxVariants)
        {
            int i = Random.Range(0, acceptablePositions.Count);
            acceptablePositions.RemoveAt(i);
        }
        return acceptablePositions;
    }

    public List<PixelPosByte> GetAcceptablePositions(int count)
    {
        List<PixelPosByte> acceptableVariants = new List<PixelPosByte>();
        for (byte i = 0; i < INNER_RESOLUTION; i++)
        {
            for (byte j = 0; j < INNER_RESOLUTION; j++)
            {
                if (map[i, j] == false) { acceptableVariants.Add(new PixelPosByte(i, j)); }
            }
        }
        if (acceptableVariants.Count == 0)
        {
            cellsStatus = 1;
            return new List<PixelPosByte>();
        }
        else
        {
            while (acceptableVariants.Count > count)
            {
                int i = Random.Range(0, acceptableVariants.Count);
                acceptableVariants.RemoveAt(i);
            }
            return acceptableVariants;
        }
    }

    public bool IsAnyBuildingInArea(SurfaceRect sa)
    {
        if (cellsStatus == 0) return false;
        bool found = false;
        foreach (Structure suro in surfaceObjects)
        {
            if (!suro.isArtificial) continue;
            int minX = -1, maxX = -1, minZ = -1, maxZ = -1;
            if (sa.x > suro.innerPosition.x) minX = sa.x; else minX = suro.innerPosition.x;
            if (sa.x + sa.size < suro.innerPosition.x + suro.innerPosition.size) maxX = sa.x + sa.size;
            else maxX = suro.innerPosition.x + suro.innerPosition.size;
            if (minX >= maxX) continue;
            if (sa.z > suro.innerPosition.z) minZ = sa.z; else minZ = suro.innerPosition.z;
            if (sa.z + sa.size < suro.innerPosition.z + suro.innerPosition.size) maxZ = sa.z + sa.size;
            else maxZ = suro.innerPosition.z + suro.innerPosition.size;
            if (minZ >= maxZ) continue;
            else { found = true; break; }
        }
        return found;
    }
    #endregion

    public UIObserver ShowOnGUI()
    {
        if (surfaceObserver == null)
        {
            surfaceObserver = UISurfacePanelController.InitializeSurfaceObserverScript();
        }
        else surfaceObserver.gameObject.SetActive(true);
        surfaceObserver.SetObservingSurface(this);
        return surfaceObserver;
    }

    #region save-load system
    override public void Save(System.IO.FileStream fs)
    {
        SaveBlockData(fs);
        SaveSurfaceBlockData(fs);
    }
    protected void SaveSurfaceBlockData(System.IO.FileStream fs)
    {
        if (grassland != null)
        {
            fs.WriteByte(1);
            fs.Write(grassland.Save().ToArray(), 0, Grassland.SERIALIZER_LENGTH);
        }
        else fs.WriteByte(0);

        int structuresCount = surfaceObjects.Count;
        var data = new List<byte>();
        if (structuresCount > 0)
        {
            structuresCount = 0;
            int i = 0;
            while (i < surfaceObjects.Count)
            {
                if (surfaceObjects[i] == null) surfaceObjects.RemoveAt(i);
                else
                {
                    var sdata = surfaceObjects[i].Save();
                    if (sdata != null && sdata.Count > 0)
                    {
                        data.AddRange(sdata);
                        structuresCount++;
                    }
                    i++;
                }
            }
        }
        fs.Write(System.BitConverter.GetBytes(structuresCount), 0, 4);
        if (structuresCount > 0)
        {
            var dataArray = data.ToArray();
            fs.Write(dataArray, 0, dataArray.Length);
        }
    }

    public void LoadSurfaceBlockData(System.IO.FileStream fs)
    {
        if (fs.ReadByte() == 1)
        {
            grassland = Grassland.CreateOn(this);
            grassland.Load(fs);
        }
        else grassland = null;

        var data = new byte[4];
        fs.Read(data, 0, 4);
        int structuresCount = System.BitConverter.ToInt32(data, 0);
        if (structuresCount > 0) Structure.LoadStructures(structuresCount, fs, this);
    }
    #endregion

    override public void Annihilate()
    {
        base.Annihilate();
        if (cellsStatus != 0)
        {
            ClearSurface(false);
        }
        if (grassland != null) grassland.Annihilation(true, false);
        myChunk.RemoveFromSurfacesList(this);
    }
}
