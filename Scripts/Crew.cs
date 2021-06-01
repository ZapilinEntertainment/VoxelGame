﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Path : byte { NoPath,LifePath, SecretPath, TechPath}
//dependency Localization.GetExpeditionName, TestYourMight

public sealed class Crew : MonoBehaviour {
    public const byte MIN_MEMBERS_COUNT = 3, MAX_MEMBER_COUNT = 9, MAX_ATTRIBUTE_VALUE = 20, MAX_LEVEL = 20;
    private const float NEUROPARAMETER_STEP = 0.05f, STAMINA_CONSUMPTION = 0.00003f, ADAPTABILITY_LOSSES = 0.02f;
    public const int MAX_CREWS_COUNT = 100;

    public static int nextID {get;private set;}	
    public static byte listChangesMarkerValue { get; private set; }
    public static List<Crew> crewsList { get; private set; }
    private static GameObject crewsContainer;

    public bool atHome { get; private set; }
	public byte membersCount {get;private set;}
	public int ID{get;private set;}
    public byte changesMarkerValue { get; private set; }
    public Expedition expedition { get; private set; }
    
    public bool? chanceMod { get; private set; }
    public byte persistence { get; private set; }
    public byte survivalSkills { get; private set; }
    public byte secretKnowledge { get; private set; }
    public byte perception { get; private set; }
    public byte intelligence { get; private set; }
    public byte techSkills { get; private set; }
    public byte freePoints { get; private set; }

    public byte level { get; private set; }
    public float stamina { get; private set; }
    public float confidence { get; private set; }
    public float unity { get; private set; }
    public float loyalty { get; private set; }
    public float adaptability { get; private set; }
    public int experience { get; private set; }
    public Path exploringPath{ get; private set; }
//при внесении изменений отредактировать Localization.GetCrewInfo

    public ushort missionsParticipated { get; private set; }
    public ushort missionsSuccessed{get;private set;}

    private const float SOFT_TEST_MAX_VALUE = 25f, STAMINA_RESTORE_SPEED = 0.05f;

    static Crew()
    {
        crewsList = new List<Crew>();
        GameMaster.realMaster.labourUpdateEvent += CrewsUpdateEvent;       
    }
    public static void CrewsUpdateEvent()
    {
        if (crewsList != null && crewsList.Count > 0)
        {
            foreach (Crew c in crewsList)
            {
                if (c.atHome)
                {
                    c.RestAtHome();
                }
            }
        }
    }

	public static void Reset() {
		crewsList = new List<Crew>();
		nextID = 0;
        listChangesMarkerValue = 0;
	}

