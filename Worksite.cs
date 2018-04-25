using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Worksite : MonoBehaviour {
	public int maxWorkers = 32;
	public int workersCount {get;protected set;}
	protected float workflow, labourTimer, workSpeed;
	public WorksiteSign sign{get; protected set;}
	protected string actionLabel;
	public bool showOnGUI = false;
	public float gui_ypos = 0;

	void Awake () {
		labourTimer = 0; workflow = 0;
		workersCount = 0;
	}

	public int AddWorkers ( int x) {
		if (workersCount == maxWorkers) return 0;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
			}
			RecalculateWorkspeed();
			return x;
		}
	}

	public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	protected abstract void RecalculateWorkspeed() ;

	void OnGUI () {
		if (showOnGUI == false) return;
		GUI.depth = UI.GUIDEPTH_WORKSITE_WINDOW;
		Rect r = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		float k = r.height;
		if (actionLabel != null) GUI.Label (r, actionLabel , GameMaster.mainGUISkin.customStyles[3]); 
		r.y += r.height;
		int wcount = (int)GUI.HorizontalSlider(r, workersCount, 0, maxWorkers);
		if (wcount != workersCount) { 
			if (wcount < workersCount) {
				FreeWorkers(workersCount - wcount);
			}
			else {
				GameMaster.colonyController.SendWorkers(wcount - workersCount, this, WorkersDestination.ForWorksite);
			}
		}
		GUI.Label(new Rect (r.x, r.y + k/2f, k,k), "0"); 
		GUI.Label(new Rect (r.xMax - k, r.y + k/2f, k,k), maxWorkers.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
		GUI.Label(new Rect(r.x + r.width/2f - k/2f, r.y, k, k), workersCount.ToString(), PoolMaster.GUIStyle_CenterOrientedLabel);
		r.y += 2 * r.height; r.height  = k;
		if (GUI.Button ( r, Localization.ui_stopWork) ) { Destroy(this); return; }
		r.y += r.height;
		if (GUI.Button ( r, Localization.menu_cancel) ) { showOnGUI = false; UI.current.DropFocus();}
	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign.gameObject);
	}
}
