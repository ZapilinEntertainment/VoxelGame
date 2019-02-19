using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GlobalMapUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Dropdown missionDropdown, shuttlesDropdown;
    [SerializeField] private Image descrButtonImage, expButtonImage;
    [SerializeField] private InputField expeditionNameField;
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RawImage pointIcon;
    [SerializeField] private Text pointLabel, pointDescription, expStatusText;
    [SerializeField] private Transform[] rings;
    [SerializeField] private GameObject exampleMarker, infoPanel, sendPanel, teamInfoblock;
    [SerializeField] private GameObject mapCanvas, mapCamera;
#pragma warning restore 0649

    private bool prepared = false, infopanelEnabled = false, descriptionMode = true;
    private float infoPanelWidth = Screen.width;
    private float[] rotationSpeed;
    private int lastDrawnStateHash = 0;
    private GlobalMap globalMap;
    private MapPoint chosenPoint;
    private List<int> shuttlesListIds = new List<int>();
    private List<RectTransform> mapMarkers;
    private List<MapPoint> mapPoints;

    private readonly Color notInteractableColor = new Color(0, 1, 1, 0), interactableColor = new Color(0, 1, 1, 0.3f), chosenColor = Color.yellow;
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
                infoPanel.SetActive(false);
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
            case MapMarkerType.MyCity: if (GameMaster.realMaster.colonyController != null) pointLabel.text = GameMaster.realMaster.colonyController.cityName ;break;
            case MapMarkerType.Shuttle:
                {
                    // вернуть flying expedition
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
                if ((mp as PointOfInterest).sentExpedition == null) expeditionNameField.text = Localization.GetWord(LocalizedWord.Expedition) + ' ' + Expedition.lastUsedID.ToString();
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
            pointDescription.text = Localization.GetMapPointDescription(chosenPoint.type, chosenPoint.subIndex);
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
                if (poi.sentExpedition != null)
                { // окно действующей экспедиции
                    expeditionNameField.text = poi.sentExpedition.name;
                    missionDropdown.gameObject.SetActive(false);
                    expStatusText.text = Localization.GetWord(LocalizedWord.Crew) + ": " + poi.sentExpedition.crew.name + '\n' +
                        Localization.GetWord(LocalizedWord.Progress) + ": " + ((poi.sentExpedition.progress * 100) / 100).ToString() + '%';

                    startButton.interactable = true;
                    startButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.RecallExpedition);
                }
                else
                {
                    //окно подготовки экспедиции                   
                    var mdrop = poi.GetAvailableMissionsDropdownData();
                    if (mdrop.Count > 0)
                    {
                        missionDropdown.options = mdrop;
                    }
                    else
                    {
                        missionDropdown.options = new List<Dropdown.OptionData>() { new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoMission)) };
                    }
                    missionDropdown.gameObject.SetActive(true);

                    var shuttlesDropdownList = new List<Dropdown.OptionData>();
                    shuttlesDropdownList.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoShuttle)));
                    shuttlesListIds.Clear();
                    shuttlesListIds.Add(-1);
                    if (Shuttle.shuttlesList.Count > 0)
                    {
                        foreach (Shuttle s in Shuttle.shuttlesList)
                        {
                            if (s.crew != null && s.crew.status == CrewStatus.Free)
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
                    startButton.interactable = (shuttlesListIds.Count > 0 & mdrop.Count > 0 & freeCount > 0);
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
    public void RenameExpedition(string s)
    {
        if (chosenPoint != null)
        {
            PointOfInterest poi = chosenPoint as PointOfInterest;
            if (poi != null && poi.sentExpedition != null)
            {
                poi.sentExpedition.name = s;
            }
            PreparePointExpedition();
        }
        else  CloseInfopanel();
    }

    public void StartButton()
    {
        if (chosenPoint != null)
        {
            if (!descriptionMode)
            {
                var poi = chosenPoint as PointOfInterest;
                if (poi.sentExpedition == null)
                { //отправить экспедицию
                    Shuttle s = Shuttle.GetShuttle(shuttlesListIds[shuttlesDropdown.value]);
                    if (s != null)
                    {
                        Expedition.CreateNewExpedition(s.crew, poi.GetMission(missionDropdown.value), QuantumTransmitter.GetFreeTransmitter(), poi, expeditionNameField.text);
                    }
                }
                else
                { //вернуть экспедицию
                    poi.sentExpedition.EndMission();
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
                float shp = Screen.height / 2f;
                for (int i = 0; i < mapMarkers.Count; i++)
                {
                    MapPoint mp = mapPoints[i];
                    mapMarkers[i].localPosition = Quaternion.AngleAxis(mp.angle, fwd) * (up * shp * mp.height);
                }
            }
        }

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
            float newScale = mapRect.localScale.y + deltaZoom;
            if (newScale > ZOOM_BORDER) newScale = ZOOM_BORDER;
            else
            {
                if (newScale < DIST_BORDER) newScale = DIST_BORDER;
            }
            if (newScale != mapRect.localScale.x)
            {
                mapRect.localScale = Vector3.one * newScale;
            }
        }

        float xpos = mapRect.position.x, ypos = mapRect.position.y;
        float radius = mapRect.rect.width * mapRect.localScale.x / 2f;
        float sw = Screen.width - infoPanelWidth;
        int sh = Screen.height;
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
        if (infoPanel.activeSelf) infoPanelWidth = infoPanel.GetComponent<RectTransform>().rect.width;
        else infoPanelWidth = 0;
        infoPanel.SetActive(false);
        LocalizeTitles();
    }
    private void RedrawMarkers()
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
                Vector3 fwd = Vector3.forward, up = Vector3.up;
                bool checkForChosen = (chosenPoint != null);
                float sh = Screen.height / 2f;
                for (int i = 0; i < n; i++)
                {
                    rt = mapMarkers[i];
                    mp = mapPoints[i];
                    if (checkForChosen && mp == chosenPoint) rt.GetComponent<Image>().color = chosenColor;
                    else
                    {
                        rt.GetComponent<Image>().color = mp is PointOfInterest ? interactableColor : notInteractableColor;
                    }
                    rt.GetChild(0).GetComponent<RawImage>().uvRect = GetMarkerRect(mp.type);
                    Button b = rt.GetComponent<Button>();;
                    b.onClick.RemoveAllListeners();
                    MapPoint mpLink = mp;
                    b.onClick.AddListener(() => { this.SelectPoint(mpLink); });
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
                if (infoPanel.activeSelf) infoPanelWidth = infoPanel.GetComponent<RectTransform>().rect.width;
                else infoPanelWidth = 0;
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


    public void LocalizeTitles()
    {
      
    }
}
