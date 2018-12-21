using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrewStatus {Free, Attributed, OnLandMission}

public sealed class Crew : MonoBehaviour {
	public const byte MIN_MEMBERS_COUNT = 3, MAX_MEMBER_COUNT = 9;
	public const int OPTIMAL_CANDIDATS_COUNT = 400;
    public const float LOW_STAMINA_VALUE = 0.2f, HIGH_STAMINA_VALUE = 0.85f;

	public static int lastFreeID {get;private set;}	
    public static int actionsHash { get; private set; }
    public static List<Crew> crewsList { get; private set; }
    private static GameObject crewsContainer;

	public int count {get;private set;}
	public float experience{get; private set;}
	public float nextExperienceLimit {get;private set;}
	public byte level {get; private set;}
	public int ID{get;private set;}
	public Shuttle shuttle{get;private set;}
	public CrewStatus status;

	public float perception{get;private set;}  // тесты на нахождение и внимательность
	public float persistence{get;private set;}   // тесты на выносливость и желание продолжать поиски
	public float luck{get;private set;}  // не показывать игроку
	public float bravery{get;private set;}  
	public float techSkills{get;private set;}
	public float survivalSkills{get;private set;} // для наземных операций
	public float teamWork{get;private set;}

	public float stamina{get;private set;}  // процент готовности, падает по мере проведения операции, восстанавливается дома
    public int missionsCompleted { get; private set; }
    public int successfulMissions{get;private set;}    

    static Crew()
    {
        crewsList = new List<Crew>();
    }

	public static void Reset() {
		crewsList = new List<Crew>();
		lastFreeID = 0;
        actionsHash = 0;
	}

    public static Crew CreateNewCrew(ColonyController home)
    {
        if (crewsList.Count >= RecruitingCenter.GetCrewsSlotsCount()) return null;
        Crew c = new GameObject(Localization.NameCrew()).AddComponent<Crew>();
        if (crewsContainer == null) crewsContainer = new GameObject("crewsContainer");
        c.transform.parent = crewsContainer.transform;

        c.level = 0;
        c.ID = lastFreeID; lastFreeID++;
        c.status = CrewStatus.Free;
        c.perception = 1; // сделать зависимость от исследованных технологий
        c.persistence = home.happiness_coefficient * 0.85f + 0.15f;
        c.luck = Random.value;
        c.bravery = 0.3f * Random.value + c.persistence * 0.3f + 0.4f * home.health_coefficient;
        c.techSkills = 0.5f * home.hq.level / 8f + 0.5f; // сделать зависимость от количества общих видов построенных зданий
        c.survivalSkills = c.persistence * 0.15f + c.luck * 0.05f + c.techSkills * 0.15f + c.bravery * 0.15f + 0.5f; // еще пара зависимостей от зданий
        c.teamWork = 0.75f * home.happiness_coefficient + 0.25f * Random.value;

        c.stamina = 0.9f + Random.value * 0.1f;
        c.count = (int)(MIN_MEMBERS_COUNT + (Random.value * 0.3f + 0.7f * (float)home.freeWorkers / (float)OPTIMAL_CANDIDATS_COUNT) * (MAX_MEMBER_COUNT - MIN_MEMBERS_COUNT));
        if (c.count > MAX_MEMBER_COUNT) c.count = MAX_MEMBER_COUNT;
        crewsList.Add(c);        
        actionsHash++;
        return c;
    }

	public void SetShuttle(Shuttle s) {
        if (s == shuttle | (s.crew != null & s.crew != this)) return;
        if (shuttle != null && shuttle.crew == this) shuttle.SetCrew(null);
        shuttle = s;
        shuttle.SetCrew(this);
        if (shuttle != null) status = CrewStatus.Attributed;
        actionsHash++;
	}

