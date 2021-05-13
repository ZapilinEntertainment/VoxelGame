public class ConnectTower : Building {
    public static ConnectTower current { get; private set; }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(StructureAnnihilationOrder.ManualDestructed);
        current = this;
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(order);
        if (current == this) current = null;
        Destroy(gameObject);
    }
}
