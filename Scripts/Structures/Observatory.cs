using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Observatory : WorkBuilding
{
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
    new public static bool CheckSpecialBuildingCondition(Plane p, ref string reason)
    {
        if (alreadyBuilt)
        {
            reason = Localization.GetRefusalReason(RefusalReason.AlreadyBuilt);
            return false;
        }
        else
        {
            if (p.pos.y != Chunk.chunkSize - 2)
            {
                reason = Localization.GetRefusalReason(RefusalReason.UnacceptableHeight);
                return false;
            }
            else
            {
                if (!p.GetBlock().HavePlane(Block.CEILING_FACE_INDEX))
                {
                    var blocks = p.myChunk.blocks;
                    ChunkPos pos = p.pos;
                    if (p.faceIndex != Block.SURFACE_FACE_INDEX) pos = pos.OneBlockHigher();
                    int size = Chunk.chunkSize;

                    int i = 0;
                    if (pos.y < size - 1)
                    {
                        if (pos.y > 1)
                        {
                            for (; i < pos.y - 1; i++)
                            {
                                ChunkPos cpos = new ChunkPos(pos.x, i, pos.z);
                                if (blocks.ContainsKey(cpos) && blocks[cpos].IsCube()) goto CHECK_FAILED;
                            }
                        }
                        for (i = pos.y + 1; i < size; i++)
                        {
                            ChunkPos cpos = new ChunkPos(pos.x, i, pos.z);
                            if (blocks.ContainsKey(cpos) && blocks[cpos].IsCube()) goto CHECK_FAILED;
                        }
                        i = 0;
                    }
                    bool[] checkArray = new bool[] { true, true, true, true, true, true, true, true };
                    //  0  1  2
                    //  3     4
                    //  5  6  7
                    if (pos.x == 0)
                    {
                        checkArray[0] = false;
                        checkArray[3] = false;
                        checkArray[5] = false;
                    }
                    else
                    {
                        if (pos.x == size - 1)
                        {
                            checkArray[2] = false;
                            checkArray[4] = false;
                            checkArray[7] = false;
                        }
                    }
                    if (pos.z == 0)
                    {
                        checkArray[5] = false;
                        checkArray[6] = false;
                        checkArray[7] = false;
                    }
                    else
                    {
                        if (pos.z == size - 1)
                        {
                            checkArray[0] = false;
                            checkArray[1] = false;
                            checkArray[2] = false;
                        }
                    }
                    foreach (bool ca in checkArray)
                    {
                        if (ca == false) goto CHECK_FAILED;
                    }
                    return true;
                }
                CHECK_FAILED:
                reason = Localization.GetRefusalReason(RefusalReason.NoEmptySpace);
                return false;
            }
        }
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (alreadyBuilt)
        {
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
            int x = b.pos.x, z = b.pos.z, y = b.pos.y + 1;
            positionsList = new List<ChunkPos>()
            {
                new ChunkPos(x - 1, y, z -1), new ChunkPos(x, y,z - 1), new ChunkPos(x + 1, y, z - 1),
                new ChunkPos(x - 1, y, z), new ChunkPos(x + 1, y,z),
                new ChunkPos(x - 1, y, z+1), new ChunkPos(x, y, z+1), new ChunkPos(x + 1, y, z + 1)
            };
            blockedBlocks = new List<Block>();
            b.myChunk.BlockRegion(positionsList, this, ref blockedBlocks);
        }
        else
        {
            if (!subscribedToRestoreBlockersUpdate)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersUpdate = true;
            }
        }
        colony?.observer?.AddFastButton(this);
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

    override public void LabourUpdate()
    {
        if (!isActive | !isEnergySupplied) return;
        if (workersCount > 0)
        {
            workSpeed = GameConstants.OBSERVATORY_FIND_SPEED_CF * (workersCount / (float)maxWorkers);
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage * workSpeed;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
        else workSpeed = 0f;
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (basement != null)
        {
            if (blockedBlocks != null)
            {
                basement.myChunk.ClearBlocksList(this, blockedBlocks, true);
            }
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
        colony?.observer?.RemoveFastButton(this);
        alreadyBuilt = false;
        Destroy(gameObject);
    }
}
