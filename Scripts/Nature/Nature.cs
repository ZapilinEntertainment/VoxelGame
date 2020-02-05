using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Nature : MonoBehaviour
{
    private EnvironmentMaster env;
    private Chunk myChunk;

    private List<Grassland> grasslands;
    private List<LifeSource> sources;
    private bool prepared = false;
    private float lifepowerReserve, lifepowerSurplus, grasslandCreateTimer;
    private const float GRASSLAND_CREATE_COST = 100f, GRASSLAND_CREATE_CHECK_TIME = 10f;

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
            var t = Time.deltaTime;
            lifepowerReserve += t * lifepowerSurplus;

            grasslandCreateTimer -= t;
            if (grasslandCreateTimer <= 0) {
                float cost = GRASSLAND_CREATE_COST * env.environmentalConditions;
                if (lifepowerReserve > cost)
                {
                    var slist = myChunk.GetSurfacesWithoutLifeforms();
                    if (slist != null)
                    {
                        CreateGrassland(slist[Random.Range(0, slist.Count)]);
                        lifepowerReserve -= cost;
                    }
                }
                grasslandCreateTimer = GRASSLAND_CREATE_CHECK_TIME;
            }
        }
    }

    public void CreateGrassland(Plane p)
    {

    }
}
