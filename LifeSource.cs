using UnityEngine;

public sealed class LifeSource : MultiblockStructure {
    private int tick = 0, lifepowerPerTick = 500;
    const int MAXIMUM_TICKS = 1000;
    public const float MAX_HP = 25000;

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetMultiblockStructureData(b,pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.lifepowerUpdateEvent += LifepowerUpdate;
            subscribedToUpdate = true;
        }
	}

	public void LifepowerUpdate () {
        tick++;
        basement.myChunk.AddLifePower(lifepowerPerTick);
        if (tick == MAXIMUM_TICKS & !destroyed)
        { // dry
            destroyed= true;
            GameMaster.realMaster.lifepowerUpdateEvent -= LifepowerUpdate;
            subscribedToUpdate = false;
            PrepareStructureForDestruction(false);
            switch (id)
            {
                case TREE_OF_LIFE_ID:
                    {
                        HarvestableResource hr = HarvestableResource.ConstructContainer(ContainerModelType.DeadTreeOfLife, ResourceType.Lumber, 5000);
                        hr.SetModelRotation(modelRotation);
                        hr.SetBasement(basement, new PixelPosByte(hr.innerPosition.x, hr.innerPosition.z));
                        break;
                    }
                case LIFESTONE_ID:
                    {
                        HarvestableResource hr = HarvestableResource.ConstructContainer(ContainerModelType.DeadLifestone, ResourceType.Stone, 5000);
                        hr.SetModelRotation(modelRotation);
                        hr.SetBasement(basement, new PixelPosByte(hr.innerPosition.x, hr.innerPosition.z));
                        break;
                    }
            }
            Destroy(gameObject);
        }
	}



    override public void Annihilate(bool forced)
    {
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        PrepareStructureForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.lifepowerUpdateEvent -= LifepowerUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
