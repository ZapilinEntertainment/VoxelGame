using System.Collections.Generic;
using UnityEngine;

public class GatherSite : Worksite
{
    float destructionTimer;
    SurfaceBlock workObject;
    const int START_WORKERS_COUNT = 5;
    // public const int MAX_WORKERS = 32

    override public void WorkUpdate()
    {
        if (workObject == null || workObject.structures.Count == 0)
        {
            StopWork();
        }
        if (workersCount > 0)
        {
            workflow += workSpeed;
            colony.gears_coefficient -= gearsDamage;
            if (workflow >= 1)
            {
                int i = 0;
                bool resourcesFound = false;
                List<Structure> strs = workObject.structures;
                while (i < strs.Count & workflow > 0)
                {
                    switch (strs[i].ID)
                    {
                        case Structure.PLANT_ID:
                            Plant p = strs[i] as Plant;
                            if (p != null)
                            {
                                byte hstage = p.GetHarvestableStage();
                                if (hstage != 255 & p.stage >= hstage)
                                {
                                    p.Harvest(false);
                                    resourcesFound = true;
                                    workflow--;
                                }
                            }
                            break;
                        case Structure.CONTAINER_ID:
                            HarvestableResource hr = strs[i] as HarvestableResource;
                            if (hr != null)
                            {
                                hr.Harvest();
                                resourcesFound = true;
                                workflow--;
                            }
                            break;
                        case Structure.RESOURCE_STICK_ID:
                            ScalableHarvestableResource shr = strs[i] as ScalableHarvestableResource;
                            if (shr != null)
                            {
                                shr.Harvest();
                                resourcesFound = true;
                                workflow--;
                            }
                            break;
                    }
                    i++;
                }
                if (resourcesFound) destructionTimer = GameMaster.LABOUR_TICK * 10;
            }
        }

        destructionTimer -= GameMaster.LABOUR_TICK;
        if (destructionTimer <= 0) StopWork();
    }

    protected override void RecalculateWorkspeed()
    {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.GATHERING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
    }
    public void Set(SurfaceBlock block)
    {
        workObject = block;
        workObject.SetWorksite(this);
        if (sign == null) sign = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefs/GatherSign")).GetComponent<WorksiteSign>();
        sign.worksite = this;
        sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.down * 0.5f * Block.QUAD_SIZE;
        actionLabel = Localization.GetActionLabel(LocalizationActionLabels.GatherInProgress);
        colony.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }
        destructionTimer = 10;
    }

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            GameMaster.realMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) MonoBehaviour.Destroy(sign.gameObject);
        
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (workObject != null)
        {
            if (workObject.worksite == this) workObject.ResetWorksite();
            workObject = null;
        }
        if (showOnGUI)
        {
            observer.SelfShutOff();
            showOnGUI = false;
            UIController.current.ChangeChosenObject(ChosenObjectType.Surface);
        }
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
    }

    #region save-load system
    override protected List<byte> Save()
    {
        if (workObject == null)
        {
            StopWork();
            return null;
        }
        var data = new List<byte>() { (byte)WorksiteType.GatherSite };
        data.Add(workObject.pos.x);
        data.Add(workObject.pos.y);
        data.Add(workObject.pos.z);
        data.AddRange(System.BitConverter.GetBytes(destructionTimer));
        data.AddRange(SerializeWorksite());
        return data;
    }
    override protected void Load(System.IO.FileStream fs, ChunkPos pos)
    {
        Set(GameMaster.realMaster.mainChunk.GetBlock(pos) as SurfaceBlock);
        var data = new byte[4];
        fs.Read(data, 0, 4);
        destructionTimer = System.BitConverter.ToSingle(data, 0);
        LoadWorksiteData(fs);
    }

    #endregion
}
