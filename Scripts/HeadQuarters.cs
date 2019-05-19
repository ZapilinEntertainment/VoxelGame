using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HeadQuarters : House {
	private bool nextStageConditionMet = false;
	private ColonyController colony;

    public static Structure GetHQ(byte i_level)
    {
        Structure s = GetStructureByID(HEADQUARTERS_ID);
        (s as HeadQuarters).SetLevel(i_level);
        return s;
    }

    protected override void SetModel()
    {
        GameObject model;
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        if (level < 2) model = Instantiate(Resources.Load<GameObject>("Structures/ZeppelinBasement"));
        else model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/HQ_level_" + level.ToString()));
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(model, true);
    }

    public void SetLevel(byte x)
    {
        level = x;
        if (basement != null) SetModel();
    }
    override public void Prepare() {
        PrepareBuilding();       
        switch (level)
        {
            case 1: energySurplus = 1f; energyCapacity = 100f; housing = 10; break;
            case 2: energySurplus = 3f; energyCapacity = 200f; housing = 30; break;
            case 3: energySurplus = 5f; energyCapacity = 400f; housing = 40; break;
            case 4: energySurplus = 12f; energyCapacity = 500f; housing = 55; break;
            case 5: energySurplus = 20f; energyCapacity = 600f; housing = 70; break;
            case 6: energySurplus = 25f; energyCapacity = 700f; housing = 80; ChangeUpgradedIndex(-1); break;
        }
    }

    public override void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        colony = GameMaster.realMaster.colonyController;
        if (colony == null)
        {
            colony = GameMaster.realMaster.gameObject.AddComponent<ColonyController>();
            colony.Prepare();
        }
        colony.SetHQ(this);

        SetBuildingData(b, pos);
        maxHp = 1000 * level;
        hp = maxHp;

        if (level > 3)
        {
            basement.myChunk.BlockByStructure(basement.pos.x, basement.pos.y + 1, basement.pos.z, this);
            if (level == 6) basement.myChunk.BlockByStructure(basement.pos.x, basement.pos.y + 2, basement.pos.z, this);
        }
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
                if (level > 2)
                {
                    if (level == 3)
                    {
                        Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y + 1, basement.pos.z);
                        if (upperBlock != null && upperBlock.type != BlockType.Shapeless)
                        {
                            refusalReason = Localization.GetRefusalReason(RefusalReason.SpaceAboveBlocked);
                            return false;
                        }
                        else return true;
                    }
                    else
                    {
                        if (level == 5)
                        {
                            Block upperBlock = basement.myChunk.GetBlock(basement.pos.x, basement.pos.y + 2, basement.pos.z);
                            if (upperBlock != null && upperBlock.type != BlockType.Shapeless)
                            {
                                refusalReason = Localization.GetRefusalReason(RefusalReason.SpaceAboveBlocked);
                                return false;
                            }
                            else return true;
                        }
                        else return true;
                    }
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
                    case 7: refusalReason = Localization.GetRefusalReason(RefusalReason.MaxLevel); break;
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
        if (level > 3)
        {
            if (level == 4) basement.myChunk.BlockByStructure(basement.pos.x, basement.pos.y + 1, basement.pos.z, this);
            if (level == 6) basement.myChunk.BlockByStructure(basement.pos.x, basement.pos.y + 2, basement.pos.z, this);
        }
        level++;
        nextStageConditionMet = CheckUpgradeCondition();
        SetModel();
        buildingObserver.CheckUpgradeAvailability();
    }
    override public ResourceContainer[] GetUpgradeCost()
    {
        int costId = HEADQUARTERS_ID;
        switch (level)
        {
            case 1: costId = ResourcesCost.HQ_LVL2_COST_ID; break;
            case 2: costId = ResourcesCost.HQ_LVL3_COST_ID; break;
            case 3: costId = ResourcesCost.HQ_LVL4_COST_ID; break;
            case 4: costId = ResourcesCost.HQ_LVL5_COST_ID; break;
            case 5: costId = ResourcesCost.HQ_LVL6_COST_ID; break;
        }
        ResourceContainer[] cost = ResourcesCost.GetCost(costId);
        return cost;
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
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
