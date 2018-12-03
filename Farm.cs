using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : WorkBuilding
{
    public int crop_id = -1;
    byte harvestableStage = 0;
    float lifepowerBoost = 1;
    const float BOOST_ACTIVITY_COST = 0.3f, HARVEST_ACTIVITY_COST = 1, PLANT_ACTIVITY_COST = 1;

    override public void Prepare()
    {
        PrepareWorkbuilding();
        switch (id)
        {
            case FARM_1_ID:
            case FARM_2_ID:
            case FARM_3_ID:
            case FARM_4_ID:
            case FARM_5_ID:
                crop_id = Plant.CROP_CORN_ID;
                lifepowerBoost = Plant.GetMaxLifeTransfer(crop_id);
                break;
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case LUMBERMILL_4_ID:
            case LUMBERMILL_5_ID:
                crop_id = Plant.TREE_OAK_ID;
                lifepowerBoost = Plant.GetMaxLifeTransfer(crop_id) / 8f;
                break;
        }
        harvestableStage = Plant.GetHarvestableStage(crop_id);        

    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        if (b.cellsStatus != 0)
        {
            int i = 0;
            Plant p = null;
            while (i < b.surfaceObjects.Count)
            {
                if (b.surfaceObjects[i] == null)
                {
                    i++;
                    continue;
                }
                else
                {
                    p = b.surfaceObjects[i] as Plant;
                    if (p != null && p.plant_ID == crop_id)
                    {
                        i++;
                        continue;
                    }
                    else b.surfaceObjects[i].Annihilate(false);
                }
            }
        }
        SetWorkbuildingData(b, pos);
        b.ReplaceMaterial(ResourceType.FERTILE_SOIL_ID);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
    }

    override protected void RecalculateWorkspeed()
    {
        workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Farming);
    }

    override public void LabourUpdate()
    {
        if (isActive & energySupplied)
        {
            workflow += workSpeed;
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
        float actionsPoints = workflow / workflowToProcess;
        workflow = 0;
        List<Structure> structures = basement.surfaceObjects;   
        
        if (basement.cellsStatus != 1)
        {
            List<PixelPosByte> pos = basement.GetAcceptablePositions((int)(actionsPoints / PLANT_ACTIVITY_COST ));
            int cost = Plant.GetCreateCost(crop_id);
            i = 0;
            while (i < pos.Count )
            {
                Plant p = Plant.GetNewPlant(crop_id);
                p.Prepare();
                p.SetBasement(basement, pos[i]);
                totalCost += cost;
                i++;
            }
        }
        if (actionsPoints > HARVEST_ACTIVITY_COST)
        {
            while (i < structures.Count & actionsPoints > 0)
            {
                if (structures[i].id == PLANT_ID)
                {
                    Plant p = structures[i] as Plant;
                    if (p.plant_ID == crop_id)
                    {
                        if (p.stage >= harvestableStage & p.growth >= 1)
                        {
                            p.Harvest();
                            actionsPoints -= HARVEST_ACTIVITY_COST;
                        }
                        else
                        {
                            if (p.lifepower < p.lifepowerToGrow)
                            {
                                p.AddLifepower((int)lifepowerBoost);
                                totalCost += lifepowerBoost;
                                actionsPoints -= BOOST_ACTIVITY_COST;
                            }
                        }
                    }
                    else
                    {
                        p.Harvest();
                    }
                }
                i++;
            }
        }
        if (totalCost > 0) basement.myChunk.TakeLifePowerWithForce((int)totalCost);
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        if (PrepareWorkbuildingForDestruction(forced))
        {
            if (basement.material_id == ResourceType.FERTILE_SOIL_ID) basement.ReplaceMaterial(ResourceType.DIRT_ID);
        }
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
