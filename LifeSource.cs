using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSource : MultiblockStructure {
	public float tick = 3, lifepowerVolume = 100, timer = 0;
	bool hasBeenSet = false;

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		PrepareMultiblockStructure(b,pos);
		hasBeenSet = true;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !hasBeenSet) return;
		timer -= Time.deltaTime * GameMaster.gameSpeed;
		if (timer <= 0 ) {
			int ticksCount = (int)(timer / tick * (-1));
			ticksCount ++;
			basement.myChunk.AddLifePower((int)lifepowerVolume);
			timer = tick;
		}
	}
}
