using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Dock : WorkBuilding {
    public static UIDockObserver dockObserver;
    private static bool announceNewShips = false;
    private static DockSystem dockSystem;

	public bool maintainingShip{get; private set;}
    public bool correctLocation { get; private set; }
    public float shipArrivingTimer { get; private set; }

    private bool subscribedToRestoreBlockersEvent = false;
	private const float LOADING_TIME = 10;
    private float loadingTimer = 0;    
	private int preparingResourceIndex;
    private Ship loadingShip;
    private List<Block> dependentBlocksList;

    public const int SMALL_SHIPS_PATH_WIDTH = 2, MEDIUM_SHIPS_PATH_WIDTH = 3, HEAVY_SHIPS_PATH_WIDTH = 4;

    static Dock()
    {
        AddToResetList(typeof(Dock));
    }

	override public void Prepare() {
		PrepareWorkbuilding();
        if (dockSystem == null) dockSystem = DockSystem.GetCurrent();
	}

    override public void SetModelRotation(int r)
    {
        if (r > 7) r %= 8;
        else
        {
            if (r < 0) r += 8;
        }
        if (r == modelRotation) return;
        else shipArrivingTimer = GameConstants.GetShipArrivingTimer();
        modelRotation = (byte)r;
        if ( basement != null)
        {
            transform.localRotation = Quaternion.Euler(0, modelRotation * 45, 0);
            CheckPositionCorrectness();
        }
    }

    override public void SetBasement(Plane b, PixelPosByte pos) {
		if (b == null) return;
        if (!GameMaster.loading)
        {
            if (CheckAddons(b)) return;
            Chunk c = b.myChunk;
            if (c.GetBlock(b.pos.x, b.pos.y, b.pos.z + 1) != null)
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
		basement.ReplaceMaterial(ResourceType.CONCRETE_ID);
		colony.AddDock(this);		
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }        
        dependentBlocksList = new List<Block>();
        if (!GameMaster.loading)
        {
            CheckPositionCorrectness();
            if (correctLocation) shipArrivingTimer = GameConstants.GetShipArrivingTimer();
        }
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

	override public void LabourUpdate () {
		if ( !isEnergySupplied ) return;
		if ( maintainingShip ) {
			if (loadingTimer > 0) {
					loadingTimer -= GameMaster.LABOUR_TICK;
					if (loadingTimer <= 0) {
						if (loadingShip != null) ShipLoading(loadingShip);
						loadingTimer = 0;
					}
			}
		}
		else {
			// ship arriving
			if (shipArrivingTimer > 0 & correctLocation) { 
				shipArrivingTimer -= GameMaster.LABOUR_TICK;
				if (shipArrivingTimer <= 0 ) {
					bool sendImmigrants = false, sendGoods = false;
					if ( dockSystem.immigrationEnabled  ) {
                        if (dockSystem.immigrationPlan > 0 & colony.totalLivespace >= colony.citizenCount | Random.value < colony.happiness_coefficient / 2f)
						sendImmigrants = true;
					}
					int transitionsCount = 0;
					for (int x = 0; x < dockSystem.isForSale.Length; x++) {
						if (dockSystem.isForSale[x] != null) transitionsCount++;
					}
					if (transitionsCount > 0) sendGoods = true;

					ShipType stype = ShipType.Cargo;
					if (sendImmigrants) {
						if (sendGoods) {
							if (Random.value > 0.55f ) stype = ShipType.Passenger;
						}
						else {
							if (Random.value < 0.05f) stype = ShipType.Private;
							else stype = ShipType.Passenger;
						}
					}
					else {
						if (sendGoods) {
							if (Random.value <= GameMaster.realMaster.warProximity) stype = ShipType.Military;
							else stype = ShipType.Cargo;
						}
						else {
							if (Random.value > 0.5f) {
								if (Random.value > 0.1f) stype = ShipType.Passenger;
								else stype = ShipType.Private;
							}
							else {
								if (Random.value > GameMaster.realMaster.warProximity) stype = ShipType.Cargo;
								else stype = ShipType.Military;
							}
						}
					}

					Ship s = PoolMaster.current.GetShip( level, stype );
                    if (s != null)
                    {
                        s.gameObject.SetActive(true);
                        if (s != null)
                        {
                            maintainingShip = true;
                            s.SetDestination(this);
                        }
                    }
                    shipArrivingTimer = GameConstants.GetShipArrivingTimer();
                }
			}
		}
	}

    public void CheckPositionCorrectness()
    {
        // #checkPositionCorrectness - Dock
        if (dependentBlocksList != null)
        {
            if (dependentBlocksList.Count > 0)
            {
                basement.myChunk.ClearBlocksList(dependentBlocksList, true);
                dependentBlocksList.Clear();
            }
        }
        else dependentBlocksList = new List<Block>();
        int corridorWidth = SMALL_SHIPS_PATH_WIDTH;
        if (level > 1)
        {
            if (level == 2) corridorWidth = MEDIUM_SHIPS_PATH_WIDTH;
            else corridorWidth = HEAVY_SHIPS_PATH_WIDTH;
        }

        switch (modelRotation)
        {
            case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z + 1, basement.pos.y - corridorWidth / 2, false, corridorWidth, this, ref dependentBlocksList); break;
            case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x + 1, basement.pos.y - corridorWidth / 2, true, corridorWidth, this, ref dependentBlocksList); break;
            case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z - corridorWidth, basement.pos.y - corridorWidth / 2, false, corridorWidth, this, ref dependentBlocksList); break;
            case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x - corridorWidth, basement.pos.y - corridorWidth / 2, true, corridorWidth, this, ref dependentBlocksList); break;
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
                //#incorrectLocationDisplaying
                switch (modelRotation)
                {
                    case 0:
                        PoolMaster.current.DrawZone(
                    new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.CHUNK_SIZE, corridorWidth, corridorWidth),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x + (0.5f + corridorWidth / 2f), transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.CHUNK_SIZE),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        PoolMaster.current.DrawZone(
                        new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z - (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                        new Vector3(Chunk.CHUNK_SIZE, corridorWidth, corridorWidth),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x -
                        (0.5f + corridorWidth / 2f), transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.CHUNK_SIZE),
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
    public override void SectionDeleted(ChunkPos pos)
    {
        if (correctLocation)
        {
            correctLocation = false;
            if (showOnGUI)
            {
                //#incorrectLocationDisplaying
                int corridorWidth = SMALL_SHIPS_PATH_WIDTH;
                if (level > 1)
                {
                    if (level == 2) corridorWidth = MEDIUM_SHIPS_PATH_WIDTH;
                    else corridorWidth = HEAVY_SHIPS_PATH_WIDTH;
                }
                switch (modelRotation)
                {
                    case 0:
                        PoolMaster.current.DrawZone(
                    new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z + (0.5f + corridorWidth/ 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.CHUNK_SIZE, corridorWidth, corridorWidth),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x + (0.5f + corridorWidth / 2f), transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.CHUNK_SIZE),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        PoolMaster.current.DrawZone(
                        new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z - (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                        new Vector3(Chunk.CHUNK_SIZE, corridorWidth, corridorWidth),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x -
                        (0.5f + corridorWidth / 2f), transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.CHUNK_SIZE),
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
    public bool CheckAddons(Plane sb)
    {
        Chunk c = sb.myChunk;
        int x = sb.pos.x, y = sb.pos.y, z = sb.pos.z;

        Block nearblock = c.GetBlock(x, y, z + 1);
        Plane nearSurfaceBlock = nearblock as Plane;
        DockAddon da;
        bool haveAddon1 = false, haveAddon2 = false;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            da = nearSurfaceBlock.structures[0] as DockAddon;
            if (da != null )
            {
                if (da.level == 1) haveAddon1 = true; else haveAddon2 = true;
            }
        }

        nearblock = c.GetBlock(x + 1, y, z);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            da = nearSurfaceBlock.structures[0] as DockAddon;
            if (da != null )
            {
                if (da.level == 1) haveAddon1 = true; else haveAddon2 = true;
            }
        }

        nearblock = c.GetBlock(x, y, z - 1);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            da = nearSurfaceBlock.structures[0] as DockAddon;
            if (da != null )
            {
                if (da.level == 1) haveAddon1 = true; else haveAddon2 = true;
            }
        }

        nearblock = c.GetBlock(x - 1, y, z);
        nearSurfaceBlock = nearblock as Plane;
        if (nearSurfaceBlock != null && nearSurfaceBlock.noEmptySpace != false)
        {
            da = nearSurfaceBlock.structures[0] as DockAddon;
            if (da != null )
            {
                if (da.level == 1) haveAddon1 = true; else haveAddon2 = true;
            }
        }

        byte newDockLevel;
        if (haveAddon1)
        {
            if (haveAddon2) newDockLevel = 3;
            else newDockLevel = 2;
        }
        else newDockLevel = 1;

        if (newDockLevel != level)
        {
            int wCount = workersCount;
            Dock d;
            switch (newDockLevel)
            {
                case 1:
                    FreeWorkers();
                    d = GetStructureByID(DOCK_ID) as Dock;
                    d.SetModelRotation(modelRotation);
                    d.SetBasement(sb, PixelPosByte.zero);
                    colony.SendWorkers(wCount, d);
                    return true;
                case 2:
                    d = GetStructureByID(DOCK_2_ID) as Dock;
                    d.SetBasement(sb, PixelPosByte.zero);
                    d.SetModelRotation(modelRotation);
                    colony.SendWorkers(wCount, d);
                    return true;
                case 3:
                    d = GetStructureByID(DOCK_3_ID) as Dock;
                    d.SetBasement(sb, PixelPosByte.zero);
                    d.SetModelRotation(modelRotation);
                    colony.SendWorkers(wCount, d);
                    return true;
                default: return false;
            }
        }
        else return false;
    }

    public void ShipLoading(Ship s)
    {
        if (loadingShip == null)
        {
            loadingTimer = LOADING_TIME;
            loadingShip = s;
            if (announceNewShips) GameLogUI.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.ShipArrived));
            return;
        }
        DockSystem.GetCurrent().HandleShip(this, s, colony);
        loadingShip = null;
        maintainingShip = false;
        s.Undock();

        shipArrivingTimer = GameConstants.GetShipArrivingTimer();     
    }
    public void SellResource(ResourceType rt, float volume)
    {
        var colony = GameMaster.realMaster.colonyController;
        float vol = colony.storage.GetResources(rt, volume);
        colony.AddEnergyCrystals(vol * ResourceType.prices[rt.ID] * GameMaster.sellPriceCoefficient);
        colony.gears_coefficient -= gearsDamage * vol;
    }
    public float BuyResource(ResourceType rt, float volume)
    {
        var colony = GameMaster.realMaster.colonyController;
        float p = ResourceType.prices[rt.ID];
        if (p != 0)
        {
            volume = colony.GetEnergyCrystals(volume * ResourceType.prices[rt.ID]) / ResourceType.prices[rt.ID];
            if (volume == 0) return 0;
        }
        colony.storage.AddResource(rt, volume);
        colony.gears_coefficient -= gearsDamage * volume;
        return volume;
    }

   

    override public void RecalculateWorkspeed()
    {
        workSpeed = (float)workersCount / (float)maxWorkers;
        if (workSpeed == 0)
        {
            if (maintainingShip & loadingShip != null) loadingShip.Undock();
        }
        gearsDamage = GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT / 10f * workSpeed;
    }

    public override UIObserver ShowOnGUI()
    {
        if (dockObserver == null) dockObserver = UIDockObserver.InitializeDockObserverScript();
        else dockObserver.gameObject.SetActive(true);
        dockObserver.SetObservingDock(this);
        showOnGUI = true;
        if (!correctLocation)
        {   //#incorrectLocationDisplaying
            int corridorWidth = SMALL_SHIPS_PATH_WIDTH;
            if (level > 1)
            {
                if (level == 2) corridorWidth = MEDIUM_SHIPS_PATH_WIDTH;
                else corridorWidth = HEAVY_SHIPS_PATH_WIDTH;
            }
            switch (modelRotation)
            {
                case 0:
                    PoolMaster.current.DrawZone(
                new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z + (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                new Vector3(Chunk.CHUNK_SIZE, corridorWidth, corridorWidth),
                new Color(1, 0.076f, 0.076f, 0.4f)
                );
                    break;
                case 2:
                    PoolMaster.current.DrawZone(
                    new Vector3(basement.pos.ToWorldSpace().x + (0.5f + corridorWidth / 2f), transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                    new Vector3(corridorWidth, corridorWidth, Chunk.CHUNK_SIZE),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 4:
                    PoolMaster.current.DrawZone(
                    new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z - (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.CHUNK_SIZE, corridorWidth, corridorWidth),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 6:
                    PoolMaster.current.DrawZone(
                    new Vector3(basement.pos.ToWorldSpace().x -
                    (0.5f + corridorWidth / 2f), transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                    new Vector3(corridorWidth, corridorWidth, Chunk.CHUNK_SIZE),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
            }
            //end
        }
        return dockObserver;
    }
    public override void DisableGUI()
    {
        if (showOnGUI)
        {
            showOnGUI = false;
            PoolMaster.current.DisableFlightZone();
            UIController.current.CloseTradePanel();
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
        if (showOnGUI & correctLocation) PoolMaster.current.DisableFlightZone();
        if (!clearFromSurface) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        colony.RemoveDock(this);
        if (maintainingShip & loadingShip != null) loadingShip.Undock();
        if (colony.docks.Count == 0 & dockObserver != null) Destroy(dockObserver);
        if (subscribedToRestoreBlockersEvent)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
        Destroy(gameObject);
    }

    #region save-load system
    public override List<byte> Save()
    {
        var data = SaveStructureData();
        data.AddRange(SaveBuildingData());
        data.AddRange(SaveWorkbuildingData());
        //dock data
        byte zero = 0, one = 1;
        data.Add(correctLocation ? one : zero);

        if (maintainingShip & loadingShip != null)
        {
            data.Add(one);
            data.AddRange(loadingShip.GetShipSerializer());
        }
        else data.Add(zero);
        data.AddRange(System.BitConverter.GetBytes(loadingTimer));
        data.AddRange(System.BitConverter.GetBytes(shipArrivingTimer));
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH + WORKBUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, sblock);
        LoadBuildingData(data, STRUCTURE_SERIALIZER_LENGTH);
        LoadWorkBuildingData(data, STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH);
        // load dock data
        correctLocation = fs.ReadByte() == 1;
        maintainingShip = fs.ReadByte() == 1;
        if (maintainingShip)
        {
            loadingShip = Ship.Load(fs, this);
        }
        data = new byte[8];
        fs.Read(data, 0, 8);
        loadingTimer = System.BitConverter.ToSingle(data, 0);
        shipArrivingTimer = System.BitConverter.ToSingle(data, 4);
    }
    #endregion
}
