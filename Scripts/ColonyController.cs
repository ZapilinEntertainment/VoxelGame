using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BirthrateMode : byte { Disabled, Normal, Improved, Lowered }
public sealed class ColonyController : MonoBehaviour
{
    public Action<float> crystalsCountUpdateEvent, happinessUpdateEvent;
    public Action<int> populationUpdateEvent;

    public string cityName { get; private set; }
    public Storage storage { get; private set; }
    public HeadQuarters hq { get; private set; }
    public MainCanvasController observer { get; private set; }
    public bool housingRecalculationNeeded = false, powerGridRecalculationNeeded = false, hospitalCoverageRecalculationNeeded; // hot
    public float gears_coefficient; // hot
    public float hospitals_coefficient { get; private set; }
    public float happinessCoefficient { get; private set; }
    public float workers_coefficient { get; private set; }
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
    public BirthrateMode birthrateMode { get; private set; }
    public List<Building> powerGrid { get; private set; }
    public List<Dock> docks { get; private set; }
    public List<House> houses { get; private set; }
    private Dictionary<(ChunkPos pos, byte face), Worksite> worksites; 
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
            hospitalCoverageRecalculationNeeded = true;
        }
    }
    private int _citizenCount, citizensToDisappear = 0;
    public float realBirthrate { get; private set; }
    public int totalLivespace { get; private set; }

    private Dictionary<int, float> happinessModifiers; private int nextHModifierID;
    private List<(float volume, float timer)> happinessAffects;
    private List<Hospital> hospitals;    
    private bool starvation = false;
    private sbyte recalculationTick = 0;
    private float birthSpeed,peopleSurplus = 0f, 
        tickTimer, birthrateCoefficient = 0f,
        targetHappiness,
        happinessIncreaseMultiplier = 1f, happinessDecreaseMultiplier = 1f, showingHappiness;
    private bool thisIsFirstSet = true, ignoreHousingRecalculationRequest = false;
    private Func<float> getNatureCf;

    public const byte MAX_HOUSING_LEVEL = 5;
    public const float LOWERED_BIRTHRATE_COEFFICIENT = 0.5f, IMPROVED_BIRTHRATE_COEFFICIENT = 1.5f;
    private const sbyte RECALCULATION_TICKS_COUNT = 5;
    private const float
        START_ENERGY_CRYSTALS_COUNT = 100,
        TICK_TIME = 1f,
        MIN_HAPPINESS = 0.3f,
        FOOD_SUPPLY_MIN_HAPPINESS = 0.2f,
        HOUSING_MIN_HAPPINESS = 0.25f,
        HEALTHCARE_MIN_HAPPINESS = 0.25f;

    void Awake()
    {
        if (thisIsFirstSet)
        {
            gears_coefficient = 2f;
            hospitals_coefficient = 0f;
            birthSpeed = GameConstants.START_BIRTHRATE_COEFFICIENT;
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
        worksites = null;
        happinessModifiers = null;
        happinessAffects = null;
    }

    public void Prepare()
    { // call from game master
        if (storage == null) storage = gameObject.AddComponent<Storage>();
        GameMaster.realMaster.everydayUpdate += EverydayUpdate;
    }
    public void LinkObserver(MainCanvasController mcc) { observer = mcc; }

    #region updating
    void Update()
    {
        if (freeWorkers > citizenCount) freeWorkers = citizenCount;
        if (showingHappiness != happinessCoefficient)
        {
            showingHappiness = Mathf.Lerp(showingHappiness, happinessCoefficient, Time.deltaTime);
            RenderSettings.skybox.SetFloat("_Saturation", showingHappiness * 0.25f + 0.75f);
        }        
        if (GameMaster.gameSpeed == 0f | hq == null | GameMaster.loading) return;
        tickTimer -= Time.deltaTime * GameMaster.gameSpeed;
        if (tickTimer <= 0f )
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
            gears_coefficient = Mathf.Clamp(gears_coefficient, GameConstants.GEARS_LOWER_LIMIT, GameConstants.GetMaxGearsCf(this));
            
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
                        observer?.StartPowerFailureTimer();
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
                                if (b.CanBePoweredOffBySystem() && (b.isActive && b.energySurplus < 0))
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
                        if (freeWorkers > 0f)
                        {
                            freeWorkers--;
                            citizenCount--;
                            if (houses.Count > 0) PoolMaster.current.CitizenLeaveEffect(houses[UnityEngine.Random.Range(0, houses.Count)].transform.position);
                            else PoolMaster.current.CitizenLeaveEffect(hq.transform.position);
                        }
                        else citizensToDisappear++;                      
                    }
                    foodSupplyHappiness = 0f;
                    realBirthrate = 0f;
                    happinessDecreaseMultiplier++;
                }
                else {
                    float fmc = foodMonthConsumption;
                    foodSupplyHappiness = 1f;
                    if (fmc >= 1f) happinessIncreaseMultiplier++;
                }
            }

            //
            float lvlCf =GetLevelSoftingCf();
            //
            //HEALTHCARE
            if (hospitalCoverageRecalculationNeeded) RecalculateHospitals();
            float healthcareHappiness = HEALTHCARE_MIN_HAPPINESS * lvlCf;
            if (hospitals_coefficient > 0)
            {
                healthcareHappiness += hospitals_coefficient * (1 - healthcareHappiness);                
                if (hospitals_coefficient > 1f)
                {
                    happinessIncreaseMultiplier++;
                }
            }
            else
            {                
                if (lvlCf >= 1f) happinessDecreaseMultiplier++;
            }

            // HOUSING
            byte level = hq.level;
            float housingHappiness = (float)totalLivespace / (float)citizenCount ;
            if (housingHappiness < HOUSING_MIN_HAPPINESS) housingHappiness = HOUSING_MIN_HAPPINESS;
            else
            {
                if (housingHappiness > 1f) housingHappiness = 1f;
            }
            // HAPPINESS CALCULATION
            targetHappiness = 1f;
            if (happinessModifiers != null)
            {
                foreach (var key in happinessModifiers)
                {
                    targetHappiness += key.Value;
                }
            }
            if (happinessAffects != null )
            {
                int i = happinessAffects.Count - 1;
                while (i >= 0)
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
            switch (level)
            {
                case 0:
                case 1:
                    if (starvation) targetHappiness *= MIN_HAPPINESS;
                    else targetHappiness *= (MIN_HAPPINESS + (1f - MIN_HAPPINESS) * housingHappiness);
                    break;
                default:
                    if (targetHappiness > foodSupplyHappiness) targetHappiness = foodSupplyHappiness;
                    if (targetHappiness > healthcareHappiness) targetHappiness = healthcareHappiness;
                    if (targetHappiness > housingHappiness) targetHappiness = housingHappiness;
                    break;
            }            
            if (happinessCoefficient != targetHappiness)
            {
                if (happinessCoefficient > targetHappiness) happinessCoefficient = Mathf.MoveTowards(happinessCoefficient, targetHappiness, GameConstants.HAPPINESS_CHANGE_SPEED * TICK_TIME * happinessDecreaseMultiplier);
                else happinessCoefficient = Mathf.MoveTowards(happinessCoefficient, targetHappiness, GameConstants.HAPPINESS_CHANGE_SPEED * TICK_TIME * happinessIncreaseMultiplier);

                happinessUpdateEvent?.Invoke(happinessCoefficient);
            }            

            //  BIRTHRATE
            {
                if (birthSpeed != 0 & !starvation)
                {
                    realBirthrate = birthSpeed *  birthrateCoefficient * happinessCoefficient * (1 + storage.GetResourceCount(ResourceType.Food) / 500f) * TICK_TIME;
                    if (peopleSurplus > 1)
                    {
                        int newborns = (int)peopleSurplus;
                        AddCitizens(newborns, true);
                        peopleSurplus -= newborns;
                    }
                }
                peopleSurplus += realBirthrate;
            }

            // WORKSPEED
            workers_coefficient = starvation ? 0.3f : (0.5f + happinessCoefficient * 0.7f) * gears_coefficient;
            //deleting citizens
            if (citizensToDisappear > 0)
            {
                StartCoroutine(DeportateWorkingCitizen(citizensToDisappear));
                citizensToDisappear = 0;
            }
        }        
    }
    public void EverydayUpdate()
    {
        var rm = GameMaster.realMaster;
        if (!rm.weNeedNoResources)
        {
            var f = rm.gameRules.foodSpendRate;
            if (f != 0f)
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
                            AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughFood), Color.red);
                        }
                    }
                    else starvation = false;
                }
                else starvation = false;
            }
            else
            {
                if (starvation) starvation = false;
            }
        }
        RecalculateHospitals();
    }
    #endregion

    public void AddCitizens(int x, bool workable)
    {
        citizenCount += x;
        if (workable) freeWorkers += x;
    }
    public void RemoveCitizens(int x, bool working)
    {
        if (working)
        {
            if (freeWorkers > 0)
            {
                if (freeWorkers >= x)
                {
                    freeWorkers -= x;
                    citizenCount -= x;
                    return;
                }
                else
                {
                    x -= freeWorkers;
                    citizenCount -= freeWorkers;
                    freeWorkers = 0;
                }
            }
            citizensToDisappear += x;
        }
        else citizenCount -= x;
    }
    private IEnumerator DeportateWorkingCitizen(int x) // чтобы не тормозить апдейт
    {
        var wbs = FindObjectsOfType<WorkBuilding>();
        int n = 0;
        if (wbs != null)
        {
            foreach (var wb in wbs)
            {
                if (wb == null || wb.workersCount == 0) continue;
                n = wb.FreeWorkers(x);// knock-knock, whos there? ME ME ME ME ME HAHAHA
                freeWorkers -= n;
                citizenCount -= n;
                x -= n;
                if (x == 0) goto END;
            }
        }
        if (x != 0 && worksites != null)
        {
            foreach (var w in worksites)
            {
                n = w.Value?.FreeWorkers(x) ?? 0;
                if (n != 0)
                {
                    freeWorkers -= n;
                    citizenCount -= n;
                    x -= n;
                    if (x == 0) break;
                }
            }
        }
        END:
        if (x != 0) citizensToDisappear += x;
        yield return null;
    }

    public void AddWorkers(int x)
    {
        freeWorkers += x;
    }
    public void SendWorkers(int x, ILabourable w)
    {
        if (freeWorkers == 0 | w == null) return;
        if (x > freeWorkers) x = freeWorkers;
        freeWorkers = freeWorkers - x + w.AddWorkers(x);
    }
    public void ConsumeWorkers(int x)
    {
        if (freeWorkers > x) {
            freeWorkers -= x;
            citizenCount -= x;
        }
        else
        {
            citizenCount -= freeWorkers;
            freeWorkers = 0;
        }
    }

    #region ListsManagement

    public void AddHousing(House h)
    {
        if (ignoreHousingRecalculationRequest) return;
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
        if (ignoreHousingRecalculationRequest) return;
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
                if (fcount != 0f) housingLevel = housingVolumes[5] / fcount * 5 + housingVolumes[4] / fcount * 4 + housingVolumes[3] / fcount * 3 + housingVolumes[2] / fcount * 2 + housingVolumes[1] / fcount;
                else housingLevel = 0;
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
    private void RecalculateHospitals()
    {
        hospitalCoverageRecalculationNeeded = false;
        hospitals_coefficient = 0f;
        bool noHospitals = hospitals == null;
        if (noHospitals || hospitals.Count == 0) {
            if (birthrateMode != BirthrateMode.Disabled) SetBirthrateMode(BirthrateMode.Disabled);
            return;
        }
        else
        {
            if (birthrateMode == BirthrateMode.Disabled) SetBirthrateMode(BirthrateMode.Normal);
        }
        if (noHospitals) return;
        int i = 0;
        float hospitalsCoverage = 0f;
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
    public void FORCED_PowerGridRecalculation()
    {
        RecalculatePowerGrid();
    }
    public List<Building> GetPowerGrid() { return powerGrid; }
    public void AddEnergy(float f)
    {
        energyStored += f;
        if (energyStored > totalEnergyCapacity) energyStored = totalEnergyCapacity;
    }

    public void SYSTEM_DocksRecalculation()
    {
        if (powerGrid != null && powerGrid.Count > 0)
        {
            foreach (var p in powerGrid)
            {
                if (p is Dock) (p as Dock).CheckPositionCorrectness();
            }
        }
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
            var pos = (p.pos, p.faceIndex);
            if (worksites == null) worksites = new Dictionary<(ChunkPos, byte), Worksite>();
            else
            {
                if (worksites.ContainsKey(pos))
                {
                    if (worksites[pos] != null) worksites[pos].StopWork(false);
                    worksites[pos] = w;
                    return;
                }
            }
            worksites.Add(pos, w);
            p.SetWorksitePresence(true);
        }
    }
    public Worksite GetWorksite(Plane p)
    {
        if (worksites == null) return null;
        else
        {
            var key = (p.pos, p.faceIndex);
            if (worksites.ContainsKey(key)) return worksites[key];
            else return null;
        }
    }
    public void RemoveWorksite(Worksite w)
    {
        if (worksites != null)
        {
            foreach (var pw in worksites)
            {
                if (pw.Value == w)
                {
                    var p = pw.Key;
                    worksites.Remove(p);
                    w.workplace?.SetWorksitePresence(false);
                    if (worksites.Count == 0) worksites = null;
                    return;
                }
            }            
        }
    }
    public void RemoveWorksite(Plane p)
    {
        var key = (p.pos, p.faceIndex);
        if (worksites != null && worksites.ContainsKey(key))
        {
            var w = worksites[key];
            if (w != null) w.StopWork(false);
            worksites.Remove(key);
            if (worksites.Count == 0) worksites = null;
        }
    }    

    public bool HaveBuilding(int sid)
    {
        if (powerGrid != null && powerGrid.Count > 0)
        {
            foreach (var p in powerGrid)
            {
                if (p == null) { powerGridRecalculationNeeded = true; continue; }
                else
                {
                    if (p.ID == sid) return true;
                }
            }
            return false;
        }
        else return false;
    }
    public Building GetBuilding(int sid)
    {
        if (powerGrid != null && powerGrid.Count > 0)
        {
            foreach (var p in powerGrid)
            {
                if (p == null) { powerGridRecalculationNeeded = true; continue; }
                else
                {
                    if (p.ID == sid) return p;
                }
            }
        }
        return null;
    }
    public int GetBuildingsCount(int sid)
    {
        if (powerGrid == null || powerGrid.Count == 0) return 0;
        else
        {
            int count = 0;
            foreach (var p in powerGrid)
            {
                if (p == null) { powerGridRecalculationNeeded = true; continue; }
                else
                {
                    if (p.ID == sid) count++;
                }
            }
            return count;
        }
    }
    public int GetBuildingsCount<T>()
    {
        if (powerGrid == null || powerGrid.Count == 0) return 0;
        else
        {
            int count = 0;
            foreach (var p in powerGrid)
            {
                if (p == null) { powerGridRecalculationNeeded = true; continue; }
                else
                {
                    if (p is T) count++;
                }
            }
            return count;
        }
    }
    public List<T> GetBuildings<T> () where T : class
    {
        if (powerGrid == null || powerGrid.Count == 0) return null;
        else
        {
            var list = new List<T>();
            foreach (var p in powerGrid)
            {
                if (p == null) { powerGridRecalculationNeeded = true; continue; }
                else
                {
                    if (p is T) list.Add(p as T);
                }
            }
            if (list.Count == 0) return null; else return list;
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
                happinessCoefficient = GameConstants.START_HAPPINESS;
                birthSpeed = GameConstants.START_BIRTHRATE_COEFFICIENT;
            }
            hq = new_hq;
        }
        GameMaster.realMaster.globalMap.LinkColonyController(this);

        QuestUI.current.CheckQuestsAccessibility(!GameMaster.loading);
        TrySetNatureCfLink();
    }
    private void TrySetNatureCfLink()
    {
        if (hq != null)
        {
            var n = hq.basement?.myChunk.GetNature();
            if (n != null) getNatureCf = n.GetNatureCf;
        }
    }

    public void AddEnergyCrystals(float v)
    {
        if (v <= 0) return;
        energyCrystalsCount += v;
        if (v > 1 && observer != null) observer.MoneyChanging(v);
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
        if (v >= 1 && observer != null) observer.MoneyChanging(-v);
        return v;
    }    
    public float FORCED_GetEnergyCrystals(float v)
    {
        energyCrystalsCount -= v;
        if (v >= 1 && observer != null) observer.MoneyChanging(-v);
        return v;
    }

    public bool TryGetEnergy(float volume)
    {
        if (energyStored >= volume)
        {
            energyStored -= volume;
            return true;
        }
        else return false;
    }

    public void SetBirthrateMode(BirthrateMode bm)
    {
        birthrateMode = bm;
        switch (bm)
        {
            case BirthrateMode.Improved: birthrateCoefficient = IMPROVED_BIRTHRATE_COEFFICIENT; break;
            case BirthrateMode.Lowered: birthrateCoefficient = LOWERED_BIRTHRATE_COEFFICIENT; break;
            case BirthrateMode.Normal: birthrateCoefficient = 1f; break;
            default: birthrateCoefficient = 0f; break;
        }
    }

    #region parameters
    public float GetLevelSoftingCf()
    {
        return 1 - (hq.level - GameConstants.HQ_MAX_LEVEL / 2) / GameConstants.HQ_MAX_LEVEL;
        // 1 - 1.33
        // 2 - 1.17
        // 3 - 1
        // 4 - 0.83
        // 5 - 0.66
        // 6 - 0.5
    }
    public Path DeterminePath()
    {
        return Path.LifePath;
    }
    public float GetPeopleCf()
    {
        if (starvation) return 0f;
        else { return citizenCount > 10000 ? 1f : (float)citizenCount / 10000f; }
    }
    public float GetTechCf()
    {
        float x = 0f;
        x += 0.25f * ((float)hq.level / (float)GameConstants.HQ_MAX_LEVEL);
        //
        const float ENERGY_MAX = 50000f;
        if (energyStored > ENERGY_MAX) x += 0.25f; else x += 0.25f * (energyStored / ENERGY_MAX);
        //
        x += 0.25f * ((float)docksLevel / 3f);
        //
        //have engine contorl tower?
        //
        return x;
    }
    public float GetCrystalCf()
    {
        const float CRYSTALS_MAX = 10000;
        if (energyCrystalsCount > CRYSTALS_MAX) return 1f;
        else
        {
            if (energyCrystalsCount < 0f) return 0f;
            else return energyCrystalsCount / CRYSTALS_MAX;
        }
    }
    public float GetNatureCf()
    {
        if (getNatureCf != null) return getNatureCf();
        else
        {
            TrySetNatureCfLink();
            if (getNatureCf != null) return getNatureCf();
            else return 0f;
        }
    }
    #endregion


    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
        storage.Save(fs);
        
        fs.Write(System.BitConverter.GetBytes(gears_coefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(happinessCoefficient), 0, 4);
        fs.Write(System.BitConverter.GetBytes(birthSpeed), 0, 4);
        fs.Write(System.BitConverter.GetBytes(energyStored), 0, 4);
        fs.Write(System.BitConverter.GetBytes(energyCrystalsCount), 0, 4); // 5 x 4, 0 - 19
        //worksites saving
        int count = 0;
        if (worksites != null)
        {
            var keys = worksites.Keys;
            Worksite w = null;
            Plane p = null;
            Block b;
            var chunk = GameMaster.realMaster.mainChunk;
            bool correctData = false;
            foreach (var fp in keys) // потому что через foreach нельзя менять список
            {
                correctData = false;
                b = chunk.GetBlock(fp.pos);
                if (b != null && b.TryGetPlane(fp.face, out p)) {
                    if (!p.destroyed)
                    {
                        w = worksites[fp];
                        if (w == null || w.destroyed)
                        {
                            worksites.Remove(fp);
                        }
                        correctData = true;
                    }
                }
                if (!correctData) worksites.Remove(fp);
            }

            count = worksites.Count;
            fs.Write(System.BitConverter.GetBytes(count), 0, 4);
            if (count > 0)
            {
                foreach (var fw in worksites)
                { 
                    fw.Value.Save(fs);
                }
            }            
        }
        else fs.Write(System.BitConverter.GetBytes(count),0,4); // 20 - 23
        // eo worksites saving
        fs.Write(System.BitConverter.GetBytes(freeWorkers), 0, 4);
        fs.Write(System.BitConverter.GetBytes(citizenCount), 0, 4);
        fs.Write(System.BitConverter.GetBytes(peopleSurplus), 0, 4);
        fs.Write(System.BitConverter.GetBytes(realBirthrate), 0, 4);
        fs.Write(System.BitConverter.GetBytes(birthSpeed), 0, 4); // 5 x 4
        fs.WriteByte((byte)birthrateMode); // 21

        if (happinessAffects != null && happinessAffects.Count > 0) // 22
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
       
        var data = new byte[24];        
        fs.Read(data, 0, data.Length);
        gears_coefficient = System.BitConverter.ToSingle(data, 0);
        happinessCoefficient = System.BitConverter.ToSingle(data, 4);
        showingHappiness = happinessCoefficient;
        RenderSettings.skybox.SetFloat("_Saturation", showingHappiness * 0.25f + 0.75f);
        birthSpeed = System.BitConverter.ToSingle(data, 8);
        energyStored = System.BitConverter.ToSingle(data, 12);
        energyCrystalsCount = System.BitConverter.ToSingle(data, 16);
        //
        int count = System.BitConverter.ToInt32(data, 20);
        if (count != 0)
        {
            Worksite.StaticLoad(fs, count);
        }
        //
        data = new byte[22]; // 24+ 1 + 1- name length
        fs.Read(data, 0, data.Length);
        freeWorkers = System.BitConverter.ToInt32(data, 0);
        citizenCount = System.BitConverter.ToInt32(data, 4);
        peopleSurplus = System.BitConverter.ToSingle(data, 8);
        realBirthrate = System.BitConverter.ToSingle(data, 12);
        birthSpeed = System.BitConverter.ToSingle(data, 16);
        SetBirthrateMode((BirthrateMode)data[20]);        

        if (data[21] == 1)
        {
            count = fs.ReadByte();
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
            d = null;
            chars = null;
        }

        if (docks.Count > 0)
        {
            foreach (var d in docks) d.CheckPositionCorrectness();
        }
    }
    #endregion

}