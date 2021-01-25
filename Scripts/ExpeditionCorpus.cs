﻿public sealed class ExpeditionCorpus : WorkBuilding {
    public static ExpeditionCorpus current;

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        if (current != null)
        {
            var c = current;
            current = null;
            c.Annihilate(true, true, false);
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
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        colony.observer?.ChangeActiveWindow(ActiveWindowMode.ExpeditionPanel);
        return workbuildingObserver;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        if (current == this)
        {
            current = null;
            var observer = colony.observer;
            if (observer != null && observer.activeFastButtons.Contains(ID))
            {
                observer.RemoveFastButton(this);
            }
        }
        Destroy(gameObject);
    }
}

