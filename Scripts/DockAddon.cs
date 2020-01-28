public class DockAddon : Building {

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);

        Chunk c = basement.myChunk;
        int x = basement.pos.x, y = basement.pos.y, z = basement.pos.z;

        Block nearblock = c.GetBlock(x, y, z + 1);
        Plane nearSurfaceBlock = nearblock as Plane;
        Dock d;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null )
            {
                d.CheckAddons(nearSurfaceBlock);
            }
        }

        nearblock = c.GetBlock(x + 1, y, z );
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null )
            {
                d.CheckAddons(nearSurfaceBlock);
            }
        }


        nearblock = c.GetBlock(x, y, z - 1);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null )
            {
                d.CheckAddons(nearSurfaceBlock);
            }
        }

        nearblock = c.GetBlock(x - 1, y, z);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null)
            {
                d.CheckAddons(nearSurfaceBlock);
            }
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        Chunk c = basement.myChunk;
        int x = basement.pos.x, y = basement.pos.y, z = basement.pos.z;
        PrepareBuildingForDestruction(clearFromSurface,returnResources,leaveRuins);        

        Block nearblock = c.GetBlock(x, y, z + 1);
        Plane nearSurfaceBlock = nearblock as Plane;
        Dock d;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null) d.CheckAddons(d.basement);
        }

        nearblock = c.GetBlock(x + 1, y, z);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null) d.CheckAddons(d.basement);
        }

        nearblock = c.GetBlock(x, y, z - 1);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null) d.CheckAddons(d.basement);
        }

        nearblock = c.GetBlock(x - 1, y, z);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            d = nearSurfaceBlock.structures[0] as Dock;
            if (d != null) d.CheckAddons(d.basement);
        }
        Destroy(gameObject);
    }
}
