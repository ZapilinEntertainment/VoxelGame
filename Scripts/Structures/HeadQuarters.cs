using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HeadQuarters : Building
{
    private bool nextStageConditionMet = false, subscribedToRestoreBlockersEvent = false;
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
        Quaternion prevRot = Quaternion.identity;
        if (transform.childCount != 0)
        {
            var p = transform.GetChild(0);
            prevRot = p.localRotation;
            Destroy(p.gameObject);
        }
        else prevRot = Quaternion.Euler(0f, 45f * modelRotation, 0f);
        if (level < 2) model = Instantiate(Resources.Load<GameObject>("Structures/ZeppelinBasement"));
        else model = Instantiate(Resources.Load<GameObject>("Structures/Buildings/HQ_level_" + level.ToString()));
        model.transform.parent = transform;
        model.transform.localRotation = prevRot;
        model.transform.localPosition = Vector3.zero;
        if (!PoolMaster.useDefaultMaterials) PoolMaster.ReplaceMaterials(model);
    }

    public void SetLevel(byte x)
    {
        level = x;
        if (basement != null) SetModel();
    }
    override public void Prepare()
    {
        PrepareBuilding();
        switch (level)
        {
            case 1: energySurplus = 1f; energyCapacity = 100f; break;
            case 2: energySurplus = 3f; energyCapacity = 200f; break;
            case 3: energySurplus = 5f; energyCapacity = 400f; break;
            case 4: energySurplus = 12f; energyCapacity = 500f; break;
            case 5: energySurplus = 20f; energyCapacity = 600f; break;
            case 6: energySurplus = 25f; energyCapacity = 700f; ChangeUpgradedIndex(-1); break;
        }
        indestructible = true;
    }

    public override void SetBasement(Plane b, PixelPosByte pos)
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

        if (level > 3 )
        {
            if (!GameMaster.loading) SetBlockers();
            else
            {
                if (!subscribedToRestoreBlockersEvent)
                {
                    GameMaster.realMaster.blockersRestoreEvent += RestoreBlockers;
                    subscribedToRestoreBlockersEvent = true;
                }
            }
        }
    }
    private void SetBlockers()
    {
        if (basement != null)
        {
            var chunk = basement.myChunk;
            var cpos = basement.GetBlockingPosition();
            chunk.CreateBlocker(cpos, this, false,false);
            if (level == 6) chunk.CreateBlocker(cpos + basement.GetLookVector(), this, false, false);
        }
        else Debug.LogError("HQ cannot set blockers - no basement set");
    }
    public void RestoreBlockers()
    {
        if (subscribedToRestoreBlockersEvent)
        {
            if (level > 3) SetBlockers();
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
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
                        if (b.ID == WORKSHOP_ID) return true;
                    }
                }
                return false;
            case 3:
                if (colony.powerGrid.Count != 0)
                {
                    foreach (Building b in colony.powerGrid)
                    {
                        if (b.ID == GRPH_ENRICHER_3_ID) return true;
                    }
                }
                return false;
            case 4:
                {
                    if (colony.powerGrid.Count != 0)
                    {
                        foreach (Building b in colony.powerGrid)
                        {
                            if (b.ID == FUEL_FACILITY_ID) return true;
                        }
                    }
                    return false;
                }
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
                        if (basement.myChunk.GetBlock(basement.pos.OneBlockHigher()) != null)
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
                            if (basement.myChunk.GetBlock(basement.pos.TwoBlocksHigher()) != null)
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
                switch (level)
                {
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
    override public void LevelUp(bool returnToUI)
    {
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!colony.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                GameLogUI.NotEnoughResourcesAnnounce();
                return;
            }
        }
        if (level > 3) SetBlockers();
        level++;
        nextStageConditionMet = CheckUpgradeCondition();
        SetModel();
        buildingObserver.CheckUpgradeAvailability();
        Quest.ResetHousingQuest();
        GameLogUI.MakeAnnouncement(Localization.LevelReachedString(level));
        if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.HQ_Upgraded);
        GameMaster.realMaster.eventTracker?.BuildingUpgraded(this);
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
        if (subscribedToRestoreBlockersEvent)
        {
            GameMaster.realMaster.blockersRestoreEvent -= RestoreBlockers;
            subscribedToRestoreBlockersEvent = false;
        }
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SaveStructureData();
        data.AddRange(SaveBuildingData());
        data.Add(level);
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sb)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH + 1];
        fs.Read(data, 0, data.Length);

        //load structure data
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        // load building data
        energySurplus = System.BitConverter.ToSingle(data, STRUCTURE_SERIALIZER_LENGTH + 1);        
        //
        level = data[STRUCTURE_SERIALIZER_LENGTH + BUILDING_SERIALIZER_LENGTH];
        // >>
        SetBasement(sb, new PixelPosByte(data[0], data[1]));
        hp = System.BitConverter.ToSingle(data, 4);
        maxHp = System.BitConverter.ToSingle(data, 8);
    }
    #endregion
}