    public static Crew CreateNewCrew(ColonyController home, int membersCount)
    {
        if (crewsList.Count >= RecruitingCenter.GetCrewsSlotsCount()) return null;
        if (crewsList.Count > MAX_CREWS_COUNT)
        {
            AnnouncementCanvasController.MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.CrewsLimitReached));
            GameMaster.LoadingFail();
            return null;
        }
        Crew c = new GameObject(Localization.NameCrew()).AddComponent<Crew>();
        if (crewsContainer == null) crewsContainer = new GameObject("crewsContainer");
        c.transform.parent = crewsContainer.transform;

        c.ID = nextID; nextID++;
        c.atHome = true;

        //normal parameters        
        c.membersCount = (byte)membersCount;
        //attributes
        c.SetAttributes(home);
        //
        crewsList.Add(c);        
        listChangesMarkerValue++;
        return c;
    }
    public static Crew GetCrewByID(int s_id)
    {
        if (s_id < 0 || crewsList.Count == 0) return null;
        else
        {
            foreach (Crew c in crewsList)
            {
                if (c != null && c.ID == s_id) return c;
            }
            return null;
        }
    }

    public void Rename(string s)
    {
        name = s;
        changesMarkerValue++;
    }
    /// <summary>
    /// for loading only
    /// </summary>
    public void SetCurrentExpedition(Expedition e)
    {
        if (e != null)
        {
            expedition = e;
            atHome = false;            
        }
        else
        {
            expedition = null;
            atHome = true;
        }
        changesMarkerValue++;
    }
    public void DrawCrewIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.iconsTexture;
        ri.uvRect = UIController.GetIconUVRect(stamina > 0.8f ? Icons.CrewGoodIcon : (stamina < 0.25f ? Icons.CrewBadIcon : Icons.CrewNormalIcon));
    }

    /// <summary>
    /// returns true if successfully completed
    /// </summary>
    /// <param name="friendliness"></param>
    /// <returns></returns>
    public bool SoftCheck( float friendliness, float situationValue) // INDEV
    {
        bool success = SOFT_TEST_MAX_VALUE * friendliness + situationValue < SoftCheckRoll();
        if (success)
        {
            RaiseUnity(1f);
            RaiseConfidence(0.75f);
        }
        return success;
    }
    public void LoseMember() // INDEV
    {
        membersCount--;        
        if (membersCount <= 0) Disappear();
        LoseConfidence(10f);
    }
	public void AddMember() { // INDEX
        membersCount++;
        LoseUnity(1f);
    }
    public void CountMission(bool victory)
    {
        missionsParticipated++;
        if (victory) missionsSuccessed++;
    }

    //system
    public void MarkAsDirty()
    {
        changesMarkerValue++;
    }

    #region attributes  
    private void SetAttributes(ColonyController c)
    {
        var values = new int[4];
        values[0] = Random.Range(0, 6); values[1] = Random.Range(0, 6);
        values[2] = Random.Range(0, 6); values[3] = Random.Range(0, 6);
        int minValIndex = 0;
        if (values[1] < values[0]) minValIndex = 1;
        if (values[2] < values[minValIndex]) minValIndex = 2;
        if (values[3] < values[minValIndex]) minValIndex = 3;
        perception = (byte)(values[0] + values[1] + values[2] + values[3] - values[minValIndex]);

        values[0] = Random.Range(0, 6); values[1] = Random.Range(0, 6);
        values[2] = Random.Range(0, 6); values[3] = Random.Range(0, 6);
        minValIndex = 0;
        if (values[1] < values[0]) minValIndex = 1;
        if (values[2] < values[minValIndex]) minValIndex = 2;
        if (values[3] < values[minValIndex]) minValIndex = 3;
        persistence = (byte)(values[0] + values[1] + values[2] + values[3] - values[minValIndex]);

        values[0] = Random.Range(0, 6); values[1] = Random.Range(0, 6);
        values[2] = Random.Range(0, 6); values[3] = Random.Range(0, 6);
        minValIndex = 0;
        if (values[1] < values[0]) minValIndex = 1;
        if (values[2] < values[minValIndex]) minValIndex = 2;
        if (values[3] < values[minValIndex]) minValIndex = 3;
        techSkills = (byte)(values[0] + values[1] + values[2] + values[3] - values[minValIndex]);

        values[0] = Random.Range(0, 6); values[1] = Random.Range(0, 6);
        values[2] = Random.Range(0, 6); values[3] = Random.Range(0, 6);
        minValIndex = 0;
        if (values[1] < values[0]) minValIndex = 1;
        if (values[2] < values[minValIndex]) minValIndex = 2;
        if (values[3] < values[minValIndex]) minValIndex = 3;
        survivalSkills = (byte)(values[0] + values[1] + values[2] + values[3] - values[minValIndex]);

        values[0] = Random.Range(0, 6); values[1] = Random.Range(0, 6);
        values[2] = Random.Range(0, 6); values[3] = Random.Range(0, 6);
        minValIndex = 0;
        if (values[1] < values[0]) minValIndex = 1;
        if (values[2] < values[minValIndex]) minValIndex = 2;
        if (values[3] < values[minValIndex]) minValIndex = 3;
        secretKnowledge = (byte)(values[0] + values[1] + values[2] + values[3] - values[minValIndex]);

        values[0] = Random.Range(0, 6); values[1] = Random.Range(0, 6);
        values[2] = Random.Range(0, 6); values[3] = Random.Range(0, 6);
        minValIndex = 0;
        if (values[1] < values[0]) minValIndex = 1;
        if (values[2] < values[minValIndex]) minValIndex = 2;
        if (values[3] < values[minValIndex]) minValIndex = 3;
        intelligence = (byte)(values[0] + values[1] + values[2] + values[3] - values[minValIndex]);

        confidence = 0.5f;
        unity = 0.5f;
        var happiness = c.happinessCoefficient;
        if (c != null) loyalty = happiness; else loyalty = 0.5f;
        adaptability = 0.5f;

        level = 1; experience = 0;
        stamina = 1f;
        RecalculatePath();

        if (happiness > GameConstants.HIGH_HAPPINESS) chanceMod = true;
        else
        {
            if (happiness < GameConstants.LOW_HAPPINESS) chanceMod = false;
            else chanceMod = null;
        }
    }

    private static float GetModifier(byte x)
    {
        return (x - 10f) / 2f;
    }

    public void RaiseConfidence(float f)
    {
        confidence += NEUROPARAMETER_STEP * f;
        if (confidence > 1f) confidence = 1f;
    }
    public void LoseConfidence(float f)
    {
        confidence -= NEUROPARAMETER_STEP * f;
        if (confidence < 0f) confidence = 0f;
    }
    public void RaiseUnity(float f)
    {
        unity += NEUROPARAMETER_STEP;
        if (unity > 1f) unity = 1f;
    }
    public void LoseUnity(float f)
    {
        unity -= NEUROPARAMETER_STEP * f;
        if (unity < 0f) unity = 0f;
    }
    public void RaiseLoyalty(float f)
    {
        loyalty += f * NEUROPARAMETER_STEP;
        if (loyalty > 1f) loyalty = 1f;
    }
    public void LoseLoyalty(float f)
    {
        loyalty -= f * NEUROPARAMETER_STEP;
        if (loyalty < 0f) loyalty = 0f;
    }
    public void RaiseAdaptability(float f)
    {
        adaptability += f * NEUROPARAMETER_STEP;
        if (adaptability > 1f) adaptability = 1f;
    }
    public void LoseAdaptibility(float f)
    {
        adaptability -= f * NEUROPARAMETER_STEP;
        if (adaptability < 0f) adaptability = 0f;
    }

    public void RestOnMission(float conditions)
    {        
        stamina += STAMINA_RESTORE_SPEED * level * conditions;
        if (stamina > 1f) stamina = 1f;
    }
    public void RestAtHome()
    {
        stamina = 1f;
        adaptability -= ADAPTABILITY_LOSSES;
        if (adaptability < 0f) adaptability = 0f;
        if (loyalty < GameMaster.realMaster.colonyController.happinessCoefficient) loyalty += NEUROPARAMETER_STEP;
    }
    public void StaminaDrain(float f)
    {
        float l = level;
        l /= MAX_LEVEL;
        stamina -= f * (1.2f - l);
        if (stamina < 0f) stamina = 0f;
    }

    public void SetChanceMod(bool? x)
    {
        chanceMod = x;
        changesMarkerValue++;
    }
    public void AddExperience(float x)
    {
        if (x < 1f) return;
        if (level < MAX_LEVEL)
        {
            experience += (int)x;
            int expCap = GetExperienceCap();
            if (experience >= expCap) LevelUp();
        }
        else experience = GetExperienceCap();
    }
    public void LevelUp()
    {
        level++;
        if (freePoints < 255) freePoints++;
        SetChanceMod(true);
    }
    public int GetExperienceCap()
    {
        switch (level)
        {
            case 1: return 50;
            case 2: return 100;
            case 3: return 120;
            case 4: return 125;
            case 5: return 130;
            case 6: return 142;
            case 7: return 156;
            case 8: return 172;
            case 9: return 190;
            case 10: return 210;
            case 11: return 232;
            case 12: return 256;
            case 13: return 280;
            case 14: return 320;
            case 15: return 360;
            case 16: return 420;
            case 17: return 480;
            default: return 512;
        }
    }
    public void ImprovePerception()
    {
        if (freePoints > 0 & perception < MAX_ATTRIBUTE_VALUE)
        {
            perception++;
            freePoints--;
            RecalculatePath();
        }
    }
    public void ImprovePersistence()
    {
        if (freePoints > 0 & persistence < MAX_ATTRIBUTE_VALUE)
        {
            persistence++;
            freePoints--;
            RecalculatePath();
        }
    }
    public void ImproveTechSkill()
    {
        if (freePoints > 0 & techSkills < MAX_ATTRIBUTE_VALUE)
        {
            techSkills++;
            freePoints--;
            RecalculatePath();
        }
    }
    public void ImproveSurvivalSkill()
    {
        if (freePoints > 0 & survivalSkills < MAX_ATTRIBUTE_VALUE)
        {
            survivalSkills++;
            freePoints--;
            RecalculatePath();
        }
    }
    public void ImproveSecretKnowledge()
    {
        if (freePoints > 0 & secretKnowledge < MAX_ATTRIBUTE_VALUE)
        {
            secretKnowledge++;
            freePoints--;
            RecalculatePath();
        }
    }
    public void ImproveIntelligence()
    {
        if (freePoints > 0 & intelligence < MAX_ATTRIBUTE_VALUE)
        {
            intelligence++;
            freePoints--;
            RecalculatePath();
        }
    }

    public float PersistenceRoll()
    {
        return Random.Range(0, 21) + GetModifier(persistence); // no neuro
    }
    public float SurvivalSkillsRoll()
    {
        return Random.Range(0, 21) + GetModifier(survivalSkills) * (0.9f + 0.15f * adaptability + 0.05f * unity);
    }
    public float PerceptionRoll()
    {
        return Random.Range(0, 21) + GetModifier(perception) * (0.9f + 0.2f * adaptability);
    }
    public float SecretKnowledgeRoll()
    {
        return Random.Range(0, 21) + GetModifier(secretKnowledge); // no neuro mod
    }
    public float IntelligenceRoll()
    {
        return Random.Range(0, 21) + GetModifier(intelligence) * (0.9f + 0.15f * adaptability + 0.05f * unity);
    }
    public float TechSkillsRoll()
    {
        return Random.Range(0, 21) + GetModifier(techSkills); // no neuro
    }

    public float HardTestRoll()
    {
        float val = Random.Range(0, 21);
        switch (exploringPath)
        {
            case Path.LifePath:
                val += (GetModifier(survivalSkills) * (0.9f + 0.6f * confidence)) * 0.7f + (GetModifier(persistence) * (0.9f + 0.6f * adaptability)) * 0.3f;
                break;
            case Path.SecretPath:
                val += (GetModifier(survivalSkills) * (0.9f + 0.2f * unity)) * 0.8f + (GetModifier(secretKnowledge) * (0.9f + 0.2f * adaptability)) * 0.2f;
                break;
            case Path.TechPath:
                val += (GetModifier(survivalSkills) * (0.9f + 0.2f * unity)) * 0.7f + (GetModifier(intelligence) * (0.9f + 0.1f * adaptability)) * 0.3f;
                break;
            default:
                val += GetModifier(survivalSkills) * (1f + 0.5f * adaptability);
                break;
        }
        return val;
    }
    public float SoftCheckRoll()
    {
        return Random.Range(0, 21) + GetModifier(persistence) * (0.5f + 0.3f * loyalty + 0.3f * confidence);
    }
    public float LoyaltyRoll()
    {
        return Random.Range(0, 21) + GetModifier((byte)(loyalty * MAX_ATTRIBUTE_VALUE));
    }
    public float AdaptabilityRoll()
    {
        return Random.Range(0, 21) + GetModifier((byte)(adaptability * MAX_ATTRIBUTE_VALUE));
    }
    public float ConfidenceRoll()
    {
        return Random.Range(0, 21) + GetModifier((byte)(confidence * MAX_ATTRIBUTE_VALUE));
    }
    public float UnityRoll()
    {
        return Random.Range(0, 21) + GetModifier((byte)(unity * MAX_ATTRIBUTE_VALUE));
    }
    public float RejectionRoll()
    {
        return Random.Range(0, 21) + GetModifier(persistence) * (0.5f + 0.5f * confidence + 0.3f * loyalty) + 0.1f * unity;
    }
    public bool TestYourMight(float difficultyClass, bool? advantage)
    {
        switch (exploringPath)
        {
            case Path.LifePath:
                {
                    if (advantage == null)
                    {
                        return (PersistenceRoll() >= difficultyClass);
                    }
                    else
                    {
                        float a = PersistenceRoll(), b = SurvivalSkillsRoll();
                        if (advantage == true)
                        {
                            if (b > a) a = b;
                        }
                        else
                        {
                            if (b < a) a = b;
                        }
                        return (a >= difficultyClass);
                    }
                }
            case Path.SecretPath:
                {
                    if (advantage == null)
                    {
                        return (SecretKnowledgeRoll() >= difficultyClass);
                    }
                    else
                    {
                        float a = SecretKnowledgeRoll(), b = PerceptionRoll();
                        if (advantage == true)
                        {
                            if (b > a) a = b;
                        }
                        else
                        {
                            if (b < a) a = b;
                        }
                        return (a >= difficultyClass);
                    }
                }
            case Path.TechPath:
                {
                    if (advantage == null) return (IntelligenceRoll() >= difficultyClass);
                    else
                    {
                        float a = IntelligenceRoll(), b = TechSkillsRoll();
                        if (advantage == true)
                        {
                            if (b > a) a = b;
                        }
                        else
                        {
                            if (b < a) a = b;
                        }
                        return (a >= difficultyClass);
                    }
                }
            default: return (UnityRoll() >= difficultyClass);
        }
    }
    #endregion

    public void Dismiss() {
        GameMaster.realMaster.colonyController.AddWorkers(membersCount);
        membersCount = 0;
        crewsList.Remove(this);
        listChangesMarkerValue++;
        Destroy(this);
    }
    public void Disappear()
    {
        crewsList.Remove(this);
        listChangesMarkerValue++;
        Destroy(this);
    }

    private void RecalculatePath()
    {
        int a = persistence + survivalSkills, b = perception + secretKnowledge, d = intelligence + techSkills;
        if (a >= b)
        {
            if (a >= d) exploringPath = Path.LifePath;
            else exploringPath = Path.TechPath;
        }
        else
        {
            if (b >= d) exploringPath = Path.SecretPath;
            else exploringPath = Path.TechPath;
        }
    }
    private void OnDestroy()
    {
        if (crewsList.Count == 0) UICrewObserver.DestroyObserver();
    }

    #region save-load system
    public static void SaveStaticData( System.IO.Stream fs)
    {
        int realCount = 0;
        var data = new List<byte>();
        if (crewsList.Count > 0)
        {
            foreach (var c in crewsList)
            {
                if (c != null)
                {
                    data.AddRange(c.Save());
                    realCount++;
                }
            }
        }
        fs.Write(System.BitConverter.GetBytes(realCount), 0, 4);
        if (realCount > 0)
        {
            var dataArray = data.ToArray();
            fs.Write(dataArray, 0, dataArray.Length);
        }
        fs.Write(System.BitConverter.GetBytes(nextID), 0, 4);
    }
    public static void LoadStaticData(System.IO.Stream fs)
    {
        if (crewsList == null) crewsList = new List<Crew>();
        if (crewsContainer == null) crewsContainer = new GameObject("crews container");
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int crewsCount = System.BitConverter.ToInt32(data, 0);

        if (crewsCount > 0)
        {
            if (crewsCount > MAX_CREWS_COUNT)
            {
                Debug.Log("crews loading error - overcount");
                GameMaster.LoadingFail();
                return;
            }
            for (int i = 0; i < crewsCount; i++)
            {
                Crew c = new GameObject().AddComponent<Crew>();
                c.transform.parent = crewsContainer.transform;
                c.Load(fs);
                crewsList.Add(c);
            }
        }
        else
        {
            if (crewsCount < 0)
            {
                Debug.Log("crews loading error - negative count");
                GameMaster.LoadingFail();
                return;
            }
        }

        fs.Read(data, 0, 4);
        nextID = System.BitConverter.ToInt32(data, 0);
    }

    public List<byte> Save()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(ID)); // 0 - 3
        byte truebyte = 1, falsebyte = 0, nullbyte = 2;
        data.AddRange(
            new byte[] {
            atHome ? truebyte : falsebyte, // 4
            membersCount, // 5
            chanceMod == null ? nullbyte : (chanceMod == true ? truebyte : falsebyte) , // 6
            persistence, // 7
            survivalSkills, // 8
            secretKnowledge, // 9
            perception, // 10
            intelligence, // 11
            techSkills, // 12
            freePoints, //13
            level //14
            });
        data.AddRange(System.BitConverter.GetBytes(stamina)); // 15 - 18
        data.AddRange(System.BitConverter.GetBytes(confidence)); // 19 - 22
        data.AddRange(System.BitConverter.GetBytes(unity)); // 23 - 26
        data.AddRange(System.BitConverter.GetBytes(loyalty)); // 27 - 30
        data.AddRange(System.BitConverter.GetBytes(adaptability)); // 31 - 34
        data.AddRange(System.BitConverter.GetBytes(experience)); // 35 - 38

        data.AddRange(System.BitConverter.GetBytes(missionsParticipated)); // 39 - 40
        data.AddRange(System.BitConverter.GetBytes(missionsSuccessed)); // 41 - 42

    //
    var nameArray = System.Text.Encoding.Default.GetBytes(name);
        int count = nameArray.Length;
        data.AddRange(System.BitConverter.GetBytes(count)); // 43 - 46 | количество байтов, не длина строки
        if (count > 0) data.AddRange(nameArray);
        
        return data;
    }
    public void Load(System.IO.Stream fs)
    {
        int LENGTH = 47;
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        ID = System.BitConverter.ToInt32(data,0);

        atHome = data[4] == 1;
        membersCount = data[5];
        if (data[6] == 2) chanceMod = null; else chanceMod = data[6] == 1;
        persistence = data[7];
        survivalSkills = data[8];
        secretKnowledge = data[9];
        perception = data[10];
        intelligence = data[11];
        techSkills = data[12];
        freePoints = data[13];
        level = data[14];
        RecalculatePath();

        stamina = System.BitConverter.ToSingle(data,15);
        confidence = System.BitConverter.ToSingle(data, 19);
        unity = System.BitConverter.ToSingle(data, 23);
        loyalty = System.BitConverter.ToSingle(data, 27);
        adaptability = System.BitConverter.ToSingle(data, 31);
        experience = System.BitConverter.ToInt32(data, 35);

        missionsParticipated = System.BitConverter.ToUInt16(data,39);
        missionsSuccessed = System.BitConverter.ToUInt16(data, 41);

        int bytesCount = System.BitConverter.ToInt32(data, 43); //выдаст количество байтов, не длину строки    
        if (bytesCount < 0 | bytesCount > 1000000)
        {
            Debug.Log("crew load error - name bytes count incorrect");
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
            name = new string(chars);
        }
    }

    #endregion
}
