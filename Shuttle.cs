using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipStatus {InPort, Leaving, Arriving, OnMission}

public class Shuttle : MonoBehaviour {
	const float START_VOLUME = 20;
	public float volume{get;private set;}
	public int id{get;private set;}
	public float condition{get;private set;} // общее состояние
	[SerializeField]
	Renderer[] renderers;
	public Crew crew{get; private set;}
	public Hangar hangar{get;private set;}
	public ShipStatus status{get;private set;}

	public static List<Shuttle> shuttlesList;
	public static int lastIndex{get;private set;}

	static Shuttle() {
		shuttlesList = new List<Shuttle>();
		lastIndex = 1;
	}

	public void FirstSet(Hangar h) {
		hangar = h;
		transform.position = hangar.transform.position;
		foreach (Renderer r in renderers) {
			r.enabled = false;
		}
		status = ShipStatus.InPort;
		id = lastIndex; lastIndex ++;
		volume = START_VOLUME;
		condition = 1;
		shuttlesList.Add(this);
	}

	public static Shuttle GetVesselById(int f_id) {
		Shuttle s = null;
		return s;
	}
}
