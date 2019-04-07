using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition
{
    public enum ExpeditionStage : byte { WayIn, WayOut, OnMission, LeavingMission, Dismissed }

    public static List<Expedition> expeditionsList { get; private set; }
    public static int actionsHash { get; private set; }
    public static int expeditionsSucceed;

    public string name;
    public readonly int ID;
    public static int lastUsedID { get; private set; } // в сохранение

    public bool hasConnection { get; private set; } // есть ли связь с центром
    public float progress { get; private set; } // прогресс текущего шага
    public int currentStep { get; private set; }
    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public PointOfInterest destination { get; private set; }
    public Crew crew;

    private bool subscribedToUpdate, missionCompleted = false;
    private float crewSpeed;
    private FlyingExpedition mapMarker;
    private QuantumTransmitter transmitter;
    private const float ONE_STEP_WORKFLOW = 100;

    // STATIC & equals
    static Expedition()
    {
        expeditionsList = new List<Expedition>();
        lastUsedID = 0;
    }
    public static void GameReset()
    {
        expeditionsList = new List<Expedition>();
        actionsHash = 0;
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
    public static Expedition CreateNewExpedition(Crew i_crew, Mission i_mission, QuantumTransmitter i_transmitter, PointOfInterest i_destination, string i_name)
    {
        if (i_crew == null | i_mission == Mission.NoMission | i_transmitter == null | i_destination == null) return null;
        else
        {
            if (i_crew.status != CrewStatus.OnMission | i_crew.shuttle == null | i_transmitter.expeditionID != -1 | i_destination.sentExpedition != null) return null;
            else
            {
                return new Expedition(i_crew, i_mission, i_transmitter, i_destination, i_name);
            }
        }
    }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Expedition e = (Expedition)obj;
        return (ID == e.ID);
    }

    /// ===============================
    private Expedition(Crew i_crew, Mission i_mission, QuantumTransmitter i_qt, PointOfInterest i_destination, string i_name)
    {
        ID = lastUsedID;
        lastUsedID++;
        name = i_name;
        missionCompleted = false;
        stage = ExpeditionStage.WayIn;
        crew = i_crew; crew.SetStatus(CrewStatus.OnMission);
        mission = i_mission;
        transmitter = i_qt; transmitter.AssignExpedition(this);
        destination = i_destination; destination.sentExpedition = this;
        hasConnection = true;
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        mapMarker = new FlyingExpedition(this, gmap.GetCityPoint(), destination, Shuttle.SPEED);
        gmap.AddPoint(mapMarker, true);
        expeditionsList.Add(this);
    }
    private Expedition(int i_id) // for loading only
    {
        ID = i_id;
        expeditionsList.Add(this);
    }
    public void MissionStart()
    {
        if (stage != ExpeditionStage.OnMission)
        {
            stage = ExpeditionStage.OnMission;
            crewSpeed = mission.CalculateCrewSpeed(crew);
            if (!subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent += this.LabourUpdate;
                subscribedToUpdate = true;
            }
            actionsHash++;
        }
    }
    public void EndMission()
    {
        switch (stage)
        {
            case ExpeditionStage.WayIn:
                {
                    mission = Mission.NoMission;
                    progress = 0;
                    stage = ExpeditionStage.WayOut;
                    mapMarker.ChangeDestination(GameMaster.realMaster.globalMap.GetCityPoint());
                    actionsHash++;
                    break;
                }
            case ExpeditionStage.OnMission:
                {
                    if (mission.TryToLeave())
                    {
                        LeaveSuccessful();
                    }
                    else
                    {
                        stage = ExpeditionStage.LeavingMission;
                    }
                    actionsHash++;
                    break;
                }
        }
    }
    private void LeaveSuccessful()
    {
        mission = Mission.NoMission;
        progress = 0;
        currentStep = 0;
        stage = ExpeditionStage.WayOut;
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        mapMarker = new FlyingExpedition(this, destination, gmap.GetCityPoint(), Shuttle.SPEED);
        gmap.AddPoint(mapMarker, true);
        actionsHash++;
    }

    /// <summary>
    /// returns true if expedition dissappeared in process, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool SectorCollapsingTest() // INDEV
    {
        return false;
    }

    public void LabourUpdate()
    {
        switch (stage)
        {
            case ExpeditionStage.OnMission:
                {
                    progress += crewSpeed;
                    if (progress >= ONE_STEP_WORKFLOW)
                    {
                        progress = 0;                        
                        bool success = false;
                        //
                        int x = Random.Range(0, 8);
                        switch (x)
                        {
                            //ignore 0 -> default
                            case 1: // mission test
                                if (mission.TestYourMight(crew)) success = true;
                                break;
                            case 2: // jump
                                if (Random.value > crew.luck * crew.perception)
                                {
                                    currentStep--;
                                    if (Random.value * crew.adaptability < destination.location.difficulty * (1 - destination.friendliness))
                                    {
                                        currentStep--;
                                        crew.LowConfidence(); // уверенность падает из-за неудач
                                    }
                                }
                                else
                                {
                                    success = true;
                                    if (Random.value * crew.luck / destination.location.difficulty < destination.friendliness) currentStep++;
                                }
                                break;
                            case 3: //suffer
                                if (crew.HardTest(destination.location.difficulty)) success = true;
                                else
                                {
                                    crew.LoseMember();
                                    if (crew == null)
                                    {
                                        Disappear();
                                        return;
                                    }
                                }
                                break;
                            case 4: // prize
                                if (Random.value < destination.friendliness * crew.luck * crew.perception)
                                {
                                    destination.location.TakeTreasure(crew);
                                }
                                success = true;
                                break;
                            case 5: //event
                                success = true;
                                if (Random.value < destination.danger) {
                                    var gmap = GameMaster.realMaster.globalMap;
                                    gmap.UpdateSector(gmap.DefineSectorIndex(destination.angle, destination.ringIndex));
                                }
                                else
                                {
                                    if (Random.value < 0.01f)
                                    {
                                        if (crew.artifact != null)
                                        {
                                            if (crew.artifact.Event() == false) crew.DropArtifact();
                                        }
                                    }
                                }
                                break;
                            case 6: //paradise ?
                                success = true;
                                if (crew.SoftCheck(destination.friendliness))
                                {
                                    Disappear();
                                    return;
                                }
                                break;
                            case 7: // silence - nothing happens

                                break;
                            case 8: // connection
                                hasConnection = !hasConnection;
                                break;
                            default:
                                success = true;
                                break;
                        }

                        crew.StaminaCheck();
                        if (success)
                        {
                            currentStep++;
                            if (!destination.explored)
                            {
                                destination.Explore(mission.type == MissionType.Exploring ? 2f : 1f);
                            }
                        }
                        //                        
                        if (currentStep >= mission.stepsCount)
                        {
                            missionCompleted = true;
                            EndMission();
                        }
                    }
                    break;
                }
            case ExpeditionStage.LeavingMission:
                {
                    progress += crewSpeed;
                    if (progress >= ONE_STEP_WORKFLOW)
                    {
                        progress = 0;
                        if (mission.TryToLeave())
                        {
                            LeaveSuccessful();
                        }
                    }
                    break;
                }
        }
    }

    public void ChangeMission(Mission m)
    {
        if (m == Mission.NoMission)
        {
            EndMission();
            return;
        }
        else
        {
            mission = m;
            currentStep = 0;
            progress = 0;
            crewSpeed = 0;
            actionsHash++;
        }
    }

    public void DrawTexture(UnityEngine.UI.RawImage iconPlace) // INDEV
    {
    }
    public void SetConnection(bool connected)
    {
        if (hasConnection != connected)
        {
            hasConnection = connected;
            GameMaster.realMaster.globalMap.MarkToUpdate();
        }
    }

    public void Dismiss()
    {        
        //зависимость : Disappear()
        if (stage == ExpeditionStage.Dismissed) return;
        else
        {
            if (crew != null) crew.SetStatus(CrewStatus.AtHome);
            if (transmitter != null) transmitter.DropExpeditionConnection();
            if (destination != null && destination.sentExpedition != null && destination.sentExpedition.ID == ID) destination.sentExpedition = null;
            if (subscribedToUpdate & !GameMaster.sceneClearing)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
            if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            stage = ExpeditionStage.Dismissed;
        }
    }
    public void Disappear() // INDEV
    {
        if (stage == ExpeditionStage.Dismissed) return;
        else
        {
            if (crew != null) crew.Disappear();
            if (transmitter != null) transmitter.DropExpeditionConnection();
            if (destination != null && destination.sentExpedition != null && destination.sentExpedition.ID == ID) destination.sentExpedition = null;
            if (subscribedToUpdate & !GameMaster.sceneClearing)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
            if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            stage = ExpeditionStage.Dismissed;
        }
    }
    #region save-load system
    public List<byte> Save()
    {
        //awaiting
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(ID));

        var nameArray = System.Text.Encoding.Default.GetBytes(name);
        int bytesCount = nameArray.Length;
        data.AddRange(System.BitConverter.GetBytes(bytesCount));
        if (bytesCount > 0) data.AddRange(nameArray);

        data.Add((byte)stage); // 0
        int crewID = -1; if (crew != null) crewID = crew.ID;
        data.AddRange(System.BitConverter.GetBytes(crewID)); // 1 - 4
        data.AddRange(mission.Save());  // 5 - 6
        data.AddRange(System.BitConverter.GetBytes(progress)); // 7 - 10
        data.AddRange(System.BitConverter.GetBytes(currentStep)); // 11 - 14
        int usingTransmitterID = (transmitter != null) ? transmitter.connectionID : -1;
        data.AddRange(System.BitConverter.GetBytes(usingTransmitterID)); // 15 - 18
        data.AddRange(System.BitConverter.GetBytes(destination.ID)); // 19 - 22
        byte zero = 0, one = 1;
        if (missionCompleted) data.Add(one); else data.Add(zero); // 23
        if (stage == ExpeditionStage.WayIn | stage == ExpeditionStage.WayOut)
        {
            data.AddRange(System.BitConverter.GetBytes(mapMarker.angle)); // 24 - 27
            data.AddRange(System.BitConverter.GetBytes(mapMarker.height));//28 - 31
        }
        return data;
    }
    public Expedition Load(System.IO.FileStream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int bytesCount = System.BitConverter.ToInt32(data, 0);
        if (bytesCount > 0)
        {
            data = new byte[bytesCount];
            fs.Read(data, 0, bytesCount);
            System.Text.Decoder d = System.Text.Encoding.Default.GetDecoder();
            var chars = new char[d.GetCharCount(data, 0, bytesCount)];
            d.GetChars(data, 0, bytesCount, chars, 0, true);
            name = new string(chars);
        }
        else name = Localization.GetWord(LocalizedWord.Expedition) + ' ' + ID.ToString();
        data = new byte[24];
        fs.Read(data, 0, data.Length);
        stage = (ExpeditionStage)data[0];
        int crewID = System.BitConverter.ToInt32(data, 1);
        if (crewID != - 1) crew = Crew.GetCrewByID(crewID);
        else crew = null;
        mission = new Mission((MissionType)data[5]);
        progress = System.BitConverter.ToSingle(data, 7);
        currentStep = System.BitConverter.ToInt32(data, 11);
        int usingTransmitterID = System.BitConverter.ToInt32(data, 15);
        if (usingTransmitterID != -1)
        {
            transmitter = QuantumTransmitter.GetTransmitterByID(usingTransmitterID);
            transmitter.AssignExpedition(this);
        }
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        destination = gmap.GetMapPointByID(System.BitConverter.ToInt32(data, 19)) as PointOfInterest;
        if (stage == ExpeditionStage.WayIn | stage == ExpeditionStage.WayOut) {
            data = new byte[8];
            fs.Read(data, 0, data.Length);
            if (stage == ExpeditionStage.WayIn) {
                mapMarker = new FlyingExpedition(this, gmap.GetCityPoint(), destination, Shuttle.SPEED);
            }
            else
            {
                mapMarker = new FlyingExpedition(this, destination, gmap.GetCityPoint(), Shuttle.SPEED);
            }
            mapMarker.SetCoords(System.BitConverter.ToSingle(data,0), System.BitConverter.ToSingle(data, 4));
        }
        else
        {
            destination.sentExpedition = this;
        }
        missionCompleted = (data[23] == 1);
        return this;
    }

    public static void SaveStaticData(System.IO.FileStream fs)
    {
        fs.Write(System.BitConverter.GetBytes(lastUsedID), 0, 4);

        int count = expeditionsList.Count;
        if (count == 0) fs.Write(System.BitConverter.GetBytes(count), 0, 4);
        else
        {
            count = 0;
            var data = new List<byte>();
            foreach (Expedition e in expeditionsList)
            {
                if (e != null)
                {
                    data.AddRange(e.Save());
                    count++;
                }
            }
            fs.Write(System.BitConverter.GetBytes(count), 0, 4);
            if (count > 0)
            {
                var dataArray = data.ToArray();
                fs.Write(dataArray, 0, dataArray.Length);
            }
        }


        fs.Write(System.BitConverter.GetBytes(expeditionsSucceed), 0, 4);
    }
    public static void LoadStaticData(System.IO.FileStream fs)
    {
        var data = new byte[8];
        fs.Read(data, 0, 8);
        lastUsedID = System.BitConverter.ToInt32(data, 0);
        int count = System.BitConverter.ToInt32(data, 4);

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
        fs.Read(data, 0, 4);
        expeditionsSucceed = System.BitConverter.ToInt32(data, 0);
    }
    #endregion
}

