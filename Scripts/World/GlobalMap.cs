﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GlobalMap : MonoBehaviour
{
    public float ascension {
        get { return _ascension; }
        set
        {
            if (value > 1f) _ascension = 1f;
            else
            {
                if (value < 0f) _ascension = 0f;
                else _ascension = value;
            }
        }
    }
    private float _ascension, ascensionTarget, updateTimer;
    public float[] ringsRotation { get; private set; }
    //при изменении размера - поменять функции save-load
    public int actionsHash { get; private set; }
    public MapPoint cityPoint { get; private set; }
    public Vector3 cityFlyDirection { get; private set; }
    public Vector3 cityLookVector { get; private set; }
    public List<MapPoint> mapPoints { get; private set; }
    public RingSector[] mapSectors { get; private set; } // нумерация от внешнего к внутреннему

    private bool prepared = false;
    private EnvironmentMaster envMaster;  
    private Environment _outerSpaceEnvironment;
    private bool SYSTEM_envWasCalculatedThisTick = false;
    public System.Action<MapPoint> pointsExploringEvent;

    //Island Engine
    public Engine.ThrustDirection engineThrustDirection { get; private set; }
    private Dictionary<int, Engine> engines;
    public float engineThrust { get; private set; }
    public bool engineControlCenterBuilt { get { return controlCenter != null; } }
    private Building controlCenter;
    //

    public const byte RINGS_COUNT = 5; // dependence : Environment.GetEnvironment
    private const byte MAX_OBJECTS_COUNT = 50;
    private const float MAX_RINGS_ROTATION_SPEED = 1, RING_RESIST_CF = 0.02f, ASCENSION_UPDATE_TIME = 10f;
    private float[] rotationSpeed;
    public readonly float[] ringsBorders = new float[] { 1f, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
    public readonly float[] sectorsDegrees = new float[] { 22.5f, 30, 30, 45, 90 };

    //affection value?
    private Dictionary<int, float> worldAffectionsList;    
    private int nextWAffectionID = 1;
    //
    private BitArray starsTypesArray;
    private bool createNewStars = true;
    private ColonyController colonyController;

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
        ascension = GameConstants.ASCENSION_STARTVALUE;
        ascensionTarget = ascension;
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
        var sunPoint = new SunPoint(startPos.x, startPos.y,  Color.white);        
        RingSector startSector = new RingSector(sunPoint, Environment.defaultEnvironment);
        startSector.SetFertility(false);
        mapSectors[startSectorIndex] = startSector;
        Vector2 dir = Quaternion.AngleAxis(Random.value * 360, Vector3.forward) * Vector2.up;
        float xpos = startPos.x + dir.x * 0.25f * sectorsDegrees[ring], ypos = startPos.y + dir.y * 0.25f * (ringsBorders[ring] - ringsBorders[ring + 1]);
        cityPoint = MapPoint.CreatePointOfType(
            xpos, //angle
            ypos,
             MapPointType.MyCity
        );
        mapPoints.Add(cityPoint); // CITY_POINT_INDEX = 0
        mapPoints.Add(sunPoint);
        //
        starsTypesArray = new BitArray((int)Environment.EnvironmentPreset.TotalCount, false);
        starsTypesArray[(int)Environment.EnvironmentPreset.Default] = true;
        createNewStars = true;
        //
        actionsHash = 0;
        prepared = true;
        //зависимость : Load()
    }
    public void LinkEnvironmentMaster(EnvironmentMaster em)
    {
        envMaster = em;
    }
    public void LinkColonyController(ColonyController c) { colonyController = c; }
    private void RecalculateStarsArray()
    {
        int length = (int)Environment.EnvironmentPreset.TotalCount;
        starsTypesArray = new BitArray(length, false); 
        foreach (var s in mapSectors)
        {
            if (s == null) continue;
            if (s.centralPoint != null && s.centralPoint.type == MapPointType.Star)
            {
                starsTypesArray[(int)s.environment.presetType] = true;
            }
        }
        createNewStars = false;
        for (int i = 0; i < length; i++)
        {
            if ( (Environment.EnvironmentPreset)i == Environment.EnvironmentPreset.Custom) continue;
            else
            {
                if (starsTypesArray[i] == false)
                {
                    createNewStars = true;
                    break;
                }
            }
        }
    }

    public bool IsPointCentral(MapPoint mp)
    {
        if (mp == null || mp.destroyed || (mp is MovingMapPoint)) return false;
        else
        {
            var rsn = DefineSectorIndex(mp);
            var rs = mapSectors[rsn];
            if (rs != null && rs.centralPoint == mp) return true;
            else return false;
        }
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
                        if (!mapPoints[i].DestructionTest())
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
            if (mp.type == MapPointType.Star) GameMaster.realMaster.environmentMaster.RecalculateCelestialDecorations();
            actionsHash++;
            return true;
        }
    }    
    private void RemovePoint(MapPoint mp, bool forced)
    {
        if (mp == null || mp.destroyed) return;
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
    private void RemovePoint(int s_id, bool forced)
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

    private RingSector CreateNewSector(int i)
    {
        MapPointType type; Path p;
        ProgressionMaster.DefinePointOfInterest(this, out type, out p);
        var pos = GetSectorPosition(i);
        RingSector rs;
        if (type != MapPointType.Star)
        {           
            var poi = new PointOfInterest(pos.x, pos.y, type, p);
            rs = new RingSector(poi, Environment.GetEnvironment(ascension, pos.y));
            AddPoint(poi, true);
        }
        else
        {
            var e = Environment.GetEnvironment(ascension, pos.y);
            int presetIndex = (int)e.presetType;
            if (starsTypesArray[presetIndex]) return null;
            else
            {
                SunPoint sunpoint = new SunPoint(
                    pos.x,
                    pos.y,
                    e.GetMapColor()
                    );
                rs = new RingSector(sunpoint, e);
                AddPoint(sunpoint, true);
                starsTypesArray[presetIndex] = true;
                createNewStars = false;
                for (int j = 0; j< (int)Environment.EnvironmentPreset.TotalCount; j++)
                {
                    if ((Environment.EnvironmentPreset)j == Environment.EnvironmentPreset.Custom) continue;
                    else
                    {
                        if (starsTypesArray[j] == false)
                        {
                            createNewStars = true;
                            break;
                        }
                    }
                }
            }
        }
        mapSectors[i] = rs;
        actionsHash++;
        return rs;
    }
    private void AddNewSector(byte index, MapPointType mtype, Environment e)
    {
        if (mapSectors[index] != null) RemoveSector(index);
        Vector2 spos = GetSectorPosition(index);
        MapPoint mpoint = MapPoint.CreatePointOfType(spos.x, spos.y, mtype);
        AddPoint(mpoint, false);
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
                    if (!p.DestructionTest()) RemovePoint(p, false);
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
                            if (!mp.DestructionTest()) RemovePoint(mp, true);
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
    public void RemoveSector(MapPoint mp)
    {
        if (mp == null || mp.destroyed) return;
        else
        {
            foreach (var rs in mapSectors)
            {
                if (rs != null && rs.centralPoint == mp)
                {
                    RemoveSector(rs);
                    return;
                }
            }
        }
    }
    public void RockSector(MapPoint mp)
    {
        if (mp == null || mp.destroyed) return;
        if (IsPointCentral(mp)) {
            var rs = mapSectors[DefineSectorIndex(mp)];
            if (rs != null)
            {
                var sv = rs.GetStabilityValue();
                if (sv < Random.value * 0.3f + 0.7f * ascension) RemoveSector(rs);                
            }
        }
    }

    public int AddWorldAffection(float f)
    {
        if (worldAffectionsList == null) worldAffectionsList = new Dictionary<int, float>();
        int id = nextWAffectionID++;
        worldAffectionsList.Add(id, f);
        return id;
    }
    public void RemoveWorldAffection(int id)
    {
        if (worldAffectionsList != null && worldAffectionsList.ContainsKey(id))
        {
            worldAffectionsList.Remove(id);
            if (worldAffectionsList.Count == 0) worldAffectionsList = null;
        }
    }
    public void ChangeWorldAffection(int id, float newVal)
    {
        if (worldAffectionsList != null && worldAffectionsList.ContainsKey(id))
        {
            worldAffectionsList[id] = newVal;
        }
    }

    public int AddEngine(Engine e)
    {
        if (engines == null)
        {
            engines = new Dictionary<int, Engine>();
            engines.Add(0, e);
            return 0;
        }
        else
        {
            if (engines.ContainsValue(e)) return -1;
            else
            {
                int nextID = -1;
                foreach (var fe in engines)
                {
                    if (fe.Key >= nextID) nextID = fe.Key;
                }
                nextID++;
                engines.Add(nextID, e);
                return nextID;
            }
        }
    }
    public void RemoveEngine(int id)
    {
        engines?.Remove(id);
    }
    public void RegisterEngineControlCenter(Building b)
    {
        controlCenter = b;
    }
    public void UnregisterEngineControlCenter(Building b)
    {
        if (controlCenter == b) controlCenter = null;
    }
    public void ChangeThrustDirection(Engine.ThrustDirection etd)
    {
        engineThrustDirection = etd;
    }

    public void ChangeCityPointHeight(float delta)
    {
        cityPoint.height += delta; // no checks needed
        CityPositionChanged();
    }
    private void CityPositionChanged()
    {
        var sIndex = DefineSectorIndex(cityPoint.angle, cityPoint.ringIndex);
        int currentSectorIndex = GetCurrentSectorIndex();
        if (sIndex != currentSectorIndex)
        {
            Environment env;
            if (mapSectors[currentSectorIndex] != null)
            {
                env = mapSectors[currentSectorIndex].environment;
            }
            else
            {
                var rs = CreateNewSector(sIndex);
                env = rs?.environment ?? GetEmptySpaceEnvironment();
            }
            envMaster.StartConvertingEnvironment(env);
        }
        else envMaster.positionChanged = true;
    }
    private float GetGlobalMapAscension()
    {
        float ringAscension = GameConstants.ASCENSION_MEDIUM;
        var ringIndex = cityPoint.ringIndex;
        if (ringIndex != 2)
        {
            switch (ringIndex)
            {
                case 0: ringAscension = GameConstants.ASCENSION_VERYLOW; break;
                case 1: ringAscension = GameConstants.ASCENSION_LOW; break;
                case 3: ringAscension = GameConstants.ASCENSION_HIGH; break;
                case 4: ringAscension = 1f; break;
            }
        }
        return ringAscension;
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
            byte x2 = (byte)Random.Range(0, RingSector.MAX_POINTS_COUNT);
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
        float f = 0, a ;
        float[] thisCycleRotations = new float[RINGS_COUNT];
        for (int i = 0; i < RINGS_COUNT; i++)
        {
            a = rotationSpeed[i] * t;
            thisCycleRotations[i] = a;
            f = ringsRotation[i];
            f -= thisCycleRotations[i];            
            if (f > 360)
            {
                f = f % 360f;
            }
            else
            {
                if (f < 0) f = 360f - Mathf.Abs(f % 360f);
            }
            ringsRotation[i] = f;
        }

        var prefAsc = ascension;
        updateTimer -= t;
        if (updateTimer <= 0f)
        {
            updateTimer = ASCENSION_UPDATE_TIME;
            
            ascensionTarget = ProgressionMaster.DefineAscension(colonyController);
            a = GetGlobalMapAscension(); if (ascensionTarget > a) ascensionTarget = a;
            a = Knowledge.GetCurrent()?.GetCompleteness() ?? 0f; if (ascensionTarget > a) ascensionTarget = a;
        }
        if (ascension != ascensionTarget) ascension = Mathf.MoveTowards(ascension, ascensionTarget, GameConstants.ASCENSION_CHANGE_SPEED * t * (1.5f - GameMaster.stability));
        float ascensionChange = prefAsc - ascension;
        //
        float prevX = cityPoint.angle, prevY = cityPoint.height;
        if (mapPoints.Count > 0)
        {
            if (GameMaster.realMaster.colonyController != null && cityPoint.ringIndex != 2)
            {
                float speed = RING_RESIST_CF;
                if (cityPoint.ringIndex == 0 || cityPoint.ringIndex == 4) speed *= speed;
                a = Mathf.MoveTowards(cityPoint.height, 0.5f, speed * t);
                ChangeCityPointHeight(a - cityPoint.height);
                // Использование stability?
            }
            int i = 0;
            while (i < mapPoints.Count)
            {
                MapPoint mp = mapPoints[i];
                mp.angle += thisCycleRotations[mp.ringIndex];
                if (mp.type == MapPointType.FlyingExpedition)
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
                                pointsExploringEvent?.Invoke(d);
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
                        AnnouncementCanvasController.MakeAnnouncement(Localization.GetCrewAction(LocalizedCrewAction.CannotReachDestination, fe.expedition.crew));
                        fe.expedition.EndMission();
                    }
                }
                i++;
            }
        }
        //
        cityLookVector = Quaternion.AngleAxis(cityPoint.angle, Vector3.up) * Vector3.forward;
        cityFlyDirection = new Vector3(cityPoint.angle - prevX + rotationSpeed[cityPoint.ringIndex], ascensionChange, cityPoint.height - prevY);
        SYSTEM_envWasCalculatedThisTick = false;
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
        int x = GetCurrentSectorIndex();
        var ms = mapSectors[x];
        if (ms != null) return ms;
        else
        {
            ms = CreateNewSector(x);
            mapSectors[x] = ms;
            return ms;
        }
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
        if (mapSectors[i] == null) return GetEmptySpaceEnvironment();
        else return mapSectors[i].environment;
    }
    public Environment GetEmptySpaceEnvironment()
    {
        if (!SYSTEM_envWasCalculatedThisTick)
        {
            _outerSpaceEnvironment = Environment.GetLerpedEnvironment(Environment.EnvironmentPreset.Space, Environment.EnvironmentPreset.WhiteSpace, ascension);
            SYSTEM_envWasCalculatedThisTick = true;
        }
        return _outerSpaceEnvironment;
    }

    public bool Search()
    {
        int x = Random.Range(0, mapSectors.Length );
        RingSector rs = mapSectors[x];
        if (rs == null)
        {
            return CreateNewSector(x) != null;            
        }
        else return UpdateSector(x); 
    }
    public void MarkToUpdate()
    {
        actionsHash++;
    }

    public void FORCED_CreatePointOfInterest()
    {
        int x = GetCurrentSectorIndex() + 1;
        var pos = GetSectorPosition(x);
        var centralPoint = MapPoint.CreatePointOfType(
        pos.x,
        pos.y,
        MapPointType.Island
        );
        mapSectors[x] = new RingSector(centralPoint, Environment.GetEnvironment(ascension, pos.y));
        AddPoint(centralPoint, true);
    }
    public void TEST_MakeNewPoint(MapPointType mmt)
    {
        int x = Random.Range(0, mapSectors.Length);
        var rs = mapSectors[x];
        if (rs == null)
        {
            AddNewSector((byte)x, mmt, Environment.defaultEnvironment);
        }
    }

    #region save-load system
    public void Save(System.IO.Stream fs)
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
                if (mp.type != MapPointType.FlyingExpedition)
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
    public void Load(System.IO.Stream fs, int saveSystemVersion)
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
            if (p.type == MapPointType.MyCity)
            {
                cityPoint = p;
                break;
            }
        }

        data = new byte[4];
        fs.Read(data, 0, 4);
        ascension = System.BitConverter.ToSingle(data, 0);

        mapSectors = RingSector.StaticLoad(fs);
        RecalculateStarsArray();
        prepared = true;
    }
    #endregion
}
