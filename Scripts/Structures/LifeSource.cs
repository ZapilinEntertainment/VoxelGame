using UnityEngine;
using System.Collections.Generic;

public sealed class LifeSource : Structure {
    private bool subscribedToRestoreBlockersUpdate = false;
    private int tick = 0, lifepowerPerTick = 50;
    const int MAXIMUM_TICKS = 1000;
    public const float MAX_HP = 25000;
    private List<Block> dependentBlocks;

    public override bool CanBeRotated()
    {
        return false;
    }

    override public void SetBasement(Plane sb, PixelPosByte pos) {
		if (sb == null) return;
		SetStructureData(sb,pos);        
        if (!GameMaster.loading) { 
            // #blocking
            Chunk chunk = basement.myChunk;
            ChunkPos cpos = basement.pos; 
            void CheckAndCreate(in ChunkPos position)
            {
                Plane p;
                Block b = chunk.GetBlock(position);
                if (b == null)
                {
                    b = chunk.AddBlock(position, ResourceType.DIRT_ID, true, true);
                    if (b == null) return;
                }
                p = b.FORCED_GetPlane(Block.UP_FACE_INDEX);
                if (p != null)
                {
                    if (chunk.IsUnderOtherBlock(p))
                    {
                        chunk.DeleteBlock(position.OneBlockForward(), BlockAnnihilationOrder.SystemDestruction);
                    }
                    var g = chunk.InitializeNature()?.CreateGrassland(p, 2000f);
                    g?.SYSTEM_UseLifepower();
                    p.BlockByStructure(this,false);
                }
            }
            ChunkPos cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z + 1);  if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x, cpos.y, cpos.z + 1);  if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z + 1); if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z); if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z); if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x - 1, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndCreate(cpos2);
            cpos2 = new ChunkPos(cpos.x + 1, cpos.y, cpos.z - 1); if (cpos2.isOkay) CheckAndCreate(cpos2);
            //
            byte x = basement.pos.x, y = (byte)(basement.pos.y + 1), z = basement.pos.z;
            if (dependentBlocks != null)
            {
                chunk.ClearBlockersList(this, dependentBlocks, true);
            }
            dependentBlocks = new List<Block>();
            var positions = new List<ChunkPos>
            {
                new ChunkPos(x - 1, y, z + 1), new ChunkPos(x , y, z + 1), new ChunkPos( x + 1, y, z + 1),
                new ChunkPos(x - 1, y, z), new ChunkPos(x , y, z), new ChunkPos( x + 1, y, z),
                new ChunkPos(x - 1, y, z - 1), new ChunkPos(x , y, z - 1), new ChunkPos( x + 1, y, z - 1),
                new ChunkPos(x - 1, y+ 1, z + 1), new ChunkPos(x , y+ 1, z + 1), new ChunkPos( x + 1, y+ 1, z + 1),
                new ChunkPos(x - 1, y+ 1, z), new ChunkPos(x , y+ 1, z), new ChunkPos( x + 1, y+ 1, z),
                new ChunkPos(x - 1, y+ 1, z - 1), new ChunkPos(x , y+ 1, z - 1), new ChunkPos( x + 1, y+ 1, z - 1),
            };
            chunk.BlockRegion(positions, this, ref dependentBlocks);
            //
        }
        else
        {
            if (!subscribedToRestoreBlockersUpdate)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersUpdate = true;
            }
        }
        basement.myChunk.InitializeNature().AddLifesource(this);
        basement.ChangeMaterial(PoolMaster.MATERIAL_GRASS_100_ID, true);
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersUpdate)
        {
            // #blocking
            Chunk chunk = basement.myChunk;
            byte x = basement.pos.x, y = (byte)(basement.pos.y + 1), z = basement.pos.z;
            if (dependentBlocks != null)
            {
                chunk.ClearBlockersList(this, dependentBlocks, true);
            }
            dependentBlocks = new List<Block>();
            var positions = new List<ChunkPos>
            {
                new ChunkPos(x - 1, y, z + 1), new ChunkPos(x , y, z + 1), new ChunkPos( x + 1, y, z + 1),
                new ChunkPos(x - 1, y, z), new ChunkPos(x , y, z), new ChunkPos( x + 1, y, z),
                new ChunkPos(x - 1, y, z - 1), new ChunkPos(x , y, z - 1), new ChunkPos( x + 1, y, z - 1),
                new ChunkPos(x - 1, y+ 1, z + 1), new ChunkPos(x , y+ 1, z + 1), new ChunkPos( x + 1, y+ 1, z + 1),
                new ChunkPos(x - 1, y+ 1, z), new ChunkPos(x , y+ 1, z), new ChunkPos( x + 1, y+ 1, z),
                new ChunkPos(x - 1, y+ 1, z - 1), new ChunkPos(x , y+ 1, z - 1), new ChunkPos( x + 1, y+ 1, z - 1),
            };
            chunk.BlockRegion(positions, this, ref dependentBlocks);
            //
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;        
        PrepareStructureForDestruction(order);
        if (basement != null && order.doSpecialChecks)
        {
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

            basement.myChunk.InitializeNature().RemoveLifesource(this);
            basement.ChangeMaterial(ResourceType.DIRT_ID, true);
            if (GameMaster.realMaster.gameMode != GameMode.Editor)
            {               
                switch (ID)
                {
                    case TREE_OF_LIFE_ID:
                        {
                            HarvestableResource hr = HarvestableResource.ConstructContainer(ContainerModelType.DeadTreeOfLife, ResourceType.Lumber, 5000);
                            hr.SetModelRotation(modelRotation);
                            hr.SetBasement(basement, new PixelPosByte(hr.surfaceRect.x, hr.surfaceRect.z));
                            break;
                        }
                    case LIFESTONE_ID:
                        {
                            HarvestableResource hr = HarvestableResource.ConstructContainer(ContainerModelType.DeadLifestone, ResourceType.Stone, 5000);
                            hr.SetModelRotation(modelRotation);
                            hr.SetBasement(basement, new PixelPosByte(hr.surfaceRect.x, hr.surfaceRect.z));
                            break;
                        }
                }
            }
            if (dependentBlocks != null)
            {
                basement.myChunk.ClearBlockersList(this, dependentBlocks, true);
            }
        }
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        Destroy(gameObject);
    }
}
