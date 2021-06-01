﻿public class ChallengeField
{
    public ChallengeType challengeType { get; private set; }
    public byte difficultyClass { get; private set; }
    public bool isHidden { get; private set; }
    public bool isPassed { get; private set; }

    public const byte MAX_DIFFICULTY = 25, TREASURE_EXP_CODE = 0, TREASURE_MONEY_CODE = 1, TREASURE_RESOURCES_CODE = 2, TREASURE_ARTIFACT_CODE = 3;

    public static ChallengeField emptyField { get { return new ChallengeField(ChallengeType.NoChallenge, 0); } }
    public static ChallengeField impassableField { get { return new ChallengeField(ChallengeType.Impassable, 0); } }

    public ChallengeField()
    {
        challengeType = ChallengeType.NoChallenge;
        difficultyClass = 0;
        isHidden = true;
        isPassed = false;
    }
    public ChallengeField(ChallengeType i_type, byte i_difficulty)
    {
        challengeType = i_type;
        difficultyClass = i_difficulty;
        isHidden = true;
        isPassed = false;
    }

    public void ChangeChallengeType(ChallengeType chtype, byte newDifficulty)
    {
        challengeType = chtype;
        difficultyClass = newDifficulty;
        isPassed = false;
        isHidden = true;
    }
    public void ChangeChallengeType(ChallengeType chtype)
    {
        challengeType = chtype;
        isPassed = false;
    }   
    public void ChangeHiddenStatus(bool x)
    {
        isHidden = x;
    }
    public void MarkAsPassed() { isPassed = true; }
    public bool IsImpassable() { return challengeType == ChallengeType.Impassable; }

    public static UnityEngine.Rect GetChallengeIconRect(ChallengeType ctype)
    {
        switch (ctype)
        {
            case ChallengeType.PersistenceTest: return UIController.GetIconUVRect(Icons.PersistenceIcon);
            case ChallengeType.SurvivalSkillsTest: return UIController.GetIconUVRect(Icons.SurvivalSkillsIcon);
            case ChallengeType.PerceptionTest: return UIController.GetIconUVRect(Icons.PerceptionIcon);
            case ChallengeType.SecretKnowledgeTest: return UIController.GetIconUVRect(Icons.SecretKnowledgeIcon);
            case ChallengeType.IntelligenceTest: return UIController.GetIconUVRect(Icons.IntelligenceIcon);
            case ChallengeType.TechSkillsTest: return UIController.GetIconUVRect(Icons.TechSkillsIcon);
            case ChallengeType.Treasure: return UIController.GetIconUVRect(Icons.TreasureIcon); ;
            case ChallengeType.QuestTest: return UIController.GetIconUVRect(Icons.QuestMarkerIcon);
            case ChallengeType.CrystalFee: return UIController.GetIconUVRect(Icons.EnergyCrystal);
            case ChallengeType.AscensionTest: return UIController.GetIconUVRect(Icons.AscensionIcon);
            case ChallengeType.Impassable:
            case ChallengeType.NoChallenge:
                return UnityEngine.Rect.zero;
            case ChallengeType.PuzzlePart: return UIController.GetIconUVRect(Icons.PuzzlePart); // colouring?
            case ChallengeType.FoundationPts: return UIController.GetIconUVRect(Icons.FoundationRoute);
            case ChallengeType.CloudWhalePts: return UIController.GetIconUVRect(Icons.CloudWhaleRoute);
            case ChallengeType.EnginePts: return UIController.GetIconUVRect(Icons.EngineRoute);
            case ChallengeType.PipesPts: return UIController.GetIconUVRect(Icons.PipesRoute);
            case ChallengeType.CrystalPts: return UIController.GetIconUVRect(Icons.CrystalRoute);
            case ChallengeType.MonumentPts: return UIController.GetIconUVRect(Icons.MonumentRoute);
            case ChallengeType.BlossomPts: return UIController.GetIconUVRect(Icons.BlossomRoute);
            case ChallengeType.PollenPts: return UIController.GetIconUVRect(Icons.PollenRoute);
            case ChallengeType.ExitTest: return UIController.GetIconUVRect(Icons.GreenArrow);
            default:
                return UIController.GetIconUVRect(Icons.Unknown);

        }
    }

    public static bool operator ==(ChallengeField A, ChallengeField B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return A.Equals(B);
    }
    public static bool operator !=(ChallengeField A, ChallengeField B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        return (int)challengeType + difficultyClass + (isHidden ? 1 : 0) + (isPassed ? 1 : 0);
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        var info = (ChallengeField)obj;
        return challengeType == info.challengeType & difficultyClass == info.difficultyClass & isHidden == info.isHidden & isPassed == info.isPassed;
    }

    #region save-load
    public byte[] Save()
    {
        return new byte[4]
        {
            (byte)challengeType,
            difficultyClass,
            isHidden ? (byte)1 : (byte)0,
            isPassed ? (byte)1 : (byte)0
        };
    }
    public static ChallengeField[,] Load(System.IO.Stream fs, int size)
    {
        var cfa = new ChallengeField[size, size];
        const int length = 4;
        var data = new byte[length];
        ChallengeField cf;
        for (var i = 0; i < size; i++)
        {
            for (var j = 0;j < size; j++)
            {
                fs.Read(data, 0, length);
                cf = new ChallengeField((ChallengeType)data[0], data[1]);
                cf.isHidden = data[2] == 1;
                cf.isPassed = data[3] == 1;
                cfa[i, j] = cf;
            }
        }
        return cfa;
    }
    #endregion
}
