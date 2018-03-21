using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageHouse : Structure {
	public float volume = 1000;

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		b.AddStructure(new SurfaceObject(innerPosition, this));
		GameMaster.colonyController.storage.AddVolume(volume);
	}

	public override void OnDestroy() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			GameMaster.colonyController.storage.ContractVolume(volume);
		}
	}
}
