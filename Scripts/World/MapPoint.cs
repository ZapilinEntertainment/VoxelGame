﻿using UnityEngine;
using System.Collections.Generic;

public enum MapPointType : byte { Unknown, MyCity, Station, Wreck, FlyingExpedition, Island, SOS, Portal,
    QuestMark, Colony, Star, Wiseman, Wonder, Resources }

// ЗАВИСИМОСТИ :
// конструктор MapPoint
// MakePointOfType
//  Localization.GetMapPointLabel()
// константы максимального количества подвидов
//  функция загрузки LoadPoints()
// GlobalMap.CreateNewSector
// Environment.GetMainPointType
// PointOfInterest.TakeTreasure, GetArtifact, GenerateChallengesArray
// ScienceLab.PointExplored
//Knowledge -point check

public class MapPoint
{
    public MapPointType type { get; protected set; }
    public bool destroyed { get; protected set; }
    public byte ringIndex { get; private set; }
    public float angle // in degrees
    {
        get { return _angle; }
        set
        {
            if (value > 360f) _angle = value % 360f;
            else
            {
                if (value < 0f) _angle = 360f - Mathf.Abs(value % 360f) ;
                else _angle = value;
            }
        }
    }
    public float height {
        get { return _height; }
        set {
            if (value > 1f) _height = 1f;
            else
            {
                if (value < 0f) height = 0f;
                else _height = value;
            }
            ringIndex = GameMaster.realMaster.globalMap.DefineRing(height);
        }
    }
    private float _height, _angle;
    protected byte name, surname, specname;

    public float stability { get; protected set; }    

