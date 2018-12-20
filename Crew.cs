using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrewStatus {Free, Attributed, OnLandMission}

public sealed class Crew {
	public const byte MIN_MEMBERS_COUNT = 3, MAX_MEMBER_COUNT = 9;
	public const int OPTIMAL_CANDIDATS_COUNT = 400;
    public const float LOW_STAMINA_VALUE = 0.2f, HIGH_STAMINA_VALUE = 0.85f;
	public static int lastNumber {get;private set;}
	public static List<Crew> crewsList{ get ;private set;} 
	public static int crewSlotsTotal {get;private set;}
    public static int crewSlotsFree { get; private set; }
    public static int totalOperations { get; private set; }

	public float salary {get; private set;}
	public int count {get;private set;}
	public string name ;
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
	public int successfulOperations{get;private set;}

    static Crew()
    {
        crewsList = new List<Crew>();
    }

	public static void Reset() {
		crewsList = new List<Crew>();
		crewSlotsTotal = 0;
        crewSlotsFree = crewSlotsTotal;
		lastNumber = 0;
	}

    public void SetCrew (ColonyController home, float hireCost) {
		level = 0;
		name = Localization.NameCrew();
		ID = lastNumber;	lastNumber ++;

		salary = ((int)((hireCost / 12f) * 100)) / 100f;
		perception = 1; // сделать зависимость от исследованных технологий
		persistence =  home.happiness_coefficient * 0.85f + 0.15f;
		luck = Random.value;
		bravery = 0.3f * Random.value + persistence * 0.3f + 0.4f * home.health_coefficient;
		techSkills = 0.5f * home.hq.level / 8f + 0.5f; // сделать зависимость от количества общих видов построенных зданий
		survivalSkills = persistence * 0.15f + luck * 0.05f + techSkills * 0.15f + bravery * 0.15f + 0.5f; // еще пара зависимостей от зданий
		teamWork = 0.75f * home.happiness_coefficient + 0.25f * Random.value;

		stamina = 0.9f + Random.value * 0.1f;
		count = (int)( MIN_MEMBERS_COUNT + (Random.value * 0.3f + 0.7f * (float)home.freeWorkers / (float)OPTIMAL_CANDIDATS_COUNT) * (MAX_MEMBER_COUNT - MIN_MEMBERS_COUNT) );
		if (count > MAX_MEMBER_COUNT) count = MAX_MEMBER_COUNT;
		crewSlotsFree--;
	}

	public bool AssignToShip(Shuttle s) {
		if (s == null || (shuttle != null & s == shuttle)) return false;
		if (s.AttributeCrew(this) == false) return false;
		shuttle = s;
		stamina -= 0.1f;
		if (stamina < 0) stamina = 0;
        return true;
	}

	public static void AddCrewSlots(int x) {
		crewSlotsTotal += x;
        crewSlotsFree += x;
	}
	public static void RemoveCrewSlots(int x) {
		crewSlotsTotal -= x;
		if (crewSlotsTotal < 0) crewSlotsTotal = 0;
        crewSlotsFree -= x;
        if (crewSlotsFree < 0) crewSlotsFree = 0;
	}

	static float CalculateExperienceLimit(byte f_level) {
		return 2 * f_level;
	}
    public void DrawCrewIcon(UnityEngine.UI.RawImage ri)
    {
        ri.texture = UIController.current.iconsTexture;
        ri.uvRect = UIController.GetTextureUV((stamina < 0.5f) ? Icons.CrewBadIcon : ((stamina > 0.85f) ? Icons.CrewGoodIcon : Icons.CrewNormalIcon));
    }

    public void DismissMember() {}
	public void AddMember() {}

	public void Dismiss() {
        GameMaster.realMaster.colonyController.AddWorkers(count);
        count = 0;
        if (status != CrewStatus.Free)
        {
            if (shuttle != null)
            {
                Shuttle s = shuttle;
                shuttle = null;
                s.UnattributeCrew(this);
            }
        }
        crewsList.Remove(this);
        crewSlotsFree++;
    }
    public void Disappear()
    {
        crewsList.Remove(this);            
        crewSlotsFree++;
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
        css.lastNumber = lastNumber;
        return css;
    }
    public static void LoadStaticData(CrewStaticSerializer css)
    {
        if (crewsList == null) crewsList = new List<Crew>();
        if (css.haveCrews)
        {
            for (int i = 0; i < css.crewsList.Count; i++)
            {
                crewsList.Add(new Crew().Load(css.crewsList[i]));
            }
        }
        crewSlotsFree -= crewsList.Count;
        lastNumber = css.lastNumber;
    }

    public CrewSerializer Save()
    {
        CrewSerializer cs = new CrewSerializer();
        cs.salary = salary;
        cs.count = count;
        cs.experience = experience;
        cs.nextExperienceLimit = nextExperienceLimit;
        cs.name = name;
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
        cs.successfulOperations = successfulOperations;
        cs.totalOperations = totalOperations;
        return cs;
    }

    public Crew Load(CrewSerializer cs)
    {
        salary = cs.salary;
        count = cs.count;
        level = cs.level;
        nextExperienceLimit = cs.nextExperienceLimit;
        experience = cs.experience;
        name = cs.name;
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
        successfulOperations = cs.successfulOperations;
        totalOperations = cs.totalOperations;
        if (cs.shuttleID != -1)
        {
            shuttle = Shuttle.GetShuttle(cs.shuttleID);
            shuttle.AttributeCrew(this);
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
