using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UIMode {View, SurfaceBlockPanel, CubeBlockPanel}

public class UI : MonoBehaviour {
	public LineRenderer lineDrawer;
	public static UI current;
	Rect workRect = new Rect(0,0,1,1), newRect = new Rect(0,0,1,1);
	UIMode mode; int argument = 0;
	SurfaceBlock chosenSurfaceBlock;
	CubeBlock chosenCubeBlock;
	Vector2 mousePos;

	void Awake() {
		current = this;
		mode = UIMode.View;
	}
	void Update() {
		mousePos = Input.mousePosition;
		mousePos.y = Screen.height - mousePos.y;
		if (Input.GetMouseButtonDown(0) && !cursorIntersectRect(workRect) ) {
			RaycastHit rh;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rh)) {
				Block b = rh.collider.transform.parent.GetComponent<Block>();
				if (b != null) {
					Vector2 cursorPos = Camera.main.WorldToScreenPoint(b.transform.position);
					workRect.x = cursorPos.x;
					workRect.y =  Screen.height - cursorPos.y;
					newRect.width = 128;newRect.height = 96;

					switch (b.type) {
					case BlockType.Surface:
						mode = UIMode.SurfaceBlockPanel;
						argument = 1;
						chosenSurfaceBlock = b.GetComponent<SurfaceBlock>();
					break;
					case BlockType.Cube:
						mode = UIMode.CubeBlockPanel;
						argument = 1;
						chosenCubeBlock = b.GetComponent<CubeBlock>();
						break;
				}
				}
			}
			else argument = 0;
		}
	}

	bool cursorIntersectRect(Rect rect) {
		if (mousePos.x >= rect.x && mousePos.x <= rect.xMax && mousePos.y >= rect.y && mousePos.y <= rect.yMax) return true;
		else return false;
	}

	void OnGUI() {
		if (workRect != newRect) {
			workRect.width = Mathf.MoveTowards(workRect.width, newRect.width, 10);
			workRect.height = Mathf.MoveTowards(workRect.height, newRect.height, 10);
		}
		switch (mode) {
		case UIMode.View:
			break;
		case UIMode.SurfaceBlockPanel:
			switch (argument) {
			case 1:
				Rect r = workRect; 
				r.height = workRect.height/3f;
				if (GUI.Button(r, Localization.ui_build)) {	argument = 2;} // list of available buildings
				r.y += r.height;

				if (chosenSurfaceBlock.cleanWorks == false) {
					if (GUI.Button(r, Localization.ui_clear)) {
						if (chosenSurfaceBlock.containBuildings)	argument = 3; //ask the confirmation to clear
						else {
							CleanSite cs =  chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
							cs.Set(chosenSurfaceBlock);
						}
					} 
				}
				else {
					if (GUI.Button(r, Localization.ui_cancel_clearing)) {Destroy(chosenSurfaceBlock.GetComponent<CleanSite>());} 
				}
				r.y += r.height;

				if (chosenSurfaceBlock.basement.digWorks == false) {
					if (GUI.Button(r, Localization.ui_clear_and_dig)) {
						if (chosenSurfaceBlock.CanBeCleared()) {
							CleanSite cs =  chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
							cs.Set(chosenSurfaceBlock);
							DigSite ds = chosenSurfaceBlock.basement.gameObject.AddComponent<DigSite>();
							ds.Set(chosenSurfaceBlock.basement, true);
						}
					}
				}
				else {
					DigSite ds = chosenSurfaceBlock.basement.GetComponent<DigSite>();
					if (ds == null) {chosenSurfaceBlock.basement.digWorks = false;}
					else {
						if (ds.dig)	{if (GUI.Button(r, Localization.ui_cancel_digging)) {Destroy(ds);}}
						else {if (GUI.Button(r, Localization.ui_cancel_pouring)) {Destroy(ds);}}
					}
				}
				break;
			case 2: // выбрать доступные для строительства домики
				
				break;
			case 3: // подтверждение на снос
				GUI.Box(workRect, Localization.ui_accept_destruction_on_clearing);
				if (GUI.Button(new Rect(workRect.x, workRect.y + workRect.height * 2f/3f, workRect.width/2, workRect.height/3f), Localization.ui_accept)) {
					CleanSite cs =  chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
					cs.Set(chosenSurfaceBlock);
					argument = 0;
				}
				if (GUI.Button(new Rect(workRect.x + workRect.width/2f, workRect.y + workRect.height * 2f/3f, workRect.width/2, workRect.height/3f), Localization.ui_decline)) argument = 1;
				break;
			}
			break;
		case UIMode.CubeBlockPanel:
			switch (argument) {
			case 1:
				GUI.Box(workRect, Localization.ui_choose_block_action);
				float ydelta = workRect.height/ 5f;
				Rect r = new Rect(workRect.x, workRect.y +workRect.height - ydelta,workRect.width, ydelta);
				//Копать
				if (chosenCubeBlock.digStatus == -1) {
					if (GUI.Button(r, Localization.ui_cancel_digging)) Destroy(chosenCubeBlock.gameObject.GetComponent<DigSite>()); 
				}
				else {
					if (GUI.Button(r, Localization.ui_dig_block)) {
						DigSite ds = chosenCubeBlock.gameObject.AddComponent<DigSite>();
						ds.Set(chosenCubeBlock, true);
					} 
				}
				r.y -= ydelta;
				//Засыпать
				if (chosenCubeBlock.volume < CubeBlock.MAX_VOLUME) {
					if (chosenCubeBlock.digStatus == 1) {
						if (GUI.Button(r, Localization.ui_cancel_pouring)) Destroy(chosenCubeBlock.GetComponent<DigSite>());
					}
					else {
						if (GUI.Button(r, Localization.ui_pourIn )) {
							DigSite ds = chosenCubeBlock.gameObject.AddComponent<DigSite>();
							ds.Set(chosenCubeBlock, false);
						}
					}
				}
				r.y -= ydelta;
				//Выровнять
				int x = chosenCubeBlock.pos.x, y = chosenCubeBlock.pos.y, z = chosenCubeBlock.pos.z;
				if (GameMaster.realMaster.mainChunk.GetBlock(x, y+1, z) == null) {
					
				}
				break;
			}
			break;
		}
	}
}
