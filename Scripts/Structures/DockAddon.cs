using System;
public class DockAddon : Building {

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        AddonCheckRequest(b.pos);
    }
    private void AddonCheckRequest(ChunkPos cpos)
    {
        ChunkPos dpos;
        var dlist = FindObjectsOfType<Dock>();
        int deltaX, deltaZ;
        if (dlist != null)
        {
            foreach (var d in dlist)
            {
                dpos = d.GetBlockPosition();
                if (dpos.y == cpos.y)
                {
                    deltaX = Math.Abs(dpos.x - cpos.x);
                    deltaZ = Math.Abs(dpos.z - cpos.z);
                    if ((deltaX == 1 & deltaZ == 0) || (deltaX == 0 & deltaZ == 1)) d.CheckAddons();
                }
            }
        }
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        bool initiateCheckRequest = true;
        ChunkPos cpos = ChunkPos.zer0;
        if (basement != null )  cpos = basement.pos;
        else initiateCheckRequest = false;
        PrepareBuildingForDestruction(order);
        if (initiateCheckRequest & order.doSpecialChecks) AddonCheckRequest(cpos);
        
        Destroy(gameObject);
    }
}
