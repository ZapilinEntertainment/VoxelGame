using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingSerializer {
	public bool isActive;
	public float energySurplus;
}

public class Building : Structure {
	public int upgradedIndex {get;private set;} // fixed by id
	public bool canBePowerSwitched = true; // fixed by asset
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public float energySurplus = 0; // can be changed later (ex.: generator)
	public float energyCapacity = 0; // fixed by asset
	public  bool connectedToPowerGrid {get; protected set;}// установлено ли подключение к электросети
	public int requiredBasementMaterialId = -1; // fixed by asset
	public byte level{get;protected set;} // fixed by id (except for mine)

	public static UIBuildingObserver buildingObserver;

	override public void Prepare() {PrepareBuilding();}

	protected void	PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
		borderOnlyConstruction = false;
		connectedToPowerGrid = false;
        upgradedIndex = -1;
		switch (id) {
		case LANDED_ZEPPELIN_ID: upgradedIndex = HQ_2_ID; level = 1; break;
		case STORAGE_0_ID: level = 0; break;
		case FARM_1_ID: upgradedIndex = FARM_2_ID; level = 1;break;
		case HQ_2_ID : upgradedIndex = HQ_3_ID; level = 2;break;
		case LUMBERMILL_1_ID: upgradedIndex = LUMBERMILL_2_ID;  level = 1;break;
		case SMELTERY_1_ID : upgradedIndex = SMELTERY_2_ID;  level = 1;break;
		case FOOD_FACTORY_4_ID: upgradedIndex = FOOD_FACTORY_5_ID;  level = 4;break;
		case STORAGE_2_ID: upgradedIndex = STORAGE_3_ID;  level = 2;break; 
		case HOUSE_2_ID: upgradedIndex = HOUSE_3_ID;  level = 2;break;
		case ENERGY_CAPACITOR_2_ID: upgradedIndex = ENERGY_CAPACITOR_3_ID;  level = 2;break;
		case FARM_2_ID : upgradedIndex = FARM_3_ID;  level = 2;break;
		case FARM_3_ID : upgradedIndex = FARM_4_ID; level = 3;break;
		case FARM_4_ID: upgradedIndex = FARM_5_ID; level = 4;break;
		case LUMBERMILL_2_ID : upgradedIndex = LUMBERMILL_3_ID; level = 2;break;
		case LUMBERMILL_3_ID : upgradedIndex = LUMBERMILL_4_ID;  level = 3;break;
		case LUMBERMILL_4_ID: upgradedIndex = LUMBERMILL_5_ID;  level = 4;break;
		case SMELTERY_2_ID: upgradedIndex = SMELTERY_3_ID;  level = 2; break;
		case SMELTERY_3_ID: upgradedIndex = SMELTERY_5_ID;  level = 3;break;
		case HQ_3_ID: upgradedIndex = HQ_4_ID;  level = 3;break;
			
		case MINE_ID: level = 1; upgradedIndex = -2; break;
		case WIND_GENERATOR_1_ID:
		case STORAGE_1_ID:
		case ENERGY_CAPACITOR_1_ID:	
		case HOUSE_1_ID:
			level = 1;
			break;
		case DOCK_ID:
			level = 1; borderOnlyConstruction = true;
			break;
		case ORE_ENRICHER_2_ID:
		case ROLLING_SHOP_ID:
		case BIOGENERATOR_2_ID:
		case HOSPITAL_2_ID:
		case MINERAL_POWERPLANT_2_ID:
			level = 2;
			break;
		case FUEL_FACILITY_3_ID:
		case GRPH_ENRICHER_ID:
		case XSTATION_ID:
		case STORAGE_3_ID:
		case HOUSE_3_ID:
		case ENERGY_CAPACITOR_3_ID:
		case MINI_GRPH_REACTOR_ID:
		case PLASTICS_FACTORY_3_ID:
			level = 3;
			break;
		case GRPH_REACTOR_4_ID:
		case HQ_4_ID:
                level = 4; upgradedIndex = -2;
                break;
		case CHEMICAL_FACTORY_ID:
		case RECRUITING_CENTER_ID:
		case EXPEDITION_CORPUS_ID:
		case QUANTUM_TRANSMITTER_ID:
			level = 4;
			break;
		case SHUTTLE_HANGAR_ID:
			level = 4; borderOnlyConstruction = true;
			break;
		case STORAGE_5_ID: 
		case HOUSE_5_ID:
		case FARM_5_ID:
		case LUMBERMILL_5_ID:
		case FOOD_FACTORY_5_ID:
		case SMELTERY_5_ID:		
		case QUANTUM_ENERGY_TRANSMITTER_ID:
			level = 5;
			break;
            case SWITCH_TOWER_ID: level = 6; break;
        }
	}
		
	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
	}

	protected void SetBuildingData(SurfaceBlock b, PixelPosByte pos) {
		SetStructureData(b,pos);
		isActive = true;
		if (energySurplus != 0 || energyCapacity >  0) {
			GameMaster.colonyController.AddToPowerGrid(this);
			connectedToPowerGrid = true;
		}
		Rename();
	}
	virtual public void SetActivationStatus(bool x) {
		isActive = x;
		if (connectedToPowerGrid) {
			GameMaster.colonyController.RecalculatePowerGrid();
		}
		ChangeRenderersView(x);
	}
	public virtual void SetEnergySupply(bool x) {
		energySupplied = x;
		ChangeRenderersView(x);
	}

	protected void ChangeRenderersView(bool setOnline) {
		if (myRenderers == null | myRenderers.Count == 0)  return;
		if (setOnline == false) {
				for (int i = 0; i < myRenderers.Count; i++) {
					if (myRenderers[i].sharedMaterials.Length > 1) {
						bool replacing = false;
						Material[] newMaterials = new Material[myRenderers[i].sharedMaterials.Length];
						for (int j = 0; j < myRenderers[i].sharedMaterials.Length; j++) {
							Material m= myRenderers[i].sharedMaterials[j];
							if (m == PoolMaster.glass_material) {m = PoolMaster.glass_offline_material; replacing = true;}
							else {
								if (m == PoolMaster.basic_material) {m = PoolMaster.basic_offline_material; replacing = true;}
								else {
										if (m == PoolMaster.energy_material ) {m = PoolMaster.energy_offline_material; replacing = true;}
										}
							}
						newMaterials[j] = m;
						}
					if (replacing) myRenderers[i].sharedMaterials = newMaterials;
					}
					else {
						Material m= myRenderers[i].sharedMaterial;
						bool replacing = false;
						if (m == PoolMaster.glass_material) {m = PoolMaster.glass_offline_material; replacing = true;}
						else {
							if (m == PoolMaster.basic_material) {m = PoolMaster.basic_offline_material; replacing = true;}
							else {
								if (m == PoolMaster.energy_material ) {m = PoolMaster.energy_offline_material; replacing = true;}
							}
						}
						if (replacing) myRenderers[i].sharedMaterial = m;
					}
				}
		}
		else { // Включение
				for (int i = 0; i < myRenderers.Count; i++) {
					if (myRenderers[i].sharedMaterials.Length > 1) {
					bool replacing = false;
					Material[] newMaterials = new Material[myRenderers[i].sharedMaterials.Length];
						for (int j = 0; j < myRenderers[i].sharedMaterials.Length; j++) {
							Material m = myRenderers[i].sharedMaterials[j];
							if (m == PoolMaster.glass_offline_material) { m = PoolMaster.glass_material; replacing = true;}
							else {
								if (m == PoolMaster.basic_offline_material) {m = PoolMaster.basic_material;replacing = true;}
								else {
									if (m == PoolMaster.energy_offline_material) { m = PoolMaster.energy_material;replacing = true;}
								}
							}
						newMaterials[j] = m;
					}
					if (replacing) myRenderers[i].sharedMaterials = newMaterials;
					}
					else {
						Material m = myRenderers[i].sharedMaterial;
						bool replacing = false;
						if (m == PoolMaster.glass_offline_material) { m = PoolMaster.glass_material; replacing = true;}
						else {
							if (m == PoolMaster.basic_offline_material) {m = PoolMaster.basic_material;replacing = true;}
							else {
								if (m == PoolMaster.energy_offline_material) { m = PoolMaster.energy_material;replacing = true;}
							}
						}
						if (replacing) myRenderers[i].sharedMaterial = m;
					}
				}
		}
	}

	override public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			if (myRenderers != null & myRenderers.Count > 0) {
				foreach (Renderer r in myRenderers) {
					r.enabled = x;
					if (r is SpriteRenderer) {
						if (r.GetComponent<MastSpriter>() != null) r.GetComponent<MastSpriter>().SetVisibility(x);
					}
				}
				if (isBasement) {
					BlockRendererController brc = gameObject.GetComponent<BlockRendererController>();
					if (brc != null) brc.SetVisibility(x);
				}
			}
		}
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss  = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetBuildingSerializer());
			ss.specificData = stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		BuildingSerializer bs = new BuildingSerializer();
		GameMaster.DeserializeByteArray<BuildingSerializer>(ss.specificData, ref bs);
	}
	protected void LoadBuildingData(BuildingSerializer bs) {
		energySurplus = bs.energySurplus;
		SetActivationStatus(bs.isActive);
	}

	protected BuildingSerializer GetBuildingSerializer() {
		BuildingSerializer bs = new BuildingSerializer();
		bs.isActive = isActive;
		bs.energySurplus = energySurplus;
		return bs;
	}
	#endregion

	override public void Rename() {
		name = Localization.GetStructureName(id) + " (" + Localization.GetWord(LocalizedWord.Level) + ' '+level.ToString() +')';
	}

	public override UIObserver ShowOnGUI() {
        if (buildingObserver == null) buildingObserver = UIBuildingObserver.InitializeBuildingObserverScript();
        else buildingObserver.gameObject.SetActive(true);
		buildingObserver.SetObservingBuilding(this);
        showOnGUI = true;
		return buildingObserver;
	}

	
    public virtual bool IsLevelUpPossible(ref string refusalReason) {
        if (level < GameMaster.colonyController.hq.level) return true;
        else
        {
            refusalReason = Localization.GetRefusalReason(RefusalReason.Unavailable);
            return false;
        }
    }
    public virtual void LevelUp( bool returnToUI) {
        if (upgradedIndex == -1) return;
        if ( !GameMaster.realMaster.weNeedNoResources )
        {
            ResourceContainer[] cost = ResourcesCost.GetCost(id);
            if (cost != null && cost.Length != 0) 
            {
                for (int i = 0; i < cost.Length; i++)
                {
                    cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * (1 - GameMaster.upgradeDiscount));
                }
                if (!GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible(cost))
                {
                    UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NotEnoughResources));
                    return;
                }
            }
        }
        Building upgraded = Structure.GetNewStructure(upgradedIndex) as Building;
        PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
        byte bzero = (byte)0;
        if (upgraded.innerPosition.x_size == 16) setPos = new PixelPosByte(bzero, innerPosition.z);
        if (upgraded.innerPosition.z_size == 16) setPos = new PixelPosByte(setPos.x, bzero);
        Quaternion originalRotation = transform.rotation;
        upgraded.SetBasement(basement, setPos);
        if (!upgraded.isBasement & upgraded.randomRotation & (upgraded.rotate90only == rotate90only))
        {
            upgraded.transform.localRotation = originalRotation;
        }
        if (returnToUI) upgraded.ShowOnGUI();
    }
    public virtual ResourceContainer[] GetUpgradeCost() {
        if (upgradedIndex == -1) return null; 
        ResourceContainer[] cost = ResourcesCost.GetCost(upgradedIndex);
        float discount = GameMaster.upgradeDiscount;
        for (int i = 0; i < cost.Length; i++) {
            cost[i] = new ResourceContainer(cost[i].type, cost[i].volume * discount);
        }
        return cost;
    }

    protected bool PrepareBuildingForDestruction()
    {
        if (connectedToPowerGrid) GameMaster.colonyController.DisconnectFromPowerGrid(this);
        return PrepareStructureForDestruction();        
    }
    override public void Annihilate(bool forced)
    {
        if (forced) { UnsetBasement(); }
        PrepareBuildingForDestruction();
        Destroy(gameObject);
    }
}
