public class ConnectTower : Building {
    public static ConnectTower current { get; private set; }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (current != null & current != this) current.Annihilate(true, true, false);
        current = this;
    }

    new public static bool CheckSpecialBuildingConditions(Plane p, ref string reason)
    {
        if (p.materialID != PoolMaster.MATERIAL_ADVANCED_COVERING_ID)
        {
            reason = Localization.GetRefusalReason(RefusalReason.MustBeBuildedOnFoundationBlock);
            return false;
        }
        else return true;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (current == this) current = null;
        Destroy(gameObject);
    }
}