    public readonly int ID;
    public static int nextID { get; private set; }
    private const byte WRECKS_TYPE_COUNT = 10;

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null ||  !(obj is MapPoint))
            return false;

        MapPoint mp = (MapPoint)obj;
        return (ID == mp.ID);
    }
    public override int GetHashCode()
    {
        return ID;
    }
    public static MapPoint CreatePointOfType(float i_angle, float i_height, MapPointType mtype)
    {
        switch (mtype)
        {
            case MapPointType.MyCity:           
            case MapPointType.FlyingExpedition:
                return new MapPoint(i_angle, i_height, mtype);

            case MapPointType.Star:
                return new SunPoint(i_angle, i_height, GameMaster.realMaster.globalMap.ascension);

            case MapPointType.Unknown:
            case MapPointType.Station:
            case MapPointType.Wreck:
            case MapPointType.Island:
            case MapPointType.SOS:
            case MapPointType.Portal:
            case MapPointType.Colony:
            case MapPointType.Wiseman:
            case MapPointType.Wonder:
            case MapPointType.Resources:
                float f = Random.value;
                var p = Path.NoPath;
                if (f > 0.1f)
                {
                    if (f > 0.7f) p = Path.TechPath;
                    else
                    {
                        if (f > 0.4f) p = Path.SecretPath;
                        else p = Path.LifePath;
                    }
                }
                return new PointOfInterest(i_angle, i_height, mtype, p);

            case MapPointType.QuestMark:
                return new MapPoint(i_angle, i_height, mtype);

            default:
                return new MapPoint(i_angle, i_height, mtype);
        }
    }
    public static float Distance(MapPoint A, MapPoint B)
    {
        if (A == null || B == null) return Mathf.Infinity;
        else
        {
            return Mathf.Sqrt((A.angle - B.angle) * (A.angle - B.angle) + (A.height - B.height) * (A.height - B.height));
        }
    }

    protected MapPoint(int i_id) // для загрузки
    {
        ID = i_id;
        destroyed = false;
    }
    protected MapPoint(float i_angle, float i_height, MapPointType mtype)
    {
        ID = nextID++;
        angle = i_angle;
        height = i_height;
        ringIndex = GameMaster.realMaster.globalMap.DefineRing(height);
        type = mtype;

        switch (mtype)
        {
            case MapPointType.Unknown:
                stability = Random.value * 0.3f;
                break;
            case MapPointType.MyCity:
                stability = 1f;
                break;
            case MapPointType.Star:
                stability = 0.5f + Random.value * 0.4f;
                break;
            case MapPointType.Station:
                stability = 0.5f;
                break;
            case MapPointType.Wreck:
                stability = 0.1f + 0.23f * Random.value;
                break;
            case MapPointType.FlyingExpedition:
                stability = 1f;
                break; // flyingExpedition.expedition.sectorCollapsingTest
            case MapPointType.Island:
                stability = 0.5f + Random.value * 0.2f;
                break;
            case MapPointType.SOS:
                stability = Random.value * 0.05f;
                break;
            case MapPointType.Portal:
                stability = 0.1f + Random.value * 0.3f;
                break;
            case MapPointType.QuestMark:
                stability = 1f;
                break;
            case MapPointType.Colony:
                stability = 0.4f + Random.value * 0.2f;
                break;
            case MapPointType.Wiseman:
                stability = 0.5f + Random.value * 0.25f;
                break;
            case MapPointType.Wonder:
                stability = Random.value;
                break;
            case MapPointType.Resources:
                stability = 0.2f + Random.value * 0.3f;
                break;
        }     
        destroyed = false;
    } 

    public void SetCoords(Vector2 pos)
    {
        if (destroyed) return;
        angle = pos.x;
        height = pos.y;
    }
    public void SetStability(float s)
    {
        stability = s;
    }
    
    /// <summary>
    /// returns true if something changed
    /// </summary>
    /// <returns></returns>
    public bool Update() // INDEV
    {
        return false;
    }
    /// <summary>
    /// returns false if point shall dissappear
    /// </summary>
    /// <returns></returns>
    virtual public bool DestructionTest()
    {
        if (destroyed) return false;
        else
        {
            if (Random.value < stability) return true;
            else
            {
                switch (type)
                {
                    case MapPointType.Unknown: return (Random.value > 0.7f); // or changing!
                    case MapPointType.MyCity: return true;
                    case MapPointType.Star: // changing!
                        return true;
                    case MapPointType.Station:
                        return (Random.value < 0.5f);
                    case MapPointType.Wreck: return Random.value < 0.33f;
                    case MapPointType.FlyingExpedition: return false; // flyingExpedition.expedition.sectorCollapsingTest
                    case MapPointType.Island: return Random.value < 0.5f;
                    case MapPointType.SOS: return Random.value < 0.05f;
                    case MapPointType.Portal:
                    case MapPointType.QuestMark: return false;
                    case MapPointType.Colony: return Random.value < 0.65f;
                    case MapPointType.Wiseman: return (Random.value < 0.85f);
                    case MapPointType.Wonder: // changing
                        return false;
                    case MapPointType.Resources: // or changing!
                        return false;
                }
                return false;
            }
        }
    }
    public void MarkAsDestroyed()
    {
        destroyed = true;
    }

    virtual public string GetName()
    {
        //name, surname, specname
        return Localization.GetMapPointTitle(type);
    }
    virtual public string GetDescription()
    {
        return Localization.GetMapPointDescription(this);
    }

    #region save-load
    public virtual List<byte> Save()
    {
        //dependency : FlyingExpedition.LoadExpeditionMarker
        var bytes = new List<byte>();
        bytes.AddRange(System.BitConverter.GetBytes(ID)); // 0 - 3
        bytes.Add((byte)type); // 4 
        bytes.AddRange(System.BitConverter.GetBytes(angle)); // 5 - 8
        bytes.AddRange(System.BitConverter.GetBytes(height)); // 9 - 12
        bytes.AddRange(System.BitConverter.GetBytes(stability)); // 13 - 16
        return bytes;
    }    

    public static List<MapPoint> LoadPoints(System.IO.Stream fs)
    {
        var pts = new List<MapPoint>();
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data,0);
        if (count > 0)
        {            
            int LENGTH = 17;
            GlobalMap gmap = GameMaster.realMaster.globalMap;
            for (int i = 0; i < count; i++)
            {
                data = new byte[LENGTH];
                fs.Read(data, 0, LENGTH);
                int ID = System.BitConverter.ToInt32(data, 0);
                var mmtype = (MapPointType)data[4];
                float angle = System.BitConverter.ToSingle(data, 5);
                float height = System.BitConverter.ToSingle(data, 9);
                float stability = System.BitConverter.ToSingle(data, 13);

                switch (mmtype)
                {
                    case MapPointType.QuestMark: // awaiting
                        break;
                    case MapPointType.Station:
                    case MapPointType.Wreck:
                    case MapPointType.Island:
                    case MapPointType.SOS:
                    case MapPointType.Portal:
                    case MapPointType.Colony:
                    case MapPointType.Wiseman:
                    case MapPointType.Wonder:
                    case MapPointType.Resources:
                        {
                            var poi = new PointOfInterest(ID);
                            // base loading
                            poi.angle = angle;
                            poi.height = height;
                            poi.ringIndex = gmap.DefineRing(height);
                            poi.type = mmtype;
                            poi.stability = stability;
                            //
                            poi.Load(fs);
                            //
                            pts.Add(poi);
                            break;
                        }
                    case MapPointType.Star:
                        {
                            var sp = new SunPoint(ID);
                            // base loading
                            sp.angle = angle;
                            sp.height = height;
                            sp.ringIndex = gmap.DefineRing(height);
                            sp.type = mmtype;
                            sp.stability = stability;
                            //
                            sp.LoadSunPointData(fs);
                            //
                            pts.Add(sp);
                            break;
                        }
                    case MapPointType.MyCity:
                    case MapPointType.Unknown:
                    default:
                        {
                            var mpoint = new MapPoint(ID);
                            // base loading
                            mpoint.angle = angle;
                            mpoint.height = height;
                            mpoint.ringIndex = gmap.DefineRing(height);
                            mpoint.type = mmtype;
                            mpoint.stability = stability;
                            //
                            pts.Add(mpoint);
                            break;
                        }
                }
            }
        }
        data = new byte[4];
        fs.Read(data, 0, 4);
        nextID = System.BitConverter.ToInt32(data, 0);
        return pts;
    }
    #endregion
}

public sealed class SunPoint : MapPoint
{
    public Color color {get;private set;} // no alpha- channel

    public SunPoint(float i_angle, float i_height,  Color i_color) : base (i_angle, i_height, MapPointType.Star)
    {
        color = i_color;
    }

    public SunPoint (float i_angle, float i_height, float ascension) : base (i_angle, i_height, MapPointType.Star)
    {
        Color c = new Color((1 - ascension) * (1 - height), ascension * (1 - height), ascension * angle);
        color = Color.Lerp(Color.white, c, Mathf.Abs(0.5f - ascension) * 2 );
    }
    /// <summary>
    /// Loading constructor
    /// </summary>
    public SunPoint(int i_id) : base (i_id) { type = MapPointType.Star; }

    public override List<byte> Save()
    {
        var data =  base.Save();
        data.AddRange(System.BitConverter.GetBytes(color.r));
        data.AddRange(System.BitConverter.GetBytes(color.g));
        data.AddRange(System.BitConverter.GetBytes(color.b));
        return data;
    }
    public void LoadSunPointData(System.IO.Stream fs)
    {
        var data = new byte[12];
        fs.Read(data, 0, data.Length);
        color = new Color(
            System.BitConverter.ToSingle(data,0),
            System.BitConverter.ToSingle(data, 4),
            System.BitConverter.ToSingle(data, 8)
            );
    }
}