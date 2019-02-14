using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FlyingExpedition : MovingMapPoint {
    public Expedition expedition { get; private set; }
    public MapPoint destination { get; private set; }

    public FlyingExpedition(float i_angle, float i_height, byte ring, MapMarkerType mtype, Expedition e, MapPoint i_destination) : base(i_angle, i_height, ring, mtype)
    {
        expedition = e;
        destination = i_destination;
    }
}
