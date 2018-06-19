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
	public int vesselsRequired{get;private set;}
	public int crewsRequired{get;private set;}
	public Expedition expedition{get;private set;}

	public bool CanBePicked() {
		if (cost > GameMaster.colonyController.energyCrystalsCount) return false;
		else {
			GameMaster.colonyController.GetEnergyCrystals(cost);
			return true;
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
}
