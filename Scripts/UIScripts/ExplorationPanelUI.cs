using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExplorationPanelUI : MonoBehaviour
{ 
    private enum InfoMode { Inactive,Expeditions, Crews, Artifacts};
#pragma warning disable 0649
    [SerializeField] private GameObject observerPanel, listHolder;
    [SerializeField] private GameObject[] items;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Image expeditionButtonImage, crewButtonImage, artifactButtonImage;
    [SerializeField] private Text emptyPanelText;
#pragma warning restore 0649

    public static ExplorationPanelUI current { get; private set; }

    private bool listEnabled = true, subscribedToUpdate = false;
    private int selectedItem
    {
        get
        {
            return _sltditm;
        }
        set
        {
            if (value < 0 | value > items.Length)
            {
                if (_sltditm != -1) items[_sltditm].GetComponent<Image>().overrideSprite = null;
                _sltditm = -1;                
            }
            else
            {
                if (_sltditm != value)
                {
                    if (_sltditm != -1) items[_sltditm].GetComponent<Image>().overrideSprite = null;
                    _sltditm = value;
                    items[_sltditm].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                }
            }
        }
    }
    private int _sltditm;
    private int lastDrawnActionHash = 0;
    private Artifact selectedArtifact;
    private Crew selectedCrew;
    private GameObject activeObserver;
    private Expedition selectedExpedition;
    private InfoMode mode;
    private int[] listIDs;
    private MainCanvasController mainObserver;
    private CrewsRepresentator crewsData;
    private ExpeditionsRepresentator expeditionsData;
    private ArtifactsRepresentator artifactsData;
    

    public static void Initialize()
    {
        var obs = UIController.GetCurrent().GetMainCanvasController();
        if (current == null)
        {
            current = Instantiate(Resources.Load<GameObject>("UIPrefs/explorationPanel"), obs.GetMainCanvasTransform()).GetComponent<ExplorationPanelUI>();
            current.mainObserver = obs;
            current.crewsData = new CrewsRepresentator(current);
            current.expeditionsData = new ExpeditionsRepresentator(current);
            current.artifactsData = new ArtifactsRepresentator(current);
        }
        current.gameObject.SetActive(true);
        current.OnEnable_Custom();
    }

    private void OnEnable_Custom()
    {
        if (mode == InfoMode.Inactive) ChangeMode(InfoMode.Expeditions);
        if (!subscribedToUpdate)
        {
            mainObserver.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
    }
    public static void Deactivate()
    {
        if (current != null) current.gameObject.SetActive(false);
    }

    public void SelectItem(int i)
    {
        selectedItem = i;
        switch (mode)
        {
            case InfoMode.Expeditions:
                {
                    UIExpeditionObserver.Show(observerPanel.GetComponent<RectTransform>(), SpriteAlignment.TopLeft, Expedition.GetExpeditionByID(listIDs[selectedItem]), false);
                    activeObserver = UIExpeditionObserver.GetObserver().gameObject;
                    if (emptyPanelText.enabled) emptyPanelText.enabled = false;
                    break;
                }
            case InfoMode.Crews:
                {
                    UICrewObserver.Show(observerPanel.GetComponent<RectTransform>(), SpriteAlignment.TopLeft, Crew.GetCrewByID(listIDs[selectedItem]), false);
                    activeObserver = UICrewObserver.GetObserver().gameObject;
                    if (emptyPanelText.enabled) emptyPanelText.enabled = false;
                    break;
                }
            case InfoMode.Artifacts:
                UIArtifactPanel.Show(observerPanel.GetComponent<RectTransform>(), SpriteAlignment.TopLeft, Artifact.GetArtifactByID(listIDs[selectedItem]), false);
                activeObserver = UIArtifactPanel.GetObserver().gameObject;
                if (emptyPanelText.enabled) emptyPanelText.enabled = false;
                break;
        }
        if (activeObserver != null) activeObserver.transform.SetAsLastSibling();
    }

    public void Show(Crew c)
    {
        if (c != null)
        {
            if (mode != InfoMode.Crews) ChangeMode(InfoMode.Crews);
                 
        }
    }
    public void ShowIfSelected(Crew c)
    {
        
    }
    public void Show(Artifact a)
    {
        if (a != null && !a.destructed)
        {
            selectedArtifact = a;
            if (mode != InfoMode.Artifacts) ChangeMode(InfoMode.Artifacts);
            else StatusUpdate();
        }
    }
    public void Show(Expedition e)
    {
        if (e != null)
        {
            selectedExpedition = e;
            if (mode != InfoMode.Expeditions) ChangeMode(InfoMode.Expeditions);
            else StatusUpdate();
        }
    }

    private void PrepareList(IListable datahoster)
    {
        selectedItem = -1;
        int objectsCount = datahoster.GetListLength();
        if (objectsCount == 0)
        {
            if (listEnabled)
            {
                listHolder.SetActive(false);
                listEnabled = false;
            }
            emptyPanelText.enabled = true;
        }
        else
        {
            int COUNT = items.Length;
            listIDs = new int[COUNT];
            int currentSelectedItem = -1;
            if (objectsCount > COUNT)
            {
                if (!datahoster.HaveSelectedObject())
                {
                    int sindex = GetListStartIndex();
                    for (int i = 0; i < COUNT; i++)
                    {
                        items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + datahoster.GetName(i+sindex) + '"';
                        listIDs[i] = datahoster.GetID(i + sindex);
                        items[i].SetActive(true);
                    }
                }

                if (datahoster.HaveSelectedObject())
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (listIDs[i] == datahoster.GetSelectedID())
                        {
                            currentSelectedItem = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                int i = 0, crewsCount = datahoster.GetListLength();
                for (; i < crewsCount; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + datahoster.GetName(i) + '"';
                    listIDs[i] = datahoster.GetID(i);
                    items[i].SetActive(true);
                }
                if (i < items.Length)
                {
                    for (; i < items.Length; i++)
                    {
                        items[i].SetActive(false);
                        listIDs[i] = -1;
                    }
                }

                if (datahoster.HaveSelectedObject())
                {
                    for (i = 0; i < crewsCount; i++)
                    {
                        if (listIDs[i] == datahoster.GetSelectedID())
                        {
                            currentSelectedItem = i;
                            //Debug.Log("selected " + currentSelectedItem.ToString() + " with id " + showingCrew.ID.ToString());
                        }
                    }
                }
            }

            if (currentSelectedItem != -1)
            {
                items[currentSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                selectedItem = currentSelectedItem;
            }

            if (!listEnabled)
            {
                listHolder.SetActive(true);
                listEnabled = true;
            }
            //настройка scrollbar ?
        }
        lastDrawnActionHash = Crew.listChangesMarkerValue;
    }
    private void ChangeMode(InfoMode nmode)
    {
        if (mode != nmode)
        {
            //prev mode            
            switch (mode)
            {
                case InfoMode.Expeditions:
                    selectedExpedition = null;
                    expeditionButtonImage.overrideSprite = null;
                    break;
                case InfoMode.Crews:
                    selectedCrew = null;
                    crewButtonImage.overrideSprite = null;
                    break;
                case InfoMode.Artifacts:
                    selectedArtifact = null;
                    artifactButtonImage.overrideSprite = null;
                    break;
            }
            mode = nmode;
            if (activeObserver != null) activeObserver.SetActive(false);
            
            switch (mode)
            {
                case InfoMode.Crews:
                    {
                        PrepareList(crewsData);
                        crewButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Crew.crewsList.Count != 0)
                        {
                            if (selectedCrew == null) SelectItem(0);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            observerPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoCrews);
                            if (!emptyPanelText.enabled) emptyPanelText.enabled = true;
                        }
                        lastDrawnActionHash = Crew.listChangesMarkerValue;
                        break;
                    }
                case InfoMode.Artifacts:
                    {
                        PrepareList(artifactsData);
                        artifactButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Artifact.artifactsList.Count != 0)
                        {
                            var ert = observerPanel.GetComponent<RectTransform>();
                            var r = new Rect(ert.position, ert.rect.size);
                            SelectItem(0);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            observerPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoArtifacts);
                            if (!emptyPanelText.enabled) emptyPanelText.enabled = true;
                        }
                        lastDrawnActionHash = Artifact.listChangesMarkerValue;
                        break;
                    }
                case InfoMode.Expeditions:
                    {
                        PrepareList(expeditionsData);
                        expeditionButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Expedition.expeditionsList.Count != 0)
                        {
                            var ert = observerPanel.GetComponent<RectTransform>();
                            SelectItem(0);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            observerPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoExpeditions);
                            if (!emptyPanelText.enabled) emptyPanelText.enabled = true;
                        }
                        lastDrawnActionHash = Expedition.listChangesMarker;
                        break;
                    }
                case InfoMode.Inactive:
                    selectedItem = -1;
                    lastDrawnActionHash = -1;
                    break;
            }
        }
    }

    public void ExpeditionsButton() { ChangeMode(InfoMode.Expeditions); }
    public void CrewsButton() { ChangeMode(InfoMode.Crews); }
    public void ArtifactsButton() { ChangeMode(InfoMode.Artifacts); }

    public void StatusUpdate()
    {
        // сами окна информации обновлять не надо - они сами подписаны
        switch (mode)
        {
            case InfoMode.Crews:
                if (lastDrawnActionHash != Crew.listChangesMarkerValue) PrepareList(crewsData);
                break;
            case InfoMode.Artifacts:
                if (lastDrawnActionHash != Artifact.listChangesMarkerValue) PrepareList(artifactsData);
                break;
            case InfoMode.Expeditions:
                if (lastDrawnActionHash != Expedition.listChangesMarker) PrepareList(expeditionsData);
                break;
        }
    }    

    private int GetListStartIndex() // 0-item real index
    {
        int totalListCount = 0, visibleListCount = items.Length;
        switch (mode)
        {
            case InfoMode.Crews: totalListCount = Crew.crewsList.Count; break;
            case InfoMode.Expeditions: totalListCount = Expedition.expeditionsList.Count; break;
            case InfoMode.Artifacts: totalListCount = Artifact.artifactsList.Count; break;
        }

        if (totalListCount < visibleListCount) return 0;
        else
        {
            float sval = scrollbar.value;
            if (sval != 0)
            {
                return ((int)(sval * (totalListCount - visibleListCount)));
            }
            else return 0;
        }
    }
    private void OnDisable()
    {
        if (activeObserver != null)
        {
            activeObserver.SetActive(false);
            activeObserver = null;
        }
        if (subscribedToUpdate)
        {
            mainObserver.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
        ChangeMode(InfoMode.Inactive);
        mainObserver.DropActiveWindow(ActiveWindowMode.ExpeditionPanel);
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            mainObserver.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
    }

    //------------------

    private interface IListable
    {
        string GetSelectedName();
        int GetSelectedID();
        int GetListLength();
        bool HaveSelectedObject();
        string GetName(int index);
        int GetID(int index);
    }
    private class CrewsRepresentator : IListable
    {
        private ExplorationPanelUI dataSource;
        private List<Crew> crewsList;
        public string GetSelectedName() { return dataSource.selectedCrew?.name ?? string.Empty; }
        public int GetSelectedID() { return dataSource.selectedCrew?.ID ?? -1; }
        public int GetListLength() { return crewsList?.Count ?? 0; }
        public bool HaveSelectedObject() { return dataSource.selectedCrew != null; }
        public string GetName(int index) { if (crewsList != null && crewsList.Count > index) return crewsList[index].name; else return string.Empty; }
        public int GetID(int index) { if (crewsList != null && crewsList.Count > index) return crewsList[index].ID; else return -1; }

        public CrewsRepresentator(ExplorationPanelUI master)
        {
            dataSource = master;
            crewsList = Crew.crewsList;
        }
    }
    private class ExpeditionsRepresentator : IListable
    {
        private ExplorationPanelUI dataSource;
        private List<Expedition> expeditionsList;
        public string GetSelectedName() { return dataSource.selectedExpedition?.crew.name ?? string.Empty; }
        public int GetSelectedID() { return dataSource.selectedExpedition?.ID ?? -1; }
        public int GetListLength() { return expeditionsList?.Count ?? 0; }
        public bool HaveSelectedObject() { return dataSource.selectedExpedition != null; }
        public string GetName(int index) { if (expeditionsList != null && expeditionsList.Count > index) return expeditionsList[index]?.crew.name; else return string.Empty; }
        public int GetID(int index) { if (expeditionsList != null && expeditionsList.Count > index) return expeditionsList[index].ID; else return -1; }

        public ExpeditionsRepresentator(ExplorationPanelUI master)
        {
            dataSource = master;
            expeditionsList = Expedition.expeditionsList;
        }
    }
    private class ArtifactsRepresentator : IListable
    {
        private ExplorationPanelUI dataSource;
        private List<Artifact> artifactsList;
        public string GetSelectedName() { return dataSource.selectedArtifact?.name ?? string.Empty; }
        public int GetSelectedID() { return dataSource.selectedArtifact?.ID ?? -1; }
        public int GetListLength() { return artifactsList?.Count ?? 0; }
        public bool HaveSelectedObject() { return dataSource.selectedArtifact != null; }
        public string GetName(int index) { if (artifactsList != null && artifactsList.Count > index) return artifactsList[index]?.name; else return string.Empty; }
        public int GetID(int index) { if (artifactsList != null && artifactsList.Count > index) return artifactsList[index].ID; else return -1; }

        public ArtifactsRepresentator(ExplorationPanelUI master)
        {
            dataSource = master;
            artifactsList = Artifact.artifactsList;
        }
    }
}
