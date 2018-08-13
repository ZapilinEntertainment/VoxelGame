using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum QuestTexturesEnum { Blocked, Awaiting, UseBuildingIcon, UseResourceFrame}

public sealed class QuestUI : MonoBehaviour {
    [SerializeField] RectTransform[] questButtons; // fiti
    [SerializeField] GameObject questInfoPanel, shuttlesOrCrewsOptions; // fiti
    [SerializeField] RectTransform stepsContainer, listContainer; // fiti
    [SerializeField] Text questName, questDescription, timer; // fiti    
    float rectTransformingSpeed = 0.8f, transformingProgress;
    RectTransform transformingRect; Vector2 resultingAnchorMin, resultingAnchorMax;
    int openedQuest = -1;
    bool transformingRectInProgress = false;
    bool prepareCrewsList = false; // else prepare shuttles list
    Quest[] visibleQuests ;
    bool[] questAccessMap;
    float[] timers;
    float questUpdateTimer = 0;
    

    const float QUEST_REFRESH_TIME = 60, QUEST_UPDATE_TIME = 1;
    public static Texture questBlocked_tx { get; private set; }
    public static Texture questAwaiting_tx { get; private set; }
    public static Texture questBuildingBack_tx { get; private set; }
    public static Texture questResourceBack_tx { get; private set; }

    public static void LoadTextures()
    {
        questBlocked_tx = Resources.Load<Texture>("Textures/questUnacceptableIcon");
        questAwaiting_tx = Resources.Load<Texture>("Textures/questAwaiting"); 
        questBuildingBack_tx = Resources.Load<Texture>("Textures/quest_buildingFrame");
        questResourceBack_tx = Resources.Load<Texture>("Textures/quest_resourceFrame");
    }

