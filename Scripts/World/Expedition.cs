using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition
{
    public enum ExpeditionStage : byte { WayIn, WayOut, OnMission, LeavingMission, Dismissed }
    public enum ExpeditionLogMessage : ushort { Empty, ContinueMission, TestPassed, TestFailed, WrongPath, FastPath, LoseConfidence, HardtestPassed, MemberLost,
    HardDisappear, TreasureFound, GlobalMapChanged, ArtifactLost, SoftDisappear, ConnectionLost, ConnectionRestored, TaskCompleted, Disapproval, NoStamina, RestOnMission,
    MissionStart, MissionChanged, MissionLeaveFail, StopMission, ReturningHome, CrystalsFound, Approval, Dismissing}
    //dependence : Localization.GetExpeditionLogMessage

    public static List<Expedition> expeditionsList { get; private set; }
    public static int listChangesMarker { get; private set; }
    public static int expeditionsSucceed;
    public static UIExpeditionObserver observer { get; private set; }
    public static int nextID { get; private set; } // в сохранение

    public readonly int ID;    

    public bool hasConnection { get; private set; } // есть ли связь с центром
    public float progress { get; private set; } // прогресс текущего шага
    public int currentStep { get; private set; }
    public byte changesMarkerValue { get; private set; }
    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public PointOfInterest destination { get; private set; }
    public Crew crew { get; private set; }

    private bool subscribedToUpdate;
    private byte lastLogIndex = 0;
    private sbyte advantages = 0;
    private float crewSpeed, collectedMoney,currentDistanceToTarget, distanceToTarget;
    private ushort[][] log;
    private FlyingExpedition mapMarker;
    private QuantumTransmitter transmitter;

    public const float ONE_STEP_WORKFLOW = 100, ONE_STEP_XP = 5f;
    private const float ONE_STEP_TO_TARGET = 0.1f;
    private const byte LOGS_COUNT = 10;
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
    public static Expedition CreateNewExpedition(Crew i_crew, Mission i_mission, QuantumTransmitter i_transmitter, PointOfInterest i_destination, string i_name)
    {
        if (i_crew == null | i_mission == null | i_transmitter == null | i_destination == null) return null;
        else
        {
            if (i_crew.status != CrewStatus.AtHome | i_crew.shuttle == null | i_transmitter.expeditionID != -1) return null;
            else
            {
                return new Expedition(i_crew, i_mission, i_transmitter, i_destination);
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
    private Expedition(Crew i_crew, Mission i_mission, QuantumTransmitter i_qt, PointOfInterest i_destination)
    {
        ID = nextID;
        nextID++;
        stage = ExpeditionStage.WayIn;
        crew = i_crew; crew.SetStatus(CrewStatus.OnMission);
        mission = i_mission;
        transmitter = i_qt; transmitter.AssignExpedition(this);
        destination = i_destination; destination.ListAnExpedition(this);
        hasConnection = true;
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        mapMarker = new FlyingExpedition(this, gmap.GetCityPoint(), destination, Shuttle.SPEED);
        gmap.AddPoint(mapMarker, true);
        expeditionsList.Add(this);
        listChangesMarker++;
        changesMarkerValue = 1;
    }
    private Expedition(int i_id) // for loading only
    {
        ID = i_id;
        expeditionsList.Add(this);
        changesMarkerValue = 1;
        listChangesMarker++;
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
            distanceToTarget = mission.GetDistanceToTarget();
            currentDistanceToTarget = distanceToTarget * (0.9f + 0.5f * Random.value);
            advantages = 0;
            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.MissionStart);
        }
    }
    public void EndMission()
    {
        switch (stage)
        {
            case ExpeditionStage.WayIn:
                {
                    // # mission drop
                    Mission.RemoveMission(mission.ID);
                    mission = null;
                    progress = 0;
                    currentStep = 0;
                    stage = ExpeditionStage.WayOut;
                    //
                    mapMarker.ChangeDestination(GameMaster.realMaster.globalMap.GetCityPoint());
                    if (hasConnection) { AddMessageToLog(new ExpeditionLogMessage[] { ExpeditionLogMessage.StopMission, ExpeditionLogMessage.ReturningHome }); }
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
                        AddMessageToLog(ExpeditionLogMessage.MissionLeaveFail);
                    }
                    break;
                }
        }
        currentDistanceToTarget = 0;
    }
    private void LeaveSuccessful()
    {
        // # mission drop
        Mission.RemoveMission(mission.ID);
        mission = null;
        progress = 0;
        currentStep = 0;
        stage = ExpeditionStage.WayOut;
        //
        GlobalMap gmap = GameMaster.realMaster.globalMap;
        mapMarker = new FlyingExpedition(this, destination, gmap.GetCityPoint(), Shuttle.SPEED);
        gmap.AddPoint(mapMarker, true);
        SetConnection(true);
        AddMessageToLog(ExpeditionLogMessage.ReturningHome);
        listChangesMarker++;
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
                    float stepVal = crew.attributes.MakeStep(destination.difficulty);
                    if (stepVal > 0)
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
                                    if (mission.TestYourMight(crew))
                                    {
                                        success = true;
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.TestPassed);
                                    }
                                    else
                                    {
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.TestFailed);
                                    }
                                    break;
                                case 2: // jump            
                                    {
                                        if (destination.TryToJump(crew))
                                        {
                                            success = true;
                                            currentDistanceToTarget -= ONE_STEP_TO_TARGET; // дополнительно
                                            if (destination.TryAdditionalJump(crew))
                                            {
                                                currentDistanceToTarget -= ONE_STEP_TO_TARGET;
                                                if (crew.attributes.PersistenceRoll() > destination.difficulty * 25f) currentDistanceToTarget -= ONE_STEP_TO_TARGET;
                                            }
                                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.FastPath);
                                        }
                                        else
                                        {
                                            if (destination.TrueWayTest(crew)) success = true;
                                            else
                                            {
                                                currentDistanceToTarget += 2 * ONE_STEP_TO_TARGET;
                                                if (destination.difficulty * 15f > crew.attributes.UnityRoll())
                                                {
                                                    crew.attributes.LoseConfidence(1f);
                                                    if (hasConnection) AddMessageToLog(new ExpeditionLogMessage[] { ExpeditionLogMessage.WrongPath, ExpeditionLogMessage.LoseConfidence });
                                                }
                                                else
                                                {
                                                    if (hasConnection) AddMessageToLog(ExpeditionLogMessage.WrongPath);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case 3: //suffer
                                    if (destination.HardTest(crew))
                                    {
                                        success = true;
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.HardtestPassed);
                                    }
                                    else
                                    {
                                        crew.LoseMember();
                                        if (crew == null)
                                        {
                                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.HardDisappear);
                                            Disappear();                                            
                                            return;
                                        }
                                        else
                                        {
                                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.MemberLost);
                                        }
                                    }
                                    break;
                                case 4: // prize
                                    success = true;
                                    if (destination.TreasureFinding(crew))
                                    {
                                        destination.TakeTreasure(crew);
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.TreasureFound);
                                    }
                                    else if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ContinueMission);                                    
                                    break;
                                case 5: //event
                                    success = true;
                                    if (destination.IsSomethingChanged())
                                    {
                                        var gmap = GameMaster.realMaster.globalMap;
                                        gmap.UpdateSector(gmap.DefineSectorIndex(destination.angle, destination.ringIndex));
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.GlobalMapChanged);
                                    }
                                    else
                                    {
                                        if (Random.value < 0.01f)
                                        {
                                            if (crew.artifact != null)
                                            {
                                                if (crew.artifact.Event() == false)
                                                {
                                                    crew.DropArtifact();
                                                    if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ArtifactLost);
                                                    break;
                                                }
                                            }
                                        }
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ContinueMission);
                                    }
                                    break;
                                case 6: //paradise ?
                                    success = true;
                                    if (!destination.SoftTest(crew))
                                    {
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.SoftDisappear);
                                        Disappear();                                        
                                        return;
                                    }
                                    if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ContinueMission);
                                    break;
                                case 7: 
                                    // silence - nothing happens
                                    break;
                                case 8: // connection
                                    SetConnection(!hasConnection);
                                    break;
                                default:
                                    success = true;
                                    if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ContinueMission);
                                    break;
                            }

                            if (success)
                            {
                                currentStep++;
                                if (destination.exploredPart < 1f)
                                {
                                    destination.Explore(mission.preset.type == MissionType.Exploring ? 2f : 1f);
                                }
                                else
                                {
                                    if (mission.preset.type == MissionType.Exploring)
                                    {                                       
                                        EndMission();
                                        break;
                                    }
                                }
                                crew.attributes.AddExperience((0.5f + destination.difficulty) * ONE_STEP_XP);
                                distanceToTarget -= ONE_STEP_TO_TARGET;
                                if (distanceToTarget <= 0f)
                                {
                                    crew.attributes.AddExperience(ONE_STEP_XP * 5f);
                                    if (Result())
                                    {
                                        if (hasConnection)
                                        {
                                            GameLogUI.MakeAnnouncement(Localization.GetCrewAction(LocalizedCrewAction.CrewTaskCompleted, crew));
                                            GameMaster.audiomaster.Notify(NotificationSound.CrewTaskCompleted);
                                            AddMessageToLog(ExpeditionLogMessage.TaskCompleted);
                                        }
                                        EndMission();
                                        break;
                                    }
                                    else
                                    {
                                        currentDistanceToTarget = mission.GetDistanceToTarget() * (0.9f + Random.value * 0.6f);
                                    }
                                }
                            }
                            else
                            {
                                if (!destination.LoyaltyTest(crew))
                                {
                                    crew.attributes.LoseLoyalty(0.25f);
                                    if (hasConnection &&  Random.value < crew.attributes.loyalty) AddMessageToLog(ExpeditionLogMessage.Disapproval);
                                }
                                if (!destination.AdaptabilityTest(crew))
                                {
                                    crew.attributes.LoseConfidence(0.25f);
                                    if (hasConnection && Random.value < crew.attributes.loyalty) AddMessageToLog(ExpeditionLogMessage.LoseConfidence);
                                }
                            }
                            //                        
                            if (currentStep >= mission.stepsCount)
                            {
                                EndMission();
                                break;
                            }
                            else crewSpeed = mission.CalculateCrewSpeed(crew);
                        }
                    }
                    else
                    {
                        if (stepVal == -1f)
                        {
                            if (hasConnection)
                            {
                                if (hasConnection)
                                {
                                    GameLogUI.MakeAnnouncement(Localization.GetCrewAction(LocalizedCrewAction.CannotCompleteMission, crew));
                                    AddMessageToLog(ExpeditionLogMessage.NoStamina);
                                }
                            }
                            EndMission();
                        }
                        else
                        {
                            crew.Rest(destination);
                            if (hasConnection)
                            {
                                AddMessageToLog(ExpeditionLogMessage.RestOnMission);
                            }
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
                        else
                        {
                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.MissionLeaveFail);
                        }
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// returns true if mission should be ended
    /// </summary>
    /// <returns></returns>
    public bool Result()
    {
        float exp = ONE_STEP_XP * 5f, adapt = 1f;
        bool end = false;
        switch (mission.preset.type)
        {
            case MissionType.Exploring: adapt += 1f; end = true; break;
            case MissionType.FindingKnowledge: adapt += 2f; exp *= 4f; end = true;  break;
            case MissionType.FindingItem:
            case MissionType.FindingPerson: end = true; break;
            case MissionType.FindingPlace: exp += Random.value * ONE_STEP_XP * 5f; end = true; break;
            case MissionType.FindingResources: destination.TakeTreasure(crew); break;
            case MissionType.FindingEntrance: exp += 0.5f * ONE_STEP_XP; break;
            case MissionType.FindingExit: exp += 0.5f * ONE_STEP_XP; end = true; break;
        }
        return end;
    }

    public void ChangeMission(Mission m)
    {
        if (m == null)
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
            AddMessageToLog(new ushort[] { (ushort)ExpeditionLogMessage.MissionChanged, (ushort)m.preset.type });
        }
    } 
    public void SetConnection(bool connected)
    {
        if (hasConnection != connected)
        {
            hasConnection = connected;
            if (!hasConnection) crew.attributes.LoseConfidence(1f);
            else crew.attributes.RaiseConfidence(1f);
            GameMaster.realMaster.globalMap.MarkToUpdate();
            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ConnectionRestored);
            else AddMessageToLog(ExpeditionLogMessage.ConnectionLost);
        }
    }
    public void CollectMoney(float f) {
        collectedMoney += f;
        crew.attributes.RaiseLoyalty(1f);
        if (hasConnection) {
            AddMessageToLog(ExpeditionLogMessage.CrystalsFound);
            if (Random.value < crew.attributes.loyalty) AddMessageToLog(ExpeditionLogMessage.Approval);
        }
    }
    public void FillLog(UnityEngine.UI.Text[] fields)
    {
        var s = string.Empty;
        if (log == null || log.Length == 0)
        {            
            fields[0].text = s;
            fields[1].text = s;
            fields[2].text = s;
            fields[3].text = s;
            fields[4].text = s;
            fields[5].text = s;
            fields[6].text = s;
            fields[7].text = s;
            fields[8].text = s;
            fields[9].text = s;
        }
        else
        {
            int i = 0;
            for (; i< lastLogIndex; i++)
            {
                fields[i].text = log[i][0] == 0 ? s : Localization.GetExpeditionLogMessage(this, log[i]);
            }
            if (i< LOGS_COUNT)
            {
                for (; i< LOGS_COUNT; i++)
                {
                    fields[i].text = s;
                }
            }
        }
    } 

    public void AddMessageToLog(ExpeditionLogMessage[] msg)
    {
        if (GameMaster.loading) return;
        if (log == null) log = new ushort[LOGS_COUNT][];
        else
        {
            if (lastLogIndex == LOGS_COUNT)
            {
                log[0] = log[1];
                log[1] = log[2];
                log[2] = log[3];
                log[3] = log[4];
                log[4] = log[5];
                log[5] = log[6];
                log[6] = log[7];
                log[7] = log[8];
                log[8] = log[9];
                var msgArray = new ushort[msg.Length];
                for (int i = 0; i < msg.Length; i++)
                {
                    msgArray[i] = (ushort)msg[i];
                }
                log[9] = msgArray;
            }
            else
            {
                var lg2 = new ushort[msg.Length];
                for (int i = 0; i < msg.Length; i++)
                {
                    lg2[i]= (ushort)msg[i];
                }
                log[lastLogIndex++] = lg2;
            }
        }
    }
    public void AddMessageToLog(ushort[] msg)
    {
        if (GameMaster.loading) return;
        if (log == null) log = new ushort[LOGS_COUNT][];
        else
        {
            if (lastLogIndex == LOGS_COUNT)
            {
                log[0] = log[1];
                log[1] = log[2];
                log[2] = log[3];
                log[3] = log[4];
                log[4] = log[5];
                log[5] = log[6];
                log[6] = log[7];
                log[7] = log[8];
                log[8] = log[9];                
                log[9] = msg;
            }
            else
            {
                log[lastLogIndex++] = msg;
            }
        }
    }
    public void AddMessageToLog(ExpeditionLogMessage msg)
    {
        AddMessageToLog(new ExpeditionLogMessage[] { msg });
    }

    public void ShowOnGUI(Rect r, SpriteAlignment alignment)
    {
        if (observer == null)
        {
            observer = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefs/expeditionPanel"), UIController.current.mainCanvas).GetComponent<UIExpeditionObserver>();
        }
        if (!observer.isActiveAndEnabled) observer.gameObject.SetActive(true);
        observer.SetPosition(r, alignment);
        observer.Show(this);
    }

    public void Dismiss() // экспедиция вернулась домой и распускается
    {        
        //зависимость : Disappear()
        if (stage == ExpeditionStage.Dismissed) return;
        else
        {
            if (crew != null) crew.SetStatus(CrewStatus.AtHome);
            if (transmitter != null) transmitter.DropExpeditionConnection();
            if (destination != null) destination.ExcludeExpeditionFromList(this);
            if (subscribedToUpdate & !GameMaster.sceneClearing)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
            if (expeditionsList.Contains(this)) expeditionsList.Remove(this);
            if (collectedMoney > 0)
            {
                GameMaster.realMaster.colonyController.AddEnergyCrystals(collectedMoney);
                collectedMoney = 0;
            }
            stage = ExpeditionStage.Dismissed;
            AddMessageToLog(new ExpeditionLogMessage[] { ExpeditionLogMessage.Dismissing, crew.attributes.loyalty > 0.5 ? ExpeditionLogMessage.Approval : ExpeditionLogMessage.Disapproval});
        }
    }
    public void Disappear() // INDEV
        // экспедиция исчезает
    {
        if (stage == ExpeditionStage.Dismissed) return;
        else
        {
            if (crew != null) crew.Disappear();
            if (transmitter != null) transmitter.DropExpeditionConnection();
            if (destination != null) destination.ExcludeExpeditionFromList(this);
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
        data.AddRange(System.BitConverter.GetBytes(ID)); // read before load
        data.Add(hasConnection ? (byte)1 : (byte)0); // 0
        data.Add((byte)stage); // 1
        data.AddRange(System.BitConverter.GetBytes(progress)); // 2 - 5
        data.AddRange(System.BitConverter.GetBytes(currentStep)); // 6 - 9
        data.AddRange(System.BitConverter.GetBytes(currentDistanceToTarget)); // 10 - 13
        data.AddRange(System.BitConverter.GetBytes(distanceToTarget)); // 14 - 17
        data.AddRange(System.BitConverter.GetBytes(mission.ID)); // 18 - 21
        data.AddRange(System.BitConverter.GetBytes(destination != null ? destination.ID : -1)); // 22 - 25
        data.AddRange(System.BitConverter.GetBytes(crew.ID)); // 26 -29
        data.AddRange(System.BitConverter.GetBytes(transmitter != null ? transmitter.connectionID : -1)); // 30 - 33
        data.AddRange(System.BitConverter.GetBytes(crewSpeed)); // 34 - 37
        data.AddRange(System.BitConverter.GetBytes(collectedMoney)); // 38 - 41
        // logs writing
        if (log == null) data.AddRange(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        else
        {
            for (int i = 0; i < LOGS_COUNT; i++)
            {
                var l = (byte)log[i].Length;
                data.Add(l);
                if (l != 0)
                {
                    foreach (ushort x in log[i])
                    {
                        data.AddRange(System.BitConverter.GetBytes(x));
                    }
                }
            }
        }
        // map marker data
        if (mapMarker != null)
        { 
            data.Add(1);
            data.AddRange(mapMarker.Save());
        }
        else data.Add(0);
        return data;
    }
    public Expedition Load(System.IO.FileStream fs)
    {
        int LENGTH = 42;
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        hasConnection = data[0] == 1;
        stage = (ExpeditionStage)data[1];
        progress = System.BitConverter.ToSingle(data, 2);
        currentStep = System.BitConverter.ToInt32(data, 6);
        currentDistanceToTarget = System.BitConverter.ToSingle(data, 10);
        distanceToTarget = System.BitConverter.ToSingle(data, 14);

        mission = Mission.GetMissionByID(System.BitConverter.ToInt32(data, 18));        
        destination = GameMaster.realMaster.globalMap.GetMapPointByID(System.BitConverter.ToInt32(data, 22)) as PointOfInterest;
        if (destination != null) destination.ListAnExpedition(this);
        crew = Crew.GetCrewByID(System.BitConverter.ToInt32(data, 26));
        if (crew != null) crew.SetCurrentExpedition(this); else Debug.Log("expedition load error - no crew");
        transmitter = QuantumTransmitter.GetTransmitterByConnectionID(System.BitConverter.ToInt32(data, 30));
        if (transmitter != null) transmitter.AssignExpedition(this);

        crewSpeed = System.BitConverter.ToSingle(data, 34);
        collectedMoney = System.BitConverter.ToSingle(data, 38);

        log = new ushort[LOGS_COUNT][];
        int l = 0, j = 0;
        lastLogIndex = 10;
        for (byte i = 0; i< LOGS_COUNT;i++)
        {
            l = fs.ReadByte();
            log[i] = new ushort[l];
            if (l > 0)
            {
                data = new byte[l * 2];
                fs.Read(data, 0, data.Length);
                for (j = 0; j< l;j++)
                {
                    log[i][j] = System.BitConverter.ToUInt16(data, j * 2);
                }
            }
            else
            {
                if (i < lastLogIndex) lastLogIndex = i;
            }
        }

        if (fs.ReadByte() == 1)
        {
            mapMarker =  FlyingExpedition.LoadExpeditionMarker(fs, this);
        }
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

