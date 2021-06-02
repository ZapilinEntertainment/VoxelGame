using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public sealed class ListController : MonoBehaviour
{
    [SerializeField] private RectTransform listHolder;
    [SerializeField] private GameObject[] buttons;
    [SerializeField] private GameObject emptyListLabel;
    [SerializeField] private Scrollbar scrollbar;
    public int activeButtonsCount { get; private set; }
    public int buttonsCount { get; private set; }
    private bool listEnabled = true, spriteLoaded = false;
    private IListable datahoster;
    private Sprite overridingSprite;
    public int selectedButtonIndex
    {
        get
        {
            return _sltditm;
        }
        set
        {            
            if (value < 0 | value > buttonsCount)
            {
                if (_sltditm != -1) buttons[_sltditm].GetComponent<Image>().overrideSprite = null;
                _sltditm = -1;
            }
            else
            {
                if (_sltditm != value)
                {
                    if (_sltditm != -1) buttons[_sltditm].GetComponent<Image>().overrideSprite = null;
                    _sltditm = value;
                    buttons[_sltditm].GetComponent<Image>().overrideSprite = overridingSprite;
                }
            }
        }
    }
    private int _sltditm;
    private Action<int> selectItemAction;

    public void AssignSelectItemAction(Action<int> a)
    {
        selectItemAction = a;
    }
    public void PrepareList(IListable i_datahoster)
    {
        datahoster = i_datahoster;
        selectedButtonIndex = -1;
        if (!spriteLoaded)
        {
            if (PoolMaster.gui_overridingSprite == null) overridingSprite = PoolMaster.LoadOverridingSprite();
            else overridingSprite = PoolMaster.gui_overridingSprite;
            spriteLoaded = true;
        }
        RefreshList();
    }
    private void RefreshList()
    {
        int totalItemsCount = datahoster.GetItemsCount();
        if (totalItemsCount == 0)
        {
            if (listEnabled)
            {
                listHolder.gameObject.SetActive(false);
                listEnabled = false;
                emptyListLabel.SetActive(true);
                scrollbar.gameObject.SetActive(false);
            }           
        }
        else
        {
            buttonsCount = buttons.Length;
            if (totalItemsCount > buttonsCount)
            {
                if (!scrollbar.isActiveAndEnabled) scrollbar.gameObject.SetActive(true);
                activeButtonsCount = buttonsCount;
                // РАСШИРЕННЫЙ СПИСОК
                if (!datahoster.HaveSelectedObject())
                {
                    int sindex = GetListStartIndex();
                    for (int i = 0; i < buttonsCount; i++)
                    {
                        buttons[i].transform.GetChild(0).GetComponent<Text>().text = datahoster.GetName(i + sindex);
                        buttons[i].SetActive(true);
                    }
                }
            }
            else
            {
                if (scrollbar.isActiveAndEnabled) scrollbar.gameObject.SetActive(false);
                activeButtonsCount = totalItemsCount;
                // КОМПАКТНЫЙ СПИСОК
                int i = 0;
                for (; i < totalItemsCount; i++)
                {
                    buttons[i].transform.GetChild(0).GetComponent<Text>().text = datahoster.GetName(i);
                    buttons[i].SetActive(true);
                }
                if (i < buttons.Length)
                {
                    for (; i < buttons.Length; i++)
                    {
                        buttons[i].SetActive(false);
                    }
                }
            }

            if (!listEnabled)
            {
                listHolder.gameObject.SetActive(true);
                listEnabled = true;
                emptyListLabel.SetActive(false);
            }            
        }
    }
    private int GetListStartIndex() // 0-item real index
    {
        int totalListCount = datahoster.GetItemsCount(), 
            visibleListCount = buttons.Length;

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

    public void ButtonClicked(int i) //может вызываться изнутри
    {
        selectedButtonIndex = i;
        selectItemAction?.Invoke(i);
    }

    public void ChangeEmptyLabelText(string s)
    {
        emptyListLabel.GetComponentInChildren<Text>().text = s;
    }

    public void SliderChanged(float x)
    {
        RefreshList();
    }
}
