using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointOfInterest : MapPoint
{
    public bool explored { get; protected set; }
    public Expedition sentExpedition; // показывает последнюю отправленную
    public ExploringLocation location { get; private set; }

    private float exploredPart = 0f;
    private List<Mission> availableMissions;

    public float richness { get; protected set; }
    public float danger { get; protected set; }
    public float mysteria { get; protected set; }
    public float friendliness { get; protected set; }

    public PointOfInterest(int i_id) : base(i_id) { }

    public PointOfInterest(float i_angle, float i_height, MapMarkerType mtype) : base(i_angle, i_height, mtype)
    {
        explored = false;
        availableMissions = new List<Mission>() { new Mission(MissionType.Exploring) };
        switch (mtype)
        {
            case MapMarkerType.Unknown:
                richness = Random.value;
                danger = Random.value;
                mysteria = 1f;
                friendliness = Random.value;
                break;
            case MapMarkerType.MyCity:
                richness = 0.05f;
                danger = 0f;
                mysteria = 0.01f;
                friendliness = 1f; // зависимость от environmental condition 
                break;
            case MapMarkerType.Star:
                richness = 0.1f;
                danger = 1f;
                mysteria = 0.1f + Random.value * 0.2f;
                friendliness = Random.value;
                break;
            case MapMarkerType.Station:
                richness = Random.value * 0.8f + 0.2f;
                danger = Random.value * 0.2f;
                mysteria = Random.value * 0.5f;
                friendliness = Random.value;
                break;
            case MapMarkerType.Wreck:
                richness = 0.3f + Random.value * 0.5f;
                danger = 0.3f + Random.value * 0.2f;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.8f;
                break;
            case MapMarkerType.Shuttle:
                richness = 0f;
                danger = 0f;
                mysteria = 0f;
                friendliness = 1f;
                break; // flyingExpedition.expedition.sectorCollapsingTest
            case MapMarkerType.Island:
                richness = 0.2f + Random.value * 0.7f;
                danger = Random.value * 0.5f;
                mysteria = 0.2f + Random.value * 0.5f;
                friendliness = Random.value * 0.7f + 0.3f;
                break;
            case MapMarkerType.SOS:
                richness = 0f;
                danger = 0.2f + 0.8f * Random.value;
                mysteria = Random.value * 0.3f;
                friendliness = Random.value * 0.5f + 0.5f;
                break;
            case MapMarkerType.Portal:
                richness = 0.1f;
                danger = 0.3f + Random.value;
                mysteria = 0.3f + Random.value * 0.7f;
                friendliness = Random.value;
                break;
            case MapMarkerType.QuestMark:
                richness = 0f; // зависит от квеста
                danger = Random.value; //
                mysteria = 0.1f; //
                friendliness = Random.value; //
                break;
            case MapMarkerType.Colony:
                richness = 0.5f + Random.value * 0.5f;
                danger = 0.1f * Random.value;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.7f + 0.3f;
                break;
            case MapMarkerType.Wiseman:
                richness = 0.1f;
                danger = 0.1f * Random.value;
                mysteria = Random.value;
                friendliness = Random.value * 0.3f + 0.7f;
                break;
            case MapMarkerType.Wonder:
                richness = 0.3f + Random.value * 0.7f;
                danger = 0.1f * Random.value;
                mysteria = 0.5f + Random.value * 0.5f;
                friendliness = Random.value;
                break;
            case MapMarkerType.Resources:
                richness = 0.5f + Random.value * 0.5f;
                danger = Random.value * 0.3f;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.6f;
                break;
        }
    }
    
    public List<Dropdown.OptionData> GetAvailableMissionsDropdownData()
    {
        var l = new List<Dropdown.OptionData>();
        foreach (Mission m in availableMissions)
        {
            l.Add(new Dropdown.OptionData(m.codename));
        }
        return l;
    }
    public Mission GetMission(int index)
    {
        if (index < 0 | index >= availableMissions.Count) return Mission.NoMission;
        else return availableMissions[index];
    }
    public void Explore(float k)
    {
        exploredPart += 0.01f * k;
        if (exploredPart >= 1f)
        {
            exploredPart = 1f;
            explored = true;
        }
    }

    #region save-load
    override public List<byte> Save()
    {
        var bytes = base.Save();
        byte zero = 0, one = 1;
        if (location == null) bytes.Add(zero);
        else
        {
            bytes.Add(one);
            bytes.AddRange(location.Save());
        }

        byte count = (byte)availableMissions.Count;
        bytes.Add(count);
        if (count > 0)
        {
            foreach (Mission m in availableMissions)
            {
                bytes.AddRange(m.Save());
            }
        }
        return bytes;
    }
    public void Load(System.IO.FileStream fs)
    {
        int x = fs.ReadByte();
        if (x == 1)
        {
            location = ExploringLocation.Load(fs);
        }

        x = fs.ReadByte(); // missionsCount
        if (availableMissions == null) availableMissions = new List<Mission>();
        else availableMissions.Clear();
        if (x > 0)
        {
            for (int i = 0; i < x; i++)
            {
                var mtype = (MissionType)fs.ReadByte();
                byte subIndex = (byte)fs.ReadByte();
                availableMissions.Add(new Mission(mtype,this));
            }
        }
    } 
    #endregion
}