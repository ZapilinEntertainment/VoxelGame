using UnityEngine;
using System.Collections.Generic;

public struct Environment
{
    public enum EnvironmentPreset : byte // dependency : ringSector.Save
    {
        Default, WhiteSpace, BlackSpace, IceSpace, FireSpace, WaterSpace, LightStorm,
        DarkCanyon, EndlessFields, UnderrealmCaverns, OceanWorld, DiedWorld, ForestWorld, DesertWorld,
        DreamRealm, AncientRuins, ModernRuins, SoulRuins, Custom
    }
    // # зависимость:
    // GetSuitableEnvironment
    // GetSuitablePoint
    // LightSettings.GetPresetLightSettings
    public struct LightSettings
    {
        public Vector3 sunDirection;
        public float maxIntensity;
        public Color sunColor, bottomColor, horizonColor;

        public LightSettings(Vector3 sunDir, float i_maxIntensity, Color i_sunColor, Color i_bottomColor, Color i_horizonColor)
        {
            sunDirection = sunDir;
            maxIntensity = i_maxIntensity;
            sunColor = i_sunColor;
            bottomColor = i_bottomColor;
            horizonColor = i_horizonColor;
        }

        public static LightSettings GetPresetLightSettings(EnvironmentPreset ep)
        {
            Vector2 v = Random.insideUnitCircle;
            switch (ep)
            {
                case EnvironmentPreset.DarkCanyon:
                    {
                        v *= 5f;
                        return new LightSettings(
                            new Vector3(v.y, -1f, v.x), 0.75f,
                            new Color(0.88f, 0.19f, 0.67f), new Color(0.6f, 0f, 0.68f), new Color(0.28f, 0.07f, 0.31f)
                            );
                    }
                case EnvironmentPreset.EndlessFields:
                    {
                        v *= 2f;
                        return new LightSettings(
                           new Vector3(v.x, -1f, v.y), 1.3f,
                           new Color(1f, 0.94f, 0.71f), new Color(0.88f, 0.82f, 0.54f), new Color(2f, 0.84f, 0f)
                           );
                    }
                case EnvironmentPreset.UnderrealmCaverns:
                    {
                        v *= 5f;
                        return new LightSettings(
                           new Vector3(v.x, -0.1f, v.y), 0.8f,
                           new Color(0.58f, 1f, 0.75f), new Color(0f, 0.65f, 0.4f), new Color(0.22f, 0.46f, 0.29f)
                           );
                    }
                case EnvironmentPreset.OceanWorld:
                    {
                        v *= 3f;
                        return new LightSettings(
                           new Vector3(v.x, -1f, v.y), 1.1f,
                           new Color(0.61f, 0.99f, 0.94f), new Color(0f, 0.62f, 1f), new Color(0f, 0f, 0.65f)
                           );
                    }
                case EnvironmentPreset.DiedWorld:
                    {
                        return new LightSettings(
                           Vector3.down, 0.75f,
                           Color.white, Color.white * 0.74f, new Color(1f, 0.61f, 0.57f)
                           );
                    }
                case EnvironmentPreset.ForestWorld:
                    {
                        v *= 5f;
                        return new LightSettings(
                           new Vector3(v.x, -1f, v.y), 0.75f,
                           new Color(1f, 0.96f, 0.74f), new Color(0.57f, 0.84f, 0.57f), new Color(0.07f, 0.43f, 0.07f)
                           );
                    }
                case EnvironmentPreset.DesertWorld:
                    {
                        v *= 3f;
                        return new LightSettings(
                           new Vector3(v.x, -0.5f, v.y), 1.2f,
                           new Color(1f, 0.74f, 0.65f), new Color(1f, 0.95f, 0.74f), new Color(0.51f, 0.48f, 0.09f)
                           );
                    }
                case EnvironmentPreset.DreamRealm:
                    {
                        v *= 2;
                        return new LightSettings(
                       new Vector3(v.x, -1f, v.y), 1f,
                       new Color(1f, 0.67f, 0.9f), new Color(0.67f, 1f, 0.97f), new Color(0.61f, 0.19f, 0.52f)
                       );
                    }
                case EnvironmentPreset.AncientRuins:
                    {
                        v *= 1.5f;
                        return new LightSettings(
                       new Vector3(v.x, -0.5f, v.y), 1f,
                       new Color(0.6f, 0.6f, 0.2f), new Color(0.82f, 0.83f, 0.17f), new Color(0.25f, 0.25f, 0.15f)
                       );
                    }
                case EnvironmentPreset.ModernRuins:
                    {
                        v *= 2f;
                        return new LightSettings(
                       new Vector3(v.x, -0.9f + Random.value * 0.2f, v.y), 1.1f,
                       new Color(0.71f, 0.98f, 1f), new Color(0.92f, 0.94f, 0.6f), new Color(0.43f, 0.54f, 0.54f)
                       );
                    }
                case EnvironmentPreset.SoulRuins:
                    {
                        return new LightSettings(
                       new Vector3(v.x, -1f, v.y), 1f,
                       new Color(0.27f, 0.83f, 0.85f), new Color(0.73f, 0.83f, 0.83f), new Color(0.04f, 0.91f, 0.95f)
                       );
                    }
                case EnvironmentPreset.LightStorm:
                    return new LightSettings(new Vector3(2f, -0.5f, 2f), 1.2f, new Color(1f, 0.99f, 0.86f), new Color(0.58f, 0.58f, 0.52f), new Color(0.96f, 0.79f, 0.4f));
                case EnvironmentPreset.WaterSpace:
                    return new LightSettings(Vector3.down, 0.5f, new Color(0.18f, 0.65f, 1f), new Color(0f, 0f, 0.4f), new Color(0.06f, 0.75f, 0.98f));
                case EnvironmentPreset.FireSpace:
                    return new LightSettings(Vector3.down, 1.5f, new Color(0.93f, 0.26f, 0.11f), new Color(0.31f, 0.07f, 0.05f), Color.yellow);
                case EnvironmentPreset.IceSpace:
                    return new LightSettings(new Vector3(1f, -1f, 1f), 0.85f, new Color(0.588f, 0.853f, 0.952f), Color.blue, Color.white);
                case EnvironmentPreset.BlackSpace:
                    return new LightSettings(new Vector3(1f, -0.3f, 1f), 0.85f, Color.white, Color.black, Color.black);
                case EnvironmentPreset.WhiteSpace:
                    return new LightSettings(Vector3.down, 1.3f, Color.white, Color.white, Color.white);
                case EnvironmentPreset.Default:
                default:
                    return new LightSettings(new Vector3(1f, -0.3f, 1f).normalized, 1f,
            Color.white, Color.white, Color.cyan * 0.5f);
            }
        }
    }

