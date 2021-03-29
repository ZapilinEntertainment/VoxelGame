using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum QuestSection : byte { Zero, One, Two,Three, Endgame, Five, Six, Seven, Eight, TotalCount}

public sealed class QuestUI : MonoBehaviour, ILocalizable
{
#pragma warning disable 0649
    [SerializeField] RectTransform[] questButtons; // fiti
    [SerializeField] GameObject questInfoPanel, closeButton, questDropButton; // fiti
    [SerializeField] RectTransform stepsContainer;
    [SerializeField] Text questName, questDescription, rewardText; // fiti  
    [SerializeField] RawImage newQuestMarker;
#pragma warning restore 0649

    public bool[] questAccessMap { get; private set; }
    public sbyte openedQuest { get; private set; }
    public Quest[] activeQuests { get; private set; }

    private bool prepared = false;
    private float rectTransformingSpeed = 0.8f, transformingProgress;
    private RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
    private bool transformingRectInProgress = false;   
    private float questUpdateTimer = 0f;
    private Queue<byte> questCreateQueue;
    private MainCanvasController myCanvas;

    private const float QUEST_UPDATE_TIME = 1, QUEST_EMPTY_TIMERVAL = -1, QUEST_AWAITING_TIMERVAL = -2;

    public static QuestUI current { get; private set; }

    public static bool IsEndquestInProgress()
    {
        if (current == null) return false;
        else
        {
            return current.activeQuests[(int)QuestSection.Endgame] != Quest.NoQuest;
        }
    }

    private void Awake()
    {
        if (!prepared) Prepare(true);
    }
    private void Start()
    {
        // если присваивать myCanvas в Awake, то все ломается
        myCanvas = UIController.GetCurrent().GetMainCanvasController();
    }
    private void Prepare(bool prepareQuestIfNone)
    {
        current = this;
        openedQuest = -1;
        
        int totalCount = (int)QuestSection.TotalCount;
        activeQuests = new Quest[totalCount];
        questCreateQueue = new Queue<byte>();
        GameMaster.realMaster.everydayUpdate += this.EverydayUpdate;
        for (int i = 0; i < activeQuests.Length; i++)
        {
            activeQuests[i] = Quest.NoQuest;
        }
        questAccessMap = new bool[totalCount];
        CheckQuestsAccessibility(prepareQuestIfNone);
        questUpdateTimer = QUEST_UPDATE_TIME;

        for (int i = 0; i < questButtons.Length; i++)
        {
            Button b = questButtons[i].GetComponent<Button>();
            sbyte index = (sbyte)i;
            b.onClick.AddListener(() =>
            {
                this.QuestButton_OpenQuest(index);
            });
        }
        LocalizeTitles();
        prepared = true;
    }

