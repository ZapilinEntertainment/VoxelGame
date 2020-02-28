using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EventChecker
{
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
}
