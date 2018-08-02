using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkersDestination {ForWorksite, ForWorkBuilding}

[System.Serializable]
public sealed class ColonyControllerSerializer{
	public StorageSerializer storageSerializer;
	public float gears_coefficient, labourEfficientcy_coefficient,
	happiness_coefficient, health_coefficient,birthrateCoefficient;

	public float energyStored,energyCrystalsCount;
	public bool haveWorksites;
	public List<WorksiteSerializer> worksites;

	public int freeWorkers, citizenCount,deathCredit;
	public float peopleSurplus = 0, housingTimer = 0,starvationTimer, starvationTime = 600, real_birthrate = 0;
}

public sealed class ColonyController : MonoBehaviour {
	const float FOOD_CONSUMPTION = 1,  HOUSING_TIME = 7;
	const float HOUSE_PROBLEM_HAPPINESS_LIMIT = 0.3f, FOOD_PROBLEM_HAPPINESS_LIMIT = 0.1f, // happines wouldnt raised upper this level if condition is not met
	HEALTHCARE_PROBLEM_HAPPINESS_LIMIT = 0.5f;

	public Storage storage{get;private set;}
	public HeadQuarters hq{get;private set;}
	public float gears_coefficient {get; private set;}
	public float hospitals_coefficient{get;private set;}
	public float labourEfficientcy_coefficient {get;private set;}
	public float happiness_coefficient {get;private set;}
	public float health_coefficient{get;private set;}
	public bool showColonyInfo = false;

	public float energyStored {get;private set;}
	public float energySurplus {get;private set;}
	public float totalEnergyCapacity {get;private set;}
	public float energyCrystalsCount {get;private set;}
	List<Building> powerGrid;
	public List<Dock> docks{get;private set;}
	public List<RollingShop> rollingShops{get;private set;} // прокатный цех
	public List<GraphoniumEnricher> graphoniumEnrichers{get;private set;}
	public List<ChemicalFactory>chemicalFactories{get;private set;}
	public List<Worksite> worksites{get;private set;}
	public byte docksLevel{get; private set;}
	public byte housesLevel{get; private set;}

	public int freeWorkers{get;private set;}
	public int citizenCount {get; private set;}
	public float birthrateCoefficient{get;private set;}
	public int deathCredit{get;private set;}
	float peopleSurplus = 0, housingTimer = 0;
	public int totalLivespace{get;private set;}
	List<House> houses; List<Hospital> hospitals;
	Rect myRect;
	float starvationTimer, starvationTime = 600, real_birthrate = 0;

	void Awake() {
		GameMaster.realMaster.everydayUpdateList.Add(this);
		GameMaster.realMaster.everyYearUpdateList.Add(this);
		gears_coefficient = 1;
		labourEfficientcy_coefficient = 1;
		health_coefficient = 1;
		hospitals_coefficient = 0;
		birthrateCoefficient = GameMaster.START_BIRTHRATE_COEFFICIENT;
		docksLevel = 0;
		energyCrystalsCount = 1000;

		houses = new List<House>();
		powerGrid = new List<Building>();
		docks = new List<Dock>();
		rollingShops = new List<RollingShop>();
		graphoniumEnrichers = new List<GraphoniumEnricher>();
		chemicalFactories = new List<ChemicalFactory>();
		worksites = new List<Worksite>();
	}

