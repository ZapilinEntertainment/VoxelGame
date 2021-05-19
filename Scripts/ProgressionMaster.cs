using UnityEngine;
using System.Collections.Generic;
public static class ProgressionMaster
{
    private const float STAR_CREATE_CHANCE = 0.05f;

    public static float DefineAscension(ColonyController c)
    {
        if (c == null) return 0f;
        float maxVal = GameConstants.ASCENSION_VERYLOW;
        var lvl = c.hq?.level ?? 0;
        if (lvl > 1)
        {
            switch (lvl)
            {
                case 2:
                case 3:
                    maxVal = GameConstants.ASCENSION_LOW; break;
                case 4: maxVal = GameConstants.ASCENSION_MEDIUM; break;
                case 5:
                case 6:
                    maxVal = GameConstants.ASCENSION_HIGH;
                    break;
            }
        }
        if (c.HaveBuilding(Structure.MONUMENT_ID)) maxVal += 10f;
        if (c.HaveBuilding(Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID)) maxVal += 10f;
        //if (HaveBuilding(Structure.ASCENSION_ENGINE)) maxVal += 10f;

        var a = Knowledge.GetCurrent()?.GetCompleteness() ?? 0f;
        if (a > maxVal) a = maxVal;
        return a;
    }

    public static void DefinePointOfInterest(GlobalMap gmap, out MapPointType mtype, out Path path)
    {
        var ascension = gmap.ascension;

        ((MapPointType pointType, Path path) pointInfo, float probability)[] probabilities;
        if (ascension <= GameConstants.ASCENSION_LOW)
        {
            const float p = 1f;
            probabilities = new ((MapPointType, Path), float)[5];
            probabilities[0] = ((MapPointType.Island, Path.TechPath), p);
            probabilities[1] = ((MapPointType.Wreck, Path.TechPath), p / 2f);
            probabilities[2] = ((MapPointType.Wreck, Path.NoPath), p / 2f);
            probabilities[3] = ((MapPointType.Resources, Path.SecretPath), p);
            probabilities[4] = ((MapPointType.Island, Path.LifePath), p);
        }
        else
        {
            if (ascension < GameConstants.ASCENSION_HIGH)
            {
                const float p = 1f;
                probabilities = new ((MapPointType, Path), float)[6];
                probabilities[0] = ((MapPointType.Station, Path.TechPath), p / 2f);
                probabilities[1] = ((MapPointType.Station, Path.NoPath), p / 2f);
                probabilities[2] = ((MapPointType.SOS, Path.TechPath), p / 2f);
                probabilities[3] = ((MapPointType.SOS, Path.NoPath), p / 2f);
                probabilities[4] = ((MapPointType.Island, Path.SecretPath), p);
                probabilities[5] = ((MapPointType.Portal, Path.LifePath), p);
            }
            else
            {
                const float p = 1f;
                probabilities = new ((MapPointType, Path), float)[4];
                probabilities[0] = ((MapPointType.Colony, Path.NoPath), p);
                probabilities[1] = ((MapPointType.Wiseman, Path.TechPath), p);
                probabilities[2] = ((MapPointType.Wonder, Path.TechPath), p);
                probabilities[3] = ((MapPointType.Wonder, Path.LifePath), p);
            }
        }
        //     
        float sum = 0f;
        foreach (var p in probabilities) sum += p.probability;
        float r = Random.value;
        float x = Random.value * sum, d;
        if (r > 0.5f)
        {
            d = sum;
            for (int i = probabilities.Length - 1; i>=0; i--)
            {
                d -= probabilities[i].probability;
                if (x >= d)
                {
                    mtype = probabilities[i].pointInfo.pointType;
                    path = probabilities[i].pointInfo.path;
                    return;
                }
            }
        }
        else
        {
            d = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                d += probabilities[i].probability;
                if (x <= d)
                {
                    mtype = probabilities[i].pointInfo.pointType;
                    path = probabilities[i].pointInfo.path;
                    return;
                }
            }
        }

        mtype = MapPointType.Unknown;
        path = Path.NoPath;
    }
    
    public static ChallengeField GetRouteBoostCell(PointOfInterest poi, float mysteria)
    {
        //
        ChallengeType ctype = ChallengeType.NoChallenge;
        switch (poi.type)
        {
            case MapPointType.Island:
                {
                    switch (poi.path)
                    {
                        case Path.TechPath: ctype = ChallengeType.FoundationPts; break;
                        case Path.SecretPath: ctype = ChallengeType.CrystalPts; break;
                        case Path.LifePath: ctype = ChallengeType.BlossomPts; break;                        
                    }
                    break;
                }
            case MapPointType.Wreck:
            case MapPointType.SOS:
                {
                    switch (poi.path)
                    {
                        case Path.TechPath:
                        case Path.NoPath: ctype = ChallengeType.EnginePts; break;
                    }
                    break;
                }
            case MapPointType.Station:
                {
                    switch (poi.path)
                    {
                        case Path.TechPath: 
                        case Path.NoPath: ctype = ChallengeType.FoundationPts; break;
                    }
                    break;
                }
            case MapPointType.Colony:
                {
                    switch (poi.path)
                    {
                        case Path.NoPath: ctype = ChallengeType.FoundationPts; break;
                    }
                    break;
                }
            case MapPointType.Portal:
                {
                    switch (poi.path)
                    {
                        case Path.LifePath: ctype = ChallengeType.BlossomPts; break;
                    }
                    break;
                }
            case MapPointType.Resources:
                {
                    switch (poi.path)
                    {
                        case Path.SecretPath: ctype = ChallengeType.CrystalPts; break;
                    }
                    break;
                }
            case MapPointType.Wonder:
                {
                    switch (poi.path)
                    {
                        case Path.TechPath: ctype = ChallengeType.CrystalPts; break;
                        case Path.LifePath: ctype = ChallengeType.BlossomPts; break;
                    }
                    break;
                }
            case MapPointType.Wiseman:
                {
                    switch (poi.path)
                    {
                        case Path.TechPath: ctype = ChallengeType.EnginePts; break;
                    }
                    break;
                }

        }
        //
        if (ctype != ChallengeType.NoChallenge)
        {
            return new ChallengeField(ctype, (byte)(1f + poi.richness / 0.25f + poi.difficulty / 0.25f));
        }
        else return new ChallengeField();
    }

}
