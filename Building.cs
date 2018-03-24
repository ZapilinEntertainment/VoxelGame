using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool isActive {get;protected set;}
	public byte level = 0;
	public List<ResourceContainer> resourcesContain;
	public float energySurplus = 0, energyCapacity = 0;
	protected bool connectedToPowerGrid = false;
	public string buildingName = "building";

	void Awake() {
		PrepareBuilding();
	}
	protected void PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		buildingName += Localization.info_level + ' ' + level.ToString();
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
	}

	protected void SetBuildingData(SurfaceBlock b, PixelPosByte pos) {
		SetStructureData(b,pos);
		isActive = true;
		if (energySurplus != 0 || energyCapacity != 0) {
			GameMaster.colonyController.AddToPowerGrid(this);
			connectedToPowerGrid = true;
		}
	}

	virtual public void SetActivationStatus(bool x) {
		if (x == isActive) return;
		ChangeBuildingActivity(x);
	}
	protected void ChangeBuildingActivity (bool x) {
		isActive = x;
		if (connectedToPowerGrid) GameMaster.colonyController.RecalculatePowerGrid();
	}


	protected void PrepareBuildingForDestruction() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			basement.artificialStructures --;
		}
		if (connectedToPowerGrid) GameMaster.colonyController.DisconnectFromPowerGrid(this);
	}

	void OnDestroy() {
		PrepareBuildingForDestruction();
	}
}
