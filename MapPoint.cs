using UnityEngine;
using System.Collections.Generic;

public enum MapMarkerType : byte { Unknown, MyCity, Station, Wreck, Shuttle, Island, SOS, Portal, QuestMark, Colony, Star, Wiseman, Wonder, Resources }
//при изменении порядка или количества - изменить маску необязательных объектов GlobalMap.TEMPORARY_POINTS_MASK
// изменить Localization.GetMapPointLabel()
// константы максимального количества подвидов
// изменить функцию загрузки LoadPoints(0

public class MapPoint
{
    public MapMarkerType type { get; protected set; }
    public byte subIndex { get; protected set; }
    public byte ringIndex;    
    public float angle;
    public float height;
    public readonly int ID;
    public static int lastUsedID { get; private set; }

    private const byte WRECKS_TYPE_COUNT = 10;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        MapPoint mp = (MapPoint)obj;
        return (ID == mp.ID);
    }

    public MapPoint(int i_id) // для загрузки
    {
        ID = i_id;
    }

    public MapPoint(float i_angle, float i_height, byte ring, MapMarkerType mtype)
    {
        ID = lastUsedID++;
        angle = i_angle;
        height = i_height;
        ringIndex = ring;
        type = mtype;
        switch (type)
        {
            case MapMarkerType.Wreck: subIndex = (byte)(Random.value * WRECKS_TYPE_COUNT); break;
            default: subIndex = 0; break;
        }
    }

    public virtual bool DestroyRequest()
    {
        return true;
    }

    #region save-load
    public virtual List<byte> Save()
    {
        var bytes = new List<byte>();
        bytes.AddRange(System.BitConverter.GetBytes(ID)); // 0 - 3
        byte codedType = (byte)type;
        bytes.Add(codedType); // 4 
        bytes.Add(subIndex);// 5
        bytes.AddRange(System.BitConverter.GetBytes(angle)); // 6 - 9
        bytes.AddRange(System.BitConverter.GetBytes(height)); // 10 - 13
        return bytes;
    }

    public static List<MapPoint> LoadPoints(System.IO.FileStream fs)
    {
        var pts = new List<MapPoint>();
        int count = fs.ReadByte();
        //Debug.Log(count);
        if (count > 0)
        {            
            int LENGTH = 14;
            for (int i = 0; i < count; i++)
            {
                var data = new byte[LENGTH];
                fs.Read(data, 0, LENGTH);
                int ID = System.BitConverter.ToInt32(data, 0);
                var mmtype = (MapMarkerType)data[4];
                //Debug.Log(data[4]);
                int subIndex = data[5];
                float angle = System.BitConverter.ToSingle(data, 6);
                float height = System.BitConverter.ToSingle(data, 10);
                GlobalMap gmap = GameMaster.realMaster.globalMap;

                switch (mmtype)
                {
                    case MapMarkerType.QuestMark: // awaiting
                        break;
                    case MapMarkerType.Station:
                    case MapMarkerType.Wreck:
                    case MapMarkerType.Island:
                    case MapMarkerType.SOS:
                    case MapMarkerType.Portal:
                    case MapMarkerType.Colony:
                    case MapMarkerType.Star:
                    case MapMarkerType.Wiseman:
                    case MapMarkerType.Wonder:
                    case MapMarkerType.Resources:
                        {
                            var poi = new PointOfInterest(ID);
                            poi.angle = angle;
                            poi.height = height;
                            poi.ringIndex = gmap.DefineRing(height);
                            poi.type = mmtype;
                            poi.Load(fs);
                            pts.Add(poi);
                            break;
                        }
                    case MapMarkerType.MyCity:
                    case MapMarkerType.Unknown:
                    default:
                        {
                            var mpoint = new MapPoint(ID);
                            mpoint.angle = angle;
                            mpoint.height = height;
                            mpoint.ringIndex = gmap.DefineRing(height);
                            mpoint.type = mmtype;
                            pts.Add(mpoint);
                            break;
                        }
                }
            }
        }
        var idata = new byte[4];
        fs.Read(idata, 0, 4);
        lastUsedID = System.BitConverter.ToInt32(idata, 0);
        return pts;
    }
    #endregion
}