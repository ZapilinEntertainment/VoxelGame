using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExpeditionCorpus : WorkBuilding {
    public static List<ExpeditionCorpus> expeditionCorpusesList { get; private set; }

    static ExpeditionCorpus() {
        expeditionCorpusesList = new List<ExpeditionCorpus>();
    }
    public static void ResetToDefaults_Static_ExpeditionCorpus()
    {
        expeditionCorpusesList = new List<ExpeditionCorpus>();
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (!expeditionCorpusesList.Contains(this)) expeditionCorpusesList.Add(this);
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
        if (expeditionCorpusesList.Contains(this)) expeditionCorpusesList.Remove(this);
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}

