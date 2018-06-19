using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData() + SaveRecruitmentCenterData();
	}

	protected string SaveRecruitmentCenterData() {
		string s="";
		if (finding) {
			s += '1'; 
			s += progress.ToString();
			s += ';';
		}
		else s += '0';
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
		//building class part
		SetActivationStatus(s_data[11] == '1');     
		//recruitment center class part
		finding = (s_data[18] ==1);
		if (finding) {
			int p = s_data.IndexOf(';', 18);
			progress = float.Parse(s_data.Substring(19, p- 18));
		}
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	
	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		Crew.RemoveCrewSlots(CREW_SLOTS_FOR_BUILDING);
	}
}
