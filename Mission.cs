using System.Collections.Generic;

public sealed class Mission {
	public static readonly Mission NoMission;

    public readonly bool requireShuttle;
    public readonly int ID;
    public int progressPoints { get; private set; }
    public string codename { get; private set; }
    //сохранение использованных айди?
    public const int UNDEFINED_ID = 0, EXPLORE_MISSION_ID = 1;

    static Mission()
    {
        NoMission = new Mission(Localization.GetPhrase(LocalizedPhrase.NoMission));
    }

    private Mission()
    {
        requireShuttle = false;
        ID = UNDEFINED_ID;
        progressPoints = 0;
        codename = string.Empty;
    }

    private Mission(string i_codename) : this()
    {
        codename = i_codename;        
    }

    public Mission(int i_id, bool i_requireShuttle)
    {
        ID = i_id;
        requireShuttle = i_requireShuttle;
        codename = Localization.GetMissionCodename(ID);
    }
}
