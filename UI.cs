using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum UIMode {View, SurfaceBlockPanel}

public class UI : MonoBehaviour {
	public LineRenderer lineDrawer;
	public static UI current;
	Rect workRect = new Rect(0,0,1,1), newRect = new Rect(0,0,1,1);
	UIMode mode; int argument = 0;
	SurfaceBlock chosenSurfaceBlock;
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
					if (b.type == BlockType.Surface) {
						Vector2 cursorPos = Camera.main.WorldToScreenPoint(b.transform.position);
						workRect.x = cursorPos.x;
						workRect.y =  Screen.height - cursorPos.y;
						newRect.width = 128;newRect.height = 96;
						mode = UIMode.SurfaceBlockPanel;
						argument = 1;
						chosenSurfaceBlock = b.GetComponent<SurfaceBlock>();
					}
				}
			}
		}
	}

	bool cursorIntersectRect(Rect rect) {
		if (mousePos.x >= rect.x && mousePos.x <= rect.xMax && mousePos.y >= rect.y && mousePos.y <= rect.yMax) return true;
		else return false;
	}

	void OnGUI() {
		GUILayout.Label(Localization.rtype_dirt_descr);
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
					if (GUI.Button(r, Localization.ui_clear)) {argument = 3;} //ask the confirmation to clear
						r.y += r.height;
				}

				if (chosenSurfaceBlock.basement.digWorks == false) {
					if (GUI.Button(r, Localization.ui_dig)) {
						if (chosenSurfaceBlock.CanBeCleared()) {
							CleanSite cs =  chosenSurfaceBlock.gameObject.AddComponent<CleanSite>();
							cs.Set(chosenSurfaceBlock);
							DigSite ds = chosenSurfaceBlock.basement.gameObject.AddComponent<DigSite>();
							ds.Set(chosenSurfaceBlock.basement, true);
						}
					}
				}
				break;
			case 2:
				
				break;
			}
			break;
		}
	}
}
