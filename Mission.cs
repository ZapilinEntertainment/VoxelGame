using System.Collections.Generic;

public sealed class Mission {
	public static readonly Mission NoMission;
    public static List<Mission> missionsList;
    public static int nextID = 0;

    public bool requireShuttle { get; private set; }
    public readonly int ID;
    public int requiredParticipantsCount { get; private set; }
    public int progressPoints { get; private set; }
    public string codename { get; private set; }
    //сохранение использованных айди?


    static Mission()
    {
        NoMission = new Mission(Localization.GetPhrase(LocalizedPhrase.NoMission), 0);
        nextID = 1;
        var lifesourceFind = new Mission("finding lifesources", 1, true);
        missionsList = new List<Mission>();
        missionsList.Add(lifesourceFind);
    }

    public static Mission GetMission(int s_id)
    {
        if (s_id < missionsList.Count)
        {
            foreach (Mission m in missionsList)
            {
                if (m.ID == s_id) return m;
            }
        }
        return NoMission;
    }

    private Mission(string name, int i_id)
    {
        codename = name;
        ID = i_id;
    }
    private Mission(string name, int participantsCount, bool i_requireShuttle)
    {
        codename = name;
        requiredParticipantsCount = participantsCount;
        requireShuttle = i_requireShuttle;
        ID = nextID++;
    }
}
