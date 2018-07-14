using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDockObserver : UIObserver
{
    [SerializeField] Text tradingButtonText, immigrationButtonText, immigrationLimitText;
    [SerializeField] GameObject tradingListPanel, immigrationPanel, tradingPanelContent;
    [SerializeField] Image immigrationToggleButtonImage;
    [SerializeField] InputField immigrationLimitInputField;
    [SerializeField] Sprite overridingSprite;
    [SerializeField] Texture greenArrow_tx, redArrow_tx;
    Dock observingDock;
    const float START_Y = -16, OPERATION_PANEL_HEIGHT = 32;
    const int MIN_VALUE_CHANGING_STEP = 5, SELL_STATUS_ICON_INDEX = 0, NAME_INDEX = 1, MINUS_BUTTON_INDEX = 2, LIMIT_VALUE_INDEX = 3, PLUS_BUTTON_INDEX = 4, DELETE_BUTTON_INDEX = 5;
    List<int> showingTradeLimits;
    List<bool> showingSellStatus;
    int showingImmigrationLimit = 0;

    void Awake() {
        showingTradeLimits = new List<int>(); showingTradeLimits.Add(0);
        showingSellStatus = new List<bool>(); showingSellStatus.Add(false);

    }

     public void SetObservingDock(Dock d)
    {
        if (d == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            observingDock = d; isObserving = true;
            UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
            if (uwb == null)
            {
                uwb = Instantiate(Resources.Load<GameObject>("UIPrefs/workBuildingObserver"), UIController.current.rightPanel.transform).GetComponent<UIWorkbuildingObserver>();
                WorkBuilding.workbuildingObserver = uwb;
            }
            else uwb.gameObject.SetActive(true);
            uwb.SetObservingWorkBuilding(observingDock);
            if (tradingListPanel.activeSelf) PrepareTradingPanel();
            else PrepareImmigrationPanel();
            
            STATUS_UPDATE_TIME = 1.5f; timer = STATUS_UPDATE_TIME;
        }
    }

    public void PrepareTradingPanel ()
    {
        if (observingDock == null)
        {
            SelfShutOff();
            return;
        }
        if ( !tradingListPanel.activeSelf ) tradingListPanel.SetActive(true);
        immigrationPanel.SetActive(false);
        tradingButtonText.transform.parent.GetComponent<Image>().overrideSprite = overridingSprite;
        immigrationButtonText.transform.parent.GetComponent<Image>().overrideSprite = null;
        RecalculateTradingPanelContent();
        UIController.current.ActivateTradePanel();
    }
    public void PrepareImmigrationPanel()
    {
        if (observingDock == null)
        {
            SelfShutOff();
            return;
        }
        if (!immigrationPanel.activeSelf)
        {
            immigrationPanel.SetActive(true);
            immigrationButtonText.transform.parent.GetComponent<Image>().overrideSprite = overridingSprite;
        }
        if (tradingListPanel.activeSelf)
        {
            tradingListPanel.SetActive(false);
            tradingButtonText.transform.parent.GetComponent<Image>().overrideSprite = null;
        }        
        if (Dock.immigrationEnabled) {
            immigrationToggleButtonImage.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ImmigrationEnabled);
            immigrationToggleButtonImage.overrideSprite = overridingSprite;
            immigrationLimitInputField.transform.parent.gameObject.SetActive(true);
            immigrationLimitText.enabled = true;
            immigrationLimitText.text = Localization.GetPhrase(LocalizedPhrase.TicketsLeft) + " : " + Dock.immigrationPlan.ToString();
        }
        else
        {
            immigrationToggleButtonImage.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ImmigrationDisabled);
            immigrationToggleButtonImage.overrideSprite = null;
            immigrationLimitText.enabled = false;
        }
    }

    override protected void StatusUpdate()
    {
        if (observingDock == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (tradingListPanel.activeSelf)
            {

                if (showingSellStatus.Count > 0)
                {
                    int i = 0;
                    bool?[] realSellStatuses = Dock.isForSale;
                    int[] realLimits = Dock.minValueForTrading;
                    return;
                    while (i < showingSellStatus.Count)
                    {
                        if (showingSellStatus[i] != realSellStatuses[i])
                        {
                            if (realSellStatuses[i] != null)
                            {
                                showingSellStatus[i] = (realSellStatuses[i] == true);
                                tradingPanelContent.transform.GetChild(i).GetChild(SELL_STATUS_ICON_INDEX).GetComponent<RawImage>().texture = (showingSellStatus[i] ? redArrow_tx : greenArrow_tx);
                            }
                            else
                            {
                                tradingPanelContent.transform.GetChild(i).GetChild(DELETE_BUTTON_INDEX).GetComponent<Button>().onClick.Invoke();
                                continue;
                            }
                        }
                        if (showingTradeLimits[i] != realLimits[i])
                        {
                            showingTradeLimits[i] = realLimits[i];
                            tradingPanelContent.transform.GetChild(i).GetChild(LIMIT_VALUE_INDEX).GetComponent<Text>().text = showingTradeLimits[i].ToString();
                        }
                        i++;
                    }
                }
            }
            else
            {
                if ((immigrationToggleButtonImage.overrideSprite == null) != Dock.immigrationEnabled)
                {
                    if (showingImmigrationLimit != Dock.immigrationPlan)
                    {
                        showingImmigrationLimit = Dock.immigrationPlan;
                        immigrationLimitText.text = Localization.GetPhrase(LocalizedPhrase.TicketsLeft) + " : " + showingImmigrationLimit.ToString();
                    }
                }
                else
                {
                    PrepareImmigrationPanel();
                }
            }
        }
    }

    public void LimitChangeButton(int buttonIndex,int resourceID, bool plus)
    {
        int x = Dock.minValueForTrading[resourceID];
        x += MIN_VALUE_CHANGING_STEP * (plus ? 1 : -1);
        if (x < 0) x = 0;
        Dock.ChangeMinValue(resourceID, x);
        tradingListPanel.transform.GetChild(buttonIndex).GetChild(LIMIT_VALUE_INDEX).GetComponent<Text>().text = x.ToString();
        showingTradeLimits[buttonIndex] = x;
    }

    public void ImmigrationToggleButton()
    {
        Dock.SetImmigrationStatus((!Dock.immigrationEnabled), 0);
        PrepareImmigrationPanel();
    }
    public void ChangeImmigrationLimitButton(int x)
    {
        showingImmigrationLimit = Dock.immigrationPlan + x;
        if (showingImmigrationLimit < 0) showingImmigrationLimit = 0;
        Dock.SetImmigrationStatus(true, showingImmigrationLimit);
        immigrationLimitText.text = Localization.GetPhrase(LocalizedPhrase.TicketsLeft) + " : " + Dock.immigrationPlan.ToString();
        immigrationLimitInputField.text = showingImmigrationLimit.ToString();
    }
    public void ImmigrationLimitChanged()
    {
        if (int.TryParse(immigrationLimitInputField.text, out showingImmigrationLimit)) {
            if (showingImmigrationLimit < 0) showingImmigrationLimit = 0;
            Dock.SetImmigrationStatus(true, showingImmigrationLimit);
        }
    }


    #region trade operations list
    public void AddLot(int resourceID)
    {        
        if (Dock.isForSale[resourceID] == null) return;
        Transform tpanel = tradingPanelContent.transform;
        int operationsCount = tpanel.childCount;
        int foundedIndex = -1;
        Transform button = null;
        for (int i = 0; i < operationsCount; i++)
        {
            button = tpanel.GetChild(i);
            if (button.gameObject.activeSelf) continue;
            else
            {
                foundedIndex = i;
                break;
            }
        }
        if ( foundedIndex == -1)
        {
            button = Instantiate(tpanel.transform.GetChild(0).gameObject, tpanel.transform).transform;
            foundedIndex = operationsCount++;
        }
        SetLotData(button, resourceID, foundedIndex);
    }

    void SetLotData(Transform gt, int resourceID, int buttonIndex)
    {
        bool? forSale = Dock.isForSale[resourceID];
        gt.gameObject.SetActive(true);
        gt.transform.localPosition = new Vector3(gt.localPosition.x, START_Y + (-1) * buttonIndex * OPERATION_PANEL_HEIGHT, gt.localPosition.z);
        gt.GetChild(SELL_STATUS_ICON_INDEX).GetComponent<RawImage>().texture = (forSale == true) ? redArrow_tx : greenArrow_tx;
        if (showingSellStatus.Count > buttonIndex) showingSellStatus[buttonIndex] = (forSale == true);
        else showingSellStatus.Add(forSale == true);
        gt.GetChild(NAME_INDEX).GetComponent<Text>().text = Localization.GetResourceName(resourceID);
        gt.GetChild(MINUS_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
            this.LimitChangeButton(buttonIndex, resourceID, false);
        });

        if (showingTradeLimits.Count > buttonIndex) showingTradeLimits[buttonIndex] = Dock.minValueForTrading[resourceID];
        else showingTradeLimits.Add(Dock.minValueForTrading[resourceID]);

        gt.GetChild(LIMIT_VALUE_INDEX).GetComponent<Text>().text = showingTradeLimits[buttonIndex].ToString();
        gt.GetChild(PLUS_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
            this.LimitChangeButton(buttonIndex, resourceID, true);
        });
        gt.GetChild(DELETE_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
            this.RemoveTradeOperation(buttonIndex, resourceID);
        });
    }

    void RecalculateTradingPanelContent()
    {
        Transform tpanel = tradingPanelContent.transform;
        int buttonsCount = tpanel.childCount, lastButtonIndex = 0;
        bool?[] saleStatus = Dock.isForSale;        

        for (int i = 0; i < ResourceType.RTYPES_COUNT; i++)
        {
            if (saleStatus[i] != null)
            {
                if (lastButtonIndex < buttonsCount)
                {
                    SetLotData(tpanel.GetChild(lastButtonIndex), i, lastButtonIndex);
                    lastButtonIndex++;
                }
                else
                {
                    Transform t = Instantiate(tpanel.transform.GetChild(0).gameObject, tpanel.transform).transform;
                    SetLotData(t, i, lastButtonIndex);
                    lastButtonIndex++;
                }
            }
            else
            {
                continue;
            }
        }
        if (lastButtonIndex < buttonsCount)
        {
            for (int i = lastButtonIndex; i < buttonsCount; i++)
            {
                tpanel.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

        public void RemoveTradeOperation(int buttonIndex, int resourceID)
    {
        int i = buttonIndex, count = tradingPanelContent.transform.childCount;
        if (i == count - 1)
        {
            tradingPanelContent.transform.GetChild(i).gameObject.SetActive(false);
        }
        else
        {
            i++;
            int positionsDown = 0;
            while (i < count)
            {
                Transform t = tradingPanelContent.transform.GetChild(i).transform;
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + OPERATION_PANEL_HEIGHT, t.localPosition.z);
                positionsDown++;
                i++;
            }
            RectTransform deleting = tradingPanelContent.transform.GetChild(buttonIndex) as RectTransform;
            deleting.SetAsLastSibling();
            deleting.localPosition = new Vector3(deleting.localPosition.x, deleting.localPosition.y - positionsDown * OPERATION_PANEL_HEIGHT, deleting.localPosition.z);
            deleting.gameObject.SetActive(false);
            showingTradeLimits.RemoveAt(buttonIndex);
            showingSellStatus.RemoveAt(buttonIndex);
        }
        Dock.ChangeSaleStatus(resourceID, null);
    }
    #endregion

    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        UIController.current.CloseTradePanel();
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingDock = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        UIController.current.CloseTradePanel();
        gameObject.SetActive(false);
    }


    public void LocalizeButtonTitles()
    {
        tradingButtonText.text = Localization.GetWord(LocalizedWord.Trading);
        immigrationButtonText.text = Localization.GetWord(LocalizedWord.Immigration);   
    }
}
