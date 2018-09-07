public class ChemicalFactory : WorkBuilding {
    public static ChemicalFactory current { get; private set; }

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(false);
        current = this;
	}

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
        if (current == this) current = null;
    }
}
