using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class ColonyControllerSerializer
{
    public StorageSerializer storageSerializer;
    public float gears_coefficient, labourEfficientcy_coefficient,
    happiness_coefficient, health_coefficient, birthrateCoefficient, realBirthrate;

    public float energyStored, energyCrystalsCount;
    public WorksiteSerializer[] worksites;

    public int freeWorkers, citizenCount;
    public float peopleSurplus = 0, housingTimer = 0, starvationTimer, real_birthrate = 0;
}

public sealed class ColonyController : MonoBehaviour
{
    const float HOUSING_TIME = 5;
    const float HOUSE_PROBLEM_HAPPINESS_LIMIT = 0.3f, FOOD_PROBLEM_HAPPINESS_LIMIT = 0.1f, // happiness wouldnt raised upper this level if condition is not met
    HEALTHCARE_PROBLEM_HAPPINESS_LIMIT = 0.5f;

    public string cityName { get; private set; }
    public Storage storage { get; private set; }
    public HeadQuarters hq { get; private set; }
    public float gears_coefficient; // hot
    public float hospitals_coefficient { get; private set; }
    public float labourEfficientcy_coefficient { get; private set; }
    public float happiness_coefficient { get; private set; }
    public float health_coefficient { get; private set; }
    public bool accumulateEnergy = true;

    public float energyStored { get; private set; }
    public float energySurplus { get; private set; }
    public float totalEnergyCapacity { get; private set; }
    public float energyCrystalsCount { get; private set; }
    public List<Building> powerGrid { get; private set; }
    public List<Dock> docks { get; private set; }
    public List<House> houses { get; private set; }
    public byte docksLevel { get; private set; }
    public float housingLevel { get; private set; }

    public int freeWorkers { get; private set; }
    public int citizenCount { get; private set; }
    float birthrateCoefficient;
    public float realBirthrate { get; private set; }
    float peopleSurplus = 0, housingTimer = 0;
    public int totalLivespace { get; private set; }
    List<Hospital> hospitals;
    private float starvationTimer;
    private bool thisIsFirstSet = true, ignoreHousingRequest = false, temporaryHousing = false;
    private const byte MAX_HOUSING_LEVEL = 5;


    void Awake()
    {
        if (thisIsFirstSet)
        {
            gears_coefficient = 2;
            labourEfficientcy_coefficient = 1;
            health_coefficient = 1;
            hospitals_coefficient = 0;
            birthrateCoefficient = GameConstants.START_BIRTHRATE_COEFFICIENT;
            docksLevel = 0;
            energyCrystalsCount = 1000;

            cityName = "default city"; // lol
        }
        houses = new List<House>();
        powerGrid = new List<Building>();
        docks = new List<Dock>();
    }
    public void ResetToDefaults()
    {
        hq = null;
        powerGrid.Clear();
        docks.Clear();
        houses.Clear();
        if (hospitals != null) hospitals.Clear();
    }

    public void Prepare()
    { // call from game master
        if (storage == null) storage = gameObject.AddComponent<Storage>();
        GameMaster.realMaster.SetColonyController(this);
        UIController.current.Prepare();
    }

