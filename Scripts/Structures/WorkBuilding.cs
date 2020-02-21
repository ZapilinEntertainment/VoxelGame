using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkBuilding : Building
{
    public static UIWorkbuildingObserver workbuildingObserver;

    public float workflow { get; protected set; }
    public float workSpeed { get; protected set; }
    public float workflowToProcess { get; protected set; }
    public int maxWorkers { get; protected set; }
    public int workersCount { get; protected set; }
    protected float gearsDamage = GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT;
    protected ColonyController colony;

    public const int WORKBUILDING_SERIALIZER_LENGTH = 16;

    override public void Prepare()
    {
        PrepareWorkbuilding();
    }
    protected void PrepareWorkbuilding()
    {
        PrepareBuilding();
        workersCount = 0;
        workflow = 0;
        switch (ID)
        {
            case DOCK_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                }
                break;
            case DOCK_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                }
                break;
            case DOCK_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 120;
                }
                break;
            case FARM_1_ID:
            case FARM_2_ID:
            case FARM_3_ID:
                {
                    workflowToProcess = 1200f;
                    maxWorkers = 100;
                }
                break;
            case FARM_4_ID:
                {
                    workflowToProcess = 50;
                    maxWorkers = 100;
                }
                break;
            case FARM_BLOCK_ID:
                {
                    workflowToProcess = 25;
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
                    workflowToProcess = 75;
                    maxWorkers = 140;
                }
                break;
            case LUMBERMILL_5_ID:
                {
                    workflowToProcess = 70;
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
            case WORKSHOP_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                }
                break;
            case FUEL_FACILITY_ID:
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
            case REACTOR_BLOCK_5_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 120;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 150;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 140;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
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
            case SHUTTLE_HANGAR_4_ID:
                {
                    workflowToProcess = Hangar.BUILD_SHUTTLE_WORKFLOW;
                    maxWorkers = 45;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    workflowToProcess = RecruitingCenter.FIND_WORKFLOW;
                    maxWorkers = 40;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                }
                break;
            case OBSERVATORY_ID:
                {
                    workflowToProcess = Observatory.SEARCH_WORKFLOW;
                    maxWorkers = 50;
                }
                break;
            case PSYCHOKINECTIC_GEN_ID:
            case SCIENCE_LAB_ID:
                {
                    maxWorkers = 40;
                    break;
                }
        }
        colony = GameMaster.realMaster.colonyController;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        //copy to expedition corpus.cs
        //copy to expedition observatory.cs
        //copy to science lab .cs
        // changed in PsychokineticGenerator
        //copy to FarmBlock
    }

    protected void SetWorkbuildingData(Plane sb, PixelPosByte pos)
    {
        SetBuildingData(sb, pos);
        // copy to PsychokineticGenerator.SetBasement()
    }

    virtual public void LabourUpdate()
    {
        if (!isActive | !isEnergySupplied) return;
        if (workersCount > 0)
        {
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
    }

    protected virtual void LabourResult()
    {
        workflow = 0;
    }

    /// <summary>
    /// returns excess workers
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
	virtual public int AddWorkers(int x)
    {
        if (workersCount == maxWorkers) return 0;
        else
        {
            if (x > maxWorkers - workersCount)
            {
                x -= (maxWorkers - workersCount);
                workersCount = maxWorkers;
            }
            else
            {
                workersCount += x;
                x = 0;
            }
            RecalculateWorkspeed();
            return x;
        }
    }

    public void FreeWorkers()
    {
        FreeWorkers(workersCount);
    }
    virtual public void FreeWorkers(int x)
    {
        if (workersCount == 0) return;
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
        RecalculateWorkspeed();
    }
    virtual public void RecalculateWorkspeed()
    {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.FACTORY_SPEED;
        gearsDamage = workSpeed * GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT;
    }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        showOnGUI = true;
        return workbuildingObserver;
        //copy to expeditionCorpus.cs
        //copy to xstation.cs
    }

    override public void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                GameLogUI.NotEnoughResourcesAnnounce();
                return;
            }
        }
        WorkBuilding upgraded = GetStructureByID(upgradedIndex) as WorkBuilding;
        upgraded.Prepare();
        PixelPosByte setPos = new PixelPosByte(surfaceRect.x, surfaceRect.z);
        if (upgraded.surfaceRect.size == 16) setPos = new PixelPosByte(0, 0);
        int workers = workersCount;
        workersCount = 0;
        if (upgraded.rotate90only & (modelRotation % 2 != 0))
        {
            upgraded.modelRotation = (byte)(modelRotation - 1);
        }
        else upgraded.modelRotation = modelRotation;
        upgraded.AddWorkers(workers);
        upgraded.SetBasement(basement, setPos);
        if (GameMaster.eventsTracking) EventChecker.BuildingUpgraded(this);
        if (returnToUI) upgraded.ShowOnGUI();
        //copied to factory.levelup
    }

    protected void PrepareWorkbuildingForDestruction(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (workersCount != 0) colony.AddWorkers(workersCount);
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface,returnResources, leaveRuins);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
        //copy to expedition corpus.cs
        //copy to hospital.cs
        //copy to observatory.cs
        //copy to science lab.cs
        //changed in PsychokineticGenerator
        //copy to farmblock
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(SaveWorkbuildingData());
        return data;
    }

    public List<byte> SaveWorkbuildingData()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(workflow));
        data.AddRange(System.BitConverter.GetBytes(workSpeed));
        data.AddRange(System.BitConverter.GetBytes(workflowToProcess));
        data.AddRange(System.BitConverter.GetBytes(workersCount));
        //SERIALIZER_LENGTH = 16;
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        LoadWorkBuildingData(fs);
    }
    protected void LoadWorkBuildingData(System.IO.FileStream fs)
    {
        var data = new byte[WORKBUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadWorkBuildingData(data, 0);
    }
    protected void LoadWorkBuildingData(byte[] data, int startIndex)
    {
        workflow = System.BitConverter.ToSingle(data, startIndex);
        workSpeed = System.BitConverter.ToSingle(data, startIndex + 4);
        workflowToProcess = System.BitConverter.ToSingle(data, startIndex + 8);
        workersCount = System.BitConverter.ToInt32(data, startIndex + 12);
    }
    #endregion
}