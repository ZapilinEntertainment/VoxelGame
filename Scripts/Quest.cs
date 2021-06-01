using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType : byte
{
    System, Progress, Foundation, CloudWhale, Engine, Pipes, Crystal, Monument, Blossom, Pollen, Endgame, Scenario, Condition, Total
}
// ограничения на кол-во - до 32х, иначе не влезет в questCompleteMask
public enum ProgressQuestID : byte
{
    Progress_HousesToMax, Progress_2Docks, Progress_2Storages, Progress_Tier2, Progress_300Population, Progress_OreRefiner, Progress_HospitalCoverage, Progress_Tier3,
    Progress_4MiniReactors, Progress_100Fuel, Progress_XStation, Progress_Tier4, Progress_CoveredFarm, Progress_CoveredLumbermill, Progress_Reactor, Progress_FirstExpedition,
    Progress_Tier5, Progress_FactoryComplex, Progress_SecondFloor, Progress_FoodStocks, LASTONE
}

public class Quest : MyObject
{
    public string name { get; protected set; }
    public string description { get; protected set; }
    public float reward { get; protected set; }
    public bool needToCheckConditions { get; protected set; }
    public bool completed { get; protected set; }

    public string[] steps { get; protected set; }
    public string[] stepsAddInfo { get; protected set; }
    public bool[] stepsFinished { get; protected set; }
    public readonly QuestType type;
    public readonly byte subIndex;

    public static uint[] questsCompletenessMask { get; private set; } // до 32-х квестов на ветку
    public static readonly Quest NoQuest, AwaitingQuest;

    protected bool subscribedToStructuresCheck = false;
    private const byte NO_QUEST_SUBINDEX = 0, AWAITING_QUEST_SUBINDEX = 1;

    //при добавлении квеста дополнить:
    // Localization -> Fill quest data
    // QuestUI -> coroutine SetNewQuest

    static Quest()
    {
        questsCompletenessMask = new uint[(int)QuestType.Total];
        NoQuest = new Quest(QuestType.System, NO_QUEST_SUBINDEX);
        AwaitingQuest = new Quest(QuestType.System, AWAITING_QUEST_SUBINDEX);
    }
    public override int GetHashCode()
    {
        var hashCode = 67631244;
        hashCode = hashCode * -1521134295 + type.GetHashCode();
        hashCode = hashCode * -1521134295 + subIndex.GetHashCode();
        return hashCode;
    }
    protected override bool IsEqualNoCheck(object obj)
    {
        var q = (Quest)obj;
        return type == q.type && subIndex == q.subIndex && completed == q.completed && stepsFinished == q.stepsFinished;
    }

    public static void SetCompletenessMask(uint[] m)
    {
        questsCompletenessMask = m;
    }

    protected Quest() { }
    public Quest(QuestType i_type, byte subID)
    {
        type = i_type;
        subIndex = subID;
        needToCheckConditions = true;
        reward = 0f;
        completed = false;
        bool standartQuest = (type == QuestType.Scenario) || (type == QuestType.Condition);
        if (!standartQuest)
        {
            byte stepsCount = 1;
            switch (i_type)
            {
                case QuestType.Progress:
                    {
                        bool defaultSettings = true;
                        switch ((ProgressQuestID)subIndex)
                        {
                            case ProgressQuestID.Progress_HousesToMax:
                                reward = 250;
                                break;
                            case ProgressQuestID.Progress_2Docks: reward = 500; break;
                            case ProgressQuestID.Progress_2Storages: reward = 200; break;
                            case ProgressQuestID.Progress_Tier2: reward = 120; break;
                            case ProgressQuestID.Progress_300Population: reward = 200; break;
                            case ProgressQuestID.Progress_OreRefiner: reward = 200; break;
                            case ProgressQuestID.Progress_HospitalCoverage: reward = 240; break;
                            case ProgressQuestID.Progress_Tier3: reward = 240; break;
                            case ProgressQuestID.Progress_4MiniReactors: reward = 800; break;
                            case ProgressQuestID.Progress_100Fuel: reward = 200; break;
                            case ProgressQuestID.Progress_XStation: reward = 120; break;
                            case ProgressQuestID.Progress_Tier4: reward = 480; break;
                            case ProgressQuestID.Progress_CoveredFarm: reward = 200; break;
                            case ProgressQuestID.Progress_CoveredLumbermill: reward = 200; break;
                            case ProgressQuestID.Progress_Reactor: reward = 220; break;
                            case ProgressQuestID.Progress_FoodStocks: reward = 120; break;
                            case ProgressQuestID.Progress_FirstExpedition:
                                defaultSettings = false;
                                stepsCount = 6;
                                reward = 4000;
                                break;
                            case ProgressQuestID.Progress_Tier5: reward = 960; break;
                            case ProgressQuestID.Progress_FactoryComplex:
                                defaultSettings = false;
                                stepsCount = 2;
                                reward = 960;
                                break;
                            case ProgressQuestID.Progress_SecondFloor:
                                defaultSettings = false;
                                stepsCount = 2;
                                reward = 420;
                                break;
                        }

                        if (defaultSettings)
                        {
                            stepsCount = 1;
                        }
                        break;
                    }
                case QuestType.Endgame:
                    //switch ((Knowledge.ResearchRoute)subID) { }
                    reward = 10000;
                    break;
                case QuestType.System:
                    if (subID == NO_QUEST_SUBINDEX) name = "no quest";
                    else name = "awaiting quest";
                    break;
                case QuestType.Foundation:
                    break;
                case QuestType.CloudWhale:
                    if (subIndex == (byte)Knowledge.CloudWhaleRouteBoosters.PointBoost) GameMaster.realMaster.globalMap.pointsExploringEvent += PointEventCheck;
                    break;
                case QuestType.Engine:
                    break;
                case QuestType.Pipes:
                    switch ((Knowledge.PipesRouteBoosters)subID)
                    {
                        case Knowledge.PipesRouteBoosters.FarmsBoost: stepsCount = 2; break;
                        case Knowledge.PipesRouteBoosters.BiomesBoost: stepsCount = 4; break;
                        case Knowledge.PipesRouteBoosters.SizeBoost: stepsCount = 3; break;
                    }
                    break;
                case QuestType.Crystal:
                    break;
                case QuestType.Monument:
                    switch ((Knowledge.MonumentRouteBoosters)subID)
                    {
                        case Knowledge.MonumentRouteBoosters.MonumentAffectionBoost: stepsCount = 2; break;
                    }
                    break;
                case QuestType.Blossom:
                    break;
                case QuestType.Pollen:
                    break;
            }
            INLINE_PrepareSteps(stepsCount);
            Localization.FillQuestData(this);
        }        
    }
    protected void INLINE_PrepareSteps(byte stepsCount)
    {
        steps = new string[stepsCount];
        stepsAddInfo = new string[stepsCount];
        stepsFinished = new bool[stepsCount];
    }
    public Quest(Knowledge.ResearchRoute rr, byte subID) : this(RouteToQuestType(rr), subID) { }
    private static QuestType RouteToQuestType(Knowledge.ResearchRoute rr)
    {
        switch (rr)
        {
            case Knowledge.ResearchRoute.Foundation: return QuestType.Foundation;
            case Knowledge.ResearchRoute.CloudWhale: return QuestType.CloudWhale;
            case Knowledge.ResearchRoute.Engine: return QuestType.Engine;
                case Knowledge.ResearchRoute.Pipes: return QuestType.Pipes;
            case Knowledge.ResearchRoute.Crystal: return QuestType.Crystal;
            case Knowledge.ResearchRoute.Monument: return QuestType.Monument;
            case Knowledge.ResearchRoute.Blossom: return QuestType.Blossom;
            case Knowledge.ResearchRoute.Pollen: return QuestType.Pollen;
            default: return QuestType.System;
        }
    }

