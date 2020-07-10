using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition
{
    public enum ExpeditionStage : byte { WayIn, WayOut, OnMission, LeavingMission, Disappeared, Dismissed }

    public static List<Expedition> expeditionsList { get; private set; }
    public static byte listChangesMarker { get; private set; }
    public static uint expeditionsLaunched { get; private set; }
    public static uint expeditionsSucceed { get; private set; }
    private static UIExpeditionObserver _observer;
    public static int nextID { get; private set; } // в сохранение

    public readonly int ID;

    public bool hasConnection { get; private set; } // есть ли связь с центром
    public byte changesMarkerValue { get; private set; }
    public ushort crystalsCollected { get; private set; }
    public byte suppliesCount { get; private set; }    
    public ExpeditionStage stage { get; private set; }
    public PointOfInterest destination { get; private set; }
    public Crew crew { get; private set; }
    public Artifact artifact { get; private set; }

    private bool subscribedToUpdate = false, missionCompleted;
    private int shuttleID = Hangar.NO_SHUTTLE_VALUE, transmissionID = QuantumTransmitter.NO_TRANSMISSION_VALUE;
    private FlyingExpedition mapMarker;
    private Vector2Int planPos = Vector2Int.zero;

    private const float FLY_SPEED = 5f;
    public const int MIN_SUPPLIES_COUNT = 20, MAX_SUPPLIES_COUNT = 200, MAX_START_CRYSTALS = 200, MAX_CRYSTALS_COLLECTED = 60000, NO_VALUE = -1, MAX_EXPEDITIONS_COUNT = 100;

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
    public Expedition(PointOfInterest i_destination, Crew c, int i_shuttleID, QuantumTransmitter transmitter, float i_supplies, float i_crystals)
    {
        // СДЕЛАТЬ: проверка компонентов и вывод ошибок
        ID = nextID++;
        stage = ExpeditionStage.WayIn;
        
        destination = i_destination; destination.AssignExpedition(this);
        crew = c; c.SetCurrentExpedition(this);

        if (Hangar.OccupyShuttle(i_shuttleID)) shuttleID = i_shuttleID;
        else
        {
            GameLogUI.MakeAnnouncement(Localization.GetExpeditionErrorText(ExpeditionComposingErrors.ShuttleUnavailable));
            Dismiss();
        }

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
        suppliesCount = (byte)i_supplies;
        crystalsCollected = (ushort)i_crystals;

        //#creating map marker
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        mapMarker = new FlyingExpedition(this, gmap.cityPoint, destination, FLY_SPEED);
        gmap.AddPoint(mapMarker, true);
        //

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

    public void StartMission()
    {
        stage = ExpeditionStage.OnMission;
        changesMarkerValue++;
    }
    public void EndMission()
    {
        switch (stage)
        {
            case ExpeditionStage.OnMission:
                stage = ExpeditionStage.WayOut;
                // #creating map marker
                GlobalMap gmap = GameMaster.realMaster.globalMap;
                mapMarker = new FlyingExpedition(this, destination, gmap.cityPoint, FLY_SPEED);
                gmap.AddPoint(mapMarker, true);
                //
                changesMarkerValue++;
                break;
            case ExpeditionStage.WayIn:
                mapMarker.ChangeDestination(GameMaster.realMaster.globalMap.cityPoint);
                changesMarkerValue++;
                break;
        }
    }
    public void DropMapMarker()
    {
        mapMarker = null;
    }
    public void CountMissionAsSuccess()
    {
        missionCompleted = true;
    }
    
    private void LabourUpdate()
    {
        // проверки на восстановление связи?
    }

    public Vector2Int GetPlanPos()
    {
        return planPos;
    }
    public void SetPlanPos(Vector2Int pos)
    {
        planPos = pos;
    }

    /// <summary>
    /// returns true if expedition dissappeared in process, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool SectorCollapsingTest() // INDEV
    {
        return Mathf.Sqrt(crew.TechSkillsRoll() * crew.SecretKnowledgeRoll()) >= 25f;
    }
    public bool SuccessfulExitTest()
    {
        return true;
        //return crew.SurvivalSkillsRoll() * (0.5f + 0.5f * Random.value) + crew.IntelligenceRoll() * (0.5f + 0.5f * Random.value) >= 30;
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
    public void PayFee(byte cost)
    {
        if (crystalsCollected <= cost) crystalsCollected = 0;
        else
        {
            crystalsCollected -= cost;            
        }
        changesMarkerValue++;
    }
    public void AddCrystals(int cost)
    {
        int x = crystalsCollected + cost;
        if (x <= MAX_CRYSTALS_COLLECTED) crystalsCollected += (ushort)cost;
        else crystalsCollected = MAX_CRYSTALS_COLLECTED;
        changesMarkerValue++;
    }
    public void SpendSupplyCrate()
    {
        if (GameMaster.realMaster.weNeedNoResources) return;
        if ( suppliesCount > 0) suppliesCount --;
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
        if (stage == ExpeditionStage.Disappeared | stage == ExpeditionStage.Dismissed) return;
        else
        {
            GameLogUI.MakeAnnouncement(Localization.GetCrewAction(LocalizedCrewAction.Returned, crew));
            if (crew != null)
            {
                crew.CountMission(missionCompleted);
                crew.SetCurrentExpedition(null);
                crew = null;
            }
            QuantumTransmitter.StopTransmission(transmissionID);
            Hangar.ReturnShuttle(shuttleID);
            if (destination != null) destination.DeassignExpedition(this);
            if (suppliesCount > 0) GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.Supplies, suppliesCount);
            if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            if (crystalsCollected > 0)
            {
                GameMaster.realMaster.colonyController.AddEnergyCrystals(crystalsCollected);
                crystalsCollected = 0;
            }
            stage = ExpeditionStage.Dismissed;
            if (missionCompleted)
            {
                expeditionsSucceed++;
                Knowledge.GetCurrent()?.ExpeditionsCheck(expeditionsSucceed);
            }

            if (subscribedToUpdate & !GameMaster.sceneClearing)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
            changesMarkerValue++;
        }        
    }
    public void Disappear() // INDEV
                            // экспедиция исчезает
    {
        if (stage == ExpeditionStage.Dismissed | stage == ExpeditionStage.Disappeared) return;
        else
        {
            if (crew != null) crew.Disappear();
            QuantumTransmitter.StopTransmission(transmissionID);
            Hangar.ReturnShuttle(shuttleID);
            if (destination != null) destination.DeassignExpedition(this);
            //if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            stage = ExpeditionStage.Disappeared;
           
            if (subscribedToUpdate & !GameMaster.sceneClearing)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
            expeditionsList.Remove(this);
            changesMarkerValue++;
        }
        changesMarkerValue++;
    }

    #region save-load system
    public List<byte> Save()
    {
        //awaiting
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(ID)); // read before load

        byte falsebyte = 0, truebyte = 1;
        data.Add(hasConnection ? truebyte : falsebyte); //0
        data.AddRange(System.BitConverter.GetBytes(crystalsCollected)); // 1 - 2
        data.Add(suppliesCount); // 3
        data.Add((byte)stage); // 4
        data.AddRange(System.BitConverter.GetBytes(destination != null ? destination.ID : NO_VALUE)); // 5 - 8
        data.AddRange(System.BitConverter.GetBytes(crew.ID)); // 9 - 12
        data.AddRange(System.BitConverter.GetBytes(artifact != null ? artifact.ID : NO_VALUE)); // 13 - 16
        data.AddRange(System.BitConverter.GetBytes(shuttleID)); // 17 - 20
        data.AddRange(System.BitConverter.GetBytes(transmissionID)); // 21 - 24
        data.Add((byte)planPos.x); // 25
        data.Add((byte)planPos.y); // 26
        if ((stage == ExpeditionStage.WayIn | stage == ExpeditionStage.WayOut) && mapMarker != null)
        {
            data.Add(truebyte); //27
            data.AddRange(System.BitConverter.GetBytes(mapMarker.angle)); // (0-3)
            data.AddRange(System.BitConverter.GetBytes(mapMarker.height)); // (4-7)
        }
        else data.Add(falsebyte); //27
        data.Add(missionCompleted ? truebyte : falsebyte); // 0
        return data;
    }
    public Expedition Load(System.IO.FileStream fs)
    {
        int LENGTH = 28; // (id excluded)
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        hasConnection = data[0] == 1;
        crystalsCollected = System.BitConverter.ToUInt16(data, 1);
        suppliesCount = data[3];
        stage = (ExpeditionStage)data[4];
        int x = System.BitConverter.ToInt32(data, 5);
        if (x != NO_VALUE)
        {
            destination = GameMaster.realMaster.globalMap.GetMapPointByID(x) as PointOfInterest;
            destination.AssignExpedition(this);
        }
        else destination = null;
        crew = Crew.GetCrewByID(System.BitConverter.ToInt32(data, 9));
        if (crew != null) crew.SetCurrentExpedition(this);
        artifact = Artifact.GetArtifactByID(System.BitConverter.ToInt32(data, 13));
        shuttleID = System.BitConverter.ToInt32(data, 17);
        transmissionID = System.BitConverter.ToInt32(data, 21);
        planPos = new Vector2Int(data[25], data[26]);
        if (data[27] == 1)
        {
            data = new byte[8];
            fs.Read(data, 0, 8);
            // #creating map marker
            GlobalMap gmap = GameMaster.realMaster.globalMap;
            mapMarker = new FlyingExpedition(this, System.BitConverter.ToSingle(data, 0), System.BitConverter.ToSingle(data, 4), 
                stage == ExpeditionStage.WayOut ? gmap.cityPoint : destination, 
                FLY_SPEED);
            gmap.AddPoint(mapMarker, true);            
            //            
        }
        missionCompleted = fs.ReadByte() == 1;
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
                if (e != null && (e.stage != ExpeditionStage.Dismissed & e.stage != ExpeditionStage.Disappeared))
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
        fs.Write(System.BitConverter.GetBytes(expeditionsLaunched), 0, 4);
        fs.Write(System.BitConverter.GetBytes(expeditionsSucceed), 0, 4);
    }
    public static void LoadStaticData(System.IO.FileStream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, data.Length);
        int count = System.BitConverter.ToInt32(data, 0);
        if (count > MAX_EXPEDITIONS_COUNT)
        {
            Debug.Log("expeditions loading error - wrong count");
            GameMaster.LoadingFail();
            return;
        }
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
        data = new byte[12];
        fs.Read(data, 0, data.Length);
        nextID = System.BitConverter.ToInt32(data, 0);
        expeditionsLaunched = System.BitConverter.ToUInt32(data, 4);
        expeditionsSucceed = System.BitConverter.ToUInt32(data, 8);
    }
    #endregion
}