	public void CreateStorage() { // call from game master
		if (storage == null) 	storage = gameObject.AddComponent<Storage>();
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;

		// ENERGY CONSUMPTION
		energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
		if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
		else {
			if (energyStored < 0) { // отключение потребителей энергии до выравнивания
                UIController.current.MakeAnnouncement(Localization.announcement_powerFailure);
				energyStored = 0;
				int i = powerGrid.Count - 1;
				while ( i >= 0 && energySurplus < 0) {
					if (powerGrid[i].energySurplus < 0) ElementPowerSwitch(i, false);
					i--;
				}
			}
		}
			
		//   STARVATION PROBLEM
		float foodSupplyHappiness = 1;
		if (starvationTimer > 0) {
			starvationTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (starvationTimer < 0) starvationTimer = starvationTime;
			float pc = starvationTimer / starvationTime;
			foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS_LIMIT * pc;
			if (pc < 0.5f) {
				pc /= 2f;
				KillCitizens((int)(citizenCount * (1 - pc)));
			}
		}
		else {
			float monthFoodReserves = citizenCount * FOOD_CONSUMPTION * GameMaster.DAYS_IN_WEEK * GameMaster.WEEKS_IN_MONTH;
			foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS_LIMIT + ( 1 - FOOD_PROBLEM_HAPPINESS_LIMIT ) * (storage.standartResources[ResourceType.FOOD_ID] / monthFoodReserves);
		}
		//HOUSING PROBLEM
		housingTimer -= Time.deltaTime * GameMaster.gameSpeed;
		if ( housingTimer <= 0 ) {
			if ( totalLivespace < citizenCount ) {
				int tentsCount = (citizenCount - totalLivespace) / 4;
				if (tentsCount > 0) {
					int step = 1,xpos, zpos;
					xpos = hq.basement.pos.x; zpos = hq.basement.pos.z;
					Chunk colonyChunk = hq.basement.myChunk;
					while (step < Chunk.CHUNK_SIZE / 2 && tentsCount > 0) {
						for (int n = 0; n < (step * 2 + 1); n++) {
							SurfaceBlock correctSurface =  colonyChunk.GetSurfaceBlock(xpos + step - n, zpos + step);
							if (correctSurface  == null) {
								correctSurface  = colonyChunk.GetSurfaceBlock(xpos + step - n, zpos - step);
							}
							if (correctSurface  != null) {
								List<PixelPosByte> positions = correctSurface.GetRandomCells(tentsCount);
								if (positions.Count > 0) {
									tentsCount -= positions.Count;
									for (int j = 0 ; j < positions.Count; j++) {
										House tent = Structure.GetNewStructure(Structure.HOUSE_0_ID) as House;
										tent.SetBasement(correctSurface, positions[j]);
									}
								}
							}
						}
						step++;
					}
				}
			}
			housingTimer = HOUSING_TIME;
		}
		float housingHappiness = 1;
		if ( housesLevel == 0) {
			housingHappiness = HOUSE_PROBLEM_HAPPINESS_LIMIT;
		}
		else{
			if (totalLivespace < citizenCount) {
				float demand = citizenCount - totalLivespace;
				housingHappiness = HOUSE_PROBLEM_HAPPINESS_LIMIT + (1 - HOUSE_PROBLEM_HAPPINESS_LIMIT) * demand / ((float)(citizenCount)) ;
			}
		}
		//HEALTHCARE
		if (health_coefficient < 1 && hospitals_coefficient > 0) {
			health_coefficient += hospitals_coefficient * GameMaster.gameSpeed * Time.deltaTime * gears_coefficient * 0.001f;
		}
		float healthcareHappiness = HEALTHCARE_PROBLEM_HAPPINESS_LIMIT + (1 - HEALTHCARE_PROBLEM_HAPPINESS_LIMIT) * hospitals_coefficient;
		healthcareHappiness *= health_coefficient;	
		// HAPPINESS CALCULATION
		happiness_coefficient = 1;
		if (housingHappiness < happiness_coefficient) happiness_coefficient = housingHappiness;
		if (healthcareHappiness < happiness_coefficient ) happiness_coefficient = healthcareHappiness;
		if (foodSupplyHappiness < happiness_coefficient) happiness_coefficient = foodSupplyHappiness;

		//  BIRTHRATE
		if (birthrateCoefficient != 0) {
			if (birthrateCoefficient > 0) {
				real_birthrate = birthrateCoefficient * Hospital.hospital_birthrate_coefficient * health_coefficient * happiness_coefficient * (1 + storage.standartResources[ResourceType.FOOD_ID] / 500f)* GameMaster.gameSpeed * Time.deltaTime;
				if (peopleSurplus > 1) {
					int newborns = (int) peopleSurplus;
					AddCitizens(newborns); 
					peopleSurplus -= newborns;
				}
			}
			else {
				real_birthrate = birthrateCoefficient * (1.1f - health_coefficient) *GameMaster.gameSpeed * Time.deltaTime;
				if (peopleSurplus < - 1) {
					KillCitizens(1); peopleSurplus += 1;
				}
			}
		}
		peopleSurplus += real_birthrate;
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
		if (!GameMaster.realMaster.weNeedNoResources) {
			//   FOOD  CONSUMPTION
			float fc = FOOD_CONSUMPTION * citizenCount;
			fc -= storage.GetResources(ResourceType.Food, fc);
			if (fc > 0) {
				fc -= storage.GetResources(ResourceType.Supplies, fc);
				if (fc > 0) {
					if (starvationTimer <= 0) starvationTimer = starvationTime;
				}
				starvationTimer = 0;
			}
			else starvationTimer = 0;
		}
	}
	void EveryYearUpdate() {
		gears_coefficient -= GameMaster.GEARS_ANNUAL_DEGRADE;
	}

