using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition
{
    public enum MissionStageType : byte { Test, Decision, Fee, Crossroad, Special, Random }
    public enum ExpeditionStage : byte { Preparation, WayIn, WayOut, OnMission }

    public static List<Expedition> expeditionsList { get; private set; }
    public static int actionsHash { get; private set; }
    public static int expeditionsSucceed;

    public string name;
    public int ID { get; private set; }
    public static int lastUsedID {get;private set;} // в сохранение

    public float progress { get; private set;} // прогресс текущего шага
    public int currentStep { get; private set; }   
    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public Crew crew { get; private set; }

    private bool subscribedToUpdate;
    private float crewSpeed;    
    private const float ONE_STEP_WORKFLOW = 100;

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
    public static void DismissExpedition(int s_id)
    {
        if (expeditionsList.Count > 0)
        {
            for (int i = 0; i < expeditionsList.Count; i++)
            {
                if (expeditionsList[i].ID == s_id)
                {
                    Expedition e = expeditionsList[i];
                    if (e.crew != null) e.DismissCrew();
                    expeditionsList.RemoveAt(i);
                    actionsHash++;
                    break;
                }
            }
        }
    }

    public Expedition()
    {
        ID = lastUsedID;
        lastUsedID++;
        expeditionsList.Add(this);
        name = Localization.GetWord(LocalizedWord.Expedition) + ' ' + ID.ToString();
        stage = ExpeditionStage.Preparation;
        mission = Mission.NoMission;
    }
    public Expedition(int i_id)
    {
        ID = i_id;       
        expeditionsList.Add(this);
        mission = Mission.NoMission;
    }

    public void Launch(PointOfInterest poi)
    {
        if (stage == ExpeditionStage.Preparation)
        {
            if (GameMaster.realMaster.globalMap.ExpeditionLaunch(this, poi))
            {
                ChangeStage( ExpeditionStage.WayIn );
            }
        }
    }
    public void MissionStart()
    {
        if (stage != ExpeditionStage.OnMission)
        {
            ChangeStage(ExpeditionStage.OnMission);
            crewSpeed = mission.CalculateCrewSpeed(crew);
            if (!subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent += this.LabourUpdate;
                subscribedToUpdate = true;
            }
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
                    ChangeStage(ExpeditionStage.WayOut);
                    break;
                }
            case ExpeditionStage.OnMission:
                {
                    if (mission != Mission.NoMission)
                    {
                        if (mission.TryToLeave())
                        {
                            mission = Mission.NoMission;
                            progress = 0;
                            ChangeStage(ExpeditionStage.WayOut);
                        }
                    }
                    else ChangeStage(ExpeditionStage.WayOut);
                    break;
                }
        }
    }

    public void LabourUpdate()
    {
        switch (stage)
        {
            case ExpeditionStage.Preparation:
                {
                    break;
                }
            case ExpeditionStage.OnMission:
                {
                    progress += crewSpeed;
                    if (progress >= ONE_STEP_WORKFLOW)
                    {
                        progress = 0;
                        //тут должны быть тесты
                        currentStep++;
                        if (currentStep >= mission.stepsCount)
                        {
                            if (mission.TryToLeave())
                            {
                                mission = Mission.NoMission;
                                ChangeStage(ExpeditionStage.WayOut); // а как с наземными миссиями?
                            }
                        }
                    }
                    break;
                }
        }
    }

    public void SetMission(Mission m)
    {
        if (m == Mission.NoMission)
        {
            DropMission();
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
    public void DropMission()
    {
        if (mission != Mission.NoMission)
        {
            mission = Mission.NoMission;
            currentStep = 0;
            progress = 0;
            crewSpeed = 0;
        }
    }
    public void SetCrew(Crew c)
    {
        if (stage == ExpeditionStage.Preparation) crew = c;
        if (crew != null & stage == ExpeditionStage.OnMission & !subscribedToUpdate) {
            GameMaster.realMaster.labourUpdateEvent += this.LabourUpdate;
            subscribedToUpdate = true;
        }
    }
    public void DismissCrew()
    {
        if (mission == Mission.NoMission & crew != null)
        {
            crew.SetStatus(CrewStatus.Free);
            crew = null;
            if (subscribedToUpdate)
            {
                GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
                subscribedToUpdate = false;
            }
        }
    }

    public void DrawTexture(UnityEngine.UI.RawImage iconPlace)
    {
        //awaiting
    }

    private void ChangeStage(ExpeditionStage nstage)
    {
        //добавить проверки
        stage = nstage;
        actionsHash++;
    }

    private void OnDestroy()
    {
        if (subscribedToUpdate & !GameMaster.sceneClearing)
        {
            GameMaster.realMaster.labourUpdateEvent -= this.LabourUpdate;
            subscribedToUpdate = false;
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

        data = new byte[15];
        stage = (ExpeditionStage)data[0];
        int crewID = System.BitConverter.ToInt32(data, 1);
        if (crewID > 0) crew = Crew.GetCrewByID(crewID);
        else crew = null;
        mission = new Mission((MissionType)data[5], data[6]);
        progress = System.BitConverter.ToSingle(data, 7);
        currentStep = System.BitConverter.ToInt32(data, 11);
        return this;
    }

    public static void SaveStaticData(System.IO.FileStream fs)
    {
        Debug.Log(lastUsedID);
        fs.Write(System.BitConverter.GetBytes(lastUsedID), 0, 4);

        int count = expeditionsList.Count;
        if (count == 0) fs.Write(System.BitConverter.GetBytes(count),0,4);
        else
        {
            count = 0;
            var data = new List<byte>();
            while (count < expeditionsList.Count)
            {
                if (expeditionsList[count] == null)
                {
                    expeditionsList.RemoveAt(count);
                    continue;
                }
                else
                {
                    data.AddRange(expeditionsList[count].Save());
                    count++;
                }
            }
            fs.Write(System.BitConverter.GetBytes(count), 0, 4);
            if (count > 0) {
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
        int count = System.BitConverter.ToInt32(data,4);
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

