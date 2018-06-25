using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MineSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public bool workFinished;
	public ChunkPos lastWorkObjectPos;
	public bool awaitingElevatorBuilding;
	public byte level;
	public List<StructureSerializer> elevators;
	public List<byte>elevatorHeights;
	public bool haveElevators;
}

public class Mine : WorkBuilding {
	CubeBlock workObject;
	bool workFinished = false;
	string actionLabel = "";
	ChunkPos lastWorkObjectPos;
	public List<Structure> elevators;
	public bool awaitingElevatorBuilding = false;

	override public void Prepare() {
		PrepareWorkbuilding();
		elevators = new List<Structure>();
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		Block bb = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y - 1, basement.pos.z);
		workObject = bb as CubeBlock;
		lastWorkObjectPos = bb.pos;
	}

	void Update() {
		if (awaitingElevatorBuilding) {
			Block b = basement.myChunk.GetBlock(lastWorkObjectPos.x, lastWorkObjectPos.y, lastWorkObjectPos.z);
			if ( b != null ) {
				if (b.type == BlockType.Cave | b.type == BlockType.Surface ) {
					Structure s = Structure.GetNewStructure(Structure.MINE_ELEVATOR_ID);
					s.SetBasement(b as SurfaceBlock, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION/2 - s.innerPosition.x_size/2, SurfaceBlock.INNER_RESOLUTION/2 - s.innerPosition.z_size/2));
					elevators.Add(s);
					awaitingElevatorBuilding = false;
					GameMaster.realMaster.AddAnnouncement(Localization.mine_levelFinished);
				}
			}
		}
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied ) return;
		if ( workObject != null ) {
			if (workersCount > 0 && !workFinished ) {
				workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
				if (workflow >= workflowToProcess) {
					LabourResult();
					workflow -= workflowToProcess;
				}
			}
		}
	}

override protected void LabourResult() {
		int x = (int) workflow;
		float production = x;
		production = workObject.Dig(x, false);
		GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
		if ( workObject!=null & workObject.volume != 0) {
			int percent = (int)((1 - (float)workObject.volume / (float) CubeBlock.MAX_VOLUME) * 100);
			actionLabel = percent.ToString() + "% " + Localization.extracted; 
			workflow -= production;	
		}
		else {
			workFinished = true;
			actionLabel = Localization.work_has_stopped;
			awaitingElevatorBuilding = true;
		}			
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Mining);
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetMineSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		MineSerializer ms = new MineSerializer();
		GameMaster.DeserializeByteArray(ss.specificData, ref ms);
		LoadMineData(ms);
	}

	protected void LoadMineData(MineSerializer ms) {
		LoadWorkBuildingData(ms.workBuildingSerializer);
		level = ms.level;
		elevators = new List<Structure>();
		if (level > 1 & ms.haveElevators) {
			for (int i = 0; i < ms.elevators.Count; i++) {
				Structure s = Structure.GetNewStructure(MINE_ELEVATOR_ID);
				s.Load(ms.elevators[i], basement.myChunk.GetBlock(basement.pos.x, ms.elevatorHeights[i], basement.pos.z) as SurfaceBlock);
				elevators.Add(s);
			}
		}
		workFinished = ms.workFinished;
		lastWorkObjectPos = ms.lastWorkObjectPos;
		awaitingElevatorBuilding = ms.awaitingElevatorBuilding;
	}

	protected MineSerializer GetMineSerializer() {
		MineSerializer ms = new MineSerializer();
		ms.workBuildingSerializer = GetWorkBuildingSerializer();
		ms.workFinished = workFinished;
		ms.lastWorkObjectPos = lastWorkObjectPos;
		ms.awaitingElevatorBuilding = awaitingElevatorBuilding;
		ms.level = level;
		ms.elevators = new List<StructureSerializer>(); ms.elevatorHeights = new List<byte>();
		ms.haveElevators = false;
		if (level > 1) {
			for (int i = 0; i < elevators.Count; i++) {
				if (elevators[i] == null) continue;
				else {
					ms.elevators.Add((elevators[i] as MineElevator).GetSerializer());
					ms.elevatorHeights.Add(elevators[i].basement.pos.y);
					ms.haveElevators = true;
				}
			}
		}
		return ms;
	}
	#endregion

	void ChangeModel(byte f_level) {
		if (f_level == level ) return;
		GameObject nextModel = Resources.Load<GameObject>("Prefs/minePref_level_" + (f_level).ToString());
		if (nextModel != null) {
			GameObject newModelGO = Instantiate(nextModel, transform.position, transform.rotation, transform);
			if (myRenderer != null) Destroy(myRenderer.gameObject);
			if (myRenderers != null) {for (int n =0; n < myRenderers.Count; n++) Destroy( myRenderers[n].gameObject );}
			myRenderers = new List<Renderer>();
			for (int n = 0; n < newModelGO.transform.childCount; n++) {
				myRenderers.Add( newModelGO.transform.GetChild(n).GetComponent<Renderer>());
				if (!visible) myRenderers[n].enabled = false;
			}
			if ( !isActive || !energySupplied ) ChangeRenderersView(false);
		}
		level = f_level;
	}

	void OnGUI() {
		if ( !showOnGUI ) return;
		GUI.skin = GameMaster.mainGUISkin;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		GUI.Label(rr, actionLabel); 
		if (workFinished && !awaitingElevatorBuilding) {
			Block b = basement.myChunk.GetBlock(lastWorkObjectPos.x, lastWorkObjectPos.y - 1, lastWorkObjectPos.z);
			if (b != null && b.type == BlockType.Cube) {
				rr.y += rr.height;
				GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
					if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
						if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
						{
							workObject = b as CubeBlock;
							lastWorkObjectPos = b.pos;
							workFinished = false;
							ChangeModel((byte)(level +1));
						}
						else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
					}
				if ( requiredResources.Length > 0) {
						rr.y += rr.height;
					for (int i = 0; i < requiredResources.Length; i++) {
						GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
						GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
						GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (requiredResources[i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
							rr.y += rr.height;
						}
					}
			}
		}
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		if (elevators.Count > 0) {
			foreach (Structure s in elevators) {
				if (s != null)	s.Annihilate(false);
			}
		}
	}
}
