using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceFilter : WorkBuilding
{
    private GeologyModule gm;
    private Storage storage;
    private float richness = 0f;
    private const float RES_VOLUME = 0.1f, CATCH_CHANCE = 0.75f, METAL_DROP_CHANCE = 0.7f;

    public override void SetBasement(Plane b, PixelPosByte pos)
    {
        base.SetBasement(b, pos);
        gm = GameMaster.geologyModule;
        storage = GameMaster.realMaster.colonyController.storage;
        var em = GameMaster.realMaster.environmentMaster;
        richness = em.envRichness;
        em.environmentChangingEvent += EnvironmentUpdate;
    }    

    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        float v = Random.value;
        if (v > CATCH_CHANCE)
        {
            v = (v - CATCH_CHANCE) / (1f - CATCH_CHANCE);
            if (v > METAL_DROP_CHANCE)
            {
                v = (v - METAL_DROP_CHANCE) / (1f - METAL_DROP_CHANCE);
                if (v > 0.5f)
                {
                    if (v > 0.9f) storage.AddResource(ResourceType.metal_N, RES_VOLUME / 2f * iterations);
                    else
                    {
                        if (v > 0.7f) storage.AddResource(ResourceType.metal_S, RES_VOLUME / 2f * iterations);
                        else storage.AddResource(ResourceType.metal_E, RES_VOLUME / 2f * iterations);
                    }
                }
                else
                {
                    if (v > 0.35f) storage.AddResource(ResourceType.metal_P, RES_VOLUME / 2f * iterations);
                    else
                    {
                        if (v > 0.25f) storage.AddResource(ResourceType.metal_M, RES_VOLUME / 2f * iterations);
                        else storage.AddResource(ResourceType.metal_K, RES_VOLUME / 2f * iterations);
                    }
                }
            }
            else
            {
               if ( (v * 100f) % 2 == 0) storage.AddResource(ResourceType.Stone, RES_VOLUME * iterations);
               else storage.AddResource(ResourceType.Dirt, RES_VOLUME * iterations);
            }
        }
    }

    private void EnvironmentUpdate(Environment e)
    {
        richness = e.richness;
    }
}
