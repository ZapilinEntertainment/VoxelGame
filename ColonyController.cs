using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class ColonyControllerSerializer{
	public StorageSerializer storageSerializer;
	public float gears_coefficient, labourEfficientcy_coefficient,
	happiness_coefficient, health_coefficient, birthrateCoefficient, realBirthrate;

	public float energyStored,energyCrystalsCount;
	public WorksiteSerializer[] worksites;

	public int freeWorkers, citizenCount,deathCredit;
	public float peopleSurplus = 0, housingTimer = 0,starvationTimer, starvationTime = 600, real_birthrate = 0;
}

public sealed class ColonyController : MonoBehaviour {
	const float FOOD_CONSUMPTION = 1,  HOUSING_TIME = 7;
	const float HOUSE_PROBLEM_HAPPINESS_LIMIT = 0.3f, FOOD_PROBLEM_HAPPINESS_LIMIT = 0.1f, // happines wouldnt raised upper this level if condition is not met
	HEALTHCARE_PROBLEM_HAPPINESS_LIMIT = 0.5f;

    public string cityName { get; private set; }
	public Storage storage{get;private set;}
	public HeadQuarters hq{get;private set;}
	public float gears_coefficient {get; private set;}
	public float hospitals_coefficient{get;private set;}
	public float labourEfficientcy_coefficient {get;private set;}
	public float happiness_coefficient {get;private set;}
	public float health_coefficient{get;private set;}
    public bool  accumulateEnergy = true;

	public float energyStored {get;private set;}
	public float energySurplus {get;private set;}
	public float totalEnergyCapacity {get;private set;}
	public float energyCrystalsCount {get;private set;}
	public List<Building> powerGrid { get; private set; }
	public List<Dock> docks{get;private set;}
	public byte docksLevel{get; private set;}
	public float housingLevel { get; private set; }

	public int freeWorkers{get;private set;}
	public int citizenCount {get; private set;}
    float birthrateCoefficient;
    public float realBirthrate { get; private set; }
	public int deathCredit{get;private set;}
	float peopleSurplus = 0, housingTimer = 0;
	public int totalLivespace{get;private set;}
	List<House> houses; List<Hospital> hospitals;
	Rect myRect;
    float starvationTimer, starvationTime = 600;
    bool thisIsFirstSet = true;

	void Awake() {
        if (thisIsFirstSet)
        {
            gears_coefficient = 2;
            labourEfficientcy_coefficient = 1;
            health_coefficient = 1;
            hospitals_coefficient = 0;
            birthrateCoefficient = GameMaster.START_BIRTHRATE_COEFFICIENT;
            docksLevel = 0;
            energyCrystalsCount = 1000;          

            cityName = "default city"; // lol
        }
        houses = new List<House>();
        powerGrid = new List<Building>();
        docks = new List<Dock>();
    }
    public void ResetToDefaults()
    {
        hq = null;
        powerGrid.Clear();
        docks.Clear();
        houses.Clear();
        if (hospitals != null) hospitals.Clear();
    }

	public void CreateStorage() { // call from game master
		if (storage == null) 	storage = gameObject.AddComponent<Storage>();
	}

