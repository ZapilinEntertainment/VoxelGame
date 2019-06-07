using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType : byte
{
    System, Progress, Endgame, Total
}
// ограничения на кол-во - до 32х, иначе не влезет в questCompleteMask
public enum ProgressQuestID : byte
{
    Progress_HousesToMax, Progress_2Docks, Progress_2Storages, Progress_Tier2, Progress_300Population, Progress_OreRefiner, Progress_HospitalCoverage, Progress_Tier3,
    Progress_4MiniReactors, Progress_100Fuel, Progress_XStation, Progress_Tier4, Progress_CoveredFarm, Progress_CoveredLumbermill, Progress_Reactor, Progress_FirstExpedition,
    Progress_Tier5, Progress_FactoryComplex, Progress_SecondFloor, LASTONE
}
public enum EndgameQuestID : byte
{
    Endgame_TransportHub_step1, Endgame_TransportHub_step2, Endgame_TransportHub_step3
}

public class Quest
{
    public string name;
    public string description;
    public float reward { get; private set; }

    public string[] steps { get; private set; }
    public string[] stepsAddInfo { get; private set; }
    public bool[] stepsFinished { get; private set; }
    public readonly QuestType type;
    public readonly byte subIndex;

    public static uint[] questsCompletenessMask { get; private set; } // до 32-х квестов на ветку
    public static readonly Quest NoQuest, AwaitingQuest;

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
    public static bool operator ==(Quest A, Quest B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return ((A.type == B.type) && (A.subIndex == B.subIndex));
    }
    public static bool operator !=(Quest A, Quest B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        var hashCode = 67631244;
        hashCode = hashCode * -1521134295 + type.GetHashCode();
        hashCode = hashCode * -1521134295 + subIndex.GetHashCode();
        return hashCode;
    }
    public override bool Equals(object obj)
    {
        if (!(obj is Quest))
        {
            return false;
        }

        var info = (Quest)obj;
        return type == info.type &&
               subIndex == info.subIndex;
    }

    public static void SetCompletenessMask(uint[] m)
    {
        questsCompletenessMask = m;
    }