	void ElementPowerSwitch( int index, bool energySupply) {
		if ( !powerGrid[index].isActive ) return;
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

	#region AddingToLists
	public void AddWorksite( Worksite w ) {
		if ( w == null ) return;
		int i = 0;
		while ( i < worksites.Count) {
			if ( worksites[i] == null ) worksites.RemoveAt(i);
			else {
				if ( worksites[i] == w ) return;
				else i++;
			}
		}
		worksites.Add(w);
	}
	public void RemoveWorksite( Worksite w) {
		if ( w == null || worksites.Count == 0) return;
		int i = 0;
		while (i < worksites.Count) {
			if ( worksites[i] == w | worksites[i] == null ) {
				docks.RemoveAt(i);
				continue;
			}
			i++;
		}
	}

	public void AddHousing(House h) {
		if (h== null) return;
		if ( houses.Count > 0) {
			foreach ( House eh in houses) {
				if ( eh == h ) return;
			}
		}
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
		housesLevel = 0;
		if (houses.Count == 0) return;
		int i = 0, normalLivespace = 0;
		List<int> tents = new List<int>();       
		while (i < houses.Count) {
			if (houses[i] == null || !houses[i].gameObject.activeSelf) {houses.RemoveAt(i); continue;}
			if ( houses[i].isActive) {
				totalLivespace += houses[i].housing;
				if (houses[i].level > housesLevel) housesLevel = housesLevel;
				if (houses[i].level == 0) 	tents.Add(i);
				else normalLivespace += houses[i].housing;
			}
			i++;
		}
        if (tents.Count > 0 & normalLivespace > citizenCount) {
			i = 0;
            int tentIndexDelta = 0; // смещение индексов влево из-за удаления
			while ( i < tents.Count & normalLivespace > citizenCount) {
                int realIndex = tents[i] + tentIndexDelta;
                if (normalLivespace - citizenCount >= houses[realIndex].housing) {                    
					normalLivespace -= houses[realIndex].housing;
					houses[realIndex].Annihilate(false);
                    tentIndexDelta--;
				}
				else break;
				i++;
			}
		}
	}

	public void AddHospital(Hospital h) {
		if (h == null) return;
		if (hospitals == null) hospitals = new List<Hospital>();
		if ( hospitals.Count > 0) {
			foreach ( Hospital eh in hospitals) {
				if ( eh == h ) return;
			}
		}
		hospitals.Add(h);
		RecalculateHospitals();
	}
	public void DeleteHospital(Hospital h) {
		int i = 0;
		while (i < hospitals.Count)  {
			if ( hospitals[i] == h) {
				hospitals.RemoveAt(i);
				RecalculateHospitals();
				break;
			}
			else i++;
		}
	}
	public void RecalculateHospitals() {
		hospitals_coefficient = 0;
		if (hospitals.Count == 0  ) return;
		int i = 0, hospitalsCoverage = 0;
		while (i <hospitals.Count) {
			if (hospitals[i] == null || !hospitals[i].gameObject.activeSelf) {hospitals.RemoveAt(i); continue;}
			if ( hospitals[i].isActive ) hospitalsCoverage += hospitals[i].coverage;
			i++;
		}
		hospitals_coefficient = (float)hospitalsCoverage / (float)citizenCount;
	}

	public void AddToPowerGrid(Building b) {
		if (b == null) return;
		int i = 0;
		while ( i < powerGrid.Count ) {
			if (powerGrid[i] == null) {
				powerGrid.RemoveAt(i);
				continue;
			}
			else {
				if (powerGrid[i] == b) return;
				i++;
			}
		}
		powerGrid.Add(b);
		ElementPowerSwitch(powerGrid.Count - 1, true);
	}
	public void DisconnectFromPowerGrid(Building b) {
		if (b == null ) return;
		int i = 0;
		while (i < powerGrid.Count)  {
			if (powerGrid[i] == null) {powerGrid.RemoveAt(i); continue;}
			if ( powerGrid[i] == b) {
				ElementPowerSwitch(i, false);
				powerGrid.RemoveAt(i);
				return;
			}
			else i++;
		}
	}
	public void RecalculatePowerGrid() {
		energySurplus = 0; totalEnergyCapacity = 0;
		if (powerGrid.Count == 0) return;
		int i =0; 
		while ( i < powerGrid.Count ) {
			if (powerGrid[i] == null) {
				powerGrid.RemoveAt(i);
				continue;
			}
			if (powerGrid[i].energySurplus >= 0 ) { //producent
				energySurplus += powerGrid[i].energySurplus;
				totalEnergyCapacity += powerGrid[i].energyCapacity;
			}
			else { // consument
				if (powerGrid[i].isActive) {
					if ( powerGrid[i].energySupplied ) {
						energySurplus += powerGrid[i].energySurplus;
						totalEnergyCapacity += powerGrid[i].energyCapacity;
					}
					else {
						if ( powerGrid[i].energySurplus < 0 && energyStored >= Mathf.Abs(powerGrid[i].energySurplus)) { 
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

	public void AddDock( Dock d ) {
		if ( d == null ) return;
		if ( docks.Count > 0) {
			foreach ( Dock ed in docks) {
				if ( ed == d ) return;
			}
		}
		docks.Add(d);
		if (d.level > docksLevel) docksLevel = d.level;
	}
	public void RemoveDock( Dock d) {
		if ( d == null || docks.Count == 0) return;
		int i = 0;
		while (i < docks.Count) {
			if (docks[i] == d) {
				docks.RemoveAt(i);
				return;
			}
			i++;
		}
	}

	public void AddRollingShop( RollingShop rs ) {
		if ( rs == null ) return;
		int i = 0;
		while ( i < rollingShops.Count ) {
			if (rollingShops[i] == null) {
				rollingShops.RemoveAt(i);
				continue;
			}
			else {
				if (rollingShops[i] == rs) return;
				else i++;
			}
		}
		rollingShops.Add(rs);
	}
	public void RemoveRollingShop( RollingShop rs) {
		if ( rs == null || rollingShops.Count == 0) return;
		int i = 0;
		while (i < rollingShops.Count) {
			if ( rollingShops[i] == null || rollingShops[i] == rs) {
				rollingShops.RemoveAt(i);
				continue;
			}
			else i++;
		}
	}
	public void AddGraphoniumEnricher( GraphoniumEnricher ge ) {
		if ( ge == null ) return;
		int i = 0;
		while (i < graphoniumEnrichers.Count) {
			if (graphoniumEnrichers[i] == null) {
				graphoniumEnrichers.RemoveAt(i);
				continue;
			}
			else {
				if ( graphoniumEnrichers[i] == ge) return;
				else i++;
			}
		}
		graphoniumEnrichers.Add(ge);
	}
	public void RemoveGraphoniumEnricher ( GraphoniumEnricher ge) {
		if ( ge == null || graphoniumEnrichers.Count == 0) return;
		int i = 0;
		while (i < graphoniumEnrichers.Count) {
			if (graphoniumEnrichers[i] == null || graphoniumEnrichers[i] == ge) {
				graphoniumEnrichers.RemoveAt(i);
				continue;
			}
			else i++;
		}
	}
	public void AddChemicalFactory( ChemicalFactory cf ) {
		if ( cf == null ) return;
		int i = 0;
		while ( i < chemicalFactories.Count) {
			if ( chemicalFactories[i] == null) {
				chemicalFactories.RemoveAt(i);
				continue;
			}
			else {
				if (chemicalFactories[i] == cf) return;
				else i++;
			}
		}
		chemicalFactories.Add(cf);
	}
	public void RemoveChemicalFactory( ChemicalFactory cf) {
		if ( cf == null || chemicalFactories.Count == 0) return;
		int i = 0;
		while (i < chemicalFactories.Count) {
			if (chemicalFactories[i] == null || chemicalFactories[i] == cf) {
				chemicalFactories.RemoveAt(i);
				continue;
			}
			else i++;
		}
	}
	#endregion

	public void SetHQ (HeadQuarters new_hq) {
		if (new_hq != null) hq = new_hq;
	}

	public void ImproveGearsCoefficient (float f) {
		if (f > 0) gears_coefficient += f;
	}

	public void AddEnergyCrystals(float v) {
		if (v <=0) return;
		energyCrystalsCount += v;
	}

    /// <summary>
    /// returns the available residue of asked sum
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
	public float GetEnergyCrystals(float v) {
		if (v > energyCrystalsCount) {v = energyCrystalsCount;energyCrystalsCount = 0;}
		else energyCrystalsCount -= v;
		return v;
	}

	#region save-load system
	public ColonyControllerSerializer Save() {
		ColonyControllerSerializer ccs = new ColonyControllerSerializer();
		ccs.storageSerializer = storage.Save();
		ccs.gears_coefficient = gears_coefficient;
		ccs.labourEfficientcy_coefficient = labourEfficientcy_coefficient;
		ccs.happiness_coefficient = happiness_coefficient;
		ccs.health_coefficient = health_coefficient;
		ccs.birthrateCoefficient = birthrateCoefficient;

		ccs.energyStored = energyStored;
		ccs.energyCrystalsCount = energyCrystalsCount;
		if (worksites.Count == 0) ccs.haveWorksites = false;
		else {
			int realCount = 0;
			ccs.worksites = new List<WorksiteSerializer>();
			foreach (Worksite w in worksites) {
				if (w == null) continue;
				WorksiteSerializer wbs = w.Save();
				if (wbs == null) continue;
				ccs.worksites.Add(wbs);
				realCount++;
			}
			if (realCount == 0) ccs.haveWorksites = false; else ccs.haveWorksites = true;
		}
		ccs.freeWorkers = freeWorkers;
		ccs.citizenCount = citizenCount;
		ccs.deathCredit = deathCredit;
		ccs.peopleSurplus = peopleSurplus;
		ccs.housingTimer = housingTimer;
		ccs.starvationTimer = starvationTimer;
		ccs.starvationTime = starvationTime;
		ccs.real_birthrate = real_birthrate;
		return ccs;
	}
	public void Load(ColonyControllerSerializer ccs) {
		if (storage == null) storage = gameObject.AddComponent<Storage>();
		storage.Load(ccs.storageSerializer);
		gears_coefficient = ccs.gears_coefficient;
		labourEfficientcy_coefficient = ccs.labourEfficientcy_coefficient;
		happiness_coefficient = ccs.happiness_coefficient;
		health_coefficient = ccs.health_coefficient;
		birthrateCoefficient = ccs.birthrateCoefficient;

		energyStored = ccs.energyStored;
		energyCrystalsCount = ccs.energyCrystalsCount;
		if (ccs.haveWorksites) {
			foreach (WorksiteSerializer ws in ccs.worksites) {
				Worksite w = null;
				Block b = GameMaster.mainChunk.GetBlock(ws.workObjectPos);
				if (b == null) continue;
				switch (ws.type) {
				case WorksiteType.Abstract : 
					w = b.gameObject.AddComponent<Worksite>();
					break;
				case WorksiteType.BlockBuildingSite:
					w= b.gameObject.AddComponent<BlockBuildingSite>();
					break;
				case WorksiteType.CleanSite:
					w = b.gameObject.AddComponent<CleanSite>();
					break;
				case WorksiteType.DigSite:
					w = b.gameObject.AddComponent<DigSite>();
					break;
				case WorksiteType.GatherSite:
					w = b.gameObject.AddComponent<GatherSite>();
					break;
				case WorksiteType.TunnelBuildingSite:
					w = b.gameObject.AddComponent<TunnelBuildingSite>();
					break;
				}
				w.Load(ws);
			}
		}
		freeWorkers =ccs.freeWorkers;
		citizenCount = ccs.citizenCount;
		deathCredit = ccs.deathCredit;
		peopleSurplus = ccs.peopleSurplus;
		housingTimer = ccs.housingTimer;
		starvationTimer = ccs.starvationTimer;
		starvationTime = ccs.starvationTime;
		real_birthrate = ccs.real_birthrate;
	}
	#endregion

	void OnDestroy() {
		GameMaster.realMaster.everydayUpdateList.Remove(this);
	}
}
