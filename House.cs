
public class House : Building {
	public int housing { get; protected set; }
    public const int TENT_VOLUME = 2;

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
	}
	protected void PrepareHouse(SurfaceBlock b, PixelPosByte pos) {
		SetBuildingData(b,pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        switch (id)
        {
            case LANDED_ZEPPELIN_ID: housing = 10;break;
            case HQ_2_ID: housing = 30;break;
            case HQ_3_ID: housing = 40;break;
            case HQ_4_ID: housing = 45;break;
            case TENT_ID: housing = TENT_VOLUME; break;
            case HOUSE_1_ID: housing = 10;break;
            case HOUSE_2_ID: housing = 50;break;
            case HOUSE_3_ID: housing = 100;break;
            case HOUSE_5_ID: housing = 800;break;
            case HOTEL_BLOCK_6_ID: housing = 600; break; // temporary
            case HOUSING_MAST_6_ID: housing = 2200; break;
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

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(forced);
        GameMaster.realMaster.colonyController.DeleteHousing(this);
        Destroy(gameObject);
    }
}
