
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

    public override void SetBasement(Plane b, PixelPosByte pos) {		
		if (b == null) return;
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        if (ID == HOUSING_MAST_6_ID )
        {
            if (!GameMaster.loading)
            {
                // # set blockers
                var bpos = basement.pos;
                basement.myChunk.AddBlock(bpos.OneBlockHigher(), this, true, true, true);
                basement.myChunk.AddBlock(bpos.TwoBlocksHigher(), this, true, true, true);
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
            var bpos = basement.pos;
            basement.myChunk.AddBlock(bpos.OneBlockHigher(), this, true, true, true);
            basement.myChunk.AddBlock(bpos.TwoBlocksHigher(), this, true, true, true);
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

    override public bool CheckSpecialBuildingCondition(Plane p, ref string reason)
    {
        if (p.materialID != PoolMaster.MATERIAL_ADVANCED_COVERING_ID)
        {
            reason = Localization.GetRefusalReason(RefusalReason.MustBeBuildedOnFoundationBlock);
            return false;
        }
        else return true;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface,returnResources,leaveRuins);
        GameMaster.realMaster.colonyController.DeleteHousing(this);
        if (ID == HOUSING_MAST_6_ID & basement != null)
        {
            var bpos = basement.pos;
            basement.myChunk.GetBlock(bpos.OneBlockHigher())?.RemoveMainStructureLink(this);
            basement.myChunk.GetBlock(bpos.TwoBlocksHigher())?.RemoveMainStructureLink(this);
        }
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        Destroy(gameObject);
    }
}
