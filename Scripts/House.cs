
public class House : Building {
	public int housing { get; protected set; }
    public const int TENT_VOLUME = 2;
    private bool subscribedToRestoreBlockersUpdate = false;

    public static int GetHousingValue (int id)
    {
        switch (id)
        { // if changing - look at localized description            
            case TENT_ID: return TENT_VOLUME;
            case HOUSE_BLOCK_ID: return 1100; 
            case HOUSING_MAST_6_ID: return 2200;
            case SETTLEMENT_CENTER_ID: return 0;
            default: return 1;
        }
    }

    override public void Prepare() {
        PrepareBuilding();
        housing = GetHousingValue(ID);
    }

    public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        if (ID == HOUSING_MAST_6_ID )
        {
            if (!GameMaster.loading)
            {
                // # set blockers
                int x = basement.pos.x, y = basement.pos.y, z = basement.pos.z;
                basement.myChunk.BlockByStructure(x, y + 1, z, this);
                basement.myChunk.BlockByStructure(x, y + 2, z, this);
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
        }
        //#
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersUpdate)
        {
            // # set blockers
            int x = basement.pos.x, y = basement.pos.y, z = basement.pos.z;
            basement.myChunk.BlockByStructure(x, y + 1, z, this);
            basement.myChunk.BlockByStructure(x, y + 2, z, this);
            //
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
    }

    override public void SetActivationStatus(bool x, bool recalculateAfter) {
		if (isActive == x) return;
		isActive = x;
        ColonyController colony = GameMaster.realMaster.colonyController;
        if (connectedToPowerGrid & recalculateAfter) colony.RecalculatePowerGrid();
        colony.RecalculateHousing();
		ChangeRenderersView(x);
	}

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface,returnResources,leaveRuins);
        GameMaster.realMaster.colonyController.DeleteHousing(this);
        if (ID == HOUSING_MAST_6_ID & basement != null)
        {
            int x = basement.pos.x, y = basement.pos.y, z = basement.pos.z;
            var b = basement.myChunk.GetBlock(x, y + 1, z);
            if (b != null && b.type == BlockType.Shapeless && b.mainStructure == this) basement.myChunk.DeleteBlock(new ChunkPos(x, y + 1, z));
            b = basement.myChunk.GetBlock(x, y + 2, z);
            if (b != null && b.type == BlockType.Shapeless && b.mainStructure == this) basement.myChunk.DeleteBlock(new ChunkPos(x, y + 2, z));
        }
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        Destroy(gameObject);
    }
}
