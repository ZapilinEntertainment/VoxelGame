using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingSerializer {
	public StructureSerializer structureSerializer;
	public bool isActive;
}

public class Building : Structure {
	public int upgradedIndex = -1; // fixed by asset
	public bool canBePowerSwitched = true; // fixed by asset
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public float energySurplus = 0, energyCapacity = 0; // fixed by asset
	public  bool connectedToPowerGrid {get; protected set;}// установлено ли подключение к электросети
	public int requiredBasementMaterialId = -1; // fixed by asset
	public byte level{get;protected set;} // fixed by id (except for mine)
	[SerializeField]
	protected List<Renderer> myRenderers; // fixed by asset
	protected static ResourceContainer[] requiredResources;

	override public void Prepare() {PrepareBuilding();}

	protected void	PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
		borderOnlyConstruction = false;
		connectedToPowerGrid = false;
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
			
		case MINE_ID:
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
		case SWITCH_TOWER_ID:
		case QUANTUM_ENERGY_TRANSMITTER_ID:
			level = 5;
			break;			

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
		if (setOnline == false) {
			if (myRenderers != null) {
				for (int i = 0; i < myRenderers.Count; i++) {
						Material m= myRenderers[i].sharedMaterial;
						bool replacing = false;
						if (m == PoolMaster.glass_material) {m = PoolMaster.glass_offline_material; replacing = true;}
						else {
							if (m == PoolMaster.colored_material) {m = PoolMaster.colored_offline_material; replacing = true;}
							else {
									if (m == PoolMaster.energy_material ) {m = PoolMaster.energy_offline_material; replacing = true;}
									}
						}
					if (replacing) myRenderers[i].sharedMaterial = m;
				}
			}
			if (myRenderer != null) {
				Material[] allMaterials = myRenderer.sharedMaterials;
				int j =0;
				while (j < allMaterials.Length) {
					if (allMaterials[j] == PoolMaster.glass_material) allMaterials[j] = PoolMaster.glass_offline_material;
					else {
						if (allMaterials[j] == PoolMaster.colored_material) allMaterials[j] = PoolMaster.colored_offline_material;
						else {
							if (allMaterials[j].name == PoolMaster.energy_material.name ) {
								allMaterials[j] = PoolMaster.energy_offline_material;
							}
						}
					}
					j++;
				}
				myRenderer.sharedMaterials = allMaterials;
			}
		}
		else {
			if (myRenderers != null) {
				for (int i = 0; i < myRenderers.Count; i++) {
						Material m = myRenderers[i].sharedMaterial;
						bool replacing = false;
						if (m == PoolMaster.glass_offline_material) { m = PoolMaster.glass_material; replacing = true;}
						else {
							if (m == PoolMaster.colored_offline_material) {m = PoolMaster.colored_material;replacing = true;}
							else {
								if (m == PoolMaster.energy_offline_material) { m = PoolMaster.energy_material;replacing = true;}
									}
						}
						if (replacing) myRenderers[i].sharedMaterial = m;
				}
			}
			if (myRenderer != null) {
				int j = 0;
				Material[] allMaterials = myRenderer.sharedMaterials;
				while (j < allMaterials.Length) {
					if (allMaterials[j] == PoolMaster.glass_offline_material) allMaterials[j] = PoolMaster.glass_material;
					else {
						if (allMaterials[j] == PoolMaster.colored_offline_material) allMaterials[j] = PoolMaster.colored_material;
						else {
							if (allMaterials[j] == PoolMaster.energy_offline_material) allMaterials[j] = PoolMaster.energy_material;
						}
					}
					j++;
				}
				myRenderer.sharedMaterials = allMaterials;
			}
		}
	}

	override public void SetVisibility( bool x) {
		if (x == visible) return;
		else {
			visible = x;
			if (myRenderers != null) {
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
			if (myRenderer != null) {
				myRenderer.enabled = x;
				if (myRenderer is SpriteRenderer) {
					if (myRenderer.GetComponent<MastSpriter>() != null) myRenderer.GetComponent<MastSpriter>().SetVisibility(x);
				}
			}
		}
	}

	//---------------------                   SAVING       SYSTEM-------------------------------
	override public byte[] Save() {
		BuildingSerializer bs = GetBuildingSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, bs);
			return stream.ToArray();
		}
	}

	protected BuildingSerializer GetBuildingSerializer() {
		BuildingSerializer bs = new BuildingSerializer();
		bs.structureSerializer = GetStructureSerializer();
		bs.isActive = isActive;
	}

	protected string SaveBuildingData() {
		string s = "";
		if (isActive) s += "1"; else s+="0";
		return s;
	}


	//---------------------------------------------------------------------------------------------------	

	protected void PrepareBuildingForDestruction() {
		if (basement != null) {
			basement.RemoveStructure(this);
			basement.artificialStructures --;
		}
		if (connectedToPowerGrid) GameMaster.colonyController.DisconnectFromPowerGrid(this);
	}

	void OnDestroy() {
		PrepareBuildingForDestruction();
	}

	override public void SetGUIVisible (bool x) {
		if (x != showOnGUI) {
			showOnGUI = x;
			if ( showOnGUI) {
				requiredResources = ResourcesCost.GetCost(id);
				if (requiredResources.Length > 0) {
					for (int i = 0; i < requiredResources.Length; i++) {
						requiredResources[i] = new ResourceContainer(requiredResources[i].type, requiredResources[i].volume * GameMaster.upgradeDiscount);
					}
				}
			}
		}
	}

	void OnGUI() {
		//sync with hospital.cs, rollingShop.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (upgradedIndex != -1 && level < GameMaster.colonyController.hq.level) {
			rr.y = GUI_UpgradeButton(rr);
		}
	}

	virtual protected float GUI_UpgradeButton( Rect rr) {
			GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
			if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
				if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
				{
					Building upgraded = Structure.GetNewStructure(upgradedIndex) as Building;
					PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
					byte bzero = (byte)0;
					if (upgraded.innerPosition.x_size == 16) setPos = new PixelPosByte(bzero, innerPosition.z);
					if (upgraded.innerPosition.z_size == 16) setPos = new PixelPosByte(setPos.x, bzero);
					Quaternion originalRotation = transform.rotation;
					upgraded.SetBasement(basement, setPos);
					if ( !upgraded.isBasement ) upgraded.transform.localRotation = originalRotation;
				}
				else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
			}
		if ( requiredResources.Length > 0) {
			rr.y += rr.height;
			Storage storage = GameMaster.colonyController.storage;
			for (int i = 0; i < requiredResources.Length; i++) {
				if (requiredResources[i].volume> storage.standartResources[requiredResources[i].type.ID]) GUI.color = Color.red;
				GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
				GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
				GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), requiredResources[i].volume.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
				GUI.color = Color.white;
				rr.y += rr.height;
			}
		}
		return rr.y;
		}
}
