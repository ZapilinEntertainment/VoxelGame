using UnityEngine;

public sealed class LifeSource : MultiblockStructure {
    private int tick = 0, lifepowerPerTick = 500;
    const int MAXIMUM_TICKS = 1000;

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
        if (tick == MAXIMUM_TICKS)
        { // dry
            GameMaster.realMaster.lifepowerUpdateEvent -= LifepowerUpdate;
            subscribedToUpdate = false;
            Transform model, mesh;
            switch (id)
            {
                case TREE_OF_LIFE_ID:
                    {
                        model = transform.GetChild(0).GetChild(0);
                        mesh = model.GetChild(1);
                        mesh.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.DeadLumber, mesh.GetComponent<MeshFilter>(), 1);
                        Destroy(model.GetChild(0).gameObject);
                        break;
                    }
                case LIFESTONE_ID:
                    {
                        model = transform.GetChild(0).GetChild(0);
                        for (int i = 0; i < model.childCount; i++)
                        {
                            mesh = model.GetChild(i);
                            if (mesh.GetComponent<MeshRenderer>().sharedMaterial == PoolMaster.energy_material)
                                mesh.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.energy_offline_material;
                            else mesh.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.Stone, mesh.GetComponent<MeshFilter>(), 1);
                        }
                        break;
                    }
            }
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
