﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class ColonyController : MonoBehaviour
{
    public Action<float> crystalsCountUpdateEvent, happinessUpdateEvent;
    public Action<int> populationUpdateEvent;

    public string cityName { get; private set; }
    public Storage storage { get; private set; }
    public HeadQuarters hq { get; private set; }
    public bool housingRecalculationNeeded = false, powerGridRecalculationNeeded = false; // hot
    public float gears_coefficient; // hot
    public float hospitals_coefficient { get; private set; }
    public float labourCoefficient
    {
        get
        {
            float f = gears_coefficient * 0.4f + health_coefficient * 0.3f + happiness_coefficient * 0.3f;
            if (f <= 0.01f) f = 0.01f;
            return f;
        }
    }
    public float happiness_coefficient { get; private set; }
    public float health_coefficient { get; private set; }
    public bool accumulateEnergy = true, buildingsWaitForReconnection = false;    

    public float energyStored { get; private set; }
    public float energySurplus { get; private set; }
    public float totalEnergyCapacity { get; private set; }
    public float energyCrystalsCount {
        get { return _energyCrystalCount; }
        private set
        {
            _energyCrystalCount = value;
            crystalsCountUpdateEvent?.Invoke(_energyCrystalCount);
        }
    }
    private float _energyCrystalCount;
    public List<Building> powerGrid { get; private set; }
    public List<Dock> docks { get; private set; }
    public List<House> houses { get; private set; }
    private Dictionary<Plane, Worksite> worksites;
    public byte docksLevel { get; private set; }
    public float housingLevel { get; private set; }
    public float foodMonthConsumption
    {
        get
        {
            return citizenCount * GameConstants.FOOD_CONSUMPTION * GameMaster.DAYS_IN_MONTH;
        }
    }

    public int freeWorkers { get; private set; }
    public int citizenCount {
        get { return _citizenCount; }
        private set {
            _citizenCount = value;
            populationUpdateEvent?.Invoke(_citizenCount);
        }
    }
    private int _citizenCount;
    public float realBirthrate { get; private set; }
    public int totalLivespace { get; private set; }

    private Dictionary<int, float> happinessModifiers; private int nextHModifierID;
    private List<(float volume, float timer)> happinessAffects;
    private List<Hospital> hospitals;
    private bool starvation = false;
    private sbyte recalculationTick = 0;
    private float birthrateCoefficient,peopleSurplus = 0f, 
        tickTimer,
        targetHappiness, targetHealth,
        happinessIncreaseMultiplier = 1f, happinessDecreaseMultiplier = 1f, showingHappiness;
    private bool thisIsFirstSet = true, ignoreHousingRequest = false;    

    public const byte MAX_HOUSING_LEVEL = 5;
    private const sbyte RECALCULATION_TICKS_COUNT = 5;
    private const float
        START_ENERGY_CRYSTALS_COUNT = 100,
        TICK_TIME = 1f,
        FOOD_SUPPLY_MIN_HAPPINESS = 0.2f,
        HOUSING_MIN_HAPPINESS = 0.15f,
        HEALTHCARE_MIN_HAPPINESS = 0.14f;


    void Awake()
    {
        if (thisIsFirstSet)
        {
            gears_coefficient = 2;
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
        happinessModifiers = null;
    }

    public void Prepare()
    { // call from game master
        if (storage == null) storage = gameObject.AddComponent<Storage>();
        var gm = GameMaster.realMaster;
        gm.SetColonyController(this);
        UIController.current.Prepare();
        gm.everydayUpdate += EverydayUpdate;
    }

    #region updating
    void Update()
    {
        if (showingHappiness != happiness_coefficient)
        {
            showingHappiness = Mathf.Lerp(showingHappiness, happiness_coefficient, Time.deltaTime);
            RenderSettings.skybox.SetFloat("_Saturation", showingHappiness * 0.25f + 0.75f);
        }
        if (GameMaster.gameSpeed == 0f | hq == null | GameMaster.loading) return;
        tickTimer -= Time.deltaTime * GameMaster.gameSpeed;
        if (tickTimer <= 0f)
        {
            tickTimer = TICK_TIME;
            happinessIncreaseMultiplier = 1f;
            happinessDecreaseMultiplier = 1f;
            var gm = GameMaster.realMaster;

            // gears:
            if (gears_coefficient > 1)
            {
                gears_coefficient -= gm.gearsDegradeSpeed * (2 - gm.environmentMaster.environmentalConditions) * TICK_TIME;
            }
            gears_coefficient = Mathf.Clamp(gears_coefficient, GameConstants.GEARS_LOWER_LIMIT, GameConstants.GEARS_UP_LIMIT);
            
            //energy:
            {
                if (energySurplus >= 0)
                {
                    if (accumulateEnergy)
                    {
                        energyStored += energySurplus * TICK_TIME;
                        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
                    }
                    if (buildingsWaitForReconnection) RecalculatePowerGrid();
                }
                else
                {
                    energyStored += energySurplus * TICK_TIME;
                    if (energyStored < 0)
                    { // отключение потребителей энергии до выравнивания
                        UIController.current.StartPowerFailureTimer();
                        energyStored = 0;
                        int i = powerGrid.Count - 1;
                        while (i >= 0 & energySurplus < 0)
                        {
                            Building b = powerGrid[i];
                            if (b == null)
                            {
                                powerGrid.RemoveAt(i);
                                powerGridRecalculationNeeded = true;
                            }
                            else
                            {
                                if (b.isActive & b.energySurplus < 0)
                                {
                                    energySurplus -= b.energySurplus;
                                    b.SetEnergySupply(false, false);
                                    powerGridRecalculationNeeded = true;
                                }
                            }
                            i--;
                        }
                    }
                }
                if (powerGridRecalculationNeeded) RecalculatePowerGrid();
            }

            //housing
            if (housingRecalculationNeeded) RecalculateHousing();

            //   STARVATION PROBLEM
            float foodSupplyHappiness = 0f;
            {
                if (starvation)
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
                        if (freeWorkers > 0f) { freeWorkers--; citizenCount--; }
                        else StartCoroutine(DeportateWorkingCitizen());
                        if (houses.Count > 0) PoolMaster.current.CitizenLeaveEffect(houses[UnityEngine.Random.Range(0, houses.Count )].transform.position);
                        else PoolMaster.current.CitizenLeaveEffect(hq.transform.position);
                    }
                    foodSupplyHappiness = 0f;
                    realBirthrate = 0f;
                    happinessDecreaseMultiplier++;
                }
                else {
                    float fmc = foodMonthConsumption;
                    foodSupplyHappiness = storage.standartResources[ResourceType.FOOD_ID] / fmc;
                    if (foodSupplyHappiness < FOOD_SUPPLY_MIN_HAPPINESS) foodSupplyHappiness = FOOD_SUPPLY_MIN_HAPPINESS;
                    if (fmc >= 1f) happinessIncreaseMultiplier++;
                }
            }

            //
            float x = GameConstants.HQ_MAX_LEVEL;
            float lvlCf = 1 - (hq.level - x / 2) / x;
            //
            //HEALTHCARE
            float healthcareHappiness = HEALTHCARE_MIN_HAPPINESS * lvlCf;
            targetHealth = gm.environmentMaster.environmentalConditions * 0.5f;
            if (hospitals_coefficient > 0)
            {
                if (hospitals_coefficient > targetHealth) targetHealth = hospitals_coefficient;
                healthcareHappiness += hospitals_coefficient * (1 - healthcareHappiness);                
                if (hospitals_coefficient > 1f)
                {
                    happinessIncreaseMultiplier++;
                    targetHealth = 1f;
                }
            }
            else
            {                
                if (lvlCf >= 1f) happinessDecreaseMultiplier++;
            }
            if (health_coefficient != targetHealth) health_coefficient = Mathf.MoveTowards(health_coefficient, targetHealth, GameConstants.HEALTH_CHANGE_SPEED);

            // HOUSING
            float housingHappiness = HOUSING_MIN_HAPPINESS * lvlCf;
            if (housingLevel < 1)
            {
                happinessDecreaseMultiplier++;
            }
            else
            {
                byte l = hq.level;
                if (l > MAX_HOUSING_LEVEL) l = MAX_HOUSING_LEVEL;
                float supply = housingLevel / l;
                housingHappiness += (1 - housingHappiness) * supply;
            }
            // HAPPINESS CALCULATION
            targetHappiness = 1;
            if (happinessModifiers != null)
            {
                foreach (var key in happinessModifiers)
                {
                    targetHappiness += key.Value;
                }
            }
            if (happinessAffects != null )
            {
                int i = happinessAffects.Count;
                while (i > 0)
                {
                    var ha = happinessAffects[i];
                    targetHappiness += ha.volume;
                    if (ha.timer > TICK_TIME)
                    {
                        happinessAffects[i] = (ha.volume, ha.timer - TICK_TIME);
                        i--;
                    }
                    else happinessAffects.RemoveAt(i);                    
                }
                if (happinessAffects.Count == 0) happinessAffects = null;
            }
            if (targetHappiness > foodSupplyHappiness) targetHappiness = foodSupplyHappiness;
            if (targetHappiness > healthcareHappiness) targetHappiness = healthcareHappiness;
            if (targetHappiness > housingHappiness) targetHappiness = housingHappiness;
            if (happiness_coefficient != targetHappiness)
            {
                if (happiness_coefficient > targetHappiness) happiness_coefficient = Mathf.MoveTowards(happiness_coefficient, targetHappiness, GameConstants.HAPPINESS_CHANGE_SPEED * TICK_TIME * happinessDecreaseMultiplier);
                else happiness_coefficient = Mathf.MoveTowards(happiness_coefficient, targetHappiness, GameConstants.HAPPINESS_CHANGE_SPEED * TICK_TIME * happinessIncreaseMultiplier);

                happinessUpdateEvent?.Invoke(happiness_coefficient);
            }            

            //  BIRTHRATE
            {
                if (birthrateCoefficient != 0 & !starvation)
                {
                    realBirthrate = birthrateCoefficient * Hospital.hospital_birthrate_coefficient * health_coefficient * happiness_coefficient * (1 + storage.standartResources[ResourceType.FOOD_ID] / 500f) * TICK_TIME;
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
                    if (!starvation)
                    {
                        starvation = true;
                        GameLogUI.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughFood), Color.red);
                    }
                }
                else starvation = false;
            }
            else starvation = false;
        }
    }
    #endregion

    public void AddCitizens(int x)
    {
        citizenCount += x;
        freeWorkers += x;
    }
    public void RemoveCitizens(int x)
    {
        if (citizenCount >= x)
        {
            citizenCount -= x;
        }
        else
        {
            citizenCount = 0;
        }
    }
    private IEnumerator DeportateWorkingCitizen() // чтобы не тормозить апдейт
    {
        bool found = false;
        var wbs = FindObjectsOfType<WorkBuilding>();
        if (wbs != null)
        {
            foreach (var wb in wbs)
            {
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
            if (worksites != null)
            {
                foreach (var pw in worksites)
                {
                    pw.Value.FreeWorkers();
                    if (freeWorkers > 0)
                    {
                        freeWorkers--;
                        citizenCount--;
                    }
                    break;
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

    #region ListsManagement

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
        housingRecalculationNeeded = true;
    }
    public void DeleteHousing(House h)
    {
        if (ignoreHousingRequest) return;
        if (houses.Contains(h))
        {
            houses.Remove(h);
            housingRecalculationNeeded = true;
        }
    }
    private void RecalculateHousing()
    {
        totalLivespace = 0;
        housingLevel = 0;
        if (hq == null) return;

        int[] housingVolumes = new int[MAX_HOUSING_LEVEL + 1];

        int i = 0, objectHousing = 0;
        byte objectLevel = 0;
        House h = null;
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
                if (h.isActive & h.isEnergySupplied)
                {
                    totalLivespace += objectHousing;
                    if (objectLevel < MAX_HOUSING_LEVEL) housingVolumes[objectLevel] += objectHousing;
                    else housingVolumes[MAX_HOUSING_LEVEL] += objectHousing;
                }
            }
        }
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
        housingRecalculationNeeded = false;
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
        powerGridRecalculationNeeded = true;
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
                powerGridRecalculationNeeded = true;
                return;
            }
            else i++;
        }
    }
    private void RecalculatePowerGrid()
    {        
        energySurplus = 0; totalEnergyCapacity = 0;
        if (powerGrid.Count == 0) return;
        int i = 0;
        List<int> checklist = new List<int>();
        buildingsWaitForReconnection = false;
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
                            checklist.Add(i);
                            buildingsWaitForReconnection = true;
                        }
                    }
                }
                i++;
            }
        }
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;

        if (buildingsWaitForReconnection & energySurplus > 0f)
        {
            i = 0;
            Building b;
            while (i < checklist.Count & energySurplus > 0f)
            {
                b = powerGrid[checklist[i]];
                if (b.energySurplus + energySurplus >= 0f)
                {
                    b.SetEnergySupply(true, false);
                    energySurplus += b.energySurplus;
                    checklist.RemoveAt(i);
                }
                else i++;
            }
            buildingsWaitForReconnection = checklist.Count != 0;
        }
        powerGridRecalculationNeeded = false;
    }
    public List<Building> GetPowerGrid() { return powerGrid; }
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

    public int AddHappinessModifier(float val)
    {
        if (happinessModifiers == null) happinessModifiers = new Dictionary<int, float>();
        int id = nextHModifierID++;
        happinessModifiers.Add(id, val);
        return id;
    }
    public void RemoveHappinessModifier(int id)
    {
        if (happinessModifiers != null) happinessModifiers.Remove(id);
    }

    public void AddHappinessAffect(float val, float time)
    {
        if (happinessAffects == null) happinessAffects = new List<(float volume, float timer)>() { (val, time) };
        else
        {
            if (happinessAffects.Count < 255)   happinessAffects.Add((val, time));
        }
    }

    public void AddWorksiteToList(Worksite w)
    {
        if (w == null || w.workplace == null) return;
        else
        {
            var p = w.workplace;
            if (worksites == null) worksites = new Dictionary<Plane, Worksite>();
            else
            {
                if (worksites.ContainsKey(p))
                {
                    if (worksites[p] != null) worksites[p].StopWork(false);
                    worksites[p] = w;
                    return;
                }
            }
            worksites.Add(p, w);
            p.SetWorksitePresence(true);
        }
    }
    public Worksite GetWorksite(Plane p)
    {
        if (worksites == null) return null;
        else
        {
            if (worksites.ContainsKey(p)) return worksites[p];
            else return null;
        }
    }
    public void ReplaceWorksiteFromList(Plane p, Worksite w, bool sendStopMessage)
    {
        if (worksites == null || !worksites.ContainsKey(p))
        {
            AddWorksiteToList(w);
            return;
        }
        else
        {
            if (worksites[p] != null && sendStopMessage) worksites[p].StopWork(false);
            worksites[p] = w;
        }
    }
    public void RemoveWorksite(Worksite w)
    {
        if (worksites != null && worksites.ContainsValue(w))
        {
            foreach (var pw in worksites)
            {
                if (pw.Value == w)
                {
                    var p = pw.Key;
                    worksites.Remove(p);
                    p.SetWorksitePresence(false);
                    if (worksites.Count == 0) worksites = null;
                    return;
                }
            }            
        }
    }
    public void RemoveWorksite(Plane p)
    {
        if (worksites != null && worksites.ContainsKey(p))
        {
            var w = worksites[p];
            if (w != null) w.StopWork(false);
            worksites.Remove(p);
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

    public Path DeterminePath()
    {
        return Path.LifePath;
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
        storage.Save(fs);
        
        fs.Write(System.BitConverter.GetBytes(gears_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(happiness_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(health_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(birthrateCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(energyStored), 0, 4);
        fs.Write(System.BitConverter.GetBytes(energyCrystalsCount), 0, 4); // 6 x 4
        //worksites saveing
        int count = 0;
        if (worksites != null)
        {            
            var keys = worksites.Keys;
            foreach (var k in keys)
            {
                if (k == null | worksites[k] == null) worksites.Remove(k);
            }
            count = worksites.Count;
            if (count > 0)
            {
                fs.WriteByte(1);
                fs.Write(System.BitConverter.GetBytes(count), 0, 4);
                keys = worksites.Keys;
                foreach (var k in keys) // т.к. могут сами удаляться при сохранении, если нет workplace
                { 
                    worksites[k].Save(fs);
                }
            }
            else fs.WriteByte(0);
        }
        else fs.WriteByte(0);
        // eo worksites saving
        fs.Write(System.BitConverter.GetBytes(freeWorkers), 0, 4);
        fs.Write(System.BitConverter.GetBytes(citizenCount), 0, 4);
        fs.Write(System.BitConverter.GetBytes(peopleSurplus), 0, 4);
        fs.Write(System.BitConverter.GetBytes(realBirthrate), 0, 4);
        fs.Write(System.BitConverter.GetBytes(birthrateCoefficient), 0, 4); // 5 x 4
        fs.WriteByte((byte)Hospital.birthrateMode); // + 1

        if (happinessAffects != null && happinessAffects.Count > 0) // + 1
        {
            fs.WriteByte(1);
            fs.WriteByte((byte)happinessAffects.Count);
            foreach (var ff in happinessAffects)
            {
                fs.Write(System.BitConverter.GetBytes(ff.volume), 0, 4);
                fs.Write(System.BitConverter.GetBytes(ff.timer), 0, 4);
            }
        }
        else fs.WriteByte(0);

        var nameArray = System.Text.Encoding.Default.GetBytes(cityName);
        count = nameArray.Length;
        fs.Write(System.BitConverter.GetBytes(count), 0, 4); // количество байтов, не длина строки
        if (count > 0) fs.Write(nameArray, 0, nameArray.Length);
    }
    public void Load(System.IO.FileStream fs)
    {
        if (storage == null) storage = gameObject.AddComponent<Storage>();
        storage.Load(fs);
       
        var data = new byte[25];        
        fs.Read(data, 0, 24);
        gears_coefficient = System.BitConverter.ToSingle(data, 0);
        happiness_coefficient = System.BitConverter.ToSingle(data, 4);
        health_coefficient = System.BitConverter.ToSingle(data, 8);
        birthrateCoefficient = System.BitConverter.ToSingle(data, 12);
        energyStored = System.BitConverter.ToSingle(data, 16);
        energyCrystalsCount = System.BitConverter.ToSingle(data, 20);
        //
        if (data[25] != 0)
        {
            data = new byte[4];
            fs.Read(data, 0, data.Length);
            int count = System.BitConverter.ToInt32(data, 0);
            Worksite.StaticLoad(fs, count);
        }
        //
        data = new byte[22]; // 20 + 1 + 1- name length
        fs.Read(data, 0, 25);
        freeWorkers = System.BitConverter.ToInt32(data, 0);
        citizenCount = System.BitConverter.ToInt32(data, 4);
        peopleSurplus = System.BitConverter.ToSingle(data, 8);
        realBirthrate = System.BitConverter.ToSingle(data, 12);
        birthrateCoefficient = System.BitConverter.ToSingle(data, 16);
        RecalculatePowerGrid();
        RecalculateHousing();
        if (hospitals != null) RecalculateHospitals();
        Hospital.SetBirthrateMode((BirthrateMode)data[20]);
        if (powerGrid.Count > 0)
        {
            WorkBuilding wb = null;
            foreach (Building b in powerGrid)
            {
                wb = b as WorkBuilding;
                if (wb != null) wb.RecalculateWorkspeed();
            }
        }

        if (data[21] == 1)
        {
            int count = fs.ReadByte();
            data = new byte[8];
            happinessAffects = new List<(float volume, float timer)>();
            for (int i = 0; i < count; i++)
            {
                fs.Read(data, 0, 8);
                happinessAffects.Add( (System.BitConverter.ToSingle(data,0), System.BitConverter.ToSingle(data,4)) );
            }
        }

        data = new byte[4];
        fs.Read(data, 0, 4);
        int bytesCount = System.BitConverter.ToInt32(data,0); //выдаст количество байтов, не длину строки
        if (bytesCount < 0 | bytesCount > 1000000)
        {
            Debug.Log("colony controller load error - name bytes count incorrect");
            GameMaster.LoadingFail();
            return;
        }        
        if (bytesCount > 0)
        {
            data = new byte[bytesCount];
            fs.Read(data, 0, bytesCount);
            System.Text.Decoder d = System.Text.Encoding.Default.GetDecoder();
            var chars = new char[d.GetCharCount(data, 0, bytesCount)];
            d.GetChars(data, 0, bytesCount, chars, 0, true);
            cityName = new string(chars);
        }
    }
    #endregion

}