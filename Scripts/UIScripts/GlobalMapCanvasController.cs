using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public sealed class GlobalMapCanvasController : MonoBehaviour, IObserverController
{// prev GlobalMapUI
#pragma warning disable 0649
    [SerializeField] private RectTransform mapRect, expeditionFastButtonsPanel;
    [SerializeField] private Button[] expeditionsFastButtons;
    [SerializeField] private RawImage pointIcon;
    [SerializeField] private Text pointLabel, pointDescription;
    [SerializeField] private Texture sectorsTexture;
    [SerializeField] private Transform[] rings;
    [SerializeField] private Transform enginePanel;
    [SerializeField] private GameObject exampleMarker, infoPanel, sendExpeditionButton;
    [SerializeField] private GameObject mapCanvas, mapCamera;
#pragma warning restore 0649
    private Text sendButtonLabel { get { return sendExpeditionButton.transform.GetChild(0).GetComponent<Text>(); } }

    public static bool needExpeditionsRedraw = false; 
    private bool prepared = false, showExpeditionInfo = false;
    private float infoPanelWidth = 0f;
    private float[] ringsRotation;
    private int lastDrawnStateHash = 0;
    private GlobalMap globalMap;
    private MapPoint chosenPoint;
    private List<RectTransform> mapMarkers;
    private List<MapPoint> mapPoints;
    private RawImage[] sectorsImages;
    private UIController uicontroller;

    private readonly Color notInteractableColor = new Color(0, 1, 1, 0), interactableColor = new Color(0, 1, 1, 0.3f), chosenColor = Color.yellow, inactiveSectorColor = new Color(1,1,1,0.3f);
    private const float ZOOM_BORDER = 9, DIST_BORDER = 1;
    private const int SECTORS_TEXTURE_RESOLUTION = 8000, MAX_EXPEDITIONS_FBUTTONS_COUNT = 20;

    //========================== PUBLIC METHODS
    public Transform GetMainCanvasTransform() { return mapCanvas.transform; }

    public void SetGlobalMap(GlobalMap gm)
    {
        if (gm == null) return;
        globalMap = gm;
        ringsRotation = globalMap.ringsRotation;
        mapPoints = globalMap.mapPoints;
    }
    public void SelectPoint(MapPoint mp)
    {
        if (chosenPoint != null)
        {
            if (mp != chosenPoint)
            {
                for (int i = 0; i < mapPoints.Count; i++)
                {
                    if (mapPoints[i] == chosenPoint)
                    {
                        mapMarkers[i].GetComponent<Image>().color = mapPoints[i] is PointOfInterest ? interactableColor : notInteractableColor;
                        break;
                    }
                }
                chosenPoint = null;
            }
            else
            {  //повторный выбор точки
                PreparePointDescription();
                return;
            }
        }
        chosenPoint = mp;
        for (int i = 0; i < mapPoints.Count; i++)
        {
            if (mapPoints[i] == chosenPoint)
            {
                mapMarkers[i].GetComponent<Image>().color = chosenColor;
                break;
            }
        }
        PreparePointDescription();
        infoPanelWidth = infoPanel.activeSelf ? infoPanel.GetComponent<RectTransform>().rect.width : 0f;
    }
    public void SelectExpedition(int ID)
    {
        FlyingExpedition fe;
        if (mapPoints.Count > 0)
        {
            var e = Expedition.GetExpeditionByID(ID);
            if (e != null)
            {
                if (e.stage == Expedition.ExpeditionStage.WayIn | e.stage == Expedition.ExpeditionStage.WayOut)
                {
                    foreach (var p in mapPoints)
                    {
                        if (p.type == MapMarkerType.FlyingExpedition)
                        {
                            fe = p as FlyingExpedition;
                            if (fe != null && fe.expedition.ID == ID)
                            {
                                SelectPoint(p);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (e.stage == Expedition.ExpeditionStage.OnMission)
                    {
                        int sid = e.destination.ID;
                        foreach (var p in mapPoints)
                        {
                            if (p.ID == sid)
                            {
                                SelectPoint(p);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    public void PreparePointDescription()
    {
        if (chosenPoint == null)
        {
            if (infoPanel.activeSelf) infoPanel.SetActive(false);
            return;
        }
        pointIcon.uvRect = GetMarkerRect(chosenPoint.type);        
        if (chosenPoint != null)
        {
            switch (chosenPoint.type)
            {
                case MapMarkerType.MyCity:
                    {
                        if (GameMaster.realMaster.colonyController != null) pointLabel.text = GameMaster.realMaster.colonyController.cityName;
                        else pointLabel.text = Localization.GetPhrase(LocalizedPhrase.YouAreHere);
                        pointDescription.text = Localization.GetMyColonyDescription();
                        if (sendExpeditionButton.activeSelf) sendExpeditionButton.SetActive(false);
                        break;
                    }
                case MapMarkerType.FlyingExpedition:
                    {
                        var e = (chosenPoint as FlyingExpedition).expedition;
                        pointLabel.text = Localization.GetExpeditionName(e);
                        pointDescription.text = Localization.GetExpeditionDescription(e);
                        switch (e.stage)
                        {
                            case Expedition.ExpeditionStage.WayIn:
                                sendButtonLabel.text = Localization.GetPhrase(LocalizedPhrase.RecallExpedition);
                                if (!sendExpeditionButton.activeSelf) sendExpeditionButton.SetActive(true);
                                break;
                            case Expedition.ExpeditionStage.WayOut:
                                if (sendExpeditionButton.activeSelf) sendExpeditionButton.SetActive(false);
                                break;
                            case Expedition.ExpeditionStage.OnMission:
                            case Expedition.ExpeditionStage.LeavingMission:                                
                                SelectPoint(e.destination);
                                return;
                            case Expedition.ExpeditionStage.Dismissed:
                            case Expedition.ExpeditionStage.Disappeared:
                                CloseInfopanel();
                                return;
                        }                    
                        break;
                    }
                default:
                    {
                        pointLabel.text = Localization.GetMapPointTitle(chosenPoint.type);
                        
                        var poi = chosenPoint as PointOfInterest;
                        if (poi != null)
                        {
                            var s = chosenPoint.GetDescription() +
                                    "\n\n" + Localization.GetWord(LocalizedWord.Difficulty) + ": " + ((int)(poi.difficulty * 100f)).ToString() + '%';
                            if (poi.workingExpedition == null)
                            {
                                sendButtonLabel.text = Localization.GetPhrase(LocalizedPhrase.SendExpedition);
                                if (!sendExpeditionButton.activeSelf) sendExpeditionButton.SetActive(true);
                            }
                            else
                            {
                                s += '\n' + Localization.GetWord(LocalizedWord.Expedition) + ": " + poi.workingExpedition.crew.name;
                                sendButtonLabel.text = Localization.GetPhrase(LocalizedPhrase.OpenExpeditionWindow);
                                if (!sendExpeditionButton.activeSelf) sendExpeditionButton.SetActive(true);
                            }
                            pointDescription.text = s;

                        }
                        else
                        {
                            pointDescription.text = chosenPoint.GetDescription();
                            if (sendExpeditionButton.activeSelf) sendExpeditionButton.SetActive(false);
                        }
                        break;
                    }
            }

            if (!infoPanel.activeSelf) infoPanel.SetActive(true);
        }
        else CloseInfopanel();
    }

    public void StartButton()
    {
        if (chosenPoint != null)
        {
            if (chosenPoint.type == MapMarkerType.FlyingExpedition)
            {
                var e = (chosenPoint as FlyingExpedition).expedition;
                if (e.stage == Expedition.ExpeditionStage.WayIn) e.EndMission();
            }
            else
            {
                var poi = chosenPoint as PointOfInterest;
                if (poi != null)
                {
                    if (poi.workingExpedition == null || poi.workingExpedition.stage != Expedition.ExpeditionStage.OnMission)
                    { // send expedition
                        var rt = infoPanel.GetComponent<RectTransform>();
                        infoPanelWidth = infoPanel.activeSelf ? rt.rect.width * rt.localScale.x : 0f;
                        float pw = (Screen.width - infoPanelWidth) * 0.95f,
                        ph = Screen.height * 0.75f, sz = ph;
                        if (pw < ph) sz = pw;
                        UIExpeditionObserver.Show(mapCanvas.GetComponent<RectTransform>(), new Rect(Vector2.zero, sz * Vector2.one), 
                            SpriteAlignment.Center, poi, true);
                    }
                    else
                    {
                        uicontroller.ShowExpedition(poi.workingExpedition);
                        CloseInfopanel();
                        return;
                    }
                }
                else
                {
                    sendExpeditionButton.SetActive(false);
                }
            }
        }
        else CloseInfopanel();
    }
    public void CloseInfopanel()
    {
        if (infoPanel.activeSelf) infoPanel.SetActive(false);    
        infoPanelWidth = 0f;
        if (chosenPoint != null & mapMarkers.Count > 0)
        for (int i = 0; i < mapMarkers.Count; i++)
        {
            if (mapPoints[i] == chosenPoint)
                {
                    mapMarkers[i].GetComponent<Image>().color = mapPoints[i] is PointOfInterest ? interactableColor : notInteractableColor;
                    break;
                }
        }
        chosenPoint = null;
    }
    public void Close()
    {
        CloseInfopanel();
        uicontroller.ChangeUIMode(UIMode.Standart, true);
    }
    // ======================== PRIVATE METHODS
    private void Start()
    {
        if (!prepared & gameObject.activeSelf) Prepare();
    }
    private void Prepare()
    {
        if (globalMap == null) return;
        uicontroller = UIController.GetCurrent();
        transform.position = Vector3.up * 0.1f;

        mapRect.gameObject.SetActive(false);
        GameObject sector;
        RingSector[] sectorsData = globalMap.mapSectors;
        sectorsImages = new RawImage[sectorsData.Length];
        int k = 0;
        for (int ring = 0; ring < GlobalMap.RINGS_COUNT; ring++)
        {
            float sectorDegree = globalMap.sectorsDegrees[ring];
            int sectorsCount = (int)(360f / sectorDegree);
            for (int i = 0; i < sectorsCount; i++)
            {
                sector = new GameObject("ring " + ring.ToString() + ", sector " + i.ToString());
                RawImage ri = sector.AddComponent<RawImage>();
                ri.raycastTarget = false;
                PrepareSector(ri, ring);
                sector.transform.parent = rings[ring];
                sector.transform.localRotation = Quaternion.Euler(Vector3.back * i * sectorDegree);
                if (sectorsData[k] != null)
                {
                    ri.color = sectorsData[k].environment.GetMapColor();
                }
                sectorsImages[k] = ri;
                k++;
            }
        }
        mapRect.gameObject.SetActive(true);

        prepared = true;
        mapMarkers = new List<RectTransform>();
        RedrawMap();
        infoPanel.SetActive(false); infoPanelWidth = 0f;
        LocalizeTitles();
    }
    public void RedrawMap()
    {
        if (!prepared) return;
        else
        {
            mapPoints = globalMap.mapPoints;
            int n = mapPoints.Count;
            //print(n);
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
                Vector3 dir = Vector3.back, up = Vector3.up * mapRect.rect.height / 2f;
                bool checkForChosen = (chosenPoint != null);
                for (int i = 0; i < n; i++)
                {
                    mp = mapPoints[i];
                    rt = mapMarkers[i];                    
                    if (checkForChosen && mp == chosenPoint) rt.GetComponent<Image>().color = chosenColor;
                    else
                    {
                        rt.GetComponent<Image>().color = mp is PointOfInterest ? interactableColor : notInteractableColor;
                    }
                    rt.GetChild(0).GetComponent<RawImage>().uvRect = GetMarkerRect(mp.type);
                    Button b = rt.GetComponent<Button>(); ;
                    b.onClick.RemoveAllListeners();
                    MapPoint mpLink = mp;
                    b.onClick.AddListener(() => { this.SelectPoint(mpLink); }); // переписываем каждый раз, поскольку массив видимых точек соответствует массиву всех точек по индексам

                    rt.localPosition = Quaternion.AngleAxis(mapPoints[i].angle, dir) * (up * mapPoints[i].height);

                    if (!rt.gameObject.activeSelf) rt.gameObject.SetActive(true);
                }
                mapRect.gameObject.SetActive(true);
                //
                ExpeditionsButtonsRedraw();
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
                expeditionFastButtonsPanel.gameObject.SetActive(false);
            }

            RingSector[] sectorsData = globalMap.mapSectors;
            for (int i = 0; i < sectorsData.Length; i++)
            {
                if (sectorsData[i] != null)
                {
                    var mp = sectorsData[i].centralPoint;
                    if (mp.type != MapMarkerType.Star) sectorsImages[i].color = sectorsData[i].environment.GetMapColor();
                    else sectorsImages[i].color = (mp as SunPoint).color;
                }
                else
                {
                    sectorsImages[i].color = inactiveSectorColor;
                }
            }

            if (globalMap.engineThrust > 0f)
            {
                if (!enginePanel.gameObject.activeSelf) enginePanel.gameObject.SetActive(true);
            }
            else
            {
                if (enginePanel.gameObject.activeSelf) enginePanel.gameObject.SetActive(false);
            }

            lastDrawnStateHash = globalMap.actionsHash;
        }
    }
    public void ExpeditionsButtonsRedraw()
    {
        var elist = Expedition.expeditionsList;
        var c = elist.Count;
        if (c > 0)
        {
            int i = 0, l = expeditionsFastButtons.Length;
            Button b;
            Expedition e;
            if (c != l)
            {
                if (c > l)
                { //увеличение списка
                    var nb = new Button[c];
                    for (; i < l; i++)
                    {
                        b = expeditionsFastButtons[i];
                        b.onClick.RemoveAllListeners();
                        e = elist[i];
                        int id = e.ID;
                        b.onClick.AddListener(() => this.SelectExpedition(id));
                        b.transform.GetChild(0).GetComponent<RawImage>().uvRect =
                            GlobalMapCanvasController.GetMarkerRect(e.GetDestinationIcon());
                        b.gameObject.SetActive(true);
                        nb[i] = b;
                    }
                    RectTransform rt = null;
                    for (; i < c; i++)
                    {
                        b = Instantiate(expeditionsFastButtons[0], expeditionFastButtonsPanel);
                        rt = b.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0f, 0.95f - 0.05f * i);
                        rt.anchorMax = new Vector2(0f, 1f - i * 0.05f);
                        rt.anchoredPosition = new Vector2(rt.sizeDelta.x / 2f, 0f);
                        b.onClick.RemoveAllListeners();
                        e = elist[i];
                        int id = e.ID;
                        b.onClick.AddListener(() => this.SelectExpedition(id));
                        b.transform.GetChild(0).GetComponent<RawImage>().uvRect = GlobalMapCanvasController.GetMarkerRect(e.GetDestinationIcon());
                        b.gameObject.SetActive(true);
                        nb[i] = b;
                    }
                    expeditionsFastButtons = nb;
                }
                else
                { // сужение списка
                    for (i = 0; i < c; i++)
                    {
                        b = expeditionsFastButtons[i];
                        b.onClick.RemoveAllListeners();
                        e = elist[i];
                        int id = e.ID;
                        b.onClick.AddListener(() => this.SelectExpedition(id));
                        b.transform.GetChild(0).GetComponent<RawImage>().uvRect =
                           GlobalMapCanvasController.GetMarkerRect(e.GetDestinationIcon());
                        b.gameObject.SetActive(true);
                    }
                    for (; i < l; i++)
                    {
                        expeditionsFastButtons[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            { // просто перезапись
                for (i = 0; i < l; i++)
                {
                    b = expeditionsFastButtons[i];
                    b.onClick.RemoveAllListeners();
                    e = elist[i];
                    int id = e.ID;
                    b.onClick.AddListener(() => this.SelectExpedition(id));
                    b.transform.GetChild(0).GetComponent<RawImage>().uvRect = GetMarkerRect(e.GetDestinationIcon());
                    b.gameObject.SetActive(true);
                }
            }
            expeditionFastButtonsPanel.gameObject.SetActive(true);
        }
        else expeditionFastButtonsPanel.gameObject.SetActive(false);
        needExpeditionsRedraw = false;
        PreparePointDescription();
    }

    private void Update()
    {
        if (!prepared) return;
        float t = Time.deltaTime * GameMaster.gameSpeed;
        Vector3 dir = Vector3.back;
        for (int i = 0; i < GlobalMap.RINGS_COUNT; i++)
        {
            rings[i].transform.rotation = Quaternion.Euler(0, 0, ringsRotation[i]);
        }
        //
        if (lastDrawnStateHash != globalMap.actionsHash)
        {
            RedrawMap();
        }
        else
        {           
            if (mapMarkers.Count > 0)
            {
                Vector3 up = Vector3.up * mapRect.rect.height / 2f;
                for (int i = 0; i < mapMarkers.Count; i++)
                {
                    MapPoint mp = mapPoints[i];
                    mapMarkers[i].localPosition = Quaternion.AngleAxis(mp.angle, dir) * (up * mp.height);
                }
                if (needExpeditionsRedraw)   ExpeditionsButtonsRedraw();
            }
        }
        //

        float deltaX = 0, deltaY = 0, deltaZoom = 0;
        if (FollowingCamera.touchscreen)
        {
            if (Input.touchCount > 0)
            {
                Touch tc = Input.GetTouch(0);
                if (Input.touchCount == 2)
                {
                    Touch tc2 = Input.GetTouch(1);
                    Vector2 tPrevPos = tc.position - tc.deltaPosition;
                    Vector2 t2PrevPos = tc2.position - tc2.deltaPosition;
                    deltaZoom = ((tPrevPos - t2PrevPos).magnitude - (tc.position - tc2.position).magnitude) / (float)Screen.height * (-2);
                }
                else
                {
                    if (Input.touchCount == 1)
                    {
                        if (tc.phase == TouchPhase.Began | tc.phase == TouchPhase.Moved)
                        {
                            float delta = tc.deltaPosition.x / (float)Screen.width * 10;
                            float swp = Screen.width / 10f;
                            deltaX = tc.deltaPosition.x / swp;
                            deltaY = tc.deltaPosition.y / swp;
                        }
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(2))
            {
                deltaX = Input.GetAxis("Mouse X");
                deltaY = Input.GetAxis("Mouse Y");
            }
            deltaZoom = Input.GetAxis("Mouse ScrollWheel");
        }

        if (deltaZoom != 0)
        {
            float newScale = mapRect.localScale.y + deltaZoom * mapRect.localScale.y;
            if (newScale > ZOOM_BORDER) newScale = ZOOM_BORDER;
            else
            {
                if (newScale < DIST_BORDER) newScale = DIST_BORDER;
            }
            Vector3 one = Vector3.one;
            if (newScale != mapRect.localScale.x)
            {
                mapRect.localScale = one * newScale;
            }
            if (mapMarkers.Count > 0)
            {
                foreach (RectTransform marker in mapMarkers)
                {
                    marker.localScale = one * (1f / newScale);
                }
            }

        }
        float xpos = mapRect.position.x, ypos = mapRect.position.y;
        float sw = Screen.width - infoPanelWidth;
        int sh = Screen.height;
        float radius = mapRect.rect.width * mapRect.localScale.x / 2f;

        if (2 * radius <= sw)
        {
            xpos = sw / 2f;
        }
        else
        {
            if (deltaX != 0)
            {
                xpos += deltaX * 30;
            }
            float leftExpart = xpos - radius;
            float rightExpart = sw - (xpos + radius);
            if (leftExpart > 0)
            {
                if (rightExpart < 0)
                {
                    xpos = radius;
                }
            }
            else
            {
                if (rightExpart > 0)
                {
                    xpos = sw - (radius);
                }
            }
        }

        if (2 * radius <= sh)
        {
            ypos = sh / 2f;
        }
        else
        {
            if (deltaY != 0)
            {
                ypos += deltaY * 30;

            }
            float upExpart = sh - ypos - radius;
            float downExpart = ypos - radius;
            if (upExpart > 0)
            {
                if (downExpart < 0) ypos = sh - radius;
            }
            else
            {
                if (downExpart > 0) ypos = radius;
            }
        }

        mapRect.position = new Vector3(xpos, ypos, 0);        
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
            if (!prepared) Prepare();
            else
            {
                if (lastDrawnStateHash != globalMap.actionsHash) RedrawMap();
                if (infoPanel.activeSelf) infoPanelWidth = infoPanel.GetComponent<RectTransform>().rect.width;
                else infoPanelWidth = 0f;
            }
        }
    }
    // =====================  AUXILIARY METHODS
    public static Rect GetMarkerRect(MapMarkerType mtype)
    {
        float p = 0.25f;
        switch (mtype)
        {
            case MapMarkerType.MyCity: return new Rect(p, 0, p, p);
            case MapMarkerType.Station: return new Rect(0, p, p, p);
            case MapMarkerType.Wreck: return new Rect(p, p, p, p);
            case MapMarkerType.FlyingExpedition: return new Rect(2 * p, p, p, p);
            case MapMarkerType.Island: return new Rect(3 * p, p, p, p);
            case MapMarkerType.SOS: return new Rect(0, 2 * p, p, p);
            case MapMarkerType.Portal: return new Rect(p, 2 * p, p, p);
            case MapMarkerType.QuestMark: return new Rect(2 * p, 2 * p, p, p);
            case MapMarkerType.Colony: return new Rect(3 * p, 2 * p, p, p);
            case MapMarkerType.Star: return new Rect(0, 3 * p, p, p);
            case MapMarkerType.Wiseman: return new Rect(p, 3 * p, p, p);
            case MapMarkerType.Wonder: return new Rect(2 * p, 3 * p, p, p);
            case MapMarkerType.Resources: return new Rect(3 * p, 3 * p, p, p);
            case MapMarkerType.Unknown:
            default:
                return new Rect(0, 0, p, p);
        }
    }   
    private void PrepareSector(RawImage ri, int ringIndex)
    {
        float p = 0.1f;       
        Rect rect = Rect.zero;        
        switch (ringIndex)
        {
            case 0:
                {
                    int i = (int)(Random.value * 8f);
                    float r = 2 * p;
                    switch (i)
                    {
                        case 0: rect = new Rect(0,0, r * 0.99f, r * 0.99f); break;
                        case 1: rect = new Rect(2f *p, 0, r * 0.99f, r * 0.99f); break;
                        case 2: rect = new Rect(4f * p, 0, r * 0.99f, r * 0.99f); break;
                        case 3: rect = new Rect(6f * p, 0,r * 0.99f, r * 0.99f); break;
                        case 4: rect = new Rect(8f * p, 0,r * 0.99f, r * 0.99f); break;
                        case 5: rect = new Rect(8f * p,r, r * 0.99f, r * 0.99f); break;
                        case 6: rect = new Rect(8f * p, 4f * p, r * 0.99f, r * 0.99f); break;
                        case 7: rect = new Rect(8f * p, 6f * p, r * 0.99f, r * 0.99f); break;
                    }                    
                    break;
                }               
            case 1:
                {
                    int i = (int)(Random.value * 7f);
                    float r = 2 * p;
                    switch (i)
                    {
                        case 0: rect = new Rect(0,r, r * 0.99f, r * 0.99f); break;
                        case 1: rect = new Rect(2f * p,r, r * 0.99f, r * 0.99f); break;
                        case 2: rect = new Rect(4f * p,r, r * 0.99f, r * 0.99f); break;
                        case 3: rect = new Rect(6f * p,r, r * 0.99f, r * 0.99f); break;
                        case 4: rect = new Rect(6f * p, 4f * p, r * 0.99f, r * 0.99f); break;
                        case 5: rect = new Rect(6f * p, 6f * p, r * 0.99f, r * 0.99f); break;
                        case 6: rect = new Rect(6f * p, 8f * p, r * 0.99f, r * 0.99f); break;
                    }
                    break;
                }
            case 2:
                {
                    int i = (int)(Random.value * 10f);
                    float r = 1.5f * p;
                    switch (i)
                    {
                        case 0: rect = new Rect(0f, 8.5f * p, r * 0.99f, r * 0.99f); break;
                        case 1: rect = new Rect(1.5f * p, 8.5f * p, r * 0.99f, r * 0.99f); break;
                        case 2: rect = new Rect(3f * p, 8.5f * p, r * 0.99f, r * 0.99f); break;
                        case 3: rect = new Rect(4.5f * p, 8.5f * p, r * 0.99f, r * 0.99f); break;
                        case 4: rect = new Rect(0f, 7 * p, r * 0.99f, r * 0.99f); break;
                        case 5: rect = new Rect(1.5f * p, 7 * p, r * 0.99f, r * 0.99f); break;
                        case 6: rect = new Rect(3f * p, 7 * p, r * 0.99f, r * 0.99f); break;
                        case 7: rect = new Rect(4.5f * p, 7 * p, r * 0.99f, r * 0.99f); break;
                        case 8: rect = new Rect(0f, 5.5f * p, r * 0.99f, r * 0.99f); break;
                        case 9: rect = new Rect(1.5f * p, 5.5f * p, r * 0.99f, r * 0.99f); break;
                    }
                    break;
                }
            case 3:
                {
                    int i = (int)(Random.value * 6f);
                    float r = 1.5f * p;
                    switch (i)
                    {
                        case 0: rect = new Rect(3f * p, 5.5f * p, r * 0.99f, r * 0.99f); break;
                        case 1: rect = new Rect(4.5f * p, 5.5f * p, r * 0.99f, r * 0.99f); break;
                        case 2: rect = new Rect(0f, 4 * p, r * 0.99f, r * 0.99f); break;
                        case 3: rect = new Rect(1.5f * p, 4 * p, r * 0.99f, r * 0.99f); break;
                        case 4: rect = new Rect(3f * p, 4 * p, r * 0.99f, r * 0.99f); break;
                        case 5: rect = new Rect(4.5f * p, 4 * p, r * 0.99f, r * 0.99f); break;
                    }
                    break;
                }
            case 4:
                {
                    int i = (int)(Random.value * 4);
                    switch(i)
                    {
                        case 0: rect = new Rect(8f * p, 9f * p, p * 0.99f, p * 0.99f); break;
                        case 1: rect = new Rect(9f * p, 9f * p, p * 0.99f, p * 0.99f); break;
                        case 2: rect = new Rect(8f * p, 8f * p, p * 0.99f, p * 0.99f); break;
                        case 3: rect = new Rect(9f * p, 8f * p, p * 0.99f, p * 0.99f); break;
                    }
                    break;
                }
        }
        ri.texture = sectorsTexture;
        ri.uvRect = rect;
        RectTransform rt = ri.rectTransform;
        rt.parent = rings[ringIndex];

        float s = Mathf.Sin(globalMap.sectorsDegrees[ringIndex] * Mathf.Deg2Rad);
        float size = mapRect.rect.height / 2f * globalMap.ringsBorders[ringIndex] * s;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        rt.pivot = Vector3.down * (1f / s - 1);
        rt.localPosition = Vector3.zero;

    }

    public void LocalizeTitles()
    {
      
    }
}
