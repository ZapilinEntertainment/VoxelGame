using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactPanel : MonoBehaviour {
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject[] items;
    [SerializeField] private RawImage icon;
    [SerializeField] private InputField nameField;
    [SerializeField] private Text status, description;
    [SerializeField] private GameObject conservateButton, passButton;
    private bool noArtifacts = true, ignoreScrollbarEvents = false, subscribedToUpdate = false;
    private int lastDrawnActionHash = 0, selectedItem = -1, itemListViewDelta = 0;
    private Artifact chosenArtifact;

    private void RedrawWindow()
    {
        // 1) list preparing
        RedrawList();
        RedrawDescriptionWindow();
        lastDrawnActionHash = Artifact.actionsHash;
    }
    private void RedrawList()
    {
        int count = Artifact.playersArtifactsList.Count;
        if (count == 0)
        {
            if (!noArtifacts)
            {
                foreach (var g in items) g.SetActive(false);
                noArtifacts = true;
            }
            chosenArtifact = null;
            selectedItem = -1;
        }
        else
        {
            if (noArtifacts)
            {
                scrollbar.transform.parent.gameObject.SetActive(true);
                noArtifacts = false;
            }

            int icount = items.Length;
            var arts = Artifact.playersArtifactsList;
            if (count < icount)
            {
                scrollbar.gameObject.SetActive(false);
                scrollbar.value = 0;
                int i = 0;
                for (; i < count; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = arts[i].name;
                    items[i].SetActive(true);
                }
                if (i < icount - 1)
                {
                    for (; i < icount; i++)
                    {
                        items[i].SetActive(false);
                    }
                }
            }
            else  /// artifacts more than list length
            {
                float s = icount;
                s /= (float)count;
                if (s != scrollbar.size)
                {
                    float savedValue = scrollbar.value;
                    ignoreScrollbarEvents = true;
                    scrollbar.size = s;
                    scrollbar.value = savedValue;
                    ignoreScrollbarEvents = false;
                }
                if (!scrollbar.gameObject.activeSelf) scrollbar.gameObject.SetActive(true);
                foreach (var g in items) g.SetActive(true);
            }

            ChosenArtifactCheck();
            if (chosenArtifact == null) SelectArtifact(0);
            else
            {
                if (selectedItem != -1) items[selectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
            }
        }
    }
    private void RedrawDescriptionWindow()
    {
        if (chosenArtifact == null)
        {
            icon.texture = Artifact.emptyArtifactFrame_tx;
            nameField.gameObject.SetActive(false);
            status.enabled = false;
            conservateButton.SetActive(false);
            passButton.SetActive(false);
            description.text = noArtifacts ? Localization.GetPhrase(LocalizedPhrase.NoArtifacts) : string.Empty;
        }
        else
        {
            icon.texture = chosenArtifact.GetTexture();
            nameField.text = chosenArtifact.name;
            status.text = Localization.GetArtifactStatus(chosenArtifact.status);
            if (chosenArtifact.researched)
            {
                // localization - write artifact info 
            }
            else
            {
                description.text = Localization.GetPhrase(LocalizedPhrase.NotResearched);
            }
            conservateButton.SetActive(chosenArtifact.status == Artifact.ArtifactStatus.UsingByCrew);
            if (chosenArtifact.status != Artifact.ArtifactStatus.Exists)
            {
                switch (chosenArtifact.status)
                {
                    case Artifact.ArtifactStatus.Researching:
                        // установка иконки иссл лаб
                        break;
                    case Artifact.ArtifactStatus.UsingByCrew:
                        var ri = passButton.transform.GetChild(0).GetComponent<RawImage>();
                        ri.texture = UIController.current.iconsTexture;
                        ri.uvRect = UIController.GetTextureUV(Icons.CrewGoodIcon);
                        break;
                    case Artifact.ArtifactStatus.UsingInMonument:
                        //иконка монумента
                        break;
                }
                passButton.SetActive(true);
            }
            else passButton.SetActive(false);
        }
    }

    public void StatusUpdate()
    {

    }

    public void ScrollbarValueChanged()
    {
        if (ignoreScrollbarEvents) return;
        else
        {

        }
    }
    
    public void SelectArtifact(int i)
    {
        int listStartIndex = GetListStartIndex();
        if (i < listStartIndex)
        {
            
        }
    }
    private void ChosenArtifactCheck()
    {
        if (chosenArtifact == null)
        {
            selectedItem = -1;
        }
        else
        {
            if (chosenArtifact.destructed)
            {
                chosenArtifact = null;
                selectedItem = -1;
            }
            else
            {
                var arts = Artifact.playersArtifactsList;
                if (arts.Count == 0)
                {
                    selectedItem = -1;
                    chosenArtifact = null;
                }
                else
                {
                    int arrayIndex = -1;
                    for (int i = 0; i < arts.Count; i++)
                    {
                        if (arts[i] == chosenArtifact)
                        {
                            arrayIndex = i; break;
                        }
                    }
                    if (arrayIndex == -1)
                    {
                        chosenArtifact = null;
                        selectedItem = -1;
                    }
                    else
                    {
                        int listStartIndex = GetListStartIndex();
                        if (arrayIndex < listStartIndex | arrayIndex >= listStartIndex + items.Length)
                        {
                            selectedItem = -1;
                        }
                        else selectedItem = arrayIndex - listStartIndex;
                    }
                }
            }
        }
    }

    private int GetListStartIndex() // 0-item real index
    {
        int listStartIndex = 0, icount = Artifact.playersArtifactsList.Count, count = items.Length;
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
            if (subscribedToUpdate)
            {
                UIController uc = UIController.current;
                if (uc != null) uc.statusUpdateEvent -= StatusUpdate;
                subscribedToUpdate = false;
            }
        }
    }
}
