using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition
{
    public enum ExpeditionStage : byte { WayIn, WayOut, OnMission, LeavingMission, Dismissed }
    public enum ExpeditionLogMessage : ushort { ContinueMission, TestPassed, TestFailed, WrongPath, FastPath, LoseConfidence, HardtestPassed, MemberLost,
    HardDisappear, TreasureFound, GlobalMapChanged, ArtifactLost, SoftDisappear, ConnectionLost, ConnectionRestored, TaskCompleted, Disapproval, NoStamina, RestOnMission,
    MissionStart, MissionLeaveFail, StopMission, ReturningHome, CrystalsFound, Approval, Dismissing}

    public static List<Expedition> expeditionsList { get; private set; }
    public static int actionsHash { get; private set; }
    public static int expeditionsSucceed;
    public static UIExpeditionObserver observer { get; private set; }
    public static int nextID { get; private set; } // в сохранение

    public string name;
    public readonly int ID;    

    public bool hasConnection { get; private set; } // есть ли связь с центром
    public float progress { get; private set; } // прогресс текущего шага
    public int currentStep { get; private set; }
    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public PointOfInterest destination { get; private set; }
    public Crew crew { get; private set; }
    public Texture icon { get; private set; } 

    private bool subscribedToUpdate;
    private float crewSpeed, collectedMoney,currentDistanceToTarget, distanceToTarget;
    private ExpeditionLogMessage[][] log;
    private FlyingExpedition mapMarker;
    private QuantumTransmitter transmitter;

    public const float ONE_STEP_WORKFLOW = 100, ONE_STEP_XP = 0.5f;
    private const float ONE_STEP_STAMINA = 0.005f, ONE_STEP_TO_TARGET = 0.1f;
    // STATIC & equals
    static Expedition()
    {
        expeditionsList = new List<Expedition>();
        nextID = 0;
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
        if (i_crew == null | i_mission == null | i_transmitter == null | i_destination == null) return null;
        else
        {
            if (i_crew.status != CrewStatus.AtHome | i_crew.shuttle == null | i_transmitter.expeditionID != -1) return null;
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
        ID = nextID;
        nextID++;
        name = i_name;
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
             MonoBehaviour.print(crewSpeed);
            if (!subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent += this.LabourUpdate;
                subscribedToUpdate = true;
            }
            distanceToTarget = mission.GetDistanceToTarget();
            currentDistanceToTarget = distanceToTarget * (0.3f + crew.luck * 0.2f) * Random.value;
            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.MissionStart);
            actionsHash++;
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
                    actionsHash++;
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
                    actionsHash++;
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
                    if (crew.stamina > 0)
                    {
                        crew.ConsumeStamina(destination.difficulty * ONE_STEP_STAMINA);
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
                                    if (Random.value > crew.luck * crew.perception)
                                    {
                                        currentDistanceToTarget -= ONE_STEP_TO_TARGET;
                                        //дополнительно
                                        if (destination.TryToJump(crew))
                                        {
                                            currentDistanceToTarget -= ONE_STEP_TO_TARGET;
                                            crew.LoseConfidence(); // уверенность падает из-за неудач
                                            if (hasConnection) {
                                                if (Random.value < crew.loyalty) AddMessageToLog(new ExpeditionLogMessage[] { ExpeditionLogMessage.WrongPath, ExpeditionLogMessage.LoseConfidence });
                                                else AddMessageToLog(ExpeditionLogMessage.WrongPath);
                                            }
                                        }
                                        else
                                        {
                                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.WrongPath);
                                        }
                                    }
                                    else
                                    {
                                        success = true;
                                        if (destination.TryAdditionalJump(crew)) currentDistanceToTarget += ONE_STEP_TO_TARGET;
                                        if (crew.persistence > Random.value) currentDistanceToTarget += ONE_STEP_TO_TARGET;
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.FastPath);
                                    }
                                    break;
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
                                            Disappear();
                                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.HardDisappear);
                                            return;
                                        }
                                        else
                                        {
                                            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.MemberLost);
                                        }
                                    }
                                    break;
                                case 4: // prize
                                    if (destination.TryTakeTreasure(crew))
                                    {
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.TreasureFound);
                                    }
                                    else if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ContinueMission);
                                    success = true;
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
                                        Disappear();
                                        if (hasConnection) AddMessageToLog(ExpeditionLogMessage.SoftDisappear);
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
                                crew.AddExperience((0.5f + destination.difficulty) * ONE_STEP_XP);
                                distanceToTarget += ONE_STEP_TO_TARGET;
                                if (distanceToTarget >= 1)
                                {
                                    crew.AddExperience(ONE_STEP_XP);
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
                                        currentDistanceToTarget = mission.GetDistanceToTarget() * (0.9f + crew.luck * 0.1f);
                                    }
                                }
                            }
                            else
                            {
                                if (!destination.LoyaltyTest(crew))
                                {
                                    crew.LoseLoyalty();
                                    if (hasConnection &&  Random.value < crew.loyalty) AddMessageToLog(ExpeditionLogMessage.Disapproval);
                                }
                                if (!destination.AdaptabilityTest(crew))
                                {
                                    crew.LoseConfidence();
                                    if (hasConnection && Random.value < crew.loyalty) AddMessageToLog(ExpeditionLogMessage.LoseConfidence);
                                }
                            }
                            //                        
                            if (currentStep >= mission.stepsCount)
                            {
                                EndMission();
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (!destination.StaminaTest(crew))
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
        switch (mission.preset.type)
        {
            case MissionType.Awaiting: return false;
            case MissionType.Exploring: crew.IncreaseAdaptability(); return false;
            case MissionType.FindingKnowledge: crew.ImproveNativeParameters(); return false;
            case MissionType.FindingItem:
            case MissionType.FindingPerson:
            case MissionType.FindingPlace: crew.AddExperience(ONE_STEP_XP); return true;
            case MissionType.FindingResources: destination.TakeTreasure(crew); return false;
            case MissionType.FindingEntrance: crew.AddExperience(ONE_STEP_XP); return false;
            case MissionType.FindingExit: crew.AddExperience(ONE_STEP_XP); return true;
            default: return false;
        }
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
            actionsHash++;
        }
    } 
    public void SetConnection(bool connected)
    {
        if (hasConnection != connected)
        {
            hasConnection = connected;
            if (!hasConnection) crew.LoseConfidence();
            else crew.IncreaseConfidence();
            GameMaster.realMaster.globalMap.MarkToUpdate();
            if (hasConnection) AddMessageToLog(ExpeditionLogMessage.ConnectionRestored);
            else AddMessageToLog(ExpeditionLogMessage.ConnectionLost);
        }
    }
    public void CollectMoney(float f) {
        collectedMoney += f;
        crew.IncreaseLoyalty();
        if (hasConnection) {
            AddMessageToLog(ExpeditionLogMessage.CrystalsFound);
            if (Random.value < crew.loyalty) AddMessageToLog(ExpeditionLogMessage.Approval);
        }
    }

    public void AddMessageToLog(ExpeditionLogMessage[] msg)
    {
        if (GameMaster.loading) return;
        if (log == null) log = new ExpeditionLogMessage[1][];
        else
        {
            if (log.Length == 10)
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
                int n = log.Length;
                var lg2 = new ExpeditionLogMessage[n + 1][];
                for (int i = 0; i < n; i++)
                {
                    lg2[i] = log[i];
                }
                lg2[n] = msg;
                log = lg2;
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
            AddMessageToLog(new ExpeditionLogMessage[] { ExpeditionLogMessage.Dismissing, crew.loyalty > 0.5 ? ExpeditionLogMessage.Approval : ExpeditionLogMessage.Disapproval});
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
        // texture?
        data.AddRange(System.BitConverter.GetBytes(crewSpeed)); // 34 - 37
        data.AddRange(System.BitConverter.GetBytes(collectedMoney)); // 38 - 41

        if (mapMarker != null) { // 42
            data.Add(1);
            data.AddRange(mapMarker.Save());
        }
        else data.Add(0);
        
        return data;
    }
    public Expedition Load(System.IO.FileStream fs)
    {
        int LENGTH = 43;
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

        if (data[42] == 1)
        {
            FlyingExpedition.LoadExpeditionMarker(fs, this);
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

