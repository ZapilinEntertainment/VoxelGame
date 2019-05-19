public class ChemicalFactory : WorkBuilding {
    public static ChemicalFactory current { get; private set; }

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetWorkbuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(true, true, false);
        current = this;
        SetActivationStatus(false, true);
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface,returnResources, leaveRuins);
        if (current == this) current = null;
        Destroy(gameObject);
    }
}
