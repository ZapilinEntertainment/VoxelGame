using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Nature : MonoBehaviour
{

    public bool needRecalculation = false;
    public float lifepowerSupport { get; private set; }
    public List<PlantType> islandFloraRegister { get; private set; }

    private bool prepared = false, sideGrasslandsSupport = false, ceilGrasslandsSupport = false;
    private float lifepower, lifepowerSurplus, grasslandCreateTimer, grasslandsUpdateTimer;
    private int lastUpdateIndex = 0, lastDonoredIndex = 0;
    private EnvironmentMaster env;
    private Chunk myChunk;
    private GameMaster gm;
    private List<Grassland> grasslands;
    public List<LifeSource> lifesources { get; private set; }
    private List<PlantType> flowerTypes, bushTypes, treeTypes;    
    private Dictionary<int, float> lifepowerAffectionList;
    private int nextLPowerAffectionID = 1;

    private const float GRASSLAND_CREATE_COST = 100f, GRASSLAND_UPDATE_COST = 2f, GRASSLAND_CREATE_CHECK_TIME = 10f, GRASSLAND_UPDATE_TIME = 1f,
        MONUMENT_AFFECTION_CF = 10f, LIFEPOWER_STORAGE_LIMIT = 100000f;
    private const int MAXIMUM_LIFESOURCES_COUNT = 100;
    #region save-load
    public void Save(System.IO.Stream fs)
    {
        fs.Write(System.BitConverter.GetBytes(lifepower),0,4); // 0 -3
        fs.Write(System.BitConverter.GetBytes(grasslandCreateTimer), 0, 4); // 4 - 7
        fs.Write(System.BitConverter.GetBytes(grasslandsUpdateTimer), 0, 4); // 8 - 11
        fs.Write(System.BitConverter.GetBytes(lastUpdateIndex), 0, 4); //12 - 15
        fs.Write(System.BitConverter.GetBytes(lastDonoredIndex), 0, 4); //16 - 19
        int count = 0; // 20 - 23
        if (grasslands != null)
        {
            foreach(var g in grasslands)
            {
                if (g==null || g.deleted)
                {
                    Recalculation();
                    break;
                }
            }
            count = grasslands.Count;
            fs.Write(System.BitConverter.GetBytes(count), 0 ,4);
            if (count > 0)
            {
                foreach (var g in grasslands)
                {
                    g.Save(fs);
                }
            }
            fs.Write(System.BitConverter.GetBytes(Grassland.SYSTEM_GetNextID()),0,4);
        }
        else fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        //
        byte ct = 0;
        if (flowerTypes != null)
        {
            ct = (byte)flowerTypes.Count;
            fs.WriteByte(ct);
            if (ct > 0)
            {
                foreach (var t in flowerTypes) fs.WriteByte((byte)t);
            }
        }
        else fs.WriteByte(ct);
        //
        ct = 0;
        if (bushTypes != null)
        {
            ct = (byte)bushTypes.Count;
            fs.WriteByte(ct);
            if (ct > 0)
            {
                foreach (var t in bushTypes) fs.WriteByte((byte)t);
            }
        }
        else fs.WriteByte(ct);
        //
        ct = 0;
        if (treeTypes != null)
        {
            ct = (byte)treeTypes.Count;
            fs.WriteByte(ct);
            if (ct > 0)
            {
                foreach (var t in treeTypes) fs.WriteByte((byte)t);
            }
        }
        else fs.WriteByte(ct);
        //
        ct = 0;
        if (islandFloraRegister != null)
        {
            ct = (byte)islandFloraRegister.Count;
            fs.WriteByte(ct);
            if (ct > 0)
            {
                foreach (var t in islandFloraRegister) fs.WriteByte((byte)t);
            }
        }
        else fs.WriteByte(ct);
        //
        ct = 0;
        if (lifepowerAffectionList != null)
        {
            ct = (byte)lifepowerAffectionList.Count;
            fs.WriteByte(ct);
            if (ct > 0)
            {
                foreach (var x in lifepowerAffectionList)
                {
                    fs.Write(System.BitConverter.GetBytes(x.Key), 0, 4);
                    fs.Write(System.BitConverter.GetBytes(x.Value), 0, 4);
                }
            }
        }
        else fs.WriteByte(ct);
    }
    public void Load(System.IO.Stream fs, Chunk c)
    {
        env = GameMaster.realMaster.environmentMaster;
        lifepowerSupport = env.lifepowerSupport;
        myChunk = c;
        //
        var data = new byte[24];
        fs.Read(data, 0, data.Length);
        lifepower = System.BitConverter.ToSingle(data, 0);
        grasslandCreateTimer = System.BitConverter.ToSingle(data, 4);
        grasslandsUpdateTimer = System.BitConverter.ToSingle(data, 8);
        lastUpdateIndex = System.BitConverter.ToInt32(data, 12);
        lastDonoredIndex = System.BitConverter.ToInt32(data, 16);
        //
        int count = System.BitConverter.ToInt32(data, 20), i;
        grasslands = null;
        if (count != 0)
        {
            grasslands = new List<Grassland>();
            var checkList = new HashSet<PlanePos>();
            for(i = 0; i< count; i++)
            {
                Grassland.Load(fs, myChunk);                
            }            
            var iddata = new byte[4];
            fs.Read(iddata, 0, 4);
            Grassland.SYSTEM_SetNextID(System.BitConverter.ToInt32(iddata, 0));
        }
        //
        count = fs.ReadByte();
        flowerTypes = new List<PlantType>();
        if (count != 0)
        {
            for (i = 0; i < count; i++)
            {
                flowerTypes.Add((PlantType)fs.ReadByte());
            }
        }
        //
        count = fs.ReadByte();
        bushTypes = new List<PlantType>();
        if (count != 0)
        {
            for (i = 0; i < count; i++)
            {
                bushTypes.Add((PlantType)fs.ReadByte());
            }
        }
        //
        count = fs.ReadByte();
        treeTypes = new List<PlantType>();
        if (count != 0)
        {
            for (i = 0; i < count; i++)
            {
                treeTypes.Add((PlantType)fs.ReadByte());
            }
        }
        //
        count = fs.ReadByte();
        islandFloraRegister = new List<PlantType>();
        if (count != 0)
        {
            for (i = 0; i < count; i++)
            {
                islandFloraRegister.Add((PlantType)fs.ReadByte());
            }
        }
        //
        count = fs.ReadByte();
        lifepowerAffectionList = new Dictionary<int, float>();
        if (count != 0)
        {
            data = new byte[8 * count];
            fs.Read(data,0, data.Length);
            int id = 0;
            for (i =0; i< count; i++)
            {
                id = i * 8;
                lifepowerAffectionList.Add(System.BitConverter.ToInt32(data, id), System.BitConverter.ToSingle(data, id + 4));
            }
        }
    }
    #endregion
   

    public static bool MaterialIsLifeSupporting(int materialID)
    {
        switch (materialID)
        {
            case ResourceType.DIRT_ID:
            case ResourceType.FERTILE_SOIL_ID:
            case PoolMaster.MATERIAL_GRASS_100_ID:
            case PoolMaster.MATERIAL_GRASS_20_ID:
            case PoolMaster.MATERIAL_GRASS_40_ID:
            case PoolMaster.MATERIAL_GRASS_60_ID:
            case PoolMaster.MATERIAL_GRASS_80_ID:
            case ResourceType.LUMBER_ID:
            case ResourceType.PLASTICS_ID:
                return true;
            default: return false;
        }
    }
    public static bool IsPlaneSuitableForGrassland(Plane p)
    {
        if (p.isQuad && MaterialIsLifeSupporting(p.materialID)) return true;
        else return false;
    }

    public void Prepare(Chunk c)
    {
        myChunk = c;
        env = GameMaster.realMaster.environmentMaster;
        lifepowerSupport = env.lifepowerSupport;
        env.environmentChangingEvent += EnvironmentSetting;
        prepared = true;
        treeTypes = new List<PlantType>() { PlantType.OakTree };
        lifepower = 100f;
        lifepowerSurplus = 2f;
        grasslandCreateTimer = GRASSLAND_CREATE_CHECK_TIME;
        gm = GameMaster.realMaster;
    }
    private void EnvironmentSetting(Environment e)
    {
        lifepowerSupport = e.lifepowerSupport;
    }
    public void FirstLifeformGeneration(float lpower)
    {
        if (!prepared) Prepare(GameMaster.realMaster.mainChunk);
        if (lifepowerSupport == 0f)
        {
            lifepower = lpower;
            return;
        }
        var slist = new List<Plane>(myChunk.GetSurfaces());        
        if (slist != null)
        {
            int count = slist.Count;
            var s = slist[Random.Range(0, count)];
            {
                var ls = Structure.GetStructureByID(Random.value > 0.5f ? Structure.LIFESTONE_ID : Structure.TREE_OF_LIFE_ID);
                ls.SetBasement(s);
            }
            slist.Remove(s); count--;

            if (lifepowerSupport < 1f)
            {
                int cutted = (int)(count * (1f - lifepowerSupport));
                if (cutted > 1)
                {
                    while (count > 0 && cutted > 0)
                    {
                        slist.RemoveAt(Random.Range(0, count));
                        count--;
                        cutted--;
                    }
                }
            }

            float lifepiece = 50f;
            if (count > 0)
            {
                Plane p;
                Grassland g;
                int i;
                while (lpower > 0f & count > 0)
                {
                    i = Random.Range(0, count);
                    p = slist[i];
                    if (p.haveGrassland)
                    {
                        g = p.GetGrassland();
                        g.AddLifepower(lifepiece);
                        lpower -= lifepiece;                       
                    }
                    else
                    {
                        if (p.IsSuitableForGrassland())
                        {
                            g = Grassland.CreateAt(p, false);
                            if (g != null)
                            {
                                g.AddLifepower(lifepiece);
                                lpower -= lifepiece;
                            }
                        }
                        else
                        {
                            slist.RemoveAt(i);
                            count--;
                        }
                    }
                }
                if (lpower > 0f) lifepower += lpower;                
            }
            needRecalculation = true;
        }
    }

    private void Update()
    {
        if (GameMaster.loading) return;

        if (!prepared) {
            Prepare(GameMaster.realMaster.mainChunk);
            return;
        }
        else
        {
            if (gm.gameMode == GameMode.Editor) return;
            if (needRecalculation) Recalculation();
            //if (Input.GetKeyDown("x"))  Debug.Log(lifepowerSurplus);
            var t = Time.deltaTime;
            lifepower += t * lifepowerSurplus * GameMaster.gameSpeed;
            int lfc = lifesources?.Count + 1 ?? 1;
            if (lfc > MAXIMUM_LIFESOURCES_COUNT) lfc = MAXIMUM_LIFESOURCES_COUNT;
            float x = LIFEPOWER_STORAGE_LIMIT * lfc;
            if (lifepower > x) lifepower = x;

            grasslandCreateTimer -= t;
            if (grasslandCreateTimer <= 0f)
            {
                float cost = GRASSLAND_CREATE_COST * env.environmentalConditions;
                if (lifepower > cost)
                {
                    bool expansion = grasslands != null;
                    Grassland g = null;
                    if (expansion) CreateGrassland(cost);
                    else
                    {
                        var slist = myChunk.GetSurfaces();
                        if (slist != null)
                        {
                            var ilist = new List<int>();
                            Plane p;
                            for (int i = 0; i < slist.Length; i++)
                            {
                                p = slist[i];
                                if (p != null && p.IsSuitableForGrassland()) ilist.Add(i);
                            }
                            if (ilist.Count != 0)
                            {
                                p = slist[ilist[Random.Range(0, ilist.Count)]];
                                g = Grassland.CreateAt(p, false);
                                if (g != null) SupportCreatedGrassland(g, cost);
                            }
                        }
                    }                    
                }
                else
                {
                    if (lifepower < 1000f && grasslands != null)
                    {
                        var g = grasslands[Random.Range(0, grasslands.Count)];
                        if (g != null && !g.deleted)
                        {
                            g.Dry();
                            lifepower += 1000f;
                            needRecalculation = true;
                        }
                        return;
                    }
                }                
                grasslandCreateTimer = GRASSLAND_CREATE_CHECK_TIME;
            }
            grasslandsUpdateTimer -= t;
            if (grasslandsUpdateTimer <= 0f && lifepower > 0f)
            {
                if (grasslands != null)
                {
                    if (lastUpdateIndex >= grasslands.Count) lastUpdateIndex = 0;
                    var g = grasslands[lastUpdateIndex];
                    if (g != null)
                    {
                        g.Update();
                        lifepower -= GRASSLAND_UPDATE_COST * g.level;
                        lastUpdateIndex++;
                    }
                    else needRecalculation = true;
                }
                grasslandsUpdateTimer = GRASSLAND_UPDATE_TIME;
            }
        }
    }
    private void Recalculation()
    {
        lifepowerSurplus = 0f;
        if (grasslands != null)
        {
            var gpos = new HashSet<PlanePos>();
            PlanePos ppos;
            bool notSuitable(Grassland g) {
                if (g == null || g.deleted) return true;
                else
                {
                    ppos = g.planePos;
                    if (gpos.Contains(ppos)) return true;
                    else
                    {
                        gpos.Add(ppos);
                        return false;
                    }
                }
            }
            grasslands.RemoveAll(notSuitable);
            if (grasslands.Count == 0) grasslands = null;
            else
            {
                foreach (var g in grasslands)
                {
                    if (g == null) Debug.Log("grasslands list error");
                    else lifepowerSurplus += g.GetLifepowerSurplus();
                }
            }
        }
        if (lifepowerAffectionList != null)
        {
            foreach (var af in lifepowerAffectionList)
            {
                lifepowerSurplus += af.Value * MONUMENT_AFFECTION_CF;
            }
        }
        needRecalculation = false;
    }
    private void SupportCreatedGrassland(Grassland g, float cost)
    {
        g.AddLifepower(cost / 4f);
        lifepower -= cost;
        Knowledge.GetCurrent()?.GrasslandsCheck(grasslands);
    }

    public void AddLifepower(float f)
    {
        lifepower += f;
    }
    public void ConsumeLifepower(float f)
    {
        lifepower -= f;
        if (lifepower < -1000f) lifepower = -1000f;
    }
    public void AddLifesource(LifeSource ls)
    {
        if (lifesources == null)
        {
            lifesources = new List<LifeSource>() { ls };
            Knowledge.GetCurrent().CountRouteBonus(Knowledge.MonumentRouteBoosters.LifesourceBoost);
            return;
        }
        else
        {
            if (!lifesources.Contains(ls)) {
                lifesources.Add(ls);
                Knowledge.GetCurrent().CountRouteBonus(Knowledge.MonumentRouteBoosters.LifesourceBoost);
            }
        }
    }
    public void RemoveLifesource(LifeSource ls)
    {
        lifesources?.Remove(ls);
    }

    public int AddLifepowerAffection(float f)
    {
        if (lifepowerAffectionList == null) lifepowerAffectionList = new Dictionary<int, float>();
        int id = nextLPowerAffectionID++;
        lifepowerAffectionList.Add(id, f);
        needRecalculation = true;
        return id;
    }
    public void RemoveLifepowerAffection(int id)
    {
        if (lifepowerAffectionList != null)
        {
            if (lifepowerAffectionList.ContainsKey(id))
            {
                lifepowerAffectionList.Remove(id);
                needRecalculation = true;
                if (lifepowerAffectionList.Count == 0) lifepowerAffectionList = null;
            }
        }
    }
    public void ChangeLifepowerAffection(int id, float newVal)
    {
        if (lifepowerAffectionList != null)
        {
            if (lifepowerAffectionList.ContainsKey(id))
            {
                lifepowerAffectionList[id] = newVal;
                needRecalculation = true;
            }
        }
    }

    public void CreateGrassland(float supplyEnergy)
    {
        int totalCount = grasslands.Count;
        if (lastDonoredIndex >= totalCount) lastDonoredIndex = 0;

        var g = grasslands[lastDonoredIndex];
        if (g == null)
        {
            needRecalculation = true;
            return;
        }
        var fi = g.faceIndex;
        List<Plane> candidates = new List<Plane>();
        Block b, myBlock = g.plane.GetBlock(); Plane p; ChunkPos cpos = g.pos;
        switch (fi)
        {
            case Block.UP_FACE_INDEX:
                {
                    // fwd
                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y + 1, cpos.z + 1), out b))
                    {
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(cpos.OneBlockForward(), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    if (sideGrasslandsSupport && myBlock.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    //right
                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x + 1, cpos.y + 1, cpos.z), out b))
                    {
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(cpos.OneBlockRight(), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    if (sideGrasslandsSupport && myBlock.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    //back
                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y + 1, cpos.z - 1), out b))
                    {
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(cpos.OneBlockBack(), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    if (sideGrasslandsSupport && myBlock.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    //left
                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x - 1, cpos.y + 1, cpos.z), out b))
                    {
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(cpos.OneBlockLeft(), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    if (sideGrasslandsSupport && myBlock.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    //up
                    if (ceilGrasslandsSupport && myChunk.blocks.TryGetValue(cpos.OneBlockHigher(), out b))
                    {
                        if (b.TryGetPlane(Block.CEILING_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    break;
                }
            case Block.SURFACE_FACE_INDEX:
                {
                    // fwd
                    if (myChunk.blocks.TryGetValue(cpos.OneBlockForward(), out b))
                    {
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y - 1, cpos.z + 1), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    //right
                    if (myChunk.blocks.TryGetValue(cpos.OneBlockRight(), out b))
                    {
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x + 1, cpos.y - 1, cpos.z), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    //back
                    if (myChunk.blocks.TryGetValue(cpos.OneBlockBack(), out b))
                    {
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y - 1, cpos.z - 1), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    //left
                    if (myChunk.blocks.TryGetValue(cpos.OneBlockLeft(), out b))
                    {
                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (sideGrasslandsSupport && b.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    else
                    {
                        if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x - 1, cpos.y - 1, cpos.z), out b))
                        {
                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        }
                    }
                    // down
                    if (sideGrasslandsSupport && myChunk.blocks.TryGetValue(cpos.OneBlockDown(), out b))
                    {
                        if (b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                        if (b.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    }
                    // up
                    if (ceilGrasslandsSupport && myBlock.TryGetPlane(Block.CEILING_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                    break;
                }
        }
        if (candidates.Count > 0)
        {
            bool IsNotSuitable(Plane px) { return !px.IsSuitableForGrassland(); }
            candidates.RemoveAll(IsNotSuitable);
            if (candidates.Count > 0)
            {
                p = candidates[Random.Range(0, candidates.Count)];
                CreateGrassland(p, supplyEnergy);       
            }
        }
        lastDonoredIndex++;
    }
    public Grassland CreateGrassland(Plane p, in float supplyEnergy)
    {
        Grassland g = null;
        if (p != null)
        {
            g = Grassland.CreateAt(p, false);
            if (g != null) SupportCreatedGrassland(g, supplyEnergy);
        }
        return g;
    }
    public void AddGrassland(Grassland g)
    {
        if (grasslands == null) grasslands = new List<Grassland>();
        else
        {
            if (!grasslands.Contains(g))
            {
                grasslands.Add(g);
                needRecalculation = true;
            }
        }       
    }
    public void RemoveGrassland(Grassland g)
    {
        if (grasslands != null)
        {
            grasslands.Remove(g);
            if (grasslands.Count == 0) grasslands = null;
            needRecalculation = true;
        }
    }
    public List<Grassland> GetGrasslandsList()
    {
        return grasslands;
    }

    public void RegisterNewLifeform(PlantType pt)
    {
        if (islandFloraRegister == null)
        {
            islandFloraRegister = new List<PlantType>() { pt };
            return;
        }
        else
        {
            if (!islandFloraRegister.Contains(pt)) islandFloraRegister.Add(pt);
        }
    }
    public void UnregisterLifeform(PlantType pt)
    {
        if (islandFloraRegister != null)
        {
            islandFloraRegister.Remove(pt);
        }
    }

    public PlantType GetPlantType(PlantCategory cat)
    {
        cat = PlantCategory.Tree;
        switch (cat)
        {
            case PlantCategory.Tree: return treeTypes[Random.Range(0, treeTypes.Count)];
            case PlantCategory.Bush: return bushTypes[Random.Range(0, bushTypes.Count)];
            default: return flowerTypes[Random.Range(0, flowerTypes.Count )];
        }
    }
    public int DEBUG_GetPlantsCount()
    {
        if (grasslands == null || grasslands.Count == 0) return 0;
        else
        {
            int x = 0;
            foreach (var g in grasslands)
            {
                x += g.plane.GetStructuresList()?.Count ?? 0;
            }
            return x;
        }
    }
    public void DEBUG_HaveGrasslandDublicates()
    {
        var alreadyChecked = new List<Plane>();
        if (grasslands != null && grasslands.Count > 0)
        {
            int i = 0, count = 0;
            Grassland g; Plane p, p2;
            for (; i < grasslands.Count - 1; i++)
            {
                g = grasslands[i];
                if (alreadyChecked.Contains(g.plane)) continue;
                else
                {
                    p = g.plane;
                    alreadyChecked.Add(p);
                    for (int j = i + 1; j < grasslands.Count; j++)
                    {
                        p2 = grasslands[j].plane;
                        if (p2 == p)
                        {
                            count++;
                        }
                    }
                }
            }
            if (count > 0)
            {
                Debug.Log("nature - grassland duplicates found: " + count.ToString());
                Debug.Log("total grasslands count: " + grasslands.Count.ToString());
                return;
            }

        }
        Debug.Log("nature - no grassland duplicates");
    }


    public float GetNatureCf()
    {
        float x = 0f;
        const float p = 0.5f;
        if (grasslands != null)
        {
            int c = grasslands.Count;
            const float MAX_GLS = 100f;
            if (c > MAX_GLS) x += p;
            else x += ((float)c / MAX_GLS) * p;            
        }
        //
        float MAX_LP = LIFEPOWER_STORAGE_LIMIT ;
        if (lifepower > MAX_LP) x += p;
        else x += p * lifepower / MAX_LP;
        //
        return x;
    }

    private void OnDestroy()
    {
        if (env != null) env.environmentChangingEvent -= EnvironmentSetting;
    }
}
