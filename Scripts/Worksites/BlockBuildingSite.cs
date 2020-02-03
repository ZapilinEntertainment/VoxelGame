using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingSite : Worksite
{
    private ResourceType rtype;
    private const int START_WORKERS_COUNT = 20;

    public override int GetMaxWorkers()  { return 400; }

    public BlockBuildingSite(Plane p, ResourceType i_type) : base(p)
    {
        rtype = i_type;
        actionLabel = Localization.GetStructureName(Structure.RESOURCE_STICK_ID);
        sign = new GameObject("Block Building Site sign").AddComponent<WorksiteSign>();
        BoxCollider bc = sign.gameObject.AddComponent<BoxCollider>();
        bc.size = new Vector3(Block.QUAD_SIZE, 0.5f, Block.QUAD_SIZE);
        bc.center = new Vector3(0, -0.75f, 0);
        bc.tag = WORKSITE_SIGN_COLLIDER_TAG;
        sign.worksite = this;
        switch (p.faceIndex)
        {
            case Block.FWD_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.forward * Block.QUAD_SIZE * 0.5f; break;
            case Block.RIGHT_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.right * Block.QUAD_SIZE * 0.5f; break;
            case Block.BACK_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.back * Block.QUAD_SIZE * 0.5f; break;
            case Block.LEFT_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.left * Block.QUAD_SIZE * 0.5f; break;
            case Block.UP_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE * 0.5f; break;
            case Block.DOWN_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * Block.QUAD_SIZE * 0.5f; break;
            case Block.SURFACE_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE * 0.5f; break;
            case Block.CEILING_FACE_INDEX: sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * Block.QUAD_SIZE * 0.5f; break;
        }

        colony.SendWorkers(START_WORKERS_COUNT, this);
        worksitesList.Add(this);
        GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
        subscribedToUpdate = true;
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
        PlaneExtension pe = workplace.GetExtension(); // создаем запросом, так как все равно понадобится

        if (pe.fullfillStatus != FullfillStatus.Empty) { // на поверхности есть какие-то структуры
            byte maxVolume = ScalableHarvestableResource.MAX_STICK_VOLUME;
            int i = 0;
            var strs = pe.GetStructuresList();
            
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
            var cpos = workplace.pos;
            switch (workplace.faceIndex)
            {
                case Block.FWD_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y, cpos.z + 1); break;
                case Block.RIGHT_FACE_INDEX: cpos = new ChunkPos(cpos.x + 1, cpos.y, cpos.z); break;
                case Block.BACK_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y, cpos.z - 1); break;
                case Block.LEFT_FACE_INDEX: cpos = new ChunkPos(cpos.x - 1, cpos.y, cpos.z); break;
                case Block.UP_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y + 1, cpos.z); break;
                case Block.DOWN_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y - 1, cpos.z + 1); break;
            }
            workplace.myChunk.AddBlock(cpos, rtype.ID, false, true);
            pe.ClearSurface(false, false, true);
            StopWork();
            return;
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
                            ScalableHarvestableResource.Create(rtype, resourceNeeded, workplace,
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
    }

    protected override void RecalculateWorkspeed()
    {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.BLOCK_BUILDING_SPEED;
        gearsDamage = GameConstants.WORKSITES_GEARS_DAMAGE_COEFFICIENT * workSpeed;
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
        if (workplace == null)
        {
            StopWork();
            return null;
        }
        var pos = workplace.pos;
        var data = new List<byte>() {
            (byte)WorksiteType.BlockBuildingSite,
            pos.x, pos.y, pos.z, workplace.faceIndex
        };
        data.AddRange(System.BitConverter.GetBytes(rtype.ID));
        data.AddRange(SerializeWorksite());        
        return data;
    }
    public static BlockBuildingSite Load(System.IO.FileStream fs, Chunk chunk)
    {
        var data = new byte[8];
        fs.Read(data, 0, data.Length);
        Plane plane = null;
        if (chunk.GetBlock(data[0], data[1], data[2])?.TryGetPlane(data[3], out plane) == true)
        {
            int x = System.BitConverter.ToInt32(data, 4);
            var cs = new BlockBuildingSite(plane, ResourceType.GetResourceTypeById(x));
            cs.LoadWorksiteData(fs);
            return cs;
        }
        else
        {
            Debug.Log("block building site load error");
            return null;
        }
    }
    #endregion
}
