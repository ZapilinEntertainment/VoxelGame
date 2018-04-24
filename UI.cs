using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UIMode {View, SurfaceBlockPanel, CubeBlockPanel, StructurePanel, WorksitePanel, GameMenu}

public class UI : MonoBehaviour {
	public LineRenderer lineDrawer;
	public static UI current;

	public UIMode mode{get;private set;}

	float leftPanelWidth;
	byte argument, showingBuildingsLevel = 1;
	public Rect rightPanelBox, upPanelBox, acceptBox,systemInfoRect, buildingGridRect;
	string systemInfoString; float systemInfoTimer = 0;
	public bool  showBuildingCreateInfo {get; private set;}
	public bool touchscreenTemporarilyBlocked = false;
	PixelPosByte bufferedPosition; int chosenBuildingIndex = 0;
	SurfaceBlock chosenSurfaceBlock; bool chosenSurfaceBlockIsBorderBlock = false;
	CubeBlock chosenCubeBlock; byte faceIndex = 10;
	Structure chosenStructure;
	Worksite chosenWorksite;
	Vector2 mousePos;
	GameObject quadSelector;

	Texture  grid16_tx, greenSquare_tx, whiteSquare_tx, whiteSpecial_tx, yellowSquare_tx,
	citizen_icon_tx, energy_icon_tx, energyCrystal_icon_tx, energyLightning_icon_tx, buildingQuad_icon_tx, demolishButton_tx;
	public Texture rightArrow_tx{get;private set;}

	List<Factory> smelteriesList; bool hasSmelteries = false;
	List<Factory> unspecializedFactories; bool hasUnspecializedFactories = false;
	public const int GUIDEPTH_UI_MAIN = 10, GUIDEPTH_WORKSITE_WINDOW = 9;


