using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingObserver : UIObserver {
	Building observingBuilding;
    bool status_connectedToPowerGrid = false, status_energySupplied = false, status_active = false, isHouse = false, canBeUpgraded = false;
	float showingEnergySurplus = 0;
	int showingHousing = 0;
    byte savedLevel = 0;
    Vector2[] savedResourcesValues;
#pragma warning disable 0649
    [SerializeField] RawImage energyImage, housingImage; //fiti
    [SerializeField] Text energyValue, housingValue, upgradeButtonText; // fiti
    [SerializeField] Button upgradeButton; // fiti
    [SerializeField] GameObject upgradeInfoPanel, chargeButton; // fiti
    [SerializeField] GameObject[] resourceCostIndicator; // fiti
    [SerializeField] Button energyButton; // fiti
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
                if (b.id == Structure.ENERGY_CAPACITOR_1_ID | b.id == Structure.ENERGY_CAPACITOR_2_ID | b.id == Structure.ENERGY_CAPACITOR_3_ID)
                {
                    chargeButton.SetActive(true);
                    energyValue.enabled = false;
                }
                else
                {
                    showingEnergySurplus = b.energySurplus;
                    if (status_energySupplied)
                    {
                        if (showingEnergySurplus <= 0)
                        {
                            energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                            energyImage.uvRect = UIController.GetTextureUV(Icons.PowerMinus);
                        }
                        else
                        {
                            energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                            energyImage.uvRect = UIController.GetTextureUV(Icons.PowerPlus);
                        }
                    }
                    else
                    {
                        energyValue.text = Localization.GetWord(LocalizedWord.Offline);
                        energyButton.GetComponent<RawImage>().uvRect = UIController.GetTextureUV(Icons.PowerOff);
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
            chargeButton.SetActive(false);
            energyValue.text = Localization.GetWord(LocalizedWord.Disabled);
            energyValue.enabled = true;            
            energyImage.uvRect = UIController.GetTextureUV(Icons.DisabledBuilding);
            energyImage.enabled = true;
        }
        energyButton.interactable = observingBuilding.canBePowerSwitched;
        //# eo redraw

		if (b is House) {
			isHouse = true;
			showingHousing = (b as House).housing;
			housingValue.text = showingHousing.ToString();
			housingValue.enabled = true;
			housingImage.enabled = true;
		}
		else {
			isHouse = false;
			housingValue.enabled = false;
			housingImage.enabled = false;
		}

        CheckUpgradeAvailability();       
	}

	override protected void StatusUpdate() {
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
                            if (showingEnergySurplus > 0)
                            {
                                energyValue.text = '+' + string.Format("{0,1:F}", showingEnergySurplus);
                                energyImage.uvRect = UIController.GetTextureUV(Icons.PowerPlus);
                            }
                            else
                            {
                                energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                                energyImage.uvRect = UIController.GetTextureUV(Icons.PowerMinus);
                            }
                        }
                        else
                        {
                            energyValue.text = Localization.GetWord(LocalizedWord.Offline);
                            energyImage.uvRect = UIController.GetTextureUV(Icons.PowerOff);
                        }
                    }
                    else
                    {
                        energyValue.text = "-//-";
                        energyImage.uvRect = UIController.GetTextureUV(Icons.DisabledBuilding);
                    }
                }
                else
                {
                    chargeButton.SetActive(false);
                    energyValue.text = Localization.GetWord(LocalizedWord.Disabled);
                    energyValue.enabled = true;
                    energyImage.uvRect = UIController.GetTextureUV(Icons.DisabledBuilding);
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
                        upgradeButtonText.text = Localization.GetWord(LocalizedWord.Upgrade);
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
        }
	}

    virtual public void UpgradeInfoPanelSwitch() {
        if (observingBuilding == null) {
            SelfShutOff();
            return;
        }
        if (upgradeInfoPanel.activeSelf)
        {
            upgradeInfoPanel.SetActive(false);
        }
        else {
            if (observingBuilding.upgradedIndex == -1) return;
            upgradeInfoPanel.SetActive(true);
            RefreshResourcesData();
        }
    }

    void RefreshResourcesData() {
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
                    t.text = Localization.GetResourceName(cost[i].type.ID) + " : " + string.Format("{0:0.##}",cost[i].volume);
                    t.color = cost[i].volume > storageVolume[cost[i].type.ID] ? Color.red : Color.white;
                    savedResourcesValues[i] = new Vector2(cost[i].type.ID,cost[i].volume);
                    resourceCostIndicator[i].SetActive(true);
                }
                else resourceCostIndicator[i].SetActive(false);
            }
        }
    }
    void CheckUpgradeAvailability() {
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
            upgradeInfoPanel.SetActive(false);
        }
        upgradeInfoPanel.SetActive(false);
    }

    public void Upgrade() {
        if (observingBuilding == null)
        {
            SelfShutOff();
            return;
        }
        else {
            observingBuilding.LevelUp(true);
            if (observingBuilding.upgradedIndex < 0)
            {
                CheckUpgradeAvailability();
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
                        energyImage.uvRect = UIController.GetTextureUV(Icons.PowerPlus);
                    }
                    else
                    {
                        energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                        energyImage.uvRect = UIController.GetTextureUV(Icons.PowerMinus);
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
                        energyValue.text = Localization.GetWord(LocalizedWord.Disabled);
                        energyImage.uvRect = UIController.GetTextureUV(Icons.PowerOff);
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
                            energyImage.uvRect = UIController.GetTextureUV(Icons.PowerPlus);
                        }
                        else
                        {
                            energyValue.text = string.Format("{0,1:F}", showingEnergySurplus);
                            energyImage.uvRect = UIController.GetTextureUV(Icons.PowerMinus);
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
                if (GameMaster.soundEnabled) GameMaster.audiomaster.MakeSound(NotificationSound.BatteryCharged);
            }
        }
        else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughEnergyCrystals));
    }

	override public void SelfShutOff() {
		isObserving = false;
		Structure.structureObserver.SelfShutOff();
		gameObject.SetActive(false);
	}

	override public void ShutOff() {
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
    }
}
