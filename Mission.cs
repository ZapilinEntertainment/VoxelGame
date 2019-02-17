using System.Collections.Generic;

public enum MissionType : byte
{
    Awaiting, Exploring, FindingKnowledge, FindingItem, FindingPerson, FindingPlace, FindingResources,
    FindingEntrance, FindingExit
}

public struct Mission {    

	public static readonly Mission NoMission;

    public MissionType type;
    public byte subIndex;
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
        return (type == p.type) & (subIndex == p.subIndex);
    }

    static Mission()
    {
        NoMission = new Mission(MissionType.Awaiting, 0);
    }

    public Mission(MissionType i_type, byte i_subIndex)
    {
        type = i_type;
        subIndex = i_subIndex;
        point = null;
        requireShuttle = false;
        stepsCount = 1; // awaiting
        codename = Localization.GetMissionCodename(type, subIndex);
    }
    public Mission (MissionType i_type, byte i_subIndex, PointOfInterest i_point) : this(i_type, i_subIndex)
    {
        requireShuttle = true;
        point = i_point;
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
    public bool TryToLeave()
    {
        //может и не получиться
        return true;
    }

    #region save-load
    public List<byte> Save()
    {
        var bytes = new List<byte>();
        bytes.Add((byte)type);
        bytes.Add(subIndex);
        return bytes;
    }
    #endregion
}
