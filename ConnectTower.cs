using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectTower : Building {
    public static ConnectTower current { get; private set; }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(false);
        current = this;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(forced);
        if (current == this) current = null;
        Destroy(gameObject);
    }

}
