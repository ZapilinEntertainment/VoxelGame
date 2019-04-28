public sealed class ExpeditionCorpus : WorkBuilding {
    public static ExpeditionCorpus current;

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        if (current != null)
        {
            var c = current;
            current = null;
            c.Annihilate(false);
        }
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (!UIController.current.activeFastButtons.Contains(id)) UIController.current.AddFastButton(this);
        current = this;
    }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        UIController.current.ChangeActiveWindow(ActiveWindowMode.ExpeditionPanel);
        return workbuildingObserver;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        if (current == this)
        {
            current = null;
            if (UIController.current.activeFastButtons.Contains(id))
            {
                UIController.current.RemoveFastButton(this);
            }
        }
        Destroy(gameObject);
    }
}

