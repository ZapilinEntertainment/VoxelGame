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
    public ExpeditionSerializer Save()
    {
        ExpeditionSerializer es = new ExpeditionSerializer();
        //awaiting
        return es;
    }
    public Expedition Load(ExpeditionSerializer es)
    {
       //awaiting
        return this;
    }

    public static ExpeditionStaticSerializer SaveStaticData()
    {
        ExpeditionStaticSerializer ess = new ExpeditionStaticSerializer();
        ess.currentExpeditions = new List<ExpeditionSerializer>();
        if (expeditionsList.Count > 0)
        {
            int i = 0;
            while (i < expeditionsList.Count)
            {
                if (expeditionsList[i] == null)
                {
                    expeditionsList.RemoveAt(i);
                    continue;
                }
                else
                {
                    ess.currentExpeditions.Add(expeditionsList[i].Save());
                }
                i++;
            }
        }
        return ess;
    }
    public static void LoadStaticData(ExpeditionStaticSerializer ess)
    {
        int i = 0; expeditionsList = new List<Expedition>();
        while (i < ess.currentExpeditions.Count)
        {
            expeditionsList.Add(new Expedition().Load(ess.currentExpeditions[i]));
            i++;
        }
    }
    #endregion
}

[System.Serializable]
public class ExpeditionSerializer
{
    
}
