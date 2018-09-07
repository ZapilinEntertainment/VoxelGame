using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTower : Building {

    public override UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
        if (GameMaster.layerCutHeight != basement.pos.y)
        {
            GameMaster.layerCutHeight = basement.pos.y;
            basement.myChunk.LayersCut();
            //UI.current.showLayerCutButtons = true;
        }
        return buildingObserver;
    }
}
