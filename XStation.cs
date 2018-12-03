public class XStation : WorkBuilding {
    public static XStation current { get; private set; }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(false);
        current = this;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(forced);
        if (current == this) current = null;
        Destroy(gameObject);
    }
}
