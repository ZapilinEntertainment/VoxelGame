using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition : MonoBehaviour
{
    public enum MissionStageType : byte { Test, Decision, Fee, Crossroad, Special, Random }
    public enum ExpeditionStage : byte { Preparation, WayIn, WayOut, OnMission }

    public static List<Expedition> expeditionsList { get; private set; }
    public static int expeditionsFinished, expeditionsSucceed;
    public static int actionsHash { get; private set; }
    private static Transform expeditionsContainer;

    public int ID { get; private set; }
    public static int lastUsedID {get;private set;} // в сохранение
    public int fuelNeeded { get; private set; }

    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public Crew crew { get; private set; }

    private bool subscribedToUpdate;
    private float crewSpeed, progress; // до следующего шага

    private const float ONE_STEP_WORKFLOW = 100;

    static Expedition()
    {
        expeditionsList = new List<Expedition>();
    }
    public static void GameReset()
    {
        expeditionsList = new List<Expedition>();
        expeditionsFinished = 0;
        expeditionsSucceed = 0;
        actionsHash = 0;
    }
    public static Expedition CreateNewExpedition()
    {
        Expedition e = new GameObject(Localization.GetWord(LocalizedWord.Expedition) + lastUsedID.ToString()).AddComponent<Expedition>();
        e.ID = lastUsedID;
        lastUsedID++;
        if (expeditionsContainer == null) expeditionsContainer = new GameObject("expeditionsContainer").transform;
        e.gameObject.transform.parent = expeditionsContainer;
        expeditionsList.Add(e);
        actionsHash++;

        e.stage = ExpeditionStage.Preparation;
        e.mission = Mission.NoMission;

        return e;
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
                    Destroy(e);
                    expeditionsList.RemoveAt(i);
                    actionsHash++;
                    break;
                }
            }
        }
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
                        bool completed = mission.NextStep();
                        if (completed) EndMission();
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
            actionsHash++;
        }
    }
    public void DropMission()
    {
        if (mission != Mission.NoMission)
        {
            mission = Mission.NoMission;
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
        return data;
    }
    public Expedition Load(System.IO.FileStream fs)
    {
       //awaiting
        return this;
    }

    public static void SaveStaticData(System.IO.FileStream fs)
    {
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
    }
    public static void LoadStaticData(System.IO.FileStream fs)
    {
        var data = new byte[4];
        fs.Read(data, 0, 4);
        int count = System.BitConverter.ToInt32(data,0);
        expeditionsList = new List<Expedition>();
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Expedition ex = CreateNewExpedition();
                ex.Load(fs);
            }
        }
    }
    #endregion
}