	void Awake() {
		current = this;
		mode = UIMode.View;
		showBuildingCreateInfo = false;
		quadSelector = Instantiate(Resources.Load<GameObject>("Prefs/QuadSelector"));
		quadSelector.SetActive(false);
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
		grid16_tx = Resources.Load<Texture>("Textures/AccessibleGrid");
		greenSquare_tx = Resources.Load<Texture>("Textures/greenSquare");
		whiteSquare_tx = Resources.Load<Texture>("Textures/whiteSquare");
		whiteSpecial_tx = Resources.Load<Texture>("Textures/whiteSpecialSquare");
		yellowSquare_tx = Resources.Load<Texture>("Textures/yellowSquare");
		citizen_icon_tx = Resources.Load<Texture>("Textures/citizen_icon");
		energy_icon_tx = Resources.Load<Texture>("Textures/energy_icon");
		energyCrystal_icon_tx = Resources.Load<Texture>("Textures/energyCrystal_icon");
		energyLightning_icon_tx = Resources.Load<Texture>("Textures/energyLightning_icon");
		buildingQuad_icon_tx = Resources.Load<Texture>("Textures/buildingQuad_icon");
		demolishButton_tx = Resources.Load<Texture>("Textures/button_demolish");
		rightArrow_tx = Resources.Load<Texture>("Textures/gui_arrowRight");

		argument = 0;

		float k = GameMaster.guiPiece;
		float upPanelHeight = k;
		leftPanelWidth = k;
		float rightPanelWidth = 8 * k;
		rightPanelBox = new Rect(Screen.width - rightPanelWidth, upPanelHeight, rightPanelWidth, Screen.height - upPanelHeight);
		upPanelBox = new Rect(0,0,Screen.width, upPanelHeight);
		acceptBox = new Rect(Screen.width / 2f - 4 * k, Screen.height / 2f - 2 * k, 8 * k, 4 * k);
		systemInfoRect = new Rect(Screen.width/2f - 6 * k,  2 *k, 12 * k, 4 *k);

		float p = Screen.width/2f / SurfaceBlock.INNER_RESOLUTION;
		float side = p * SurfaceBlock.INNER_RESOLUTION;
		buildingGridRect = new Rect(rightPanelBox.x - side - 2 * p, Screen.height/2f - side/2f, side, side);
	}
	void Update() {
		mousePos = Input.mousePosition;
		mousePos.y = Screen.height - mousePos.y;
		if (Input.GetMouseButtonDown(0) && !cursorIntersectGUI(mousePos) ) {
			RaycastHit rh;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rh)) {
				DropFocus();
				Structure s = rh.collider.GetComponent<Structure>();
				if (s != null) {
					chosenStructure = s;
					chosenStructure.showOnGUI = true;
					Vector2 cursorPos = Camera.main.WorldToScreenPoint(s.transform.position);
					mode = UIMode.StructurePanel;
					GameMaster.realMaster.SetLookPoint (chosenStructure.transform.position);
				}
				else { // not a structure
					if (rh.collider.transform.parent != null) {
						Block b = rh.collider.transform.parent.GetComponent<Block>();
						if (b != null) {
							float x ,y,z,d = Block.QUAD_SIZE/2f;
							switch (b.type) {
							case BlockType.Cave:
							case BlockType.Surface:
									mode = UIMode.SurfaceBlockPanel;
									chosenSurfaceBlock = b.GetComponent<SurfaceBlock>();
									x= chosenSurfaceBlock.transform.position.x; y = chosenSurfaceBlock.transform.position.y - d + 0.01f;  z = chosenSurfaceBlock.transform.position.z;
									lineDrawer.SetPositions(new Vector3[5]{new Vector3(x - d, y, z+d), new Vector3(x+d,y,z+d), new Vector3(x+d,y,z-d), new Vector3(x - d, y, z-d), new Vector3(x-d, y,z+d)});
									lineDrawer.material = PoolMaster.lr_green_material;
									lineDrawer.enabled = true;
									GameMaster.realMaster.SetLookPoint (chosenSurfaceBlock.transform.position + Vector3.down * Block.QUAD_SIZE /2f);
									argument = 1;
								if (chosenSurfaceBlock.pos.x == 0 || chosenSurfaceBlock.pos.x == Chunk.CHUNK_SIZE - 1 || chosenSurfaceBlock.pos.z == 0 || chosenSurfaceBlock.pos.z == Chunk.CHUNK_SIZE - 1)
								chosenSurfaceBlockIsBorderBlock = true; else chosenSurfaceBlockIsBorderBlock = false;
								break;
							case BlockType.Cube:
								chosenCubeBlock = b.GetComponent<CubeBlock>();
									for (byte i =0; i< 6; i++) {
										if (chosenCubeBlock.faces[i] == null) continue;
										if (chosenCubeBlock.faces[i].GetComponent<Collider>() == rh.collider ) {faceIndex = i;break;}
									}
									if (faceIndex != 10) {
										mode = UIMode.CubeBlockPanel;
										switch (faceIndex) {
										case 0: 
											quadSelector.transform.position = chosenCubeBlock.transform.position + Vector3.forward * (Block.QUAD_SIZE/2f + 0.01f);
											quadSelector.transform.rotation = Quaternion.Euler(90,0,0);
											break;
										case 1:
											quadSelector.transform.position = chosenCubeBlock.transform.position + Vector3.right * (Block.QUAD_SIZE/2f + 0.01f);
											quadSelector.transform.rotation = Quaternion.Euler(0,0,-90);
											break;
										case 2:
											quadSelector.transform.position = chosenCubeBlock.transform.position + Vector3.back * (Block.QUAD_SIZE/2f + 0.01f);
											quadSelector.transform.rotation = Quaternion.Euler(-90,0,0);
											break;
										case 3:
											quadSelector.transform.position = chosenCubeBlock.transform.position + Vector3.left * (Block.QUAD_SIZE/2f + 0.01f);
											quadSelector.transform.rotation = Quaternion.Euler(0,0,90);
											break;
										case 4:
											quadSelector.transform.position = chosenCubeBlock.transform.position + Vector3.up * (Block.QUAD_SIZE/2f + 0.01f);
											quadSelector.transform.rotation = Quaternion.Euler(0,0,0);
											break;
										case 5:
											quadSelector.transform.position = chosenCubeBlock.transform.position + Vector3.down * (Block.QUAD_SIZE/2f + 0.01f);
											quadSelector.transform.rotation = Quaternion.Euler(0,0,180);
											break;
										}
										quadSelector.SetActive(true);
									GameMaster.realMaster.SetLookPoint (quadSelector.transform.position);
									}	
								break;
							}
						}
					}
					else { // no transfrom parent
						WorksiteSign ws = rh.collider.GetComponent<WorksiteSign>();
						if (ws != null) {
							chosenWorksite = ws.worksite;
							ws.showOnGUI = true;
							mode = UIMode.WorksitePanel;
						}
					}
			}				
			}
			else DropFocus();
		}
		if (systemInfoTimer > 0) {
			systemInfoTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (systemInfoTimer <= 0) {
				systemInfoString = null;
			}
		}
	}


	bool cursorIntersectGUI(Vector2 point) {
		if (touchscreenTemporarilyBlocked) return true;
		if ( point.y <= upPanelBox.height || point.x  <= leftPanelWidth) return true;
		if (mode != UIMode.View) {
			if (point.x >= rightPanelBox.x) return true;
			if (showBuildingCreateInfo) return true;
		}
		return false;
	}

	public void DropFocus() {
		lineDrawer.enabled = false;
		quadSelector.SetActive(false);
		chosenCubeBlock = null; 
		if (chosenStructure != null) {chosenStructure.showOnGUI = false;chosenStructure = null; }
		chosenSurfaceBlock = null; 
		if (chosenWorksite != null) {chosenWorksite.HideGUI(); chosenWorksite = null;}
		showBuildingCreateInfo = false;
		argument = 0;
		touchscreenTemporarilyBlocked = false;
	}

	void SwitchUIMode(UIMode newMode) {
		switch (newMode) {
		case UIMode.View:
			mode = UIMode.View;
			DropFocus();
			break;
		case UIMode.GameMenu:
			mode = UIMode.GameMenu;
			DropFocus();
			break;
		}
	}

	void ChangeArgument (byte newArg) {
		argument = newArg;
		switch (mode) {
		case UIMode.SurfaceBlockPanel:
			switch (argument) {
			case 1: // main actions menu
				chosenStructure = null;
				showBuildingCreateInfo = false;
				bufferedPosition = PixelPosByte.Empty;
				break;
			case 3:
				chosenBuildingIndex = -1;
				chosenStructure = null;
				showBuildingCreateInfo = false;
				break;
			}
			break;
		case UIMode.StructurePanel:
			
			break;
		}
	}

	public void ChangeSystemInfoString(string s) {
		if ( s == null ) return;
		systemInfoString = s;
		systemInfoTimer = 2;
	}

	void OnGUI() {
		GUI.skin = GameMaster.mainGUISkin;
		float k = GameMaster.guiPiece;
		//upLeft infopanel
		GUI.Box(upPanelBox, GUIContent.none);
		ColonyController cc = GameMaster.colonyController; 
		if (cc != null) {
			GUI.DrawTexture(new Rect(0,0, k,k), citizen_icon_tx, ScaleMode.StretchToFill);	
			GUI.Label(new Rect(k,0, 4*k,k), cc.freeWorkers.ToString() + " / " + cc.citizenCount.ToString() + " / "+ cc.totalLivespace.ToString());
			GUI.DrawTexture(new Rect(5*k, 0, k, k), energy_icon_tx, ScaleMode.StretchToFill);
			string energySurplusString = ((int)cc.energySurplus).ToString(); if (cc.energySurplus > 0) energySurplusString = '+' + energySurplusString;
			GUI.Label( new Rect ( 6 * k, 0, 5 * k, k) , ((int)cc.energyStored).ToString() + " / " + ((int)cc.totalEnergyCapacity).ToString() + " ( " + energySurplusString + " )" );
			GUI.DrawTexture ( new Rect ( 11 * k, 0, k, k), energyCrystal_icon_tx, ScaleMode.StretchToFill ) ;
			GUI.Label ( new Rect ( 12 * k, 0, k, k ), ((int)cc.energyCrystalsCount).ToString() ) ;
		} 
		//upRight infopanel
		Rect ur = new Rect(Screen.width - 4 *k, 0, 4 *k, upPanelBox.height);
		if (GUI.Button(ur, Localization.menu_gameMenuButton) ) SwitchUIMode(UIMode.GameMenu);
		ur.x -= ur.width;

		if (GUI.Button(ur, Localization.menu_colonyInfo)) {
			if ( !GameMaster.colonyController.showColonyInfo ) {
				GameMaster.colonyController.showColonyInfo = true;
				GameMaster.colonyController.storage.showStorage = false;
			}
			else GameMaster.colonyController.showColonyInfo = false;
		}
		ur.x -= ur.width;
		if (GUI.Button(ur, Localization.ui_storage_name)) {
			if ( !GameMaster.colonyController.storage.showStorage  ) {
				GameMaster.colonyController.storage.showStorage = true;
				GameMaster.colonyController.showColonyInfo = false;
			}
			else {
				GameMaster.colonyController.storage.showStorage = false;
			}
		}	


		//  RIGHT  PANEL : 
		if (mode != UIMode.View) {
			int guiDepth = GUI.depth;
			GUI.depth = GUIDEPTH_UI_MAIN;
			GUI.Box(rightPanelBox, GUIContent.none);
			Rect rr = new Rect(rightPanelBox.x, rightPanelBox.y, rightPanelBox.width, k);
			if (GUI.Button(rr, "Close Panel")) SwitchUIMode(UIMode.View); // СДЕЛАТЬ  ШТОРКОЙ
			rr.y += rr.height;

			switch (mode) {
			case UIMode.SurfaceBlockPanel:
				#region surfaceBlockPanel
				if (chosenSurfaceBlock == null) {
					mode = UIMode.View;
					DropFocus();
					break;
				}
				GUI.Label(rr, "Block (" + chosenSurfaceBlock.pos.x.ToString() + " , " + chosenSurfaceBlock.pos.y + " , " + chosenSurfaceBlock.pos.z + ')');
				rr.y += 2 * rr.height;
				switch (argument) {
				case 1:
					//BUILDING BUTTON
					if (GUI.Button(rr, Localization.ui_build)) ChangeArgument(3); // list of available buildings
					rr.y += rr.height;
					//GATHER BUTTON
					GatherSite gs = chosenSurfaceBlock.GetComponent<GatherSite>();
					if (gs == null) {
						if (GUI.Button(rr, Localization.ui_toGather)) { 
							gs = chosenSurfaceBlock.gameObject.AddComponent<GatherSite>();
							gs.Set(chosenSurfaceBlock);
						}
					}
					else {
						if (GUI.Button(rr, Localization.ui_cancelGathering)) Destroy(gs);
					} 
					rr.y += rr.height;
					//DIG BUTTON
					if (chosenSurfaceBlock.indestructible == false) {
						CleanSite cs = chosenSurfaceBlock.GetComponent<CleanSite>();
						if (cs == null) {
							if (GUI.Button(rr, Localization.ui_dig_block)) {
								if (chosenSurfaceBlock.artificialStructures > 0) ChangeArgument(2);
								else {
									cs = chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
									cs.Set(chosenSurfaceBlock, true);
								}
							}
						}
					}
					rr.y += rr.height;
					break;

				case 2: // подтверждение на снос при очистке
					GUI.Label(acceptBox, Localization.ui_accept_destruction_on_clearing);
					Rect r2 = new Rect(acceptBox.x, acceptBox.y + acceptBox.height / 2f,acceptBox.width/2f, acceptBox.height/2f);
					if (GUI.Button(r2, Localization.ui_accept)) {
						CleanSite cs2 =  chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
						cs2.Set(chosenSurfaceBlock, true);
						ChangeArgument(1);
					}
					r2.x += r2.width;
					if (GUI.Button(r2 , Localization.ui_decline)) ChangeArgument(1);
					break;
				case 3: // выбрать доступные для строительства домики
					if (GUI.Button(rr, Localization.menu_cancel)) {
						ChangeArgument(1);			
					}
					rr.y += rr.height;
					GUI.DrawTexture(new Rect(rr.x + (showingBuildingsLevel - 1) * rr.height, rr.y, rr.height, rr.height), PoolMaster.orangeSquare_tx, ScaleMode.StretchToFill);
					if (GameMaster.colonyController.hq != null) {
						if (GUI.Button(new Rect(rr.x, rr.y, rr.height, rr.height), "1")) {
							showingBuildingsLevel = 1;
							ChangeArgument(3);
						}
						if (GameMaster.colonyController.hq.level >= 2) {
							if (GUI.Button(new Rect(rr.x + rr.height, rr.y, rr.height, rr.height), "2")) {
								showingBuildingsLevel = 2;
								ChangeArgument(3);
							}
						}
					}
					rr.y += rr.height;
					int buildingIndex = 0;
					List<Building> showingBuildingsList = null;
					switch (showingBuildingsLevel) {
					case 1: showingBuildingsList = GameMaster.colonyController.buildings_level_1;break;
					case 2: showingBuildingsList = GameMaster.colonyController.buildings_level_2;break;
					case 3: showingBuildingsList = GameMaster.colonyController.buildings_level_3;break;
					case 4: showingBuildingsList = GameMaster.colonyController.buildings_level_4;break;
					case 5: showingBuildingsList = GameMaster.colonyController.buildings_level_5;break;
					case 6: showingBuildingsList = GameMaster.colonyController.buildings_level_6;break;
					}
					if ( showingBuildingsList != null && showingBuildingsList.Count != 0 ) {
						foreach (Building bd in showingBuildingsList) {
						if (bd.borderOnlyConstruction && !chosenSurfaceBlockIsBorderBlock) continue;
						if (GUI.Button(rr, Localization.structureName[bd.nameIndex])) {
							if (chosenStructure != bd ) {
								chosenStructure = bd; 
								chosenBuildingIndex = buildingIndex;
								showBuildingCreateInfo = true;
								if (chosenStructure.type != StructureType.MainStructure ) argument = 4;
								break;
							}
							else { // уже выбрано
								chosenStructure = null;
								showBuildingCreateInfo = false;
								break;
							}
						}
						rr.y += rr.height;
						// отрисовка информации о выбранном здании
						if (chosenBuildingIndex == buildingIndex && showBuildingCreateInfo)
						{
							Building chosenBuilding = chosenStructure as Building ;
							GUI.DrawTexture(new Rect(rr.x, rr.y, k, rr.height) , energyLightning_icon_tx, ScaleMode.ScaleToFit);
							float ens =  chosenBuilding.energySurplus;
							string enSurplusString = ens.ToString();
							if (ens > 0) enSurplusString = '+' + enSurplusString;
							GUI.Label( new Rect(rr.x + k, rr.y, rr.width/2f - k, rr.height) , enSurplusString);
							GUI.DrawTexture( new Rect(rr.x + rr.width/2f, rr.y, k, rr.height ) , buildingQuad_icon_tx, ScaleMode.StretchToFill);
							GUI.Label( new Rect(rr.x + rr.width/2f + 2 *k, rr.y, rr.width/2f, rr.height) , chosenStructure.innerPosition.x_size.ToString() + " x " + chosenStructure.innerPosition.z_size.ToString());
							rr.y += rr.height;
							foreach (ResourceContainer rc in ResourcesCost.info[chosenBuilding.resourcesContainIndex]) {
								GUI.Label(new Rect(rr.x, rr.y, rr.width * 0.75f, rr.height), rc.type.name);
								float wx = rr.width * 0.875f; if (wx > rr.height) wx = rr.height;
								GUI.DrawTexture( new Rect (rr.x + rr.width * 0.75f, rr.y, wx, wx), rc.type.icon, ScaleMode.ScaleToFit );
								GUI.Label( new Rect(rr.x + rr.width * 0.875f, rr.y, wx,wx), rc.volume.ToString());
								rr.y += rr.height;
							}
							//для крупных структур:
							if (chosenStructure.type == StructureType.MainStructure) {
								if (GUI.Button ( new Rect(rr.x + rr.width/4f, rr.y, rr.width / 2f, rr.height), Localization.ui_build )) {
									PixelPosByte mainStructurePosition = new PixelPosByte(SurfaceBlock.INNER_RESOLUTION / 2 - chosenStructure.innerPosition.x_size/2, SurfaceBlock.INNER_RESOLUTION / 2  -  chosenStructure.innerPosition.z_size/2);
									if (GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( ResourcesCost.info[chosenBuilding.resourcesContainIndex] )){
										if ( chosenSurfaceBlock.IsAnyBuildingInArea( new SurfaceRect (mainStructurePosition.x, mainStructurePosition.y, chosenStructure.innerPosition.x_size, chosenStructure.innerPosition.z_size) ) == false) {
											Structure s = Instantiate(chosenStructure).GetComponent<Structure>();
											s.gameObject.SetActive(true);
											s.SetBasement(chosenSurfaceBlock, mainStructurePosition);
											ChangeArgument(3);
											break;
										}
										else {
											bufferedPosition = mainStructurePosition; 
											ChangeArgument(5);
											break;
										}
									}
									else {
										systemInfoString = "Not enough resources"; systemInfoTimer = 2;
									}
								}
								rr.y += rr.height;
							}
						}
						buildingIndex++;
					}
				}
					// сетка для размещения постройки
					if (argument != 4) GUI.DrawTexture(buildingGridRect, grid16_tx, ScaleMode.StretchToFill); // чтобы не дублировалось
					float p = buildingGridRect.width / SurfaceBlock.INNER_RESOLUTION;
					int n = 0;
					while ( n < chosenSurfaceBlock.surfaceObjects.Count) {
						if (chosenSurfaceBlock.surfaceObjects[n] == null) {chosenSurfaceBlock.RequestAnnihilationAtIndex(n); n++; continue;}
						Texture t = PoolMaster.quadSelector_tx;
						switch (chosenSurfaceBlock.surfaceObjects[n].type) {
						case StructureType.HarvestableResources: t = PoolMaster.orangeSquare_tx;break;
						case StructureType.MainStructure: t = whiteSpecial_tx;break;
						case StructureType.Plant: t = greenSquare_tx;break;
						case StructureType.Structure : t = whiteSquare_tx;break;
						}
						SurfaceRect sr= chosenSurfaceBlock.surfaceObjects[n].innerPosition;
						GUI.DrawTexture(new Rect(buildingGridRect.x + sr.x * p, buildingGridRect.y + buildingGridRect.height -  (sr.z+ sr.z_size) * p , p * sr.x_size, p * sr.z_size), t, ScaleMode.StretchToFill);
						n++;
					}
						
					break;
				case 4:
					GUI.DrawTexture(buildingGridRect, grid16_tx, ScaleMode.StretchToFill); 
					p = buildingGridRect.width / SurfaceBlock.INNER_RESOLUTION;
					SurfaceRect surpos = chosenStructure.innerPosition;
					for (byte i =0; i < chosenSurfaceBlock.map.GetLength(0); i++) {
						for (byte j = 0; j < chosenSurfaceBlock.map.GetLength(1); j++) {
							if ( i <= SurfaceBlock.INNER_RESOLUTION - chosenStructure.innerPosition.x_size && j <= SurfaceBlock.INNER_RESOLUTION - chosenStructure.innerPosition.z_size) {
								if (GUI.Button(new Rect(buildingGridRect.x + i * p, buildingGridRect.y + buildingGridRect.height - (j+1) *p, p, p), yellowSquare_tx, GameMaster.mainGUISkin.customStyles[1])) {								
									if (GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( ResourcesCost.info[(chosenStructure as Building).resourcesContainIndex] )) {
										surpos.x = i; surpos.z = j;
										if ( chosenSurfaceBlock.IsAnyBuildingInArea(surpos) == false) {
											Structure s = Instantiate(chosenStructure);
											s.gameObject.SetActive(true);
											s.SetBasement(chosenSurfaceBlock, new PixelPosByte(i,j));
										}
										else {
											bufferedPosition = new PixelPosByte(surpos.x, surpos.z); 
											ChangeArgument(5);
											break;
										}
									}
									else { systemInfoString = "Not enough resources"; systemInfoTimer = 2;}
								}
							}
						}
					}
					goto case 3;
					break;
				case 5: // подтверждение на снос при строительстве
					GUI.Box(acceptBox, Localization.ui_accept_destruction_on_clearing);
					if (GUI.Button (new Rect(acceptBox.x, acceptBox.y + acceptBox.height/2f, acceptBox.width/2f, acceptBox.height/2f), Localization.ui_accept)) {
						if ( bufferedPosition != PixelPosByte.Empty ) {
							Structure s = Instantiate(chosenStructure);
							s.gameObject.SetActive(true);
							s.SetBasement(chosenSurfaceBlock, bufferedPosition);
							ChangeArgument(3);
						}
					} 
					if (GUI.Button (new Rect(acceptBox.x + acceptBox.width/2f, acceptBox.y + acceptBox.height/2f, acceptBox.width/2f, acceptBox.height/2f), Localization.ui_decline)) {
						bufferedPosition = PixelPosByte.Empty;
						ChangeArgument(3);
					}
					break;
				}
				break;
				#endregion
			case UIMode.CubeBlockPanel:
				if (chosenCubeBlock == null) {
					mode = UIMode.View;
					DropFocus();
					break;
				}
				#region cubeBlockPanel
				if (faceIndex == 4) { // верхний
					if ( !chosenCubeBlock.career ) {
						ChunkPos cpos = chosenCubeBlock.pos;
						cpos = new ChunkPos(cpos.x, cpos.y + 1, cpos.z);
						chosenCubeBlock.myChunk.AddBlock(cpos, BlockType.Surface, chosenCubeBlock.material_id);
						chosenSurfaceBlock = chosenCubeBlock.myChunk.GetBlock(cpos.x, cpos.y, cpos.z) as SurfaceBlock;
						mode = UIMode.SurfaceBlockPanel;
						chosenCubeBlock = null;
					}
				}
				#endregion
				break;
			case UIMode.WorksitePanel:
				if (chosenWorksite == null) {
					mode = UIMode.View;
					DropFocus();
					break;
				}
				// on worksite.cs.OnGUI
				break;
			case UIMode.StructurePanel:
				if (chosenStructure == null) {
					mode = UIMode.View;
					DropFocus();
					break;
				}
				rr.y += rr.height;
				Building b = chosenStructure as Building;
				if ( b != null) {
					if (GUI.Button(new Rect(rr.xMax - rr.height, rr.y, rr.height, rr.height), demolishButton_tx)) {
						mode = UIMode.View;
						b.Demolish();
						DropFocus();
						return;
					}
					rr.y += rr.height;
					GUI.Label(rr,  Localization.structureName[chosenStructure.nameIndex] + " (" + Localization.info_level + b.level.ToString() + ") "); 
					rr.y += rr.height;

					bool act = GUI.Toggle(new Rect(rr.x, rr.y, rr.width / 2f, rr.height), b.isActive, Localization.ui_activeSelf); 
					if (act != b.isActive) b.SetActivationStatus(act);
					GUI.DrawTexture( new Rect (rr.x + rr.width/2f, rr.y, rr.height, rr.height), energyLightning_icon_tx, ScaleMode.StretchToFill);
					if (b.isActive && b.energySupplied) {
						string surplusString = b.energySurplus.ToString();
						if (b.energySurplus > 0) surplusString = '+' + surplusString;
						GUI.Label( new Rect(rr.x + rr.width/2f + rr.height, rr.y, rr.width/2f - rr.height, rr.height),  surplusString );
					}
					else GUI.Label( new Rect(rr.x + rr.width/2f + rr.height, rr.y, rr.width/2f - rr.height, rr.height),  "offline" );
					rr.y += rr.height;
					WorkBuilding wb = b as WorkBuilding;
					if (wb != null) {
						GUI.Label (new Rect(rr.x, rr.y, rr.height, rr.height), "0" );
						int wcount = (int)GUI.HorizontalSlider(new Rect(rr.x + rr.height, rr.y, rr.width - 2 * rr.height, rr.height), wb.workersCount, 0, wb.maxWorkers);
						GUI.Label( new Rect (rr.xMax - rr.height, rr.y, rr.height, rr.height), wb.maxWorkers.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
						GUI.Label( new Rect (rr.x + rr.width /2f - rr.height/4f, rr.y, rr.height, rr.height/2f), wb.workersCount.ToString() );
						if (wcount != wb.workersCount) {
							if (wcount > wb.workersCount) GameMaster.colonyController.SendWorkers(wcount - wb.workersCount, wb, WorkersDestination.ForWorkBuilding);
							else wb.FreeWorkers(wb.workersCount - wcount);
						}
						rr.y += rr.height;
					}
				}
				chosenStructure.gui_ypos = rr.y;
				break;
			}
	}
		if (systemInfoString != null) GUI.Label(systemInfoRect, systemInfoString, PoolMaster.GUIStyle_SystemAlert);
	}

	public void AddFactoryToList (Factory f) {
		switch ( f.factoryType ) {
		case FactoryType.Simple:
			switch (f.specialization) {
			case FactorySpecialization.Unspecialized:
				if ( !hasUnspecializedFactories ) unspecializedFactories =new List<Factory>();
				hasUnspecializedFactories = true;
				unspecializedFactories.Add(f);
				break;
			case FactorySpecialization.Smeltery:
				if ( !hasSmelteries ) smelteriesList =new List<Factory>();
				hasSmelteries= true;
				smelteriesList.Add(f);
				break;
			}
			break;
		}
	}


	public void RemoveFromFactoriesList (Factory f) {
		switch ( f.factoryType ) {
		case FactoryType.Simple:
			switch ( f.specialization ) {
			case FactorySpecialization.Smeltery:
				RemoveObjectFromFactoryList( smelteriesList, f, ref hasSmelteries);
				break;
			case FactorySpecialization.Unspecialized:
				RemoveObjectFromFactoryList( unspecializedFactories, f, ref hasUnspecializedFactories);
				break;
			}
			break;
		}
	}
	void RemoveObjectFromFactoryList(List<Factory> list, Factory obj, ref bool presenceMarker) {
		if (list == null) {presenceMarker = false; return;}
		int i = 0;
		while ( i < list.Count ) {
			if (list[i] == null || list[i] == obj) list.RemoveAt(i);
			i++;
		}
		if (list.Count == 0) {
			presenceMarker = false;
		}
	}
}
