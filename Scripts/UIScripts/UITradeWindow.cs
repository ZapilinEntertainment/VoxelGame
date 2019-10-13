using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UITradeWindow : UIObserver {
#pragma warning disable 0649
    [SerializeField] GameObject[] resourcesButtons;
    [SerializeField] GameObject resourceInfoPanel;
    [SerializeField] Text resourceName, buyButtonText, sellButtonText, limitText, priceText, demandText, descriptionText;
    [SerializeField] InputField limitInputField;
    [SerializeField] Sprite overridingButtonSprite;
    [SerializeField] RawImage resourcePicture;
#pragma warning restore 0649

    int chosenResourceID = -1, chosenButtonIndex = -1, showingLimit;
    float showingPrice = 0, showingDemand = 0;
    List<int> tradableResources;
    bool? resourceIsForSale = null;

    private void Awake()
    {
        tradableResources = new List<int>() {
            ResourceType.STONE_ID,  ResourceType.DIRT_ID,  ResourceType.LUMBER_ID,  ResourceType.METAL_K_ID,
            ResourceType.METAL_M_ID,  ResourceType.METAL_E_ID,  ResourceType.METAL_N_ID,  ResourceType.METAL_P_ID,
            ResourceType.METAL_S_ID,  ResourceType.MINERAL_F_ID,  ResourceType.MINERAL_L_ID,  ResourceType.PLASTICS_ID,
            ResourceType.FOOD_ID ,  ResourceType.CONCRETE_ID,  ResourceType.METAL_K_ORE_ID,  ResourceType.METAL_M_ORE_ID,
            ResourceType.METAL_E_ORE_ID, ResourceType.METAL_N_ORE_ID , ResourceType.METAL_P_ORE_ID,  ResourceType.METAL_S_ORE_ID,
            ResourceType.FUEL_ID,  ResourceType.GRAPHONIUM_ID,  ResourceType.SUPPLIES_ID, ResourceType.SNOW_ID
        };
        LocalizeTitles();
    }

    override public void LocalizeTitles() {
        buyButtonText.text = Localization.GetWord(LocalizedWord.Buy);
        sellButtonText.text = Localization.GetWord(LocalizedWord.Sell);
        limitText.text = Localization.GetWord(LocalizedWord.Limitation);
        priceText.text = Localization.GetWord(LocalizedWord.Price);
        demandText.text = Localization.GetWord(LocalizedWord.Demand);
    }

    override public void StatusUpdate()
    {
        if ( isObserving )
        {
            if (chosenResourceID == -1)
            {
                isObserving = false;
                return;
            }
            UpdateSaleMarkers();
            if (resourceIsForSale != Dock.isForSale[chosenResourceID])
            {
                ChangeSellStatus(Dock.isForSale[chosenResourceID]);
            }
            if (resourceIsForSale != null)
            {
                bool selling = (resourceIsForSale == true);
                if (selling)
                {
                    if (showingPrice != ResourceType.prices[chosenResourceID] * GameMaster.sellPriceCoefficient)
                    {
                        showingPrice = ResourceType.prices[chosenResourceID] * GameMaster.sellPriceCoefficient;
                        priceText.text = Localization.GetWord(LocalizedWord.Price) + " : " + string.Format("{0:0.###}", showingPrice);
                    }
                }
                else { 
                    if (showingPrice != ResourceType.prices[chosenResourceID])
                    {
                        showingPrice = ResourceType.prices[chosenResourceID];
                        priceText.text = Localization.GetWord(LocalizedWord.Price) + " : " + string.Format("{0:0.###}", showingPrice);
                    }
                }
                if (showingLimit != Dock.minValueForTrading[chosenResourceID])
                {
                    showingLimit = Dock.minValueForTrading[chosenResourceID];
                    limitInputField.text = showingLimit.ToString();
                }                
            }       
            else
            {
                if (showingPrice != ResourceType.prices[chosenResourceID])
                {
                    showingPrice = ResourceType.prices[chosenResourceID];
                    priceText.text = Localization.GetWord(LocalizedWord.Price) + " : " + string.Format("{0:0.###}", showingPrice);
                }
            }

            if (showingDemand != ResourceType.demand[chosenResourceID])
            {
                showingDemand = ResourceType.demand[chosenResourceID];
                demandText.text = Localization.GetWord(LocalizedWord.Demand) + " : " + string.Format("{0:0.###}", showingDemand);
            }
        }
    }

    void UpdateSaleMarkers()
    {
        bool?[] saleStatus = Dock.isForSale;
        for (int i = 0; i < resourcesButtons.Length; i++)
        {
            if (i < tradableResources.Count)
            {
                int index = tradableResources[i];
                RawImage saleStatusMarker = resourcesButtons[i].transform.GetChild(2).GetComponent<RawImage>();
                if (saleStatus[index] == null) saleStatusMarker.enabled = false;
                else
                {
                    saleStatusMarker.uvRect = UIController.GetIconUVRect((saleStatus[index] == true) ? Icons.RedArrow : Icons.GreenArrow);
                }
            }
            else
            {
                resourcesButtons[i].SetActive(false);
            }
        }
    }

    public void SelectResource(int buttonIndex, int resourceID)
    {
        if (chosenButtonIndex != -1) resourcesButtons[chosenButtonIndex].GetComponent<Image>().overrideSprite = null;
        chosenButtonIndex = buttonIndex;
        resourcesButtons[chosenButtonIndex].GetComponent<Image>().overrideSprite = overridingButtonSprite;
        chosenResourceID = resourceID;

        resourceInfoPanel.SetActive(true);
        resourceName.text = Localization.GetResourceName(chosenResourceID);
        descriptionText.text = Localization.GetResourcesDescription(chosenResourceID);
        resourcePicture.uvRect = ResourceType.GetResourceIconRect(chosenResourceID);
        ChangeSellStatus(Dock.isForSale[chosenResourceID]);

        resourceIsForSale = Dock.isForSale[resourceID];
        if (resourceIsForSale != null)
        {
            bool selling = (resourceIsForSale == true);                 
            limitText.transform.parent.gameObject.SetActive(true);
            showingPrice = selling ? ResourceType.prices[chosenResourceID] * GameMaster.sellPriceCoefficient : ResourceType.prices[chosenResourceID];
            showingLimit = Dock.minValueForTrading[chosenResourceID];
            limitInputField.text = showingLimit.ToString();            
            RawImage ri = resourcesButtons[chosenButtonIndex].transform.GetChild(2).GetComponent<RawImage>();
            ri.enabled = true;
            ri.uvRect = UIController.GetIconUVRect(selling ? Icons.RedArrow : Icons.GreenArrow);
        }
        else
        {
            limitInputField.transform.parent.gameObject.SetActive(false);
            showingPrice = ResourceType.prices[chosenResourceID];
        }        
        priceText.text = Localization.GetWord(LocalizedWord.Price) + " : " + string.Format("{0:0.###}", showingPrice);
        showingDemand = ResourceType.demand[chosenResourceID];
        demandText.text = Localization.GetWord(LocalizedWord.Demand) + " : " + string.Format("{0:0.###}", showingDemand);
        isObserving = true;
    }
    public void ChangeSellStatus( bool? isForSale)
    {
        if (chosenResourceID == -1) return;
        
        if (isForSale != null)
        {           
            resourceIsForSale = isForSale;
            bool selling = (resourceIsForSale == true);
            buyButtonText.transform.parent.GetComponent<Image>().overrideSprite = selling ? null : overridingButtonSprite;
            sellButtonText.transform.parent.GetComponent<Image>().overrideSprite = selling ? overridingButtonSprite : null;
            showingPrice = selling ? ResourceType.prices[chosenResourceID] * GameMaster.sellPriceCoefficient : ResourceType.prices[chosenResourceID];
            priceText.text = Localization.GetWord(LocalizedWord.Price) + " : " + string.Format("{0:0.###}", showingPrice);
            limitInputField.transform.parent.gameObject.SetActive(true);
            showingLimit = Dock.minValueForTrading[chosenResourceID];
            limitInputField.text = showingLimit.ToString();
            showingDemand = ResourceType.demand[chosenResourceID];
            demandText.text = Localization.GetWord(LocalizedWord.Demand) + " : " + string.Format("{0:0.###}", showingDemand);
            RawImage ri = resourcesButtons[chosenButtonIndex].transform.GetChild(2).GetComponent<RawImage>();
            ri.enabled = true;
            ri.uvRect = UIController.GetIconUVRect(selling ? Icons.RedArrow : Icons.GreenArrow);
        }
        else
        {
            resourceIsForSale = null;
            showingPrice = 0;
            showingLimit = 0;
            showingDemand = 0;
            limitText.transform.parent.gameObject.SetActive(false);
            buyButtonText.transform.parent.GetComponent<Image>().overrideSprite = null;
            sellButtonText.transform.parent.GetComponent<Image>().overrideSprite = null;
            resourcesButtons[chosenButtonIndex].transform.GetChild(2).GetComponent<RawImage>().enabled = false;
        }
    }
    public void ChangeLimit(int val)
    {
        if (chosenResourceID == -1) return;
        int x = Dock.minValueForTrading[chosenResourceID];
        x += val;
        if (x < 0) x = 0;
        Dock.ChangeMinValue(chosenResourceID, x);
        showingLimit = x;
        limitInputField.text = showingLimit.ToString();
        showingLimit = x;
    } 
    public void ChangeLimit()
    {
        int x = 0;
        if (int.TryParse(limitInputField.text, out x))
        {
            if (x < 0) x = 0;
            Dock.ChangeMinValue(chosenResourceID, x);
            limitInputField.text = x.ToString();
            showingLimit = x;
        }
    }
    public void UpdateResourceButtons()
    {
        bool?[] saleStatus = Dock.isForSale;
        for (int i = 0; i < resourcesButtons.Length; i++)
        {
            if (i < tradableResources.Count) {
                GameObject res = resourcesButtons[i];
                res.SetActive(true);
                res.transform.GetChild(0).GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(tradableResources[i]);
                res.transform.GetChild(1).GetComponent<Text>().text = Localization.GetResourceName(tradableResources[i]);
                int id = tradableResources[i];
                int index = i;
                res.GetComponent<Button>().onClick.AddListener(() => {
                    this.SelectResource(index, id);
                });
                RawImage saleStatusMarker = res.transform.GetChild(2).GetComponent<RawImage>();
                if (saleStatus[tradableResources[i]] == null) saleStatusMarker.enabled = false;
                else
                {
                    saleStatusMarker.uvRect = UIController.GetIconUVRect((saleStatus[tradableResources[i]] == true) ? Icons.RedArrow : Icons.GreenArrow);
                    saleStatusMarker.enabled = true;
                }
            }
            else
            {
                resourcesButtons[i].SetActive(false);
            }
        }
    }
    public void BuyButton() {
        if (resourceIsForSale != false)
        { 
            Dock.ChangeSaleStatus(chosenResourceID, false);
            ChangeSellStatus(false);
        }
        else
        {
            Dock.ChangeSaleStatus(chosenResourceID, null);
            ChangeSellStatus(null);
        }
    }
    public void SellButton()
    {
        if (resourceIsForSale == true)
        {
            ChangeSellStatus(null);
            Dock.ChangeSaleStatus(chosenResourceID, null);
        }
        else
        {
            ChangeSellStatus(true);
            Dock.ChangeSaleStatus(chosenResourceID, true);
        }
    }

    new private void OnEnable()
    {
        transform.SetAsLastSibling();
        UpdateResourceButtons();
        UIController.current.ChangeActiveWindow(ActiveWindowMode.TradePanel);
        if (!subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
    }
    new private void OnDisable()
    {
        if (UIController.current.currentActiveWindowMode == ActiveWindowMode.TradePanel) UIController.current.ChangeActiveWindow(ActiveWindowMode.NoWindow);
        if (subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent -= StatusUpdate;
            subscribedToUpdate = false;
        }
    }

    public override void SelfShutOff() {
        if (chosenButtonIndex != -1)
        {
            resourcesButtons[chosenButtonIndex].GetComponent<Image>().overrideSprite = null;
            chosenButtonIndex = -1;
        }
        chosenResourceID = -1;
        resourceInfoPanel.SetActive(false);
        isObserving = false;
        UIController.current.DropActiveWindow(ActiveWindowMode.TradePanel);
        gameObject.SetActive(false);
    }
}
