using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum QuestSection : byte { Progress, One, Two,Three, Four, Five, Six, Seven, Endgame, TotalCount}

public sealed class QuestUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] RectTransform[] questButtons; // fiti
    [SerializeField] GameObject questInfoPanel, closeButton; // fiti
    [SerializeField] RectTransform stepsContainer;
    [SerializeField] Text questName, questDescription, rewardText; // fiti    
#pragma warning restore 0649

    public bool[] questAccessMap { get; private set; }
    public sbyte openedQuest { get; private set; }
    public Quest[] activeQuests { get; private set; }

    private float rectTransformingSpeed = 0.8f, transformingProgress;
    private RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
    private bool transformingRectInProgress = false;
    private bool prepareCrewsList = false; // else prepare shuttles list        
    private float questUpdateTimer = 0;

    private const float QUEST_REFRESH_TIME = 30, QUEST_UPDATE_TIME = 1, QUEST_EMPTY_TIMERVAL = -1, QUEST_AWAITING_TIMERVAL = -2;

    public static QuestUI current { get; private set; }

    private void Awake()
    {
        current = this;
        openedQuest = -1;
        int totalCount = (int)QuestSection.TotalCount;
        activeQuests = new Quest[totalCount];
        for (int i = 0; i < activeQuests.Length; i++)
        {
            activeQuests[i] = Quest.NoQuest;
        }
        questAccessMap = new bool[totalCount];
        questAccessMap[(int)QuestSection.Progress] = true ;
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
        float t = Time.deltaTime * GameMaster.gameSpeed;
        questUpdateTimer -= t;
        bool checkConditions = questUpdateTimer <= 0;
        if (checkConditions) questUpdateTimer = QUEST_UPDATE_TIME;
        for (sbyte i = 0; i < activeQuests.Length; i++)
        {
            Quest q = activeQuests[i];
            if (q == Quest.NoQuest) continue;
            if (checkConditions) q.CheckQuestConditions();
            if (openedQuest == i)
            {
                PrepareStepsList(q);
            }
        }     
    }

    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        closeButton.SetActive(true);
        PrepareBasicQuestWindow();
        UIController.current.ChangeActiveWindow(ActiveWindowMode.QuestPanel);
    }
    public void Deactivate()
    {
        GetComponent<Image>().enabled = false;
        closeButton.SetActive(false);
        questButtons[0].transform.parent.gameObject.SetActive(false);
        questInfoPanel.SetActive(false);
        UIController controller = UIController.current;
        controller.ActivateLeftPanel();
        controller.DropActiveWindow(ActiveWindowMode.QuestPanel);
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
                if (q != Quest.NoQuest)
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
                    ri.texture = UIController.current.iconsTexture;
                    ri.uvRect = UIController.GetTextureUV(Icons.QuestAwaitingIcon);
                    t.text = "...";
                    t.color = Color.grey;
                }
            }
            else
            {
                btn.GetComponent<Button>().interactable = false;
                RawImage ri = btn.GetChild(0).GetComponent<RawImage>();
                t.text = string.Empty;
                ri.texture = UIController.current.iconsTexture;
                ri.uvRect = UIController.GetTextureUV(Icons.QuestBlockedIcon);
            }
        }
    }
    public void QuestButton_OpenQuest(sbyte index)
    {
        Quest q = activeQuests[index];
        if (q == Quest.NoQuest) return;
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
        
        questName.text = q.name;
        questDescription.text = q.description;
        rewardText.text = Localization.GetWord(LocalizedWord.Reward) + " : " + ((int)q.reward).ToString();
        PrepareStepsList(q);
        // цена и кнопка запуска

    }

    public IEnumerator WaitForNewQuest(int i)
    {
        if (activeQuests[i] != Quest.NoQuest)
        {
            yield return null;
        }
        yield return new WaitForSeconds(QUEST_REFRESH_TIME);
        if (activeQuests[i] == Quest.NoQuest) SetNewQuest(i);
    }

    public void UnblockQuestButton(QuestSection qs)
    {
        if (qs == QuestSection.TotalCount) return;
        int index = (int)qs;
        questAccessMap[index] = true;
        if (activeQuests[index] == Quest.NoQuest) StartCoroutine(WaitForNewQuest(index));
    }

    public void DropQuest()
    {
        if (openedQuest == -1) return;
        else
        {
            activeQuests[openedQuest] = Quest.NoQuest;
            WaitForNewQuest(openedQuest);
            ReturnToQuestList();
        }
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
            activeQuests[i] = Quest.NoQuest;
            return;
        }
        // поиск подходящих среди отложенных
        Quest q = Quest.NoQuest;
        switch ((QuestSection)i) {
            case QuestSection.Progress: q = Quest.GetProgressQuest(); break;
            case QuestSection.Endgame:
                uint mask = Quest.questsCompletenessMask[i];
                if (mask == 1) q = new Quest(QuestType.Endgame, 1);
                else
                {
                    if (mask == 3) q = new Quest(QuestType.Endgame, 2);
                }
                break;
        }
        
        if (q == Quest.NoQuest )
        {
            StartCoroutine(WaitForNewQuest(i));
            return;
        }
        else activeQuests[i] = q;

        if (openedQuest == -1 & GetComponent<Image>().enabled) PrepareBasicQuestWindow();
        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NewQuestAvailable));
    }   
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

    #region save-load
    public void Save(System.IO.FileStream fs)
    {
        // access map
        int count = questAccessMap.Length;
        fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        byte one = 1, zero = 0;
        for (int i = 0; i < count; i++)
        {
            fs.WriteByte(questAccessMap[i] ? one : zero);
        }

        //completeness mask
        count = Quest.questsCompletenessMask.Length;
        fs.Write(System.BitConverter.GetBytes(count),0,4);
        for (int i =0; i < count; i++)
        {
            fs.Write(System.BitConverter.GetBytes(Quest.questsCompletenessMask[i]), 0, 4);
        }       
        //active quests
        count = activeQuests.Length;
        fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        for (int i = 0; i < count; i++)
        {
            if (activeQuests[i] != Quest.NoQuest)
            {
                fs.WriteByte(one);
                var data = activeQuests[i].Save().ToArray();
                fs.Write(data, 0, data.Length);
            }
            else fs.WriteByte(zero);
        }
    }
    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[4];
        //access mask
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data, 0);
        questAccessMap = new bool[count];
        data = new byte[count];
        fs.Read(data, 0, count);
        for (int i = 0; i < count;i++)
        {
            questAccessMap[i] = data[i] == 1;
        }

        //completeness mask
        fs.Read(data, 0, 4);
        count = System.BitConverter.ToInt32(data, 0);
        uint[] mask = new uint[count];
        for (int i = 0; i < count; i++)
        {
            fs.Read(data, 0, 4);
            mask[i] = System.BitConverter.ToUInt32(data, 0);
        }
        Quest.SetCompletenessMask(mask);

        //active quests
        data = new byte[4];
        fs.Read(data, 0, 4);
        count = System.BitConverter.ToInt32(data, 0);
        activeQuests = new Quest[count];
        for (int i = 0; i < count; i++)
        {
            var marker = fs.ReadByte();
            if (marker == 0)
            {
                activeQuests[i] = Quest.NoQuest;
                if (questAccessMap[i] == true) WaitForNewQuest(i);
            }
            else
            {
                activeQuests[i] = Quest.Load(fs);
            }
        }        
    }
    #endregion
}


