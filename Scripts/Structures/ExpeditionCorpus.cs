public sealed class ExpeditionCorpus : WorkBuilding {
    public static ExpeditionCorpus current;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        if (current != null)
        {
            var c = current;
            current = null;
            c.Annihilate(StructureAnnihilationOrder.ManualDestructed);
        }
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        var observer = colony.observer;
        if (observer != null && !observer.activeFastButtons.Contains(ID)) observer.AddFastButton(this);
        current = this;
    }

    public override UIObserver ShowOnGUI()
    {
        var wo = base.ShowOnGUI();
        colony.observer?.ChangeActiveWindow(ActiveWindowMode.ExpeditionPanel);
        return wo;
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(order);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        if (current == this)
        {
            current = null;
            if (order.doSpecialChecks)
            {
                var observer = colony.observer;
                if (observer != null && observer.activeFastButtons.Contains(ID))
                {
                    observer.RemoveFastButton(this);
                }
            }
        }
        Destroy(gameObject);
    }
}

