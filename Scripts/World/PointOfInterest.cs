using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointOfInterest : MapPoint
{
    protected float richness, danger, mysteria, friendliness;
    public readonly float difficulty;
    public float exploredPart { get; protected set; }
    public List<MissionPreset> availableMissions { get; private set; }
    public List<Expedition> workingExpeditions { get; private set; }

    public PointOfInterest(int i_id) : base(i_id) {
        difficulty = RecalculateDifficulty();
    }
    public PointOfInterest(float i_angle, float i_height, MapMarkerType mtype) : base(i_angle, i_height, mtype)
    {
        difficulty = RecalculateDifficulty();
        exploredPart = 0f;
    }

    public void ListAnExpedition(Expedition e)
    {
        if (workingExpeditions == null)
        {
            workingExpeditions = new List<Expedition>();
            workingExpeditions.Add(e);
        }
        else
        {
            if (!workingExpeditions.Contains(e)) workingExpeditions.Add(e);
        }
    }
    public void ExcludeExpeditionFromList(Expedition e)
    {
        if (workingExpeditions != null)
        {
            workingExpeditions.Remove(e);
            if (workingExpeditions.Count == 0) workingExpeditions = null;
        }
    }
    public Mission GetMissionByIndex(int i)
    {
        if (availableMissions != null && availableMissions.Count > i)
        {
            return new Mission(availableMissions[i]);
        }
        else return null;
    }

    public void Explore(float k)
    {
        exploredPart += k;
        if (exploredPart >= 1f) exploredPart = 1f;
        GameMaster.realMaster.researchStar.PointExplored(this);
    }  
    public bool TreasureFinding(Crew c)
    {
        if (c.attributes.PerceptionRoll() > 20f * (0.9f + difficulty * 0.3f - friendliness * 0.3f))
        {
            TakeTreasure(c);
            return true;
        }
        else return false;
    }
    public void TakeTreasure(Crew c)
    {
        if (c.artifact == null)
        {
            if (Random.value < GameConstants.ARTIFACT_FOUND_CHANCE)
            {
                c.SetArtifact(GetArtifact());
                GameLogUI.MakeAnnouncement(Localization.GetPhrase(LocalizedPhrase.CrewFoundArtifact));
                return;
            }
        }
        GainResources(c);
    }    
    public void GainResources(Crew c)
    {
        List<ResourceType> typesList = null;
        switch (type)
        {
            case MapMarkerType.Station:
                typesList = new List<ResourceType>() { ResourceType.metal_K, ResourceType.metal_M, ResourceType.Plastics, ResourceType.Fuel };
                if (Random.value < 0.4f) c.attributes.AddExperience(Expedition.ONE_STEP_XP * difficulty);
                break;
            case MapMarkerType.Wreck:
                typesList = new List<ResourceType>() { ResourceType.metal_S, ResourceType.Fuel, ResourceType.metal_M, ResourceType.Graphonium };
                break;
            case MapMarkerType.Island:
                typesList = new List<ResourceType>() { ResourceType.metal_M_ore, ResourceType.metal_E_ore, ResourceType.metal_N_ore, ResourceType.metal_P_ore };
                break;
            case MapMarkerType.SOS:
                typesList = new List<ResourceType>() { ResourceType.metal_S, ResourceType.Fuel, ResourceType.Supplies };
                if (Random.value < 0.3f) c.attributes.AddExperience(Expedition.ONE_STEP_XP * difficulty);
                break;
            case MapMarkerType.Portal:
                typesList = new List<ResourceType>() { ResourceType.metal_N, ResourceType.metal_N_ore, ResourceType.Graphonium };
                break;
            case MapMarkerType.QuestMark:
                // ?
                break;
            case MapMarkerType.Colony:
                typesList = new List<ResourceType>() { ResourceType.Food, ResourceType.Supplies, ResourceType.metal_K };
                break;
            case MapMarkerType.Wiseman:
                c.attributes.AddExperience(Expedition.ONE_STEP_XP * 20f * difficulty);
                c.attributes.RaiseAdaptability(1f);
                break;
            case MapMarkerType.Wonder:
                if (Random.value > 0.5f) typesList = new List<ResourceType>() { ResourceType.metal_N, ResourceType.Graphonium };
                else c.attributes.AddExperience(Expedition.ONE_STEP_XP * 5f);
                break;
            case MapMarkerType.Resources: // or changing!
                typesList = new List<ResourceType>()
                {
                    ResourceType.metal_K_ore, ResourceType.metal_M_ore, ResourceType.metal_N_ore, ResourceType.metal_E_ore, ResourceType.metal_P_ore,
                    ResourceType.metal_S_ore, ResourceType.mineral_F, ResourceType.mineral_L
                };
                break;
        }
        if (typesList != null && typesList.Count > 0)
        {
            GameMaster.realMaster.colonyController.storage.AddResource(typesList[Random.Range(0,typesList.Count - 1)], 5f * c.membersCount / (float)Crew.MAX_MEMBER_COUNT * c.attributes.persistence);
        }
    }
    public bool TryToJump(Crew c)
    {
        return (c.attributes.PerceptionRoll() < 10f * difficulty * (1f - 0.3f * friendliness));
    }
    public bool TryAdditionalJump(Crew c)
    {
        float a = c.attributes.PerceptionRoll();
        float b = c.attributes.PerceptionRoll();
        if (b < a) a = b;
        return a > 20f * difficulty * (1f - 0.2f * friendliness);
    }
    public float GetRestValue()
    {
        return (0.4f + danger * 0.3f + difficulty * 0.3f + friendliness * 0.2f) * 0.5f;
    }
    public bool HardTest(Crew c)
    {
        return c.attributes.HardTestRoll() >= danger * (20f + 10f * difficulty);
    }
    public bool SoftTest(Crew c)
    {
        return c.attributes.SoftCheckRoll() >= friendliness * (10f + 10f * difficulty);
    }
    public bool LoyaltyTest(Crew c)
    {
        return c.attributes.LoyaltyRoll() >= danger * 20f * (1f - friendliness) ;
    }
    public bool AdaptabilityTest(Crew c)
    {
        return c.attributes.AdaptabilityRoll() >= mysteria * 20f;
    }
    public bool TrueWayTest(Crew c)
    {
        return 20f * difficulty * (1f - friendliness) < c.attributes.IntelligenceRoll();
    }
    public bool IsSomethingChanged()
    {
        return Random.value < danger;
    }

    protected float RecalculateDifficulty()
    {
        float locationDifficulty = 0f;
        switch (type)
        {
            case MapMarkerType.Unknown:
                richness = Random.value;
                danger = Random.value;
                mysteria = 1f;
                friendliness = Random.value;
                locationDifficulty = Random.value;
                break;
            case MapMarkerType.MyCity:
                richness = 0.05f;
                danger = 0f;
                mysteria = 0.01f;
                friendliness = GameMaster.realMaster.environmentMaster.environmentalConditions;
                locationDifficulty = 0f;
                break;
            // SUNPOINT:
            //case MapMarkerType.Star:  
            // richness = 0.1f;
            // danger = 1f;
            //  mysteria = 0.1f + Random.value * 0.2f;
            //  friendliness = Random.value;
            // locationDifficulty = 1f;
            //  break;
            case MapMarkerType.Station:
                richness = Random.value * 0.8f + 0.2f;
                danger = Random.value * 0.2f;
                mysteria = Random.value * 0.5f;
                friendliness = Random.value;
                locationDifficulty = 0.2f + Random.value * 0.2f;
                break;
            case MapMarkerType.Wreck:
                richness = 0.3f + Random.value * 0.5f;
                danger = 0.3f + Random.value * 0.2f;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.8f;
                locationDifficulty = 0.7f + Random.value * 0.2f;
                break;
            case MapMarkerType.Shuttle:
                richness = 0f;
                danger = 0f;
                mysteria = 0f;
                friendliness = 1f;
                locationDifficulty = 0f;
                break; // flyingExpedition.expedition.sectorCollapsingTest
            case MapMarkerType.Island:
                richness = 0.2f + Random.value * 0.7f;
                danger = Random.value * 0.5f;
                mysteria = 0.2f + Random.value * 0.5f;
                friendliness = Random.value * 0.7f + 0.3f;
                locationDifficulty = Random.value * 0.4f + 0.4f * danger;
                break;
            case MapMarkerType.SOS:
                richness = 0f;
                danger = 0.2f + 0.8f * Random.value;
                mysteria = Random.value * 0.3f;
                friendliness = Random.value * 0.5f + 0.5f;
                locationDifficulty = Random.value * 0.5f + 0.25f;
                break;
            case MapMarkerType.Portal:
                richness = 0.1f;
                danger = 0.3f + Random.value;
                mysteria = 0.3f + Random.value * 0.7f;
                friendliness = Random.value;
                locationDifficulty = Random.value * 0.6f + 0.4f;
                break;
            case MapMarkerType.QuestMark:
                // устанавливается квестом                
                break;
            case MapMarkerType.Colony:
                richness = 0.5f + Random.value * 0.5f;
                danger = 0.1f * Random.value;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.7f + 0.3f;
                locationDifficulty = 0.3f * Random.value;
                break;
            case MapMarkerType.Wiseman:
                richness = 0.1f;
                danger = 0.1f * Random.value;
                mysteria = Random.value;
                friendliness = Random.value * 0.3f + 0.7f;
                locationDifficulty = Random.value * (1 - friendliness);
                break;
            case MapMarkerType.Wonder:
                richness = 0.3f + Random.value * 0.7f;
                danger = 0.1f * Random.value;
                mysteria = 0.5f + Random.value * 0.5f;
                friendliness = Random.value;
                locationDifficulty = Random.value;
                break;
            case MapMarkerType.Resources:
                richness = 0.5f + Random.value * 0.5f;
                danger = Random.value * 0.3f;
                mysteria = 0.1f * Random.value;
                friendliness = Random.value * 0.6f;
                locationDifficulty = 0.25f + 0.25f * Random.value;
                break;
        }
        float _difficulty = ((danger + mysteria + locationDifficulty - friendliness) * 0.5f + Random.value * 0.5f) * (1f + GameMaster.realMaster.GetDifficultyCoefficient()) * 0.5f;
        if (_difficulty < 0f) _difficulty = 0f; // ну мало ли
        else if (_difficulty > 1f) _difficulty = 1f;
        return _difficulty;
    }
    protected Artifact GetArtifact()
    {
        float a = mysteria * friendliness * 0.8f + 0.2f * Random.value;
        var atype = Artifact.AffectionType.NoAffection;
        bool researched = false;
        switch (type)
        {
            case MapMarkerType.Unknown:
                atype = Artifact.AffectionType.SpaceAffection;
                break;
            case MapMarkerType.Station:
                if (Random.value > 0.5f) researched = true;
                atype = Random.value > 0.5f ? Artifact.AffectionType.LifepowerAffection : Artifact.AffectionType.StabilityAffection;
                break;
            case MapMarkerType.Wreck:
                if (Random.value < 0.1f) researched = true;
                break;
            case MapMarkerType.Island:
                atype = Random.value > 0.4f ? Artifact.AffectionType.StabilityAffection : Artifact.AffectionType.LifepowerAffection;
                break;
            case MapMarkerType.Portal:
                if (Random.value > 0.5f) atype = Artifact.AffectionType.StabilityAffection;
                break;
            case MapMarkerType.Colony:
                if (Random.value > 0.6f) atype = Artifact.AffectionType.LifepowerAffection; else atype = Artifact.AffectionType.StabilityAffection;
                if (Random.value > 0.3f) researched = true;
                break;
            case MapMarkerType.Wiseman:
                {
                    float f = Random.value;
                    if (f <= 0.33f) atype = Artifact.AffectionType.SpaceAffection;
                    else
                    {
                        if (f >= 0.66f) atype = Artifact.AffectionType.StabilityAffection;
                        else atype = Artifact.AffectionType.LifepowerAffection;
                    }
                    researched = true;
                    break;
                }
            case MapMarkerType.Wonder:
                atype = Artifact.AffectionType.StabilityAffection;
                break;
        }

        var art = new Artifact(
            (friendliness * 0.5f + richness * 0.5f) * 0.6f * Random.value + 0.4f,
            (richness * 0.7f + 0.3f * Random.value) * 0.6f + 0.4f * Random.value,
            (danger * richness * 0.55f + 0.45f * Random.value) * 0.5f + 0.5f * Random.value,
           atype
            );
        art.SetResearchStatus(researched);
        return art;
    }

    #region save-load
    override public List<byte> Save()
    {
        var data = base.Save();
        data.AddRange(System.BitConverter.GetBytes(richness)); // 0 - 3
        data.AddRange(System.BitConverter.GetBytes(danger)); // 4 - 7
        data.AddRange(System.BitConverter.GetBytes(mysteria)); // 8 - 11
        data.AddRange(System.BitConverter.GetBytes(friendliness)); // 12 - 15
        data.AddRange(System.BitConverter.GetBytes(exploredPart)); // 16 - 19

        byte missionsCount = 0;
        var missionsData = new List<byte>();
        if (availableMissions != null)
        {
            foreach (MissionPreset mp in availableMissions)
            {
                missionsData.AddRange(mp.Save());
                missionsCount++;
            }
        }
        data.Add(missionsCount); // 24
        if (missionsCount != 0) data.AddRange(missionsData);
        return data;
    }
    public void Load(System.IO.FileStream fs)
    {
        int LENGTH = 21;
        var data = new byte[LENGTH];
        fs.Read(data, 0, LENGTH);
        richness = System.BitConverter.ToSingle(data, 0);
        danger = System.BitConverter.ToSingle(data, 4);
        mysteria = System.BitConverter.ToSingle(data, 8);
        friendliness = System.BitConverter.ToSingle(data, 12);
        exploredPart = System.BitConverter.ToSingle(data, 16);        
        byte n = data[20];
        if (n > 0)
        {
            availableMissions = new List<MissionPreset>();
            for (int i = 0; i < n; i++)
            {
                availableMissions.Add(MissionPreset.Load(fs));
            }
        }
    } 
    #endregion
}