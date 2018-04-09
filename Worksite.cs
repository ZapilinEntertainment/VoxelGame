using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Worksite : MonoBehaviour {
	public int maxWorkers = 32;
	public int workersCount {get;protected set;}
	protected float workflow, labourTimer, workSpeed;
	public WorksiteSign sign{get; protected set;}

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

	public void HideGUI() {
		sign.showOnGUI = false;
	}

	public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	protected abstract void RecalculateWorkspeed() ;

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign.gameObject);
	}
}
