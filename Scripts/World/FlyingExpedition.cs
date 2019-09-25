using System.Collections.Generic;

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
        if (expedition == null) return true;
        else return expedition.SectorCollapsingTest();
    }

    #region save-load
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(speed));
        return data;
    }
    public static FlyingExpedition LoadExpeditionMarker(System.IO.FileStream fs, Expedition e)
    {
        int LENGTH = 22; // 18 + 4, changed
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        var fe = new FlyingExpedition(System.BitConverter.ToInt32(data, 0), e);
        fe.type = MapMarkerType.Shuttle;
        fe.subIndex = data[5];
        fe.angle = System.BitConverter.ToSingle(data, 6);
        fe.height = System.BitConverter.ToSingle(data, 10);
        fe.stability = System.BitConverter.ToSingle(data, 14);

        fe.speed = System.BitConverter.ToSingle(data, 18);       
        fe.destination = e.destination;
        GameMaster.realMaster.globalMap.AddPoint(fe, true);
        return fe;
    }
    #endregion
}
