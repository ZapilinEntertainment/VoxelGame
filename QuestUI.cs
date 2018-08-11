using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestUI : MonoBehaviour {
    [SerializeField] RectTransform[] questButtons; // fiti
    [SerializeField] GameObject questInfoPanel; // fiti
    [SerializeField] Text questName, questDescription, timer; // fiti    
    float rectTransformingSpeed = 0.8f, transformingProgress;
    RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
    int openedQuest = -1;
    bool transformingRectInProgress = false;
    Quest[] visibleQuests ;
    bool[] questAccessMap;
    float[] timers;

    const float QUEST_REFRESH_TIME = 60;
    Texture questSlotBlocked_tx, questAwaiting_tx;

    private void Awake()
    {
        int l = questButtons.Length;
        visibleQuests = new Quest[l];
        questAccessMap = new bool[l];
        timers = new float[l];
        questSlotBlocked_tx = Resources.Load<Texture>("Textures/questUnacceptableIcon");
       // questAwaiting_tx = Resources.Load<Texture>("Texture/questAwaiting");
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
                transform.GetChild(0).gameObject.SetActive(false); // buttons container
                questInfoPanel.SetActive(true);
            }
        }
        float f = 0, t = Time.deltaTime * GameMaster.gameSpeed;
        for (int i = 0; i < visibleQuests.Length;i ++)
        {
            Quest q = visibleQuests[i];
            if (q == null) continue;
            f = timers[i];
            if (f == -1) continue;
            if (timer)
            f -= t;
            if ( f <= 0 )
            {                
                if (q.picked)  DropQuest(i);
                else  SetNewQuest(i);
            } 
        }
    }

    private void PrepareBasicQuestWindow() // открыть окно всех квестов
    {
        openedQuest = -1;
        transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 0; i < questButtons.Length; i++)
        {
            RectTransform rt = questButtons[i];
            if (questAccessMap[i] == true)
            {
                questButtons[i].GetComponent<Button>().interactable = true;
                Quest q = visibleQuests[i];                
                if (q != null)
                {
                    rt.GetChild(0).GetComponent<RawImage>().texture = q.poster;
                    Text t = rt.GetChild(1).GetComponent<Text>();
                    t.text = q.name;
                    t.color = (q.picked ? Color.cyan : Color.white);
                }
                else
                {
                    rt.GetChild(0).GetComponent<RawImage>().texture = questAwaiting_tx;
                    Text t = rt.GetChild(1).GetComponent<Text>();
                    t.text = "...";
                    t.color = Color.grey;
                }
            }
            else
            {
                questButtons[i].GetComponent<Button>().interactable = false;
                rt.GetChild(0).GetComponent<RawImage>().texture = questSlotBlocked_tx;
                rt.GetChild(1).GetComponent<Text>().text = string.Empty;
            }
        }
    }

    public void QuestButton_OpenQuest(int index) 
    {
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
        openedQuest = index;
        Quest q = visibleQuests[openedQuest];
        questName.text = q.name;
        questDescription.text = q.description;
        // таймер, цена
        // требования шаттлов и команд
    }



    IEnumerator WaitForNewQuest(int i)
    {
        yield return new WaitForSeconds(QUEST_REFRESH_TIME);
        SetNewQuest(i);
    }

    private void DropQuest(int i)
    {
        visibleQuests[i].Stop();
        visibleQuests[i] = null;
        timers[i] = 0;
    }
    private void SetNewQuest(int i)
    {
        if (questAccessMap[i] == false) return;
        // поиск подходящих среди отложенных
       Quest q = new Quest();
        if (!q.picked) {
            if (q.useTimerBeforeTaking) timers[i] = q.questLifeTimer;
            else timers[i] = -1;
        }
        else // autopicked quests
        {
            if (q.useTimerAfterTaking) timers[i] = q.questRealizationTimer;
            else timers[i] = -1;
        }
        visibleQuests[i] = q;
    }  

    private void OnEnable()
    {
        PrepareBasicQuestWindow();
    }

    private void CheckQuestSlots()
    {
        questAccessMap[0] = true; // story and progress
        questAccessMap[1] = true; // trading
        questAccessMap[2] = true; // polytics

        if (Crew.crewsList.Count > 3) {
            int hqLevel = GameMaster.colonyController.hq.level;
            questAccessMap[3] = (hqLevel > 4); //exploring
            questAccessMap[4] = (hqLevel > 5); // problems or jokes
            questAccessMap[5] = (hqLevel > 6); // additional quests
            if (Crew.crewsList.Count > 6 & QuantumTransmitter.transmittersList.Count > 3)
            {
                questAccessMap[6] = (ChemicalFactory.current != null); // science
                questAccessMap[7] = GameMaster.mainChunk.lifePower < 2000; // lifepower quests
                float hc = GameMaster.colonyController.happiness_coefficient;
                questAccessMap[8] = (hc > 0.5f & hc < 1); // himitsu quests
            }
            else
            {
                questAccessMap[6] = false;
                questAccessMap[7] = false;
                questAccessMap[8] = false;
            }
        }
        else
        {
            questAccessMap[3] = false;
            questAccessMap[4] = false;
            questAccessMap[5] = false;
            questAccessMap[6] = false;
            questAccessMap[7] = false;
            questAccessMap[8] = false;
        }
        // добавить проверки на новые квесты для разблокированных ячеек
        
    }

    public void CloseQuestWindow()
    {
        if (openedQuest == -1) gameObject.SetActive(false);
        else ReturnToQuestList();
    }   

    void ReturnToQuestList()
    {
        if (openedQuest == -1) return;
        QuestButton_RestoreButtonRect(openedQuest);
        transformingRectInProgress = true;
        transformingProgress = 0;        
        PrepareBasicQuestWindow();
    }

    void QuestButton_RestoreButtonRect(int i)
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
}
