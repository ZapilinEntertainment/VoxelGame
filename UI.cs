using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UIMode {View, SurfaceBlockPanel, CubeBlockPanel, StructurePanel, WorksitePanel}

public class UI : MonoBehaviour {
	public LineRenderer lineDrawer;
	public static UI current;

	UIMode mode; int argument = 0, serviceRectArgument = 0;
	public Rect chosenObjectRect_real = Rect.zero, chosenObjectRect_planned = Rect.zero, staticRect = Rect.zero, 
	objectInfo_rect = Rect.zero, serviceBoxRect = Rect.zero;
	bool staticRectActive = false, infoRectActive = false;
	PixelPosByte bufferedPosition; int bufferedArgument = -1;
	SurfaceBlock chosenSurfaceBlock;
	CubeBlock chosenCubeBlock; byte faceIndex = 10;
	Structure chosenStructure;
	Worksite chosenWorksite;
	Vector2 mousePos;
	GameObject quadSelector;
	public float upPanelHeight, leftPanelWidth;
	Texture  cellSelectionFrame_tx, grid16_tx, greenSquare_tx, orangeSquare_tx, whiteSquare_tx, whiteSpecial_tx, yellowSquare_tx,
		citizen_icon_tx, energy_icon_tx, energyCrystal_icon_tx;

	List<Factory> smelteriesList; bool hasSmelteries = false;
	List<Factory> unspecializedFactories; bool hasUnspecializedFactories = false;

