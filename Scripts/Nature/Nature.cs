using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Nature : MonoBehaviour
{
    private EnvironmentMaster env;
    private Chunk myChunk;
    public bool needRecalculation = false;
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
        prepared = true;
    }

    private void Update()
    {
        if (!prepared) return;
        else {
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
            var t = Time.deltaTime;
            lifepower += t * lifepowerSurplus;

            grasslandCreateTimer -= t;
            if (grasslandCreateTimer <= 0f) {
                float cost = GRASSLAND_CREATE_COST * env.environmentalConditions,
                    multiplier = 1f;
                
                if (lifepower > cost)
                {
                    var slist = myChunk.GetSurfacesWithoutLifeforms();
                    if (slist != null)
                    {
                        CreateGrassland(slist[Random.Range(0, slist.Count)]);
                        lifepower -= cost;
                    }
                    else multiplier = 2f;
                }
                grasslandCreateTimer = GRASSLAND_CREATE_CHECK_TIME * multiplier;
            }
            grasslandsUpdateTimer -= t;
            if (grasslandsUpdateTimer <= 0f && lifepower > 0f)
            {
                if (grasslands != null)
                {
                    int count = grasslands.Count;
                    if (lastUpdateIndex >= count)
                    {
                        lastUpdateIndex = 0;
                        var g = grasslands[lastUpdateIndex];
                        g.Update();
                        lifepower -= GRASSLAND_UPDATE_COST * g.level;
                        lastUpdateIndex++;
                    }
                }
                grasslandsUpdateTimer = GRASSLAND_UPDATE_TIME;
            }
        }
    }

    public void AddLifepower(float f)
    {
        lifepower += f;
    }

    public void CreateGrassland(Plane p)
    {

    }
    public void AddGrassland(Grassland g)
    {
        if (grasslands == null) grasslands = new List<Grassland>();
        else
        {
            if (grasslands.Contains(g)) return;
        }
        grasslands.Add(g);
    }
    public void RemoveGrassland(Grassland g)
    {
        if (grasslands != null)
        {
            grasslands.Remove(g);
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
        switch (cat)
        {
            case PlantCategory.Tree: return treeTypes[Random.Range(0, treeTypes.Count - 1)];
            case PlantCategory.Bush: return bushTypes[Random.Range(0, bushTypes.Count - 1)];
            default: return flowerTypes[Random.Range(0, flowerTypes.Count - 1)];
        }
    }
}
