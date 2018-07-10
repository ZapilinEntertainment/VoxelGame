using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpeditionCorpus : WorkBuilding {
	static List<Expedition> currentExpeditions;
	static List<Quest> delayedQuests;
	static Quest[] quests;
	static List<Shuttle> suitableShuttles;
	const int MAX_AVAILABLE_QUESTS = 9;
	static bool[] questAccessibilityMap, questsCompletenessMap;
	bool showSuitableShuttlesList = false;
	static bool staticFirstSet = false;

	void Awake() {
		if (!staticFirstSet) { Reset();staticFirstSet = true;}
	}

	public static void Reset() {
		currentExpeditions = new List<Expedition>();
		questAccessibilityMap = new bool[MAX_AVAILABLE_QUESTS];
		questAccessibilityMap[0] = true;
		questAccessibilityMap[1] = true;
		questAccessibilityMap[2] = true;
		questAccessibilityMap[3] = true;
		for (int i = 4; i < MAX_AVAILABLE_QUESTS; i++) {
			questAccessibilityMap[i] = false;
		}
		questsCompletenessMap = new bool[Quest.TOTAL_QUESTS_COUNT];
		for (int i = 0; i < Quest.TOTAL_QUESTS_COUNT; i++) {
			questsCompletenessMap[i] =false;
		}
		quests = new Quest[MAX_AVAILABLE_QUESTS];
		delayedQuests = new List<Quest>();
		suitableShuttles = new List<Shuttle>();
		staticFirstSet = true;
	}

	public static void InitializeQuest(int id) {
		if (questsCompletenessMap[id] == true) return;
		questsCompletenessMap[id] = true;
		Quest q = Quest.Create(id);
		bool questSet = false;
		for (int i = 0; i < MAX_AVAILABLE_QUESTS;i++) {
			if (quests[i] == null & questAccessibilityMap[i] == true) {
				quests[i] = q;
				questSet = true;
				break;
			}
		}
		if ( !questSet ) delayedQuests.Add(q);
	}

	void OLDOnGUI() {
		// base on buiding.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(0,0,0,0);
		int freeVessels = 0;
		if (Shuttle.shuttlesList.Count > 0) {
			foreach (Shuttle s in Shuttle.shuttlesList) {
				if (s == null) return;
				if (s.status == ShipStatus.InPort) freeVessels++;
			}
		}
		GUI.Label(rr, Localization.quests_vesselsAvailable + ": " + freeVessels.ToString() ); rr.y += rr.height;
		int freeCrews = 0;
		if (Crew.crewsList.Count > 0) {
			foreach (Crew c in Crew.crewsList) {
				if (c == null) return;
				if ( c.status == CrewStatus.Idle_noShip ) freeCrews++;
			}
		}
		GUI.Label(rr, Localization.quests_crewsAvailable + ": " + freeCrews.ToString() ); rr.y += rr.height;
		int freeTransmitters = 0;
		if (QuantumTransmitter.transmittersList.Count > 0) {
			foreach (QuantumTransmitter qt in QuantumTransmitter.transmittersList) {
				if (qt == null) return;
				if (qt.tracingExpedition == null ) freeTransmitters++;
			}
		}
		GUI.Label(rr, Localization.quests_transmittersAvailable + ": " + freeTransmitters.ToString() ); rr.y += rr.height;

		//quests window
		Rect qw = new Rect(0,0,0,0);
		GUI.Box(qw, GUIContent.none);
		float h = qw.height / 3f;
		if (qw.width / 3f < h) h = qw.width/3f;
		for (int i = 0; i < MAX_AVAILABLE_QUESTS; i++) {
			Rect qr = new Rect(qw.x + (i %3) * h, qw.y + (i / 3 )* h, h,  h);
			if (quests[i] != null) GUI_QuestWindow(quests[i], qr);
			else GUI.Box(qr, PoolMaster.quest_unacceptableIcon);
		}
	}

	float GUI_QuestWindow(Quest q, Rect r) {
		GUI.skin = GameMaster.mainGUISkin;
		GUI.DrawTexture(r, q.poster);
		float p = r.height / 6f;
		GUI.Label(new Rect(r.x, r.y, r.width - 2 * p, p), q.name);
		if (q.picked) {
			if (q.useTimerBeforeTaking ) GUI.Label(new Rect(r.x,r.y, 2 *p, p), ((int)(q.questLifeTimer / 60f)).ToString() + ':' + string.Format("{0:d2}",((int)(q.questLifeTimer))%60));
			 // выбор кораблей
			if (suitableShuttles.Count == 0) GUI.Label(new Rect(r.x, r.y + p, r.width/2f, p), Localization.quests_no_suitable_vessels);
			else {
				Rect r2 = new Rect(r.x + p, r.y + p, r.width/2f - p, p), r3 = new Rect(r.x + r.width/2f + p, r.y +p, r.width/2f, p);
				if (q.expedition != null) {
					int i = 0;
					while (i < q.expedition.shuttles.Count) {
						if (GUI.Button(new Rect(r2.x, r2.y, p,p), PoolMaster.minusButton_tx) | q.expedition.shuttles[i] == null) {
							q.expedition.shuttles.RemoveAt(i);continue;
						}
						GUI.Label(r2, "\"" + q.expedition.shuttles[i].name + "\""); // как выводить инфо?
						if (q.expedition.shuttles[i].crew != null) {
							GUI.Label(r3,q.expedition.shuttles[i].crew.name);
							r3.y += r3.height;
						}
						r2.y += r2.height;
						i++;
					}
				}
				if (GUI.Button(r2, Localization.ui_showVesselsList)) showSuitableShuttlesList = !showSuitableShuttlesList;
				if (showSuitableShuttlesList) {
					r2.x += p/2f; r2.width -= p/2f;
					int i = 0;
					while ( i< suitableShuttles.Count){
						if (suitableShuttles[i] == null) {
							suitableShuttles.RemoveAt(i);
							continue;
						}
						if (suitableShuttles[i].assignedToExpedition == null) {
							Shuttle.GUI_DrawShuttleIcon(suitableShuttles[i], new Rect(r2.x - p/2f, r2.y + p/4f, p/2f, p/2f));
							if (GUI.Button(r2, suitableShuttles[i].name)) {
								if (q.expedition == null) q.InitializeExpedition();
								q.expedition.AssignShuttle(suitableShuttles[i]);
								r2.y += r2.height;
							}
						}
						i++;
					}
				}
			}
		}
		else { // not picked			
			if (q.useTimerAfterTaking ) GUI.Label(new Rect(r.x,r.y, 2 *p, p), ((int)(q.questRealizationTimer / 60f)).ToString() + ':' + string.Format("{0:d2}",((int)(q.questRealizationTimer))%60));
			GUI.Label(new Rect(r.x, r.y + p, r.width - 3 * p,  p), Localization.quests_vesselsRequired + ": " + q.shuttlesRequired);
			GUI.Label(new Rect(r.x, r.y + 2 *p, r.width - 3 *p, p), Localization.quests_crewsRequired + ": " + q.crewsRequired);
			GUI.Label(new Rect(r.x, r.yMax - 3 *p, r.width, 3 *p), q.description);
			if (GUI.Button(new Rect(r.xMax - 3 *p, r.y + p, 3 *p, 3*p), PoolMaster.plusButton_tx)) {
				if (q.CanBePicked() == true) {
					q.picked = true;
					suitableShuttles = q.PickSuitableShuttles();
				}
			}
			GUI.Label(new Rect(r.xMax - 3 *p,r.y + 3 *p, 3*p,p), Localization.cost + ": "+ Localization.CostInCoins(q.cost));
		}
		return 0;
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
		ess.delayedQuests = new List<int>();
		if (delayedQuests != null && delayedQuests.Count > 0) {
			int i = 0;
			while (i< delayedQuests.Count) {
				if (delayedQuests[i] == null) {
					delayedQuests.RemoveAt(i);
					continue;
				}
				else {
					ess.delayedQuests.Add(delayedQuests[i].ID);
				}
				i++;
			}
		}
		ess.suitableShuttles = new List<int>();
		if (suitableShuttles != null && suitableShuttles.Count > 0) {
			int i = 0;
			while (i< suitableShuttles.Count) {
				if (suitableShuttles[i] == null) {
					suitableShuttles.RemoveAt(i);
					continue;
				}
				else {
					ess.suitableShuttles.Add(suitableShuttles[i].ID);
				}
				i++;
			}
		}
		ess.availableQuests = new List<int>();
		if (quests == null) quests = new Quest[MAX_AVAILABLE_QUESTS];
		for (int i = 0; i < quests.Length; i++) {
			if (quests[i] == null) continue;
			else ess.availableQuests.Add(quests[i].ID);
		}
		ess.questAccessibilityMap = questAccessibilityMap;
		ess.questsCompletenessMap = questsCompletenessMap;
		return ess;
	}
	public static void LoadStaticData( ExpeditionCorpusStaticSerializer ess ) {
		int i =0;
		quests = new Quest[MAX_AVAILABLE_QUESTS];
		while (i < ess.availableQuests.Count) {
			quests[i] = Quest.Create(ess.availableQuests[i]);
			i++;
		}
		questAccessibilityMap = ess.questAccessibilityMap;
		questsCompletenessMap = ess.questsCompletenessMap;
		i = 0; delayedQuests = new List<Quest>();
		while ( i < ess.delayedQuests.Count) {
			delayedQuests.Add(Quest.Create(ess.delayedQuests[i]));
			i++;
		}
		i = 0; suitableShuttles = new List<Shuttle>();
		while ( i < ess.suitableShuttles.Count) {
			suitableShuttles.Add(Shuttle.GetShuttle(ess.suitableShuttles[i]));
			i++;
		}
		i = 0; currentExpeditions = new List<Expedition>();
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
	public List<int> delayedQuests, suitableShuttles, availableQuests;
	public bool[] questAccessibilityMap, questsCompletenessMap;
}
[System.Serializable]
public class ExpeditionSerializer {
	public int ID, quest_ID;
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

	public ExpeditionSerializer Save() {
		ExpeditionSerializer es = new ExpeditionSerializer();
		es.ID = ID;
		es.quest_ID = (quest == null ? -1 : quest.ID);
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
		quest = ( es.quest_ID == -1 ? null : Quest.GetQuest(es.quest_ID));
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

