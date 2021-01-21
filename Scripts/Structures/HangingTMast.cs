using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HangingTMast : WorkBuilding
{
    private bool subscribedToRestoreBlockersEvent = false;
    private int lifepowerAffectionID = -1;
    private float lifepowerSurplus;
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
            var cpos = basement.GetLookingPosition();
            if (cpos != basement.pos) chunk.CreateBlocker(cpos, this, false, false);
            chunk.CreateBlocker(cpos + basement.GetLookVector(), this, false, false);
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

    private void RecalculateLifepowerSurplus()
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
            float newLPSurplus = 0f;
            if (workersCount < maxWorkers / 2)
            {
                newLPSurplus = workersCount * LIFEPOWER_PER_UNIT;
            }
            else
            {
                int d = workersCount - maxWorkers/2;
                newLPSurplus = maxWorkers / 2 * LIFEPOWER_PER_UNIT + d * LIFEPOWER_PER_UNIT * 1.1f;
            }
            if (newLPSurplus != lifepowerSurplus)
            {
                if (lifepowerAffectionID == -1)
                    lifepowerAffectionID = basement.myChunk.GetNature().AddLifepowerAffection(newLPSurplus);
                else basement.myChunk.GetNature().ChangeLifepowerAffection(lifepowerAffectionID, newLPSurplus);
            }
        }
    }
    public override float GetWorkSpeed()
    {
        return lifepowerSurplus;
    }

    override public int AddWorkers(int x)
    {
        var w = base.AddWorkers(x);
        RecalculateLifepowerSurplus();
        return w;
    }
    override public void FreeWorkers(int x)
    {
        base.FreeWorkers(x);
        RecalculateLifepowerSurplus();
    }

    #region save-load
    #endregion
}
