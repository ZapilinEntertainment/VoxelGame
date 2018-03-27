using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkersDestination {ForWorksite, ForWorkBuilding}

public class ColonyController : MonoBehaviour {
	float foodCount = 0;
	const float FOOD_CONSUMPTION = 1;
	public Storage storage; HeadQuarters hq;
	public float gears_coefficient {get; private set;}
	public float labourEfficientcy_coefficient {get;private set;}
	public float happiness_coefficient {get;private set;}
	public float health_coefficient{get;private set;}
	public List<Building> buildings_level_1{get;private set;}
	public Mine[] minePrefs {get;private set;}
	public bool showColonyInfo = false;

	public float energyStored {get;private set;}
	public float energySurplus {get;private set;}
	public float totalEnergyCapacity {get;private set;}
	public float energyCrystalsCount {get;private set;}
	List<Building> powerGrid;

	public int freeWorkers{get;private set;}
	public int citizenCount {get; private set;}
	public int totalLivespace{get;private set;}
	public float birthrate {get; private set;}
	List<House> houses;


	void Awake() {
		GameMaster.colonyController = this;
		if (storage == null) storage = gameObject.AddComponent<Storage>();
		GameMaster.realMaster.everydayUpdateList.Add(this);
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		gears_coefficient = 1;
		labourEfficientcy_coefficient = 1;
		happiness_coefficient = 1;
		health_coefficient = 1;

		buildings_level_1 = new List<Building>();
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/House_level_1")) );
		buildings_level_1[0].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/Smeltery_level_1")) );
		buildings_level_1[1].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/WindGenerator_level_1")) );
		buildings_level_1[2].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/EnergyCapacitor_level_1")) );
		buildings_level_1[3].gameObject.SetActive(false);

		minePrefs = new Mine[6];
		minePrefs[1] = Instantiate(Resources.Load<Mine>("Structures/Buildings/Mine_level_1"));
		minePrefs[1].gameObject.SetActive(false);
		houses = new List<House>();
		powerGrid = new List<Building>();
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;

		energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
		if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
		else {
			if (energyStored < 0) { // отключение потребителей энергии до выравнивания
				energyStored = 0;
				int i = powerGrid.Count - 1;
				float energySurplusCurrent = energySurplus;
				while ( i >= 0 && energySurplus < 0) {
					if (powerGrid[i].energySurplus < 0) {
						powerGrid[i].SetEnergySupply(false);
						energySurplusCurrent -= powerGrid[i].energySurplus;
						if (energySurplusCurrent >= 0) {
							RecalculatePowerGrid();
							break;
						}
					}
					i--;
				}
			}
		}
	}

	public void AddHousing(House h) {
		if (h== null) return;
		houses.Add(h);
		RecalculateHousing();
	}
	public void DeleteHousing(House h) {
		int i = 0;
		while (i < houses.Count)  {
			if ( houses[i] == h) {
				RecalculateHousing();
				break;
			}
			else i++;
		}
	}
	public void RecalculateHousing() {
		totalLivespace = 0; birthrate = 0;
		if (houses.Count == 0) return;
		int i = 0;
		while (i <houses.Count) {
			if (houses[i] == null) {houses.RemoveAt(i); continue;}
			if ( houses[i].isActive) {
				totalLivespace += houses[i].housing;
				birthrate += houses[i].birthrate;
			}
			i++;
		}
	}
	public void AddToPowerGrid(Building b) {
		if (b == null) return;
		powerGrid.Add(b);
		if (b.energySurplus > 0) b.SetEnergySupply(true);
		RecalculatePowerGrid();
	}
	public void DisconnectFromPowerGrid(Building b) {
		if (b == null ) return;
		int i = 0;
		while (i < powerGrid.Count)  {
			if ( powerGrid[i] == b) {
				powerGrid.RemoveAt(i);
				RecalculatePowerGrid();
				break;
			}
			else i++;
		}
	}
	public void RecalculatePowerGrid() {
		energySurplus = 0; totalEnergyCapacity = 0;
		if (powerGrid.Count == 0) return;
		int i =0; 
		while ( i < powerGrid.Count ) {
			if (powerGrid[i] == null) {powerGrid.RemoveAt(i); continue;}
			if (powerGrid[i].isActive) {
				if ( powerGrid[i].energySupplied ) {
					energySurplus += powerGrid[i].energySurplus;
					totalEnergyCapacity += powerGrid[i].energyCapacity;
				}
				else {
					if ( powerGrid[i].energySurplus >= 0) { // just in case
						powerGrid[i].SetEnergySupply(true);
						energySurplus += powerGrid[i].energySurplus;
						totalEnergyCapacity += powerGrid[i].energyCapacity;
					} 
					else {
						if (totalEnergyCapacity >= Mathf.Abs(powerGrid[i].energySurplus)) {
							powerGrid[i].SetEnergySupply(true);
							energySurplus += powerGrid[i].energySurplus;
							totalEnergyCapacity += powerGrid[i].energyCapacity;
						}
					}
				}
			}
			i++;
		}
	}

	public void AddCitizens(int x) {
		citizenCount += x;
		freeWorkers += x;
	}
	public void AddWorkers(int x) {
		freeWorkers += x;
	}
	public void SendWorkers( int x, Component destination,  WorkersDestination destinationCode ) {
		if (freeWorkers == 0) return;
		if (x > freeWorkers) x = freeWorkers;
		switch (destinationCode) {
		case WorkersDestination.ForWorksite:
			Worksite ws = destination as Worksite;
			if (ws == null) return;
			else 	freeWorkers -= ws.AddWorkers(x);
			break;
		case WorkersDestination.ForWorkBuilding:
			WorkBuilding wb = destination as WorkBuilding;
			if (wb == null) return;
			else freeWorkers -= wb.AddWorkers(x);
			break;
		}
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
		}
	}
}