	static float CalculateExperienceLimit(byte f_level) {
		return 2 * f_level;
	}
    public void DrawCrewIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.current.iconsTexture;
        ri.uvRect = UIController.GetTextureUV((stamina < 0.5f) ? Icons.CrewBadIcon : ((stamina > 0.85f) ? Icons.CrewGoodIcon : Icons.CrewNormalIcon));
    }

    public void DismissMember() {
        actionsHash++;
    }
	public void AddMember() {
        actionsHash++;
    }

	public void Dismiss() {
        GameMaster.realMaster.colonyController.AddWorkers(count);
        count = 0;
        if (status != CrewStatus.Free)
        {
            if (shuttle != null && shuttle.crew == this) shuttle.SetCrew(null);
        }
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

    #region save-load system
    public static CrewStaticSerializer SaveStaticData()
    {
        CrewStaticSerializer css = new CrewStaticSerializer();
        css.haveCrews = false; css.crewsList = new List<CrewSerializer>();
        if (crewsList != null && crewsList.Count > 0)
        {
            int i = 0;
            while (i < crewsList.Count)
            {
                Crew c = crewsList[i];
                if (c == null)
                {
                    crewsList.RemoveAt(i);
                    continue;
                }
                else
                {
                    if (c.status == CrewStatus.Free) css.crewsList.Add(c.Save());
                }
                i++;
            }
            if (css.crewsList.Count > 0) css.haveCrews = true;
        }
        css.lastNumber = lastFreeID;
        return css;
    }
    public static void LoadStaticData(CrewStaticSerializer css)
    {
        if (crewsList == null) crewsList = new List<Crew>();
        if (css.haveCrews)
        {
            if (crewsContainer == null) crewsContainer = new GameObject("crews container");
            for (int i = 0; i < css.crewsList.Count; i++)
            {
                Crew c = new GameObject(css.crewsList[i].name).AddComponent<Crew>();
                c.transform.parent = crewsContainer.transform;
                c.Load(css.crewsList[i]);
                crewsList.Add(c);
            }
        }
        lastFreeID = css.lastNumber;
    }

    public CrewSerializer Save()
    {
        CrewSerializer cs = new CrewSerializer();
        cs.count = count;
        cs.experience = experience;
        cs.nextExperienceLimit = nextExperienceLimit;
        cs.name = gameObject.name;
        cs.level = level;
        cs.ID = ID;
        cs.shuttleID = (shuttle == null ? -1 : shuttle.ID);
        cs.status = status;

        cs.perception = perception;
        cs.persistence = persistence;
        cs.luck = luck;
        cs.bravery = bravery;
        cs.techSkills = techSkills;
        cs.survivalSkills = survivalSkills;
        cs.teamWork = teamWork;
        cs.stamina = stamina;
        cs.successfulOperations = successfulMissions;
        cs.totalOperations = missionsCompleted;
        return cs;
    }

    public Crew Load(CrewSerializer cs)
    {
        count = cs.count;
        level = cs.level;
        nextExperienceLimit = cs.nextExperienceLimit;
        experience = cs.experience;
        ID = cs.ID;
        status = cs.status;
        perception = cs.perception;
        persistence = cs.persistence;
        luck = cs.luck;
        bravery = cs.bravery;
        techSkills = cs.techSkills;
        survivalSkills = cs.survivalSkills;
        teamWork = cs.teamWork;
        stamina = cs.stamina;
        successfulMissions = cs.successfulOperations;
        missionsCompleted = cs.totalOperations;
        if (cs.shuttleID != -1)
        {
            shuttle = Shuttle.GetShuttle(cs.shuttleID);
            shuttle.SetCrew(this);
        }
        return this;
    }

    #endregion
}

[System.Serializable]
public class CrewSerializer {
	public float salary,  experience, nextExperienceLimit;
	public string name ;
	public byte level ;
	public int ID, shuttleID,count;
	public CrewStatus status;

	public float perception, persistence, luck,bravery, techSkills,survivalSkills,teamWork,stamina;
	public int successfulOperations, totalOperations;
}

[System.Serializable]
public class CrewStaticSerializer {
	public bool haveCrews;
	public List<CrewSerializer> crewsList;
    public int lastNumber;
}