    void Update()
    {
        if (transformingRectInProgress)
        {
            transformingProgress = Mathf.MoveTowards(transformingProgress, 1, rectTransformingSpeed * Time.deltaTime);
            transformingRect.anchorMin = Vector2.Lerp(transformingRect.anchorMin, resultingAnchorMin, transformingProgress);
            transformingRect.anchorMax = Vector2.Lerp(transformingRect.anchorMax, resultingAnchorMax, transformingProgress);
            if (transformingProgress == 1)
            {
                transformingProgress = 0;
                transformingRectInProgress = false;
                if (openedQuest != -1)
                { // окно квеста открылось                    
                    questInfoPanel.SetActive(true);
                    questButtons[openedQuest].gameObject.SetActive(false);
                }
                else
                { // возврат ко всем квестам
                    PrepareBasicQuestWindow();
                }
            }
        }
        //
        float t = Time.deltaTime * GameMaster.gameSpeed;
        questUpdateTimer -= t;
        bool checkConditions = questUpdateTimer <= 0;
        if (checkConditions)
        {
            questUpdateTimer = QUEST_UPDATE_TIME;
            for (sbyte i = 0; i < activeQuests.Length; i++)
            {
                Quest q = activeQuests[i];
                if (q == Quest.NoQuest || q == Quest.AwaitingQuest) continue;
                else
                {
                    if (q.needToCheckConditions) q.CheckQuestConditions();
                    if (openedQuest == i)
                    {
                        PrepareStepsList(q);
                    }
                }
            }
        }

        //
        //if (Input.GetKeyDown("k")) Debug.Log(questCreateTimer);
    }
    private void EverydayUpdate()
    {
        if (questCreateQueue.Count > 0)
        {
            var element = questCreateQueue.Dequeue();
            SetNewQuest(element);
        }
    }

    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        closeButton.SetActive(true);
        newQuestMarker.enabled = false;
        PrepareBasicQuestWindow();
        myCanvas.ChangeActiveWindow(ActiveWindowMode.QuestPanel);
    }
    public void Deactivate()
    {
        GetComponent<Image>().enabled = false;
        closeButton.SetActive(false);
        questButtons[0].transform.parent.gameObject.SetActive(false);
        questInfoPanel.SetActive(false);
        myCanvas.ActivateLeftPanel();
        myCanvas.DropActiveWindow(ActiveWindowMode.QuestPanel);
    }
    public void CloseQuestWindow()
    {
        if (openedQuest == -1) Deactivate();
        else ReturnToQuestList();
    }
    public void PrepareBasicQuestWindow() // открыть окно всех квестов
    {
        if (!GetComponent<Image>().enabled) return;
        openedQuest = -1;
        transform.GetChild(0).gameObject.SetActive(true); // quest buttons
        transform.GetChild(1).gameObject.SetActive(false); // quest info
        for (int i = 0; i < questButtons.Length; i++)
        {
            RectTransform btn = questButtons[i];
            questButtons[i].gameObject.SetActive(true);
            RectTransform rt = questButtons[i];
            Text t = rt.GetChild(1).GetComponent<Text>();
            if (questAccessMap[i] == true)
            {
                Quest q = activeQuests[i];
                if (q != Quest.NoQuest && q!= Quest.AwaitingQuest)
                {
                    btn.GetComponent<Button>().interactable = true;
                    Quest.SetQuestTexture(q, btn.GetComponent<Image>(), rt.GetChild(0).GetComponent<RawImage>());                    
                    t.text = q.name;
                    t.color = Color.cyan;
                }
                else
                {
                    btn.GetComponent<Button>().interactable = false;
                    RawImage ri = btn.GetChild(0).GetComponent<RawImage>();
                    ri.texture = UIController.iconsTexture;
                    ri.uvRect = UIController.GetIconUVRect(Icons.QuestAwaitingIcon);
                    t.text = "...";
                    t.color = Color.grey;
                }
            }
            else
            {
                btn.GetComponent<Button>().interactable = false;
                RawImage ri = btn.GetChild(0).GetComponent<RawImage>();
                t.text = string.Empty;
                ri.texture = UIController.iconsTexture;
                ri.uvRect = UIController.GetIconUVRect(Icons.QuestBlockedIcon);
            }
        }
    }
    public void QuestButton_OpenQuest(sbyte index)
    {
        Quest q = activeQuests[index];
        if (q == Quest.NoQuest || q == Quest.AwaitingQuest) return; // вообще-то, лишняя проверка
        openedQuest = index;
        transformingRect = questButtons[index];
        transformingRectInProgress = true;
        transformingProgress = 0;
        resultingAnchorMin = Vector2.zero;
        resultingAnchorMax = Vector2.one;        
        for (int i = 0; i < questButtons.Length; i++)
        {
            if (i == index) continue;
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
        }
        RefreshObservingQuestData(openedQuest);
    }
    private void RefreshObservingQuestData(int index)
    {
        var q = activeQuests[index];
        questName.text = q.name;
        questDescription.text = q.description;
        (questDescription.transform.parent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, questDescription.rectTransform.rect.height);
        rewardText.text = Localization.GetWord(LocalizedWord.Reward) + " : " + ((int)q.reward).ToString();
        questDropButton.SetActive(q.type != QuestType.Scenario);
        PrepareStepsList(q);
    }
    private void PrepareStepsList(Quest q)
    {
        int x = q.steps.Length;
        int stepLabelsCount = stepsContainer.childCount;
        for (int i = 0; i < stepLabelsCount; i++)
        {
            GameObject so = stepsContainer.GetChild(i).gameObject;
            if (i < x)
            {
                so.SetActive(true);
                so.GetComponent<Text>().text = q.steps[i] + q.stepsAddInfo[i];
                so.transform.GetChild(0).GetComponent<RawImage>().uvRect = UIController.GetIconUVRect(q.stepsFinished[i] ? Icons.TaskCompleted : Icons.TaskFrame);
            }
            else so.SetActive(false);
        }
    }

    public void BlockQuestPosition(QuestSection qs)
    {
        if (qs == QuestSection.TotalCount) return;
        int index = (int)qs;
        questAccessMap[index] = false;
        if (GetComponent<Image>().enabled & openedQuest == -1) PrepareBasicQuestWindow();
    }
    public void UnblockQuestPosition(QuestSection qs, bool prepareQuestIfNone)
    {
        if (qs == QuestSection.TotalCount) return;
        byte index = (byte)qs;
        questAccessMap[index] = true;
        if (GetComponent<Image>().enabled & openedQuest == -1) PrepareBasicQuestWindow(); 
        if (activeQuests[index] == Quest.NoQuest && prepareQuestIfNone ) StartNewQuestAwaiting(index);
    }
    public void DropQuest()
    {
        if (openedQuest == -1) return;
        else
        {
            activeQuests[openedQuest] = Quest.NoQuest;
            if (openedQuest != (int)QuestSection.Endgame) StartNewQuestAwaiting((byte)openedQuest);
            ReturnToQuestList();
        }
    }
    public void ResetQuestCell(Quest q)
    {
        if (q == Quest.NoQuest) return;
        for(byte i = 0; i < activeQuests.Length; i++)
        {
            if (activeQuests[i] == q)
            {
                activeQuests[i] = Quest.NoQuest;
                if (i != (int)QuestSection.Endgame) StartNewQuestAwaiting(i);
            }
        }
    }
    

    public void CheckQuestsAccessibility(bool prepareQuestIfNone)
    {
        bool createNewQuests = prepareQuestIfNone & GameMaster.realMaster.UseQuestAutoCreating();
        questAccessMap[0] = true; 
        if (prepareQuestIfNone && activeQuests[0] == Quest.NoQuest) StartNewQuestAwaiting(0);
        questAccessMap[1] = true;
        if (prepareQuestIfNone && activeQuests[1] == Quest.NoQuest) StartNewQuestAwaiting(1);
        questAccessMap[2] = true;
        if (prepareQuestIfNone && activeQuests[2] == Quest.NoQuest) StartNewQuestAwaiting(2);
        HeadQuarters hq = GameMaster.realMaster.colonyController?.hq;
        if (hq != null)
        {
            var lvl = hq.level;
            if (lvl >= 2)
            {
                if (questAccessMap[3] == false) UnblockQuestPosition(QuestSection.Three, prepareQuestIfNone);
                else
                {
                    if (prepareQuestIfNone && activeQuests[3] == Quest.NoQuest) StartNewQuestAwaiting(3);
                }
                if (lvl >= 3)
                {
                    if (questAccessMap[4] == false) UnblockQuestPosition(QuestSection.Five, prepareQuestIfNone);
                    else
                    {
                        if (prepareQuestIfNone && activeQuests[4] == Quest.NoQuest) StartNewQuestAwaiting(4);
                    }
                    if (lvl >= 4)
                    {
                        if (questAccessMap[6] == false) UnblockQuestPosition(QuestSection.Six, prepareQuestIfNone);
                        else
                        {
                            if (prepareQuestIfNone && activeQuests[6] == Quest.NoQuest) StartNewQuestAwaiting(5);
                        }
                        if (lvl >= 5)
                        {
                            if (questAccessMap[7] == false) UnblockQuestPosition(QuestSection.Seven, prepareQuestIfNone);
                            else
                            {
                                if (prepareQuestIfNone && activeQuests[7] == Quest.NoQuest) StartNewQuestAwaiting(7);
                            }
                            if (lvl >= 6)
                            {
                                if (questAccessMap[8] == false) UnblockQuestPosition(QuestSection.Eight, prepareQuestIfNone);
                                else
                                {
                                    if (prepareQuestIfNone && activeQuests[8] == Quest.NoQuest) StartNewQuestAwaiting(8);
                                }
                            }
                        }
                    }
                }
            }
        }
        else return;
            
    }
    public Quest FindQuest(Knowledge.ResearchRoute rr, byte subIndex)
    {
        Quest q;
        QuestType qt = QuestType.Total;
        switch (rr)
        {
            case Knowledge.ResearchRoute.Foundation: qt = QuestType.Foundation; break;
            case Knowledge.ResearchRoute.CloudWhale: qt = QuestType.CloudWhale; break;
            case Knowledge.ResearchRoute.Engine: qt = QuestType.Engine; break;
            case Knowledge.ResearchRoute.Pipes: qt = QuestType.Pipes; break;
            case Knowledge.ResearchRoute.Crystal: qt = QuestType.Crystal; break;
            case Knowledge.ResearchRoute.Monument: qt = QuestType.Monument; break;
            case Knowledge.ResearchRoute.Blossom: qt = QuestType.Blossom; break;
            case Knowledge.ResearchRoute.Pollen: qt = QuestType.Pollen; break;
        }
        for (int i = 0; i < activeQuests.Length; i++)
        {
            q = activeQuests[i];
            if (q != Quest.NoQuest)
            {
                if (q.type == qt && q.subIndex == subIndex) return q;
            }
        }
        return null;
    }
    public void FindAndCompleteQuest(Knowledge.ResearchRoute rr, byte subIndex)
    {
        var q = FindQuest(rr, subIndex);
        q?.MakeQuestCompleted();
    }

    private void StartNewQuestAwaiting(byte i)
    {
        if (!questCreateQueue.Contains(i)) questCreateQueue.Enqueue(i);
    }
    public void SetNewQuest(byte i)
    {
        if (questAccessMap[i] == false)
        {
            activeQuests[i] = Quest.NoQuest;
            return;
        }
        // поиск подходящих среди отложенных
        Quest q = Quest.NoQuest;
        switch ((QuestSection)i) {
            case QuestSection.Zero: q = Quest.GetProgressQuest(); break;
            case QuestSection.Endgame:
                break;
                uint mask = Quest.questsCompletenessMask[(int)QuestType.Endgame];
                if (mask == 0) q = new Quest(QuestType.Endgame, 0);
                else
                {
                    if (mask == 3) q = new Quest(QuestType.Endgame, 2);
                    else
                    {
                        if (mask == 1) q = new Quest(QuestType.Endgame, 1);
                    }
                }
                break;
            default:
                q = Knowledge.GetCurrent().GetHelpingQuest();
                break;
        }

        if (q == Quest.NoQuest)
        {
            StartNewQuestAwaiting(i);
            return;
        }
        else
        {
            activeQuests[i] = q;
            if (openedQuest == -1) newQuestMarker.enabled = true;
        }

        if (openedQuest == -1 & GetComponent<Image>().enabled) PrepareBasicQuestWindow();
        AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NewQuestAvailable));
        if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.newQuestAvailable);
    }
    public void StartEndQuest(byte routeIndex)
    {
        int endIndex = (int)QuestSection.Endgame;
        if (activeQuests[endIndex] == Quest.NoQuest || activeQuests[endIndex] == null)
        {
            activeQuests[endIndex] = new Quest(QuestType.Endgame, routeIndex);
            questAccessMap[endIndex] = true;
        }
        else
        {
            AnnouncementCanvasController.MakeImportantAnnounce(Localization.GetAnnouncementString(GameAnnouncements.AlreadyHaveEndquest));
        }
        if (openedQuest == -1 & GetComponent<Image>().enabled) PrepareBasicQuestWindow();
    }

    public ScenarioQuest SYSTEM_NewScenarioQuest(Scenario s)
    {
        var q = new ScenarioQuest(s);
        int sectionIndex = (int)q.DefineQuestSection();
        activeQuests[sectionIndex] = q;
        questAccessMap[sectionIndex] = true;
        if (openedQuest == -1 & GetComponent<Image>().enabled) PrepareBasicQuestWindow();
        return q;
    }
    public Button SYSTEM_GetQuestButton(int i)
    {
        return questButtons[i].GetComponent<Button>();
    }
    public Button SYSTEM_GetCloseButton()
    {
        return closeButton.GetComponent<Button>();
    }

    public Quest GetActiveQuest()
    {
        if (openedQuest != -1) return activeQuests[openedQuest];
        else return Quest.NoQuest;
    }
    public bool IsEnabled() { return GetComponent<Image>().enabled; }

    private void ReturnToQuestList()
    {
        if (openedQuest != -1)
        {
            QuestButton_RestoreButtonRect(openedQuest);
            transformingRectInProgress = true;
            transformingProgress = 0;
        }
        PrepareBasicQuestWindow();
    }
    private void QuestButton_RestoreButtonRect(int i)
    {
        switch (i)
        {
            case 0:
                resultingAnchorMin = new Vector2(0, 0.66f);
                resultingAnchorMax = new Vector2(0.33f, 1);
                break;
            case 1:
                resultingAnchorMin = new Vector2(0.33f, 0.66f);
                resultingAnchorMax = new Vector2(0.66f, 1);
                break;
            case 2:
                resultingAnchorMin = new Vector2(0.66f, 0.66f);
                resultingAnchorMax = new Vector2(1, 1);
                break;
            case 3:
                resultingAnchorMin = new Vector2(0, 0.33f);
                resultingAnchorMax = new Vector2(0.33f, 0.66f);
                break;
            case 4:
                resultingAnchorMin = new Vector2(0.33f, 0.33f);
                resultingAnchorMax = new Vector2(0.66f, 0.66f);
                break;
            case 5:
                resultingAnchorMin = new Vector2(0.66f, 0.33f);
                resultingAnchorMax = new Vector2(1, 0.66f);
                break;
            case 6:
                resultingAnchorMin = new Vector2(0, 0);
                resultingAnchorMax = new Vector2(0.33f, 0.33f);
                break;
            case 7:
                resultingAnchorMin = new Vector2(0.33f, 0);
                resultingAnchorMax = new Vector2(0.66f, 0.33f);
                break;
            case 8:
                resultingAnchorMin = new Vector2(0.66f, 0);
                resultingAnchorMax = new Vector2(1, 0.33f);
                break;
        }
    }

    public void LocalizeTitles()
    {
        Transform t = questInfoPanel.transform;
        t.GetChild(4).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Goals);
        t.GetChild(6).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Reject);
        Localization.AddToLocalizeList(this);
    }
    private void OnDestroy()
    {
        Localization.RemoveFromLocalizeList(this);
    }

    #region save-load
    public void Save(System.IO.FileStream fs)
    {
        // access map
        CheckQuestsAccessibility(false);
        int questsCount = (int)QuestSection.TotalCount;
        byte one = 1, zero = 0;
        for (int i = 0; i < questsCount; i++)
        {
            fs.WriteByte(questAccessMap[i] ? one : zero);
        }

        //completeness mask
        int count = Quest.questsCompletenessMask.Length;
        fs.Write(System.BitConverter.GetBytes(count),0,4);
        for (int i =0; i < count; i++)
        {
            fs.Write(System.BitConverter.GetBytes(Quest.questsCompletenessMask[i]), 0, 4);
        }       
        //active quests
        for (int i = 0; i < questsCount; i++)
        {
            if (activeQuests[i] != Quest.NoQuest)
            {
                if (activeQuests[i] == Quest.AwaitingQuest) fs.WriteByte(2);
                else
                {
                    fs.WriteByte(one);
                    var data = activeQuests[i].Save().ToArray();
                    fs.Write(data, 0, data.Length);
                }
            }
            else fs.WriteByte(zero);
        }
        //
        byte queue = (byte)questCreateQueue.Count;
        fs.WriteByte(queue);
        if (queue != 0)
        {
            var qa = questCreateQueue.ToArray();
            for (var b = 0; b < queue; b++)
            {
                fs.WriteByte(qa[b]);
            }
        }
    }
    public void Load(System.IO.FileStream fs)
    {
        if (!prepared) Prepare(false);
        int questsCount = (int)QuestSection.TotalCount;
        //access mask
        questAccessMap = new bool[questsCount];
        var data = new byte[questsCount];
        fs.Read(data, 0, data.Length);
        for (int i = 0; i < questsCount; i++)
        {
            questAccessMap[i] = data[i] == 1;
        }

        //completeness mask
        data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data, 0);
        uint[] mask = new uint[count];
        for (int i = 0; i < count; i++)
        {
            fs.Read(data, 0, 4);
            mask[i] = System.BitConverter.ToUInt32(data, 0);
        }
        Quest.SetCompletenessMask(mask);

        //active quests
        for (byte i = 0; i < questsCount; i++)
        {
            activeQuests[i] = Quest.NoQuest;
            var marker = fs.ReadByte();
            if (marker != 0)
            {
                if (marker != 2) activeQuests[i] = Quest.Load(fs);
                else StartNewQuestAwaiting(i);
            }
            else
            {
                if (questAccessMap[i] == true) StartNewQuestAwaiting(i);
            }
        }
        //quest awaiting        
        byte queue = (byte)fs.ReadByte();
        if (queue != 0)
        {
            var qa = new byte[queue];
            for (var b = 0; b < queue; b++)
            {
                qa[b] = (byte)fs.ReadByte();
            }
            questCreateQueue = new Queue<byte>(qa);
        }
        else questCreateQueue = new Queue<byte>();

    }
    #endregion
}


