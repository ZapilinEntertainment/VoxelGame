using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GlobalMap : MonoBehaviour
{
    public float ascension { get; private set; }
    public float[] ringsRotation { get; private set; }
    //при изменении размера - поменять функции save-load
    public int actionsHash { get; private set; }
    public MapPoint cityPoint { get; private set; }
    public Vector3 cityFlyDirection { get; private set; }
    public List<MapPoint> mapPoints { get; private set; }
    public RingSector[] mapSectors { get; private set; } // нумерация от внешнего к внутреннему

    private bool prepared = false, mapInterfaceActive = false;
    public GameObject observer { get; private set; }

    public const byte RINGS_COUNT = 5;
    private const byte MAX_OBJECTS_COUNT = 50;
    private const float MAX_RINGS_ROTATION_SPEED = 1;
    private float[] rotationSpeed;
    public readonly float[] ringsBorders = new float[] { 1, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
    public readonly float[] sectorsDegrees = new float[] { 22.5f, 30, 30, 45, 90 };

    private void Start()
    {
        if (!prepared) Prepare();
    }

    public void Prepare()
    {
        transform.position = Vector3.up * 0.1f;

        rotationSpeed = new float[RINGS_COUNT];
        rotationSpeed[0] = (Random.value - 0.5f) * MAX_RINGS_ROTATION_SPEED;
        rotationSpeed[1] = (Random.value - 0.5f) * MAX_RINGS_ROTATION_SPEED;
        rotationSpeed[2] = (Random.value - 0.5f) * MAX_RINGS_ROTATION_SPEED;
        rotationSpeed[3] = (Random.value - 0.5f) * MAX_RINGS_ROTATION_SPEED;
        rotationSpeed[4] = (Random.value - 0.5f) * MAX_RINGS_ROTATION_SPEED;
        ringsRotation = new float[RINGS_COUNT];

        mapPoints = new List<MapPoint>();
        int sectorsCount = 0;
        for (int i = 0; i < RINGS_COUNT; i++)
        {
            sectorsCount += (int)(360f / sectorsDegrees[i]);
        }
        mapSectors = new RingSector[sectorsCount];
        ascension = 0f;
        //start sector:
        byte ring = RINGS_COUNT / 2;
        sectorsCount = (int)(360f / sectorsDegrees[ring]);
        int min = 0;
        for (int i = 0; i < ring; i++)
        {
            min += (int)(360f / sectorsDegrees[i]);
        }
        int startSectorIndex = Random.Range(min, min + sectorsCount);

        Vector2 startPos = GetSectorPosition(startSectorIndex);
        var ls = Environment.LightSettings.GetPresetLightSettings(Environment.EnvironmentPreset.Default);
        var sunPoint = new SunPoint(startPos.x, startPos.y,  ls.sunColor);        
        RingSector startSector = new RingSector(sunPoint, new Environment(Environment.EnvironmentPreset.Default, ls ));
        startSector.SetFertility(false);
        mapSectors[startSectorIndex] = startSector;
        Vector2 dir = Quaternion.AngleAxis(Random.value * 360, Vector3.forward) * Vector2.up;
        float xpos = startPos.x + dir.x * 0.25f * sectorsDegrees[ring], ypos = startPos.y + dir.y * 0.25f * (ringsBorders[ring] - ringsBorders[ring + 1]);
        cityPoint = MapPoint.CreatePointOfType(
            xpos, //angle
            ypos,
             MapMarkerType.MyCity
        );
        mapPoints.Add(cityPoint); // CITY_POINT_INDEX = 0
        mapPoints.Add(sunPoint);        
        actionsHash = 0;
        prepared = true;
        //зависимость : Load()
    }

    public bool AddPoint(MapPoint mp, bool forced)
    {
        if (mapPoints.Contains(mp)) return false;
        else
        {
            if (!forced)
            {
                if (mapPoints.Count >= MAX_OBJECTS_COUNT)
                {
                    bool placeCleared = false;
                    int i = 0;
                    while (i < mapPoints.Count)
                    {
                        if (mapPoints[i].DestructionTest())
                        {
                            mapPoints.RemoveAt(i);
                            placeCleared = true;
                            break;
                        }
                        i++;
                    }
                    if (!placeCleared) return false;
                }
            }
            mapPoints.Add(mp);
            if (mp.type == MapMarkerType.Star) GameMaster.realMaster.environmentMaster.RecalculateCelestialDecorations();
            actionsHash++;
            return true;
        }
    }    
    public void RemovePoint(MapPoint mp, bool forced)
    {
        if (!forced & mp.stability == 1) return;
        else 
        {
            if (mapPoints.Contains(mp))
            {
                mapPoints.Remove(mp);
                if (!mp.destroyed) mp.MarkAsDestroyed();
                actionsHash++;
            }
        }
    }
    public void RemovePoint(int s_id, bool forced)
    {
        if (mapPoints.Count > 0)
        {
            for (int i = 0;i < mapPoints.Count; i++)
            {
                if (mapPoints[i].ID == s_id)
                {
                    RemovePoint(mapPoints[i], forced);
                    return;
                }
            }
        }
    }
    public MapPoint GetMapPointByID(int s_id)
    {
        if (s_id < 0 || mapPoints.Count == 0) return null;
        else
        {
            foreach (MapPoint mp in mapPoints)
            {
                if (mp != null && mp.ID == s_id)
                {
                    return mp;
                }
            }
            return null;
        }
    }

    private void CreateNewSector(int i)
    {
        var availableTypes = MapPoint.GetAvailablePointsType(ascension);

        var pos = GetSectorPosition(i);
        int x = Random.Range(0, availableTypes.Count - 1);
        var inpos = GetSectorPosition(i);
        byte ring = DefineRing(pos.y);
        if (availableTypes[x] != MapMarkerType.Star)
        {            
            MapPoint centralPoint = MapPoint.CreatePointOfType(
                pos.x,
                pos.y,
                availableTypes[x]
                );
            mapSectors[i] = new RingSector(centralPoint, Environment.GetSuitableEnvironment(ascension));
            AddPoint(centralPoint, true);
        }
        else
        {
            var e = Environment.GetSuitableEnvironment(ascension);
            SunPoint sunpoint = new SunPoint(
                pos.x,
                pos.y,
                e.lightSettings.sunColor
                );
            mapSectors[i] = new RingSector(sunpoint, e);
            AddPoint(sunpoint, true);
        }
        actionsHash++;
    }
    private void AddNewSector(byte index, MapMarkerType mtype, Environment e)
    {
        if (mapSectors[index] != null) RemoveSector(index);
        Vector2 spos = GetSectorPosition(index);
        MapPoint mpoint = MapPoint.CreatePointOfType(spos.x, spos.y, mtype);
        mapPoints.Add(mpoint);
        RingSector rs = new RingSector(mpoint, e);
        mapSectors[index] = rs;
        actionsHash++;
    }
    private void AddSector(RingSector rs, byte index, bool forced)
    {
        if (mapSectors[index] != null & !forced) return;
        else
        {
            if (mapSectors[index] != null) RemoveSector(index);
            mapSectors[index] = rs;
        }
    }
    private void RemoveSector(byte arrayIndex)
    {
        RingSector rs = mapSectors[arrayIndex];
        if (rs != null)
        {
            if (rs.centralPoint != null)
            {
                RemovePoint(rs.centralPoint, true);
            }
            if (rs.innerPointsIDs.Count > 0)
            {
                MapPoint p = null;
                foreach (var mp in rs.innerPointsIDs)
                {
                    p = GetMapPointByID(mp.Value);
                    if (p.DestructionTest()) RemovePoint(p, false);
                }
                rs.innerPointsIDs.Clear();
            }            
            if (mapPoints.Count > 0)
            {
                int x = arrayIndex, i = 0;
                MapPoint mp = null ;
                while (i < mapPoints.Count)
                {
                    mp = mapPoints[i];
                    if (mp == null)
                    {
                        mapPoints.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        x = DefineSectorIndex(mp.angle, mp.ringIndex);
                        if (x == arrayIndex)
                        {
                            if (mp.DestructionTest()) RemovePoint(mp, true);
                        }
                        i++;
                    }
                }
                
            }
            mapSectors[arrayIndex] = null;
            actionsHash++;
        }
    }
    public void RemoveSector(RingSector rs)
    {
        RingSector srs = null;
        int sid = rs.ID;
        for (byte i = 0; i < mapSectors.Length; i++)
        {
            srs = mapSectors[i];
            if (srs != null && srs.ID == sid)
            {
                RemoveSector(i);
                return;
            }
        }
    }

    /// <summary>
    /// returns true if something has changed
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public bool UpdateSector(int i)
    {
        var rs = mapSectors[i];
        if (rs == null)
        {
            return false;
        }
        else
        {
            byte x2 = (byte)Random.Range(0, RingSector.MAX_POINTS_COUNT - 1);
            if (rs.innerPointsIDs.ContainsKey(x2)) // в этой позиции уже есть точка
            {
                MapPoint mp = null;
                int id = -1;
                if (rs.innerPointsIDs.TryGetValue(x2, out id))
                {
                    mp = GetMapPointByID(id);
                    if (mp != null) return mp.Update();
                    else return false;
                }
                else
                {
                    rs.innerPointsIDs.Remove(x2);
                    return false;
                }
            }
            else // пустая позиция
            {
                return rs.CreateNewPoint(x2, ascension, Observatory.GetVisibilityCoefficient());
            }
        }
    } 

    private void Update()
    {
        if (!prepared) return;

        float t = Time.deltaTime * GameMaster.gameSpeed;
        float f = 0;
        for (int i = 0; i < RINGS_COUNT; i++)
        {
            f = ringsRotation[i];
            f -= rotationSpeed[i] * t;
            if (f > 360f)
            {
                f %= 360;
            }
            else
            {
                if (f < 0) f += 360;
            }
            ringsRotation[i] = f;
        }

        float ascensionChange = 0;
        float prevX = cityPoint.angle, prevY = cityPoint.height;
        if (mapPoints.Count > 0)
        {
            if (GameMaster.realMaster.colonyController != null)
            {
                //float s = GameMaster.realMaster.stability;
                //if (s > 1f) s = 1f;
                //if (cpoint.height != s)
                //{
                //    cpoint.height = 1f - s;
                //    cpoint.ringIndex = DefineRing(s);
                //}
            }
            int i = 0;
            while (i < mapPoints.Count)
            {
                MapPoint mp = mapPoints[i];
                mp.angle += rotationSpeed[mp.ringIndex] * t;
                if (mp.type == MapMarkerType.FlyingExpedition)
                {
                    FlyingExpedition fe = (mp as FlyingExpedition);
                    MapPoint d = fe.destination;
                    if (d != null)
                    {
                        if (mp.height != d.height)
                        {
                            mp.height = Mathf.MoveTowards(mp.height, d.height, fe.speed * t * 0.01f);
                        }
                        else
                        {
                            if (mp.angle != d.angle) mp.angle = Mathf.MoveTowardsAngle(mp.angle, d.angle, fe.speed * t);
                            else
                            {
                                mapPoints.RemoveAt(i);
                                fe.expedition.DropMapMarker();
                                if (fe.expedition.stage == Expedition.ExpeditionStage.WayIn)
                                {
                                    fe.expedition.StartMission();
                                }
                                else
                                {
                                    fe.expedition.Dismiss();
                                }                                
                                actionsHash++;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        GameLogUI.MakeAnnouncement(Localization.GetCrewAction(LocalizedCrewAction.CannotReachDestination, fe.expedition.crew));
                        fe.expedition.EndMission();
                    }
                }
                i++;
            }
        }
        cityFlyDirection = new Vector3(cityPoint.angle - prevX + rotationSpeed[cityPoint.ringIndex], ascensionChange, cityPoint.height - prevY);

        

        //test
        if (false)
        {
            bool motion = false;
            float angle = cityPoint.angle;
            if (Input.GetKey("l"))
            {
                angle += 4 * Time.deltaTime;
                if (angle > 360f) angle = 360f - angle;
                else { if (angle < 0) angle += 360f; }
                motion = true;
            }
            else
            {
                if (Input.GetKey("j"))
                {
                    angle -= 4 * Time.deltaTime;
                    if (angle > 360f) angle = 360f - angle;
                    else { if (angle < 0) angle += 360f; }
                    motion = true;
                }
            }
            float height = cityPoint.height;
            if (Input.GetKey("i"))
            {
                height += 0.03f * Time.deltaTime;
                if (height > 1) height = 1;
                else { if (height < 0) height = 0; }
                motion = true;
            }
            else
            {
                if (Input.GetKey("k"))
                {
                    height -= 0.03f * Time.deltaTime;
                    if (height > 1) height = 1;
                    else { if (height < 0) height = 0; }
                    motion = true;
                }
            }

            if (motion)
            {
                cityPoint.angle = angle;
                cityPoint.height = height;
                GameMaster.realMaster.environmentMaster.positionChanged = true;
                var sIndex = DefineSectorIndex(angle, cityPoint.ringIndex);
                int currentSectorIndex = GetCurrentSectorIndex();
                if (sIndex != currentSectorIndex)
                {
                    if (mapSectors[currentSectorIndex] != null)
                    {
                        GameMaster.realMaster.environmentMaster.RefreshEnvironment();
                    }
                    else
                    {
                        GameMaster.realMaster.environmentMaster.SetEnvironment(Environment.defaultEnvironment);
                    }
                }
            }
        }
    }

    //=============  
    public byte DefineRing(float ypos)
    {
        if (ypos < ringsBorders[2])
        {
            if (ypos < ringsBorders[4]) return 4;
            else
            {
                if (ypos > ringsBorders[3]) return 2;
                else return 3;
            }
        }
        else
        {
            if (ypos > ringsBorders[1]) return 0;
            else return 1;
        }
    }
    public byte DefineSectorIndex(float angle, byte ring)
    {
        byte index = 0;
        angle += ringsRotation[ring]; // так как вращается наоборот
        if (angle > 360f) angle -= 360f;
        else { if (angle < 0) angle += 360f; }

        if (ring > 0)
        {
            for (int i = 0; i < ring; i++)
            {
                index += (byte)(360f / sectorsDegrees[i]);
            }
        }
        index += (byte)(angle / sectorsDegrees[ring]);
        return index;
    }
    public byte DefineSectorIndex(MapPoint mp)
    {
        return DefineSectorIndex(mp.angle, mp.ringIndex);
    }
    public byte DefineLocalSectorIndex(byte index) // положение внутри кольца
    {
        for (int i = 0; i < RINGS_COUNT; i++)
        {
            byte sectorsInRing = (byte)(360f / sectorsDegrees[i]);
            if (index >= sectorsInRing) index -= sectorsInRing;
            else return index;
        }
        return index;
    }

    public RingSector GetCurrentSector()
    {
        return mapSectors[GetCurrentSectorIndex()];
    }
    public int GetCurrentSectorIndex()
    {
        if (cityPoint != null) return DefineSectorIndex(cityPoint);
        else return 0; 
    }    
    public Vector2 GetSectorPosition(int index)
    {
        byte i = 0;
        for (; i < RINGS_COUNT; i++)
        {
            int sectorsInRing = (int)(360f / sectorsDegrees[i]);
            if (index >= sectorsInRing) index -= sectorsInRing;
            else break;
        }
        float sd = sectorsDegrees[i];
        float angle = sd * index + sd / 2f - ringsRotation[i];
        if (angle > 360f) angle = 360f - angle;
            else { if (angle < 0) angle += 360f; }
        float h = ringsBorders[i] - (ringsBorders[i] - ringsBorders[i + 1]) / 2f;
        return new Vector2(angle, h);
    }
    public Vector2 GetCurrentSectorCenter()
    {
        return GetSectorPosition(GetCurrentSectorIndex());
    }
    public Environment GetCurrentEnvironment()
    {
        int i = GetCurrentSectorIndex();
        if (mapSectors[i] == null) return Environment.defaultEnvironment;
        else return mapSectors[i].environment;
    }

    public bool Search()
    {
        int x = Random.Range(0, mapSectors.Length - 1);
        RingSector rs = mapSectors[x];
        if (rs == null)
        {
            CreateNewSector(x);
            return true;
        }
        else return UpdateSector(x); 
    }
    public void ShowOnGUI()
    {
        if (!prepared) return;
        if (observer == null)
        {
            observer = Instantiate(Resources.Load<GameObject>("UIPrefs/globalMapUI"));
            observer.GetComponent<GlobalMapUI>().SetGlobalMap(this);
        }
        mapInterfaceActive = true;
        GameMaster.realMaster.environmentMaster.DisableDecorations();
        if (!observer.activeSelf) observer.SetActive(true);
    }
    public void MarkToUpdate()
    {
        actionsHash++;
    }
    public void MapInterfaceDisabled()
    {
        mapInterfaceActive = false;
        GameMaster.realMaster.environmentMaster.EnableDecorations();
    }

    public void FORCED_CreatePointOfInterest()
    {
        int x = GetCurrentSectorIndex() + 1;
        var pos = GetSectorPosition(x);
        var centralPoint = MapPoint.CreatePointOfType(
        pos.x,
        pos.y,
        MapMarkerType.Island
        );
        mapSectors[x] = new RingSector(centralPoint, Environment.GetSuitableEnvironment(ascension));
        AddPoint(centralPoint, true);
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[0]), 0, 4); // 0 - 3
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[1]), 0, 4); // 4 - 7
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[2]), 0, 4); // 8 - 11
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[3]), 0, 4); // 12 - 15
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[4]), 0, 4); // 16 - 19

        fs.Write(System.BitConverter.GetBytes(ringsRotation[0]), 0, 4); // 20 - 23
        fs.Write(System.BitConverter.GetBytes(ringsRotation[1]), 0, 4); // 24 - 27
        fs.Write(System.BitConverter.GetBytes(ringsRotation[2]), 0, 4); // 28 - 31
        fs.Write(System.BitConverter.GetBytes(ringsRotation[3]), 0, 4); // 32 - 35
        fs.Write(System.BitConverter.GetBytes(ringsRotation[4]), 0, 4); // 36 - 39

        int realCount = 0;
        var savedata = new List<byte>();
        if (mapPoints.Count > 0)
        {             
            foreach (MapPoint mp in mapPoints)
            {
                if (mp.type != MapMarkerType.FlyingExpedition)
                {
                    savedata.AddRange(mp.Save());
                    realCount++;
                }                
            }
        }
        fs.Write(System.BitConverter.GetBytes(realCount),0,4);
        if (realCount > 0)
        {
            var saveArray = savedata.ToArray();
            fs.Write(saveArray, 0, saveArray.Length);
        }
        fs.Write(System.BitConverter.GetBytes(MapPoint.nextID), 0, 4);    //зависимость : mapPoint.LoadPoints()
        fs.Write(System.BitConverter.GetBytes(ascension), 0, 4);

        RingSector.StaticSave(fs, mapSectors);
    }
    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[40];
        fs.Read(data, 0, data.Length);
        rotationSpeed = new float[RINGS_COUNT];
        rotationSpeed[0] = System.BitConverter.ToSingle(data, 0);
        rotationSpeed[1] = System.BitConverter.ToSingle(data, 4);
        rotationSpeed[2] = System.BitConverter.ToSingle(data, 8);
        rotationSpeed[3] = System.BitConverter.ToSingle(data, 12);
        rotationSpeed[4] = System.BitConverter.ToSingle(data, 16);
        ringsRotation = new float[RINGS_COUNT];
        ringsRotation[0] = System.BitConverter.ToSingle(data, 20);
        ringsRotation[1] = System.BitConverter.ToSingle(data, 24);
        ringsRotation[2] = System.BitConverter.ToSingle(data, 28);
        ringsRotation[3] = System.BitConverter.ToSingle(data, 32);
        ringsRotation[4] = System.BitConverter.ToSingle(data, 36);
        if (!prepared)
        {
            transform.position = Vector3.up * 0.1f;
            actionsHash = 1;            
        }
        if (mapPoints == null) mapPoints = new List<MapPoint>();
        else mapPoints.Clear();
        mapPoints = MapPoint.LoadPoints(fs);
        foreach (var p in mapPoints)
        {
            if (p.type == MapMarkerType.MyCity)
            {
                cityPoint = p;
                break;
            }
        }

        data = new byte[4];
        fs.Read(data, 0, 4);
        ascension = System.BitConverter.ToSingle(data, 0);

        mapSectors = RingSector.StaticLoad(fs);

        prepared = true;
    }
    #endregion
}
