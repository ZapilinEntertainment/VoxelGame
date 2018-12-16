using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum QuestSection : byte { Progress, One, Two,Three, Four, Five, Six, Seven, Endgame, TotalCount}

public sealed class QuestUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] RectTransform[] questButtons; // fiti
    [SerializeField] GameObject questInfoPanel; // fiti
    [SerializeField] RectTransform stepsContainer, listContainer; // fiti
    [SerializeField] Text questName, questDescription, timer, rewardText; // fiti    
#pragma warning restore 0649

    public bool[] questAccessMap { get; private set; }
    public sbyte openedQuest { get; private set; }
    public Quest[] activeQuests { get; private set; }

    private float rectTransformingSpeed = 0.8f, transformingProgress;
    private RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
    private bool transformingRectInProgress = false;
    private bool prepareCrewsList = false; // else prepare shuttles list        
    private float[] timers;
    private float questUpdateTimer = 0;

    private const float QUEST_REFRESH_TIME = 30, QUEST_UPDATE_TIME = 1, QUEST_EMPTY_TIMERVAL = -1, QUEST_AWAITING_TIMERVAL = -2;

    public static Sprite questBlocked_tx { get; private set; }
    public static Sprite questAwaiting_tx { get; private set; }
    public static Sprite questBuildingBack_tx { get; private set; }
    public static Sprite questResourceBack_tx { get; private set; }
    public static QuestUI current { get; private set; }

    public static void LoadTextures()
    {
        questBlocked_tx = Resources.Load<Sprite>("Textures/questUnacceptableIcon");
        questAwaiting_tx = Resources.Load<Sprite>("Textures/questAwaiting");
        questBuildingBack_tx = Resources.Load<Sprite>("Textures/quest_buildingFrame");
        questResourceBack_tx = Resources.Load<Sprite>("Textures/quest_resourceFrame");
    }

    private void Awake()
    {
        current = this;
        openedQuest = -1;
        int totalCount = (int)QuestSection.TotalCount;
        activeQuests = new Quest[totalCount];
        questAccessMap = new bool[totalCount];
        questAccessMap[(int)QuestSection.Progress] = true ;
        timers = new float[totalCount];
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
        float f = 0, t = Time.deltaTime * GameMaster.gameSpeed;
        questUpdateTimer -= t;
        bool checkConditions = questUpdateTimer <= 0;
        if (checkConditions) questUpdateTimer = QUEST_UPDATE_TIME;
        for (sbyte i = 0; i < activeQuests.Length; i++)
        {
            Quest q = activeQuests[i];
            if (q == null) continue;
            if (q.completed)
            {
                activeQuests[i] = null;
                if (GetComponent<Image>().enabled & (openedQuest == -1)) PrepareBasicQuestWindow();
                StartCoroutine(WaitForNewQuest(i));
                continue;
            }
            if (checkConditions) q.CheckQuestConditions();

            if (openedQuest == i)
            {
                PrepareStepsList(q);
            }

            f = timers[i];
            if (f != -1)
            {
                f -= t;
                if (f <= 0)
                {
                    DropQuest(i);
                }
            }
        }       

    }

    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        PrepareBasicQuestWindow();
        UIController.current.ChangeActiveWindow(ActiveWindowMode.QuestPanel);
    }
    public void Deactivate()
    {
        GetComponent<Image>().enabled = false;
        questButtons[0].transform.parent.gameObject.SetActive(false);
        questInfoPanel.SetActive(false);
        UIController controller = UIController.current;
        controller.ActivateLeftPanel();
        controller.DropActiveWindow(ActiveWindowMode.NoWindow);
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
            if (questAccessMap[i] == true)
            {
                Quest q = activeQuests[i];
                if (q != null)
                {
                    btn.GetComponent<Button>().interactable = true;
                    Quest.SetQuestTexture(q, btn.GetComponent<Image>(), rt.GetChild(0).GetComponent<RawImage>());
                    Text t = rt.GetChild(1).GetComponent<Text>();
                    t.text = q.name;
                    t.color = (q.picked ? Color.cyan : Color.white);
                }
                else
                {
                    btn.GetComponent<Button>().interactable = false;
                    btn.GetComponent<Image>().overrideSprite = questAwaiting_tx;
                    btn.GetChild(0).GetComponent<RawImage>().enabled = false;
                    Text t = rt.GetChild(1).GetComponent<Text>();
                    t.text = "...";
                    t.color = Color.grey;
                }
            }
            else
            {
                btn.GetComponent<Button>().interactable = false;
                btn.GetComponent<Image>().overrideSprite = questBlocked_tx;
                btn.GetChild(0).GetComponent<RawImage>().enabled = false;
                rt.GetChild(1).GetComponent<Text>().text = string.Empty;
            }
        }
    }
    public void QuestButton_OpenQuest(sbyte index)
    {
        transformingRect = questButtons[index];
        transformingRectInProgress = true;
        transformingProgress = 0;
        resultingAnchorMin = Vector2.zero;
        resultingAnchorMax = Vector2.one;
        openedQuest = index;
        for (int i = 0; i < questButtons.Length; i++)
        {
            if (i == index) continue;
            else
            {
                questButtons[i].gameObject.SetActive(false);
            }
        }

        Quest q = activeQuests[openedQuest];
        questName.text = q.name;
        questDescription.text = q.description;
        rewardText.text = Localization.GetWord(LocalizedWord.Reward) + " : " + ((int)q.reward).ToString();
        if (timers[index] != -1) timer.text = string.Format("{0:0.00}", timers[index]);
        PrepareStepsList(q);
        // цена и кнопка запуска

    }
    public IEnumerator WaitForNewQuest(int i)
    {
        timers[i] = QUEST_AWAITING_TIMERVAL;
        yield return new WaitForSeconds(QUEST_REFRESH_TIME);
        if (activeQuests[i] == null) SetNewQuest(i);
    }

    public void UnblockQuestButton(QuestSection qs)
    {
        if (qs == QuestSection.TotalCount) return;
        int index = (int)qs;
        questAccessMap[index] = true;
        if (activeQuests[index] == null & timers[index] != QUEST_AWAITING_TIMERVAL) StartCoroutine(WaitForNewQuest(index));
    }
    public void ResetQuestCell(Quest q)
    {
        if (q == Quest.NoQuest) return;
        for(int i = 0; i < activeQuests.Length; i++)
        {
            if (activeQuests[i] == q)
            {
                activeQuests[i] = Quest.NoQuest;
                WaitForNewQuest(i);
            }
        }
    }

    private void SetNewQuest(int i)
    {
        if (questAccessMap[i] == false)
        {
            activeQuests[i] = null;
            timers[i] = QUEST_EMPTY_TIMERVAL;
            return;
        }
        // поиск подходящих среди отложенных
        Quest q = null;
        switch ((QuestType)i) {
            case QuestType.Progress: q = Quest.GetProgressQuest(); break;
            case QuestType.Endgame:
                uint mask = Quest.questsCompletenessMask[i];
                if (mask == 1) q = new Quest(QuestType.Endgame, 1);
                else
                {
                    if (mask == 3) q = new Quest(QuestType.Endgame, 2);
                }
                break;
        }
        if (q == null )
        {
            if (timers[i] != QUEST_AWAITING_TIMERVAL) StartCoroutine(WaitForNewQuest(i));
            return;
        }
        if (!q.picked)
        {
            timers[i] = q.questLifeTimer;
        }
        else // autopicked quests
        {
            timers[i] = q.questRealizationTimer;
        }
        activeQuests[i] = q;
        if (openedQuest == -1 & GetComponent<Image>().enabled) PrepareBasicQuestWindow();
        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NewQuestAvailable));
    }   
    private void ReturnToQuestList()
    {
        if (openedQuest == -1) return;
        QuestButton_RestoreButtonRect(openedQuest);
        transformingRectInProgress = true;
        transformingProgress = 0;
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
                so.transform.GetChild(0).GetComponent<RawImage>().uvRect = UIController.GetTextureUV(q.stepsFinished[i] ? Icons.TaskCompleted : Icons.TaskFrame);
            }
            else so.SetActive(false);
        }
    }

    public QuestStaticSerializer Save()
    {
        QuestStaticSerializer qss = new QuestStaticSerializer();
        qss.questsCompletenessMask = Quest.questsCompletenessMask;
        qss.activeQuests = new QuestSerializer[activeQuests.Length];
        qss.questAccessMap = questAccessMap;
        for (int i = 0; i < qss.activeQuests.Length; i++)
        {
            if (activeQuests[i] != null) qss.activeQuests[i] = activeQuests[i].Save();
            else qss.activeQuests[i] = null;
        }
        return qss;
    }
    public void Load(QuestStaticSerializer qss)
    {
        questAccessMap = qss.questAccessMap;
        Quest.SetCompletenessMask(qss.questsCompletenessMask);
        activeQuests = new Quest[questButtons.Length];        
        for (int i = 0; i < qss.activeQuests.Length; i++)
        {
            if (qss.activeQuests[i] == null) continue;
            else
            {
                activeQuests[i] = Quest.Load(qss.activeQuests[i]);
            }
        }        
    }
}

[System.Serializable]
public class QuestStaticSerializer
{
    public uint[] questsCompletenessMask;
    public QuestSerializer[] activeQuests;
    public bool[] questAccessMap;
}

