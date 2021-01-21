using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCenter : Building
{
    private bool subscribedToRestoreBlockersUpdate = false;


    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        base.SetBasement(b, pos);
        GameMaster.realMaster.globalMap?.RegisterEngineControlCenter(this);
    }

    public static bool CheckForSpace(Plane p)
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
    private void SetBlockers()
    {
        if (basement != null)
        {
            var c = basement.myChunk;
            var bpos = basement.pos.OneBlockHigher();
            c.CreateBlocker(bpos, this, false, false);
            c.CreateBlocker(bpos.OneBlockHigher(), this, false, false);
        }
        else Debug.LogError("engine cannot set blockers - no basement set");
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersUpdate)
        {
            SetBlockers();
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
    }

    public override void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (ID == CONTROL_CENTER_ID) GameMaster.realMaster.globalMap?.UnregisterEngineControlCenter(this);
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
        base.Annihilate(clearFromSurface, returnResources, leaveRuins);
    }
}
