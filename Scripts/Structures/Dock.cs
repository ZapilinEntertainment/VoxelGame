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
	private const float LOADING_TIME = 20f;
    private float loadingTimer = 0f, availableVolume = 0f;    
	private int preparingResourceIndex;
    private Ship loadingShip;
    private List<Block> dependentBlocksList;

    public const int SMALL_SHIPS_PATH_WIDTH = 2, MEDIUM_SHIPS_PATH_WIDTH = 3, HEAVY_SHIPS_PATH_WIDTH = 4;
    private const float WORK_PER_WORKER = 10f;

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
        if (!GameMaster.loading)
        {
            if (r != modelRotation) shipArrivingTimer = GameConstants.GetShipArrivingTimer();
        }
        modelRotation = (byte)r;
        var model = transform.childCount > 0 ? transform.GetChild(0) : null;
        if (basement != null && model != null)
        {
            model.transform.localRotation = Quaternion.Euler(0f, 45f * modelRotation, 0f);
            CheckPositionCorrectness();
        }
    }

    override public void SetBasement(Plane b, PixelPosByte pos) {
		if (b == null) return;
        var loading = GameMaster.loading;
        if (!loading)
        {            
            Chunk c = b.myChunk;
            if (c.GetBlock(b.pos.OneBlockForward()) != null)
            {
                if (c.GetBlock(b.pos.OneBlockRight()) == null) modelRotation = 2;
                else
                {
                    if (c.GetBlock(b.pos.OneBlockBack()) == null) modelRotation = 4;
                    else
                    {
                        if (c.GetBlock(b.pos.OneBlockLeft()) == null) modelRotation = 6;
                    }
                }
            }            
        }
        SetWorkbuildingData(b, pos);	
        colony.AddDock(this);		
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }        
        dependentBlocksList = new List<Block>();        
        if (!loading)
        {
            basement.ChangeMaterial(ResourceType.CONCRETE_ID, true);
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
        SetModelRotation(modelRotation);
        CheckAddons();
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

    public override float GetWorkSpeed()
    {
        return shipArrivingTimer;
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
        if (GameMaster.loading) return;
        // #checkPositionCorrectness - Dock
        if (dependentBlocksList != null)
        {
            if (dependentBlocksList.Count > 0)
            {
                basement.myChunk.ClearBlocksList(this, dependentBlocksList, true);
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

        if (basement.faceIndex == Block.SURFACE_FACE_INDEX)
        {
            switch (modelRotation)
            {
                case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z + 1, basement.pos.y - corridorWidth / 2, false, corridorWidth, this, ref dependentBlocksList); break;
                case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x + 1, basement.pos.y - corridorWidth / 2, true, corridorWidth, this, ref dependentBlocksList); break;
                case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z - corridorWidth, basement.pos.y - corridorWidth / 2, false, corridorWidth, this, ref dependentBlocksList); break;
                case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x - corridorWidth, basement.pos.y - corridorWidth / 2, true, corridorWidth, this, ref dependentBlocksList); break;
            }
        }
        else
        {
            switch (modelRotation)
            {
                case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z + 1, basement.pos.y - corridorWidth / 2 + 1, false, corridorWidth, this, ref dependentBlocksList); break;
                case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x + 1, basement.pos.y - corridorWidth / 2 + 1, true, corridorWidth, this, ref dependentBlocksList); break;
                case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z - corridorWidth, basement.pos.y - corridorWidth / 2 + 1, false, corridorWidth, this, ref dependentBlocksList); break;
                case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x - corridorWidth, basement.pos.y - corridorWidth / 2 + 1, true, corridorWidth, this, ref dependentBlocksList); break;
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
                //#incorrectLocationDisplaying
                switch (modelRotation)
                {
                    case 0:
                        PoolMaster.current.DrawZone(
                    new Vector3(Chunk.chunkSize / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.chunkSize, corridorWidth, corridorWidth),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x + (0.5f + corridorWidth / 2f), transform.position.y, Chunk.chunkSize / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.chunkSize),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        PoolMaster.current.DrawZone(
                        new Vector3(Chunk.chunkSize / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z - (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                        new Vector3(Chunk.chunkSize, corridorWidth, corridorWidth),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x -
                        (0.5f + corridorWidth / 2f), transform.position.y, Chunk.chunkSize / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.chunkSize),
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
                    new Vector3(Chunk.chunkSize / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z + (0.5f + corridorWidth/ 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.chunkSize, corridorWidth, corridorWidth),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x + (0.5f + corridorWidth / 2f), transform.position.y, Chunk.chunkSize / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.chunkSize),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        PoolMaster.current.DrawZone(
                        new Vector3(Chunk.chunkSize / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z - (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                        new Vector3(Chunk.chunkSize, corridorWidth, corridorWidth),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.pos.ToWorldSpace().x -
                        (0.5f + corridorWidth / 2f), transform.position.y, Chunk.chunkSize / 2f * Block.QUAD_SIZE),
                        new Vector3(corridorWidth, corridorWidth, Chunk.chunkSize),
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

    /// <summary>
    /// returns true if need to be upgraded
    /// </summary>
    /// <returns></returns>
    public bool CheckAddons()
    {
        var alist = FindObjectsOfType<DockAddon>();
        bool haveAddon1 = false, haveAddon2 = false;
        if (alist == null) return false;
        else
        {
            ChunkPos cpos = basement.pos, apos;
            int xdelta, zdelta;
            foreach (var addon in alist)
            {
                apos = addon.GetBlockPosition();
                if (apos.y == cpos.y)
                {
                    xdelta = Mathf.Abs(apos.x - cpos.x);
                    zdelta = Mathf.Abs(apos.z - cpos.z);
                    if ( (xdelta == 1 & zdelta == 0) || (xdelta == 0 & zdelta == 1) )
                    {
                        if (addon.level == 1) haveAddon1 = true; else haveAddon2 = true;
                    }
                }
            }
            byte newDockLevel = level;
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
                        d.SetBasement(basement, PixelPosByte.zero);
                        colony.SendWorkers(wCount, d);
                        return true;
                    case 2:
                        d = GetStructureByID(DOCK_2_ID) as Dock;
                        d.SetBasement(basement, PixelPosByte.zero);
                        d.SetModelRotation(modelRotation);
                        colony.SendWorkers(wCount, d);
                        return true;
                    case 3:
                        d = GetStructureByID(DOCK_3_ID) as Dock;
                        d.SetBasement(basement, PixelPosByte.zero);
                        d.SetModelRotation(modelRotation);
                        colony.SendWorkers(wCount, d);
                        return true;
                    default: return false;
                }
            }
            else return false;
        }
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
        availableVolume = workersCount * WORK_PER_WORKER;
        DockSystem.GetCurrent().HandleShip(this, s, colony);
        loadingShip = null;
        maintainingShip = false;
        s.Undock();

        shipArrivingTimer = GameConstants.GetShipArrivingTimer();     
    }
    public void SellResource(ResourceType rt, float volume)
    {
        if (availableVolume <= 0f) return;
        var colony = GameMaster.realMaster.colonyController;
        if (volume > availableVolume) volume = availableVolume;
        float vol = colony.storage.GetResources(rt, volume);
        float money = vol * ResourceType.prices[rt.ID] * GameMaster.sellPriceCoefficient;
        colony.AddEnergyCrystals(money);
        colony.gears_coefficient -= gearsDamage * vol;
        availableVolume -= vol;
        GameLogUI.MakeAnnouncement(Localization.GetSellMsg(rt, vol, money));
    }
    public float BuyResource(ResourceType rt, float volume)
    {
        if (availableVolume <= 0f) return 0f;
        if (volume > availableVolume) volume = availableVolume;
        var colony = GameMaster.realMaster.colonyController;
        float p = ResourceType.prices[rt.ID], money = 0f;
        if (p != 0)
        {
            money = colony.GetEnergyCrystals(volume * ResourceType.prices[rt.ID]);
            volume = money / ResourceType.prices[rt.ID];            
            if (volume == 0) return 0f;
            else GameLogUI.MakeAnnouncement(Localization.GetBuyMsg(rt, volume, money));
        }
        colony.storage.AddResource(rt, volume);
        colony.gears_coefficient -= gearsDamage * volume;
        availableVolume -= volume;
        return volume;
    }  


    override public void FreeWorkers(int x)
    {
        base.FreeWorkers(x);
        if (workersCount == 0 && maintainingShip && loadingShip != null) loadingShip.Undock();
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
                new Vector3(Chunk.chunkSize / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z + (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                new Vector3(Chunk.chunkSize, corridorWidth, corridorWidth),
                new Color(1, 0.076f, 0.076f, 0.4f)
                );
                    break;
                case 2:
                    PoolMaster.current.DrawZone(
                    new Vector3(basement.pos.ToWorldSpace().x + (0.5f + corridorWidth / 2f), transform.position.y, Chunk.chunkSize / 2f * Block.QUAD_SIZE),
                    new Vector3(corridorWidth, corridorWidth, Chunk.chunkSize),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 4:
                    PoolMaster.current.DrawZone(
                    new Vector3(Chunk.chunkSize / 2f * Block.QUAD_SIZE, transform.position.y, basement.pos.ToWorldSpace().z - (0.5f + corridorWidth / 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.chunkSize, corridorWidth, corridorWidth),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 6:
                    PoolMaster.current.DrawZone(
                    new Vector3(basement.pos.ToWorldSpace().x -
                    (0.5f + corridorWidth / 2f), transform.position.y, Chunk.chunkSize / 2f * Block.QUAD_SIZE),
                    new Vector3(corridorWidth, corridorWidth, Chunk.chunkSize),
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
            basement.myChunk.ClearBlocksList(this, dependentBlocksList, true);
            dependentBlocksList.Clear();            
        }
        if (showOnGUI & correctLocation) PoolMaster.current.DisableFlightZone();
        if (!clearFromSurface) { basement = null; }
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
        SetModelRotation(modelRotation);
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
