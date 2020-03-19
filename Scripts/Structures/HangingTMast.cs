using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingTMast : WorkBuilding
{
    private bool subscribedToRestoreBlockersEvent = false;
    private int lifepowerAffectionID = -1;
    private const float LIFEPOWER_PER_UNIT = 1f;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!GameMaster.loading) SetBlockers();
        else
        {
            if (!subscribedToRestoreBlockersEvent)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersEvent = true;
            }
        }
    }

    private void SetBlockers()
    {
        if (basement != null)
        {
            var chunk = basement.myChunk;
            var cpos = basement.GetBlockingPosition();
            if (cpos != basement.pos) chunk.CreateBlocker(cpos, this, false);
            chunk.CreateBlocker(cpos + basement.GetLookVector(), this, false);
        }
        else Debug.LogError("HangingTMast cannot set blockers - no basement set");
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersEvent)
        {
            SetBlockers();
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
    }

    override public void RecalculateWorkspeed()
    {
        if (workersCount == 0)
        {
            if (lifepowerAffectionID != -1)
            {
                basement.myChunk.GetNature().RemoveLifepowerAffection(lifepowerAffectionID);
                lifepowerAffectionID = -1;
            }
        }
        else
        {
            float newWorkspeed = 0f;
            if (workersCount < maxWorkers / 2)
            {
                newWorkspeed = workersCount * LIFEPOWER_PER_UNIT;
            }
            else
            {
                int d = workersCount - maxWorkers/2;
                newWorkspeed = maxWorkers / 2 * LIFEPOWER_PER_UNIT + d * LIFEPOWER_PER_UNIT * 1.1f;
            }
            if (newWorkspeed != workSpeed)
            {
                if (lifepowerAffectionID == -1)
                    lifepowerAffectionID = basement.myChunk.GetNature().AddLifepowerAffection(newWorkspeed);
                else basement.myChunk.GetNature().ChangeLifepowerAffection(lifepowerAffectionID, newWorkspeed);
            }
        }
    }
}
