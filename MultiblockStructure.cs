using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiblockStructure : Structure {
	List<Block> filledBlocks;
	public byte height = 1;

	public void PartCollapse(ChunkPos pos) {
		
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, xsize_to_set, zsize_to_set);
		b.AddStructure(new SurfaceObject(innerPosition, this));
		Chunk myChunk = basement.myChunk;
		for (byte i = 1; i < height; i++) {
			myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + i), b.pos.z, this);
		}
	}
}
