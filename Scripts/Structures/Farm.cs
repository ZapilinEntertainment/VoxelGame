using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Farm : WorkBuilding
{
    private int lastPlantIndex;
    public PlantType cropType;
    private const float ACTION_LIFEPOWER_COST = 1f;

    override public void Prepare()
    {
        PrepareWorkbuilding();
        switch (ID)
        {
            case FARM_1_ID:
            case FARM_2_ID:
            case FARM_3_ID:
            case COVERED_FARM:
            case FARM_BLOCK_ID:
                cropType = PlantType.Corn;
                break;
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case COVERED_LUMBERMILL:
            case LUMBERMILL_BLOCK_ID:
                cropType = PlantType.OakTree;
                break;
        }  
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
        var gl = basement.GetGrassland();
        if (gl == null) gl = basement.FORCED_GetExtension().InitializeGrassland();
        gl?.SetCultivatingStatus(true);
        b.ChangeMaterial(ResourceType.FERTILE_SOIL_ID, true);
    }

    override public void LabourUpdate()
    {
        if (isActive & isEnergySupplied)
        {
            workSpeed = colony.workspeed * workersCount * GameConstants.OPEN_FARM_SPEED;
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage * workSpeed;
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
        workflow -= actionsPoints * workflowToProcess;

        if (basement.fulfillStatus != FullfillStatus.Full)
        {
            List<PixelPosByte> pos = basement.FORCED_GetExtension().GetAcceptableCellPositions(actionsPoints);
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
            int count = allplants.Length, complexity;            
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
                complexity = p.GetPlantComplexity();
                if (p.type == cropType)
                {
                    if (!p.IsFullGrown() & p.type == cropType)
                    {
                        p.UpdatePlant();
                        totalCost += ACTION_LIFEPOWER_COST * complexity;
                        lastPlantIndex++;
                        actionsPoints -= 10 * complexity;
                    }
                    else
                    {
                        actionsPoints -= complexity;
                        p.Harvest(true);
                        indexes.RemoveAt(lastPlantIndex);
                        count--;                        
                    }
                }
                else
                {
                    p.Harvest(false);
                    indexes.RemoveAt(lastPlantIndex);
                    count--;
                    actionsPoints -= complexity;
                }
            }
        }
        if (actionsPoints > 0) workflow += actionsPoints * workflowToProcess;
        else workflow -= actionsPoints * (-1) * workflowToProcess;
        if (totalCost > 0) {
            var gl = basement.GetGrassland();
            if (gl == null) gl = basement.FORCED_GetExtension().InitializeGrassland();
            gl.TakeLifepower(totalCost);
        }
    }

    new public static bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
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

    public override bool ShowWorkspeed()
    {
        return true;
    }

    #region save-load
    public override List<byte> Save()
    {
        var data =  base.Save();
        data.AddRange(System.BitConverter.GetBytes(lastPlantIndex));
        return data;
    }
    public override void Load(FileStream fs, Plane sblock)
    {
        base.Load(fs, sblock);
        var data = new byte[4];
        fs.Read(data, 0, data.Length);
        lastPlantIndex = System.BitConverter.ToInt32(data, 0);
        var gl = basement.GetGrassland();
        if (gl == null) gl = basement.FORCED_GetExtension().InitializeGrassland();
        gl?.SetCultivatingStatus(true);
    }
    #endregion
}
