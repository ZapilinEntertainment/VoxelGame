using System.Collections.Generic;

public sealed class Mission {
	public static readonly Mission NoMission;
    public static List<Mission> missionsList;

    public bool requireShuttle { get; private set; }
    public int requiredParticipantsCount { get; private set; }
    public string codename { get; private set; }

    static Mission()
    {
        NoMission = new Mission(Localization.GetPhrase(LocalizedPhrase.NoMission));
    }

    private Mission(string name)
    {
        codename = name;
    }
}
