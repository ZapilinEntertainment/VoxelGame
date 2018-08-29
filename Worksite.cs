using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorksiteSerializer {
	public WorksiteType type;
	public ChunkPos workObjectPos;
	public int maxWorkers,workersCount;
	public float workflow, labourTimer, workSpeed;
	public byte[] specificData;
}

public enum WorksiteType {Abstract, BlockBuildingSite, CleanSite, DigSite, GatherSite, TunnelBuildingSite}

public abstract class Worksite : MonoBehaviour {
	public int maxWorkers = 32;
	public int workersCount {get;protected set;}
    protected float workflow, labourTimer;
    public float workSpeed { get; protected set; }
	public WorksiteSign sign{get; protected set;}
	public string actionLabel { get; protected set; }
	public bool showOnGUI = false;
	public float gui_ypos = 0;
    public static UIWorkbuildingObserver observer; // все правильно, он на две ставки работает

	void Awake () {
		labourTimer = 0; workflow = 0;
		workersCount = 0; 
	}

	/// <summary>
	/// return excess workers
	/// </summary>
	/// <returns>The workers.</returns>
	/// <param name="x">The x coordinate.</param>
	public int AddWorkers ( int x) {
		if (workersCount == maxWorkers) return x;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
                x = 0;
			}
			RecalculateWorkspeed();
			return x;
		}
	}

	public void FreeWorkers() {FreeWorkers(workersCount);}
    ///
	public void FreeWorkers(int x) { 
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	protected abstract void RecalculateWorkspeed() ;

    private void OnDestroy()
    {
        if (workersCount > 0 & !GameMaster.applicationStopWorking) GameMaster.colonyController.AddWorkers(workersCount);
    }

    #region save-load system
    virtual public WorksiteSerializer Save() {
		WorksiteSerializer ws = GetWorksiteSerializer();
		ws.type = WorksiteType.Abstract;
		return ws;
	}
	virtual public void Load(WorksiteSerializer ws) {
		LoadWorksiteData(ws);
	}

	protected WorksiteSerializer GetWorksiteSerializer() {
		WorksiteSerializer ws = new WorksiteSerializer();
		ws.maxWorkers = maxWorkers;
		ws.workersCount = workersCount;
		ws.labourTimer = labourTimer;
		ws.workflow = workflow;
		ws.workSpeed = workSpeed;
		return ws;
	}
	protected void LoadWorksiteData(WorksiteSerializer ws) {
		maxWorkers = ws.maxWorkers;
		workersCount = ws.workersCount;
		labourTimer = ws.labourTimer;
		workflow = ws.workflow;
		workSpeed = ws.workSpeed;
	}
	#endregion

    public UIObserver ShowOnGUI()
    {
        if (observer == null) observer = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else observer.gameObject.SetActive(true);
        observer.SetWorksiteObserver(this);
        showOnGUI = true;
        return observer;
    }

    virtual public void StopWork()
    {
        if (workersCount > 0)
        {
            GameMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Destroy(sign.gameObject);
        Destroy(this);
    }
}
