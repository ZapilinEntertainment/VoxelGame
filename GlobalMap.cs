using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSector
{
    public readonly MapPoint centralPoint;
    public readonly Environment environment;
    public const int MAX_INTEREST_POINT_COUNT = 3;


    public RingSector(SunPoint sun_point, Environment.EnvironmentPreset environmentPresetType)
    {
        centralPoint = sun_point;
        var ls = Environment.defaultEnvironment.lightSettings;
        environment = new Environment(environmentPresetType, new Environment.LightSettings(sun_point, 1, ls.bottomColor, ls.horizonColor));
        centralPoint.SetStability(true);
    }
    public RingSector(MapPoint i_point, Environment i_environment)
    {
        centralPoint = i_point;
        environment = i_environment;
        centralPoint.SetStability(true);
    }
}

public sealed class GlobalMap : MonoBehaviour
{
    public float ascension { get; private set; }
    public float ascensionLowBorder { get; private set; }
    public float ascensionUpBorder { get; private set; }
    public float[] ringsRotation { get; private set; }
    //при изменении размера - поменять функции save-load
    public int actionsHash { get; private set; }
    public Vector3 cityFlyDirection { get; private set; }
    public List<MapPoint> mapPoints { get; private set; }
    public RingSector[] mapSectors { get; private set; } // нумерация от внешнего к внутреннему

    private bool prepared = false;
    private int currentSectorIndex = 0;
    private GameObject mapUI_go;
    private Dictionary<SunPoint, Transform> stars;

    public const byte RINGS_COUNT = 5;
    private const byte MAX_OBJECTS_COUNT = 50;
    public const int CITY_POINT_INDEX = 0, SUN_POINT_INDEX = 1;
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

        ascension = GameConstants.START_HAPPINESS;
        ascensionLowBorder = ascension - 0.1f;
        ascensionUpBorder = ascension + 0.1f;

        stars = new Dictionary<SunPoint, Transform>();
        //start sector:
        byte ring = RINGS_COUNT / 2;
        sectorsCount = (int)(360f / sectorsDegrees[ring]);
        int min = 0;
        for (int i = 0; i < ring; i++)
        {
            min += (int)(360f / sectorsDegrees[i]);
        }
        currentSectorIndex = Random.Range(min, min + sectorsCount);

        Vector2 startPos = GetSectorPosition(currentSectorIndex);
        var sunPoint = new SunPoint(startPos.x, startPos.y,  new Color(Random.value, Random.value, Random.value));
        var ls = Environment.defaultEnvironment.lightSettings;
        RingSector startSector = new RingSector(sunPoint, new Environment(Environment.EnvironmentPreset.Default, new Environment.LightSettings(sunPoint, 1f, ls.bottomColor, ls.horizonColor) ));
        mapSectors[currentSectorIndex] = startSector;
        Vector2 dir = Quaternion.AngleAxis(Random.value * 360, Vector3.forward) * Vector2.up;
        MapPoint cityPoint = new MapPoint(
            startPos.x + dir.x * 0.25f * sectorsDegrees[ring], //angle
            startPos.y + dir.y * 0.25f * (ringsBorders[ring] - ringsBorders[ring + 1]),
             MapMarkerType.MyCity
        );
        mapPoints.Add(cityPoint); // CITY_POINT_INDEX = 0
        mapPoints.Add(sunPoint); // SUN_POINT_INDEX = 1

        var g = new GameObject("star");
        g.layer = GameConstants.CLOUDS_LAYER;
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = PoolMaster.GetStarSprite(false);
        sr.sharedMaterial = PoolMaster.billboardMaterial;
        sr.color = startSector.environment.lightSettings.sunColor;
        stars.Add(sunPoint, g.transform);

        //TEST:

        //

