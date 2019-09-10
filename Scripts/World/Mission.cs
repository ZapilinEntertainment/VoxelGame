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

//класс-тестировщик на проходение проверок командой
public sealed class Mission {    
	public static readonly Mission NoMission;
    private static int nextID;
    private static List<Mission> missions;

    public MissionType type { get; private set; }
    public readonly int stepsCount, ID;
    public readonly bool requireShuttle;

    private byte nameIdentifierA = 0, nameIdentifierB = 0;

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
    public override int GetHashCode()
    {
        return ID + stepsCount * 6 + nameIdentifierA * 2 + nameIdentifierB * 2;
    }

    static Mission()
    {
        NoMission = new Mission(MissionType.Awaiting);
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
        requireShuttle = false;
        stepsCount = 1; // awaiting
        ID = nextID++;
        if (missions == null) missions = new List<Mission>();
        missions.Add(this);
    }
    public Mission (MissionType i_type, PointOfInterest i_point) : this(i_type)
    {
        requireShuttle = true;
        switch (type)
        {
            case MissionType.Exploring: stepsCount = 5 + (int)(i_point.difficulty * 6f); break;
            case MissionType.FindingKnowledge: stepsCount = 4 + (int)(Random.Range(0,6) * i_point.difficulty);break;
            case MissionType.FindingItem: stepsCount = 2 + (int)(Random.Range(0, 4) * i_point.difficulty); break;
            case MissionType.FindingPerson: stepsCount = 4 + (int)(Random.Range(0, 5) * i_point.difficulty); break;
            case MissionType.FindingPlace: stepsCount = 2 + (int)(Random.Range(0, 3) * i_point.difficulty); break;
            case MissionType.FindingResources: stepsCount = 3 + (int)(Random.Range(0, 2) * i_point.difficulty); break;
            default: stepsCount = 1 + (int)i_point.difficulty;break;
                //остальные - по единице
        }
        ID = nextID++;
        nameIdentifierA = 0;
        nameIdentifierB = nameIdentifierA;
        if (missions == null) missions = new List<Mission>();
        missions.Add(this);
    }
    /// <summary>
    /// loading constructor
    /// </summary>
    public Mission (int i_ID, int i_stepsCount, bool i_requireShuttle)
    {
        ID = i_ID;
        stepsCount = i_stepsCount;              
        requireShuttle = i_requireShuttle;
        missions.Add(this);
        nameIdentifierA = 0;
        nameIdentifierB = nameIdentifierA;
    }
    public string GetName()
    {
        return Localization.GetMissionName(type, nameIdentifierA, nameIdentifierB);
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

    #region save-load
    public static void StaticSave(System.IO.FileStream fs) 
    {
        int count = 0;
        List<byte> savedata = null;
        if (missions != null && missions.Count > 0)
        {
            foreach (Mission m in missions)
            {
                savedata.AddRange(System.BitConverter.GetBytes(m.ID)); // 0 -3
                savedata.AddRange(System.BitConverter.GetBytes(m.stepsCount)); // 4 - 7
                savedata.Add(m.requireShuttle ? (byte)1 : (byte)0); // 8
                savedata.Add((byte)m.type); // 9
                savedata.Add(m.nameIdentifierA);// 10
                savedata.Add(m.nameIdentifierB);// 11

                count++;
            }
        }
        fs.Write(System.BitConverter.GetBytes(count),0,4);
        if (count > 0) {
            var saveArray = savedata.ToArray();
            fs.Write(saveArray, 0, saveArray.Length);
        }
    }
    public static void StaticLoad(System.IO.FileStream fs)
    {
        if (missions != null) missions.Clear();
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data, 0);
        if (count > 0)
        {
            Mission m;
            for (int i = 0; i < count; i++)
            {
                data = new byte[12];
                fs.Read(data, 0, data.Length);
                m = new Mission(
                    System.BitConverter.ToInt32(data, 0), // id
                    System.BitConverter.ToInt32(data, 4), //stepsCount
                    data[8] == 1
                    );
                m.type = (MissionType)data[9];
                m.nameIdentifierA = data[10];
                m.nameIdentifierB = data[11];
            }
            data = new byte[4];
            fs.Read(data, 0, 4);
            nextID = System.BitConverter.ToInt32(data, 0);
        }
    }
    #endregion
}
