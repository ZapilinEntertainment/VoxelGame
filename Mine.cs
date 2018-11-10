using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MineSerializer {
	public WorkBuildingSerializer workBuildingSerializer;
	public bool workFinished;
	public ChunkPos lastWorkObjectPos;
	public bool awaitingElevatorBuilding;
	public byte level;
	public List<StructureSerializer> elevators;
	public List<byte>elevatorHeights;
	public bool haveElevators;
}

public class Mine : WorkBuilding {
	CubeBlock workObject;
	bool workFinished = false;
	ChunkPos lastWorkObjectPos;
	public List<Structure> elevators;
	public bool awaitingElevatorBuilding = false;

	override public void Prepare() {
		PrepareWorkbuilding();
		elevators = new List<Structure>();
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
		Block bb = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y - 1, basement.pos.z);
		workObject = bb as CubeBlock;
		lastWorkObjectPos = bb.pos;
	}

	override public void LabourUpdate() {
		if (awaitingElevatorBuilding) {
			Block b = basement.myChunk.GetBlock(lastWorkObjectPos.x, lastWorkObjectPos.y, lastWorkObjectPos.z);
			if ( b != null ) {
				if (b.type == BlockType.Cave | b.type == BlockType.Surface ) {
					Structure s = GetStructureByID(MINE_ELEVATOR_ID);
					s.SetBasement(b as SurfaceBlock, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION/2 - s.innerPosition.size/2, SurfaceBlock.INNER_RESOLUTION/2 - s.innerPosition.size/2));
					elevators.Add(s);
					awaitingElevatorBuilding = false;
                    UIController.current.MakeAnnouncement(Localization.GetActionLabel(LocalizationActionLabels.MineLevelFinished));
				}
			}
		}
		if ( !isActive | !energySupplied ) return;
        if (workObject != null)
        {
            if (workersCount > 0 && !workFinished)
            {
                workflow += workSpeed;
                if (workflow >= workflowToProcess)
                {
                    LabourResult();
                    workflow -= workflowToProcess;
                }
            }
        }
	}

override protected void LabourResult() {
		int x = (int) workflow;
		float production = x;
		production = workObject.Dig(x, false);
		GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
		if ( workObject!=null && workObject.volume != 0) {
		    float percent = workObject.volume / (float) CubeBlock.MAX_VOLUME;
			if (showOnGUI) workbuildingObserver.SetActionLabel( string.Format("{0:0.##}", (1 - percent) * 100) + "% " + Localization.GetActionLabel(LocalizationActionLabels.Extracted)); 
			workflow -= production;	
		}
		else {
			workFinished = true;
            if (showOnGUI) workbuildingObserver.SetActionLabel(Localization.GetActionLabel(LocalizationActionLabels.WorkStopped));
			awaitingElevatorBuilding = true;
		}			
	}

	override protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Mining);
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetMineSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		MineSerializer ms = new MineSerializer();
		GameMaster.DeserializeByteArray(ss.specificData, ref ms);
		LoadMineData(ms);
	}

	protected void LoadMineData(MineSerializer ms) {
		level = ms.level;
		LoadWorkBuildingData(ms.workBuildingSerializer);
		elevators = new List<Structure>();
		if (level > 1 & ms.haveElevators) {
			for (int i = 0; i < ms.elevators.Count; i++) {
				Structure s = Structure.GetStructureByID(MINE_ELEVATOR_ID);
				s.Load(ms.elevators[i], basement.myChunk.GetBlock(basement.pos.x, ms.elevatorHeights[i], basement.pos.z) as SurfaceBlock);
				elevators.Add(s);
			}
		}
		workFinished = ms.workFinished;
		lastWorkObjectPos = ms.lastWorkObjectPos;
		awaitingElevatorBuilding = ms.awaitingElevatorBuilding;
	}

	protected MineSerializer GetMineSerializer() {
		MineSerializer ms = new MineSerializer();
		ms.workBuildingSerializer = GetWorkBuildingSerializer();
		ms.workFinished = workFinished;
		ms.lastWorkObjectPos = lastWorkObjectPos;
		ms.awaitingElevatorBuilding = awaitingElevatorBuilding;
		ms.level = level;
		ms.elevators = new List<StructureSerializer>(); ms.elevatorHeights = new List<byte>();
		ms.haveElevators = false;
		if (level > 1) {
			for (int i = 0; i < elevators.Count; i++) {
				if (elevators[i] == null) continue;
				else {
					ms.elevators.Add((elevators[i] as MineElevator).GetSerializer());
					ms.elevatorHeights.Add(elevators[i].basement.pos.y);
					ms.haveElevators = true;
				}
			}
		}
		return ms;
	}
	#endregion

	void UpgradeMine(byte f_level) {
		if (f_level == level ) return;
		GameObject nextModel = Resources.Load<GameObject>("Prefs/minePref_level_" + (f_level).ToString());
		if (nextModel != null) {
            Transform model = transform.GetChild(0);
            if (model != null) Destroy(model.gameObject);
            GameObject newModelGO = Instantiate(nextModel, transform.position, transform.rotation, transform);			
            newModelGO.SetActive(visible);
            if (!isActive | !energySupplied) ChangeRenderersView(false);
        }		
		level = f_level;
	}

    override public bool IsLevelUpPossible(ref string refusalReason)
    {
        if (workFinished && !awaitingElevatorBuilding)
        {
            Block b = basement.myChunk.GetBlock(lastWorkObjectPos.x, lastWorkObjectPos.y - 1, lastWorkObjectPos.z);
            if (b != null && b.type == BlockType.Cube) return true;
            else
            {
                refusalReason = Localization.GetRefusalReason(RefusalReason.NoBlockBelow);
                return false;
            }
        }
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.WorkNotFinished);
            return false;
        }
    }

    override public void LevelUp(bool returnToUI)
    {
        Block b = basement.myChunk.GetBlock(lastWorkObjectPos.x, lastWorkObjectPos.y - 1, lastWorkObjectPos.z);
        if (b != null && b.type == BlockType.Cube)
        {
            if (!GameMaster.realMaster.weNeedNoResources)
            {
                ResourceContainer[] cost = GetUpgradeCost();
                if (cost != null && cost.Length != 0)
                {
                    if (!GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
                    {
                        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                        return;
                    }
                }
            }
            workObject = b as CubeBlock;
            lastWorkObjectPos = b.pos;
            workFinished = false;
            UpgradeMine((byte)(level + 1));
        }        
    }
    override public ResourceContainer[] GetUpgradeCost()
    {
        ResourceContainer[] cost = ResourcesCost.GetCost(MINE_ID);
        float discount = GameMaster.realMaster.upgradeCostIncrease + level - 1;
        for (int i = 0; i < cost.Length; i++)
        {
            cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * discount);
        }
        return cost;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(forced);
       // if (elevators.Count > 0)
       // {
       //     foreach (Structure s in elevators)
         //   {
         //       if (s != null) s.Annihilate(false);
          //  }
        //}
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
        }
        Destroy(gameObject);
    }
}
