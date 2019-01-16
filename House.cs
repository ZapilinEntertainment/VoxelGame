
public class House : Building {
	public int housing { get; protected set; }
    public const int TENT_VOLUME = 2;

    public static int GetHousingValue (int id)
    {
        switch (id)
        { // if changing - look at localized description
            case LANDED_ZEPPELIN_ID: return 10;
            case HQ_2_ID: return 30; 
            case HQ_3_ID: return 40;
            case HQ_4_ID: return 45;
            case TENT_ID: return TENT_VOLUME;
            case HOUSE_1_ID: return 10;
            case HOUSE_2_ID: return 50; 
            case HOUSE_3_ID: return 100; 
            case HOUSE_5_ID: return 800; 
            case HOTEL_BLOCK_6_ID: return 600; // temporary
            case HOUSING_MAST_6_ID: return 2200;
            default: return 1;
        }
    }

    override public void Prepare() {
        PrepareBuilding();
        housing = GetHousingValue(id);
    }

    public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        //#
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
