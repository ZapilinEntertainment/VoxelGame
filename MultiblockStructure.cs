public class MultiblockStructure : Structure {
	public byte additionalHeight = 1;

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetMultiblockStructureData(b, pos);
    }

    protected void SetMultiblockStructureData(SurfaceBlock b, PixelPosByte pos)
    {
        SetStructureData(b, pos);
        Chunk myChunk = basement.myChunk;
        for (byte i = 1; i < additionalHeight; i++)
        {
            myChunk.BlockByStructure(b.pos.x, (byte)(b.pos.y + i), b.pos.z, this);
        }        
    }
}
