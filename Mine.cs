using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		SetWorkbuildingData(b, pos);
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
                if (b.type == BlockType.Surface)
                {
                    Structure s = GetStructureByID(MINE_ELEVATOR_ID);
                    s.SetBasement(b as SurfaceBlock, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION / 2 - s.innerPosition.size / 2, SurfaceBlock.INNER_RESOLUTION / 2 - s.innerPosition.size / 2));
                    elevators.Add(s);
                    awaitingElevatorBuilding = false;
                    UIController.current.MakeAnnouncement(Localization.GetActionLabel(LocalizationActionLabels.MineLevelFinished));
                }
                else
                {
                    if (b.type == BlockType.Cave)
                    {
                        CaveBlock cvb = b as CaveBlock;
                        if (cvb.haveSurface)
                        {
                            Structure s = GetStructureByID(MINE_ELEVATOR_ID);
                            s.SetBasement(cvb, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION / 2 - s.innerPosition.size / 2, SurfaceBlock.INNER_RESOLUTION / 2 - s.innerPosition.size / 2));
                            elevators.Add(s);
                            awaitingElevatorBuilding = false;
                            UIController.current.MakeAnnouncement(Localization.GetActionLabel(LocalizationActionLabels.MineLevelFinished));
                        }
                        else
                        {
                            workFinished = true;
                            awaitingElevatorBuilding = false;
                            SetActivationStatus(false, true);
                            FreeWorkers();
                            if (showOnGUI) workbuildingObserver.SetActionLabel(Localization.GetRefusalReason(RefusalReason.NoBlockBelow));
                        }
                    }
                }
			}
		}
		if ( !isActive | !isEnergySupplied ) return;
        if (workObject != null)
        {
            if (workersCount > 0 && !workFinished)
            {
                workflow += workSpeed;
                colony.gears_coefficient -= gearsDamage;
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
		GameMaster.geologyModule.CalculateOutput(production, workObject, colony.storage);
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

	override public void RecalculateWorkspeed() {
		workSpeed = GameMaster.realMaster.CalculateWorkspeed(workersCount, WorkType.Mining);
        gearsDamage = GameConstants.FACTORY_GEARS_DAMAGE_COEFFICIENT * workSpeed;
	}

	void UpgradeMine(byte f_level) {
		if (f_level == level ) return;
		GameObject nextModel = Resources.Load<GameObject>("Prefs/minePref_level_" + (f_level).ToString());
		if (nextModel != null) {
            Transform model = transform.GetChild(0);
            if (model != null) Destroy(model.gameObject);
            GameObject newModelGO = Instantiate(nextModel, transform.position, transform.rotation, transform);			
            newModelGO.SetActive(visible);
            if (!isActive | !isEnergySupplied) ChangeRenderersView(false);
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
                    if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
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

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(SerializeMine());
        return data;
    }

    protected List<byte> SerializeMine()
    {
        byte zero = 0, one = 1;
        int elevatorsCount = elevators.Count;
        var elevatorsData = new List<byte>();
        if (elevatorsCount > 0) {            
            int i = 0;
            while (i < elevators.Count)
            {
                if (elevators[i] == null)
                {
                    elevators.RemoveAt(i);
                }
                else
                {
                    Structure elevator = elevators[i];
                    elevatorsData.Add(elevator.basement.pos.y);
                    elevatorsData.Add(elevator.modelRotation);
                    byte ehp = (byte)((elevator.hp / elevator.maxHp) * 255);
                    elevatorsData.Add(ehp);
                    i++;
                }
            }
            elevatorsCount = elevators.Count;
        }

        var data = new List<byte>()
        {
            workFinished ? one : zero,  //0
            lastWorkObjectPos.x,        //1
            lastWorkObjectPos.y,        //2
            lastWorkObjectPos.z,        //3
            awaitingElevatorBuilding ? one : zero, //4
            level,                                  //5
        };
        data.AddRange(System.BitConverter.GetBytes(elevatorsCount)); // 6 - 9
        if (elevatorsCount > 0)
        {
            data.AddRange(elevatorsData); // 10 +
        }
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, sblock);
        LoadBuildingData(data, STRUCTURE_SERIALIZER_LENGTH);
        LoadMineData(fs);
    }
    protected void LoadMineData(System.IO.FileStream fs)
    {
        var wdata = new byte[WORKBUILDING_SERIALIZER_LENGTH];
        fs.Read(wdata, 0, wdata.Length);

        var data = new byte[10];
        fs.Read(data, 0, data.Length);
        level = data[ 5];

        LoadWorkBuildingData(wdata, 0);

        elevators = new List<Structure>();
        int elevatorsCount = System.BitConverter.ToInt32(data, 6);
        if (elevatorsCount > 0)
        {
            Chunk chunk = basement.myChunk;
            byte x = basement.pos.x, z = basement.pos.z;

            int eDataLength = 3;
            var eData = new byte[eDataLength];            
            for (int i = 0; i < elevatorsCount; i++)
            {
                fs.Read(eData, 0, eDataLength);
                Structure s = GetStructureByID(MINE_ELEVATOR_ID);
                s.SetModelRotation(eData[1]);
                s.SetBasement(chunk.GetBlock(x, eData[0],z) as SurfaceBlock, new PixelPosByte(SurfaceBlock.INNER_RESOLUTION / 2 - s.innerPosition.size /2, SurfaceBlock.INNER_RESOLUTION / 2 - s.innerPosition.size / 2));
                s.SetHP(eData[2] / 255f * s.maxHp);
                elevators.Add(s);
            }
        }

        workFinished = data[0] == 1;
        lastWorkObjectPos = new ChunkPos(data[1], data[2], data[3]);
        if (!workFinished) workObject = basement.myChunk.GetBlock(lastWorkObjectPos) as CubeBlock;
        awaitingElevatorBuilding = data[4] == 1;
    }   
    #endregion
}
