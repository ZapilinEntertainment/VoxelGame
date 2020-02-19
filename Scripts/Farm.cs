using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding
{
    private int lastPlantIndex;
    public PlantType cropType;
    private const float ACTION_LIFEPOWER_COST = 10f;

    override public void Prepare()
    {
        PrepareWorkbuilding();
        switch (ID)
        {
            case FARM_1_ID:
            case FARM_2_ID:
            case FARM_3_ID:
            case FARM_4_ID:
            case FARM_5_ID:
                cropType = PlantType.Corn;
                break;
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case LUMBERMILL_4_ID:
            case LUMBERMILL_5_ID:
                cropType = PlantType.OakTree;
                break;
        }  
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetWorkbuildingData(b, pos);
        b.ChangeMaterial(ResourceType.FERTILE_SOIL_ID, true);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        var gl = basement.GetGrassland();
        if (gl == null) gl = basement.GetExtension().InitializeGrassland();
        gl.SetCultivatingStatus(true);
    }

    override public void RecalculateWorkspeed()
    {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.OPEN_FARM_SPEED;
        gearsDamage = GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT * workSpeed;
    }
    override public void LabourUpdate()
    {
        if (isActive & isEnergySupplied)
        {
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage;
            if (workflow >= workflowToProcess)
            {
                LabourResult();
            }
        }
    }
    override protected void LabourResult()
    {
        int i = 0;
        float totalCost = 0;
        int actionsPoints = (int)(workflow / workflowToProcess);

        if (basement.fulfillStatus != FullfillStatus.Full)
        {
            List<PixelPosByte> pos = basement.GetExtension().GetAcceptableCellPositions(actionsPoints);
            i = 0;
            while (i < pos.Count)
            {
                Plant p = Plant.GetNewPlant(cropType);
                p.SetBasement(basement, pos[i]);
                actionsPoints--;
                totalCost += ACTION_LIFEPOWER_COST;
                i++;
            }
        }
        else
        {
            var allplants = basement.GetPlants();
            var indexes = new List<int>();
            int count = allplants.Length;            
            indexes.Capacity = count;
            for (i = 0; i < count; i++)
            {
                indexes.Add(i);
            }
            Plant p;
            while (actionsPoints > 0 & indexes.Count > 0)
            {
                if (lastPlantIndex >= count) lastPlantIndex = 0;
                p = allplants[indexes[lastPlantIndex]];
                if (p.type == cropType)
                {
                    if (!p.IsFullGrown() & p.type == cropType)
                    {
                        p.UpdatePlant();
                        totalCost += ACTION_LIFEPOWER_COST;
                        lastPlantIndex++;
                        actionsPoints -= 4;
                    }
                    else
                    {
                        p.Harvest(true);
                        indexes.RemoveAt(lastPlantIndex);
                        count--;
                        actionsPoints--;
                    }
                }
                else
                {
                    p.Harvest(false);
                    indexes.RemoveAt(lastPlantIndex);
                    count--;
                    actionsPoints -= 2;
                }
            }
        }
        if (totalCost > 0) {
            var gl = basement.GetGrassland();
            if (gl == null) gl = basement.GetExtension().InitializeGrassland();
            gl.TakeLifepower(totalCost);
        }
    }

    override public bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
    {
        if (!Nature.MaterialIsLifeSupporting(p.materialID))
        {
            refusalReason = Localization.GetRestrictionPhrase(RestrictionKey.UnacceptableSurfaceMaterial);
            return false;
        }
        else return true;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) { basement = null; }
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        basement?.GetGrassland()?.SetCultivatingStatus(false);
        if ((basement != null & clearFromSurface) && basement.materialID == ResourceType.FERTILE_SOIL_ID) basement.ChangeMaterial(ResourceType.DIRT_ID, true);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
