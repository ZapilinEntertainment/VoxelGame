using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeStone : MultiblockStructure {
	Chunk myChunk;

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		b.AddStructure(new SurfaceObject(innerPosition, this));
		myChunk = basement.myChunk;
		for (byte i = 1; i < height; i++) {
			myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + i), b.pos.z, this);
		}
		myChunk.AddLifePower(GameMaster.START_LIFEPOWER);
	}
}
