using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExpeditionCorpus : WorkBuilding {
    public static List<ExpeditionCorpus> expeditionCorpusesList { get; private set; }
    private const int EXPEDITIONS_SLOTS_PER_BUILDING = 4;

    static ExpeditionCorpus() {
        expeditionCorpusesList = new List<ExpeditionCorpus>();
    }
    public static void ResetToDefaults_Static_ExpeditionCorpus()
    {
        expeditionCorpusesList = new List<ExpeditionCorpus>();
    }
    public static int GetExpeditionsSlotsCount()
    {
        int t = QuantumTransmitter.transmittersList.Count;
        int e = expeditionCorpusesList.Count * EXPEDITIONS_SLOTS_PER_BUILDING;
        if (t < e) return t;
        else return e;
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (!expeditionCorpusesList.Contains(this)) expeditionCorpusesList.Add(this);
    }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        UIController.current.ChangeActiveWindow(ActiveWindowMode.ExpeditionPanel);
        return workbuildingObserver;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (expeditionCorpusesList.Contains(this)) expeditionCorpusesList.Remove(this);
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}

[System.Serializable]
public class ExpeditionStaticSerializer {
	public List<ExpeditionSerializer> currentExpeditions;
}
[System.Serializable]
public class ExpeditionSerializer {
	public int ID;
    public QuestSerializer qs;
	public List<int> shuttles_ID;
	public float progress;
	public bool haveTransmitter;
	public ChunkPos transmitterPosition;
}


public class Expedition {
    // переделать в monobehaviour чтобы не было проблем с null
	public Quest quest{get;private set;}
	public List<Shuttle> shuttles;
	public float progress{get;private set;}
	public QuantumTransmitter transmitter{get;private set;}
	public int ID{get;private set;}

    public static List<Expedition> expeditionsList { get; private set; }
    public static int expeditionsFinished, expeditionsSucceed;

    static Expedition ()
    {
        expeditionsList = new List<Expedition>();
    }
    public static void GameReset()
    {
        expeditionsList = new List<Expedition>();
        expeditionsFinished = 0;
        expeditionsSucceed = 0;
    }

    public Expedition()
    {
        shuttles = new List<Shuttle>();
    }

	public void Initialize(Quest q) {
		shuttles = new List<Shuttle>();
	}

	public void SetTransmitter(QuantumTransmitter qt) {
		transmitter = qt;
	}

	public void AssignShuttle( Shuttle s ) {
		if (s == null) return;
		shuttles.Add(s);
		s.AssignTo(this);
	}
	public void DetachShuttle(Shuttle s) {
		if (s == null | shuttles.Count == 0) return;
		int i =0;
		while (i < shuttles.Count) {
			if (shuttles[i] == null )
			{
				shuttles.RemoveAt(i);
				continue;
			}
			else {
				if (shuttles[i] == s) {
					shuttles[i].AssignTo(null);
					shuttles.RemoveAt(i);
					return;
				}
			}
			i++;
		}
	}
    public void DetachShuttle(int index)
    {
        if (shuttles.Count >= index) return;
        else
        {
            shuttles[index].AssignTo(null);
            shuttles.RemoveAt(index);
        }
    }

    #region save-load system
    public ExpeditionSerializer Save() {
		ExpeditionSerializer es = new ExpeditionSerializer();
		es.ID = ID;
		//es.quest_ID = (quest == null ? -1 : quest.ID);
		es.shuttles_ID = new List<int>();
		if (shuttles.Count > 0) {
			int i = 0;
			while (i < shuttles.Count) {
				if (shuttles[i] == null) {
					shuttles.RemoveAt(i);
					continue;
				}
				else {
					es.shuttles_ID.Add(shuttles[i].ID);
					i++;
				}
			}
		}
		es.progress = progress;
		if (transmitter != null) {
			es.transmitterPosition = transmitter.basement.pos;
			es.haveTransmitter = true;
		}
		else es.haveTransmitter = false;
		return es;
	}
	public Expedition Load( ExpeditionSerializer es) {
		ID = es.ID;
		
		shuttles = new List<Shuttle>();
		if (es.shuttles_ID.Count > 0) {
			foreach (int i in es.shuttles_ID) {
				AssignShuttle( Shuttle.GetShuttle(i) );
			}
		}
		progress = es.progress;
		if (es.haveTransmitter) {
			SurfaceBlock transmitterBasis = GameMaster.realMaster.mainChunk.GetBlock(es.transmitterPosition) as SurfaceBlock;
			foreach (Structure s in transmitterBasis.surfaceObjects) {
				if (s is QuantumTransmitter) {
					transmitter = s as QuantumTransmitter;
					transmitter.SetExpedition(this);
					break;
				}
			}
		}
		else transmitter = null;
		return this;
	}
    
    public static ExpeditionStaticSerializer SaveStaticData()
    {
        ExpeditionStaticSerializer ess = new ExpeditionStaticSerializer();
        ess.currentExpeditions = new List<ExpeditionSerializer>();
        if (expeditionsList.Count > 0)
        {
            int i = 0;
            while (i < expeditionsList.Count)
            {
                if (expeditionsList[i] == null)
                {
                    expeditionsList.RemoveAt(i);
                    continue;
                }
                else
                {
                    ess.currentExpeditions.Add(expeditionsList[i].Save());
                }
                i++;
            }
        }
        return ess;
    }
    public static void LoadStaticData(ExpeditionStaticSerializer ess)
    {
        int i = 0; expeditionsList = new List<Expedition>();
        while (i < ess.currentExpeditions.Count)
        {
            expeditionsList.Add(new Expedition().Load(ess.currentExpeditions[i]));
            i++;
        }
    }
    #endregion
}

