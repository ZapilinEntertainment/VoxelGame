using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observatory : WorkBuilding {
    public static bool alreadyBuilt = false;
    public const float SEARCH_WORKFLOW = 250, CHANCE_TO_FIND = 0.3f;
    private bool mapOpened = false;    
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

    protected override void LabourResult()
    {
        workflow = 0;
        // new object searched
        float f = Random.value;
        if (Random.value <= CHANCE_TO_FIND)
        {
            if (GameMaster.realMaster.globalMap.Search())
            {
                // visual effect
            }
        }
    }
    override public void RecalculateWorkspeed()
    {
        workSpeed = (colony.gears_coefficient + colony.health_coefficient + colony.happiness_coefficient - 2) * GameConstants.OBSERVATORY_FIND_SPEED_CF * (workersCount / (float)maxWorkers);
        gearsDamage = 0;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
        if (basement != null)
        {
            if (blockedBlocks != null)
            {
                basement.myChunk.ClearBlocksList(blockedBlocks, true);
            }
            if (basement.type == BlockType.Surface) basement.myChunk.DeleteBlock(basement.pos);
        }        
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
