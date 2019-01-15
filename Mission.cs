using System.Collections.Generic;

public sealed class Mission {
	public static readonly Mission NoMission, FindingParadise;
    public static List<Mission> missionsList;

    public bool requireShuttle { get; private set; }
    public int requiredParticipantsCount { get; private set; }
    public string codename { get; private set; }

    static Mission()
    {
        NoMission = new Mission(Localization.GetPhrase(LocalizedPhrase.NoMission));
        FindingParadise = new Mission("finding Paradise", 1, true);
        missionsList = new List<Mission>();
        missionsList.Add(FindingParadise);
    }

    private Mission(string name)
    {
        codename = name;
    }
    private Mission(string name, int participantsCount, bool i_requireShuttle)
    {
        codename = name;
        requiredParticipantsCount = participantsCount;
        requireShuttle = i_requireShuttle;
    }
}
