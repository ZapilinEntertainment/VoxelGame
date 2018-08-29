using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GeneratorFuel {Biofuel, MineralFuel,Graphonium}
public class Powerplant : WorkBuilding {
	[SerializeField]
	GeneratorFuel fuelType;
	ResourceType fuel;
	[SerializeField]
	 float output = 100, fuelCount = 1, fuelBurnTime = 24, fuelLoadTryingTime = 2;
    float ftimer, takenFuel = 0;
    public float fuelLeft { get; private set; }

	override public void Prepare() {
		PrepareWorkbuilding();
		switch (fuelType) {
		case GeneratorFuel.Biofuel: fuel = ResourceType.Food;break;
		case GeneratorFuel.MineralFuel: fuel = ResourceType.mineral_F;break;
		case GeneratorFuel.Graphonium : fuel = ResourceType.Graphonium;break;
		}
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 ) return;
		if (ftimer > 0) ftimer -= Time.deltaTime * GameMaster.gameSpeed;
		if (ftimer <= 0) {
            if (workersCount > 0 && isActive) takenFuel = GameMaster.colonyController.storage.GetResources(fuel, fuelCount);
            else takenFuel = 0;
			float newEnergySurplus = 0;
			if (takenFuel == 0) {
				newEnergySurplus = 0;
				ftimer = fuelLoadTryingTime;
			}
			else {
                float rel = workersCount / (float)maxWorkers;
				if (workersCount > 0) {
					if (rel > 0.5f) {
                        if (rel > 0.83f)
                        {
                            rel = (rel - 0.83f) / 0.16f;
                            newEnergySurplus = Mathf.Lerp(0.5f, 1, rel) * output;
                        }
                        else
                        {
                            rel = (rel - 0.5f) / 0.33f;
                            newEnergySurplus = Mathf.Lerp(0, 0.5f, rel) * output;
                        }
                    }
					else {
                        if (rel > 0.16f)
                        {
                            rel = (rel - 0.16f) / 0.34f;
                            newEnergySurplus = Mathf.Lerp(0.25f, 0.5f, rel) * output;
                        }
                        else
                        {
                            rel /= 0.1f;
                            newEnergySurplus = Mathf.Lerp(0, 0.1f, rel) * output;
                        }
                    }
				}
				else newEnergySurplus = 0;
				ftimer = fuelBurnTime * takenFuel / fuelCount * rel;
            }
			if (newEnergySurplus != energySurplus) {
				energySurplus = newEnergySurplus;
				GameMaster.colonyController.RecalculatePowerGrid();
			}
		}
        fuelLeft = takenFuel / fuelCount * ftimer/fuelBurnTime;
    }

	override public int AddWorkers (int x) { // не используется recalculate workspeed
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

	override public void FreeWorkers(int x)
    { // не используется recalculate workspeed
        if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, ftimer);
			ss.specificData = stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss,sblock);
		GameMaster.DeserializeByteArray(ss.specificData, ref ftimer);
	}
	#endregion

    public int GetFuelResourseID() { return fuel.ID; }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        UIController.current.ActivateProgressPanel(ProgressPanelMode.Powerplant);
        showOnGUI = true;
        return workbuildingObserver;
    }

}
