using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionPanelUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button expeditionsButton, shuttlesButton, crewsButton;
    [SerializeField] private Text countString, progressBarText, actionLabel, infoPanel_text;
    [SerializeField] private InputField nameField;
    [SerializeField] private RectTransform listContainer;
    [SerializeField] private RawImage icon;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private Scrollbar itemsScrollbar;
    [SerializeField] private GameObject addButton, removeButton, infoPanel, infoPanel_passButton, 
        infoPanel_scrollView, infoPanel_hangarButton, infoPanel_expeditionPreparePanel;
    [SerializeField] private Dropdown infoPanel_dropdown;
#pragma warning restore 0649
    private bool listConstructed = false, subscribedToUpdate = false;
    private int listStartIndex = 0, lastSelectedItem = -1, lastCrewHashValue = 0, lastShuttlesHashValue = 0, lastExpeditionsHashValue = 0;
    private float itemHeight = 0;

    private enum ExpeditionPanelSection : byte { NoChosenSection, Expeditions, Shuttles, Crews };
    private ExpeditionPanelSection chosenSection = ExpeditionPanelSection.NoChosenSection;
    private Crew chosenCrew;
    private Shuttle chosenShuttle;
    private Expedition chosenExpedition;
    private RectTransform[] items;
    private List<int> expeditionPreparingIDsList;
    private List<MonoBehaviour> dropdownList;

    public void Activate()
    {
        if (!listConstructed) ConstructList();
        if (!subscribedToUpdate) UIController.current.statusUpdateEvent += StatusUpdate;
        RedrawWindow(chosenSection == ExpeditionPanelSection.NoChosenSection ? ExpeditionPanelSection.Expeditions : chosenSection);
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
        UIController uc = UIController.current;
        uc.DropActiveWindow(ActiveWindowMode.ExpeditionPanel);        
    }

    public void StatusUpdate()
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Crews:
                if (lastCrewHashValue != Crew.actionsHash) RedrawWindow(chosenSection);
                if (chosenCrew != null & lastShuttlesHashValue != Shuttle.actionsHash) RefreshShuttlesDropdown();
                break;
            case ExpeditionPanelSection.Shuttles:
                if (lastShuttlesHashValue != Shuttle.actionsHash) RedrawWindow(chosenSection);
                break;
            case ExpeditionPanelSection.Expeditions:
                if (lastExpeditionsHashValue != Expedition.actionsHash) RedrawWindow(chosenSection);
                if (chosenExpedition != null)
                {
                    if (chosenExpedition.mission != Mission.NoMission)
                    {
                        if (chosenExpedition.mission.requireShuttle)
                        {
                            if (lastShuttlesHashValue != Shuttle.actionsHash) RefreshExpeditionPreparePanel();
                        }
                        else
                        {
                            if (lastCrewHashValue != Crew.actionsHash) RefreshExpeditionPreparePanel();
                        }
                    }
                }
                break;
        }
    }

    public void SelectSection(int i)
    {
        ExpeditionPanelSection newSection;
        switch (i)
        {
            case 0: newSection = ExpeditionPanelSection.Expeditions; break;
            case 1: newSection = ExpeditionPanelSection.Shuttles; break;
            case 2: newSection = ExpeditionPanelSection.Crews; break;
            default: newSection = ExpeditionPanelSection.NoChosenSection; return;
        }
        RedrawWindow(newSection);
    }

    private void RedrawWindow(ExpeditionPanelSection newSection)
    {
        if (chosenSection != newSection)
        {
            switch (chosenSection)
            {
                case ExpeditionPanelSection.Expeditions:
                    chosenExpedition = null;
                    expeditionsButton.GetComponent<Image>().overrideSprite = null;
                    break;
                case ExpeditionPanelSection.Shuttles:
                    chosenShuttle = null;
                    shuttlesButton.GetComponent<Image>().overrideSprite = null;
                    break;
                case ExpeditionPanelSection.Crews:
                    chosenCrew = null;
                    crewsButton.GetComponent<Image>().overrideSprite = null;
                    break;
            }
            switch (newSection)
            {
                case ExpeditionPanelSection.Expeditions: expeditionsButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case ExpeditionPanelSection.Shuttles: shuttlesButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case ExpeditionPanelSection.Crews: crewsButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
            }
            if (lastSelectedItem != -1)
            {
                items[lastSelectedItem].GetComponent<Image>().overrideSprite = null;
                lastSelectedItem = -1;
            }
        }
        chosenSection = newSection;

        int expeditionsCount = Expedition.expeditionsList.Count,
            hangarsCount = Hangar.hangarsList.Count,
            recruitingCentersCount = RecruitingCenter.recruitingCentersList.Count,
            shuttlesCount = Shuttle.shuttlesList.Count,
            crewsCount = Crew.crewsList.Count,
            transmittersCount = QuantumTransmitter.transmittersList.Count
            ;

        if (chosenSection == ExpeditionPanelSection.NoChosenSection)
        {
            UIController uc = UIController.current;
            if (uc.currentActiveWindowMode == ActiveWindowMode.ExpeditionPanel) uc.ChangeActiveWindow(ActiveWindowMode.NoWindow);
            return;
        }
        else
        {
            switch (chosenSection)
            {
                case ExpeditionPanelSection.Expeditions:
                    {
                        addButton.SetActive(transmittersCount > expeditionsCount);
                        if (transmittersCount > 0)
                        {
                            countString.text = expeditionsCount.ToString() + " / " + transmittersCount.ToString();
                        }
                        else countString.text = Localization.GetPhrase(LocalizedPhrase.NoTransmitters);
                        RefreshItems();
                        RedrawChosenInfo();
                    }
                    break;
                case ExpeditionPanelSection.Shuttles:
                    {
                        shuttlesButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                        addButton.SetActive(shuttlesCount < hangarsCount);
                        countString.text = shuttlesCount.ToString() + " / " + hangarsCount.ToString();
                        RefreshItems();
                        RedrawChosenInfo();
                    }
                    break;
                case ExpeditionPanelSection.Crews:
                    {
                        crewsButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                        addButton.SetActive(crewsCount < RecruitingCenter.GetCrewsSlotsCount());
                        countString.text = crewsCount.ToString() + " / " + RecruitingCenter.GetCrewsSlotsCount().ToString();
                        RefreshItems();
                        RedrawChosenInfo();
                        break;
                    }
            }
        }
    }
    private void RedrawChosenInfo()
    {
        bool enableElements = false;
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Expeditions:
                {
                    if (chosenExpedition != null) 
                    {
                        // загрузка логов?
                        enableElements = true;
                        nameField.text = chosenExpedition.name;
                        nameField.readOnly = true;

                        chosenExpedition.DrawTexture(icon);

                        progressBarText.text = ((int)(chosenExpedition.progress * 100)).ToString() + " %";
                        progressBarImage.fillAmount = chosenExpedition.progress;
                        progressBarImage.color = PoolMaster.gameOrangeColor;

                        removeButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);

                        RefreshMissionsDropdown();
                        if (chosenExpedition.mission != Mission.NoMission)
                        {
                            RefreshExpeditionPreparePanel();
                            infoPanel_expeditionPreparePanel.SetActive(true);
                        }
                        else
                        {
                            infoPanel_expeditionPreparePanel.SetActive(false);
                        }
                        infoPanel_passButton.SetActive(false);
                        infoPanel_scrollView.SetActive(true);
                        infoPanel_hangarButton.SetActive(false);

                        //exp canvas enabling

                        lastExpeditionsHashValue = Expedition.actionsHash;                        
                    }
                    break;
                }
            case ExpeditionPanelSection.Shuttles:
                {
                    if (chosenShuttle != null)
                    {
                        enableElements = true;
                        nameField.text = chosenShuttle.name;
                        nameField.readOnly = false;

                        chosenShuttle.DrawShuttleIcon(icon);
                        icon.transform.GetChild(0).GetComponent<Text>().text = string.Empty;

                        float condition = chosenShuttle.condition;
                        progressBarText.text = ((int)(condition * 100)).ToString() + " %";
                        progressBarImage.fillAmount = condition;
                        if (condition > Shuttle.BAD_CONDITION_THRESHOLD) progressBarImage.color = PoolMaster.gameOrangeColor;
                        else progressBarImage.color = Color.red;

                        removeButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Disassemble);

                        actionLabel.text = Localization.GetShuttleStatus(chosenShuttle);

                        RefreshCrewsDropdown();
                        infoPanel_expeditionPreparePanel.SetActive(false);
                        infoPanel_passButton.SetActive(chosenShuttle.crew != null);                        
                        infoPanel_text.text = string.Empty;

                        infoPanel_hangarButton.SetActive(true);
                        infoPanel_scrollView.SetActive(true);

                        lastShuttlesHashValue = Shuttle.actionsHash;
                    }
                    break;
                }
            case ExpeditionPanelSection.Crews:
                {
                    if (chosenCrew != null) 
                    {
                        enableElements = true;
                        nameField.text = chosenCrew.name;
                        nameField.readOnly = false;

                        chosenCrew.DrawCrewIcon(icon);
                        icon.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Level) + " : " + chosenCrew.level.ToString();

                        float stamina = chosenCrew.stamina;
                        progressBarText.text = ((int)(stamina * 100)).ToString() + " %";
                        progressBarImage.fillAmount = stamina;
                        if (stamina < Crew.LOW_STAMINA_VALUE) progressBarImage.color = Color.red;
                        else progressBarImage.color = PoolMaster.gameOrangeColor;
                        progressBarText.transform.parent.gameObject.SetActive(true);

                        removeButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);
                        actionLabel.text = Localization.GetCrewStatus(chosenCrew);
                        
                        RefreshShuttlesDropdown();
                        infoPanel_expeditionPreparePanel.gameObject.SetActive(false);
                        infoPanel_scrollView.SetActive(false);
                        infoPanel_hangarButton.SetActive(false);

                        infoPanel_text.text = Localization.GetCrewInfo(chosenCrew);
                        infoPanel_text.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 12 * (infoPanel_text.fontSize + infoPanel_text.lineSpacing));
                        (infoPanel_text.transform.parent as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0,infoPanel_text.rectTransform.rect.height);                        
                        infoPanel_passButton.SetActive(chosenCrew.shuttle != null);
                        lastCrewHashValue = Crew.actionsHash;
                    }
                    break;
                }
        }
        nameField.gameObject.SetActive(enableElements);
        icon.gameObject.SetActive(enableElements);
        progressBarText.transform.parent.gameObject.SetActive(enableElements);
        actionLabel.transform.parent.gameObject.SetActive(enableElements);
        removeButton.SetActive(enableElements);
        infoPanel.SetActive(enableElements);
    }
    private void RefreshItems()
    {
        int itemsCount = items.Length;
        switch (chosenSection)
        {
            case ExpeditionPanelSection.NoChosenSection:
                foreach (RectTransform rt in items) rt.gameObject.SetActive(false);
                itemsScrollbar.gameObject.SetActive(false);
                break;

            case ExpeditionPanelSection.Expeditions:
                {
                    int expeditionsCount = Expedition.expeditionsList.Count;
                    if (expeditionsCount > 0)
                    {
                        int endIndex = itemsCount;
                        if (expeditionsCount > itemsCount)
                        {
                            itemsScrollbar.gameObject.SetActive(true);
                            itemsScrollbar.size = itemsCount / expeditionsCount;
                            itemsScrollbar.value = 0;
                            endIndex = itemsCount;
                        }
                        else
                        {
                            itemsScrollbar.gameObject.SetActive(false);
                            endIndex = expeditionsCount;
                        }
                        int i = 0;
                        List<Expedition> exlist = Expedition.expeditionsList;
                        while (i < endIndex)
                        {
                            Transform t = items[i];
                            Expedition ex = exlist[listStartIndex + i];
                            if (ex == null)
                            {
                                i++;
                                t.gameObject.SetActive(false);
                                continue;
                            }
                            t.GetChild(0).GetComponent<Text>().text = ex.name;
                            if (ex.progress == 0) t.GetChild(2).gameObject.SetActive(false);
                            else
                            {
                                Transform it = t.GetChild(2);
                                it.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (1 - ex.progress) * itemHeight, itemHeight * ex.progress);
                                it.gameObject.SetActive(true);
                            }
                            t.gameObject.SetActive(true);
                            i++;
                        }
                        while (i < itemsCount)
                        {
                            items[i++].gameObject.SetActive(false);
                        }
                    }
                    else goto case ExpeditionPanelSection.NoChosenSection;
                }
                break;
            case ExpeditionPanelSection.Shuttles:
                {
                    int shuttlesCount = Shuttle.shuttlesList.Count;
                    if (shuttlesCount > 0)
                    {
                        int endIndex = itemsCount;
                        if (shuttlesCount > itemsCount)
                        {
                            itemsScrollbar.gameObject.SetActive(true);
                            itemsScrollbar.size = itemsCount / shuttlesCount;
                            itemsScrollbar.value = 0;
                            endIndex = itemsCount;
                        }
                        else
                        {
                            itemsScrollbar.gameObject.SetActive(false);
                            endIndex = shuttlesCount;
                        }
                        int i = 0;
                        List<Shuttle> slist = Shuttle.shuttlesList;
                        while (i < endIndex)
                        {
                            Transform t = items[i];
                            Shuttle s = slist[listStartIndex + i];
                            if (s == null)
                            {
                                i++;
                                t.gameObject.SetActive(false);
                                continue;
                            }
                            t.GetChild(0).GetComponent<Text>().text = '"' + s.name + '"';
                            if (s.condition == 0) t.GetChild(2).gameObject.SetActive(false);
                            else
                            {
                                Transform it = t.GetChild(2);
                                it.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (1 - s.condition) * itemHeight, itemHeight * s.condition);
                                it.gameObject.SetActive(true);
                            }
                            t.gameObject.SetActive(true);
                            i++;
                        }
                        while (i < itemsCount)
                        {
                            items[i++].gameObject.SetActive(false);
                        }
                    }
                    else goto case ExpeditionPanelSection.NoChosenSection;
                }
                break;
            case ExpeditionPanelSection.Crews:
                {
                    int crewsCount = Crew.crewsList.Count;
                    if (crewsCount > 0)
                    {
                        int endIndex;
                        if (crewsCount > itemsCount)
                        {
                            float ccount = crewsCount;
                            itemsScrollbar.size = itemsCount / ccount;
                            endIndex = listStartIndex + itemsCount;
                            if (endIndex >= crewsCount)
                            {
                                endIndex = crewsCount - 1;
                                listStartIndex = crewsCount - itemsCount;
                            }

                            itemsScrollbar.enabled = false;
                            if (listStartIndex == 0) itemsScrollbar.value = 0;
                            else
                            {
                                if (endIndex == crewsCount - 1) itemsScrollbar.value = 1;
                                else itemsScrollbar.value = (listStartIndex + endIndex) / 2f / ccount;
                            }
                            itemsScrollbar.enabled = true;

                            endIndex = itemsCount;
                            itemsScrollbar.gameObject.SetActive(true);
                        }
                        else
                        {
                            itemsScrollbar.gameObject.SetActive(false);
                            itemsScrollbar.size = 1;
                            itemsScrollbar.value = 0;
                            endIndex = crewsCount;
                            listStartIndex = 0;
                        }

                        int i = 0;
                        List<Crew> clist = Crew.crewsList;
                        while (i < endIndex)
                        {
                            Transform t = items[i];
                            Crew c = clist[listStartIndex + i];
                            if (c == null)
                            {
                                i++;
                                t.gameObject.SetActive(false);
                                continue;
                            }
                            t.GetChild(0).GetComponent<Text>().text = '"' + c.name + '"';
                            if (c.stamina == 0) t.GetChild(2).gameObject.SetActive(false);
                            else
                            {
                                Transform it = t.GetChild(2);
                                it.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (1 - c.stamina) * itemHeight, itemHeight * c.stamina);
                                it.gameObject.SetActive(true);
                            }
                            t.gameObject.SetActive(true);
                            i++;
                        }
                        while (i < itemsCount)
                        {
                            items[i++].gameObject.SetActive(false);
                        }
                    }
                    else goto case ExpeditionPanelSection.NoChosenSection;
                }
                break;
        }
    }

    private void RefreshShuttlesDropdown()
    {
        var options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoShuttle)));
        var shuttles = Shuttle.shuttlesList;
        int myShuttleIndex = -1;
        infoPanel_dropdown.enabled = false;
        Shuttle s;
        if (shuttles.Count > 0)
        {
            dropdownList = new List<MonoBehaviour>();
            for (int i = 0; i < shuttles.Count; i++)
            {
                s = shuttles[i];
                if (s == null) continue;
                char c = '\"';
                if (s.crew != null) options.Add(new Dropdown.OptionData(c + s.name + c + " : " + c + s.crew.name + c));
                else
                {
                    if (s == chosenCrew.shuttle)
                    {
                        myShuttleIndex = i;
                        options.Add(new Dropdown.OptionData(c + s.name + c + " <<"));
                    }
                    else
                    {
                        options.Add(new Dropdown.OptionData(c + s.name + c));
                    }
                }
                dropdownList.Add(s);
                
            }
        }
        infoPanel_dropdown.options = options;
        if (chosenCrew.shuttle == null) infoPanel_dropdown.value = 0;
        else
        {
            if (myShuttleIndex != -1) infoPanel_dropdown.value = myShuttleIndex + 1;
        }
        infoPanel_dropdown.enabled = true;
        lastShuttlesHashValue = Shuttle.actionsHash;
    }
    private void RefreshCrewsDropdown()
    {
        var options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoCrew)));
        var crews = Crew.crewsList;
        int myCrewIndex = -1;
        infoPanel_dropdown.enabled = false;
        Crew c;
        if (crews.Count > 0)
        {
            dropdownList = new List<MonoBehaviour>();
            for (int i = 0; i < crews.Count; i++)
            {
                c = crews[i];
                if (c == null) continue;
                if (c.shuttle != null) options.Add(new Dropdown.OptionData('\"' + c.name + '\"'));
                else options.Add(new Dropdown.OptionData('\"' + c.name + '\"' + '*'));
                dropdownList.Add(c);
                if (c == chosenShuttle.crew)
                {
                    myCrewIndex = i;
                }
            }
        }
        infoPanel_dropdown.options = options;
        if (chosenShuttle.crew == null) infoPanel_dropdown.value = 0;
        else
        {
            if (myCrewIndex != -1) infoPanel_dropdown.value = myCrewIndex + 1;
        }
        infoPanel_dropdown.enabled = true;
        lastCrewHashValue = Crew.actionsHash;
    }
    private void RefreshMissionsDropdown()
    {
        var options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoMission)));
        var missions = Mission.missionsList;
        int myMissionIndex = -1;
        infoPanel_dropdown.enabled = false;
        Mission m;
        if (missions.Count > 0)
        {
            dropdownList = new List<MonoBehaviour>();
            for (int i = 0; i < missions.Count; i++)
            {
                m = missions[i];
                options.Add(new Dropdown.OptionData( m.codename));
                if (m == chosenExpedition.mission)
                {
                    myMissionIndex = i;
                }
            }
        }
        infoPanel_dropdown.options = options;
        if (chosenExpedition.mission == Mission.NoMission) infoPanel_dropdown.value = 0;
        else
        {
            if (myMissionIndex != -1) infoPanel_dropdown.value = myMissionIndex + 1;
        }
        infoPanel_dropdown.enabled = true;
        lastExpeditionsHashValue = Expedition.actionsHash;
    }
    private void RefreshExpeditionPreparePanel()
    {
        if (chosenExpedition == null || chosenExpedition.mission == Mission.NoMission)
        {
            infoPanel_expeditionPreparePanel.SetActive(false);
            return;
        }
        else
        {
            Transform ppt = infoPanel_expeditionPreparePanel.transform;
            int childCount = ppt.childCount;
            Mission m = chosenExpedition.mission;

            //participants strings
            int pcount = chosenExpedition.participants.Count;
            if (pcount > 0)
            {
                char c = '"';
                RectTransform example = ppt.GetChild(1) as RectTransform;

                int i = 0;
                for (; i < pcount; i++)
                {
                    Transform pstring;
                    if (i + 1 < childCount) pstring = ppt.GetChild(i + 1);
                    else
                    {
                        pstring = Instantiate(example, ppt);
                        pstring.transform.localPosition = example.transform.localPosition + Vector3.down * example.rect.height * i;
                    }

                    if (chosenExpedition.participants[i] != null)
                    {
                            Crew crew = chosenExpedition.participants[i];
                            pstring.GetChild(0).GetComponent<Text>().text = c + crew.name + c;

                            Button b = pstring.GetChild(1).GetComponent<Button>(); // pass button
                            b.onClick.RemoveAllListeners();
                            int x = crew.ID;
                            b.onClick.AddListener(() => { this.ExpeditionPanel_PassButton(x); });
                            pstring.GetChild(1).gameObject.SetActive(true);

                            b = pstring.GetChild(2).GetComponent<Button>(); // delete button
                            b.onClick.RemoveAllListeners();
                            int y = i;
                            b.onClick.AddListener(() => { this.ExpeditionPanel_RemoveParticipant(y); });
                            pstring.GetChild(2).gameObject.SetActive(true);
                    }

                    pstring.gameObject.SetActive(true);
                }
                i++;
                if (i < childCount)
                {
                    for (; i< childCount; i++)
                    {
                        ppt.GetChild(i).gameObject.SetActive(false);
                    }
                }
                lastCrewHashValue = Crew.actionsHash;
            }
            else
            {
                ppt.GetChild(1).gameObject.SetActive(false);
                if (ppt.childCount > 2)
                {
                    for (int i = 2; i < ppt.childCount;i++)
                    {
                        ppt.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            //dropdown:
            if (pcount < m.requiredParticipantsCount)
            {
                Dropdown d = ppt.GetChild(0).GetComponent<Dropdown>();
                d.enabled = false;
                var optionsList = new List<Dropdown.OptionData>();
                optionsList.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.AddShuttle)));
                expeditionPreparingIDsList = new List<int>();
                expeditionPreparingIDsList.Add(-1);
                if (Crew.crewsList.Count > 0)
                {
                    foreach (Crew c in Crew.crewsList)
                    {
                        if (c.status == CrewStatus.Free)
                        {
                            optionsList.Add(new Dropdown.OptionData(c.name));
                            expeditionPreparingIDsList.Add(c.ID);
                        }
                    }
                    lastShuttlesHashValue = Shuttle.actionsHash;
                }
                d.options = optionsList;
                d.value = 0;
                d.enabled = true;
                d.gameObject.SetActive(true);
            }
            else
            {
                ppt.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void ScrollbarChanged()
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.NoChosenSection: return;
            case ExpeditionPanelSection.Crews:
                {
                    int crewsCount = Crew.crewsList.Count, itemsCount = items.Length;
                    float sval = itemsScrollbar.value, ssize = itemsScrollbar.size;
                    if (sval != 0)
                    {
                        if (sval != 1)
                        {
                            listStartIndex = (int)((sval - ssize / 2f) * crewsCount);
                            if (listStartIndex < 0) listStartIndex = 0;
                            else
                            {
                                if (listStartIndex > crewsCount - itemsCount) listStartIndex = crewsCount - itemsCount;
                            }
                        }
                        else listStartIndex = crewsCount - itemsCount;
                    }
                    else listStartIndex = 0;

                    if (lastSelectedItem != -1)
                    {
                        items[lastSelectedItem].GetComponent<Image>().overrideSprite = null;
                        lastSelectedItem = -1;
                    }

                    List<Crew> clist = Crew.crewsList;
                    for (int i = 0; i < itemsCount; i++)
                    {
                        RectTransform rt = items[i];
                        Crew c = clist[listStartIndex + i];
                        if (c == null)
                        {
                            rt.gameObject.SetActive(false);
                            continue;
                        }
                        else
                        {
                            if (c == chosenCrew)
                            {
                                lastSelectedItem = i;
                                items[lastSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                            }
                            rt.GetChild(0).GetComponent<Text>().text = c.name;
                            if (c.stamina == 0) rt.GetChild(2).gameObject.SetActive(false);
                            else
                            {
                                Transform it = rt.GetChild(2);
                                it.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (1 - c.stamina) * itemHeight, itemHeight * c.stamina);
                                it.gameObject.SetActive(true);
                            }
                            rt.gameObject.SetActive(true);
                        }
                    }
                    break;
                }
        }
    }

    public void SelectItem(int i)
    {
        if (lastSelectedItem != i)
        {
            if (lastSelectedItem != -1)
            {
                items[lastSelectedItem].GetComponent<Image>().overrideSprite = null;
            }
            items[i].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
            lastSelectedItem = i;
        }

        i += listStartIndex;
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Expeditions:
                if (i < Expedition.expeditionsList.Count)
                {
                    chosenExpedition = Expedition.expeditionsList[i];
                    break;
                }
                else
                {
                    RefreshItems();
                    return;
                }
            case ExpeditionPanelSection.Shuttles:
                if (i < Shuttle.shuttlesList.Count)
                {
                    chosenShuttle = Shuttle.shuttlesList[i];
                    break;
                }
                else
                {
                    RefreshItems();
                    return;
                }
            case ExpeditionPanelSection.Crews:
                if (i < Crew.crewsList.Count)
                {
                    chosenCrew = Crew.crewsList[i];
                    break;
                }
                else
                {
                    RefreshItems();
                    return;
                }
        }
        RedrawChosenInfo();
    }    

    private void ConstructList()
    {
        listConstructed = true;
        Transform itemsHolder = listContainer.GetChild(0);
        RectTransform itemExample = itemsHolder.GetChild(0).GetComponent<RectTransform>();
        itemHeight = itemExample.rect.height;
        int count = (int)(itemsHolder.GetComponent<RectTransform>().rect.height / itemHeight) + 1;
        items = new RectTransform[count];
        items[0] = itemExample;
        for (int i = 1; i < count; i++)
        {
            RectTransform rt = Instantiate(itemExample, itemsHolder) as RectTransform;
            rt.localPosition = itemExample.localPosition + Vector3.down * (i) * itemHeight;
            items[i] = rt;
            int buttonIndex = i;
            rt.GetComponent<Button>().onClick.AddListener(() => { this.SelectItem(buttonIndex); });
        }
        items[0].GetComponent<Button>().onClick.AddListener(() => { this.SelectItem(0); });
    }
   

    public void ChangeName(string s)
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Shuttles:
                if (chosenShuttle != null) chosenShuttle.name = s;
                RedrawWindow(chosenSection);
                break;
            case ExpeditionPanelSection.Crews:
                if (chosenCrew != null) chosenCrew.name = s;
                RedrawWindow(chosenSection);
                break;
        }
    }
    public void AddButton()
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Expeditions:
                if (QuantumTransmitter.transmittersList.Count - Expedition.expeditionsList.Count > 0)
                {
                    chosenExpedition = Expedition.CreateNewExpedition();
                    RedrawWindow(chosenSection);
                }
                else addButton.SetActive(false);
                break;
            case ExpeditionPanelSection.Shuttles:
                {
                    int c = Hangar.hangarsList.Count;
                    if (c > 0 && c > Shuttle.shuttlesList.Count)
                    {
                        Hangar reservedVariant = null;
                        foreach (Hangar h in Hangar.hangarsList)
                        {
                            if (h.shuttle == null)
                            {
                                if (h.constructing)
                                {
                                    reservedVariant = h;
                                }
                                else
                                {
                                    // #switching to hangar
                                    Deactivate();
                                    UIController.current.Select(h);
                                    //
                                    return;
                                }
                            }
                        }
                        if (reservedVariant != null)
                        {
                            // #switching to hangar
                            Deactivate();
                            UIController.current.Select(reservedVariant);
                            //
                            return;
                        }
                    }
                    else addButton.SetActive(false);
                    break;
                }
            case ExpeditionPanelSection.Crews:
                {
                    if (Crew.crewsList.Count < RecruitingCenter.GetCrewsSlotsCount())
                    {
                        RecruitingCenter reservedVariant = null;
                        foreach (RecruitingCenter rc in RecruitingCenter.recruitingCentersList)
                        {
                            if (rc.finding) reservedVariant = rc;
                            else
                            {
                                // #switching to recruiting center
                                Deactivate();
                                UIController.current.Select(rc);
                                return;
                                //
                            }
                        }
                        if (reservedVariant != null)
                        {
                            // #switching to recruiting center
                            Deactivate();
                            UIController.current.Select(reservedVariant);
                            return;
                            //
                        }
                    }
                    else addButton.SetActive(false);
                    break;
                }
        }
    }
    public void RemoveButton()
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Expeditions:
                // awaiting
                break;
            case ExpeditionPanelSection.Shuttles:
                if (chosenShuttle != null) chosenShuttle.Deconstruct();
                else removeButton.SetActive(false);
                break;
            case ExpeditionPanelSection.Crews:
                if (chosenCrew != null) chosenCrew.Dismiss();
                else removeButton.SetActive(false);
                break;
        }
        RedrawWindow(chosenSection);
    }

    public void InfoPanel_SetDropdownValue(int i)
    {
        switch (chosenSection)
        {            
            case ExpeditionPanelSection.Crews:
                {
                    if (chosenCrew == null)
                    {
                        RedrawChosenInfo();
                        break;
                    }
                    if (i == 0) chosenCrew.SetShuttle(null);
                    else
                    {
                        chosenCrew.SetShuttle(dropdownList[i - 1] as Shuttle);
                    }
                    RedrawChosenInfo();
                    break;
                }
            case ExpeditionPanelSection.Shuttles:
                {
                    if (chosenShuttle == null)
                    {
                        RedrawChosenInfo();
                    }
                    else
                    {
                        if (i == 0) {
                            if (chosenShuttle.crew != null) chosenShuttle.crew.SetShuttle(null);
                            else chosenShuttle.SetCrew(null);
                        }
                        else
                        {
                            (dropdownList[i - 1] as Crew).SetShuttle(chosenShuttle);
                        }
                        RedrawChosenInfo();                        
                    }
                    break;
                }
            case ExpeditionPanelSection.Expeditions:
                if (chosenExpedition == null)  RedrawChosenInfo();
                else
                {
                    if (i != 0) chosenExpedition.SetMission(Mission.missionsList[i - 1]);
                    else chosenExpedition.DropMission();
                }
                break;
        }
    }
    public void InfoPanel_PassButton()
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Crews:
                {
                    if (chosenCrew == null) RedrawChosenInfo();
                    else
                    {
                        if (chosenCrew.shuttle != null)
                        {
                            chosenShuttle = chosenCrew.shuttle;
                            RedrawWindow(ExpeditionPanelSection.Shuttles);
                        }
                        else infoPanel_passButton.SetActive(false);

                    }
                    break;
                }
            case ExpeditionPanelSection.Shuttles:
                {
                    if (chosenShuttle == null) RedrawChosenInfo();
                    else
                    {
                        if (chosenShuttle.crew != null)
                        {
                            chosenCrew = chosenShuttle.crew;
                            RedrawWindow(ExpeditionPanelSection.Crews);
                        }
                        else infoPanel_passButton.SetActive(false);

                    }
                    break;
                }
        }
    }
    public void InfoPanel_HangarButton()
    {
        if (chosenShuttle != null)
        {
            Deactivate();
            UIController.current.Select(chosenShuttle.hangar);
        }
        else RedrawChosenInfo();
    }
    public void ExpeditionDropdown_SetParticipant(int i)
    {
        if (chosenExpedition == null) {
            infoPanel_expeditionPreparePanel.SetActive(false);
            return;
        }
        if (expeditionPreparingIDsList[i] != - 1)
        {
            //Crew c = Crew.(expeditionPreparingIDsList[i]);
           // if (s != null)
           // {
            //    s.AssignTo(chosenExpedition);
            //    RefreshExpeditionPreparePanel();
           // }
        }
    }
    public void ExpeditionPanel_PassButton(int i)
    {
        if (chosenExpedition != null )
        {
            if (chosenExpedition.mission.requireShuttle)
            {
                Shuttle s = Shuttle.GetShuttle(i);
                if (s != null)
                {
                    chosenShuttle = s;
                    SelectSection(1);
                }
                else RefreshExpeditionPreparePanel();
            }
            else
            {
                //for crews
            }
        }
    }
    public void ExpeditionPanel_RemoveParticipant(int i)
    {
        if (chosenExpedition != null && (chosenExpedition.participants.Count > i ))
        {
            
            RefreshExpeditionPreparePanel();
        }
    }

    private void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        if (subscribedToUpdate)
        {
            UIController uc = UIController.current;
            if (uc != null) uc.statusUpdateEvent -= StatusUpdate;
        }
    }
}

