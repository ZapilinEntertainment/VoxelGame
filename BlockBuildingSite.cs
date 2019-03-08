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
            colony.gears_coefficient -= gearsDamage;
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
                            count = GameMaster.realMaster.colonyController.storage.GetResources(rtype, count);
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
        workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Pouring);
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
    }
    public void Set(SurfaceBlock block, ResourceType type)
    {
        workObject = block;
        workObject.SetWorksite(this);
        rtype = type;
        actionLabel = Localization.GetStructureName(Structure.RESOURCE_STICK_ID);
        colony = GameMaster.realMaster.colonyController;
        colony.SendWorkers(START_WORKERS_COUNT, this);
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
            sign.transform.position = workObject.pos.ToWorldSpace() + Vector3.up * 0.5f * Block.QUAD_SIZE;
        }
    }

    override public void StopWork()
    {
        if (destroyed) return;
        else destroyed = true;
        if (workersCount > 0)
        {
            colony.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) MonoBehaviour.Destroy(sign.gameObject);        
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (workObject != null & workObject.worksite == this) workObject.ResetWorksite();
        if (showOnGUI)
        {
            observer.SelfShutOff();
            showOnGUI = false;
            UIController.current.ChangeChosenObject(ChosenObjectType.None);
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
        var data = new List<byte>() { (byte)WorksiteType.BlockBuildingSite };
        data.Add(workObject.pos.x);
        data.Add(workObject.pos.y);
        data.Add(workObject.pos.z);
        data.AddRange(System.BitConverter.GetBytes(rtype.ID));
        data.AddRange(SerializeWorksite());        
        return data;
    }
    override protected void Load(System.IO.FileStream fs, ChunkPos cpos)
    {
        byte[] data = new byte[4];
        fs.Read(data, 0, 4);
        Set(
            GameMaster.realMaster.mainChunk.GetBlock(cpos) as SurfaceBlock, 
            ResourceType.GetResourceTypeById(System.BitConverter.ToInt32(data, 0))
            );
        LoadWorksiteData(fs);
    }
    #endregion
}
