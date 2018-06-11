using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadQuarters : House {
	bool nextStageConditionMet = false;
	ColonyController colony;
	
	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		colony = GameMaster.colonyController;
		colony.SetHQ(this);
	}

	void Update() {
		if ( showOnGUI && colony != null) {
			switch (level) {
			case 1:  
				nextStageConditionMet = (colony.docks.Count != 0); 
				break;
			case 2:  
				nextStageConditionMet = (colony.rollingShops.Count != 0);
				break;		
			case 3:
				nextStageConditionMet = (colony.graphoniumEnrichers.Count != 0);
				break;
			case 4:
				nextStageConditionMet = (colony.chemicalFactories.Count != 0);
				break;
			}
		}
	}

	void OnGUI() {
		if ( !showOnGUI ) return;
		if (level < 7) {
				Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
				if (nextStageConditionMet) {
					GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
					if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
						if (level < 4) {
								if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
								{
									Building upgraded = Structure.GetNewStructure(upgradedIndex) as Building;
									upgraded.SetBasement(basement, PixelPosByte.zero);
								}
								else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
							}
							else { // building blocks on the top
								if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
								{
									Chunk chunk = basement.myChunk;
									ChunkPos upperPos = new ChunkPos( basement.pos.x, basement.pos.y + 1, basement.pos.z);
									Block upperBlock = chunk.GetBlock(basement.pos.x, basement.pos.y + 1, basement.pos.z);
									if (upperBlock == null) chunk.AddBlock(upperPos, BlockType.Surface, ResourceType.CONCRETE_ID);
									else {
										switch (upperBlock.type) {
										case BlockType.Shapeless:
										case BlockType.Cave:
											chunk.ReplaceBlock(upperPos, BlockType.Surface, upperBlock.material_id, false );
										break;
										case BlockType.Cube:
											GameMaster.realMaster.AddAnnouncement(Localization.hq_upper_surface_blocked);
											colony.storage.AddResources(requiredResources);
											return;
										}
									}
									SurfaceBlock upperSurface = upperBlock as SurfaceBlock;	
									Building upgraded = Instantiate(Resources.Load<Building>("Prefs/HQ_addon"));
									Quaternion originalRotation = transform.rotation;
									upgraded.SetBasement(upperSurface, PixelPosByte.zero);
									if ( !upgraded.isBasement ) upgraded.transform.rotation = originalRotation;
									level++;
							}
							else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
						}
				}
				if (level > 4) {
					rr.y += rr.height;
					Color c = GUI.color;
					GUI.color = Color.red;
					GUI.Label(rr, Localization.hq_upgrade_warning);
					GUI.color = c;
				}
			}
				else {
					Color c = GUI.color;
					GUI.color = Color.red;
					switch (level) {
					case 1: GUI.Label(rr, Localization.hq_refuse_reason_1, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 2: GUI.Label(rr, Localization.hq_refuse_reason_2, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 3: GUI.Label(rr, Localization.hq_refuse_reason_3, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 4: GUI.Label(rr, Localization.hq_refuse_reason_4, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 5: GUI.Label(rr, Localization.hq_refuse_reason_5, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					case 6: GUI.Label(rr, Localization.hq_refuse_reason_6, PoolMaster.GUIStyle_CenterOrientedLabel);break;
					}
					GUI.color = c;
				}
				rr.y += rr.height;
			if ( requiredResources.Length > 0) {
				Storage storage = GameMaster.colonyController.storage;
				for (int i = 0; i < requiredResources.Length; i++) {
					if (requiredResources[i].volume > storage.standartResources[requiredResources[i].type.ID]) GUI.color = Color.red;
					GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
					GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), requiredResources[i].volume.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
						rr.y += rr.height;
					GUI.color = Color.white;
					}
				}
			}
	}
}
