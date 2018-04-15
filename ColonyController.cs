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

	public List<Dock> docks{get;private set;}
	public float shipArrivingTimer = 0;
	byte docksLevel = 0;
	const float SHIP_ARRIVING_TIME = 10; // for max difficulty

	public int freeWorkers{get;private set;}
	public int citizenCount {get; private set;}
	public float birthrateCoefficient{get;private set;}
	public int deathCredit{get;private set;}
	float peopleSurplus = 0;
	public int totalLivespace{get;private set;}
	List<House> houses;
	Rect myRect;
	float starvationTimer, starvationTime = 600;

	void Awake() {
		GameMaster.colonyController = this;
		if (storage == null) storage = gameObject.AddComponent<Storage>();
		GameMaster.realMaster.everydayUpdateList.Add(this);
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		gears_coefficient = 1;
		labourEfficientcy_coefficient = 1;
		happiness_coefficient = 1;
		health_coefficient = 1;
		birthrateCoefficient = GameMaster.START_BIRTHRATE_COEFFICIENT;

		buildings_level_1 = new List<Building>();
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/House_level_1")) );
		buildings_level_1[0].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/Smeltery_level_1")) );
		buildings_level_1[1].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/WindGenerator_level_1")) );
		buildings_level_1[2].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/EnergyCapacitor_level_1")) );
		buildings_level_1[3].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/Lumbermill_level_1")) );
		buildings_level_1[4].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/Farm_level_1")) );
		buildings_level_1[5].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/Mine_level_1")) );
		buildings_level_1[6].gameObject.SetActive(false);
		buildings_level_1.Add( Instantiate(Resources.Load<Building>("Structures/Buildings/Dock_level_1")) );
		buildings_level_1[7].gameObject.SetActive(false);

		minePrefs = new Mine[6];
		minePrefs[1] = Instantiate(Resources.Load<Mine>("Structures/Buildings/Mine_level_1"));
		minePrefs[1].gameObject.SetActive(false);
		houses = new List<House>();
		powerGrid = new List<Building>();
		docks = new List<Dock>();
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;

		// ENERGY CONSUMPTION
		energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
		if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
		else {
			if (energyStored < 0) { // отключение потребителей энергии до выравнивания
				GameMaster.realMaster.AddAnnouncement(Localization.announcement_powerFailure);
				energyStored = 0;
				int i = powerGrid.Count - 1;
				while ( i >= 0 && energySurplus < 0) {
					ElementPowerSwitch(i, false);
					i--;
				}
			}
		}
		//   Starvation mechanics
		if (starvationTimer > 0) {
			starvationTimer -= Time.deltaTime * GameMaster.gameSpeed;
			float pc = starvationTimer / starvationTime;
			if (pc < 0.5f) {
				pc /= 2f;
				KillCitizens((int)(citizenCount * (1 - pc)));
			}
		}

		//  BIRTHRATE
		if (birthrateCoefficient != 0) {
			if (birthrateCoefficient > 0) {
				peopleSurplus += birthrateCoefficient * health_coefficient * happiness_coefficient * (1 + storage.standartResources[ResourceType.FOOD_ID] / 500f)* GameMaster.gameSpeed * Time.deltaTime;
				if (peopleSurplus > 1) {
					int newborns = (int) peopleSurplus;
					AddCitizens(newborns); 
					peopleSurplus -= newborns;
				}
			}
			else {
				peopleSurplus += birthrateCoefficient * (1.1f - health_coefficient) *GameMaster.gameSpeed * Time.deltaTime;
				if (peopleSurplus < - 1) {
					KillCitizens(1); peopleSurplus += 1;
				}
			}
		}

		//   SHIPS ARRIVING
		if (shipArrivingTimer > 0) {
			shipArrivingTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (shipArrivingTimer <= 0 && docks.Count != 0) {
				List<int>freeDocks = new List<int>();
				int i = docks.Count - 1;
				while ( i >= 0) {
					if (docks[i] == null) {docks.RemoveAt(i); i--; continue;}
					if ( docks[i].maintainingShip == false ) freeDocks.Add(i);
					i--;
				}
				if (freeDocks.Count > 0) {
					i = (int)(Random.value * (freeDocks.Count - 1));
					bool  sendImmigrants = false, sendGoods = false;
					if (Dock.immigrantsMonthLimit > 0  && Dock.immigrationEnabled ) {
						if (Random.value < 0.3f ||totalLivespace > citizenCount) sendImmigrants = true;
					}
					int transitionsCount = 0;
					for (int x = 0; x < Dock.isForSale.Length; x++) {
						if (Dock.isForSale[x] != null) transitionsCount++;
					}
					if (transitionsCount > 0) sendGoods = true;
					ShipType stype = ShipType.Cargo;
					if (sendImmigrants) {
						if (sendGoods) {
							if (Random.value > 0.55f ) stype = ShipType.Passenger;
						}
						else {
							if (Random.value < 0.05f) stype = ShipType.Private;
							else stype = ShipType.Passenger;
						}
					}
					else {
						if (sendGoods) {
							if (Random.value <= GameMaster.warProximity) stype = ShipType.Military;
							else stype = ShipType.Cargo;
						}
						else {
							if (Random.value > 0.5f) {
								if (Random.value > 0.1f) stype = ShipType.Passenger;
								else stype = ShipType.Private;
							}
							else {
								if (Random.value > GameMaster.warProximity) stype = ShipType.Cargo;
								else stype = ShipType.Military;
							}
						}
					}
					Ship s = PoolMaster.current.GetShip(docks[i].level, stype);
					if (s!= null) {
						s.SetDestination(docks[i]);
					}
					else print ("error:no ship given");
				}
				shipArrivingTimer = SHIP_ARRIVING_TIME * GameMaster.tradeVesselsTrafficCoefficient;
			}
		}
	}


	void ElementPowerSwitch( int index, bool energySupply) {
		if ( !powerGrid[index].isActive || powerGrid[index].energySupplied == energySupply) return;
		powerGrid[index].SetEnergySupply(energySupply);
		if (energySupply) {
			energySurplus += powerGrid[index].energySurplus;
			totalEnergyCapacity += powerGrid[index].energyCapacity;
		}
		else {
			energySurplus -= powerGrid[index].energySurplus;
			totalEnergyCapacity -= powerGrid[index].energyCapacity;
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
				houses.RemoveAt(i);
				RecalculateHousing();
				break;
			}
			else i++;
		}
	}
	public void RecalculateHousing() {
		totalLivespace = 0;
		if (houses.Count == 0) return;
		int i = 0;
		while (i <houses.Count) {
			if (houses[i] == null) {houses.RemoveAt(i); continue;}
			if ( houses[i].isActive) {
				totalLivespace += houses[i].housing;
			}
			i++;
		}
	}
	public void AddToPowerGrid(Building b) {
		if (b == null) return;
		powerGrid.Add(b);
		ElementPowerSwitch(powerGrid.Count - 1, true);
	}
	public void DisconnectFromPowerGrid(Building b) {
		if (b == null ) return;
		int i = 0;
		while (i < powerGrid.Count)  {
			if ( powerGrid[i] == b) {
				ElementPowerSwitch(i, false);
				powerGrid.RemoveAt(i);
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
			if (powerGrid[i].isActive) {
				if ( powerGrid[i].energySupplied ) {
					energySurplus += powerGrid[i].energySurplus;
					totalEnergyCapacity += powerGrid[i].energyCapacity;
				}
				else {
					if ( powerGrid[i].energySurplus >= 0 || (powerGrid[i].energySurplus < 0 && energyStored >= Mathf.Abs(powerGrid[i].energySurplus))) { 
						powerGrid[i].SetEnergySupply(true); 
						energySurplus += powerGrid[i].energySurplus;
						totalEnergyCapacity += powerGrid[i].energyCapacity;
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
	public void KillCitizens(int x) {
		if (freeWorkers < x) {			
			deathCredit += x - freeWorkers;
			freeWorkers = 0;
		}
		else freeWorkers -= x;
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
		//   FOOD  CONSUMPTION
		float fc = FOOD_CONSUMPTION * citizenCount;
		if (fc >= storage.standartResources[ResourceType.FOOD_ID]) {
			storage.standartResources[ResourceType.FOOD_ID] = 0;
			if (starvationTimer <= 0) {
				starvationTimer = starvationTime;
				GameMaster.realMaster.AddAnnouncement(Localization.announcement_starvation);
			}
		}
		else {
			storage.standartResources[ResourceType.FOOD_ID] -= fc;
			if (starvationTimer > 0) starvationTimer = 0;
		}
	}
	void EveryYearUpdate() {
		gears_coefficient -= GameMaster.GEARS_ANNUAL_DEGRADE;
	}

	public void AddDock( Dock d ) {
		if ( d == null ) return;
		docks.Add(d);
		if (d.level > docksLevel) docksLevel = d.level;
		if (docks.Count == 1) { // first dock
			shipArrivingTimer = SHIP_ARRIVING_TIME * GameMaster.tradeVesselsTrafficCoefficient * (1 - (float)docksLevel * 2 / 100f);
		}
		else shipArrivingTimer /= 2f;
	}

	public void AddEnergyCrystals(float v) {
		if (v <=0) return;
		energyCrystalsCount += v;
	}
	public float GetEnergyCrystals(float v) {
		if (v > energyCrystalsCount) v = energyCrystalsCount;
		else energyCrystalsCount -= v;
		return v;
	}

	void OnDestroy() {
		GameMaster.realMaster.everydayUpdateList.Remove(this);
	}

	void OnGUI () {
		float k = GameMaster.guiPiece;
		if (showColonyInfo) {
			if (UI.current.mode != UIMode.View) myRect = new Rect(Screen.width - 16 *k, UI.current.upPanelBox.height, 8*k, k);
			else myRect = new Rect(Screen.width - 8 *k, UI.current.upPanelBox.height, 8*k, k);
			GUI.Box(myRect, GUIContent.none);
			Rect leftPart = new Rect(myRect.x, myRect.y, myRect.width * 0.75f, k);
			Rect rightPart = new Rect(myRect.x + myRect.width/2f, myRect.y,myRect.width/2, leftPart.height);
		}
	}
}
