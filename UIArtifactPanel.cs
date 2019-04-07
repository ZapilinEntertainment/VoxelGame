using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactPanel : MonoBehaviour {
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject[] items;
    [SerializeField] private RawImage icon, iconBase;
    [SerializeField] private InputField nameField;
    [SerializeField] private Text status, description;
    [SerializeField] private GameObject conservateButton, passButton;
    private bool noArtifacts = false, descriptionOff = false, ignoreScrollbarEvents = false, subscribedToUpdate = false;
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

                int listStartIndex = GetListStartIndex();
                for (int i = 0; i < icount; i++)
                {
                    items[i].transform.GetChild(0).GetComponent<Text>().text = arts[listStartIndex + i].name;
                    items[i].SetActive(true);
                }
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
            if (!descriptionOff)
            {
                icon.texture = Artifact.emptyArtifactFrame_tx;
                iconBase.gameObject.SetActive(false);
                nameField.gameObject.SetActive(false);
                status.enabled = false;
                conservateButton.SetActive(false);
                passButton.SetActive(false);
                description.text = noArtifacts ? Localization.GetPhrase(LocalizedPhrase.NoArtifacts) : string.Empty;
                descriptionOff = true;
            }
        }
        else
        {            
            icon.texture = chosenArtifact.GetTexture();
            icon.color = chosenArtifact.GetColor();
            nameField.text = chosenArtifact.name;
            status.text = Localization.GetArtifactStatus(chosenArtifact.status);
            if (chosenArtifact.researched)
            {
                iconBase.enabled = true;
                iconBase.color = chosenArtifact.GetHaloColor();
                // localization - write artifact info 
            }
            else
            {
                description.text = Localization.GetPhrase(LocalizedPhrase.NotResearched);
                iconBase.enabled = false;
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

            if (descriptionOff)
            {
                iconBase.gameObject.SetActive(true);
                nameField.gameObject.SetActive(true);
                status.enabled = true;
                descriptionOff = false;
            }
        }
    }

    private void Update()
    {
        if (chosenArtifact != null)
        {
            if (chosenArtifact.researched & chosenArtifact.activated)
            {
                iconBase.transform.Rotate(Vector3.forward, chosenArtifact.frequency * 10f * Time.deltaTime);
            }
        }
    }

    public void ShowArtifact(Artifact a)
    {
        var arts = Artifact.playersArtifactsList;
        int count = arts.Count;
        if (count == 0) return;
        else
        {
            int realIndex = -1;
            for (int i = 0; i < count; i++)
            {
                if (arts[i] == a)
                {
                    realIndex = i;
                    break;
                }
            }
            if (realIndex == -1) return;
            else
            {
                chosenArtifact = a;
                float x = realIndex;
                x /= (float)count;
                scrollbar.value = x;
            }
        }
    }

    // buttons
    public void NameChanged()
    {
        if (chosenArtifact != null)
        {
            chosenArtifact.ChangeName(nameField.text);
        }
        else RedrawDescriptionWindow();
    }
    public void ConservateButton()
    {
        if (chosenArtifact != null)
        {
            if (chosenArtifact.owner != null)
            {
                chosenArtifact.owner.DropArtifact();
                // добавить в хранилище
                return;
            }
        }
        RedrawDescriptionWindow();
    }
    public void PassButton()
    {
        if (chosenArtifact != null)
        {
            switch (chosenArtifact.status)
            {
                case Artifact.ArtifactStatus.Exists: RedrawDescriptionWindow();break;
                case Artifact.ArtifactStatus.Researching:
                    //goto research center
                    break;
                case Artifact.ArtifactStatus.UsingByCrew:
                    if (chosenArtifact.owner != null)
                    {
                        chosenArtifact.owner.ShowOnGUI();
                        gameObject.SetActive(false);
                    }
                    else RedrawDescriptionWindow();
                    break;
                case Artifact.ArtifactStatus.UsingInMonument:
                    // goto monument
                    break;
            }
        }
        else RedrawDescriptionWindow();
    }
    public void ScrollbarValueChanged()
    {
        if (ignoreScrollbarEvents) return;
        else RedrawList();
    }
    public void SelectArtifact(int i)
    {
        int listStartIndex = GetListStartIndex(), realIndex = i + listStartIndex;
        if (selectedItem != -1 & selectedItem != i)
        {
            items[selectedItem].GetComponent<Image>().overrideSprite = null;
        }
        chosenArtifact = Artifact.playersArtifactsList[realIndex];
        selectedItem = i;
        items[selectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
        RedrawWindow();
    }
    //

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
            UIController.current.statusUpdateEvent += RedrawDescriptionWindow;
            subscribedToUpdate = true;
        }
    }
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent -= RedrawDescriptionWindow;
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
                if (uc != null) uc.statusUpdateEvent -= RedrawDescriptionWindow;
                subscribedToUpdate = false;
            }
        }
    }
}
