using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiblockStructure : Structure {
	List<Block> filledBlocks;
	public byte additionalHeight = 1;

	public void PartCollapse(ChunkPos pos) {
		
	}

	protected void PrepareMultiblockStructure(SurfaceBlock b, PixelPosByte pos) {
		basement = b;
		innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.x_size, innerPosition.z_size);
		b.AddStructure(this);
		Chunk myChunk = basement.myChunk;
		for (byte i = 1; i < additionalHeight; i++) {
			myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + i), b.pos.z, this);
		}
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		PrepareMultiblockStructure(b,pos);
	}
}
