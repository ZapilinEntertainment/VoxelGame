using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorksiteSign : MonoBehaviour {
	public Worksite worksite;
	string actionLabel = "actionLabel";
	public bool showOnGUI = false;

	public void Set (Worksite ws ) {
		if (ws == null) { Destroy(gameObject); return;}
		worksite = ws;
		if (worksite is GatherSite) actionLabel = Localization.ui_gather_in_progress;
		else {
			if (worksite is CleanSite) actionLabel = Localization.ui_clean_in_progress;
			else {
				if (worksite is DigSite) {
					DigSite ds = ws as DigSite;
					if ( ds.dig ) actionLabel = Localization.ui_dig_in_progress; else actionLabel = Localization.ui_pouring_in_progress;
				}
			}
			}
	}


	void OnGUI () {
		if (showOnGUI == false) return;
		float k = GameMaster.guiPiece;

		Rect r = UI.current.chosenObjectRect_real;
		GUI.Box(r, GUIContent.none);
		r.height /= 5f;
		GUI.Label (r, actionLabel, GameMaster.mainGUISkin.customStyles[3]); 
		r.y += r.height + k/2f; r.height = 1.5f * k; 

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
		GUI.Label(new Rect (r.x + r.width - k, r.y + k/2f, k,k), worksite.maxWorkers.ToString(), GameMaster.mainGUISkin.customStyles[(int)GUIStyles.RightOrientedLabel]);
		GUI.Label(new Rect(r.x + r.width/2f - k/2f, r.y, k, k), wcount.ToString(), GameMaster.mainGUISkin.customStyles[(int)GUIStyles.CenterOrientedLabel]);
		r.y += r.height; r.height  = k;
		if (GUI.Button ( r, Localization.ui_stopWork) ) { Destroy(worksite); }
		r.y += r.height;
		if (GUI.Button ( r, Localization.menu_cancel) ) { showOnGUI = false; UI.current.DropFocus();}
	}
}
