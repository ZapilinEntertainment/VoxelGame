using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapMarkerType : byte { Unknown, MyCity, Station, Wreck, Shuttle, Island, SOS, Portal, QuestMark, OtherColony, Star, Wiseman, Wonder, Resources }

public class GlobalMapUI : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private Transform[] rings;
    [SerializeField] private GameObject exampleMarker;
    [SerializeField] private GameObject mapCamera, mapCanvas;
#pragma warning restore 0649
    private static GlobalMapUI current;

    private bool prepared = false;
    private float sh;
    public float[] ringsBorders { get; private set; }
    private float[] rotationSpeed;
    private MapPoint cityPoint;
    private Vector3 mapCenter;
    private List<PointOfInterest> points;      

    public static GlobalMapUI GetCurrent()
    {
        if (current == null)
        {
            current = Instantiate(Resources.Load<GameObject>("UIPrefs/globalMapUI")).GetComponent<GlobalMapUI>();
            current.Prepare();
        }
        return current;
    }

    private void Start()
    {
        if (!prepared) Prepare();        
    }

    private void Prepare()
    {        
        ringsBorders = new float[6] { 1, 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
        int resolution = (int)(Screen.height * QualitySettings.GetQualityLevel() / 2f);
        float r = ringsBorders[1];
        rings[0].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[0]), r);
        r = ringsBorders[2] / r;
        rings[1].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[1]), r);
        r = ringsBorders[3] / r;
        rings[2].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[2]), r);
        r = ringsBorders[4] / r;
        rings[3].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[3]), r);
        r = ringsBorders[5] / r;
        rings[4].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[4]), r);

        rotationSpeed = new float[5];
        rotationSpeed[0] = (Random.value - 0.5f) * 30;
        rotationSpeed[1] = (Random.value - 0.5f) * 30;
        rotationSpeed[2] = (Random.value - 0.5f) * 30;
        rotationSpeed[3] = (Random.value - 0.5f) * 30;
        rotationSpeed[4] = (Random.value - 0.5f) * 30;      

        points = new List<PointOfInterest>();
        
        mapCenter = new Vector3(Screen.width / 2f, sh / 2f, 0);
        prepared = true;

        GameObject cityMarker = Instantiate(exampleMarker,mapCanvas.transform);
        cityMarker.GetComponent<RawImage>().uvRect = GetMarkerRect(MapMarkerType.MyCity);
        cityMarker.GetComponent<Button>().interactable = false;
        cityMarker.gameObject.SetActive(true);
        cityPoint = cityMarker.AddComponent<MapPoint>();
        float h = GameConstants.START_HAPPINESS;
        cityPoint.Initialize(cityMarker.GetComponent<RectTransform>(), Random.value * 360, h, DefineRing(h), MapMarkerType.MyCity);

        LocalizeTitles();
    }

    public void Activate()
    {
        if (!prepared)
        {
            Prepare();
        }
        UIController.current.gameObject.SetActive(false);
        FollowingCamera.main.gameObject.SetActive(false);
        mapCamera.SetActive(true);
        mapCanvas.SetActive(true);
    }
    public void Close()
    {
        mapCamera.SetActive(false);
        mapCanvas.SetActive(false);
        UIController.current.gameObject.SetActive(true);
        FollowingCamera.main.gameObject.SetActive(true);
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
        if (cityPoint.angle >= 360) cityPoint.angle %= 360;
        cityPoint.rect.localPosition = Quaternion.AngleAxis(cityPoint.angle, Vector3.forward) * (Vector3.up * cityPoint.height * Screen.height / 2f) ;
    }

    private Texture2D GetTorusTexture(int resolution, float innerRadiusValue)
    {
        byte[] rawdata = new byte[resolution * resolution * 4];
        float squaredRadius = resolution * resolution / 4f, innerSquaredRadius = squaredRadius * innerRadiusValue * innerRadiusValue;
        float half = resolution / 2f;
        float sr = 0;
        byte one = 255, zero = 0;

        int k = 0;
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                sr = (x - half) * (x - half) + (y - half) * (y - half);
                if (sr <= squaredRadius & sr >= innerSquaredRadius)
                {
                    rawdata[k] = one;
                    rawdata[k + 1] = one;
                    rawdata[k + 2] = one;                    
                    rawdata[k + 3] = one;
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

    public void LocalizeTitles()
    {
        mapCanvas.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Close);
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
    public float angle;
    public float height;

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