    #region updating
    void Update()
    {
        if (GameMaster.gameSpeed == 0 | hq == null | GameMaster.loading) return;
        float t = Time.deltaTime * GameMaster.gameSpeed;

        if (gears_coefficient > 1)
        {
            GameMaster gm = GameMaster.realMaster;
            gears_coefficient -= gm.gearsDegradeSpeed * t * (2 - gm.environmentMaster.environmentalConditions);
        }
        gears_coefficient = Mathf.Clamp(gears_coefficient, GameConstants.GEARS_LOWER_LIMIT, GameConstants.GEARS_UP_LIMIT);
        // ENERGY CONSUMPTION
        {
            if (energySurplus > 0)
            {
                if (accumulateEnergy)
                {
                    energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
                    if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
                }
            }
            else
            {
                energyStored += energySurplus * Time.deltaTime * GameMaster.gameSpeed;
                if (energyStored < 0)
                { // отключение потребителей энергии до выравнивания
                    UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.PowerFailure));
                    energyStored = 0;
                    int i = powerGrid.Count - 1;
                    bool powerGridChanged = false;
                    while (i >= 0 & energySurplus < 0)
                    {
                        Building b = powerGrid[i];
                        if (b == null)
                        {
                            powerGrid.RemoveAt(i);
                            powerGridChanged = true;
                        }
                        else
                        {
                            if (b.isActive & b.energySurplus < 0)
                            {
                                energySurplus -= b.energySurplus;
                                b.SetEnergySupply(false, false);                                
                                powerGridChanged = true;
                            }                         
                        }
                        i--;
                    }
                    if (powerGridChanged) RecalculatePowerGrid();
                }
            }
        }
        //   STARVATION PROBLEM
        float foodSupplyHappiness = 1;
        {
            if (starvationTimer > 0)
            {
                starvationTimer -= t;
                if (starvationTimer < 0)
                {
                    if (citizenCount == 1)
                    {
                        citizenCount = 0;
                        PoolMaster.current.CitizenLeaveEffect(hq.transform.position);
                        GameMaster.realMaster.MakeGameOver(Localization.GetDefeatReason(DefeatReason.NoCitizen));
                        return;
                    }
                    else
                    {
                        starvationTimer = GameConstants.STARVATION_TIME;
                        if (freeWorkers > 0) { freeWorkers--; citizenCount--; }
                        else StartCoroutine(DeportateWorkingCitizen());
                        PoolMaster.current.CitizenLeaveEffect(houses[(int)(Random.value * (houses.Count - 1))].transform.position);
                    }
                }
                foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS_LIMIT;
                realBirthrate = 0;
            }
            else
            {
                float monthFoodReserves = citizenCount * GameConstants.FOOD_CONSUMPTION * GameMaster.DAYS_IN_MONTH;
                foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS_LIMIT + (1 - FOOD_PROBLEM_HAPPINESS_LIMIT) * (storage.standartResources[ResourceType.FOOD_ID] / monthFoodReserves);
            }
        }
        //HOUSING PROBLEM
        float housingHappiness = 1;
        {
            housingTimer -= t;
            if (housingTimer <= 0)
            {
                if (temporaryHousing | citizenCount > totalLivespace)
                {
                    RecalculateHousing();
                }
                housingTimer = HOUSING_TIME;
            }
            if (housingLevel == 0)
            {
                housingHappiness = HOUSE_PROBLEM_HAPPINESS_LIMIT;
            }
            else
            {
                if (totalLivespace < citizenCount)
                {
                    float demand = citizenCount - totalLivespace;
                    housingHappiness = HOUSE_PROBLEM_HAPPINESS_LIMIT * ( 2 - demand / citizenCount);
                }
                else
                {
                    byte l = hq.level;
                    if (l > MAX_HOUSING_LEVEL) l = MAX_HOUSING_LEVEL;
                    housingHappiness = housingLevel / l;
                }
            }
            housingHappiness = Mathf.Clamp(housingHappiness, HOUSE_PROBLEM_HAPPINESS_LIMIT, 1);
        }
        //HEALTHCARE
        if (health_coefficient < 1 && hospitals_coefficient > 0)
        {
            health_coefficient += hospitals_coefficient * t * gears_coefficient * 0.001f;
        }
        float healthcareHappiness = HEALTHCARE_PROBLEM_HAPPINESS_LIMIT + (1 - HEALTHCARE_PROBLEM_HAPPINESS_LIMIT) * hospitals_coefficient;
        healthcareHappiness *= health_coefficient;
        // HAPPINESS CALCULATION
        happiness_coefficient = 1;
        if (housingHappiness < happiness_coefficient) happiness_coefficient = housingHappiness;
        if (healthcareHappiness < happiness_coefficient) happiness_coefficient = healthcareHappiness;
        if (foodSupplyHappiness < happiness_coefficient) happiness_coefficient = foodSupplyHappiness;
        happiness_coefficient = Mathf.Clamp01(happiness_coefficient);

        //  BIRTHRATE
        {
            if (birthrateCoefficient != 0 & starvationTimer <= 0)
            {
                realBirthrate = birthrateCoefficient * Hospital.hospital_birthrate_coefficient * health_coefficient * happiness_coefficient * (1 + storage.standartResources[ResourceType.FOOD_ID] / 500f) * t;
                if (peopleSurplus > 1)
                {
                    int newborns = (int)peopleSurplus;
                    AddCitizens(newborns);
                    peopleSurplus -= newborns;
                }
            }
            peopleSurplus += realBirthrate;
        }
    }
    public void EverydayUpdate()
    {
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            float foodConsumption = GameConstants.FOOD_CONSUMPTION * citizenCount;
            float foodDemand = foodConsumption - storage.GetResources(ResourceType.Food, foodConsumption);
            if (foodDemand > 0)
            {
                foodDemand -= storage.GetResources(ResourceType.Supplies, foodDemand);
                if (foodDemand > 0)
                {
                    if (starvationTimer <= 0)
                    {
                        starvationTimer = GameConstants.STARVATION_TIME;
                        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughFood), Color.red);
                    }
                }
                else starvationTimer = 0;
            }
            else starvationTimer = 0;
        }
    }
    #endregion

    public void AddCitizens(int x)
    {
        citizenCount += x;
        freeWorkers += x;
    }
    private IEnumerator DeportateWorkingCitizen() // чтобы не тормозить апдейт
    {
        bool found = false;
        Block[,,] blocks = GameMaster.realMaster.mainChunk.blocks;
        SurfaceBlock sb = null;
        WorkBuilding wb = null;
        foreach (Block b in blocks)
        {
            sb = b as SurfaceBlock;
            if (sb == null || sb.cellsStatus == 0) continue;
            foreach (Structure s in sb.surfaceObjects)
            {
                wb = s as WorkBuilding;
                if (wb == null || wb.workersCount == 0) continue;
                wb.FreeWorkers(1);// knock-knock, whos there? ME ME ME ME ME HAHAHA
                found = true;
                break;
            }
        }
        if (found)
        {
            if (freeWorkers > 0)
            {
                freeWorkers--;
                citizenCount--;
            }
        }
        else
        {
            if (Worksite.worksitesList.Count > 0)
            {
                foreach (Worksite w in Worksite.worksitesList)
                {
                    if (w == null || w.workersCount == 0) continue;
                    else
                    {
                        w.FreeWorkers();
                        if (freeWorkers > 0)
                        {
                            freeWorkers--;
                            citizenCount--;
                        }
                        break;
                    }
                }
            }
        }

        yield return null;
    }

    public void AddWorkers(int x)
    {
        freeWorkers += x;
    }
    public void SendWorkers(int x, WorkBuilding w)
    {
        if (freeWorkers == 0 | w == null) return;
        if (x > freeWorkers) x = freeWorkers;
        freeWorkers = freeWorkers - x + w.AddWorkers(x);
    }
    public void SendWorkers(int x, Worksite w)
    {
        if (freeWorkers == 0 | w == null) return;
        if (x > freeWorkers) x = freeWorkers;
        freeWorkers = freeWorkers - x + w.AddWorkers(x);
    }

    #region AddingToLists

    public void AddHousing(House h)
    {
        if (ignoreHousingRequest) return;
        if (houses.Count > 0)
        {
            foreach (House eh in houses)
            {
                if (eh == h) return;
            }
        }
        houses.Add(h);
        RecalculateHousing();
    }
    public void DeleteHousing(House h)
    {
        if (ignoreHousingRequest) return;
        int i = 0;
        while (i < houses.Count)
        {
            if (houses[i] == h)
            {
                houses.RemoveAt(i);
                RecalculateHousing();
                break;
            }
            else i++;
        }
    }
    public void RecalculateHousing()
    {
        totalLivespace = 0;
        housingLevel = 0;
        if (hq == null) return;

        int[] housingVolumes = new int[MAX_HOUSING_LEVEL + 1];

        int i = 0, objectHousing = 0;
        byte objectLevel = 0;
        House h = null;
        ignoreHousingRequest = true;
        while (i < houses.Count)
        {
            h = houses[i];
            if (h == null)
            {
                houses.RemoveAt(i);
                continue;
            }
            else
            {
                objectHousing = h.housing;
                objectLevel = h.level;
                i++;
                if (objectLevel == 0)
                {
                    housingVolumes[0] += objectHousing;
                }
                else
                {
                    if (h.isActive & h.isEnergySupplied)
                    {
                        totalLivespace += objectHousing;
                        if (objectLevel < MAX_HOUSING_LEVEL) housingVolumes[objectLevel] += objectHousing;
                        else housingVolumes[MAX_HOUSING_LEVEL] += objectHousing;
                    }
                }
            }
        }

        int housingDemand = citizenCount - totalLivespace;
        int temporaryHousingValue = housingVolumes[0];
        int newTentsCount = 0;

        if (housingDemand > 0)
        {// недостаток жилья
            if (temporaryHousingValue - housingDemand >= House.TENT_VOLUME)
            { // но есть временное жилье, которого даже больше, чем нужно
                newTentsCount = -1 * Mathf.CeilToInt((temporaryHousingValue - housingDemand) / House.TENT_VOLUME );
            }
            else
            { // даже если временное есть, его недостаточно
                newTentsCount = Mathf.CeilToInt((housingDemand - temporaryHousingValue) / House.TENT_VOLUME);
            }
        }
        else
        {
            if (housingDemand < 0 & temporaryHousingValue > 0) // избыток жилья и еще стоит временное
            {
                housingDemand *= -1;
                newTentsCount = -1 * temporaryHousingValue / House.TENT_VOLUME;
            }
        }
        totalLivespace += temporaryHousingValue;
        if (newTentsCount != 0)
        {
            if (newTentsCount > 0)
            {// добавление новых палаток
                int step = 1, xpos, zpos;
                xpos = hq.basement.pos.x; zpos = hq.basement.pos.z;
                Chunk colonyChunk = hq.basement.myChunk;
                while (step < Chunk.CHUNK_SIZE / 2 & newTentsCount > 0)
                {
                    for (int n = 0; n < (step * 2 + 1); n++)
                    {
                        SurfaceBlock correctSurface = colonyChunk.GetSurfaceBlock(xpos + step - n, zpos + step);
                        if (correctSurface == null)
                        {
                            correctSurface = colonyChunk.GetSurfaceBlock(xpos + step - n, zpos - step);
                        }
                        if (correctSurface != null)
                        {
                            List<PixelPosByte> positions = correctSurface.GetRandomCells(newTentsCount);
                            if (positions.Count > 0)
                            {
                                newTentsCount -= positions.Count;
                                for (int j = 0; j < positions.Count; j++)
                                {
                                    House tent = Structure.GetStructureByID(Structure.TENT_ID) as House;
                                    tent.SetBasement(correctSurface, positions[j]);
                                    houses.Add(tent);
                                    housingVolumes[0] += tent.housing;
                                    totalLivespace += tent.housing;
                                }
                            }
                        }
                    }
                    step++;
                }
            }
            else
            { // удаление палаток
                i = 0;
                h = null;
                while (i < houses.Count & newTentsCount < 0)
                {
                    h = houses[i];
                    if (h != null && h.level == 0)
                    {                        
                        housingVolumes[0] -= h.housing;
                        totalLivespace -= h.housing;
                        h.Annihilate(false);
                        houses.RemoveAt(i);
                        newTentsCount++;
                    }
                    else i++;
                }
            }
        }
        ignoreHousingRequest = false;
        temporaryHousing = (housingVolumes[0] > 0);

        int allLivespace = totalLivespace;
        float usingLivespace = 0;
        // принимается, что все расселены от максимального уровня к минимальному
        if (housingVolumes[5] >= allLivespace)
        {
            housingLevel = 5;
        }
        else // можно и рекурсией
        {
            allLivespace -= housingVolumes[5];
            usingLivespace += housingVolumes[5];
            if (housingVolumes[4] >= allLivespace)
            {
                housingVolumes[4] = allLivespace;
                usingLivespace += allLivespace;
                allLivespace = 0;
                housingVolumes[3] = 0;
                housingVolumes[2] = 0;
                housingVolumes[1] = 0;
                housingVolumes[0] = 0;
            }
            else
            {
                allLivespace -= housingVolumes[4];
                usingLivespace += housingVolumes[4];

                if (housingVolumes[3] >= allLivespace)
                {
                    housingVolumes[3] = allLivespace;
                    usingLivespace += allLivespace;
                    allLivespace = 0;
                    housingVolumes[2] = 0;
                    housingVolumes[1] = 0;
                    housingVolumes[0] = 0;
                }
                else
                {
                    allLivespace -= housingVolumes[3];
                    usingLivespace += housingVolumes[3];

                    if (housingVolumes[2] >= allLivespace)
                    {
                        housingVolumes[2] = allLivespace;
                        usingLivespace += allLivespace;
                        allLivespace = 0;
                        housingVolumes[1] = 0;
                        housingVolumes[0] = 0;
                    }
                    else
                    {
                        allLivespace -= housingVolumes[2];
                        usingLivespace += housingVolumes[2];

                        if (housingVolumes[1] >= allLivespace)
                        {
                            housingVolumes[1] = allLivespace;
                            usingLivespace += allLivespace;
                            allLivespace = 0;
                            housingVolumes[0] = 0;
                        }
                        else
                        {
                            allLivespace -= housingVolumes[1];
                            usingLivespace += housingVolumes[1];

                            if (housingVolumes[0] >= allLivespace)
                            {
                                housingVolumes[0] = allLivespace;
                                usingLivespace += allLivespace;
                                allLivespace = 0;
                            }
                            else
                            {
                                allLivespace -= housingVolumes[0];
                                usingLivespace += housingVolumes[0];
                            }
                        }
                    }
                }
            }
        }

        housingLevel = housingVolumes[5] / usingLivespace * 5 + housingVolumes[4] / usingLivespace * 4 + housingVolumes[3] / usingLivespace * 3 + housingVolumes[2] / usingLivespace * 2 + housingVolumes[1] / usingLivespace;
    }

    public void AddHospital(Hospital h)
    {
        if (h == null) return;
        if (hospitals == null) hospitals = new List<Hospital>();
        if (hospitals.Count > 0)
        {
            foreach (Hospital eh in hospitals)
            {
                if (eh == h) return;
            }
        }
        hospitals.Add(h);
        RecalculateHospitals();
    }
    public void DeleteHospital(Hospital h)
    {
        int i = 0;
        while (i < hospitals.Count)
        {
            if (hospitals[i] == h)
            {
                hospitals.RemoveAt(i);
                RecalculateHospitals();
                break;
            }
            else i++;
        }
    }
    public void RecalculateHospitals()
    {
        hospitals_coefficient = 0;
        if (hospitals.Count == 0) return;
        int i = 0;
        float hospitalsCoverage = 0;
        while (i < hospitals.Count)
        {
            if (hospitals[i].isActive) hospitalsCoverage += hospitals[i].coverage;
            i++;
        }
        if (citizenCount != 0)
        {
            hospitals_coefficient = (float)hospitalsCoverage / (float)citizenCount;
        }
        else hospitals_coefficient = 0;
    }

    public void AddToPowerGrid(Building b)
    {
        if (b == null) return;
        int i = 0;
        while (i < powerGrid.Count)
        {
            if (powerGrid[i] == null)
            {
                powerGrid.RemoveAt(i);
                continue;
            }
            else
            {
                if (powerGrid[i] == b) return;
                i++;
            }
        }
        powerGrid.Add(b);
        b.SetEnergySupply(true, false);
        RecalculatePowerGrid();
    }
    public void DisconnectFromPowerGrid(Building b)
    {
        if (b == null) return;
        int i = 0;
        while (i < powerGrid.Count)
        {
            if (powerGrid[i] == null) { powerGrid.RemoveAt(i); continue; }
            if (powerGrid[i] == b)
            {
                b.SetEnergySupply(false, false);
                powerGrid.RemoveAt(i);
                RecalculatePowerGrid();
                return;
            }
            else i++;
        }
    }
    public void RecalculatePowerGrid()
    {
        energySurplus = 0; totalEnergyCapacity = 0;
        if (powerGrid.Count == 0) return;
        int i = 0;
        while (i < powerGrid.Count)
        {
            if (powerGrid[i] == null)
            {
                powerGrid.RemoveAt(i);
                continue;
            }
            else
            {
                if (powerGrid[i].energySurplus >= 0)
                { //producent
                    energySurplus += powerGrid[i].energySurplus;
                    totalEnergyCapacity += powerGrid[i].energyCapacity;
                }
                else
                { // consument
                    if (powerGrid[i].isActive)
                    {
                        totalEnergyCapacity += powerGrid[i].energyCapacity;
                        if (powerGrid[i].isEnergySupplied)
                        {
                            energySurplus += powerGrid[i].energySurplus;
                        }
                        else
                        {
                            if (powerGrid[i].energySurplus < 0 & energyStored >= Mathf.Abs(powerGrid[i].energySurplus) * 2)
                            {
                                powerGrid[i].SetEnergySupply(true, false);
                                energySurplus += powerGrid[i].energySurplus;
                            }
                        }
                    }
                }
                i++;
            }           
        }
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
    }
    public void AddEnergy(float f)
    {
        energyStored += f;
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
    }

    public void AddDock(Dock d)
    {
        if (d == null) return;
        if (docks.Count > 0)
        {
            foreach (Dock ed in docks)
            {
                if (ed == d) return;
            }
        }
        docks.Add(d);
        if (d.level > docksLevel) docksLevel = d.level;
    }
    public void RemoveDock(Dock d)
    {
        if (d == null || docks.Count == 0) return;
        int i = 0;
        while (i < docks.Count)
        {
            if (docks[i] == d)
            {
                docks.RemoveAt(i);
                return;
            }
            i++;
        }
    }
    #endregion

    public void SetHQ(HeadQuarters new_hq)
    {
        if (new_hq != null) hq = new_hq;
        QuestUI.current.StartCoroutine(QuestUI.current.WaitForNewQuest(0));
    }

    public void AddEnergyCrystals(float v)
    {
        if (v <= 0) return;
        energyCrystalsCount += v;
        if (v > 1) UIController.current.MoneyChanging(v);
    }

    /// <summary>
    /// returns the available residue of asked sum
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
	public float GetEnergyCrystals(float v)
    {
        if (v > energyCrystalsCount) { v = energyCrystalsCount; energyCrystalsCount = 0; }
        else energyCrystalsCount -= v;
        if (v >= 1) UIController.current.MoneyChanging(-v);
        return v;
    }

    #region save-load system
    public ColonyControllerSerializer Save()
    {
        ColonyControllerSerializer ccs = new ColonyControllerSerializer();
        ccs.storageSerializer = storage.Save();
        ccs.gears_coefficient = gears_coefficient;
        ccs.labourEfficientcy_coefficient = labourEfficientcy_coefficient;
        ccs.happiness_coefficient = happiness_coefficient;
        ccs.health_coefficient = health_coefficient;
        ccs.birthrateCoefficient = birthrateCoefficient;

        ccs.energyStored = energyStored;
        ccs.energyCrystalsCount = energyCrystalsCount;
        ccs.worksites = Worksite.StaticSave();
        ccs.freeWorkers = freeWorkers;
        ccs.citizenCount = citizenCount;
        ccs.peopleSurplus = peopleSurplus;
        ccs.housingTimer = housingTimer;
        ccs.starvationTimer = starvationTimer;
        ccs.real_birthrate = realBirthrate;
        ccs.birthrateCoefficient = birthrateCoefficient;
        return ccs;
    }
    public void Load(ColonyControllerSerializer ccs)
    {
        if (storage == null) storage = gameObject.AddComponent<Storage>();
        storage.Load(ccs.storageSerializer);
        gears_coefficient = ccs.gears_coefficient;
        labourEfficientcy_coefficient = ccs.labourEfficientcy_coefficient;
        happiness_coefficient = ccs.happiness_coefficient;
        health_coefficient = ccs.health_coefficient;
        birthrateCoefficient = ccs.birthrateCoefficient;

        energyStored = ccs.energyStored;
        energyCrystalsCount = ccs.energyCrystalsCount;
        if (ccs.worksites.Length > 0) Worksite.StaticLoad(ccs.worksites);
        freeWorkers = ccs.freeWorkers;
        citizenCount = ccs.citizenCount;
        peopleSurplus = ccs.peopleSurplus;
        housingTimer = ccs.housingTimer;
        starvationTimer = ccs.starvationTimer;
        realBirthrate = ccs.realBirthrate;
        birthrateCoefficient = ccs.birthrateCoefficient;
        RecalculatePowerGrid();
        RecalculateHousing();
        if (hospitals != null) RecalculateHospitals();
    }
    #endregion

}