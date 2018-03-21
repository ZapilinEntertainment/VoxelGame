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
	public List<Building> buildings_level_1{get;private set;}
	public Mine[] minePrefs {get;private set;}
	public List<WorkBuilding> workBuildings;
	public List<DigSite> digSites;
	public List<CleanSite> cleanSites;
	public List<GatherSite>gatherSites;
	public int freeWorkers{get;private set;}
	public int housing{get;private set;}
	float workersAppointTimer = 0;
	const float APPOINT_TICK = 2;
	public float digPriority = 0.2f, clearPriority = 0.2f, buildingsPriority = 0.2f,gatherPriority = 0.4f;
	public bool showColonyInfo = false;
	public byte housingLevel = 0;
	public float totalEnergyCapacity = 0;


	void Awake() {
		GameMaster.colonyController = this;
		if (storage == null) storage = gameObject.AddComponent<Storage>();
		GameMaster.realMaster.everydayUpdateList.Add(this);
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		gears_coefficient = 1;
		workEfficiency_coefficient = 1;
		happiness_coefficient = 1;
		health_coefficient = 1;

		workBuildings = new List<WorkBuilding>(); digSites = new List<DigSite>(); cleanSites = new List<CleanSite>(); gatherSites = new List<GatherSite>();
		buildings_level_1 = new List<Building>();
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/House_level_1")) );
		buildings_level_1[0].gameObject.SetActive(false);
		minePrefs = new Mine[6];
		minePrefs[1] = Instantiate(Resources.Load<Mine>("Structures/Buildings/Mine_level_1"));
		minePrefs[1].gameObject.SetActive(false);
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
				int gatherersDemand = 0;
				if (gatherSites.Count > 0) {
					int i = 0;
					while (i < gatherSites.Count) {
						if (gatherSites[i] == null) {gatherSites.RemoveAt(i); continue;}
						else {gatherersDemand += (GatherSite.MAX_WORKERS - gatherSites[i].workersCount); i++;}
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

				float totalDemands = digWorkersDemand * digPriority + clearWorkersDemand * clearPriority + workersDemand * buildingsPriority + gatherersDemand * gatherPriority;
				int workersForDigging = (int)(freeWorkers * digWorkersDemand * digPriority / totalDemands);
				freeWorkers -= workersForDigging;
				int workersForClearing = (int)(freeWorkers * clearWorkersDemand * clearPriority / totalDemands);
				freeWorkers -= workersForClearing;
				int workersForGathering = (int)(freeWorkers * gatherersDemand * gatherPriority / totalDemands);
				freeWorkers -= workersForGathering;

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
				if (workersForGathering > 0) {
					int i = 0;
					while (workersForGathering > 0 && i < gatherSites.Count) {
						int delta = GatherSite.MAX_WORKERS - gatherSites[i].workersCount;
						if (delta <= 0) {i++; continue;}
						if (delta < workersForGathering) {gatherSites[i].AddWorkers(delta); workersForGathering -= delta; i++; }
						else {gatherSites[i].AddWorkers(workersForGathering); workersForGathering= 0; break;}
					}
				}
				freeWorkers += workersForDigging + workersForClearing + workersForGathering;
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
		if (showColonyInfo) {
			Rect r = new Rect(Screen.width - 12 *k, UI.current.upPanelHeight, 4*k, k);
			UI.current.serviceBoxRect = r;
			Rect leftPart = new Rect(r.x, r.y, r.width * 0.75f, k);
			Rect rightPart = new Rect(r.x + r.width/2f, r.y,r.width/2, leftPart.height);
			int population = freeWorkers;
			GUI.Label(leftPart, Localization.info_population); GUI.Label(rightPart, population.ToString() + " / " + housing.ToString(), GameMaster.mainGUISkin.customStyles[0]);
		}
	}
}