    private void Awake()
    {
        int l = questButtons.Length;
        visibleQuests = new Quest[l];
        questAccessMap = new bool[l];
        CheckQuestSlots();
        timers = new float[l];       
        questUpdateTimer = QUEST_UPDATE_TIME;
    }

    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        PrepareBasicQuestWindow();
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
                    transform.GetChild(0).gameObject.SetActive(false); // buttons container
                    questInfoPanel.SetActive(true);
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
                if (q.picked)  DropQuest(i); //?????
                else  SetNewQuest(i);
            } 
            else
            {
               if (checkConditions) q.CheckQuestConditions();
            }
        }
        

    }

    private void PrepareBasicQuestWindow() // открыть окно всех квестов
    {
        openedQuest = -1;
        transform.GetChild(0).gameObject.SetActive(true); // quest buttons
        transform.GetChild(1).gameObject.SetActive(false); // quest info
        for (int i = 0; i < questButtons.Length; i++)
        {
            questButtons[i].gameObject.SetActive(true);
            RectTransform rt = questButtons[i];
            if (questAccessMap[i] == true)
            {
                Quest q = visibleQuests[i];                                          
                if (q != null)
                {
                    questButtons[i].GetComponent<Button>().interactable = true;
                    rt.GetChild(0).GetComponent<RawImage>().texture = Quest.GetQuestTexture(q.ID);
                    Text t = rt.GetChild(1).GetComponent<Text>();
                    t.text = q.name;
                    t.color = (q.picked ? Color.cyan : Color.white);
                }
                else
                {
                    questButtons[i].GetComponent<Button>().interactable = false;
                    SetTexture(QuestTexturesEnum.Awaiting, rt.GetChild(0).GetComponent<RawImage>());
                    Text t = rt.GetChild(1).GetComponent<Text>();
                    t.text = "...";
                    t.color = Color.grey;
                }
            }
            else
            {
                questButtons[i].GetComponent<Button>().interactable = false;
                SetTexture(QuestTexturesEnum.Blocked, rt.GetChild(0).GetComponent<RawImage>());
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
        if (timers[index] != -1) timer.text = string.Format("{0:0.00}", timers[index]);
        if (q.shuttlesRequired == 0)
        {
            if (q.crewsRequired == 0)
            {
                shuttlesOrCrewsOptions.SetActive(false);
                stepsContainer.anchorMax = Vector2.one;
            }
            else
            {
                shuttlesOrCrewsOptions.SetActive(true);
                stepsContainer.anchorMax = new Vector2(0.5f, 1);
                shuttlesOrCrewsOptions.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Crews);
                prepareCrewsList = true;
            }
        }
        else
        {
            shuttlesOrCrewsOptions.SetActive(true);
            stepsContainer.anchorMax = new Vector2(0.5f, 1);
            shuttlesOrCrewsOptions.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Shuttles);
            prepareCrewsList = false;
        }
        PrepareList();
        // цена и кнопка запуска
    }
    private void PrepareList()
    {
        Quest q = visibleQuests[openedQuest];
        if (prepareCrewsList)
        {
            int count = q.crews.Count, i = 0;
            if (count != 0)
            {                
                for (; i < count; i++)
                {
                    Transform t = listContainer.GetChild(i + 1);
                    if (t == null)  t = Instantiate(listContainer.GetChild(1), listContainer).transform;
                    t.GetComponent<Text>().text = '\"' +q.crews[i].name + '\"';
                    int rindex = i;
                    t.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
                        this.RemoveItemFromList(rindex);
                    });
                }
            }
            count = listContainer.childCount;
            if (i < count)
            {
                for (; i < count; i++)
                {
                    listContainer.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Expedition e = q.expedition;
            int count = 0, i = 0;
            if (e != null)
            {
                count = e.shuttles.Count;
                if (count != 0)
                {
                    for (; i < count; i++)
                    {
                        Transform t = listContainer.GetChild(i + 1);
                        if (t == null) t = Instantiate(listContainer.GetChild(1), listContainer).transform;
                        t.GetComponent<Text>().text = '\"' + e.shuttles[i].name + '\"';
                        int rindex = i;
                        t.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                        {
                            this.RemoveItemFromList(rindex);
                        });
                    }
                }
            }
            count = listContainer.childCount;
            if (i < count)
            {
                for (; i < count; i++)
                {
                    listContainer.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }
    public void RemoveItemFromList(int index)
    {
        if (prepareCrewsList) visibleQuests[openedQuest].RemoveCrew(index);
        else visibleQuests[openedQuest].expedition.DetachShuttle(index);
    }
    public void PrepareDropdown()
    {
        Quest q = visibleQuests[openedQuest];
        List<Dropdown.OptionData> buttons = new List<Dropdown.OptionData>();
        if (prepareCrewsList)
        {
            // готовить список команд            
            var crews = q.crews;
            if (crews.Count > 0)
            {
                for (int i = 0; i < crews.Count; i++)
                {
                    buttons.Add(new Dropdown.OptionData('\"' + crews[i].name + '\"'));
                }
            }
        }
        else
        {
            // готовить список челноков
            var shuttles = q.expedition.shuttles;
            if (shuttles.Count > 0)
            {
                for (int i = 0; i < shuttles.Count; i++)
                {
                    buttons.Add(new Dropdown.OptionData('\"' + shuttles[i].name + '\"'));
                }
            }            
        }
        Dropdown d = listContainer.GetChild(0).GetComponent<Dropdown>();
        d.options = buttons;
        d.RefreshShownValue();
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
        if (questAccessMap[i] == false)
        {
            visibleQuests[i] = null;
            return;
        }
        // поиск подходящих среди отложенных
       Quest q ;
        switch (i)
        {
            default: return;
            case Quest.PROGRESS_QUESTS_INDEX: q = Quest.GetProgressQuest(); break;
        }
        if (!q.picked) {
            timers[i] = q.questLifeTimer;
        }
        else // autopicked quests
        {
            timers[i] = q.questRealizationTimer;
        }
        visibleQuests[i] = q;
        if (openedQuest == -1) PrepareBasicQuestWindow();
        print("new quest set");
    }  

    private void CheckQuestSlots()
    {
        questAccessMap[Quest.PROGRESS_QUESTS_INDEX] = true; // story and progress
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
        if (openedQuest == -1) {
            GetComponent<Image>().enabled = false;
            questButtons[0].transform.parent.gameObject.SetActive(false);
            questInfoPanel.SetActive(false);
            UIController.current.ActivateLeftPanel();
        }
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

    #region quests checks
    public void CheckProgressQuest()
    {
        if (visibleQuests[Quest.PROGRESS_QUESTS_INDEX] == null) SetNewQuest(Quest.PROGRESS_QUESTS_INDEX);
    }
    #endregion

    public static void SetTexture(QuestTexturesEnum qte, RawImage ri)
    {
        switch (qte)
        {
            case QuestTexturesEnum.Awaiting:
                ri.texture = questAwaiting_tx;
                ri.uvRect = new Rect(0, 0, questAwaiting_tx.width, questAwaiting_tx.height);
                break;
            case QuestTexturesEnum.Blocked:
                ri.texture = questBlocked_tx;
                ri.uvRect = new Rect(0, 0, questBlocked_tx.width, questBlocked_tx.height);
                break;
            case QuestTexturesEnum.UseBuildingIcon:
                ri.texture = questBuildingBack_tx;
                ri.uvRect = new Rect(0, 0, questBuildingBack_tx.width, questBuildingBack_tx.height);
                break;
            case QuestTexturesEnum.UseResourceFrame:
                ri.texture = questResourceBack_tx;
                ri.uvRect = new Rect(0, 0, questResourceBack_tx.width, questResourceBack_tx.height);
                break;
        }
        
    }
}

