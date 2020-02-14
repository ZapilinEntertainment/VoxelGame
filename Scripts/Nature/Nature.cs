using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Nature : MonoBehaviour
{
    private EnvironmentMaster env;
    private Chunk myChunk;
    public bool needRecalculation = false;
    public float environmentalConditions { get; private set; }
    private bool prepared = false;
    private float lifepower, lifepowerSurplus, grasslandCreateTimer, grasslandsUpdateTimer;
    private int lastUpdateIndex = 0;
    private List<Grassland> grasslands;
    private List<LifeSource> sources;
    private List<PlantType> flowerTypes, bushTypes, treeTypes;
    private List<PlantType> islandFlora;

    private const float GRASSLAND_CREATE_COST = 100f, GRASSLAND_UPDATE_COST = 2f, GRASSLAND_CREATE_CHECK_TIME = 10f, GRASSLAND_UPDATE_TIME = 1f;

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
        environmentalConditions = env.environmentalConditions;
        prepared = true;
        treeTypes = new List<PlantType>() { PlantType.OakTree };
        lifepower = 100f;
        lifepowerSurplus = 2f;
    }

    private void Update()
    {
        if (!prepared) {
            Prepare(GameMaster.realMaster.mainChunk);
            return;
        }
        else
        {
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
                needRecalculation = false;
            }
            //if (Input.GetKeyDown("x"))  Debug.Log(lifepowerSurplus);
            var t = Time.deltaTime;
            lifepower += t * lifepowerSurplus;

            grasslandCreateTimer -= t;
            if (grasslandCreateTimer <= 0f)
            {
                float cost = GRASSLAND_CREATE_COST * env.environmentalConditions;

                if (lifepower > cost)
                {
                    bool expansion = grasslands != null;
                    if (expansion)
                    {
                        var g = grasslands[Random.Range(0, grasslands.Count - 1)];
                        var fi = g.faceIndex;
                        List<Plane> candidates = new List<Plane>();
                        Block b, myBlock = g.plane.myBlockExtension.myBlock; Plane p; ChunkPos cpos = g.pos;
                        switch (fi)
                        {
                            case Block.UP_FACE_INDEX:
                                {
                                    // fwd
                                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y + 1, cpos.z + 1), out b))
                                    {
                                        if (b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    }
                                    else
                                    {
                                        if (myChunk.blocks.TryGetValue(cpos.OneBlockForward(), out b))
                                        {
                                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);                                            
                                        }                                        
                                    }
                                    if (myBlock.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    //right
                                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x + 1, cpos.y + 1, cpos.z), out b))
                                    {
                                        if (b.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    }
                                    else
                                    {
                                        if (myChunk.blocks.TryGetValue(cpos.OneBlockRight(), out b))
                                        {
                                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);                                           
                                        }
                                    }
                                    if (myBlock.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    //back
                                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y + 1, cpos.z - 1), out b))
                                    {
                                        if (b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    }
                                    else
                                    {
                                        if (myChunk.blocks.TryGetValue(cpos.OneBlockBack(), out b))
                                        {
                                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);                                            
                                        }
                                    }
                                    if (myBlock.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    //left
                                    if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x - 1, cpos.y + 1, cpos.z), out b))
                                    {
                                        if (b.TryGetPlane(Block.RIGHT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    }
                                    else
                                    {
                                        if (myChunk.blocks.TryGetValue(cpos.OneBlockLeft(), out b))
                                        {
                                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);                                            
                                        }
                                    }
                                    if (myBlock.TryGetPlane(Block.LEFT_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    //up
                                    if (myChunk.blocks.TryGetValue(cpos.OneBlockHigher(), out b))
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
                                        if (b.TryGetPlane(Block.BACK_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                        if (b.TryGetPlane(Block.SURFACE_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    }
                                    else
                                    {
                                        if (myChunk.blocks.TryGetValue(new ChunkPos(cpos.x, cpos.y - 1, cpos.z + 1), out b))
                                        {                                            
                                            if (b.TryGetPlane(Block.UP_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                        }                                        
                                    }
                                    if (myChunk.blocks.TryGetValue(cpos.OneBlockDown(), out b))
                                    {
                                        if (b.TryGetPlane(Block.FWD_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    }
                                    //right

                                    //
                                    if (myBlock.TryGetPlane(Block.CEILING_FACE_INDEX, out p) && !p.haveGrassland) candidates.Add(p);
                                    break;
                                }
                        }
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

    public void AddLifepower(float f)
    {
        lifepower += f;
    }

    public Grassland CreateGrassland(Plane p)
    {
        return new Grassland(p, this);
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
            needRecalculation = true;
        }
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
            case PlantCategory.Tree: return treeTypes[Random.Range(0, treeTypes.Count - 1)];
            case PlantCategory.Bush: return bushTypes[Random.Range(0, bushTypes.Count - 1)];
            default: return flowerTypes[Random.Range(0, flowerTypes.Count - 1)];
        }
    }
}
