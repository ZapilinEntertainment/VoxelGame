using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ChallengeType : byte
{
    NoChallenge = 0, Impassable = 1, Random = 2, PersistenceTest, SurvivalSkillsTest, PerceptionTest, SecretKnowledgeTest,
    IntelligenceTest, TechSkillsTest, Treasure, QuestTest, CrystalFee, AscensionTest, PuzzlePart, FoundationPts, CloudWhalePts,
    EnginePts, PipesPts, CrystalPts, MonumentPts, BlossomPts, PollenPts, ExitTest
}
//dependency : ChallengeField.GetChallengeIconRect, Localization.GetChallengeLabel
//ExploringMinigameUI : FieldAction(), Pass()


public sealed class ExploringMinigameUI : MonoBehaviour
{   

    [SerializeField] private GameObject deckHolder;
    [SerializeField] private Text challengeDifficultyLabel, playerResultLabel, challengeLabel, rollText, suppliesLabel, crystalsLabel, staminaPercentage;
    [SerializeField] private Transform infoPanel;
    [SerializeField] private RectTransform crewMarker;
    [SerializeField] private Image innerRollRing, outerRollRing, staminaBar;
    [SerializeField] private GameObject exampleButton, challengePanel, passButton;
    [SerializeField] private GameObject rollButton;
    private Text passText { get { return passButton.transform.GetChild(0).GetComponent<Text>(); } }
    private Transform crewPanel { get { return infoPanel.GetChild(0); } }
    private Transform membersPanel { get { return crewPanel.GetChild(2); } }

    private static ExploringMinigameUI current;
    public static bool minigameActive { get; private set; }

    private Expedition observingExpedition;
    private Crew observingCrew;
    private PointOfInterest observingPoint;
    //
    private byte size = 8;
    private bool moveMarker = false, rollEffect = false, canPassThrough = true, needInfoRefreshing = false, launchedFromMap = false;
    private int selectedField = -1;
    private float fieldSize = 100f, pingpongVal = 0f;
    private string testLabel = string.Empty;
    private GameObject[] buttons;

    private readonly Color activeFieldColor = Color.white, disabledFieldColor = new Color(1f, 0.7f, 0.7f, 1f),
        reachableIconColor = Color.white, unreachableIconColor = Color.gray;

    private const float MARKER_MOVE_SPEED = 5f, CHALLENGE_PANEL_CLOSING_TIME = 3f, ROLL_RINGS_OUTER_SPEED = 6f, ROLL_RINGS_INNER_SPEED = 2f, ROLL_RINGS_DISAPPEAR_SPEED = 5f,
        STAMINA_PER_STEP = 0.01f;
    private const byte MAX_DIFFICULTY = 25;

    public static void ShowExpedition(Expedition e, bool isLaunchedFromMap)
    {        
        if (e == null) return;
        else
        {
            if (e.stage == Expedition.ExpeditionStage.OnMission)
            {
                if (current == null)
                {
                    current = Instantiate(Resources.Load<GameObject>("UIPrefs/ExploringMinigameInterface")).GetComponent<ExploringMinigameUI>();
                }
                if (!current.gameObject.activeSelf) current.gameObject.SetActive(true);
                current.launchedFromMap = isLaunchedFromMap;
                current.Show(e);
            }
            else return;
        }
    }
    public static void ActivateIfEnabled()
    {
        if (minigameActive)
        {
            current.EnableDeckHolder();
        }
    }
    public static void Disable()
    {
        if (current != null) current.gameObject.SetActive(false);
    }

    private void Awake()
    {
        RectTransform rt = deckHolder.GetComponent<RectTransform>();
        float height = Screen.height;
        if (rt.rect.width < height) height = rt.rect.width;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rt.anchoredPosition = new Vector3(-height / 2f, 0f, 0f);

        infoPanel.gameObject.SetActive(false);
        infoPanel.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.StopMission);
        var mempanel = membersPanel;
        rt = mempanel.GetComponent<RectTransform>();
        float p = rt.rect.width / (float)Crew.MAX_MEMBER_COUNT;
        if (p > rt.rect.height) p = rt.rect.height;
        float ax = p / rt.rect.width, ay = p / rt.rect.height;
        rt = mempanel.GetChild(0).GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(ax * p, ay);
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        var g = rt.gameObject;
        if (Crew.MAX_MEMBER_COUNT > 1)
        {
            for (int i = 1; i > Crew.MAX_MEMBER_COUNT; i++)
            {
                rt = Instantiate(g, mempanel).GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(ax * i, 0);
                rt.anchorMax = new Vector2(ax * (i + 1) * p, ay);
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
            }
        }

