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

	//---------SAVE   SYSTEM----------------
	public virtual string Save() {
		return SaveWorksite();
	}
	protected string SaveWorksite() {
		string s = "";
		s += string.Format("{0:d3}", workersCount);
		s += string.Format("{0:d4}", (int)(workflow * 100));
		s += string.Format("{0:d4}", (int)(labourTimer * 100));
		return s;
	}
	public virtual void Load(string s) {
		workersCount = int.Parse(s.Substring(1,3));
		workflow = int.Parse(s.Substring(4,4)) / 100f;
		labourTimer = int.Parse(s.Substring(8,4)) / 100f;
	}
	// --------------------------------------------------------

	void OnGUI () {
		if (showOnGUI == false) return;
		GUI.depth = UI.GUIDEPTH_WORKSITE_WINDOW;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		float p = rr.height;
		GUI.Label (new Rect(rr.x , rr.y, p, p), "0" );
		GUI.Label ( new Rect (rr.xMax - p, rr.y, p, p), maxWorkers.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
		int wcount = (int)GUI.HorizontalSlider(new Rect(rr.x +  p, rr.y, rr.width - 2 * p, p), workersCount, 0, maxWorkers);
		if (wcount != workersCount) {
			if (wcount > workersCount) GameMaster.colonyController.SendWorkers(wcount - workersCount, this, WorkersDestination.ForWorksite);
			else FreeWorkers(workersCount - wcount);
		}
		rr.y += p;
		p *= 1.5f;
		if ( workersCount > 0 && GUI.Button (new Rect( rr.x, rr.y, p, p ), PoolMaster.minusX10Button_tx)) { FreeWorkers(workersCount);}
		if ( workersCount > 0 && GUI.Button (new Rect( rr.x + p, rr.y, p, p ), PoolMaster.minusButton_tx)) { FreeWorkers(1);}
		GUI.Label ( new Rect (rr.x + 2 *p, rr.y, rr.width - 4 * p, p), workersCount.ToString(), PoolMaster.GUIStyle_CenterOrientedLabel );
		if ( workersCount != maxWorkers && GUI.Button (new Rect( rr.xMax - 2 *p, rr.y, p, p ), PoolMaster.plusButton_tx) ) { GameMaster.colonyController.SendWorkers(1, this, WorkersDestination.ForWorksite);}
		if ( workersCount != maxWorkers &&GUI.Button (new Rect( rr.xMax - p, rr.y, p, p ), PoolMaster.plusX10Button_tx)) { GameMaster.colonyController.SendWorkers(maxWorkers - workersCount,this, WorkersDestination.ForWorksite);}
		rr.y += p;
		GUI.Label ( new Rect (rr.x , rr.y, rr.width , rr.height), actionLabel, PoolMaster.GUIStyle_CenterOrientedLabel );
		rr.y += rr.height;

		if (GUI.Button ( rr, Localization.ui_stopWork) ) { Destroy(this); return; }
		rr.y += rr.height;
		if (GUI.Button ( rr, Localization.menu_cancel) ) { showOnGUI = false; UI.current.DropFocus();}
	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign.gameObject);
	}
}
