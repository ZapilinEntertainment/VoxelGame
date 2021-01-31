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

    public float rotationSpeed = 65, zoomSpeed = 50, moveSpeed = 30;   
    float rotationSmoothCoefficient = 0;
	float rotationSmoothAcceleration = 0.1f;
	Vector3 moveSmoothCoefficient = Vector3.zero;
	float moveSmoothAcceleration= 0.03f;
    private float touchRightBorder = Screen.width;
	public Vector3 deltaLimits = new Vector3 (0.1f, 0.1f, 0.1f);
	[SerializeField] private Vector3 camPoint = new Vector3(0,3,-3);

	Vector3 lookPoint;
	bool changingBasePos = false,  zoom_oneChangeIgnore = false, camRotationBlocked = false;
    public static float camRotateTrace { get; private set; } // чтобы не кликалось после поворота камеры
#pragma warning disable 0649
    [SerializeField] Transform celestialCamera;
    [SerializeField] RectTransform controllerBack, controllerStick;
    [SerializeField] GameObject camUpButton, camLowButton;
#pragma warning restore 0649
    const float MAX_ZOOM = 0.3f, MAX_FAR = 50, MAX_LOOKPOINT_RADIUS = 50;

    private bool camPosChanged = false;
    private bool? verticalMovement = null;
    public Vector2 controllerStickOriginalPos, camMoveVector;

    public delegate void CameraChangedHandler();
    public event CameraChangedHandler cameraChangedEvent;

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

    void Start()
    {		
		camTransform.position = transform.position + transform.TransformDirection(camPoint);
		camTransform.LookAt(transform.position);
        Vector3 castPoint = camTransform.position - camTransform.forward * MAX_FAR;
        RaycastHit rh;
        if (Physics.SphereCast(castPoint, 0.173f, camTransform.forward, out rh, MAX_FAR))
        {
            transform.position = rh.point;
        }

        controllerStickOriginalPos = new Vector2(controllerBack.position.y, controllerBack.position.y); // ?
        camMoveVector = Vector2.zero;
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
            if (mv.x / moveSmoothCoefficient.x > 0) mv.x += moveSmoothAcceleration; else moveSmoothCoefficient.x = 0;
            if (mv.y / moveSmoothCoefficient.y > 0) mv.y += moveSmoothAcceleration; else moveSmoothCoefficient.y = 0;
            if (mv.z / moveSmoothCoefficient.z > 0) mv.z += moveSmoothAcceleration; else moveSmoothCoefficient.z = 0;
            mv.x *= (1 + moveSmoothCoefficient.x);
            mv.y *= (1 + moveSmoothCoefficient.y);
            mv.z *= (1 + moveSmoothCoefficient.z);
            Vector3 endPoint = transform.position + transform.TransformDirection(mv) * moveSpeed * Time.deltaTime ;
            float d = (endPoint - GameMaster.sceneCenter).magnitude;
            if (d < MAX_LOOKPOINT_RADIUS || (transform.position - GameMaster.sceneCenter).magnitude > d) transform.position = endPoint;
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
                                transform.RotateAround(transform.position, Vector3.up, rspeed * delta * 0.1f);
                                rotationSmoothCoefficient += rotationSmoothAcceleration;
                                a = true;
                            }

                            delta = t.deltaPosition.y / (float)Screen.height;
                            if (Mathf.Abs(delta) > 0.01f)
                            {
                                StopCameraMovement();
                                cam.transform.RotateAround(transform.position, cam.transform.TransformDirection(Vector3.left), rspeed * delta * 0.1f);
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
                float x = (1.1f - zl / (MAX_FAR - MAX_ZOOM));
                float zoomSmoothCoefficient = 1f / (x * x);
                float zspeed = zoomSpeed * Time.deltaTime * (1 + 2 * zoomSmoothCoefficient) * delta * (-1);               
                float m = Mathf.Clamp(zl + zspeed, MAX_ZOOM, MAX_FAR);
                cam.transform.localPosition *= (m / zl);               
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
       
        celestialCamera.rotation = camTransform.rotation;
        //if (moveSmoothCoefficient > 2) moveSmoothCoefficient = 2;

        camPos = camTransform.position;
        camPosChanged = true;

        if ( camRotateTrace > 0) camRotateTrace -= Time.deltaTime;
    }    

    void LateUpdate()
   {
        if (camPosChanged)
        {
            cameraChangedEvent?.Invoke();
            //OakTree.CameraUpdate();
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
}
