using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Settlement : House
{
    // изменение цены доп зданий в resource cost
    public enum SettlementStructureType : byte { Empty, House, Garden, Shop }
    private static float gardensCf = 0f, shopsCf = 0f;
    private static List<Settlement> settlements;

    public byte pointsFilled { get; private set; }
    public byte maxPoints { get; private set; }
    private float updateTimer = UPDATE_TIME;

    public const byte MAX_POINTS_COUNT = 60;
    private const byte MAX_HOUSING_LEVEL = 8;
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
            switch (level)
            {
                case 1: maxPoints = 16; break;
                case 2: maxPoints = 18; break;
                case 3: maxPoints = 20; break;
                case 4: maxPoints = 24;break;
                case 5: maxPoints = 28; break;
                case 6: maxPoints = 32;break;
                case 7: maxPoints = 36; break;
                case 8:maxPoints = 40; break;
                default: maxPoints = 12; break;
            }
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
        var map = new SettlementStructure[x, x];
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
                    xpos = s.surfaceRect.x / SettlementStructure.CELLSIZE;
                    zpos = s.surfaceRect.z / SettlementStructure.CELLSIZE;
                    map[xpos, zpos] = s2;
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
                    if (map[i,j] == null) emptyPositions.Add(new PixelPosByte(i * 2, j * 2));
                }
            }
            emptyPositions.Remove(new PixelPosByte(surfaceRect.x, surfaceRect.z));
            emptyPositions.Remove(new PixelPosByte(surfaceRect.x + 2, surfaceRect.z));
            emptyPositions.Remove(new PixelPosByte(surfaceRect.x, surfaceRect.z + 2));
            emptyPositions.Remove(new PixelPosByte(surfaceRect.x + 2, surfaceRect.z + 2));

            if (emptyPositions.Count > 0)
            {
                var chosenPosition = emptyPositions[Random.Range(0, emptyPositions.Count - 1)];
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
                    s2.SetData(chosenType, level, this);
                    s2.SetBasement(basement, chosenPosition);
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
