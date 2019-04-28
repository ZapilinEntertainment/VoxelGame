using System.Collections.Generic;
using UnityEngine;

public enum MissionType : byte
{
    Awaiting, Exploring, FindingKnowledge, FindingItem, FindingPerson, FindingPlace, FindingResources, FindingEntrance, FindingExit
}
// Dependencies:
// конструктор
//  TestYourMight()
//GetDistanceToTarget
// Result

//структура для проходения проверок командой
public struct Mission {   
	public static readonly Mission NoMission;

    public MissionType type;
    public bool requireShuttle;    
    public PointOfInterest point;
    public string codename;    
    public int stepsCount;

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

    public Mission(MissionType i_type)
    {
        type = i_type;
        point = null;
        requireShuttle = false;
        stepsCount = 1; // awaiting
        codename = Localization.GetMissionCodename(type);
    }
    public Mission (MissionType i_type,PointOfInterest i_point) : this(i_type)
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
        var bytes = new List<byte>();
        return bytes;
        // зависимости:
        // Expedition.Load()
        // PointOfInterest.Load()
    }
    #endregion
}
