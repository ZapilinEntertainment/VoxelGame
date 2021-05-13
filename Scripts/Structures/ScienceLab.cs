using System.Collections.Generic;
using UnityEngine;

public sealed class ScienceLab : WorkBuilding
{
    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        colony?.observer?.AddFastButton(this);
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(order);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
