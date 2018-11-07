using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorkBuildingSerializer {
	public BuildingSerializer buildingSerializer;
	public float workflow, workSpeed, workflowToProcess;
	public int workersCount;
}

public abstract class WorkBuilding : Building {
	public float workflow {get;protected set;} 
	public float workSpeed {get;protected set;}
	public float workflowToProcess{get; protected set;}
	public int maxWorkers { get; protected set; }
	public int workersCount {get; protected set;} 
    public static UIWorkbuildingObserver workbuildingObserver;

	override public void Prepare() {
		PrepareWorkbuilding();
	}
	protected void PrepareWorkbuilding() {
		PrepareBuilding();
		workersCount = 0;
		workflow = 0;
        switch (id)
        {
            case DOCK_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                 }
                break;
            case FARM_1_ID:
                {
                    workflowToProcess = 16;
                    maxWorkers = 100;
                }
                break;
            case FARM_2_ID:
                {
                    workflowToProcess = 15;
                    maxWorkers = 80;
                }
                break;
            case FARM_3_ID:
                {
                    workflowToProcess = 14;
                    maxWorkers = 70;
                }
                break;
            case FARM_4_ID:
                {
                    workflowToProcess = 11;
                    maxWorkers = 100;
                }
                break;
            case FARM_5_ID:
                {
                    workflowToProcess = 11;
                    maxWorkers = 300;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    workflowToProcess = 50;
                    maxWorkers = 80;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    workflowToProcess = 45;
                    maxWorkers = 80;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    workflowToProcess = 40;
                    maxWorkers = 80;
                }
                break;
            case LUMBERMILL_4_ID:
                {
                    workflowToProcess = 35;
                    maxWorkers = 140;
                }
                break;
            case LUMBERMILL_5_ID:
                {
                    workflowToProcess = 35;
                    maxWorkers = 280;
                }
                break;
            case MINE_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                }
                break;
            case SMELTERY_1_ID:
                {
                    workflowToProcess = 1; // зависит от рецепта
                    maxWorkers = 40;
                }
                break;
            case SMELTERY_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                }
                break;
            case SMELTERY_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 100;
                }
                break;
            case SMELTERY_5_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 400;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    workflowToProcess = 40;
                    maxWorkers = 20;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 50;
                }
                break;
            case MINERAL_POWERPLANT_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                }
                break;
            case ROLLING_SHOP_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                }
                break;
            case FUEL_FACILITY_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 100;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 150;
                }
                break;
            case FOOD_FACTORY_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 140;
                }
                break;
            case FOOD_FACTORY_5_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 250;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                }
                break;
            case XSTATION_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                }
                break;
            case CHEMICAL_FACTORY_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 45;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                }
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

    virtual public void LabourUpdate()
    {
		if ( !isActive | !energySupplied) return;
		if (workersCount > 0) {
			workflow += workSpeed;
			if (workflow >= workflowToProcess) {
				LabourResult();
			}
		}
	}

	protected virtual void LabourResult() {
		workflow = 0;
	}

    /// <summary>
    /// returns excess workers
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
	virtual public int AddWorkers (int x) {
		if (workersCount == maxWorkers) return 0;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
                x = 0;
			}
			RecalculateWorkspeed();
			return x;
		}
	}

	public void FreeWorkers() {
		FreeWorkers(workersCount);
	}
	virtual public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	virtual protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Manufacturing);
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetWorkBuildingSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		WorkBuildingSerializer wbs = new WorkBuildingSerializer();
		GameMaster.DeserializeByteArray<WorkBuildingSerializer>(ss.specificData, ref wbs);
		LoadWorkBuildingData(wbs);
	}
	protected void LoadWorkBuildingData (WorkBuildingSerializer wbs) {
		LoadBuildingData(wbs.buildingSerializer);
		workersCount = wbs.workersCount;
		workflow = wbs.workflow;
		workSpeed = wbs.workSpeed;
		workflowToProcess = wbs.workflowToProcess;
        RecalculateWorkspeed();
	}

	public WorkBuildingSerializer GetWorkBuildingSerializer() {
		WorkBuildingSerializer wbs = new WorkBuildingSerializer();
		wbs.buildingSerializer = GetBuildingSerializer();
		wbs.workflow = workflow;
		wbs.workSpeed = workSpeed;
		wbs.workflowToProcess = workflowToProcess;
		wbs.workersCount = workersCount;
		return wbs;
	}

    #endregion

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        return workbuildingObserver;
    }

    override public void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                return;
            }
        }
        WorkBuilding upgraded = GetStructureByID(upgradedIndex) as WorkBuilding;
        upgraded.Prepare();
        PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
        if (upgraded.innerPosition.size == 16) setPos = new PixelPosByte(0, 0);
        int workers = workersCount;
        workersCount = 0;
        if (upgraded.rotate90only & (modelRotation % 2 != 0))
        {
            upgraded.modelRotation = (byte)(modelRotation - 1);
        }
        else upgraded.modelRotation = modelRotation;
        upgraded.AddWorkers(workers);
        upgraded.SetBasement(basement, setPos);
        if (returnToUI) upgraded.ShowOnGUI();
    }

    protected bool PrepareWorkbuildingForDestruction(bool forced) {		
		if (workersCount != 0) GameMaster.colonyController.AddWorkers(workersCount);
        return PrepareBuildingForDestruction(forced);
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
