using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLife : MultiblockStructure {
	Chunk myChunk;
	
	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		basement = b;
		Content myContent = Content.Structure; if (isMainStructure) myContent = Content.MainStructure;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set,zsize_to_set, myContent, gameObject);
		b.AddStructure(innerPosition);
		myChunk = basement.myChunk;
		for (byte i = 1; i < height; i++) {
			myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + i), b.pos.z, this);
		}
		myChunk.AddLifePower(GameMaster.START_LIFEPOWER);
	}
}
