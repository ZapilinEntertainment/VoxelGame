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
    private int selectedItem = -1, lastDrawnActionHash = 0;
    private Artifact showingArtifact;
    private Crew showingCrew;
    private GameObject activeObserver;
    private Expedition showingExpedition;
    private InfoMode mode;
    private List<int> listIDs;
    

    public static void Initialize()
    {
        if (current == null) current = Instantiate(Resources.Load<GameObject>("UIPrefs/explorationPanel"), UIController.current.mainCanvas).GetComponent<ExplorationPanelUI>();
        current.gameObject.SetActive(true);
    }
    public static void Deactivate()
    {
        if (current != null) current.gameObject.SetActive(false);
    }
    public static void RestoreSession(Expedition e)
    {
        if (current == null) Initialize();
        else if (!current.gameObject.activeSelf) current.gameObject.SetActive(true);
        current.Show(e);
    }

    public void SelectItem(int i)
    {
        if (selectedItem != i)
        {
            if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
            selectedItem = i;
            items[selectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
        }
        var realIndex = i + GetListStartIndex();
        switch (mode)
        {
            case InfoMode.Expeditions:
                {
                    if (realIndex >= Expedition.expeditionsList.Count) PrepareExpeditionsList();
                    else
                    {
                        var e = Expedition.expeditionsList[realIndex];
                        var ert = observerPanel.GetComponent<RectTransform>();
                        e.ShowOnGUI(ert.rect, ert, SpriteAlignment.BottomLeft, true);
                        activeObserver = Expedition.GetObserver().gameObject;
                        if (emptyPanelText.enabled) emptyPanelText.enabled = false;
                    }
                    break;
                }
            case InfoMode.Crews:
                {
                    if (realIndex >= Crew.crewsList.Count) PrepareCrewsList();
                    else
                    {
                        var ert = observerPanel.GetComponent<RectTransform>();
                        UICrewObserver.Show(ert, ert.rect,  SpriteAlignment.BottomLeft, Crew.crewsList[realIndex], false);
                        activeObserver = UICrewObserver.GetCrewObserver().gameObject;
                        if (emptyPanelText.enabled) emptyPanelText.enabled = false;
                    }
                    break;
                }
            case InfoMode.Artifacts:
                if (realIndex >= Artifact.artifactsList.Count) PrepareArtifactsList();
                else
                {
                    var e = Artifact.artifactsList[realIndex];
                    var ert = observerPanel.GetComponent<RectTransform>();
                    var r = new Rect(ert.localPosition, ert.rect.size * ert.localScale.x);
                    e.ShowOnGUI( ert, r, SpriteAlignment.BottomLeft, false);
                    activeObserver = Artifact.observer.gameObject;
                    if (emptyPanelText.enabled) emptyPanelText.enabled = false;
                }
                break;
        }
        if (activeObserver != null) activeObserver.transform.SetAsLastSibling();
    }

    public void Show(Crew c)
    {
        if (c != null)
        {
            showingCrew = c;
            if (mode != InfoMode.Crews) ChangeMode(InfoMode.Crews);
            else StatusUpdate();        
        }
    }
    public void Show(Artifact a)
    {
        if (a != null && !a.destructed)
        {
            showingArtifact = a;
            if (mode != InfoMode.Artifacts) ChangeMode(InfoMode.Artifacts);
            else StatusUpdate();
        }
    }
    public void Show(Expedition e)
    {
        if (e != null)
        {
            showingExpedition = e;
            if (mode != InfoMode.Expeditions) ChangeMode(InfoMode.Expeditions);
            else StatusUpdate();
        }
    }

    private void PrepareCrewsList()
    {
        var crews = Crew.crewsList;
        if (crews.Count == 0)
        {
            if (listEnabled)
            {
                listHolder.SetActive(false);
                listEnabled = false;
            }
        }
        else
        {
            listIDs = new List<int>();
            int currentSelectedItem = -1;
            if (crews.Count > items.Length)
            {
                int sindex = GetListStartIndex();
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + crews[i + sindex].name + '"';
                    listIDs.Add(crews[i + sindex].ID);
                    items[i].SetActive(true);
                }


                if (showingCrew != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (listIDs[i] == showingCrew.ID)
                        {
                            currentSelectedItem = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (; i < crews.Count; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + crews[i].name + '"';
                    listIDs.Add(crews[i].ID);
                    items[i].SetActive(true);
                }
                if (i < items.Length)
                {
                    for (; i < items.Length; i++)
                    {
                        items[i].SetActive(false);
                    }
                }

                if (showingCrew != null)
                {
                    for (i = 0; i < listIDs.Count; i++)
                    {
                        if (listIDs[i] == showingCrew.ID)
                        {
                            currentSelectedItem = i;
                        }
                    }
                }
            }

            if (currentSelectedItem != selectedItem)
            {
                print(currentSelectedItem);
                if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                if (currentSelectedItem != -1)
                {
                    items[currentSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    selectedItem = currentSelectedItem;
                }
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
    private void PrepareArtifactsList()
    {
        var arts = Artifact.artifactsList;
        if (arts.Count == 0)
        {
            if (listEnabled)
            {
                listHolder.SetActive(false);
                listEnabled = false;
            }
        }
        else
        {
            listIDs = new List<int>();
            int currentSelectedItem = -1;
            if (arts.Count > items.Length)
            {
                int sindex = GetListStartIndex();
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + arts[i + sindex].name + '"';
                    listIDs.Add(arts[i + sindex].ID);
                    items[i].SetActive(true);
                }


                if (showingArtifact != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (listIDs[i] == showingArtifact.ID)
                        {
                            currentSelectedItem = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (; i < arts.Count; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + arts[i].name + '"';
                    listIDs.Add(arts[i].ID);
                    items[i].SetActive(true);
                }
                if (i < items.Length)
                {
                    for (; i < items.Length; i++)
                    {
                        items[i].SetActive(false);
                    }
                }
                if (showingArtifact != null)
                {
                    for (i = 0; i < listIDs.Count; i++)
                    {
                        if (listIDs[i] == showingArtifact.ID)
                        {
                            currentSelectedItem = i;
                        }
                    }
                }
            }

            if (currentSelectedItem != selectedItem)
            {
                if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                if (currentSelectedItem != -1)
                {
                    items[currentSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    selectedItem = currentSelectedItem;
                }
            }

            if (!listEnabled)
            {
                listHolder.SetActive(true);
                listEnabled = true;
            }
            //настройка scrollbar ?
        }
        lastDrawnActionHash = Artifact.listChangesMarkerValue;
    }
    private void PrepareExpeditionsList()
    {
        var exps = Expedition.expeditionsList;
        if (exps.Count == 0)
        {
            if (listEnabled)
            {
                listHolder.SetActive(false);
                listEnabled = false;
            }
        }
        else
        {
            listIDs = new List<int>();
            int currentSelectedItem = -1;
            if (exps.Count > items.Length)
            {
                int sindex = GetListStartIndex();
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + exps[i + sindex].crew.name + '"';
                    listIDs.Add(exps[i + sindex].ID);
                    items[i].SetActive(true);
                }


                if (showingExpedition != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (listIDs[i] == showingExpedition.ID)
                        {
                            currentSelectedItem = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (; i < exps.Count; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + exps[i].crew.name + '"';
                    listIDs.Add(exps[i].ID);
                    items[i].SetActive(true);
                }
                if (i < items.Length)
                {
                    for (; i < items.Length; i++)
                    {
                        items[i].SetActive(false);
                    }
                }
                if (showingExpedition != null)
                {
                    for (i = 0; i < listIDs.Count; i++)
                    {
                        if (listIDs[i] == showingExpedition.ID)
                        {
                            currentSelectedItem = i;
                        }
                    }
                }
            }

            if (currentSelectedItem != selectedItem)
            {
                if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                if (currentSelectedItem != -1)
                {
                    items[currentSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    selectedItem = currentSelectedItem;
                }
            }

            if (!listEnabled)
            {
                listHolder.SetActive(true);
                listEnabled = true;
            }
            //настройка scrollbar ?
        }
        lastDrawnActionHash = Expedition.listChangesMarker;
    }

    private void ChangeMode(InfoMode nmode)
    {
        if (mode != nmode)
        {
            //prev mode            
            switch (mode)
            {
                case InfoMode.Expeditions:
                    showingExpedition = null;
                    expeditionButtonImage.overrideSprite = null;
                    break;
                case InfoMode.Crews:
                    showingCrew = null;
                    crewButtonImage.overrideSprite = null;
                    break;
                case InfoMode.Artifacts:
                    showingArtifact = null;
                    artifactButtonImage.overrideSprite = null;
                    break;
            }
            mode = nmode;
            if (activeObserver != null) activeObserver.SetActive(false);
            selectedItem = -1;
            switch (mode)
            {
                case InfoMode.Crews:
                    {
                        PrepareCrewsList();
                        crewButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Crew.crewsList.Count != 0)
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
                            observerPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoCrews);
                            if (!emptyPanelText.enabled) emptyPanelText.enabled = true;
                        }
                        lastDrawnActionHash = Crew.listChangesMarkerValue;
                        break;
                    }
                case InfoMode.Artifacts:
                    {
                        PrepareArtifactsList();
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
                        PrepareExpeditionsList();
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
                if (lastDrawnActionHash != Crew.listChangesMarkerValue) PrepareCrewsList();
                break;
            case InfoMode.Artifacts:
                if (lastDrawnActionHash != Artifact.listChangesMarkerValue) PrepareArtifactsList();
                break;
            case InfoMode.Expeditions:
                if (lastDrawnActionHash != Expedition.listChangesMarker) PrepareExpeditionsList();
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

    private void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
        if (mode == InfoMode.Inactive) ChangeMode(InfoMode.Expeditions);
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
            UIController.current.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
        ChangeMode(InfoMode.Inactive);
        if (UIController.current != null) UIController.current.DropActiveWindow(ActiveWindowMode.ExpeditionPanel);
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
    }
}
