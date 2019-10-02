using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class Crew : MonoBehaviour {
    public enum ExploringPath : byte { Default, PathOfLife, SecretPath, PathOfMind } // dependency : GetStaminaModifier
    public enum CrewStatus : byte { AtHome, OnMission, Travelling }
    // dependencies
    //Localization.GetCrewStatus()
    // Rest()
    public const byte MIN_MEMBERS_COUNT = 3, MAX_MEMBER_COUNT = 9;
    private const byte MAX_ATTRIBUTE_VALUE = 20, MAX_LEVEL = 20;
    private const float NEUROPARAMETER_STEP = 0.05f, STAMINA_CONSUMPTION = 0.00003f, STAMINA_REPLENISH_SPEED = 0.05f, ADAPTABILITY_LOSSES = 0.02f;

    public static int nextID {get;private set;}	
    public static byte listChangesMarkerValue { get; private set; }
    public static List<Crew> crewsList { get; private set; }
    private static GameObject crewsContainer;
    public static UICrewObserver crewObserver { get; private set; }

	public int membersCount {get;private set;}
	public int ID{get;private set;}
    public byte changesMarkerValue { get; private set; }
    public Artifact artifact { get; private set; }
    public Expedition currentExpedition { get; private set; }
	public Shuttle shuttle{get;private set;}
	public CrewStatus status { get; private set; }

    
    public byte persistence { get; private set; }
    public byte survivalSkills { get; private set; }
    public byte secretKnowledge { get; private set; }
    public byte perception { get; private set; }
    public byte intelligence { get; private set; }
    public byte techSkills { get; private set; }
    public byte level { get; private set; }
    public float stamina { get; private set; }
    public ushort proficiencies { get; private set; }
    public float confidence { get; private set; }
    public float unity { get; private set; }
    public float loyalty { get; private set; }
    public float adaptability { get; private set; }
    public int experience { get; private set; }
    public ExploringPath exploringPath{ get; private set; }
//при внесении изменений отредактировать Localization.GetCrewInfo

public int missionsParticipated { get; private set; }
    public int missionsSuccessed{get;private set;}    

    static Crew()
    {
        crewsList = new List<Crew>();
        GameMaster.realMaster.lifepowerUpdateEvent += CrewsUpdateEvent;
    }
    private Crew() { }
    public static void CrewsUpdateEvent()
    {
        if (crewsList != null && crewsList.Count > 0)
        {
            foreach (Crew c in crewsList)
            {
                if (c.status == CrewStatus.AtHome)
                {
                    c.Rest(null);
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
        Crew c = new GameObject(Localization.NameCrew()).AddComponent<Crew>();
        if (crewsContainer == null) crewsContainer = new GameObject("crewsContainer");
        c.transform.parent = crewsContainer.transform;

        c.ID = nextID; nextID++;
        c.status = CrewStatus.AtHome;

        //normal parameters        
        c.membersCount = membersCount;
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
    public static void DisableObserver()
    {
        if (crewObserver != null && crewObserver.gameObject.activeSelf) crewObserver.gameObject.SetActive(false);
    }

    public void ShowOnGUI(Rect rect, SpriteAlignment alignment, bool useCloseButton) 
    {
        if (crewObserver == null)
        {
           crewObserver = Instantiate(Resources.Load<GameObject>("UIPrefs/crewPanel"), UIController.current.mainCanvas).GetComponent<UICrewObserver>();
            crewObserver.LocalizeTitles();
        }
        if (!crewObserver.gameObject.activeSelf) crewObserver.gameObject.SetActive(true);
        crewObserver.SetPosition(rect, alignment);
        crewObserver.ShowCrew(this, useCloseButton);
    }

	public void SetShuttle(Shuttle s) {
        if (s == shuttle) return;
        else {
            if (s == null)
            {
                if (shuttle != null) shuttle.SetCrew(null);
                shuttle = null;
            }
            else
            {
                if (shuttle != null & s != shuttle) shuttle.SetCrew(null);
                s.SetCrew(this);
                shuttle = s;
            }
            changesMarkerValue++;
        }               
	}
    public void SetStatus(CrewStatus cs)
    {
        status = cs;
        if (shuttle != null)
        {
            if (status == CrewStatus.AtHome) shuttle.SetDockingStatus(true);
            else shuttle.SetDockingStatus(false);
        }
        changesMarkerValue++;
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
        currentExpedition = e;
        changesMarkerValue++;
    }
    public void DrawCrewIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.current.iconsTexture;
        ri.uvRect = UIController.GetTextureUV(stamina > 0.8f ? Icons.CrewGoodIcon : (stamina < 0.25f ? Icons.CrewBadIcon : Icons.CrewNormalIcon));
    }

    public bool HardTest(float hardness, float situationValue) // INDEV
    {
        if (artifact != null)
        {
            bool? protection = artifact.StabilityTest(hardness);
            if (protection == null) DropArtifact();
            else
            {
                if (protection == true)
                {
                    RaiseConfidence(1f);
                    return true;
                }
            }
        }
        //нет артефакта или защита не сработала
        bool success = hardness * GameConstants.HARD_TEST_MAX_VALUE + situationValue < HardTestRoll();
        if (success)
        {
            RaiseUnity(1f);
            RaiseAdaptability(1f);
            return true;
        }
        else return false;
    }

    /// <summary>
    /// returns true if successfully completed
    /// </summary>
    /// <param name="friendliness"></param>
    /// <returns></returns>
    public bool SoftCheck( float friendliness, float situationValue) // INDEV
    {
        bool success = GameConstants.SOFT_TEST_MAX_VALUE * friendliness + situationValue < SoftCheckRoll();
        if (success)
        {
            RaiseUnity(1f);
            RaiseConfidence(0.75f);
        }
        return success;
    }
    public void Rest(PointOfInterest place)
    {
        if (place != null)
        {
            Rest(place.GetRestValue());
            RaiseUnity(0.5f);
        }
        else // at home
        {
            Restore();
        }
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

    public void SetArtifact(Artifact a)
    {
        if (a == null) return;
        else
        {
            //если уже есть артефакт
            if (artifact != null)
            {
                if (status == CrewStatus.AtHome) artifact.Conservate();
                else artifact.Destroy();
            }
            //
            artifact = a;
            artifact.SetOwner(this);
            RaiseConfidence(2f);
            RaiseLoyalty(1f);
        }
    }
    public void DropArtifact()
    {
        if (artifact != null)
        {
            artifact.SetOwner(null);
            artifact = null;
            LoseConfidence(0.5f);
            listChangesMarkerValue++;
        }
    }
    //system
    public void ClearArtifactField(Artifact a)
    {
        if (artifact == a) artifact = null;
    }
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
        loyalty = c.happiness_coefficient;
        adaptability = 0.5f;

        level = 1; experience = 0;
        stamina = 1f;
        exploringPath = ExploringPath.Default;
        proficiencies = 0;
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
    public void Rest(float f)
    {        
        stamina += (int)(STAMINA_REPLENISH_SPEED * (1 + adaptability) * (1 + unity) * level * f);
        if (stamina > 1f) stamina = 1f;
    }
    public void Restore()
    {
        stamina = 1f;
        adaptability -= ADAPTABILITY_LOSSES;
        if (adaptability < 0f) adaptability = 0f;
        if (loyalty < GameMaster.realMaster.colonyController.happiness_coefficient) loyalty += NEUROPARAMETER_STEP;
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
        ushort proficiency = 2;
        if (level > 4)
        {
            if (level < 13)
            {
                if (level < 9) proficiency = 3; else proficiency = 4;
            }
            else
            {
                if (level > 16) proficiency = 5; else proficiency = 6;
            }
        }
        proficiencies += proficiency;
    }
    public int GetExperienceCap()
    {
        switch (level)
        {
            case 1: return 300;
            case 2: return 900;
            case 3: return 2700;
            case 4: return 6500;
            case 5: return 14000;
            case 6: return 23000;
            case 7: return 34000;
            case 8: return 48000;
            case 9: return 64000;
            case 10: return 85000;
            case 11: return 100000;
            case 12: return 12000;
            case 13: return 140000;
            case 14: return 165000;
            case 15: return 195000;
            case 16: return 225000;
            case 17: return 265000;
            case 18: return 305000;
            default: return 355000;
        }
    }
    public void ImprovePerception()
    {
        if (proficiencies > 0 & perception < MAX_ATTRIBUTE_VALUE)
        {
            perception++;
            proficiencies--;
        }
    }
    public void ImprovePersistence()
    {
        if (proficiencies > 0 & persistence < MAX_ATTRIBUTE_VALUE)
        {
            persistence++;
            proficiencies--;
        }
    }
    public void ImproveTechSkill()
    {
        if (proficiencies > 0 & techSkills < MAX_ATTRIBUTE_VALUE)
        {
            techSkills++;
            proficiencies--;
        }
    }
    public void ImproveSurvivalSkill()
    {
        if (proficiencies > 0 & survivalSkills < MAX_ATTRIBUTE_VALUE)
        {
            survivalSkills++;
            proficiencies--;
        }
    }
    public void ImproveEnlightment()
    {
        if (proficiencies > 0 & secretKnowledge < MAX_ATTRIBUTE_VALUE)
        {
            secretKnowledge++;
            proficiencies--;
        }
    }
    public void ImproveIntelligence()
    {
        if (proficiencies > 0 & intelligence < MAX_ATTRIBUTE_VALUE)
        {
            intelligence++;
            proficiencies--;
        }
    }

    public float PersistenceRoll()
    {
        return Random.Range(0, 20) + GetModifier(persistence); // no neuro
    }
    public float SurvivalSkillsRoll()
    {
        return Random.Range(0, 20) + GetModifier(survivalSkills) * (0.9f + 0.15f * adaptability + 0.05f * unity);
    }

    public float PerceptionRoll()
    {
        return Random.Range(0, 20) + GetModifier(perception) * (0.9f + 0.2f * adaptability);
    }
    public float SecretKnowledgeRoll()
    {
        return Random.Range(0, 20) + GetModifier(secretKnowledge); // no neuro mod
    }

    public float IntelligenceRoll()
    {
        return Random.Range(0, 20) + GetModifier(intelligence) * (0.9f + 0.15f * adaptability + 0.05f * unity);
    }
    public float TechSkillsRoll()
    {
        return Random.Range(0, 20) + GetModifier(techSkills); // no neuro
    }

    public float HardTestRoll()
    {
        float val = Random.Range(0, 20);
        switch (exploringPath)
        {
            case ExploringPath.PathOfLife:
                val += (GetModifier(survivalSkills) * (0.9f + 0.6f * confidence)) * 0.7f + (GetModifier(persistence) * (0.9f + 0.6f * adaptability)) * 0.3f;
                break;
            case ExploringPath.SecretPath:
                val += (GetModifier(survivalSkills) * (0.9f + 0.2f * unity)) * 0.8f + (GetModifier(secretKnowledge) * (0.9f + 0.2f * adaptability)) * 0.2f;
                break;
            case ExploringPath.PathOfMind:
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
        return Random.Range(0, 20) + GetModifier(persistence) * (0.5f + 0.3f * loyalty + 0.3f * confidence);
    }
    public float LoyaltyRoll()
    {
        return Random.Range(0, 20) + GetModifier((byte)(loyalty * MAX_ATTRIBUTE_VALUE));
    }
    public float AdaptabilityRoll()
    {
        return Random.Range(0, 20) + GetModifier((byte)(adaptability * MAX_ATTRIBUTE_VALUE));
    }
    public float ConfidenceRoll()
    {
        return Random.Range(0, 20) + GetModifier((byte)(confidence * MAX_ATTRIBUTE_VALUE));
    }
    public float UnityRoll()
    {
        return Random.Range(0, 20) + GetModifier((byte)(unity * MAX_ATTRIBUTE_VALUE));
    }
    public float RejectionRoll()
    {
        return Random.Range(0, 20) + GetModifier(persistence) * (0.5f + 0.5f * confidence + 0.3f * loyalty) + 0.1f * unity;
    }

    public float MakeStep(float difficulty)
    {        
        if (stamina <= 0)
        {
            if (loyalty < 0.33f && 10f * difficulty > RejectionRoll()) return -1f;
            else return 0f;
        }
        else
        {
            float step = GameConstants.EXPLORE_SPEED * (0.5f + 0.3f * unity + 0.5f * adaptability - 0.5f * difficulty);
            stamina-= STAMINA_CONSUMPTION * difficulty * (1f - 0.5f * level / (float)MAX_LEVEL);
            if (step < 0f) return 0f; else return step;
        }
    }
    public bool TestYourMight(float difficultyClass, bool? advantage)
    {
        switch (exploringPath)
        {
            case ExploringPath.PathOfLife:
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
            case ExploringPath.SecretPath:
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
            case ExploringPath.PathOfMind:
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
        if (shuttle != null && shuttle.crew == this) shuttle.SetCrew(null);
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

    private void OnDestroy()
    {
        if (crewsList.Count == 0 & crewObserver != null) Destroy(crewObserver);
    }

    #region save-load system
    public static void SaveStaticData( System.IO.FileStream fs)
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
    public static void LoadStaticData(System.IO.FileStream fs)
    {
        if (crewsList == null) crewsList = new List<Crew>();
        if (crewsContainer == null) crewsContainer = new GameObject("crews container");
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int crewsCount = System.BitConverter.ToInt32(data, 0);

        if (crewsCount > 0)
        {
            for (int i = 0; i < crewsCount; i++)
            {
                Crew c = new GameObject().AddComponent<Crew>();
                c.transform.parent = crewsContainer.transform;
                c.Load(fs);
                crewsList.Add(c);
            }
        }

        fs.Read(data, 0, 4);
        nextID = System.BitConverter.ToInt32(data, 0);
    }

    public List<byte> Save()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(ID)); // 0 - 3
        int nodata = -1;
        data.AddRange(System.BitConverter.GetBytes(shuttle == null ? nodata : shuttle.ID));//4- 7
        data.AddRange(System.BitConverter.GetBytes(artifact == null ? nodata : artifact.ID)); // 8 - 11
        data.AddRange(System.BitConverter.GetBytes(membersCount)); // 12 - 15
        data.AddRange(System.BitConverter.GetBytes(missionsSuccessed)); // 16 - 19
        data.AddRange(System.BitConverter.GetBytes(missionsParticipated)); // 20 - 23
        data.AddRange(new byte[]
        {
            (byte)status, // 24
            persistence, // 25
            survivalSkills, // 26
            secretKnowledge, // 27
            perception, // 28
            intelligence, // 29
            techSkills, // 30
            level, // 31
            (byte)exploringPath // 32
        });

        data.AddRange(System.BitConverter.GetBytes(stamina)); // 33-36
        data.AddRange(System.BitConverter.GetBytes(proficiencies)); //37-38

        data.AddRange(System.BitConverter.GetBytes(confidence)); // 39-42
        data.AddRange(System.BitConverter.GetBytes(unity)); // 43-46
        data.AddRange(System.BitConverter.GetBytes(loyalty)); // 47-50
        data.AddRange(System.BitConverter.GetBytes(adaptability)); // 51-54

        data.AddRange(System.BitConverter.GetBytes(experience)); // 55-58
        //
        var nameArray = System.Text.Encoding.Default.GetBytes(name);
        int count = nameArray.Length;
        data.AddRange(System.BitConverter.GetBytes(count)); // 59-62 | количество байтов, не длина строки
        if (count > 0) data.AddRange(nameArray);
        
        return data;
    }
    public void Load(System.IO.FileStream fs)
    {
        int LENGTH = 63;
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        ID = System.BitConverter.ToInt32(data,0);
        int shuttleID = System.BitConverter.ToInt32(data, 4);
        if (shuttleID != -1)
        {
            shuttle = Shuttle.GetShuttle(shuttleID);
            if (shuttle != null) shuttle.SetCrew(this);
        }
        else shuttle = null;
        int artifactID = System.BitConverter.ToInt32(data, 8);
        if (artifactID != -1)
        {
            artifact = Artifact.GetArtifactByID(artifactID);
            if (artifact != null) artifact.SetOwner(this);
        }
        else artifact = null;

        membersCount = System.BitConverter.ToInt32(data, 12);
        missionsSuccessed = System.BitConverter.ToInt32(data, 16);
        missionsParticipated = System.BitConverter.ToInt32(data, 20);

        status = (CrewStatus)data[24];
        persistence = data[25];
        survivalSkills = data[26];
        secretKnowledge = data[27];
        perception = data[28];
        intelligence = data[29];
        techSkills = data[30];
        level = data[31];
        exploringPath = (ExploringPath)data[32];

        stamina = System.BitConverter.ToSingle(data,33);
        proficiencies = System.BitConverter.ToUInt16(data, 37);

        confidence = System.BitConverter.ToSingle(data, 39);
        unity = System.BitConverter.ToSingle(data, 43);
        loyalty = System.BitConverter.ToSingle(data, 47);
        adaptability = System.BitConverter.ToSingle(data, 51);

        experience = System.BitConverter.ToInt32(data, 55);

        int bytesCount = System.BitConverter.ToInt32(data, 59); //выдаст количество байтов, не длину строки        
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
