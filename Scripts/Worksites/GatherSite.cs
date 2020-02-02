using System.Collections.Generic;
using UnityEngine;

public class GatherSite : Worksite
{
    private float destructionTimer;
    private const int START_WORKERS_COUNT = 5;
    // public const int MAX_WORKERS = 32

    public GatherSite(Plane i_plane, byte i_faceIndex) : base(i_plane, i_faceIndex)
    {
        sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/GatherSign")).GetComponent<WorksiteSign>();
        sign.worksite = this;
        sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * 0.5f * Block.QUAD_SIZE;
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

    override public void WorkUpdate()
    {
        if (workplace == null)
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
                List<Structure> strs = workplace.GetStructuresList();
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
        if (workplace != null)
        {
            workplace.RemoveWorksiteLink(this);
            workplace = null;
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
        if (workplace == null)
        {
            StopWork();
            return null;
        }
        else
        {
            var pos = workplace.pos;
            var data = new List<byte>() {
                (byte)WorksiteType.GatherSite, pos.x ,pos.y, pos.z, faceIndex
            };
            data.AddRange(System.BitConverter.GetBytes(destructionTimer));
            data.AddRange(SerializeWorksite());
            return data;
        }
    }
    public static GatherSite Load(System.IO.FileStream fs, Chunk chunk)
    {
        var data = new byte[8];
        fs.Read(data, 0, data.Length);
        Plane plane = null;
        if (chunk.GetBlock(data[0], data[1], data[2])?.TryGetPlane(data[3], out plane) == true)
        {
            var cs = new GatherSite(plane, data[3]);
            cs.destructionTimer = System.BitConverter.ToSingle(data, 4);
            cs.LoadWorksiteData(fs);
            return cs;
        }
        else
        {
            Debug.Log("gather site load error");
            return null;
        }
    }

    #endregion
}
