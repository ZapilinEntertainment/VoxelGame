using System.Collections.Generic;
using UnityEngine;

public sealed class IslandEngine : Powerplant
{
    private const float BASIC_THRUST = 0.01f;
    public byte moveDirection = 0;
    override public void Prepare()
    {
        PrepareWorkbuilding();
        fuel = ResourceType.Fuel;
        output = 0f;
        fuelNeeds = 10f;
        fuelLeft = 0f;
        fuelBurnTime = 200f; // ticks
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
            if (tickTimer == 0) output = 0f;
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
}
