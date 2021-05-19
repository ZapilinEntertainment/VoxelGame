using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointOfInterest : MapPoint
{
    public float richness { get; protected set; }
    protected float danger, mysteria, friendliness;
    protected ChallengeField[,] challengeArray;

    public readonly float difficulty;
    public Path path { get; protected set; }
    public Expedition workingExpedition { get; protected set; }
    private const float ONE_STEP_CRYSTALS = 10f, ONE_STEP_XP = 10f;

    public PointOfInterest(int i_id) : base(i_id) {
        difficulty = CalculateDifficulty();
    }
    public PointOfInterest(float i_angle, float i_height, MapPointType mtype, Path i_path) : base(i_angle, i_height, mtype)
    {
        difficulty = CalculateDifficulty();
        path = i_path;
    }

    public void AssignExpedition(Expedition e)
    {
        workingExpedition = e;
    }
    public void DeassignExpedition(Expedition e)
    {
        if (workingExpedition == e) workingExpedition = null;
    }

    public void ResetChallengesArray()
    {
        challengeArray = null;
    }
    public ref ChallengeField[,] GetChallengesArrayRef()
    {
        if (challengeArray == null) GenerateChallengesArray();
        return ref challengeArray;
    }
    public ref ChallengeField GetChallengeField(int x, int y)
    {
        if (challengeArray == null) GenerateChallengesArray();
        return ref challengeArray[x, y];
    }
    public ref ChallengeField GetChallengeField(int i)
    {
        int size = GetChallengesArraySize();
        return ref challengeArray[i % size, i / size];
    }
    public int GetChallengesArraySize()
    {
        if (challengeArray == null) GenerateChallengesArray();
        return challengeArray.GetLength(0);
    }
    
    public void ConvertToChallengeable(int xpos, int zpos)
    {
        if (challengeArray != null)
        {
            int sz = challengeArray.GetLength(0);
            if (xpos < sz && zpos < sz)
            {
                var cf = challengeArray[xpos, zpos];
                if (cf.challengeType == ChallengeType.Impassable | cf.challengeType == ChallengeType.NoChallenge)
                {
                    float x = Random.value;
                    if (x > 0.9f) cf.ChangeChallengeType(ChallengeType.NoChallenge, 0);
                    else
                    {
                        if (x < 0.33f) cf.ChangeChallengeType(ChallengeType.PersistenceTest, (byte)(ChallengeField.MAX_DIFFICULTY * 0.85f * (0.5f + 0.5f * difficulty)));
                        else
                        {
                            if (x > 0.6f) cf.ChangeChallengeType(ChallengeType.PerceptionTest, (byte)(ChallengeField.MAX_DIFFICULTY * 0.85f * (0.5f + 0.5f * difficulty)));
                            else cf.ChangeChallengeType(ChallengeType.IntelligenceTest, (byte)(ChallengeField.MAX_DIFFICULTY * 0.85f * (0.5f + 0.5f * difficulty)));
                        }
                    }
                    cf.ChangeHiddenStatus(false);
                }
            }
        }
    }

    public bool HardTest(Crew c)
    {
        return c.HardTestRoll() >= danger * (20f + 10f * difficulty);
    }
    public bool SoftTest(Crew c)
    {
        return c.SoftCheckRoll() >= friendliness * (10f + 10f * difficulty);
    }
    public bool LoyaltyTest(Crew c)
    {
        return c.LoyaltyRoll() >= danger * 20f * (1f - friendliness) ;
    }
    public bool AdaptabilityTest(Crew c)
    {
        return c.AdaptabilityRoll() >= mysteria * 20f;
    }
    public bool TrueWayTest(Crew c)
    {
        return 20f * difficulty * (1f - friendliness) < c.IntelligenceRoll();
    }
    public bool IsSomethingChanged()
    {
        return Random.value < danger;
    }

    protected float CalculateDifficulty()
    {
        (float min, float max) localDifficulty = (0f,0f);
        (float min, float max) lowDiffuculty = (0.05f,0.25f), 
            normalDifficulty = (0.25f, 0.75f), 
            highDifficulty = (0.75f, 1f);
        switch (type)
        {
            case MapPointType.Unknown:
                richness = Random.value;
                danger = Random.value;
                mysteria = 1f;
                friendliness = Random.value;
                localDifficulty = (0f,1f);
                break;
            case MapPointType.MyCity:
                richness = 0.05f;
                danger = 0f;
                mysteria = 0.01f;
                friendliness = GameMaster.realMaster.environmentMaster.environmentalConditions;
                localDifficulty = (0f, 0f);
                break;
            // SUNPOINT:
            //case MapMarkerType.Star:  
            // richness = 0.1f;
            // danger = 1f;
            //  mysteria = 0.1f + Random.value * 0.2f;
            //  friendliness = Random.value;
            // locationDifficulty = 1f;
            //  break;
            case MapPointType.Station:
                richness = Random.value * 0.8f + 0.2f;
                danger = Random.value * 0.2f;
                mysteria = Random.value * 0.5f;
                friendliness = Random.value;
                localDifficulty = lowDiffuculty;
                break;
            case MapPointType.Wreck:
                richness = 0.3f + Random.value * 0.5f;
                danger = 0.3f + Random.value * 0.2f;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.8f;
                localDifficulty = highDifficulty;
                break;
            case MapPointType.FlyingExpedition:
                richness = 0f;
                danger = 0f;
                mysteria = 0f;
                friendliness = 1f;
                localDifficulty = (0f,0f);
                break; // flyingExpedition.expedition.sectorCollapsingTest
            case MapPointType.Island:
                richness = 0.2f + Random.value * 0.7f;
                danger = Random.value * 0.5f;
                mysteria = 0.2f + Random.value * 0.5f;
                friendliness = Random.value * 0.7f + 0.3f;
                localDifficulty = normalDifficulty;
                break;
            case MapPointType.SOS:
                richness = 0f;
                danger = 0.2f + 0.8f * Random.value;
                mysteria = Random.value * 0.3f;
                friendliness = Random.value * 0.5f + 0.5f;
                localDifficulty = highDifficulty;
                break;
            case MapPointType.Portal:
                richness = 0.1f;
                danger = 0.3f + Random.value;
                mysteria = 0.3f + Random.value * 0.7f;
                friendliness = Random.value;
                localDifficulty = highDifficulty;
                break;
            case MapPointType.QuestMark:
                // устанавливается квестом                
                break;
            case MapPointType.Colony:
                richness = 0.5f + Random.value * 0.5f;
                danger = 0.1f * Random.value;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.7f + 0.3f;
                localDifficulty = lowDiffuculty;
                break;
            case MapPointType.Wiseman:
                richness = 0.1f;
                danger = 0.1f * Random.value;
                mysteria = Random.value;
                friendliness = Random.value * 0.3f + 0.7f;
                localDifficulty = normalDifficulty;
                break;
            case MapPointType.Wonder:
                richness = 0.3f + Random.value * 0.7f;
                danger = 0.1f * Random.value;
                mysteria = 0.5f + Random.value * 0.5f;
                friendliness = Random.value;
                localDifficulty = highDifficulty;
                break;
            case MapPointType.Resources:
                richness = 0.5f + Random.value * 0.5f;
                danger = Random.value * 0.3f;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.6f;
                localDifficulty = highDifficulty;
                break;
        }
        float _difficulty = 0f;
        if (localDifficulty.max == localDifficulty.min) _difficulty = localDifficulty.max;
        else
        {
            float modifiers = GameMaster.realMaster.globalMap.ascension+ danger - friendliness + (1f -height) ;
            _difficulty = localDifficulty.min + (Random.value * (localDifficulty.max - localDifficulty.min)) + modifiers * 0.25f;
        }
        if (_difficulty < localDifficulty.min) _difficulty = localDifficulty.min;
        else if (_difficulty > 1f) _difficulty = 1f;
        return _difficulty;
    }
    protected Artifact GetArtifact()
    {
        float a = mysteria * friendliness * 0.8f + 0.2f * Random.value;
        return new Artifact(
            stability * (0.9f + 0.2f * Random.value), 
            richness * (0.9f + 0.2f * Random.value),
            danger * (0.9f + 0.2f * Random.value),
            path
            );
    }
    public void OneStepReward(Expedition e)
    {
        e.AddCrystals((int)(ONE_STEP_CRYSTALS * ((0.75f + 0.5f * (0.5f + 0.5f * friendliness) * Random.value) * (1f + difficulty) * (0.25f + 0.75f * richness) )));
        e.crew.AddExperience(ONE_STEP_XP * (1f + difficulty));
    }

    virtual protected void GenerateChallengesArray()
    {
        int size = 5 + Random.Range(0, (int)(3f * mysteria + 3f * richness));
        challengeArray = new ChallengeField[size, size];

        // generating:
        int sqr = size * size;
        float totalVariety = danger + mysteria + friendliness + difficulty;
        float abilityTestChance = danger / totalVariety, mysteriaChance = mysteria / totalVariety,
            giftChance = friendliness / totalVariety, puzzlePartChance = difficulty / totalVariety;
        float maxDifficulty = ChallengeField.MAX_DIFFICULTY * difficulty;
        float v = Random.value, df;

        var atests = new ChallengeType[6];
        switch (path)
        {
            case Path.TechPath:
                {
                    if (v > 0.5f)
                    {
                        atests[0] = ChallengeType.IntelligenceTest;
                        atests[1] = ChallengeType.TechSkillsTest;
                    }
                    else
                    {
                        atests[1] = ChallengeType.IntelligenceTest;
                        atests[0] = ChallengeType.TechSkillsTest;
                    }
                    v = Random.value;
                    if (v < 0.5f)
                    {
                        atests[2] = ChallengeType.PerceptionTest;
                        atests[3] = ChallengeType.PersistenceTest;
                        if (v < 0.25f)
                        {
                            atests[4] = ChallengeType.SurvivalSkillsTest;
                            atests[5] = ChallengeType.SecretKnowledgeTest;
                        }
                        else
                        {
                            atests[5] = ChallengeType.SurvivalSkillsTest;
                            atests[4] = ChallengeType.SecretKnowledgeTest;
                        }
                    }
                    else
                    {
                        atests[3] = ChallengeType.PersistenceTest;
                        atests[2] = ChallengeType.PerceptionTest;
                        if (v < 0.75f)
                        {
                            atests[4] = ChallengeType.SurvivalSkillsTest;
                            atests[5] = ChallengeType.SecretKnowledgeTest;
                        }
                        else
                        {
                            atests[5] = ChallengeType.SurvivalSkillsTest;
                            atests[4] = ChallengeType.SecretKnowledgeTest;
                        }
                    }
                    break;
                }
            case Path.SecretPath:
                {
                    if (v > 0.5f)
                    {
                        atests[0] = ChallengeType.PerceptionTest;
                        atests[1] = ChallengeType.SecretKnowledgeTest;
                    }
                    else
                    {
                        atests[1] = ChallengeType.PerceptionTest;
                        atests[0] = ChallengeType.SecretKnowledgeTest;
                    }
                    v = Random.value;
                    if (v < 0.5f)
                    {
                        atests[2] = ChallengeType.PersistenceTest;
                        atests[3] = ChallengeType.IntelligenceTest;
                        if (v < 0.25f)
                        {
                            atests[4] = ChallengeType.SurvivalSkillsTest;
                            atests[5] = ChallengeType.TechSkillsTest;
                        }
                        else
                        {
                            atests[5] = ChallengeType.SurvivalSkillsTest;
                            atests[4] = ChallengeType.TechSkillsTest;
                        }
                    }
                    else
                    {
                        atests[3] = ChallengeType.PersistenceTest;
                        atests[2] = ChallengeType.IntelligenceTest;
                        if (v < 0.75f)
                        {
                            atests[4] = ChallengeType.SurvivalSkillsTest;
                            atests[5] = ChallengeType.TechSkillsTest;
                        }
                        else
                        {
                            atests[5] = ChallengeType.SurvivalSkillsTest;
                            atests[4] = ChallengeType.TechSkillsTest;
                        }
                    }
                    break;
                }
            case Path.LifePath:
            default:
                {
                    if (v > 0.5f)
                    {
                        atests[0] = ChallengeType.PersistenceTest;
                        atests[1] = ChallengeType.SurvivalSkillsTest;
                    }
                    else
                    {
                        atests[1] = ChallengeType.PersistenceTest;
                        atests[0] = ChallengeType.SurvivalSkillsTest;
                    }
                    v = Random.value;
                    if (v < 0.5f)
                    {
                        atests[2] = ChallengeType.PerceptionTest;
                        atests[3] = ChallengeType.IntelligenceTest;
                        if (v < 0.25f)
                        {
                            atests[4] = ChallengeType.SecretKnowledgeTest;
                            atests[5] = ChallengeType.TechSkillsTest;
                        }
                        else
                        {
                            atests[5] = ChallengeType.SecretKnowledgeTest;
                            atests[4] = ChallengeType.TechSkillsTest;
                        }
                    }
                    else
                    {
                        atests[3] = ChallengeType.PerceptionTest;
                        atests[2] = ChallengeType.IntelligenceTest;
                        if (v < 0.75f)
                        {
                            atests[4] = ChallengeType.SecretKnowledgeTest;
                            atests[5] = ChallengeType.TechSkillsTest;
                        }
                        else
                        {
                            atests[5] = ChallengeType.SecretKnowledgeTest;
                            atests[4] = ChallengeType.TechSkillsTest;
                        }
                    }
                    break;
                }
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (v < richness)
                {
                    df = (0.7f - 0.1f * friendliness + 0.3f * Random.value);
                    v = v / richness;
                    if (Random.value < 0.8f)
                    {
                        if (v < abilityTestChance)
                        { // abilities tests
                            v = Random.value;
                            if (v < 0.55f)
                            {
                                if (v < 0.3f) challengeArray[x, y] = new ChallengeField(atests[0], (byte)(maxDifficulty * df));
                                else challengeArray[x, y] = new ChallengeField(atests[1], (byte)(maxDifficulty * df));
                            }
                            else
                            {
                                if (v < 0.9f)
                                {
                                    if (v < 0.75f) challengeArray[x, y] = new ChallengeField(atests[2], (byte)(maxDifficulty * df));
                                    else challengeArray[x, y] = new ChallengeField(atests[3], (byte)(maxDifficulty * df));
                                }
                                else
                                {
                                    if (v < 0.97f) challengeArray[x, y] = new ChallengeField(atests[4], (byte)(maxDifficulty * df));
                                    else challengeArray[x, y] = new ChallengeField(atests[5], (byte)(maxDifficulty * df));
                                }
                            }
                        }
                        else
                        {
                            float sum = abilityTestChance + giftChance;
                            if (v < abilityTestChance + giftChance)
                            { // mysteria - changers
                                if (danger > 0.9f && mysteria > 0.5f && Random.value > 0.5f)
                                { // global changer -- INDEV
                                    challengeArray[x, y] = ChallengeField.emptyField;
                                }
                                else
                                { // local changer -- INDEV
                                    challengeArray[x, y] = ChallengeField.emptyField;
                                }
                            }
                            else
                            {
                                sum += mysteriaChance;
                                if (v < sum)
                                {
                                    //gift
                                    v = Random.value;
                                    if (v < 0.7f)
                                    {
                                        if (v < 0.5f) challengeArray[x, y] = new ChallengeField(ChallengeType.Treasure, ChallengeField.TREASURE_EXP_CODE);
                                        else challengeArray[x, y] = new ChallengeField(ChallengeType.Treasure, ChallengeField.TREASURE_MONEY_CODE);
                                    }
                                    else
                                    {
                                        if (v > 0.9f) challengeArray[x, y] = new ChallengeField(ChallengeType.Treasure, ChallengeField.TREASURE_ARTIFACT_CODE);
                                        else challengeArray[x, y] = new ChallengeField(ChallengeType.Treasure, ChallengeField.TREASURE_RESOURCES_CODE);
                                    }
                                }
                                else
                                { // puzzle parts
                                    v = (v - sum) / (1f - sum);
                                    if (v > 0.5f)
                                    {                                    
                                        //details                                        
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
                                    else
                                    { // route points                                        
                                        challengeArray[x, y] = ProgressionMaster.GetRouteBoostCell(this, mysteria);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (v < mysteria) challengeArray[x, y] = new ChallengeField(ChallengeType.Random, (byte)(maxDifficulty * df));
                        else
                        {
                            v = Random.value;
                            if (v < 0.66f)
                            {
                                if (v < 0.33f) challengeArray[x, y] = new ChallengeField(ChallengeType.CrystalFee, (byte)(15 * df));
                                else challengeArray[x, y] = new ChallengeField(ChallengeType.AscensionTest, (byte)(100 * df));
                            }
                            else challengeArray[x, y] = ChallengeField.impassableField;
                        }
                    }
                }
                else
                {
                    v = (v - richness) / (1f - richness);
                    if (v > danger & Random.value < 0.85f) challengeArray[x, y] = ChallengeField.emptyField;
                    else challengeArray[x, y] = ChallengeField.impassableField;
                }
            }
        }

        //check
        int sz = size - 1;
        challengeArray[sz, sz].ChangeChallengeType(ChallengeType.ExitTest, (byte)(maxDifficulty * (0.3f + 0.3f * friendliness + 0.4f * danger)));
        challengeArray[0, 0] = ChallengeField.emptyField;

        int prevY = 0, ystep;
        for (int i =1; i < size; i++)
        {
            v = Random.value;
            if (v <= 0.5f) ystep = 1;
            else
            {
                if (v >= 0.8f) ystep = -1;
                else ystep = 0;
            }
            prevY += ystep;
            if (prevY < 0)
            {
                if (Random.value > 0.5f) prevY = 1;
                else prevY = 0;
            }
            else
            {
                if (prevY >= size)
                {
                    if (Random.value > 0.5f) prevY = sz - 1;
                    else prevY = sz;
                }
            }
        }

        if (challengeArray[sz - 1, sz].IsImpassable() && challengeArray[sz - 1, sz - 1].IsImpassable() && challengeArray[sz, sz - 1].IsImpassable())
        {
            challengeArray[sz - 1, sz - 1] = ChallengeField.emptyField;
        }
        if (challengeArray[1, 0].IsImpassable() && challengeArray[1, 1].IsImpassable() && challengeArray[0, 1].IsImpassable())
        {
            challengeArray[1, 1] = ChallengeField.emptyField;
        }
    }

    #region save-load
    override public List<byte> Save()
    {
        var data = base.Save();        
        data.AddRange(System.BitConverter.GetBytes(richness)); // 0 - 3
        data.AddRange(System.BitConverter.GetBytes(danger)); // 4 - 7
        data.AddRange(System.BitConverter.GetBytes(mysteria)); // 8 - 11
        data.AddRange(System.BitConverter.GetBytes(friendliness)); // 12 - 15
        data.Add((byte)path); // 16
        if (challengeArray != null)
        {
            data.Add(1);
            byte size = (byte)challengeArray.GetLength(0);
            data.Add(size);
            for (byte i = 0; i < size; i++) // foreach не гарантирует точный порядок
            {
                for (byte j = 0; j < size; j++)
                {
                    data.AddRange(challengeArray[i, j].Save());
                }
            }
        }
        else data.Add(0); // 17
        return data;
    }
    public void Load(System.IO.FileStream fs)
    {
        int LENGTH = 18;
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        richness = System.BitConverter.ToSingle(data, 0);
        danger = System.BitConverter.ToSingle(data, 4);
        mysteria = System.BitConverter.ToSingle(data, 8);
        friendliness = System.BitConverter.ToSingle(data, 12);
        path = (Path)data[16];
        if (data[17] == 1)
        {
            int size = fs.ReadByte();
            challengeArray = ChallengeField.Load(fs, size);
        }
    } 
    #endregion
}