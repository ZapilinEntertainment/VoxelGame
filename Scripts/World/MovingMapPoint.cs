using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingMapPoint : MapPoint
{
    public Vector2 moveVector { get; protected set; }

    public MovingMapPoint(float i_angle, float i_height, byte ring, MapPointType mtype) : base(i_angle, i_height, mtype)
    {
        moveVector = Vector2.zero;
    }

    #region save-load
    // не используются
    override public List<byte> Save()
    {
        var bytes = base.Save();
        bytes.AddRange(System.BitConverter.GetBytes(moveVector.x));
        bytes.AddRange(System.BitConverter.GetBytes(moveVector.y));
        return bytes;
    }
    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[8];
        fs.Read(data, 0, 8);
        moveVector = new Vector2(System.BitConverter.ToSingle(data, 0), System.BitConverter.ToSingle(data, 4));
    }
    #endregion
}
