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
    private int listSelectedItem = -1, selectedSlotIndex = -1, lastDrawnArtifactsHash = -1;
    private Monument observingMonument;
    private List<int> ids;
    private const int SLOT_IMAGE_CHILD_INDEX = 0, SLOT_TEXT_CHILD_INDEX = 1;

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
            else
            {
                if (listHolder.activeSelf) PrepareList();
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
                    t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                }
                else
                {
                    var t2 = t.GetChild(SLOT_IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(SLOT_TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                }
                // 1
                t = slots[1]; a = observingMonument.artifacts[1];
                if (a == null)
                {
                    t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                }
                else
                {
                    var t2 = t.GetChild(SLOT_IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(SLOT_TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                }
                // 2
                t = slots[2]; a = observingMonument.artifacts[2];
                if (a == null)
                {
                    t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                }
                else
                {
                    var t2 = t.GetChild(SLOT_IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(SLOT_TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                }
                // 3
                t = slots[3]; a = observingMonument.artifacts[3];
                if (a == null)
                {
                    t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                    t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                }
                else
                {
                    var t2 = t.GetChild(SLOT_IMAGE_CHILD_INDEX);
                    t2.GetComponent<RawImage>().texture = a.GetTexture();
                    t2.gameObject.SetActive(true);
                    t2 = t.GetChild(SLOT_TEXT_CHILD_INDEX);
                    t2.GetComponent<Text>().text = '"' + a.name + '"';
                    t2.gameObject.SetActive(true);
                }
            }
            else
            {
                //0
                Transform t = slots[0];
                Artifact a = observingMonument.artifacts[0];
                t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                //1
                t = slots[1];
                a = observingMonument.artifacts[1];
                t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                //2
                t = slots[2];
                a = observingMonument.artifacts[2];
                t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
                //3
                t = slots[3];
                a = observingMonument.artifacts[3];
                t.GetChild(SLOT_IMAGE_CHILD_INDEX).gameObject.SetActive(false);
                t.GetChild(SLOT_TEXT_CHILD_INDEX).gameObject.SetActive(false);
            }
            lastDrawnArtifactsHash = Artifact.actionsHash;
        }
    }

    public void SelectSlot(int i)
    {        
        // нужна подсветка выбранного слота
        if (selectedSlotIndex != i)
        {
            if (selectedSlotIndex != -1) slots[selectedSlotIndex].GetComponent<Image>().overrideSprite = null;
            selectedSlotIndex = i;
            slots[selectedSlotIndex].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
            PrepareList();
        }
        else
        {
            if (listHolder.activeSelf)
            {
                listHolder.SetActive(false);
                if (selectedSlotIndex != -1) {
                    slots[selectedSlotIndex].GetComponent<Image>().overrideSprite = null;
                    selectedSlotIndex = -1;
                }
            }
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
            SetObservingMonument(observingMonument);
        }
    }

    override public void StatusUpdate()
    {
        if (lastDrawnArtifactsHash != Artifact.actionsHash)
        {

        }
    }

    public void PrepareList()
    {
        if (observingMonument == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (selectedSlotIndex == -1)
            {
                listHolder.SetActive(false);
                return;
            }
            if (Artifact.artifactsList.Count == 0)
            {
                GameLogUI.MakeImportantAnnounce(Localization.GetPhrase(LocalizedPhrase.NoArtifacts));
                return;
            }
            var arts = new List<Artifact>();
            ids = new List<int>() { -1 };
            var selectedArtifact = observingMonument.artifacts[selectedSlotIndex];
            bool slotWithArtifact = selectedArtifact != null;
            foreach (var a in Artifact.artifactsList)
            {
                if (a.affectionType != Artifact.AffectionType.NoAffection & a.researched)
                {
                    if (a.status == Artifact.ArtifactStatus.OnConservation | (slotWithArtifact && selectedArtifact.ID == a.ID))
                    {
                        arts.Add(a);
                        ids.Add(a.ID);
                    }
                }
            }
            int artsCount = arts.Count;
            if (artsCount > 0)
            {
                // подготовка списка
                
                items[0].transform.GetChild(0).GetComponent<Text>().text = slotWithArtifact ? Localization.GetPhrase(LocalizedPhrase.ClearSlot) : '<' + Localization.GetPhrase(LocalizedPhrase.NoArtifact) + '>' ;
                int newSelectedItem = -1;

                if (artsCount + 1 > items.Length)
                {
                    // артефакты не помещаются в одну страницу
                    if (!scrollbar.gameObject.activeSelf)
                    {
                        scrollbar.value = 0;
                        scrollbar.gameObject.SetActive(true);
                    }
                    float rsize = items.Length;
                    rsize /= artsCount;
                    if (scrollbar.size != rsize) scrollbar.size = rsize;

                    //#preparePositionedList
                    int sindex = GetListStartIndex();
                    for (int i = 1; i < items.Length; i++)
                    {
                        items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + arts[i + sindex - 1].name + '"';
                        items[i].SetActive(true);
                    }

                    
                    if (selectedArtifact != null)
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (ids[i] == selectedArtifact.ID)
                            {
                                newSelectedItem = i;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (sindex != 0) newSelectedItem = -1;
                        else newSelectedItem = 0;
                    }
                    //
                }
                else
                {
                    // артефактов меньше, чем позиций списка
                    if (scrollbar.gameObject.activeSelf) scrollbar.gameObject.SetActive(false);
                    int i = 0;
                    for (; i < arts.Count; i++)
                    {
                        items[i + 1].transform.GetChild(0).GetComponent<Text>().text = '"' + arts[i].name + '"';
                        items[i + 1].SetActive(true);
                    }
                    i++;
                    if (i < items.Length)
                    {
                        for (; i < items.Length; i++)
                        {
                            items[i].SetActive(false);
                        }
                    }

                    if (selectedArtifact != null)
                    {
                        newSelectedItem = ids.Count - 1;
                    }
                    else newSelectedItem = 0;
                }

                if (newSelectedItem != listSelectedItem)
                {
                    if (listSelectedItem != -1) items[listSelectedItem].GetComponent<Image>().overrideSprite = null;
                    if (newSelectedItem != -1)  items[newSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    listSelectedItem = newSelectedItem;
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

    public void ScrollbarChanged()
    {
        if (observingMonument == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (ids.Count < items.Length)
            {
                PrepareList();
                return;
            }
            else
            {
                int count = ids.Count;
                int sindex = GetListStartIndex();
                int realIndex;
                var selectedArtifact = observingMonument.artifacts[selectedSlotIndex];
                bool slotWithArtifact = selectedArtifact != null;
                for (int i = 0; i < items.Length; i++)
                {
                    realIndex = i + sindex;
                    if (ids[realIndex] != -1)
                    {
                        var a = Artifact.GetArtifactByID(ids[realIndex]);
                        items[i].transform.GetChild(0).GetComponent<Text>().text = '"' + a.name + '"';                        
                    }
                    else
                    {
                        items[i].transform.GetChild(0).GetComponent<Text>().text = slotWithArtifact ? Localization.GetPhrase(LocalizedPhrase.ClearSlot) : '<' + Localization.GetPhrase(LocalizedPhrase.NoArtifact) + '>'; 
                    }
                    items[i].SetActive(true);
                }

                int newSelectedItem = -1;
                
                if ( slotWithArtifact)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (ids[i + sindex] == selectedArtifact.ID)
                        {
                            newSelectedItem = i;
                            break;
                        }
                    }
                }
                else
                {
                    if (sindex != 0) newSelectedItem = -1;
                    else newSelectedItem = 0;
                }
                if (newSelectedItem != listSelectedItem)
                {
                    if (listSelectedItem != -1) items[listSelectedItem].GetComponent<Image>().overrideSprite = null;
                    if (newSelectedItem != -1) items[newSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    listSelectedItem = newSelectedItem;
                }
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
                if (listSelectedItem != -1) items[listSelectedItem].GetComponent<Image>().overrideSprite = null;
                listSelectedItem = i;
                items[listSelectedItem].GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                if (ids[i] == -1)
                {
                    ClearSlot(selectedSlotIndex);
                }
                else
                {
                    var a = Artifact.GetArtifactByID(ids[i]);
                    if (a != null && (!a.destructed & a.affectionType != Artifact.AffectionType.NoAffection))
                    {
                        observingMonument.AddArtifact(a, selectedSlotIndex);
                        SetObservingMonument(observingMonument);
                    }
                }
                return;
            }
            else PrepareList();
        }
    }

    private int GetListStartIndex() // 0-item real index
    {
        // no checks needed
        int visibleListCount = items.Length, totalListCount = ids.Count; ;
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

    override public void SelfShutOff()
    {
        isObserving = false;
        if (selectedSlotIndex != -1)
        {
            slots[selectedSlotIndex].GetComponent<Image>().overrideSprite = null;
            selectedSlotIndex = -1;
        }
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
