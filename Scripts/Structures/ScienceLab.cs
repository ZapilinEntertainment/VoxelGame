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
    override public void LabourUpdate()
    {
        if (!isActive | !isEnergySupplied) return;
        if (workersCount > 0)
        {
            workSpeed = colony.workspeed * workersCount * GameConstants.RESEARCH_SPEED;
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage * workSpeed;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
        else workSpeed = 0f;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
