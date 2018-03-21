using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building {
	public int housing = 2;
	public static float GROW_SPEED = 0.01f;

	void Awake() {
		buildingName = "House lv."+ level.ToString();
		innerPosition = new SurfaceRect(0,0,xsize_to_set, zsize_to_set);
		isArtificial = markAsArtificial;
		type = setType;
	}

	public override void SetBasement(SurfaceBlock b, PixelPosByte pos, List<ResourceContainer> f_resourcesContain) {
		if (b == null) return;
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		b.AddStructure(new SurfaceObject(innerPosition, this));
		resourcesContain = f_resourcesContain;
		GameMaster.colonyController.AddHousing(housing);
		if (GameMaster.colonyController.housingLevel < level) GameMaster.colonyController.housingLevel = level;
	}
}
