using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExploringMinigameUI : MonoBehaviour
{
    public enum ChallengeType : byte { NoChallenge = 0, Impassable = 1, Random = 2, PersistenceTest, SurvivalSkillsText, PerceptionTest, SecretKnowledgeTest,
    IntelligenceTest, TechSkillsTest, Treasure,  QuestTest, CrystalFee, AscensionTest }
    //dependency : DeckField.ChangeChallengeType, FieldAction, Localization.GetChallengeLabel

    private sealed class DeckField
    {
        public bool isHidden { get; private set; }        
        public ChallengeType challengeType { get; private set; }
        public GameObject button { get; private set; }
        public byte difficultyClass { get; private set; }

        public DeckField (GameObject i_button, ChallengeType i_chType, byte i_difficultyClass, bool i_hidden)
        {
            button = i_button;
            ChangeChallengeType(i_chType, i_difficultyClass);
            isHidden = i_hidden;
        }
        private DeckField()
        {
            isHidden = false;
            button = null;
            difficultyClass = 20;
            challengeType = ChallengeType.Random;
        }

        public void ChangeChallengeType(ChallengeType chtype, byte newDifficulty)
        {
            challengeType = chtype;
            difficultyClass = newDifficulty;
            var img = button.transform.GetChild(0).GetComponent<RawImage>();
            switch (challengeType)
            {
                case ChallengeType.PersistenceTest: img.uvRect = UIController.GetIconUVRect(Icons.PersistenceIcon); break;
                case ChallengeType.SurvivalSkillsText: img.uvRect = UIController.GetIconUVRect(Icons.SurvivalSkillsIcon);break;
                case ChallengeType.PerceptionTest: img.uvRect = UIController.GetIconUVRect(Icons.PerceptionIcon); break;
                case ChallengeType.SecretKnowledgeTest: img.uvRect = UIController.GetIconUVRect(Icons.SecretKnowledgeIcon); break;
                case ChallengeType.IntelligenceTest: img.uvRect = UIController.GetIconUVRect(Icons.IntelligenceIcon); break;
                case ChallengeType.TechSkillsTest: img.uvRect = UIController.GetIconUVRect(Icons.TechSkillsIcon); break;
                case ChallengeType.Treasure: img.uvRect = UIController.GetIconUVRect(Icons.TreasureIcon); break;
                case ChallengeType.QuestTest: img.uvRect = UIController.GetIconUVRect(Icons.QuestMarkerIcon); break;
                case ChallengeType.CrystalFee: img.uvRect = UIController.GetIconUVRect(Icons.CrewGoodIcon); break;
                case ChallengeType.AscensionTest: img.uvRect = UIController.GetIconUVRect(Icons.AscensionIcon); break;
                case ChallengeType.Impassable:
                case ChallengeType.NoChallenge:
                    img.uvRect = Rect.zero; break;
                case ChallengeType.Random:
                default:
                    img.uvRect = UIController.GetIconUVRect(Icons.Unknown); break;
                
            }
        }
    }

    [SerializeField] private GameObject deckHolder;
    [SerializeField] private DeckField[] fields;
    [SerializeField] private Text expeditionStatusInfo, challengeDifficultyLabel, playerResultLabel, challengeLabel;
    [SerializeField] private RectTransform crewMarker;
    [SerializeField] private Image innerRollRing, outerRollRing;
    [SerializeField] private GameObject exampleButton, challengePanel, passButton;
    [SerializeField] private GameObject rollButton;
    private Text passText { get { return passButton.transform.GetChild(0).GetComponent<Text>(); } }

    private static ExploringMinigameUI current;

    private Expedition observingExpedition;
    //test
    private Crew crew;
    private Vector2Int crewPos;
    private byte size = 8;
    private bool moveMarker = false, rollEffect = false;
    private int selectedField = -1;
    private float fieldSize = 100f, moneyCollected = 0f;
    private float missionDifficulty = 1f, pointDifficulty = 1f, pointFriendliness = 1f, pointMysteria = 1f;
    private string testLabel = string.Empty;
    private ChallengeType[] chArray;
    private readonly Color activeFieldColor = Color.white, disabledFieldColor = new Color(1f, 0.7f, 0.7f, 1f);

    private const float MARKER_MOVE_SPEED = 5f, CHALLENGE_PANEL_CLOSING_TIME = 3f,ROLL_RINGS_OUTER_SPEED = 6f, ROLL_RINGS_INNER_SPEED = 2f, ROLL_RINGS_DISAPPEAR_SPEED = 5f;
    private const byte MAX_DIFFICULTY = 20;

    public static void ShowExpedition(Expedition e)
    {
        if (e == null || e.stage == Expedition.ExpeditionStage.Dismissed) return;
        else
        {
            if (current == null)
            {
                current = Instantiate(Resources.Load<GameObject>("UIPrefs/ExploringMinigameInterface")).GetComponent<ExploringMinigameUI>();
            }
        }
    }

    private void Start()
    {
        crew = Crew.CreateNewCrew(null, 9);
        PrepareDeck();
        crewPos = Vector2Int.zero;
    }

    private void Update()
    {
        float t = Time.deltaTime;
        if (moveMarker)
        {
            var endpos = new Vector3((crewPos.x + 0.5f - size / 2f) * fieldSize, (crewPos.y + 0.5f - size / 2f) * fieldSize, 0f);
            crewMarker.localPosition = Vector3.Lerp(crewMarker.localPosition, endpos, MARKER_MOVE_SPEED * t);
            if (crewMarker.localPosition == endpos) moveMarker = false;
        }
        if (rollEffect)
        {
            innerRollRing.fillAmount = Mathf.MoveTowards(innerRollRing.fillAmount, 0f, ROLL_RINGS_DISAPPEAR_SPEED * t);
            float x = Mathf.MoveTowards(outerRollRing.fillAmount, 0f, ROLL_RINGS_DISAPPEAR_SPEED * t);
            outerRollRing.fillAmount = x;
            if (x == 0f)
            {
                rollEffect = false;
                rollButton.SetActive(false);
                AfterRoll();
            }
        }
        if (rollButton.activeSelf)
        {
            innerRollRing.transform.Rotate(Vector3.forward, ROLL_RINGS_INNER_SPEED * t);
            outerRollRing.transform.Rotate(Vector3.back, ROLL_RINGS_OUTER_SPEED * t);
        }
    }

    private void RefreshInfo()
    {
        if (observingExpedition == null || observingExpedition.stage == Expedition.ExpeditionStage.Dismissed)
        {
            observingExpedition = null;
            expeditionStatusInfo.text = string.Empty;
            gameObject.SetActive(false);
        }
        else
        {
            var c = observingExpedition.crew;
            expeditionStatusInfo.text = '"' + c.name + "\"\n" +
                Localization.GetWord(LocalizedWord.Step) + ' ' + observingExpedition.currentStep.ToString() + '/' +
                observingExpedition.mission.stepsCount.ToString() + '\n' +
                Localization.GetWord(LocalizedWord.Stamina) + ": " + ((int)(c.stamina * 100)).ToString() + "%\n\n" +
                Localization.GetPhrase(LocalizedPhrase.CrystalsCollected) + ": " + ((int)observingExpedition.collectedMoney).ToString() + '\n' +
                Localization.GetPhrase(LocalizedPhrase.SuppliesLeft) + ": " + observingExpedition.suppliesCount.ToString();
        }
    }

    private void PrepareDeck()
    {
       // if (observingExpedition == null || observingExpedition.stage == Expedition.ExpeditionStage.Dismissed)
      //  {
      //      observingExpedition = null;
      //      gameObject.SetActive(false);
     //   }
      //  else
        {
            if (fields == null)
            {
                fields = new DeckField[0];   
            }
            challengePanel.SetActive(false);

            int sqr = size * size, len = fields.Length, i;
            FillChallengesArray(size);

            if (len < sqr)
            {
                var f2 = new DeckField[sqr];               
                for (i = 0; i < len; i++)
                {
                    f2[i] = fields[i];
                }
                fields = f2;

                GameObject g;
                Button b;

                Transform parent = deckHolder.transform;

                float difsum = missionDifficulty + pointDifficulty, posdif ,sz = size;
                for (i = 0; i < sqr - len; i++)
                {
                    g = Instantiate(exampleButton);
                    g.transform.parent = parent;
                    b = g.GetComponent<Button>();
                    int x = len + i; // must be in separate variable because of lambda expression!
                    b.onClick.AddListener(delegate { this.FieldAction(x); } );

                    posdif = (i / size) / sz * (i %size) / sz;
                    fields[x] = new DeckField(g, chArray[i], (byte)(MAX_DIFFICULTY * (difsum / 3f + Random.value * (3f - difsum) * posdif) ) , false);
                }
            }
            else
            {
                for (i = sqr; i < len; i++)
                {
                    fields[i].button.SetActive(false);
                }
            }
            float p = 1f / size;
            RectTransform rt;
            GameObject gx;
            for (i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    gx = fields[i * size + j].button;
                    rt = gx.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(j * p, i * p);
                    rt.anchorMax = new Vector2((j + 1) * p, (i + 1) * p);
                    rt.offsetMax = Vector2.zero;
                    rt.offsetMin = Vector2.zero;

                    if (chArray[i * size + j] == ChallengeType.Impassable)
                    {
                        gx.GetComponent<Image>().color = Color.gray;
                        gx.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        gx.GetComponent<Image>().color = disabledFieldColor;
                        gx.GetComponent<Button>().interactable = true;
                    }

                    gx.SetActive(true);
                }
            }

            fieldSize = deckHolder.GetComponent<RectTransform>().rect.width * p;
            crewMarker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fieldSize);
            crewMarker.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fieldSize);
            crewMarker.localPosition = new Vector3((crewPos.x + 0.5f - size / 2f) * fieldSize, (crewPos.y + 0.5f - size/2f) * fieldSize,0f);
            i = crewPos.y * size + crewPos.x;
            bool up = crewPos.y + 1 < size, down = crewPos.y - 1 >= 0, left = crewPos.x - 1 >= 0, right = crewPos.x + 1 < size;
            DeckField df;
            if (left)
            {
                df = fields[i - 1];
                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                if (up)
                {
                    df = fields[i + size - 1];
                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                }
                if (down)
                {
                    df = fields[i - size - 1];
                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                }
            }
            if (up)
            {
                df = fields[i + size];
                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
            }
            if (down)
            {
                df = fields[i - size];
                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
            }
            if (right)
            {
                df = fields[i + 1];
                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                if (up)
                {
                    df = fields[i + size + 1];
                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                }
                if (down)
                {
                    df = fields[i - size + 1];
                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                }
            }
            fields[i].button.GetComponent<Image>().color = activeFieldColor;
        }
    }
    private void FillChallengesArray(int size)
    {
        int sqr = size * size;
        chArray = new ChallengeType[sqr];
        float totalVariety = missionDifficulty + pointDifficulty + pointFriendliness + pointMysteria;
        float missionTestChance = missionDifficulty / totalVariety, environmentTestChance = pointDifficulty / totalVariety,
            giftChance = pointFriendliness / totalVariety;
        float r, s2 = missionTestChance + environmentTestChance, s3 = s2 + giftChance;
        int i;
        for (i = 0; i < sqr; i++)
        {
            if (Random.value < 0.12f + pointDifficulty * 0.2f)
            {
                chArray[i] = ChallengeType.Impassable;
            }
            else
            {
                r = Random.value;
                if (r > s2)
                {
                    if (r > s3)
                    { // mysteria
                        if (Random.value < pointMysteria)
                        {
                            if (Random.value > 0.5f) chArray[i] = ChallengeType.SecretKnowledgeTest;
                            else chArray[i] = ChallengeType.PerceptionTest;
                        }
                        else
                        {
                            if (Random.value < pointDifficulty) chArray[i] = ChallengeType.SecretKnowledgeTest;
                            else chArray[i] = ChallengeType.AscensionTest;
                        }
                    }
                    else
                    { // gift
                        if (Random.value < pointFriendliness) chArray[i] = ChallengeType.Treasure;
                        else
                        {
                            if (Random.value > pointDifficulty) chArray[i] = ChallengeType.NoChallenge;
                            else chArray[i] = ChallengeType.Random;
                        }
                    }
                }
                else
                {
                    if (r > missionTestChance)
                    { // environment
                        if (Random.value > pointDifficulty) chArray[i] = ChallengeType.PersistenceTest;
                        else
                        {
                            if (Random.value > missionDifficulty) chArray[i] = ChallengeType.SurvivalSkillsText;
                            else chArray[i] = ChallengeType.Random;
                        }
                    }
                    else
                    { // mission
                        if (Random.value > missionDifficulty)
                        {
                            if (Random.value < pointFriendliness) chArray[i] = ChallengeType.Treasure;
                            else chArray[i] = ChallengeType.IntelligenceTest;
                        }
                        else
                        {
                            if (Random.value > 0.5f) chArray[i] = ChallengeType.IntelligenceTest;
                            else chArray[i] = ChallengeType.TechSkillsTest;
                        }
                    }
                }
            }
        }
        chArray[sqr - 1] = ChallengeType.QuestTest;
        chArray[0] = ChallengeType.NoChallenge;

        //check
        if (chArray[sqr - 2] == ChallengeType.Impassable && chArray[sqr - size - 2] == ChallengeType.Impassable && chArray[sqr - size - 1] == ChallengeType.Impassable)
        {
            chArray[sqr - size - 2] = ChallengeType.NoChallenge;
        }
        if (chArray[1] == ChallengeType.Impassable && chArray[size] == ChallengeType.Impassable && chArray[size + 1] == ChallengeType.Impassable)
        {
            chArray[size + 1] = ChallengeType.NoChallenge;
        }
    }

    public void FieldAction(int i)
    {
        int ypos = i / size, xpos = i % size;
        if (xpos != crewPos.x || ypos != crewPos.y)
        {
            int xdelta = xpos - crewPos.x, ydelta = ypos - crewPos.y;
            if (Mathf.Abs(xdelta) < 2 && Mathf.Abs(ydelta) < 2)
            {
                    selectedField = i;
                    var df = fields[i];
                    bool useChallengePanel = false, useRollSystem = false;
                    switch (df.challengeType)
                    {
                        case ChallengeType.NoChallenge:
                            useChallengePanel = false;
                            MoveCrewToField(selectedField);
                            break;
                        case ChallengeType.Random:
                            break;

                        case ChallengeType.PersistenceTest:
                            useChallengePanel = true;
                            challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.PersistenceTest);
                            useRollSystem = true;
                            break;
                        case ChallengeType.SurvivalSkillsText:
                            useChallengePanel = true;
                            challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.SurvivalSkillsText);
                            useRollSystem = true;
                            break;
                        case ChallengeType.PerceptionTest:
                            useChallengePanel = true;
                            challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.PerceptionTest);
                            useRollSystem = true;
                            break;
                        case ChallengeType.SecretKnowledgeTest:
                            useChallengePanel = true;
                            challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.SecretKnowledgeTest);
                            useRollSystem = true;
                            break;
                        case ChallengeType.IntelligenceTest:
                            useChallengePanel = true;
                            challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.IntelligenceTest);
                            useRollSystem = true;
                            break;
                        case ChallengeType.TechSkillsTest:
                            useChallengePanel = true;
                            challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.TechSkillsTest);
                            useRollSystem = true;
                            break;

                        case ChallengeType.Treasure:
                            useChallengePanel = true;
                            break;
                        case ChallengeType.QuestTest:
                            useChallengePanel = true;
                            useRollSystem = true;
                            break;
                        case ChallengeType.CrystalFee:
                            useChallengePanel = true;
                            challengeDifficultyLabel.text = df.difficultyClass.ToString();
                            playerResultLabel.text = ((int)moneyCollected).ToString();
                            playerResultLabel.enabled = true;
                            rollButton.SetActive(false);
                            passButton.SetActive(true);
                            break;
                        case ChallengeType.AscensionTest:
                            useChallengePanel = true;
                            challengeDifficultyLabel.text = ((int)(df.difficultyClass / 255f * 100f)).ToString() + '%';
                            playerResultLabel.text = ((int)(GameMaster.realMaster.globalMap.ascension * 100f)).ToString() + '%';
                            playerResultLabel.enabled = true;
                            rollButton.SetActive(false);
                            passButton.SetActive(true);
                            break;
                        default:
                            useChallengePanel = false;
                            passButton.SetActive(false);
                            break;
                    }
                    if (useRollSystem)
                    {
                        challengeDifficultyLabel.text = df.difficultyClass.ToString();
                        playerResultLabel.enabled = false;
                        rollButton.SetActive(true);
                        passButton.SetActive(false);
                    }
                    if (useChallengePanel == true)
                    {
                        outerRollRing.fillAmount = 1f;
                        innerRollRing.fillAmount = 1f;
                        challengePanel.transform.SetAsLastSibling();
                        challengePanel.SetActive(true);
                    }
                    else
                    {
                        challengePanel.SetActive(false);
                    }
            }
        }
                      
    }
    public void Roll()
    {
        if (rollEffect) return;
        else
        {
            rollEffect = true;
            if (GameMaster.soundEnabled)
            {
                GameMaster.audiomaster.MakeSoundEffect(SoundEffect.DicesRoll);
            }
        }
    }
    public void Pass()
    {
        if (selectedField > 0 && selectedField < fields.Length)
        {
            MoveCrewToField(selectedField);
        }
        else passButton.SetActive(false);
        challengePanel.SetActive(false);
    }
    private void AfterRoll()
    {
        if (selectedField < 0 | selectedField > fields.Length)
        {
            challengePanel.SetActive(false);
            return;
        }
        else
        {
            var df = fields[selectedField];
            float result = 0f;
            switch (df.challengeType)
            {
                case ChallengeType.PersistenceTest:
                    {                        
                        result = crew.PersistenceRoll();
                        break;
                    }
                case ChallengeType.SurvivalSkillsText:
                    {
                        result = crew.SurvivalSkillsRoll();
                        break;
                    }
                case ChallengeType.PerceptionTest:
                    {
                        result = crew.PerceptionRoll();
                        break;
                    }
                case ChallengeType.SecretKnowledgeTest:
                    {
                        result = crew.SecretKnowledgeRoll();
                        break;
                    }
                case ChallengeType.IntelligenceTest:
                    {
                        result = crew.IntelligenceRoll();
                        break;
                    }
                case ChallengeType.TechSkillsTest:
                    {
                        result = crew.TechSkillsRoll();
                        break;
                    }
            }
            playerResultLabel.text = ((int)result).ToString();
            if (result >= df.difficultyClass)
            {
                if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.SuccessfulRoll);
                fields[selectedField].ChangeChallengeType(ChallengeType.NoChallenge, 0);
                crew.RaiseAdaptability(0.5f);
                crew.AddExperience(5f);
                passText.text = Localization.GetWord(LocalizedWord.Continue);                
            }
            else
            {
                if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.RollFail);
                crew.RaiseAdaptability(0.25f);
                passText.text = Localization.GetWord(LocalizedWord.Return);
                selectedField = -1;
            }
            playerResultLabel.enabled = true;
            passButton.SetActive(true);
        }
    }

    private void MoveCrewToField(int i)
    {
        fieldSize = deckHolder.GetComponent<RectTransform>().rect.width * 1f / size;
        int ypos = i / size, xpos = i % size;        
        if (xpos != crewPos.x || ypos != crewPos.y)
        {
            int xdelta = xpos - crewPos.x, ydelta = ypos - crewPos.y;
            if (Mathf.Abs(xdelta) < 2 && Mathf.Abs(ydelta) < 2)
            {
                DeckField df;
                int cindex = crewPos.y * size + crewPos.x;
                if (xdelta == 0)
                {
                    bool right = crewPos.x + 1 < size, left = crewPos.x - 1 >= 0;
                    if (ydelta > 0) // (0,1)
                    {
                        if (crewPos.y - 1 >= 0)
                        {
                            df = fields[cindex - size];
                            if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                            if (left)
                            {
                                df = fields[cindex - size - 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                            }
                            if (right)
                            {
                                df = fields[cindex - size + 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                            }
                        }
                        if (crewPos.y + 2 < size)
                        {
                            df = fields[cindex + 2 * size];
                            if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                            if (left)
                            {
                                df = fields[cindex + 2 * size - 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                            }
                            if (right)
                            {
                                df = fields[cindex + 2 * size + 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                            }
                        }
                    }
                    else //(0,-1)
                    {
                        if (crewPos.y + 1 < size)
                        {
                            df = fields[cindex + size];
                            if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                            if (left)
                            {
                                df = fields[cindex + size - 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                            }
                            if (right)
                            {
                                df = fields[cindex + size + 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                            }
                        }
                        if (crewPos.y - 2 >= 0)
                        {
                            df = fields[cindex - 2 * size];
                            if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                            if (left)
                            {
                                df = fields[cindex - 2 * size - 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                            }
                            if (right)
                            {
                                df = fields[cindex - 2 * size + 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                            }
                        }
                    }
                }
                else
                {
                    if (ydelta == 0)
                    {
                        bool up = crewPos.y + 1 < size, down = crewPos.y - 1 >= 0;
                        if (xdelta > 0) // (1,0)
                        {
                            if (crewPos.x - 1 >= 0)
                            {
                                df = fields[cindex - 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                if (up)
                                {
                                    df = fields[cindex + size - 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                }
                                if (down)
                                {
                                    df = fields[cindex - 1 - size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                }
                            }
                            if (crewPos.x + 2 < size)
                            {
                                df = fields[cindex + 2];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                if (up)
                                {
                                    df = fields[cindex + size + 2];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                }
                                if (down)
                                {
                                    df = fields[cindex - size + 2];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                }
                            }
                        }
                        else // (-1,0)
                        {
                            if (crewPos.x + 1 < size)
                            {
                                df = fields[cindex + 1];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                if (up)
                                {
                                    df = fields[cindex + size + 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                }
                                if (down)
                                {
                                    df = fields[cindex - size + 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                }
                            }
                            if (crewPos.x - 2 >= 0)
                            {
                                df = fields[cindex - 2];
                                if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                if (up)
                                {
                                    df = fields[cindex - 2 + size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                }
                                if (down)
                                {
                                    df = fields[cindex - 2 - size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool up = crewPos.y + 1 < size, down = crewPos.y - 1 >= 0,
                            right = crewPos.x + 1 < size, left = crewPos.x - 1 >= 0;
                        if (ydelta > 0)
                        {
                            bool up2 = crewPos.y + 2 < size;
                            if (xdelta > 0) // (1,1)
                            {
                                if (left)
                                {
                                    df = fields[cindex - 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (up)
                                    {
                                        df = fields[cindex + size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                    if (down)
                                    {
                                        df = fields[cindex - size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                if (down)
                                {
                                    df = fields[cindex - size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (right)
                                    {
                                        df = fields[cindex - size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                //
                                bool right2 = crewPos.x + 2 < size;
                                if (up2)
                                {
                                    df = fields[cindex + 2 * size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (right)
                                    {
                                        df = fields[cindex + 2 * size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                    if (right2)
                                    {
                                        df = fields[cindex + 2 * size + 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                                if (right2)
                                {
                                    df = fields[cindex + 2];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (up)
                                    {
                                        df = fields[cindex + size + 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                            }
                            else // (-1,1)
                            {
                                if (right)
                                {
                                    df = fields[cindex + 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (up)
                                    {
                                        df = fields[cindex + size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                    if (down)
                                    {
                                        df = fields[cindex - size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                if (down)
                                {
                                    df = fields[cindex - size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (left)
                                    {
                                        df = fields[cindex - size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                //
                                bool left2 = crewPos.x - 2 >= 0;
                                if (up2)
                                {
                                    df = fields[cindex + 2 * size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (left)
                                    {
                                        df = fields[cindex + 2 * size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                    if (left2)
                                    {
                                        df = fields[cindex + 2 * size - 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                                if (left2)
                                {
                                    df = fields[cindex - 2];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (up)
                                    {
                                        df = fields[cindex + size - 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool down2 = crewPos.y - 2 >= 0;
                            if (xdelta > 0) // (1,-1)
                            {
                                if (up)
                                {
                                    df = fields[cindex + size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (right)
                                    {
                                        df = fields[cindex + size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                    if (left)
                                    {
                                        df = fields[cindex + size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                if (left)
                                {
                                    df = fields[cindex - 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (down)
                                    {
                                        df = fields[cindex - size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                //
                                if (crewPos.x + 2 < size)
                                {
                                    df = fields[cindex + 2];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (down)
                                    {
                                        df = fields[cindex - size + 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                    if (down2)
                                    {
                                        df = fields[cindex - 2 * size + 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                                if (down2)
                                {
                                    df = fields[cindex - 2 * size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (right)
                                    {
                                        df = fields[cindex - 2 * size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                            }
                            else //(-1,-1)
                            {
                                if (up)
                                {
                                    df = fields[cindex + size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (left)
                                    {
                                        df = fields[cindex + size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                    if (right)
                                    {
                                        df = fields[cindex + size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                if (right)
                                {
                                    df = fields[cindex + 1];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    if (down)
                                    {
                                        df = fields[cindex - size + 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = disabledFieldColor;
                                    }
                                }
                                //
                                if (crewPos.x - 2 >= 0)
                                {
                                    df = fields[cindex - 2];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (down)
                                    {
                                        df = fields[cindex - size - 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                    if (down2)
                                    {
                                        df = fields[cindex - 2 * size - 2];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                                if (down2)
                                {
                                    df = fields[cindex - 2 * size];
                                    if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    if (left)
                                    {
                                        df = fields[cindex - 2 * size - 1];
                                        if (df.challengeType != ChallengeType.Impassable) df.button.GetComponent<Image>().color = activeFieldColor;
                                    }
                                }
                            }
                        }
                    }
                }
                crewPos = new Vector2Int(xpos, ypos);
                moveMarker = true;
            }
        }
    }

    public void OpenCrewPanel()
    {
        if (crew != null)
        {
            crew.ShowOnGUI(new Rect(0f,0f, Screen.width / 2f, Screen.height / 2f), SpriteAlignment.TopRight, true);
        }
    }

}