	void Update() {
		if (GameMaster.gameSpeed == 0) return;
        float t = Time.deltaTime * GameMaster.gameSpeed;

        // ENERGY CONSUMPTION
        {
            if (energySurplus > 0)
            {
                if (accumulateEnergy)
                {
                    energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
                    if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
                }
            }
            else
            {
                energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
                if (energyStored < 0)
                { // отключение потребителей энергии до выравнивания
                    UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.PowerFailure));
                    energyStored = 0;
                    int i = powerGrid.Count - 1;
                    while (i >= 0 && energySurplus < 0)
                    {
                        if (powerGrid[i].energySurplus < 0) ElementPowerSwitch(i, false);
                        i--;
                    }
                }
            }
        }
        //   STARVATION PROBLEM
        float foodSupplyHappiness = 1;
        {            
            if (starvationTimer > 0)
            {
                starvationTimer -= t;
                if (starvationTimer < 0) starvationTimer = starvationTime;
                float pc = starvationTimer / starvationTime;
                foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS_LIMIT * pc;
                if (pc < 0.5f)
                {
                    pc /= 2f;
                    KillCitizens((int)(citizenCount * (1 - pc)));
                }
            }
            else
            {
                float monthFoodReserves = citizenCount * FOOD_CONSUMPTION * GameMaster.DAYS_IN_WEEK * GameMaster.WEEKS_IN_MONTH;
                foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS_LIMIT + (1 - FOOD_PROBLEM_HAPPINESS_LIMIT) * (storage.standartResources[ResourceType.FOOD_ID] / monthFoodReserves);
            }
        }
        //HOUSING PROBLEM
        float housingHappiness = 1;
        {
            housingTimer -= t;
            if (housingTimer <= 0)
            {
                if (totalLivespace < citizenCount)
                {
                    int tentsCount = (citizenCount - totalLivespace) / 4;
                    if (tentsCount > 0)
                    {
                        int step = 1, xpos, zpos;
                        xpos = hq.basement.pos.x; zpos = hq.basement.pos.z;
                        Chunk colonyChunk = hq.basement.myChunk;
                        while (step < Chunk.CHUNK_SIZE / 2 && tentsCount > 0)
                        {
                            for (int n = 0; n < (step * 2 + 1); n++)
                            {
                                SurfaceBlock correctSurface = colonyChunk.GetSurfaceBlock(xpos + step - n, zpos + step);
                                if (correctSurface == null)
                                {
                                    correctSurface = colonyChunk.GetSurfaceBlock(xpos + step - n, zpos - step);
                                }
                                if (correctSurface != null)
                                {
                                    List<PixelPosByte> positions = correctSurface.GetRandomCells(tentsCount);
                                    if (positions.Count > 0)
                                    {
                                        tentsCount -= positions.Count;
                                        for (int j = 0; j < positions.Count; j++)
                                        {
                                            House tent = Structure.GetStructureByID(Structure.HOUSE_0_ID) as House;
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
            if (housingLevel == 0)
            {
                housingHappiness = HOUSE_PROBLEM_HAPPINESS_LIMIT;
            }
            else
            {
                if (totalLivespace < citizenCount)
                {
                    float demand = citizenCount - totalLivespace;
                    housingHappiness = HOUSE_PROBLEM_HAPPINESS_LIMIT + (1 - HOUSE_PROBLEM_HAPPINESS_LIMIT) * (1 - demand / ((float)(citizenCount)));
                }
                else
                {
                    housingHappiness = housingLevel / 5f;
                }
            }
        }
		//HEALTHCARE
		if (health_coefficient < 1 && hospitals_coefficient > 0) {
			health_coefficient += hospitals_coefficient * t * gears_coefficient * 0.001f;
		}
		float healthcareHappiness = HEALTHCARE_PROBLEM_HAPPINESS_LIMIT + (1 - HEALTHCARE_PROBLEM_HAPPINESS_LIMIT) * hospitals_coefficient;
		healthcareHappiness *= health_coefficient;	
		// HAPPINESS CALCULATION
		happiness_coefficient = 1;
		if (housingHappiness < happiness_coefficient) happiness_coefficient = housingHappiness;
		if (healthcareHappiness < happiness_coefficient ) happiness_coefficient = healthcareHappiness;
		if (foodSupplyHappiness < happiness_coefficient) happiness_coefficient = foodSupplyHappiness;

        //  BIRTHRATE
        {
            if (birthrateCoefficient != 0)
            {
                if (birthrateCoefficient > 0)
                {
                    realBirthrate = birthrateCoefficient * Hospital.hospital_birthrate_coefficient * health_coefficient * happiness_coefficient * (1 + storage.standartResources[ResourceType.FOOD_ID] / 500f) * t;
                    if (peopleSurplus > 1)
                    {
                        int newborns = (int)peopleSurplus;
                        AddCitizens(newborns);
                        peopleSurplus -= newborns;
                    }
                }
                else
                {
                    realBirthrate = birthrateCoefficient * (1.1f - health_coefficient) * t;
                    if (peopleSurplus < -1)
                    {
                        deathCredit++;
                        peopleSurplus++;
                    }
                }
            }
            peopleSurplus += realBirthrate;
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
	public void SendWorkers( int x, WorkBuilding w ) {
		if (freeWorkers == 0 | w == null) return;
		if (x > freeWorkers) x = freeWorkers;
		freeWorkers = freeWorkers - x + w.AddWorkers(x);
	}
    public void SendWorkers(int x, Worksite w)
    {
        if (freeWorkers == 0 | w == null) return;
        if (x > freeWorkers) x = freeWorkers;
        freeWorkers = freeWorkers - x + w.AddWorkers(x);
    }

    public void EverydayUpdate() {
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
	public void EveryYearUpdate() {
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
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
    }

	#region AddingToLists

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
    public void RecalculateHousing()
    {
        totalLivespace = 0;
        housingLevel = 0;
        if (houses.Count == 0) return;
        int i = 0, normalLivespace = 0;
        List<int> tents = new List<int>();
        float[] housingVolumes = new float[6];
        while (i < houses.Count)
        {
            House h = houses[i];
            if (h.isActive)
            {
                totalLivespace += h.housing;
                if (h.level == 0) tents.Add(i);
                else normalLivespace += h.housing;
                if (h is HeadQuarters == false) housingVolumes[h.level] += h.housing;
                else housingVolumes[5] += h.housing;
            }
            i++;
        }
        if (tents.Count > 0 & normalLivespace > citizenCount)
        {
            i = 0;
            int tentIndexDelta = 0; // смещение индексов влево из-за удаления
            while (i < tents.Count & normalLivespace > citizenCount)
            {
                int realIndex = tents[i] + tentIndexDelta;
                House h = houses[realIndex];
                if (normalLivespace - citizenCount >= h.housing)
                {
                    normalLivespace -= h.housing;
                    housingVolumes[0] -= h.housing;
                    h.Annihilate(false);
                    tentIndexDelta--;
                }
                else break;
                i++;
            }
        }
        if (housingVolumes[0] < 0) housingVolumes[0] = 0;
        float allLivespace = totalLivespace;
        float usingLivespace = 0;
        // принимается, что все расселены от максимального уровня к минимальному
        if (housingVolumes[5] >= allLivespace)
        {
            housingLevel = 5;
        }
        else // можно и рекурсией
        {
            allLivespace -= housingVolumes[5];
            usingLivespace += housingVolumes[5];
            if (housingVolumes[4] >= allLivespace)
            {
                housingVolumes[4] = allLivespace;
                usingLivespace += allLivespace;
                allLivespace = 0;
                housingVolumes[3] = 0;
                housingVolumes[2] = 0;
                housingVolumes[1] = 0;
                housingVolumes[0] = 0;
            }
            else
            {
                allLivespace -= housingVolumes[4];
                usingLivespace += housingVolumes[4];

                if (housingVolumes[3] >= allLivespace)
                {
                    housingVolumes[3] = allLivespace;
                    usingLivespace += allLivespace;
                    allLivespace = 0;
                    housingVolumes[2] = 0;
                    housingVolumes[1] = 0;
                    housingVolumes[0] = 0;
                }
                else
                {
                    allLivespace -= housingVolumes[3];
                    usingLivespace += housingVolumes[3];

                    if (housingVolumes[2] >= allLivespace)
                    {
                        housingVolumes[2] = allLivespace;
                        usingLivespace += allLivespace;
                        allLivespace = 0;
                        housingVolumes[1] = 0;
                        housingVolumes[0] = 0;
                    }
                    else
                    {
                        allLivespace -= housingVolumes[2];
                        usingLivespace += housingVolumes[2];

                        if (housingVolumes[1] >= allLivespace)
                        {
                            housingVolumes[1] = allLivespace;
                            usingLivespace += allLivespace;
                            allLivespace = 0;
                            housingVolumes[0] = 0;
                        }
                        else
                        {
                            allLivespace -= housingVolumes[1];
                            usingLivespace += housingVolumes[1];

                            if (housingVolumes[0] >= allLivespace)
                            {
                                housingVolumes[0] = allLivespace;
                                usingLivespace += allLivespace;
                                allLivespace = 0;
                            }
                            else
                            {
                                allLivespace -= housingVolumes[0];
                                usingLivespace += housingVolumes[0];
                            }
                        }
                    }
                }
            }
        }

        housingLevel = housingVolumes[5] / usingLivespace * 5 + housingVolumes[4] / usingLivespace * 4 + housingVolumes[3] / usingLivespace * 3 + housingVolumes[2] / usingLivespace * 2 + housingVolumes[1] / usingLivespace;
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
        int i = 0;
        float hospitalsCoverage = 0;
		while (i <hospitals.Count) {
			if ( hospitals[i].isActive ) hospitalsCoverage += hospitals[i].coverage;
			i++;
		}
        if (citizenCount != 0)
        {
            hospitals_coefficient = (float)hospitalsCoverage / (float)citizenCount;
        }
        else hospitals_coefficient = 0;
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
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
	}
    public void AddEnergy(float f)
    {
        energyStored += f;
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
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
	#endregion

	public void SetHQ (HeadQuarters new_hq) {
		if (new_hq != null) hq = new_hq;
        QuestUI.current.StartCoroutine(QuestUI.current.WaitForNewQuest(0));
    }

	public void ImproveGearsCoefficient (float f) {
		if (f > 0) gears_coefficient += f;
	}

	public void AddEnergyCrystals(float v) {
		if (v <=0) return;
		energyCrystalsCount += v;
        if (v > 1) UIController.current.MoneyChanging(v);
	}

    /// <summary>
    /// returns the available residue of asked sum
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
	public float GetEnergyCrystals(float v) {
		if (v > energyCrystalsCount) {v = energyCrystalsCount;energyCrystalsCount = 0;}
		else energyCrystalsCount -= v;
        if (v >= 1) UIController.current.MoneyChanging(-v);
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
        ccs.worksites = Worksite.StaticSave();
		ccs.freeWorkers = freeWorkers;
		ccs.citizenCount = citizenCount;
		ccs.deathCredit = deathCredit;
		ccs.peopleSurplus = peopleSurplus;
		ccs.housingTimer = housingTimer;
		ccs.starvationTimer = starvationTimer;
		ccs.starvationTime = starvationTime;
		ccs.real_birthrate = realBirthrate;
        ccs.birthrateCoefficient = birthrateCoefficient;
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
        if (ccs.worksites.Length > 0) Worksite.StaticLoad(ccs.worksites);
		freeWorkers = ccs.freeWorkers;
		citizenCount = ccs.citizenCount;
		deathCredit = ccs.deathCredit;
		peopleSurplus = ccs.peopleSurplus;
		housingTimer = ccs.housingTimer;
		starvationTimer = ccs.starvationTimer;
		starvationTime = ccs.starvationTime;
		realBirthrate = ccs.realBirthrate;
        birthrateCoefficient = ccs.birthrateCoefficient;
	}
	#endregion

}