    private void PointEventCheck(MapPoint mp)
    {
        if (!completed)
        {
            if (mp is PointOfInterest)
            {
                switch (Knowledge.GetBoostedRoute(mp as PointOfInterest))
                {
                    case Knowledge.ResearchRoute.Foundation:
                        if (type == QuestType.Foundation && subIndex == (byte)Knowledge.FoundationRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.CloudWhale:
                        if (type == QuestType.CloudWhale && subIndex == (byte)Knowledge.CloudWhaleRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.Engine:
                        if (type == QuestType.Engine && subIndex == (byte)Knowledge.EngineRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.Pipes:
                        if (type == QuestType.Pipes && subIndex == (byte)Knowledge.PipesRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.Crystal:
                        if (type == QuestType.Crystal && subIndex == (byte)Knowledge.CrystalRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.Monument:
                        if (type == QuestType.Monument && subIndex == (byte)Knowledge.MonumentRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.Blossom:
                        if (type == QuestType.Blossom && subIndex == (byte)Knowledge.BlossomRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                    case Knowledge.ResearchRoute.Pollen:
                        if (type == QuestType.Pollen && subIndex == (byte)Knowledge.PollenRouteBoosters.PointBoost) MakeQuestCompleted();
                        break;
                }
            }
        }
        GameMaster.realMaster.globalMap.pointsExploringEvent -= this.PointEventCheck;
    }

    public void SetStepCompleteness(int index, bool x)
    {
        if (index < stepsFinished.Length) stepsFinished[index] = x;
    }
    public void ChangeConditionsCheckStatus(bool x)
    {
        needToCheckConditions = x;
    }
    virtual public void CheckQuestConditions()
    {        
        ColonyController colony = GameMaster.realMaster.colonyController;
        switch (type)
        {
            case QuestType.Progress:
                switch ((ProgressQuestID)subIndex)
                {
                    case ProgressQuestID.Progress_HousesToMax:
                        {
                            float hl = colony.housingLevel;
                            byte hql = colony.hq.level;
                            if (hql > ColonyController.MAX_HOUSING_LEVEL) hql = ColonyController.MAX_HOUSING_LEVEL;
                            stepsAddInfo[0] = string.Format("{0:0.##}", hl) + '/' + hql.ToString();
                            if (hl >= hql) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_2Docks:
                        stepsAddInfo[0] = colony.docks.Count.ToString() + "/2";
                        if (colony.docks.Count >= 2) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_2Storages:
                        stepsAddInfo[0] = (colony.storage.warehouses.Count - 1).ToString() + "/2";
                        if (colony.storage.warehouses.Count >= 3) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_Tier2:
                        if (colony.hq.level >= 2) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_300Population:
                        {
                            var ic = DockSystem.GetImmigrantsTotalCount();
                            stepsAddInfo[0] = ic.ToString() + "/300";
                            if (ic >= 300) MakeQuestCompleted();
                            break;
                        }
                    case ProgressQuestID.Progress_OreRefiner:
                        {
                            if (colony.HaveBuilding(Structure.ORE_ENRICHER_2_ID)) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_HospitalCoverage:
                        stepsAddInfo[0] = string.Format("{0:0.###}", colony.hospitals_coefficient) + " / 1";
                        if (colony.hospitals_coefficient >= 1) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_Tier3:
                        if (colony.hq.level >= 3) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_4MiniReactors:
                        {
                            int mrc = colony.GetBuildingsCount(Structure.MINI_GRPH_REACTOR_3_ID);
                            stepsAddInfo[0] = mrc.ToString() + "/4";
                            if (mrc >= 4) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_100Fuel:
                        {
                            int f = (int)colony.storage.GetResourceCount(ResourceType.Fuel);
                            stepsAddInfo[0] = f.ToString() + "/100";
                            if (f >= 100) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_XStation:
                        if (XStation.current != null) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_Tier4:
                        if (colony.hq.level >= 4) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_CoveredFarm:
                        {
                            List<Building> powerGrid = colony.powerGrid;
                            foreach (Building b in powerGrid)
                            {
                                if (b == null) continue;
                                else
                                {
                                    if (b.ID == Structure.COVERED_FARM | b.ID == Structure.FARM_BLOCK_ID)
                                    {
                                        MakeQuestCompleted();
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case ProgressQuestID.Progress_CoveredLumbermill:
                        {
                            List<Building> powerGrid = colony.powerGrid;
                            foreach (Building b in powerGrid)
                            {
                                if (b == null) continue;
                                else
                                {
                                    if (b.ID == Structure.COVERED_LUMBERMILL | b.ID == Structure.LUMBERMILL_BLOCK_ID)
                                    {
                                        MakeQuestCompleted();
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case ProgressQuestID.Progress_Reactor:
                        {
                            if (colony.HaveBuilding(Structure.GRPH_REACTOR_4_ID) | colony.HaveBuilding(Structure.REACTOR_BLOCK_5_ID)) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_FirstExpedition:
                        {
                            byte completeness = 0;
                            int count = Crew.crewsList.Count;
                            if (count > 0)
                            {
                                completeness++;
                                stepsFinished[0] = true;
                                stepsAddInfo[0] = count.ToString() + "/1";
                            }
                            else
                            {
                                stepsFinished[0] = false;
                                stepsAddInfo[0] = "0/1";
                            }
                            // shuttles
                            count = Hangar.GetTotalShuttlesCount();
                            if (count > 0)
                            {
                                completeness++;
                                stepsFinished[1] = true;
                                stepsAddInfo[1] = count.ToString() + "/1";
                            }
                            else { 
                                stepsFinished[1] = false;
                                stepsAddInfo[1] = "0/1";
                             }
                            //observatory
                            if (Observatory.alreadyBuilt)
                            {
                                completeness++;
                                stepsFinished[2] = true;
                            }
                            else stepsFinished[2] = false;
                            //transmitter\
                            count = QuantumTransmitter.transmittersList.Count;
                            if (count > 0)
                            {
                                completeness++;
                                stepsFinished[3] = true;
                                stepsAddInfo[3] = count.ToString() + "/1";
                            }
                            else
                            {
                                stepsFinished[3] = false;
                                stepsAddInfo[3] = "0/1";
                            }
                            // expeditions
                            if (Expedition.expeditionsLaunched > 0)
                            {
                                completeness++;
                                stepsFinished[4] = true;
                                stepsAddInfo[4] = Expedition.expeditionsLaunched.ToString() + "/1";
                            }
                            else
                            {
                                stepsFinished[4] = false;
                                stepsAddInfo[4] = "0/1";
                            }
                            // expeditions completed
                            if (Expedition.expeditionsSucceed >= 1)
                            {
                                completeness++;
                                stepsAddInfo[5] = Expedition.expeditionsSucceed.ToString() + "/1";
                                stepsFinished[5] = true;
                            }
                            else
                            {
                                stepsAddInfo[5] = "0/1";
                                stepsFinished[5] = false;
                            }
                            if (completeness == 6) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_Tier5:
                        if (colony.hq.level >= 5) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_FactoryComplex:
                        {
                            List<Building> powerGrid = colony.powerGrid;
                            List<ChunkPos> blocksPositions = new List<ChunkPos>(), factoriesPositions = new List<ChunkPos>();
                            foreach (Building b in powerGrid)
                            {
                                if (b == null) continue;
                                else
                                {
                                    if (b.ID == Structure.SMELTERY_BLOCK_ID)
                                    {
                                       blocksPositions.Add(b.GetBlockPosition());
                                    }
                                    else
                                    {
                                        if (b.ID == Structure.SMELTERY_3_ID | b.ID == Structure.SMELTERY_2_ID | b.ID == Structure.SMELTERY_1_ID)
                                        {
                                            factoriesPositions.Add(b.GetBlockPosition());
                                        }
                                    }
                                }
                            }
                            if (blocksPositions.Count != 0 & factoriesPositions.Count != 0)
                            {
                                bool founded = false;
                                foreach (ChunkPos cpos in blocksPositions)
                                {
                                    foreach (ChunkPos uppos in factoriesPositions)
                                    {
                                        if (uppos.x == cpos.x && uppos.y == cpos.y + 1 && uppos.z == cpos.z + 1)
                                        {
                                            MakeQuestCompleted();
                                            founded = true;
                                            break;
                                        }
                                        if (founded) break;
                                    }
                                }
                            }
                        }
                        break;
                    case ProgressQuestID.Progress_SecondFloor:
                        {
                            /*
                            List<ChunkPos> checkForBuildings = new List<ChunkPos>();
                            var slist = GameMaster.realMaster.mainChunk.GetSurfacesList();
                            foreach (Plane sb in slist)
                            {
                                if (sb == null || sb.noEmptySpace == false) continue;
                                {
                                    foreach (Structure s in sb.structures)
                                    {
                                        if (!s.isBasement) continue;
                                        if (s.ID == Structure.COLUMN_ID)
                                        {
                                            if (sb.pos.y < Chunk.CHUNK_SIZE) checkForBuildings.Add(new ChunkPos(sb.pos.x, sb.pos.y + 1, sb.pos.z));
                                        }
                                    }
                                }
                            }
                            if (checkForBuildings.Count > 0)
                            {
                                Chunk ch = GameMaster.realMaster.mainChunk;
                                foreach (ChunkPos cpos in checkForBuildings)
                                {
                                    Block b = ch.GetBlock(cpos);
                                    if (b == null) continue;
                                    else
                                    {
                                        Plane sb = b as Plane;
                                        if (sb == null) continue;
                                        else
                                        {
                                            if (sb.noEmptySpace != false & sb.artificialStructures > 0)
                                            {
                                                MakeQuestCompleted();
                                                break;
                                            }
                                        }
                                    }
                                }
                                stepsFinished[0] = true;
                            }
                            else stepsFinished[0] = false;
                            */
                        }
                        break;
                    case ProgressQuestID.Progress_FoodStocks:
                        {
                            var f = colony.storage.GetResourceCount(ResourceType.FOOD_ID);
                            var fmc = colony.foodMonthConsumption;
                            stepsAddInfo[0] = ((int)f).ToString() + '/' + ((int)fmc).ToString();
                            if (f >= fmc) MakeQuestCompleted();
                            break;
                        }
                }
                break;
            case QuestType.Endgame:
                // Сделать последовательное появление эффектов и не завершать кв, пока все не появится
                switch ((Knowledge.ResearchRoute)subIndex)
                {
                    case Knowledge.ResearchRoute.Foundation:
                        {
                            int a = colony.citizenCount, b = Knowledge.R_F_QUEST_POPULATION_COND;
                            steps[0] = "Текущее население: " + a.ToString() + " / " + b.ToString();
                            if (a >= b)
                            {
                                MakeQuestCompleted();
                            }
                            break;
                        }
                }
                break;
            case QuestType.Foundation:
                {
                    switch ((Knowledge.FoundationRouteBoosters)subIndex)
                    {
                        case Knowledge.FoundationRouteBoosters.HappinessBoost:
                            stepsAddInfo[0] = string.Format("{0:0.##}", colony.happinessCoefficient * 100) + '%'
                                + " / " + string.Format("{0:0.##}", Knowledge.R_F_HAPPINESS_COND * 100) + '%';
                            if (colony.happinessCoefficient >= Knowledge.R_F_HAPPINESS_COND) MakeQuestCompleted();
                            break;
                        case Knowledge.FoundationRouteBoosters.ImmigrantsBoost:
                            {
                                var ic = DockSystem.GetImmigrantsTotalCount();
                                var nic = Knowledge.R_F_IMMIGRANTS_CONDITION;
                                stepsAddInfo[0] = ic.ToString() + " / " + nic.ToString();
                                if (ic >= nic) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.FoundationRouteBoosters.PopulationBoost:
                            {
                                var cc = colony.citizenCount;
                                var nc = Knowledge.R_F_POPULATION_COND;
                                stepsAddInfo[0] = cc.ToString() + " / " + nc.ToString();
                                if (cc >= nc) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.FoundationRouteBoosters.SettlementBoost:
                            {
                                var hs = colony.houses;
                                if (hs != null && hs.Count > 0)
                                {
                                    foreach (var h in hs)
                                    {
                                        if (h is Settlement)
                                        {
                                            var s = h as Settlement;
                                            if (s.level >= Knowledge.R_F_SETTLEMENT_LEVEL_COND)
                                            {
                                                MakeQuestCompleted();
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case Knowledge.FoundationRouteBoosters.HotelBoost:
                            {
                                if (colony.GetBuildingsCount(Structure.HOTEL_BLOCK_6_ID) > 0) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.FoundationRouteBoosters.HousingMastBoost:
                            {
                                if (colony.GetBuildingsCount(Structure.HOUSING_MAST_6_ID) > 0) MakeQuestCompleted();
                                break;
                            }
                    }
                    break;
                }
            case QuestType.CloudWhale:
                {
                    switch ((Knowledge.CloudWhaleRouteBoosters)subIndex)
                    {
                        case Knowledge.CloudWhaleRouteBoosters.StreamGensBoost:
                            {
                                int count = colony.GetBuildingsCount(Structure.WIND_GENERATOR_1_ID);
                                if (count >= Knowledge.R_CW_STREAMGENS_COUNT_COND) MakeQuestCompleted();
                                else
                                {
                                    stepsAddInfo[0] = count.ToString()+ " /" + Knowledge.R_CW_STREAMGENS_COUNT_COND.ToString();
                                }
                                break;
                            }
                        case Knowledge.CloudWhaleRouteBoosters.CrewsBoost:
                            {
                                int count = 0;
                                var crewslist = Crew.crewsList;
                                if (crewslist != null)
                                {
                                    foreach (var c in crewslist)
                                    {
                                        if (c.level > Knowledge.R_CW_CREW_LEVEL_COND) count++;
                                    }
                                }
                                if (count >= Knowledge.R_CW_CREWS_COUNT_COND) MakeQuestCompleted();
                                else
                                {
                                    stepsAddInfo[0] = count.ToString() + " /" + Knowledge.R_CW_CREWS_COUNT_COND.ToString();
                                }
                                break;
                            }
                        case Knowledge.CloudWhaleRouteBoosters.ArtifactBoost:
                            {
                                var alist = Artifact.artifactsList;
                                if (alist != null && alist.Count > 0)
                                {
                                    foreach (var a in alist)
                                    {
                                        if (a.affectionPath == Path.TechPath && a.status != Artifact.ArtifactStatus.Uncontrollable)
                                        {
                                            if (a.status != Artifact.ArtifactStatus.Uncontrollable) MakeQuestCompleted();
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        case Knowledge.CloudWhaleRouteBoosters.XStationBoost:
                            {
                                if (XStation.current != null) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.CloudWhaleRouteBoosters.StabilityEnforcerBooster:
                            {
                                if (colony.HaveBuilding(Structure.STABILITY_ENFORCER_ID)) MakeQuestCompleted();
                                break;
                            }
                    }
                    break;
                }
            case QuestType.Engine:
                {
                    switch((Knowledge.EngineRouteBoosters)subIndex)
                    {
                        case Knowledge.EngineRouteBoosters.EnergyBoost:
                            {
                                var e = colony.energyStored;
                                if (e >= Knowledge.R_E_ENERGY_STORED_COND) MakeQuestCompleted();
                                else
                                {
                                    stepsAddInfo[0] = ((int)(e)).ToString() + " / " + Knowledge.R_E_ENERGY_STORED_COND.ToString();
                                }
                                break;
                            }                       
                        case Knowledge.EngineRouteBoosters.GearsBoost:
                            {
                                var g = colony.gears_coefficient;
                                if (g >= Knowledge.R_E_GEARS_COND) MakeQuestCompleted();
                                else
                                {
                                    stepsAddInfo[0] = string.Format("{0:0.#}", g) + " / " + Knowledge.R_E_GEARS_COND.ToString();
                                }
                                break;
                            }
                        case Knowledge.EngineRouteBoosters.FactoryBoost:
                            {
                                int count = colony.GetBuildingsCount(Structure.SMELTERY_BLOCK_ID);
                                if (count >= Knowledge.R_E_FACTORYCUBES_COUNT) MakeQuestCompleted();
                                else stepsAddInfo[0] = count.ToString() + " / " + Knowledge.R_E_FACTORYCUBES_COUNT.ToString();
                                break;
                            }
                        case Knowledge.EngineRouteBoosters.IslandEngineBoost:
                            {
                                if (colony.HaveBuilding(Structure.ENGINE_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.EngineRouteBoosters.ControlCenterBoost:
                            {
                                if (colony.HaveBuilding(Structure.CONTROL_CENTER_ID)) MakeQuestCompleted();
                                break;
                            }
                    }
                    break;
                }
            case QuestType.Pipes:
                {
                    switch((Knowledge.PipesRouteBoosters)subIndex)
                    {
                        case Knowledge.PipesRouteBoosters.FarmsBoost:
                            {
                                int count1 = colony.GetBuildingsCount<CoveredFarm>(), 
                                    count2 = colony.GetBuildingsCount<Farm>();
                                if (count2 == 0 && count1 > 0) MakeQuestCompleted();
                                else
                                {
                                    stepsAddInfo[0] = count2.ToString();
                                    stepsAddInfo[1] = count1.ToString();
                                }                               
                                break;
                            }
                        case Knowledge.PipesRouteBoosters.SizeBoost:
                            {
                                var blocks = GameMaster.realMaster.mainChunk.blocks;
                                byte xmax = 0, ymax = xmax, zmax = xmax;
                                if (blocks != null)
                                {
                                    //#islandSizeCheck
                                    if (blocks.Count < Knowledge.R_P_ISLAND_SIZE_COND)
                                    {
                                        MakeQuestCompleted();
                                        break;
                                    }
                                    else
                                    {
                                        var csize = Chunk.chunkSize;
                                        byte xmin = csize, ymin = xmin,  zmin = xmin;
                                        ChunkPos cpos;
                                        foreach (var b in blocks)
                                        {
                                            cpos = b.Value.pos;
                                            if (cpos.x > xmax) xmax = cpos.x;
                                            else
                                            {
                                                if (cpos.x < xmin) xmin = cpos.x;
                                            }
                                            if (cpos.y > ymax) ymax = cpos.y;
                                            else
                                            {
                                                if (cpos.y < ymin) ymin = cpos.y;
                                            }
                                            if (cpos.z > zmax) zmax = cpos.z;
                                            else
                                            {
                                                if (cpos.z < zmin) zmin = cpos.z;
                                            }
                                        }
                                        int xsize = xmax - xmin, ysize = ymax - ymin, zsize = zmax - zmin;
                                        byte cond = 0;
                                        int sc = Knowledge.R_P_ISLAND_SIZE_COND;
                                        if (xsize <= sc) cond++;
                                        if (ysize <= sc) cond++;
                                        if (zsize <= sc) cond++;
                                        if (cond >= 2)
                                        {
                                            MakeQuestCompleted();
                                        }
                                    }
                                }
                                stepsAddInfo[0] = xmax.ToString();
                                stepsAddInfo[1] = ymax.ToString();
                                stepsAddInfo[2] = zmax.ToString();
                                //
                                break;
                            }
                        case Knowledge.PipesRouteBoosters.FuelBoost:
                            {
                                int v = (int)GameMaster.realMaster.colonyController.storage.GetResourceCount(ResourceType.Fuel);
                                stepsAddInfo[0] = v.ToString() + " / " + ((int)Knowledge.R_P_FUEL_CONDITION).ToString();
                                if (v >= Knowledge.R_P_FUEL_CONDITION) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PipesRouteBoosters.BiomesBoost:
                            {
                                var e = GameMaster.realMaster.globalMap.GetCurrentEnvironment();
                                if (e.presetType == Environment.EnvironmentPreset.Ocean) stepsFinished[0] = true;
                                else
                                {
                                    if (e.presetType == Environment.EnvironmentPreset.Fire) stepsFinished[1] = true;
                                    else
                                    {
                                        if (e.presetType == Environment.EnvironmentPreset.Space) stepsFinished[2] = true;
                                        else
                                        {
                                            if (e.presetType == Environment.EnvironmentPreset.Meadows) stepsFinished[3] = true;
                                        }
                                    }
                                }
                                if (stepsFinished[0] & stepsFinished[1] & stepsFinished[2] & stepsFinished[3]) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PipesRouteBoosters.QETBoost:
                            {
                                if (QuantumEnergyTransmitter.current != null) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PipesRouteBoosters.CapacitorMastBoost:
                            {
                                if (colony.HaveBuilding(Structure.CAPACITOR_MAST_ID)) MakeQuestCompleted();
                                break;
                            }
                    }
                    break;
                }
            case QuestType.Crystal:
                {
                    switch ((Knowledge.CrystalRouteBoosters)subIndex)
                    {
                        case Knowledge.CrystalRouteBoosters.MoneyBoost:
                            {
                                int count = (int)colony.energyCrystalsCount;
                                stepsAddInfo[0] = ((int)count).ToString() + " / " + Knowledge.R_C_MONEY_COND.ToString();
                                if (count >= Knowledge.R_C_MONEY_COND) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.CrystalRouteBoosters.PinesBoost:
                            {
                                if (GameMaster.realMaster.mainChunk?.CheckForPlanttype(PlantType.CrystalPine) ?? false) MakeQuestCompleted(); 
                                break;
                            }
                        case Knowledge.CrystalRouteBoosters.GCubeBoost:
                            {
                                var blocks = GameMaster.realMaster.mainChunk?.blocks;
                                if (blocks != null)
                                {
                                    Block b;
                                    var sm = ResourceType.GRAPHONIUM_ID;
                                    foreach (var bv in blocks)
                                    {
                                        b = bv.Value;
                                        if (b != null && b.GetMaterialID() == sm)
                                        {
                                            MakeQuestCompleted();
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        case Knowledge.CrystalRouteBoosters.BiomeBoost:
                            {
                                if (GameMaster.realMaster.globalMap.GetCurrentEnvironment().presetType == Environment.EnvironmentPreset.Crystal) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.CrystalRouteBoosters.CrystalliserBoost:
                            {
                                if (colony.HaveBuilding(Structure.CRYSTALLISER_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.CrystalRouteBoosters.CrystalMastBoost:
                            {
                                if (colony.HaveBuilding(Structure.CRYSTAL_MAST_ID)) MakeQuestCompleted();
                                break;
                            }
                    }
                    break;
                }
            case QuestType.Monument:
                {
                    switch((Knowledge.MonumentRouteBoosters)subIndex)
                    {
                        case Knowledge.MonumentRouteBoosters.MonumentAffectionBoost:
                            {
                                var mlist = colony.GetBuildings<Monument>();
                                int mcount = 0, count = 0;
                                byte cond = 0;
                                if (mlist != null)
                                {
                                    mcount = mlist.Count;
                                    foreach (var m in mlist)
                                    {
                                        if (m.affectionPath == Path.TechPath && m.affectionValue == Knowledge.R_M_MONUMENTS_AFFECTION_CONDITION) count++;
                                    }
                                }
                                string s = " / " + Knowledge.R_M_MONUMENTS_COUNT_COND.ToString();
                                stepsAddInfo[0] = mcount.ToString() + s;
                                if (mcount >= Knowledge.R_M_MONUMENTS_COUNT_COND)
                                {
                                    stepsFinished[0] = true;
                                    cond++;
                                }
                                else stepsFinished[0] = false;
                                stepsAddInfo[1] = count.ToString() + s;
                                if (count >= Knowledge.R_M_MONUMENTS_COUNT_COND)
                                {
                                    stepsFinished[1] = true;
                                    cond++;
                                }
                                else
                                {
                                    stepsFinished[1] = false;
                                }
                                if (cond == 2) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.MonumentRouteBoosters.LifesourceBoost:
                            {
                                var list = GameMaster.realMaster.mainChunk?.TryGetNature()?.lifesources;
                                int count = 0;
                                if (list != null)
                                {
                                    count = list.Count;
                                    if (count >= 2) MakeQuestCompleted();
                                }
                                stepsAddInfo[0] = count.ToString() + " / 2";
                                break;
                            }
                        case Knowledge.MonumentRouteBoosters.BiomeBoost:
                            {
                                if (GameMaster.realMaster.globalMap.GetCurrentEnvironment().presetType == Environment.EnvironmentPreset.Ruins) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.MonumentRouteBoosters.ExpeditionsBoost:
                            {
                                var count = Expedition.expeditionsSucceed;
                                if (count >= Knowledge.R_M_SUCCESSFUL_EXPEDITIONS_COUNT_COND) MakeQuestCompleted();
                                else stepsAddInfo[0] = count.ToString() + " / " + Knowledge.R_M_SUCCESSFUL_EXPEDITIONS_COUNT_COND.ToString();
                                break;
                            }
                        case Knowledge.MonumentRouteBoosters.MonumentConstructionBoost:
                            {
                                if (colony.HaveBuilding(Structure.MONUMENT_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.MonumentRouteBoosters.AnchorMastBoost:
                            {
                                if (colony.HaveBuilding(Structure.ANCHOR_MAST_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.MonumentRouteBoosters.PointBoost:
                            break;
                    }
                    break;
                }
            case QuestType.Blossom:
                {
                    switch ((Knowledge.BlossomRouteBoosters)subIndex)
                    {
                        case Knowledge.BlossomRouteBoosters.GrasslandsBoost:
                            {
                                var c = GameMaster.realMaster.mainChunk;
                                var glist = c?.TryGetNature()?.GetGrasslandsList();
                                int gcount = 0;
                                if (glist != null)
                                {
                                    gcount = glist.Count;
                                }
                                float scount = c.GetSurfacesCount();
                                if (c != null && scount > 0)
                                {
                                    var f = gcount / scount;
                                    int pc = (int)(f * 100f);
                                    stepsAddInfo[0] = pc.ToString() + "% / " + ((int)Knowledge.R_B_GRASSLAND_RATIO_COND * 100f).ToString();
                                }
                                else stepsAddInfo[0] = "0% / " + ((int)Knowledge.R_B_GRASSLAND_RATIO_COND * 100f).ToString();
                                break;
                            }
                        case Knowledge.BlossomRouteBoosters.ArtifactBoost:
                            {
                                var alist = Artifact.artifactsList;
                                if (alist != null && alist.Count > 0)
                                {
                                    foreach (var a in alist)
                                    {
                                        if (a != null && a.affectionPath == Path.SecretPath)
                                        {
                                            MakeQuestCompleted();
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        case Knowledge.BlossomRouteBoosters.BiomeBoost:
                            {
                                if (GameMaster.realMaster.globalMap.GetCurrentEnvironment().presetType == Environment.EnvironmentPreset.Forest) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.BlossomRouteBoosters.Unknown:
                            break;
                        case Knowledge.BlossomRouteBoosters.GardensBoost:
                            {
                                break;
                            }
                        case Knowledge.BlossomRouteBoosters.HTowerBoost:
                            {
                                if (colony.HaveBuilding(Structure.HANGING_TMAST_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.BlossomRouteBoosters.PointBoost:
                            break;
                    }
                    break;
                }
            case QuestType.Pollen:
                {
                    switch ((Knowledge.PollenRouteBoosters)subIndex)
                    {
                        case Knowledge.PollenRouteBoosters.FlowersBoost:
                            {
                                if (GameMaster.realMaster.mainChunk?.CheckForPlanttype(PlantType.PollenFlower) ?? false) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PollenRouteBoosters.AscensionBoost:
                            {
                                var asc = GameMaster.realMaster.globalMap.ascension;
                                if (asc >= GameConstants.ASCENSION_HIGH) MakeQuestCompleted();
                                else stepsAddInfo[0] = ((int)(asc * 100f)).ToString() + "% /" + ((int)(GameConstants.ASCENSION_HIGH * 100f)).ToString() +'%';
                                break;
                            }
                        case Knowledge.PollenRouteBoosters.BiomeBoost:
                            {
                                if (GameMaster.realMaster.globalMap.GetCurrentEnvironment().presetType == Environment.EnvironmentPreset.Pollen) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PollenRouteBoosters.FilterBoost:
                            {
                                if (colony.HaveBuilding(Structure.RESOURCE_FILTER_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PollenRouteBoosters.ProtectorCoreBoost:
                            {
                                if (colony.HaveBuilding(Structure.PROTECTION_CORE_ID)) MakeQuestCompleted();
                                break;
                            }
                        case Knowledge.PollenRouteBoosters.PointBoost:
                            break;
                    }
                        break;
                }

        }
    }   
    protected void StructureCheck(Structure s)
    {
        switch (type)
        {
                          
        }
    }

    virtual public void StopQuest(bool uiRedrawCall)
    {
        if (completed) return;
        if (uiRedrawCall) QuestUI.current.ResetQuestCell(this);
        if (subscribedToStructuresCheck) GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.StructureCheck;
        completed = true;
    }
    virtual public void MakeQuestCompleted()
    {
        if (completed) return;
        if (type != QuestType.Scenario && type != QuestType.Condition)
        {
            AnnouncementCanvasController.MakeAnnouncement(Localization.AnnounceQuestCompleted(name));
            uint x = (uint)Mathf.Pow(2, subIndex);
            if ((questsCompletenessMask[(int)type] & x) == 0) questsCompletenessMask[(int)type] += x;
        }       
        QuestUI.current.ResetQuestCell(this);
        GameMaster.realMaster.colonyController.AddEnergyCrystals(reward);
        completed = true;
        if (subscribedToStructuresCheck) GameMaster.realMaster.eventTracker.buildingConstructionEvent -= this.StructureCheck;
    }
    public static void ResetHousingQuest() // if hq lvl up
    {
        uint m = questsCompletenessMask[(int)QuestType.Progress],
            v = (uint)Mathf.Pow(2, (byte)ProgressQuestID.Progress_HousesToMax);
        if ((m & v) != 0) m -= v;
    }

    virtual public void FillText(string[] s)
    {
        name = s[0];
        description = s[1];
        if (steps.Length == 1) { steps[0] = s[2]; }
        else
        {
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i] = s[i + 2];
            }
        }
    }
    virtual public void FillText(string i_name, string i_descr)
    {
        name = i_name;
        description = i_descr;
    }
    virtual public void FillText(string i_name, string i_descr, string i_step)
    {
        name = i_name;
        description = i_descr;
        steps[0] = i_step;
    }

    #region allQuestList
    public Quest GetAutogeneratedQuest()
    {
        return Quest.NoQuest;
    }

    public static Quest GetProgressQuest()
    {
        uint mask = questsCompletenessMask[(int)QuestType.Progress];
        List<ProgressQuestID> complementQuests = new List<ProgressQuestID>();
        for (int i = 0; i < (int)ProgressQuestID.LASTONE; i++)
        {
            if ((mask & (uint)Mathf.Pow(2, i)) == 0) complementQuests.Add((ProgressQuestID)i); // loool
        }

        if (complementQuests.Count > 0)
        {
            ColonyController colony = GameMaster.realMaster.colonyController;
            int lvl = colony.hq?.level ?? -1;
            if (lvl == -1) return Quest.NoQuest;
            List<ProgressQuestID> acceptableQuest = new List<ProgressQuestID>();
            var storage = colony.storage;

            for (int i = 0; i < complementQuests.Count; i++)
            {
                ProgressQuestID q = complementQuests[i];
                switch (q)
                {
                    case ProgressQuestID.Progress_HousesToMax: if (colony.housingLevel < lvl & lvl != 4) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_FoodStocks:
                        {
                            if (storage.GetResourceCount(ResourceType.Food) < colony.foodMonthConsumption / 2f) acceptableQuest.Add(q);break;
                        }
                    case ProgressQuestID.Progress_2Docks: if (colony.docks.Count < 2 & lvl > 1) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_2Storages: if (colony.storage.warehouses.Count < 2) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier2:
                        {
                            if (
                                lvl == 1
                                &
                                ((questsCompletenessMask[(int)QuestType.Progress] & (uint)Mathf.Pow(2, (byte)ProgressQuestID.Progress_HousesToMax)) != 0)
                                )
                                acceptableQuest.Add(q);
                            break;
                        }
                    case ProgressQuestID.Progress_300Population: if (colony.citizenCount < 250) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_OreRefiner: if (lvl >= 2 && !colony.HaveBuilding(Structure.ORE_ENRICHER_2_ID) ) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_HospitalCoverage: if (colony.hospitals_coefficient < 1 & lvl > 1) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier3: if (lvl == 2) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_4MiniReactors: if (lvl == 4) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_100Fuel: if (storage.GetResourceCount(ResourceType.Fuel) < 90 & lvl > 3) acceptableQuest.Add(q); break;
                    //case ProgressQuestID.Progress_XStation: if (lvl > 2 & XStation.current == null ) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier4: if (lvl == 3) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_CoveredFarm:
                    case ProgressQuestID.Progress_CoveredLumbermill:
                    case ProgressQuestID.Progress_FirstExpedition: if (lvl > 3) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Reactor: if (lvl > 4) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier5: if (lvl == 4) acceptableQuest.Add(q); break;
                    //case ProgressQuestID.Progress_FactoryComplex: if (lvl > 4) acceptableQuest.Add(q); break;
                    //case ProgressQuestID.Progress_SecondFloor: if (lvl > 2) acceptableQuest.Add(q); break;
                }
            }
            if (acceptableQuest.Count > 0)
            {
                ProgressQuestID pqi = acceptableQuest[Random.Range(0, acceptableQuest.Count)];
                Quest q = new Quest(QuestType.Progress, (byte)pqi);
                q.CheckQuestConditions();
                return q;
            }
            else return Quest.NoQuest;
        }
        else return Quest.NoQuest;
    }
    #endregion

    public static void SetQuestTexture(Quest q, UnityEngine.UI.Image buttonImage, UnityEngine.UI.RawImage iconPlace)
    {
        // for square textures only
        Texture icon = null;
        Rect iconRect = Rect.zero;
        switch (q.type)
        {
            case QuestType.Progress:
                {
                    switch ((ProgressQuestID)q.subIndex)
                    {
                        default: return;
                        case ProgressQuestID.Progress_HousesToMax:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.SETTLEMENT_CENTER_ID);
                            break;
                        case ProgressQuestID.Progress_2Docks:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.DOCK_ID);
                            break;
                        case ProgressQuestID.Progress_2Storages:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.STORAGE_0_ID);
                            break;
                        case ProgressQuestID.Progress_Tier2:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_300Population:
                            icon = UIController.iconsTexture;
                            iconRect = UIController.GetIconUVRect(Icons.Citizen);
                            break;
                        case ProgressQuestID.Progress_OreRefiner:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.ORE_ENRICHER_2_ID);
                            break;
                        case ProgressQuestID.Progress_HospitalCoverage:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HOSPITAL_ID);
                            break;
                        case ProgressQuestID.Progress_Tier3:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_4MiniReactors:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.MINI_GRPH_REACTOR_3_ID);
                            break;
                        case ProgressQuestID.Progress_100Fuel:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.resourcesIcons;
                            iconRect = ResourceType.GetResourceIconRect(ResourceType.FUEL_ID);
                            break;
                        case ProgressQuestID.Progress_XStation:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.XSTATION_3_ID);
                            break;
                        case ProgressQuestID.Progress_Tier4:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_CoveredFarm:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.COVERED_FARM);
                            break;
                        case ProgressQuestID.Progress_CoveredLumbermill:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.COVERED_LUMBERMILL);
                            break;
                        case ProgressQuestID.Progress_Reactor:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.GRPH_REACTOR_4_ID);
                            break;
                        case ProgressQuestID.Progress_FirstExpedition:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.iconsTexture;
                            iconRect = UIController.GetIconUVRect(Icons.GuidingStar);
                            break;
                        case ProgressQuestID.Progress_Tier5:
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_FactoryComplex:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.SMELTERY_BLOCK_ID);
                            break;
                        case ProgressQuestID.Progress_SecondFloor:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.COLUMN_ID);
                            break;
                        case ProgressQuestID.Progress_FoodStocks:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.GetCurrent()?.GetMainCanvasController()?.resourcesIcons;
                            iconRect = ResourceType.GetResourceIconRect(ResourceType.SUPPLIES_ID);
                            break;
                    }
                    break;
                }
            case QuestType.Endgame:
                {
                    icon = Resources.Load<Texture>("Textures/endGameIcons");
                    switch ((Knowledge.ResearchRoute)q.subIndex)
                    {
                        case Knowledge.ResearchRoute.Foundation:
                            icon = UIController.iconsTexture;
                            iconRect = UIController.GetIconUVRect(Icons.FoundationRoute);
                            break;
                    }
                    break;
                }
            case QuestType.Foundation:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.FoundationRoute);
                break;
            case QuestType.CloudWhale:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.CloudWhaleRoute);
                break;
            case QuestType.Engine:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.EngineRoute);
                break;
            case QuestType.Pipes:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.PipesRoute);
                break;
            case QuestType.Crystal:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.CrystalRoute);
                break;
            case QuestType.Monument:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.MonumentRoute);
                break;
            case QuestType.Blossom:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.BlossomRoute);
                break;
            case QuestType.Pollen:
                icon = UIController.iconsTexture;
                iconRect = UIController.GetIconUVRect(Icons.PollenRoute);
                break;
            case QuestType.Scenario:
                (q as ScenarioQuest).GetIconInfo(ref icon, ref iconRect);
                break;
            case QuestType.Condition:
                (q as ConditionQuest).GetIconInfo(ref icon, ref iconRect);
                break;
            default:
                icon = UIController.GetCurrent()?.GetMainCanvasController()?.buildingsIcons;
                iconRect = Structure.GetTextureRect(Structure.UNKNOWN_ID);
                break;
        }
        if (icon != null)
        {
            iconPlace.texture = icon;
            iconPlace.uvRect = iconRect;
            iconPlace.enabled = true;
        }
        else
        {
            iconPlace.texture = null;
            iconPlace.enabled = false;
        }
    }

    #region save-load
    public virtual List<byte> Save()
    {
        var data = new List<byte>() { (byte)type, subIndex };
        int stepsCount = stepsFinished.Length;
        data.AddRange(System.BitConverter.GetBytes(stepsCount));
        byte one = 1, zero = 0;
        if (stepsCount > 0)
        {
            foreach (bool b in stepsFinished)
            {
                data.Add(b ? one : zero);
            }
        }
        return data;
    }
    public static Quest Load(System.IO.Stream fs)
    {
        Quest q = new Quest((QuestType)fs.ReadByte(), (byte)fs.ReadByte());
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int stepsCount = System.BitConverter.ToInt32(data, 0);
        data = new byte[stepsCount];
        fs.Read(data, 0, data.Length);
        q.stepsFinished = new bool[stepsCount];
        for (int i = 0; i < stepsCount; i++)
        {
            q.stepsFinished[i] = data[i] == 1;
        }
        return q;
    }
    
    #endregion
}
