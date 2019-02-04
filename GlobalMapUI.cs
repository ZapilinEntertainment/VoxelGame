using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapMarkerType : byte { Unknown, MyCity, Station, Wreck, Shuttle, Island, SOS, Portal, QuestMark, OtherColony, Star, Wiseman, Wonder, Resources }

public class GlobalMapUI : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private Transform[] rings;
    [SerializeField] private GameObject exampleMarker;
#pragma warning restore 0649
    private bool prepared = false;
    private float sh;
    public float[] ringsBorders { get; private set; }
    private float[] rotationSpeed;
    private Canvas mapCanvas;
    private MapPoint cityPoint;
    private Vector3 mapCenter;
    private List<PointOfInterest> points;      

    private void Start()
    {
        if (!prepared) Prepare();        
    }

    private void Prepare()
    {        
        ringsBorders = new float[6] { 1, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
        sh = Screen.height;
        float outer = sh * ringsBorders[0], inner = sh * ringsBorders[1];
        int resolution = (int)(Screen.height * QualitySettings.GetQualityLevel() / 2f);
        rings[0].GetComponent<RawImage>().texture = GetTorusTexture((int)outer, (int)inner, resolution);
        outer = inner; inner = sh * ringsBorders[2];
        rings[1].GetComponent<RawImage>().texture = GetTorusTexture((int)outer, (int)inner, resolution);
        outer = inner; inner = sh * ringsBorders[3];
        rings[2].GetComponent<RawImage>().texture = GetTorusTexture((int)outer, (int)inner, resolution);
        outer = inner; inner = sh * ringsBorders[4];
        rings[3].GetComponent<RawImage>().texture = GetTorusTexture((int)outer, (int)inner, resolution);
        outer = inner; inner = sh * ringsBorders[5];
        rings[4].GetComponent<RawImage>().texture = GetTorusTexture((int)outer, (int)inner, resolution);

        rotationSpeed = new float[5];
        rotationSpeed[0] = (Random.value - 0.5f) * 30;
        rotationSpeed[1] = (Random.value - 0.5f) * 30;
        rotationSpeed[2] = (Random.value - 0.5f) * 30;
        rotationSpeed[3] = (Random.value - 0.5f) * 30;
        rotationSpeed[4] = (Random.value - 0.5f) * 30;      

        points = new List<PointOfInterest>();
        
        mapCenter = new Vector3(Screen.width / 2f, sh / 2f, 0);

        GameObject cityMarker = Instantiate(exampleMarker,transform);
        cityMarker.GetComponent<RawImage>().uvRect = GetMarkerRect(MapMarkerType.MyCity);
        cityMarker.GetComponent<Button>().interactable = false;
        cityMarker.gameObject.SetActive(true);
        cityPoint = cityMarker.AddComponent<MapPoint>();
        float h = GameConstants.START_HAPPINESS;
        cityPoint.Initialize(cityMarker.GetComponent<RectTransform>(), Random.value * 360, h, DefineRing(h), MapMarkerType.MyCity);

        prepared = true;
    }

    public void Activate()
    {
        if (!prepared)
        {
            Prepare();
        }
    }

    private void Update()
    {
        if (!prepared) return;
        float t = Time.deltaTime * GameMaster.gameSpeed;
        rings[0].transform.Rotate(Vector3.forward * rotationSpeed[0] * t);
        rings[1].transform.Rotate(Vector3.forward * rotationSpeed[1] * t);
        rings[2].transform.Rotate(Vector3.forward * rotationSpeed[2] * t);
        rings[3].transform.Rotate(Vector3.forward * rotationSpeed[3] * t);
        rings[4].transform.Rotate(Vector3.forward * rotationSpeed[4] * t);

        if (GameMaster.realMaster.colonyController != null)
        {
            float h = 1 - GameMaster.realMaster.colonyController.happiness_coefficient;
            if (h != cityPoint.height)
            {
                cityPoint.height = h;
                cityPoint.ringIndex = DefineRing(h);
            }
        }
        cityPoint.angle += rotationSpeed[cityPoint.ringIndex] * t;
        cityPoint.rect.position = mapCenter + Quaternion.AngleAxis(cityPoint.angle, Vector3.forward) * (Vector3.up * cityPoint.height * sh / 2f);
    }

    private Texture2D GetTorusTexture(int outerRadius, int innerRadius, int resolution)
    {
        byte[] rawdata = new byte[resolution * resolution * 4];
        float sr = 0;
        float squaredRadius = outerRadius * outerRadius, squaredInnerRadius = innerRadius * innerRadius;
        float halfpoint = resolution / 2f, sqWidth = squaredRadius - squaredInnerRadius;
        int k = 0;
        byte one = 255, zero = 0;
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                sr = (x - halfpoint) * (x - halfpoint) + (y - halfpoint) * (y - halfpoint);   
                if (sr <= squaredRadius & sr >= squaredInnerRadius)
                {
                    if (squaredRadius - sr < 4)
                    {
                        //border
                        rawdata[k] = zero;
                        rawdata[k + 1] = one;
                        rawdata[k + 2] = one;
                    }
                    else
                    {
                        rawdata[k] = 255;
                        rawdata[k + 1] = 255;
                        rawdata[k + 2] = 255;
                    }
                    float d = (squaredRadius - sr) / sqWidth;
                    if (d <= 0.1f) d = Mathf.Sin((d - 0.1f) / 0.2f * Mathf.PI);
                    else
                    {
                        if (d >= 0.9f) d = Mathf.Sin((d - 0.9f) / 0.9f * Mathf.PI);
                        else d = 1;
                    }
                    rawdata[k + 3] = (byte)(d * one);
                }
                else
                {
                    rawdata[k] = zero;
                    rawdata[k + 1] = zero;
                    rawdata[k + 2] = zero;
                    rawdata[k + 3] = zero;
                }                
                k += 4;
            }
        }
        Texture2D tx = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tx.LoadRawTextureData(rawdata);
        tx.Apply();
        return tx;
    }

    private Rect GetMarkerRect(MapMarkerType mtype)
    {
        float p = 0.25f;
        switch (mtype)
        {
            case MapMarkerType.MyCity: return new Rect(p,0,p,p);
            case MapMarkerType.Station: return new Rect(0, p, p, p);
            case MapMarkerType.Wreck: return new Rect(p,p,p,p);
            case MapMarkerType.Shuttle: return new Rect(2 * p, p, p, p);
            case MapMarkerType.Island: return new Rect(3 * p, p, p, p);
            case MapMarkerType.SOS: return new Rect(0, 2 * p, p, p);
            case MapMarkerType.Portal: return new Rect(p, 2 * p, p, p);
            case MapMarkerType.QuestMark: return new Rect(2 * p, 2 * p, p, p);
            case MapMarkerType.OtherColony: return new Rect(3 * p, 2 * p, p, p);
            case MapMarkerType.Star: return new Rect(0, 3 * p, p, p);
            case MapMarkerType.Wiseman: return new Rect(p, 3 * p, p, p);
            case MapMarkerType.Wonder: return new Rect(2 * p, 3 * p, p, p);
            case MapMarkerType.Resources: return new Rect(3 * p, 3 * p, p, p);
            case MapMarkerType.Unknown:
            default:
                return new Rect(0, 0, p, p);
        }
    }

    private byte DefineRing(float ypos)
    {
        if (ypos < ringsBorders[2])
        {
            if (ypos < ringsBorders[4]) return 4;
            else
            {
                if (ypos > ringsBorders[3]) return 2;
                else return 3;
            }
        }
        else
        {
            if (ypos > ringsBorders[1]) return 0;
            else return 1;
        }
    }

    #region save-load system
    public void Save(System.IO.FileStream fs)
    {

    }
    public void Load(System.IO.FileStream fs)
    {

    }
    #endregion
}