        //
        actionsHash = 0;
        prepared = true;
        //зависимость : Load()
    }

    public MapPoint GetCityPoint() { return mapPoints[CITY_POINT_INDEX]; }
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
                        if (!mapPoints[i].stable)
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
            if (mp.type == MapMarkerType.Star)
            {
                var sp = mp as SunPoint;
                if (!stars.ContainsKey(sp)) {
                    var g = new GameObject("star");
                    g.layer = GameConstants.CLOUDS_LAYER;
                    var sr = g.AddComponent<SpriteRenderer>();
                    sr.sprite = PoolMaster.GetStarSprite(false);
                    sr.sharedMaterial = PoolMaster.billboardMaterial;
                    stars.Add(sp, g.transform );
                }
            }
            actionsHash++;
            return true;
        }
    }
    public void RemovePoint(MapPoint mp)
    {
        if (mp.stable) return;
        if (mapPoints.Contains(mp))
        {
            if (mp.type == MapMarkerType.Star)
            {
                var sp = mp as SunPoint;
                if (stars.ContainsKey(sp))
                {
                    stars.Remove(sp);
                }
            }
            mapPoints.Remove(mp);
            actionsHash++;
        }
    }
    private void RemovePoint(int index)
    {
        var mp = mapPoints[index];
        if ( mp!= null && !mp.stable)
        {
            if (mp.type == MapMarkerType.Star)
            {
                var sp = mp as SunPoint;
                if (stars.ContainsKey(sp))
                {
                    stars.Remove(sp);
                }
            }
            mapPoints.RemoveAt(index);            
            actionsHash++;
        }
    }
    public MapPoint GetMapPointByID(int s_id)
    {
        if (mapPoints.Count == 0) return null;
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

    private void AddNewSector(byte index, MapMarkerType mtype, Environment e)
    {
        if (mapSectors[index] != null) RemoveSector(index);
        Vector2 spos = GetSectorPosition(index);
        MapPoint mpoint = new MapPoint(spos.x, spos.y, mtype);
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
    private void RemoveSector(byte index)
    {
        if (mapSectors[index] != null)
        {
            if (mapPoints.Count > 0)
            {
                MapPoint mp = mapSectors[index].centralPoint;
                byte ring = mp.ringIndex;
                float angleStep = sectorsDegrees[ring] / 2f;
                float heightStep = (ringsBorders[ring] - ringsBorders[ring + 1]) / 2f;

                int i = 0;
                while (i < mapPoints.Count)
                {
                    if (!mapPoints[i].stable)
                    {
                        RemovePoint(mapPoints[i]);
                    }
                    else
                    {
                        if (mapPoints[i].type == MapMarkerType.Shuttle)
                        {
                            FlyingExpedition fe = mapPoints[i] as FlyingExpedition;
                            if (!fe.expedition.SectorCollapsingTest())
                            {
                                fe.expedition.Dissappear(false);
                                fe.SetStability(false);
                                RemovePoint(fe);
                            }
                        }
                    }
                }
            }
            actionsHash++;
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
        var cpoint = GetCityPoint();
        float prevX = cpoint.angle, prevY = cpoint.height;

        if (mapPoints.Count > 0)
        {
            if (GameMaster.realMaster.colonyController != null)
            {
                float h = GameMaster.realMaster.colonyController.happiness_coefficient;
                if (h != ascension)
                {
                    ascensionChange = h - ascension;
                    ascension = h;
                    //ascension sectors check
                }
            }
            int i = 0;
            while (i < mapPoints.Count)
            {
                MapPoint mp = mapPoints[i];
                mp.angle += rotationSpeed[mp.ringIndex] * t;
                if (mp.type == MapMarkerType.Shuttle)
                {
                    FlyingExpedition fe = (mp as FlyingExpedition);
                    MapPoint d = fe.destination;
                    if (d != null)
                    {
                        if (mp.height != d.height)
                        {
                            mp.height = Mathf.MoveTowards(mp.height, d.height, fe.speed * t);
                            mp.ringIndex = DefineRing(mp.height);
                        }
                        else
                        {
                            if (mp.angle != d.angle) mp.angle = Mathf.MoveTowardsAngle(mp.angle, d.angle, fe.speed * t);
                            else
                            {
                                if (fe.expedition.stage == Expedition.ExpeditionStage.WayIn)
                                {
                                    fe.expedition.MissionStart();
                                }
                                else
                                {
                                    fe.expedition.Dismiss();
                                }
                                mapPoints.RemoveAt(i);
                                actionsHash++;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        GameLogUI.MakeAnnouncement(Localization.GetExpeditionStatus(LocalizedExpeditionStatus.CannotReachDestination, fe.expedition));
                        fe.expedition.EndMission();
                    }
                }
                i++;
            }
        }

        cityFlyDirection = new Vector3(cpoint.angle - prevX + rotationSpeed[cpoint.ringIndex], ascensionChange, cpoint.height - prevY);

        var cp = Quaternion.AngleAxis(cpoint.angle, Vector3.back) * (Vector3.up * cpoint.height);
        if (stars.Count > 0)
        {
            foreach (var se in stars)
            {
                var p = Quaternion.AngleAxis(se.Key.angle, Vector3.back) * (Vector3.up * se.Key.height);
                p -= cp;
                p.z = p.y;
                p.y = 0.2f;
                se.Value.position = p * 10;
            }
        }

        //test
        bool motion = false;
        float angle = cpoint.angle;
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
        float height = cpoint.height;
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
            cpoint.ChangeCoords(angle, height);
            GameMaster.realMaster.environmentMaster.positionChanged = true;
            var sIndex = DefineSectorIndex(angle, cpoint.ringIndex);
            if (sIndex != currentSectorIndex)
            {
                currentSectorIndex = sIndex;
                if (mapSectors[currentSectorIndex] != null)
                {
                    GameMaster.realMaster.environmentMaster.ChangeEnvironment(mapSectors[currentSectorIndex].environment);
                }
                else
                {
                    GameMaster.realMaster.environmentMaster.ChangeEnvironment(Environment.defaultEnvironment);
                }
            }
        }
    }

    //=============  PUBLIC METHODS
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
        return GetSectorPosition(currentSectorIndex);
    }

    public Environment GetCurrentEnvironment()
    {
        MapPoint cityPoint = mapPoints[CITY_POINT_INDEX];
        byte si = DefineSectorIndex(cityPoint.angle, cityPoint.ringIndex);
        if (mapSectors[si] == null) return Environment.defaultEnvironment;
        else return mapSectors[si].environment;
    }

    public bool Search()
    {
        MapMarkerType mmtype = MapMarkerType.Unknown;
        float f = Random.value;
        float height = 0.5f;
        if (f <= 0.5f)
        {//resources            
            f *= 2;
            if (f <= 0.6f)
            {
                mmtype = MapMarkerType.Resources;
                height = 0.1f + 0.89f * Random.value;
            }
            else
            {
                if (f > 0.9f)
                {
                    mmtype = MapMarkerType.Island;
                    height = 0.45f - 0.3f * Random.value;
                }
                else
                {
                    mmtype = MapMarkerType.Wreck;
                    height = 0.8f + Random.value * 0.2f;
                }
            }
        }
        else
        {
            if (f <= 0.7f)
            {// exp
                f = Random.value;
                if (f <= 0.5f)
                {
                    if (f < 0.25f)
                    {
                        mmtype = MapMarkerType.Wiseman;
                        height = 0.1f + Random.value * 0.15f;
                    }
                    else
                    {
                        mmtype = MapMarkerType.Wonder;
                        height = 0.1f + Random.value * 0.7f;
                    }
                }
                else
                {
                    if (f > 0.8f)
                    {
                        if (f > 0.9f)
                        {
                            mmtype = MapMarkerType.Portal;
                            height = 0.8f + 0.15f * Random.value;
                        }
                        else
                        {
                            mmtype = MapMarkerType.Island;
                            height = 0.55f + 0.3f * Random.value;
                        }
                    }
                    else
                    {
                        if (f > 0.65f)
                        {
                            mmtype = MapMarkerType.Wreck;
                            height = 0.7f + 0.2f * Random.value;
                        }
                        else
                        {
                            mmtype = MapMarkerType.SOS;
                            height = 0.1f + 0.9f * Random.value;
                        }
                    }
                }
            }
            else
            {
                if (f > 0.9f)
                { // special
                    //ограничения на количество!
                    f = Random.value;
                    if (f > 0.5f)
                    {
                        return false;
                    }
                    else
                    {
                        if (f > 0.75f)
                        {
                            mmtype = MapMarkerType.Colony;
                            height = 0.3f + 0.5f * Random.value;
                        }
                        else
                        {
                            mmtype = MapMarkerType.Station;
                            height = 0.9f + 0.09f * Random.value;
                        }
                    }
                }
                else
                { // quest-starting objects
                    return false;
                }
            }
        }

        float angle = Random.value * 360;
        bool somethingFound = false;
        switch (mmtype)
        {
            case MapMarkerType.Station:
            case MapMarkerType.Wreck:
            case MapMarkerType.Island:
            case MapMarkerType.SOS:
            case MapMarkerType.Portal:
            case MapMarkerType.Colony:
            case MapMarkerType.Wiseman:
            case MapMarkerType.Wonder:
            case MapMarkerType.Resources:
                {
                    somethingFound = AddPoint(new PointOfInterest(angle, height, DefineRing(height), mmtype), false);
                    break;
                }
            case MapMarkerType.Star:
                somethingFound = AddPoint(new MapPoint(angle, height, mmtype), false);
                break;
        }
        if (somethingFound)
        {
            GameLogUI.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.NewObjectFound));
            if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.newObjectFound);
        }
        return somethingFound;
    }
    public void ShowOnGUI()
    {
        if (!prepared) return;
        if (mapUI_go == null)
        {
            mapUI_go = Instantiate(Resources.Load<GameObject>("UIPrefs/globalMapUI"));
            mapUI_go.GetComponent<GlobalMapUI>().SetGlobalMap(this);
        }
        if (!mapUI_go.activeSelf) mapUI_go.SetActive(true);
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[0]), 0, 4);
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[1]), 0, 4);
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[2]), 0, 4);
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[3]), 0, 4);
        fs.Write(System.BitConverter.GetBytes(rotationSpeed[4]), 0, 4);

        if (mapPoints.Count > 0)
        {
            int realCount = 0;
            var saveArray = new List<byte>();
            foreach (MapPoint mp in mapPoints)
            {
                if (mp.type != MapMarkerType.Shuttle)
                {
                    saveArray.AddRange(mp.Save());
                    realCount++;
                }
            }
            if (realCount > 0)
            {
                fs.WriteByte((byte)realCount);
                fs.Write(saveArray.ToArray(), 0, saveArray.Count);
            }
        }
        else fs.WriteByte(0);
        fs.Write(System.BitConverter.GetBytes(MapPoint.lastUsedID), 0, 4);
        //зависимость : mapPoint.LoadPoints()
    }
    public void Load(System.IO.FileStream fs)
    {
        var data = new byte[RINGS_COUNT * 4];
        fs.Read(data, 0, data.Length);
        rotationSpeed = new float[RINGS_COUNT];
        rotationSpeed[0] = System.BitConverter.ToSingle(data, 0);
        rotationSpeed[1] = System.BitConverter.ToSingle(data, 4);
        rotationSpeed[2] = System.BitConverter.ToSingle(data, 8);
        rotationSpeed[3] = System.BitConverter.ToSingle(data, 12);
        rotationSpeed[4] = System.BitConverter.ToSingle(data, 16);
        if (!prepared)
        {
            transform.position = Vector3.up * 0.1f;
            actionsHash = 1;
            prepared = true;
        }

        if (mapPoints == null) mapPoints = new List<MapPoint>();
        else mapPoints.Clear();
        mapPoints = MapPoint.LoadPoints(fs);
    }
    #endregion
}
