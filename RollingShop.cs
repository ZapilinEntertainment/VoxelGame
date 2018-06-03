using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum RollingShopMode {NoActivity, GearsUpgrade, BoatParts}
public class RollingShop : WorkBuilding {
	RollingShopMode mode;
	bool showModes = false;
	const float GEARS_UP_LIMIT = 3, GEARS_UPGRADE_STEP = 0.1f;

	override public void Prepare() {
		PrepareWorkbuilding();
		mode = RollingShopMode.NoActivity;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		GameMaster.colonyController.AddRollingShop(this);
	}

	override protected void LabourResult() {
		switch (mode) {
		case RollingShopMode.BoatParts:
			break;
		case RollingShopMode.GearsUpgrade:
			if (GameMaster.colonyController.gears_coefficient < GEARS_UP_LIMIT) GameMaster.colonyController.ImproveGearsCoefficient(GEARS_UPGRADE_STEP * workflow / workflowToProcess);
			break;
		}
	}

	public int GetActivityModeIndex() {
		switch (mode) {
		case RollingShopMode.NoActivity: return 0;
		case RollingShopMode.GearsUpgrade: return 1;
		case RollingShopMode.BoatParts: return 2;
		default: return 0;
		}
	}

	public void SetMode(int i) {
		switch (i) {
		case 0: mode = RollingShopMode.NoActivity;break;
		case 1: mode = RollingShopMode.GearsUpgrade;break;
		case 2: mode = RollingShopMode.BoatParts;break;
		default: mode = RollingShopMode.NoActivity;break;
		}
	}

	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData()+SaveRollingShopData();
	}

	protected string SaveRollingShopData() {
		string s = "";
		s +=string.Format("{0:d1}", GetActivityModeIndex());
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		//workbuilding class part
		workflow = int.Parse(s_data.Substring(12,3)) / 100f;
		AddWorkers(int.Parse(s_data.Substring(15,3)));
		//rollingshop class part
		SetMode( int.Parse(s_data[18].ToString()) );
		//building class part
		SetActivationStatus(s_data[11] == '1');     
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	

	void OnDestroy() {
		GameMaster.colonyController.RemoveRollingShop(this);
		PrepareBuildingForDestruction();
	}

	void OnGUI() {
		//based on building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		// rolling shop functional
		if (GUI.Button(rr, Localization.ui_setMode)) showModes = !showModes; rr.y += rr.height;
		if (showModes) {
			if ( GUI.Button(rr, Localization.no_activity) ) {
				showModes = false;
				mode = RollingShopMode.NoActivity; 
			} rr.y += rr.height;
			if (GUI.Button(rr, Localization.rollingShop_gearsProduction)) {
				showModes = false;
				mode = RollingShopMode.GearsUpgrade; 
			}
				rr.y += rr.height;
			if (GUI.Button(rr, Localization.rollingShop_boatPartsProduction)) {
				showModes = false;
				mode = RollingShopMode.BoatParts;  
			}
				rr.y += rr.height;
		}
		switch (mode) {
		case RollingShopMode.NoActivity:
			GUI.Label( rr, Localization.ui_currentMode + " : " + Localization.no_activity, PoolMaster.GUIStyle_CenterOrientedLabel);
			break;
		case RollingShopMode.GearsUpgrade:
			GUI.Label( rr, Localization.ui_currentMode + " : " + Localization.rollingShop_gearsProduction, PoolMaster.GUIStyle_CenterOrientedLabel);
			rr.y += rr.height;
			GUI.Label( rr, Localization.info_gearsCoefficient + " : " + string.Format("{0:0.###}", GameMaster.colonyController.gears_coefficient));
			break;
		case RollingShopMode.BoatParts:
			GUI.Label( rr, Localization.ui_currentMode + " : " + Localization.rollingShop_boatPartsProduction, PoolMaster.GUIStyle_CenterOrientedLabel);
			break;
		}
	}
}
