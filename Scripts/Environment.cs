using UnityEngine;
using System.Collections.Generic;

public sealed class Environment
{
    public enum EnvironmentPreset : byte
    {
        Default, Ocean, Meadows, WhiteSpace, // normal
        Space, Ice, Desert, Fire, // dead
        Ruins, Crystal, Forest, Pollen, // ascended
        Pipe // special
            // edge -> center
    }
    //dependecies:
    // PickEnvironmentPreset
    // PickMainPointType

    public static readonly Environment defaultEnvironment;
    public readonly EnvironmentPreset presetType;
    public float conditions { get; private set; }
    public readonly float richness, lifepowerSupport, stability, lightIntensityMultiplier;
    public readonly Color bottomColor, skyColor, horizonColor;
    private const float DEFAULT_CONDITIONS = 0.75f, DEFAULT_RICHNESS = 0.2f, DEFAULT_LP_SUPPORT = 0.8f, DEFAULT_STABILITY = 0.5f;
    private static Dictionary<EnvironmentPreset, Environment> elist;

    static Environment()
    {
        defaultEnvironment = new Environment(EnvironmentPreset.Default);
    } 
    public static Environment GetEnvironment(EnvironmentPreset ep)
    {
        if (elist != null)
        {
            if (elist.ContainsKey(ep)) return elist[ep];
            else
            {
                var e = new Environment(ep);
                elist.Add(ep, e);
                return e;
            }
        }
        else
        {
            var e = new Environment(ep);
            elist = new Dictionary<EnvironmentPreset, Environment>();
            elist.Add(ep, e);
            return e;
        }
    }
    public static Environment GetEnvironment(float ascension, float height)
    {
        return GetEnvironment(PickEnvironmentPreset(ascension, height));
    }
    private Environment(EnvironmentPreset ep)
    {
        presetType = ep;
        switch(ep)
        {
            case EnvironmentPreset.Desert:
                conditions = 0.3f;
                lifepowerSupport = 0.1f;
                lightIntensityMultiplier = 1.1f;
                stability = 0.4f;
                skyColor = new Color(1f, 0.74f, 0.65f);
                bottomColor = new Color(1f, 0.95f, 0.74f);
                horizonColor = Color.white;
                break;
            case EnvironmentPreset.WhiteSpace:
                conditions = 1f;
                lifepowerSupport = 0.01f;
                lightIntensityMultiplier = 1.5f;
                stability = 0f;
                skyColor = Color.white;
                bottomColor = skyColor;
                horizonColor = skyColor;
                break;
            case EnvironmentPreset.Pipe:
                conditions = DEFAULT_CONDITIONS;
                lifepowerSupport = DEFAULT_LP_SUPPORT / 2f;
                stability = 0.98f;
                lightIntensityMultiplier = 0.25f;
                skyColor = new Color(0.58f, 1f, 0.75f);
                bottomColor = new Color(0f, 0.65f, 0.4f);
                horizonColor = new Color(0.22f, 0.46f, 0.29f);
                break;
            case EnvironmentPreset.Ice:
                conditions = 0.25f;
                lifepowerSupport = 0.2f;
                stability = 0.55f;
                lightIntensityMultiplier = 0.7f;
                skyColor = new Color(0.588f, 0.853f, 0.952f);
                bottomColor = Color.blue;
                horizonColor = Color.white;
                break;
            case EnvironmentPreset.Pollen:
                conditions = 0.4f;
                lifepowerSupport = 0.6f;
                stability = 0.3f;
                lightIntensityMultiplier = 0.8f;
                skyColor = new Color(1f, 0.99f, 0.86f);
                bottomColor = new Color(0.58f, 0.58f, 0.52f);
                horizonColor = new Color(0.96f, 0.79f, 0.4f);
                break;
            case EnvironmentPreset.Forest:
                conditions = 0.89f;
                lifepowerSupport = 0.8f;
                stability = DEFAULT_STABILITY;
                lightIntensityMultiplier = 1f;
                skyColor = new Color(1f, 0.96f, 0.74f);
                bottomColor = new Color(0.57f, 0.84f, 0.57f);
                horizonColor = new Color(0.07f, 0.43f, 0.07f);
                break;
            case EnvironmentPreset.Ruins:
                conditions = 0.4f;
                lifepowerSupport = 0.3f;
                stability = 0.6f;
                lightIntensityMultiplier = 0.9f;
                skyColor = new Color(0.6f, 0.6f, 0.2f);
                bottomColor = new Color(0.82f, 0.83f, 0.17f);
                horizonColor = new Color(0.25f, 0.25f, 0.15f);
                break;
            case EnvironmentPreset.Crystal:
                conditions = 0.5f;
                lifepowerSupport = 0.1f;
                stability = 0.85f;
                lightIntensityMultiplier = 0.85f;
                skyColor = new Color(0.27f, 0.83f, 0.85f);
                bottomColor = new Color(0.73f, 0.83f, 0.83f);
                horizonColor = new Color(0.04f, 0.91f, 0.95f);
                break;
            case EnvironmentPreset.Meadows:
                conditions = DEFAULT_CONDITIONS * 1.1f;
                lifepowerSupport = DEFAULT_LP_SUPPORT * 1.2f;
                stability = DEFAULT_STABILITY;
                lightIntensityMultiplier = 1f;
                skyColor = new Color(1f, 0.94f, 0.71f);
                bottomColor = new Color(0.88f, 0.82f, 0.54f);
                horizonColor = new Color(2f, 0.84f, 0f);
                break;
            case EnvironmentPreset.Space:
                conditions = 0f;
                lifepowerSupport = 0f;
                stability = 0.1f;
                lightIntensityMultiplier = 1f;
                skyColor = Color.white;
                bottomColor = Color.black;
                horizonColor = Color.cyan * 0.25f;
                break;
            case EnvironmentPreset.Fire:
                conditions = 0.2f;
                lifepowerSupport = 0.1f;
                stability = 0.33f;
                lightIntensityMultiplier = 1.1f;
                skyColor = new Color(0.93f, 0.26f, 0.11f);
                bottomColor = new Color(0.31f, 0.07f, 0.05f);
                horizonColor = Color.yellow;
                break;
            case EnvironmentPreset.Ocean:
                conditions = DEFAULT_CONDITIONS * 0.9f;
                lifepowerSupport = DEFAULT_LP_SUPPORT * 0.75f;
                stability = 0.8f;
                lightIntensityMultiplier = 0.5f;
                skyColor = new Color(0.61f, 0.99f, 0.94f);
                bottomColor = new Color(0f, 0.62f, 1f);
                horizonColor = new Color(0f, 0f, 0.65f);
                break;
            default:
                presetType = EnvironmentPreset.Default;
                conditions = DEFAULT_CONDITIONS;
                lifepowerSupport = DEFAULT_LP_SUPPORT;
                stability = DEFAULT_STABILITY;
                lightIntensityMultiplier = 1f;
                bottomColor = Color.white;
                skyColor = Color.white;
                horizonColor = Color.cyan * 0.5f;
                break;
        }
    }    
    public static EnvironmentPreset PickEnvironmentPreset(float ascension, float height)
    {
        float outerVar = (height - 0.7f) / 0.3f; if (outerVar < 0f) outerVar = 0f;
        float svar = 1f - Mathf.Abs(height - 0.6f) / 0.4f; if (svar < 0f) svar = 0f;
        float svar2 = 1f - Mathf.Abs(height - 0.4f) / 0.4f; if (svar2 < 0f) svar2 = 0f;
        float innerVar = 1f - height / 0.3f; if (innerVar < 0f) innerVar = 0f;
        float x = Random.value;
        if (x < 0.1f) x /= 0.1f;
        else
        {
            if (x > 0.9f) x = (x - 0.9f) / 0.1f;
        }        

        var list = new List<(EnvironmentPreset, float)>() // вероятность
        {
            (EnvironmentPreset.Default, 1f)        
        };
        float sum = 0f ,f;
        if (svar != 0f)
        {
            f = svar * x;
            list.Add((EnvironmentPreset.Ocean, f));
            sum += f;
        }
        if (svar2 != 0f)
        {
            f = svar2 * x;
            list.Add((EnvironmentPreset.Meadows, f));
            sum += f;
        }
        if (innerVar != 0f)
        {
            f = innerVar * x;
            list.Add((EnvironmentPreset.WhiteSpace, f));
            sum += f;
        }
        float avar;
        if (ascension < 0.5f)
        {
            avar = 1f - ascension / 0.5f;
            if (outerVar != 0f)
            {
                f = outerVar * avar * (1 - x);
                list.Add((EnvironmentPreset.Space, f));
                sum += f;
            }
            if (svar != 0f)
            {
                f = svar * avar * (1 - x);
                list.Add((EnvironmentPreset.Ice, f));
                sum += f;
            }
            if (svar2 != 0f)
            {
                f = svar2 * avar * (1 - x);
                list.Add((EnvironmentPreset.Desert, f));
                sum += f;
            }
            if (innerVar != 0f)
            {
                f = innerVar * avar * (1 - x);
                list.Add((EnvironmentPreset.Fire, f));
                sum += f;
            }
        }
        else
        {
            avar = (ascension - 0.5f) / 0.5f;
            if (outerVar != 0f)
            {
                f = outerVar * avar * (1 - x);
                list.Add((EnvironmentPreset.Ruins, f));
                sum += f;
            }
            if (svar != 0f)
            {
                f = svar * avar * (1 - x);
                list.Add((EnvironmentPreset.Crystal, f));
                sum += f;
            }
            if (svar2 != 0f)
            {
                f = svar2 * avar * (1 - x);
                list.Add((EnvironmentPreset.Forest,f ));
                sum += f;
            }
            if (innerVar != 0f)
            {
                f = innerVar * avar * (1 - x);
                list.Add((EnvironmentPreset.Pollen, f));
                sum += f;
            }
        }
        //
        if (list.Count == 1) return list[0].Item1;
        x = Random.value * sum;
        int i;
        if (x > 0.5f)
        {
            i = list.Count - 1;
            do
            {
                if (x >= list[i].Item2)
                {
                    return list[i].Item1;
                }
                i--;
            }
            while (i >= 0);
        }
        else
        {
            i = 1;
            do
            {
                if (x < list[i].Item2)
                {
                    return list[i - 1].Item1;
                }
                i++;
            }
            while (i < list.Count);
        }
        return EnvironmentPreset.Default;
    }
    public MapMarkerType PickMainPointType()
    {           
        MapMarkerType[] list = new MapMarkerType[4];
        switch (presetType)
        {           
            case EnvironmentPreset.Ocean:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.SOS;
                list[2] = MapMarkerType.Island;               
                list[3] = MapMarkerType.Wonder;
                break;
            case EnvironmentPreset.Meadows:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Wreck;                
                list[2] = MapMarkerType.Station;
                list[3] = MapMarkerType.Colony;
                break;
            case EnvironmentPreset.WhiteSpace:
                list[0] = MapMarkerType.Star;
                list[1] = MapMarkerType.Wiseman;
                list[2] = MapMarkerType.Wonder;
                list[3] = MapMarkerType.Portal;
                break;
            case EnvironmentPreset.Space:
            case EnvironmentPreset.Ice:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Wreck;
                list[2] = MapMarkerType.SOS;
                list[3] = MapMarkerType.Station;
                break;
            case EnvironmentPreset.Desert:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Wreck;
                list[2] = MapMarkerType.SOS;
                list[3] = MapMarkerType.Island;
                break;
            case EnvironmentPreset.Fire:
                list[0] = MapMarkerType.Star;
                list[1] = MapMarkerType.Wreck;
                list[2] = MapMarkerType.SOS;
                list[3] = MapMarkerType.Portal;
                break;
            case EnvironmentPreset.Ruins:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Wreck;
                list[2] = MapMarkerType.Wiseman;
                list[3] = MapMarkerType.Colony;
                break;
            case EnvironmentPreset.Crystal:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Wiseman;
                list[2] = MapMarkerType.Island;
                list[3] = MapMarkerType.Portal;
                break;
            case EnvironmentPreset.Forest:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Island;
                list[2] = MapMarkerType.Colony;
                list[3] = MapMarkerType.Wonder;
                break;
            case EnvironmentPreset.Pollen:
                list[0] = MapMarkerType.SOS;
                list[1] = MapMarkerType.Wreck;
                list[2] = MapMarkerType.Island;
                list[3] = MapMarkerType.Portal;
                break;
            case EnvironmentPreset.Pipe:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Wreck;
                list[2] = MapMarkerType.Star;
                list[3] = MapMarkerType.Island;
                break;
            default:
                list[0] = MapMarkerType.Resources;
                list[1] = MapMarkerType.Island;
                list[2] = MapMarkerType.Station;
                list[3] = MapMarkerType.Wonder;
                break;
        }
        float x = Random.value;
        if (x > 0.3f)
        {
            if (x > 0.6f) return list[0];
            else return list[1];
        }
        else
        {
            if (x < 0.1f) return list[3];
            else return list[2];
        }
    }

    public Color GetMapColor()
    {
        return Color.Lerp(skyColor, Color.white, 0.6f);
    }
}    
