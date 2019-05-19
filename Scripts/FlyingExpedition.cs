public sealed class FlyingExpedition : MapPoint {
    public readonly Expedition expedition;
    public MapPoint destination { get; private set; }
    public float speed { get; private set; }

    public FlyingExpedition(Expedition e, MapPoint startPoint, MapPoint i_destination, float i_speed) : base(startPoint.angle, startPoint.height, MapMarkerType.Shuttle)
    {
        expedition = e;
        destination = i_destination;
        speed = i_speed;
        stability = 1f;
    }
    
    public void ChangeDestination(MapPoint mp)
    {
        destination = mp;
    }

    override public bool DestructionTest()
    {
        if (expedition == null) return true;
        else return expedition.SectorCollapsingTest();
    }
}
