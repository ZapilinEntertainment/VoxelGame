using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorksiteSerializer {
	public WorksiteType type;
	public ChunkPos workObjectPos;
	public int maxWorkers,workersCount;
	public float workflow, workSpeed;
	public byte[] specificData;
}

public enum WorksiteType {Abstract, BlockBuildingSite, CleanSite, DigSite, GatherSite, TunnelBuildingSite}

public abstract class Worksite {
	public int maxWorkers = 32;
	public int workersCount {get;protected set;}
    protected float workflow;
    public float workSpeed { get; protected set; }
	public WorksiteSign sign{get; protected set;}
	public string actionLabel { get; protected set; }
	public bool showOnGUI = false, deleted = false;
	public float gui_ypos = 0;
    protected bool subscribedToUpdate = false;

    public static UIWorkbuildingObserver observer; // все правильно, он на две ставки работает
    protected static List<Worksite> worksitesList;

    static Worksite()
    {
        worksitesList = new List<Worksite>();
    }

    public virtual void WorkUpdate()
    {
    }

	/// <summary>
	/// return excess workers
	/// </summary>
	/// <returns>The workers.</returns>
	/// <param name="x">The x coordinate.</param>    /// 
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

    #region save-load system
    virtual protected WorksiteSerializer Save() {
		WorksiteSerializer ws = GetWorksiteSerializer();
		ws.type = WorksiteType.Abstract;
		return ws;
	}
	virtual protected void Load(WorksiteSerializer ws) {
		LoadWorksiteData(ws);
	}

	protected WorksiteSerializer GetWorksiteSerializer() {
		WorksiteSerializer ws = new WorksiteSerializer();
		ws.maxWorkers = maxWorkers;
		ws.workersCount = workersCount;
		ws.workflow = workflow;
		ws.workSpeed = workSpeed;
		return ws;
	}
	protected void LoadWorksiteData(WorksiteSerializer ws) {
		maxWorkers = ws.maxWorkers;
		workersCount = ws.workersCount;
		workflow = ws.workflow;
		workSpeed = ws.workSpeed;
	}

    public static WorksiteSerializer[] StaticSave()
    {
        if (worksitesList.Count == 0) return new WorksiteSerializer[0];
        var wa = new WorksiteSerializer[worksitesList.Count];
            for (int i =0; i < wa.Length; i++)
            {
                wa[i] = worksitesList[i].Save();
            }
            return wa;
    }
    public static void StaticLoad(WorksiteSerializer[] wdata)
    {
        worksitesList = new List<Worksite>(wdata.Length);
        Worksite w = null;
        for (int i = 0; i < worksitesList.Count; i++)
        {            
            switch (wdata[i].type)
            {
                default: w = null; ;break;
                case WorksiteType.BlockBuildingSite:
                    w = new BlockBuildingSite();
                    break;
                case WorksiteType.CleanSite:
                    w = new CleanSite();
                    break;
                case WorksiteType.DigSite:
                    w = new DigSite();
                    break;
                case WorksiteType.GatherSite:
                    w = new GatherSite();
                    break;
                case WorksiteType.TunnelBuildingSite:
                    w = new TunnelBuildingSite();
                    break;
            }
            if (w != null)
            {
                w.Load(wdata[i]);
                worksitesList[i] = w;
            }
        }
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
        if (deleted) return;
        else deleted = true;
        if (workersCount > 0)
        {
            GameMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Object.Destroy(sign.gameObject);
    }
}
