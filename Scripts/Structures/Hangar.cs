using UnityEngine;
using System.Collections.Generic;

public sealed class Hangar : WorkBuilding
{
    public enum HangarStatus : byte { NoShuttle, ConstructingShuttle, ShuttleInside, ShuttleOnMission}
    public static List<Hangar> hangarsList { get; private set; }    
    public static int listChangesMarkerValue { get; private set; }
    private static int nextShuttleID = 0;

    public bool correctLocation { get; private set; }
    public HangarStatus status { get; private set; }
    public static UIHangarObserver hangarObserver;
    private bool subscribedToRestoreBlockersEvent = false;
    private int shuttleID = NO_SHUTTLE_VALUE;
    private List<Block> dependentBlocksList;

    public const int NO_SHUTTLE_VALUE = -1;

    static Hangar()
    {
        hangarsList = new List<Hangar>();
        AddToResetList(typeof(Hangar));
    }
    public static void ResetStaticData()
    {
        hangarsList = new List<Hangar>();
    }
    public static int GetFreeShuttlesCount()
    {
        if (hangarsList.Count == 0) return 0;
        else
        {
            int c = 0;
            foreach (var h in hangarsList)
            {
                if (h.status == HangarStatus.ShuttleInside) c++;
            }
            return c;
        }
    }
    public static int GetTotalShuttlesCount()
    {
        if (hangarsList.Count == 0) return 0;
        else
        {
            int c = 0;
            foreach (var h in hangarsList)
            {
                if (h.status == HangarStatus.ShuttleInside | h.status == HangarStatus.ShuttleOnMission) c++;
            }
            return c;
        }
    }
    public static int GetFreeShuttleID()
    {
        if (hangarsList.Count == 0) return NO_SHUTTLE_VALUE;
        else
        {
            foreach (var h in hangarsList)
            {
                if (h.shuttleID != NO_SHUTTLE_VALUE && h.status == HangarStatus.ShuttleInside)
                {
                    return h.shuttleID;
                }
            }
            return NO_SHUTTLE_VALUE;
        }
    }
    public static bool OccupyShuttle(int s_id)
    {
        if (hangarsList.Count > 0)
        {
            foreach (var h in hangarsList)
            {
                if (h.shuttleID == s_id)
                {
                    h.status = HangarStatus.ShuttleOnMission;
                    listChangesMarkerValue++;
                    return true;
                }
            }
        }
        return false;
    }
    public static void ReturnShuttle(int s_id)
    {
        if (s_id == NO_SHUTTLE_VALUE) return;
        if (hangarsList.Count > 0)
        {
            foreach (var h in hangarsList)
            {
                if (h.shuttleID == s_id)
                {
                    h.status = HangarStatus.ShuttleInside;
                    listChangesMarkerValue++;
                    return;
                }
            }
        }
    }

