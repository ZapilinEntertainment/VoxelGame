using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : Building
{
    public enum ThrustDirection : byte { Offline, Clockwise, CounterclockWise, Inside, Outward, StabilityManeuver}

    private int engineID = -1;
    private bool subscribedToRestoreBlockersUpdate = false;
    public const float THRUST = 1f; 

    override protected void SwitchActivityState()
    {
        if (engineID == -1) engineID = GameMaster.realMaster.InitializeGlobalMap().AddEngine(this);
        ChangeRenderersView(isActive & isEnergySupplied);        
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
                var pos = p.pos.OneBlockDown();
                var b = c.GetBlock(pos);
                if (b == null )
                {
                    pos = pos.OneBlockDown();
                    b = c.GetBlock(pos);
                    if (b == null ) {
                        b = c.GetBlock(pos.OneBlockDown());
                        if (b == null ) return true;
                        else return false;
                    }
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
            var bpos = basement.pos.OneBlockDown();
            c.CreateBlocker(bpos, this, false, false);
            bpos = bpos.OneBlockDown();
            c.CreateBlocker(bpos, this, false, false);
            c.CreateBlocker(bpos.OneBlockDown(), this,false, false);
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

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (order.doSpecialChecks)
        {
            if (engineID != -1)
            {
                GameMaster.realMaster.globalMap.RemoveEngine(engineID);
            }
            if (basement != null)
            {
                var bpos = basement.pos;
                basement.myChunk.GetBlock(bpos.OneBlockHigher())?.DropBlockerLink(this);
                basement.myChunk.GetBlock(bpos.TwoBlocksHigher())?.DropBlockerLink(this);
            }
        }
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        base.Annihilate(order);
    }
}
