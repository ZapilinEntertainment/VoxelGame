using System.Collections.Generic;
using UnityEngine;

public enum MissionType : byte
{
    Awaiting, Exploring, FindingKnowledge, FindingItem, FindingPerson, FindingPlace, FindingResources, FindingEntrance, FindingExit
}
// Dependencies:
//MissionPreview
// Point of interest
// конструктор
//  TestYourMight()
//GetDistanceToTarget
// Result

//тестер для проходения проверок командой
public class Mission {    
	public static readonly Mission NoMission;
    public static int nextID { get; protected set; }
    private static List<Mission> missions;

    public string name
    {
        get
        {
            return Localization.GetMissionStandartName(type);
        }
    }
    public MissionType type { get; protected set; }
    public PointOfInterest point { get; protected set; }
    public readonly int stepsCount, ID;
    public readonly bool requireShuttle;

    public static bool operator ==(Mission lhs, Mission rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(Mission lhs, Mission rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Mission p = (Mission)obj;
        return (type == p.type);
    }

    static Mission()
    {
        NoMission = new Mission(MissionType.Awaiting);
    }
    public static void SetNextIDValue(int x)
    {
        nextID = x;
    }
    public static Mission GetMissionByID(int s_id)
    {
        if (s_id > 0 && missions != null && missions.Count > 0)
        {
            foreach (Mission m in missions)
            {
                if (m.ID == s_id) return m;
            }
            return NoMission;
        }
        else return NoMission;
    }
    public static void RemoveMission(int d_id)
    {
        if (d_id > 0 & missions != null && missions.Count > 0)
        {
            for (int i = 0; i< missions.Count; i++)
            {
                if (missions[i].ID == d_id)
                {
                    missions.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public Mission(MissionType i_type)
    {
        type = i_type;
        point = null;
        requireShuttle = false;
        stepsCount = 1; // awaiting
        ID = nextID++;
        if (missions == null) missions = new List<Mission>();
        missions.Add(this);
    }
    public Mission (MissionType i_type, PointOfInterest i_point) : this(i_type)
    {
        requireShuttle = true;
        point = i_point;
        switch (type)
        {
            case MissionType.Exploring: stepsCount = 10; break;
            case MissionType.FindingKnowledge: stepsCount = 3 + Random.Range(0,9);break;
            case MissionType.FindingItem: stepsCount = 2 + Random.Range(0, 4);break;
            case MissionType.FindingPerson: stepsCount = 4 + Random.Range(0,3);break;
            case MissionType.FindingPlace: stepsCount = 2 + Random.Range(0, 3);break;
            case MissionType.FindingResources: stepsCount = 3;break;
                //остальные - по единице
        }
        ID = nextID++;
        if (missions == null) missions = new List<Mission>();
        missions.Add(this);
    }
    /// <summary>
    /// loading constructor
    /// </summary>
    public Mission (int i_ID, int i_stepsCount,  bool i_requireShuttle)
    {
        ID = i_ID;
        stepsCount = i_stepsCount;              
        requireShuttle = i_requireShuttle;
        missions.Add(this);
    }

    public float CalculateCrewSpeed(Crew c)
    {
        if (c == null) return 0;
        else
        {
            // вообще должно зависеть от самой миссии
            return c.teamWork * c.unity + c.persistence * c.confidence + 0.1f * c.loyalty + c.adaptability;
        }
    }
    public float GetDistanceToTarget()
    {
        switch (type)
        {
            case MissionType.Awaiting: return 0;
            case MissionType.Exploring: return stepsCount + 1;
            case MissionType.FindingKnowledge: return stepsCount * (0.5f + Random.value);
            case MissionType.FindingItem: return stepsCount * (0.3f + Random.value * 0.5f);
            case MissionType.FindingPerson: return stepsCount * (0.6f + Random.value);
            case MissionType.FindingPlace: return stepsCount * (0.4f + Random.value * 0.3f);
            case MissionType.FindingResources: return stepsCount / 2f + Random.value * 2f;
            case MissionType.FindingEntrance: return stepsCount * (0.25f + Random.value * 0.3f);
            case MissionType.FindingExit: return stepsCount * (0.35f + Random.value * 0.2f);
            default: return stepsCount / 2f;
        }
    }
    public bool TestYourMight(Crew c)
    {
        switch (type)
        {
            // зависимость от точки?
            case MissionType.Exploring:
                return c.perception * 0.7f + 0.2f * c.persistence + 0.1f * c.luck > 0.3f * Random.value + 0.2f;
            case MissionType.FindingKnowledge:
                return c.perception * 0.6f + 0.1f *c.persistence + 0.3f * c.luck > 0.5f * Random.value + 0.3f ;
            case MissionType.FindingItem:
                return c.perception * 0.5f * c.persistence + 0.1f * c.luck + 0.4f > 0.5f * Random.value + 0.2f;
            case MissionType.FindingPerson:
                return c.perception * 0.7f + c.luck * 0.1f + c.teamWork * 0.2f > 0.5f * Random.value + 0.4f;
            case MissionType.FindingPlace:
                return c.persistence * 0.5f + c.perception * 0.15f + c.teamWork * 0.3f + c.luck * 0.05f > 0.7f * Random.value + 0.3f; 
            case MissionType.FindingResources:
                return c.perception * 0.2f + c.persistence * 0.3f + c.luck * 0.1f + 0.2f * c.teamWork + 0.2f * c.techSkills > 0.2f + 0.3f * Random.value;
            case MissionType.FindingEntrance:
                return c.perception * 0.3f + c.luck * 0.1f + c.teamWork * c.unity * 0.6f > 0.5f + Random.value * 0.5f;
            case MissionType.FindingExit:
                return c.perception * 0.4f * c.adaptability + c.luck * 0.4f + c.teamWork * 0.2f > 0.5f + 0.5f * Random.value;
            default: return true;
        }
    }
    public bool TryToLeave() // INDEV
    {
        //может и не получиться
        return true;
    }

    /// <summary>
    /// returns true if mission should be ended
    /// </summary>
    /// <returns></returns>
    public bool Result(Expedition e)
    {
        switch (type)
        {
            case MissionType.Awaiting: return false;
            case MissionType.Exploring: e.crew.IncreaseAdaptability(); return false;
            case MissionType.FindingKnowledge: e.crew.ImproveNativeParameters(); return false;
            case MissionType.FindingItem: 
            case MissionType.FindingPerson: 
            case MissionType.FindingPlace: e.crew.AddExperience(Expedition.ONE_STEP_XP); return true;
            case MissionType.FindingResources: point.TakeTreasure(e.crew); return false;
            case MissionType.FindingEntrance: e.crew.AddExperience(Expedition.ONE_STEP_XP); return false;
            case MissionType.FindingExit: e.crew.AddExperience(Expedition.ONE_STEP_XP); return true;
            default: return false;
        }
    }

    #region save-load
    public List<byte> Save() 
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(ID));
        data.AddRange(System.BitConverter.GetBytes(stepsCount));
        data.Add(requireShuttle ? (byte)1 : (byte)0);
        data.Add((byte)type);
        return data;
    }
    public static void StaticLoad(System.IO.FileStream fs, int count, PointOfInterest i_point)
    {
        missions = new List<Mission>();
        byte[] data;
        Mission m;
        for (int i = 0; i < count; i++)
        {
            data = new byte[10];
            fs.Read(data, 0, 9);
            m = new Mission(
                System.BitConverter.ToInt32(data, 0), // id
                System.BitConverter.ToInt32(data, 4), //stepsCount
                data[8] == 1
                );
            m.type = (MissionType)data[9];
            m.point = i_point;
        }
        data = new byte[4];
        fs.Read(data, 0, 4);
        nextID = System.BitConverter.ToInt32(data, 0);
    }
    #endregion
}
