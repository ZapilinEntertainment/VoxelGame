using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorksiteSign : MonoBehaviour {
	public Worksite worksite;
	public string actionLabel = "actionLabel";
	public bool showOnGUI = false;

	public void Set (Worksite ws ) {
		if (ws == null) { Destroy(gameObject); return;}
		worksite = ws;
	}


	void OnGUI () {
		if (showOnGUI == false) return;
		float k = GameMaster.guiPiece;
		GUI.depth = UI.GUIDEPTH_WORKSITE_WINDOW;
		Rect r = UI.current.rightPanelBox; r.height = k; r.y += r.height;
		GUI.Label (r, actionLabel , GameMaster.mainGUISkin.customStyles[3]); 
		r.y += r.height;

		int wcount = (int)GUI.HorizontalSlider(new Rect(r.x + k, r.y, r.width - 2 *k, 1.5f * k), worksite.workersCount, 0, worksite.maxWorkers);
		if (wcount != worksite.workersCount) { 
			if (wcount < worksite.workersCount) {
				worksite.FreeWorkers(worksite.workersCount - wcount);
		}
		else {
				GameMaster.colonyController.SendWorkers(wcount - worksite.workersCount, worksite, WorkersDestination.ForWorksite);
		}
	}
		GUI.Label(new Rect (r.x, r.y + k/2f, k,k), "0"); 
		GUI.Label(new Rect (r.x + r.width - k, r.y + k/2f, k,k), worksite.maxWorkers.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
		GUI.Label(new Rect(r.x + r.width/2f - k/2f, r.y, k, k), wcount.ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
		r.y += 2 * r.height; r.height  = k;
		if (GUI.Button ( r, Localization.ui_stopWork) ) { Destroy(worksite); }
		r.y += r.height;
		if (GUI.Button ( r, Localization.menu_cancel) ) { showOnGUI = false; UI.current.DropFocus();}
	}
}
