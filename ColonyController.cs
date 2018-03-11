using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonyController : MonoBehaviour {
	public int citizenCount {get; private set;}
	float foodCount = 0;
	const float FOOD_CONSUMPTION = 1;
	public Storage storage; HeadQuarters hq;
	public float gears_coefficient {get; private set;}
	public float workEfficiency_coefficient {get;private set;}
	public float happiness_coefficient {get;private set;}
	public float health_coefficient{get;private set;}
	List<Building> alwaysAvailableBuildings;
	List<Building> workingBuildings;
	public int freeWorkers{get;private set;}

	void Awake() {
		GameMaster.colonyController = this;
		GameMaster.realMaster.everydayUpdateList.Add(this);
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		gears_coefficient = 1;
		workEfficiency_coefficient = 1;
		//happiness_coefficient = 1;
		health_coefficient = 1;
	}

	public void AddWorkers(int x) {
		if (x <= 0) return;
		freeWorkers += x;
	}

	void EverydayUpdate() {
		if (citizenCount > 0) foodCount -= FOOD_CONSUMPTION * citizenCount;
	}
	void EveryYearUpdate() {
		gears_coefficient -= GameMaster.GEARS_ANNUAL_DEGRADE;
	}


	void OnDestroy() {
		GameMaster.realMaster.everydayUpdateList.Remove(this);
	}
}
