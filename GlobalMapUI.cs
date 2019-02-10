using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapMarkerType : byte { Unknown, MyCity, Station, Wreck, Shuttle, Island, SOS, Portal, QuestMark, OtherColony, Star, Wiseman, Wonder, Resources }
//при изменении порядка или количества - изменить маску необязательных объектов

public class GlobalMapUI : MonoBehaviour {
#pragma warning disable 0649
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private Transform[] rings;
    [SerializeField] private GameObject exampleMarker;
    [SerializeField] private GameObject mapCanvas, mapCamera;
#pragma warning restore 0649

    private bool prepared = false;    
    private float[] rotationSpeed;
    private int lastDrawnStateHash = 0;
    private GlobalMap globalMap;
    private List<RectTransform> mapMarkers;
    private List<MapPoint> mapPoints;

    protected readonly Color notInteractableColor = new Color(0, 1, 1, 0), interactableColor = new Color(0, 1, 1, 0.3f);   
    private const float ZOOM_BORDER = 9, DIST_BORDER = 1;

    //========================== PUBLIC METHODS

    public void SetGlobalMap(GlobalMap gm)
    {
        if (gm == null) return;
        globalMap = gm;
        rotationSpeed = globalMap.rotationSpeed;
        mapPoints = globalMap.mapPoints;
    }
    public void SelectPoint(MapPoint mp)
    {
        print(mp.type);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        UIController.current.gameObject.SetActive(true);
        FollowingCamera.main.gameObject.SetActive(true);
    }
    // ======================== PRIVATE METHODS
    private void Start()
    {
        if (!prepared & gameObject.activeSelf) Prepare();
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
        if (lastDrawnStateHash != globalMap.actionsHash)
        {
            RedrawMarkers();
        }
        else
        {
            if (mapMarkers.Count > 0)
            {
                Vector3 fwd = Vector3.forward, up = Vector3.up;
                float sh = Screen.height / 2f;
                for (int i = 0; i < mapMarkers.Count; i++)
                {
                    mapMarkers[i].localPosition = Quaternion.AngleAxis(mapPoints[i].angle, fwd) * (up * sh * mapPoints[i].height);
                }
            }
        }
        if (FollowingCamera.touchscreen)
        {
            if (Input.touchCount == 2)
            {
                Touch tc = Input.GetTouch(0), tc2 = Input.GetTouch(1);
                Vector2 tPrevPos = tc.position - tc.deltaPosition;
                Vector2 t2PrevPos = tc2.position - tc2.deltaPosition;
                float delta = ((tPrevPos - t2PrevPos).magnitude - (tc.position - tc2.position).magnitude) / (float)Screen.height * (-2);
                if (delta != 0)
                {

                }
            }
        }
        else
        {
            if (Input.GetMouseButton(2))
            {
                //ограничения на передвижение?
                float xmove = Input.GetAxis("Mouse X"), ymove = Input.GetAxis("Mouse Y");
                if (xmove != 0 | ymove != 0)  mapRect.position += new Vector3(xmove, ymove, 0) * 30;
            }

            float delta = Input.GetAxis("Mouse ScrollWheel");
            float y = mapRect.localScale.y + delta;
            if (delta != 0 & y < ZOOM_BORDER & y > DIST_BORDER)
            {
                mapRect.localScale += Vector3.one * delta;
            }
        }
    }

    private void Prepare()
    {
        if (globalMap == null) return;
        transform.position = Vector3.up * 0.1f;
        int resolution = (int)(Screen.height * QualitySettings.GetQualityLevel() / 2f);
        float[] ringsBorders = globalMap.ringsBorders;
        float r = ringsBorders[1]; // 0.8f
        rings[0].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[0]), r);
        r = 0.75f;
        rings[1].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[1]), r);
        r = 2f / 3f;
        rings[2].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[2]), r);
        r = 0.5f;
        rings[3].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[3]), r);
        r = 0.5f;
        rings[4].GetComponent<RawImage>().texture = GetTorusTexture((int)(resolution * ringsBorders[4]), r);
        prepared = true;
        mapMarkers = new List<RectTransform>();
        RedrawMarkers();
        LocalizeTitles();        
    }

    private void RedrawMarkers()
    {
        if (!prepared) return;
        else
        {
            mapPoints = globalMap.mapPoints;
            int n = mapPoints.Count;
            int c = mapMarkers.Count;
            if (n > 0)
            {
                mapRect.gameObject.SetActive(false);
                if (c != n)
                {                    
                    if (c > n)
                    {                        
                        for (int i = c - 1; i >= n; i--)
                        {
                            int a = mapMarkers.Count - 1;
                            Destroy(mapMarkers[a].gameObject);
                            mapMarkers.RemoveAt(a);
                        }
                    }
                    else
                    {
                        for (int i = c; i < n; i++)
                        {
                            mapMarkers.Add(Instantiate(exampleMarker, mapRect).GetComponent<RectTransform>());
                        }
                    }
                }
                RectTransform rt;
                MapPoint mp;
                Vector3 fwd = Vector3.forward, up = Vector3.up;
                float sh = Screen.height / 2f;
                for (int i = 0; i < n;i++)
                {
                    rt = mapMarkers[i];
                    mp = mapPoints[i];
                    rt.GetComponent<Image>().color = mp.interactable ? interactableColor : notInteractableColor;
                    rt.GetChild(0).GetComponent<RawImage>().uvRect = GetMarkerRect(mp.type);
                    Button b = rt.GetComponent<Button>();
                    b.interactable = mp.interactable
;
                    b.onClick.RemoveAllListeners();
                    if (mp.interactable)
                    {                          
                        MapPoint mpLink = mp;
                        b.onClick.AddListener(() => { this.SelectPoint(mpLink); });
                    }
                    rt.localPosition = Quaternion.AngleAxis(mapPoints[i].angle, fwd) * (up * sh * mapPoints[i].height);
                    if (!rt.gameObject.activeSelf) rt.gameObject.SetActive(true);
                }
                mapRect.gameObject.SetActive(true);
            }
            else
            {
                if (mapMarkers.Count > 0)
                {                    
                    for (int i = 0; i < c; i++)
                    {
                        Destroy(mapMarkers[0].gameObject);
                        mapMarkers.RemoveAt(0);
                    }
                }
            }
            lastDrawnStateHash = globalMap.actionsHash;
        }
    }

    private void OnEnable()
    {
        if (globalMap == null)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            UIController.current.gameObject.SetActive(false);
            FollowingCamera.main.gameObject.SetActive(false);
            if (!prepared) Prepare();
            else
            {
                if (lastDrawnStateHash != globalMap.actionsHash) RedrawMarkers();
            }
        }
    }
    // =====================  AUXILIARY METHODS

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
    

    public void LocalizeTitles()
    {
        mapCanvas.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Close);
    }   
}
