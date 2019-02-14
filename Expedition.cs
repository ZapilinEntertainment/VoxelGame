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

    public float progress { get; private set; }
    public float cost { get; private set; }
    public int ID { get; private set; }
    public static int lastUsedID {get;private set;} // в сохранение
    public int fuelNeeded { get; private set; }

    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public Crew crew { get; private set; }
    public List<MissionStageType> stages;    

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
        e.stages = new List<MissionStageType>();

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
                stage = ExpeditionStage.WayIn;
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
    }
    public void DismissCrew()
    {
        if (mission == Mission.NoMission & crew != null)
        {
            crew.SetStatus(CrewStatus.Free);
            crew = null;
        }
    }

    public void DrawTexture(UnityEngine.UI.RawImage iconPlace)
    {
        //awaiting
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

