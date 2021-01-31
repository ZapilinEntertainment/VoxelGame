using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : WorkBuilding {
    private byte diggingMask = 0, currentDiggingPosition = 0;
    // 23 24  9 10 11
    // 22  8  1  2 12
    // 21  7  0  3 13
    // 20  6  5  4 14
    // 19 18 17 16 15

	override public void SetBasement(Plane b, PixelPosByte pos) {
		if (b == null) return;
		SetWorkbuildingData(b, pos);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
        }
		Block bb = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y - 1, basement.pos.z);
		//workObject = bb as CubeBlock;
		
	}

    public override bool ShowWorkspeed()
    {
        return true;
    }

    void UpgradeMine(byte f_level)
    {
        if (f_level == level) return;
        GameObject nextModel = Resources.Load<GameObject>("Prefs/minePref_level_" + (f_level).ToString());
        if (nextModel != null)
        {
            Transform model = transform.GetChild(0);
            if (model != null) Destroy(model.gameObject);
            GameObject newModelGO = Instantiate(nextModel, transform.position, transform.rotation, transform);
            newModelGO.SetActive(visibilityMode != VisibilityMode.Invisible);
            if (!isActive | !isEnergySupplied) ChangeRenderersView(false);
        }
        level = f_level;
    }

    /*
    override public bool IsLevelUpPossible(ref string refusalReason)
    {
        if (workFinished && !awaitingElevatorBuilding)
        {
            //Block b = basement.myChunk.GetBlock(lastWorkObjectPos.x, lastWorkObjectPos.y - 1, lastWorkObjectPos.z);
            //if (b != null && b.type == BlockType.Cube) return true;
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
    */

    override public void LevelUp(bool returnToUI)
    {

        //if (b != null && b.type == BlockType.Cube)
        {
            if (!GameMaster.realMaster.weNeedNoResources)
            {
                ResourceContainer[] cost = GetUpgradeCost();
                if (cost != null && cost.Length != 0)
                {
                    if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
                    {
                        AnnouncementCanvasController.NotEnoughResourcesAnnounce();
                        return;
                    }
                }
            }
         //   workObject = b as CubeBlock;
            
            UpgradeMine((byte)(level + 1));
            GameMaster.realMaster.eventTracker?.BuildingUpgraded(this);
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareWorkbuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
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
        
        return data;
    }

    protected List<byte> SerializeMine()
    {
        var data = new List<byte>();
      
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadStructureData(data, sblock);
        LoadBuildingData(data, STRUCTURE_SERIALIZER_LENGTH);
        LoadMineData(fs);
    }
    protected void LoadMineData(System.IO.FileStream fs)
    {
       
    }   
    #endregion
}
