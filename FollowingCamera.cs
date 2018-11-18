using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class FollowingCamera : MonoBehaviour {
    public static FollowingCamera main { get; private set; }
    public static Camera cam { get; private set; }
    public static Transform camTransform; // unprotected - HOT - means using very often
    public static Transform camBasisTransform { get; private set; }
    public static Vector3 camPos; // unprotected - HOT
    public static bool touchscreen { get; private set; }
    public static bool camRotationBlocked = false;

    public float rotationSpeed = 65, zoomSpeed = 50, moveSpeed = 30;
	float rotationSmoothCoefficient = 0;
	float rotationSmoothAcceleration = 0.1f;
	float zoomSmoothCoefficient = 0;
	float zoomSmoothAcceleration = 0.05f;
	Vector3 moveSmoothCoefficient = Vector3.zero;
	float moveSmoothAcceleration= 0.03f;
    private float touchRightBorder = Screen.width;
	public Vector3 deltaLimits = new Vector3 (0.1f, 0.1f, 0.1f);
	[SerializeField] private Vector3 camPoint = new Vector3(0,5,-5);

	Vector3 lookPoint;
	bool changingBasePos = false, changingCamZoom = false, zoom_oneChangeIgnore = false;
	public static float optimalDistance { get; private set; }
#pragma warning disable 0649
    [SerializeField] bool useAutoZooming = true;
    [SerializeField] UnityEngine.UI.Slider zoomSlider, xSlider; // fiti
    [SerializeField] Transform cloudCamera;
#pragma warning restore 0649
    const string CAM_ZOOM_DIST_KEY = "CameraZoomDistance";
    const float MAX_ZOOM = 0.3f, MAX_FAR = 50, MAX_LOOKPOINT_RADIUS = 50;

    private bool handleSprites = false, camPosChanged = false;
    private bool? verticalMovement = null;
    private List<Transform>  mastBillboards;
    private List<int>  mastBillboardsIDs;

    public delegate void CameraChangedHandler();
    public event CameraChangedHandler cameraChangedEvent;


    private void Awake()
    {
        if (main != null & main != this) Destroy(main);
        main = this;

        camBasisTransform = transform;
        camTransform = camBasisTransform.GetChild(0);
        cam = camTransform.GetComponent<Camera>();

        mastBillboards = new List<Transform>();
        mastBillboardsIDs = new List<int>();

        touchscreen = Input.touchSupported;
    }

    void Start()
    {		
        if (PlayerPrefs.HasKey(CAM_ZOOM_DIST_KEY)) optimalDistance = PlayerPrefs.GetFloat(CAM_ZOOM_DIST_KEY);
        else optimalDistance = 5;
		camTransform.position = transform.position + transform.TransformDirection(camPoint);
		camTransform.LookAt(transform.position);

        GameObject[] msprites = GameObject.FindGameObjectsWithTag("AddToMastSpritesList");
        if (msprites != null) {
            foreach (GameObject g in msprites) {
                mastBillboards.Add(g.transform);
                mastBillboardsIDs.Add(g.GetInstanceID());
            }
        }
    }

    public void ResetLists()
    {
        mastBillboards.Clear();
        mastBillboardsIDs.Clear();
        handleSprites = false;
    }

    public static void CenterCamera(Vector3 point)
    {
        camBasisTransform.position = point;
        camTransform.localPosition = (point - camTransform.position).normalized * optimalDistance;
        camTransform.LookAt(point);
    }

    void Update()
    {
        if (cam == null) return;
        Vector3 mv = Vector3.zero;
        mv = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.Space)) mv.y = 1;
        else
        {
          if (Input.GetKey(KeyCode.LeftControl)) mv.y = -1;
        }
        if (mv != Vector3.zero) StopCameraMovement();
        else
        {
            if (verticalMovement == null) moveSmoothCoefficient = Vector3.zero;
            else
            {
                if (verticalMovement == true) mv.y = 1; else mv.y = -1;
            }
        }

        if (mv != Vector3.zero)
        {
            if (mv.x / moveSmoothCoefficient.x > 0) mv.x += moveSmoothAcceleration; else moveSmoothCoefficient.x = 0;
            if (mv.y / moveSmoothCoefficient.y > 0) mv.y += moveSmoothAcceleration; else moveSmoothCoefficient.y = 0;
            if (mv.z / moveSmoothCoefficient.z > 0) mv.z += moveSmoothAcceleration; else moveSmoothCoefficient.z = 0;
            mv.x *= (1 + moveSmoothCoefficient.x);
            mv.y *= (1 + moveSmoothCoefficient.y);
            mv.z *= (1 + moveSmoothCoefficient.z);
            Vector3 endPoint = transform.position + mv * moveSpeed * Time.deltaTime;
            float d = (endPoint - GameMaster.sceneCenter).magnitude;
            if (d < MAX_LOOKPOINT_RADIUS || (transform.position - GameMaster.sceneCenter).magnitude > d) transform.Translate(mv * moveSpeed * Time.deltaTime, Space.Self);
            //transform.Translate(mv * 30 * Time.deltaTime,Space.Self);
        }

        float delta = 0;
        if (touchscreen )
        {
            if (Input.touchCount > 0 & !camRotationBlocked)
            {
                Touch t = Input.GetTouch(0);
                if (t.position.x < touchRightBorder)
                {
                    if (Input.touchCount == 1)
                    {
                        if (t.phase == TouchPhase.Began | t.phase == TouchPhase.Moved)
                        {
                            bool a = false, b = false; //rotation detectors
                            float rspeed = rotationSpeed * Time.deltaTime * (1 + rotationSmoothCoefficient);
                            delta = t.deltaPosition.x / (float)Screen.width * 10;
                            if (delta != 0)
                            {
                                StopCameraMovement();
                                transform.RotateAround(transform.position, Vector3.up, rspeed * delta);
                                rotationSmoothCoefficient += rotationSmoothAcceleration;
                                a = true;
                            }

                            delta = t.deltaPosition.y / (float)Screen.height * 10;
                            if (delta != 0)
                            {
                                StopCameraMovement();
                                cam.transform.RotateAround(transform.position, cam.transform.TransformDirection(Vector3.left), rspeed * delta);
                                rotationSmoothCoefficient += rotationSmoothAcceleration;
                                b = true;
                            }

                            if (a == false & b == false) rotationSmoothCoefficient = 0;
                        }
                    }

                    if (Input.touchCount == 2)
                    {
                        //https://unity3d.com/ru/learn/tutorials/topics/mobile-touch/pinch-zoom
                        Touch t2 = Input.GetTouch(1);
                        if (t2.position.x < touchRightBorder)
                        {
                            Vector2 tPrevPos = t.position - t.deltaPosition;
                            Vector2 t2PrevPos = t2.position - t2.deltaPosition;
                            float deltaMagnitudeDiff = ((tPrevPos - t2PrevPos).magnitude - (t.position - t2.position).magnitude) / (float)Screen.height * (-2);
                            if (cam.orthographic) // на будущее
                            {
                                cam.orthographicSize += deltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
                                cam.orthographicSize = Mathf.Max(cam.orthographicSize, 0.1f);
                            }
                            else
                            {
                                delta = deltaMagnitudeDiff;
                                if (delta != 0)
                                {
                                    StopCameraMovement();
                                    float zspeed = zoomSpeed * Time.deltaTime * (1 + zoomSmoothCoefficient) * delta;                                   
                                    float zl = cam.transform.localPosition.magnitude;
                                    if (zl + zspeed > MAX_FAR) zspeed = MAX_FAR - zl;
                                    else
                                    {
                                        if (zl + zspeed < MAX_ZOOM & zl + zspeed > 0) zspeed = MAX_ZOOM - zspeed;
                                    }
                                    cam.transform.Translate(Vector3.forward * zspeed, Space.Self);

                                    if (zoomSlider != null) // удалить ?
                                    {
                                        float dist = cam.transform.localPosition.magnitude;
                                        if (dist > zoomSlider.maxValue)
                                        {
                                            dist = zoomSlider.maxValue;
                                        }
                                        else
                                        {
                                            if (dist < zoomSlider.minValue)
                                            {
                                                dist = zoomSlider.minValue;
                                            }
                                        }
                                        if (dist != camTransform.localPosition.magnitude)
                                        {
                                            cam.transform.localPosition = cam.transform.localPosition.normalized * dist;
                                            zoom_oneChangeIgnore = true;
                                            zoomSlider.value = dist;
                                        }
                                    }
                                    zoomSmoothCoefficient += zoomSmoothAcceleration;
                                }
                                else zoomSmoothCoefficient = 0;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(2))
            {
                bool a = false, b = false; //rotation detectors
                float rspeed = rotationSpeed * Time.deltaTime * (1 + rotationSmoothCoefficient);
                delta = Input.GetAxis("Mouse X");
                if (delta != 0)
                {
                    StopCameraMovement();
                    transform.RotateAround(transform.position, Vector3.up, rspeed * delta);
                    rotationSmoothCoefficient += rotationSmoothAcceleration;
                    a = true;
                }

                delta = Input.GetAxis("Mouse Y");
                if (delta != 0)
                {
                    StopCameraMovement();
                    cam.transform.RotateAround(transform.position, cam.transform.TransformDirection(Vector3.left), rspeed * delta);
                    rotationSmoothCoefficient += rotationSmoothAcceleration;
                    b = true;
                }

                if (a == false && b == false) rotationSmoothCoefficient = 0;
            }

            delta = Input.GetAxis("Mouse ScrollWheel");
            if (delta != 0)
            {
                StopCameraMovement();
                float zspeed = zoomSpeed * Time.deltaTime * (1 + zoomSmoothCoefficient) * delta * (-1);
                cam.transform.Translate((cam.transform.position - transform.position) * zspeed, Space.World);
                float zl = cam.transform.localPosition.magnitude;
                if (zl > MAX_FAR) cam.transform.localPosition *= MAX_FAR / zl;
                else
                {
                    if (zl < MAX_ZOOM) cam.transform.localPosition *= MAX_ZOOM / zl;
                }

                if (zoomSlider != null) // удалить ?
                {
                    float dist = cam.transform.localPosition.magnitude;
                    if (dist > zoomSlider.maxValue)
                    {
                        dist = zoomSlider.maxValue;
                    }
                    else
                    {
                        if (dist < zoomSlider.minValue)
                        {
                            dist = zoomSlider.minValue;
                        }
                    }
                    if (dist != camTransform.localPosition.magnitude)
                    {
                        cam.transform.localPosition = cam.transform.localPosition.normalized * dist;
                        zoom_oneChangeIgnore = true;
                        zoomSlider.value = dist;
                    }
                }
                zoomSmoothCoefficient += zoomSmoothAcceleration;
            }
            else zoomSmoothCoefficient = 0;
        }

        if (changingBasePos)
        {
            transform.position = Vector3.MoveTowards(transform.position, lookPoint, (1 + moveSmoothCoefficient.x) * moveSpeed / 2f * Time.deltaTime);
            if (transform.position == lookPoint)
            {
                changingBasePos = false;
                moveSmoothCoefficient = Vector3.zero;
            }
            else moveSmoothCoefficient.x = moveSmoothAcceleration * moveSmoothAcceleration;
        }
        if (changingCamZoom)
        {
            Vector3 endPoint = camTransform.localPosition.normalized * optimalDistance;
            cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, endPoint, zoomSpeed / 5f * Time.deltaTime * (1 + zoomSmoothCoefficient));
            cam.transform.LookAt(transform.position);
            if (cam.transform.localPosition.magnitude / endPoint.magnitude == 1)
            {
                changingCamZoom = false;
                zoomSmoothCoefficient = 0;
            }
            else
            {
                zoomSmoothCoefficient = zoomSmoothAcceleration * zoomSmoothAcceleration * zoomSmoothAcceleration;
                zoom_oneChangeIgnore = true;
                if (zoomSlider != null) zoomSlider.value = cam.transform.localPosition.magnitude;
            }
        }
        cloudCamera.rotation = camTransform.rotation;
        //if (moveSmoothCoefficient > 2) moveSmoothCoefficient = 2;

        camPos = camTransform.position;
        camPosChanged = true;
    }
    

    void LateUpdate()
   {
        if (camPosChanged)
        {
            cameraChangedEvent();
            if (handleSprites) StartCoroutine(SpritesHandler());
            camPosChanged = false;
        }
   }
   public void WeNeedUpdate()
   {
        camPosChanged = true;
   }

    #region sprites handling
    IEnumerator SpritesHandler()
    {
        if (mastBillboards.Count > 0)
        {
            foreach (Transform t in mastBillboards)
            {
                Vector3 cpos = Vector3.ProjectOnPlane(camPos - t.position, t.up);
                t.transform.forward = cpos.normalized;
            }
        }
        yield return null;
    }
    public void AddMastSprite(Transform t)
    {
        int id = t.GetInstanceID();
        if (mastBillboards.Count != 0)
        {
            foreach (int i in mastBillboardsIDs)
            {
                if (i == id) return;
            }
        }
        mastBillboards.Add(t);
        mastBillboardsIDs.Add(id);

        Vector3 cpos = Vector3.ProjectOnPlane(camPos - t.position, t.up);
        t.transform.forward = cpos.normalized;

        handleSprites = true;
    }
    public void RemoveMastSprite(int id)
    {
        if (mastBillboards.Count == 0) return;
        else
        {
            // при замещении блоков иногда работает вхолостую - почему?
            int i = 0;
            while (i < mastBillboards.Count)
            {
                if (mastBillboardsIDs[i] == id)
                {
                    mastBillboardsIDs.RemoveAt(i);
                    mastBillboards.RemoveAt(i);
                    break;
                }
                i++;
            }
            handleSprites = (mastBillboards.Count != 0);
        }
    }
    #endregion
    

	public void SetLookPoint( Vector3 point ) {
		Vector3 camPrevPos = cam.transform.position;
		lookPoint = point;        
        if (lookPoint != transform.position) changingBasePos = true;
        if (useAutoZooming)
        {
            float d = cam.transform.localPosition.magnitude;
            if (d > optimalDistance) changingCamZoom = true;
        }
        verticalMovement = null;
	}

    private void StopCameraMovement()
    {
        //#region dropping camera auto moving
        if (changingBasePos)
        {
            changingBasePos = false;
            moveSmoothCoefficient = Vector3.zero;
        }
        if (changingCamZoom)
        {
            changingCamZoom = false;
            rotationSmoothCoefficient = 0;
        }
        verticalMovement = null;
        //#endregion
    }

    public void RotateY(float val)
    {
        transform.rotation = Quaternion.Euler(0, val * 360, 0);
    }
    public void RotateX(float val)
    {
        float agl = val * 90 - camTransform.localRotation.eulerAngles.x;
        camTransform.RotateAround(transform.position, camTransform.TransformDirection(Vector3.right), agl);        
    }   
    public void Zoom(float x)
    {
        if (zoom_oneChangeIgnore)
        {
            zoom_oneChangeIgnore = false;
            return;
        }
        cam.transform.localPosition = cam.transform.localPosition.normalized * x;
    }
    public void StartMoveUp() { StopCameraMovement(); verticalMovement = true; }
    public void EndMoveUp() { StopCameraMovement(); if (verticalMovement == true) verticalMovement = null; }
    public void StartMoveDown() { StopCameraMovement(); verticalMovement = false; }
    public void EndMoveDown() { StopCameraMovement(); if (verticalMovement == false) verticalMovement = null; }

    public void SetTouchRightBorder(float f)
    {
        touchRightBorder = f;
    }
    public void ResetTouchRightBorder()
    {
        touchRightBorder = Screen.width;
    }

    public static void SetOptimalDistance(float d)
    {
        if (d != optimalDistance)
        {
            optimalDistance = d;
            PlayerPrefs.SetFloat(CAM_ZOOM_DIST_KEY, optimalDistance);
            main.SetLookPoint(main.lookPoint);
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat(CAM_ZOOM_DIST_KEY, optimalDistance);
    }

    private void OnGUI()
    {
        GUILayout.Label(touchRightBorder.ToString());
    }
}
