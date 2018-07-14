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
    int showingImmigrationLimit = 0;

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
                RecalculateTradingPanelContent();
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
    public void LimitChangeButton(int resourceID, bool plus)
    {
        int x = Dock.minValueForTrading[resourceID];
        x += MIN_VALUE_CHANGING_STEP * (plus ? 1 : -1);
        if (x < 0) x = 0;
        Dock.ChangeMinValue(resourceID, x);
        RecalculateTradingPanelContent();
        timer = STATUS_UPDATE_TIME / 2f;
    }

    void RecalculateTradingPanelContent()
    {
        Transform tpanel = tradingPanelContent.transform;
        bool?[] saleStatus = Dock.isForSale;
        int buttonsCount = tpanel.childCount;
        List<int> realOperations = new List<int>();
        for (int i = 0; i < ResourceType.RTYPES_COUNT; i++)
        {
            if (saleStatus[i] != null) realOperations.Add(i);
        }
        if (realOperations.Count > 0)
        {
            int i = 0;
            for (; i < realOperations.Count; i++)
            {
                Transform t = null;
                int resID = realOperations[i];
                if (i < buttonsCount) t = tpanel.GetChild(i);
                else t = Instantiate(tpanel.GetChild(0).gameObject, tpanel).transform;
                t.gameObject.SetActive(true);
                t.transform.localPosition = new Vector3(t.localPosition.x, START_Y + (-1) * i * OPERATION_PANEL_HEIGHT, t.localPosition.z);
                t.GetChild(SELL_STATUS_ICON_INDEX).GetComponent<RawImage>().texture = (saleStatus[resID] == true) ? redArrow_tx : greenArrow_tx;
                t.GetChild(NAME_INDEX).GetComponent<Text>().text = Localization.GetResourceName(resID);
                int x = new int();
                x = resID;
                t.GetChild(MINUS_BUTTON_INDEX).GetComponent<Button>().onClick.RemoveAllListeners();
                t.GetChild(MINUS_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
                    this.LimitChangeButton(x, false);
                });
                t.GetChild(LIMIT_VALUE_INDEX).GetComponent<Text>().text = Dock.minValueForTrading[resID].ToString();
                t.GetChild(PLUS_BUTTON_INDEX).GetComponent<Button>().onClick.RemoveAllListeners();
                t.GetChild(PLUS_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
                    this.LimitChangeButton(x, true);
                });
                t.GetChild(DELETE_BUTTON_INDEX).GetComponent<Button>().onClick.RemoveAllListeners();
                t.GetChild(DELETE_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
                    this.RemoveTradeOperation(x);
                });
            }
            if (i < buttonsCount)
            {
                for (int j = i; j < buttonsCount; j++)
                {
                    tpanel.GetChild(j).gameObject.SetActive(false);
                }
            }
            if (!tpanel.gameObject.activeSelf) tpanel.gameObject.SetActive(true);
        }
        else
        {
            if (tpanel.gameObject.activeSelf) tpanel.gameObject.SetActive(false);
        }
        RectTransform rt = tpanel as RectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, OPERATION_PANEL_HEIGHT * realOperations.Count);
    }

    public void RemoveTradeOperation(int resourceID)
    {
        Dock.ChangeSaleStatus(resourceID, null);
        RecalculateTradingPanelContent();
        timer = STATUS_UPDATE_TIME / 2f;
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
