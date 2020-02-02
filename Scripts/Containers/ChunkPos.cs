public struct ChunkPos
{
    public byte x, y, z;
    public static ChunkPos zer0 { get { return new ChunkPos(0, 0, 0); } }
    public ChunkPos(byte xpos, byte ypos, byte zpos)
    {
        x = xpos; y = ypos; z = zpos;
    }
    public ChunkPos(int xpos, int ypos, int zpos)
    {
        if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0; if (zpos < 0) zpos = 0;
        x = (byte)xpos; y = (byte)ypos; z = (byte)zpos;
    }
    public UnityEngine.Vector3 ToWorldSpace()
    {
        return new UnityEngine.Vector3(x, y, z) * Block.QUAD_SIZE;
    }
    public static bool operator ==(ChunkPos lhs, ChunkPos rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ChunkPos lhs, ChunkPos rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        ChunkPos p = (ChunkPos)obj;
        return (x == p.x) & (y == p.y) & (z == p.z);
    }
    public override int GetHashCode()
    {
        return x * 100 + y * 10 + z;
    }
    public override string ToString()
    {
        return '(' + x.ToString() + ' ' + y.ToString() + ' ' + z.ToString() + ')';
    }
}
