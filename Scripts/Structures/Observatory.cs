using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Observatory : WorkBuilding
{
    public static bool alreadyBuilt = false;
    public const float CHANCE_TO_FIND = 0.3f;
    private bool mapOpened = false, subscribedToRestoreBlockersUpdate = false;
    private List<Block> blockedBlocks;

    static Observatory()
    {
        GameMaster.staticResetFunctions += ResetObservatoryStaticData;
    }
    public static void ResetObservatoryStaticData()
    {
        alreadyBuilt = false;
    }
    public static float GetVisibilityCoefficient()
    {
        return 1f;
    }
    public static bool CheckSpecialBuildingCondition(Plane p, ref string reason)
    {
        if (alreadyBuilt)
        {
            reason = Localization.GetRefusalReason(RefusalReason.AlreadyBuilt);
            return false;
        }
        else
        {
            if (p.pos.y < Chunk.chunkSize / 2)
            {
                reason = Localization.GetRefusalReason(RefusalReason.UnacceptableHeight);
                return false;
            }
            else
            {
                if (!p.GetBlock().HavePlane(Block.CEILING_FACE_INDEX))
                {
                    var pos = p.pos;
                    if (pos.y < Chunk.chunkSize - 1)
                    {
                        if (p.faceIndex == Block.UP_FACE_INDEX) pos = pos.OneBlockHigher();
                        var chunk = p.myChunk;
                        var pos2 = pos.OneBlockForward();
                        Block b;
                        bool Blocked(in ChunkPos cpos)
                        {
                            b = chunk.GetBlock(cpos);
                            if (b == null) return chunk.IsAnyStructureInABlockSpace(cpos);
                            else
                            {
                                return (!b.IsBlocker() && !b.IsSurface());
                            }
                        }
                        if (Blocked(pos2.OneBlockLeft())) goto CHECK_FAILED;
                        if (Blocked(pos2)) goto CHECK_FAILED;
                        if (Blocked(pos2.OneBlockRight())) goto CHECK_FAILED;
                        if (Blocked(pos.OneBlockLeft())) goto CHECK_FAILED;
                        if (Blocked(pos.OneBlockRight())) goto CHECK_FAILED;
                        pos2 = pos.OneBlockBack();
                        if (Blocked(pos2.OneBlockLeft())) goto CHECK_FAILED;
                        if (Blocked(pos2)) goto CHECK_FAILED;
                        if (Blocked(pos2.OneBlockRight())) goto CHECK_FAILED;
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
        if (!subscribedToChunkUpdate)
        {
            basement.myChunk.ChunkUpdateEvent += this.ChunkUpdated;
            subscribedToChunkUpdate = true;
        }
        if (!GameMaster.loading)
        {
            Chunk chunk = basement.myChunk;
            ChunkPos cpos = basement.pos;
            Plane p;
            void CheckAndBlock(in ChunkPos position)
            {
                Block bx = chunk.GetBlock(position);
                if (bx != null && bx.TryGetPlane(Block.UP_FACE_INDEX, out p)) p.BlockByStructure(this);
            }
            ChunkPos cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndBlock(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndBlock(cpos2);
            //
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
    public override void ChunkUpdated()
    {
        if (GameMaster.loading) return;
        // проверка на блокирование вновь появившихся блоков окружения

    }

    protected override void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        // new object searched
        if (iterations == 1)
        {
            if (Random.value <= CHANCE_TO_FIND)
            {
                if (GameMaster.realMaster.globalMap.Search())
                {
                    // visual effect
                }
            }
        }
        else
        {
            for (int i = 0; i < iterations; i++)
            {

                if (Random.value <= CHANCE_TO_FIND)
                {
                    GameMaster.realMaster.globalMap.Search();
                }
            }
        }
    }    

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(order);
        if (basement != null && order.doSpecialChecks)
        {
            if (blockedBlocks != null)
            {
                basement.myChunk.ClearBlockersList(this, blockedBlocks, true);
            }
            Chunk chunk = basement.myChunk;
            ChunkPos cpos = basement.pos;
            Plane p;
            void CheckAndUnblock(in ChunkPos position)
            {
                Block bx = chunk.GetBlock(position);
                if (bx != null && bx.TryGetPlane(Block.UP_FACE_INDEX, out p)) p.UnblockFromStructure(this);
            }
            ChunkPos cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndUnblock(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndUnblock(cpos2);

            if (subscribedToChunkUpdate)
            {
                chunk.ChunkUpdateEvent -= this.ChunkUpdated;
                subscribedToChunkUpdate = false;
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
       
        if (order.doSpecialChecks) colony?.observer?.RemoveFastButton(this);
        alreadyBuilt = false;
        Destroy(gameObject);
    }
}