    public enum SkySphereType : byte
    {
        Stars, Clouds, Ice, Fire, OceanSurface, LightStorm, RainySky, RealSky, Hangar, Cavern
    }
    public enum BeltType : byte
    {
        NoBelt, Light, CavernExit, Caverns, Ruins, Mechanisms, StoneWalls
    }
    public enum LowSphereType : byte
    {
        Stars, Clouds, Ice, Fire, Ocean, GlowingClouds, Abyss, Canyons, Fields, Forest, Desert, Ruins
    }
    public enum DecorationsPack : byte
    {
        NoDecorations, PeaksUp, PeaksDown, CyclopeBuildings, AncientRuins, ModernRuins, Mechanisms, Trees, Gardens
    }
    public enum Weather : byte
    {
        NoWeather, Rain, Snow, Rays, Ashes, Foliage, Pollen
    }

    public static readonly Environment defaultEnvironment;

    public readonly EnvironmentPreset presetType;
    public readonly float conditions; // 0 is hell, 1 is very favourable
    public LightSettings lightSettings;

    static Environment()
    {
        defaultEnvironment = new Environment(EnvironmentPreset.Default);
    }   
    public static Environment GetSuitableEnvironment(float ascension)
    {
        int x;
        EnvironmentPreset presetType = EnvironmentPreset.Default;
        if (ascension < 0.3f) // low
        {
            x = Random.Range(0, 6); 
            switch (x)
            {
                case 0:
                    {
                        presetType = EnvironmentPreset.BlackSpace;
                        break;
                    }
                case 1:
                    {
                        presetType = EnvironmentPreset.IceSpace;
                        break;
                    }
                case 2:
                    {
                        presetType = EnvironmentPreset.FireSpace;
                        break;
                    }
                case 3:
                    {
                        presetType = EnvironmentPreset.DarkCanyon;
                        break;
                    }
                case 4:
                    {
                        presetType = EnvironmentPreset.DiedWorld;
                        break;
                    }
                case 5:
                    {
                        presetType = EnvironmentPreset.AncientRuins;
                        break;
                    }
            }
        }
        else
        {
            if (ascension > 0.7f) // high
            {
                x = Random.Range(0, 5);
                switch (x)
                {
                    case 0:
                        {
                            presetType = EnvironmentPreset.WhiteSpace;
                            break;
                        }
                    case 1:
                        {
                            presetType = EnvironmentPreset.EndlessFields;
                            break;
                        }
                    case 2:
                        {
                            presetType = EnvironmentPreset.DesertWorld;
                            break;
                        }
                    case 3:
                        {
                            presetType = EnvironmentPreset.DreamRealm;
                            break;
                        }
                    case 4:
                        {
                            presetType = EnvironmentPreset.SoulRuins;
                            break;
                        }
                }
            }
            else // normal
            {
                x = Random.Range(0, 7);
                switch (x)
                {
                    case 0:
                        {
                            presetType = EnvironmentPreset.Default;
                            break;
                        }
                    case 1:
                        {
                            presetType = EnvironmentPreset.WaterSpace;
                            break;
                        }
                    case 2:
                        {
                            presetType = EnvironmentPreset.LightStorm;
                            break;
                        }
                    case 3:
                        {
                            presetType = EnvironmentPreset.UnderrealmCaverns;
                            break;
                        }
                    case 4:
                        {
                            presetType = EnvironmentPreset.OceanWorld;
                            break;
                        }
                    case 5:
                        {
                            presetType = EnvironmentPreset.ForestWorld;
                            break;
                        }
                    case 6:
                        {
                            presetType = EnvironmentPreset.ModernRuins;
                            break;
                        }
                }
            }
        }
        return new Environment(presetType, LightSettings.GetPresetLightSettings(presetType));
    }  

