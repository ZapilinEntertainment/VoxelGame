using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HangarSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public bool constructing;
	public int shuttle_id;
}

public class Hangar : WorkBuilding {
	static int hangarsCount = 0;
	public Shuttle shuttle{get; private set;}
	const float CREW_HIRE_BASE_COST = 100;
	bool constructing = false, showCrews = false;

	public static void Reset() {
		hangarsCount = 0;
	}

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
		hangarsCount++;
		if (hangarsCount == 1) ExpeditionCorpus.InitializeQuest(Quest.FIRST_COMMUNICATOR_SET_ID);
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if ( constructing & workersCount > 0 & shuttle == null) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
			}
		}
	}

	protected virtual void LabourResult() {
		shuttle = Instantiate(Resources.Load<GameObject>("Prefs/shuttle")).GetComponent<Shuttle>();
		shuttle.FirstSet(this);
		constructing = false;
		workflow -= workflowToProcess;
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.MachineConstructing);
	}

	public override void SetGUIVisible (bool x) {
		if (x) {
			showOnGUI = true;
		}
		else {
			showOnGUI = false;
			UI.current.HideCrewCard();
		}
	}

	void OnGUI() {
		//based on building.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		Color ncolor = GUI.color;
		if (shuttle != null) {
			Shuttle.GUI_DrawShuttleIcon(shuttle, rr);
			GUI.Label(new Rect(rr.x + rr.height, rr.y , rr.width - rr.height, rr.height), shuttle.name + " (" + ((int)(shuttle.condition * 100)).ToString() + "%)"); 
			rr.y += rr.height;
			if (shuttle.condition < 1) {
				if (GUI.Button(new Rect(rr.x, rr.y, rr.width/2f, rr.height), Localization.hangar_repairFor + ' '+Localization.resources)) {
					shuttle.RepairForResources();
				}
				if (GUI.Button(new Rect(rr.x + rr.width/2f, rr.y, rr.width/2f, rr.height), Localization.hangar_repairFor + ' '+Localization.coins)) {
					shuttle.RepairForCoins();
				}
				GUI.Box(new Rect(rr.x, rr.y, rr.width/2f, rr.height * 4), GUIContent.none);
				GUI.Box(new Rect(rr.x + rr.width/2f, rr.y, rr.width/2f, rr.height * 4), GUIContent.none);
				rr.y += rr.height;
				ResourceContainer[] repairCost = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
				for (int i =0; i < repairCost.Length;i++) {
					repairCost[i]= new ResourceContainer(repairCost[i].type, repairCost[i].volume * (1 - shuttle.condition));
					bool notEnough = false;
					if (repairCost[i].volume > GameMaster.colonyController.storage.standartResources[repairCost[i].type.ID]) {GUI.color =Color.red;notEnough = true;}
					GUI.DrawTexture(new Rect(rr.x, rr.y+ i * rr.height, rr.height, rr.height), repairCost[i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x + rr.height, rr.y + i * rr.height, rr.width/2f - 2 * rr.height, rr.height), repairCost[i].type.name);
					GUI.Label(new Rect(rr.width/2f - rr.height, rr.y+ i * rr.height, rr.height, rr.height), repairCost[i].type.icon, PoolMaster.GUIStyle_RightOrientedLabel);
					if (notEnough) GUI.color = Color.white;
				}
				GUI.Label(new Rect(rr.x + rr.width/2f, rr.y, rr.width/2f, rr.height), string.Format("{0:0.##}", shuttle.cost * (1 - shuttle.condition)));
				rr.y += 3 * rr.height;
			}
			if (shuttle.fuelReserves < shuttle.fuelCapacity) {
				if (GUI.Button(rr, Localization.hangar_refuel)) {
					shuttle.Refuel();
				}
			}
		
			if (shuttle.crew != null) {
				GUI.Label(new Rect(rr.x + rr.height, rr.y, rr.width - rr.height, rr.height), shuttle.crew.name); rr.y += rr.height;
				GUI.Label(rr, Localization.hangar_readiness + ":   " + ((int)(shuttle.crew.stamina * 100)).ToString() + '%');rr.y += rr.height;
				if (GUI.Button(rr, Localization.ui_showCrewCard)) UI.current.ShowCrewCard(shuttle.crew); rr.y += rr.height;
			}
			else 	GUI.Label(rr, Localization.hangar_noCrew); rr.y += rr.height;

			if (GUI.Button(rr, Localization.ui_showCrewsList)) showCrews = !showCrews;
			rr.y += rr.height;
			if (showCrews) {
				if (Crew.crewsList.Count == 0) GUI.Label(rr, " <"+Localization.empty+"> ");
				else {
					foreach (Crew c in Crew.crewsList) {
						if (c == null) continue;
						if (c.shuttle != null && c.shuttle == shuttle) GUI.DrawTexture(rr, PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
						Crew.GUI_DrawCrewIcon(c, rr);
						if (GUI.Button(new Rect(rr.x + rr.height, rr.y, rr.width - 2 * rr.height, rr.height), c.name + " (" + (c.shuttle != null ? c.shuttle.name : Localization.ui_noShip) + ')')) {
							c.ChangeShip(shuttle);
							showCrews = false;
							UI.current.HideCrewCard();
						}
						if (GUI.Button(new Rect(rr.xMax - rr.height, rr.y, rr.height,rr.height), "i")) {
							UI.current.ShowCrewCard(c);
						}
						rr.y += rr.height;
					}
				}
			}
		}
		else {
			if (!constructing) {
				Storage storage = GameMaster.colonyController.storage;
				GUI.color = Color.yellow;
				GUI.Label(rr, Localization.hangar_noShuttle, PoolMaster.GUIStyle_CenterOrientedLabel); rr.y += rr.height;
				GUI.color = ncolor;
				ResourceContainer[] shuttleCost = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
				GUI.Box(new Rect(rr.x ,rr.y, rr.width, rr.height * ( shuttleCost.Length + 1 )), GUIContent.none);
				if (GUI.Button (rr, Localization.ui_build)) {
					if (storage.CheckBuildPossibilityAndCollectIfPossible(shuttleCost)) {
						constructing = true;
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
			else { // shuttle is building
				GUI.DrawTexture(new Rect(rr.x, rr.y, rr.width * (workflow / workflowToProcess), rr.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
				GUI.Label(rr, Localization.ui_assemblyInProgress + " (" + ((int)(workflow / workflowToProcess * 100)).ToString() + "%)" );
				rr.y += rr.height;
			}
		}
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHangarSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		HangarSerializer hs = new HangarSerializer();
		GameMaster.DeserializeByteArray<HangarSerializer>(ss.specificData, ref hs);
		constructing = hs.constructing;
		LoadWorkBuildingData(hs.workBuildingSerializer);
		shuttle = Shuttle.GetShuttle(hs.shuttle_id);
	}

	HangarSerializer GetHangarSerializer() {
		HangarSerializer hs = new HangarSerializer();
		hs.workBuildingSerializer = GetWorkBuildingSerializer();
		hs.constructing = constructing;
		hs.shuttle_id = (shuttle == null ? -1 : shuttle.ID);
		return hs;
	}
	#endregion

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		hangarsCount--;
	}
}
