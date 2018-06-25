using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RecruitingCenterSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public float backupSpeed, progress;
	public bool finding;
}

public class RecruitingCenter : WorkBuilding {
	float backupSpeed = 0.02f, progress = 0;
	bool finding = false;
	ColonyController colonyController;
	const int CREW_SLOTS_FOR_BUILDING = 4, START_CREW_COST = 150;
	public static float hireCost = -1;

	public static void Reset() {
		hireCost = START_CREW_COST + ((int)(GameMaster.difficulty) - 2) * 50;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (hireCost == -1) Reset();
		bool movement = false;
		if (basement != null) movement = true;
		if (b == null) return;
		SetBuildingData(b, pos);
		colonyController = GameMaster.colonyController;
		if ( !movement ) Crew.AddCrewSlots( CREW_SLOTS_FOR_BUILDING );
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (workersCount > 0) {
			if (finding) {
				float candidatsCountFactor = colonyController.freeWorkers / Crew.OPTIMAL_CANDIDATS_COUNT;
				if (candidatsCountFactor > 1) candidatsCountFactor = 1;
				progress += ( workSpeed * 0.3f + colonyController.happiness_coefficient * 0.3f  + candidatsCountFactor * 0.3f + 0.1f * Random.value )* GameMaster.gameSpeed * Time.deltaTime / workflowToProcess;
				if (progress >= 1) {
					Crew ncrew = new Crew();
					ncrew.SetCrew(colonyController, hireCost);
					Crew.crewsList.Add(ncrew);
					progress = 0;
					finding = false;
					GameMaster.realMaster.AddAnnouncement(Localization.AnnounceCrewReady(ncrew.name));
					hireCost = hireCost * (1 + GameMaster.HIRE_COST_INCREASE);
					hireCost = ((int)(hireCost * 100)) / 100f;
				}
			}
		}
		else {
			if (progress > 0) {
				progress -= backupSpeed * GameMaster.gameSpeed * Time.deltaTime;
				if (progress < 0) progress = 0;
			}
		}
	}

	virtual protected void RecalculateWorkspeed() {
		workSpeed = (float)workersCount / (float)maxWorkers;
	}

	void OnGUI() {
		//sync with hospital.cs, rollingShop.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		GUI.Label(rr, Localization.ui_freeSlots + " : " + Crew.crewSlots.ToString(), Crew.crewSlots == 0 ? PoolMaster.GUIStyle_COLabel_red : PoolMaster.GUIStyle_CenterOrientedLabel);
		rr.y += rr.height;
		if ( !finding) {
			if (GUI.Button(rr, Localization.hangar_hireCrew + " (" + Localization.CostInCoins(hireCost) + ')', colonyController.energyCrystalsCount < hireCost ? PoolMaster.GUIStyle_Button_red : GUI.skin.button)) {
				if (colonyController.energyCrystalsCount >= hireCost) {
					colonyController.GetEnergyCrystals(hireCost);
					finding = true;
				}
			}
			rr.y += rr.height;
		}
		else {
			GUI.Label(rr, Localization.ui_recruitmentInProgress); rr.y += rr.height;
			GUI.DrawTexture(new Rect(rr.x, rr.y, rr.width * progress, rr.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
			GUI.Label(rr, ((int)(progress * 100)).ToString() + '%');
			rr.y += rr.height;
		}
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetRecruitingCenterSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}
	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		RecruitingCenterSerializer rcs = new RecruitingCenterSerializer();
		GameMaster.DeserializeByteArray<RecruitingCenterSerializer>(ss.specificData, ref rcs);
		LoadWorkBuildingData(rcs.workBuildingSerializer);
		backupSpeed = rcs.backupSpeed;
		finding = rcs.finding;
		progress = rcs.progress;
	}

	protected RecruitingCenterSerializer GetRecruitingCenterSerializer() {
		RecruitingCenterSerializer rcs = new RecruitingCenterSerializer();
		rcs.workBuildingSerializer = GetWorkBuildingSerializer();
		rcs.backupSpeed = backupSpeed;
		rcs.finding = finding;
		rcs.progress = progress;
		return rcs;
	}
	#endregion
	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		Crew.RemoveCrewSlots(CREW_SLOTS_FOR_BUILDING);
	}
}
