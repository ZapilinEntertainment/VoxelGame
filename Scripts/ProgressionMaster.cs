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

    public static MapPointType DefineMapPointType(GlobalMap gmap)
    {
        var availableTypes = new List<MapPointType>() { MapPointType.Resources };
        if (createNewStars && Random.value < STAR_CREATE_CHANCE) availableTypes.Add(MapPointType.Star);
        byte stage;
        if (ascension <= GameConstants.ASCENSION_MEDIUM)
        {
            if (ascension <= GameConstants.ASCENSION_VERYLOW) stage = 0;
            else
            {
                if (ascension > GameConstants.ASCENSION_LOW) stage = 2;
                else stage = 1;
            }
        }
        else
        {
            if (ascension > GameConstants.ASCENSION_VERYHIGH) stage = 5;
            else
            {
                if (ascension > GameConstants.ASCENSION_HIGH) stage = 4;
                else stage = 3;
            }
        }
        float r = Random.value;
        int rx = (int)(r / 0.2f);
        switch (stage)
        {
            case 0:
            case 1: // less than 0.3
                availableTypes.Add(MapPointType.Wreck);
                availableTypes.Add(MapPointType.SOS);
                break;
            case 2: // 0.3 - 0.5
                availableTypes.Add(MapPointType.Wreck);
                availableTypes.Add(MapPointType.SOS);
                availableTypes.Add(MapPointType.Station);
                availableTypes.Add(MapPointType.Island);
                availableTypes.Add(MapPointType.Colony);
                break;
            case 3: // 0.5 - 0.7
                availableTypes.Add(MapPointType.Station);
                availableTypes.Add(MapPointType.Island);
                availableTypes.Add(MapPointType.Colony);
                availableTypes.Add(MapPointType.Portal);
                break;
            case 4: // 0.7 - 0.9
                availableTypes.Add(MapPointType.Island);
                availableTypes.Add(MapPointType.Colony);
                availableTypes.Add(MapPointType.Portal);
                availableTypes.Add(MapPointType.Wonder);
                availableTypes.Add(MapPointType.Wiseman);
                break;
            case 5: // more than 0.9
                availableTypes.Add(MapPointType.Wonder);
                availableTypes.Add(MapPointType.Wiseman);
                break;
        }
        //     

        var pos = GetSectorPosition(i);
        int x = Random.Range(0, availableTypes.Count);
    }
    public static byte DefinePuzzlePartColorcode(PointOfInterest poi)
    {
        float redChance = 0.33f, greenChance = 0.33f, blueChance = 0.33f,
                                            cyanChance = mysteria / 10f + difficulty / 10f,
                                            blackChance = difficulty / 50f,
                                            whiteChance = difficulty / 25f + Random.value * friendliness * 0.25f;
        sum = redChance + greenChance + blueChance + cyanChance + blueChance + whiteChance;

        v = (v - 0.5f) * 2f * sum;
        if (v < redChance + greenChance + blueChance)
        {
            if (v < redChance) { challengeArray[x, y] = new ChallengeField(ChallengeType.PuzzlePart, Knowledge.REDCOLOR_CODE); }
            else
            {
                if (v < redChance + greenChance) challengeArray[x, y] = new ChallengeField(ChallengeType.PuzzlePart, Knowledge.GREENCOLOR_CODE);
                else challengeArray[x, y] = new ChallengeField(ChallengeType.PuzzlePart, Knowledge.BLUECOLOR_CODE);
            }
        }
        else
        {
            if (whiteChance == 0) challengeArray[x, y] = new ChallengeField(ChallengeType.PuzzlePart, Knowledge.CYANCOLOR_CODE);
            else
            {
                if (v > redChance + greenChance + blueChance + whiteChance) challengeArray[x, y] = new ChallengeField(ChallengeType.PuzzlePart, Knowledge.CYANCOLOR_CODE);
                else challengeArray[x, y] = new ChallengeField(ChallengeType.PuzzlePart, Knowledge.WHITECOLOR_CODE);
            }
        }
    }
    public static ChallengeField GetRouteBoostCell(PointOfInterest poi, float mysteria)
    {
        ChallengeType[] ctypes = new ChallengeType[3];
        bool moreMystic = mysteria > 0.5f, moreSpecific = false;
        switch (type)
        {
            case MapPointType.Station:
                {
                    moreSpecific = friendliness > 0.5f;
                    ctypes[0] = ChallengeType.CrystalPts;
                    ctypes[1] = ChallengeType.MonumentPts;
                    ctypes[2] = ChallengeType.FoundationPts;
                    break;
                }
            case MapPointType.Wreck:
                {
                    moreSpecific = difficulty > 0.5f;
                    ctypes[0] = ChallengeType.EnginePts;
                    ctypes[1] = ChallengeType.PollenPts;
                    ctypes[2] = ChallengeType.CloudWhalePts;
                    break;
                }
            case MapPointType.Island:
                {
                    moreSpecific = richness > 0.5f;
                    ctypes[0] = ChallengeType.BlossomPts;
                    ctypes[1] = ChallengeType.PollenPts;
                    ctypes[2] = ChallengeType.CloudWhalePts;
                    break;
                }
            case MapPointType.SOS:
                {
                    moreSpecific = danger > 0.5f;
                    ctypes[0] = ChallengeType.CloudWhalePts;
                    ctypes[1] = ChallengeType.EnginePts;
                    ctypes[2] = ChallengeType.PipesPts;
                    break;
                }
            case MapPointType.Portal:
                {
                    moreSpecific = difficulty > 0.5f;
                    ctypes[0] = ChallengeType.PipesPts;
                    ctypes[1] = ChallengeType.CrystalPts;
                    ctypes[2] = ChallengeType.EnginePts;
                    break;
                }
            case MapPointType.Colony:
                {
                    moreSpecific = richness > 0.5f;
                    ctypes[0] = ChallengeType.FoundationPts;
                    ctypes[1] = ChallengeType.MonumentPts;
                    ctypes[2] = ChallengeType.CrystalPts;
                    break;
                }
            case MapPointType.Wiseman:
                {
                    moreSpecific = friendliness < 0.5f;
                    ctypes[0] = ChallengeType.PollenPts;
                    ctypes[1] = ChallengeType.PipesPts;
                    ctypes[2] = ChallengeType.BlossomPts;
                    break;
                }
            case MapPointType.Wonder:
                {
                    moreSpecific = Random.value > 0.5f;
                    ctypes[0] = ChallengeType.MonumentPts;
                    ctypes[1] = ChallengeType.BlossomPts;
                    ctypes[2] = ChallengeType.FoundationPts;
                    break;
                }

            case MapPointType.Star:
            case MapPointType.QuestMark:
            case MapPointType.Resources:
            case MapPointType.FlyingExpedition:
            case MapPointType.Unknown:
            case MapPointType.MyCity:
                {
                    moreSpecific = true;
                    v *= 2f;
                    if (v > 0.5f)
                    {
                        if (v < 0.25f) ctypes[0] = ChallengeType.FoundationPts;
                        else ctypes[0] = ChallengeType.EnginePts;
                    }
                    else
                    {
                        if (v > 0.75f) ctypes[0] = ChallengeType.BlossomPts;
                        else ctypes[0] = ChallengeType.CrystalPts;
                    }
                    ctypes[1] = Random.value > 0.5f ? ChallengeType.CloudWhalePts : ChallengeType.MonumentPts;
                    ctypes[2] = Random.value > 0.5f ? ChallengeType.PipesPts : ChallengeType.PollenPts;
                    break;
                }
        }
        byte val = (byte)(1f + richness / 0.25f + difficulty / 0.25f);
        if (moreMystic & moreSpecific)
        {
            if (v < 0.4f) challengeArray[x, y] = new ChallengeField(ctypes[0], val);
            else
            {
                if (v > 0.7f) challengeArray[x, y] = new ChallengeField(ctypes[1], val);
                else challengeArray[x, y] = new ChallengeField(ctypes[2], val);
            }
        }
        else
        {
            if (v < 0.4f) challengeArray[x, y] = new ChallengeField(ctypes[0], val);
            else
            {
                if (moreMystic) challengeArray[x, y] = new ChallengeField(ctypes[1], val);
                else challengeArray[x, y] = new ChallengeField(ctypes[2], val);
            }
        }
    }
}
