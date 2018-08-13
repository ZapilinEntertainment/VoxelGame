using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpeditionCorpus : WorkBuilding {
	static List<Expedition> currentExpeditions;


    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        UIController.current.ActivateQuestUI();
        UIController.current.ActivateExpeditionCorpusPanel();
        return workbuildingObserver;
    }



	#region save-load system
	public static ExpeditionCorpusStaticSerializer SaveStaticData() {
		ExpeditionCorpusStaticSerializer ess = new ExpeditionCorpusStaticSerializer();
		ess.currentExpeditions = new List<ExpeditionSerializer>();
		if (currentExpeditions != null && currentExpeditions.Count > 0 ) {
			int i = 0;
			while (i< currentExpeditions.Count) {
				if (currentExpeditions[i] == null) {
					currentExpeditions.RemoveAt(i);
					continue;
				}
				else {
					ess.currentExpeditions.Add(currentExpeditions[i].Save());
				}
				i++;
			}
		}
		return ess;
	}
	public static void LoadStaticData( ExpeditionCorpusStaticSerializer ess ) {
		int i = 0; currentExpeditions = new List<Expedition>();
		while ( i< ess.currentExpeditions.Count) {
			currentExpeditions.Add(new Expedition().Load(ess.currentExpeditions[i]));
			i++;
		}
	}
	#endregion
}

[System.Serializable]
public class ExpeditionCorpusStaticSerializer {
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
	public Quest quest{get;private set;}
	public List<Shuttle> shuttles;
	public float progress{get;private set;}
	public QuantumTransmitter transmitter{get;private set;}
	public int ID{get;private set;}

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
		quest = new Quest().Load(this, es.qs);
		shuttles = new List<Shuttle>();
		if (es.shuttles_ID.Count > 0) {
			foreach (int i in es.shuttles_ID) {
				AssignShuttle( Shuttle.GetShuttle(i) );
			}
		}
		progress = es.progress;
		if (es.haveTransmitter) {
			SurfaceBlock transmitterBasis = GameMaster.mainChunk.GetBlock(es.transmitterPosition) as SurfaceBlock;
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
}

