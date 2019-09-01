using System.Collections.Generic;
using UnityEngine;

public sealed class ScienceLab : WorkBuilding
{
    public enum Research : sbyte { NoResearch = -1, PopulationSecret = 0, AscensionSecret = 7, CrystalSecret = 14, LifepowerSecret = 21, Last = 28 }
    public enum ResearchRoute : byte { Population = 0, CloudWhale, Pipe, Crystal, Monument, NeuroTree, Butterfly, Greenhouse, Total }
    //dependence: AddResearchPoints()
    public static bool?[] researchStatus;
    public static int knowledgePoints { get; private set; }
    public static float[] routePoints;

    private static bool[] routeFullfilled;

    private const float ONE_STEP_VALUE = 1f, PARTVAL20 = 0.2f, PARTVAL40 = 0.4f, PARTVAL60 = 0.6f, PARTVAL80 = 0.8f, BASIC_EXPLORING_BOOST = 0.1f, SUBROUTES_CF = 0.6f;

    #region science
    static ScienceLab()
    {
        int count = (int)Research.Last;
        researchStatus = new bool?[count];
        for (int i = 3; i < count; i++)
        {
            researchStatus[i] = null;
        }
        researchStatus[(int)Research.PopulationSecret] = false;
        researchStatus[(int)Research.AscensionSecret] = false;
        researchStatus[(int)Research.CrystalSecret] = false;
        researchStatus[(int)Research.LifepowerSecret] = false;

        count = (int)ResearchRoute.Total;
        routePoints = new float[count];
        routeFullfilled = new bool[count];
    }
    public static void PointExplored(PointOfInterest poi)
    {
        float val = BASIC_EXPLORING_BOOST * poi.difficulty * (1.1f - poi.exploredPart);
        switch (poi.type)
        {
            case MapMarkerType.Station:
                {
                    AddResearchPoints(Random.value < 0.8f ? ResearchRoute.Population : ResearchRoute.Pipe, val);
                    break;
                }
            case MapMarkerType.Wreck:
                {
                    AddResearchPoints(ResearchRoute.Pipe, val);
                    break;
                }
            case MapMarkerType.Island:
                {
                    AddResearchPoints(ResearchRoute.Butterfly, val);
                    break;
                }
            case MapMarkerType.SOS:
                {
                    float f = Random.value;
                    if (f <= 0.33f) AddResearchPoints(ResearchRoute.Population, val);
                    else
                    {
                        if (f >= 0.66f) AddResearchPoints(ResearchRoute.Pipe, val);
                        else AddResearchPoints(ResearchRoute.Monument, val);
                    }
                    break;
                }
            case MapMarkerType.Portal:
                {
                    if (Random.value < 0.8) AddResearchPoints(ResearchRoute.Pipe, val);
                    else AddResearchPoints(ResearchRoute.Monument, val);
                    break;
                }
            case MapMarkerType.Colony:
                {
                    if (Random.value < 0.8f) AddResearchPoints(ResearchRoute.Population, val);
                    else AddResearchPoints(ResearchRoute.Butterfly, val);
                    break;
                }
            case MapMarkerType.Wiseman:
                {
                    if (Random.value > 0.5f) AddResearchPoints(ResearchRoute.Butterfly, val);
                    else AddResearchPoints(ResearchRoute.Pipe, val);
                    break;
                }
            case MapMarkerType.Wonder:
                {
                    float f = Random.value;
                    if (f < 0.7f) AddResearchPoints(ResearchRoute.Monument, val);
                    else
                    {
                        if (f > 0.9f) AddResearchPoints(ResearchRoute.Pipe, val);
                        else AddResearchPoints(ResearchRoute.Butterfly, val);
                    }
                    break;
                }
            case MapMarkerType.Resources:
                {
                    float f = Random.value;
                    if (f < 0.5f)
                    {
                        if (f < 0.25f) AddResearchPoints(ResearchRoute.Population, val * 0.1f);
                        else AddResearchPoints(ResearchRoute.Pipe, val * 0.1f);
                    }
                    else
                    {
                        if (f > 0.75f) AddResearchPoints(ResearchRoute.Monument, val * 0.1f);
                        else AddResearchPoints(ResearchRoute.Butterfly, val * 0.1f);
                    }
                    break;
                }
        }
    }
    public static void AddResearchPoints(ResearchRoute route, float val)
    {
        int index = (int)route;
        const float POINTS_TO_FULLFILL = 4 * ONE_STEP_VALUE;
        if (!routeFullfilled[index])
        {
            routePoints[index] += val;
            if (routePoints[index] > POINTS_TO_FULLFILL) routeFullfilled[index] = true;
        }
        switch (route)
        {
            case ResearchRoute.Population:
                {
                    int sideIndex = (int)ResearchRoute.CloudWhale;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    sideIndex = (int)ResearchRoute.Greenhouse;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    break;
                }
            case ResearchRoute.Pipe:
                {
                    int sideIndex = (int)ResearchRoute.CloudWhale;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    sideIndex = (int)ResearchRoute.Crystal;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    break;
                }
            case ResearchRoute.Monument:
                {
                    int sideIndex = (int)ResearchRoute.Crystal;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    sideIndex = (int)ResearchRoute.NeuroTree;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    break;
                }
            case ResearchRoute.Butterfly:
                {
                    int sideIndex = (int)ResearchRoute.NeuroTree;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    sideIndex = (int)ResearchRoute.Greenhouse;
                    if (!routeFullfilled[sideIndex])
                    {
                        routePoints[sideIndex] += val;
                        if (routePoints[sideIndex] >= POINTS_TO_FULLFILL) routeFullfilled[sideIndex] = true;
                    }
                    break;
                }
        }
    }
    public static ScienceTabUI.ScienceTabPart GetTechIcon(int index)
    {
        var r = (Research)index;
        float v = 0f;
        switch (r)
        {
            case Research.CrystalSecret:
                if (researchStatus[index] == true) return ScienceTabUI.ScienceTabPart.ExploredPosition;
                else
                {
                    v = routePoints[(int)ResearchRoute.Monument];
                    goto END;
                }
            case Research.LifepowerSecret:
                if (researchStatus[index] == true) return ScienceTabUI.ScienceTabPart.ExploredPosition;
                else
                {
                    v = routePoints[(int)ResearchRoute.Butterfly];
                    goto END;
                }
            case Research.AscensionSecret:
                if (researchStatus[index] == true) return ScienceTabUI.ScienceTabPart.ExploredPosition;
                else
                {
                    v = routePoints[(int)ResearchRoute.Pipe];
                    goto END;
                }
            case Research.PopulationSecret:
                if (researchStatus[index] == true) return ScienceTabUI.ScienceTabPart.ExploredPosition;
                else
                {
                    v = routePoints[(int)ResearchRoute.Population];
                    goto END;
                }
            default: return ScienceTabUI.ScienceTabPart.UnexploredPosition;
        }
        END:
        if (v >= PARTVAL60)
        {
            if (v >= PARTVAL80) return ScienceTabUI.ScienceTabPart.Position80;
            else return ScienceTabUI.ScienceTabPart.Position60;
        }
        else
        {
            if (v < PARTVAL20) return ScienceTabUI.ScienceTabPart.Position20;
            else return ScienceTabUI.ScienceTabPart.Position40;
        }
    }
    #endregion

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        UIController.current.AddFastButton(this);
    }
    override public void LabourUpdate()
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
        var slabs = Component.FindObjectsOfType<ScienceLab>();
        if (slabs.Length == 1 && slabs[0] == this) ScienceTabUI.DestroyInterface();
        Destroy(gameObject);
    }
}
