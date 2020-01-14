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

    public bool Intersect(SurfaceRect sr)
    {
        int leftX = -1, rightX = -1;
        if (x > sr.x) leftX = x; else leftX = sr.x;
        if (x + size > sr.x + sr.size) rightX = sr.x + sr.size; else rightX = x + size;
        if (leftX < rightX)
        {
            int topZ = -1, downZ = -1;
            if (z > sr.z) downZ = z; else downZ = sr.z;
            if (z + size > sr.z + sr.size) topZ = sr.z + sr.size; else topZ = z + size;
            return topZ > downZ;
        }
        else return false;
    }
    public bool Intersect(int xpos, int zpos, int xsize, int zsize)
    {
        int leftX = -1, rightX = -1;
        if (x > xpos) leftX = x; else leftX = xpos;
        if (x + size > xpos + xsize) rightX = xpos + xsize; else rightX = x + size;
        if (leftX < rightX)
        {
            int topZ = -1, downZ = -1;
            if (z > zpos) downZ = z; else downZ = zpos;
            if (z + size > zpos + zsize) topZ = zpos + zsize; else topZ = z + size;
            return topZ > downZ;
        }
        else return false;
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
    public override string ToString()
    {
        return '(' + x.ToString() + ' ' + z.ToString() +") size:" + size.ToString();
    }
}

public class SurfaceBlock : Block
{
    public const byte INNER_RESOLUTION = 16;
    public Grassland grassland { get; protected set; }
    public List<Structure> structures { get; protected set; }
    public bool? noEmptySpace { get; protected set; }// true - full, false - empty, null - not either
    public int artificialStructures { get; protected set; }
    public bool[,] map { get; protected set; }
    public BlockRendererController structureBlockRenderer { get; protected set; }
    public bool haveSupportingStructure { get; protected set; }

    public static UISurfacePanelController surfaceObserver;

