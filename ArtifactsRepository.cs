using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactsRepository : Building
{
    public const int ARTIFACT_SLOTS = 16;
    private Artifact[] containingArtifacts;

    override public void SetActivationStatus(bool x, bool recalculateAfter)
    {
        isActive = x;
        // тесты артефактов
        if (connectedToPowerGrid & recalculateAfter)
        {
            GameMaster.realMaster.colonyController.RecalculatePowerGrid();
        }
        ChangeRenderersView(x & isEnergySupplied);
    }

    override public void SetEnergySupply(bool x, bool recalculateAfter)
    {
        isEnergySupplied = x;
        // тесты артефактов
        if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.RecalculatePowerGrid();
        ChangeRenderersView(x & isActive);
    }

    override public UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;


        // open artifacts window
        return buildingObserver;
    }
    override public void DisableGUI()
    {
        showOnGUI = false;
        //disable artifacts window
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(forced);
        basement = null;
        // тест на прочность для артефактов, дропнуть оставшиеся
        Destroy(gameObject);
    }
}
