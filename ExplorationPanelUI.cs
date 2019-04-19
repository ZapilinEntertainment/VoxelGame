using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExplorationPanelUI : MonoBehaviour
{
    private enum InfoMode { Inactive,Expeditions, Crews, Shuttles, Artifacts};
#pragma warning disable 0649
    [SerializeField] private GameObject emptyPanel, listHolder;
    [SerializeField] private GameObject[] items;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Image expeditionButtonImage, crewButtonImage, shuttleButtonImage, artifactButtonImage;
#pragma warning restore 0649

    public static ExplorationPanelUI current { get; private set; }

    private bool listEnabled = true, subscribedToUpdate = false;
    private int selectedItem = -1, lastDrawnActionHash = 0;
    private Artifact showingArtifact;
    private Crew showingCrew;
    private GameObject activeObserver;
    private Expedition showingExpedition;
    private Shuttle showingShuttle;
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

    public void SelectItem(int i)
    {
        var realIndex = i + GetListStartIndex();
        switch (mode)
        {
            case InfoMode.Expeditions:
                {
                    if (realIndex >= Expedition.expeditionsList.Count) PrepareExpeditionsList();
                    else
                    {
                        if (selectedItem != i)
                        {
                            if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                            selectedItem = i;
                            items[selectedItem].GetComponent<Image>().overrideSprite = null;
                        }
                        var e = Expedition.expeditionsList[realIndex];
                        e.ShowOnGUI(emptyPanel.transform.position, SpriteAlignment.TopLeft);
                        activeObserver = Expedition.observer.gameObject;
                        if (emptyPanel.activeSelf) emptyPanel.SetActive(false);
                    }
                    break;
                }
            case InfoMode.Crews:
                {
                    if (realIndex >= Crew.crewsList.Count) PrepareCrewsList();
                    else
                    {
                        if (selectedItem != i)
                        {
                            if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                            selectedItem = i;
                            items[selectedItem].GetComponent<Image>().overrideSprite = null;
                        }
                        var e = Crew.crewsList[realIndex];
                        var ert = emptyPanel.GetComponent<RectTransform>();
                        var r = new Rect(ert.position, ert.rect.size);
                        e.ShowOnGUI(r, SpriteAlignment.TopLeft, false);
                        activeObserver = Crew.crewObserver.gameObject;
                        if (emptyPanel.activeSelf) emptyPanel.SetActive(false);
                    }
                    break;
                }
            case InfoMode.Shuttles:
                if (realIndex >= Shuttle.shuttlesList.Count) PrepareShuttlesList();
                else
                {
                    if (selectedItem != i)
                    {
                        if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                        selectedItem = i;
                        items[selectedItem].GetComponent<Image>().overrideSprite = null;
                    }
                    var e = Shuttle.shuttlesList[realIndex];
                    var ert = emptyPanel.GetComponent<RectTransform>();
                    var r = new Rect(ert.position, ert.rect.size);
                    e.ShowOnGUI(true,r, SpriteAlignment.TopLeft, false);
                    activeObserver = Shuttle.observer.gameObject;
                    if (emptyPanel.activeSelf) emptyPanel.SetActive(false);
                }
                break;
            case InfoMode.Artifacts:
                if (realIndex >= Artifact.playersArtifactsList.Count) PrepareArtifactsList();
                else
                {
                    if (selectedItem != i)
                    {
                        if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = null;
                        selectedItem = i;
                        items[selectedItem].GetComponent<Image>().overrideSprite = null;
                    }
                    var e = Artifact.playersArtifactsList[realIndex];
                    var ert = emptyPanel.GetComponent<RectTransform>();
                    var r = new Rect(ert.position, ert.rect.size);
                    e.ShowOnGUI(r, SpriteAlignment.TopLeft, false);
                    activeObserver = Artifact.observer.gameObject;
                    if (emptyPanel.activeSelf) emptyPanel.SetActive(false);
                }
                break;
        }
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
    public void Show(Shuttle s)
    {
        if (s != null )
        {
            showingShuttle = s;
            if (mode != InfoMode.Artifacts) ChangeMode(InfoMode.Shuttles);
            else StatusUpdate();
        }
    }
    public void Show(Expedition e)
    {
        if (e != null && e.stage != Expedition.ExpeditionStage.Dismissed)
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
    }
    private void PrepareArtifactsList()
    {
        var arts = Artifact.playersArtifactsList;
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
    }
    private void PrepareShuttlesList()
    {
        var shuttles = Shuttle.shuttlesList;
        if (shuttles.Count == 0)
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
            if (shuttles.Count > items.Length)
            {
                int sindex = GetListStartIndex();
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + shuttles[i + sindex].name + '"';
                    listIDs.Add(shuttles[i + sindex].ID);
                    items[i].SetActive(true);
                }


                if (showingShuttle != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (listIDs[i] == showingShuttle.ID)
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
                for (; i < shuttles.Count; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + shuttles[i].name + '"';
                    listIDs.Add(shuttles[i].ID);
                    items[i].SetActive(true);
                }
                if (i < items.Length)
                {
                    for (; i < items.Length; i++)
                    {
                        items[i].SetActive(false);
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
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + exps[i + sindex].name + '"';
                    listIDs.Add(exps[i + sindex].ID);
                    items[i].SetActive(true);
                }


                if (showingExpedition != null)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (listIDs[i] == showingShuttle.ID)
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
                    items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + exps[i].name + '"';
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
    }

    private void ChangeMode(InfoMode nmode)
    {
        if (mode != nmode)
        {
            switch (mode)
            {
                case InfoMode.Expeditions: expeditionButtonImage.overrideSprite = null; break;
                case InfoMode.Crews: crewButtonImage.overrideSprite = null;break;
                case InfoMode.Shuttles: shuttleButtonImage.overrideSprite = null;break;
                case InfoMode.Artifacts: artifactButtonImage.overrideSprite = null;break;
            }
            mode = nmode;
            if (activeObserver != null) activeObserver.SetActive(false);
            switch (mode)
            {
                case InfoMode.Crews:
                    {
                        showingArtifact = null;
                        showingExpedition = null;
                        showingShuttle = null;
                        PrepareCrewsList();
                        crewButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Crew.crewsList.Count != 0)
                        {
                            var ert = emptyPanel.GetComponent<RectTransform>();
                            var r = new Rect(ert.position, ert.rect.size);
                            Crew.crewsList[0].ShowOnGUI(r, SpriteAlignment.TopLeft, false);
                            activeObserver = Crew.crewObserver.gameObject;
                            emptyPanel.SetActive(false);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            emptyPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoCrews);
                            emptyPanel.SetActive(true);
                        }
                        lastDrawnActionHash = Crew.actionsHash;
                        break;
                    }
                case InfoMode.Artifacts:
                    {
                        showingCrew = null;
                        showingExpedition = null;
                        showingShuttle = null;
                        PrepareArtifactsList();
                        artifactButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Artifact.playersArtifactsList.Count != 0)
                        {
                            var ert = emptyPanel.GetComponent<RectTransform>();
                            var r = new Rect(ert.position, ert.rect.size);
                            Artifact.playersArtifactsList[0].ShowOnGUI(r, SpriteAlignment.TopLeft, false);
                            activeObserver = Artifact.observer.gameObject;
                            emptyPanel.SetActive(false);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            emptyPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoArtifacts);
                            emptyPanel.SetActive(true);
                        }
                        lastDrawnActionHash = Artifact.actionsHash;
                        break;
                    }
                case InfoMode.Shuttles:
                    {
                        showingCrew = null;
                        showingExpedition = null;
                        showingArtifact = null;
                        PrepareShuttlesList();
                        shuttleButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Shuttle.shuttlesList.Count != 0)
                        {
                            var ert = emptyPanel.GetComponent<RectTransform>();
                            var r = new Rect(ert.position, ert.rect.size);
                            Shuttle.shuttlesList[0].ShowOnGUI(true,r, SpriteAlignment.TopLeft, false);
                            activeObserver = Shuttle.observer.gameObject;
                            emptyPanel.SetActive(false);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            emptyPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoShuttles);
                            emptyPanel.SetActive(true);
                        }
                        lastDrawnActionHash = Shuttle.actionsHash;
                        break;
                    }
                case InfoMode.Expeditions:
                    {
                        showingCrew = null;
                        showingShuttle = null;
                        showingArtifact = null;
                        PrepareExpeditionsList();
                        expeditionButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;

                        if (Expedition.expeditionsList.Count != 0)
                        {
                            Expedition.expeditionsList[0].ShowOnGUI(emptyPanel.transform.position, SpriteAlignment.TopLeft);
                            activeObserver = Expedition.observer.gameObject;
                            emptyPanel.SetActive(false);
                        }
                        else
                        {
                            if (activeObserver != null)
                            {
                                activeObserver.SetActive(false);
                                activeObserver = null;
                            }
                            emptyPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoExpeditions);
                            emptyPanel.SetActive(true);
                        }
                        lastDrawnActionHash = Expedition.actionsHash;
                        break;
                    }
            }
        }
    }

    public void ExpeditionsButton() { ChangeMode(InfoMode.Expeditions); }
    public void CrewsButton() { ChangeMode(InfoMode.Crews); }
    public void ShuttlesButton() { ChangeMode(InfoMode.Shuttles); }
    public void ArtifactsButton() { ChangeMode(InfoMode.Artifacts); }

    public void StatusUpdate()
    {
        // сами окна информации обновлять не надо - они сами подписаны
        switch (mode)
        {
            case InfoMode.Crews:
                if (lastDrawnActionHash != Crew.actionsHash) PrepareCrewsList();
                break;
            case InfoMode.Artifacts:
                if (lastDrawnActionHash != Artifact.actionsHash) PrepareArtifactsList();
                break;
            case InfoMode.Shuttles:
                if (lastDrawnActionHash != Shuttle.actionsHash) PrepareShuttlesList();
                break;
            case InfoMode.Expeditions:
                if (lastDrawnActionHash != Expedition.actionsHash) PrepareExpeditionsList();
                break;
        }
    }    

    private int GetListStartIndex() // 0-item real index
    {
        int listStartIndex = 0, icount = 0, count = items.Length;
        switch (mode)
        {
            case InfoMode.Crews: icount = Crew.crewsList.Count; break;
            case InfoMode.Shuttles: icount = Shuttle.shuttlesList.Count;break;
            case InfoMode.Expeditions: icount = Expedition.expeditionsList.Count; break;
            case InfoMode.Artifacts: icount = Artifact.playersArtifactsList.Count; break;
        }
        float sval = scrollbar.value;
        if (sval != 0)
        {
            if (sval != 1)
            {
                listStartIndex = (int)((sval - (1f / icount) * 0.5f) * count);
                if (listStartIndex < 0) listStartIndex = 0;
                else
                {
                    if (listStartIndex > count - icount) listStartIndex = count - icount;
                }
            }
            else listStartIndex = count - icount;
        }
        else listStartIndex = 0;
        return listStartIndex;
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
