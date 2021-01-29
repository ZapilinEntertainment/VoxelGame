using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Farm : WorkBuilding
{
    private int lastPlantIndex;
    private float _cropComplexity, _plantLPCost = 0f, _updateLPCost = 1f, lifepowerSupport;
    private int _cropPlantCost = 1, _cropUpdateCost = 1, _cropHarvestCost = 1; // MUST NOT BE ZERO!
    public PlantType cropType;    

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
                _cropComplexity = Corn.COMPLEXITY;
                _cropPlantCost = 1;
                _cropUpdateCost = 1;
                _cropHarvestCost = 1;
                _plantLPCost = 1f;
                _updateLPCost = 1f;
                break;
            case LUMBERMILL_1_ID:
            case LUMBERMILL_2_ID:
            case LUMBERMILL_3_ID:
            case COVERED_LUMBERMILL:
            case LUMBERMILL_BLOCK_ID:
                cropType = PlantType.OakTree;
                _cropComplexity = OakTree.COMPLEXITY;
                _cropPlantCost = 1;
                _cropUpdateCost = 3;
                _cropHarvestCost = 10;
                _plantLPCost = 1f;
                _updateLPCost = 5f;
                break;
        }  
    }
    override public float GetLabourCoefficient()
    {
        return base.GetLabourCoefficient() * lifepowerSupport / _cropComplexity;
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

        var emaster = GameMaster.realMaster.environmentMaster;
        lifepowerSupport = emaster.lifepowerSupport;
        emaster.environmentChangingEvent += this.EnvironmentChange;
    }
    private void EnvironmentChange(Environment e)
    {
        lifepowerSupport = e.lifepowerSupport;
    }

    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
        //
        var field = basement.FORCED_GetExtension();               
        Plant p;
        float totalCost = 0f;
        int MAX_TRIES_COUNT = (int)(16f * GameMaster.gameSpeed);
        int i = MAX_TRIES_COUNT;

        if (field.fulfillStatus != FullfillStatus.Full)
        { // setting saplings    
            List<PixelPosByte> freePositions = field.GetRandomCells(iterations / _cropPlantCost);
            int positionsCount = freePositions.Count, index;
            while (i > 0 && iterations > _cropPlantCost && positionsCount > 0)
            {
                i--;
                p = Plant.GetNewPlant(cropType);
                index = Random.Range(0, positionsCount);
                p.SetBasement(basement, freePositions[index]);
                freePositions.RemoveAt(index);
                positionsCount--;
                iterations -= _cropPlantCost;
                totalCost += _plantLPCost;
            }
        }
        if (field.fulfillStatus == FullfillStatus.Full && iterations > 1)
        {
            var plants = field.GetPlants();
            if (plants != null && plants.Length != 0) {
                i = MAX_TRIES_COUNT;
                while (iterations > 0 && i > 0)
                {
                    i--;
                    if (lastPlantIndex >= plants.Length) lastPlantIndex = 0;
                    p = plants[lastPlantIndex];
                    if (p == null)
                    {
                        lastPlantIndex++;
                        continue;
                    }
                    else
                    {
                        if (p.type == cropType)
                        {
                            if (p.IsFullGrown())
                            {
                                // HARVEST CROP
                                if (iterations >= _cropHarvestCost)
                                {
                                    p.Harvest(true);
                                    iterations -= _cropHarvestCost;
                                    lastPlantIndex++;                                    
                                }
                                else break;
                            }
                            else
                            {
                                // CROP UPDATE
                                if (iterations >= _cropUpdateCost)
                                {
                                    p.UpdatePlant();
                                    iterations -= _cropUpdateCost;
                                    lastPlantIndex++;
                                    totalCost += _updateLPCost;
                                }
                                else break;
                            }
                        }
                        else
                        {
                            // HARVEST NON-CROP PLANT
                            int cost = (int)(p.GetPlantComplexity() * p.stage);
                            if (iterations >= cost)
                            {
                                p.Harvest(false);
                                iterations -= cost;
                            }
                            else break;
                        }
                    }
                }
            }
        }
        workflow += iterations;
        
        //
        
        if (totalCost > 0)
        {
            var gl = basement.GetGrassland();
            if (gl == null) basement.TryCreateGrassland(out gl);
            gl.TakeLifepower(totalCost);
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
    public override string UI_GetProductionSpeedInfo()
    {
        return Localization.GetWord(LocalizedWord.Effectiveness) + ": " + ((int)(GetLabourCoefficient() * GameConstants.GetWorkComplexityCf(WorkType.OpenFarming) * 100f)).ToString() + '%';
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
