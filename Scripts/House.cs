
public class House : Building {
	public int housing { get; protected set; }
    public const int TENT_VOLUME = 2;

    public static int GetHousingValue (int id)
    {
        switch (id)
        { // if changing - look at localized description            
            case TENT_ID: return TENT_VOLUME;
            case HOUSE_BLOCK_ID: return 4100; 
            case HOTEL_BLOCK_6_ID: return 600; // temporary
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface,returnResources,leaveRuins);
        GameMaster.realMaster.colonyController.DeleteHousing(this);
        Destroy(gameObject);
    }
}
