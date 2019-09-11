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
    private static int nextID;
    private static List<Mission> missions;

    public readonly MissionPreset preset;
    public Expedition performer { get; private set; }
    public readonly int stepsCount, ID;   
    private byte nameIdentifierA = 0, nameIdentifierB = 0;

    public static bool operator ==(Mission lhs, Mission rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(Mission lhs, Mission rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Mission p = (Mission)obj;
        return (ID == p.ID);
    }
    public override int GetHashCode()
    {
        return ID + stepsCount * 6 + nameIdentifierA * 2 + nameIdentifierB * 2;
    }

    public static Mission GetMissionByID(int s_id)
    {
        if (s_id > 0 && missions != null && missions.Count > 0)
        {
            foreach (Mission m in missions)
            {
                if (m.ID == s_id) return m;
            }
            return null;
        }
        else return null;
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

    public Mission(MissionPreset mp)
    {
        preset = mp;
        stepsCount = 1; // awaiting
        ID = nextID++;
        if (missions == null) missions = new List<Mission>();
        missions.Add(this);
    }
    public Mission (MissionPreset i_mp, PointOfInterest i_point) : this(i_mp)
    {
        switch (i_mp.type)
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
        nameIdentifierA = 0;
        nameIdentifierB = nameIdentifierA;
    }
    /// <summary>
    /// loading constructor
    /// </summary>
    public Mission (MissionPreset i_mp, int i_ID, int i_stepsCount)
    {
        preset = i_mp;
        ID = i_ID;
        stepsCount = i_stepsCount;                  
        nameIdentifierA = 0;
        nameIdentifierB = nameIdentifierA;
        if (missions == null) missions = new List<Mission>();
        missions.Add(this);
    }
    public string GetName()
    {
        if (!preset.isUnique) return Localization.GetMissionName(preset.type, nameIdentifierA, nameIdentifierB);
        else return "<unique mission>";
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
        switch (preset.type)
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
        switch (preset.type)
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
            savedata = new List<byte>();
            foreach (Mission m in missions)
            {
                savedata.AddRange(m.preset.Save());
                savedata.AddRange(System.BitConverter.GetBytes(m.ID)); // 0 -3
                savedata.AddRange(System.BitConverter.GetBytes(m.stepsCount)); // 4 - 7
                savedata.Add(m.nameIdentifierA);// 8
                savedata.Add(m.nameIdentifierB);//9               
                count++;
            }
        }
        fs.Write(System.BitConverter.GetBytes(count),0,4);
        if (count > 0) {
            var saveArray = savedata.ToArray();
            fs.Write(saveArray, 0, saveArray.Length);
        }
        fs.Write(System.BitConverter.GetBytes(nextID),0,4);
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
                var mp = MissionPreset.Load(fs);
                data = new byte[10];
                fs.Read(data, 0, data.Length);
                m = new Mission(
                    mp, // preset
                    System.BitConverter.ToInt32(data, 0), // id
                    System.BitConverter.ToInt32(data, 4) //stepsCount
                    );
                m.nameIdentifierA = data[8];
                m.nameIdentifierB = data[9];
            }            
        }
        data = new byte[4];
        fs.Read(data, 0, 4);
        nextID = System.BitConverter.ToInt32(data, 0);
    }
    #endregion
}

public struct MissionPreset
{
    public readonly MissionType type;
    public readonly bool requireShuttle, isUnique;
    public readonly int subIndex;

    public static readonly MissionPreset ExploringPreset;

    static MissionPreset()
    {
        ExploringPreset = new MissionPreset(MissionType.Exploring, true, false, 0);
    }
    public MissionPreset(MissionType i_type, bool i_reqShuttle, bool i_unique, int i_subIndex)
    {
        type = i_type;
        requireShuttle = i_reqShuttle;
        isUnique = i_unique;
        subIndex = i_subIndex;
    }

    public List<byte> Save()
    {
        const byte trueByte = 1, falseByte = 0;
        var data = new List<byte>
        {
            (byte)type,
            requireShuttle ? trueByte : falseByte,
            isUnique ? trueByte : falseByte
        };
        data.AddRange(System.BitConverter.GetBytes(subIndex));
        return data;
    }
    public static MissionPreset Load(System.IO.FileStream fs) 
    {
        var data = new byte[7];
        fs.Read(data, 0, data.Length);
        int i_subIndex = System.BitConverter.ToInt32(data, 3);
        return new MissionPreset((MissionType)data[0], data[1] == 1, data[2] == 1, i_subIndex);
    }
}
