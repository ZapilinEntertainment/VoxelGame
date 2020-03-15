
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
            if (!GameMaster.loading) SetBlockersForHousingMast();
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
    private void SetBlockersForHousingMast()
    {
        if (basement != null)
        {
            var c = basement.myChunk;
            var bpos = basement.pos.OneBlockHigher();
            c.CreateBlocker(bpos, this, false);
            c.CreateBlocker(bpos.OneBlockHigher(), this, false);
        }
        else UnityEngine.Debug.LogError("house cannot set blockers - no basement set");
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersUpdate)
        {
            SetBlockersForHousingMast();
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
    }

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        GameMaster.realMaster.colonyController.housingRecalculationNeeded = true;
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
            basement.myChunk.GetBlock(bpos.OneBlockHigher())?.DropBlockerLink(this);
            basement.myChunk.GetBlock(bpos.TwoBlocksHigher())?.DropBlockerLink(this);
        }
        if (subscribedToRestoreBlockersUpdate)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersUpdate = false;
        }
        Destroy(gameObject);
    }
}
