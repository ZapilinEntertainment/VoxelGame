﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GlobalMap : MonoBehaviour {

    public float[] ringsBorders { get; private set; }
    public float[] rotationSpeed { get; private set; }
    public int actionsHash { get; private set; }      
    public List<MapPoint> mapPoints { get; private set; }

    private bool prepared = false;
    private GameObject mapUI_go;

    private const int CITY_POINT_INDEX = 0;
    private const int TEMPORARY_POINTS_MASK = 15593;
    private const int MAX_OBJECTS_COUNT = 50;
    private const float SHUTTLES_SPEED = 4;

    private void Start()
    {
        if (!prepared) Prepare();
    }

    public void Prepare()
    {
        transform.position = Vector3.up * 0.1f;

        ringsBorders = new float[6] { 1, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
        rotationSpeed = new float[5];
        rotationSpeed[0] = (Random.value - 0.5f) * 3;
        rotationSpeed[1] = (Random.value - 0.5f) * 3;
        rotationSpeed[2] = (Random.value - 0.5f) * 3;
        rotationSpeed[3] = (Random.value - 0.5f) * 3;
        rotationSpeed[4] = (Random.value - 0.5f) * 3;

        mapPoints = new List<MapPoint>();
        float h = GameConstants.START_HAPPINESS;
        AddPoint(new MapPoint(Random.value * 360, h, DefineRing(h), MapMarkerType.MyCity));

        actionsHash = 0;
        prepared = true;
    }

    private bool AddPoint(MapPoint mp)
    {
        if (mapPoints.Contains(mp)) return false;
        else
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

    private void Update()
    {
        if (!prepared) return;       

        if (mapPoints.Count > 0)
        {
            float t = Time.deltaTime * GameMaster.gameSpeed;

            if (GameMaster.realMaster.colonyController != null)
            {
                MapPoint cityPoint = mapPoints[CITY_POINT_INDEX];
                float h = 1 - GameMaster.realMaster.colonyController.happiness_coefficient;
                if (h != cityPoint.height)
                {
                    cityPoint.height = h;
                    cityPoint.ringIndex = DefineRing(h);
                }
            }

            int i = 0;
            while (i < mapPoints.Count)
            {
                MapPoint mp = mapPoints[i];
                if (mp.type == MapMarkerType.Shuttle)
                {
                    MapPoint destination = (mp as FlyingExpedition).destination;
                    mp.height = Mathf.MoveTowards(mp.height, destination.height, SHUTTLES_SPEED * t);
                    mp.angle = Mathf.MoveTowardsAngle(mp.angle, destination.angle, SHUTTLES_SPEED * t);
                    if (mp.height == destination.height & mp.angle == destination.angle)
                    {
                        (mp as FlyingExpedition).DestinationReached(destination);
                        continue;
                    }
                }
                mp.angle += rotationSpeed[mp.ringIndex] * t;
                i++;
            }
        }
    }
    
    //=============  PUBLIC METHODS

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
                        mmtype = MapMarkerType.Star;
                        height = 0.15f + 0.7f * Random.value;
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
        switch (mmtype)
        {
            case MapMarkerType.Station:
            case MapMarkerType.Wreck:
            case MapMarkerType.Island:
            case MapMarkerType.SOS: 
            case MapMarkerType.Portal: 
            case MapMarkerType.Colony: 
            case MapMarkerType.Star:
            case MapMarkerType.Wiseman: 
            case MapMarkerType.Wonder: 
            case MapMarkerType.Resources:
                {
                    return AddPoint(new PointOfInterest(angle, height, DefineRing(height), mmtype));
                }
            default: return false;
        }
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

    public bool ExpeditionLaunch(Expedition e, MapPoint destination)
    {
        if (mapPoints.Count == 0) return false;
        else
        {
            MapPoint cityPoint = mapPoints[CITY_POINT_INDEX];
            FlyingExpedition fePoint = new FlyingExpedition(cityPoint.angle, cityPoint.height, cityPoint.ringIndex, MapMarkerType.Shuttle, e, destination);
            return AddPoint(fePoint);
        }
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {
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
                print(realCount);
                print(saveArray.Count);
            }
        }
        else fs.WriteByte(0);
    }
    public void Load(System.IO.FileStream fs)
    {
        if (mapPoints == null) mapPoints = new List<MapPoint>();
        else mapPoints.Clear();
        mapPoints = MapPoint.LoadPoints(fs);
    }
    #endregion
}
