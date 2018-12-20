using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionPanelUI : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private Button expeditionsButton, shuttlesButton, crewsButton, addButton;
    [SerializeField] private Text expeditionLog, countString, progressBarText, actionLabel;
    [SerializeField] private InputField nameField;
    [SerializeField] private RectTransform listContainer;
    [SerializeField] private RawImage icon;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private Scrollbar itemsScrollbar;
#pragma warning restore 0649
    private bool listConstructed = false;
    private int listStartIndex = 0, lastSelectedItem = -1;
    private float itemHeight = 0;

    private enum ExpeditionPanelSection : byte { NoChosenSection, Expeditions, Shuttles, Crews};
    private ExpeditionPanelSection chosenSection = ExpeditionPanelSection.NoChosenSection;
    private Crew chosenCrew;
    private Shuttle chosenShuttle;
    private Expedition chosenExpedition;
    private RectTransform[] items;

    public void Activate()
    {
        if (!listConstructed) ConstructList();
        Redraw(chosenSection == ExpeditionPanelSection.NoChosenSection ? ExpeditionPanelSection.Expeditions : chosenSection);
        gameObject.SetActive(true);
    }

    private void Redraw (ExpeditionPanelSection newSection)
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
                case ExpeditionPanelSection.Expeditions:  expeditionsButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case ExpeditionPanelSection.Shuttles: shuttlesButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case ExpeditionPanelSection.Crews:  crewsButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
            }
            if (lastSelectedItem != -1)
            {
                items[lastSelectedItem].GetComponent<Image>().overrideSprite = null;
                lastSelectedItem = -1;
            }
        }
        chosenSection = newSection;        

        int expeditionsCorpusesCount = ExpeditionCorpus.expeditionCorpusesList.Count,
            expeditionsCount = Expedition.expeditionsList.Count,
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
                        addButton.interactable = (expeditionsCorpusesCount > 0) & (shuttlesCount > 0) & (QuantumTransmitter.transmittersList.Count > 0);
                        countString.text = expeditionsCount.ToString() + " / " + ExpeditionCorpus.GetExpeditionsSlotsCount().ToString();
                        RefreshItems();
                        //
                        // выбранная экспедиция:
                        if (chosenExpedition != null)
                        {
                            expeditionLog.enabled = true;
                            // загрузка логов
                            nameField.text = chosenExpedition.quest.name;
                            nameField.readOnly = true;
                            nameField.gameObject.SetActive(true);

                            Quest.SetQuestTexture(chosenExpedition.quest, null, icon);
                            icon.gameObject.SetActive(true);

                            progressBarText.text = ((int)(chosenExpedition.progress * 100)).ToString() + " %";
                            progressBarImage.fillAmount = chosenExpedition.progress;
                            progressBarImage.color = PoolMaster.gameOrangeColor;
                            progressBarText.transform.parent.gameObject.SetActive(true);

                            //actionLabel.enabled = true;
                            // запись в actionlabel
                        }
                        else
                        {
                            nameField.gameObject.SetActive(false);
                            icon.gameObject.SetActive(false);
                            progressBarText.transform.parent.gameObject.SetActive(false);
                            actionLabel.transform.parent.gameObject.SetActive(false);
                        }
                    }
                    break;
                case ExpeditionPanelSection.Shuttles:
                    {
                        shuttlesButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                        addButton.interactable = (hangarsCount > 0) & (shuttlesCount < hangarsCount);
                        expeditionLog.enabled = false;
                        countString.text = shuttlesCount.ToString() + " / " + hangarsCount.ToString();
                        RefreshItems();

                        if (chosenShuttle != null)
                        {
                            nameField.text = chosenShuttle.name;
                            nameField.readOnly = false;
                            nameField.gameObject.SetActive(true);

                            chosenShuttle.DrawShuttleIcon(icon);
                            icon.gameObject.SetActive(true);

                            float condition = chosenShuttle.condition;
                            progressBarText.text = ((int)(condition * 100)).ToString() + " %";
                            progressBarImage.fillAmount = condition;
                            if (condition > Shuttle.BAD_CONDITION_THRESHOLD) progressBarImage.color = PoolMaster.gameOrangeColor;
                            progressBarImage.color = Color.red;
                            progressBarText.transform.parent.gameObject.SetActive(true);

                            // запись в actionlabel
                        }
                        else
                        {
                            nameField.gameObject.SetActive(false);
                            icon.gameObject.SetActive(false);
                            progressBarText.transform.parent.gameObject.SetActive(false);
                            actionLabel.transform.parent.gameObject.SetActive(false);
                        }
                    }
                    break;
                case ExpeditionPanelSection.Crews:
                    {
                        crewsButton.GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                        addButton.interactable = (crewsCount > 0);
                        expeditionLog.enabled = false;
                        countString.text = crewsCount.ToString() + " / " + RecruitingCenter.GetCrewsSlotsCount().ToString();
                        RefreshItems();                       

                        if (chosenCrew != null)
                        {
                            nameField.text = chosenCrew.name;
                            nameField.readOnly = false;
                            nameField.gameObject.SetActive(true);

                            chosenCrew.DrawCrewIcon(icon);
                            icon.gameObject.SetActive(true);

                            float stamina = chosenCrew.stamina;
                            progressBarText.text = ((int)(stamina * 100)).ToString() + " %";
                            progressBarImage.fillAmount = stamina;
                            if (stamina < Crew.LOW_STAMINA_VALUE) progressBarImage.color = Color.red;
                            else progressBarImage.color = PoolMaster.gameOrangeColor;
                            progressBarText.transform.parent.gameObject.SetActive(true);

                            //actionLabel.enabled = true;
                            // запись в actionlabel
                        }
                        else
                        {
                            nameField.gameObject.SetActive(false);
                            icon.gameObject.SetActive(false);
                            progressBarText.transform.parent.gameObject.SetActive(false);
                            actionLabel.transform.parent.gameObject.SetActive(false);
                        }
                        break;
                    }
            }
        }
    }

    public void SelectSection (int i)
    {
        ExpeditionPanelSection newSection;
        switch (i)
        {
            case 0: newSection = ExpeditionPanelSection.Expeditions;break;
            case 1: newSection = ExpeditionPanelSection.Shuttles; break;
            case 2: newSection = ExpeditionPanelSection.Crews; break;
            default: newSection = ExpeditionPanelSection.NoChosenSection; return;
        }
        Redraw(newSection);
    }
    public void ChangeName(string s)
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Shuttles:
                if (chosenShuttle != null) chosenShuttle.name = s;
                else Redraw(chosenSection);
                break;
            case ExpeditionPanelSection.Crews:
                if (chosenCrew != null) chosenCrew.name = s;
                else Redraw(chosenSection);
                break;
        }
    }
    public void AddButton()
    {
        switch (chosenSection)
        {
            case ExpeditionPanelSection.Expeditions:
                if (ExpeditionCorpus.GetExpeditionsSlotsCount() - Expedition.expeditionsList.Count > 0)
                {
                    chosenExpedition = new Expedition();
                    Redraw(chosenSection);
                }
                else addButton.interactable = false;
                break;
            case ExpeditionPanelSection.Shuttles:
                {
                    int c = Hangar.hangarsList.Count;
                    if ( c > 0 && c > Shuttle.shuttlesList.Count)
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
                                    gameObject.SetActive(false);
                                    h.ShowOnGUI();
                                    return;
                                }
                            }
                        }
                        if (reservedVariant != null)
                        {
                            gameObject.SetActive(false);
                            reservedVariant.ShowOnGUI();
                            return;
                        }
                    }
                    else addButton.interactable = false;
                    break;
                }
            case ExpeditionPanelSection.Crews:
                {
                    int c = Crew.crewsList.Count;
                    if (c > 0 && c < RecruitingCenter.GetCrewsSlotsCount())
                    {
                        RecruitingCenter reservedVariant = null;
                        foreach (RecruitingCenter rc in RecruitingCenter.recruitingCentersList)
                        {
                            if (rc.finding) reservedVariant = rc;
                            else
                            {
                                gameObject.SetActive(false);
                                rc.ShowOnGUI();
                                return;
                            }
                        }
                        if (reservedVariant != null)
                        {
                            gameObject.SetActive(false);
                            reservedVariant.ShowOnGUI();
                            return;
                        }
                }
                    else addButton.interactable = false;
                    break;
                }
        }
    }
    public void CloseButton()
    {
        UIController uc = UIController.current;
        if (uc.currentActiveWindowMode == ActiveWindowMode.ExpeditionPanel) uc.ChangeActiveWindow(ActiveWindowMode.NoWindow);
        gameObject.SetActive(false);
    }

    public void SelectItem(int i)
    {
        if (lastSelectedItem != i) {
            if (lastSelectedItem != -1)
            {
                items[lastSelectedItem].GetComponent<Image>().overrideSprite = null;
            }
            items[i].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
            lastSelectedItem = i;
        }
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
        Redraw(chosenSection);
        //
    }

    private void ConstructList()
    {
        listConstructed = true;
        Transform itemsHolder = listContainer.GetChild(0);
        RectTransform itemExample = itemsHolder.GetChild(0).GetComponent<RectTransform>();
        itemHeight = itemExample.rect.height;
        int count = (int)(itemsHolder.GetComponent<RectTransform>().rect.height / itemHeight);
        items = new RectTransform[count + 1];
        items[0] = itemExample;
        for (int i = 0; i < count ; i++)
        {
            RectTransform rt = Instantiate(itemExample, itemsHolder) as RectTransform;
            rt.localPosition = itemExample.localPosition + Vector3.down * (i + 1) * itemHeight;
            items[i + 1] = rt;
            int buttonIndex = i;
            rt.GetComponent<Button>().onClick.AddListener(() => { this.SelectItem(buttonIndex); });
        }       
        items[0].GetComponent<Button>().onClick.AddListener(() => { this.SelectItem(0); });
    }

    private void RefreshItems()
    {
        int itemsCount = items.Length;
        switch (chosenSection)
        {
            case ExpeditionPanelSection.NoChosenSection:
                foreach (RectTransform rt in items) rt.gameObject.SetActive(false);
                itemsScrollbar.enabled = false;
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
                            t.GetChild(0).GetComponent<Text>().text = ex.quest.name;
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
                            t.GetChild(0).GetComponent<Text>().text = s.name;
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
                        int endIndex = itemsCount;
                        if (crewsCount > itemsCount)
                        {
                            itemsScrollbar.gameObject.SetActive(true);
                            itemsScrollbar.size = itemsCount / crewsCount;
                            itemsScrollbar.value = 0;
                            endIndex = itemsCount;
                        }
                        else
                        {
                            itemsScrollbar.gameObject.SetActive(false);
                            endIndex = crewsCount;
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
                            t.GetChild(0).GetComponent<Text>().text = c.name;
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
}

