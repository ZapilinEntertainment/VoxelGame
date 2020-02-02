using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public sealed class PlaneExtension
{    
    private readonly Plane myPlane;
    private List<Structure> structures;
    public FullfillStatus fullfillStatus;
    public int artificialStructuresCount { get; private set; }
    private BitArray map;
    public const byte INNER_RESOLUTION = 16;

    public static UISurfacePanelController surfaceObserver;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        PlaneExtension pe = (PlaneExtension)obj;
        return pe.myPlane == myPlane && map == pe.map;
    }
    public override int GetHashCode()
    {
        return myPlane.GetHashCode() + map.GetHashCode() + artificialStructuresCount;
    }

    public PlaneExtension(Plane i_myPlane, Structure i_mainStructure)
    {        
        ResetMap();
        artificialStructuresCount = 0;
        myPlane = i_myPlane;
        if (i_mainStructure != null) AddStructure(i_mainStructure);
    }
    private void ResetMap()
    {
        map = new BitArray(INNER_RESOLUTION * INNER_RESOLUTION);
        map.SetAll(false);
    }

    public void RecalculateSurface()
    {
        ResetMap();
        artificialStructuresCount = 0;
        fullfillStatus = FullfillStatus.Empty;

        bool allCellsEmpty = true;
        if (structures != null)
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
                if (s.isArtificial) artificialStructuresCount++;
                SurfaceRect sr = s.surfaceRect;
                if (sr.size != INNER_RESOLUTION)
                {
                    int i = 0, j = 0;
                    while (j < sr.size & sr.x + i < INNER_RESOLUTION)
                    {
                        while (i < sr.size & sr.z + j < INNER_RESOLUTION)
                        {
                            map.Set((sr.x + i) * INNER_RESOLUTION + sr.z + j, true);
                            i++;
                        }
                        i = 0; // обнуляй переменные !
                        j++;
                    }
                }
                else
                {
                    map.SetAll(true);
                    fullfillStatus = FullfillStatus.Full;
                }
                a++;
            }
            if (structures.Count == 0) structures = null;
        }
        if (fullfillStatus == FullfillStatus.Unknown)
        {
            if (allCellsEmpty)
            {
                fullfillStatus = FullfillStatus.Empty;
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
                    fullfillStatus = FullfillStatus.Full;
                }
                else
                {
                    fullfillStatus = FullfillStatus.Unknown;
                }
            }
        }
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
        if (fullfillStatus != FullfillStatus.Empty)
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
            AddCellStructure(s);
            return;
        }
        Structure savedBasementForNow = null;
        if (fullfillStatus != FullfillStatus.Empty)
        {
            SurfaceRect sr = s.surfaceRect;
            int i = 0;
            if (sr == SurfaceRect.full)
            {// destroy everything there
                ClearSurface(false, true, false); // false так как не нужна лишняя проверка
            }
            else
            {
                while (i < structures.Count)
                {
                    if (structures[i] != null)
                    {
                        if (structures[i].surfaceRect.Intersect(sr))
                        {
                            structures[i].Annihilate(false, true, false);
                        }
                    }
                    i++;
                }
            }
        }
        SetStructureTransform(s);
        if (savedBasementForNow != null)
        {
            savedBasementForNow.Annihilate(false, true, false);
        }
        RecalculateSurface();
    }
    /// <summary>
    /// Do not use directly, use "Set Basement" instead
    /// </summary>
    public void AddCellStructure(Structure s)
    {
        if (s == null) return;
        byte x = s.surfaceRect.x, z = s.surfaceRect.z;
        if (map[x * INNER_RESOLUTION + z] == true)
        {
            int i = 0;
            while (i < structures.Count)
            {
                if (structures[i] == null) { structures.RemoveAt(i); continue; }
                SurfaceRect sr = structures[i].surfaceRect;
                if (sr.x <= x & sr.z <= z & sr.x + sr.size > x & sr.z + sr.size > z)
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
        SetStructureTransform(s);
        RecalculateSurface();
    }
    private void SetStructureTransform(Structure s)
    {
        var t = s.transform;
        t.parent = myPlane.myBlockExtension.myBlock.myChunk.transform;
        t.position = GetLocalPosition(s.surfaceRect);
        s.SetVisibility(myPlane.visible);
        switch (myPlane.faceIndex)
        {
            case Block.FWD_FACE_INDEX:

                break;
            case Block.RIGHT_FACE_INDEX: t.localRotation = Quaternion.Euler(-90f, s.modelRotation * 45f, 0f); break;
            case Block.BACK_FACE_INDEX: t.localRotation = Quaternion.Euler(0f, s.modelRotation * 45f, -90f); break;
            case Block.LEFT_FACE_INDEX: t.localRotation = Quaternion.Euler(90f, s.modelRotation * 45f, 0f); break;
            case Block.DOWN_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
                t.localRotation = Quaternion.Euler(0f, s.modelRotation * 45f, 90f); break;
            case Block.UP_FACE_INDEX:
            case Block.SURFACE_FACE_INDEX:
            default:
                t.localRotation = Quaternion.Euler(0f, s.modelRotation * 45f, 0f); break;
        }
        structures.Add(s);
    }

	public void ClearSurface(bool check, bool returnResources, bool deleteExtensionLink)
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
        else
        {
            if (deleteExtensionLink) myPlane.NullifyExtesionLink(this);
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

    #region giving info
    public Vector3 GetLocalPosition(float x, float z)
    {
        Vector3 leftBottomCorner = myPlane.myBlockExtension.myBlock.pos.ToWorldSpace(), xdir, zdir;
        float q = Block.QUAD_SIZE;
        switch (myPlane.faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                leftBottomCorner += new Vector3(0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.left * q;
                zdir = Vector3.up * q;
                break;
            case Block.RIGHT_FACE_INDEX:
                leftBottomCorner += new Vector3(0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
            case Block.BACK_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.up * q;
                break;
            case Block.LEFT_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, 0.5f) * q;
                xdir = Vector3.back * q;
                zdir = Vector3.up * q;
                break;
            case Block.DOWN_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.CEILING_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, +0.5f, -0.5f) * q;
                xdir = Vector3.right * q;
                zdir = Vector3.back * q;
                break;
            case Block.UP_FACE_INDEX:
                leftBottomCorner += new Vector3(-0.5f, +0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
            case Block.SURFACE_FACE_INDEX:
            default:
                leftBottomCorner += new Vector3(-0.5f, -0.5f, -0.5f) * q;
                xdir = Vector3.forward * q;
                zdir = Vector3.up * q;
                break;
        }
        float ir = INNER_RESOLUTION;
        return leftBottomCorner + xdir * x / ir + zdir * z / ir;
    }
    public Vector3 GetLocalPosition(SurfaceRect sr)
    {
        return GetLocalPosition(sr.x + sr.size / 2f, sr.z + sr.size / 2f);
    }
    /// <summary>
    /// returns in 0 - 1 
    /// </summary>
    public Vector2 WorldToMapPosition(Vector3 point)
    {
        Vector3 dir = point - GetLocalPosition(0, 0);
        switch (myPlane.faceIndex)
        {
            case Block.FWD_FACE_INDEX: return new Vector2(dir.x, dir.y);
            case Block.RIGHT_FACE_INDEX: return new Vector2(dir.z, dir.y);
            case Block.BACK_FACE_INDEX: return new Vector2(dir.x, dir.y);
            case Block.LEFT_FACE_INDEX: return new Vector2(dir.z, dir.y);
            case Block.DOWN_FACE_INDEX:
            case Block.CEILING_FACE_INDEX:
                return new Vector2(dir.x, dir.z);
            case Block.SURFACE_FACE_INDEX:
            case Block.UP_FACE_INDEX:
            default:
                return new Vector2(dir.x, dir.z);
        }
    }
    public int GetStructuresCount()
    {
        if (structures == null) return 0;
        else return structures.Count;
    }
    public List<Structure> GetStructuresList()
    {
        return structures;
    }
    #endregion

    public void EnvironmentalStrike(Vector3 hitpoint, byte radius, float damage)
    {
        if (fullfillStatus == FullfillStatus.Empty) return;
        else
        {
            if (fullfillStatus == FullfillStatus.Full & structures.Count == 1)
            {
                structures[0].ApplyDamage(damage);
                return;
            }
            else
            {
                Vector2 inpos = WorldToMapPosition(hitpoint);
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
        if (fullfillStatus == FullfillStatus.Full) return PixelPosByte.Empty;
        else
        {
            if (fullfillStatus == FullfillStatus.Empty) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
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
        if (fullfillStatus != FullfillStatus.Full)
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
        if (fullfillStatus == FullfillStatus.Full || size >= INNER_RESOLUTION || size < 1) return PixelPosByte.Empty;
        if (fullfillStatus == FullfillStatus.Empty) return new PixelPosByte((byte)(Random.value * (INNER_RESOLUTION - 1)), (byte)(Random.value * (INNER_RESOLUTION - 1)));
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
                if (map[xpos * INNER_RESOLUTION + zpos] == true) width = 0; else width++;
                if (width >= size)
                {
                    bool appliable = true;
                    for (int xdelta = 1; xdelta < size; xdelta++)
                    {
                        for (int zdelta = 0; zdelta < size; zdelta++)
                        {
                            if (map[(xpos + xdelta) * INNER_RESOLUTION + zpos + zdelta] == true) { appliable = false; break; }
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
                                map[(xpos + xdelta) * INNER_RESOLUTION + zpos + zdelta] = true;
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
                if (map[xpos * INNER_RESOLUTION + zpos] == true) width = 0; else width++;
                if (width >= zsize)
                {
                    bool appliable = true;
                    for (int xdelta = 1; xdelta < xsize; xdelta++)
                    {
                        for (int zdelta = 0; zdelta < zsize; zdelta++)
                        {
                            if (map[(xpos + xdelta) * INNER_RESOLUTION + zpos + zdelta] == true) { appliable = false; break; }
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
                                map[(xpos + xdelta) * INNER_RESOLUTION + zpos + zdelta] = true;
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
                if (map[i * INNER_RESOLUTION + j] == false) { acceptableVariants.Add(new PixelPosByte(i, j)); }
            }
        }
        if (acceptableVariants.Count == 0)
        {
            fullfillStatus = FullfillStatus.Full;
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
        if (fullfillStatus == FullfillStatus.Empty) return false;
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
        if (fullfillStatus == FullfillStatus.Empty) return null;
        else
        {
            var p = WorldToMapPosition(point);
            if (p.x < 0 | p.x > 1 | p.y < 0 | p.y < 1) return null;
            else
            {
                byte xpos = (byte)(p.x * INNER_RESOLUTION), zpos = (byte)(p.y * INNER_RESOLUTION);
                if (map[xpos * INNER_RESOLUTION + zpos] == false) return null;
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

                    if (!map[x0 * INNER_RESOLUTION + z0] & !map[ (x0 + 1) * INNER_RESOLUTION + z0] & 
                        !map[x0 * INNER_RESOLUTION + z0 + 1] & !map[(x0 + 1) * INNER_RESOLUTION + z0 + 1])
                    {
                        if (val < volume)
                        {
                            scr = ScalableHarvestableResource.Create(rtype, val, myPlane, new PixelPosByte(x0, z0));
                            volume -= val;
                        }
                        else
                        {
                            scr = ScalableHarvestableResource.Create(rtype, (byte)volume, myPlane, new PixelPosByte(x0, z0));
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
        //surfaceObserver.SetObservingSurface(this);
        return surfaceObserver;
    }

    public void Annihilate(bool compensateStructures)
    {
        if (fullfillStatus != FullfillStatus.Empty)
        {
            foreach (var s in structures)
            {
                s.Annihilate(false, compensateStructures, false);
            }
        }
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
        //base.Save(fs);


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

    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int structuresCount = System.BitConverter.ToInt32(data, 0);
        if (structuresCount > INNER_RESOLUTION * INNER_RESOLUTION | structuresCount < 0)
        {
            Debug.Log("surface block load error - incorrect structures count");
            GameMaster.LoadingFail();
            return;
        }
        if (structuresCount > 0) Structure.LoadStructures(structuresCount, fs, myPlane);
    }
    #endregion
}

