using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GlobalMap : MonoBehaviour {    
    
    public float[] rotationSpeed { get; private set; }
    //при изменении размера - поменять функции save-load
    public int actionsHash { get; private set; }      
    public List<MapPoint> mapPoints { get; private set; }

    private bool prepared = false;
    private GameObject mapUI_go;

    public const byte RINGS_COUNT = 5;
    private const byte MAX_OBJECTS_COUNT = 50;
    public const int CITY_POINT_INDEX = 0, SUN_POINT_INDEX = 1;
    private const int TEMPORARY_POINTS_MASK = 15593;
    private const float MAX_RINGS_ROTATION_SPEED = 1;
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

        mapPoints = new List<MapPoint>();
        float h = GameConstants.START_HAPPINESS;
        AddPoint(new MapPoint(Random.value * 360, h, DefineRing(h), MapMarkerType.MyCity), true);
        h = 0.9f;
        AddPoint(new MapPoint(Random.value * 360, h, DefineRing(h), MapMarkerType.Star), true);

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
                        int mmt = (int)mapPoints[i].type;
                        if ((mmt & TEMPORARY_POINTS_MASK) != 0)
                        {
                            if (mapPoints[i].DestroyRequest())
                            {
                                mapPoints.RemoveAt(i);
                                placeCleared = true;
                                break;
                            }
                        }
                        i++;
                    }
                    if (!placeCleared) return false;
                }
            }
            mapPoints.Add(mp);
            actionsHash++;
            return true;
        }
    }
    public void RemovePoint(MapPoint mp)
    {
        if (mapPoints.Contains(mp))
        {
            mapPoints.Remove(mp);
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

    private void Update()
    {
        if (Input.GetKeyDown("x"))
        {
            if (AddPoint(new PointOfInterest(0.3f, 0.3f, 1, MapMarkerType.Resources), true)) Debug.Log("okay");
        }

        if (!prepared) return;       

        if (mapPoints.Count > 0)
        {
            float t = Time.deltaTime * GameMaster.gameSpeed;

            if (GameMaster.realMaster.colonyController != null)
            {
                MapPoint cityPoint = mapPoints[CITY_POINT_INDEX];
                float h = 1 - GameMaster.realMaster.colonyController.happiness_coefficient;
                if (cityPoint.height != h)
                {
                    cityPoint.height = h;
                    cityPoint.ringIndex = DefineRing(h);
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
                    somethingFound =  AddPoint(new PointOfInterest(angle, height, DefineRing(height), mmtype), false);
                    break;
                }
            case MapMarkerType.Star:
                somethingFound =  AddPoint(new MapPoint(angle, height, DefineRing(height), mmtype), false);
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
        if (mapUI_go == null) {
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
        fs.Write(System.BitConverter.GetBytes(MapPoint.lastUsedID),0,4);
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
