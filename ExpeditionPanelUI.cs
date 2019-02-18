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
    private List<int> dropdownList, dropdownList2;

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
                        if (chosenExpedition.mission != Mission.NoMission)
                        {
                            RefreshExpeditionPreparePanel();
                            infoPanel_expeditionPreparePanel.SetActive(true);
                            removeButton.SetActive(true);
                        }
                        else
                        {
                            infoPanel_expeditionPreparePanel.SetActive(false);
                            removeButton.SetActive(false);
                        }
                        infoPanel_passButton.SetActive(false);
                        infoPanel_scrollView.SetActive(true);
                        infoPanel_hangarButton.SetActive(false);

                        //exp canvas enabling

                        lastExpeditionsHashValue = Expedition.actionsHash;                        
                    }
                    else removeButton.SetActive(false);
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
                        removeButton.SetActive(true);

                        lastShuttlesHashValue = Shuttle.actionsHash;
                    }
                    else removeButton.SetActive(false);
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
                        removeButton.SetActive(true);
                        lastCrewHashValue = Crew.actionsHash;
                    }
                    else removeButton.SetActive(false);
                    break;
                }
        }
        nameField.gameObject.SetActive(enableElements);
        icon.gameObject.SetActive(enableElements);
        progressBarText.transform.parent.gameObject.SetActive(enableElements);
        actionLabel.transform.parent.gameObject.SetActive(enableElements);       
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

    
        }
    }

    private void RefreshShuttlesDropdown()
    {
        var options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoShuttle)));
        dropdownList = new List<int>() { -1 };
        var shuttles = Shuttle.shuttlesList;
        infoPanel_dropdown.enabled = false;
        if (shuttles.Count > 0)
        {
            char c = '\"';
            foreach (Shuttle s in shuttles)
            {
                if (s == null) continue;                
                if (s.crew != null) options.Add(new Dropdown.OptionData(c + s.name + c + " : " + c + s.crew.name + c));
                else
                {
                   options.Add(new Dropdown.OptionData(c + s.name + c));
                }
                dropdownList.Add(s.ID);                
            }
        }
        infoPanel_dropdown.options = options;
        if (chosenCrew.shuttle == null) infoPanel_dropdown.value = 0;
        else
        {
           int id = chosenCrew.shuttle.ID;
           for (int i = 0; i < dropdownList.Count; i++)
            {
                if (dropdownList[i] == id)
                {
                    infoPanel_dropdown.value = i;
                    break;
                }
            }
        }
        infoPanel_dropdown.enabled = true;
        lastShuttlesHashValue = Shuttle.actionsHash;
    }
    private void RefreshCrewsDropdown()
    {
        var options = new List<Dropdown.OptionData>();
        dropdownList = new List<int>() { -1 };
        options.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoCrew)));
        var crews = Crew.crewsList;        
        if (crews.Count > 0)
        {            
            foreach (Crew c in crews)
            {
                if (c == null) continue;
                if (c.shuttle != null) options.Add(new Dropdown.OptionData('\"' + c.name + '\"'));
                else options.Add(new Dropdown.OptionData('\"' + c.name + '\"' + '*'));
                dropdownList.Add(c.ID);
            }
        }
        infoPanel_dropdown.enabled = false;
        infoPanel_dropdown.options = options;
        if (chosenShuttle.crew == null) infoPanel_dropdown.value = 0;
        else
        {
            int id = chosenShuttle.crew.ID;
            for (int i = 0; i < dropdownList.Count; i++)
            {
                if (dropdownList[i] == id)
                {
                    infoPanel_dropdown.value = i;
                    break;
                }
            }
        }
        infoPanel_dropdown.enabled = true;
        lastCrewHashValue = Crew.actionsHash;
    }
    private void RefreshCrewsDropdownForChosenExpedition(Dropdown d)
    {        
        var options = new List<Dropdown.OptionData>();
        dropdownList2 = new List<int>();
        options.Add(new Dropdown.OptionData('<' + Localization.GetPhrase(LocalizedPhrase.AddCrew) + '>'));
        dropdownList2.Add(-1);

        if (chosenExpedition == null || chosenExpedition.mission == Mission.NoMission)
        {
            RefreshExpeditionPreparePanel();
            return;
        }
        else {
            var crews = Crew.crewsList;
            Crew c;
            if (crews.Count > 0)
            {                
                if (!chosenExpedition.mission.requireShuttle)
                {
                    for (int i = 0; i < crews.Count; i++)
                    {
                        c = crews[i];
                        if (c == null) continue;
                        if (c.shuttle != null) options.Add(new Dropdown.OptionData('\"' + c.name + "\" - \" " + c.shuttle.name + '\"'));
                        else options.Add(new Dropdown.OptionData('\"' + c.name + '\"'));
                        dropdownList2.Add(c.ID);
                    }
                }
                else
                {
                    for (int i = 0; i < crews.Count; i++)
                    {
                        c = crews[i];
                        if (c == null) continue;
                        if (c.shuttle != null) options.Add(new Dropdown.OptionData('\"' + c.name + '\"'));
                        dropdownList.Add(c.ID);
                    }
                }                
            }
            d.enabled = false;
            d.value = 0;
            d.options = options;
            d.enabled = true;
            lastCrewHashValue = Crew.actionsHash;
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
                addButton.SetActive(false);
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
                if (chosenExpedition != null && chosenExpedition.mission != Mission.NoMission)
                {
                    int d_id = chosenExpedition.ID;
                    chosenExpedition = null;
                    Expedition.DismissExpedition(d_id);
                }
                else
                {
                    removeButton.SetActive(false);
                }
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
                        chosenCrew.SetShuttle(Shuttle.GetShuttle(dropdownList[i]));
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
                            (Crew.GetCrewByID(dropdownList[i])).SetShuttle(chosenShuttle);
                        }
                        RedrawChosenInfo();                        
                    }
                    break;
                }
            case ExpeditionPanelSection.Expeditions:
                if (chosenExpedition == null)  RedrawChosenInfo();
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

