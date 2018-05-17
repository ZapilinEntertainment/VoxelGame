using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool canBePowerSwitched = true;
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public byte level = 0;
	[SerializeField]
	public int resourcesContainIndex = 0;
	public float energySurplus = 0, energyCapacity = 0;
	public  bool connectedToPowerGrid {get; protected set;}// подключение, контролирующееся игроком
	public bool borderOnlyConstruction{get;protected set;}
	public Building nextStage; 
	public int requiredBasementMaterialId = -1;
	[SerializeField]
	public Transform renderersTransform;

	void Awake() {
		PrepareBuilding();
	}
	protected void PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
		borderOnlyConstruction = false;
		if (renderersTransform == null) renderersTransform = transform.GetChild(0);
		connectedToPowerGrid = false;
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
		if (renderersTransform == null || renderersTransform.childCount == 0 ) return;
		if (setOnline == false) {
			for (int i = 0; i < renderersTransform.childCount; i++) {
				MeshRenderer mr = renderersTransform.GetChild(i).GetComponent<MeshRenderer>();
				if (mr != null) {
					int j = 0;
					Material[] allMaterials = mr.sharedMaterials;
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
					mr.sharedMaterials = allMaterials;
				}
			}
		}
		else {
			for (int i = 0; i < renderersTransform.childCount; i++) {
				MeshRenderer mr = renderersTransform.GetChild(i).GetComponent<MeshRenderer>();
				if (mr != null) {
					int j = 0;
					Material[] allMaterials = mr.sharedMaterials;
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
					mr.sharedMaterials = allMaterials;
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
			ResourceContainer[] rleft = new ResourceContainer[ResourcesCost.info[resourcesContainIndex].Length];
			for (int i = 0 ; i < rleft.Length; i++) {
				rleft[i] = new ResourceContainer(ResourcesCost.info[resourcesContainIndex][i].type, ResourcesCost.info[resourcesContainIndex][i].volume * (1 - GameMaster.demolitionLossesPercent));
			}
		}
		Destroy(gameObject);
	}

	void OnDestroy() {
		PrepareBuildingForDestruction();
	}

	void OnGUI() {
		//sync with hospital.cs, rollingShop.cs
		if ( !showOnGUI ) return;
		Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
		if (nextStage != null && level < GameMaster.colonyController.hq.level) {
			rr.y = GUI_UpgradeButton(rr);
		}
	}

	protected float GUI_UpgradeButton( Rect rr) {
			GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
			if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
				ResourceContainer[] requiredResources = new ResourceContainer[ResourcesCost.info[nextStage.resourcesContainIndex].Length];
				if (requiredResources.Length > 0) {
					for (int i = 0; i < requiredResources.Length; i++) {
						requiredResources[i] = new ResourceContainer(ResourcesCost.info[nextStage.resourcesContainIndex][i].type, ResourcesCost.info[nextStage.resourcesContainIndex][i].volume * (1 - GameMaster.upgradeDiscount));
					}
				}
				if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
				{
					Building upgraded = Instantiate(nextStage);
					PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
					byte bzero = (byte)0;
					if (upgraded.xsize_to_set == 16) setPos = new PixelPosByte(bzero, innerPosition.z);
					if (upgraded.zsize_to_set == 16) setPos = new PixelPosByte(setPos.x, bzero);
					upgraded.SetBasement(basement, setPos);
					upgraded.transform.localRotation = transform.localRotation;
				}
				else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
			}
			if ( ResourcesCost.info[ nextStage.resourcesContainIndex ].Length > 0) {
				rr.y += rr.height;
				for (int i = 0; i < ResourcesCost.info[ nextStage.resourcesContainIndex ].Length; i++) {
					GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.icon, ScaleMode.StretchToFill);
					GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), ResourcesCost.info[ nextStage.resourcesContainIndex ][i].type.name);
					GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (ResourcesCost.info[ nextStage.resourcesContainIndex ][i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
					rr.y += rr.height;
				}
			}
		return rr.y;
		}
}