	void Awake() {
		current = this;
		mode = UIMode.View;
		quadSelector = Instantiate(Resources.Load<GameObject>("Prefs/QuadSelector"));
		quadSelector.SetActive(false);
		upPanelHeight = GameMaster.guiPiece;
		leftPanelWidth = GameMaster.guiPiece;
		GameMaster.realMaster.AddToCameraUpdateBroadcast(gameObject);
		cellSelectionFrame_tx = Resources.Load<Texture>("Textures/quadSelector");
		grid16_tx = Resources.Load<Texture>("Textures/AccessibleGrid");
		greenSquare_tx = Resources.Load<Texture>("Textures/greenSquare");
		orangeSquare_tx = Resources.Load<Texture>("Textures/orangeSquare");
		whiteSquare_tx = Resources.Load<Texture>("Textures/whiteSquare");
		whiteSpecial_tx = Resources.Load<Texture>("Textures/whiteSpecialSquare");
		yellowSquare_tx = Resources.Load<Texture>("Textures/yellowSquare");
		citizen_icon_tx = Resources.Load<Texture>("Textures/citizen_icon");
		energy_icon_tx = Resources.Load<Texture>("Textures/energy_icon");
		energyCrystal_icon_tx = Resources.Load<Texture>("Textures/energyCrystal_icon");
	}
	void Update() {
		mousePos = Input.mousePosition;
		mousePos.y = Screen.height - mousePos.y;
		if (Input.GetMouseButtonDown(0) && !cursorIntersectGUI(mousePos) ) {
			if (serviceRectArgument == -1) serviceBoxRect = Rect.zero;
			RaycastHit rh;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rh)) {
				DropFocus();
				if (rh.collider.transform.parent != null) {
				Block b = rh.collider.transform.parent.GetComponent<Block>();
				if (b != null) {
					Vector2 cursorPos = Camera.main.WorldToScreenPoint(b.transform.position);
					chosenObjectRect_planned.x = cursorPos.x;
					chosenObjectRect_planned.y =  Screen.height - cursorPos.y;
					float x ,y,z,d = Block.QUAD_SIZE/2f;
					if ( b.type == BlockType.Surface ) {
							mode = UIMode.SurfaceBlockPanel;
							ChangeArgument(1);
							chosenSurfaceBlock = b.GetComponent<SurfaceBlock>();
							x= chosenSurfaceBlock.transform.position.x; y = chosenSurfaceBlock.transform.position.y - d + 0.01f;  z = chosenSurfaceBlock.transform.position.z;
							lineDrawer.SetPositions(new Vector3[5]{new Vector3(x - d, y, z+d), new Vector3(x+d,y,z+d), new Vector3(x+d,y,z-d), new Vector3(x - d, y, z-d), new Vector3(x-d, y,z+d)});
							lineDrawer.material = PoolMaster.lr_green_material;
							lineDrawer.enabled = true;
						GameMaster.realMaster.SetLookPoint (chosenSurfaceBlock.transform.position + Vector3.down * Block.QUAD_SIZE /2f);
					}
					else {
						chosenCubeBlock = b.GetComponent<CubeBlock>();
							for (byte i =0; i< 6; i++) {
								if (chosenCubeBlock.faces[i] == null) continue;
								if (chosenCubeBlock.faces[i].GetComponent<Collider>() == rh.collider ) {faceIndex = i;break;}
							}
							if (faceIndex != 10) {
								mode = UIMode.CubeBlockPanel;
								ChangeArgument(1);
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
					}
				}
				else {
					Structure s = rh.collider.GetComponent<Structure>();
					if (s != null) {
						chosenStructure = s;
						Vector2 cursorPos = Camera.main.WorldToScreenPoint(b.transform.position);
						chosenObjectRect_planned.x = cursorPos.x;
						chosenObjectRect_planned.y =  Screen.height - cursorPos.y;
						mode = UIMode.StructurePanel;
						GameMaster.realMaster.SetLookPoint (chosenStructure.transform.position);
						ChangeArgument(1);
					}
				}
				CameraUpdate(Camera.main.transform);
				}
				else {
					WorksiteSign ws = rh.collider.GetComponent<WorksiteSign>();
					if (ws != null) {
						chosenWorksite = ws.worksite;
						ws.showOnGUI = true;
						mode = UIMode.WorksitePanel;
						ChangeArgument(1);
					}
				}
			}
			else DropFocus();
		}
	}

	public void CameraUpdate(Transform t) {
		Vector2 npos = new Vector2(chosenObjectRect_real.x, chosenObjectRect_real.y);
		Camera c = t.GetComponent<Camera>();
		Vector3 correctionVector = Vector3.zero;
		switch (mode) {
		case UIMode.CubeBlockPanel: 
			if (chosenCubeBlock == null) DropFocus();
			else {
				correctionVector = t.TransformDirection(Vector3.right) * Block.QUAD_SIZE * 0.7f;
				npos = c.WorldToScreenPoint(chosenCubeBlock.transform.position + correctionVector); 
				npos.y = Screen.height - npos.y;	
			}
			break;
		case UIMode.StructurePanel:
			if (chosenStructure == null) DropFocus();
			else {
					correctionVector = t.TransformDirection(Vector3.right) * (chosenStructure.innerPosition.x_size + chosenStructure.innerPosition.z_size)/4f * Block.QUAD_SIZE / (float)SurfaceBlock.INNER_RESOLUTION;
					npos = c.WorldToScreenPoint(chosenStructure.transform.position + correctionVector); 
					npos.y = Screen.height - npos.y;	
			}			
			break;
		case UIMode.SurfaceBlockPanel:
			if (chosenSurfaceBlock== null) DropFocus();
			else {
				correctionVector = t.TransformDirection(Vector3.right) * Block.QUAD_SIZE * 0.7f;
				npos = c.WorldToScreenPoint(chosenSurfaceBlock.transform.position + correctionVector); 
				npos.y = Screen.height - npos.y;
			}
			break;
		}
		chosenObjectRect_real.x = npos.x; chosenObjectRect_real.y = npos.y;
		chosenObjectRect_planned.x = chosenObjectRect_real.x ; chosenObjectRect_planned.y = chosenObjectRect_real.y;
	}

	bool cursorIntersectGUI(Vector2 point) {
		if ( point.y <= upPanelHeight || point.x  <= leftPanelWidth) return true;
		if (serviceBoxRect.width != 0)  {
			if (point.x >= serviceBoxRect.x && point.y <= serviceBoxRect.y + serviceBoxRect.height) return true;
		}
		if (mode != UIMode.View) {
			if (point.x >= chosenObjectRect_real.x && point.x <= chosenObjectRect_real.x + chosenObjectRect_real.width 
				&& point.y >= chosenObjectRect_real.y && point.y <= chosenObjectRect_real.y + chosenObjectRect_real.height) return true;
		}
		if (staticRectActive) {
			if (point.x >= staticRect.x && point.y >= staticRect.y && point.x <= staticRect.x + staticRect.width && point.y <= staticRect.y + staticRect.height) return true;
		}
		return false;
	}

	public void DropFocus() {
		mode = UIMode.View;
		ChangeArgument(0);
		lineDrawer.enabled = false;
		quadSelector.SetActive(false);
		chosenObjectRect_planned.width = 0;
		chosenObjectRect_planned.height = 0;
		chosenCubeBlock = null; chosenStructure = null; chosenSurfaceBlock = null; 
		if (chosenWorksite != null) {chosenWorksite.HideGUI();chosenWorksite = null;}
		if (serviceRectArgument == -1) serviceBoxRect = Rect.zero;
		staticRect = Rect.zero; staticRectActive = false;
		infoRectActive = false; objectInfo_rect = Rect.zero;
	}

	void ChangeArgument(int f_argument) {
		argument = f_argument;
		if (serviceRectArgument == 1 && f_argument != 0) serviceRectArgument = 0;
		float k = GameMaster.guiPiece;
		switch (mode) {
		case UIMode.View:
			break;
		case UIMode.SurfaceBlockPanel:
			switch (argument) {
			case 1: // choose action with block
				chosenObjectRect_planned.width = 4.5f * k;
				chosenObjectRect_planned.height = 4.5f *k;
				break;
			case 2: // accept destruction on clearing area
				chosenObjectRect_planned.width = 6 *k;
				chosenObjectRect_planned.height = 3 * k;
				break;
			case 3: // choose building 
				chosenObjectRect_planned.width = 0;
				chosenObjectRect_planned.width = 0;
				serviceBoxRect = new Rect(Screen.width - 4 *k, upPanelHeight, 4 *k, k * (GameMaster.colonyController.buildings_level_1.Count + 1));
				if (serviceRectArgument != 0) {GameMaster.colonyController.showColonyInfo = false; GameMaster.colonyController.storage.showStorage = false;}
				serviceRectArgument = -1;

				// build list and build grid
				float p = Screen.width/2f / SurfaceBlock.INNER_RESOLUTION;
				float side = p * SurfaceBlock.INNER_RESOLUTION;
				staticRect = new Rect(Screen.width / 2f -  side/2f, Screen.height/2f - side/2f, side, side);
				if (chosenSurfaceBlock.map == null) chosenSurfaceBlock.GetBooleanMap();
				break;
			case 5: //accept destruction on construction site
				staticRect = new Rect(Screen.width /2f - 4*k, Screen.height/2f - 2*k, 8*k, 4*k);
				break;
			}
			break;
		case UIMode.CubeBlockPanel:
			switch (argument)
			{
			case 1: // choose action
				chosenObjectRect_planned.width = 4 * k;
				if (chosenCubeBlock.career) chosenObjectRect_planned.height = 3 *k; else chosenObjectRect_planned.height = 2 *k;
				break;
			}
			break;
		case UIMode.StructurePanel:
			break;
		case UIMode.WorksitePanel:
			Vector2 signPos = Camera.main.WorldToScreenPoint(chosenWorksite.transform.position);
			signPos.y = Screen.height - signPos.y;
			chosenObjectRect_planned = new Rect(signPos.x - 2 *k, signPos.y - 2 *k, 4 *k, 5*k);
			break;
		}
	}

	void OnGUI() {
		staticRectActive = false;
		upPanelHeight = GameMaster.guiPiece;
		leftPanelWidth = GameMaster.guiPiece;
		GUI.skin = GameMaster.mainGUISkin;
		if (chosenObjectRect_real != chosenObjectRect_planned) {
			chosenObjectRect_real.width = Mathf.MoveTowards(chosenObjectRect_planned.width, chosenObjectRect_planned.width, 10);
			chosenObjectRect_real.height = Mathf.MoveTowards(chosenObjectRect_planned.height, chosenObjectRect_planned.height, 10);
			chosenObjectRect_real.x = Mathf.MoveTowards(chosenObjectRect_planned.x, chosenObjectRect_planned.x, 20);
			chosenObjectRect_real.y = Mathf.MoveTowards(chosenObjectRect_planned.y, chosenObjectRect_planned.y, 20);
		}
		if (chosenSurfaceBlock == null && mode == UIMode.SurfaceBlockPanel) DropFocus();
		if (chosenCubeBlock == null && mode == UIMode.CubeBlockPanel) DropFocus();

		float k = GameMaster.guiPiece;

		//upLeft infopanel
		ColonyController cc = GameMaster.colonyController; 
		if (cc != null) {
			GUI.DrawTexture(new Rect(0,0, k,k), citizen_icon_tx, ScaleMode.StretchToFill);	
			GUI.Label(new Rect(k,0, 4*k,k), cc.freeWorkers.ToString() + " / " + cc.citizenCount.ToString() + " / "+ cc.totalLivespace.ToString());
			GUI.DrawTexture(new Rect(5*k, 0, k, k), energy_icon_tx, ScaleMode.StretchToFill);
			string energySurplusString = ((int)cc.energySurplus).ToString(); if (cc.energySurplus > 0) energySurplusString = '+' + energySurplusString;
			GUI.Label( new Rect ( 6 * k, 0, 4 * k, k) , ((int)cc.energyStored).ToString() + " / " + ((int)cc.totalEnergyCapacity).ToString() + " ( " + energySurplusString + " )" );
			GUI.DrawTexture ( new Rect ( 10 * k, 0, k, k), energyCrystal_icon_tx, ScaleMode.StretchToFill ) ;
			GUI.Label ( new Rect ( 11 * k, 0, k, k ), ((int)cc.energyCrystalsCount).ToString() ) ;
		} 
		//upRight infopanel
		Rect ur = new Rect(Screen.width - 4 *k, 0, 4 *k, upPanelHeight);
		if (GUI.Button(ur, Localization.menu_gameMenuButton) ) {
			if (serviceRectArgument != 1) {
				serviceBoxRect = new Rect(ur.x, upPanelHeight, ur.width, 4* k);
				serviceRectArgument = 1;
				GameMaster.colonyController.storage.showStorage = false;
				GameMaster.colonyController.showColonyInfo = false;
				DropFocus();
			}
			else {
				serviceRectArgument = 0;
				serviceBoxRect = Rect.zero;
			}
		}
		ur.x -= ur.width;
		if (GUI.Button(ur, Localization.ui_storage_name)) {
			if (serviceRectArgument != 2 ) {
				serviceBoxRect = new Rect(ur.x, upPanelHeight, ur.width * 2,  4 * k);
				serviceRectArgument = 2;
				GameMaster.colonyController.storage.showStorage = true;
				GameMaster.colonyController.showColonyInfo = false;
			}
			else {
				serviceRectArgument = 0;
				GameMaster.colonyController.storage.showStorage = false;
				serviceBoxRect = Rect.zero;
			}
		}
		ur.x -= ur.width;
		if (GUI.Button(ur, Localization.menu_colonyInfo)) {
			if (serviceRectArgument != 3) {
			serviceBoxRect = new Rect(ur.x, upPanelHeight, ur.width,  4 * k);
			serviceRectArgument = 3;
			GameMaster.colonyController.showColonyInfo = true;
			GameMaster.colonyController.storage.showStorage = false;
			}
			else {
				serviceRectArgument = 0;
				serviceBoxRect = Rect.zero;
				GameMaster.colonyController.showColonyInfo = false;
			}
		}
		if (serviceRectArgument != 0) GUI.Box(serviceBoxRect, GUIContent.none);

		// -------------SWITCHING      UI     MODES : 
		switch (mode) {
		case UIMode.View:
			switch (argument) {
			case 1: // system menu				
				break;
			case 2: // storage
				break;
			case 3: // colony info
				break;
			}
			break;

		case UIMode.SurfaceBlockPanel:
			#region surfaceBlockPanel
			if (chosenObjectRect_real.width != 0 && chosenObjectRect_real.height != 0) GUI.Box(chosenObjectRect_real, GUIContent.none);
			switch (argument) {
			case 1:
				//BUILDING BUTTON
				Rect r = chosenObjectRect_real; 
				r.height /= 4f;
				if (GUI.Button(r, Localization.ui_build)) {	ChangeArgument(3); } // list of available buildings
				r.y += r.height;
				//GATHER BUTTON
				GatherSite gs = chosenSurfaceBlock.GetComponent<GatherSite>();
				if (gs == null) {
					if (GUI.Button(r, Localization.ui_toGather)) { 
						gs = chosenSurfaceBlock.gameObject.AddComponent<GatherSite>();
						gs.Set(chosenSurfaceBlock);
					}
				}
				else {
					if (GUI.Button(r, Localization.ui_cancelGathering)) Destroy(gs);
				} 
				r.y += r.height;
				//DIG BUTTON
				if (chosenSurfaceBlock.indestructible == false) {
					CleanSite cs = chosenSurfaceBlock.GetComponent<CleanSite>();
					DigSite ds = chosenSurfaceBlock.basement.GetComponent<DigSite>();
					if (cs == null) {
						if (GUI.Button(r, Localization.ui_dig_block)) {
							if (chosenSurfaceBlock.artificialStructures > 0) ChangeArgument(2);
							else {
								cs = chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
								cs.Set(chosenSurfaceBlock, true);
							}
						}
					}
				}
				r.y += r.height;
				// CANCEL BUTTON
				if (GUI.Button(r, Localization.menu_cancel)) {DropFocus();}
				break;

			case 2: // подтверждение на снос при очистке
				Rect r2 = chosenObjectRect_real;
				r2.height /= 2f;
				GUI.Label(r2, Localization.ui_accept_destruction_on_clearing);
				r2.width /=2f; r2.y = chosenObjectRect_real.y + chosenObjectRect_real.height / 2f;
				if (GUI.Button(r2, Localization.ui_accept)) {
					CleanSite cs2 =  chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
					cs2.Set(chosenSurfaceBlock, true);
					argument = 1;
				}
				r2.x += r2.width;
				if (GUI.Button(r2 , Localization.ui_decline)) ChangeArgument(1);
				break;
			case 3: // выбрать доступные для строительства домики
				Rect r3 = serviceBoxRect; r3.height = k;
				if (GUI.Button(r3, Localization.menu_cancel)) {
					ChangeArgument(1); 
					serviceRectArgument = 0;
					chosenStructure = null;
				}
				r3.y += r3.height;
				foreach (Building bd in GameMaster.colonyController.buildings_level_1) {
					if (GUI.Button(r3, bd.structureName)) {
						chosenStructure = bd; 
						if (chosenStructure.type != StructureType.MainStructure) {	ChangeArgument(4); }
						else {
							PixelPosByte mainStructurePosition = new PixelPosByte(SurfaceBlock.INNER_RESOLUTION / 2 - chosenStructure.innerPosition.x_size/2, SurfaceBlock.INNER_RESOLUTION / 2  -  chosenStructure.innerPosition.z_size/2);
							if ( chosenSurfaceBlock.IsAnyBuildingInArea( new SurfaceRect (mainStructurePosition.x, mainStructurePosition.y, chosenStructure.innerPosition.x_size, chosenStructure.innerPosition.z_size) ) == false) {
								Structure s = Instantiate(chosenStructure).GetComponent<Structure>();
								s.gameObject.SetActive(true);
								s.SetBasement(chosenSurfaceBlock, mainStructurePosition);
								chosenStructure = null;
							}
							else {
								bufferedPosition = mainStructurePosition; 
								bufferedArgument = 3;
								ChangeArgument(5);
							}
						}
					}
					r3.y += r3.height;
				}

				if (chosenStructure == null) GUI.DrawTexture(staticRect, grid16_tx, ScaleMode.StretchToFill);
				staticRectActive = true;
				float p = staticRect.width / SurfaceBlock.INNER_RESOLUTION;
				foreach (SurfaceObject so in chosenSurfaceBlock.surfaceObjects) {
					Texture t = cellSelectionFrame_tx;
					switch (so.structure.type) {
					case StructureType.HarvestableResources: t = orangeSquare_tx;break;
					case StructureType.MainStructure: t = whiteSpecial_tx;break;
					case StructureType.Plant: t = greenSquare_tx;break;
					case StructureType.Structure : t = whiteSquare_tx;break;
					}
					GUI.DrawTexture(new Rect(staticRect.x + so.rect.x * p, staticRect.y + staticRect.height -  (so.rect.z+ so.rect.z_size) * p , p * so.rect.x_size, p * so.rect.z_size), t, ScaleMode.StretchToFill);
				}
					
				break;
			case 4:
				GUI.DrawTexture(staticRect, grid16_tx, ScaleMode.StretchToFill);
				p = staticRect.width / SurfaceBlock.INNER_RESOLUTION;
				SurfaceRect surpos = chosenStructure.innerPosition;
				for (byte i =0; i < chosenSurfaceBlock.map.GetLength(0); i++) {
					for (byte j = 0; j < chosenSurfaceBlock.map.GetLength(1); j++) {
						if ( i <= SurfaceBlock.INNER_RESOLUTION - chosenStructure.innerPosition.x_size && j <= SurfaceBlock.INNER_RESOLUTION - chosenStructure.innerPosition.z_size) {
							if (GUI.Button(new Rect(staticRect.x + i * p, staticRect.y + staticRect.height - (j+1) *p, p, p), yellowSquare_tx, GameMaster.mainGUISkin.customStyles[1])) {								
								surpos.x = i; surpos.z = j;
								if ( chosenSurfaceBlock.IsAnyBuildingInArea(surpos) == false) {
									Structure s = Instantiate(chosenStructure).GetComponent<Structure>();
									s.gameObject.SetActive(true);
									s.SetBasement(chosenSurfaceBlock, new PixelPosByte(i,j));
								}
								else {
									bufferedPosition = new PixelPosByte(surpos.x, surpos.z); 
									bufferedArgument = 3;
									ChangeArgument(5);
								}
							}
						}
					}
				}
				goto case 3;
				break;
			case 5: // подтверждение на снос при строительстве
				GUI.Box(staticRect, Localization.ui_accept_destruction_on_clearing);
				staticRectActive = true;
				Rect r5 = new Rect(staticRect.x, staticRect.y + staticRect.height /2f, staticRect.width/2f, staticRect.height/2f);
				if (GUI.Button (r5, Localization.ui_accept)) {
					if ( bufferedPosition != PixelPosByte.Empty ) {
						Structure s = Instantiate(chosenStructure);
						s.gameObject.SetActive(true);
						s.SetBasement(chosenSurfaceBlock, bufferedPosition);
						chosenStructure = null;
						bufferedPosition = PixelPosByte.Empty;
						ChangeArgument(bufferedArgument);
					}
				} r5.x += r5.width;
				if (GUI.Button (r5, Localization.ui_decline)) {
					bufferedPosition = PixelPosByte.Empty;
					ChangeArgument(bufferedArgument);
				}
				break;
			case 6: // размещение шахт
				Mine m = chosenStructure.GetComponent<Mine>();
				float p6 = staticRect.width / SurfaceBlock.INNER_RESOLUTION;
				if (m != null) {
					int width = 0;
					bool[] line = new bool[SurfaceBlock.INNER_RESOLUTION];
					switch (m.oriented) {
					case 0: // north, z+
						width = SurfaceBlock.INNER_RESOLUTION - m.innerPosition.z_size;
						for (int i = 0; i < SurfaceBlock.INNER_RESOLUTION; i++) {
							line[i] = chosenSurfaceBlock.map [i, width];
						}
						break;
					case 1: // east, x+
						width = SurfaceBlock.INNER_RESOLUTION - m.innerPosition.x_size;
						for (int i = SurfaceBlock.INNER_RESOLUTION - 1; i >=0; i--) {
							line[i] = chosenSurfaceBlock.map [width, i];
						}
						break;
					case 2:  // south, z-
						width = m.innerPosition.x_size - 1;
						for (int i = 0; i < SurfaceBlock.INNER_RESOLUTION; i++) {
							line[i] = chosenSurfaceBlock.map [i, width];
						}
						break;
					case 3: // west, x-
						width = m.innerPosition.x_size - 1 ;
						for (int i = 0; i < SurfaceBlock.INNER_RESOLUTION; i++) {
							line[i] = chosenSurfaceBlock.map [width, i];
						}
						break;
					}
					Rect r6 = staticRect; r6.width /= (float)SurfaceBlock.INNER_RESOLUTION;
					for (int i = 0; i < SurfaceBlock.INNER_RESOLUTION; i++) {
						if (GUI.Button(r6, yellowSquare_tx)) {
							if (line[i] == true) {
								// ????
							}
						}
						r6.x += p6;
					}
				}
				break;
			case 7: // размещение причалов
				break;
			}
			break;
			#endregion
		case UIMode.CubeBlockPanel:
			#region cubeBlockPanel
			if (chosenObjectRect_real.width != 0 && chosenObjectRect_real.height != 0) GUI.Box(chosenObjectRect_real, GUIContent.none);
			switch (argument) {
			case 1:				
				Rect r = chosenObjectRect_real;
				byte actionsCount = 1;
				bool digButton = false, pourInButton = false;
				DigSite ds = chosenCubeBlock.GetComponent<DigSite>();
				Block sb = chosenCubeBlock.myChunk.GetBlock(chosenCubeBlock.pos.x, chosenCubeBlock.pos.y + 1, chosenCubeBlock.pos.z);
				if (ds == null && chosenCubeBlock.indestructible == false && ( sb == null || (sb != null && sb.type != BlockType.Surface) )) {
					actionsCount++;
					digButton = true;
					if (chosenCubeBlock.career) {
						actionsCount++;
						pourInButton = true;
					}
					chosenObjectRect_planned.height = actionsCount * k;
					 r.height /= (float)actionsCount;
					if (digButton) {
						if (GUI.Button (r, Localization.ui_dig_block)) { chosenCubeBlock.gameObject.AddComponent<DigSite>().Set(chosenCubeBlock, true); }
						r.y += r.height;
					}
					if (pourInButton) {
						if (GUI.Button (r, Localization.ui_pourIn)) { chosenCubeBlock.gameObject.AddComponent<DigSite>().Set(chosenCubeBlock, false); }
						r.y += r.height;
					}
				}
				if (GUI.Button(r, Localization.menu_cancel)) DropFocus();
				break;
			}
			#endregion
			break;
		case UIMode.WorksitePanel:
			// on worksite.cs.OnGUI
			break;
		case UIMode.StructurePanel:
			Building b = chosenStructure as Building;
			GUI.Box(chosenObjectRect_real, GUIContent.none);
			Rect rs1 = chosenObjectRect_real; 
			if ( b != null) {
				rs1.height /= 5;
				if (GUI.Button(new Rect(rs1.x + rs1.width - k, rs1.y, k, k), "Destroy")) {b.Demolish();}
				rs1.y += rs1.height;
				GUI.Label(rs1, chosenStructure.structureName + ' ' + Localization.info_level + b.level.ToString()); rs1.y += rs1.height;
				bool act = GUI.Toggle(rs1, b.isActive, Localization.ui_activeSelf);
				if (act != b.isActive) b.SetActivationStatus(act);
				Factory f = chosenStructure as Factory;
				if ( f != null ) {
					// изображения ресурсов!
				}
			}
			break;
		}
	}

	public void AddFactoryToList (Factory f) {
		switch (f.specialization) {
		case FactorySpecialization.Smeltery:
			if ( !hasSmelteries ) {smelteriesList = new List<Factory>(); hasSmelteries = true;}
			smelteriesList.Add(f);
			break;
			default:
			if (!hasUnspecializedFactories) {unspecializedFactories = new List<Factory>(); hasUnspecializedFactories = true;}
			unspecializedFactories.Add(f);
			break;
		}
	}

	public void RemoveFromFactoriesList (Factory f) {
		switch (f.specialization) {
		case FactorySpecialization.Unspecialized:
			if (hasUnspecializedFactories) {
				for ( int i = 0; i < unspecializedFactories.Count; i++) {
					if (unspecializedFactories[i] == f ) { unspecializedFactories.RemoveAt(i); break; }
				}
				if (unspecializedFactories.Count == 0) { unspecializedFactories = null; hasUnspecializedFactories = false; }
			}
			break;
		case FactorySpecialization.Smeltery:
			if ( hasSmelteries ) {
				for ( int i = 0; i < smelteriesList.Count; i++) {
					if (smelteriesList[i] == f ) { smelteriesList.RemoveAt(i); break; }
				}
				if (smelteriesList.Count == 0) { smelteriesList = null; hasSmelteries = false; }
				}
			break;
		}
	}
}
