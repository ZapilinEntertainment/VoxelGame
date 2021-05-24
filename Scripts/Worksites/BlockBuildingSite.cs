using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingSite : Worksite
{
    private ResourceType rtype;
    private const int START_WORKERS_COUNT = 20;

    public BlockBuildingSite(Plane p, ResourceType i_type) : base(p)
    {
        rtype = i_type;
        actionLabel = Localization.GetStructureName(Structure.RESOURCE_STICK_ID);
        sign = new GameObject("Block Building Site sign").AddComponent<WorksiteSign>();
        BoxCollider bc = sign.gameObject.AddComponent<BoxCollider>();     
        bc.tag = WORKSITE_SIGN_COLLIDER_TAG;
        sign.worksite = this;
        switch (p.faceIndex)
        {
            case Block.FWD_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.forward * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(1f,1f,0.5f) * Block.QUAD_SIZE;
                break;
            case Block.RIGHT_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.right * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(0.5f, 1f, 1f) * Block.QUAD_SIZE;
                break;
            case Block.BACK_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.back * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(1f, 1f, 0.5f) * Block.QUAD_SIZE;
                break;
            case Block.LEFT_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.left * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(0.5f, 1f, 0.5f) * Block.QUAD_SIZE;
                break;
            case Block.UP_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(1f, 0.5f, 1f) * Block.QUAD_SIZE;
                break;
            case Block.DOWN_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(1f, 0.5f, 1f) * Block.QUAD_SIZE;
                break;
            case Block.SURFACE_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.up * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(1f, 0.5f, 1f) * Block.QUAD_SIZE;
                break;
            case Block.CEILING_FACE_INDEX:
                sign.transform.position = workplace.pos.ToWorldSpace() + Vector3.down * Block.QUAD_SIZE * 0.75f;
                bc.size = new Vector3(1f, 0.5f, 1f) * Block.QUAD_SIZE;
                break;
        }
        maxWorkersCount = 400;
        colony.SendWorkers(START_WORKERS_COUNT, this);
        gearsDamage = GameConstants.GEARS_DAMAGE_COEFFICIENT * 1.2f;
        workComplexityCoefficient = GameConstants.GetWorkComplexityCf(WorkType.BlockBuilding);
    }

    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        workflow -= iterations;
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
        PlaneExtension pe = workplace.FORCED_GetExtension(); // создаем запросом, так как все равно понадобится

        while (iterations > 0)
        {
            iterations--;
            if (pe.fulfillStatus != FullfillStatus.Empty)
            { // на поверхности есть какие-то структуры
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
                            s.Annihilate(StructureAnnihilationOrder.GrindedByWorksite);
                            return;
                        }
                        else
                        {
                            if (s.ID == Structure.PLANT_ID)
                            {
                                (s as Plant).Harvest(false);
                            }
                            else s.Annihilate(StructureAnnihilationOrder.GrindedByWorksite);
                            deletedStructures++;
                            if (deletedStructures >= 4) break; // не больше 4-х удалений за тик
                        }
                    }
                    i++;
                }
            }
            shr = null;

            if (finishedPillarsCount == maxPillarsCount)
            {
                if (iterations == 0) actionLabel = Localization.GetActionLabel(LocalizationActionLabels.BlockCompleted);
                var cpos = workplace.pos;
                switch (workplace.faceIndex)
                {
                    case Block.FWD_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y, cpos.z + 1); break;
                    case Block.RIGHT_FACE_INDEX: cpos = new ChunkPos(cpos.x + 1, cpos.y, cpos.z); break;
                    case Block.BACK_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y, cpos.z - 1); break;
                    case Block.LEFT_FACE_INDEX: cpos = new ChunkPos(cpos.x - 1, cpos.y, cpos.z); break;
                    case Block.UP_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y + 1, cpos.z); break;
                    case Block.DOWN_FACE_INDEX: cpos = new ChunkPos(cpos.x, cpos.y - 1, cpos.z); break;
                }
                workplace.myChunk.AddBlock(cpos, rtype.ID, false, true);
                pe.ClearSurface(PlaneAnnihilationOrder.BlockbuildingPartsReplacement);
                StopWork(true);
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
                        int epcount = emptyPositionsIndexes.Count;
                        if (epcount > 0)
                        {
                            int combinedIndex = emptyPositionsIndexes[Random.Range(0, epcount)];
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
                        shr = unfinishedPillarsList[Random.Range(0, unfinishedCount)];
                        resourceTaken = shr.AddResource(rtype, resourceTaken);
                        if (resourceTaken > 0) colony.storage.AddResource(rtype, resourceTaken);
                    }
                    else {
                        if (showOnGUI && iterations == 0) actionLabel = Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources);
                    }
                }
            }            
        }
    }

    #region save-load system
    override public void Save(System.IO.FileStream fs)
    {
        if (workplace == null)
        {
            StopWork(true);
            return;
        }
        var pos = workplace.pos;
        fs.WriteByte((byte)WorksiteType.BlockBuildingSite);
        fs.WriteByte(pos.x);
        fs.WriteByte(pos.y);
        fs.WriteByte(pos.z);
        fs.WriteByte(workplace.faceIndex);
        fs.Write(System.BitConverter.GetBytes(rtype.ID),0,4);
        SerializeWorksite(fs);
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
