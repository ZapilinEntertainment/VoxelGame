using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingSite : Worksite
{
    SurfaceBlock workObject;
    const float BUILDING_SPEED = 0.002f;
    ResourceType rtype;
    const int START_WORKERS_COUNT = 20;

    public override int GetMaxWorkers()
    {
        return 400;
    }

    override public void WorkUpdate()
    {
        if (workObject == null)
        {
            StopWork();
        }
        if (workersCount > 0)
        {
            workflow += workSpeed;
            LabourResult();
        }
    }

    void LabourResult()
    {
        float totalResources = 0;
        actionLabel = "";
        bool?[,] pillarsMap = new bool?[8, 8];
        for (int a = 0; a < 8; a++)
        {
            for (int b = 0; b < 8; b++)
            {
                pillarsMap[a, b] = null;
            }
        }
        int placesToWork = 64;
        List<ScalableHarvestableResource> unfinishedPillars = new List<ScalableHarvestableResource>();

        if (workObject.cellsStatus != 0)
        {
            int i = 0;
            while (i < workObject.surfaceObjects.Count)
            {
                ScalableHarvestableResource shr = workObject.surfaceObjects[i] as ScalableHarvestableResource;
                if (shr == null)
                {
                    workObject.surfaceObjects[i].Annihilate(false);
                }
                else
                {
                    if (shr.resourceCount == ScalableHarvestableResource.MAX_VOLUME)
                    {
                        placesToWork--;
                        pillarsMap[shr.innerPosition.x / 2, shr.innerPosition.z / 2] = true;
                        totalResources += ScalableHarvestableResource.MAX_VOLUME;
                    }
                    else
                    {
                        pillarsMap[shr.innerPosition.x / 2, shr.innerPosition.z / 2] = false;
                        totalResources += shr.resourceCount;
                        unfinishedPillars.Add(shr);
                    }
                }
                i++;
            }
        }

        if (placesToWork == 0)
        {
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.BlockCompleted);
            workObject.ClearSurface(false); // false так как все равно его удаляем
            workObject.myChunk.ReplaceBlock(workObject.pos, BlockType.Cube, rtype.ID, rtype.ID, false);
            StopWork();
        }
        else
        {
            int pos = (int)(placesToWork * Random.value);
            int n = 0;
            for (int a = 0; a < 8; a++)
            {
                for (int b = 0; b < 8; b++)
                {
                    if (pillarsMap[a, b] == true) continue;
                    else
                    {
                        if (n == pos)
                        {
                            ScalableHarvestableResource shr = null;
                            float count = BUILDING_SPEED * workflow;
                            if (pillarsMap[a, b] == false)
                            {
                                foreach (ScalableHarvestableResource fo in unfinishedPillars)
                                {
                                    if (fo.innerPosition.x / 2 == a & fo.innerPosition.z / 2 == b)
                                    {
                                        shr = fo;
                                        break;
                                    }
                                }
                                if (count > ScalableHarvestableResource.MAX_VOLUME - shr.resourceCount) count = ScalableHarvestableResource.MAX_VOLUME - shr.resourceCount;
                                shr.AddResource(rtype, count);
                            }
                            else
                            {
                                shr = Structure.GetStructureByID(Structure.RESOURCE_STICK_ID) as ScalableHarvestableResource;
                                shr.AddResource(rtype, count);
                                shr.SetBasement(workObject, new PixelPosByte(a * 2, b * 2));
                            }
                            count = GameMaster.colonyController.storage.GetResources(rtype, count);
                            if (count == 0)
                            {
                                if (showOnGUI) actionLabel = Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources);
                            }
                            else
                            {
                                totalResources += count;                                
                                actionLabel = string.Format("{0:0.##}", totalResources / (float)CubeBlock.MAX_VOLUME * 100f) + '%';
                            }                            
                            return;
                        }
                        else n++;
                    }
                }
            }
        }
    }

    protected override void RecalculateWorkspeed()
    {
        workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Pouring);
    }
    public void Set(SurfaceBlock block, ResourceType type)
    {
        workObject = block;
        workObject.SetWorksite(this);
        rtype = type;
        actionLabel = Localization.GetStructureName(Structure.RESOURCE_STICK_ID);
        GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }

        if (sign == null)
        {
            sign = new GameObject("Block Building Site sign").AddComponent<WorksiteSign>();
            BoxCollider bc = sign.gameObject.AddComponent<BoxCollider>();
            bc.size = new Vector3(Block.QUAD_SIZE, 0.5f, Block.QUAD_SIZE);
            bc.center = new Vector3(0, - 0.25f, 0);
            bc.tag = WORKSITE_SIGN_COLLIDER_TAG;
            sign.worksite = this;
            sign.transform.position = workObject.transform.position;
        }
    }

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            GameMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Object.Destroy(sign.gameObject);
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
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
        }
        Destroy(this);
    }


    #region save-load system
    override protected WorksiteSerializer Save()
    {
        if (workObject == null)
        {
            StopWork();
            return null;
        }
        WorksiteSerializer ws = GetWorksiteSerializer();
        ws.type = WorksiteType.BlockBuildingSite;
        ws.workObjectPos = workObject.pos;
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        {
            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, rtype.ID);
            ws.specificData = stream.ToArray();
        }
        return ws;
    }
    override protected void Load(WorksiteSerializer ws)
    {
        LoadWorksiteData(ws);
        int res_id = 0;
        GameMaster.DeserializeByteArray<int>(ws.specificData, ref res_id);
        Set(GameMaster.mainChunk.GetBlock(ws.workObjectPos) as SurfaceBlock, ResourceType.GetResourceTypeById(res_id));
    }
    #endregion
}
