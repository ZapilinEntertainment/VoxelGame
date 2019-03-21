using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HeadQuarters : House {
	private bool nextStageConditionMet = false;
	ColonyController colony;
	GameObject rooftop;
	
	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
        colony = GameMaster.realMaster.colonyController;
        if (colony == null)
        {
            colony = GameMaster.realMaster.gameObject.AddComponent<ColonyController>();
            colony.Prepare();
        }
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        //#
        if (level > 3 ) {
			if (rooftop == null) {
                if (b.myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + 1), b.pos.z, this))
                {
                    rooftop = PoolMaster.GetRooftop(false, true);
                    rooftop.transform.parent = transform.GetChild(0);
                    rooftop.transform.localPosition = Vector3.up * (level - 3) * Block.QUAD_SIZE;
                }
			}
			if (level > 4) {
				int i = 5;
				while (i <= level) {
                    b.myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + i - 4), b.pos.z, this);
					GameObject addon = Instantiate(Resources.Load<GameObject>("Structures/HQ_Addon"));
					addon.transform.parent = transform.GetChild(0);
					addon.transform.localPosition = Vector3.zero + (i - 3.5f) * Vector3.up * Block.QUAD_SIZE;
					addon.transform.localRotation = transform.GetChild(0).localRotation;
                    if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(addon, true);
					i++;
				}
				BoxCollider bc = transform.GetChild(0).GetComponent<BoxCollider>();
				bc.center = Vector3.up * (level - 3) * Block.QUAD_SIZE/2f;
				bc.size = new Vector3(Block.QUAD_SIZE, (level - 3) * Block.QUAD_SIZE, Block.QUAD_SIZE );
			}
		}		        
        colony.SetHQ(this);
	}

    bool CheckUpgradeCondition()
    {
        switch (level)
        {
            default: return false;
            case 1: return (colony.docks.Count != 0);
            case 2:
                if (colony.powerGrid.Count != 0)
                {
                    foreach (Building b in colony.powerGrid)
                    {
                        if (b.id == WORKSHOP_ID) return true;
                    }
                }
                return false;
            case 3:
                if (colony.powerGrid.Count != 0) 
                {
                    foreach (Building b in colony.powerGrid)
                    {
                        if (b.id == GRPH_ENRICHER_3_ID) return true;
                    }
                }
                return false;
            case 4: return (ChemicalFactory.current != null);
            case 5: return true;
        }
    }

    override public bool IsLevelUpPossible(ref string refusalReason)
    {
        if (level < GameConstants.HQ_MAX_LEVEL)
        {
            if (nextStageConditionMet)
            {
                if (level > 4)
                {
                    ChunkPos upperPos = new ChunkPos(basement.pos.x, basement.pos.y + (level - 3), basement.pos.z);
                    Block upperBlock = basement.myChunk.GetBlock(upperPos.x, upperPos.y, upperPos.z);
                    if (upperBlock != null && upperBlock.type == BlockType.Cube)
                    {
                        refusalReason = Localization.GetRefusalReason(RefusalReason.SpaceAboveBlocked);
                        return false;
                    }
                    else return true;
                }
                else return true;
            }
            else
            {
                switch (level) {
                    case 1: refusalReason = Localization.GetRefusalReason(RefusalReason.HQ_RR1); break;
                    case 2: refusalReason = Localization.GetRefusalReason(RefusalReason.HQ_RR2); break;
                    case 3: refusalReason = Localization.GetRefusalReason(RefusalReason.HQ_RR3); break;
                    case 4: refusalReason = Localization.GetRefusalReason(RefusalReason.HQ_RR4); break;
                    case 5: refusalReason = Localization.GetRefusalReason(RefusalReason.HQ_RR5); break;
                    case 6: refusalReason = Localization.GetRefusalReason(RefusalReason.HQ_RR6); break;
                }
                return false;
            }
        }
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.MaxLevel);
            return false;
        }
    }
    override public void LevelUp( bool returnToUI )
    {
        if ( !GameMaster.realMaster.weNeedNoResources )
        {
            ResourceContainer[] cost = GetUpgradeCost() ;
                if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
                {
                    GameLogUI.NotEnoughResourcesAnnounce();
                    return;
                }
        }
        if (level < 4)
        {
                Building upgraded = GetStructureByID(upgradedIndex) as Building;
                upgraded.SetBasement(basement, PixelPosByte.zero);
                if (returnToUI) upgraded.ShowOnGUI();
        }
        else
        { // building blocks on the top
                Chunk chunk = basement.myChunk;
                ChunkPos upperPos = new ChunkPos(basement.pos.x, basement.pos.y + (level - 3), basement.pos.z);
                chunk.BlockByStructure(upperPos.x, upperPos.y, upperPos.z, this);
                Transform model = transform.GetChild(0);
                GameObject addon = Instantiate(Resources.Load<GameObject>("Structures/HQ_Addon"));
                addon.transform.parent = model;
                addon.transform.localPosition = Vector3.zero + (level - 2.5f) * Vector3.up * Block.QUAD_SIZE;
                addon.transform.localRotation = model.localRotation;
                if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(addon, true);
                BoxCollider bc = model.GetComponent<BoxCollider>();
                bc.size = new Vector3(Block.QUAD_SIZE, (level - 3) * Block.QUAD_SIZE, Block.QUAD_SIZE);
                bc.center = Vector3.up * (level - 3) * Block.QUAD_SIZE / 2f;
                if (rooftop == null)
                {
                    rooftop = PoolMaster.GetRooftop(false, true);
                    rooftop.transform.parent = model;
                }
                rooftop.transform.localPosition = Vector3.up * (level - 2) * Block.QUAD_SIZE;
                level++;
        }
    }
    override public ResourceContainer[] GetUpgradeCost()
    {
        if (level < 4)
        {
            ResourceContainer[] cost = ResourcesCost.GetCost(upgradedIndex);
            float discount = GameMaster.realMaster.upgradeDiscount;
            for (int i = 0; i < cost.Length; i++)
            {
                cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * discount);
            }
            return cost;
        }
        else {
            ResourceContainer[] cost = ResourcesCost.GetCost(HQ_4_ID);
            float discount = GameMaster.realMaster.upgradeCostIncrease + level - 4;
            for (int i = 0; i < cost.Length; i++)
            {
                cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * discount);
            }
            return cost;
        }
    }

    public override UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        nextStageConditionMet = CheckUpgradeCondition();
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
        return buildingObserver;
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(forced);
        // removed colony.RemoveHousing because of dropping hq field
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = base.Save();
        data.Add(level);
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sb)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadBuildingData(data, STRUCTURE_SERIALIZER_LENGTH);
        Prepare();
        //load structure data
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        // >>
        level = (byte)fs.ReadByte();
        // >>
        SetBasement(sb, new PixelPosByte(data[0], data[ 1]));
        hp = System.BitConverter.ToSingle(data, 4);
        maxHp = System.BitConverter.ToSingle(data,  8);
    }


    
    #endregion
}
