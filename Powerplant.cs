using UnityEngine; // mathf
public class Powerplant : WorkBuilding {
	ResourceType fuel;
    private float output, fuelNeeds, fuelBurnTime, tickTimer;
    public float fuelLeft { get; private set; }

	override public void Prepare() {
		PrepareWorkbuilding();
		switch (id) {
		case BIOGENERATOR_2_ID:
                fuel = ResourceType.Food;
                output = 50;
                fuelNeeds = 10;
                fuelLeft = 0;
                fuelBurnTime = 30 * 1f / GameMaster.LABOUR_TICK; // 30 sec
                break;
		case MINERAL_POWERPLANT_2_ID:
                fuel = ResourceType.mineral_F;
                output = 100;
                fuelNeeds = 1;
                fuelLeft = 0;
                fuelBurnTime = 6 * 1f / GameMaster.LABOUR_TICK; // 6 sec
                break;
		case GRPH_REACTOR_4_ID :
                fuel = ResourceType.Graphonium;
                output = 4000;
                fuelNeeds = 1;
                fuelLeft = 0;
                fuelBurnTime = 60 * 1f / GameMaster.LABOUR_TICK; // 60 sec
                break;
		}
	}

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
    }

    public override void LabourUpdate()
    {
        if (tickTimer == 0)
        {
            if (workersCount > 0 & isActive)
            {
                float fuelTaken = GameMaster.colonyController.storage.GetResources(fuel, fuelNeeds * (workersCount / (float)maxWorkers));
                tickTimer = fuelBurnTime * (fuelTaken / fuelNeeds);                
            }
            if (tickTimer == 0 & energySurplus != 0)
            {
                energySurplus = 0;
                GameMaster.colonyController.RecalculatePowerGrid();
            }
        }
        else
        {
            tickTimer--;            
            float rel = workersCount / (float)maxWorkers;
            float newEnergySurplus = 0;
            if (rel != 0)
            {
                if (rel > 0.5f)
                {
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
                else
                {
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
            if (newEnergySurplus != energySurplus)
            {
                energySurplus = newEnergySurplus;
                GameMaster.colonyController.RecalculatePowerGrid();
            }
        }
        fuelLeft = tickTimer / fuelBurnTime;
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
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, tickTimer);
			ss.specificData = stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss,sblock);
		GameMaster.DeserializeByteArray(ss.specificData, ref tickTimer);
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

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }

}
