using UnityEngine;
using System.Collections.Generic;

public sealed class Artifact {
    public enum AffectionType { NoAffection, LifepowerAffection, StabilityAffection, SpaceAffection}
    //dependency : PointOfInterest.GetArtifact
    // GetAffectionIconRect()
    // localization.GetAffectionTitle
    public enum ArtifactStatus { Exists, UsingByCrew, Researching, UsingInMonument, OnConservation}

    public bool destructed { get; private set; }
    public bool researched { get; private set; }
    public float stability { get; private set; }
    public float saturation { get; private set; }
    public float frequency { get; private set; }
    public int ID { get; private set; }
    public string name { get; private set; }
    public ArtifactStatus status { get; private set; }
    public Crew owner { get; private set; }
    public readonly AffectionType affectionType;
    private Texture2D texture;

    private static readonly Sprite[] affectionRings;
    public static readonly Texture emptyArtifactFrame_tx;
    public static int actionsHash = 0, lastUsedID = 0;    
    public static List<Artifact> artifactsList;
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
        emptyArtifactFrame_tx = Resources.Load<Texture>("Textures/emptyArtifactFrame");
        affectionRings = Resources.LoadAll<Sprite>("Textures/affectionRings");
        artifactsList = new List<Artifact>();
    }

    public static Artifact GetArtifactByID (int id)
    {
        if (artifactsList.Count == 0) return null;
        else
        {
            foreach (var a in artifactsList)
            {
                if (a.ID == id) return a;
            }
            return null;
        }
    }
    public static Sprite GetAffectionSprite(AffectionType atype)
    {
        switch (atype)
        {
            case AffectionType.SpaceAffection: return affectionRings[0];
            case AffectionType.LifepowerAffection: return affectionRings[1];
            case AffectionType.StabilityAffection: return affectionRings[2];
            default: return affectionRings[3];
        }
    }

    public Artifact (float i_stability, float i_saturation, float i_frequency, AffectionType i_type, bool i_activated)
    {
        i_stability = Mathf.Clamp01(i_stability);
        stability = i_stability;
        i_saturation = Mathf.Clamp01(i_saturation);
        saturation = i_saturation;
        frequency = i_frequency;
        affectionType = i_type;
        destructed = false;
        researched = false;
        name = Localization.NameArtifact(this);
        status = ArtifactStatus.Exists;
        ID = lastUsedID++;
        artifactsList.Add(this);        

        actionsHash++;
    }

    public bool? StabilityTest(float hardness)
    {
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

    /// <summary>
    /// возвращает true, если не исчез
    /// </summary>
    /// <returns></returns>
    public bool Event()  // INDEV
    {
        //switch type?
        return true;
    }

    public float GetAffectionValue()
    {
        switch (affectionType)
        {
            case AffectionType.SpaceAffection: return (1f - stability) * (1f - stability) * frequency;
            case AffectionType.LifepowerAffection: return saturation * frequency;
            case AffectionType.StabilityAffection: return (stability + saturation) / 2f;
            default: return stability * saturation * frequency;
        }
    }
    public Color GetColor()
    {
        if (destructed) return Color.black;
        else
        {
           return new Color(1 - stability, saturation, frequency * frequency);
        }
    }
    public Color GetHaloColor()
    {
        switch (affectionType)
        {
            case AffectionType.SpaceAffection:
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

    public Color GetAffectionColor()
    {
        Color c;
        switch (affectionType)
        {
            case AffectionType.LifepowerAffection: c= new Color(0.2f, 0.79f, 0.14f);break;
            case AffectionType.SpaceAffection: c = new Color(0.49f, 0.21f, 0.51f); break;
            case AffectionType.StabilityAffection: c = new Color(0, 1f, 1f); break;
            default: c =  Color.gray;break;
        }
        c.a = GetAffectionValue();
        return c;
    }

    public Texture GetTexture() // INDEV
    {
        if (texture == null)
        {
            byte[] data = new byte[256];// 64px           

            var mainColor = GetColor();
            var emissionColor = GetHaloColor();
            var affectionColor = GetAffectionColor();

            int i = 27 * 4;
            byte r = (byte)(affectionColor.r * 255f),
                g = (byte)(affectionColor.g * 255f),
                b = (byte)(affectionColor.b * 255f),
                a = (byte)(affectionColor.a * 255f);
            // core:
            data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
            data[i + 4] = r; data[i + 5] = g; data[i + 6] = b; data[i + 7] = a;
            i += 32;
            data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
            data[i + 4] = r; data[i + 5] = g; data[i + 6] = b; data[i + 7] = a;
            // diagonals
            if (frequency < 0.33f)
            {
                var dColor = Color.Lerp(emissionColor, Color.white, 0.5f);
                r = (byte)(dColor.r * 255f);
                g = (byte)(dColor.g * 255f);
                b = (byte)(dColor.b * 255f);
                a = 128;

                i = 18 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 21 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 42 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 45 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
            }
            else
            {
                if (frequency <= 0.66f)
                {
                    var dColor = Color.Lerp(emissionColor, Color.white, 0.33f);
                    r = (byte)(dColor.r * 255f);
                    g = (byte)(dColor.g * 255f);
                    b = (byte)(dColor.b * 255f);
                    a = 170;

                    i = 18 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 21 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 42 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 45 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;

                    dColor = Color.Lerp(emissionColor, Color.white, 0.66f);
                    r = (byte)(dColor.r * 255f);
                    g = (byte)(dColor.g * 255f);
                    b = (byte)(dColor.b * 255f);
                    a = 85;

                    i = 9 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 14 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 49 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 54 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                }
                else
                {
                    var dColor = Color.Lerp(emissionColor, Color.white, 0.25f);
                    r = (byte)(dColor.r * 255f);
                    g = (byte)(dColor.g * 255f);
                    b = (byte)(dColor.b * 255f);
                    a = 64;

                    i = 18 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 21 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 42 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 45 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;

                    dColor = Color.Lerp(emissionColor, Color.white, 0.5f);
                    r = (byte)(dColor.r * 255f);
                    g = (byte)(dColor.g * 255f);
                    b = (byte)(dColor.b * 255f);
                    a = 128;

                    i = 9 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 14 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 49 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 54 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;

                    dColor = Color.Lerp(emissionColor, Color.white, 0.75f);
                    r = (byte)(dColor.r * 255f);
                    g = (byte)(dColor.g * 255f);
                    b = (byte)(dColor.b * 255f);
                    a = 192;

                    i = 0;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 7 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 56 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 63 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                }
            }
            //main
            if (saturation < 0.33f)
            {
                var mColor = Color.Lerp(mainColor, Color.black, 0.25f);
                r = (byte)(mColor.r * 255f);
                g = (byte)(mColor.g * 255f);
                b = (byte)(mColor.b * 255f);
                a = 128;

                i = 19 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 20 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 26 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 29 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 34 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 37 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 43 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                i = 44 * 4;
                data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
            }
            else
            {
                byte[] outerRingIndexes = new byte[] { 1, 2, 3, 4, 5, 6, 8, 15, 16, 23, 24, 31, 32, 39, 40, 47, 48, 55, 57, 58, 59, 60, 61, 62 },
                    middleRingIndexes = new byte[] { 10, 11, 12, 13, 17, 22, 25, 30, 33, 38, 41, 46, 50, 51, 52, 53 };
                if (saturation <= 0.66f)
                {
                    var mColor = Color.Lerp(mainColor, Color.black, 0.1f);
                    r = (byte)(mColor.r * 255f);
                    g = (byte)(mColor.g * 255f);
                    b = (byte)(mColor.b * 255f);
                    a = 170;

                    i = 19 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 20 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 26 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 29 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 34 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 37 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 43 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 44 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;

                    mColor = Color.Lerp(mainColor, Color.black, 0.5f);
                    r = (byte)(mColor.r * 255f);
                    g = (byte)(mColor.g * 255f);
                    b = (byte)(mColor.b * 255f);
                    a = 85;
                    foreach (int x in middleRingIndexes)
                    {
                        i = x * 4;
                        data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    }
                }
                else
                {
                    var mColor = Color.Lerp(mainColor, Color.black, 0.1f);
                    r = (byte)(mColor.r * 255f);
                    g = (byte)(mColor.g * 255f);
                    b = (byte)(mColor.b * 255f);
                    a = 192;

                    i = 19 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 20 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 26 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 29 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 34 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 37 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 43 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    i = 44 * 4;
                    data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;

                    mColor = Color.Lerp(mainColor, Color.black, 0.2f);
                    r = (byte)(mColor.r * 255f);
                    g = (byte)(mColor.g * 255f);
                    b = (byte)(mColor.b * 255f);
                    a = 128;
                    foreach (int x in middleRingIndexes)
                    {
                        i = x * 4;
                        data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    }

                    mColor = Color.Lerp(mainColor, Color.black, 0.3f);
                    r = (byte)(mColor.r * 255f);
                    g = (byte)(mColor.g * 255f);
                    b = (byte)(mColor.b * 255f);
                    a = 64;
                    foreach (int x in outerRingIndexes)
                    {
                        i = x * 4;
                        data[i] = r; data[i + 1] = g; data[i + 2] = b; data[i + 3] = a;
                    }
                }
            }
            //build
            texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.LoadRawTextureData(data);
            texture.Apply();
        }
        return texture;
    }

    public static Rect GetAffectionIconRect(AffectionType atype)
    {
        switch (atype)
        {
            default: return Rect.zero;
        }
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
        if (owner != null)
        {
            owner.ClearArtifactField(this);
            owner = null;
        }
        actionsHash++;
    }
    public void UseInMonument()
    {
        owner = null;
        status = ArtifactStatus.UsingInMonument;
        actionsHash++;
    }
    public void SetResearchStatus(bool x) { researched = x; }

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
    public void Destroy()
    {
        destructed = true;
        if (artifactsList.Contains(this)) artifactsList.Remove(this);
        actionsHash++;
    }
}
