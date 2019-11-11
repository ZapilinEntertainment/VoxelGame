using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition
{
    public enum ExpeditionStage : byte { WayIn, WayOut, OnMission, LeavingMission, Disappeared, Dismissed }

    public static List<Expedition> expeditionsList { get; private set; }
    public static byte listChangesMarker { get; private set; }
    public static int expeditionsSucceed;
    private static UIExpeditionObserver _observer;
    public static int nextID { get; private set; } // в сохранение

    public readonly int ID;

    public bool hasConnection { get; private set; } // есть ли связь с центром
    public byte changesMarkerValue { get; private set; }
    public byte crystalsCollected { get; private set; }
    public byte suppliesCount { get; private set; }
    public ExpeditionStage stage { get; private set; }
    public PointOfInterest destination { get; private set; }
    public Crew crew { get; private set; }
    public Artifact artifact { get; private set; }

    private bool subscribedToUpdate = false;
    private FlyingExpedition mapMarker;
    private int shuttleID = Hangar.NO_SHUTTLE_VALUE, transmissionID = QuantumTransmitter.NO_TRANSMISSION_VALUE;

    private const float FLY_SPEED = 5f;
    public const int MIN_SUPPLIES_COUNT = 20, MAX_SUPPLIES_COUNT = 200, MAX_START_CRYSTALS = 200;

    // STATIC & equals
    static Expedition()
    {
        expeditionsList = new List<Expedition>();
        nextID = 0;
    }
    public static void GameReset()
    {
        expeditionsList = new List<Expedition>();
        listChangesMarker = 0;
    }
    public static Expedition GetExpeditionByID(int s_id)
    {
        if (expeditionsList.Count == 0) return null;
        else
        {
            foreach (Expedition e in expeditionsList)
            {
                if (e != null && e.ID == s_id) return e;
            }
            return null;
        }
    }
    public static void ChangeTransmissionStatus(int t_id, bool? status)
    {
        if (expeditionsList.Count > 0)
        {
            foreach (Expedition e in expeditionsList)
            {
                if (e.transmissionID == t_id)
                {
                    e.ChangeTransmissionStatus(status);
                    return;
                }
            }
        }
    }
    public static UIExpeditionObserver GetObserver()
    {
        if (_observer == null)
        {
            _observer = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefs/expeditionPanel"), 
                UIController.current.mainCanvas).GetComponent<UIExpeditionObserver>();
        }
        return _observer;
    }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Expedition e = (Expedition)obj;
        return (ID == e.ID);
    }
    public override int GetHashCode()
    {
        return ID;
    }

    /// ===============================
    public Expedition(PointOfInterest i_destination, Crew c, int i_shuttleID, QuantumTransmitter transmitter)
    {
        ID = nextID++;
        stage = ExpeditionStage.WayIn;
        
        destination = i_destination; destination.AssignExpedition(this);
        crew = c; c.SetCurrentExpedition(this);

        shuttleID = i_shuttleID;
        if (transmitter != null)
        {
            transmissionID = transmitter.StartTransmission();
            hasConnection = true;
        }
        else
        {
            transmissionID = QuantumTransmitter.NO_TRANSMISSION_VALUE;
            hasConnection = false;
        }

        GlobalMap gmap = GameMaster.realMaster.globalMap;
        mapMarker = new FlyingExpedition(this, gmap.cityPoint, destination, FLY_SPEED);
        gmap.AddPoint(mapMarker, true);        

        expeditionsList.Add(this);
        listChangesMarker++;
    }
    private Expedition(int i_id) // for loading only
    {
        ID = i_id;
        expeditionsList.Add(this);
        listChangesMarker++;
        changesMarkerValue = 0;
    }

    public void LabourUpdate()
    {

    }

    public void StartMission()
    {
        stage = ExpeditionStage.OnMission;
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
            subscribedToUpdate = false;
        }
    }
    public void EndMission()
    {

    }

    /// <summary>
    /// returns true if expedition dissappeared in process, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool SectorCollapsingTest() // INDEV
    {
        return false;
    }

    public void ChangeTransmissionStatus(bool? x)
    {
        if (x == null)
        {
            transmissionID = QuantumTransmitter.NO_TRANSMISSION_VALUE;
            hasConnection = false;
            crew.LoseConfidence(2f);
            changesMarkerValue++;
            if (mapMarker != null) GameMaster.realMaster.globalMap.MarkToUpdate();
        }
        else
        {
            if (transmissionID != QuantumTransmitter.NO_TRANSMISSION_VALUE)
            {
                if (x == true)
                {
                    if (!hasConnection)
                    {
                        hasConnection = true;
                        crew.RaiseConfidence(1f);
                        changesMarkerValue++;
                        if (mapMarker != null) GameMaster.realMaster.globalMap.MarkToUpdate();
                    }
                }
                else
                {
                    if (hasConnection)
                    {
                        hasConnection = false;
                        crew.LoseConfidence(1f);
                        changesMarkerValue++;
                        if (mapMarker != null) GameMaster.realMaster.globalMap.MarkToUpdate();
                    }
                }
            }
        }
    }

    public void ShowOnGUI(Rect r, SpriteAlignment alignment, bool onMainCanvas)
    {
        var ob = GetObserver();
        if (!ob.isActiveAndEnabled) ob.gameObject.SetActive(true);
        ob.SetPosition(r, alignment, onMainCanvas);
        ob.Show(this);
    }

    public void Dismiss() // экспедиция вернулась домой и распускается
    {
        //зависимость : Disappear()
        if (stage == ExpeditionStage.Disappeared) return;
        else
        {
            if (crew != null) crew.SetCurrentExpedition(null);
            QuantumTransmitter.StopTransmission(transmissionID);
            Hangar.ReturnShuttle(shuttleID);
            if (destination != null) destination.DeassignExpedition(this);
            if (suppliesCount > 0) GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.Supplies, suppliesCount);
            //if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            if (crystalsCollected > 0)
            {
                GameMaster.realMaster.colonyController.AddEnergyCrystals(crystalsCollected);
                crystalsCollected = 0;
            }
            stage = ExpeditionStage.Dismissed;           
        }
        changesMarkerValue++;
    }
    public void Disappear() // INDEV
                            // экспедиция исчезает
    {
        if (stage == ExpeditionStage.Dismissed) return;
        else
        {
            if (crew != null) crew.Disappear();
            QuantumTransmitter.StopTransmission(transmissionID);
            Hangar.ReturnShuttle(shuttleID);
            if (destination != null) destination.DeassignExpedition(this);
            if (subscribedToUpdate & !GameMaster.sceneClearing)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
            //if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            stage = ExpeditionStage.Dismissed;
        }
        changesMarkerValue++;
    }

    #region save-load system
    public List<byte> Save()
    {
        //awaiting
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(ID)); // read before load
        
        return data;
    }
    public Expedition Load(System.IO.FileStream fs)
    {
        int LENGTH = 37; // full length - 4 (id excluded)
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        
        return this;
    }

    public static void SaveStaticData(System.IO.FileStream fs)
    {
        int realCount = 0;
        var savedata = new List<byte>();
        if (expeditionsList.Count > 0)
        {
            foreach (Expedition e in expeditionsList)
            {
                if (e != null && e.stage != ExpeditionStage.Dismissed)
                {
                    savedata.AddRange(e.Save());
                    realCount++;
                }
            }
        }

        fs.Write(System.BitConverter.GetBytes(realCount), 0, 4);
        if (realCount > 0)
        {
            var dataArray = savedata.ToArray();
            fs.Write(dataArray, 0, dataArray.Length);
        }
        fs.Write(System.BitConverter.GetBytes(nextID), 0, 4);
        fs.Write(System.BitConverter.GetBytes(expeditionsSucceed), 0, 4);
    }
    public static void LoadStaticData(System.IO.FileStream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data, 0);
        expeditionsList = new List<Expedition>();
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                fs.Read(data, 0, 4);
                int r_id = System.BitConverter.ToInt32(data, 0);
                Expedition e = new Expedition(r_id);
                e.Load(fs);

            }
        }
        data = new byte[8];
        fs.Read(data, 0, 8);
        nextID = System.BitConverter.ToInt32(data, 0);
        expeditionsSucceed = System.BitConverter.ToInt32(data, 4);
    }
    #endregion
}

