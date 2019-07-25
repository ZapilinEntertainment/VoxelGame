using UnityEngine;
using System.Collections.Generic;

public sealed class Hangar : WorkBuilding
{
    public static List<Hangar> hangarsList;

    public Shuttle shuttle { get; private set; }
    const float CREW_HIRE_BASE_COST = 100;
    public bool constructing { get; private set; }
    public bool correctLocation { get; private set; }
    public static UIHangarObserver hangarObserver;
    private bool subscribedToRestoreBlockersEvent = false;
    private List<Block> dependentBlocksList;

    public const float BUILD_SHUTTLE_WORKFLOW = 12000;

    static Hangar()
    {
        hangarsList = new List<Hangar>();
        AddToResetList(typeof(Hangar));
    }
    public static void ResetStaticData()
    {
        hangarsList = new List<Hangar>();
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
        if (basement != null)
        {
            transform.localRotation = Quaternion.Euler(0, modelRotation * 45, 0);
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
                        len = (Chunk.CHUNK_SIZE - pos.x - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(pos.x + Block.QUAD_SIZE / 2f + len / 2f, pos.y, pos.z),
                        new Vector3(len, 1, 1),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        len = (Chunk.CHUNK_SIZE - pos.z - 1) * Block.QUAD_SIZE;
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
                basement.myChunk.ClearBlocksList(dependentBlocksList, true);
                dependentBlocksList.Clear();
            }
        }
    }

    public void StartConstruction()
    {
        if (!constructing)
        {
            constructing = true;
            if (!subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
                subscribedToUpdate = true;
            }
        }
    }
    public void StopConstruction()
    {
        if (constructing)
        {
            constructing = false;
            if (subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
                subscribedToUpdate = false;
            }
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
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
        if (!hangarsList.Contains(this)) hangarsList.Add(this);
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
                basement.myChunk.ClearBlocksList(dependentBlocksList, true);
                dependentBlocksList.Clear();
            }
        }
        else dependentBlocksList = new List<Block>();
        switch (modelRotation)
        {
            case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y, basement.pos.z + 1), 0, 1, this, ref dependentBlocksList); break;
            case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x + 1, basement.pos.y, basement.pos.z), 2, 1, this, ref dependentBlocksList); break;
            case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y, basement.pos.z - 1), 4, 1, this, ref dependentBlocksList); break;
            case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x - 1, basement.pos.y, basement.pos.z), 6, 1, this, ref dependentBlocksList); break;
        }
        if (correctLocation)
        {
            if (subscribedToChunkUpdate)
            {
                basement.myChunk.ChunkUpdateEvent -= ChunkUpdated;
                subscribedToChunkUpdate = false;
            }
        }
        else
        {
            if (!subscribedToChunkUpdate)
            {
                basement.myChunk.ChunkUpdateEvent += ChunkUpdated;
                subscribedToChunkUpdate = true;
            }
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
                        len = (Chunk.CHUNK_SIZE - pos.z - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                    new Vector3(pos.x, pos.y, pos.z + Block.QUAD_SIZE / 2f + len / 2f),
                    new Vector3(1, 1, len),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        len = (Chunk.CHUNK_SIZE - pos.x - 1) * Block.QUAD_SIZE;
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
            if (constructing)
            {
                workflow += workSpeed;
                colony.gears_coefficient -= gearsDamage;
                if (workflow >= workflowToProcess)
                {
                    LabourResult();
                }
            }
            else
            {
                if (shuttle != null && (shuttle.docked & shuttle.condition < 1))
                {
                    shuttle.condition += workSpeed / 4f;
                    if (shuttle.condition > 1) shuttle.condition = 1;
                }
            }
        }
    }

    override protected void LabourResult()
    {
        shuttle = Instantiate(Resources.Load<GameObject>("Prefs/shuttle"), transform).GetComponent<Shuttle>();
        shuttle.FirstSet(this);
        constructing = false;
        workflow -= workflowToProcess;
        if (showOnGUI)
        {
            hangarObserver.PrepareHangarWindow();
        }
        GameLogUI.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ShuttleConstructed));
    }

    override public void RecalculateWorkspeed()
    {
        workSpeed = colony.labourCoefficient * workersCount * GameConstants.MACHINE_CONSTRUCTING_SPEED;
        gearsDamage = GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT * workSpeed / 2f;
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
                    len = (Chunk.CHUNK_SIZE - pos.x - 1) * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(pos.x + Block.QUAD_SIZE / 2f + len / 2f, pos.y, pos.z),
                    new Vector3(len, 1, 1),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 4:
                    len = (Chunk.CHUNK_SIZE - pos.z - 1) * Block.QUAD_SIZE;
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

    /// <summary>
    /// TEST USE ONLY
    /// </summary>
    /// <param name="s"></param>
    public void AssignShuttle(Shuttle s)
    {
        shuttle = s;
        constructing = false;
    }
    public void DeconstructShuttle()
    {
        if (shuttle == null) return;
        else
        {
            shuttle.Deconstruct();
            DropShuttle();
        }
    }
    private void DropShuttle()
    {
        shuttle = null;
        hangarObserver.PrepareHangarWindow();
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
    }  

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        if (basement != null & dependentBlocksList != null && dependentBlocksList.Count != 0)
        {
            basement.myChunk.ClearBlocksList(dependentBlocksList, true);
            dependentBlocksList.Clear();
        }
        if (showOnGUI & !correctLocation) PoolMaster.current.DisableFlightZone();
        if (!clearFromSurface) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        if (hangarsList.Contains(this)) hangarsList.Remove(this);
        if (shuttle != null) shuttle.Deconstruct();

        if (hangarsList.Count == 0 & hangarObserver != null) Destroy(hangarObserver);
        if (subscribedToRestoreBlockersEvent)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(SaveHangarDara());
        return data;
    }
    private List<byte> SaveHangarDara()
    {
        int shuttleIndex = -1;
        if (shuttle != null) shuttleIndex = shuttle.ID;
        byte truebyte = 1, falsebyte = 0;
        var data =  new List<byte>() {   constructing ? truebyte : falsebyte, correctLocation ? truebyte : falsebyte}; // 0 , 1
        data.AddRange(System.BitConverter.GetBytes(shuttleIndex)); // 2 - 5
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH + WORKBUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, sblock);
        LoadBuildingData(data, STRUCTURE_SERIALIZER_LENGTH);
        constructing = fs.ReadByte() == 1;
        correctLocation = fs.ReadByte() == 1;
        if (constructing & !subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        LoadWorkBuildingData(data,STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH);
        data = new byte[4];
        fs.Read(data, 0, 4);
        int shuttleID = System.BitConverter.ToInt32(data, 0);
        if (shuttleID != -1)
        {
            shuttle = Shuttle.GetShuttle(shuttleID);
            shuttle.AssignToHangar(this);
            if (shuttle.docked)
            {
                shuttle.transform.parent = transform;
                shuttle.SetVisibility(false);
            }
        }
        else shuttle = null;
    }  
    #endregion
}
