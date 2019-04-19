using UnityEngine;
using System.Collections.Generic;

public sealed class Artifact {
    public enum AffectionType { NoAffection, LifepowerAffection, StabilityAffection, EventsAffection}
    public enum ArtifactStatus { Exists, UsingByCrew, Researching, UsingInMonument, OnConservation}

    public bool activated { get; private set; }
    public bool destructed { get; private set; }
    public bool researched { get; private set; }
    public float stability { get; private set; }
    public float saturation { get; private set; }
    public float frequency { get; private set; }
    public int ID { get; private set; }
    public string name { get; private set; }
    public ArtifactStatus status { get; private set; }
    public Crew owner { get; private set; }
    public readonly AffectionType type;
    private Texture icon;

    public static readonly Texture emptyArtifactFrame_tx;
    public static int actionsHash = 0, lastUsedID = 0;    
    public static List<Artifact> playersArtifactsList;
    public static UIArtifactPanel observer { get; private set; }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        MapPoint mp = (MapPoint)obj;
        return (ID == mp.ID);
    }
    public override int GetHashCode()
    {
        return (int)(stability + saturation + frequency + ID);
    }

    static Artifact()
    {
        emptyArtifactFrame_tx = Resources.Load<Texture>("Resources/Textures/emptyArtifactFrame");
        playersArtifactsList = new List<Artifact>();
    }

    public static Artifact GetArtifactByID (int id)
    {
        if (playersArtifactsList.Count == 0) return null;
        else
        {
            foreach (var a in playersArtifactsList)
            {
                if (a.ID == id) return a;
            }
            return null;
        }
    }

    public Artifact (float i_stability, float i_saturation, float i_frequency, AffectionType i_type, bool i_activated)
    {
        i_stability = Mathf.Clamp01(i_stability);
        stability = i_stability;
        i_saturation = Mathf.Clamp01(i_saturation);
        saturation = i_saturation;
        frequency = i_frequency;
        type = i_type;
        activated = i_activated;
        destructed = false;
        researched = false;
        name = Localization.NameArtifact();
        status = ArtifactStatus.Exists;
        ID = lastUsedID++;
        playersArtifactsList.Add(this);

        actionsHash++;
    }

    public bool? StabilityTest(float hardness)
    {
        if (!activated && Random.value > 0.01f * (byte)GameMaster.difficulty)
        {
            activated = true;
        }
        if (Random.value * hardness > stability)
        {
            stability -= (byte)(hardness * 16f);
            if (stability <= 0)
            {
                Destroy();
                return null;
            }
            else return false;
        }
        else return true;
    }
    public void Destroy()
    {
        destructed = true;
        if (playersArtifactsList.Contains(this)) playersArtifactsList.Remove(this);
        actionsHash++;
    }

    /// <summary>
    /// возвращает true, если не исчез
    /// </summary>
    /// <returns></returns>
    public bool Event()  // INDEV
    {
        //switch type?
        return true;
    }

    public Color GetColor()
    {
        if (destructed) return new Color(0f, 0f, 0f, 0f);
        else
        {
            if (!researched) return new Color(1f, 1f, 1f, 0.5f);
            else
            {
                var col = new Color((1 - stability), 1f, 1f);
                col = Color.Lerp(Color.black, col, saturation);
                col.a = Mathf.Clamp01(frequency);
                return col;
            }
        }
    }
    public Color GetHaloColor()
    {
        switch (type)
        {
            case AffectionType.EventsAffection:
                return new Color(stability * 0.3f, 0f, frequency);
            case AffectionType.LifepowerAffection:
                return new Color(0f, saturation, frequency * 0.7f);
            case AffectionType.StabilityAffection:
                return new Color(stability, saturation * 0.25f, 0f);
            case AffectionType.NoAffection:
            default:
                return new Color(1f, 1f, 1f, 0.5f * saturation);
        }
    }
    public Texture GetTexture() // INDEV
    {
        return emptyArtifactFrame_tx;
    }

    public void SetOwner(Crew c)
    {
        owner = c;
        if (owner != null) status = ArtifactStatus.UsingByCrew;
    }
    public void ChangeName(string s)
    {
        name = s;        
    }
    public void Conservate()
    {
        status = ArtifactStatus.OnConservation;
        owner = null;
        actionsHash++;
    }

    public void ShowOnGUI(Rect r, SpriteAlignment alignment, bool useCloseButton)
    {
        if (observer == null)
        {
            observer = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefs/artifactPanel"), UIController.current.mainCanvas).GetComponent<UIArtifactPanel>();
        }
        observer.gameObject.SetActive(true);
        observer.SetPosition(r, alignment);
        observer.ShowArtifact(this, useCloseButton);
    }
}
