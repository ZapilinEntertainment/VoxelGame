using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public sealed class HangarSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public bool constructing;
	public int shuttle_id;
}

public sealed class Hangar : WorkBuilding {
	static int hangarsCount = 0;
	public Shuttle shuttle{get; private set;}
	const float CREW_HIRE_BASE_COST = 100;
    public bool constructing { get; private set; }
    public bool correctLocation { get; private set; }
    public static UIHangarObserver hangarObserver;
    private List<Block> dependentBlocksList;

    public static void ResetToDefaults_Static_Hangar() {
		hangarsCount = 0;
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
                case 2: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x + 1, basement.pos.y, basement.pos.z ), 2, 1, this, ref dependentBlocksList); break;
                case 4: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x, basement.pos.y, basement.pos.z - 1), 4, 1, this, ref dependentBlocksList); break;
                case 6: correctLocation = basement.myChunk.BlockShipCorridorIfPossible(new Vector3Int(basement.pos.x - 1, basement.pos.y, basement.pos.z ), 6, 1, this, ref dependentBlocksList); break;
            }
            if (showOnGUI)
            {
                if (!correctLocation)
                {
                    //#incorrectLocationDisplaying - Hangar
                    float len = 1;
                    float x = basement.transform.position.x, y = basement.transform.position.y, z = basement.transform.position.z;
                    switch (modelRotation)
                    {
                        case 0:
                            len = basement.pos.z * Block.QUAD_SIZE;
                            PoolMaster.current.DrawZone(
                        new Vector3(x, y, z + Block.QUAD_SIZE / 2f + len / 2f),
                        new Vector3(1,1, len),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                            break;
                        case 2:
                            len = (Chunk.CHUNK_SIZE - x - 1) * Block.QUAD_SIZE;
                            PoolMaster.current.DrawZone(
                            new Vector3(x + Block.QUAD_SIZE / 2f + len / 2f, y, z),
                            new Vector3(len, 1, 1),
                            new Color(1, 0.076f, 0.076f, 0.4f)
                            );
                            break;
                        case 4:
                            len = (Chunk.CHUNK_SIZE - z - 1) * Block.QUAD_SIZE;
                            PoolMaster.current.DrawZone(
                            new Vector3(x, y , z - Block.QUAD_SIZE/2f - len / 2f),
                            new Vector3(1, 1, len),
                            new Color(1, 0.076f, 0.076f, 0.4f)
                            );
                            break;
                        case 6:
                            len = basement.pos.x * Block.QUAD_SIZE;
                            PoolMaster.current.DrawZone(
                            new Vector3( x  - Block.QUAD_SIZE/2f - len/2f, y, z),
                            new Vector3( len, 1, 1),
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
    override public void ChunkUpdated(ChunkPos pos)
    {
        if (basement == null) return;
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
        if (showOnGUI)
        {
            if (!correctLocation)
            {
                //#incorrectLocationDisplaying - Hangar
                float len = 1;
                float x = basement.transform.position.x, y = basement.transform.position.y, z = basement.transform.position.z;
                switch (modelRotation)
                {
                    case 0:
                        len = basement.pos.z * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                    new Vector3(x, y, z + Block.QUAD_SIZE / 2f + len / 2f),
                    new Vector3(1, 1, len),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                        break;
                    case 2:
                        len = (Chunk.CHUNK_SIZE - x - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(x + Block.QUAD_SIZE / 2f + len / 2f, y, z),
                        new Vector3(len, 1, 1),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 4:
                        len = (Chunk.CHUNK_SIZE - z - 1) * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(x, y, z - Block.QUAD_SIZE / 2f - len / 2f),
                        new Vector3(1, 1, len),
                        new Color(1, 0.076f, 0.076f, 0.4f)
                        );
                        break;
                    case 6:
                        len = basement.pos.x * Block.QUAD_SIZE;
                        PoolMaster.current.DrawZone(
                        new Vector3(x - Block.QUAD_SIZE / 2f - len / 2f, y, z),
                        new Vector3(len, 1, 1),
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

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
		Transform modelTransform = transform.GetChild(0);
		if (basement.pos.z == 0) {
			modelTransform.transform.localRotation = Quaternion.Euler(0, 180,0); 
		}
		else {
			if (basement.pos.z != Chunk.CHUNK_SIZE - 1) {
				if (basement.pos.x == 0) {
					modelTransform.transform.localRotation = Quaternion.Euler(0, -90,0); 
				}
				else {
					if (basement.pos.x == Chunk.CHUNK_SIZE - 1) {
						modelTransform.transform.localRotation = Quaternion.Euler(0, 90,0);
					}
				}
			}
		}
		hangarsCount++;        
	}

    public override void LabourUpdate() {
        if ( isActive & energySupplied & constructing)  {
			workflow += workSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
			}
		}
	}

	override protected void LabourResult() {
		shuttle = Instantiate(Resources.Load<GameObject>("Prefs/shuttle")).GetComponent<Shuttle>();
		shuttle.FirstSet(this);
		constructing = false;
		workflow -= workflowToProcess;
        if (showOnGUI)
        {
            hangarObserver.PrepareHangarWindow();
        }
        UIController.current.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.ShuttleConstructed));
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.MachineConstructing);
	}

    public override UIObserver ShowOnGUI()
    {
        if (hangarObserver == null) hangarObserver = UIHangarObserver.InitializeHangarObserverScript() ;
        else hangarObserver.gameObject.SetActive(true);
        hangarObserver.SetObservingHangar(this);
        showOnGUI = true;
        if (!correctLocation)
        {
            //#incorrectLocationDisplaying - Hangar
            float len = 1;
            float x = basement.transform.position.x, y = transform.position.y, z = basement.transform.position.z;
            switch (modelRotation)
            {
                case 0:
                    len = basement.pos.z * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                new Vector3(x, y, z + Block.QUAD_SIZE / 2f + len / 2f),
                new Vector3(1, 1, len),
                new Color(1, 0.076f, 0.076f, 0.4f)
                );
                    break;
                case 2:
                    len = (Chunk.CHUNK_SIZE - x - 1) * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(x + Block.QUAD_SIZE / 2f + len / 2f, y, z),
                    new Vector3(len, 1, 1),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 4:
                    len = (Chunk.CHUNK_SIZE - z - 1) * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(x, y, z - Block.QUAD_SIZE / 2f - len / 2f),
                    new Vector3(1, 1, len),
                    new Color(1, 0.076f, 0.076f, 0.4f)
                    );
                    break;
                case 6:
                    len = basement.pos.x * Block.QUAD_SIZE;
                    PoolMaster.current.DrawZone(
                    new Vector3(x - Block.QUAD_SIZE / 2f - len / 2f, y, z),
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
            PoolMaster.current.DisableZone();
        }
    }

    public void DeconstructShuttle()
    {
        if (shuttle == null) return;
        else
        {
            shuttle.Deconstruct();
            shuttle = null;
            hangarObserver.PrepareHangarWindow();
        }
    }

    #region save-load system
    override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHangarSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		HangarSerializer hs = new HangarSerializer();
		GameMaster.DeserializeByteArray<HangarSerializer>(ss.specificData, ref hs);
		constructing = hs.constructing;
		LoadWorkBuildingData(hs.workBuildingSerializer);
		shuttle = Shuttle.GetShuttle(hs.shuttle_id);
	}

	HangarSerializer GetHangarSerializer() {
		HangarSerializer hs = new HangarSerializer();
		hs.workBuildingSerializer = GetWorkBuildingSerializer();
		hs.constructing = constructing;
		hs.shuttle_id = (shuttle == null ? -1 : shuttle.ID);
		return hs;
	}
	#endregion

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (basement != null & dependentBlocksList != null && dependentBlocksList.Count != 0)
        {
            basement.myChunk.ClearBlocksList(dependentBlocksList, true);
            dependentBlocksList.Clear();
        }
        if (showOnGUI & !correctLocation) PoolMaster.current.DisableZone();
        if (forced) { UnsetBasement(); }
        PrepareWorkbuildingForDestruction(forced);
        hangarsCount--;
        if (shuttle != null) shuttle.Deconstruct();
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
