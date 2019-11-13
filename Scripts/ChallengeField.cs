public sealed class ChallengeField
{
    public ChallengeType challengeType { get; private set; }
    public byte difficultyClass { get; private set; }
    public bool isHidden { get; private set; }
    public bool isPassed { get; private set; }

    public static ChallengeField emptyField, impassableField;
    public const byte MAX_DIFFICULTY = 25, TREASURE_EXP_CODE = 0, TREASURE_MONEY_CODE = 1, TREASURE_RESOURCES_CODE = 2, TREASURE_ARTIFACT_CODE = 3;

    static ChallengeField()
    {
        emptyField = new ChallengeField(ChallengeType.NoChallenge, 0);
        impassableField = new ChallengeField(ChallengeType.Impassable, MAX_DIFFICULTY);
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
            case ChallengeType.CrystalFee: return UIController.GetIconUVRect(Icons.CrewGoodIcon);
            case ChallengeType.AscensionTest: return UIController.GetIconUVRect(Icons.AscensionIcon);
            case ChallengeType.Impassable:
            case ChallengeType.NoChallenge:
                return UnityEngine.Rect.zero;
            case ChallengeType.Random:
            default:
                return UIController.GetIconUVRect(Icons.Unknown);

        }
    }
}
