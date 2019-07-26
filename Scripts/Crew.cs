using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrewStatus : byte {AtHome, OnMission, Travelling}
// dependencies
//Localization.GetCrewStatus()
// Rest()

public sealed class Crew : MonoBehaviour {
	public const byte MIN_MEMBERS_COUNT = 3, MAX_MEMBER_COUNT = 9;
    public const int CREW_INFO_STRINGS_COUNT = 14;
    public const float LOW_STAMINA_VALUE = 0.2f, HIGH_STAMINA_VALUE = 0.85f, CHANGING_SHUTTLE_STAMINA_CONSUMPTION = 0.1f;
    private const float NEUROPARAMETER_STEP = 0.01f, NATIVE_CHAR_IMPROVEMENT_STEP = 0.02f, STAMINA_REFRESH_SPEED = 0.02f;

	public static int nextID {get;private set;}	
    public static int actionsHash { get; private set; }
    public static List<Crew> crewsList { get; private set; }
    private static GameObject crewsContainer;
    public static UICrewObserver crewObserver { get; private set; }

	public int membersCount {get;private set;}
	public float experience{get; private set;}
	public float nextExperienceLimit {get;private set;}
	public byte level {get; private set;}
	public int ID{get;private set;}
    public Artifact artifact { get; private set; }
    public Expedition currentExpedition { get; private set; }
	public Shuttle shuttle{get;private set;}
	public CrewStatus status { get; private set; }

	public float perception { get; private set; }// тесты на нахождение и внимательность
    public float persistence { get; private set; }
	public float luck { get; private set; }
	public float bravery { get; private set; }
	public float techSkills { get; private set; }
	public float survivalSkills { get; private set; }
	public float teamWork { get; private set; }
    //при внесении изменений отредактировать Localization.GetCrewInfo

        //neuroparameters:
    public float confidence { get; private set; }
    public float unity { get; private set; }
    public float loyalty { get; private set; }
    public float adaptability { get; private set; }
    public float stamina { get; private set; } // процент готовности, падает по мере проведения операции, восстанавливается дома

public int missionsParticipated { get; private set; }
    public int missionsSuccessed{get;private set;}    

    static Crew()
    {
        crewsList = new List<Crew>();
        GameMaster.realMaster.lifepowerUpdateEvent += CrewsUpdateEvent;
    }
    public static void CrewsUpdateEvent()
    {
        if (crewsList != null && crewsList.Count > 0)
        {
            foreach (Crew c in crewsList)
            {
                if (c.status != CrewStatus.OnMission)
                {
                    c.Rest(null);
                }
            }
        }
    }

	public static void Reset() {
		crewsList = new List<Crew>();
		nextID = 0;
        actionsHash = 0;
	}

