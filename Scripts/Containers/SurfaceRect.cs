[System.Serializable]
public struct SurfaceRect
{
    public byte x, z, size;
    public SurfaceRect(byte f_x, byte f_z, byte f_size)
    {
        if (f_x < 0) f_x = 0; if (f_x >= ImprovedPlane.INNER_RESOLUTION) f_x = ImprovedPlane.INNER_RESOLUTION - 1;
        if (f_z < 0) f_z = 0; if (f_z >= ImprovedPlane.INNER_RESOLUTION) f_z = ImprovedPlane.INNER_RESOLUTION - 1;
        if (f_size < 1) f_size = 1; if (f_size > ImprovedPlane.INNER_RESOLUTION) f_size = ImprovedPlane.INNER_RESOLUTION;
        x = f_x;
        z = f_z;
        size = f_size;
    }

    static SurfaceRect()
    {
        one = new SurfaceRect(0, 0, 1);
        full = new SurfaceRect(0, 0, ImprovedPlane.INNER_RESOLUTION);
    }

    public bool Intersect(SurfaceRect sr)
    {
        int leftX = -1, rightX = -1;
        if (x > sr.x) leftX = x; else leftX = sr.x;
        if (x + size > sr.x + sr.size) rightX = sr.x + sr.size; else rightX = x + size;
        if (leftX < rightX)
        {
            int topZ = -1, downZ = -1;
            if (z > sr.z) downZ = z; else downZ = sr.z;
            if (z + size > sr.z + sr.size) topZ = sr.z + sr.size; else topZ = z + size;
            return topZ > downZ;
        }
        else return false;
    }
    public bool Intersect(int xpos, int zpos, int xsize, int zsize)
    {
        int leftX = -1, rightX = -1;
        if (x > xpos) leftX = x; else leftX = xpos;
        if (x + size > xpos + xsize) rightX = xpos + xsize; else rightX = x + size;
        if (leftX < rightX)
        {
            int topZ = -1, downZ = -1;
            if (z > zpos) downZ = z; else downZ = zpos;
            if (z + size > zpos + zsize) topZ = zpos + zsize; else topZ = z + size;
            return topZ > downZ;
        }
        else return false;
    }

    public static bool operator ==(SurfaceRect lhs, SurfaceRect rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(SurfaceRect lhs, SurfaceRect rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        SurfaceRect p = (SurfaceRect)obj;
        return (x == p.x) & (z == p.z) & (size == p.size);
    }
    public override int GetHashCode()
    {
        return x + z + size;
    }
    public static readonly SurfaceRect one;
    public static readonly SurfaceRect full;
    public override string ToString()
    {
        return '(' + x.ToString() + ' ' + z.ToString() + ") size:" + size.ToString();
    }
}
