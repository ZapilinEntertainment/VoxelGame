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
        if (xpos < 0) xpos = 255; if (ypos < 0) ypos = 255; if (zpos < 0) zpos = 255;
        x = (byte)xpos; y = (byte)ypos; z = (byte)zpos;
    }

    public UnityEngine.Vector3 ToWorldSpace()
    {
        return new UnityEngine.Vector3(x, y, z) * Block.QUAD_SIZE;
    }
    public ChunkPos OneBlockHigher()
    {
        return new ChunkPos(x, y + 1, z);
    }
    public ChunkPos TwoBlocksHigher()
    {
        return new ChunkPos(x, y + 2, z);
    }
    public ChunkPos OneBlockForward()
    {
        return new ChunkPos(x, y, z + 1);
    }
    public ChunkPos OneBlockRight()
    {
        return new ChunkPos(x + 1, y, z);
    }
    public ChunkPos OneBlockBack()
    {
        return new ChunkPos(x, y, z - 1);
    }
    public ChunkPos OneBlockLeft()
    {
        return new ChunkPos(x - 1, y, z);
    }
    public ChunkPos OneBlockDown()
    {
        return new ChunkPos(x, y - 1, z);
    }

    public static bool operator ==(ChunkPos lhs, ChunkPos rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ChunkPos lhs, ChunkPos rhs) { return !(lhs.Equals(rhs)); }
    public static ChunkPos operator + (ChunkPos lhs, UnityEngine.Vector3 rhs) {
        return new ChunkPos((int)(lhs.x + rhs.x), (int)(lhs.y + rhs.y), (int)(lhs.z + rhs.z));
    }

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
