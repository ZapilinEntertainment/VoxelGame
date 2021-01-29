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
                workComplexityCoefficient = Corn.COMPLEXITY;
                break;
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case COVERED_LUMBERMILL:
            case LUMBERMILL_BLOCK_ID:
                cropType = PlantType.OakTree;
                workComplexityCoefficient = OakTree.COMPLEXITY;
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
        if (gl == null) basement.TryCreateGrassland(out gl);
        b.ChangeMaterial(ResourceType.FERTILE_SOIL_ID, true);
    }

    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        int i = 0;
        float totalCost = 0;
        Plant p;

        if (basement.fulfillStatus != FullfillStatus.Full)
        {
            if (iterations == 1)
            {
                p = Plant.GetNewPlant(cropType);
                p.SetBasement(basement, basement.FORCED_GetExtension().GetRandomCell());
                totalCost += ACTION_LIFEPOWER_COST;
            }
            else
            {
                List<PixelPosByte> pos = basement.FORCED_GetExtension().GetAcceptableCellPositions(iterations);
                i = 0;
                while (i < pos.Count)
                {
                    p = Plant.GetNewPlant(cropType);
                    p.SetBasement(basement, pos[i]);
                    totalCost += ACTION_LIFEPOWER_COST;
                    i++;
                }
            }
        }
        else
        {
            var allplants = basement.GetPlants();
            int count = allplants.Length;
            if (lastPlantIndex >= count) lastPlantIndex = 0;

            if (count > 0) {
                if (iterations == 1)
                {
                    p = allplants[lastPlantIndex];
                    if (p.type == cropType)
                    {
                        if (!p.IsFullGrown())
                        {
                            p.UpdatePlant();
                            totalCost += ACTION_LIFEPOWER_COST * workComplexityCoefficient;

                            if (lastPlantIndex >= count) lastPlantIndex = 0;
                        }
                        else
                        {
                            p.Harvest(true);
                        }
                    }
                    else
                    {
                        p.Harvest(false);
                    }
                    lastPlantIndex++;
                }
                else
                {
                    while (iterations > 0)
                    {
                        p = allplants[lastPlantIndex];
                        if (p.type == cropType)
                        {
                            if (!p.IsFullGrown())
                            {
                                p.UpdatePlant();
                                totalCost += ACTION_LIFEPOWER_COST * p.GetPlantComplexity();
                            }
                            else p.Harvest(true);
                        }
                        else p.Harvest(false);
                        iterations--;
                        lastPlantIndex++;
                        if (lastPlantIndex >= count) lastPlantIndex = 0;
                    }
                }
            }
            if (totalCost > 0)
            {
                var gl = basement.GetGrassland();
                if (gl == null) basement.TryCreateGrassland(out gl);
                gl.TakeLifepower(totalCost);
            }
        }
    }

    public static bool CheckSpecialBuildingCondition(Plane p, ref string refusalReason)
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
        if (gl == null) basement.TryCreateGrassland(out gl);
    }
    #endregion
}
