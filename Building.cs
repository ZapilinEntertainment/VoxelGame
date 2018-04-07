using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public byte level = 0;
	public List<ResourceContainer> resourcesContain {get;protected set;}
	public string resourcesContainSet = "";
	public float energySurplus = 0, energyCapacity = 0;
	protected bool connectedToPowerGrid = false; // подключение, контролирующееся игроком

	void Awake() {
		PrepareBuilding();
	}
	protected void PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
		if (resourcesContainSet != null) {
			resourcesContain = ResourceType.DecodeResourcesString(resourcesContainSet);
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
		resourcesContainSet = null;
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
		if (resourcesContain != null && resourcesContain.Count != 0 && GameMaster.demolitionLossesPercent != 1) {
			foreach (ResourceContainer rc in resourcesContain) {
				rc.Get(GameMaster.demolitionLossesPercent * rc.volume);
				GameMaster.colonyController.storage.AddResources(rc);
			}
		}
		Destroy(gameObject);
	}
	void OnDestroy() {
		PrepareBuildingForDestruction();
	}
}
