using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public sealed class GlobalMapUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Dropdown missionDropdown, shuttlesDropdown;
    [SerializeField] private Image descrButtonImage, expButtonImage;
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RawImage pointIcon;
    [SerializeField] private Text pointLabel, pointDescription, expStatusText, expeditionNameField;
    [SerializeField] private Texture sectorsTexture;
    [SerializeField] private Transform[] rings;
    [SerializeField] private GameObject exampleMarker, infoPanel, sendPanel, teamInfoblock;
    [SerializeField] private GameObject mapCanvas, mapCamera;
#pragma warning restore 0649

    private bool prepared = false, infopanelEnabled = false, descriptionMode = true, showExpeditionInfo = false;
    private float infoPanelWidth = Screen.width;
    private float[] ringsRotation;
    private int lastDrawnStateHash = 0;
    private Expedition showingExpedition;
    private GlobalMap globalMap;
    private MapPoint chosenPoint;
    private List<int> shuttlesListIds = new List<int>();
    private List<RectTransform> mapMarkers;
    private List<MapPoint> mapPoints;
    private RawImage[] sectorsImages;

    private readonly Color notInteractableColor = new Color(0, 1, 1, 0), interactableColor = new Color(0, 1, 1, 0.3f), chosenColor = Color.yellow, inactiveSectorColor = new Color(1,1,1,0.3f);
    private const float ZOOM_BORDER = 9, DIST_BORDER = 1;
    private const int SECTORS_TEXTURE_RESOLUTION = 8000;

    //========================== PUBLIC METHODS

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
                if (descriptionMode) { PreparePointDescription(); return; }
                else { PreparePointExpedition(); return; }
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
        switch (chosenPoint.type) {
            case MapMarkerType.MyCity:
                if (GameMaster.realMaster.colonyController != null) pointLabel.text = GameMaster.realMaster.colonyController.cityName ;break;
            case MapMarkerType.Shuttle:
                {
                    var e = (chosenPoint as FlyingExpedition).expedition;
                    pointLabel.text = '"' + e.crew.name + '"';
                    pointDescription.text =
                        e.mission != null ? Localization.GetWord(LocalizedWord.Mission) + ": " + e.mission.GetName() :
                        Localization.GetWord(LocalizedWord.Return);
                        ;
                    break;
                }
            default: pointLabel.text = Localization.GetMapPointTitle(chosenPoint.type); break;
        }
        pointIcon.uvRect = GetMarkerRect(chosenPoint.type);
        if (descriptionMode) PreparePointDescription();
        else
        {
            if (mp is PointOfInterest)
            {
                if ((mp as PointOfInterest).workingExpeditions == null) expeditionNameField.text = Localization.GetWord(LocalizedWord.Expedition) + ' ' + Expedition.nextID.ToString();
                PreparePointExpedition();
            }
            else
            {
                PreparePointDescription();
            }
        }

        infoPanelWidth = infoPanel.GetComponent<RectTransform>().rect.width;
    }

    public void PreparePointDescription()
    {
        if (chosenPoint != null)
        {            
            descriptionMode = true;

            showExpeditionInfo = false;
            showingExpedition = null;

            if (chosenPoint.type != MapMarkerType.Shuttle) pointDescription.text = Localization.GetMapPointDescription(chosenPoint.type, chosenPoint.subIndex);
            else
            {
                var e = (chosenPoint as FlyingExpedition).expedition;
                pointDescription.text = e.mission != null ? Localization.GetWord(LocalizedWord.Mission) + ": " + e.mission.GetName() :
                        Localization.GetWord(LocalizedWord.Return);
            }
            pointDescription.gameObject.SetActive(true);

            descrButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;
            expButtonImage.overrideSprite = null;
            expButtonImage.gameObject.SetActive(chosenPoint is PointOfInterest);
            if (sendPanel.activeSelf) sendPanel.SetActive(false);

            startButton.gameObject.SetActive(false);

            infoPanel.SetActive(true);
            infopanelEnabled = true;
        }
        else CloseInfopanel();
    }
    public void PreparePointExpedition()
    {
        if (chosenPoint != null)
        {
            PointOfInterest poi = chosenPoint as PointOfInterest;
            if (poi == null)
            {
                PreparePointDescription();
                return;
            }
            else
            {
                descriptionMode = false;
                pointDescription.gameObject.SetActive(false);

                descrButtonImage.overrideSprite = null;
                expButtonImage.overrideSprite = PoolMaster.gui_overridingSprite;
                expButtonImage.gameObject.SetActive(true);

                if (sendPanel.activeSelf) sendPanel.SetActive(false);
                shuttlesDropdown.gameObject.SetActive(false);

                if (poi.workingExpeditions != null)
                { // окно действующей экспедиции
                    showExpeditionInfo = true;
                    showingExpedition = poi.workingExpeditions[0];
                    if (showingExpedition != null)
                    {
                        expeditionNameField.text = '"' + showingExpedition.crew.name + '"';
                        missionDropdown.gameObject.SetActive(false);
                        if (showingExpedition.hasConnection)
                        {
                            switch (showingExpedition.stage)
                            {
                                case Expedition.ExpeditionStage.WayIn:
                                    expStatusText.text = Localization.GetActionLabel(LocalizationActionLabels.FlyingToMissionPoint);
                                    break;
                                case Expedition.ExpeditionStage.OnMission:
                                    expStatusText.text = Localization.GetWord(LocalizedWord.Crew) + ": " + showingExpedition.crew.name + '\n' +
                                        Localization.GetWord(LocalizedWord.Progress) + ": " + ((int)(showingExpedition.progress * 100)).ToString() + "%\n" +
                                        showingExpedition.currentStep.ToString() + " / " + showingExpedition.mission.stepsCount.ToString()
                                        ;
                                    break;
                                case Expedition.ExpeditionStage.WayOut:
                                    expStatusText.text = Localization.GetActionLabel(LocalizationActionLabels.FlyingHome);
                                    break;
                                case Expedition.ExpeditionStage.LeavingMission:
                                    expStatusText.text = Localization.GetActionLabel(LocalizationActionLabels.TryingToLeave);
                                    break;
                                case Expedition.ExpeditionStage.Dismissed:
                                    expStatusText.text = Localization.GetActionLabel(LocalizationActionLabels.Dissmissed);
                                    break;
                            }

                            startButton.interactable = true;
                            startButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.RecallExpedition);
                        }
                        else
                        {
                            expStatusText.text = Localization.GetPhrase(LocalizedPhrase.ConnectionLost);
                            startButton.interactable = false;
                            startButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.RecallExpedition);
                        }
                    }
                    else PreparePointDescription();
                }
                else
                {
                    //окно подготовки экспедиции       
                    showExpeditionInfo = false;
                    showingExpedition = null;
                    var opts = new List<Dropdown.OptionData>();
                    if (poi.exploredPart < 1f) opts.Add(new Dropdown.OptionData(Localization.GetMissionName(MissionType.Exploring)));
                    if (poi.availableMissions != null)
                    {
                        foreach (MissionPreset mp in poi.availableMissions)
                        {
                            opts.Add(new Dropdown.OptionData(Localization.GetMissionName(mp)));
                        }
                    }

                    bool enableStartButton = false;
                    if (opts.Count == 0)
                    {
                        missionDropdown.options = new List<Dropdown.OptionData>() { new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoMission)) };
                        missionDropdown.interactable = false;
                    }
                    else
                    {
                        missionDropdown.options = opts;
                        missionDropdown.interactable = true;
                        enableStartButton = true;
                    }

                    missionDropdown.gameObject.SetActive(true);

                    var shuttlesDropdownList = new List<Dropdown.OptionData>();
                    shuttlesDropdownList.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoCrew)));
                    shuttlesListIds.Clear();
                    shuttlesListIds.Add(-1);
                    if (Shuttle.shuttlesList.Count > 0)
                    {
                        foreach (Shuttle s in Shuttle.shuttlesList)
                        {
                            if (s.crew != null && s.crew.status == Crew.CrewStatus.AtHome)
                            {
                                shuttlesDropdownList.Add(new Dropdown.OptionData(s.crew.name));
                                shuttlesListIds.Add(s.ID);
                            }
                        }
                    }
                    if (shuttlesListIds.Count == 0)
                    {
                        shuttlesDropdownList.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoSuitableShuttles)));
                    }
                    shuttlesDropdown.options = shuttlesDropdownList;
                    shuttlesDropdown.gameObject.SetActive(true);

                    int freeCount = QuantumTransmitter.transmittersList.Count - Expedition.expeditionsList.Count;
                    if (freeCount > 0) expStatusText.text = Localization.GetPhrase(LocalizedPhrase.UnoccupiedTransmitters) + freeCount.ToString();
                    else expStatusText.text = Localization.GetPhrase(LocalizedPhrase.NoTransmitters);
                    
                    teamInfoblock.gameObject.SetActive(false);
                    startButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Launch);
                    startButton.interactable = (shuttlesListIds.Count > 0 & enableStartButton & freeCount > 0);
                }
                startButton.gameObject.SetActive(true);
                sendPanel.SetActive(true);
                if (!infopanelEnabled)
                {
                    infoPanel.SetActive(true);
                    infopanelEnabled = true;
                }
            }
        }
        else CloseInfopanel();
    }

    public void StartButton()
    {
        if (chosenPoint != null)
        {
            if (!descriptionMode)
            {
                var poi = chosenPoint as PointOfInterest;
                if (showingExpedition == null)
                { //отправить экспедицию
                    Shuttle s = Shuttle.GetShuttle(shuttlesListIds[shuttlesDropdown.value]);
                    if (s != null)
                    {
                        var q = QuantumTransmitter.GetFreeTransmitter();
                        if (q != null) {
                            Mission m = null;
                            if (missionDropdown.value != 0) m = poi.GetMissionByIndex(missionDropdown.value - 1);
                            else m = new Mission(MissionPreset.ExploringPreset, poi);
                            if (m != null) Expedition.CreateNewExpedition(s.crew, m,q, poi, expeditionNameField.text);
                        }
                    }
                }
                else
                { //вернуть экспедицию
                    if (showingExpedition.mission != null) showingExpedition.EndMission();
                }
                PreparePointExpedition();
            }
            else PreparePointDescription();
        }
        else CloseInfopanel();
    }
    public void CloseInfopanel()
    {
        infoPanel.SetActive(false);
        infopanelEnabled = false;        
        infoPanelWidth = 0;
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
        gameObject.SetActive(false);
        UIController.current.gameObject.SetActive(true);
        FollowingCamera.main.gameObject.SetActive(true);
    }   
    // ======================== PRIVATE METHODS
    private void Start()
    {
        if (!prepared & gameObject.activeSelf) Prepare();
    }
    private void Prepare()
    {
        if (globalMap == null) return;
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
                    ri.color = sectorsData[k].environment.lightSettings.sunColor;
                }
                sectorsImages[k] = ri;
                k++;
            }
        }
        mapRect.gameObject.SetActive(true);

        prepared = true;
        mapMarkers = new List<RectTransform>();
        RedrawMap();
        if (infoPanel.activeSelf) infoPanelWidth = infoPanel.GetComponent<RectTransform>().rect.width;
        else infoPanelWidth = 0;
        infoPanel.SetActive(false);
        LocalizeTitles();
    }
    private void RedrawMap()
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
                    b.onClick.AddListener(() => { this.SelectPoint(mpLink); });

                    rt.localPosition = Quaternion.AngleAxis(mapPoints[i].angle, dir) * (up * mapPoints[i].height);

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

            RingSector[] sectorsData = globalMap.mapSectors;
            for (int i = 0; i < sectorsData.Length; i++)
            {
                if (sectorsData[i] != null)
                {
                    var mp = sectorsData[i].centralPoint;
                    if (mp.type != MapMarkerType.Star) sectorsImages[i].color = sectorsData[i].environment.lightSettings.sunColor;
                    else sectorsImages[i].color = (mp as SunPoint).color;
                }
                else
                {
                    sectorsImages[i].color = inactiveSectorColor;
                }
            }

            lastDrawnStateHash = globalMap.actionsHash;
        }
    }

    private void Update()
    {
        if (!prepared) return;
        float t = Time.deltaTime * GameMaster.gameSpeed;
        Vector3 dir = Vector3.back;
        for (int i = 0; i < GlobalMap.RINGS_COUNT; i++)
        {
            rings[i].transform.rotation = Quaternion.Euler(0,0, ringsRotation[i]);
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
            }
        }

        if (infopanelEnabled)
        {
            if (chosenPoint != null)
            {
                PointOfInterest poi = chosenPoint as PointOfInterest;
                if (poi != null)
                {
                    if (showingExpedition != null)
                    {
                        if (!descriptionMode)
                        {
                            if (showExpeditionInfo)
                            {
                                if (showingExpedition.hasConnection)
                                {
                                    if (showingExpedition.stage == Expedition.ExpeditionStage.OnMission)
                                    {
                                        expStatusText.text = Localization.GetWord(LocalizedWord.Crew) + ": " + showingExpedition.crew.name + '\n' +
                                       Localization.GetWord(LocalizedWord.Progress) + ": " + showingExpedition.currentStep + '/' + showingExpedition.mission.stepsCount;
                                    }
                                    else
                                    {
                                        if (showingExpedition.stage == Expedition.ExpeditionStage.Dismissed)
                                        {
                                            PreparePointExpedition();
                                        }
                                    }
                                }
                            }
                            else PreparePointExpedition();
                        }
                    }
                    else
                    {
                        if (!descriptionMode & showExpeditionInfo)  PreparePointExpedition();
                    }
                }
            }
            else CloseInfopanel();
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
            UIController.current.gameObject.SetActive(false);
            FollowingCamera.main.gameObject.SetActive(false);
            if (!prepared) Prepare();
            else
            {
                if (lastDrawnStateHash != globalMap.actionsHash) RedrawMap();
                if (infoPanel.activeSelf) infoPanelWidth = infoPanel.GetComponent<RectTransform>().rect.width;
                else infoPanelWidth = 0;
            }
        }
    }
    private void OnDisable()
    {
        if (globalMap != null) globalMap.MapInterfaceDisabled();
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
            case MapMarkerType.Shuttle: return new Rect(2 * p, p, p, p);
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
