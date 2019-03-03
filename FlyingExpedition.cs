using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FlyingExpedition : MapPoint {
    public readonly Expedition expedition;
    public MapPoint destination { get; private set; }
    public float speed { get; private set; }

    public FlyingExpedition(Expedition e, MapPoint startPoint, MapPoint i_destination, float i_speed) : base(startPoint.angle, startPoint.height, startPoint.ringIndex, MapMarkerType.Shuttle)
    {
        expedition = e;
        destination = i_destination;
        speed = i_speed;
        stable = true;
    }
    
    public void ChangeDestination(MapPoint mp)
    {
        destination = mp;
    }
}