    public Environment(EnvironmentPreset ep)
    {
        switch (ep)
        {
            case EnvironmentPreset.Default:
            default:
                presetType = EnvironmentPreset.Default;
                lightSettings = LightSettings.GetPresetLightSettings(ep);
                conditions = 1;
                break;
        }
    }
    public Environment(EnvironmentPreset ep, LightSettings ls)
    {
        switch (ep)
        {
            case EnvironmentPreset.Default:
            default:
                presetType = EnvironmentPreset.Default;
                lightSettings = ls;
                conditions = 1;
                break;
        }
    }

    public float GetInnerEventTime()
    {
        if (presetType != EnvironmentPreset.Default)
        {
            return 20f;
        }
        else return 0;
    }
    public void InnerEvent()
    {
        switch (presetType)
        {
            case EnvironmentPreset.ModernRuins:
                int size = 3;
                var g = Constructor.GetModernRuinPart(size);
                GameMaster.realMaster.environmentMaster.AddDecoration(size, g);
                break;
        }        
    }
   
    public MapMarkerType GetSuitablePointType(float ascension)
    {
        var availableTypes = MapPoint.GetAvailablePointsType(ascension);
        int x = Random.Range(0, availableTypes.Count);
        var mp = MapPoint.CreatePointOfType(0, 0, availableTypes[x]);
        List<MapMarkerType> envtypes = new List<MapMarkerType>() {MapMarkerType.Star};
        //full:
        //envtypes = new MapMarkerType[]
        //        {
       //             MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.Island, MapMarkerType.SOS,
       //             MapMarkerType.Portal, MapMarkerType.Colony, MapMarkerType.Star, MapMarkerType.Wiseman, MapMarkerType.Wonder, MapMarkerType.Resources
        //        };
        switch (presetType)
        {
            case EnvironmentPreset.WhiteSpace:
                envtypes = new List<MapMarkerType>() { MapMarkerType.Island, MapMarkerType.Star, MapMarkerType.Wiseman, MapMarkerType.Wonder };
                break;
            case EnvironmentPreset.BlackSpace:
                envtypes = new List<MapMarkerType>() { MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.SOS, MapMarkerType.Portal, MapMarkerType.Wonder, MapMarkerType.Resources };
                break;
            case EnvironmentPreset.IceSpace:
            case EnvironmentPreset.FireSpace:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.SOS, MapMarkerType.Portal,
                    MapMarkerType.Star, MapMarkerType.Wiseman, MapMarkerType.Wonder, MapMarkerType.Resources
                };
                break;
            case EnvironmentPreset.WaterSpace:
                envtypes = new List<MapMarkerType>()
               {
                    MapMarkerType.Station, MapMarkerType.Wreck, 
                    MapMarkerType.Portal, MapMarkerType.Colony, MapMarkerType.Wonder
               };
               break;
            case EnvironmentPreset.LightStorm:
               envtypes = new List<MapMarkerType>()
               {
                    MapMarkerType.Wreck, MapMarkerType.SOS,
                    MapMarkerType.Star, MapMarkerType.Wonder, MapMarkerType.Resources
               };
               break;
            case EnvironmentPreset.DarkCanyon:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.Island, MapMarkerType.SOS,
                    MapMarkerType.Portal, MapMarkerType.Colony, MapMarkerType.Star, MapMarkerType.Resources
                };
                break;
            case EnvironmentPreset.EndlessFields:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Island, MapMarkerType.Portal, MapMarkerType.Colony,
                    MapMarkerType.Star, MapMarkerType.Wiseman, MapMarkerType.Wonder
                };
                break;
            case EnvironmentPreset.UnderrealmCaverns:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.Island, MapMarkerType.SOS,
                    MapMarkerType.Portal, MapMarkerType.Colony, MapMarkerType.Star, MapMarkerType.Wiseman, MapMarkerType.Wonder, MapMarkerType.Resources
                };
                break;
            case EnvironmentPreset.OceanWorld:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.Island, MapMarkerType.SOS,
                    MapMarkerType.Portal, MapMarkerType.Colony, MapMarkerType.Wonder
                };
                break;
            case EnvironmentPreset.DiedWorld:
            case EnvironmentPreset.DesertWorld:
            case EnvironmentPreset.SoulRuins:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Station, MapMarkerType.Wreck, MapMarkerType.Island,
                    MapMarkerType.Star, MapMarkerType.Wiseman, 
                };
                break;
            case EnvironmentPreset.ForestWorld:
                envtypes = new List<MapMarkerType>()
                {
                    MapMarkerType.Wreck, MapMarkerType.Island, MapMarkerType.SOS,
                    MapMarkerType.Resources
                };
                break;
            case EnvironmentPreset.DreamRealm:
               envtypes = new List<MapMarkerType>()
               {
                    MapMarkerType.Island,
                    MapMarkerType.Colony, MapMarkerType.Star, MapMarkerType.Wiseman, MapMarkerType.Wonder, MapMarkerType.Resources
               };
                break;
            case EnvironmentPreset.AncientRuins:
            case EnvironmentPreset.ModernRuins:
                envtypes = new List<MapMarkerType>()
               {
                    MapMarkerType.Station, MapMarkerType.SOS,
                    MapMarkerType.Wiseman, MapMarkerType.Wonder, MapMarkerType.Resources
               };
                break;
        }
        int i = 0;
        while (i < envtypes.Count)
        {
            if (!availableTypes.Contains(envtypes[i])) {
                envtypes.RemoveAt(i);
                continue;
            }
            else
            {
                i++;
            }
        }
        return envtypes[Random.Range(0, envtypes.Count)];
    }
}
