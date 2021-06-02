using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class FollowingCamera : MonoBehaviour {
    public static FollowingCamera main { get; private set; }
    public static Camera cam { get; private set; }
    private static Transform camTransform;
    private static Transform camBasisTransform;
    public static Vector3 camPos { get; private set; }
    public static bool touchscreen { get; private set; }

    private float rotationSpeed = 130, zoomSpeed = 40, moveSpeed = 15;   
    private float rotationSmoothCoefficient = 0, rotationSmoothAcceleration = 0.1f, moveSmoothAcceleration = 0.03f;
	private Vector3 moveSmoothCoefficient = Vector3.zero;
    private float touchRightBorder = Screen.width;
	public Vector3 deltaLimits = new Vector3 (0.1f, 0.1f, 0.1f);
	private readonly Vector3 DEFAULT_CAM_POINT = new Vector3(0,3,-3);

	Vector3 lookPoint;
	private bool changingBasePos = false,  zoom_oneChangeIgnore = false, camRotationBlocked = false, useEnvironmentalCamera = false,
        positionLoaded = false, lookOnTarget = false;
    public static float camRotateTrace { get; private set; } // чтобы не кликалось после поворота камеры
#pragma warning disable 0649
    [SerializeField] private bool initializeEnvCameraOnStart = false;
    [SerializeField] private Transform celestialCamera;
    [SerializeField] private RectTransform controllerBack, controllerStick;
    [SerializeField] private GameObject camUpButton, camLowButton;
#pragma warning restore 0649
    const float MAX_ZOOM = 0.3f, MAX_FAR = 50, ENV_CAMERA_CF = 0.01f, MAX_VISIBILITY_RANGE = 200f;

    private bool camPosChanged = false;
    private bool? verticalMovement = null;
    public Vector2 controllerStickOriginalPos, camMoveVector;
    private Transform environmentalCamera;

    public event System.Action cameraChangedEvent;

    public static void SetTouchControl(bool x)
    {
        if (main != null)
        {
            if (x)
            {
                touchscreen = true;
                main.controllerBack.gameObject.SetActive(true);
                main.camUpButton.SetActive(true);
                main.camLowButton.SetActive(true);
            }
            else
            {
                touchscreen = false;
                main.controllerBack.gameObject.SetActive(false);
                main.camUpButton.SetActive(false);
                main.camLowButton.SetActive(false);
            }
        }
    }

    private void Awake()
    {
        if (main != null & main != this) Destroy(main);
        main = this;

        camBasisTransform = transform;
        camTransform = camBasisTransform.GetChild(0);
        cam = camTransform.GetComponent<Camera>();
        SetTouchControl(Input.touchSupported);
    }

    private void Start()
    {
        if (!positionLoaded) CameraToStartPosition();
        else positionLoaded = false;
        //        
        controllerStickOriginalPos = new Vector2(controllerBack.position.y, controllerBack.position.y); // ?
        camMoveVector = Vector2.zero;

        if (initializeEnvCameraOnStart) EnableEnvironmentalCamera();
    }
    private void ResetInnerCamera()
    {
        camTransform.position = transform.position + transform.TransformDirection(DEFAULT_CAM_POINT);        
    }
    public void CameraToStartPosition()
    {
        ResetInnerCamera();
        camTransform.LookAt(transform.position);

        var cpos = GameMaster.sceneCenter;
        float csize = Chunk.chunkSize / 2f;
        float angle = Random.value * 360f;
        transform.position = cpos + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * csize + Vector3.up * 4f;
        transform.LookAt(new Vector3(cpos.x, transform.position.y, cpos.z), Vector3.up);
    }

    void Update()
    {
        if (cam == null) return;
        controllerStick.anchoredPosition = camMoveVector * 0.75f;
        Vector2 mv_0 = camMoveVector / controllerStickOriginalPos.y;
        Vector3 mv = new Vector3(mv_0.x, 0, mv_0.y);

        float av = Input.GetAxis("Horizontal");
        if (av != 0) mv.x = av;
        av = Input.GetAxis("Vertical");
        if (av != 0) mv.z = av;
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
            if (lookOnTarget)
            {
                ResetInnerCamera();
            }
            if (mv.x / moveSmoothCoefficient.x > 0) mv.x += moveSmoothAcceleration; else moveSmoothCoefficient.x = 0;
            if (mv.y / moveSmoothCoefficient.y > 0) mv.y += moveSmoothAcceleration; else moveSmoothCoefficient.y = 0;
            if (mv.z / moveSmoothCoefficient.z > 0) mv.z += moveSmoothAcceleration; else moveSmoothCoefficient.z = 0;
            mv.x *= (1 + moveSmoothCoefficient.x);
            mv.y *= (1 + moveSmoothCoefficient.y);
            mv.z *= (1 + moveSmoothCoefficient.z);
            Vector3 endPoint = transform.position + transform.TransformDirection(mv) * moveSpeed * Time.deltaTime ;
            float d = (endPoint - GameMaster.sceneCenter).magnitude;
            if (d < MAX_VISIBILITY_RANGE || (transform.position - GameMaster.sceneCenter).magnitude > d) transform.position = endPoint;
            //transform.Translate(mv * 30 * Time.deltaTime,Space.Self);
        }

        float delta = 0;
        if (touchscreen & camMoveVector == Vector2.zero)
        {            
            if (Input.touchCount > 0 & !camRotationBlocked)
            {
                Touch t = Input.GetTouch(0);
                if (t.position.x < touchRightBorder & t.position.x > 0.4f *Screen.height)
                {
                    if (Input.touchCount == 1)
                    {
                        if (t.phase == TouchPhase.Began | t.phase == TouchPhase.Moved)
                        {
                            bool a = false, b = false; //rotation detectors
                            float rspeed = rotationSpeed * Time.deltaTime * (1 + rotationSmoothCoefficient);
                            delta = t.deltaPosition.x / (float)Screen.width;
                            if (Mathf.Abs(delta) > 0.01f)
                            {
                                StopCameraMovement();
                                transform.RotateAround(transform.position, Vector3.up, rspeed * delta * 20f);
                                rotationSmoothCoefficient += rotationSmoothAcceleration;
                                a = true;
                            }

                            delta = t.deltaPosition.y / (float)Screen.height;
                            if (Mathf.Abs(delta) > 0.01f)
                            {
                                StopCameraMovement();
                                cam.transform.RotateAround(transform.position, cam.transform.TransformDirection(Vector3.left), rspeed * delta * 20f);
                                rotationSmoothCoefficient += rotationSmoothAcceleration;
                                b = true;
                            }

                            if (a == false & b == false)  rotationSmoothCoefficient = 0;
                            else camRotateTrace = 0.5f;
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
                                    float zl = cam.transform.localPosition.magnitude;
                                    float x = (1.1f - zl / (MAX_FAR - MAX_ZOOM));
                                    float zoomSmoothCoefficient = 1f / (x * x);
                                    float zspeed = zoomSpeed * Time.deltaTime * (1 + 2 * zoomSmoothCoefficient) * delta * (-1);
                                    float m = Mathf.Clamp(zl + zspeed, MAX_ZOOM, MAX_FAR);
                                    cam.transform.localPosition *= (m / zl);
                                }
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
                float zl = cam.transform.localPosition.magnitude;
                if (zl != 0)
                {
                    float x = (1.1f - zl / (MAX_FAR - MAX_ZOOM));
                    float zoomSmoothCoefficient = 1f / (x * x);
                    float zspeed = zoomSpeed * Time.deltaTime * (1 + 2 * zoomSmoothCoefficient) * delta * (-1);
                    float m = Mathf.Clamp(zl + zspeed, MAX_ZOOM, MAX_FAR);
                    cam.transform.localPosition *= (m / zl);
                }
                else
                {
                    cam.transform.localPosition = Vector3.zero;
                }
            }
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

        camPos = camTransform.position;
        camPosChanged = true;

        // Add cameras
        celestialCamera.rotation = camTransform.rotation;
        //if (moveSmoothCoefficient > 2) moveSmoothCoefficient = 2;
        if (useEnvironmentalCamera)
        {
            environmentalCamera.rotation = camTransform.rotation;
            environmentalCamera.position = camPos / 100f;
        }
        //


        if ( camRotateTrace > 0) camRotateTrace -= Time.deltaTime;
    }    

    void LateUpdate()
   {
        if (camPosChanged)
        {
            cameraChangedEvent?.Invoke();           
            camPosChanged = false;            
        }
   }
   public void WeNeedUpdate()
   {
        camPosChanged = true;
   }    

	public void SetLookPoint( Vector3 point ) {
		Vector3 camPrevPos = cam.transform.position;
		lookPoint = point;        
        if (lookPoint != transform.position) changingBasePos = true;
        verticalMovement = null;
        if (camTransform.localPosition == Vector3.zero)
        {
            camTransform.position = transform.position + transform.TransformDirection(DEFAULT_CAM_POINT);
            camTransform.LookAt(transform.position);
        }       
	}
    public void SetObservingPosition(Vector3 point, Vector3 target)
    {
        camBasisTransform.position = point;
        camBasisTransform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(target-point, Vector3.up));
        camTransform.localPosition = Vector3.up * 0.1f;
        camTransform.LookAt(target);
        lookOnTarget = true;
    }

    private void StopCameraMovement()
    {
        //#region dropping camera auto moving
        if (changingBasePos)
        {
            changingBasePos = false;
            moveSmoothCoefficient = Vector3.zero;
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

    public void CameraRotationBlock (bool blocking)
    {
        camRotationBlocked = blocking;
    }
    public void ControllerStickActivity(bool enabled)
    {
        if (controllerBack != null) controllerBack.transform.parent.gameObject.SetActive(enabled);
    }
    public void CamControllerDrag()
    {
        if (touchscreen & Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float x = controllerStickOriginalPos.x * 2, y = controllerStickOriginalPos.x * 2;
            if (Input.touchCount > 1 & (touch.position.x < x | touch.position.y < y))
            {
                foreach (Touch t in Input.touches)
                {
                    if (t.position.x < x & t.position.y < y)
                    {
                        touch = t;
                        break;
                    }
                }
            }
            camMoveVector = touch.position - controllerStickOriginalPos;            
        }
        else
        {
            camMoveVector = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - controllerStickOriginalPos;
        }
        float m = controllerStickOriginalPos.y;
        if (camMoveVector.magnitude > m) camMoveVector = camMoveVector.normalized * m;
    }
    public void StopCamDrag()
    {
        camMoveVector = Vector2.zero;
    }
    
    public void SetTouchRightBorder(float f)
    {
        touchRightBorder = f;
    }
    public void ResetTouchRightBorder()
    {
        touchRightBorder = Screen.width;
    }

    public void EnableEnvironmentalCamera()
    {
        if (!useEnvironmentalCamera)
        {
            var g = new GameObject("Environmental camera");
            int lmask = GameConstants.GetEnvironmentLayerMask();
            g.layer = lmask;
            var c = g.AddComponent<Camera>();
            c.cullingMask = 1 << lmask;
            c.farClipPlane = 100f;
            c.clearFlags = CameraClearFlags.Nothing;
            c.depth = -50;
            c.nearClipPlane = 0.01f;
            c.farClipPlane = 100f;
            c.useOcclusionCulling = false;
            environmentalCamera = g.transform;
            environmentalCamera.position = camPos / 100f;
            environmentalCamera.rotation = camTransform.rotation;
            useEnvironmentalCamera = true;
        }
    }
    public void DisableEnvironmentalCamera()
    {
        if (useEnvironmentalCamera)
        {
            environmentalCamera.gameObject.SetActive(false);
            useEnvironmentalCamera = false;
        }
    }

    #region save-load
    public void Save(System.IO.Stream fs)
    {
        var v = camBasisTransform.position;
        fs.Write(System.BitConverter.GetBytes(v.x),0,4);
        fs.Write(System.BitConverter.GetBytes(v.y), 0, 4);
        fs.Write(System.BitConverter.GetBytes(v.z), 0, 4);
        v = camBasisTransform.rotation.eulerAngles;
        fs.Write(System.BitConverter.GetBytes(v.x), 0, 4);
        fs.Write(System.BitConverter.GetBytes(v.y), 0, 4);
        fs.Write(System.BitConverter.GetBytes(v.z), 0, 4);
        //
        fs.Write(System.BitConverter.GetBytes(camPos.x), 0, 4);
        fs.Write(System.BitConverter.GetBytes(camPos.y), 0, 4);
        fs.Write(System.BitConverter.GetBytes(camPos.z), 0, 4);
        v = camTransform.rotation.eulerAngles;
        fs.Write(System.BitConverter.GetBytes(v.x), 0, 4);
        fs.Write(System.BitConverter.GetBytes(v.y), 0, 4);
        fs.Write(System.BitConverter.GetBytes(v.z), 0, 4);
    }
    public void Load(System.IO.Stream fs)
    {
        const int length = 48;
        var data = new byte[length];
        fs.Read(data, 0, length);
        int i = 0;
        float d (in int x) { return System.BitConverter.ToSingle(data, x); }
        Vector3 v(in int x) { return new Vector3(d(x), d(x + 4), d(x + 8));}
        camBasisTransform.position = v(i); i += 12;
        camBasisTransform.rotation = Quaternion.Euler(v(i)); i += 12;
        camTransform.position = v(i); i += 12;
        camTransform.rotation = Quaternion.Euler(v(i));
        positionLoaded = true;
    }
    #endregion
}
