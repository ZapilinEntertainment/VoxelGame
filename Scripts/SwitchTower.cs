using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTower : Building {

    override public UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
        if (GameMaster.layerCutHeight != basement.pos.y)
        {
            if (UIController.current.showLayerCut) UIController.current.LayerCutButton();
            GameMaster.layerCutHeight = basement.pos.y;
            basement.myChunk.LayersCut();
            //UI.current.showLayerCutButtons = true;
        }
        return buildingObserver;
    }

    override public void DisableGUI()
    {
        if (UIController.current.showLayerCut) UIController.current.LayerCutButton();
        showOnGUI = false;
        GameMaster.layerCutHeight = Chunk.CHUNK_SIZE;
        basement.myChunk.LayersCut();

    }
}