class MapPoint : MonoBehaviour
{
    public byte ringIndex;
    public MapMarkerType type { get; protected set;}
    public RectTransform rect { get; protected set; }
    public float angle {
        get { return angle; }
        set { if (value > 360) angle = value % 360;
            else
            {
                if (value < 0) angle = (value % 360) + 360;
            }
            }
    }
    public float height { get { return height; } set { if (value > 1) height = 1; else { if (value < 0) height = 0; } } }

    public void Initialize(RectTransform t, float i_angle, float i_height, byte ring, MapMarkerType mtype)
    {
        rect = t;
        angle = i_angle;
        height = i_height;
        ringIndex = ring;
        type = mtype;
    }
}

class PointOfInterest : MapPoint
{    
    public Mission mission { get; protected set; }
    public Expedition sentExpedition { get; protected set; }

    public void Initialize(RectTransform t, float i_angle, float i_height, byte ring, MapMarkerType mtype, Mission m)
    {
        rect = t;
        angle = i_angle;
        height = i_height;
        ringIndex = ring;
        type = mtype;
        mission = m;
    }

    public void SendExpedition(Expedition e)
    {
        if (sentExpedition == null) sentExpedition = e;
        else
        {
            if (sentExpedition.stage == Expedition.ExpeditionStage.Preparation)
            {
                sentExpedition.Dismiss();
                sentExpedition = e;
            }
        }
    }
}
