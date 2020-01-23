[System.Serializable]
public struct PixelPosByte
{
    public byte x, y;
    public bool exists;
    public static readonly PixelPosByte Empty, zero, one;
    public PixelPosByte(byte xpos, byte ypos) { x = xpos; y = ypos; exists = true; }
    public PixelPosByte(int xpos, int ypos)
    {
        if (xpos < 0) xpos = 0; if (ypos < 0) ypos = 0;
        x = (byte)xpos; y = (byte)ypos;
        exists = true;
    }
    static PixelPosByte()
    {
        Empty = new PixelPosByte(0, 0); Empty.exists = false;
        zero = new PixelPosByte(0, 0); // but exists
        one = new PixelPosByte(1, 1);
    }

    public static bool operator ==(PixelPosByte lhs, PixelPosByte rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(PixelPosByte lhs, PixelPosByte rhs) { return !(lhs.Equals(rhs)); }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        PixelPosByte p = (PixelPosByte)obj;
        return (x == p.x) && (y == p.y) && (exists == p.exists);
    }

    public override int GetHashCode()
    {
        if (exists) return x * y;
        else return x * y * (-1);
    }
}