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

	void Awake() {
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
					Structure s = Instantiate(PoolMaster.mineElevator_pref).GetComponent<Structure>();
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
						ResourceContainer[] requiredResources = new ResourceContainer[ResourcesCost.info[ResourcesCost.MINE_UPGRADE_INDEX].Length];
						if (requiredResources.Length > 0) {
							for (int i = 0; i < requiredResources.Length; i++) {
							requiredResources[i] = new ResourceContainer(ResourcesCost.info[ResourcesCost.MINE_UPGRADE_INDEX][i].type, ResourcesCost.info[ResourcesCost.MINE_UPGRADE_INDEX][i].volume * level);
							}
						}
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
				if ( ResourcesCost.info[ ResourcesCost.MINE_UPGRADE_INDEX ].Length > 0) {
						rr.y += rr.height;
					for (int i = 0; i < ResourcesCost.info[ ResourcesCost.MINE_UPGRADE_INDEX ].Length; i++) {
						GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), ResourcesCost.info[ ResourcesCost.MINE_UPGRADE_INDEX ][i].type.icon, ScaleMode.StretchToFill);
						GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), ResourcesCost.info[ ResourcesCost.MINE_UPGRADE_INDEX ][i].type.name);
						GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (ResourcesCost.info[ ResourcesCost.MINE_UPGRADE_INDEX ][i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
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
