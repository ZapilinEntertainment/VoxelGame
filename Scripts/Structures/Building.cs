using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure
{
    public bool canBePowerSwitched { get; protected set; }
    public bool isActive { get; protected set; }
    public bool isEnergySupplied { get; protected set; } //управляется только ColonyController'ом
    public bool connectedToPowerGrid { get; protected set; }// установлено ли подключение к электросети
    public float energySurplus { get; protected set; }
    public float energyCapacity { get; protected set; }

    private int _upgradedIndex = -1;
    public int upgradedIndex { get { return _upgradedIndex; } private set { _upgradedIndex = value; } }
    private byte _level = 1;
    public byte level { get { return _level; } protected set { _level = value; } }

    public static UIBuildingObserver buildingObserver;
    public static int[] GetApplicableBuildingsList(byte i_level, Plane p)
    {
        List<int> bdlist;
        if (p.faceIndex == Block.UP_FACE_INDEX | p.faceIndex == Block.SURFACE_FACE_INDEX)
        {
            switch (i_level)
            {
                case 1:
                    bdlist = new List<int> {
                    WIND_GENERATOR_1_ID,
                    PSYCHOKINECTIC_GEN_ID,
                    STORAGE_1_ID,
                    SETTLEMENT_CENTER_ID,
                    FARM_1_ID,
                    LUMBERMILL_1_ID,
                    SMELTERY_1_ID,
                    COMPOSTER_ID,
                   // MINE_ID,
                    DOCK_ID
                };
                    break;
                case 2:
                    bdlist = new List<int> {
                    STORAGE_2_ID,
                    FARM_2_ID,
                    LUMBERMILL_2_ID,
                    SMELTERY_2_ID,
                    ORE_ENRICHER_2_ID,
                    BIOGENERATOR_2_ID,
                    MINERAL_POWERPLANT_2_ID,
                    ENERGY_CAPACITOR_1_ID,                    
                    HOSPITAL_ID,
                    WORKSHOP_ID
                    };
                    break;
                case 3:
                    {
                        bdlist = new List<int> {
                    FARM_3_ID,
                    LUMBERMILL_3_ID,
                    SMELTERY_3_ID,
                    PLASTICS_FACTORY_3_ID,
                    MINI_GRPH_REACTOR_3_ID,
                    ENERGY_CAPACITOR_2_ID,
                    GRPH_ENRICHER_3_ID
                };
                        if (Settlement.maxAchievedLevel >= 3) bdlist.Insert(0, SETTLEMENT_CENTER_ID);
                    }
                    break;
                case 4:
                    bdlist = new List<int> {
                    COVERED_FARM,
                    COVERED_LUMBERMILL,
                    SUPPLIES_FACTORY_4_ID,
                    SHUTTLE_HANGAR_4_ID,
                    RECRUITING_CENTER_4_ID,
                    EXPEDITION_CORPUS_4_ID,
                    QUANTUM_TRANSMITTER_4_ID,
                    DOCK_ADDON_1_ID,
                    //ARTIFACTS_REPOSITORY_ID,
                    OBSERVATORY_ID,
                    FUEL_FACILITY_ID
                    };
                    break;
                case 5:
                    {
                        bdlist = new List<int>();
                        if (!p.isTerminal)
                        {

                            bdlist.Add(FOUNDATION_BLOCK_5_ID);
                            bdlist.Add(STORAGE_BLOCK_ID);
                            bdlist.Add(FARM_BLOCK_ID);
                            bdlist.Add(LUMBERMILL_BLOCK_ID);
                            bdlist.Add(SMELTERY_BLOCK_ID);

                        };
                        bdlist.Add(HOSPITAL_2_ID);
                    bdlist.Add(GRPH_REACTOR_4_ID);
                    bdlist.Add(SCIENCE_LAB_ID);
                    if (Settlement.maxAchievedLevel >= 5)
                    {
                        bdlist.Insert(0, SETTLEMENT_CENTER_ID);
                    }
                        break;
                    }                            
                case 6:
                    {
                        bdlist = new List<int> {
                    //CONNECT_TOWER_6_ID,
                    //HOTEL_BLOCK_6_ID,
                    //HOUSING_MAST_6_ID,
                    DOCK_ADDON_2_ID
                    //SWITCH_TOWER_ID,
                    
                };
                        if (!p.isTerminal) bdlist.Add(REACTOR_BLOCK_5_ID);
                        if (Settlement.maxAchievedLevel >= 6)
                        {                            
                            if (Settlement.maxAchievedLevel == Settlement.MAX_HOUSING_LEVEL)
                                bdlist.Insert(2, HOUSE_BLOCK_ID);
                        }
                    }
                    break;
                default: bdlist = new List<int>(); break;
            }           
        }
        else
        {
            bool bottom = p.faceIndex == Block.DOWN_FACE_INDEX | p.faceIndex == Block.CEILING_FACE_INDEX;
            switch(i_level)
            {
                case 1:
                    bdlist = new List<int> {
                    STORAGE_1_ID,
                    SMELTERY_1_ID,                    
                };
                    break;
                case 2:
                    bdlist = new List<int>
                    {
                    STORAGE_2_ID,
                    SMELTERY_2_ID,
                    ENERGY_CAPACITOR_1_ID,
                    };
                    break;
                case 3:
                    {
                        bdlist = new List<int>
                {
                    SMELTERY_3_ID,
                    MINI_GRPH_REACTOR_3_ID,
                    ENERGY_CAPACITOR_2_ID
                };
                    }
                    break;
                case 4:
                    bdlist = new List<int>
                    {
                         COVERED_FARM,
                    COVERED_LUMBERMILL,
                    FUEL_FACILITY_ID
                    };
                    break;
                case 5:
                    if (!p.isTerminal)
                    {
                        bdlist = new List<int>
                    {
                    FOUNDATION_BLOCK_5_ID,
                    STORAGE_BLOCK_ID,
                    FARM_BLOCK_ID,
                    LUMBERMILL_BLOCK_ID,
                    SMELTERY_BLOCK_ID,
                    REACTOR_BLOCK_5_ID,
                    };
                    }
                    else bdlist = new List<int>();
                    break;
                case 6:
                    if (!p.isTerminal)
                    {
                        bdlist = new List<int>
                    {
                        HOUSE_BLOCK_ID
                    };
                    }
                    else bdlist = new List<int>();
                    if (bottom) bdlist.Add(HOUSING_MAST_6_ID);
                    break;
                default: bdlist = new List<int>(); break;
            }
            if ( bottom && i_level <= Settlement.maxAchievedLevel) bdlist.Add(SETTLEMENT_CENTER_ID);
        }
        if (i_level == 6 && (Knowledge.KnowledgePrepared() | GameMaster.realMaster.IsInTestMode)) Knowledge.GetCurrent().AddUnblockedBuildings(p.faceIndex, ref bdlist);
        return bdlist.ToArray();
    }

    public const int BUILDING_SERIALIZER_LENGTH = 5;

    public static float GetEnergyCapacity(int id)
    {
        switch (id)
        {
            case ENERGY_CAPACITOR_1_ID:
            case ENGINE_ID:
                return 1000f;
            case ENERGY_CAPACITOR_2_ID: return 4000f;
            case CAPACITOR_MAST_ID: return 10000f;

            case SETTLEMENT_CENTER_ID: return 100f;
            case HOUSE_BLOCK_ID: return 150f;
            case HOUSING_MAST_6_ID: return 1000f;

            case DOCK_ID: return 24f;
            case DOCK_2_ID: return 80f;
            case DOCK_3_ID: return 150f;

            case FARM_1_ID: return 10f;
            case FARM_2_ID: return 20f;
            case FARM_3_ID: return 40f;
            case COVERED_FARM: return 120f;
            case FARM_BLOCK_ID: return 300f;

            case LUMBERMILL_1_ID: return 10f;
            case LUMBERMILL_2_ID: return 40f;
            case LUMBERMILL_3_ID: return 80f;
            case COVERED_LUMBERMILL: return 80f;
            case LUMBERMILL_BLOCK_ID: return 160f;

            case SMELTERY_1_ID: return 40f;
            case SMELTERY_2_ID: return 60f;
            case SMELTERY_3_ID: return 100f;
            case SMELTERY_BLOCK_ID: return 160f;

            case SUPPLIES_FACTORY_4_ID: return 20f;
            case SUPPLIES_FACTORY_5_ID: return 80f;

            case FOUNDATION_BLOCK_5_ID:
            case WIND_GENERATOR_1_ID:
                return 10f;
            case SCIENCE_LAB_ID:
            case XSTATION_3_ID: return 12f;

            case EXPEDITION_CORPUS_4_ID:
            case RECRUITING_CENTER_4_ID:
            case GRPH_ENRICHER_3_ID:
            case MINE_ID:
            case MINERAL_POWERPLANT_2_ID:
            case WORKSHOP_ID:
            case BIOGENERATOR_2_ID: return 20f;

            case SHUTTLE_HANGAR_4_ID:
            case PLASTICS_FACTORY_3_ID:
            case ORE_ENRICHER_2_ID:
            case PSYCHOKINECTIC_GEN_ID:
                return 40f;

            case FUEL_FACILITY_ID:
            case HOSPITAL_ID: return 50f;
            case HOSPITAL_2_ID: return 150f;

            case DOCK_ADDON_1_ID:
            case DOCK_ADDON_2_ID:
            case MINI_GRPH_REACTOR_3_ID: return 100f;

            case ARTIFACTS_REPOSITORY_ID: return 160f;

            case CONNECT_TOWER_6_ID: return 192f;

            case OBSERVATORY_ID:
            case REACTOR_BLOCK_5_ID:
            case COMPOSTER_ID:
                return 200f;

            case HOTEL_BLOCK_6_ID: return 300f;

            case MONUMENT_ID: return 800f;
            default: return 0;
        }
    }
    public static float GetEnergySurplus(int id)
    {
        switch (id)
        {
            case SETTLEMENT_CENTER_ID: return -2f;
            case SETTLEMENT_STRUCTURE_ID: return -0.5f;
            case HOUSE_BLOCK_ID: return -400f;
            case HOUSING_MAST_6_ID: return -350f;
            case ENGINE_ID:
            case HOTEL_BLOCK_6_ID:  return -200f;

            case DOCK_ID: return -10f;
            case DOCK_2_ID: return -20f;
            case DOCK_3_ID: return -30f;

            case FARM_1_ID: return -1f;
            case FARM_2_ID: return -4f;
            case FARM_3_ID: return -20f;
            case COVERED_FARM: return -80f;
            case FARM_BLOCK_ID: return -200f;

            case LUMBERMILL_1_ID: return -2f;
            case LUMBERMILL_2_ID: return -4f;
            case LUMBERMILL_3_ID: return -20f;
            case COVERED_LUMBERMILL: return -80f;
            case LUMBERMILL_BLOCK_ID: return -200f;

            case SUPPLIES_FACTORY_4_ID: return -25f;
            case SUPPLIES_FACTORY_5_ID: return -40f;

            case MINE_ID: return -2f;

            case SMELTERY_1_ID: return -8f;
            case SMELTERY_2_ID:
            case COMPOSTER_ID:  return -15f;
            case SMELTERY_3_ID: return -40f;
            case SMELTERY_BLOCK_ID: return -80f;

            case HOSPITAL_ID: return -15f;
            case HOSPITAL_2_ID: return -45f;
            case SHUTTLE_HANGAR_4_ID:
            case MONUMENT_ID:
            case XSTATION_3_ID: return -10f;
            case WORKSHOP_ID: return -8f;
            case EXPEDITION_CORPUS_4_ID:
            case RECRUITING_CENTER_4_ID:
            case ORE_ENRICHER_2_ID: return -12f;
            case ARTIFACTS_REPOSITORY_ID: return -16f;

            case SCIENCE_LAB_ID:
            case PLASTICS_FACTORY_3_ID:
            case FUEL_FACILITY_ID: return -20f;

            case OBSERVATORY_ID: return -50f;
            case CONNECT_TOWER_6_ID: return -64f;
            case GRPH_ENRICHER_3_ID: return -70f;


            case MINERAL_POWERPLANT_2_ID: return Powerplant.MINERAL_F_PP_OUTPUT;
            case FOUNDATION_BLOCK_5_ID:
            case WIND_GENERATOR_1_ID:
                return 10f;
            case BIOGENERATOR_2_ID: return Powerplant.BIOGEN_OUTPUT;
            case MINI_GRPH_REACTOR_3_ID: return Powerplant.GRPH_REACTOR_OUTPUT * 0.1f;
            case GRPH_REACTOR_4_ID: return Powerplant.GRPH_REACTOR_OUTPUT;
            case REACTOR_BLOCK_5_ID: return Powerplant.REACTOR_BLOCK_5_OUTPUT;

            default: return 0;
        }
    }

    override public void Prepare() { PrepareBuilding(); }

    protected void ChangeUpgradedIndex(int x)
    {
        upgradedIndex = x;
    }

    /// <summary>
    /// do not use directly, use Prepare() instead
    /// </summary>
    /// <param name="i_id"></param>
    /// <returns></returns>
	protected void PrepareBuilding()
    {
        PrepareStructure();
        isActive = false;
        isEnergySupplied = false;
        connectedToPowerGrid = false;

        upgradedIndex = -1;
        canBePowerSwitched = false;
        energySurplus = GetEnergySurplus(ID);
        energyCapacity = GetEnergyCapacity(ID);
        level = 1;

        switch (ID)
        {
            case HEADQUARTERS_ID:
                {
                    upgradedIndex = 0;
                    break;
                }
            case STORAGE_0_ID:
                {
                    level = 0;
                    upgradedIndex = STORAGE_1_ID;
                }
                break;
            case STORAGE_1_ID:
                {
                    level = 1;
                    upgradedIndex = STORAGE_2_ID;
                }
                break;
            case STORAGE_2_ID:
                {
                    upgradedIndex = STORAGE_BLOCK_ID;
                    level = 2;
                }
                break;
            case STORAGE_BLOCK_ID:
                {
                    level = 5;
                }
                break;
            case TENT_ID:
                {
                    level = 0;
                }
                break;
            case HOUSE_BLOCK_ID:
                {
                    level = 5;
                }
                break;
            case DOCK_ID:
                {
                    level = 1;
                    canBePowerSwitched = true;
                }
                break;
            case DOCK_2_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case DOCK_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case ENERGY_CAPACITOR_1_ID:
                {
                    upgradedIndex = ENERGY_CAPACITOR_2_ID;
                    level = 1;
                }
                break;
            case ENERGY_CAPACITOR_2_ID:
                {
                    level = 3;
                }
                break;
            case FARM_1_ID:
                {
                    upgradedIndex = FARM_2_ID;
                    canBePowerSwitched = true;
                }
                break;
            case FARM_2_ID:
                {
                    upgradedIndex = FARM_3_ID;
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case FARM_3_ID:
                {
                    upgradedIndex = -1;
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case COVERED_FARM:
                {
                    upgradedIndex = FARM_BLOCK_ID;
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case FARM_BLOCK_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case LUMBERMILL_1_ID:
                {
                    upgradedIndex = LUMBERMILL_2_ID;
                    canBePowerSwitched = true;
                }
                break;
            case LUMBERMILL_2_ID:
                {
                    upgradedIndex = LUMBERMILL_3_ID;
                    canBePowerSwitched = true;
                }
                break;
            case LUMBERMILL_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case COVERED_LUMBERMILL:
                {
                    upgradedIndex = LUMBERMILL_BLOCK_ID;
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case LUMBERMILL_BLOCK_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case MINE_ID:
                {
                    upgradedIndex = 0;
                    canBePowerSwitched = true;
                    level = 1;
                }
                break;
            case SMELTERY_1_ID:
                {
                    upgradedIndex = SMELTERY_2_ID;
                    canBePowerSwitched = true;
                }
                break;
            case SMELTERY_2_ID:
                {
                    upgradedIndex = SMELTERY_3_ID;
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case SMELTERY_3_ID:
                {
                    upgradedIndex = SMELTERY_BLOCK_ID;
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case SMELTERY_BLOCK_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case BIOGENERATOR_2_ID:
                {
                    level = 2;
                }
                break;
            case HOSPITAL_ID:
                {
                    upgradedIndex = HOSPITAL_2_ID;
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case HOSPITAL_2_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                    break;
                }
            case MINERAL_POWERPLANT_2_ID:
                {
                    level = 2;
                }
                break;
            case ORE_ENRICHER_2_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case WORKSHOP_ID:
                {
                    canBePowerSwitched = true;
                    level = 2;
                }
                break;
            case MINI_GRPH_REACTOR_3_ID:
                {
                    level = 3;
                }
                break;
            case FUEL_FACILITY_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case GRPH_REACTOR_4_ID:
                {
                    level = 4;
                    upgradedIndex = REACTOR_BLOCK_5_ID;
                }
                break;
            case PLASTICS_FACTORY_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case SUPPLIES_FACTORY_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case SUPPLIES_FACTORY_5_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                }
                break;
            case GRPH_ENRICHER_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case XSTATION_3_ID:
                {
                    canBePowerSwitched = true;
                    level = 3;
                }
                break;
            case QUANTUM_ENERGY_TRANSMITTER_5_ID:
                {
                    canBePowerSwitched = true; // for debugging
                    level = 5;
                }
                break;
            case SWITCH_TOWER_ID:
                {
                    level = 6;
                }
                break;
            case SHUTTLE_HANGAR_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case RECRUITING_CENTER_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case EXPEDITION_CORPUS_4_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                }
                break;
            case QUANTUM_TRANSMITTER_4_ID:
                {
                    level = 4;
                }
                break;
            case REACTOR_BLOCK_5_ID:
                {
                    level = 5;
                }
                break;
            case FOUNDATION_BLOCK_5_ID:
                {
                    level = 5;
                }
                break;
            case CONNECT_TOWER_6_ID:
                {
                    canBePowerSwitched = true;
                    level = 6;
                }
                break;
            case ENGINE_ID:
            case HOTEL_BLOCK_6_ID:
                {
                    canBePowerSwitched = true;
                    level = 6;
                }
                break;
            case HOUSING_MAST_6_ID:
                {
                    level = 6;
                }
                break;
            case DOCK_ADDON_2_ID:
                {
                    level = 2;
                }
                break;
            case OBSERVATORY_ID:
                {
                    canBePowerSwitched = true;
                    level = 5;
                    break;
                }
            case ARTIFACTS_REPOSITORY_ID:
                {
                    canBePowerSwitched = true;
                    level = 4;
                    break;
                }
            case MONUMENT_ID:
                {
                    level = 5;
                    break;
                }
            case SETTLEMENT_CENTER_ID:
                {
                    level = 1;
                    upgradedIndex = 0;
                    break;
                }
            case SCIENCE_LAB_ID:
                {
                    canBePowerSwitched = true;
                    level = 6;
                    break;
                }
            case COMPOSTER_ID:
                {
                    level = 2;
                    canBePowerSwitched = true;
                    break;
                }
        }
    }
    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        SetBuildingData(b, pos);
        //copy to Gardens.cs
    }
    protected void SetBuildingData(Plane b, PixelPosByte pos)
    {
        SetStructureData(b, pos);
        isActive = true;
        if (energySurplus != 0 | energyCapacity > 0)
        {
            GameMaster.realMaster.colonyController.AddToPowerGrid(this);
            connectedToPowerGrid = true;
        }
    }


    virtual public void SetActivationStatus(bool x, bool sendRecalculationRequest)
    {
        bool activityStateChanged = false;
        if (isActive != x )
        {
            activityStateChanged = (isActive & isEnergySupplied) != (x & isEnergySupplied);
            isActive = x;
        }
        if (connectedToPowerGrid & sendRecalculationRequest)
        {
            GameMaster.realMaster.colonyController.powerGridRecalculationNeeded = true;
        }
       if (activityStateChanged)  SwitchActivityState();
    }
    public void SetEnergySupply(bool x, bool recalculateAfter)
    {
        bool activityStateChanged = false;
        if (isEnergySupplied != x)
        {
            activityStateChanged = (isActive & isEnergySupplied) != (isActive & x);
            isEnergySupplied = x;
        }
        if (connectedToPowerGrid & recalculateAfter) GameMaster.realMaster.colonyController.powerGridRecalculationNeeded = true;
        if (activityStateChanged) SwitchActivityState();
    }
    virtual protected void SwitchActivityState()
    {
        ChangeRenderersView(isActive & isEnergySupplied);
        //copy to QuantumTrasmitter.cs
        // copy to Engine.cs
    }
    virtual protected void ChangeRenderersView(bool setOnline)
    {
        if (transform.childCount == 0) return;
        Renderer[] myRenderers = transform.GetChild(0).GetComponentsInChildren<Renderer>();
        if (myRenderers == null | myRenderers.Length == 0) return;
        if (setOnline) PoolMaster.SwitchMaterialsToOnline(myRenderers);
        else PoolMaster.SwitchMaterialsToOffline(myRenderers);
        //copy to SettlementStructure.SetActivationStatus
        //copy to StorageBlock and other IPlanables
    }
    override public void SetVisibility(bool x)
    {
        if (x == isVisible) return;
        else
        {
            isVisible = x;
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(isVisible);
            }
        }
    }

    public override UIObserver ShowOnGUI()
    {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
        buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
        return buildingObserver;
    }


    public virtual bool IsLevelUpPossible(ref string refusalReason)
    {
        if (level < GameMaster.realMaster.colonyController.hq.level) return true;
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.Unavailable);
            return false;
        }
    }
    public virtual void LevelUp(bool returnToUI)
    {
        if (upgradedIndex == -1) return;
        if (!GameMaster.realMaster.weNeedNoResources)
        {
            ResourceContainer[] cost = GetUpgradeCost();
            if (!GameMaster.realMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
            {
                GameLogUI.NotEnoughResourcesAnnounce();
                return;
            }
        }
        Building upgraded = GetStructureByID(upgradedIndex) as Building;
        PixelPosByte setPos = new PixelPosByte(surfaceRect.x, surfaceRect.z);
        if (upgraded.surfaceRect.size == 16) setPos = new PixelPosByte(0, 0);
        if (upgraded.rotate90only & (modelRotation % 2 != 0))
        {
            upgraded.modelRotation = (byte)(modelRotation - 1);
        }
        else upgraded.modelRotation = modelRotation;
        upgraded.SetBasement(basement, setPos);
        GameMaster.realMaster.eventTracker?.BuildingUpgraded(this);
        if (returnToUI) upgraded.ShowOnGUI();
    }
    public virtual ResourceContainer[] GetUpgradeCost()
    {
        if (upgradedIndex == -1) return null;
        ResourceContainer[] cost = ResourcesCost.GetCost(upgradedIndex);
        float discount = GameMaster.realMaster.upgradeDiscount;
        for (int i = 0; i < cost.Length; i++)
        {
            cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * (1 - discount));
        }
        return cost;
    }

    protected void PrepareBuildingForDestruction(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (connectedToPowerGrid) GameMaster.realMaster.colonyController.DisconnectFromPowerGrid(this);
        PrepareStructureForDestruction(clearFromSurface, returnResources, leaveRuins);
    }
    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed) return;
        else destroyed = true;        
        PrepareBuildingForDestruction(clearFromSurface, returnResources, leaveRuins);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SaveStructureData();
        data.AddRange(SaveBuildingData());
        return data;
        // dependence: Settlement PsychokineticGenerator Hotel
    }

    protected List<byte> SaveBuildingData()
    {
        // copied to Settlement.Save()        
        var data = new List<byte>() { isActive ? (byte)1 : (byte)0 };
        data.AddRange(System.BitConverter.GetBytes(energySurplus));
        return data;
    }

    override public void Load(System.IO.FileStream fs, Plane sblock)
    {
        LoadStructureData(fs, sblock);
        LoadBuildingData(fs);
        // dependence in Settlement PsychokineticGenerator  QuantumEnergyTransmitter Hotel
    }
    protected void LoadBuildingData(System.IO.FileStream fs)
    {
        var data = new byte[BUILDING_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        LoadBuildingData(data, 0);
    }
    protected void LoadBuildingData(byte[] data, int startIndex)
    {
        energySurplus = System.BitConverter.ToSingle(data, startIndex + 1);
        SetActivationStatus(data[startIndex] == 1, true);
        //copy to HeadQuarters
    }
    #endregion
}