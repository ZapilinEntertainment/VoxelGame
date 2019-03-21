using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingMapPoint : MapPoint {
    public Vector2 moveVector { get; protected set; }

    public MovingMapPoint(float i_angle, float i_height, byte ring, MapMarkerType mtype) : base(i_angle, i_height, mtype)
    {
        moveVector = Vector2.zero;
        stable = true;
    }

    #region save-load
    override public List<byte> Save()
    {
        var bytes = base.Save();
        bytes.AddRange(System.BitConverter.GetBytes(moveVector.x));
        bytes.AddRange(System.BitConverter.GetBytes(moveVector.y));
        return bytes;
    }
    #endregion
}
