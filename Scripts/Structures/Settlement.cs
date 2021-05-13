using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Settlement : House
{
    // изменение цены доп зданий в resource cost
    public enum SettlementStructureType : byte { Empty, House, Garden, Shop, DoublesizeHouse,Extension, SystemBlocked}
    public static byte maxAchievedLevel { get; private set; }
    private static float gardensCf = 0f, shopsCf = 0f;
    private static List<Settlement> settlements;

    private bool ignoreRecalculationRequests = false;
    public bool needRecalculation = false; // hot
    public byte pointsFilled { get; private set; }
    public byte maxPoints { get; private set; }
    private float updateTimer = UPDATE_TIME;

    public const byte MAX_POINTS_COUNT = 60;
    public const byte MAX_HOUSING_LEVEL = 8, FIRST_EXTENSION_LEVEL = 3, SECOND_EXTENSION_LEVEL = 6;
    private const float UPDATE_TIME = 10f, 
        SHOPS_DEMAND_CF = 0.02f, GARDENS_DEMAND_CF = 0.01f
        ;    

    static Settlement()
    {
        GameMaster.staticResetFunctions += ResetStaticData;
        settlements = new List<Settlement>();
        maxAchievedLevel = 1;
    }
    public static void ResetStaticData()
    {
        settlements = new List<Settlement>();
        gardensCf = 0f;
        shopsCf = 0f;
        maxAchievedLevel = 1;
    }
    public static void TotalRecalculation()
    {
        bool loading = GameMaster.loading;
        gardensCf = 0f; shopsCf = 0f;
        float centerEnergySurplus = GetEnergySurplus(SETTLEMENT_CENTER_ID),
            partEnergyConsumption = GetEnergySurplus(SETTLEMENT_STRUCTURE_ID);
        SettlementStructure s2;
        if (settlements.Count > 0)
        {            
            foreach (var center in settlements)
            {
                center.housing = 0;
                center.energySurplus = centerEnergySurplus;
                center.pointsFilled = 0;
                center.maxPoints = center.GetMaxPoints();

                if (center.basement.fulfillStatus != FullfillStatus.Empty)
                {
                    var slist = center.basement.GetStructuresList();
                    foreach (var s in slist)
                    {
                        if (s.ID == SETTLEMENT_STRUCTURE_ID)
                        {
                            s2 = (s as SettlementStructure);
                            if (loading) s2.AssignSettlement(center);
                            switch (s2.type)
                            {
                                case SettlementStructureType.House: center.housing += (int)s2.value; break;
                                case SettlementStructureType.Garden: gardensCf += s2.value; break;
                                case SettlementStructureType.Shop: shopsCf += s2.value; break;
                            }
                            if (s2.level < FIRST_EXTENSION_LEVEL)
                            {
                                center.pointsFilled++;
                                center.energySurplus += partEnergyConsumption * center.level;
                            }
                            else
                            {
                                if (s2.level >= SECOND_EXTENSION_LEVEL)
                                {
                                    center.pointsFilled += 9;
                                    center.energySurplus += partEnergyConsumption * center.level * 9;
                                }
                                else
                                {
                                    center.pointsFilled += 4;
                                    center.energySurplus += partEnergyConsumption * center.level * 4;
                                }
                            }
                            s2.SetActivationStatus(center.isEnergySupplied);
                        }
                    }
                }
                if (center.level > maxAchievedLevel) maxAchievedLevel = center.level;
            }
        }
        if (!loading) // wait for colony loading
        {
            var colony = GameMaster.realMaster.colonyController;
            colony.housingRecalculationNeeded = true;
            colony.powerGridRecalculationNeeded = true;
        }
    }

    public void SetLevel(byte i_level)
    {
        level = i_level;
        if (basement != null)
        {
            SetModel();
            maxPoints = GetMaxPoints();
            Recalculate();
        }
    }
   override protected void SetModel()
    {
        //switch skin index
        GameObject model;
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        model = GetSettlementSpire(level);
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
        
    }
    public static GameObject GetSettlementSpire(byte lvl)
    {
        GameObject m;
        switch (lvl)
        {
            case 8:
            case 7:
            case 6: m= Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_6")); break;
            case 5: m = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_5")); break;
            case 4: m = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_4")); break;
            case 3: m = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_3")); break;
            case 2: m = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_2")); break;
            default: m = Instantiate(Resources.Load<GameObject>("Structures/Settlement/settlementCenter_1")); break;
        }
        if (!PoolMaster.useDefaultMaterials) PoolMaster.ReplaceMaterials(m);
        return m;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        //#set house data
        SetBuildingData(b, pos);
        GameMaster.realMaster.colonyController.AddHousing(this);
        //#
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += LabourUpdate;
            subscribedToUpdate = true;
            settlements.Add(this);
            maxPoints = GetMaxPoints();
            Recalculate();
        }
        //
    }
    protected override void SwitchActivityState()
    {
        base.SwitchActivityState();
        var slist = basement.GetStructuresList();
        foreach (var s in slist)
        {
            if (s != null && s.ID == SETTLEMENT_STRUCTURE_ID)
            {
                (s as SettlementStructure).SetActivationStatus(isEnergySupplied);
            }
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

    public void LabourUpdate()
    {
        if (isEnergySupplied)
        {
            updateTimer -= GameMaster.LIFEPOWER_TICK;
            if (updateTimer <= 0)
            {
                if (pointsFilled < maxPoints) CreateNewBuilding(false);
                updateTimer = UPDATE_TIME;
                if (level > 1) updateTimer += UPDATE_TIME;
            }
        }
        if (needRecalculation)
        {
            Recalculate();
            needRecalculation = false;
        }
    }

    public void CreateNewBuilding(bool forced)
    {
        if (basement == null)
        {
            print("settlement error");
            return;
        }

        var colony = GameMaster.realMaster.colonyController;
        if (colony.energySurplus <= 0)
        {
            if (forced) AnnouncementCanvasController.MakeImportantAnnounce(Localization.GetPhrase(LocalizedPhrase.NotEnoughEnergySupply));
            return;
        }

        // dependency : recalculate()
        int prevHousing = housing;
        float prevEnergySurplus = energySurplus;
        housing = 0;
        pointsFilled = 0;
        energySurplus = GetEnergySurplus(ID);
        float onePartEnergyConsumption = GetEnergySurplus(SETTLEMENT_STRUCTURE_ID);

        int x = PlaneExtension.INNER_RESOLUTION / SettlementStructure.CELLSIZE, xpos, zpos;
        var map = new SettlementStructureType[x, x];
        int centerPos = x / 2 - 1;
        map[centerPos, centerPos] = SettlementStructureType.SystemBlocked;
        map[centerPos, centerPos + 1] = SettlementStructureType.SystemBlocked;
        map[centerPos + 1, centerPos] = SettlementStructureType.SystemBlocked;
        map[centerPos + 1, centerPos + 1] = SettlementStructureType.SystemBlocked;
        if (basement.fulfillStatus != FullfillStatus.Empty)
        {
            SettlementStructure s2;
            var slist = basement.GetStructuresList();
            foreach (var s in slist)
            {
                if (s.ID == SETTLEMENT_STRUCTURE_ID)
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
                            else type = SettlementStructureType.House;
                            break;
                        case SettlementStructureType.Garden:
                            gardensCf += s2.value;
                            type = SettlementStructureType.Garden;
                            break;
                        case SettlementStructureType.Shop:
                            shopsCf += s2.value;
                            type = SettlementStructureType.Shop;
                            break;
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
        int citizensCount = colony.citizenCount;
        float neededShopsCf = citizensCount * SHOPS_DEMAND_CF, neededGardensCf = citizensCount * GARDENS_DEMAND_CF;
        bool upSurface = basement.faceIndex == Block.SURFACE_FACE_INDEX | basement.faceIndex == Block.UP_FACE_INDEX;
        bool needHousing = colony.totalLivespace <= citizensCount, 
            needGardens = (neededGardensCf > gardensCf) & upSurface , 
            needShops = neededShopsCf > shopsCf & upSurface;
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
                var chosenPosition = emptyPositions[Random.Range(0, emptyPositions.Count)];                
                xpos = chosenPosition.x; zpos = chosenPosition.y;

                var chosenType = SettlementStructureType.Empty;
                float f = Random.value;
                if (needHousing)
                {
                    if (forced) chosenType = SettlementStructureType.House;
                    else
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
                }
                else
                {
                    if (forced) chosenType = SettlementStructureType.House;
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
                }

                if (chosenType != SettlementStructureType.Empty)
                {
                    var s = GetStructureByID(SETTLEMENT_STRUCTURE_ID);
                    var s2 = (s as SettlementStructure);
                    byte buildLevel = FIRST_EXTENSION_LEVEL - 1;
                    if (buildLevel > level) buildLevel = level;
                    if (level >= FIRST_EXTENSION_LEVEL & chosenType == SettlementStructureType.House)
                    {
                        bool upperRowCheck = zpos + 1 < x,
                             lowerRowCheck = zpos - 1 >= 0,
                             rightColumnCheck = xpos + 1 < x,
                             leftColumnCheck = xpos - 1 >= 0,
                             buildDoublesize = false
                             ;
                        if (level < SECOND_EXTENSION_LEVEL) {
                            // 2 x 2
                            var localMap = new bool[3, 3];
                            localMap[0, 0] = (leftColumnCheck & lowerRowCheck) && (map[xpos - 1, zpos - 1] == chosenType);
                            localMap[0, 1] = lowerRowCheck && (map[xpos, zpos - 1] == chosenType);
                            localMap[0, 2] = (rightColumnCheck & lowerRowCheck) && map[xpos + 1, zpos - 1] == chosenType;
                            localMap[1, 0] = leftColumnCheck && map[xpos - 1, zpos] == chosenType;
                            localMap[1, 2] = rightColumnCheck && map[xpos + 1, zpos] == chosenType;
                            localMap[2, 0] = (leftColumnCheck & upperRowCheck) && map[xpos - 1, zpos + 1] == chosenType;
                            localMap[2, 1] = upperRowCheck && map[xpos, zpos + 1] == chosenType;
                            localMap[2, 2] = (rightColumnCheck & upperRowCheck) && map[xpos + 1, zpos + 1] == chosenType;
                            if (localMap[0, 0] & localMap[0, 1] & localMap[1, 0])
                            {
                                xpos--; zpos--;
                                buildDoublesize = true;
                            }
                            else
                            {
                                if (localMap[2, 0] & localMap[1, 0] & localMap[2, 1])
                                {
                                    zpos--;
                                    buildDoublesize = true;
                                }
                                else
                                {
                                    if (localMap[0, 2] & localMap[0, 1] & localMap[1, 2])
                                    {
                                        xpos--; zpos++;
                                        buildDoublesize = true;
                                    }
                                    else buildDoublesize = localMap[2, 2] & localMap[2, 1] & localMap[1, 2];
                                }
                            }
                            if (buildDoublesize)
                            {
                                buildLevel = level < SECOND_EXTENSION_LEVEL ? level : (byte)(SECOND_EXTENSION_LEVEL - 1);
                            }
                        }
                        else
                        { // 3 x 3
                            var localMap = new bool[5, 5];
                            bool upperRowCheck2 = zpos + 2 < x,
                                lowerRowCheck2 = zpos - 2 >= 0,
                                rightColumnCheck2 = xpos + 2 < x,
                                leftColumnCheck2 = xpos - 2 >= 0;
                            SettlementStructureType exType = SettlementStructureType.DoublesizeHouse, mtype = SettlementStructureType.Empty; // изменить, если добавятся другие типы
                            { // собрано для удобного сворачивания
                                // x - 2
                                if (leftColumnCheck2)
                                {
                                    int x2 = xpos - 2;
                                    if (lowerRowCheck2)
                                    {
                                        mtype = map[x2, zpos - 2];
                                        if (mtype == chosenType) localMap[0, 0] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[0, 0] = true;
                                                localMap[0, 1] = true;
                                                localMap[1, 0] = true;
                                                localMap[1, 1] = true;
                                            }
                                        }
                                    }
                                    if (lowerRowCheck & !localMap[0, 1])
                                    {
                                        mtype = map[x2, zpos - 1];
                                        if (mtype == chosenType) localMap[0, 1] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[0, 1] = true;
                                                localMap[0, 2] = true;
                                                localMap[1, 1] = true;
                                                localMap[1, 2] = true;
                                            }
                                        }
                                    }
                                    if (!localMap[0, 2])
                                    {
                                        mtype = map[x2, zpos];
                                        if (mtype == chosenType) localMap[0, 2] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[0, 2] = true;
                                                localMap[0, 3] = true;
                                                localMap[1, 2] = true;
                                                localMap[1, 3] = true;
                                            }
                                        }
                                    }
                                    if (upperRowCheck & !localMap[0, 3])
                                    {
                                        mtype = map[x2, zpos + 1];
                                        if (mtype == chosenType) localMap[0, 3] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[0, 3] = true;
                                                localMap[0, 4] = true;
                                                localMap[1, 3] = true;
                                                localMap[1, 4] = true;
                                            }
                                        }
                                    }
                                    if (upperRowCheck2 & !localMap[0, 4])
                                    {
                                        localMap[0, 4] = map[x2, zpos + 2] == chosenType;
                                    }
                                }
                                // x - 1
                                if (leftColumnCheck)
                                {
                                    int x2 = xpos - 1;
                                    if (lowerRowCheck2 & !localMap[1, 0])
                                    {
                                        mtype = map[x2, zpos - 2];
                                        if (mtype == chosenType) localMap[1, 0] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[1, 0] = true;
                                                localMap[1, 1] = true;
                                                localMap[2, 0] = true;
                                                localMap[2, 1] = true;
                                            }
                                        }
                                    }
                                    // не сливать в одно условие, так как присваивание результата условия может перезаписать значение, установленное ранее
                                    if (!localMap[1, 1] & lowerRowCheck) localMap[1, 1] = (map[x2, zpos - 1] == chosenType);
                                    if (!localMap[1, 2]) localMap[1, 2] = (map[x2, zpos] == chosenType);
                                    if (upperRowCheck & !localMap[1, 3])
                                    {
                                        mtype = map[x2, zpos + 1];
                                        if (mtype == chosenType) localMap[1, 3] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[1, 3] = true;
                                                localMap[1, 4] = true;
                                                localMap[2, 3] = true;
                                                localMap[2, 4] = true;
                                            }
                                        }
                                    }
                                    if (upperRowCheck2 & !localMap[1, 4])
                                    {
                                        localMap[1, 4] = map[x2, zpos + 2] == chosenType;
                                    }
                                }
                                // x
                                if (lowerRowCheck2 & !localMap[2, 0])
                                {
                                    mtype = map[xpos, zpos - 2];
                                    if (mtype == chosenType) localMap[2, 0] = true;
                                    else
                                    {
                                        if (mtype == exType)
                                        {
                                            localMap[2, 0] = true;
                                            localMap[2, 1] = true;
                                            localMap[3, 0] = true;
                                            localMap[3, 1] = true;
                                        }
                                    }
                                }
                                if (lowerRowCheck & !localMap[2, 1])
                                {
                                    localMap[2, 1] = map[xpos, zpos - 1] == chosenType;
                                }
                                if (upperRowCheck & !localMap[2, 3])
                                {
                                    mtype = map[xpos, zpos + 1];
                                    if (mtype == chosenType) localMap[2, 3] = true;
                                    else
                                    {
                                        if (mtype == exType)
                                        {
                                            localMap[2, 3] = true;
                                            localMap[2, 4] = true;
                                            localMap[3, 3] = true;
                                            localMap[3, 4] = true;
                                        }
                                    }
                                }
                                if (upperRowCheck2 & !localMap[2, 4])
                                {
                                    localMap[2, 4] = map[xpos, zpos + 2] == chosenType;
                                }
                                // x + 1
                                if (rightColumnCheck)
                                {
                                    int x2 = xpos + 1;
                                    if (lowerRowCheck2 & !localMap[3, 0])
                                    {
                                        mtype = map[x2, zpos - 2];
                                        if (mtype == chosenType) localMap[3, 0] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[3, 0] = true;
                                                localMap[3, 1] = true;
                                                localMap[4, 0] = true;
                                                localMap[4, 1] = true;
                                            }
                                        }
                                    }
                                    if (lowerRowCheck & !localMap[3, 1])
                                    {
                                        mtype = map[x2, zpos - 1];
                                        if (mtype == chosenType) localMap[3, 1] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[3, 1] = true;
                                                localMap[3, 2] = true;
                                                localMap[4, 1] = true;
                                                localMap[4, 2] = true;
                                            }
                                        }
                                    }
                                    if (!localMap[3, 2])
                                    {
                                        mtype = map[x2, zpos];
                                        if (mtype == chosenType) localMap[3, 2] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[3, 2] = true;
                                                localMap[3, 3] = true;
                                                localMap[4, 2] = true;
                                                localMap[4, 3] = true;
                                            }
                                        }
                                    }
                                    if (upperRowCheck & !localMap[3, 3])
                                    {
                                        mtype = map[x2, zpos + 1];
                                        if (mtype == chosenType) localMap[3, 3] = true;
                                        else
                                        {
                                            if (mtype == exType)
                                            {
                                                localMap[3, 3] = true;
                                                localMap[3, 4] = true;
                                                localMap[4, 3] = true;
                                                localMap[4, 4] = true;
                                            }
                                        }
                                    }
                                    if (upperRowCheck2 & !localMap[3, 4])
                                    {
                                        localMap[3, 4] = map[x2, zpos + 2] == chosenType;
                                    }
                                }
                                // x + 2
                                if (rightColumnCheck2)
                                {
                                    int x2 = xpos + 2;
                                    if (lowerRowCheck2 & !localMap[4, 0])
                                    {
                                        localMap[4, 0] = map[x2, zpos - 2] == chosenType;
                                    }
                                    if (lowerRowCheck & !localMap[4, 1])
                                    {
                                        localMap[4, 1] = map[x2, zpos - 1] == chosenType;
                                    }
                                    if (!localMap[4, 2])
                                    {
                                        localMap[4, 2] = map[x2, zpos] == chosenType;
                                    }
                                    if (upperRowCheck & !localMap[4, 3])
                                    {
                                        localMap[4, 3] = map[x2, zpos + 1] == chosenType;
                                    }
                                    if (upperRowCheck2 & !localMap[4, 4])
                                    {
                                        localMap[4, 4] = map[x2, zpos + 2] == chosenType;
                                    }
                                }
                            }

                            bool buildTriplesize = false;
                            int doubleSizeX = xpos, doubleSizeZ = zpos;
                            if ( localMap[3, 2] ) { // проверка на центр правой стороны (3,2)
                                if (localMap[2, 3] & localMap[3, 3])
                                { // right - up (central check included)
                                    if (map[xpos, zpos + 1] == exType)
                                    {
                                        if (localMap[4, 4] & localMap[4, 3] & localMap[4, 2]) buildTriplesize = true;
                                    }
                                    else
                                    {
                                        if (map[xpos + 1, zpos + 1] == exType)
                                        {
                                            if (localMap[2, 4] & localMap[4, 2]) buildTriplesize = true;
                                        }
                                        else
                                        {
                                            if (map[xpos + 1, zpos] == exType)
                                            {
                                                if (localMap[2, 4] & localMap[3, 4] & localMap[4, 4]) buildTriplesize = true;
                                            }
                                            else
                                            { // no extended
                                                if (localMap[2, 4] & localMap[3, 4] & localMap[4, 4] & localMap[4, 3] & localMap[4, 2])
                                                {
                                                    buildTriplesize = true;
                                                }
                                                else {
                                                    if (localMap[2, 1] & localMap[3, 1] & localMap[4, 1])
                                                    {
                                                        buildTriplesize = true;
                                                        zpos--;
                                                    }
                                                    else buildDoublesize = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                { // right - bottom (central check included)
                                    if (localMap[2,1] & localMap[3,1])
                                    {
                                        if (lowerRowCheck2 && map[xpos, zpos - 2] == exType)
                                        {
                                            if (localMap[3, 2] & localMap[4, 2] & localMap[4, 1] & localMap[4, 0]) buildTriplesize = true;
                                        }
                                        else
                                        {
                                            if (lowerRowCheck2 && map[xpos + 1, zpos - 2] == exType)
                                            {
                                                if (localMap[2, 0] & localMap[2, 1] & localMap[3, 2] & localMap[3, 4]) buildTriplesize = true;
                                            }
                                            else
                                            {
                                                if (map[xpos + 1, zpos - 1] == exType)
                                                {
                                                    if (localMap[2, 1] & localMap[2, 0] & localMap[3, 0] & localMap[4, 0]) buildTriplesize = true;
                                                }
                                                else
                                                { // no extended
                                                    if (localMap[2, 0] & localMap[3, 0] & localMap[4, 0] & localMap[4, 1] & localMap[4, 2]) buildTriplesize = true;
                                                    else
                                                    {
                                                        buildDoublesize = true;
                                                        doubleSizeZ = zpos - 1;
                                                    }
                                                }
                                            }
                                        }
                                        if (buildTriplesize) zpos -= 2;
                                    }
                                }   
                            }
                            if (!buildTriplesize & localMap[1,2])
                            { // левая сторона
                                if (localMap[1,3] & localMap[2,3])
                                { // left - up
                                    if (leftColumnCheck2 && map[xpos - 2, zpos] == exType)
                                    {
                                        if (localMap[0, 4] & localMap[1, 4] & localMap[2, 4] & localMap[2, 3])
                                        {
                                            buildTriplesize = true;
                                            xpos -= 2;
                                        }
                                    }
                                    else
                                    {
                                        if (leftColumnCheck2 && map[xpos - 2, zpos + 1] == exType)
                                        {
                                            if (localMap[0,2] & localMap[1,2] & localMap[2,3] & localMap[2,4])
                                            {
                                                buildTriplesize = true;
                                                xpos -= 2;
                                            }
                                        }
                                        else
                                        {
                                            if (map[xpos - 1, zpos + 1] == exType)
                                            {
                                                if (localMap[0,2] & localMap[0,3] & localMap[0,4])
                                                {
                                                    buildTriplesize = true;
                                                    xpos --;
                                                }
                                            }
                                            else
                                            { // no extended
                                                if (localMap[0,2] & localMap[0,3] & localMap[0, 4] & localMap[1,4] & localMap[2,4])
                                                {
                                                    buildTriplesize = true;
                                                    xpos -= 2;
                                                }
                                                else
                                                {
                                                    buildDoublesize = true;
                                                    doubleSizeX = xpos - 1;
                                                    doubleSizeZ = zpos;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (localMap[1,1] & localMap[2,1])
                                    { // left - bottom
                                        if (leftColumnCheck2 && map[xpos - 2, zpos - 1] == exType)
                                        {
                                            if (localMap[0,0] & localMap[1,0] & localMap[2,0])
                                            {
                                                buildTriplesize = true;
                                                xpos -= 2;
                                                zpos -= 2;
                                            }                                            
                                        }
                                        else
                                        {
                                            if ( (leftColumnCheck2 & lowerRowCheck2) && map[xpos - 2, zpos - 2] == exType )
                                            {
                                                if (localMap[0,2] & localMap[2,0])
                                                {
                                                    buildTriplesize = true;
                                                    xpos -= 2;
                                                    zpos -= 2;
                                                }
                                            }
                                            else
                                            {
                                                if (lowerRowCheck2 && map[xpos - 1, zpos - 2] == exType)
                                                {
                                                    if (localMap[0,0] & localMap[0,1] & localMap[0, 2])
                                                    {
                                                        buildTriplesize = true;
                                                        xpos -= 2;
                                                        zpos -= 2;
                                                    }
                                                }
                                                else
                                                { // no extended
                                                    if (localMap[0,2] & localMap[0,1] & localMap[0,0] & localMap[1,0] & localMap[2,0])
                                                    {
                                                        buildTriplesize = true;
                                                        xpos -= 2;
                                                        zpos -= 2;
                                                    }
                                                    else
                                                    {
                                                        buildDoublesize = true;
                                                        doubleSizeX = xpos - 1;
                                                        doubleSizeZ = zpos - 1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (buildTriplesize & buildDoublesize) buildDoublesize = false;
                            if (buildDoublesize)
                            {
                                xpos = doubleSizeX;
                                zpos = doubleSizeZ;
                                buildLevel = SECOND_EXTENSION_LEVEL - 1;
                            }
                            else
                            {
                                if (buildTriplesize)
                                {
                                    buildLevel = level;
                                }
                            }
                        }
                    }
                    s2.SetData(chosenType, buildLevel, this);
                    s2.SetBasement(basement, new PixelPosByte(xpos * SettlementStructure.CELLSIZE, zpos * SettlementStructure.CELLSIZE));
                    Recalculate();
                }
            }            
        }        
    }
    private void Recalculate()
    {
        if (GameMaster.loading || ignoreRecalculationRequests) return; // wait for total recalculation
        // dependency : create new building()
        // dependecy : total recalculation()
        int prevHousing = housing;
        float prevEnergySurplus = energySurplus;
        housing = 0;
        gardensCf = 0f;
        shopsCf = 0f;
        pointsFilled = 0;
        energySurplus = GetEnergySurplus(ID);
        float onePartEnergyConsumption = GetEnergySurplus(SETTLEMENT_STRUCTURE_ID);

        if (basement.fulfillStatus != FullfillStatus.Empty)
        {
            SettlementStructure s2;
            var slist = basement.GetStructuresList();
            foreach (var s in slist)
            {
                if (s.ID == SETTLEMENT_STRUCTURE_ID)
                {
                    s2 = (s as SettlementStructure);
                    switch (s2.type)
                    {
                        case SettlementStructureType.House: housing += (int)s2.value; break;
                        case SettlementStructureType.Garden: gardensCf += s2.value; break;
                        case SettlementStructureType.Shop: shopsCf += s2.value; break;
                    }
                    if (s2.level < FIRST_EXTENSION_LEVEL)
                    {
                        pointsFilled++;
                        energySurplus += onePartEnergyConsumption * level;
                    }
                    else
                    {
                        if (s2.level >= SECOND_EXTENSION_LEVEL)
                        {
                            pointsFilled += 9;
                            energySurplus += onePartEnergyConsumption * level * 9;
                        }
                        else
                        {
                            pointsFilled += 4;
                            energySurplus += onePartEnergyConsumption * level * 4;
                        }
                    }
                    s2.SetActivationStatus(isEnergySupplied);
                }
            }
        }
        var colony = GameMaster.realMaster.colonyController;
        colony.powerGridRecalculationNeeded = true;
        colony.housingRecalculationNeeded = true;
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
        if (level < MAX_HOUSING_LEVEL) return true;
        else
        {
            if (pointsFilled != MAX_POINTS_COUNT)
            {
                refusalReason = Localization.GetRefusalReason(RefusalReason.MaxLevel);
                return false;
            }
            else
            {
                return true; // convert to cube
            }
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
                AnnouncementCanvasController.NotEnoughResourcesAnnounce();
                return;
            }
        }
        if (level < MAX_HOUSING_LEVEL)
        {
            level++;
            if (level > maxAchievedLevel) maxAchievedLevel = level;
            SetModel();
            maxPoints = GetMaxPoints();
            if (showOnGUI)
            {
               buildingObserver.StatusUpdate();
            }
            var rm = GameMaster.realMaster;
            rm.colonyController.housingRecalculationNeeded = true;
            rm.eventTracker?.BuildingUpgraded(this);
        }
        else
        {
            if (pointsFilled == MAX_POINTS_COUNT)
            {
                Building upgraded = GetStructureByID(HOUSE_BLOCK_ID) as Building;
                upgraded.SetBasement(basement, PixelPosByte.zero);
                if (returnToUI) upgraded.ShowOnGUI();
                GameMaster.realMaster.eventTracker?.BuildingUpgraded(upgraded);
            }
        }
        
    }
    override public ResourceContainer[] GetUpgradeCost()
    {
        ResourceContainer[] cost;
        if (level < MAX_HOUSING_LEVEL) {
            cost = ResourcesCost.GetSettlementUpgradeCost(level);
        }
        else
        {
            cost = ResourcesCost.GetCost(HOUSE_BLOCK_ID);
            float discount = GameMaster.realMaster.upgradeDiscount;
            for (int i = 0; i < cost.Length; i++)
            {
                cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * (1 - discount));
            }
        }
        return cost;
    }


    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        if (basement != null)
        {
            ignoreRecalculationRequests = true;
            var slist = basement.GetStructuresList();
            bool atLeastOneWasDestroyed = false;
            foreach (var s in slist)
            {
                var norder = order.ChangeMessageSending(false);
                if (s != null && s.ID == SETTLEMENT_STRUCTURE_ID)
                {
                    s.Annihilate(norder);
                    atLeastOneWasDestroyed = true;
                }
            }
            if (atLeastOneWasDestroyed) basement.extension?.RecalculateSurface();
            var colony = GameMaster.realMaster.colonyController;
            colony.housingRecalculationNeeded = true;
            colony.powerGridRecalculationNeeded = true;
        }
        PrepareBuildingForDestruction(order);        
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= LabourUpdate;
            subscribedToUpdate = false;
            settlements.Remove(this);
        }
        if (order.doSpecialChecks)
        {
            GameMaster.realMaster.colonyController.DeleteHousing(this);
            maxAchievedLevel = 1;
            if (settlements.Count > 0)
            {
                foreach (var s in settlements)
                {
                    if (s.level > maxAchievedLevel) maxAchievedLevel = s.level;
                }
            }
        }
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SaveStructureData();
        data.Add(isActive ? (byte)1 : (byte)0);
        data.Add(level);
        return data;
    }
    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + 2];
        fs.Read(data, 0, data.Length);
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        var ppos = new PixelPosByte(data[0], data[1]);        
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 12);

        SetActivationStatus(data[16] == 1, false);
        SetLevel(data[17]);
        SetBasement(sblock, ppos);        
    }
    #endregion
}
