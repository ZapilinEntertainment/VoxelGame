using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Dock : WorkBuilding {
	bool correctLocation = false;
	public static bool?[] isForSale{get; private set;}
	public static int[] minValueForTrading{get; private set;}
	ColonyController colony;
	public static int immigrationPlan {get; private set;} 
	public static bool immigrationEnabled{get; private set;}
	public bool maintainingShip{get; private set;}
	Ship loadingShip;
	const float LOADING_TIME = 10;
	float loadingTimer = 0, shipArrivingTimer = 0;
	const float SHIP_ARRIVING_TIME = 300;
	int preparingResourceIndex;
    public static UIDockObserver dockObserver;
    private List<Block> dependentBlocksList;
    const int SMALL_SHIPS_PATH_WIDTH = 2;

	public static void ResetToDefaults_Static_Dock() {
		isForSale = new bool?[ResourceType.RTYPES_COUNT];
		minValueForTrading= new int[ResourceType.RTYPES_COUNT];
		immigrationEnabled = true;
		immigrationPlan = 0;
	}

	override public void Prepare() {
		PrepareWorkbuilding();
		if (isForSale == null) {
			ResetToDefaults_Static_Dock();
		}
	}

    override public void SetModelRotation(int r)
    {
        if (r > 7) r %= 8;
        else
        {
            if (r < 0) r += 8;
        }
        modelRotation = (byte)r;
        if (transform.childCount != 0 & basement != null)
        {
            transform.localRotation = Quaternion.Euler(0, modelRotation * 45, 0);
            // #checkPositionCorrectness
            switch (modelRotation)
            {
                case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z + 1, basement.pos.y - 1, false, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
                case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x + 1, basement.pos.y - 1, true, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
                case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z - 2, basement.pos.y - 1, false, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
                case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x - 2, basement.pos.y - 1, true, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
            }
            if (showOnGUI) {
                if (!correctLocation)
                {
                    //#incorrectLocationDisplaying
                    switch (modelRotation)
                    {
                        case 0:
                            PoolMaster.current.DrawZone(
                        new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, basement.transform.position.y, basement.transform.position.z + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                        new Vector3(Chunk.CHUNK_SIZE, SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                            break;
                        case 2:
                            PoolMaster.current.DrawZone(
                            new Vector3(basement.transform.position.x + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f), basement.transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                            new Vector3(SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH, Chunk.CHUNK_SIZE),
                            new Color(1, 0.076f, 0.076f, 0.4f)
                            );
                            break;
                        case 4:
                            PoolMaster.current.DrawZone(
                            new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, basement.transform.position.y, basement.transform.position.z - (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                            new Vector3(Chunk.CHUNK_SIZE, SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH),
                            new Color(1, 0.076f, 0.076f, 0.4f)
                            );
                            break;
                        case 6:
                            PoolMaster.current.DrawZone(
                            new Vector3(basement.transform.position.x -
                            (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f), basement.transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                            new Vector3(SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH, Chunk.CHUNK_SIZE),
                            new Color(1, 0.076f, 0.076f, 0.4f)
                            );
                            break;
                    }
                    //end
                }
                else PoolMaster.current.DisableZone();
            }
            // end
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		Transform modelTransform = transform.GetChild(0);		
		basement.ReplaceMaterial(ResourceType.CONCRETE_ID);
		colony = GameMaster.colonyController;
		colony.AddDock(this);		
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
        if (!subscribedToChunkUpdate)
        {
            basement.myChunk.ChunkUpdateEvent += ChunkUpdated;
            subscribedToChunkUpdate = true;
        }
        dependentBlocksList = new List<Block>();
        // #checkPositionCorrectness
        switch (modelRotation)
        {
            case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible( basement.pos.z + 1, basement.pos.y - 1, false, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList ); break;
            case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x + 1, basement.pos.y - 1, true, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
            case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z - 2, basement.pos.y - 1, false, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
            case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x - 2, basement.pos.y - 1, true, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
        }
        // end
        if (correctLocation) shipArrivingTimer = SHIP_ARRIVING_TIME * GameMaster.tradeVesselsTrafficCoefficient * (1 - (colony.docksLevel * 2 / 100f)) / 2f;
    }

	override public void LabourUpdate () {
		if ( !energySupplied ) return;
		if ( maintainingShip ) {
			if (loadingTimer > 0) {
					loadingTimer -= (1 + workSpeed) * GameMaster.LABOUR_TICK;
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
					if ( immigrationPlan > 0  && immigrationEnabled ) {
						if (Random.value < 0.3f || colony.totalLivespace > colony.citizenCount) sendImmigrants = true;
					}
					int transitionsCount = 0;
					for (int x = 0; x < isForSale.Length; x++) {
						if (isForSale[x] != null) transitionsCount++;
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
							if (Random.value <= GameMaster.warProximity) stype = ShipType.Military;
							else stype = ShipType.Cargo;
						}
						else {
							if (Random.value > 0.5f) {
								if (Random.value > 0.1f) stype = ShipType.Passenger;
								else stype = ShipType.Private;
							}
							else {
								if (Random.value > GameMaster.warProximity) stype = ShipType.Cargo;
								else stype = ShipType.Military;
							}
						}
					}
					Ship s = PoolMaster.current.GetShip( level, stype );
					if ( s!= null ) {
						maintainingShip = true;
						s.SetDestination( this );
					}
					//else print ("error:no ship given");
				}
			}
		}
	}

    override public void ChunkUpdated(ChunkPos pos)
    { 
        if (basement == null | dependentBlocksList == null) return;
        // #checkPositionCorrectness
        switch (modelRotation)
        {
            case 0: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z + 1, basement.pos.y - 1, false, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
            case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x + 1, basement.pos.y - 1, true, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
            case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.z - 2, basement.pos.y - 1, false, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
            case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(basement.pos.x - 2, basement.pos.y - 1, true, SMALL_SHIPS_PATH_WIDTH, this, ref dependentBlocksList); break;
        }
        if (showOnGUI)
        {
            if (correctLocation) PoolMaster.current.DisableZone();
            else
            {
                //#incorrectLocationDisplaying
                switch (modelRotation)
                {
                    case 0:
                        PoolMaster.current.DrawZone(
                    new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, basement.transform.position.y, basement.transform.position.z + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.CHUNK_SIZE, SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.transform.position.x + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f), basement.transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                        new Vector3(SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH, Chunk.CHUNK_SIZE),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        PoolMaster.current.DrawZone(
                        new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, basement.transform.position.y, basement.transform.position.z - (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                        new Vector3(Chunk.CHUNK_SIZE, SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        PoolMaster.current.DrawZone(
                        new Vector3(basement.transform.position.x -
                        (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f), basement.transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                        new Vector3(SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH, Chunk.CHUNK_SIZE),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                }
                //end
            }
        }
        // end
    }

    public void ShipLoading(Ship s) {
		if (loadingShip == null) {
			loadingTimer = LOADING_TIME;
			loadingShip = s;
			return;
		}
		int peopleBefore = immigrationPlan;
		switch (s.type) {
		case ShipType.Passenger:
			if (immigrationPlan > 0) {
				if (s.volume > immigrationPlan) {GameMaster.colonyController.AddCitizens(immigrationPlan); immigrationPlan = 0;}
				else {GameMaster.colonyController.AddCitizens(s.volume); immigrationPlan -= s.volume;}
			}
			if (isForSale[ResourceType.FOOD_ID] != null) {
				if (isForSale[ResourceType.FOOD_ID] == true) SellResource(ResourceType.Food, s.volume * 0.1f);
				else BuyResource(ResourceType.Food, s.volume * 0.1f);
			}
			break;
		case ShipType.Cargo:
			float totalDemand= 0;
			List<int> buyPositions = new List<int>();
			for (int i = 0; i < ResourceType.RTYPES_COUNT; i ++) {
				if (isForSale[i] == null) continue;
				if (isForSale[i] == true) {
					totalDemand += ResourceType.demand[i];
				}
				else {
					if ( colony.storage.standartResources[i] <= minValueForTrading[i])	buyPositions.Add(i);
				}
			}
			if (totalDemand > 0) {
				float demandPiece = 1 / totalDemand;
				for (int i = 0; i < ResourceType.RTYPES_COUNT; i ++) {
					if (isForSale[i] == true) SellResource(ResourceType.resourceTypesArray[i], ResourceType.demand[i] * demandPiece * s.volume);
				}
			}
			if (buyPositions.Count > 0) {
				float v = s.volume;
				while (v > 0 && buyPositions.Count > 0) {
					int buyIndex = (int)(Random.value * buyPositions.Count - 1); // index in index arrays
					int i = buyPositions[buyIndex]; // real index
					float buyVolume = minValueForTrading[i] - colony.storage.standartResources[i]; 
					if (buyVolume < 0) buyVolume = 0;
					if (v < buyVolume) buyVolume = v;
					BuyResource(ResourceType.resourceTypesArray[i], buyVolume);
					v -= buyVolume;
					buyPositions.RemoveAt(buyIndex);
				}
			}
			break;
		case ShipType.Military:
			if (GameMaster.warProximity < 0.5f && Random.value < 0.1f && immigrationPlan > 0) {
				int veterans =(int)( s.volume * 0.02f );
				if (veterans > immigrationPlan) veterans = immigrationPlan;
				colony.AddCitizens(veterans);
			}
			if ( isForSale[ResourceType.FUEL_ID] == true) SellResource(ResourceType.Fuel, s.volume * 0.5f * (Random.value * 0.5f + 0.5f));
			if (GameMaster.warProximity > 0.5f) {
				if (isForSale[ResourceType.METAL_S_ID] == true) SellResource(ResourceType.metal_S, s.volume * 0.1f);
				if (isForSale[ResourceType.METAL_K_ID] == true) SellResource(ResourceType.metal_K, s.volume * 0.05f);
				if (isForSale[ResourceType.METAL_M_ID] == true) SellResource(ResourceType.metal_M, s.volume * 0.1f);
			}
			break;
		case ShipType.Private:
			if ( isForSale[ResourceType.FUEL_ID] == true) SellResource(ResourceType.Fuel, s.volume * 0.8f);
			if ( isForSale[ResourceType.FOOD_ID] == true) SellResource(ResourceType.Fuel, s.volume * 0.15f);
			break;
		}
		loadingShip = null;
		maintainingShip = false;
		s.Undock();

		shipArrivingTimer = SHIP_ARRIVING_TIME * GameMaster.tradeVesselsTrafficCoefficient * (1 - (colony.docksLevel * 2 / 100f))  ;
		float f = 1;
		if (colony.docks.Count != 0) f /= (float)colony.docks.Count;
		if ( f < 0.1f ) f = 0.1f;
		shipArrivingTimer /= f;

		int newPeople = peopleBefore - immigrationPlan;
		if (newPeople > 0) UIController.current.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ColonistsArrived) + " (" + newPeople.ToString() + ')');
	}
	private void SellResource(ResourceType rt, float volume) {
		float vol = colony.storage.GetResources(rt, volume);
		colony.AddEnergyCrystals(vol * ResourceType.prices[rt.ID] * GameMaster.sellPriceCoefficient);
	}
	private void BuyResource(ResourceType rt, float volume) {
		volume = colony.GetEnergyCrystals(volume * ResourceType.prices[rt.ID]) / ResourceType.prices[rt.ID];
		colony.storage.AddResource(rt, volume);
	}

	public static void SetImmigrationStatus ( bool x, int count) {
		immigrationEnabled = x;
		immigrationPlan = count;
	}
    public static void ChangeMinValue(int index, int val)
    {
        minValueForTrading[index] = val;
    }
    public static void ChangeSaleStatus(int index, bool? val)
    {
        if (isForSale[index] == val) return;
        else
        {
            isForSale[index] = val;
        }
    }

	#region save-load system
	public static DockStaticSerializer SaveStaticDockData() {
		DockStaticSerializer dss = new DockStaticSerializer();
		dss.isForSale = isForSale;
		dss.minValueForTrading = minValueForTrading;
		dss.prices = ResourceType.prices;
		dss.demand = ResourceType.demand;
		dss.immigrationPlan = immigrationPlan;
		dss.immigrationEnabled = immigrationEnabled;
		return dss;
	}

	public static void LoadStaticData( DockStaticSerializer dss) {
		isForSale = dss.isForSale;
		minValueForTrading = dss.minValueForTrading;
		ResourceType.prices = dss.prices;
		ResourceType.demand = dss.demand;
		immigrationPlan = dss.immigrationPlan;
		immigrationEnabled = dss.immigrationEnabled;
	}

	public override StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetDockSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}
	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		DockSerializer ds= new DockSerializer();
		GameMaster.DeserializeByteArray<DockSerializer>(ss.specificData, ref ds);
		LoadDockData(ds);
	}
	void LoadDockData(DockSerializer ds) {
		LoadWorkBuildingData(ds.workBuildingSerializer);
		correctLocation = ds.correctLocation;
		maintainingShip =ds.maintainingShip;
		if (maintainingShip) {
			ShipSerializer ss = ds.loadingShip;
			Ship s = PoolMaster.current.GetShip( ss.level, ss.type );
			s.Load(ss, this);
			loadingShip = s;
		}
		loadingTimer =  ds.loadingTimer;
		shipArrivingTimer = ds.shipArrivingTimer;
	} 

	DockSerializer GetDockSerializer() {
		DockSerializer ds = new DockSerializer();
		ds.workBuildingSerializer = GetWorkBuildingSerializer();
		ds.correctLocation = correctLocation;
		ds.maintainingShip = maintainingShip;
		if (maintainingShip && loadingShip != null) ds.loadingShip =  loadingShip.GetShipSerializer();
		ds.loadingTimer = loadingTimer;
		ds.shipArrivingTimer = shipArrivingTimer;
		return ds;
	}
    #endregion

    override protected void RecalculateWorkspeed()
    {
        workSpeed = (float)workersCount / (float)maxWorkers;
        if (workSpeed == 0)
        {
            if (maintainingShip & loadingShip != null) loadingShip.Undock();
        }
    }

    public override UIObserver ShowOnGUI()
    {
        if (dockObserver == null) dockObserver = UIDockObserver.InitializeDockObserverScript();
        else dockObserver.gameObject.SetActive(true);
        dockObserver.SetObservingDock(this);
        showOnGUI = true;
        if (!correctLocation)
        {   //#incorrectLocationDisplaying
            switch (modelRotation)
            {
                case 0:
                    PoolMaster.current.DrawZone(
                new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, basement.transform.position.y, basement.transform.position.z + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                new Vector3(Chunk.CHUNK_SIZE, SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH),
                new Color(1, 0.076f, 0.076f, 0.4f)
                );
                    break;
                case 2:
                    PoolMaster.current.DrawZone(
                    new Vector3(basement.transform.position.x + (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f), basement.transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                    new Vector3(SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH, Chunk.CHUNK_SIZE),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 4:
                    PoolMaster.current.DrawZone(
                    new Vector3(Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE, basement.transform.position.y, basement.transform.position.z - (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f) * Block.QUAD_SIZE),
                    new Vector3(Chunk.CHUNK_SIZE, SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 6:
                    PoolMaster.current.DrawZone(
                    new Vector3(basement.transform.position.x -
                    (0.5f + SMALL_SHIPS_PATH_WIDTH / 2f), basement.transform.position.y, Chunk.CHUNK_SIZE / 2f * Block.QUAD_SIZE),
                    new Vector3(SMALL_SHIPS_PATH_WIDTH, SMALL_SHIPS_PATH_WIDTH, Chunk.CHUNK_SIZE),
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
            PoolMaster.current.DisableZone();
        }
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (basement != null & dependentBlocksList != null && dependentBlocksList.Count != 0)
        {
            basement.myChunk.ClearBlocksList(dependentBlocksList, true);
        }
        PrepareWorkbuildingForDestruction(forced);
        GameMaster.colonyController.RemoveDock(this);
        if (maintainingShip & loadingShip != null) loadingShip.Undock();        
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}

[System.Serializable]
public class DockSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public bool correctLocation, maintainingShip;
	public ShipSerializer loadingShip;
	public float loadingTimer = 0, shipArrivingTimer = 0;
}
[System.Serializable]
public class DockStaticSerializer {
	public bool?[] isForSale;
	public int[] minValueForTrading;
	public float[] prices, demand;
	public int immigrationPlan ;
	public bool immigrationEnabled;
}
