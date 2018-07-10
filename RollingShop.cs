using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RollingShopMode {NoActivity, GearsUpgrade}

[System.Serializable]
public class RollingShopSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public RollingShopMode mode;
}

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
		case RollingShopMode.GearsUpgrade:
			if (GameMaster.colonyController.gears_coefficient < GEARS_UP_LIMIT) GameMaster.colonyController.ImproveGearsCoefficient(GEARS_UPGRADE_STEP * workflow / workflowToProcess);
			break;
		}
	}


	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetRollingShopSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}
	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		RollingShopSerializer rss = new RollingShopSerializer();
		GameMaster.DeserializeByteArray<RollingShopSerializer>(ss.specificData, ref rss);
		LoadWorkBuildingData(rss.workBuildingSerializer);
		mode = rss.mode;
	}

	protected RollingShopSerializer GetRollingShopSerializer() {
		RollingShopSerializer rss = new RollingShopSerializer();
		rss.workBuildingSerializer = GetWorkBuildingSerializer();
		rss.mode = mode;
		return rss;
	}
	#endregion

	void OnDestroy() {
		GameMaster.colonyController.RemoveRollingShop(this);
		PrepareBuildingForDestruction();
	}

	void OLDOnGUI() {
		//based on building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(0,0,0,0);
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
		}
	}
}
