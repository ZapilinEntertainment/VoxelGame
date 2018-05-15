using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GeneratorFuel {Biofuel, MineralFuel,Graphonium}
public class Powerplant : WorkBuilding {
	[SerializeField]
	GeneratorFuel fuelType;
	ResourceType fuel;
	[SerializeField]
	 float output = 100, fuelCount = 1, fuelBurnTime = 10, fuelLoadTryingTime = 2;
	float ftimer = 0;

	void Awake() {
		PrepareWorkbuilding();
		switch (fuelType) {
		case GeneratorFuel.Biofuel: fuel = ResourceType.Food;break;
		case GeneratorFuel.MineralFuel: fuel = ResourceType.mineral_F;break;
		case GeneratorFuel.Graphonium : fuel = ResourceType.Graphonium;break;
		}
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !connectedToPowerGrid || !isActive) return;
		if (ftimer > 0) ftimer -= Time.deltaTime;
		if (ftimer <= 0) {
			float takenFuel =  GameMaster.colonyController.storage.GetResources(fuel, fuelCount);
			float newEnergySurplus = 0;
			if (takenFuel == 0) {
				newEnergySurplus = 0;
				ftimer = fuelLoadTryingTime;
			}
			else {
				if (workersCount > 0) {
					if (workersCount >  maxWorkers / 2) {
						if (workersCount > maxWorkers * 5f/6f) {
							newEnergySurplus = output;
						}
						else newEnergySurplus = output/2f;
					}
					else {
						if (workersCount > maxWorkers / 6) newEnergySurplus = output * 0.25f;
						else newEnergySurplus = output * 0.1f;
					}
				}
				else newEnergySurplus = 0;
				ftimer = fuelBurnTime * takenFuel / fuelCount * 2 * ((float)workersCount / (float)maxWorkers);
			}
			if (newEnergySurplus != energySurplus) {
				energySurplus = newEnergySurplus;
				GameMaster.colonyController.RecalculatePowerGrid();
			}
		}
	}

	public int AddWorkers (int x) {
		if (workersCount == maxWorkers) return 0;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
			}
			return x;
		}
	}

	public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
	}
}
