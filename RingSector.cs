using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RingSector
{
    public bool destroyed { get; private set; }
    public readonly int ID;
    public readonly MapPoint centralPoint;
    public readonly Environment environment;
    public Dictionary<byte, MapPoint> points;
    public const int MAX_POINTS_COUNT = 10;
    public static int lastUsedID { get; private set; }

    public static void SetLastUsedID(int x)
    {
        x = lastUsedID;
    }
    public Vector2 GetInnerPointPosition(byte x)
    {
        Vector2 localPos = Vector2.zero;
        switch (x)
        {
            case 0: localPos = new Vector2(-0.7f, -0.7f);break;
            case 1: localPos = new Vector2(-0.7f, 0f); break;
            case 2: localPos = new Vector2(-0.7f, 0.7f); break;
            case 3: localPos = new Vector2(-0.2f, -0.7f); break;
            case 4: localPos = new Vector2(-0.2f, 0.7f); break;
            case 5: localPos = new Vector2(0.2f, -0.7f); break;
            case 6: localPos = new Vector2(0.2f, 0.7f); break;
            case 7: localPos = new Vector2(0.7f, -0.7f); break;
            case 8: localPos = new Vector2(0.7f, 0f); break;
            case 9: localPos = new Vector2(0.7f, 0.7f); break;
        }
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        int ring = centralPoint.ringIndex;
        return new Vector2(
            centralPoint.angle + gmap.sectorsDegrees[ring] * 0.5f * localPos.x,
            centralPoint.height + (gmap.ringsBorders[ring] - gmap.ringsBorders[ring + 1]) * 0.5f * localPos.y
            );
    }

    public RingSector(SunPoint sun_point, Environment.EnvironmentPreset environmentPresetType)
    {
        ID = lastUsedID;
        lastUsedID++;
        centralPoint = sun_point;
        var ls = Environment.defaultEnvironment.lightSettings;
        environment = new Environment(environmentPresetType, new Environment.LightSettings(sun_point, 1, ls.bottomColor, ls.horizonColor));
        centralPoint.SetStability(1f);
        points = new Dictionary<byte, MapPoint>();
        destroyed = false;
    }
    public RingSector(MapPoint i_point, Environment i_environment)
    {
        ID = lastUsedID;
        lastUsedID++;
        centralPoint = i_point;
        environment = i_environment;
        centralPoint.SetStability(1f);
        points = new Dictionary<byte, MapPoint>();
        destroyed = false;
    }

    public void MarkAsDestroyed()
    {
        destroyed = true;
    }

    public bool CreateNewPoint(byte positionIndex, float ascension, float visibility)
    {
        if (points.ContainsKey(positionIndex)) return false;
        else
        {
            var pos = GetInnerPointPosition(positionIndex);
            MapPoint mp;
            if (Random.value < visibility)
            {
                mp = MapPoint.CreatePointOfType(pos.x, pos.y, environment.GetSuitablePointType(ascension));
            }
            else
            {
                mp = MapPoint.CreatePointOfType(pos.x, pos.y, MapMarkerType.Unknown);
            }
            if (mp != null)
            {
                if (GameMaster.realMaster.globalMap.AddPoint(mp, false))
                {
                    points.Add(positionIndex, mp);
                    return true;
                }
                else return false;
            }
            else return false;
        }
    }
}
