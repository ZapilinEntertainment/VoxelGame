using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class ColonyController : MonoBehaviour
{
    const float HOUSING_TIME = 5;
    const float HOUSING_PROBLEM_HAPPINESS = 0.3f, FOOD_PROBLEM_HAPPINESS = 0.1f, // happiness wouldnt raised upper this level if condition is not met
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
    private float starvationTimer, targetHappiness;
    private bool thisIsFirstSet = true, ignoreHousingRequest = false, temporaryHousing = false, housingCountChanges = false;

    public const byte MAX_HOUSING_LEVEL = 5;
    private const float START_ENERGY_CRYSTALS_COUNT = 100;


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
            energyCrystalsCount = START_ENERGY_CRYSTALS_COUNT;

            cityName = "My Colony"; 
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
                    UIController.current.StartPowerFailureTimer();
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

        float happinessIncreaseMultiplier = 1, happinessDecreaseMultiplier = 1;
        //   STARVATION PROBLEM
        float foodSupplyHappiness = 0;
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
                        GameMaster.realMaster.GameOver(GameEndingType.ColonyLost);
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
                foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS;
                happinessDecreaseMultiplier++;
                realBirthrate = 0;
            }
            else
            {
                float monthFoodReserves = citizenCount * GameConstants.FOOD_CONSUMPTION * GameMaster.DAYS_IN_MONTH;
                foodSupplyHappiness = FOOD_PROBLEM_HAPPINESS + (1 - FOOD_PROBLEM_HAPPINESS) * (storage.standartResources[ResourceType.FOOD_ID] / monthFoodReserves);
                if (foodSupplyHappiness > 1)
                {
                    foodSupplyHappiness = 1;
                    happinessIncreaseMultiplier++;
                }
            }
        }
        //
        float lvlCf = 1 - (hq.level - 1) / (GameConstants.HQ_MAX_LEVEL - 1);
        //HOUSING PROBLEM
        float housingHappiness = 0;
        {
            housingTimer -= t;
            if (housingTimer <= 0)
            {
                if (temporaryHousing | housingCountChanges | citizenCount > totalLivespace)
                {
                    RecalculateHousing();
                }
                housingTimer = HOUSING_TIME;
            }
            if (housingLevel == 0)
            {
                housingHappiness = HOUSING_PROBLEM_HAPPINESS * lvlCf;
                happinessDecreaseMultiplier++;
            }
            else
            {
                byte l = hq.level;
                if (l > MAX_HOUSING_LEVEL) l = MAX_HOUSING_LEVEL;
                housingHappiness = housingLevel / l * (1 - HOUSING_PROBLEM_HAPPINESS * lvlCf) + HOUSING_PROBLEM_HAPPINESS * lvlCf;
            }
        }
        //HEALTHCARE
        if (health_coefficient < 1 && hospitals_coefficient > 0)
        {
            health_coefficient += hospitals_coefficient * t * gears_coefficient * 0.001f;
        }
        float healthcareHappiness = HEALTHCARE_PROBLEM_HAPPINESS_LIMIT * lvlCf + (1 - HEALTHCARE_PROBLEM_HAPPINESS_LIMIT * lvlCf) * hospitals_coefficient;
        healthcareHappiness *= health_coefficient;
        // HAPPINESS CALCULATION
        targetHappiness = 1;
        if (housingHappiness < targetHappiness) targetHappiness = housingHappiness;
        if (healthcareHappiness < targetHappiness) targetHappiness = healthcareHappiness;
        if (foodSupplyHappiness < targetHappiness) targetHappiness = foodSupplyHappiness;
        if (happiness_coefficient != targetHappiness)
        {
            if (happiness_coefficient > targetHappiness) happiness_coefficient = Mathf.MoveTowards(happiness_coefficient, targetHappiness, GameConstants.HAPPINESS_CHANGE_SPEED * t * happinessDecreaseMultiplier);
            else happiness_coefficient = Mathf.MoveTowards(happiness_coefficient, targetHappiness, GameConstants.HAPPINESS_CHANGE_SPEED * t * happinessIncreaseMultiplier);
        }

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
        housingCountChanges = true;
        housingTimer = 0;
    }
    public void DeleteHousing(House h)
    {
        if (ignoreHousingRequest) return;
        if (houses.Contains(h))
        {
            houses.Remove(h);
            housingCountChanges = true;
            housingTimer = 0;
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

        //leveling
        if (citizenCount != 0)
        {
            int lspace = citizenCount;
            float fcount = citizenCount;
            if (housingVolumes[5] >= lspace)
            {
                housingLevel = 5;
            }
            else
            {
                lspace -= housingVolumes[5];
                if (housingVolumes[4] >= lspace)
                {
                    housingVolumes[4] = lspace;
                    housingVolumes[3] = 0;
                    housingVolumes[2] = 0;
                    housingVolumes[1] = 0;
                }
                else
                {
                    lspace -= housingVolumes[4];
                    if (housingVolumes[3] >= lspace)
                    {
                        housingVolumes[3] = lspace;
                        housingVolumes[2] = 0;
                        housingVolumes[1] = 0;
                    }
                    else
                    {
                        lspace -= housingVolumes[3];
                        if (housingVolumes[2] >= lspace)
                        {
                            housingVolumes[2] = lspace;
                            housingVolumes[1] = 0;
                        }
                        else
                        {
                            lspace -= housingVolumes[2];
                            if (housingVolumes[1] >= lspace)
                            {
                                housingVolumes[1] = lspace;
                            }
                        }
                    }
                }
                housingLevel = housingVolumes[5] / fcount * 5 + housingVolumes[4] / fcount * 4 + housingVolumes[3] / fcount * 3 + housingVolumes[2] / fcount * 2 + housingVolumes[1] / fcount;
            }
        }
        else housingLevel = 0;
        housingCountChanges = false;
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

    public void RenameColony(string nm)
    {
        cityName = nm;
    }

    public void SetHQ(HeadQuarters new_hq)
    {
        if (new_hq != null)
        {
            if (hq == null)
            {
                happiness_coefficient = GameConstants.START_HAPPINESS;
                birthrateCoefficient = GameConstants.START_BIRTHRATE_COEFFICIENT;
            }
            hq = new_hq;
        }
        QuestUI.current.CheckQuestsAccessibility();
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
    public void Save(System.IO.FileStream fs)
    {
        storage.Save(fs);

        fs.Write(System.BitConverter.GetBytes(gears_coefficient),0,4);
        fs.Write(System.BitConverter.GetBytes(labourEfficientcy_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(happiness_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(health_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(birthrateCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(energyStored), 0, 4);
        fs.Write(System.BitConverter.GetBytes(energyCrystalsCount), 0, 4); // 7 x 4

        Worksite.StaticSave(fs);
        fs.Write(System.BitConverter.GetBytes(freeWorkers), 0, 4);
        fs.Write(System.BitConverter.GetBytes(citizenCount), 0, 4);
        fs.Write(System.BitConverter.GetBytes(peopleSurplus), 0, 4);
        fs.Write(System.BitConverter.GetBytes(housingTimer), 0, 4);
        fs.Write(System.BitConverter.GetBytes(starvationTimer), 0, 4);
        fs.Write(System.BitConverter.GetBytes(realBirthrate), 0, 4);
        fs.Write(System.BitConverter.GetBytes(birthrateCoefficient), 0, 4); // 7 x 4

        var nameArray = System.Text.Encoding.Default.GetBytes(cityName);
        int count = nameArray.Length;
        fs.Write(System.BitConverter.GetBytes(count),0,4); // количество байтов, не длина строки
        if (count > 0) fs.Write(nameArray,0, nameArray.Length);
    }
    public void Load(System.IO.FileStream fs)
    {
        if (storage == null) storage = gameObject.AddComponent<Storage>();
        storage.Load(fs);

        var data = new byte[28];
        fs.Read(data, 0, 28);
        gears_coefficient = System.BitConverter.ToSingle(data,0);
        labourEfficientcy_coefficient = System.BitConverter.ToSingle(data, 4);
        happiness_coefficient = System.BitConverter.ToSingle(data, 8);
        health_coefficient = System.BitConverter.ToSingle(data, 12);
        birthrateCoefficient = System.BitConverter.ToSingle(data, 16);
        energyStored = System.BitConverter.ToSingle(data, 20);
        energyCrystalsCount = System.BitConverter.ToSingle(data, 24);

        Worksite.StaticLoad(fs);

        data = new byte[32]; // 28 + 4- name length
        fs.Read(data, 0, 32);
        freeWorkers = System.BitConverter.ToInt32(data, 0);
        citizenCount = System.BitConverter.ToInt32(data, 4);
        peopleSurplus = System.BitConverter.ToSingle(data, 8);
        housingTimer = System.BitConverter.ToSingle(data, 12);
        starvationTimer = System.BitConverter.ToSingle(data, 16);
        realBirthrate = System.BitConverter.ToSingle(data, 20);
        birthrateCoefficient = System.BitConverter.ToSingle(data, 24);
        RecalculatePowerGrid();
        RecalculateHousing();
        if (hospitals != null) RecalculateHospitals();
        if (powerGrid.Count > 0)
        {
            WorkBuilding wb = null;
            foreach (Building b in powerGrid)
            {
                wb = b as WorkBuilding;
                if (wb != null) wb.RecalculateWorkspeed();
            }
        }

        int bytesCount = System.BitConverter.ToInt32(data, 28); //выдаст количество байтов, не длину строки
        data = new byte[bytesCount];
        fs.Read(data, 0, bytesCount);
        if (bytesCount > 0)
        {
            System.Text.Decoder d = System.Text.Encoding.Default.GetDecoder();
            var chars = new char[d.GetCharCount(data, 0, bytesCount)];
            d.GetChars(data, 0, bytesCount, chars, 0, true);
            cityName = new string(chars);
        }
    }
    #endregion

}