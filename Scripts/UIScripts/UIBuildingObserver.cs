using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingObserver : UIObserver {
	private Building observingBuilding;
    private bool status_connectedToPowerGrid = false, status_energySupplied = false, status_active = false, canBeUpgraded = false, infoPanel_InUpgradeMode = true;
	private float showingEnergySurplus = 0;
	private int showingHousing = 0;
    private byte savedLevel = 0;
    private Vector2[] savedResourcesValues;
#pragma warning disable 0649
    [SerializeField] private RawImage energyImage, housingImage; //fiti
    [SerializeField] private Text energyValue, housingValue, upgradeButtonText, additionalText; // fiti
    [SerializeField] private Button upgradeButton; // fiti
    [SerializeField] private GameObject upgradeInfoPanel, chargeButton, additionalButton; // fiti
    [SerializeField] private GameObject[] resourceCostIndicator; // fiti
    [SerializeField] private Button energyButton; // fiti
#pragma warning restore 0649

    private void Awake()
    {
        savedResourcesValues = new Vector2[resourceCostIndicator.Length];
    }

    public static UIBuildingObserver InitializeBuildingObserverScript()
    {
        UIBuildingObserver ub = Instantiate(Resources.Load<GameObject>("UIPrefs/buildingObserver"), UIController.current.rightPanel.transform).GetComponent<UIBuildingObserver>();
        Building.buildingObserver = ub;
        ub.LocalizeTitles();
        return ub;
    }

    public void SetObservingBuilding(Building b) {
		if (b == null) {
			SelfShutOff();
			return;
		}
		UIStructureObserver us = Structure.structureObserver;
        if (us == null) us = UIStructureObserver.InitializeStructureObserverScript();
        else us.gameObject.SetActive(true);
		observingBuilding = b; isObserving = true;
		us.SetObservingStructure(observingBuilding);

        // #redraw
		status_connectedToPowerGrid = b.connectedToPowerGrid;
        status_active = b.isActive;
        status_energySupplied = b.isEnergySupplied;

        if (status_active)
        {
            if (status_connectedToPowerGrid)
            {
                if (b.ID == Structure.ENERGY_CAPACITOR_1_ID | b.ID == Structure.ENERGY_CAPACITOR_2_ID )
                {
                    chargeButton.SetActive(true);
                    energyValue.enabled = false;
                }
                else
                {
                    showingEnergySurplus = b.energySurplus;
                    if (status_energySupplied)
                    {
                        if (observingBuilding.ID != Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID)
                        {
                            if (showingEnergySurplus <= 0)
                            {
                                energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                                energyImage.uvRect = UIController.GetIconUVRect(observingBuilding.canBePowerSwitched ? Icons.PowerButton : Icons.PowerMinus);
                            }
                            else
                            {
                                energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                                energyImage.uvRect = UIController.GetIconUVRect(Icons.PowerPlus);
                            }
                        }
                        else
                        {
                            energyValue.text = string.Format("{0,1:F}", showingEnergySurplus) + " / " + ((int)(GameConstants.ENERGY_IN_CRYSTAL)).ToString();
                            energyImage.uvRect = UIController.GetIconUVRect(Icons.PowerPlus);
                        }
                    }
                    else
                    {
                        energyValue.text = Localization.GetWord(LocalizedWord.Offline);
                        energyButton.GetComponent<RawImage>().uvRect = UIController.GetIconUVRect(Icons.OutOfPowerButton);
                    }
                    energyValue.enabled = true;
                    chargeButton.SetActive(false);
                }
                energyImage.enabled = true;
            }
            else
            {
                energyValue.enabled = false;
                energyImage.enabled = false;
            }
        }
        else
        {
            bool ps = observingBuilding.canBePowerSwitched;
            chargeButton.SetActive(false);
            energyValue.text = ps ? Localization.GetPhrase( LocalizedPhrase.PressToTurnOn) : Localization.GetWord(LocalizedWord.Offline);
            energyValue.enabled = true;            
            energyImage.uvRect = UIController.GetIconUVRect(ps ? Icons.TurnOn : Icons.OutOfPowerButton);
            energyImage.enabled = true;
        }
        energyButton.interactable = observingBuilding.canBePowerSwitched;
        //# eo redraw
        bool enableAdditionalElements = false;
		if (b is House) {
			showingHousing = (b as House).housing;
			housingValue.text = showingHousing.ToString();
			housingValue.enabled = true;
			housingImage.enabled = true;
            if (b.ID == Structure.SETTLEMENT_CENTER_ID)
            {
                enableAdditionalElements = true;
                var st = b as Settlement;
                additionalText.text = st.pointsFilled.ToString() + " / " + st.maxPoints.ToString();
                additionalButton.GetComponent<Button>().interactable = st.pointsFilled < Settlement.MAX_POINTS_COUNT;
            }
		}
		else {
			housingValue.enabled = false;
			housingImage.enabled = false;
		}
        if (enableAdditionalElements)
        {
            additionalButton.SetActive(true);
            additionalText.gameObject.SetActive(true);
        }
        else
        {
            if (additionalButton.activeSelf)
            {
                additionalButton.SetActive(false);
                additionalText.gameObject.SetActive(false);
            }
        }

        CheckUpgradeAvailability();       
	}

	override public void StatusUpdate() {
		if ( !isObserving ) return;
        if (observingBuilding == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            bool mustBeRedrawn = false;
            if (status_active != observingBuilding.isActive)    mustBeRedrawn = true;
            else
            {
                if (status_active )
                {
                    if (status_connectedToPowerGrid != observingBuilding.connectedToPowerGrid) mustBeRedrawn = true;
                    else
                    {
                        if (status_connectedToPowerGrid)
                        {
                            if (status_energySupplied != observingBuilding.isEnergySupplied) mustBeRedrawn = true;
                            else
                            {
                                if (status_energySupplied & showingEnergySurplus != observingBuilding.energySurplus) mustBeRedrawn = true;
                                else
                                {
                                    if (showingEnergySurplus != observingBuilding.energySurplus)
                                    {
                                        showingEnergySurplus = observingBuilding.energySurplus;
                                        energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                                        mustBeRedrawn = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (mustBeRedrawn)
            {
                // #redraw - changed
                status_connectedToPowerGrid = observingBuilding.connectedToPowerGrid;
                status_active = observingBuilding.isActive;
                status_energySupplied = observingBuilding.isEnergySupplied;

                showingEnergySurplus = observingBuilding.energySurplus;
                if (status_active)
                {
                    if (status_connectedToPowerGrid)
                    {
                        if (status_energySupplied)
                        {
                            if (observingBuilding.ID != Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID)
                            {
                                if (showingEnergySurplus <= 0)
                                {
                                    energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                                    energyImage.uvRect = UIController.GetIconUVRect(observingBuilding.canBePowerSwitched ? Icons.PowerButton : Icons.PowerMinus);
                                }
                                else
                                {
                                    energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                                    energyImage.uvRect = UIController.GetIconUVRect(Icons.PowerPlus);
                                }
                            }
                            else
                            {
                                energyValue.text = string.Format("{0,1:F}", showingEnergySurplus) + " / " + ((int)(GameConstants.ENERGY_IN_CRYSTAL)).ToString();
                                energyImage.uvRect = UIController.GetIconUVRect(Icons.PowerPlus);
                            }
                        }
                        else
                        {
                            energyValue.text = Localization.GetWord(LocalizedWord.Offline);
                            energyImage.uvRect = UIController.GetIconUVRect(Icons.OutOfPowerButton);
                        }
                    }
                    else
                    {
                        energyValue.text = "-//-";
                        energyImage.uvRect = UIController.GetIconUVRect(Icons.DisabledBuilding);
                    }
                }
                else
                {
                    chargeButton.SetActive(false);
                    energyValue.text = Localization.GetWord(LocalizedWord.Disabled);
                    energyValue.enabled = true;
                    energyImage.uvRect = UIController.GetIconUVRect(Icons.DisabledBuilding);
                    energyImage.enabled = true;
                }
            }
            //# eo redraw

            if (canBeUpgraded) {
                string s = upgradeButtonText.text;
                string answer = string.Empty;
                upgradeButton.interactable = observingBuilding.IsLevelUpPossible(ref answer);
                if (answer != s) {
                    if (answer == string.Empty)
                    {
                        if (observingBuilding.ID != Structure.SETTLEMENT_CENTER_ID)
                            upgradeButtonText.text = Localization.GetWord(LocalizedWord.Upgrade);
                        else
                        {
                            var sc = observingBuilding as Settlement;
                            if (sc.level == Settlement.MAX_HOUSING_LEVEL & sc.pointsFilled == Settlement.MAX_POINTS_COUNT)
                            {
                                upgradeButtonText.text = Localization.GetPhrase(LocalizedPhrase.ConvertToBlock);
                            }
                            else
                            {
                                upgradeButtonText.text = Localization.GetWord(LocalizedWord.Upgrade);
                            }
                        }
                        upgradeButtonText.color = Color.white;
                    }
                    else {
                        upgradeButtonText.text = answer;
                        upgradeButtonText.color = Color.yellow;
                    }
                }
            }
            if (savedLevel != observingBuilding.level) {
                Structure.structureObserver.CheckName();
                CheckUpgradeAvailability();
                if (resourceCostIndicator[0].activeSelf) {
                    RefreshResourcesData();
                }
                savedLevel = observingBuilding.level;
            }
            else
            {
                if (resourceCostIndicator[0].activeSelf) {
                    float[] storageVal = GameMaster.realMaster.colonyController.storage.standartResources;
                    for (int i = 0; i < resourceCostIndicator.Length; i++) {
                        GameObject g = resourceCostIndicator[i];
                        if (g.activeSelf) {
                            Text t = g.transform.GetChild(0).GetComponent<Text>();
                            t.color = savedResourcesValues[i].y > storageVal[(int)savedResourcesValues[i].x] ? Color.red : Color.white;
                        }
                    }
                }
            }

            if (observingBuilding.ID == Structure.SETTLEMENT_CENTER_ID)
            {
                var sc = observingBuilding as Settlement;
                showingHousing = sc.housing;
                housingValue.text = showingHousing.ToString();
                additionalText.text = sc.pointsFilled.ToString() + " / " + sc.maxPoints.ToString();
                additionalButton.GetComponent<Button>().interactable = sc.pointsFilled < Settlement.MAX_POINTS_COUNT;
            }
        }
	}

    virtual public void UpgradeInfoPanelSwitch() {
        if (observingBuilding == null) {
            SelfShutOff();
            return;
        }
        if (upgradeInfoPanel.activeSelf)
        {
            if (infoPanel_InUpgradeMode) upgradeInfoPanel.SetActive(false);
        }
        else {
            if (observingBuilding.upgradedIndex == -1) return;
            upgradeInfoPanel.SetActive(true);            
            infoPanel_InUpgradeMode = true;
        }
        RefreshResourcesData();
    }
    public void AdditionalButtonPanelSwitch()
    {
        if (observingBuilding == null)
        {
            SelfShutOff();
            return;
        }
        else
        {
            if (infoPanel_InUpgradeMode)
            {
                infoPanel_InUpgradeMode = false;
                if (!upgradeInfoPanel.activeSelf) upgradeInfoPanel.SetActive(true);
            }
            else
            {
                if (upgradeInfoPanel.activeSelf) upgradeInfoPanel.SetActive(false);
            }
            RefreshResourcesData();
        }
    }
    public void CancelButton()
    {
        upgradeInfoPanel.SetActive(false);
        infoPanel_InUpgradeMode = true;
    }

    void RefreshResourcesData() {
        if (infoPanel_InUpgradeMode)
        {
            ResourceContainer[] cost = observingBuilding.GetUpgradeCost();
            if (cost != null && cost.Length != 0)
            {
                float[] storageVolume = GameMaster.realMaster.colonyController.storage.standartResources;
                for (int i = 0; i < resourceCostIndicator.Length; i++)
                {
                    if (i < cost.Length)
                    {
                        resourceCostIndicator[i].GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(cost[i].type.ID);
                        Text t = resourceCostIndicator[i].transform.GetChild(0).GetComponent<Text>();
                        t.text = Localization.GetResourceName(cost[i].type.ID) + " : " + string.Format("{0:0.##}", cost[i].volume);
                        t.color = cost[i].volume > storageVolume[cost[i].type.ID] ? Color.red : Color.white;
                        savedResourcesValues[i] = new Vector2(cost[i].type.ID, cost[i].volume);
                        resourceCostIndicator[i].SetActive(true);
                    }
                    else resourceCostIndicator[i].SetActive(false);
                }
            }
        }
        else
        {
            ResourceContainer[] cost = ResourcesCost.GetAdditionalSettlementBuildingCost(observingBuilding.level);
            if (cost != null && cost.Length != 0)
            {
                float[] storageVolume = GameMaster.realMaster.colonyController.storage.standartResources;
                for (int i = 0; i < resourceCostIndicator.Length; i++)
                {
                    if (i < cost.Length)
                    {
                        resourceCostIndicator[i].GetComponent<RawImage>().uvRect = ResourceType.GetResourceIconRect(cost[i].type.ID);
                        Text t = resourceCostIndicator[i].transform.GetChild(0).GetComponent<Text>();
                        t.text = Localization.GetResourceName(cost[i].type.ID) + " : " + string.Format("{0:0.##}", cost[i].volume);
                        t.color = cost[i].volume > storageVolume[cost[i].type.ID] ? Color.red : Color.white;
                        savedResourcesValues[i] = new Vector2(cost[i].type.ID, cost[i].volume);
                        resourceCostIndicator[i].SetActive(true);
                    }
                    else resourceCostIndicator[i].SetActive(false);
                }
            }
        }
    }
    public void CheckUpgradeAvailability() {
        if (observingBuilding.upgradedIndex != -1)
        {
            canBeUpgraded = true;
            upgradeButton.gameObject.SetActive(true);
            string s = upgradeButtonText.text;
            string answer = string.Empty;
            bool upgradePossibleNow = observingBuilding.IsLevelUpPossible(ref answer);
            upgradeButton.interactable = upgradePossibleNow;
            if (answer != s)
            {
                if (answer == string.Empty)
                {
                    upgradeButtonText.text = Localization.GetWord(LocalizedWord.Upgrade);
                    upgradeButtonText.color = Color.white;
                }
                else
                {
                    upgradeButtonText.text = answer;
                    upgradeButtonText.color = Color.yellow;
                }
            }
            if (savedLevel != observingBuilding.level)
            {
                Structure.structureObserver.CheckName();
                if (resourceCostIndicator[0].activeSelf)
                {
                    RefreshResourcesData();
                }
                savedLevel = observingBuilding.level;
            }
        }
        else
        {
            canBeUpgraded = false;
            upgradeButton.gameObject.SetActive(false);
            if (upgradeInfoPanel.activeSelf) upgradeInfoPanel.SetActive(false);
        }
    }

    public void Upgrade() {
        if (observingBuilding == null)
        {
            SelfShutOff();
            return;
        }
        else {
            if (infoPanel_InUpgradeMode)
            {
                observingBuilding.LevelUp(true);
                upgradeInfoPanel.SetActive(false);
                if (observingBuilding.upgradedIndex < 0)
                {
                    CheckUpgradeAvailability();
                }                
            }
            else
            {
                var s2 = observingBuilding as Settlement;
                s2.CreateNewBuilding(true);
                bool x = s2.pointsFilled < Settlement.MAX_POINTS_COUNT;
                additionalButton.GetComponent<Button>().interactable = x;
                if (x == false) upgradeInfoPanel.SetActive(false);
                StatusUpdate();
            }
        }
    }

    public void PowerToggle() {
        if (observingBuilding == null) {
            SelfShutOff();
            return;
        }
        else
        {
            if ( !observingBuilding.canBePowerSwitched ) return;
            if (!observingBuilding.isActive)
            {
                observingBuilding.SetActivationStatus(true, true);
                if (status_active == false)
                {
                    status_active = true;
                    if (status_energySupplied != observingBuilding.isEnergySupplied)
                    {
                        status_energySupplied = observingBuilding.isEnergySupplied;
                        showingEnergySurplus = observingBuilding.energySurplus;                        
                    }
                    if (showingEnergySurplus > 0)
                    {
                        energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                        energyImage.uvRect = UIController.GetIconUVRect(Icons.PowerPlus);
                    }
                    else
                    {
                        energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                        energyImage.uvRect = UIController.GetIconUVRect(observingBuilding.canBePowerSwitched ? Icons.PowerButton : Icons.PowerMinus);
                    }
                }
            }
            else
            {
                if (observingBuilding.isEnergySupplied)
                {
                    observingBuilding.SetActivationStatus(false, true);
                    if (status_active == true)
                    {
                        status_active = false;
                        energyValue.text = Localization.GetPhrase(LocalizedPhrase.PressToTurnOn);
                        energyImage.uvRect = UIController.GetIconUVRect(Icons.TurnOn);
                    }
                }
                else
                {
                    observingBuilding.SetEnergySupply(true, true);

                    if (status_energySupplied == false)
                    {
                        status_energySupplied = true;
                        showingEnergySurplus = observingBuilding.energySurplus;
                        if (showingEnergySurplus > 0)
                        {
                            energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                            energyImage.uvRect = UIController.GetIconUVRect(Icons.PowerPlus);
                        }
                        else
                        {
                            energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                            energyImage.uvRect = UIController.GetIconUVRect(observingBuilding.canBePowerSwitched ? Icons.PowerButton : Icons.PowerMinus);
                        }
                    }
                }
            }
        }
    }
    public void Charge()
    {
        ColonyController colony = GameMaster.realMaster.colonyController;
        if (colony.energyCrystalsCount >= 1)
        {
            if (colony.energyStored != colony.totalEnergyCapacity)
            {
                colony.GetEnergyCrystals(1);
                colony.AddEnergy(GameConstants.ENERGY_IN_CRYSTAL);
                if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.BatteryCharged);
            }
        }
        else GameLogUI.NotEnoughMoneyAnnounce();
    }

	override public void SelfShutOff() {
		isObserving = false;
        infoPanel_InUpgradeMode = true;
		Structure.structureObserver.SelfShutOff();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
        infoPanel_InUpgradeMode = true;
        isObserving = false;
		observingBuilding = null;
		Structure.structureObserver.ShutOff();
		gameObject.SetActive(false);
	}

    public override void LocalizeTitles()
    {
        Transform t = upgradeInfoPanel.transform;
        t.GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.UpgradeCost);
        t.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Accept);
        t.GetChild(3).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);
        additionalButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.AddBuilding);
    }
}
