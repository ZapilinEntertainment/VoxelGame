public class XStation : WorkBuilding {
    public static XStation current { get; private set; }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(true, true, false);
        current = this;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (current == this) current = null;
        Destroy(gameObject);
    }
}
