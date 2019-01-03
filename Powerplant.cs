using UnityEngine; // mathf
using System.Collections.Generic;

public class Powerplant : WorkBuilding {
	ResourceType fuel;
    private float output, fuelNeeds, fuelBurnTime;
    public float fuelLeft { get; private set; }
    private int tickTimer = 0;

	override public void Prepare() {
		PrepareWorkbuilding();
		switch (id) {
		case BIOGENERATOR_2_ID:
                fuel = ResourceType.Food;
                output = 400;
                fuelNeeds = 10;
                fuelLeft = 0;
                fuelBurnTime = 1000; // ticks
                break;
		case MINERAL_POWERPLANT_2_ID:
                fuel = ResourceType.mineral_F;
                output = 400;
                fuelNeeds = 1;
                fuelLeft = 0;
                fuelBurnTime = 600; // ticks
                break;
		case GRPH_REACTOR_4_ID :
                fuel = ResourceType.Graphonium;
                output = 4000;
                fuelNeeds = 1;
                fuelLeft = 0;
                fuelBurnTime = 6000 ; //ticks
                break;
            case REACTOR_BLOCK_5_ID:
                fuel = ResourceType.Graphonium;
                output = 16000;
                fuelNeeds = 4;
                fuelLeft = 0;
                fuelBurnTime = 5800; //ticks
                break;
		}
	}

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
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
                float fuelTaken = colony.storage.GetResources(fuel, fuelNeeds * (workersCount / (float)maxWorkers));
                tickTimer = (int)(fuelBurnTime * (fuelTaken / fuelNeeds));                
            }
            if (tickTimer == 0 & energySurplus != 0)
            {
                energySurplus = 0;
                colony.RecalculatePowerGrid();
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
                colony.RecalculatePowerGrid();
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
		colony.AddWorkers(x);
	}	

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

    #region save-load system

    override public List<byte> Save()
    {
        var data = SerializeStructure();
        data.AddRange(SerializeBuilding());
        float saved_wtp = workflowToProcess; // подмена неиспользуемого поля
        workflowToProcess = tickTimer;
        data.AddRange(SerializeWorkBuilding());
        workflowToProcess = saved_wtp;
        return data;
    }

    override public int Load(byte[] data, int startIndex, SurfaceBlock sblock)
    {
        startIndex = base.Load(data, startIndex, sblock);
        tickTimer = (int)workflowToProcess;
        workflowToProcess = 1;
        return startIndex;
    }
    #endregion
}
