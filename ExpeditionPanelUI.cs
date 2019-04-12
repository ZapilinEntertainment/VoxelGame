using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExpeditionPanelUI : MonoBehaviour
{
    private enum InfoMode { Expeditions, Crews, Shuttles, Artifacts};
#pragma warning disable 0649
    [SerializeField] private GameObject emptyPanel, listHolder;
    [SerializeField] private GameObject[] items;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Image expeditionButtonImage, crewButtonImage, shuttleButtonImage, artifactButtonImage;
#pragma warning restore 0649

    public static ExpeditionPanelUI current { get; private set; }

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
        if (current == null) current = Instantiate(Resources.Load<GameObject>("UIPrefs/expeditionPanel"), UIController.current.mainCanvas).GetComponent<ExpeditionPanelUI>();
        current.gameObject.SetActive(true);
    }

    public void Show(Crew c)
    {
        if (c != null)
        {
            showingCrew = c;
            if (mode != InfoMode.Crews) ChangeMode(InfoMode.Crews);
            else {
                PrepareCrewsList();
                Crew.crewObserver.ShowCrew(showingCrew);
            }           
        }
    }
    public void Show(Artifact a)
    {
        if (a != null && !a.destructed)
        {
            if (mode != InfoMode.Artifacts) ChangeMode(InfoMode.Artifacts);
            else
            {

            }
        }
    }

    private void ChangeMode(InfoMode nmode)
    {
        if (mode != nmode)
        {
            mode = nmode;
            switch (mode)
            {
                case InfoMode.Crews:
                    {
                        showingArtifact = null;
                        showingExpedition = null;
                        showingShuttle = null;
                        PrepareCrewsList();

                        if (Crew.crewsList.Count != 0)
                        {
                            Crew.crewsList[0].ShowOnGUI(emptyPanel.transform.position, SpriteAlignment.TopLeft);
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

                        if (Artifact.playersArtifactsList.Count != 0)
                        {
                            Artifact.playersArtifactsList.ShowOnGUI(emptyPanel.transform.position, SpriteAlignment.TopLeft);
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
                            emptyPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoArtifacts);
                            emptyPanel.SetActive(true);
                        }
                        lastDrawnActionHash = Artifact.actionsHash;
                        break;
                    }
            }
        }
    }

    public void StatusUpdate()
    {
        switch (mode)
        {
            case InfoMode.Crews:
                if (lastDrawnActionHash != Crew.actionsHash) PrepareCrewsList();
                break;
            case InfoMode.Artifacts:
                if (lastDrawnActionHash != Artifact.actionsHash) PrepareArtifactsList();
                break;
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
                        if (listIDs[i] == showingCrew.ID) {
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
                    for (; i< items.Length; i++)
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


                if (showingArtifact!= null)
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
