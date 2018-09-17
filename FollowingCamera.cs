using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class FollowingCamera : MonoBehaviour {
    public static FollowingCamera main { get; private set; }
    public static Camera cam { get; private set; }
    public static Transform camTransform; // unprotected - HOT
    public static Transform camBasisTransform { get; private set; }
    public static Vector3 camPos; // unprotected - HOT

    public float rotationSpeed = 65, zoomSpeed = 50, moveSpeed = 30;
	float rotationSmoothCoefficient = 0;
	float rotationSmoothAcceleration = 0.05f;
	float zoomSmoothCoefficient = 0;
	float zoomSmoothAcceleration = 0.05f;
	Vector3 moveSmoothCoefficient = Vector3.zero;
	float moveSmoothAcceleration= 0.03f;
	public Vector3 deltaLimits = new Vector3 (0.1f, 0.1f, 0.1f);
	[SerializeField] private Vector3 camPoint = new Vector3(0,5,-5);

	Vector3 lookPoint;
	bool changingBasePos = false, changingCamZoom = false, zoom_oneChangeIgnore = false;
	public static float optimalDistance { get; private set; }
    [SerializeField] bool useAutoZooming = true;
    [SerializeField] UnityEngine.UI.Slider zoomSlider, xSlider; // fiti
    [SerializeField] Transform cloudCamera;
    const string CAM_ZOOM_DIST_KEY = "CameraZoomDistance";

    private bool handleSprites = false, camPosChanged = false;
    private List<Transform> billboards, mastBillboards;
    private List<int> billboardsIDs, mastBillboardsIDs;
    public delegate void CameraChangedHandler();
    public event CameraChangedHandler cameraChangedEvent;


    private void Awake()
    {
        if (main != null & main != this) Destroy(main);
        main = this;

        camBasisTransform = transform;
        camTransform = camBasisTransform.GetChild(0);
        cam = camTransform.GetComponent<Camera>();

        billboards = new List<Transform>();
        mastBillboards = new List<Transform>();
        billboardsIDs = new List<int>();
        mastBillboardsIDs = new List<int>();
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
        billboards.Clear();
        mastBillboards.Clear();
        billboardsIDs.Clear();
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
        Vector3 prevCamPos = camPos;
        Vector3 mv = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetKey(KeyCode.Space)) mv.y = 1;
        else
        {
            if (Input.GetKey(KeyCode.LeftControl)) mv.y = -1;
        }
        if (mv != Vector3.zero)
        {
            #region dropping camera auto moving
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
            #endregion

            if (mv.x / moveSmoothCoefficient.x > 0) mv.x += moveSmoothAcceleration; else moveSmoothCoefficient.x = 0;
            if (mv.y / moveSmoothCoefficient.y > 0) mv.y += moveSmoothAcceleration; else moveSmoothCoefficient.y = 0;
            if (mv.z / moveSmoothCoefficient.z > 0) mv.z += moveSmoothAcceleration; else moveSmoothCoefficient.z = 0;
            mv.x *= (1 + moveSmoothCoefficient.x);
            mv.y *= (1 + moveSmoothCoefficient.y);
            mv.z *= (1 + moveSmoothCoefficient.z);
            transform.Translate(mv * moveSpeed * Time.deltaTime, Space.Self);
            //transform.Translate(mv * 30 * Time.deltaTime,Space.Self);
        }
        else moveSmoothCoefficient = Vector3.zero;

        float delta = 0;
        if (Input.GetMouseButton(2))
        {
            bool a = false, b = false; //rotation detectors
            float rspeed = rotationSpeed * Time.deltaTime * (1 + rotationSmoothCoefficient);
            delta = Input.GetAxis("Mouse X");
            if (delta != 0)
            {
                #region dropping camera auto moving
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
                #endregion
                transform.RotateAround(transform.position, Vector3.up, rspeed * delta);
                rotationSmoothCoefficient += rotationSmoothAcceleration;
                a = true;
            }

            delta = Input.GetAxis("Mouse Y");
            if (delta != 0)
            {
                #region dropping camera auto moving
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
                #endregion
                cam.transform.RotateAround(transform.position, cam.transform.TransformDirection(Vector3.left), rspeed * delta);
                rotationSmoothCoefficient += rotationSmoothAcceleration;
                b = true;
            }

            if (a == false && b == false) rotationSmoothCoefficient = 0;
        }


        delta = Input.GetAxis("Mouse ScrollWheel");
        if (delta != 0)
        {
            #region dropping camera auto moving
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
            #endregion
            float zspeed = zoomSpeed * Time.deltaTime * (1 + zoomSmoothCoefficient) * delta * (-1);
            cam.transform.Translate((cam.transform.position - transform.position) * zspeed, Space.World);

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
                zoomSlider.value = cam.transform.localPosition.magnitude;
            }
        }
        cloudCamera.rotation = camTransform.rotation;
        //if (moveSmoothCoefficient > 2) moveSmoothCoefficient = 2;

        camPos = camTransform.position;
        camPosChanged = true;

        if (GameMaster.editMode)
        {
            bool? leftClick = null;
            if (Input.GetMouseButtonDown(0))
            {
                leftClick = true;
            }
            else
            {
                if (Input.GetMouseButtonDown(1)) leftClick = false;
            }
            if (leftClick != null)
            {
                Vector2 mpos = Input.mousePosition;
                RaycastHit rh;
                if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out rh))
                {
                    GameObject collided = rh.collider.gameObject;
                    if (collided.tag == "BlockCollider")
                    {
                        Block b = collided.transform.parent.gameObject.GetComponent<Block>();
                        if (b == null)
                        {
                            b = collided.transform.parent.parent.gameObject.GetComponent<Block>();
                        }
                        Vector3Int cpos = new Vector3Int(b.pos.x, b.pos.y, b.pos.z);
                        if (leftClick == true) // добавляем блок
                        {
                            if (b.type == BlockType.Cube)
                            {
                                float coordsDelta = rh.point.z - b.transform.position.z;
                                if (Mathf.Abs(coordsDelta) >= Block.QUAD_SIZE / 2f)
                                {
                                    if (coordsDelta > 0)
                                    {
                                        cpos.z += 1;
                                        if (cpos.z >= Chunk.CHUNK_SIZE) goto END_OF_RAYCAST_CHECK;
                                    }
                                    else
                                    {
                                        cpos.z -= 1;
                                        if (cpos.z < 0) goto END_OF_RAYCAST_CHECK;
                                    }
                                }
                                coordsDelta = rh.point.x - b.transform.position.x;
                                if (Mathf.Abs(coordsDelta) >= Block.QUAD_SIZE / 2f)
                                {
                                    if (coordsDelta > 0)
                                    {
                                        cpos.x += 1;
                                        if (cpos.x >= Chunk.CHUNK_SIZE) goto END_OF_RAYCAST_CHECK;
                                    }
                                    else
                                    {
                                        cpos.x -= 1;
                                        if (cpos.x < 0) goto END_OF_RAYCAST_CHECK;
                                    }
                                }
                                coordsDelta = rh.point.y - b.transform.position.y;
                                if (Mathf.Abs(coordsDelta) >= Block.QUAD_SIZE / 2f)
                                {
                                    if (coordsDelta > 0)
                                    {
                                        cpos.y += 1;
                                        if (cpos.y >= Chunk.CHUNK_SIZE) goto END_OF_RAYCAST_CHECK;
                                    }
                                    else
                                    {
                                        cpos.y -= 1;
                                        if (cpos.y < 0) goto END_OF_RAYCAST_CHECK;
                                    }
                                }
                                GameMaster.mainChunk.AddBlock(new ChunkPos(cpos.x, cpos.y, cpos.z), BlockType.Cube, ResourceType.STONE_ID, true);
                            }
                            else // surface block
                            {
                                GameMaster.mainChunk.ReplaceBlock(b.pos, BlockType.Cube, b.material_id, true);
                            }
                        }
                        else // удаляем блок
                        {
                            if (b.type == BlockType.Surface | b.type == BlockType.Cave) {
                                GameMaster.mainChunk.DeleteBlock(new ChunkPos(cpos.x, cpos.y - 1, cpos.z));
                            }
                            GameMaster.mainChunk.DeleteBlock(new ChunkPos(cpos.x, cpos.y, cpos.z));
                        }
                    }
                }
            }
            END_OF_RAYCAST_CHECK:;
        }
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
        if (billboards.Count > 0)
        {
            foreach (Transform t in billboards)   t.LookAt(camPos);
        }
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

    public bool AddSprite(Transform t)
    {
        int id = t.GetInstanceID();
        if (billboards.Count != 0)
        {
            foreach (int i in billboardsIDs)
            {
                if (i == id) return false;
            }
        }
        billboards.Add(t);
        billboardsIDs.Add(id);
        t.LookAt(camPos);
        handleSprites = true;
        return true;
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

    public void RemoveSprite(int id)
    {
        if (billboards.Count == 0) return;
        else
        {
            int i = 0;
            while (i < billboards.Count)
            {
                if (billboardsIDs[i] == id)
                {
                    billboardsIDs.RemoveAt(i);
                    billboards.RemoveAt(i);
                    break;
                }
                i++;
            }
            handleSprites = ((billboards.Count == 0) & (mastBillboards.Count == 0));
        }
    }
    public void RemoveMastSprite(int id)
    {
        if (mastBillboards.Count == 0) return;
        else
        {
            int i = 0;
            bool found = false;
            while (i < mastBillboards.Count)
            {
                if (mastBillboardsIDs[i] == id)
                {
                    mastBillboardsIDs.RemoveAt(i);
                    mastBillboards.RemoveAt(i);
                    found = true;
                    break;
                }
                i++;
            }
            if (!found) print("not found");
            handleSprites = ((billboards.Count == 0) & (mastBillboards.Count == 0));
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
            if (d / optimalDistance > 1 | d/optimalDistance < 0.5f) changingCamZoom = true;
        }
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
    public static void SetOptimalDistance(float d)
    {
        if (d != optimalDistance)
        {
            optimalDistance = d;
            PlayerPrefs.SetFloat(CAM_ZOOM_DIST_KEY, optimalDistance);
            main.SetLookPoint(main.lookPoint);
        }
    }

    public void CheckTouches()
    {
        if (cam == null | Input.touchCount == 0) return;
        Vector2 a = Vector2.zero, b = Vector2.zero;
        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Moved)
            {
                if (a == Vector2.zero) a = t.deltaPosition;
                else b = t.deltaPosition;
            }
        }
        if (b == Vector2.zero)
        {
            if (a != Vector2.zero) // one moved touch
            {
                a = new Vector2(a.x / Screen.width / 2f, a.y / Screen.height / 2f);
                if (a.x > a.y) RotateX(a.x); else RotateY(a.y);
            }
        }
        else // two touches
        {
            if (Vector2.Dot(a,b) < -0.75f) 
            {
                float z = Vector2.Distance(a, b);
                z /= Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
                Zoom( z * (zoomSlider.maxValue - zoomSlider.minValue));
            }
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetFloat(CAM_ZOOM_DIST_KEY, optimalDistance);
    }
}
