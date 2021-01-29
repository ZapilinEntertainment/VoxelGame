using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIDockObserver : UIObserver
{
    #pragma warning disable 0649
    [SerializeField] private Text tradingButtonText, immigrationButtonText, immigrationLimitText, nextShipTimer;
    [SerializeField] private GameObject tradingListPanel, immigrationPanel, tradingPanelContent;
    [SerializeField] private Image immigrationToggleButtonImage;
    [SerializeField] private InputField immigrationLimitInputField;
    [SerializeField] private Sprite overridingSprite;
    #pragma warning restore 0649
    private Dock observingDock;
    private const float START_Y = -16, OPERATION_PANEL_HEIGHT = 32;
    private const int MIN_VALUE_CHANGING_STEP = 5, SELL_STATUS_ICON_INDEX = 0, NAME_INDEX = 1, MINUS_BUTTON_INDEX = 2, LIMIT_VALUE_INDEX = 3, PLUS_BUTTON_INDEX = 4, DELETE_BUTTON_INDEX = 5;
    private int showingImmigrationLimit = 0;
    private DockSystem dockSystem;

    public static UIDockObserver InitializeDockObserverScript()
    {
        UIDockObserver udo = Instantiate(Resources.Load<GameObject>("UIPrefs/dockObserver"), mycanvas.rightPanel.transform).GetComponent<UIDockObserver>();
        Dock.dockObserver = udo;
        udo.LocalizeTitles();
        return udo;
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
            if (dockSystem == null) dockSystem = DockSystem.GetCurrent();
            observingDock = d; isObserving = true;
            UIWorkbuildingObserver uwb = WorkBuilding.workbuildingObserver;
            if (uwb == null) uwb = UIWorkbuildingObserver.InitializeWorkbuildingObserverScript();
            else uwb.gameObject.SetActive(true);
            uwb.SetObservingPlace(observingDock);
            if (tradingListPanel.activeSelf) PrepareTradingPanel();
            else PrepareImmigrationPanel();
            if (observingDock.correctLocation) nextShipTimer.text = observingDock.shipArrivingTimer.ToString();
            else nextShipTimer.text = Localization.GetPhrase(LocalizedPhrase.PathBlocked);
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
        RefreshTradeOperationsList();
        mycanvas.ActivateTradePanel();
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
        if (dockSystem.immigrationEnabled) {
            immigrationToggleButtonImage.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ColonizationEnabled);
            immigrationToggleButtonImage.overrideSprite = overridingSprite;
            immigrationLimitInputField.text = dockSystem.immigrationPlan.ToString();
            immigrationLimitInputField.transform.parent.gameObject.SetActive(true);
            immigrationLimitText.enabled = true;
            immigrationLimitText.text = Localization.GetPhrase(LocalizedPhrase.TicketsLeft) + " : " + dockSystem.immigrationPlan.ToString();
        }
        else
        {
            immigrationToggleButtonImage.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.ColonizationDisabled);
            immigrationToggleButtonImage.overrideSprite = null;
            immigrationLimitText.enabled = false;
        }
    }

    override public void StatusUpdate()
    {
        if (!isObserving) return;
        if (observingDock == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (tradingListPanel.activeSelf)
            {
                RefreshTradeOperationsList();
            }
            else
            {
                if ((immigrationToggleButtonImage.overrideSprite == null) != dockSystem.immigrationEnabled)
                {
                    if (showingImmigrationLimit != dockSystem.immigrationPlan)
                    {
                        showingImmigrationLimit = dockSystem.immigrationPlan;
                        immigrationLimitText.text = Localization.GetPhrase(LocalizedPhrase.TicketsLeft) + " : " + showingImmigrationLimit.ToString();
                    }
                }
                else
                {
                    PrepareImmigrationPanel();
                }
            }
            if (observingDock.correctLocation) nextShipTimer.text = observingDock.shipArrivingTimer.ToString();
            else nextShipTimer.text = Localization.GetPhrase(LocalizedPhrase.PathBlocked);
        }
    } 

    public void ImmigrationToggleButton()
    {
        dockSystem.SetImmigrationStatus((!dockSystem.immigrationEnabled), showingImmigrationLimit);
        PrepareImmigrationPanel();
    }
    public void ChangeImmigrationLimitButton(int x)
    {
        showingImmigrationLimit = dockSystem.immigrationPlan + x;
        if (showingImmigrationLimit < 0) showingImmigrationLimit = 0;
        dockSystem.SetImmigrationStatus(true, showingImmigrationLimit);
        immigrationLimitText.text = Localization.GetPhrase(LocalizedPhrase.TicketsLeft) + " : " + dockSystem.immigrationPlan.ToString();
        immigrationLimitInputField.text = showingImmigrationLimit.ToString();
    }
    public void ImmigrationLimitChanged()
    {
        if (int.TryParse(immigrationLimitInputField.text, out showingImmigrationLimit)) {
            if (showingImmigrationLimit < 0) showingImmigrationLimit = 0;
            dockSystem.SetImmigrationStatus(true, showingImmigrationLimit);
        }
    }


    #region trade operations list    
    public void LimitChangeButton(int resourceID, bool plus)
    {
        int x = dockSystem.minValueForTrading[resourceID];
        x += MIN_VALUE_CHANGING_STEP * (plus ? 1 : -1);
        if (x < 0) x = 0;
        dockSystem.ChangeMinValue(resourceID, x);
        RefreshTradeOperationsList();
    }

    void RefreshTradeOperationsList()
    {
        Transform tpanel = tradingPanelContent.transform;
        bool?[] saleStatus = dockSystem.isForSale;
        int buttonsCount = tpanel.childCount;
        List<int> realOperations = new List<int>();
        for (int i = 0; i < ResourceType.TYPES_COUNT; i++)
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
                t.GetChild(SELL_STATUS_ICON_INDEX).GetComponent<RawImage>().uvRect = UIController.GetIconUVRect((saleStatus[resID] == true) ? Icons.RedArrow : Icons.GreenArrow);
                t.GetChild(NAME_INDEX).GetComponent<Text>().text = Localization.GetResourceName(resID);
                int x = new int();
                x = resID;
                t.GetChild(MINUS_BUTTON_INDEX).GetComponent<Button>().onClick.RemoveAllListeners();
                t.GetChild(MINUS_BUTTON_INDEX).GetComponent<Button>().onClick.AddListener(() => {
                    this.LimitChangeButton(x, false);
                });
                t.GetChild(LIMIT_VALUE_INDEX).GetComponent<Text>().text = dockSystem.minValueForTrading[resID].ToString();
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
        dockSystem.ChangeSaleStatus(resourceID, null);
        RefreshTradeOperationsList();
    }
    #endregion

    override public void SelfShutOff()
    {
        isObserving = false;
        WorkBuilding.workbuildingObserver.SelfShutOff();
        mycanvas.ChangeActiveWindow(ActiveWindowMode.NoWindow);
        gameObject.SetActive(false);
    }

    override public void ShutOff()
    {
        isObserving = false;
        observingDock = null;
        WorkBuilding.workbuildingObserver.ShutOff();
        mycanvas.ChangeActiveWindow(ActiveWindowMode.NoWindow);
        gameObject.SetActive(false);
    }

    public override void LocalizeTitles()
    {
        tradingButtonText.text = Localization.GetWord(LocalizedWord.Trading);
        immigrationButtonText.text = Localization.GetWord(LocalizedWord.Colonization);
    }
}
