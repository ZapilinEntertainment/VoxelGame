using System.Collections.Generic;

public sealed class FlyingExpedition : MapPoint {
    public readonly Expedition expedition;
    public MapPoint destination { get; private set; }
    public float speed { get; private set; }

    public FlyingExpedition(Expedition e, MapPoint startPoint, MapPoint i_destination, float i_speed) : base(startPoint.angle, startPoint.height, MapPointType.FlyingExpedition)
    {
        expedition = e;
        destination = i_destination;
        speed = i_speed;
        stability = 1f;
    }
    public FlyingExpedition(Expedition e, float i_angle, float i_height, MapPoint i_destination, float i_speed) : base(i_angle, i_height, MapPointType.FlyingExpedition)
    {
        expedition = e;
        destination = i_destination;
        speed = i_speed;
        stability = 1f;
    }
    /// <summary>
    /// Loading constructor
    /// </summary>
    public FlyingExpedition(int i_id, Expedition e) : base(i_id) {
        expedition = e;
    }
    
    public void ChangeDestination(MapPoint mp)
    {
        destination = mp;
    }

    override public bool DestructionTest()
    {
        if (expedition == null) return false;
        else return expedition.SectorCollapsingTest();
    }

    #region save-load
    override public List<byte> Save()
    {
        return null;
    }
    #endregion
}
