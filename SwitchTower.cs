using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTower : Building {

	public override void SetGUIVisible (bool x) {
		if (x == true ) {
			if (showOnGUI == false) {
				if (GameMaster.layerCutHeight != basement.pos.y) {
					GameMaster.layerCutHeight = basement.pos.y ;
					basement.myChunk.LayersCut();
					UI.current.showLayerCutButtons = true;
				}
			}
		}
		showOnGUI = x;
	}
}