    override public void SetModelRotation(int r)
    {
        if (r > 7) r %= 8;
        else
        {
            if (r < 0) r += 8;
        }
        if (r == modelRotation) return;
        modelRotation = (byte)r;
        var model = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (basement != null && model != null)
        {
            model.transform.localRotation = Quaternion.Euler(0f, 45f * modelRotation, 0f);
            CheckPositionCorrectness();
        }
    }
    public override void SectionDeleted(ChunkPos cpos)
    {
        if (correctLocation)
        {
            correctLocation = false;
            if (showOnGUI)
            {
                //#incorrectLocationDisplaying - Hangar
                float len = 1;
                Vector3 pos = basement.pos.ToWorldSpace();
                switch (modelRotation)
                {
                    case 0:
                        len = basement.pos.z * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                    new Vector3(pos.x, pos.y, pos.z + Block.QUAD_SIZE / 2f + len / 2f),
                    new Vector3(1, 1, len),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        len = (Chunk.chunkSize - pos.x - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x + Block.QUAD_SIZE / 2f + len / 2f, pos.y, pos.z),
                        new Vector3(len, 1, 1),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        len = (Chunk.chunkSize - pos.z - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x, pos.y, pos.z - Block.QUAD_SIZE / 2f - len / 2f),
                        new Vector3(1, 1, len),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        len = basement.pos.x * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x - Block.QUAD_SIZE / 2f - len / 2f, pos.y, pos.z),
                        new Vector3(len, 1, 1),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                }
                //end
            }
            if (basement != null & dependentBlocksList != null && dependentBlocksList.Count != 0)
            {
                basement.myChunk.ClearBlocksList(this, dependentBlocksList, true);
                dependentBlocksList.Clear();
            }
        }
    }

    public void StartConstruction()
    {
        if (status == HangarStatus.NoShuttle)
        {
            status = HangarStatus.ConstructingShuttle;
            if (!subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
                subscribedToUpdate = true;
            }
        }
    }
    public void StopConstruction()
    {
        if (status == HangarStatus.ConstructingShuttle)
        {
            status = HangarStatus.NoShuttle;
            var cost = ResourcesCost.GetCost(ResourcesCost.SHUTTLE_BUILD_COST_ID);
            float pc = workflow / workComplexityCoefficient;
            for (int i= 0; i< cost.Length; i++)
            {
                cost[i] = cost[i].ChangeVolumeToPercent(1f - pc);
            }
            colony.storage.AddResources(cost);
            workflow = 0f;
            if (subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
                subscribedToUpdate = false;
            }
        }
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        if (!GameMaster.loading)
        {
            Chunk c = b.myChunk;
            if (c.GetBlock(b.pos.x, b.pos.y, b.pos.z + 1) == null) modelRotation = 0;
            else
            {
                if (c.GetBlock(b.pos.x + 1, b.pos.y, b.pos.z) == null) modelRotation = 2;
                else
                {
                    if (c.GetBlock(b.pos.x, b.pos.y, b.pos.z - 1) == null) modelRotation = 4;
                    else
                    {
                        if (c.GetBlock(b.pos.x - 1, b.pos.y, b.pos.z) == null) modelRotation = 6;
                    }
                }
            }
            if (!hangarsList.Contains(this))
            {
                status = HangarStatus.NoShuttle;
                shuttleID = NO_SHUTTLE_VALUE;
                hangarsList.Add(this);
            }
        }
        else
        {
            if (!hangarsList.Contains(this)) hangarsList.Add(this);
        }
        SetWorkbuildingData(b, pos);
        if (!GameMaster.loading) CheckPositionCorrectness();
        else
        {
            if (!subscribedToRestoreBlockersEvent)
            {
                GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                subscribedToRestoreBlockersEvent = true;
            }
        }               
    }
    public void RestoreBlockers()
    {
        // установка блокираторов после загрузки
        if (subscribedToRestoreBlockersEvent & correctLocation)
        {
            CheckPositionCorrectness();
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
    }

    private void CheckPositionCorrectness()
    {
        // #checkPositionCorrectness - Hangar
        if (dependentBlocksList != null)
        {
            if (dependentBlocksList.Count > 0)
            {
                basement.myChunk.ClearBlocksList(this, dependentBlocksList, true);
                dependentBlocksList.Clear();
            }
        }
        else dependentBlocksList = new List<Block>();
        if (basement.faceIndex == Block.SURFACE_FACE_INDEX)
        {
            switch (modelRotation)
            {
                case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y, basement.pos.z + 1), 0, 1, this, ref dependentBlocksList); break;
                case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x + 1, basement.pos.y, basement.pos.z), 2, 1, this, ref dependentBlocksList); break;
                case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y, basement.pos.z - 1), 4, 1, this, ref dependentBlocksList); break;
                case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x - 1, basement.pos.y, basement.pos.z), 6, 1, this, ref dependentBlocksList); break;
            }
        }
        else
        {
            switch (modelRotation)
            {
                case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y + 1, basement.pos.z + 1), 0, 1, this, ref dependentBlocksList); break;
                case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x + 1, basement.pos.y + 1, basement.pos.z), 2, 1, this, ref dependentBlocksList); break;
                case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y + 1, basement.pos.z - 1), 4, 1, this, ref dependentBlocksList); break;
                case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x - 1, basement.pos.y + 1, basement.pos.z), 6, 1, this, ref dependentBlocksList); break;
            }
        }
        if (!subscribedToChunkUpdate)
        {
            basement.myChunk.ChunkUpdateEvent += ChunkUpdated;
            subscribedToChunkUpdate = true;
        }
        if (showOnGUI)
        {
            if (!correctLocation)
            {
                //#incorrectLocationDisplaying - Hangar
                float len = 1;
                Vector3 pos = basement.pos.ToWorldSpace();
                switch (modelRotation)
                {
                    case 0:
                        len = (Chunk.chunkSize - pos.z - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                    new Vector3(pos.x, pos.y, pos.z + Block.QUAD_SIZE / 2f + len / 2f),
                    new Vector3(1, 1, len),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        len = (Chunk.chunkSize - pos.x - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x + Block.QUAD_SIZE / 2f + len / 2f, pos.y, pos.z),
                        new Vector3(len, 1, 1),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        len = basement.pos.z * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x, pos.y, pos.z - Block.QUAD_SIZE / 2f - len / 2f),
                        new Vector3(1, 1, len),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        len = basement.pos.x * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x - Block.QUAD_SIZE / 2f - len / 2f, pos.y, pos.z),
                        new Vector3(len, 1, 1),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                }
                //end
            }
            else PoolMaster.current.DisableFlightZone();
        }
        // end
    }
    override public void ChunkUpdated()
    {
        if (correctLocation) return;
        else CheckPositionCorrectness();
    }

    public override void LabourUpdate()
    {
        if (isActive & isEnergySupplied)
        {
            if (status == HangarStatus.ConstructingShuttle)
            {
                float work = GetLabourCoefficient();
                workflow += work;
                colony.gears_coefficient -= gearsDamage * work;
                if (workflow >= 1f)
                {
                    LabourResult((int)workflow);
                }
            }
        }
    }
    override protected void LabourResult(int iterations)
    {
        if (iterations < 1) return;
        shuttleID = nextShuttleID++;
        status = HangarStatus.ShuttleInside;
        if (workersCount > 0) FreeWorkers();
        workflow = 0f;
        listChangesMarkerValue++;
        if (showOnGUI)
        {
            hangarObserver.PrepareHangarWindow();
        }
        AnnouncementCanvasController.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ShuttleConstructed));
    }

    public override UIObserver ShowOnGUI()
    {
        if (hangarObserver == null) hangarObserver = UIHangarObserver.InitializeHangarObserverScript();
        else hangarObserver.gameObject.SetActive(true);
        hangarObserver.SetObservingHangar(this);
        showOnGUI = true;
        if (!correctLocation)
        {
            //#incorrectLocationDisplaying - Hangar
            float len = 1;
            Vector3 pos = basement.pos.ToWorldSpace();
            switch (modelRotation)
            {
                case 0:
                    len = basement.pos.z * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                new Vector3(pos.x, pos.y, pos.z + Block.QUAD_SIZE / 2f + len / 2f),
                new Vector3(1, 1, len),
                new Color(1, 0.076f, 0.076f, 0.4f)
                );
                    break;
                case 2:
                    len = (Chunk.chunkSize - pos.x - 1) * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(pos.x + Block.QUAD_SIZE / 2f + len / 2f, pos.y, pos.z),
                    new Vector3(len, 1, 1),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 4:
                    len = (Chunk.chunkSize - pos.z - 1) * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(pos.x, pos.y, pos.z - Block.QUAD_SIZE / 2f - len / 2f),
                    new Vector3(1, 1, len),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 6:
                    len = basement.pos.x * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(pos.x - Block.QUAD_SIZE / 2f - len / 2f, pos.y, pos.z),
                    new Vector3(len, 1, 1),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
            }
            //end
        }
        return hangarObserver;
    }
    public override void DisableGUI()
    {
        if (showOnGUI)
        {
            showOnGUI = false;
            PoolMaster.current.DisableFlightZone();
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (basement != null & dependentBlocksList != null && dependentBlocksList.Count != 0)
        {
            basement.myChunk.ClearBlocksList(this, dependentBlocksList, true);
            dependentBlocksList.Clear();
        }
        if (showOnGUI & !correctLocation) PoolMaster.current.DisableFlightZone();
        if (!clearFromSurface) basement = null;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (hangarsList.Contains(this)) hangarsList.Remove(this);

        if (hangarsList.Count == 0 & hangarObserver != null) Destroy(hangarObserver);
        if (subscribedToRestoreBlockersEvent)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
        listChangesMarkerValue++;
        Destroy(gameObject);
    }

    public void FORCED_MakeShuttle()
    {
        if (status == HangarStatus.NoShuttle) LabourResult(1);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(SaveHangarData());
        return data;
    }
    private List<byte> SaveHangarData()
    {
        var data =  new List<byte>() {correctLocation ? (byte)1 : (byte)0, (byte)status};
        data.AddRange(System.BitConverter.GetBytes(shuttleID));
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH + WORKBUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, sblock);
        LoadBuildingData(data, STRUCTURE_SERIALIZER_LENGTH);
        SetModelRotation(modelRotation);
        var hdata = new byte[6];
        fs.Read(hdata, 0, hdata.Length);
        correctLocation = hdata[0] == 1;
        status = (HangarStatus)hdata[1];
        if (status == HangarStatus.ShuttleInside || status == HangarStatus.ShuttleOnMission)
        {
            shuttleID = System.BitConverter.ToInt32(hdata, 2);
        }
        else shuttleID = NO_SHUTTLE_VALUE;

        if (status == HangarStatus.ConstructingShuttle & !subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }

        LoadWorkBuildingData(data,STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH);
        if (shuttleID >= nextShuttleID) nextShuttleID = shuttleID + 1;
    }  
    #endregion
}
