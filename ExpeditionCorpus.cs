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
	}

	override public void SetGUIVisible (bool x) {
		if (x != showOnGUI) {
			showOnGUI = x;
			UI.current.touchscreenTemporarilyBlocked = showOnGUI;
		}
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

	void OnGUI() {
		// base on buiding.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
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
				if ( !c.onMission ) freeCrews++;
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
		Rect qw = new Rect(UI.current.leftPanelWidth, UI.current.upPanelBox.height, Screen.width - UI.current.leftPanelWidth - UI.current.rightPanelBox.width, Screen.height - UI.current.upPanelBox.height);
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
			GUI.Label(new Rect(r.x, r.y + p, r.width - 3 * p,  p), Localization.quests_vesselsRequired + ": " + q.vesselsRequired);
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

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
		if (showOnGUI) UI.current.touchscreenTemporarilyBlocked = false;
	}
}

public class Expedition {
	public Quest quest{get;private set;}
	public List<Shuttle> shuttles;
	public float progress{get;private set;}
	public QuantumTransmitter transmitter{get;private set;}

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
			if (shuttles[i] == null | shuttles[i] == s)
			{
				shuttles.RemoveAt(i);
				continue;
			}
			i++;
		}
	}
}
