using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crew {
	public const byte MIN_MEMBERS_COUNT = 3, MAX_MEMBER_COUNT = 9;
	public const int OPTIMAL_CANDIDATS_COUNT = 400;
	public static int lastNumber {get;private set;}
	public static List<Crew> crewsList{get;private set;}
	public static int crewSlots {get;private set;}

	public float salary {get; private set;}
	public int count {get;private set;}
	public string name ;
	public float experience{get; private set;}
	public float nextExperienceLimit {get;private set;}
	public byte level {get; private set;}
	public int id{get;private set;}
	public Shuttle shuttle{get;private set;}

	public float perception{get;private set;}  // тесты на нахождение и внимательность
	public float persistence{get;private set;}   // тесты на выносливость и желание продолжать поиски
	public float luck{get;private set;}  // не показывать игроку
	public float bravery{get;private set;}  
	public float techSkills{get;private set;}
	public float survivalSkills{get;private set;} // для наземных операций
	public float teamWork{get;private set;}

	public float stamina{get;private set;}  // процент готовности, падает по мере проведения операции, восстанавливается дома
	public int successfulOperations{get;private set;}
	public int totalOperations{get;private set;}

	public bool onMission{get;private set;}

	void Awake() {
		if (crewsList == null) crewsList = new List<Crew>();
	}

	public void SetCrew (ColonyController home, float hireCost) {
		level = 0;
		name = Localization.NameCrew();
		id = lastNumber;
		lastNumber ++;
		if (lastNumber >= 1000) lastNumber = 0;

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
		crewSlots --;
	}

	public void ChangeShip(Shuttle s) {
		if (s == null || (shuttle!= null & s == shuttle)) return;
		if (s.SetCrew(this) == false) return;
		shuttle = s;
		stamina -= 0.1f;
		if (stamina < 0) stamina = 0;
	}

	public static void AddCrewSlots(int x) {
		crewSlots+=x;
	}
	public static void RemoveCrewSlots(int x) {
		crewSlots-=x;
		if (crewSlots < 0) crewSlots = 0;
	}

	/// <summary>
	/// Uses for loading only
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	public static void SetLastNumber(int x) {
		lastNumber = x;
	}

	public static string[] SaveStaticData() {
		List<string> result = new List<string>();
		return result.ToArray();
	}

	public string Save() {
		string s ="";
		s += id.ToString() +',';
		s += name + ',';
		s += count.ToString() +',';
		s += level.ToString() + ',';
		s += salary.ToString() + ',';
		s += experience.ToString() + ',';
		if (shuttle != null) s += shuttle.id.ToString() + ','; else s += "-1,";

		s += perception.ToString() + ',';
		s += persistence.ToString() + ',';
		s += luck.ToString() + ',';
		s += bravery.ToString() + ',';
		s += techSkills.ToString() + ',';
		s += survivalSkills.ToString() + ',';
		s += teamWork.ToString() + ',';

		s += stamina.ToString() + ',';
		s += successfulOperations.ToString() + ',';
		s +=totalOperations.ToString() + ',';
		if (onMission) s += '1'; else s+= '0';
		return s;
	}

	public static void Reset() {
		crewsList = new List<Crew>();
		crewSlots = 0;
		lastNumber = 0;
	}

	public static void Load(string s, int count) {
		int p1 = 0, p2 = s.IndexOf(',');
		for (int i = 0; i < count; i++) {
			Crew c = new Crew();
			c.id = int.Parse (s.Substring(p1,p2 - p1)); p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.name = s.Substring(p1,p2-p1); p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.count = byte.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.level = byte.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.salary = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.experience = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.nextExperienceLimit = CalculateExperienceLimit(c.level);
			c.shuttle = Shuttle.GetVesselById(int.Parse (s.Substring(p1,p2 - p1)));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.perception = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.persistence =  float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.luck = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.bravery = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.techSkills = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.survivalSkills = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.teamWork = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.stamina = float.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.successfulOperations =int.Parse (s.Substring(p1,p2 - p1));  p1 = p2+1; p2 = s.IndexOf(',', p1);
			c.totalOperations = int.Parse (s.Substring(p1,p2 - p1)); 
			if (s[p2+1] == '1') c.onMission = true; else c.onMission = false;
			p1 = p2+3; 
			p2 = s.IndexOf(',', p1);

			crewsList.Add(c); crewSlots--;
		}
	}

	public static float GUI_DrawCrewIcon(Crew cw, Rect rr) {
		if (cw.shuttle != null) GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), cw.shuttle.condition > 0.85f ? PoolMaster.shuttle_good_icon : ( cw.shuttle.condition  < 0.5f ? PoolMaster.shuttle_bad_icon : PoolMaster.shuttle_normal_icon), ScaleMode.StretchToFill);
		GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), cw.stamina < 0.5f ? PoolMaster.crew_bad_icon : ( cw.stamina > 0.85f ? PoolMaster.crew_good_icon : PoolMaster.crew_normal_icon), ScaleMode.StretchToFill );
		return 0;
	}

	static float CalculateExperienceLimit(byte f_level) {
		return Mathf.Pow(2, f_level);
	}

	public void DismissMember() {}
	public void AddMember() {}

	public void Delete() {
		GameMaster.colonyController.AddWorkers(count);
		if (shuttle != null) {
			shuttle.SetCrew(null);
		}
		int i =0;
		while (i < crewsList.Count) {
			if (crewsList[i] == this) {
				crewsList.RemoveAt(i);
				crewSlots++;
			}
			else	i++;
		}
	}
}
