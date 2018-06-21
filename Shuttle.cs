using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipStatus {InPort, Leaving, Arriving, OnMission}

public class Shuttle : MonoBehaviour {
	const float START_VOLUME = 20, STANDART_COST = 700, STANDART_FUEL_CAPACITY = 100;
	public float volume{get;private set;}
	public float cost{get;private set;}
	public float fuelReserves{get;private set;}
	public float fuelCapacity{get;private set;}
	public int id{get;private set;}
	public float condition{get;private set;} // общее состояние
	[SerializeField]
	Renderer[] renderers;
	public Crew crew{get; private set;}
	public Hangar hangar{get;private set;}
	public ShipStatus status{get;private set;}
	public Expedition assignedToExpedition{get;private set;}

	public static List<Shuttle> shuttlesList;
	public static int lastIndex{get;private set;}

	void Awake() {
		if (shuttlesList == null) shuttlesList = new List<Shuttle>();
	}

	public static void Reset() {
		shuttlesList = new List<Shuttle>();
		lastIndex = 0;
	}

	public void FirstSet(Hangar h) {
		hangar = h;
		transform.position = hangar.transform.position;
		foreach (Renderer r in renderers) {
			r.enabled = false;
		}
		status = ShipStatus.InPort;
		name = Localization.NameShuttle();
		id = lastIndex; lastIndex ++;
		volume = START_VOLUME;
		condition = 1;
		cost =STANDART_COST;
		fuelCapacity = STANDART_FUEL_CAPACITY;
		fuelReserves = GameMaster.colonyController.storage.GetResources(ResourceType.Fuel, fuelCapacity);
		shuttlesList.Add(this);
	}

	public void RepairForResources() {}
	public void RepairForCoins() {}
	public void Refuel() {}

	public static Shuttle GetVesselById(int f_id) {
		Shuttle s = null;
		return s;
	}

	public bool SetCrew(Crew c) {
		if (crew != null & c != null) return false;
		else {crew = c;return true;}
	}

	// used only by Expedition class, use expedition.AssignShuttle
	public void AssignTo(Expedition e) {
		assignedToExpedition = e;
	}

	public static string[] SaveStaticData() {
		List<string> result = new List<string>();
		return result.ToArray();
	}

	public static float GUI_DrawShuttleIcon(Shuttle sh, Rect rr) {
		GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), sh.condition > 0.85f ? PoolMaster.shuttle_good_icon : ( sh.condition  < 0.5f ? PoolMaster.shuttle_bad_icon : PoolMaster.shuttle_normal_icon), ScaleMode.StretchToFill);
		if (sh.crew != null) GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), sh.crew.stamina < 0.5f ? PoolMaster.crew_bad_icon : ( sh.crew.stamina > 0.85f ? PoolMaster.crew_good_icon : PoolMaster.crew_normal_icon), ScaleMode.StretchToFill );
		return 0;
	}
}
