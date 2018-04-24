using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public byte level = 0;
	[SerializeField]
	public int resourcesContainIndex = 0;
	public float energySurplus = 0, energyCapacity = 0;
	protected bool connectedToPowerGrid = false; // подключение, контролирующееся игроком
	public bool borderOnlyConstruction{get;protected set;}
	public Building nextStage; 

	void Awake() {
		PrepareBuilding();
	}
	protected void PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
		borderOnlyConstruction = false;
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
	}
	public void SetEnergySupply(bool x) {
		energySupplied = x;
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
		if ( !showOnGUI ) return;
		if (nextStage != null && level < GameMaster.colonyController.hq.level) {
			Rect rr = new Rect(UI.current.rightPanelBox.x, gui_ypos, UI.current.rightPanelBox.width, GameMaster.guiPiece);
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
					upgraded.SetBasement(basement, PixelPosByte.zero);
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
		}
	}
}
