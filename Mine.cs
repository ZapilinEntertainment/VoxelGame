using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : WorkBuilding {
	CubeBlock workObject;
	public float criticalVolume = 0.75f;
	bool workFinished = false;
	string actionLabel = "";

	void Awake() {
		PrepareWorkbuilding();
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		workObject = basement.basement; // <SurfaceBlock>.<CubeBlock>
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (workersCount > 0 && !workFinished) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
				workflow -= workflowToProcess;
			}
		}
	}

	override protected void LabourResult() {
		if (workflow > 1) {
			int x = (int) workflow;
			float production = x;
			production = workObject.Dig(x, true);
			GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
			int percent = (int)((1 - (float)workObject.volume / (float) CubeBlock.MAX_VOLUME) * 100);
			if (workObject.volume <= criticalVolume) {
				workFinished = true;
				actionLabel = percent.ToString() + "% " + Localization.extracted + ". " + Localization.work_has_stopped;
			}
			else actionLabel = percent.ToString() + "% " + Localization.extracted + " / (" + ((int)(criticalVolume*100)).ToString() + "%)"; 
			workflow -= production;	
		}
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Mining);
	}

	void OnGUI() {
		if ( !showOnGUI ) return;
		Rect r = UI.current.rightPanelBox; r.y = gui_ypos; r.height = GameMaster.guiPiece;
		GUI.Label(r, actionLabel);
	}
}
