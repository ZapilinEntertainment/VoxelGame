using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool canBeUpgraded{get;protected set;}
	public bool canBePowerSwitched = true;
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	[SerializeField]
	public int resourcesContainIndex = 0;
	public float energySurplus = 0, energyCapacity = 0;
	public  bool connectedToPowerGrid {get; protected set;}// подключение, контролирующееся игроком
	public int requiredBasementMaterialId = -1;
	[SerializeField]
	protected Renderer[] myRenderers;
	protected static ResourceContainer[] requiredResources;

	public void Awake() {
		isActive = false;
		energySupplied = false;
		borderOnlyConstruction = false;
		connectedToPowerGrid = false;
		hp = maxHp;
		isBasement = false; isArtificial = true; borderOnlyConstruction = false;
		hp = maxHp;
		isBasement = false; isArtificial = true; borderOnlyConstruction = false;
		switch ( id ) {
		case LANDED_ZEPPELIN_ID: 
		case MINE_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure; canBeUpgraded = true;
			break;	
		case WIND_GENERATOR_ID:
		case ORE_ENRICHER_ID:
		case ROLLING_SHOP_ID:
		case FUEL_FACILITY_ID:
		case GRPH_REACTOR_ID:
		case GRPH_ENRICHER_ID:
		case XSTATION_ID:
		case QUANTUM_ENERGY_TRANSMITTER_ID:
		case CHEMICAL_FACTORY_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			break;		
		case DOCK_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			borderOnlyConstruction = true;
			break;		
		case STORAGE_ID:
			switch ( level ) {
			case 0:
			case 5: innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
			case 1: innerPosition = new SurfaceRect(0,0,4,4); type = StructureType.Structure; break;
			case 2: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure;  canBeUpgraded =true; break;
			case 3: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure;  break;
			}
			break;
		case HOUSE_ID:
			switch ( level ) {
			case 0: innerPosition = SurfaceRect.one; type = StructureType.Structure; break;
			case 1: innerPosition = new SurfaceRect( 0, 0, 4,4); type = StructureType.Structure;break;
			case 2: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure; canBeUpgraded = true; break;
			case 3: innerPosition = new SurfaceRect(0,0,6,6); type = StructureType.Structure; break;
			case 5: innerPosition = SurfaceRect.full; type = StructureType.MainStructure; isBasement = true; break;
			}
			break;
		case ENERGY_CAPACITOR_ID:
			switch (level) {
			case 1: innerPosition = new SurfaceRect (0,0, 2, 4); type = StructureType.Structure; break;
			case 2: innerPosition = new SurfaceRect (0,0,4,8);type = StructureType.Structure; canBeUpgraded = true; break;
			case 3: innerPosition = new SurfaceRect (0,0,4,8);type = StructureType.Structure; break;
				break;
			}
			break;
		case FARM_ID:
		case LUMBERMILL_ID:
		case PLASTICS_FACTORY_ID:
		case FOOD_FACTORY_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			if ( level > 4) isBasement = true;
			else canBeUpgraded = true;
			break;
		case SMELTERY_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			if ( level > 4) {isBasement = true; borderOnlyConstruction = true;}
			else canBeUpgraded = true;
			break;
		case HQ_ID:
			innerPosition = SurfaceRect.full; type = StructureType.MainStructure;
			canBeUpgraded = true;
			if ( level > 3) isBasement = true;
			break;
		case BIOGENERATOR_ID:
			innerPosition = new SurfaceRect(0,0,4,10); type = StructureType.Structure;
			break;
		case HOSPITAL_ID:
		case MINI_GRPH_REACTOR_ID:
			innerPosition = new SurfaceRect(0,0,8,8); type = StructureType.Structure;
			break;
		case MINERAL_POWERPLANT_ID:
			innerPosition = new SurfaceRect(0,0,10,10); type = StructureType.Structure;
			break;
		}
		visible = true;
		visible = true;
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
	public void SetEnergySupply(bool x) {
		energySupplied = x;
		ChangeRenderersView(x);
	}

	protected void ChangeRenderersView(bool setOnline) {
		if (setOnline == false) {
			if (myRenderers != null) {
				for (int i = 0; i < myRenderers.Length; i++) {
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
				for (int i = 0; i < myRenderers.Length; i++) {
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
		
	protected void PrepareBuildingForDestruction() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			basement.artificialStructures --;
		}
		if (connectedToPowerGrid) GameMaster.colonyController.DisconnectFromPowerGrid(this);
	}

	public void Demolish() {
		if (resourcesContainIndex != 0 && GameMaster.demolitionLossesPercent != 1) {
			ResourceContainer[] rleft = ResourcesCost.GetCost(id,level);
			for (int i = 0 ; i < rleft.Length; i++) {
				rleft[i] = new ResourceContainer(rleft[i].type, rleft[i].volume * (1 - GameMaster.demolitionLossesPercent));
			}
		}
		Destroy(gameObject);
	}

	void OnDestroy() {
		PrepareBuildingForDestruction();
	}

	override public void SetGUIVisible (bool x) {
		if (x != showOnGUI) {
			showOnGUI = x;
			if ( showOnGUI) {
				requiredResources = ResourcesCost.GetCost(id, (byte)(level + 1));
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
		if (canBeUpgraded && level < GameMaster.colonyController.hq.level) {
			rr.y = GUI_UpgradeButton(rr);
		}
	}

	virtual protected float GUI_UpgradeButton( Rect rr) {
			GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
			if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
				if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
				{
				Building upgraded = Structure.LoadStructure(id, (byte)(level + 1)) as Building;
					upgraded.Awake();
					PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
					byte bzero = (byte)0;
				if (upgraded.innerPosition.x_size == 16) setPos = new PixelPosByte(bzero, innerPosition.z);
				if (upgraded.innerPosition.z_size == 16) setPos = new PixelPosByte(setPos.x, bzero);
					Quaternion originalRotation = transform.rotation;
					upgraded.SetBasement(basement, setPos);
				upgraded.transform.localRotation = originalRotation;
				}
				else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
			}
		if ( requiredResources.Length > 0) {
			rr.y += rr.height;
			for (int i = 0; i < requiredResources.Length; i++) {
				GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
				GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
				GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (requiredResources[i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
				rr.y += rr.height;
			}
		}
		return rr.y;
		}
}
