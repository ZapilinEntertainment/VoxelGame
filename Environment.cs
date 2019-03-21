using UnityEngine;

public struct Environment
{
    public enum EnvironmentPresets
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

        public LightSettings(MapPoint i_sun, float i_maxIntensity, Color i_sunColor, Color i_bottomColor, Color i_horizonColor)
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
            sunColor = i_sunColor;
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

    public readonly EnvironmentPresets presetType;
    public readonly float conditions;
    public LightSettings lightSettings;

    static Environment()
    {
        defaultEnvironment = new Environment(EnvironmentPresets.Default, new LightSettings(
            new Vector3(1f,-0.3f,1f).normalized, 0f, 
            Color.white, new Color(0.843f, 0.843f, 0.843f), new Color(0.2261f, 0.6226f, 0.6039f)
            ));
    }

    public Environment(EnvironmentPresets ep, LightSettings ls)
    {
        switch (ep)
        {
            case EnvironmentPresets.Default:
            default:
                presetType = EnvironmentPresets.Default;
                lightSettings = ls;
                conditions = 1;
                break;
        }
    }

    public float GetEventTime()
    {
        if (presetType != EnvironmentPresets.Default)
        {
            return 20f;
        }
        else return 0;
    }
    public void Event()
    {
        switch (presetType)
        {
            case EnvironmentPresets.ModernRuins:
                int size = 3;
                var g = Constructor.GetModernRuinPart(size);
                GameMaster.realMaster.environmentMaster.AddDecoration(size, g);
                break;
        }        
    }
}
