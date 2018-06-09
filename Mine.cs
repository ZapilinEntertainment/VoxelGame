using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
				if (b.type == BlockType.Cave ) {
					Structure s = Structure.GetNewStructure(Structure.MINE_ELEVATOR_ID);
					s.SetBasement(b as SurfaceBlock, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION/2 - s.innerPosition.x_size/2, SurfaceBlock.INNER_RESOLUTION/2 - s.innerPosition.z_size/2));
					elevators.Add(s);
					awaitingElevatorBuilding = false;
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
				if (workObject.volume == 0) {
					workFinished = true;
					actionLabel = Localization.work_has_stopped;
					production = workObject.Dig(x, false);
					awaitingElevatorBuilding = true;
				}
				else {
					production = workObject.Dig(x, false);
					GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
					int percent = (int)((1 - (float)workObject.volume / (float) CubeBlock.MAX_VOLUME) * 100);
					actionLabel = percent.ToString() + "% " + Localization.extracted; 
				}
				workflow -= production;	
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Mining);
	}

	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData() +SaveMineData();
	}

	protected string SaveMineData() {
		string s = "";
		if (elevators != null && elevators.Count > 0) {
			foreach (Structure str in elevators) {
				if (str == null) continue;
				s += string.Format("{0:d2}", str.basement.pos.y);
			}
		}
		s += 'e';
		if (workFinished) s += 'f'; else s += 'n';
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		//mine class part
		int endIndex = s_data.IndexOf("e", 18);
		if (endIndex > 18 ) {
			int k = 18, block_xpos = basement.pos.x, block_zpos = basement.pos.z;
			elevators = new List<Structure>();
			byte endDepth = basement.pos.y;
			while ( s_data[k] != 'e') {
				byte ypos = (byte)int.Parse(s_data.Substring(k,2));
				Structure elevator = Structure.GetNewStructure(Structure.MINE_ELEVATOR_ID);
				elevator.SetBasement( basement.myChunk.GetBlock(block_xpos, ypos, block_zpos) as SurfaceBlock, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION/2 - elevator.innerPosition.x_size/2, SurfaceBlock.INNER_RESOLUTION/2 - elevator.innerPosition.z_size/2)  );
				elevators.Add(elevator);
				endDepth = ypos;
				k+=2;
			}
			if ( s_data[k+1] =='f' ) workFinished = true; else workFinished = false;
			Block b = basement.myChunk.GetBlock(block_xpos, endDepth - 1, block_zpos);
			if ( b != null && b.type == BlockType.Cube) {
				workObject = b as CubeBlock;
				lastWorkObjectPos = workObject.pos;
			}
			awaitingElevatorBuilding = false;
		}
		//workbuilding class part
		workflow = int.Parse(s_data.Substring(12,3)) / 100f;
		AddWorkers(int.Parse(s_data.Substring(15,3)));
		//building class part
		SetActivationStatus(s_data[11] == '1');     
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	

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
							GameObject nextModel = Resources.Load<GameObject>("Prefs/minePref_level_" + (level+1).ToString());
							if (nextModel != null) {
								GameObject newModelGO = Instantiate(nextModel, transform.position, transform.rotation, transform);
								if (myRenderer != null) Destroy(myRenderer.gameObject);
								if (myRenderers != null) {for (int n =0; n < myRenderers.Length; n++) Destroy( myRenderers[n].gameObject );}
								myRenderers = new Renderer[newModelGO.transform.childCount];
								for (int n = 0; n < newModelGO.transform.childCount; n++) {
									myRenderers[n] = newModelGO.transform.GetChild(n).GetComponent<Renderer>();
									if (!visible) myRenderers[n].enabled = false;
								}
								if ( !isActive || !energySupplied ) ChangeRenderersView(false);
							}
						level++;
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
