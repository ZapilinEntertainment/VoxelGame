using UnityEngine; // mathf
using System.Collections.Generic;

public class Powerplant : WorkBuilding
{
    protected ResourceType fuel;
    protected float output, fuelNeeds, fuelBurnTime;
    public float fuelLeft { get; protected set; }
    protected int tickTimer = 0;

    public const int BIOGEN_OUTPUT = 40, MINERAL_F_PP_OUTPUT = 180, GRPH_REACTOR_OUTPUT = 1000, REACTOR_BLOCK_5_OUTPUT = 3000;

    override public void Prepare()
    {
        PrepareWorkbuilding();
        switch (ID)
        {
            case BIOGENERATOR_2_ID:
                fuel = ResourceType.Food;
                output = BIOGEN_OUTPUT;
                fuelNeeds = 10;
                fuelLeft = 0;
                fuelBurnTime = 200; // ticks
                break;
            case MINERAL_POWERPLANT_2_ID:
                fuel = ResourceType.mineral_F;
                output = MINERAL_F_PP_OUTPUT;
                fuelNeeds = 1;
                fuelLeft = 0;
                fuelBurnTime = 600; // ticks
                break;
            case GRPH_REACTOR_4_ID:
                fuel = ResourceType.Graphonium;
                output = GRPH_REACTOR_OUTPUT;
                fuelNeeds = 1;
                fuelLeft = 0;
                fuelBurnTime = 6000; //ticks
                break;
            case REACTOR_BLOCK_5_ID:
                fuel = ResourceType.Graphonium;
                output = REACTOR_BLOCK_5_OUTPUT;
                fuelNeeds = 4;
                fuelLeft = 0;
                fuelBurnTime = 5800; //ticks
                break;
        }
        //dependency : IslandEngine
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
        ChangeRenderersView(energySurplus > 0f);
    }

    public override float GetWorkSpeed()
    {
        return energySurplus;
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
            if (tickTimer == 0 & energySurplus != 0f)
            {
                energySurplus = 0f;
                colony.powerGridRecalculationNeeded = true;
                ChangeRenderersView(false);
            }
        }
        else
        {
            tickTimer--;            
            float newEnergySurplus = GetEnergyOutput();            
            if (newEnergySurplus != energySurplus)
            {
                energySurplus = newEnergySurplus;
                colony.powerGridRecalculationNeeded = true;
                ChangeRenderersView(energySurplus > 0f);
            }
        }
        fuelLeft = tickTimer / fuelBurnTime;
    }
    private float GetEnergyOutput()
    {
        float rel = workersCount / (float)maxWorkers;
        return rel * output;
    }

    /// <summary>
	/// return excess workers
	/// </summary>
	override public int AddWorkers(int x)
    { // не используется recalculate workspeed
        if (workersCount == maxWorkers) return x;
        else
        {
            if (x > maxWorkers - workersCount)
            {
                x = maxWorkers - workersCount;
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

    override public void FreeWorkers(int x)
    { // не используется recalculate workspeed
        if (x > workersCount) x = workersCount;
        workersCount -= x;
        colony.AddWorkers(x);
    }

    public int GetFuelResourceID() { return fuel.ID; }

    public override UIObserver ShowOnGUI()
    {
        if (workbuildingObserver == null) workbuildingObserver = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
        else workbuildingObserver.gameObject.SetActive(true);
        workbuildingObserver.SetObservingWorkBuilding(this);
        UIController.current.ActivateProgressPanel(ProgressPanelMode.Powerplant);
        showOnGUI = true;
        return workbuildingObserver;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
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
        var data = SaveStructureData();
        data.AddRange(SaveBuildingData());
        data.AddRange(System.BitConverter.GetBytes(workersCount));
        data.AddRange(System.BitConverter.GetBytes(tickTimer));
        data.AddRange(System.BitConverter.GetBytes(fuelLeft));
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane p)
    {
        LoadStructureData(fs, p);
        LoadBuildingData(fs);
        var data = new byte[12];
        fs.Read(data, 0, data.Length);
        workersCount = System.BitConverter.ToInt32(data, 0);
        tickTimer = System.BitConverter.ToInt32(data,4);
        fuelLeft = System.BitConverter.ToSingle(data,8);
    }
    #endregion
}