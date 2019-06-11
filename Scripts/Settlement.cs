using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Settlement : House
{
    // изменение цены доп зданий в resource cost
    public enum SettlementStructureType : byte { Empty, House, Garden, Shop, DoublesizeHouse,Extension, SystemBlocked}
    private static float gardensCf = 0f, shopsCf = 0f;
    private static List<Settlement> settlements;

    public byte pointsFilled { get; private set; }
    public byte maxPoints { get; private set; }
    private float updateTimer = UPDATE_TIME;

    public const byte MAX_POINTS_COUNT = 60;
    private const byte MAX_HOUSING_LEVEL = 8, FIRST_EXTENSION_LEVEL = 3, SECOND_EXTENSION_LEVEL = 6;
    private const float UPDATE_TIME = 1f, 
        SHOPS_DEMAND_CF = 0.02f, GARDENS_DEMAND_CF = 0.01f
        ;    

    static Settlement()
    {
        AddToResetList(typeof(Settlement));
        settlements = new List<Settlement>();
    }
    public static void ResetStaticData()
    {
        settlements = new List<Settlement>();
        gardensCf = 0f;
        shopsCf = 0f;
    }

   override protected void SetModel()
    {
        //switch skin index
        GameObject model;
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        switch (level)
        {
            case 6: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_6")); break;
            case 5: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_5")); break;
            case 4: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_4")); break;
            case 3: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_1")); break;
            case 2: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_2")); break;
            default: model = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_1")); break;
        }
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(model, true);
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        //#
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.lifepowerUpdateEvent += LifepowerUpdate;
            subscribedToUpdate = true;
            settlements.Add(this);
            maxPoints = GetMaxPoints();
        }
    }
    private byte GetMaxPoints()
    {
        switch (level)
        {
            case 1: return 16; 
            case 2: return 18; 
            case 3: return 20; 
            case 4: return 24; 
            case 5: return 28;
            case 6: return 32; 
            case 7: return 36; 
            case 8: return 40; 
            default: return 12; 
        }
    }

    public void LifepowerUpdate()
    {
        if (isEnergySupplied)
        {
            updateTimer -= GameMaster.LIFEPOWER_TICK;
            if (updateTimer <= 0)
            {
                if (pointsFilled < maxPoints) CreateNewBuilding(false);
                updateTimer = UPDATE_TIME * level;
            }
        }
    }

    public void CreateNewBuilding(bool forced)
    {
        if (basement == null)
        {
            print("settlement error");
            return;
        }

        // dependency : recalculate()
        int prevHousing = housing;
        float prevEnergySurplus = energySurplus;
        housing = 0;
        gardensCf = 0f;
        shopsCf = 0f;
        pointsFilled = 0;
        energySurplus = GetEnergySurplus(id);
        float onePartEnergyConsumption = GetEnergySurplus(SETTLEMENT_STRUCTURE_ID);

        int x = SurfaceBlock.INNER_RESOLUTION / SettlementStructure.CELLSIZE, xpos, zpos;
        var map = new SettlementStructureType[x, x];
        int centerPos = x / 2 - 1;
        map[centerPos, centerPos] = SettlementStructureType.SystemBlocked;
        map[centerPos, centerPos + 1] = SettlementStructureType.SystemBlocked;
        map[centerPos + 1, centerPos] = SettlementStructureType.SystemBlocked;
        map[centerPos + 1, centerPos + 1] = SettlementStructureType.SystemBlocked;
        if (basement.noEmptySpace != false)
        {
            SettlementStructure s2;
            foreach (var s in basement.structures)
            {
                if (s.id == SETTLEMENT_STRUCTURE_ID)
                {
                    s2 = (s as SettlementStructure);
                    var type = SettlementStructureType.Empty;
                    bool extended = false;
                    switch (s2.type)
                    {
                        case SettlementStructureType.House:
                            housing += (int)s2.value;
                            if (s2.level >= FIRST_EXTENSION_LEVEL)
                            {
                                type = SettlementStructureType.DoublesizeHouse;
                                extended = true;
                            }
                            break;
                        case SettlementStructureType.Garden: gardensCf += s2.value; break;
                        case SettlementStructureType.Shop: shopsCf += s2.value; break;
                    }
                    energySurplus += onePartEnergyConsumption;
                    xpos = s.surfaceRect.x / SettlementStructure.CELLSIZE;
                    zpos = s.surfaceRect.z / SettlementStructure.CELLSIZE;
                    map[xpos, zpos] = type;
                    if (extended)
                    { // 2 x 2
                        map[xpos + 1, zpos] = SettlementStructureType.Extension;
                        map[xpos + 1, zpos + 1] = SettlementStructureType.Extension;
                        map[xpos, zpos + 1] = SettlementStructureType.Extension;
                        pointsFilled += 3;
                        energySurplus += 3f * onePartEnergyConsumption;
                        if (s2.level >= SECOND_EXTENSION_LEVEL)
                        { // 3 x 3
                            map[xpos + 2, zpos] = SettlementStructureType.Extension;
                            map[xpos + 2, zpos + 1] = SettlementStructureType.Extension;
                            map[xpos + 2, zpos + 2] = SettlementStructureType.Extension;
                            map[xpos + 1, zpos + 2] = SettlementStructureType.Extension;
                            map[xpos , zpos + 2] = SettlementStructureType.Extension;
                            pointsFilled += 5;
                            energySurplus += 5f * onePartEnergyConsumption;
                        }
                    }
                    pointsFilled++;
                }
            }
        }
        //

        var colony = GameMaster.realMaster.colonyController;
        int citizensCount = colony.citizenCount;
        float neededShopsCf = citizensCount * SHOPS_DEMAND_CF, neededGardensCf = citizensCount * GARDENS_DEMAND_CF;
        bool needHousing = colony.totalLivespace <= citizensCount, needGardens = neededGardensCf >= 1f, needShops = neededShopsCf >= 1f;
        if (needHousing | needGardens | needShops | forced)
        {                          
            var emptyPositions = new List<PixelPosByte>();
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    if (map[i,j] == SettlementStructureType.Empty) emptyPositions.Add(new PixelPosByte(i, j));
                }
            }

            if (emptyPositions.Count > 0)
            {
                var chosenPosition = emptyPositions[Random.Range(0, emptyPositions.Count - 1)];
                xpos = chosenPosition.x; zpos = chosenPosition.y;
                var chosenType = SettlementStructureType.Empty;
                float f = Random.value;
                if (needHousing)
                {
                    if (needGardens)
                    {
                        if (needShops)
                        {
                            if (f <= 0.7f) chosenType = SettlementStructureType.House;
                            else
                            {
                                if (f > 0.85f) chosenType = SettlementStructureType.Shop;
                                else chosenType = SettlementStructureType.Garden;
                            }
                        }
                        else
                        {
                            if (f > 0.7f) chosenType = SettlementStructureType.Garden;
                            else chosenType = SettlementStructureType.House;
                        }
                    }
                    else chosenType = SettlementStructureType.House;
                }
                else
                {
                    if (needGardens)
                    {
                        if (needShops)
                        {
                            if (f > 0.5f) chosenType = SettlementStructureType.Shop;
                            else chosenType = SettlementStructureType.Garden;
                        }
                        else chosenType = SettlementStructureType.Garden;
                    }
                    else
                    {
                        if (needShops) chosenType = SettlementStructureType.Shop;
                    }
                }
                if (chosenType != SettlementStructureType.Empty)
                {
                    var s = GetStructureByID(SETTLEMENT_STRUCTURE_ID);
                    var s2 = (s as SettlementStructure);
                    byte buildLevel = level;
                    if (level >= FIRST_EXTENSION_LEVEL)
                    {
                        bool upperRowCheck = zpos + 1 < x,
                             lowerRowCheck = zpos - 1 >= 0,
                             rightColumnCheck = xpos + 1 < x,
                             leftColumnCheck = xpos - 1 >= 0;
                        if (level < SECOND_EXTENSION_LEVEL)
                        { // 2 x 2
                            var localMap = new bool[3, 3];
                            localMap[0, 0] = (leftColumnCheck & lowerRowCheck) && (map[xpos - 1, zpos - 1] == chosenType);
                            localMap[0, 1] = lowerRowCheck && (map[xpos, zpos - 1] == chosenType);
                            localMap[0, 2] = (rightColumnCheck & lowerRowCheck) && map[xpos + 1, zpos - 1] == chosenType;
                            localMap[1, 0] = leftColumnCheck && map[xpos - 1, zpos] == chosenType ;
                            localMap[1, 2] = rightColumnCheck && map[xpos + 1, zpos] == chosenType ;
                            localMap[2, 0] = (leftColumnCheck & upperRowCheck) && map[xpos - 1, zpos + 1] == chosenType ;
                            localMap[2, 1] = upperRowCheck && map[xpos, zpos + 1] == chosenType ;
                            localMap[2, 2] = (rightColumnCheck & upperRowCheck) && map[xpos + 1, zpos + 1] == chosenType ;

                            if (localMap[0,0] & localMap[0, 1] & localMap[1, 0])
                            {
                                xpos--; zpos--;
                                energySurplus -= 3 * onePartEnergyConsumption;
                            }
                            else
                            {
                                if (localMap[2,0]& localMap[1,0] & localMap[2,1])
                                {
                                    zpos--;
                                    energySurplus -= 3 * onePartEnergyConsumption;
                                }
                                else
                                {
                                    if (localMap[0,2] & localMap[0,1] & localMap[1,2])
                                    {
                                        xpos--; zpos++;
                                        energySurplus -= 3 * onePartEnergyConsumption;
                                    }
                                    else
                                    {
                                        if (localMap[2, 2] & localMap[2, 1] & localMap[1, 2])
                                        {     
                                            energySurplus -= 3 * onePartEnergyConsumption;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    s2.SetData(chosenType, buildLevel, this);
                    s2.SetBasement(basement, new PixelPosByte(xpos * SettlementStructure.CELLSIZE, zpos * SettlementStructure.CELLSIZE));

                    switch (s2.type)
                    {
                        case SettlementStructureType.House: housing += (int)s2.value; break;
                        case SettlementStructureType.Garden: gardensCf += s2.value; break;
                        case SettlementStructureType.Shop: shopsCf += s2.value; break;
                    }
                    energySurplus += onePartEnergyConsumption;
                    pointsFilled++;
                }
            }            
        }

        if (prevEnergySurplus != energySurplus) colony.RecalculatePowerGrid();
        if (prevHousing != housing) colony.RecalculateHousing();
    }
    public void Recalculate()
    {
        // dependency : create new building()
        int prevHousing = housing;
        float prevEnergySurplus = energySurplus;
        housing = 0;
        gardensCf = 0f;
        shopsCf = 0f;
        pointsFilled = 0;
        energySurplus = GetEnergySurplus(id);
        float onePartEnergyConsumption = GetEnergySurplus(SETTLEMENT_STRUCTURE_ID);

        if (basement.noEmptySpace != false)
        {
            SettlementStructure s2;
            foreach (var s in basement.structures)
            {
                if (s.id == SETTLEMENT_STRUCTURE_ID)
                {
                    s2 = (s as SettlementStructure);
                    switch (s2.type)
                    {
                        case SettlementStructureType.House: housing += (int)s2.value; break;
                        case SettlementStructureType.Garden: gardensCf += s2.value; break;
                        case SettlementStructureType.Shop: shopsCf += s2.value; break;
                    }
                    energySurplus += onePartEnergyConsumption;
                    pointsFilled++;
                }
            }
        }
    }

    public override UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
        return buildingObserver;
    }

    override public bool IsLevelUpPossible(ref string refusalReason)
    {
        if (level < GameMaster.realMaster.colonyController.hq.level)
        {
            if (pointsFilled == maxPoints)
            {
                refusalReason = Localization.GetPhrase(LocalizedPhrase.NoFreeSlots);
                return false;
            }
            else return true;
        }
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.Unavailable);
            return false;
        }
    }
    override public void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!GameMaster.realMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                GameLogUI.NotEnoughResourcesAnnounce();
                return;
            }
        }
        if (level < MAX_HOUSING_LEVEL)
        {
            level++;
            SetModel();
        }
        else
        {
            Building upgraded = GetStructureByID(HOUSE_BLOCK_ID) as Building;
            upgraded.SetBasement(basement, PixelPosByte.zero);
            if (returnToUI) upgraded.ShowOnGUI();
        }
    }
    override public ResourceContainer[] GetUpgradeCost()
    {
        ResourceContainer[] cost = ResourcesCost.GetSettlementUpgradeCost(level);
        float discount = GameMaster.realMaster.upgradeDiscount;
        for (int i = 0; i < cost.Length; i++)
        {
            cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * (1 - discount));
        }
        return cost;
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        GameMaster.realMaster.colonyController.DeleteHousing(this);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.lifepowerUpdateEvent -= LifepowerUpdate;
            subscribedToUpdate = false;
            settlements.Remove(this);
        }
        Destroy(gameObject);
    }
}
