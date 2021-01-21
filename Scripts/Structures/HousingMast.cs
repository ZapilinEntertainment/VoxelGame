using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingMast : House
{
    private bool subscribedToRestoreBlockersUpdate = false;

    public override void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        if (!GameMaster.loading) SetBlockersForHousingMast();
        else
        {
            if (!subscribedToRestoreBlockersUpdate)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersUpdate = true;
            }
        }
    }

    public static  bool CheckForSpace(Plane p)
    {
        if (p == null || p.destroyed) return false;
        else
        {
            if (p.isTerminal) return true;
            else
            {
                var c = p.myChunk;
                var pos = p.pos.OneBlockHigher();
                var b = c.GetBlock(pos);
                if (b == null )
                {
                    b = c.GetBlock(pos.OneBlockHigher());
                    if (b == null ) return true;
                    else return false;
                }
                else return false;
            }
        }
    }
    private void SetBlockersForHousingMast()
    {
        if (basement != null)
        {
            var c = basement.myChunk;
            var bpos = basement.pos.OneBlockHigher();
            c.CreateBlocker(bpos, this, false, false);
            c.CreateBlocker(bpos.OneBlockHigher(), this, false, false);
        }
        else UnityEngine.Debug.LogError("housing mast cannot set blockers - no basement set");
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersUpdate)
        {
            SetBlockersForHousingMast();
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        GameMaster.realMaster.colonyController.DeleteHousing(this);
        if (basement != null)
        {
            var bpos = basement.pos;
            basement.myChunk.GetBlock(bpos.OneBlockHigher())?.DropBlockerLink(this);
            basement.myChunk.GetBlock(bpos.TwoBlocksHigher())?.DropBlockerLink(this);
        }
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        Destroy(gameObject);
    }
}