    public static Crew CreateNewCrew(ColonyController home, float recruitsFullfill)
    {
        if (crewsList.Count >= RecruitingCenter.GetCrewsSlotsCount()) return null;
        Crew c = new GameObject(Localization.NameCrew()).AddComponent<Crew>();
        if (crewsContainer == null) crewsContainer = new GameObject("crewsContainer");
        c.transform.parent = crewsContainer.transform;

        c.level = 0;
        c.ID = nextID; nextID++;
        c.status = CrewStatus.AtHome;

        //normal parameters
        c.perception =  0.3f + Random.value * 0.5f;
        c.persistence = 0.3f + Random.value * 0.5f;
        c.luck = Random.value;
        c.bravery = 0.3f + Random.value * 0.6f;
        float lvl_cf = home.hq.level / GameConstants.HQ_MAX_LEVEL;
        c.techSkills = 0.1f * Random.value + lvl_cf * 0.45f + home.gears_coefficient / GameConstants.GEARS_UP_LIMIT * 0.45f; // сделать зависимость от количества общих видов построенных зданий
        float hcf = home.hospitals_coefficient;
        if (hcf > 1) hcf = 1;
        c.survivalSkills = 0.3f * Random.value + 0.3f * home.health_coefficient + 0.4f * hcf ; // еще пара зависимостей от зданий
        c.teamWork = 0.7f * home.happiness_coefficient + 0.1f * Random.value;
        //neuroparameters:
        c.confidence = home.happiness_coefficient;
        c.unity = 0.5f + (c.teamWork - 0.5f) * 0.75f;
        c.loyalty = home.happiness_coefficient;
        c.adaptability = lvl_cf * 0.5f;

        c.stamina = 0.9f + Random.value * 0.1f;
        c.membersCount = (int)(MAX_MEMBER_COUNT * (recruitsFullfill * 0.5f + 0.2f * Random.value + 0.3f * home.health_coefficient));
        if (c.membersCount > MAX_MEMBER_COUNT) c.membersCount = MAX_MEMBER_COUNT;
        crewsList.Add(c);        
        actionsHash++;
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
            stamina -= CHANGING_SHUTTLE_STAMINA_CONSUMPTION;
            actionsHash++;
        }               
	}
    public void SetStatus(CrewStatus cs)
    {
        status = cs;
    }
    public void Rename(string s)
    {
        name = s;
    }
    /// <summary>
    /// for loading only
    /// </summary>
    public void SetCurrentExpedition(Expedition e)
    {
        currentExpedition = e;
    }

    public void AddExperience(float f)
    {
        experience += f * (0.75f +  0.5f * adaptability);
    }
	static float CalculateExperienceLimit(byte f_level) {
        return f_level < 3 ? 2 * f_level : f_level * f_level;
	}
    public void DrawCrewIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.current.iconsTexture;
        ri.uvRect = UIController.GetTextureUV((stamina < 0.5f) ? Icons.CrewBadIcon : ((stamina > 0.85f) ? Icons.CrewGoodIcon : Icons.CrewNormalIcon));
    }

    public bool HardTest(float hardness) // INDEV
    {
        if (artifact != null)
        {
            bool? protection = artifact.StabilityTest(hardness);
            if (protection == null) DropArtifact();
            else
            {
                if (protection == true)
                {
                    confidence += NEUROPARAMETER_STEP;
                    return true;
                }
            }
        }
        //нет артефакта или защита не сработала
        bool success = true;
        if (success)
        {
            unity += NEUROPARAMETER_STEP;
            adaptability += NEUROPARAMETER_STEP;
            return true;
        }
        else return false;
    }

    /// <summary>
    /// returns true if successfully completed
    /// </summary>
    /// <param name="friendliness"></param>
    /// <returns></returns>
    public bool SoftCheck( float friendliness) // INDEV
    {
        bool success = true;
        if (success)
        {
            unity += NEUROPARAMETER_STEP;
            adaptability += NEUROPARAMETER_STEP;
        }
        return success;
    }
    public bool ConsumeStamina(float f)
    {
        stamina -= f;
        if (stamina <= 0)
        {
            stamina = 0;
            return false;
        }
        else return true;
    }
    public void Rest(PointOfInterest place)
    {
        if (place != null)
        {
            stamina += 0.5f * (0.4f + place.danger * 0.3f + place.difficulty * 0.3f + place.friendliness * 0.2f);
            unity += NEUROPARAMETER_STEP;
        }
        else
        {
            if (status == CrewStatus.AtHome)
            {
                stamina += STAMINA_REFRESH_SPEED;
                adaptability -= NEUROPARAMETER_STEP / 4f;
            }
            else
            {
                stamina += STAMINA_REFRESH_SPEED / 2f;
                adaptability -= NEUROPARAMETER_STEP / 8f;
            }
        }
    }

    /// <summary>
    /// improves persistence, perception, bravery or teamwork
    /// </summary>
    public void ImproveNativeParameters()
    {
        float f = Random.value;
        if (f < 0.5f)
        {
            if (f < 0.25f)
            {
                persistence += NATIVE_CHAR_IMPROVEMENT_STEP;
                confidence += NEUROPARAMETER_STEP;
            }
            else
            {
                perception += NATIVE_CHAR_IMPROVEMENT_STEP;
                adaptability += NEUROPARAMETER_STEP;
            }
        }
        else
        {
            if (f > 0.75f)
            {
                bravery += NATIVE_CHAR_IMPROVEMENT_STEP;

            }
        }
    }

    public void DismissMember() { // INDEV
        // перерасчет характеристик
        if (membersCount < MAX_MEMBER_COUNT / 2)
        {
            unity += NEUROPARAMETER_STEP;
            confidence -= NEUROPARAMETER_STEP;
        }
    }
    public void LoseMember() // INDEV
    {
        membersCount--;        
        if (membersCount <= 0) Disappear();
        else
        {
            // перерасчет характеристик
        }
        confidence -= 4 * NEUROPARAMETER_STEP;
        unity += NEUROPARAMETER_STEP;
    }
	public void AddMember() { // INDEX
        membersCount++;
        // перерасчет характеристик
        if (membersCount >= 5) unity -= NEUROPARAMETER_STEP / 2f;
        else unity -= NEUROPARAMETER_STEP;
    }

    public void LoseConfidence() { confidence -= NEUROPARAMETER_STEP * (1 - unity) * (1 - loyalty);  }
    public void IncreaseConfidence() { confidence += NEUROPARAMETER_STEP * (1 + unity / 2f);  }
    public void LoseLoyalty() { loyalty -= NEUROPARAMETER_STEP * (1 - unity) * (1 - confidence); }
    public void IncreaseLoyalty() { loyalty += NEUROPARAMETER_STEP * (1 - adaptability); }
    public void IncreaseAdaptability() { adaptability += NEUROPARAMETER_STEP; }

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
            loyalty += NEUROPARAMETER_STEP;
            confidence += NEUROPARAMETER_STEP;
        }
    }
    public void DropArtifact()
    {
        if (artifact != null)
        {
            artifact.SetOwner(null);
            artifact = null;
            confidence -= NEUROPARAMETER_STEP;
            unity -= NEUROPARAMETER_STEP;
            actionsHash++;
        }
    }
    //system
    public void ClearArtifactField(Artifact a)
    {
        if (artifact == a) artifact = null;
    }

	public void Dismiss() {
        GameMaster.realMaster.colonyController.AddWorkers(membersCount);
        membersCount = 0;
        if (shuttle != null && shuttle.crew == this) shuttle.SetCrew(null);
        crewsList.Remove(this);
        actionsHash++;
        Destroy(this);
    }
    public void Disappear()
    {
        crewsList.Remove(this);
        actionsHash++;
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (crewsList.Count == 0 & crewObserver != null) Destroy(crewObserver);
    }

    #region save-load system
    public static void SaveStaticData( System.IO.FileStream fs)
    {
        var data = new List<byte>();
        int crewsCount = crewsList.Count;
        if (crewsCount > 0)
        {
            crewsCount = 0;
            while (crewsCount < crewsList.Count)
            {
                Crew c = crewsList[crewsCount];
                if (c == null)
                {
                    crewsList.RemoveAt(crewsCount);
                    continue;
                }
                else
                {
                    data.AddRange(c.Save());
                    crewsCount++;
                };
            }
        }
        fs.Write(System.BitConverter.GetBytes(crewsCount), 0, 4);
        if (crewsCount > 0)
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

        while (crewsCount >0)
        {
            Crew c = new GameObject().AddComponent<Crew>();
            c.transform.parent = crewsContainer.transform;
            c.Load(fs);
            crewsList.Add(c);
            crewsCount--;
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

        var nameArray = System.Text.Encoding.Default.GetBytes(name);
        int count = nameArray.Length;
        // 12 - 15
        data.AddRange(System.BitConverter.GetBytes(count)); // количество байтов, не длина строки
        if (count > 0) data.AddRange(nameArray); 

        //0
        data.AddRange(System.BitConverter.GetBytes(membersCount)); 
        data.AddRange(System.BitConverter.GetBytes(experience)); 
        data.AddRange(System.BitConverter.GetBytes(nextExperienceLimit));  
        data.AddRange(System.BitConverter.GetBytes(perception)); 
        data.AddRange(System.BitConverter.GetBytes(persistence)); 
        data.AddRange(System.BitConverter.GetBytes(luck)); 
        data.AddRange(System.BitConverter.GetBytes(bravery));
        data.AddRange(System.BitConverter.GetBytes(techSkills));
        data.AddRange(System.BitConverter.GetBytes(survivalSkills));
        data.AddRange(System.BitConverter.GetBytes(teamWork));
        data.AddRange(System.BitConverter.GetBytes(stamina));
        data.AddRange(System.BitConverter.GetBytes(missionsSuccessed));
        data.AddRange(System.BitConverter.GetBytes(missionsParticipated));
        // 52
        data.Add(level); // 53
        data.Add((byte)status); // 54
        return data;
    }

    public void Load(System.IO.FileStream fs)
    {
        int LENGTH = 16;
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        ID = System.BitConverter.ToInt32(data,0);
        int shuttleID = System.BitConverter.ToInt32(data, 4);
        if (shuttleID != -1)
        {
            shuttle = Shuttle.GetShuttle(shuttleID);
            shuttle.SetCrew(this);
        }
        else shuttle = null;
        int artifactID = System.BitConverter.ToInt32(data, 8);
        if (artifactID != -1)
        {
            artifact = Artifact.GetArtifactByID(artifactID);
            artifact.SetOwner(this);
        }
        else artifact = null;

        int bytesCount = System.BitConverter.ToInt32(data, 12); //выдаст количество байтов, не длину строки        
        if (bytesCount > 0)
        {
            data = new byte[bytesCount];
            fs.Read(data, 0, bytesCount);
            System.Text.Decoder d = System.Text.Encoding.Default.GetDecoder();
            var chars = new char[d.GetCharCount(data, 0, bytesCount)];
            d.GetChars(data, 0, bytesCount, chars, 0, true);
            name = new string(chars);
        }

        data = new byte[54];
        fs.Read(data, 0, data.Length);
        membersCount = System.BitConverter.ToInt32(data, 0);
        experience = System.BitConverter.ToSingle(data, 4);
        nextExperienceLimit = System.BitConverter.ToSingle(data, 8);
        perception = System.BitConverter.ToSingle(data, 12);
        persistence = System.BitConverter.ToSingle(data, 16);
        luck = System.BitConverter.ToSingle(data, 20);
        bravery = System.BitConverter.ToSingle(data, 24);
        techSkills = System.BitConverter.ToSingle(data, 28);
        survivalSkills = System.BitConverter.ToSingle(data, 32);
        teamWork = System.BitConverter.ToSingle(data, 36);
        stamina = System.BitConverter.ToSingle(data, 40);
        missionsSuccessed = System.BitConverter.ToInt32(data, 44);
        missionsParticipated = System.BitConverter.ToInt32(data, 48);
        level = data[52];
        status = (CrewStatus)data[53];
    }

    #endregion
}
