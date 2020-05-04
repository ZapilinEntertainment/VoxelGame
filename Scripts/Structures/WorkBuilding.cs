using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkBuilding : Building
{
    public static UIWorkbuildingObserver workbuildingObserver;

    public float workflow { get; protected set; }
    public float workflowToProcess { get; protected set; }
    public int maxWorkers { get; protected set; }
    public int workersCount { get; protected set; }
    protected float workSpeed, gearsDamage;
    protected ColonyController colony;

    public const int WORKBUILDING_SERIALIZER_LENGTH = 8;

    override public void Prepare()
    {
        PrepareWorkbuilding();
    }
    protected void PrepareWorkbuilding()
    {
        PrepareBuilding();
        workersCount = 0;
        workflow = 0;
        gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT;
        switch (ID)
        {
            case DOCK_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.2f;
                }
                break;
            case DOCK_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.15f;
                }
                break;
            case DOCK_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 120;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.1f;
                }
                break;
            case FARM_1_ID:
                {
                    workflowToProcess = 10f;
                    maxWorkers = 100;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.8f;
                }
                break;
            case FARM_2_ID:
                {
                    workflowToProcess = 8f;
                    maxWorkers = 100;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.7f;
                }
                break;
            case FARM_3_ID:
                {
                    workflowToProcess = 5f;
                    maxWorkers = 100;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.6f;
                }
                break;
            case COVERED_FARM:
                {
                    workflowToProcess = 50f;
                    maxWorkers = 100;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.5f;
                }
                break;
            case FARM_BLOCK_ID:
                {
                    workflowToProcess = 25;
                    maxWorkers = 300;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.5f;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    workflowToProcess = 70;
                    maxWorkers = 80;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.6f;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    workflowToProcess = 60;
                    maxWorkers = 80;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.5f;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    workflowToProcess = 50;
                    maxWorkers = 80;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.45f;
                }
                break;
            case COVERED_LUMBERMILL:
                {
                    workflowToProcess = 100;
                    maxWorkers = 140;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 1.1f;
                }
                break;
            case LUMBERMILL_BLOCK_ID:
                {
                    workflowToProcess = 70;
                    maxWorkers = 280;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 1.2f;
                }
                break;
            case MINE_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.75f;
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
            case SMELTERY_BLOCK_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 400;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.95f;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    workflowToProcess = 40;
                    maxWorkers = 20;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.2f;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 50;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.45f;
                }
                break;
            case MINERAL_POWERPLANT_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.3f;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 2f;
                }
                break;
            case WORKSHOP_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                    gearsDamage = 0f;
                }
                break;
            case FUEL_FACILITY_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 100;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.68f;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 80;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.6f;
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 120;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.4f;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 150;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.8f;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 140;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.7f;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 250;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.7f;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 1.4f;
                }
                break;
            case XSTATION_3_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 40;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.1f;
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
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.1f;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    workflowToProcess = 1;
                    maxWorkers = 60;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.1f;
                }
                break;
            case OBSERVATORY_ID:
                {
                    workflowToProcess = Observatory.SEARCH_WORKFLOW;
                    maxWorkers = 50;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.12f;
                }
                break;
            case PSYCHOKINECTIC_GEN_ID:
            case SCIENCE_LAB_ID:
                {
                    maxWorkers = 40;
                    gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 0.14f;
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
        //copy to StabilityEnforcer.cs
        //copy to HangingTMast.cs
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
            workSpeed = colony.workspeed * workersCount * GameConstants.FACTORY_SPEED;
            gearsDamage = workSpeed * GameConstants.GEARS_DAMAGE_COEFFICIENT;
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage * workSpeed;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
        else workSpeed = 0f;
        // changecopy to coveredfarm.cs
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
    }

    virtual public float GetWorkSpeed()
    {
        return workSpeed;
    }
    virtual public bool ShowWorkspeed() { return false; }

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
        GameMaster.realMaster.eventTracker?.BuildingUpgraded(this);
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
        //copy to stability enforcer
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
        data.AddRange(System.BitConverter.GetBytes(workersCount));
        //SERIALIZER_LENGTH = 8;
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        var data = new byte[WORKBUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadWorkBuildingData(data, 0);
    }
    protected void LoadWorkBuildingData(byte[] data, int startIndex)
    {
        workflow = System.BitConverter.ToSingle(data, startIndex);
        workersCount = System.BitConverter.ToInt32(data, startIndex + 4);
    }
    #endregion
}