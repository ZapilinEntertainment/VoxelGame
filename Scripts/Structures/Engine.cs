using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : Building
{
    private int engineID = -1;
    public const float THRUST = 1f; 
    override protected void SwitchActivityState()
    {
        if (engineID == -1) engineID = GameMaster.realMaster.colonyController?.AddEngine(this) ?? -1;
        ChangeRenderersView(isActive & isEnergySupplied);        
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (engineID != -1)
        {
            GameMaster.realMaster.colonyController?.RemoveEngine(engineID);
        }
        Destroy(gameObject);
    }
}
