using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EventChecker
{
    private bool kn_immigrantsTracking = true;
    public System.Action<Structure> buildingConstructionEvent;
    public System.Action<Building> buildingUpgradeEvent;

    public void BuildingUpgraded(Building b)
    {
        buildingUpgradeEvent?.Invoke(b);
    }
    public void BuildingConstructed(Structure s)
    {
        buildingConstructionEvent?.Invoke(s);
    }

    public void ImmigrantsCheck(uint count)
    {
        if (kn_immigrantsTracking)
        {
            Knowledge.GetCurrent()?.ImmigrantsCheck(count);
        }
    }

    public void StopTracking(Knowledge.ResearchRoute route, byte boosterIndex)
    {
        switch (route)
        {
            case Knowledge.ResearchRoute.Foundation:
                switch ((Knowledge.FoundationRouteBoosters)boosterIndex)
                {
                    case Knowledge.FoundationRouteBoosters.ImmigrantsBoost: kn_immigrantsTracking = false; break;
                }
                break;
        }
    }
}
