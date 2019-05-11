using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIMonumentObserver : UIObserver
{
#pragma warning disable 0649
    [SerializeField] private RawImage affectionIcon;
    [SerializeField] private Text affectionText;
    [SerializeField] private Transform[] slots;
    [SerializeField] private GameObject[] items;
    [SerializeField] private GameObject listHolder;
    [SerializeField] private Scrollbar scrollbar;
#pragma warning restore 0649
    private int listSelectedItem = -1, selectedSlotIndex = -1;
    private Monument observingMonument;
    private List<int> ids;
    private const int IMAGE_CHILD_INDEX = 0, TEXT_CHILD_INDEX = 1, CLOSEBUTTON_CHILD_INDEX = 2;

    public static UIMonumentObserver InitializeMonumentObserverScript()
    {
        UIMonumentObserver ub = Instantiate(Resources.Load<GameObject>("UIPrefs/monumentObserver"), UIController.current.rightPanel.transform).GetComponent<UIMonumentObserver>();
        Monument.SetObserver(ub);
        ub.LocalizeTitles();
        return ub;
    }

    //отключать слоты артефактов когда нет питания ?

    public void SetObservingMonument(Monument m)
    {
        if (m == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (m != observingMonument)
            {
                observingMonument = m;
                isObserving = true;
                if (listHolder.activeSelf) listHolder.SetActive(false);
            }

            var bo = Building.buildingObserver;
            if (bo == null) bo = UIBuildingObserver.InitializeBuildingObserverScript();
            bo.SetObservingBuilding(m);

            var at = observingMonument.affectionType;
            affectionIcon.uvRect = Artifact.GetAffectionIconRect(at);
            affectionText.text = Localization.GetAffectionTitle(at);
            if (observingMonument.artifacts != null)
            {
                //0
                Transform t = slots[0];
                Artifact a = observingMonument.artifacts[0];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
                // 1
                t = slots[1]; a = observingMonument.artifacts[1];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
                // 2
                t = slots[2]; a = observingMonument.artifacts[2];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
                // 3
                t = slots[3]; a = observingMonument.artifacts[3];
                if (a == null)
                {
                    t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetComponent<Button>().interactable = true;
                }
                else
                {
                    var t2 = t.GetChild(IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                    t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(true);
                    t.GetComponent<Button>().interactable = false;
                }
            }
            else
            {
                //0
                Transform t = slots[0];
                Artifact a = observingMonument.artifacts[0];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
                //1
                t = slots[1];
                a = observingMonument.artifacts[1];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
                //2
                t = slots[2];
                a = observingMonument.artifacts[2];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
                //3
                t = slots[3];
                a = observingMonument.artifacts[3];
                t.GetChild(IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(TEXT_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(CLOSEBUTTON_CHILD_INDEX).gameObject.SetActive(false);
                t.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void SelectSlot(int i)
    {        
        if (selectedSlotIndex != i)
        {
            selectedSlotIndex = i;
            PrepareList(selectedSlotIndex);
        }
        else
        {
            if (listHolder.activeSelf) listHolder.SetActive(false);
        }
    }
    public void ClearSlot(int x)
    {
        if (observingMonument == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            observingMonument.RemoveArtifact(x);
            if (listHolder.activeSelf) listHolder.SetActive(false);
            SetObservingMonument(observingMonument);
        }
    }

    public void PrepareList(int slotIndex)
    {
        if (observingMonument == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (Artifact.playersArtifactsList.Count == 0)
            {
                GameLogUI.MakeImportantAnnounce(Localization.GetPhrase(LocalizedPhrase.NoArtifacts));
                return;
            }
            var arts = new List<Artifact>();
            foreach (var a in Artifact.playersArtifactsList)
            {
                if (a.status == Artifact.ArtifactStatus.OnConservation & a.affectionType != Artifact.AffectionType.NoAffection & a.researched) arts.Add(a);
            }
            int artsCount = arts.Count;
            if (artsCount > 0)
            {
                // подготовка списка
                if (ids == null) ids = new List<int>() { -1};
                items[0].transform.GetChild(0).GetComponent<Text>().text = '<' + Localization.GetPhrase(LocalizedPhrase.NoArtifact) + '>';
                int currentSelectedItem = -1;
                if (artsCount + 1 > items.Length)
                {
                    int sindex = GetListStartIndex(artsCount);
                    for (int i = 1; i < items.Length; i++)
                    {
                        items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + arts[i + sindex - 1].name + '"';
                        ids.Add(arts[i + sindex - 1].ID);
                        items[i].SetActive(true);
                    }

                    var selectedArtifact = observingMonument.artifacts[slotIndex];
                    if (selectedArtifact != null)
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (ids[i] == selectedArtifact.ID)
                            {
                                currentSelectedItem = i;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    print(arts.Count);
                    int i = 1;
                    for (; i < arts.Count; i++)
                    {
                        items[i ].transform.GetChild(0).GetComponent<Text>().text = '"' + arts[i - 1].name + '"';
                        ids.Add(arts[i].ID);
                        items[i ].SetActive(true);
                    }
                    if (i < items.Length)
                    {
                        for (; i < items.Length; i++)
                        {
                            items[i].SetActive(false);
                        }
                    }
                    var selectedArtifact = observingMonument.artifacts[slotIndex];
                    if (selectedArtifact != null)
                    {
                        for (i = 0; i < ids.Count; i++)
                        {
                            if (ids[i] == selectedArtifact.ID)
                            {
                                currentSelectedItem = i;
                            }
                        }
                    }
                }

                if (currentSelectedItem != listSelectedItem)
                {
                    if (listSelectedItem != -1) items[listSelectedItem].GetComponent<Image>().overrideSprite = null;
                    if (currentSelectedItem != -1)
                    {
                        items[currentSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                        listSelectedItem = currentSelectedItem;
                    }
                }
                if (!listHolder.activeSelf) listHolder.SetActive(true);
            }
            else
            {
                if (listHolder.activeSelf)
                {
                    if (ids != null) ids.Clear();
                    listHolder.SetActive(false);
                }
                GameLogUI.MakeImportantAnnounce(Localization.GetPhrase(LocalizedPhrase.NoSuitableArtifacts));
                return;
            }
        }
    }

    public void SelectItem(int i)
    {
        if (observingMonument == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (ids.Count > i)
            {
                listSelectedItem = i;
                if (ids[i] == -1)
                {
                    ClearSlot(selectedSlotIndex);
                    return;
                }
                else
                {
                    var a = Artifact.GetArtifactByID(ids[i]);
                    if (a != null && (!a.destructed & a.affectionType != Artifact.AffectionType.NoAffection))
                    {
                        observingMonument.AddArtifact(a, selectedSlotIndex);
                    }
                }
            }
            PrepareList(selectedSlotIndex);
        }
    }

    private int GetListStartIndex(int suitableArtifactsLength) // 0-item real index
    {        
        // no checks needed
        int listStartIndex = 0, count = items.Length;
        if (suitableArtifactsLength < count) return 0;
        else
        {
            float sval = scrollbar.value;
            if (sval != 0)
            {
                if (sval != 1)
                {
                    listStartIndex = (int)((sval - (1f / suitableArtifactsLength) * 0.5f) * count);
                    if (listStartIndex < 0) listStartIndex = 0;
                    else
                    {
                        if (listStartIndex > count - suitableArtifactsLength) listStartIndex = count - suitableArtifactsLength;
                    }
                }
                else listStartIndex = count - suitableArtifactsLength;
            }
            else listStartIndex = 0;
            return listStartIndex;
        }
    }

    override public void SelfShutOff()
    {
        isObserving = false;
        selectedSlotIndex = -1;
        observingMonument = null;
        if (listHolder.activeSelf)
        {
            if (listSelectedItem != -1)
            {
                items[listSelectedItem].GetComponent<Image>().overrideSprite = null;
                listSelectedItem = -1;
            }
            listHolder.SetActive(false);
        }
        if (ids != null) ids.Clear();
        gameObject.SetActive(false);
    }
}
