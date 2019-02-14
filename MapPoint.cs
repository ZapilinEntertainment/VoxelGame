using UnityEngine;


public class MapPoint
{ 
    public byte subIndex { get; protected set; }
    public byte ringIndex;
    public MapMarkerType type { get; protected set; }
    public float angle;
    public float height;

    private const byte WRECKS_TYPE_COUNT = 10;

    public MapPoint(float i_angle, float i_height, byte ring, MapMarkerType mtype)
    {
        angle = i_angle;
        height = i_height;
        ringIndex = ring;
        type = mtype;
        switch (type)
        {
            case MapMarkerType.Wreck: subIndex = (byte)(Random.value * WRECKS_TYPE_COUNT); break;
            default: subIndex = 0; break;
        }
    }

    public virtual bool DestroyRequest()
    {
        return true;
    }
}