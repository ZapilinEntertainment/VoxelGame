using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorksiteType : byte { Abstract, BlockBuildingSite, CleanSite, DigSite, GatherSite }

public abstract class Worksite
{
    public static UIWorkbuildingObserver observer; // все правильно, он на две ставки работает

    public Plane workplace { get; protected set; }
    public int workersCount { get; protected set; }
    public WorksiteSign sign;
    public string actionLabel { get; protected set; }
    public bool showOnGUI = false;
    public bool destroyed { get; protected set; }
    public float gui_ypos = 0;
    public const string WORKSITE_SIGN_COLLIDER_TAG = "WorksiteSign";

    protected bool subscribedToUpdate = false;
    protected float workflow, gearsDamage, workSpeed;
    protected ColonyController colony;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Worksite w = (Worksite)obj;
        return workplace == w.workplace & workersCount == w.workersCount & workflow == w.workflow;
    }
    public override int GetHashCode()
    {
        return workplace.GetHashCode() + workersCount;
    }

    public Worksite(Plane i_workplace)
    {
        colony = GameMaster.realMaster.colonyController;
        workplace = i_workplace;
        colony.AddWorksiteToList(this);
        destroyed = false;

        GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
        subscribedToUpdate = true;
    }

    public virtual int GetMaxWorkers() { return 32; }
    public virtual void WorkUpdate()
    {
    }
    public virtual float GetWorkSpeed() { return 0f; }
    /// <summary>
    /// returns excess workers
    /// </summary>
    public int AddWorkers(int x)
    {
        int maxWorkers = GetMaxWorkers();
        if (workersCount == maxWorkers) return x;
        else
        {
            if (x > maxWorkers - workersCount)
            {
                x -= (maxWorkers - workersCount);
                workersCount = maxWorkers;
            }
            else
            {
                workersCount += x;
                x = 0;
            }
            return x;
        }
    }
    public void FreeWorkers() { FreeWorkers(workersCount); }
    public void FreeWorkers(int x)
    {
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
    }
    public static void TransferWorkers(Worksite source, Worksite destination)
    {
        int x = source.workersCount;
        source.workersCount = 0;
        int sum = destination.workersCount + x;
        int maxWorkers = destination.GetMaxWorkers();
        if (sum > maxWorkers)
        {
            GameMaster.realMaster.colonyController.AddWorkers(sum - maxWorkers);
            sum = maxWorkers;
        }
        destination.workersCount = sum;
    }

    public UIObserver ShowOnGUI()
    {
        if (observer == null) observer = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else observer.gameObject.SetActive(true);
        observer.SetObservingWorksite(this);
        showOnGUI = true;
        return observer;
    }

    virtual public void StopWork(bool removeFromListRequest)
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            colony.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Object.Destroy(sign.gameObject);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (showOnGUI)
        {
            if (observer.observingWorksite == this)
            {
                observer.SelfShutOff();
                UIController.current.ChangeChosenObject(ChosenObjectType.Plane);
            }
            showOnGUI = false;
        }
        if (removeFromListRequest) colony?.RemoveWorksite(this);
    }

    #region save-load system   
    public virtual void Save( System.IO.FileStream fs)
    {
        fs.WriteByte((byte)WorksiteType.Abstract);
        SerializeWorksite(fs);
    }
    protected void SerializeWorksite(System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(workersCount), 0,4);
        fs.Write(System.BitConverter.GetBytes(workflow),0,4);
    }


    virtual protected void Load(System.IO.FileStream fs, ChunkPos pos)
    {
        LoadWorksiteData(fs);
    }
    protected void LoadWorksiteData(System.IO.FileStream fs)
    {
        byte[] data = new byte[8];
        fs.Read(data, 0, 8);
        workersCount = System.BitConverter.ToInt32(data, 0);
        workflow = System.BitConverter.ToSingle(data, 4);
    }

    public static void StaticLoad(System.IO.FileStream fs, int count)
    {
        if (count < 0 | count > 1000)
        {
            Debug.Log("worksites loading error - incorrect count");
            GameMaster.LoadingFail();
            return;
        }
        if (count > 0)
        {
            Worksite w = null;
            Chunk chunk = GameMaster.realMaster.mainChunk;
            for (int i = 0; i < count; i++)
            {
                switch ((WorksiteType)fs.ReadByte())
                {
                    case WorksiteType.CleanSite:
                        {
                            w = CleanSite.Load(fs, chunk);
                            break;
                        }
                    case WorksiteType.DigSite:
                        {
                            w = DigSite.Load(fs, chunk);
                            break;
                        }
                    case WorksiteType.BlockBuildingSite:
                        {
                            w = BlockBuildingSite.Load(fs, chunk);
                            break;
                        }
                    
                    case WorksiteType.GatherSite:
                        {
                            w = GatherSite.Load(fs, chunk);
                            break;
                        }
                    default: w = null; break;
                }
            }
        }
    }
    #endregion
}
