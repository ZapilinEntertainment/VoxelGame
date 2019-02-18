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

    private const byte WRECKS_TYPE_COUNT = 10;

    public MapPoint(float i_angle, float i_height, byte ring, MapMarkerType mtype)
    {
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
        bytes.Add((byte)type);
        bytes.Add(subIndex);
        bytes.AddRange(System.BitConverter.GetBytes(angle));
        bytes.AddRange(System.BitConverter.GetBytes(height));
        return bytes;
    }

    public static List<MapPoint> LoadPoints(System.IO.FileStream fs)
    {
        var pts = new List<MapPoint>();
        int count = fs.ReadByte();
        Debug.Log(count);
        if (count > 0)
        {
            var mmtype = (MapMarkerType)fs.ReadByte();
            int subIndex = fs.ReadByte();
            var data = new byte[8];
            fs.Read(data, 0, 8);
            float angle = System.BitConverter.ToSingle(data, 0);
            float height = System.BitConverter.ToSingle(data, 4);
            GlobalMap gmap = GameMaster.realMaster.globalMap;

            switch (mmtype)
            {
                case MapMarkerType.Shuttle: break; // ignore
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
                        var poi = new PointOfInterest(angle, height, gmap.DefineRing(height), mmtype);
                        poi.Load(fs);
                        pts.Add(poi);                        
                        break;
                    }
                case MapMarkerType.MyCity:
                case MapMarkerType.Unknown:
                    pts.Add(new MapPoint(angle, height, gmap.DefineRing(height), mmtype));
                    break;
            }
        }
        return pts;
    }
    #endregion
}