using UnityEngine;
using System.Collections.Generic;

public sealed class Artifact {
    public enum AffectionType { NoAffection, LifepowerAffection, StabilityAffection, EventsAffection}
    public enum ArtifactStatus { Exists, UsingByCrew, Researching, UsingInMonument}

    public bool activated { get; private set; }
    public bool destructed { get; private set; }
    public bool researched { get; private set; }
    public byte stability { get; private set; }
    public byte saturation { get; private set; }
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

    public static bool operator ==(Artifact lhs, Artifact rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(Artifact lhs, Artifact rhs) { return !(lhs.Equals(rhs)); }
    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        Artifact p = (Artifact)obj;
        return p.ID == ID;
    }
    public override int GetHashCode()
    {
        return (int)(stability + saturation + frequency + ID);
    }

    static Artifact()
    {
        emptyArtifactFrame_tx = Resources.Load<Texture>("Resources/Textures/emptyArtifactFrame");
    }
    
    public static void AddArtifactToCollection(Artifact a)
    {
        actionsHash++;
    }
    public static void RemoveArtifactFromCollection(Artifact a)
    {
        actionsHash++;
    }

    public Artifact (float i_stability, float i_saturation, float i_frequency, AffectionType i_type, bool i_activated)
    {
        i_stability = Mathf.Clamp01(i_stability);
        stability = (byte)(i_stability * 256f);
        i_saturation = Mathf.Clamp01(i_saturation);
        saturation = (byte)(i_saturation * 256f);
        frequency = i_frequency;
        type = i_type;
        activated = i_activated;
        destructed = false;
        researched = false;
        name = Localization.NameArtifact();
        status = ArtifactStatus.Exists;
        ID = lastUsedID++;
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
                destructed = true;
                RemoveArtifactFromCollection(this);
                return null;
            }
            else return false;
        }
        else return true;
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
}
