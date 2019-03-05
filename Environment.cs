using UnityEngine;

public struct Environment
{
    public enum EnvironmentPresets
    {
        Default, WhiteSpace, BlackSpace, IceSpace, FireSpace, WaterSpace, LightStorm,
        DarkCanyon, EndlessFields, UnderrealmCaverns, OceanWorld, UndergroundWorld, DiedWorld, ForestWorld, DesertWorld,
        DreamRealm, AncientRuins, ModernRuins, SoulRuins, Custom
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

    public readonly SkySphereType skyType;
    public readonly BeltType beltType;
    public readonly LowSphereType lowSphereType;
    public readonly DecorationsPack decorationsPack;
    public readonly Weather constantWeather;

    public readonly float conditions;

    public readonly Color upColor, downColor, middleColor, effectsColor, sunColor;

    static Environment()
    {
        defaultEnvironment = new Environment(EnvironmentPresets.Default, Color.white);
    }

    public Environment(EnvironmentPresets ep, Color i_sunColor)
    {
        switch (ep)
        {
            case EnvironmentPresets.Default:
            default:
                presetType = EnvironmentPresets.Default;
                skyType = SkySphereType.Stars;
                beltType = BeltType.NoBelt;
                lowSphereType = LowSphereType.Clouds;
                decorationsPack = DecorationsPack.NoDecorations;
                constantWeather = Weather.NoWeather;

                upColor = Color.white;
                downColor = upColor;
                effectsColor = upColor;
                sunColor = i_sunColor;
                middleColor = i_sunColor;

                conditions = 1;
                break;
        }
    }
}