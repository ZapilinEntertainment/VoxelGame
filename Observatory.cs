using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observatory : WorkBuilding {
    public static bool alreadyBuilt = false;
    private bool mapOpened = false;
    public const float SEARCH_WORKFLOW = 100;
    private List<Block> blockedBlocks;

    public static void ResetBuiltMarker()
    {
        alreadyBuilt = false;
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        Chunk chunk = b.myChunk;
        Block lowerBlock = chunk.GetBlock(b.pos.x, b.pos.y - 1, b.pos.z);
        if (lowerBlock != null)
        {
            indestructible = true;
            chunk.DeleteBlock(lowerBlock.pos);
            indestructible = false;
        }        
        List<ChunkPos> positionsList = new List<ChunkPos>();
        int x = b.pos.x, z = b.pos.z;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j< 2; j++)
            {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++)
                {
                    positionsList.Add(new ChunkPos(x + i, k, z + j));
                }                
            }
        }
        positionsList.Remove(new ChunkPos(x, b.pos.y, z));
        positionsList.Remove(new ChunkPos(x, b.pos.y - 1, z));
        blockedBlocks = new List<Block>();
        chunk.BlockRegion(positionsList, this, ref blockedBlocks);
        UIController.current.AddFastButton(this);
        alreadyBuilt = true;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (blockedBlocks != null & basement != null)
        {
            basement.myChunk.ClearBlocksList(blockedBlocks, true);
        }
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        UIController.current.RemoveFastButton(this);
        alreadyBuilt = false;
        Destroy(gameObject);
    }
}
