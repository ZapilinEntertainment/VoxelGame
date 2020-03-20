using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class RingSector
{
    public bool destroyed { get; private set; }
    public bool fertile { get; private set; } // can sector produce new points inside?
    public readonly int ID;
    public readonly MapPoint centralPoint; // always presented
    public readonly Environment environment;
    public Dictionary<byte, int> innerPointsIDs; // dependency : static save & load
    public const int MAX_POINTS_COUNT = 10;
    public static int lastFreeID { get; private set; }

    public static void SetLastUsedID(int x)
    {
        x = lastFreeID;
    }

    public RingSector(SunPoint sun_point, Environment.EnvironmentPreset environmentPresetType)
    {
        ID = lastFreeID;
        lastFreeID++;
        centralPoint = sun_point;
        centralPoint.SetStability(1f);
        environment = Environment.GetEnvironment(environmentPresetType);
        sun_point.SetStability(1f);
        innerPointsIDs = new Dictionary<byte, int>();
        destroyed = false;
    }
    public RingSector(MapPoint i_point, Environment i_environment)
    {
        ID = lastFreeID;
        lastFreeID++;
        centralPoint = i_point;
        centralPoint.SetStability(1f);
        environment = i_environment;
        i_point.SetStability(1f);
        innerPointsIDs = new Dictionary<byte, int>();
        destroyed = false;
    }
    //loading constructor
    public RingSector(int i_ID, MapPoint i_centralPoint, Environment.EnvironmentPreset e_preset)
    {
        ID = i_ID;
        centralPoint = i_centralPoint;
        centralPoint.SetStability(1f);
        environment = Environment.GetEnvironment(e_preset);
        innerPointsIDs = new Dictionary<byte, int>();
        destroyed = false;
    }

    public float GetVisualSaturationValue() // значение насыщенности цветов биома в зависимости от удаленности от центра
    {
        var gmap = GameMaster.realMaster.globalMap;
        var cp = gmap.cityPoint;
        byte ring =  centralPoint.ringIndex;
        float angleDelta = cp.angle - centralPoint.angle;
        if (Mathf.Abs(angleDelta) > 180f)
        {
            if (centralPoint.angle > cp.angle)
            {
                angleDelta = (360f - centralPoint.angle) + cp.angle;
            }
            else
            {
                angleDelta = (360f - cp.angle) + centralPoint.angle;
            }
        }
        
        float angleX = angleDelta / (gmap.sectorsDegrees[ring] / 2f);
        float heightY = (cp.height - centralPoint.height) / ((gmap.ringsBorders[ring] - gmap.ringsBorders[ring + 1]) / 2f);
        Vector2 lookDist = new Vector2(angleX, heightY);

        float d = lookDist.magnitude;
        if (d > 1) return 0f;
        else return Mathf.Sin((d + 1) * 90 * Mathf.Deg2Rad);
    }

    public Vector2 GetInnerPointPosition(byte x)
    {
        Vector2 localPos = Vector2.zero;
        switch (x)
        {
            case 0: localPos = new Vector2(-0.7f, -0.7f);break;
            case 1: localPos = new Vector2(-0.7f, 0f); break;
            case 2: localPos = new Vector2(-0.7f, 0.7f); break;
            case 3: localPos = new Vector2(-0.2f, -0.7f); break;
            case 4: localPos = new Vector2(-0.2f, 0.7f); break;
            case 5: localPos = new Vector2(0.2f, -0.7f); break;
            case 6: localPos = new Vector2(0.2f, 0.7f); break;
            case 7: localPos = new Vector2(0.7f, -0.7f); break;
            case 8: localPos = new Vector2(0.7f, 0f); break;
            case 9: localPos = new Vector2(0.7f, 0.7f); break;
            default: Debug.Log("RingSector.cs: non-correct inner position");break;
        }
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        int ring = centralPoint.ringIndex;
        return new Vector2(
            centralPoint.angle + gmap.sectorsDegrees[ring] * 0.5f * localPos.x,
            centralPoint.height + (gmap.ringsBorders[ring] - gmap.ringsBorders[ring + 1]) * 0.5f * localPos.y
            );
    }
    public void AddInnerPoint(MapPoint mp, byte index)
    {        
        if (innerPointsIDs.ContainsKey(index)) {
            int pti = -1;
            innerPointsIDs.TryGetValue(index, out pti);
            var pt = GameMaster.realMaster.globalMap.GetMapPointByID(pti);
            if (pt != null) GameMaster.realMaster.globalMap.RemovePoint(pti, true);
            innerPointsIDs.Remove(index);
        }
        mp.SetCoords(GetInnerPointPosition(index));
        innerPointsIDs.Add(index, mp.ID);
    }
      

    public void MarkAsDestroyed()
    {
        destroyed = true;
    }
    public void SetFertility(bool x)
    {
        fertile = x;
    }

    public bool CreateNewPoint(byte positionIndex, float ascension, float visibility)
    {
        if (innerPointsIDs.ContainsKey(positionIndex) | !fertile) return false;
        else
        {
            var pos = GetInnerPointPosition(positionIndex);
            MapPoint mp;
            if (Random.value < visibility)
            {
                mp = MapPoint.CreatePointOfType(pos.x, pos.y, environment.PickMainPointType() );
            }
            else
            {
                mp = MapPoint.CreatePointOfType(pos.x, pos.y, MapMarkerType.Unknown);
            }
            if (mp != null)
            {
                if (GameMaster.realMaster.globalMap.AddPoint(mp, false))
                {
                    innerPointsIDs.Add(positionIndex, mp.ID);
                    return true;
                }
                else return false;
            }
            else return false;
        }
    }

    #region save-load
    public static void StaticSave(System.IO.FileStream fs, RingSector[] sectorsArray)
    {
        fs.Write(System.BitConverter.GetBytes(sectorsArray.Length),0,4);

        byte zeroByte = 0, oneByte = 1;
        var gmap = GameMaster.realMaster.globalMap;
        int info = -1;
        RingSector s;
        for (int i = 0; i < sectorsArray.Length; i++)
        {
            s = sectorsArray[i];
            if (s == null || s.destroyed) fs.WriteByte(zeroByte);
            else
            {
                fs.WriteByte(oneByte);   
                
                fs.Write(System.BitConverter.GetBytes(s.ID),0,4); // 0 -3 
                if (s.centralPoint != null) info = s.centralPoint.ID; else info = -1;
                fs.Write(System.BitConverter.GetBytes(info),0,4); // 4 - 7
                fs.WriteByte((byte)s.environment.presetType); // 8

                info = s.innerPointsIDs.Count;
                fs.Write(System.BitConverter.GetBytes(info),0,4); // 9 - 12
                if (info > 0)
                {
                    foreach (var ip in s.innerPointsIDs)
                    {
                        fs.WriteByte(ip.Key);
                        fs.Write(System.BitConverter.GetBytes(ip.Value),0,4);
                    }
                }
                fs.WriteByte(s.fertile ? oneByte : zeroByte); // 13
            }

        }
        fs.Write(System.BitConverter.GetBytes(lastFreeID),0,4);
    }

    public static RingSector[] StaticLoad(System.IO.FileStream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data, 0), readVal1 = -1, readVal2 = -1;
        var sectors = new RingSector[count];
        MapPoint f_centralPoint = null;
        var gmap = GameMaster.realMaster.globalMap;
        for (int i = 0; i < count; i++)
        {
            readVal1 = fs.ReadByte();
            if (readVal1 == 0) continue;
            else
            {
                data = new byte[14];
                fs.Read(data, 0, data.Length);
                readVal1 = System.BitConverter.ToInt32(data, 0);
                readVal2 = System.BitConverter.ToInt32(data, 4);
                f_centralPoint = gmap.GetMapPointByID(readVal2);

                var rs = new RingSector(readVal1, f_centralPoint, (Environment.EnvironmentPreset)data[8]);
                readVal1 = System.BitConverter.ToInt32(data, 9);
                if (readVal1 > 0)
                {
                    data = new byte[5 * readVal1 + 1]; // byte + int
                    fs.Read(data, 0, data.Length);
                    for (int j = 0; j< readVal1;j++)
                    {
                        rs.innerPointsIDs.Add(data[j * 5], System.BitConverter.ToInt32(data, 5 * j + 1));
                    }                    
                }
                rs.fertile = data[data.Length - 1] == 1;

                if (f_centralPoint != null) sectors[i] = rs;
            }
        }
        data = new byte[4];
        fs.Read(data, 0, 4);
        lastFreeID = System.BitConverter.ToInt32(data, 0);
        return sectors;
    }
    #endregion
}
