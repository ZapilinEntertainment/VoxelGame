using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorksiteSerializer {
	public WorksiteType type;
	public ChunkPos workObjectPos;
	public int workersCount;
	public float workflow, workSpeed;
	public byte[] specificData;
}

public enum WorksiteType {Abstract, BlockBuildingSite, CleanSite, DigSite, GatherSite, TunnelBuildingSite}

public abstract class Worksite : MonoBehaviour {
	public int workersCount {get;protected set;}
    protected float workflow;
    public float workSpeed { get; protected set; }
    public WorksiteSign sign;
	public string actionLabel { get; protected set; }
	public bool showOnGUI = false, destroyed = false;
	public float gui_ypos = 0;
    protected bool subscribedToUpdate = false;
    public const string WORKSITE_SIGN_COLLIDER_TAG = "WorksiteSign";
    

    public static UIWorkbuildingObserver observer; // все правильно, он на две ставки работает
    public static List<Worksite> worksitesList { get; protected set; }

    static Worksite()
    {
        worksitesList = new List<Worksite>();
    }
    public virtual int GetMaxWorkers() { return 32; }
    public virtual void WorkUpdate()
    {
    }

	/// <summary>
	/// return excess workers
	/// </summary>
	/// <returns>The workers.</returns>
	/// <param name="x">The x coordinate.</param>    /// 
	public int AddWorkers ( int x) {
        int maxWorkers = GetMaxWorkers();
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
	public void FreeWorkers(int x) { 
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.realMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}

    public static void TransferWorkers(Worksite source, Worksite destination)
    {
        int x = source.workersCount;
        source.workersCount = 0;
        source.workSpeed = 0;
        int sum = destination.workersCount + x;
        int maxWorkers = destination.GetMaxWorkers();
        if (sum > maxWorkers) {
            GameMaster.realMaster.colonyController.AddWorkers(sum - maxWorkers);
            sum = maxWorkers;
        }
        destination.workersCount = sum; 
        destination.RecalculateWorkspeed();
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
		ws.workersCount = workersCount;
		ws.workflow = workflow;
		ws.workSpeed = workSpeed;
		return ws;
	}
	protected void LoadWorksiteData(WorksiteSerializer ws) {
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
        worksitesList = new List<Worksite>();
        Worksite w = null;
        Chunk chunk = GameMaster.realMaster.mainChunk;
        for (int i = 0; i < wdata.Length; i++)
        {
            WorksiteSerializer ws = wdata[i];
            ChunkPos cpos = ws.workObjectPos;
            switch (ws.type)
            {
                default: w = null; break;
                case WorksiteType.BlockBuildingSite:
                    {
                        SurfaceBlock sblock = chunk.GetBlock(cpos) as SurfaceBlock;
                        if (sblock != null)
                        {
                            w = sblock.gameObject.AddComponent<BlockBuildingSite>();
                            worksitesList.Add(w);
                            w.Load(ws);
                        }
                        else continue;
                        break;
                    }
                case WorksiteType.CleanSite:
                    {
                        SurfaceBlock sblock = chunk.GetBlock(cpos) as SurfaceBlock;
                        if (sblock != null)
                        {
                            w = sblock.gameObject.AddComponent<CleanSite>();
                            worksitesList.Add(w);
                            w.Load(ws);
                        }
                        else continue;
                        break;
                    }
                case WorksiteType.DigSite:
                    {
                        CubeBlock cb = chunk.GetBlock(cpos) as CubeBlock;
                        if (cb != null)
                        {
                            w = cb.gameObject.AddComponent<DigSite>();
                            worksitesList.Add(w);
                            w.Load(ws);
                        }
                        else continue;
                        break;
                    }
                case WorksiteType.GatherSite:
                    {
                        SurfaceBlock sblock = chunk.GetBlock(cpos) as SurfaceBlock;
                        if (sblock != null)
                        {
                            w = sblock.gameObject.AddComponent<GatherSite>();
                            worksitesList.Add(w);
                            w.Load(ws);
                        }
                        else continue;
                        break;
                    }
                case WorksiteType.TunnelBuildingSite:
                    {
                        CubeBlock cb = chunk.GetBlock(cpos) as CubeBlock;
                        if (cb != null)
                        {
                            w = cb.gameObject.AddComponent<TunnelBuildingSite>();
                            worksitesList.Add(w);
                            w.Load(ws);
                        }
                        else continue;
                        break;
                    }
            }
        }
    }
	#endregion

    public UIObserver ShowOnGUI()
    {
        if (observer == null) observer = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else observer.gameObject.SetActive(true);
        observer.SetObservingWorksite(this);
        showOnGUI = true;
        return observer;
    }

    virtual public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            GameMaster.realMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Destroy(sign.gameObject);
        Destroy(this);
    }
}
