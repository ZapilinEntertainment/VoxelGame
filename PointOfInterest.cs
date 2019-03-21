using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointOfInterest : MapPoint
{
    public bool explored { get; protected set; }
    public Expedition sentExpedition; // показывает последнюю отправленную
    public ExploringLocation location { get; private set; }
    private List<Mission> availableMissions;

    public PointOfInterest(int i_id) : base(i_id) { }

    public PointOfInterest(float i_angle, float i_height, byte ring, MapMarkerType mtype) : base(i_angle, i_height, mtype)
    {
        explored = false;
        availableMissions = new List<Mission>() { new Mission(MissionType.Exploring, 0) };
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
                availableMissions.Add(new Mission(mtype, subIndex, this));
            }
        }
    } 
    #endregion
}