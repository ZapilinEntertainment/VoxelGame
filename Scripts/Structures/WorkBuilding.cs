using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialWorkMode : MyObject
{
    public readonly int ID;
    public readonly string workname;
    public System.Action resultAction;

    protected override bool IsEqualNoCheck(object obj)
    {
        return ID == (obj as SpecialWorkMode).ID;
    }

    private SpecialWorkMode() { }
    public SpecialWorkMode(int i_ID, string i_label, System.Action i_action)
    {
        ID = i_ID;
        workname = i_label;
        resultAction = i_action;
    }
}

public abstract class WorkBuilding : Building, ILabourable
{
    public static UIWorkbuildingObserver workbuildingObserver;

    public float workflow { get; protected set; }
    public int maxWorkers { get; protected set; }
    public int workersCount { get; protected set; }   
    protected float factoryCoefficient = 1f, workComplexityCoefficient = 1f,  gearsDamage = 0f;
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
                    maxWorkers = 40;
                    gearsDamage *= 0.2f;
                }
                break;
            case DOCK_2_ID:
                {
                    maxWorkers = 80;
                    gearsDamage *= 0.15f;
                }
                break;
            case DOCK_3_ID:
                {
                    maxWorkers = 120;
                    gearsDamage *= 0.1f;
                }
                break;
            case FARM_1_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.OpenFarming);
                    maxWorkers = 100;
                    gearsDamage *= 0.5f;
                }
                break;
            case FARM_2_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.OpenFarming);
                    factoryCoefficient = 1.2f;
                    maxWorkers = 100;
                    gearsDamage *= 0.6f;
                }
                break;
            case FARM_3_ID:
                {
                    factoryCoefficient = 1.5f;
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.OpenFarming);
                    maxWorkers = 100;
                    gearsDamage *= 0.7f;
                }
                break;
            case COVERED_FARM:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.HydroponicsFarming);
                    factoryCoefficient = 2f;
                    maxWorkers = 100;
                    gearsDamage *= 0.9f;
                }
                break;
            case FARM_BLOCK_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.HydroponicsFarming);
                    factoryCoefficient = 2.2f;
                    maxWorkers = 300;
                    gearsDamage *= 0.85f;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.OpenLumbering);
                    maxWorkers = 80;
                    gearsDamage *= 0.5f;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.OpenLumbering);
                    factoryCoefficient = 1.3f;
                    maxWorkers = 80;
                    gearsDamage *= 0.6f;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.OpenLumbering);
                    factoryCoefficient = 1.8f;
                    maxWorkers = 80;
                    gearsDamage *= 0.7f;
                }
                break;
            case COVERED_LUMBERMILL:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.HydroponicsLumbering);
                    factoryCoefficient = 3f;
                    maxWorkers = 140;
                    gearsDamage *= 1.2f;
                }
                break;
            case LUMBERMILL_BLOCK_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.HydroponicsLumbering);
                    factoryCoefficient = 3.2f;
                    maxWorkers = 280;
                    gearsDamage *= 1.3f;
                }
                break;
            case MINE_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.Mining);
                    maxWorkers = 60;
                    gearsDamage *= 1.5f;
                }
                break;
            case SMELTERY_1_ID:
                {                    
                    maxWorkers = 40;
                }
                break;
            case SMELTERY_2_ID:
                {
                    factoryCoefficient = 1.2f;
                    maxWorkers = 60;
                    gearsDamage *= 1.2f;
                }
                break;
            case SMELTERY_3_ID:
                {
                    factoryCoefficient = 1.5f;
                    maxWorkers = 100;
                    gearsDamage *= 1.4f;
                }
                break;
            case SMELTERY_BLOCK_ID:
                {
                    factoryCoefficient = 2f;
                    maxWorkers = 400;
                    gearsDamage *= 1.6f;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    maxWorkers = 20;
                    gearsDamage *= 0.01f;
                }
                break;
            case HOSPITAL_ID:
                {
                    maxWorkers = 50;
                    gearsDamage *= 0.01f;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    maxWorkers = 200;
                    gearsDamage *= 0.01f;
                    break;
                }
            case MINERAL_POWERPLANT_2_ID:
                {
                    maxWorkers = 60;
                    gearsDamage *= 0.01f;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    maxWorkers = 80;
                    gearsDamage *= 2f;
                }
                break;
            case WORKSHOP_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.GearsUpgrading);
                    maxWorkers = 200;
                    gearsDamage = 0f;
                }
                break;
            case FUEL_FACILITY_ID:
                {
                    maxWorkers = 100;
                    gearsDamage *= 0.68f;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    maxWorkers = 80;
                    gearsDamage *= 0.03f;
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    maxWorkers = 120;
                    gearsDamage *= 0.04f;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    maxWorkers = 150;
                    gearsDamage *= 1.3f;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    maxWorkers = 140;
                    gearsDamage *= 1.2f;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    maxWorkers = 250;
                    gearsDamage *= 1.2f;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    maxWorkers = 60;
                    gearsDamage *= 2.2f;
                }
                break;
            case XSTATION_3_ID:
                {
                    maxWorkers = 40;
                    gearsDamage *= 0.01f;
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.ShuttleConstructing);
                    maxWorkers = 45;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.Recruiting);
                    maxWorkers = 40;
                    gearsDamage *= 0.01f;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    maxWorkers = 60;
                    gearsDamage *= 0.01f;
                }
                break;
            case OBSERVATORY_ID:
                {
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.ObservatoryFindCycle);
                    maxWorkers = 50;
                    gearsDamage *= 0.02f;
                }
                break;
            case PSYCHOKINECTIC_GEN_ID:
            case SCIENCE_LAB_ID:
                {
                    maxWorkers = 40;
                    gearsDamage *= 0.03f;
                    break;
                }
            case COMPOSTER_ID:
                {
                    maxWorkers = 60;
                    gearsDamage *= 0.3f;
                    break;
                }
            case ANCHOR_BASEMENT_ID:
                {
                    maxWorkers = 1500;
                    gearsDamage *= 0.1f;
                    workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.AnchorBasement_Lift);
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


    #region ILabourable
    public bool IsWorksite() { return false; }
    virtual public float GetLabourCoefficient()
    {
        return colony.workers_coefficient * factoryCoefficient * workersCount /  workComplexityCoefficient;
    }
    virtual public void LabourUpdate()
    {        
        if (!isActive | !isEnergySupplied) return;
        INLINE_WorkCalculation();
    }   
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
    public int FreeWorkers()
    {
        return FreeWorkers(workersCount);
    }

    /// <summary>
    /// returns the count of workers been free
    /// </summary>
    public virtual int FreeWorkers(int x)
    {
        if (workersCount == 0) return 0;
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
        return x;
    }
    public int GetWorkersCount() { return workersCount; }
    public int GetMaxWorkersCount() { return maxWorkers; }
    public bool MaximumWorkersReached() { return workersCount == maxWorkers; }
    virtual public bool ShowUIInfo() { return false; }
    virtual public string UI_GetInfo() {
        return ((int)(GetLabourCoefficient() * 100f)).ToString() + '%';
    }
    #endregion

    virtual protected void LabourResult(int iterations)
    {
        workflow -= iterations;
    }
    protected void INLINE_WorkCalculation()
    {
        float work = GetLabourCoefficient();
        workflow += work;
        colony.gears_coefficient -= gearsDamage * work;
        if (workflow >= 1f) LabourResult((int)workflow);
    }


    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingPlace(this);
        showOnGUI = true;
        return workbuildingObserver;
    }

    override public void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                AnnouncementCanvasController.NotEnoughResourcesAnnounce();
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