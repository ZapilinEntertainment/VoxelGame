using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadQuartersSerializer {
	public BuildingSerializer buildingSerializer;
	public bool nextStageConditionMet;
	public byte level;
}

public class HeadQuarters : House {
    [SerializeField]
	bool nextStageConditionMet = false;
	ColonyController colony;
	GameObject rooftop;
	
	public override void SetBasement(SurfaceBlock b, PixelPosByte pos) {		
		if (b == null) return;
		PrepareHouse(b,pos);
		if (level > 3 ) {
			if (rooftop == null) {
				rooftop = Object.Instantiate(Resources.Load<GameObject>("Structures/HQ_rooftop"));
				rooftop.transform.parent = model.transform;
				rooftop.transform.localPosition = Vector3.up * (level - 3) * Block.QUAD_SIZE;
			}
			if (level > 4) {
				int i = 5;
				while (i <= level) {
					b.myChunk.BlockByStructure( b.pos.x, (byte)(b.pos.y + i - 4), b.pos.z, this);
					GameObject addon = Object.Instantiate(Resources.Load<GameObject>("Structures/HQ_Addon"));
					addon.transform.parent = model.transform;
					addon.transform.localPosition = Vector3.zero + (i - 3.5f) * Vector3.up * Block.QUAD_SIZE;
					addon.transform.localRotation = model.transform.localRotation;
					i++;
				}
				BoxCollider bc = model.GetComponent<BoxCollider>();
				bc.center = Vector3.up * (level - 3) * Block.QUAD_SIZE/2f;
				bc.size = new Vector3(Block.QUAD_SIZE, (level - 3) * Block.QUAD_SIZE, Block.QUAD_SIZE );
			}
		}
		colony = GameMaster.colonyController;
		colony.SetHQ(this);
	}


    bool CheckUpgradeCondition()
    {
        switch (level)
        {
            default: return false;
            case 1: return (colony.docks.Count != 0);
            case 2: return (RollingShop.current != null );
            case 3: return (GraphoniumEnricher.current != null);
            case 4: return (ChemicalFactory.current != null);
            case 5: return true;
        }
    }

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHeadQuartersSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load (StructureSerializer ss, SurfaceBlock sb) {
		HeadQuartersSerializer hqs = new HeadQuartersSerializer();
		GameMaster.DeserializeByteArray<HeadQuartersSerializer>(ss.specificData, ref hqs);
		level = hqs.level; 
		LoadStructureData(ss, sb);
		LoadBuildingData(hqs.buildingSerializer);
		nextStageConditionMet = hqs.nextStageConditionMet;
	} 
		

	protected HeadQuartersSerializer GetHeadQuartersSerializer() {
		HeadQuartersSerializer hqs = new HeadQuartersSerializer();
		hqs.level = level;
		hqs.nextStageConditionMet = nextStageConditionMet;
		hqs.buildingSerializer = GetBuildingSerializer();
		return hqs;
	}
	#endregion


    override public bool IsLevelUpPossible(ref string refusalReason)
    {
        if (level < 7)
        {
            if (nextStageConditionMet)
            {
                if (level > 4)
                {
                    ChunkPos upperPos = new ChunkPos(basement.pos.x, basement.pos.y + (level - 3), basement.pos.z);
                    Block upperBlock = basement.myChunk.GetBlock(upperPos.x, upperPos.y, upperPos.z);
                    if (upperBlock != null)
                    {
                        if (upperBlock.type == BlockType.Cube)
                        {
                            refusalReason = Localization.GetRefusalReason(RefusalReason.SpaceAboveBlocked);
                            return false;
                        }
                        else return true;
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
                if (!GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
                {
                    UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
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
                Block upperBlock = chunk.GetBlock(upperPos.x, upperPos.y, upperPos.z);
                if (upperBlock != null)
                {
                    chunk.BlockByStructure(upperPos.x, upperPos.y, upperPos.z, this);
                }
                GameObject addon = Object.Instantiate(Resources.Load<GameObject>("Structures/HQ_Addon"));
                addon.transform.parent = model.transform;
                addon.transform.localPosition = Vector3.zero + (level - 2.5f) * Vector3.up * Block.QUAD_SIZE;
                addon.transform.localRotation = model.transform.localRotation;
                BoxCollider bc = model.GetComponent<BoxCollider>();
                bc.size = new Vector3(Block.QUAD_SIZE, (level - 3) * Block.QUAD_SIZE, Block.QUAD_SIZE);
                bc.center = Vector3.up * (level - 3) * Block.QUAD_SIZE / 2f;
                if (rooftop == null)
                {
                    rooftop = Object.Instantiate(Resources.Load<GameObject>("Structures/HQ_rooftop"));
                    rooftop.transform.parent = model.transform;
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
            float discount = GameMaster.upgradeDiscount;
            for (int i = 0; i < cost.Length; i++)
            {
                cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * discount);
            }
            return cost;
        }
        else {
            ResourceContainer[] cost = ResourcesCost.GetCost(HQ_4_ID);
            float discount = GameMaster.upgradeCostIncrease + level - 4;
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
}
