using UnityEngine;

public struct Environment
{
    public enum EnvironmentPreset
    {
        Default, WhiteSpace, BlackSpace, IceSpace, FireSpace, WaterSpace, LightStorm,
        DarkCanyon, EndlessFields, UnderrealmCaverns, OceanWorld, UndergroundWorld, DiedWorld, ForestWorld, DesertWorld,
        DreamRealm, AncientRuins, ModernRuins, SoulRuins, Custom
    }
    public struct LightSettings
    {
        public readonly bool sunIsMapPoint;
        public readonly MapPoint sun;
        public Vector3 direction;
        public float maxIntensity;
        public Color sunColor, bottomColor, horizonColor;

        public LightSettings(SunPoint i_sun, float i_maxIntensity, Color i_bottomColor, Color i_horizonColor)
        {
            sun = i_sun;
            if (sun != null)
            {
                sunIsMapPoint = true;
                direction = Vector3.forward;
            }
            else
            {
                sunIsMapPoint = false;
                direction = new Vector3(-0.5f, -0.1f, -0.5f);
            }
            maxIntensity = i_maxIntensity;
            sunColor = i_sun.color;
            bottomColor = i_bottomColor;
            horizonColor = i_horizonColor;
        }
        public LightSettings(Vector3 dir, float i_maxIntensity, Color i_sunColor, Color i_bottomColor, Color i_horizonColor)
        {
            sun = null;
            sunIsMapPoint = false;
            direction = dir;
            maxIntensity = i_maxIntensity;
            sunColor = i_sunColor;
            bottomColor = i_bottomColor;
            horizonColor = i_horizonColor;
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
    public readonly float conditions;
    public LightSettings lightSettings;

    static Environment()
    {
        defaultEnvironment = new Environment(EnvironmentPreset.Default, new LightSettings(
            new Vector3(1f,-0.3f,1f).normalized, 0f, 
            Color.white, new Color(0.843f, 0.843f, 0.843f), new Color(0.2261f, 0.6226f, 0.6039f)
            ));
    }

    public static LightSettings GetDefaultLightSettings(EnvironmentPreset ep)
    {
        switch (ep) {
            case EnvironmentPreset.LightStorm:
                return new LightSettings(new Vector3(2f,-0.5f, 2f), 1.2f, new Color(1f, 0.99f, 0.86f), new Color(0.58f, 0.58f, 0.52f), new Color(0.96f, 0.79f, 0.4f))
            case EnvironmentPreset.WaterSpace:
                return new LightSettings(Vector3.down, 0.5f, new Color(0.18f, 0.65f, 1f), new Color(0f, 0f, 0.4f), new Color(0.06f, 0.75f, 0.98f));
            case EnvironmentPreset.FireSpace:
                return new LightSettings(Vector3.down, 1.5f, new Color(0.93f, 0.26f, 0.11f),new Color(0.31f, 0.07f, 0.05f), Color.yellow);
            case EnvironmentPreset.IceSpace:
                return new LightSettings(new Vector3(1f, -1f, 1f), 0.85f, new Color(0.588f, 0.853f, 0.952f), Color.blue, Color.white);
            case EnvironmentPreset.BlackSpace:
                return new LightSettings(new Vector3(1f, -0.3f, 1f), 0.85f, Color.white, Color.black, Color.black);
            case EnvironmentPreset.WhiteSpace:
                return new LightSettings(Vector3.down, 1.3f, Color.white, Color.white, Color.white);
            case EnvironmentPreset.Default:
            default:
                return defaultEnvironment.lightSettings;
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

    public float GetEventTime()
    {
        if (presetType != EnvironmentPreset.Default)
        {
            return 20f;
        }
        else return 0;
    }
    public void Event()
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
}
