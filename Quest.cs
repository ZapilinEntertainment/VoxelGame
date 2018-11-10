using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Progress
}
public enum ProgressQuestID
{
    Progress_HousesToMax, Progress_2Docks, Progress_2Storages, Progress_Tier2, Progress_300Population, Progress_OreRefiner, Progress_HospitalCoverage, Progress_Tier3,
    Progress_4MiniReactors, Progress_100Fuel, Progress_XStation, Progress_Tier4, Progress_CoveredFarm, Progress_CoveredLumbermill, Progress_Reactor, Progress_FirstExpedition,
    Progress_Tier5, Progress_FactoryComplex, Progress_SecondFloor, LASTONE
}
// also add new in GetQuestTexture

public class Quest {
    public string name = string.Empty;
    public string description = string.Empty;
    public bool picked = false;
    public bool completed { get; private set; }
    public bool canBeDelayed { get; private set; }
    public float questLifeTimer { get; private set; }
    public float questRealizationTimer { get; private set; }
    public float reward { get; private set; }

    public string[] steps { get; private set; }
    public string[] stepsAddInfo { get; private set; }
    public bool[] stepsFinished { get; private set; }
    public int shuttlesRequired { get; private set; }
    public int crewsRequired { get; private set; }
    public Expedition expedition { get; private set; }
    public List<Crew> crews { get; private set; }
    public readonly QuestType type;
    public readonly byte subIndex;

    public static uint[] questsCompletenessMask; // до 32-х квестов на ветку

    static Quest () {
        questsCompletenessMask = new uint[1];
	}    

    public Quest( QuestType i_type, byte subID)
    {
        type = i_type;
        subIndex = subID;
        crews = new List<Crew>();
        completed = false;
        bool setByDefault = true;
        switch (i_type)
        {
            case QuestType.Progress:
                switch ((ProgressQuestID)subIndex)
                {
                    case ProgressQuestID.Progress_HousesToMax: reward = 250; break;
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
                        setByDefault = false;
                        steps = new string[4];
                        stepsAddInfo = new string[4];
                        stepsFinished = new bool[4];
                        shuttlesRequired = 0;
                        crewsRequired = 0;
                        questLifeTimer = -1;
                        questRealizationTimer = -1;
                        reward = 400;
                        break;
                    case ProgressQuestID.Progress_Tier5: reward = 960; break;
                    case ProgressQuestID.Progress_FactoryComplex:
                        setByDefault = false;
                        steps = new string[2];
                        stepsAddInfo = new string[2];
                        stepsFinished = new bool[2];
                        shuttlesRequired = 0;
                        crewsRequired = 0;
                        questLifeTimer = -1;
                        questRealizationTimer = -1;
                        reward = 960;
                        break;
                    case ProgressQuestID.Progress_SecondFloor:
                        setByDefault = false;
                        steps = new string[2];
                        stepsAddInfo = new string[2];
                        stepsFinished = new bool[2];
                        shuttlesRequired = 0;
                        crewsRequired = 0;
                        questLifeTimer = -1;
                        questRealizationTimer = -1;
                        reward = 420;
                        break;
                }

                if (setByDefault)
                {
                    steps = new string[1];
                    stepsAddInfo = new string[1];
                    stepsFinished = new bool[1];
                    shuttlesRequired = 0;
                    crewsRequired = 0;
                    questLifeTimer = -1;
                    questRealizationTimer = -1;
                }
                Localization.FillProgressQuest(this);
                break;
        }
        canBeDelayed = true;
    }
    
