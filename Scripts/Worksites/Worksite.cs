using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorksiteType : byte { Abstract, BlockBuildingSite, CleanSite, DigSite, GatherSite, TunnelBuildingSite }

public abstract class Worksite
{
    public static UIWorkbuildingObserver observer; // все правильно, он на две ставки работает
    public static List<Worksite> worksitesList { get; protected set; }

    public Plane workplace { get; protected set; }
    protected byte faceIndex;
    public int workersCount { get; protected set; }
    public float workSpeed { get; protected set; }
    public WorksiteSign sign;
    public string actionLabel { get; protected set; }
    public bool showOnGUI = false, destroyed = false;
    public float gui_ypos = 0;
    public const string WORKSITE_SIGN_COLLIDER_TAG = "WorksiteSign";

    protected bool subscribedToUpdate = false;
    protected float workflow, gearsDamage;
    protected static ColonyController colony;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Worksite w = (Worksite)obj;
        return w.faceIndex == faceIndex && workplace == w.workplace;
    }
    public override int GetHashCode()
    {
        return workplace.GetHashCode() + faceIndex + workersCount;
    }


    static Worksite()
    {
        worksitesList = new List<Worksite>();
    }
    public Worksite(Plane i_workplace, byte i_faceIndex)
    {
        workplace = i_workplace;
        faceIndex = i_faceIndex;
        workplace.SetWorksite(this);
    }

    public static void SetColonyLink(ColonyController cc) { colony = cc; }

    public virtual int GetMaxWorkers() { return 32; }
    public virtual void WorkUpdate()
    {
    }

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
            RecalculateWorkspeed();
            return x;
        }
    }
    public void FreeWorkers() { FreeWorkers(workersCount); }
    public void FreeWorkers(int x)
    {
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
        RecalculateWorkspeed();
    }
    public static void TransferWorkers(Worksite source, Worksite destination)
    {
        int x = source.workersCount;
        source.workersCount = 0;
        source.workSpeed = 0;
        int sum = destination.workersCount + x;
        int maxWorkers = destination.GetMaxWorkers();
        if (sum > maxWorkers)
        {
            GameMaster.realMaster.colonyController.AddWorkers(sum - maxWorkers);
            sum = maxWorkers;
        }
        destination.workersCount = sum;
        destination.RecalculateWorkspeed();
    }
    protected abstract void RecalculateWorkspeed();

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
            colony.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) MonoBehaviour.Destroy(sign.gameObject);
        worksitesList.Remove(this);
    }

    #region save-load system   
    protected virtual List<byte> Save()
    {
        var data = SerializeWorksite();
        data.Insert(0, (byte)WorksiteType.Abstract);
        return data;
    }
    protected List<byte> SerializeWorksite()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(workersCount));
        data.AddRange(System.BitConverter.GetBytes(workflow));
        data.AddRange(System.BitConverter.GetBytes(workSpeed));
        return data;
    }


    virtual protected void Load(System.IO.FileStream fs, ChunkPos pos)
    {
        LoadWorksiteData(fs);
    }
    protected void LoadWorksiteData(System.IO.FileStream fs)
    {
        byte[] data = new byte[12];
        fs.Read(data, 0, 12);
        workersCount = System.BitConverter.ToInt32(data, 0);
        workflow = System.BitConverter.ToSingle(data, 4);
        workSpeed = System.BitConverter.ToSingle(data, 8);
    }

    public static void StaticSave(System.IO.FileStream fs)
    {
        int count = worksitesList.Count;
        List<byte> saveList = new List<byte>();
        if (count > 0)
        {
            count = 0;
            while (count < worksitesList.Count)
            {
                if (worksitesList[count] == null)
                {
                    worksitesList.RemoveAt(count);
                    continue;
                }
                else
                {
                    saveList.AddRange(worksitesList[count].Save());
                    count++;
                }
            }
        }
        fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        if (count > 0)
        {
            var dataArray = saveList.ToArray();
            fs.Write(dataArray, 0, dataArray.Length);
        }
    }
    public static void StaticLoad(System.IO.FileStream fs)
    {
        worksitesList = new List<Worksite>();
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data, 0);
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
                switch ((WorksiteType)data[0])
                {
                    case WorksiteType.CleanSite:
                        {
                            w = CleanSite.Load(fs, chunk);
                            break;
                        }
                    case WorksiteType.BlockBuildingSite:
                        {
                            Plane sblock = chunk.GetBlock(pos) as Plane;
                            if (sblock != null)
                            {
                                w = new BlockBuildingSite();
                                worksitesList.Add(w);
                                w.Load(fs, pos);
                            }
                            break;
                        }

                    case WorksiteType.DigSite:
                        {
                            CubeBlock cb = chunk.GetBlock(pos) as CubeBlock;
                            if (cb != null)
                            {
                                w = new DigSite();
                                worksitesList.Add(w);
                                w.Load(fs, pos);
                            }
                            break;
                        }
                    case WorksiteType.GatherSite:
                        {
                            Plane sblock = chunk.GetBlock(pos) as Plane;
                            if (sblock != null)
                            {
                                w = new GatherSite();
                                worksitesList.Add(w);
                                w.Load(fs, pos);
                            }
                            break;
                        }
                    case WorksiteType.TunnelBuildingSite:
                        {
                            CubeBlock cb = chunk.GetBlock(pos) as CubeBlock;
                            if (cb != null)
                            {
                                w = new TunnelBuildingSite();
                                worksitesList.Add(w);
                                w.Load(fs, pos);
                            }
                            break;
                        }
                    default: w = null; break;
                }
            }
        }
    }
    #endregion
}
