using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExplorationPanelUI : MonoBehaviour
{ 
    private enum InfoMode { Inactive,Expeditions, Crews, Artifacts};
#pragma warning disable 0649
    [SerializeField] private GameObject observerPanel;
    [SerializeField] private Image expeditionButtonImage, crewButtonImage, artifactButtonImage;
    [SerializeField] private ListController listController;
#pragma warning restore 0649

    public static ExplorationPanelUI current { get; private set; }
    private bool subscribedToUpdate = false;   
    private int lastDrawnActionHash = 0;
    private Artifact selectedArtifact;
    private Crew selectedCrew;
    private GameObject activeObserver;
    private Expedition selectedExpedition;
    private InfoMode mode;
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
            current.listController.AssignSelectItemAction(current.SelectItem);
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

    public void SelectItem(int index)
    {
        switch (mode)
        {
            case InfoMode.Expeditions:
                {
                    UIExpeditionObserver.Show(observerPanel.GetComponent<RectTransform>(), SpriteAlignment.TopLeft, Expedition.expeditionsList[index], false);
                    activeObserver = UIExpeditionObserver.GetObserver().gameObject;
                    break;
                }
            case InfoMode.Crews:
                {
                    UICrewObserver.Show(observerPanel.GetComponent<RectTransform>(), SpriteAlignment.TopLeft, Crew.crewsList[index], false);
                    activeObserver = UICrewObserver.GetObserver().gameObject;
                    break;
                }
            case InfoMode.Artifacts:
                UIArtifactPanel.Show(observerPanel.GetComponent<RectTransform>(), SpriteAlignment.TopLeft, Artifact.artifactsList[index], false);
                activeObserver = UIArtifactPanel.GetObserver().gameObject;
                break;
        }
        if (activeObserver != null) activeObserver.transform.SetAsLastSibling();
    }

    // LIST
    public void ShowInList(Crew c)
    {
        if (c != null)
        {
            selectedCrew = c;
            listController.PrepareList(crewsData);
        }
    }
    //

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
                        listController.PrepareList(crewsData);
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
                            listController.ChangeEmptyLabelText(Localization.GetPhrase(LocalizedPhrase.NoCrews));
                        }
                        lastDrawnActionHash = Crew.listChangesMarkerValue;
                        if (!listController.isActiveAndEnabled) listController.gameObject.SetActive(true);
                        break;
                    }
                case InfoMode.Artifacts:
                    {
                        listController.PrepareList(artifactsData);
                        artifactButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Artifact.artifactsList.Count != 0)
                        {
                            if (selectedArtifact == null) SelectItem(0);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            listController.ChangeEmptyLabelText(Localization.GetPhrase(LocalizedPhrase.NoArtifacts));
                        }
                        lastDrawnActionHash = Artifact.listChangesMarkerValue;
                        if (!listController.isActiveAndEnabled) listController.gameObject.SetActive(true);
                        break;
                    }
                case InfoMode.Expeditions:
                    {
                        listController.PrepareList(expeditionsData);
                        expeditionButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Expedition.expeditionsList.Count != 0)
                        {                           
                            if (selectedExpedition == null) SelectItem(0);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            listController.ChangeEmptyLabelText(Localization.GetPhrase(LocalizedPhrase.NoExpeditions));
                        }
                        lastDrawnActionHash = Expedition.listChangesMarker;
                        if (!listController.isActiveAndEnabled) listController.gameObject.SetActive(true);
                        break;
                    }
                case InfoMode.Inactive:
                    if (listController.isActiveAndEnabled) listController.gameObject.SetActive(false);
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
                if (lastDrawnActionHash != Crew.listChangesMarkerValue) listController.PrepareList(crewsData);
                break;
            case InfoMode.Artifacts:
                if (lastDrawnActionHash != Artifact.listChangesMarkerValue) listController.PrepareList(artifactsData);
                break;
            case InfoMode.Expeditions:
                if (lastDrawnActionHash != Expedition.listChangesMarker) listController.PrepareList(expeditionsData);
                break;
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

    
    private class CrewsRepresentator : IListable
    {
        private ExplorationPanelUI dataSource;
        private List<Crew> crewsList;
        public int GetItemsCount() { return crewsList?.Count ?? 0; }
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
        public int GetItemsCount() { return expeditionsList?.Count ?? 0; }
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
        public int GetItemsCount() { return artifactsList?.Count ?? 0; }
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
