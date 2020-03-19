using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceFilter : WorkBuilding
{
    private GeologyModule gm;

    public override void SetBasement(Plane b, PixelPosByte pos)
    {
        base.SetBasement(b, pos);
        gm = GameMaster.geologyModule;
    }

    override protected void LabourResult()
    {
        workflow = 0;
        //output
    }
}
