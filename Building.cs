using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public byte level = 0;
	public MeshRenderer myRenderer;
	public List<ResourceContainer> resourcesContain;
	public float energySurplus = 0, energyCapacity = 0;
	public string buildingName = "building";

	void Awake() {
		hp = maxHp;
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
		if (energyCapacity != 0) GameMaster.colonyController.totalEnergyCapacity += energyCapacity;
	}

	public virtual void SetBasement(SurfaceBlock b, PixelPosByte pos, List<ResourceContainer> f_resourcesContain) {
		if (b == null) return;
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		b.AddStructure(new SurfaceObject(innerPosition, this));
		resourcesContain = f_resourcesContain;
	}


	public override void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			basement.artificialStructures --;
		}
		if (energyCapacity != 0) GameMaster.colonyController.totalEnergyCapacity -= energyCapacity;
	}
}
