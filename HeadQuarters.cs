using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadQuarters : House {
	bool nextStageConditionMet = false;
	
	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		GameMaster.colonyController.SetHQ(this);
	}

	void Update() {
		if ( showOnGUI) {
			switch (level) {
			case 1:  
				nextStageConditionMet = (GameMaster.colonyController.docks.Count != 0); 
				break;
			case 2:  
				nextStageConditionMet = (GameMaster.colonyController.rollingShops.Count != 0);
				break;			
			}
		}
	}

	void OnGUI() {
		if ( !showOnGUI ) return;
		if (nextStage != null ) {
			Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
			if (nextStageConditionMet) {
				GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
				if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
					ResourceContainer[] requiredResources = new ResourceContainer[ResourcesCost.info[nextStage.resourcesContainIndex].Length];
					if (requiredResources.Length > 0) {
						for (int i = 0; i < requiredResources.Length; i++) {
							requiredResources[i] = new ResourceContainer(ResourcesCost.info[nextStage.resourcesContainIndex][i].type, ResourcesCost.info[nextStage.resourcesContainIndex][i].volume * (1 - GameMaster.upgradeDiscount));
						}
					}
					if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
					{
						Building upgraded = Instantiate(nextStage);
						upgraded.SetBasement(basement, PixelPosByte.zero);
					}
					else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
				}
			}
			else {
				Color c = GUI.color;
				GUI.color = Color.red;
				switch (level) {
				case 1: GUI.Label(rr, Localization.hq_refuse_reason_1, PoolMaster.GUIStyle_CenterOrientedLabel);break;
				case 2: GUI.Label(rr, Localization.hq_refuse_reason_2, PoolMaster.GUIStyle_CenterOrientedLabel);break;
				}
				GUI.color = c;
			}
			rr.y += rr.height;
			if ( ResourcesCost.info[ nextStage.resourcesContainIndex ].Length > 0) {
				for (int i = 0; i < ResourcesCost.info[ nextStage.resourcesContainIndex ].Length; i++) {
					GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.name);
					GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (ResourcesCost.info[ nextStage.resourcesContainIndex ][i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
					rr.y += rr.height;
				}
			}
		}
	}
}