    public void CheckQuestConditions()
    {
        ColonyController colony = GameMaster.colonyController;
        switch (type)
        {
            case QuestType.Progress:
                switch ((ProgressQuestID)subIndex)
                {
                    case ProgressQuestID.Progress_HousesToMax:
                        {
                            float hl = colony.housingLevel;
                            byte hql = colony.hq.level;
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


                            if (Shuttle.shuttlesList.Count > 0)
                            {
                                completeness++;
                                stepsFinished[1] = true;
                                stepsAddInfo[1] = Shuttle.shuttlesList.Count.ToString() + "/1";
                            }
                            else
                            {
                                stepsFinished[1] = false;
                                stepsAddInfo[1] = "0/1";
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
                            foreach (SurfaceBlock sb in GameMaster.mainChunk.surfaceBlocks)
                            {
                                if (sb == null || sb.cellsStatus == 0) continue;
                                else
                                {
                                    foreach (Structure s in sb.surfaceObjects)
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
                                Chunk ch = GameMaster.mainChunk;
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
                                            if (sb.cellsStatus != 0 & sb.artificialStructures > 0)
                                            {
                                                MakeQuestCompleted();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
                break;
        }
    }

    public void RemoveCrew(int index)
    {
        if (crews.Count <= index) return;
        else crews.RemoveAt(index);
    }

    public void MakeQuestCompleted()
    {
        completed = true;
        UIController.current.MakeAnnouncement(Localization.AnnounceQuestCompleted(name));
        switch (type)
        {
            case QuestType.Progress:
                uint x = (uint)Mathf.Pow(2, subIndex);
                if ((questsCompletenessMask[(int)type] & x) == 0) questsCompletenessMask[(int)type] += x;
                break;
        }
        GameMaster.colonyController.AddEnergyCrystals(reward);
    }

    public void Stop()
    {
        if (!picked) return;
        if (canBeDelayed)
        {
            switch (type) {
                case QuestType.Progress:
            switch ((ProgressQuestID)subIndex)
            {
                case ProgressQuestID.Progress_HousesToMax:
                case ProgressQuestID.Progress_2Docks:
                case ProgressQuestID.Progress_2Storages:
                case ProgressQuestID.Progress_Tier2:
                case ProgressQuestID.Progress_300Population:
                case ProgressQuestID.Progress_OreRefiner:
                case ProgressQuestID.Progress_HospitalCoverage:
                case ProgressQuestID.Progress_Tier3:
                case ProgressQuestID.Progress_4MiniReactors:
                case ProgressQuestID.Progress_100Fuel:
                case ProgressQuestID.Progress_XStation:
                case ProgressQuestID.Progress_Tier4:
                case ProgressQuestID.Progress_CoveredFarm:
                case ProgressQuestID.Progress_CoveredLumbermill:
                case ProgressQuestID.Progress_Reactor:
                case ProgressQuestID.Progress_FirstExpedition:
                case ProgressQuestID.Progress_Tier5:
                case ProgressQuestID.Progress_FactoryComplex:
                case ProgressQuestID.Progress_SecondFloor:
                            // nothing happens?
                            break;
            }
                    break;
        }
        }
    }

    #region allQuestList
    public Quest GetAutogeneratedQuest()
    {
        return null;
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
            ColonyController colony = GameMaster.colonyController;
            int lvl = colony.hq.level;
            List<ProgressQuestID> acceptableQuest = new List<ProgressQuestID>();
            for (int i = 0; i< complementQuests.Count;i++ )
            {
                switch ((ProgressQuestID)i)
                {
                    case ProgressQuestID.Progress_HousesToMax:   if (colony.housingLevel < lvl) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_2Docks: if (colony.docks.Count < 2) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_2Storages: if (colony.storage.warehouses.Count < 2) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_Tier2: if (lvl == 1) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_300Population: if (colony.citizenCount < 250) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_OreRefiner: if (lvl >= 2) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_HospitalCoverage: if (colony.hospitals_coefficient < 1 & lvl > 1) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_Tier3: if (lvl == 2) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_4MiniReactors: if (lvl == 4) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_100Fuel: if (colony.storage.standartResources[ResourceType.FUEL_ID] < 90 & lvl > 3) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_XStation: if (lvl > 2 & XStation.current == null ) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_Tier4: if (lvl == 3) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_CoveredFarm: 
                    case ProgressQuestID.Progress_CoveredLumbermill:
                    case ProgressQuestID.Progress_Reactor: 
                    case ProgressQuestID.Progress_FirstExpedition: if (lvl > 3) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_Tier5: if (lvl == 4) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_FactoryComplex: if (lvl > 4) acceptableQuest.Add(complementQuests[i]); break;
                    case ProgressQuestID.Progress_SecondFloor: if (lvl > 2) acceptableQuest.Add(complementQuests[i]); break;
                }
            }
            if (acceptableQuest.Count > 0)
            {
                ProgressQuestID pqi = acceptableQuest[(int)(Random.value * acceptableQuest.Count)];
                Quest q = new Quest(QuestType.Progress, (byte)pqi);                              
                q.picked = true;
                q.CheckQuestConditions();
                return q;
            }
            else return null;
        }
        else return null;        
    }
    #endregion

    public static void SetQuestTexture(Quest q, UnityEngine.UI.Image buttonImage, UnityEngine.UI.RawImage iconPlace)
    {
        // for square textures only
        Texture icon;
        Sprite overridingSprite = null;
        Rect iconRect;
        switch (q.type) {
            default: return;
            case QuestType.Progress:
            switch ((ProgressQuestID)q.subIndex)
        {
            default: return;
            case ProgressQuestID.Progress_HousesToMax:
                overridingSprite = QuestUI.questBuildingBack_tx;
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.HOUSE_1_ID);
                break;
            case ProgressQuestID.Progress_2Docks:
                overridingSprite = QuestUI.questBuildingBack_tx;
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.DOCK_ID);
                break;
            case ProgressQuestID.Progress_2Storages:
                overridingSprite = QuestUI.questBuildingBack_tx;
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.STORAGE_0_ID);
                break;
            case ProgressQuestID.Progress_Tier2:
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.HQ_2_ID);
                break;
            case ProgressQuestID.Progress_300Population:
                icon = UIController.current.iconsTexture;
                iconRect = UIController.GetTextureUV(Icons.Citizen);
                break;
            case ProgressQuestID.Progress_OreRefiner:
                overridingSprite = QuestUI.questBuildingBack_tx;
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.ORE_ENRICHER_2_ID);
                break;
            case ProgressQuestID.Progress_HospitalCoverage:
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.HOSPITAL_2_ID);
                break;
            case ProgressQuestID.Progress_Tier3:
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.HQ_2_ID);
                break;
            case ProgressQuestID.Progress_4MiniReactors:
                overridingSprite = QuestUI.questBuildingBack_tx;
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.MINI_GRPH_REACTOR_3_ID);
                break;
            case ProgressQuestID.Progress_100Fuel:
                overridingSprite = QuestUI.questResourceBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.resourcesTexture;
                iconRect = ResourceType.GetTextureRect(ResourceType.FUEL_ID);
                break;
            case ProgressQuestID.Progress_XStation:
                overridingSprite = QuestUI.questBuildingBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.XSTATION_3_ID);
                break;
            case ProgressQuestID.Progress_Tier4:
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.HQ_3_ID);
                break;
            case ProgressQuestID.Progress_CoveredFarm:
                overridingSprite = QuestUI.questBuildingBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.FARM_4_ID);
                break;
            case ProgressQuestID.Progress_CoveredLumbermill:
                overridingSprite = QuestUI.questBuildingBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.LUMBERMILL_4_ID);
                break;
            case ProgressQuestID.Progress_Reactor:
                overridingSprite = QuestUI.questBuildingBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.GRPH_REACTOR_4_ID);
                break;
            case ProgressQuestID.Progress_FirstExpedition:
                overridingSprite = QuestUI.questBlocked_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.iconsTexture;
                iconRect = UIController.GetTextureUV(Icons.GuidingStar);
                break;
            case ProgressQuestID.Progress_Tier5:
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.HQ_4_ID);
                break;
            case ProgressQuestID.Progress_FactoryComplex:
                overridingSprite = QuestUI.questBuildingBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.SMELTERY_5_ID);
                break;
            case ProgressQuestID.Progress_SecondFloor:
                overridingSprite = QuestUI.questBuildingBack_tx;
                iconRect = new Rect(0, 0, 1, 1);
                icon = UIController.current.buildingsTexture;
                iconRect = Structure.GetTextureRect(Structure.COLUMN_ID);
                break;
        }
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
        buttonImage.overrideSprite = overridingSprite;
    }

    #region save-load system
    /// <summary>
    /// use only in QuestUI.current.Save()
    /// </summary>
    /// <returns></returns>
    public static QuestStaticSerializer SaveStaticData() {
		QuestStaticSerializer qss = new QuestStaticSerializer();
        qss.questsCompletenessMask = questsCompletenessMask;	
		return qss;
	}
    /// <summary>
    /// use only on QuestUI.current.Load();
    /// </summary>
    /// <param name="qss"></param>
	public static void LoadStaticData(QuestStaticSerializer qss) {
        questsCompletenessMask = qss.questsCompletenessMask;
	}

	public QuestSerializer Save() {
		QuestSerializer qs = new QuestSerializer();
        qs.picked = picked;
        qs.completed = completed;
        qs.stepsFinished = stepsFinished;
        qs.type = type;
        qs.subIndex = subIndex;  
		return qs;
	}
	public static Quest Load(QuestSerializer qs) {
        Quest q = new Quest(qs.type, qs.subIndex);
        q.stepsFinished = qs.stepsFinished;
        q.completed = qs.completed;
        q.picked = qs.picked;
		return q;
	}
	#endregion
}

[System.Serializable]
public class QuestSerializer {
    public bool picked, completed;
    public bool[] stepsFinished;
    public QuestType type;
    public byte subIndex;    
}
[System.Serializable]
public class QuestStaticSerializer {
    public uint[] questsCompletenessMask; // до 32-х квестов на ветку
    public QuestSerializer[] visibleQuests;
}

