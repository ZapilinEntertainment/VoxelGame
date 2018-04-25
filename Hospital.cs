using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BirthrateMode {Lowered, Normal, Improved}
public class Hospital : House {
	[SerializeField]
	int _coverage = 100;
	public int coverage {get;private set;}
	public static float hospital_birthrate_coefficient = 1;
	public static  BirthrateMode birthrateMode{get; private set;}
	public static float loweredCoefficient = 0.5f, improvedCoefficient = 1.5f;

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		coverage = _coverage;
		GameMaster.colonyController.AddHospital(this);
	}

	override public void SetActivationStatus(bool x) {
		if (isActive == x) return;
		isActive = x;
		GameMaster.colonyController.RecalculateHousing();
		GameMaster.colonyController.RecalculateHospitals();
	}

	void OnGUI() {
		//from building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (nextStage != null && level < GameMaster.colonyController.hq.level) {
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
			if ( ResourcesCost.info[ nextStage.resourcesContainIndex ].Length > 0) {
				rr.y += rr.height;
				for (int i = 0; i < ResourcesCost.info[ nextStage.resourcesContainIndex ].Length; i++) {
					GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.name);
					GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (ResourcesCost.info[ nextStage.resourcesContainIndex ][i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
					rr.y += rr.height;
				}
			}
		}
		byte p = (byte)birthrateMode;
		GUI.DrawTexture(new Rect(rr.x, rr.y + p * rr.height, rr.width, rr.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
		if (GUI.Button(rr, Localization.lowered_birthrate + " (" + string.Format("{0:0.##}", loweredCoefficient)  + "%)")) {
			birthrateMode = BirthrateMode.Lowered; 
			hospital_birthrate_coefficient = loweredCoefficient;
		}
		rr.y += rr.height;
		if (GUI.Button(rr, Localization.normal_birthrate + " (100%)")) {
			birthrateMode = BirthrateMode.Normal; 
			hospital_birthrate_coefficient = 1;
		}
			rr.y += rr.height;
		if (GUI.Button(rr, Localization.improved_birthrate + " (" + string.Format("{0:0.##}", improvedCoefficient)  + "%)")) {
			birthrateMode = BirthrateMode.Improved; 
			hospital_birthrate_coefficient = improvedCoefficient;
		}
	}
}