        var t = suppliesLabel.transform.parent;
        t.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(ResourceType.SUPPLIES_ID);
        t.GetChild(2).GetComponent<RawImage>().uvRect = UIController.GetIconUVRect(Icons.EnergyCrystal);
        infoPanel.gameObject.SetActive(true);
    }
    private void Show(Expedition e)
    {
        observingExpedition = e;
        observingCrew = e.crew;
        observingPoint = observingExpedition.destination;
        size = (byte)observingPoint.GetChallengesArraySize();        
        PrepareDeck();
        RefreshInfo();
        infoPanel.GetChild(0).GetChild(1).GetComponent<Text>().text = '"' + observingCrew.name + '"';
    }
    private void PrepareDeck()
    {
        if (buttons == null) buttons = new GameObject[0];
        challengePanel.SetActive(false);

        int sqr = size * size, len = buttons.Length, ypos;

        if (len < sqr)
        {
            if (len != 0)
            {
                var b2 = new GameObject[sqr];
                for (ypos = 0; ypos < len; ypos++)
                {
                    b2[ypos] = buttons[ypos];
                }
                buttons = b2;
            }
            else
            {
                buttons = new GameObject[sqr];
            }

            GameObject g;
            Button b;

            Transform parent = deckHolder.transform;

            for (ypos = 0; ypos < sqr - len; ypos++)
            {
                g = Instantiate(exampleButton);
                g.transform.parent = parent;
                b = g.GetComponent<Button>();
                int v = len + ypos; // must be in separate variable because of lambda expression!
                b.onClick.AddListener(delegate { this.FieldAction(v); });
                buttons[v] = g;
            }
        }
        else
        {
            for (ypos = sqr; ypos < len; ypos++)
            {
                buttons[ypos].SetActive(false);
            }
        }
        float p = 1f / size;
        RectTransform rt;
        GameObject gx;
        ChallengeField cf;
        bool impassable;
        RawImage img;
        var chfields = observingPoint.GetChallengesArrayRef();
        for (ypos = 0; ypos < size; ypos++)
        {
            for (int xpos = 0; xpos < size; xpos++)
            {
                gx = buttons[ypos * size + xpos];
                rt = gx.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(xpos * p, ypos * p);
                rt.anchorMax = new Vector2((xpos + 1) * p, (ypos + 1) * p);
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;

                cf = chfields[xpos, ypos];
                impassable = cf.IsImpassable();
                if (impassable)
                {
                    gx.GetComponent<Image>().color = Color.gray;
                    gx.GetComponent<Button>().interactable = false;
                }
                else
                {
                    gx.GetComponent<Image>().color = disabledFieldColor;
                    gx.GetComponent<Button>().interactable = true;
                }
                //#change button icon
                img = gx.transform.GetChild(0).GetComponent<RawImage>();
                if (cf.isPassed || impassable)
                {
                    img.enabled = false;
                }
                else
                {
                    if (cf.isHidden)
                    {
                        img.uvRect = UIController.GetIconUVRect(Icons.Unknown);
                        img.color = unreachableIconColor;
                        img.enabled = true;
                    }
                    else
                    {
                        if (cf.challengeType != ChallengeType.NoChallenge)
                        {
                            img.uvRect = ChallengeField.GetChallengeIconRect(cf.challengeType);
                            if (cf.challengeType != ChallengeType.PuzzlePart) img.color = reachableIconColor;
                            else img.color = Knowledge.colors[cf.difficultyClass];
                            img.enabled = true;
                        }
                        else
                        {
                            img.enabled = false;
                        }
                    }

                }
                //
                gx.SetActive(true);
            }
        }

        fieldSize = deckHolder.GetComponent<RectTransform>().rect.width * p;
        crewMarker.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fieldSize);
        crewMarker.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fieldSize);
        var cpos = observingExpedition.GetPlanPos();
        int x = cpos.x, y = cpos.y;
        crewMarker.localPosition = new Vector3((x + 0.5f - size / 2f) * fieldSize, (y + 0.5f - size / 2f) * fieldSize, 0f);
        int cindex = y * size + x;
        bool up = y + 1 < size, down = y - 1 >= 0, left = x - 1 >= 0, right = x + 1 < size;

        if (left)
        {
            cf = chfields[x - 1, y];
            if (!cf.IsImpassable())
            {
                buttons[cindex - 1].GetComponent<Image>().color = activeFieldColor;
                if (cf.isHidden)
                {
                    cf.ChangeHiddenStatus(false);
                    RedrawButtonIcon(cindex - 1);
                }
            }
            if (up)
            {
                cf = chfields[x - 1, y + 1];
                if (!cf.IsImpassable())
                {
                    buttons[cindex + size - 1].GetComponent<Image>().color = activeFieldColor;
                    if (cf.isHidden)
                    {
                        cf.ChangeHiddenStatus(false);
                        RedrawButtonIcon(cindex + size - 1);
                    }
                }
            }
            if (down)
            {
                cf = chfields[x - 1, y - 1];
                if (!cf.IsImpassable())
                {
                    buttons[cindex - size - 1].GetComponent<Image>().color = activeFieldColor;
                    if (cf.isHidden)
                    {
                        cf.ChangeHiddenStatus(false);
                        RedrawButtonIcon(cindex - size - 1);
                    }
                }
            }
        }
        if (up)
        {
            cf = chfields[x, y + 1];
            if (!cf.IsImpassable())
            {
                buttons[cindex + size].GetComponent<Image>().color = activeFieldColor;
                if (cf.isHidden)
                {
                    cf.ChangeHiddenStatus(false);
                    RedrawButtonIcon(cindex + size);
                }
            }
        }
        if (down)
        {
            cf = chfields[x, y - 1];
            if (!cf.IsImpassable())
            {
                buttons[cindex - size].GetComponent<Image>().color = activeFieldColor;
                if (cf.isHidden)
                {
                    cf.ChangeHiddenStatus(false);
                    RedrawButtonIcon(cindex - size);
                }
            }
        }
        if (right)
        {
            cf = chfields[x + 1, y];
            if (!cf.IsImpassable())
            {
                buttons[cindex + 1].GetComponent<Image>().color = activeFieldColor;
                if (cf.isHidden)
                {
                    cf.ChangeHiddenStatus(false);
                    RedrawButtonIcon(cindex + 1);
                }
            }
            if (up)
            {
                cf = chfields[x + 1, y + 1];
                if (!cf.IsImpassable())
                {
                    buttons[cindex + size + 1].GetComponent<Image>().color = activeFieldColor;
                    if (cf.isHidden)
                    {
                        cf.ChangeHiddenStatus(false);
                        RedrawButtonIcon(cindex + size + 1);
                    }
                }
            }
            if (down)
            {
                cf = chfields[x + 1, y - 1];
                if (!cf.IsImpassable())
                {
                    buttons[cindex - size + 1].GetComponent<Image>().color = activeFieldColor;
                    if (cf.isHidden)
                    {
                        cf.ChangeHiddenStatus(false);
                        RedrawButtonIcon(cindex - size + 1);
                    }
                }
            }
        }
        cf = chfields[x, y];
        buttons[cindex].GetComponent<Image>().color = activeFieldColor;
        cf.ChangeHiddenStatus(false);
        cf.MarkAsPassed();
        RedrawButtonIcon(cindex);
    }

    private void Update()
    {
        float t = Time.deltaTime;
        if (moveMarker)
        {
            var cpos = observingExpedition.GetPlanPos();
            var endpos = new Vector3((cpos.x + 0.5f - size / 2f) * fieldSize, (cpos.y + 0.5f - size / 2f) * fieldSize, 0f);
            crewMarker.localPosition = Vector3.Lerp(crewMarker.localPosition, endpos, MARKER_MOVE_SPEED * t);
            if (crewMarker.localPosition == endpos)
            {
                moveMarker = false;                
            }
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
            pingpongVal = Mathf.PingPong(pingpongVal, 1f);
            rollText.transform.localScale = Vector3.one * (1f + pingpongVal * 0.5f);
        }
        if (needInfoRefreshing) RefreshInfo();
    }

    private void RefreshInfo()
    {
        if (observingExpedition == null | observingExpedition.stage == Expedition.ExpeditionStage.Dismissed)
        {
            observingExpedition = null;

            gameObject.SetActive(false);
        }
       else
        {
            var cpanel = crewPanel;
            cpanel.gameObject.SetActive(false);
            /*
            var m = membersPanel;           
            int c = m.childCount;
            if (observingCrew.membersCount == Crew.MAX_MEMBER_COUNT)
            {
                for (int i = 0; i < c; i++)
                {
                    m.GetChild(i).gameObject.SetActive(true);
                }
            }
            else
            {
                if (observingCrew.membersCount == 0) return;
                int i = 0;
                for (; i < observingCrew.membersCount; i++)
                {
                    m.GetChild(i).gameObject.SetActive(true);
                }
                for (; i< c; i++)
                {
                    m.GetChild(i).gameObject.SetActive(false);
                }
            }
            */

            staminaBar.fillAmount = observingCrew.stamina;
            staminaPercentage.text = ((int)(observingCrew.stamina * 100f)).ToString() + '%';
            suppliesLabel.text = observingExpedition.suppliesCount.ToString();
            crystalsLabel.text = observingExpedition.crystalsCollected.ToString();

            cpanel.gameObject.SetActive(true);
        }
        needInfoRefreshing = false;
    }   

    public void FieldAction(int i)
    {
        int ypos = i / size, xpos = i % size;
        var cpos = observingExpedition.GetPlanPos();
        if (xpos != cpos.x || ypos != cpos.y)
        {
            int xdelta = xpos - cpos.x, ydelta = ypos - cpos.y;
            if (Mathf.Abs(xdelta) < 2 && Mathf.Abs(ydelta) < 2)
            {
                selectedField = i;
                var cf = observingPoint.GetChallengeField(xpos, ypos);
                TYPE_SWITCH:
                bool useChallengePanel = false, useRollSystem = false;                
                switch (cf.challengeType)
                {
                    case ChallengeType.NoChallenge:
                        MoveCrewToField(selectedField);
                        break;
                    case ChallengeType.PuzzlePart:
                    case ChallengeType.FoundationPts:
                    case ChallengeType.CloudWhalePts:
                    case ChallengeType.EnginePts:
                    case ChallengeType.PipesPts:
                    case ChallengeType.CrystalPts:
                    case ChallengeType.MonumentPts:
                    case ChallengeType.BlossomPts:
                    case ChallengeType.PollenPts:
                    case ChallengeType.Treasure:
                        canPassThrough = true;
                        Pass();
                        // + эффекты
                        break;
                    case ChallengeType.Random:
                        int x = Random.Range(0, 10);
                        var newtype = ChallengeType.NoChallenge;
                        switch (x)
                        {
                            case 0:
                                newtype = ChallengeType.PersistenceTest;
                                break;
                            case 1:
                                newtype = ChallengeType.SurvivalSkillsTest;
                                break;
                            case 2:
                                newtype = ChallengeType.PerceptionTest;
                                break;
                            case 3:
                                newtype = ChallengeType.SecretKnowledgeTest;
                                break;
                            case 4:
                                newtype = ChallengeType.TechSkillsTest;
                                break;
                            case 5:
                                newtype = ChallengeType.IntelligenceTest;
                                break;
                            case 6:
                                newtype = ChallengeType.CrystalFee;
                                break;
                            case 7:
                                newtype = ChallengeType.AscensionTest;
                                break;
                            case 8:
                                newtype = ChallengeType.Treasure;
                                break;
                        }
                        cf.ChangeChallengeType(newtype); RedrawButtonIcon(i);
                        if (cf.challengeType != ChallengeType.Random) goto TYPE_SWITCH;
                        break;

                    case ChallengeType.PersistenceTest:
                        useChallengePanel = true;
                        challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.PersistenceTest);
                        useRollSystem = true;
                        break;
                    case ChallengeType.SurvivalSkillsTest:
                        useChallengePanel = true;
                        challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.SurvivalSkillsTest);
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
                    case ChallengeType.QuestTest:
                        challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.QuestTest);
                        useChallengePanel = true;
                        useRollSystem = true;
                        break;
                    case ChallengeType.ExitTest:
                        challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.ExitTest);
                        useChallengePanel = true;
                        useRollSystem = true;
                        break;
                    case ChallengeType.CrystalFee:
                        useChallengePanel = true;
                        challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.CrystalFee);
                        challengeDifficultyLabel.text = cf.difficultyClass.ToString();
                        var crystalsCollected = observingExpedition.crystalsCollected;
                        playerResultLabel.text = crystalsCollected.ToString();
                        playerResultLabel.enabled = true;

                        if (crystalsCollected >= cf.difficultyClass)
                        {
                            passText.text = Localization.GetWord(LocalizedWord.Pass);
                            canPassThrough = true;
                        }
                        else
                        {
                            passText.text = Localization.GetWord(LocalizedWord.Return);
                            canPassThrough = false;
                        }
                        break;
                    case ChallengeType.AscensionTest:
                        useChallengePanel = true;
                        challengeLabel.text = Localization.GetChallengeLabel(ChallengeType.AscensionTest);
                        float a = GameMaster.realMaster.globalMap.ascension , b = cf.difficultyClass / 255f;
                        challengeDifficultyLabel.text = ((int)(b * 100f)).ToString() + '%';
                        playerResultLabel.text = ((int)(a * 100f)).ToString() + '%';
                        playerResultLabel.enabled = true;

                        if (a >= b)
                        {
                            passText.text = Localization.GetWord(LocalizedWord.Pass);
                            canPassThrough = true;
                        }
                        else
                        {
                            passText.text = Localization.GetWord(LocalizedWord.Return);
                            canPassThrough = false;
                        }
                        break;                    
                    default:
                        useChallengePanel = false;
                        passButton.SetActive(false);
                        break;
                }
                
                if (useChallengePanel == true)
                {
                    if (useRollSystem)
                    {
                        challengeDifficultyLabel.text = cf.difficultyClass.ToString();
                        playerResultLabel.enabled = false;
                        outerRollRing.fillAmount = 1f;
                        innerRollRing.fillAmount = 1f;
                        rollText.text = Localization.GetWord(LocalizedWord.Roll);
                        rollButton.SetActive(true);
                        passButton.SetActive(false);
                    }
                    else
                    {
                        passButton.SetActive(true);
                        rollButton.SetActive(false);
                    }

                    challengePanel.transform.SetAsLastSibling();
                    challengePanel.SetActive(true);
                }
                else
                {
                    challengePanel.SetActive(false);
                }
                if (cf.isHidden)
                {
                    cf.ChangeHiddenStatus(false);
                    RedrawButtonIcon(i);
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
        if (canPassThrough)
        {
            MoveCrewToField(selectedField);

            var cf = observingPoint.GetChallengeField(selectedField);
            switch (cf.challengeType)
            {
                case ChallengeType.CrystalFee:
                    observingExpedition.PayFee(cf.difficultyClass);
                    needInfoRefreshing = true;
                    break;
                case ChallengeType.Treasure:
                    observingExpedition.AddCrystals(cf.difficultyClass);
                    needInfoRefreshing = true;
                    break;
                case ChallengeType.PuzzlePart:
                    Knowledge.GetCurrent().AddPuzzlePart(cf.difficultyClass);
                    break;
                case ChallengeType.FoundationPts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Foundation, cf.difficultyClass);
                    break;
                case ChallengeType.CloudWhalePts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.CloudWhale, cf.difficultyClass);
                    break;
                case ChallengeType.EnginePts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Engine, cf.difficultyClass);
                    break;
                case ChallengeType.PipesPts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Pipes, cf.difficultyClass);
                    break;
                case ChallengeType.CrystalPts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Crystal, cf.difficultyClass);
                    break;
                case ChallengeType.MonumentPts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Monument, cf.difficultyClass);
                    break;
                case ChallengeType.BlossomPts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Blossom, cf.difficultyClass);
                    break;
                case ChallengeType.PollenPts:
                    Knowledge.GetCurrent().AddResearchPoints(Knowledge.ResearchRoute.Pollen, cf.difficultyClass);
                    break;
                case ChallengeType.ExitTest:
                    if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.LocationSuccessExit);
                    observingExpedition.CountMissionAsSuccess();
                    StopMissionButton(false);
                    return;
                default:
                    if (!cf.isPassed)
                    {
                        observingPoint.OneStepReward(observingExpedition);
                        needInfoRefreshing = true;
                    }
                    break;
            }
            if (cf.challengeType != ChallengeType.NoChallenge)
            {
                if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.SuccessfulRoll);
                cf.ChangeChallengeType(ChallengeType.NoChallenge, 0);
            }
            cf.MarkAsPassed();
            RedrawButtonIcon(selectedField);        
        }
        else passButton.SetActive(false);
        challengePanel.SetActive(false);
    }
    private void AfterRoll()
    {
        if (selectedField < 0 | selectedField > buttons.Length)
        {
            challengePanel.SetActive(false);
            return;
        }
        else
        {            

            var cf = observingPoint.GetChallengeField(selectedField);
            float result = 0f;
            switch (cf.challengeType)
            {
                case ChallengeType.PersistenceTest:
                    {
                        result = observingCrew.PersistenceRoll();
                        break;
                    }
                case ChallengeType.SurvivalSkillsTest:
                    {
                        result = observingCrew.SurvivalSkillsRoll();
                        break;
                    }
                case ChallengeType.PerceptionTest:
                    {
                        result = observingCrew.PerceptionRoll();
                        break;
                    }
                case ChallengeType.SecretKnowledgeTest:
                    {
                        result = observingCrew.SecretKnowledgeRoll();
                        break;
                    }
                case ChallengeType.IntelligenceTest:
                    {
                        result = observingCrew.IntelligenceRoll();
                        break;
                    }
                case ChallengeType.TechSkillsTest:
                    {
                        result = observingCrew.TechSkillsRoll();
                        break;
                    }
                case ChallengeType.ExitTest:
                    {
                        switch (observingCrew.exploringPath)
                        {
                            case Path.TechPath: result = (observingCrew.IntelligenceRoll() + observingCrew.TechSkillsRoll())/2f; break;
                            case Path.SecretPath: result = (observingCrew.SecretKnowledgeRoll() + observingCrew.PerceptionRoll()) / 2f; break;
                            default: result = (observingCrew.SurvivalSkillsRoll() + observingCrew.PersistenceRoll()) / 2f; break;
                        }
                        break;
                    }
            }
            
            if (result >= cf.difficultyClass)
            {
                if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.SuccessfulRoll);
                if (cf.challengeType != ChallengeType.ExitTest) cf.ChangeChallengeType(ChallengeType.NoChallenge, 0);
                observingCrew.RaiseAdaptability(0.5f);
                observingCrew.AddExperience(5f);
                passText.text = Localization.GetWord(LocalizedWord.Pass);
                canPassThrough = true;

                playerResultLabel.text = ((int)result).ToString();
                playerResultLabel.enabled = true;
            }
            else
            {
                if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSoundEffect(SoundEffect.RollFail);
                observingCrew.RaiseAdaptability(0.25f);
                passText.text = Localization.GetWord(LocalizedWord.Return);
                canPassThrough = false;

                outerRollRing.fillAmount = 1f;
                innerRollRing.fillAmount = 1f;
                rollText.text = ((int)result).ToString();
                rollButton.SetActive(true);
            }

            observingCrew.StaminaDrain(STAMINA_PER_STEP * observingPoint.difficulty * cf.difficultyClass / (0.5f + 0.5f *result) * 4f);
            passButton.SetActive(true);
            needInfoRefreshing = true;
        }
    }
    private void MoveCrewToField(int newIndex)
    {
        fieldSize = deckHolder.GetComponent<RectTransform>().rect.width * 1f / size;
        int iy = newIndex / size, ix = newIndex % size;
        var cpos = observingExpedition.GetPlanPos();
        var chfields = observingPoint.GetChallengesArrayRef();
        if (ix != cpos.x || iy != cpos.y)
        {
            int x = cpos.x, y = cpos.y;
            int xdelta = ix - x, ydelta = iy - y;
            if (Mathf.Abs(xdelta) < 2 && Mathf.Abs(ydelta) < 2)
            {
                List<Vector2Int> activateList = new List<Vector2Int>(), deactivateList = new List<Vector2Int>();
                int cindex = y * size + x;
                if (xdelta == 0)
                {
                    bool right = x + 1 < size, left = x - 1 >= 0;
                    if (ydelta > 0) // (0,1)
                    {
                        if (y - 1 >= 0)
                        {
                            deactivateList.Add(new Vector2Int(x, y - 1));
                            if (left)
                            {
                                deactivateList.Add(new Vector2Int(x - 1, y - 1));
                            }
                            if (right)
                            {
                                deactivateList.Add(new Vector2Int(x + 1, y - 1));
                            }
                        }
                        if (y + 2 < size)
                        {
                            activateList.Add(new Vector2Int(x, y + 2));
                            if (left)
                            {
                                activateList.Add(new Vector2Int(x - 1, y + 2));
                            }
                            if (right)
                            {
                                activateList.Add(new Vector2Int(x + 1, y + 2));
                            }
                        }
                    }
                    else //(0,-1)
                    {
                        if (y + 1 < size)
                        {
                            deactivateList.Add(new Vector2Int(x, y + 1));
                            if (left)
                            {
                                deactivateList.Add(new Vector2Int(x - 1, y + 1));
                            }
                            if (right)
                            {
                                deactivateList.Add(new Vector2Int(x + 1, y + 1));
                            }
                        }
                        if (y - 2 >= 0)
                        {
                            activateList.Add(new Vector2Int(x, y - 2));
                            if (left)
                            {
                                activateList.Add(new Vector2Int(x - 1, y - 2));
                            }
                            if (right)
                            {
                                activateList.Add(new Vector2Int(x + 1, y - 2));
                            }
                        }
                    }
                }
                else
                {
                    if (ydelta == 0)
                    {
                        bool up = y + 1 < size, down = y - 1 >= 0;
                        if (xdelta > 0) // (1,0)
                        {
                            if (x - 1 >= 0)
                            {
                                deactivateList.Add(new Vector2Int(x - 1, y));
                                if (up)
                                {
                                    deactivateList.Add(new Vector2Int(x - 1, y + 1));
                                }
                                if (down)
                                {
                                    deactivateList.Add(new Vector2Int(x - 1, y - 1));
                                }
                            }
                            if (x + 2 < size)
                            {
                                activateList.Add(new Vector2Int(x + 2, y));
                                if (up)
                                {
                                    activateList.Add(new Vector2Int(x + 2, y + 1));
                                }
                                if (down)
                                {
                                    activateList.Add(new Vector2Int(x + 2, y - 1));
                                }
                            }
                        }
                        else // (-1,0)
                        {
                            if (x + 1 < size)
                            {
                                deactivateList.Add(new Vector2Int(x + 1, y));
                                if (up)
                                {
                                    deactivateList.Add(new Vector2Int(x + 1, y + 1));
                                }
                                if (down)
                                {
                                    deactivateList.Add(new Vector2Int(x + 1, y - 1));
                                }
                            }
                            if (x - 2 >= 0)
                            {
                                activateList.Add(new Vector2Int(x - 2, y));
                                if (up)
                                {
                                    activateList.Add(new Vector2Int(x - 2, y + 1));
                                }
                                if (down)
                                {
                                    activateList.Add(new Vector2Int(x - 2, y - 1));
                                }
                            }
                        }
                    }
                    else
                    {
                        bool up = y + 1 < size, down = y - 1 >= 0,
                            right = x + 1 < size, left = x - 1 >= 0;
                        if (ydelta > 0)
                        {
                            bool up2 = y + 2 < size;
                            if (xdelta > 0) // (1,1)
                            {
                                if (left)
                                {
                                    deactivateList.Add(new Vector2Int(x - 1, y));
                                    if (up)
                                    {
                                        deactivateList.Add(new Vector2Int(x - 1, y + 1));
                                    }
                                    if (down)
                                    {
                                        deactivateList.Add(new Vector2Int(x - 1, y - 1));
                                    }
                                }
                                if (down)
                                {
                                    deactivateList.Add(new Vector2Int(x, y - 1));
                                    if (right)
                                    {
                                        deactivateList.Add(new Vector2Int(x + 1, y - 1));
                                    }
                                }
                                //
                                bool right2 = x + 2 < size;
                                if (up2)
                                {
                                    activateList.Add(new Vector2Int(x, y + 2));
                                    if (right)
                                    {
                                        activateList.Add(new Vector2Int(x + 1, y + 2));
                                    }
                                    if (right2)
                                    {
                                        activateList.Add(new Vector2Int(x + 2, y + 2));
                                    }
                                }
                                if (right2)
                                {
                                    activateList.Add(new Vector2Int(x + 2, y));
                                    if (up)
                                    {
                                        activateList.Add(new Vector2Int(x + 2, y + 1));
                                    }
                                }
                            }
                            else // (-1,1)
                            {
                                if (right)
                                {
                                    deactivateList.Add(new Vector2Int(x + 1, y));
                                    if (up)
                                    {
                                        deactivateList.Add(new Vector2Int(x + 1, y + 1));
                                    }
                                    if (down)
                                    {
                                        deactivateList.Add(new Vector2Int(x + 1, y - 1));
                                    }
                                }
                                if (down)
                                {
                                    deactivateList.Add(new Vector2Int(x, y - 1));
                                    if (left)
                                    {
                                        deactivateList.Add(new Vector2Int(x - 1, y - 1));
                                    }
                                }
                                //
                                bool left2 = x - 2 >= 0;
                                if (up2)
                                {
                                    activateList.Add(new Vector2Int(x, y + 2));
                                    if (left)
                                    {
                                        activateList.Add(new Vector2Int(x - 1, y + 2));
                                    }
                                    if (left2)
                                    {
                                        activateList.Add(new Vector2Int(x - 2, y + 2));
                                    }
                                }
                                if (left2)
                                {
                                    activateList.Add(new Vector2Int(x - 2, y));
                                    if (up)
                                    {
                                        activateList.Add(new Vector2Int(x - 2, y + 1));
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool down2 = y - 2 >= 0;
                            if (xdelta > 0) // (1,-1)
                            {
                                if (up)
                                {
                                    deactivateList.Add(new Vector2Int(x, y + 1));
                                    if (right)
                                    {
                                        deactivateList.Add(new Vector2Int(x + 1, y + 1));
                                    }
                                    if (left)
                                    {
                                        deactivateList.Add(new Vector2Int(x - 1, y + 1));
                                    }
                                }
                                if (left)
                                {
                                    deactivateList.Add(new Vector2Int(x - 1, y));
                                    if (down)
                                    {
                                        deactivateList.Add(new Vector2Int(x - 1, y - 1));
                                    }
                                }
                                //
                                if (x + 2 < size)
                                {
                                    activateList.Add(new Vector2Int(x + 2, y));
                                    if (down)
                                    {
                                        activateList.Add(new Vector2Int(x + 2, y - 1));
                                    }
                                    if (down2)
                                    {
                                        activateList.Add(new Vector2Int(x + 2, y - 2));
                                    }
                                }
                                if (down2)
                                {
                                    activateList.Add(new Vector2Int(x, y - 2));
                                    if (right)
                                    {
                                        activateList.Add(new Vector2Int(x + 1, y - 2));
                                    }
                                }
                            }
                            else //(-1,-1)
                            {
                                if (up)
                                {
                                    deactivateList.Add(new Vector2Int(x, y + 1));
                                    if (left)
                                    {
                                        deactivateList.Add(new Vector2Int(x - 1, y + 1));
                                    }
                                    if (right)
                                    {
                                        deactivateList.Add(new Vector2Int(x + 1, y + 1));
                                    }
                                }
                                if (right)
                                {
                                    deactivateList.Add(new Vector2Int(x + 1, y));
                                    if (down)
                                    {
                                        deactivateList.Add(new Vector2Int(x + 1, y - 1));
                                    }
                                }
                                //
                                if (x - 2 >= 0)
                                {
                                    activateList.Add(new Vector2Int(x - 2, y));
                                    if (down)
                                    {
                                        activateList.Add(new Vector2Int(x - 2, y - 1));
                                    }
                                    if (down2)
                                    {
                                        activateList.Add(new Vector2Int(x - 2, y - 2));
                                    }
                                }
                                if (down2)
                                {
                                    activateList.Add(new Vector2Int(x, y - 2));
                                    if (left)
                                    {
                                        activateList.Add(new Vector2Int(x - 1, y - 2));
                                    }
                                }
                            }
                        }
                    }
                }
                ChallengeField cf;
                if (deactivateList.Count > 0)
                {
                    foreach (var p in deactivateList)
                    {
                        cf = chfields[p.x, p.y];
                        if (!cf.IsImpassable())
                        {
                            buttons[p.x + p.y * size].GetComponent<Image>().color = disabledFieldColor;
                        }
                    }
                }
                if (activateList.Count > 0)
                {
                    foreach (var p in activateList)
                    {
                        cf = chfields[p.x, p.y];
                        if (!cf.IsImpassable())
                        {
                            buttons[p.x + p.y * size].GetComponent<Image>().color = activeFieldColor;
                            if (cf.isHidden)
                            {
                                cf.ChangeHiddenStatus(false);
                                RedrawButtonIcon(p.x + p.y * size);
                            }
                        }
                    }
                }
                observingExpedition.SetPlanPos(new Vector2Int(ix, iy));
                //print(ix.ToString() + ' ' + iy.ToString() + " exploringMinigameUI:1058");
                moveMarker = true;
                cf = chfields[ix, iy];
                cf.MarkAsPassed();
                RedrawButtonIcon(newIndex);

                // next pass availability check:
                {
                    var variants = new List<int>();
                    bool right = ix < size - 1, up = iy < size - 1, down = iy > 0;
                    if (up) variants.Add(newIndex + size);
                    if (down) variants.Add(newIndex - size);
                    if (right)
                    {
                        variants.Add(newIndex + 1);
                        if (up) variants.Add(newIndex + 1 + size);
                        if (down) variants.Add(newIndex + 1 - size);
                    }
                    if (variants.Count > 0)
                    {
                        var blockedCells = new List<int>();
                        byte exits = 0;
                        ChallengeType ctype;
                        foreach (var v in variants)
                        {
                            ctype = chfields[v % size, v / size].challengeType;
                            if (ctype == ChallengeType.Impassable) blockedCells.Add(v);
                            else exits++;
                        }
                        int bc = blockedCells.Count;
                        if (exits == 0 && bc > 0)
                        {
                            if (bc == 1) observingPoint.ConvertToChallengeable(blockedCells[0] % size, blockedCells[0] / size);
                            else
                            {
                                bc = Random.Range(0, bc);
                                observingPoint.ConvertToChallengeable(blockedCells[bc] % size, blockedCells[bc] / size);
                            }
                        }
                    }
                }

                observingCrew.StaminaDrain(STAMINA_PER_STEP * observingPoint.difficulty);
                observingExpedition.SpendSupplyCrate();
                needInfoRefreshing = true;
            }
        }
    }

    private void RedrawButtonIcon(int index)
    {
        //#change button icon
        var img = buttons[index].transform.GetChild(0).GetComponent<RawImage>();
        var cf = observingPoint.GetChallengeField(index);
        if (cf.isPassed || cf.IsImpassable())
        {
            img.enabled = false; return;
        }
        else
        {
            if (cf.isHidden)
            {
                img.uvRect = UIController.GetIconUVRect(Icons.Unknown);
                img.color = unreachableIconColor;
                img.enabled = true;
            }
            else
            {
                if (cf.challengeType != ChallengeType.NoChallenge)
                {
                    img.uvRect = ChallengeField.GetChallengeIconRect(cf.challengeType);
                    if (cf.challengeType != ChallengeType.PuzzlePart) img.color = reachableIconColor;
                    else img.color = Knowledge.colors[cf.difficultyClass];
                    img.enabled = true;
                }
                else
                {
                    img.enabled = false;
                }
            }            
        }
        //
    }    
        
    public void OpenCrewPanel()
    {
        if (observingCrew != null)
        {
            var c = Crew.crewObserver;
            if (c != null && c.isActiveAndEnabled)
            {
                c.gameObject.SetActive(false);
            }
            else
            {
                var r = deckHolder.GetComponent<RectTransform>().rect;
                observingCrew.ShowOnGUI(new Rect(deckHolder.transform.position.x, deckHolder.transform.position.y, r.width, r.height), SpriteAlignment.Center, true);
                deckHolder.SetActive(false);
            }
        }
    }
    public void EnableDeckHolder()
    {
        deckHolder.SetActive(true);
    }

    public void StopMissionButton(bool check)
    {        
        if (observingExpedition != null)
        {
            if (!check) observingExpedition.EndMission();
            else {
                if (!observingExpedition.SuccessfulExitTest()) observingExpedition.Disappear();
                else observingExpedition.EndMission();
            }
            observingExpedition = null;            
            observingCrew = null;
        }
        if (observingPoint != null)
        {
            observingPoint.ResetChallengesArray();
            GameMaster.realMaster.globalMap.RockSector(observingPoint);
            observingPoint = null;
        }
        gameObject.SetActive(false);        
    }

    private void OnEnable()
    {
        minigameActive = true;
    }
    private void OnDisable()
    {
        minigameActive = false;
        if (launchedFromMap) { GameMaster.realMaster.globalMap.ShowOnGUI(); }
        else
        {
            UIController.SetActivity(true);
            ExplorationPanelUI.RestoreSession(observingExpedition);
        }
    }
}


