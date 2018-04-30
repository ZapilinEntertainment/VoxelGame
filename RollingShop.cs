using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum RollingShopMode{NoActivity, GearsUpgrade, BoatParts}
public class RollingShop : WorkBuilding {
	RollingShopMode mode;
	bool showModes = false;
	const float GEARS_UP_LIMIT = 3, GEARS_UPGRADE_STEP = 0.1f;

	void Awake() {
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
			if (GameMaster.colonyController.gears_coefficient < GEARS_UP_LIMIT) GameMaster.colonyController.ImproveGearsCoefficient(GEARS_UPGRADE_STEP);
			break;
		}
	}

	void OnDestroy() {
		GameMaster.colonyController.RemoveRollingShop(this);
		PrepareBuildingForDestruction();
	}

	void OnGUI() {
		//based on building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (nextStage != null && level < GameMaster.colonyController.hq.level) {
			rr.y = GUI_UpgradeButton(rr);
		}
		// rolling shop functional
		if (GUI.Button(rr, Localization.ui_setMode)) showModes = !showModes; rr.y += rr.height;
		if (showModes) {
			if ( GUI.Button(rr, Localization.no_activity) ) mode = RollingShopMode.NoActivity;  rr.y += rr.height;
			if (GUI.Button(rr, Localization.rollingShop_gearsProduction)) mode = RollingShopMode.GearsUpgrade;  rr.y += rr.height;
			if (GUI.Button(rr, Localization.rollingShop_boatPartsProduction)) mode = RollingShopMode.BoatParts;  rr.y += rr.height;
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
