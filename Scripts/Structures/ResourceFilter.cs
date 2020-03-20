using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceFilter : WorkBuilding
{
    private GeologyModule gm;
    private Storage storage;
    private const float RES_VOLUME = 0.1f, CATCH_CHANCE = 0.75f;

    public override void SetBasement(Plane b, PixelPosByte pos)
    {
        base.SetBasement(b, pos);
        gm = GameMaster.geologyModule;
        storage = GameMaster.realMaster.colonyController.storage;
    }

    override protected void LabourResult()
    {
        workflow = 0;
        float v = Random.value;
        if (v > 0.75f)
        {

        }
        else
        {
            
        }
    }
}