    public Quest(QuestType i_type, byte subID)
    {
        type = i_type;
        subIndex = subID;
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
                        case ProgressQuestID.Progress_2Storages: reward = 100; break;
                        case ProgressQuestID.Progress_Tier2: reward = 120; break;
                        case ProgressQuestID.Progress_300Population: reward = 100; break;
                        case ProgressQuestID.Progress_OreRefiner: reward = 200; break;
                        case ProgressQuestID.Progress_HospitalCoverage: reward = 240; break;
                        case ProgressQuestID.Progress_Tier3: reward = 240; break;
                        case ProgressQuestID.Progress_4MiniReactors: reward = 800; break;
                        case ProgressQuestID.Progress_100Fuel: reward = 210; break;
                        case ProgressQuestID.Progress_XStation: reward = 120; break;
                        case ProgressQuestID.Progress_Tier4: reward = 480; break;
                        case ProgressQuestID.Progress_CoveredFarm: reward = 200; break;
                        case ProgressQuestID.Progress_CoveredLumbermill: reward = 200; break;
                        case ProgressQuestID.Progress_Reactor: reward = 220; break;
                        case ProgressQuestID.Progress_FirstExpedition:
                            defaultSettings = false;
                            stepsCount = 4;
                            reward = 400;
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
                switch ((EndgameQuestID)subID)
                {
                    case EndgameQuestID.Endgame_TransportHub_step1:
                        stepsCount = 3;
                        reward = 1000;
                        break;
                    case EndgameQuestID.Endgame_TransportHub_step2:
                        stepsCount = 2;
                        reward = 1000;
                        break;
                    case EndgameQuestID.Endgame_TransportHub_step3:
                        stepsCount = 3;
                        reward = 1000;
                        break;
                }
                break;
            case QuestType.System:
                if (subID == NO_QUEST_SUBINDEX) name = "no quest";
                else name = "awaiting quest";
                break;
        }
        steps = new string[stepsCount];
        stepsAddInfo = new string[stepsCount];
        stepsFinished = new bool[stepsCount];
        Localization.FillProgressQuest(this);
    }

    public void CheckQuestConditions()
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
                        stepsAddInfo[0] = colony.citizenCount.ToString() + "/300";
                        if (colony.citizenCount >= 300) MakeQuestCompleted();
                        break;
                    case ProgressQuestID.Progress_OreRefiner:
                        {
                            List<Building> powerGrid = colony.powerGrid;
                            foreach (Building b in powerGrid)
                            {
                                if (b == null) continue;
                                else
                                {
                                    if (b.id == Structure.ORE_ENRICHER_2_ID)
                                    {
                                        MakeQuestCompleted();
                                        break;
                                    }
                                }
                            }
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
                            List<Building> powerGrid = colony.powerGrid;
                            byte mrc = 0;
                            foreach (Building b in powerGrid)
                            {
                                if (b == null) continue;
                                else
                                {
                                    if (b.id == Structure.MINI_GRPH_REACTOR_3_ID) mrc++;
                                }
                            }
                            stepsAddInfo[0] = mrc.ToString() + "/4";
                            if (mrc >= 4) MakeQuestCompleted();
                        }
                        break;
                    case ProgressQuestID.Progress_100Fuel:
                        {
                            int f = (int)colony.storage.standartResources[ResourceType.FUEL_ID];
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
                                    if (b.id == Structure.FARM_4_ID | b.id == Structure.FARM_5_ID)
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
                                    if (b.id == Structure.LUMBERMILL_4_ID | b.id == Structure.LUMBERMILL_5_ID)
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
                            List<Building> powerGrid = colony.powerGrid;
                            foreach (Building b in powerGrid)
                            {
                                if (b == null) continue;
                                else
                                {
                                    if (b.id == Structure.GRPH_REACTOR_4_ID)
                                    {
                                        MakeQuestCompleted();
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case ProgressQuestID.Progress_FirstExpedition:
                        {
                            byte completeness = 0;
                            if (Crew.crewsList.Count > 0)
                            {
                                completeness++;
                                stepsFinished[0] = true;
                                stepsAddInfo[0] = Crew.crewsList.Count.ToString() + "/1";
                            }
                            else
                            {
                                stepsFinished[0] = false;
                                stepsAddInfo[0] = "0/1";
                            }


                            if (Expedition.expeditionsList.Count > 0)
                            {
                                completeness++;
                                stepsFinished[2] = true;
                                stepsAddInfo[2] = Expedition.expeditionsList.Count.ToString() + "/1";
                            }
                            else
                            {
                                stepsFinished[2] = false;
                                stepsAddInfo[2] = "0/1";
                            }
                            if (Expedition.expeditionsSucceed >= 1)
                            {
                                completeness++;
                                stepsAddInfo[3] = Expedition.expeditionsSucceed.ToString() + "/1";
                                stepsFinished[3] = true;
                            }
                            else
                            {
                                stepsAddInfo[3] = "0/1";
                                stepsFinished[3] = false;
                            }
                            if (completeness == 4) MakeQuestCompleted();
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
                                    if (b.id == Structure.SMELTERY_5_ID)
                                    {
                                        if (b.basement != null) blocksPositions.Add(b.basement.pos);
                                    }
                                    else
                                    {
                                        if (b.id == Structure.SMELTERY_3_ID | b.id == Structure.SMELTERY_2_ID | b.id == Structure.SMELTERY_1_ID)
                                        {
                                            if (b.basement != null) factoriesPositions.Add(b.basement.pos);
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
                            List<ChunkPos> checkForBuildings = new List<ChunkPos>();
                            foreach (SurfaceBlock sb in GameMaster.realMaster.mainChunk.surfaceBlocks)
                            {
                                if (sb == null || sb.noEmptySpace == false) continue;
                                else
                                {
                                    foreach (Structure s in sb.structures)
                                    {
                                        if (!s.isBasement) continue;
                                        if (s.id == Structure.COLUMN_ID)
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
                                        SurfaceBlock sb = b as SurfaceBlock;
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
                        }
                        break;
                }
                break;
            case QuestType.Endgame:
                switch ((EndgameQuestID)subIndex)
                {
                    case EndgameQuestID.Endgame_TransportHub_step1:
                        {
                            byte conditionsMet = 0;

                            int docksNeeded = 4;
                            int docksCount = colony.docks.Count;
                            if (docksCount >= docksNeeded)
                            {
                                stepsFinished[0] = true;
                                conditionsMet++;
                            }
                            else stepsFinished[0] = false;
                            stepsAddInfo[0] = docksCount.ToString() + " / " + docksNeeded.ToString();

                            if (colony.docksLevel >= 3)
                            {
                                stepsFinished[1] = true;
                                conditionsMet++;
                            }
                            else stepsFinished[1] = false;

                            int storagesNeeded = 2;
                            int storagesCount = 0;
                            foreach (StorageHouse sh in colony.storage.warehouses)
                            {
                                if (sh.level == 5) storagesCount++;
                            }
                            if (storagesCount >= storagesNeeded)
                            {
                                stepsFinished[2] = true;
                                conditionsMet++;
                            }
                            else stepsFinished[2] = false;
                            stepsAddInfo[2] = storagesCount.ToString() + " / " + storagesNeeded.ToString();

                            if (conditionsMet == 3) MakeQuestCompleted();
                        }
                        break;
                    case EndgameQuestID.Endgame_TransportHub_step2:
                        {
                            byte conditionsMet = 0;
                            if (ControlCenter.current != null)
                            {
                                stepsFinished[0] = true;
                                conditionsMet++;
                            }
                            else stepsFinished[0] = false;
                            if (ConnectTower.current != null)
                            {
                                stepsFinished[1] = true;
                                conditionsMet++;
                            }
                            else stepsFinished[1] = false;
                            if (conditionsMet == 2) MakeQuestCompleted();
                            break;
                        }
                    case EndgameQuestID.Endgame_TransportHub_step3:
                        {
                            byte conditionsMet = 0;
                            foreach (Building b in colony.powerGrid)
                            {
                                if (b.id == Structure.REACTOR_BLOCK_5_ID)
                                {
                                    stepsFinished[0] = true;
                                    conditionsMet++;
                                    break;
                                }
                            }
                            if (conditionsMet == 0) stepsFinished[0] = false;

                            byte housingMastsNeeded = 5, housingMastsCount = 0;
                            bool hotelFound = false;
                            foreach (Building b in colony.houses)
                            {
                                if (b.id == Structure.HOUSING_MAST_6_ID) housingMastsCount++; // можно запихнуть и в проверку выше
                                else
                                {
                                    if (b.id == Structure.HOTEL_BLOCK_6_ID) hotelFound = true;
                                }
                            }
                            if (housingMastsCount >= housingMastsNeeded)
                            {
                                stepsFinished[1] = true;
                                conditionsMet++;
                            }
                            else stepsFinished[1] = false;
                            stepsAddInfo[1] = housingMastsCount.ToString() + " / " + housingMastsNeeded.ToString();
                            stepsFinished[2] = hotelFound;
                            if (conditionsMet == 2 & hotelFound)
                            {
                                MakeQuestCompleted();
                                GameMaster.realMaster.GameOver(GameEndingType.TransportHubVictory);
                            }
                        }
                        break;
                }
                break;
        }
    }

    public void MakeQuestCompleted()
    {
        GameLogUI.MakeAnnouncement(Localization.AnnounceQuestCompleted(name));
        uint x = (uint)Mathf.Pow(2, subIndex);
        if ((questsCompletenessMask[(int)type] & x) == 0) questsCompletenessMask[(int)type] += x;
        if (type == QuestType.Endgame & subIndex != 2)
        {
            QuestUI.current.SetNewQuest((int)QuestSection.Endgame);
        }
        else QuestUI.current.ResetQuestCell(this);
        GameMaster.realMaster.colonyController.AddEnergyCrystals(reward);

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
            int lvl = colony.hq.level;
            List<ProgressQuestID> acceptableQuest = new List<ProgressQuestID>();
            for (int i = 0; i < complementQuests.Count; i++)
            {
                ProgressQuestID q = complementQuests[i];
                switch (q)
                {
                    case ProgressQuestID.Progress_HousesToMax: if (colony.housingLevel < lvl & lvl != 4) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_2Docks: if (colony.docks.Count < 2) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_2Storages: if (colony.storage.warehouses.Count < 2) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier2: if (lvl == 1) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_300Population: if (colony.citizenCount < 250) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_OreRefiner: if (lvl >= 2) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_HospitalCoverage: if (colony.hospitals_coefficient < 1 & lvl > 1) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier3: if (lvl == 2) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_4MiniReactors: if (lvl == 4) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_100Fuel: if (colony.storage.standartResources[ResourceType.FUEL_ID] < 90 & lvl > 3) acceptableQuest.Add(q); break;
                    //case ProgressQuestID.Progress_XStation: if (lvl > 2 & XStation.current == null ) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier4: if (lvl == 3) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_CoveredFarm:
                    case ProgressQuestID.Progress_CoveredLumbermill:
                    case ProgressQuestID.Progress_Reactor:
                    case ProgressQuestID.Progress_FirstExpedition: if (lvl > 3) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_Tier5: if (lvl == 4) acceptableQuest.Add(q); break;
                    //case ProgressQuestID.Progress_FactoryComplex: if (lvl > 4) acceptableQuest.Add(q); break;
                    case ProgressQuestID.Progress_SecondFloor: if (lvl > 2) acceptableQuest.Add(q); break;
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
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.SETTLEMENT_CENTER_ID);
                            break;
                        case ProgressQuestID.Progress_2Docks:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.DOCK_ID);
                            break;
                        case ProgressQuestID.Progress_2Storages:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.STORAGE_0_ID);
                            break;
                        case ProgressQuestID.Progress_Tier2:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_300Population:
                            icon = UIController.current.iconsTexture;
                            iconRect = UIController.GetTextureUV(Icons.Citizen);
                            break;
                        case ProgressQuestID.Progress_OreRefiner:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.ORE_ENRICHER_2_ID);
                            break;
                        case ProgressQuestID.Progress_HospitalCoverage:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HOSPITAL_2_ID);
                            break;
                        case ProgressQuestID.Progress_Tier3:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_4MiniReactors:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.MINI_GRPH_REACTOR_3_ID);
                            break;
                        case ProgressQuestID.Progress_100Fuel:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.resourcesIcons;
                            iconRect = ResourceType.GetResourceIconRect(ResourceType.FUEL_ID);
                            break;
                        case ProgressQuestID.Progress_XStation:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.XSTATION_3_ID);
                            break;
                        case ProgressQuestID.Progress_Tier4:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_CoveredFarm:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.FARM_4_ID);
                            break;
                        case ProgressQuestID.Progress_CoveredLumbermill:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.LUMBERMILL_4_ID);
                            break;
                        case ProgressQuestID.Progress_Reactor:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.GRPH_REACTOR_4_ID);
                            break;
                        case ProgressQuestID.Progress_FirstExpedition:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.iconsTexture;
                            iconRect = UIController.GetTextureUV(Icons.GuidingStar);
                            break;
                        case ProgressQuestID.Progress_Tier5:
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.HEADQUARTERS_ID);
                            break;
                        case ProgressQuestID.Progress_FactoryComplex:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.SMELTERY_5_ID);
                            break;
                        case ProgressQuestID.Progress_SecondFloor:
                            iconRect = new Rect(0, 0, 1, 1);
                            icon = UIController.current.buildingsIcons;
                            iconRect = Structure.GetTextureRect(Structure.COLUMN_ID);
                            break;
                    }
                    break;
                }
            case QuestType.Endgame:
                {
                    icon = Resources.Load<Texture>("Textures/endGameIcons");
                    switch ((EndgameQuestID)q.subIndex)
                    {
                        case EndgameQuestID.Endgame_TransportHub_step1:
                        case EndgameQuestID.Endgame_TransportHub_step2:
                        case EndgameQuestID.Endgame_TransportHub_step3:
                            iconRect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);
                            break;
                    }
                    break;
                }
            default: return;
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
    public List<byte> Save()
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
    public static Quest Load(System.IO.FileStream fs)
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
