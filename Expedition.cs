using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Expedition : MonoBehaviour
{    
    public float progress { get; private set; }  
    public float cost { get; private set; }
    public int ID { get; private set; }
    public int participantsCount { get; private set; }
    public int fuelNeeded { get; private set; }

    public ExpeditionStage stage { get; private set; }
    public Mission mission { get; private set; }
    public List<MonoBehaviour> participants { get; private set; }
    public List<MissionStageType> stages;

    public enum MissionStageType : byte { Test, Decision, Fee, Crossroad, Special, Random}
    public enum ExpeditionStage : byte { Preparation, WayIn, WayThrough, WayOut}
    
    public static List<Expedition> expeditionsList { get; private set; }
    public static int expeditionsFinished, expeditionsSucceed;
    public static int actionsHash { get; private set; }
    private static Transform expeditionsContainer;

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
        Expedition e = new GameObject().AddComponent<Expedition>();
        if (expeditionsContainer == null) expeditionsContainer = new GameObject("expeditionsContainer").transform;
        e.gameObject.transform.parent = expeditionsContainer;
        expeditionsList.Add(e);
        actionsHash++;

        e.stage = ExpeditionStage.Preparation;
        e.mission = Mission.NoMission;
        e.participants = new List<MonoBehaviour>();
        e.stages = new List<MissionStageType>();

        return e;
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
                expeditionsList.Add(new Expedition().Load(fs));
            }
        }
    }
    #endregion
}

