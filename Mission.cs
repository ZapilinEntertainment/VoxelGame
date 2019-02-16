using System.Collections.Generic;

public enum MissionType : byte
{
    Awaiting, Exploring, FindingKnowledge, FindingItem, FindingPerson, FindingPlace, FindingResources,
    FindingEntrance, FindingExit
}

public sealed class Mission {    

	public static readonly Mission NoMission;

    public readonly bool requireShuttle;
    public readonly MissionType type;
    public readonly PointOfInterest point;
    public int currentStep { get; private set; }
    public int totalSteps { get; private set; }
    public string codename { get; private set; }
    private byte subIndex;
    //сохранение использованных айди?

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
    public bool NextStep()
    {
        currentStep++;
        return (currentStep == totalSteps);
    }
    public bool TryToLeave()
    {
        //может и не получиться
        return true;
    }
}
