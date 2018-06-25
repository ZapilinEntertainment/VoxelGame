using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest  {
	public string name{get;private set;} 
	public string description{get;private set;} 
	public Texture poster{get;private set;} 
	public float cost{get;private set;} 
	public bool picked = false;
	public bool useTimerBeforeTaking{get;private set;}
	public bool useTimerAfterTaking{get;private set;}
	public float questLifeTimer {get;private set;}
	public float questRealizationTimer {get;private set;}
	public int shuttlesRequired{get;private set;}
	public int crewsRequired{get;private set;}
	public Expedition expedition{get;private set;}
	public int ID{get;private set;}
	public const int FIRST_COMMUNICATOR_SET_ID = 1;
	public const int TOTAL_QUESTS_COUNT = 2;

	public static List<Quest> questsList;

	static Quest () {
		questsList = new List<Quest>();
	}

	public bool CanBePicked() {
		if (cost > GameMaster.colonyController.energyCrystalsCount) return false;
		else	return true;
	}

	public static Quest GetQuest(int id) {
		if (questsList.Count == 0) return null;
		else {
			int i =0;
			while (i<questsList.Count) {
				if (questsList[i] == null) {
					questsList.RemoveAt(i);
					continue;
				}
				else {
					if (questsList[i].ID == id) return questsList[i];
					i++;
				}
			}
			return null;
		}
	}

	public List<Shuttle> PickSuitableShuttles() {
		List<Shuttle> suitable = new List<Shuttle>();
		foreach (Shuttle s in Shuttle.shuttlesList) {
			if (s == null) continue;
			if (s.status == ShipStatus.InPort & s.crew != null) suitable.Add(s); 
		}
		return suitable;
	}

	public void InitializeExpedition() {
		if (expedition != null) return;
		else expedition = new Expedition();
		expedition.Initialize(this);
	}

	/// <summary>
	/// uses only by expedition.Initialize quest
	/// </summary>
	/// <param name="id">Identifier.</param>
	public static Quest Create(int id) {
		Quest q = new Quest();
		q.poster = PoolMaster.quest_defaultIcon;
		q.name = "default quest";
		q.shuttlesRequired = 1;
		q.crewsRequired = 1;
		questsList.Add(q);
		return q;
	} 
	#region save-load system
	public static QuestStaticSerializer SaveStaticData() {
		QuestStaticSerializer qss = new QuestStaticSerializer();
		qss.quests = new List<QuestSerializer>();
		if (questsList != null) {
		int i =0;
		while (i < questsList.Count) {
			if (questsList[i] == null) {
				questsList.RemoveAt(i);
				continue;
			}
			else {
				qss.quests.Add(questsList[i].Save());
				i++;
			}
		}
		}
		return qss;
	}
	public static void LoadStaticData(QuestStaticSerializer qss) {
		questsList = new List<Quest>();
		if (qss.quests.Count > 0) {
			for (int i = 0; i < qss.quests.Count; i++) {
				questsList.Add(new Quest().Load(null, qss.quests[i]));
			}
		}
	}

	public QuestSerializer Save() {
		QuestSerializer qs = new QuestSerializer();
		qs.name = name;
		qs.description = description;
		qs.cost = cost;
		qs.picked = picked;
		qs.useTimerBeforeTaking = useTimerBeforeTaking;
		qs.useTimerAfterTaking = useTimerAfterTaking;
		qs.shuttlesRequired = shuttlesRequired;
		qs.crewsRequired = crewsRequired;
		return qs;
	}
	public Quest Load(Expedition e, QuestSerializer qs) {
		name = qs.name;
		description = qs.description;
		cost =qs.cost;
		picked = qs.picked;
		useTimerAfterTaking = qs.useTimerAfterTaking;
		useTimerBeforeTaking = qs.useTimerBeforeTaking;
		shuttlesRequired = qs.shuttlesRequired;
		crewsRequired = qs.crewsRequired;
		expedition = e;
		return this;
	}
	#endregion
}

[System.Serializable]
public class QuestSerializer {
	public string name,description;
	public bool picked,useTimerBeforeTaking, useTimerAfterTaking;
	public float questLifeTimer,questRealizationTimer, cost;
	public int shuttlesRequired,crewsRequired, ID;
}
[System.Serializable]
public class QuestStaticSerializer {
	public List<QuestSerializer> quests;
}

