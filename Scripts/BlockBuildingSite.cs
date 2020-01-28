using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingSite : Worksite
{
    Plane workObject;
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
            if (workflow >= 20)
            {
                LabourResult();
                workflow-= 20;
            }
        }
    }

    void LabourResult()
    {
        actionLabel = "";        
        int length = PlaneExtension.INNER_RESOLUTION / ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE;
        bool?[,] pillarsMap = new bool?[length, length]; // true - full pillar, false - unfinished, null - no pillar
        for (int a = 0; a < length; a++)
        {
            for (int b = 0; b < length; b++)
            {
                pillarsMap[a, b] = null;
            }
        }
        int maxPillarsCount = length * length;
        int emptyPositions = maxPillarsCount;

        int finishedPillarsCount = 0, totalResourcesCount = 0, deletedStructures = 0;
        var unfinishedPillarsList = new List<ScalableHarvestableResource>();
        ScalableHarvestableResource shr = null;

        /*
        if (workObject.noEmptySpace != false) { // на поверхности есть какие-то структуры
            byte maxVolume = ScalableHarvestableResource.MAX_STICK_VOLUME;
            int i = 0;
            var strs = workObject.structures;
            
            while (i < strs.Count)
            {
                shr = strs[i] as ScalableHarvestableResource;
                if (shr != null)
                {
                    if (shr.mainResource != rtype) shr.Harvest();
                    else
                    {
                        if (shr.resourceCount == maxVolume)
                        {
                            finishedPillarsCount++;
                            pillarsMap[shr.surfaceRect.x / ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE, shr.surfaceRect.z / ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE] = true;
                        }
                        else
                        {
                            unfinishedPillarsList.Add(shr);
                            pillarsMap[shr.surfaceRect.x / ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE, shr.surfaceRect.z / ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE] = false;
                            totalResourcesCount += shr.resourceCount;
                        }
                    }
                }
                else
                {
                    Structure s = strs[i];
                    if (s.isArtificial)
                    {
                        s.Annihilate(true, true, false);
                        return;
                    }
                    else
                    {
                        if (s.ID == Structure.PLANT_ID)
                        {
                            (s as Plant).Harvest(false);
                            if (s != null) s.Annihilate(true, false, false);
                        }
                        else s.Annihilate(true, true, false);
                        deletedStructures++;
                        if (deletedStructures >= 4) return; // не больше 4-х удалений за тик
                    }
                }
                i++;
            }            
        }
        shr = null;

        if (finishedPillarsCount == maxPillarsCount)
        {
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.BlockCompleted);
            workObject.ClearSurface(false, false); // false так как все равно его удаляем
            workObject.myChunk.ReplaceBlock(workObject.pos, BlockType.Cube, rtype.ID, rtype.ID, false);
            workObject.myChunk.RenderStatusUpdate();
            StopWork();
        }
        else
        {
            totalResourcesCount += finishedPillarsCount * ScalableHarvestableResource.MAX_STICK_VOLUME;
            int unfinishedCount = unfinishedPillarsList.Count;
            byte resourceNeeded = ScalableHarvestableResource.RESOURCES_PER_LEVEL;
            float resourceTaken = colony.storage.GetResources(rtype, resourceNeeded);
            bool newContainerCreated = false;

            if (unfinishedCount + finishedPillarsCount < maxPillarsCount)
            {                
                if (Random.value > 0.5f && resourceTaken == resourceNeeded) // creating new container
                {
                        var emptyPositionsIndexes = new List<int>();
                        for (int x = 0; x < length; x++)
                        {
                            for (int z = 0; z < length; z++)
                            {
                                if (pillarsMap[x, z] == null) emptyPositionsIndexes.Add(x * length + z);
                            }
                        }
                        pillarsMap = null;
                        int epcount = emptyPositionsIndexes.Count;
                        if (epcount > 0)
                        {
                            int combinedIndex = emptyPositionsIndexes[Random.Range(0, epcount - 1)];
                            ScalableHarvestableResource.Create(rtype, resourceNeeded, workObject,
                                new PixelPosByte(
                                    (combinedIndex / length) * ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE,
                                    (combinedIndex % length) * ScalableHarvestableResource.RESOURCE_STICK_RECT_SIZE)
                                    );
                            newContainerCreated = true;
                        }
                        emptyPositionsIndexes = null;
                }               
            }
            // докидываем в существующий
            if (unfinishedCount > 0)
            {
                if (newContainerCreated) resourceTaken = colony.storage.GetResources(rtype, resourceNeeded);
                if (resourceTaken > 1)
                {
                    shr = unfinishedPillarsList[Random.Range(0, unfinishedCount - 1)];
                    resourceTaken = shr.AddResource(rtype, resourceTaken);
                    if (resourceTaken > 0) colony.storage.AddResource(rtype, resourceTaken);
                }
                else { if (showOnGUI) actionLabel = Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources); }
            }
        }
        actionLabel = string.Format("{0:0.##}", totalResourcesCount / (float)CubeBlock.MAX_VOLUME * 100f) + '%';
        */
    }

    protected override void RecalculateWorkspeed()
    {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.BLOCK_BUILDING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
    }
    public void Set(Plane block, ResourceType type)
    {
        workObject = block;
        //workObject.SetWorksite(this);
        rtype = type;
        actionLabel = Localization.GetStructureName(Structure.RESOURCE_STICK_ID);
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
            bc.center = new Vector3(0, - 0.75f, 0);
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
        //if (workObject != null & workObject.worksite == this) workObject.ResetWorksite();
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
        /*
        Set(
            GameMaster.realMaster.mainChunk.GetBlock(cpos) as Plane, 
            ResourceType.GetResourceTypeById(System.BitConverter.ToInt32(data, 0))
            );
            */
        LoadWorksiteData(fs);
    }
    #endregion
}
