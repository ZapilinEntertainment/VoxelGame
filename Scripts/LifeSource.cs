using UnityEngine;
using System.Collections.Generic;

public sealed class LifeSource : Structure {
    private int tick = 0, lifepowerPerTick = 500;
    const int MAXIMUM_TICKS = 1000;
    public const float MAX_HP = 25000;
    private List<Block> dependentBlocks;

    override public void SetBasement(SurfaceBlock sb, PixelPosByte pos) {
		if (sb == null) return;
		SetStructureData(sb,pos);        
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.lifepowerUpdateEvent += LifepowerUpdate;
            subscribedToUpdate = true;
        }
        { //blocking
            Chunk chunk = basement.myChunk;
            byte x = basement.pos.x, y = (byte)(basement.pos.y + 1), z = basement.pos.z;
            if (dependentBlocks != null)
            {
                chunk.ClearBlocksList(dependentBlocks, true);
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
        }
    }

	public void LifepowerUpdate () {
        tick++;
        basement.myChunk.AddLifePower(lifepowerPerTick);
        if (tick == MAXIMUM_TICKS & !destroyed)
        { // dry
            Annihilate(true, false, false);
        }
	}



    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        PrepareStructureForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.lifepowerUpdateEvent -= LifepowerUpdate;
            subscribedToUpdate = false;
        }
        if (basement != null )
        {
            if (!GameMaster.editMode)
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
                basement.myChunk.ClearBlocksList(dependentBlocks, true);
            }
        }
        Destroy(gameObject);
    }
}
