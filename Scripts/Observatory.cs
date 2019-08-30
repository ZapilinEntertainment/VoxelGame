using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Observatory : WorkBuilding {
    public static bool alreadyBuilt = false;
    public const float SEARCH_WORKFLOW = 250, CHANCE_TO_FIND = 0.3f;
    private bool mapOpened = false, subscribedToRestoreBlockersUpdate = false;    
    private List<Block> blockedBlocks;

    static Observatory()
    {
        AddToResetList(typeof(Observatory));        
    }
    public static void ResetStaticData()
    {
        alreadyBuilt = false;
    }

    public static float GetVisibilityCoefficient()
    {
        return 1f;
    }
    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (alreadyBuilt) {
            Destroy(gameObject);
            return;
        } 
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }        
        if (!GameMaster.loading)
        {
            List<ChunkPos> positionsList = new List<ChunkPos>();
            int x = b.pos.x, z = b.pos.z, y = b.pos.y;
            positionsList = new List<ChunkPos>()
            {
                new ChunkPos(x - 1, y, z -1), new ChunkPos(x, y,z - 1), new ChunkPos(x + 1, y, z - 1),
                new ChunkPos(x - 1, y, z), new ChunkPos(x + 1, y,z),
                new ChunkPos(x - 1, y, z+1), new ChunkPos(x, y, z+1), new ChunkPos(x + 1, y, z + 1)
            };
            blockedBlocks = new List<Block>();
            b.myChunk.BlockRegion(positionsList, this, ref blockedBlocks);
        }
        else {
            if (!subscribedToRestoreBlockersUpdate)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersUpdate = true;
            }
        }
        UIController.current.AddFastButton(this);
        alreadyBuilt = true;
    }
    public void RestoreBlockers()
    {
        List<ChunkPos> positionsList = new List<ChunkPos>();
        int x = basement.pos.x, z = basement.pos.z, y = basement.pos.y;
        positionsList = new List<ChunkPos>()
            {
                new ChunkPos(x - 1, y, z -1), new ChunkPos(x, y,z - 1), new ChunkPos(x + 1, y, z - 1),
                new ChunkPos(x - 1, y, z), new ChunkPos(x + 1, y,z),
                new ChunkPos(x - 1, y, z+1), new ChunkPos(x, y, z+1), new ChunkPos(x + 1, y, z + 1)
            };
        blockedBlocks = new List<Block>();
        basement.myChunk.BlockRegion(positionsList, this, ref blockedBlocks);
        GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
        subscribedToRestoreBlockersUpdate = false;
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
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
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        UIController.current.RemoveFastButton(this);
        alreadyBuilt = false;
        Destroy(gameObject);
    }
}
