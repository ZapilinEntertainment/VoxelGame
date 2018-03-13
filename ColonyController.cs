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
	public List<Building> workBuildings;
	public List<DigSite> digSites;
	public List<CleanSite> cleanSites;
	public int freeWorkers{get;private set;}
	public int housing{get;private set;}
	float workersAppointTimer = 0;
	const float APPOINT_TICK = 2;
	public float digPriority = 0.4f, clearPriority = 0.4f, buildingsPriority = 0.2f;

	bool showStorage = false;

	void Awake() {
		GameMaster.colonyController = this;
		if (storage == null) storage = new Storage();
		GameMaster.realMaster.everydayUpdateList.Add(this);
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		gears_coefficient = 1;
		workEfficiency_coefficient = 1;
		happiness_coefficient = 1;
		health_coefficient = 1;

		workBuildings = new List<Building>(); digSites = new List<DigSite>(); cleanSites = new List<CleanSite>();
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
		workersAppointTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (workersAppointTimer <= 0) {
				if (freeWorkers > 0) {
					int digWorkersDemand = 0; 
					if (digSites.Count > 0) {
						int i =0;
						while (i < digSites.Count) {
							if (digSites[i] == null) {digSites.RemoveAt(i);continue;}
							else {digWorkersDemand += (DigSite.MAX_WORKERS - digSites[i].workersCount); i++;}
						}
					}
					int clearWorkersDemand = 0; 
					if (cleanSites.Count > 0) {
						int i = 0;
						while (i < cleanSites.Count) {
							if (cleanSites[i] == null) {cleanSites.RemoveAt(i);continue;}
							else {clearWorkersDemand += (CleanSite.MAX_WORKERS - cleanSites[i].workersCount);i++;}
						}
					}
				int workersDemand = 0;
				if (workBuildings.Count > 0) {
					int i = 0;
					while (i < workBuildings.Count) {
						if (workBuildings[i] == null) {workBuildings.RemoveAt(i); continue;}
						else {workersDemand += (workBuildings[i].maxWorkers - workBuildings[i].workersCount); i++;}
					}
				}
				float totalDemands = digWorkersDemand * digPriority + clearWorkersDemand * clearPriority + workersDemand * buildingsPriority;
				int workersForDigging = (int)(freeWorkers * digWorkersDemand * digPriority / totalDemands);
				freeWorkers -= workersForDigging;
				int workersForClearing = (int)(freeWorkers * clearWorkersDemand * clearPriority / totalDemands);
				freeWorkers -= workersForClearing;

				if (workersForDigging > 0) {
					int i = 0;
					while (workersForDigging > 0 && i < digSites.Count) {
						int delta = DigSite.MAX_WORKERS - digSites[i].workersCount;
						if (delta <= 0) {i++; continue;}
						if (delta < workersForDigging) {digSites[i].AddWorkers(delta); workersForDigging -= delta; i++; }
						else {digSites[i].AddWorkers(workersForDigging); workersForDigging = 0; break;}
					}
				}
				if (workersForClearing > 0) {
					int i = 0;
					while (workersForClearing > 0 && i < cleanSites.Count) {
						int delta = CleanSite.MAX_WORKERS - cleanSites[i].workersCount;
						if (delta <= 0) {i++; continue;}
						if (delta < workersForClearing) {cleanSites[i].AddWorkers(delta); workersForClearing -= delta; i++; }
						else {cleanSites[i].AddWorkers(workersForClearing); workersForClearing = 0; break;}
					}
				}
				freeWorkers += workersForDigging + workersForClearing;
				if (freeWorkers > 0) {
					int i = 0;
					while (freeWorkers > 0 && i < workBuildings.Count) {
						int delta = workBuildings[i].maxWorkers - workBuildings[i].workersCount;
						if (delta < freeWorkers) {workBuildings[i].AddWorkers(delta); freeWorkers -= delta; i++; }
						else {workBuildings[i].AddWorkers(freeWorkers); freeWorkers = 0; break;}
					}
				}
				}
			workersAppointTimer = APPOINT_TICK;
		}
	}

	public void AddHousing(int x) {
		if (x > 0) housing += x;
	}
	public void DescreaseHousing(int x) {
		housing -= x; if (housing < 0) housing = 0;
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

	void OnGUI () {
		float k = GameMaster.guiPiece;
		int sw = Screen.width;
		if (GUI.Button(new Rect(sw - 4 * k, 0, 4 * k , k), Localization.ui_storage_name)) { 
			storage.storageRect = new Rect(sw - 4*k, 0, 4*k, k);
			storage.showStorage = !storage.showStorage;
		}
	}
}