    public SurfaceBlock (Chunk f_chunk, ChunkPos f_chunkPos, int f_material_id) : base (f_chunk, f_chunkPos)
    {
        type = BlockType.Surface;
        material_id = f_material_id;

        noEmptySpace = false; map = new bool[INNER_RESOLUTION, INNER_RESOLUTION];
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++) map[i, j] = false;
        }
        structures = new List<Structure>();
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
        noEmptySpace = null;

        bool allCellsEmpty = true;
        if (structures.Count != 0)
        {
            int a = 0;
            while (a < structures.Count)
            {
                Structure s = structures[a];
                if (s == null)
                {
                    structures.RemoveAt(a);
                    continue;
                }
                else allCellsEmpty = false;
                if (s.isArtificial) artificialStructures++;
                if (s.isBasement)
                {
                    haveSupportingStructure = true;
                    mainStructure = s;
                }
                SurfaceRect sr = s.surfaceRect;
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
                    noEmptySpace = true;
                }
                a++;
            }
        }
        if (noEmptySpace == null)
        {
            if (allCellsEmpty)
            {
                noEmptySpace = false;
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
                    noEmptySpace = true;
                }
                else
                {
                    noEmptySpace = null;
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
        if (noEmptySpace != false)
        {
            foreach (Structure s in structures)
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
                SurfaceRect sr = s.surfaceRect;
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
    /// <summary>
    /// Do not use directly, use "Set Basement" instead
    /// </summary>
    /// <param name="s">S.</param>
    public void AddStructure(Structure s)
    { // with autoreplacing
        if (s == null) return;
        if (s.surfaceRect.x > INNER_RESOLUTION | s.surfaceRect.z > INNER_RESOLUTION)
        {
            return;
        }
        if (s.surfaceRect.size == 1 && s.surfaceRect.size == 1)
        {
            AddCellStructure(s, new PixelPosByte(s.surfaceRect.x, s.surfaceRect.z));
            return;
        }
        Structure savedBasementForNow = null;
        if (noEmptySpace != false)
        {
            SurfaceRect sr = s.surfaceRect;
            int i = 0;
            if (sr == SurfaceRect.full)
            {// destroy everything there
                ClearSurface(false, true); // false так как не нужна лишняя проверка
            }
            else
            {
                while (i < structures.Count)
                {
                    if (structures[i] != null)
                    {
                        if (structures[i].surfaceRect.Intersect(sr))
                        {
                            if (structures[i].isBasement) savedBasementForNow = structures[i];
                            else structures[i].Annihilate(false, true, false);
                        }
                    }
                    i++;
                }
            }
        }
        structures.Add(s);
        s.transform.parent = myChunk.transform;
        s.transform.position = GetLocalPosition(s.surfaceRect);
        if (myChunk.GetVisibilityMask(pos) == 0) s.SetVisibility(false); else s.SetVisibility(true);
        s.transform.localRotation = Quaternion.Euler(0, s.modelRotation * 45, 0);
        if (savedBasementForNow != null)
        {
            savedBasementForNow.Annihilate(false, true, false);
        }
        RecalculateSurface();
    }

    /// <summary>
    /// collider check - enables surface collider, if inactive
    /// </summary>
    /// <param name="colliderCheck"></param>
	public void ClearSurface(bool check, bool returnResources)
    {
        if (structures.Count > 0)
        {
            for (int i = 0; i < structures.Count; i++)
            {
                structures[i].Annihilate(false, returnResources, false); // чтобы не вызывали removeStructure здесь
            }
            structures.Clear();
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
            while (i < structures.Count)
            {
                if (structures[i] == null) { structures.RemoveAt(i); continue; }
                SurfaceRect sr = structures[i].surfaceRect;
                if (sr.x <= ppos.x & sr.z <= ppos.y & sr.x + sr.size > ppos.x & sr.z + sr.size > ppos.y)
                {
                    if (structures[i].indestructible)
                    {
                        s.Annihilate(false, false, false);
                        return;
                    }
                    else
                    {
                        structures[i].Annihilate(false, false, false);
                        break;
                    }
                }
                i++;
            }
        }
        structures.Add(s);
        s.transform.position = GetLocalPosition(new SurfaceRect(ppos.x, ppos.y, 1));
        s.transform.rotation = Quaternion.Euler(0, s.modelRotation * 45, 0);
        if (myChunk.GetVisibilityMask(pos) == 0) s.SetVisibility(false); else s.SetVisibility(true);
        RecalculateSurface();
    }   

    public override void ReplaceMaterial(int newId)
    {
        material_id = newId;
        if (grassland != null) grassland.Annihilation(false, false);
        myChunk.ChangeBlockVisualData(this, 6);
    }
    public void ReplaceGrassTexture(int id)
    {
        if (grassland != null)
        {
            material_id = id;
            myChunk.ChangeBlockVisualData(this, 6);
        }
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
    /// <summary>
    /// Remove structure data from this block structures map
    /// </summary>
    public void RemoveStructure(Structure s)
    {
        int count = structures.Count;
        if (count == 0) return;
        for (int i = 0; i < count; i++)
        {
            if (structures[i] == s)
            {
                structures.RemoveAt(i);
                break;
            }
        }
        RecalculateSurface();
    }
    public void TransferStructures(SurfaceBlock receiver)
    {
        if (noEmptySpace == false) return;
        else
        {
            foreach (Structure s in structures)
            {
                if (s == null) return;
                else
                {
                    s.ChangeBasement(receiver);
                }
            }
            structures.Clear();
            map = RecalculateSurface();
        }
    }

    override public List<BlockpartVisualizeInfo> GetVisualDataList(byte visibilityMask)
    {
        if ((visibilityMask & 64) != 0) return new List<BlockpartVisualizeInfo>() { GetFaceVisualData(6) };
        else return null;
    }
    override public BlockpartVisualizeInfo GetFaceVisualData(byte face)
    {
        if (face == 6) return new BlockpartVisualizeInfo(
            pos,
            new MeshVisualizeInfo(6, PoolMaster.GetMaterialType(material_id), myChunk.GetLightValue(pos)),
            MeshType.Quad,
            material_id
            );
        else return null;
    }

    public Vector3 GetLocalPosition(SurfaceRect sr)
    {
        Vector3 leftBottomCorner = pos.ToWorldSpace() + new Vector3(-0.5f, -0.5f, -0.5f) * QUAD_SIZE;
        float res = INNER_RESOLUTION;
        float xpos = sr.x + sr.size / 2f;
        float zpos = sr.z + sr.size / 2f;
        return (leftBottomCorner + new Vector3((xpos / res) * QUAD_SIZE, 0f, (1 - zpos / res) * QUAD_SIZE));
    }
    public Vector3 GetLocalPosition(byte x, byte z)
    {
        Vector3 leftBottomCorner = pos.ToWorldSpace() + new Vector3(-0.5f, -0.5f, -0.5f) * QUAD_SIZE;
        float ir = INNER_RESOLUTION, half = 1f / ir / 2f;
        return leftBottomCorner + new Vector3(x / ir + half, 0f, z / ir + half);
    }
    public Vector2 WorldToMapCoordinates(Vector3 point)
    {
        Vector3 leftDownCorner = GetLocalPosition(0, 0) - new Vector3(0.5f, 0, 0.5f) * QUAD_SIZE / (float)INNER_RESOLUTION;
        return new Vector2(point.x - leftDownCorner.x, QUAD_SIZE - (point.z - leftDownCorner.z)) / QUAD_SIZE;
    }

    public void EnvironmentalStrike(Vector3 hitpoint, byte radius, float damage)
    {
        if (noEmptySpace == false) return;
        else
        {
            if (noEmptySpace == true & structures.Count == 1)
            {
                structures[0].ApplyDamage(damage);
                return;
            }
            else
            {
                Vector2 inpos = WorldToMapCoordinates(hitpoint);
                byte xpos = (byte)(inpos.x * INNER_RESOLUTION),
                    ypos = (byte)(inpos.y * INNER_RESOLUTION);
                if (radius > 1)
                {
                    int x0 = xpos - radius,
                        z0 = ypos - radius,
                        x1 = xpos + radius,
                        z1 = ypos + radius;
                    if (x0 < 0) x0 = 0;
                    if (z0 < 0) z0 = 0;
                    if (x1 >= INNER_RESOLUTION) x1 = INNER_RESOLUTION - 1;
                    if (z1 >= INNER_RESOLUTION) z1 = INNER_RESOLUTION - 1;
                    List<Structure> strs = new List<Structure>();
                    int i = 0;
                    Structure s = null;
                    while (i < structures.Count)
                    {
                        s = structures[i];
                        if (s == null)
                        {
                            structures.RemoveAt(i);
                            continue;
                        }
                        else
                        {
                            if (s.surfaceRect.Intersect(x0, z0, x1 - x0, z1 - z0))
                            {
                                if (s.ID == Structure.PLANT_ID) (s as Plant).Dry();
                                else s.ApplyDamage(damage);
                            }
                            if (s != null) i++;
                        }
                    }
                }
            }
        }
    }

    #region structures positioning     

    public PixelPosByte GetRandomCell()
    {
        if (noEmptySpace == true) return PixelPosByte.Empty;
        else
        {
            if (noEmptySpace == false) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
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
        if (noEmptySpace != true)
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

    public PixelPosByte GetRandomPosition(byte size)
    {
        if (noEmptySpace == true || size >= INNER_RESOLUTION || size < 1) return PixelPosByte.Empty;
        if (noEmptySpace == false) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
        else return GetAcceptablePosition(size);
    }

    PixelPosByte GetAcceptablePosition(byte size)
    {
        List<PixelPosByte> acceptablePositions = new List<PixelPosByte>();
        for (int xpos = 0; xpos <= INNER_RESOLUTION - size; xpos++)
        {
            int width = 0;
            for (int zpos = 0; zpos <= INNER_RESOLUTION - size; zpos++)
            {
                if (map[xpos, zpos] == true) width = 0; else width++;
                if (width >= size)
                {
                    bool appliable = true;
                    for (int xdelta = 1; xdelta < size; xdelta++)
                    {
                        for (int zdelta = 0; zdelta < size; zdelta++)
                        {
                            if (map[xpos + xdelta, zpos + zdelta] == true) { appliable = false; break; }
                        }
                        if (appliable == false) break;
                    }
                    if (appliable)
                    {
                        acceptablePositions.Add(new PixelPosByte(xpos, zpos)); width = 0;
                        for (int xdelta = 1; xdelta < size; xdelta++)
                        {
                            for (int zdelta = 0; zdelta < size; zdelta++)
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
            noEmptySpace = true;
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
        if (noEmptySpace == false) return false;
        bool found = false;
        foreach (Structure suro in structures)
        {
            if (!suro.isArtificial) continue;
            int minX = -1, maxX = -1, minZ = -1, maxZ = -1;
            if (sa.x > suro.surfaceRect.x) minX = sa.x; else minX = suro.surfaceRect.x;
            if (sa.x + sa.size < suro.surfaceRect.x + suro.surfaceRect.size) maxX = sa.x + sa.size;
            else maxX = suro.surfaceRect.x + suro.surfaceRect.size;
            if (minX >= maxX) continue;
            if (sa.z > suro.surfaceRect.z) minZ = sa.z; else minZ = suro.surfaceRect.z;
            if (sa.z + sa.size < suro.surfaceRect.z + suro.surfaceRect.size) maxZ = sa.z + sa.size;
            else maxZ = suro.surfaceRect.z + suro.surfaceRect.size;
            if (minZ >= maxZ) continue;
            else { found = true; break; }
        }
        return found;
    }
    public Structure GetBuildingByHitpoint(Vector3 point)
    {
        if (noEmptySpace == false) return null;
        else
        {
            var p = WorldToMapCoordinates(point);
            if (p.x < 0 | p.x > 1 | p.y < 0 | p.y < 1) return null;
            else
            {
                byte xpos = (byte)(p.x * INNER_RESOLUTION), zpos = (byte)(p.y * INNER_RESOLUTION);
                if (map[xpos, zpos] == false) return null;
                else
                {
                    SurfaceRect sr;
                    foreach (var s in structures)
                    {
                        sr = s.surfaceRect;
                        if (xpos >= sr.x & xpos <= sr.x + sr.size)
                        {
                            if (zpos >= sr.z & zpos <= sr.z + sr.size)
                            {
                                return s;
                            }
                        }
                    }
                    return null;
                }
            }
        }
    }

    public int ScatterResources(SurfaceRect fill_rect, ResourceType rtype, int volume)
    {
        if (volume == 0 | fill_rect.size == 0) return volume;
        else
        {
            // ScalableHarvestableResource stick rect size == 2
            int rowcount = fill_rect.size / 2;
            float rowCount_f = rowcount;
            byte val = 0, limitVal = ScalableHarvestableResource.MAX_STICK_VOLUME, minVal = ScalableHarvestableResource.RESOURCES_PER_LEVEL;

            float maxStickVolume = volume; maxStickVolume /= fill_rect.size; maxStickVolume /= fill_rect.size;
            if (maxStickVolume < limitVal) maxStickVolume = limitVal;
            int x0, z0;

            ScalableHarvestableResource scr = null;
            for (int x = 0; x < rowcount; x++)
            {
                for (int z = 0; z < rowcount; z++)
                {
                    if (volume <= minVal) goto ENDCYCLE;
                    x0 = fill_rect.x + x * 2;
                    z0 = fill_rect.z + z * 2;
                    val = (byte)(Mathf.PerlinNoise(x / rowCount_f, z / rowCount_f) * maxStickVolume);
                    if (val < minVal) val = minVal;
                    else
                    {
                        if (val > limitVal) val = limitVal;
                    }

                    if (!map[x0, z0] & !map[x0 + 1, z0] & !map[x0, z0 + 1] & !map[x0 + 1, z0 + 1])
                    {
                        if (val < volume)
                        {
                            scr = ScalableHarvestableResource.Create(rtype, val, this, new PixelPosByte(x0, z0));
                            volume -= val;
                        }
                        else
                        {
                            scr = ScalableHarvestableResource.Create(rtype, (byte)volume, this, new PixelPosByte(x0, z0));
                            volume = 0;
                        }
                    }
                }
            }            
        }
        ENDCYCLE:
        return volume;
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

        int structuresCount = structures.Count;
        var data = new List<byte>();
        if (structuresCount > 0)
        {
            structuresCount = 0;
            int i = 0;
            while (i < structures.Count)
            {
                if (structures[i] == null) structures.RemoveAt(i);
                else
                {
                    var sdata = structures[i].Save();
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
        if (structuresCount > INNER_RESOLUTION * INNER_RESOLUTION | structuresCount < 0)
        {
            Debug.Log("surface block load error - incorrect structures count");
            GameMaster.LoadingFail();
            return;
        }
        if (structuresCount > 0) Structure.LoadStructures(structuresCount, fs, this);
    }
    #endregion

    override public void Annihilate()
    {
        base.Annihilate();
        if (noEmptySpace != false)
        {
            ClearSurface(false, false);
        }
        if (grassland != null) grassland.Annihilation(true, false);
        myChunk.RemoveFromSurfacesList(this);
    }
}
