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
    public bool destroyed { get; protected set; }
    public bool stable { get; protected set; }
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
        destroyed = false;
    }

    public MapPoint(float i_angle, float i_height, MapMarkerType mtype)
    {
        ID = lastUsedID++;
        angle = i_angle;
        height = i_height;
        ringIndex = GameMaster.realMaster.globalMap.DefineRing(height);
        type = mtype;
        stable = false;
        destroyed = false;
        switch (type)
        {
            case MapMarkerType.Wreck: subIndex = (byte)(Random.Range(0, WRECKS_TYPE_COUNT)); break;
            default: subIndex = 0; break;
        }
    }

    public void ChangeCoords(float i_angle, float i_height)
    {
        if (destroyed) return;
        angle = i_angle;
        height = i_height;
        ringIndex = GameMaster.realMaster.globalMap.DefineRing(height);
    }
    public void SetStability(bool x)
    {
        if (destroyed) return;
        stable = x;
    }

    /// <summary>
    /// returns true if something changed
    /// </summary>
    /// <returns></returns>
    public bool Update() // INDEV
    {
        if (destroyed) return false;
        bool destroyPoint = true;
        switch (type)
        {
            case MapMarkerType.Unknown:
                break;
            case MapMarkerType.MyCity:
                break;
            case MapMarkerType.Star:
                break;
            case MapMarkerType.Station:
                break;
            case MapMarkerType.Wreck:
                break;
            case MapMarkerType.Shuttle:
                break;
            case MapMarkerType.Island:
                break;
            case MapMarkerType.SOS:
                break;
            case MapMarkerType.Portal:
                break;
            case MapMarkerType.QuestMark:
                break;
            case MapMarkerType.Colony:
                break;
            case MapMarkerType.Wiseman:
                break;
            case MapMarkerType.Wonder:
                break;
            case MapMarkerType.Resources:
                break;
        }
        if (destroyPoint)
        {
            destroyed = true;
            GameMaster.realMaster.globalMap.RemovePoint(this, true);
        }
        return false;
    }
    /// <summary>
    /// returns true if point shall dissappear
    /// </summary>
    /// <returns></returns>
    virtual public bool DestructionTest()
    {
        if (destroyed) return true;
        else
        {
            if (stable) return false;
            else
            {
                switch (type)
                {
                    case MapMarkerType.Unknown: return (Random.value < 0.7f); // or changing!
                    case MapMarkerType.MyCity: return false;
                    case MapMarkerType.Star: // changing!

                        break;
                    case MapMarkerType.Station:
                        return (Random.value > 0.5f);
                    case MapMarkerType.Wreck: return Random.value > 0.33f;
                    case MapMarkerType.Shuttle: return true; // flyingExpedition.expedition.sectorCollapsingTest
                    case MapMarkerType.Island: return Random.value > 0.5f;
                    case MapMarkerType.SOS: return Random.value > 0.05f;
                    case MapMarkerType.Portal:
                    case MapMarkerType.QuestMark: return true;
                    case MapMarkerType.Colony: return Random.value > 0.65f;
                    case MapMarkerType.Wiseman: return (Random.value > 0.85f);
                    case MapMarkerType.Wonder: // changing
                        return true;
                    case MapMarkerType.Resources: // or changing!
                        return true;
                }
                return true;
            }
        }
    }
    public void MarkAsDestroyed()
    {
        destroyed = true;
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
        if (stable) bytes.Add((byte)1); else bytes.Add((byte)0); // 14
        return bytes;
    }

    public static List<MapPoint> LoadPoints(System.IO.FileStream fs)
    {
        var pts = new List<MapPoint>();
        int count = fs.ReadByte();
        //Debug.Log(count);
        if (count > 0)
        {            
            int LENGTH = 15;
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
                bool stable = (data[14] == 1);
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
                            poi.stable = stable;
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
                            mpoint.stable = stable;
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

public sealed class SunPoint : MapPoint
{
    public Color color {get;private set;}

    public SunPoint(float i_angle, float i_height,  Color i_color) : base (i_angle, i_height, MapMarkerType.Star)
    {
        color = i_color;
    }

    public SunPoint (float i_angle, float i_height, float ascension) : base (i_angle, i_height, MapMarkerType.Star)
    {
        Color c = new Color((1 - ascension) * (1 - height), ascension * (1 - height), ascension * angle);
        color = Color.Lerp(Color.white, c, Mathf.Abs(0.5f - ascension) * 2 );
    }
}