using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public byte level = 0;
	public List<ResourceContainer> resourcesContain;
	public float energySurplus = 0, energyCapacity = 0;
	protected bool connectedToPowerGrid = false; // подключение, контролирующееся игроком

	void Awake() {
		PrepareBuilding();
	}
	protected void PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
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
	}
	public void SetEnergySupply(bool x) {
		energySupplied = x;
	}

	public void DisconnectFromPowerGrid() {
		
	}

	protected void PrepareBuildingForDestruction() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			basement.artificialStructures --;
		}
		if (connectedToPowerGrid) GameMaster.colonyController.DisconnectFromPowerGrid(this);
	}

	public void Demolish() {
		if (resourcesContain.Count != 0) {
			foreach (ResourceContainer rc in resourcesContain) {
				GameMaster.colonyController.storage.AddResources(rc);
			}
		}
		Destroy(gameObject);
	}
	void OnDestroy() {
		PrepareBuildingForDestruction();
	}
}
