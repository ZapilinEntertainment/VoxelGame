
public class House : Building {
	public int housing { get; protected set; }
    public const int TENT_VOLUME = 2;
    

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
    }      

    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        GameMaster.realMaster.colonyController.housingRecalculationNeeded = true;
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(order);
        if (order.doSpecialChecks)
        {
            GameMaster.realMaster.colonyController.DeleteHousing(this);
            if (ID == HOUSING_MAST_6_ID & basement != null)
            {
                var bpos = basement.pos;
                basement.myChunk.GetBlock(bpos.OneBlockHigher())?.DropBlockerLink(this);
                basement.myChunk.GetBlock(bpos.TwoBlocksHigher())?.DropBlockerLink(this);
            }
        }
        Destroy(gameObject);
    }
}
