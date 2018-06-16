using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hangar : WorkBuilding {
	public Shuttle shuttle{get; private set;}
	public Crew crew{get;private set;}
	const float CREW_HIRE_BASE_COST = 100;

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		Transform meshTransform = transform.GetChild(0);
		if (basement.pos.z == 0) {
			meshTransform.transform.localRotation = Quaternion.Euler(0, 180,0); 
		}
		else {
			if (basement.pos.z != Chunk.CHUNK_SIZE - 1) {
				if (basement.pos.x == 0) {
					meshTransform.transform.localRotation = Quaternion.Euler(0, -90,0); 
				}
				else {
					if (basement.pos.x == Chunk.CHUNK_SIZE - 1) {
						meshTransform.transform.localRotation = Quaternion.Euler(0, 90,0);
					}
				}
			}
		}
	}

	void OnGUI() {
		//based on building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		Color ncolor = GUI.color;
		if (shuttle != null) {
			
		}
		else {
			Storage storage = GameMaster.colonyController.storage;
			GUI.color = Color.yellow;
			GUI.Label(rr, Localization.hangar_noShuttle, PoolMaster.GUIStyle_CenterOrientedLabel); rr.y += rr.height;
			GUI.color = ncolor;
			ResourceContainer[] shuttleCost = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
			GUI.Box(new Rect(rr.x ,rr.y, rr.width, rr.height * ( shuttleCost.Length + 1 )), GUIContent.none);
			if (GUI.Button (rr, Localization.ui_build)) {
				if (storage.CheckBuildPossibilityAndCollectIfPossible(shuttleCost)) {
					//shuttle = Instantiate(Resources.Load<Shu>)
				}
				else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
			}
			rr.y += rr.height;
			foreach ( ResourceContainer rc in shuttleCost ) {
				GUI.DrawTexture( new Rect(rr.x ,rr.y, rr.height, rr.height), rc.type.icon, ScaleMode.StretchToFill );
				if ( storage.standartResources[rc.type.ID] < rc.volume ) {
					GUI.color = Color.red;
					GUI.Label( new Rect(rr.x + rr.height, rr.y, rr.width - rr.height, rr.height), rc.type.name);
					GUI.color = ncolor;
				}
				else GUI.Label( new Rect(rr.x + rr.height, rr.y, rr.width - rr.height, rr.height), rc.type.name);
				rr.y += rr.height;
			} 
		}

		if (crew != null) {
			
		}
		else {
			
		}
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
	}
}
