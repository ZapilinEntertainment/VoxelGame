using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Nature : MonoBehaviour
{
    private EnvironmentMaster env;
    private Chunk myChunk;
    private GameMaster gm;
    public bool needRecalculation = false;
    public float lifepowerSupport { get; private set; }
    private bool prepared = false, sideGrasslandsSupport = false, ceilGrasslandsSupport = false;
    private float lifepower, lifepowerSurplus, grasslandCreateTimer, grasslandsUpdateTimer;
    private int lastUpdateIndex = 0;
    private List<Grassland> grasslands;
    private List<LifeSource> lifesources;
    private List<PlantType> flowerTypes, bushTypes, treeTypes;
    private List<PlantType> islandFlora;
    private Dictionary<int, float> lifepowerAffectionList;
    private int nextLPowerAffectionID = 1;

    private const float GRASSLAND_CREATE_COST = 100f, GRASSLAND_UPDATE_COST = 2f, GRASSLAND_CREATE_CHECK_TIME = 10f, GRASSLAND_UPDATE_TIME = 1f,
        MONUMENT_AFFECTION_CF = 10f;
    #region save-load
    public void Save(System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(lifepower),0,4); // 0 -3
        fs.Write(System.BitConverter.GetBytes(grasslandCreateTimer), 0, 4); // 4 - 7
        fs.Write(System.BitConverter.GetBytes(grasslandsUpdateTimer), 0, 4); // 8 - 11
        fs.Write(System.BitConverter.GetBytes(lastUpdateIndex), 0, 4); //12 - 15
        int count = 0; // 16 - 19
        if (grasslands != null)
        {
            var glist = new List<Grassland>();
            foreach (var g in grasslands)
            {
                if (g != null) glist.Add(g);
            }
            count = glist.Count;
            fs.Write(System.BitConverter.GetBytes(count), 0 ,4);
            if (count > 0)
            {
                foreach (var g in glist)
                {
                    g.Save(fs);
                }
            }
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
        if (islandFlora != null)
        {
            ct = (byte)islandFlora.Count;
            fs.WriteByte(ct);
            if (ct > 0)
            {
                foreach (var t in islandFlora) fs.WriteByte((byte)t);
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
    public void Load(System.IO.FileStream fs, Chunk c)
    {
        env = GameMaster.realMaster.environmentMaster;
        lifepowerSupport = env.lifepowerSupport;
        myChunk = c;
        //
        var data = new byte[20];
        fs.Read(data, 0, data.Length);
        lifepower = System.BitConverter.ToSingle(data, 0);
        grasslandCreateTimer = System.BitConverter.ToSingle(data, 4);
        grasslandsUpdateTimer = System.BitConverter.ToSingle(data, 8);
        lastUpdateIndex = System.BitConverter.ToInt32(data, 12);
        //
        int count = System.BitConverter.ToInt32(data, 16), i;
        grasslands = null;
        if (count != 0)
        {
            grasslands = new List<Grassland>();
            Grassland g;
            for(i = 0; i< count; i++)
            {
                g = Grassland.Load(fs, this, myChunk);
                if (g != null) grasslands.Add(g);
            }
            if (grasslands.Count == 0) grasslands = null;
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
        islandFlora = new List<PlantType>();
        if (count != 0)
        {
            for (i = 0; i < count; i++)
            {
                islandFlora.Add((PlantType)fs.ReadByte());
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
                return true;
            default: return false;
        }
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
        gm = GameMaster.realMaster;
    }
    private void EnvironmentSetting(Environment e)
    {
        lifepowerSupport = e.lifepowerSupport;
    }
    public void FirstSet(float lpower)
    {
        if (!prepared) Prepare(GameMaster.realMaster.mainChunk);
        var slist = new List<Plane>(myChunk.surfaces);
        if (slist != null)
        {
            int count = slist.Count;
            var s = slist[Random.Range(0, count)];
            {
                var ls = Structure.GetStructureByID(Random.value > 0.5f ? Structure.LIFESTONE_ID : Structure.TREE_OF_LIFE_ID);
                ls.SetBasement(s);
            }
            slist.Remove(s); count--;

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
                        p.GetGrassland().AddLifepower(lifepiece);
                        lpower -= lifepiece;
                    }
                    else
                    {
                        if (Random.value < lifepowerSupport)
                        {
                            g = p.FORCED_GetExtension().InitializeGrassland();
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
            if (gm.gameMode != GameMode.Play) return;
            if (needRecalculation)
            {
                lifepowerSurplus = 0f;
                if (grasslands != null)
                {
                    foreach (var g in grasslands)
                    {
                        lifepowerSurplus += g.GetLifepowerSurplus();
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
            //if (Input.GetKeyDown("x"))  Debug.Log(lifepowerSurplus);
            var t = Time.deltaTime;
            lifepower += t * lifepowerSurplus * GameMaster.gameSpeed;

            grasslandCreateTimer -= t;
            if (grasslandCreateTimer <= 0f)
            {
                float cost = GRASSLAND_CREATE_COST * env.environmentalConditions;
                if (lifepower > cost)
                {
                    bool expansion = grasslands != null;
                    Grassland g = null;
                    if (expansion)
                    {
                        g = grasslands[Random.Range(0, grasslands.Count)];
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
                                    if (myChunk.blocks.TryGetValue(cpos.OneBlockRight(), out b)) {
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
                            p = candidates[Random.Range(0, candidates.Count)];
                            g = CreateGrassland(p);
                            if (g != null) SupportCreatedGrassland(g, cost);
                        }                        
                    }
                    else
                    {
                        var slist = myChunk.surfaces;
                        if (slist != null)
                        {
                            var ilist = new List<int>();
                            Plane p;
                            for (int i= 0; i<slist.Length; i++)
                            {
                                p = slist[i];
                                if (MaterialIsLifeSupporting(p.materialID) && !p.haveGrassland) ilist.Add(i);
                            }
                            if (ilist.Count != 0)
                            {
                                p = slist[ilist[Random.Range(0, ilist.Count)]];
                                g = CreateGrassland(p);
                                if (g != null) SupportCreatedGrassland(g, cost);
                            }
                        }
                    }                    
                }
                else
                {
                    if (lifepower < 1000f && grasslands != null)
                    {
                        grasslands[Random.Range(0, grasslands.Count )].Dry();
                        lifepower += 1000f;
                        needRecalculation = true;
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
                    int count = grasslands.Count;
                    if (lastUpdateIndex >= count) lastUpdateIndex = 0;
                    var g = grasslands[lastUpdateIndex];
                    g.Update();
                    lifepower -= GRASSLAND_UPDATE_COST * g.level;
                    lastUpdateIndex++;
                }
                grasslandsUpdateTimer = GRASSLAND_UPDATE_TIME;
            }
        }
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

    public Grassland CreateGrassland(Plane p)
    {
        if (p.fulfillStatus != FullfillStatus.Full) return new Grassland(p, this);
        else return null; 
    }
    public void AddGrassland(Grassland g)
    {
        if (grasslands == null) grasslands = new List<Grassland>();
        else
        {
            if (grasslands.Contains(g)) return;
        }
        grasslands.Add(g);
        needRecalculation = true;
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
        if (islandFlora == null)
        {
            islandFlora = new List<PlantType>() { pt };
            return;
        }
        else
        {
            if (!islandFlora.Contains(pt)) islandFlora.Add(pt);
        }
    }
    public void UnregisterLifeform(PlantType pt)
    {
        if (islandFlora != null)
        {
            islandFlora.Remove(pt);
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
}
