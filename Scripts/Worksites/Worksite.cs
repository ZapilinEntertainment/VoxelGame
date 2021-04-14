using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorksiteType : byte { Abstract, BlockBuildingSite, CleanSite, DigSite, GatherSite }

public abstract class Worksite : ILabourable
{
    public static UIWorkbuildingObserver observer; // все правильно, он на две ставки работает

    public Plane workplace { get; protected set; }
    public int workersCount { get; protected set; }
    public int maxWorkersCount { get; protected set; }
    public WorksiteSign sign;
    public string actionLabel { get; protected set; }
    protected bool showOnGUI = false;
    public bool destroyed { get; protected set; }
    public float gui_ypos = 0;
    public const string WORKSITE_SIGN_COLLIDER_TAG = "WorksiteSign";

    protected bool subscribedToUpdate = false;
    protected float workflow = 0f, gearsDamage, workComplexityCoefficient = 1f;
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
        maxWorkersCount = 32;
        colony.AddWorksiteToList(this);
        destroyed = false;        

        GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
        subscribedToUpdate = true;
    }

    #region ILabourable
    public bool IsWorksite() { return true; }
    virtual public float GetLabourCoefficient()
    {
        return colony.workers_coefficient * workersCount / workComplexityCoefficient ;
    }
    virtual public void LabourUpdate()
    {
        if (workplace == null)
        {
            StopWork(true);
        }
        INLINE_WorkCalculation();
    }  
    virtual public int AddWorkers(int x)
    {
        if (workersCount == maxWorkersCount) return x;
        else
        {
            if (x > maxWorkersCount - workersCount)
            {
                x -= (maxWorkersCount - workersCount);
                workersCount = maxWorkersCount;
            }
            else
            {
                workersCount += x;
                x = 0;
            }
            return x;
        }
    }
    public int FreeWorkers()
    {
        return FreeWorkers(workersCount);
    }
    virtual public int FreeWorkers(int x)
    {
        if (workersCount == 0) return 0;
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
        return x;
    }
    public int GetWorkersCount() { return workersCount; }
    public int GetMaxWorkersCount() { return maxWorkersCount; }
    public bool MaximumWorkersReached() { return workersCount == maxWorkersCount; }
    virtual public bool ShowUIInfo() { return false; }
    virtual public string UI_GetInfo() { return string.Empty; }
    virtual public void DisabledOnGUI()
    {
        showOnGUI = false;
    }
    #endregion

    protected void INLINE_WorkCalculation()
    {
        float work = GetLabourCoefficient();
        workflow += work;
        colony.gears_coefficient -= gearsDamage * work;
        if (workflow >= 1f) LabourResult((int)workflow);
    }

    virtual protected void LabourResult(int iterations)
    {
        workflow -= iterations;
    }
    public static void TransferWorkers(Worksite source, Worksite destination)
    {
        int x = source.workersCount;
        source.workersCount = 0;
        int sum = destination.workersCount + x;
        int maxWorkers = destination.maxWorkersCount;
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
        observer.SetObservingPlace(this);
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
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        if (showOnGUI)
        {
            if (observer.observingPlace == this)
            {
                observer.SelfShutOff();
                UIController.GetCurrent().GetMainCanvasController().ChangeChosenObject(ChosenObjectType.Plane);
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
