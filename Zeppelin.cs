using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Zeppelin : MonoBehaviour {
    public static Zeppelin current {get;private set;}
#pragma warning disable 0649
    [SerializeField] private Transform leftScrew,rightScrew, body;
    [SerializeField] private AudioSource propeller_as;
#pragma warning restore 0649

    private float flySpeed = 5, destructionTimer;
    private bool landPointSet = false, destroyed = false;
    private bool? landingByZAxis = null;

    private const float SCREWS_ROTATION_SPEED = 500;

    public SurfaceBlock landingSurface { get; private set; }
    private GameObject landingMarkObject;
    private LineRenderer lineDrawer;

    void Start() {
        float x = Random.value * 360;
        float cs = Chunk.CHUNK_SIZE / 2;
        Vector3 rpos = Quaternion.AngleAxis(x , Vector3.up ) * Vector3.forward * cs * 1.5f * Block.QUAD_SIZE;
        
        rpos.y = cs;
        rpos.x += cs;
        rpos.z += cs;
        transform.position = rpos;        
        Vector3 v = GameMaster.sceneCenter - transform.position;
        v.y = 0;
        transform.forward = Quaternion.AngleAxis(-90, Vector3.up) * v;
		leftScrew.Rotate(0, Random.value * 360, 0);
		rightScrew.Rotate(0, Random.value * 360, 0);

        landingMarkObject = Instantiate(Resources.Load<GameObject>("Prefs/LandingX")) as GameObject;
        landingMarkObject.SetActive(false);

        UIController.current.mainCanvas.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(Click);
        lineDrawer = GetComponent<LineRenderer>();

        if (current != null) Destroy(current);
        current = this;
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(gameObject);
    }

	void Update() {
        if (destroyed) return;
        if (!landPointSet)
        {
            transform.RotateAround(GameMaster.sceneCenter, Vector3.up, flySpeed * Time.deltaTime * GameMaster.gameSpeed);
            leftScrew.Rotate(0, SCREWS_ROTATION_SPEED * Time.deltaTime * GameMaster.gameSpeed, 0);
            rightScrew.Rotate(0, SCREWS_ROTATION_SPEED * Time.deltaTime * GameMaster.gameSpeed, 0);
        }
        else
        {
            if (destructionTimer > 0)
            {
                destructionTimer -= Time.deltaTime * GameMaster.gameSpeed;
                if (destructionTimer <= 0)
                {
                    destroyed = true;
                    //hq
                    Structure s = Structure.GetStructureByID(Structure.LANDED_ZEPPELIN_ID);
                    if (landingByZAxis == true) s.SetModelRotation(0); else s.SetModelRotation(2);
                    s.SetBasement(landingSurface, PixelPosByte.zero);
                    //storage
                    s = Structure.GetStructureByID(Structure.STORAGE_0_ID);
                    SurfaceBlock sb = (landingByZAxis == true) ?
                        landingSurface.myChunk.GetBlock(landingSurface.pos.x, landingSurface.pos.y, landingSurface.pos.z + 1) as SurfaceBlock
                        :
                        landingSurface.myChunk.GetBlock(landingSurface.pos.x + 1, landingSurface.pos.y, landingSurface.pos.z) as SurfaceBlock;
                    s.SetBasement(sb, PixelPosByte.zero);
                    
                    GameMaster.realMaster.SetStartResources();
                    PoolMaster.current.BuildSplash(transform.position);
                    if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.ColonyFounded);
                    FollowingCamera.main.SetLookPoint(s.transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }

    public void Click()
    {
        if (landPointSet | destroyed) return;
        RaycastHit rh;
        if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh))
        {
            GameObject collided = rh.collider.gameObject;
            if (collided.tag == Block.BLOCK_COLLIDER_TAG)
            {
                Block b = collided.transform.parent.GetComponent<Block>();
                if (b == null) b = collided.transform.parent.parent.GetComponent<Block>(); // cave block                
                if (b != null && (b is SurfaceBlock))
                {
                    landingSurface = null;
                    landingByZAxis = null;
                    Chunk chunk = b.myChunk;
                    Block minusTwoBlock, minusOneBlock, plusOneBlock, plusTwoBlock;
                    int x = b.pos.x, y = b.pos.y, z = b.pos.z;
                    bool[] suitable = new bool[5];
                    suitable[2] = true;
                    // direction 0 - fwd    
                    {
                        minusOneBlock = chunk.GetBlock(x, y, z - 1); suitable[1] = (minusOneBlock != null) && (minusOneBlock is SurfaceBlock);
                        minusTwoBlock = chunk.GetBlock(x, y, z - 2); suitable[0] = (minusTwoBlock != null) && (minusTwoBlock is SurfaceBlock);
                        plusOneBlock = chunk.GetBlock(x, y, z + 1); suitable[3] = (plusOneBlock != null) && (plusOneBlock is SurfaceBlock);
                        plusTwoBlock = chunk.GetBlock(x, y, z + 2); suitable[4] = (plusTwoBlock != null) && (plusTwoBlock is SurfaceBlock);
                        if (suitable[1])
                        {
                            if (suitable[0])
                            {
                                landingByZAxis = true;
                                landingSurface = minusOneBlock as SurfaceBlock;
                                goto DRAW_LINE;
                            }
                            else
                            {
                                if (suitable[3])
                                {
                                    landingByZAxis = true;
                                    landingSurface = b as SurfaceBlock;
                                    goto DRAW_LINE;
                                }
                            }
                        }
                        else
                        {
                            if (suitable[3])
                            {
                                if (suitable[4])
                                {
                                    landingByZAxis = true;
                                    landingSurface = plusOneBlock as SurfaceBlock;
                                    goto DRAW_LINE;
                                }
                            }
                        }
                    }
                    //direction 2 - right
                    {
                        minusOneBlock = chunk.GetBlock(x - 1, y, z); suitable[1] = (minusOneBlock != null) && (minusOneBlock is SurfaceBlock);
                        minusTwoBlock = chunk.GetBlock(x - 2, y, z - 2); suitable[0] = (minusTwoBlock != null) && (minusTwoBlock is SurfaceBlock);
                        plusOneBlock = chunk.GetBlock(x + 1, y, z); suitable[3] = (plusOneBlock != null) && (plusOneBlock is SurfaceBlock);
                        plusTwoBlock = chunk.GetBlock(x + 2, y, z + 2); suitable[4] = (plusTwoBlock != null) && (plusTwoBlock is SurfaceBlock);
                        if (suitable[1])
                        {
                            if (suitable[0])
                            {
                                landingByZAxis = false;
                                landingSurface = minusOneBlock as SurfaceBlock;
                                goto DRAW_LINE;
                            }
                            else
                            {
                                if (suitable[3])
                                {
                                    landingByZAxis = false;
                                    landingSurface = b as SurfaceBlock;
                                    goto DRAW_LINE;
                                }
                            }
                        }
                        else
                        {
                            if (suitable[3])
                            {
                                if (suitable[4])
                                {
                                    landingByZAxis = false;
                                    landingSurface = plusOneBlock as SurfaceBlock;
                                    goto DRAW_LINE;
                                }
                            }
                        }
                    }
                    if (landingSurface == null)
                    {
                        lineDrawer.enabled = false;
                        UIController.current.DeactivateLandButton();
                        landingMarkObject.SetActive(false);                        
                        return;
                    }
                    DRAW_LINE:
                    Vector3[] positions = new Vector3[4];
                    Vector3 cpos = landingSurface.transform.position;
                    float q = Block.QUAD_SIZE;
                    float h = cpos.y - 0.5f * q;
                    if (landingByZAxis == false)
                    {
                        positions[0] = new Vector3( cpos.x - 1.5f * q, h, cpos.z + 0.5f * q );
                        positions[1] = new Vector3(cpos.x + 1.5f * q, h, cpos.z + 0.5f * q);
                        positions[2] = new Vector3(cpos.x + 1.5f * q, h, cpos.z - 0.5f * q);
                        positions[3] = new Vector3(cpos.x - 1.5f * q, h, cpos.z - 0.5f * q);
                        //positions[4] = positions[0];
                    }
                    else
                    {
                        positions[0] = new Vector3(cpos.x - 0.5f * q, h, cpos.z + 1.5f * q);
                        positions[1] = new Vector3(cpos.x + 0.5f * q, h, cpos.z + 1.5f * q);
                        positions[2] = new Vector3(cpos.x + 0.5f * q, h, cpos.z - 1.5f * q);
                        positions[3] = new Vector3(cpos.x - 0.5f * q, h, cpos.z - 1.5f * q);
                        //positions[4] = positions[0];
                    }
                    lineDrawer.SetPositions(positions);
                    lineDrawer.enabled = true;
                    landingMarkObject.transform.position = landingSurface.transform.position + Vector3.down * Block.QUAD_SIZE * 0.45f;
                    landingMarkObject.SetActive(true);
                    UIController.current.ActivateLandButton();
                }
            }
        }
    }

    public void Land()
    {
        if (!landPointSet & !destroyed)
        {
            if (landingSurface != null)
            {
                if (UIController.current != null && UIController.current.currentActiveWindowMode == ActiveWindowMode.GameMenu) return;
                landPointSet = true;
                Vector3 newPos = landingSurface.transform.position + Vector3.down * Block.QUAD_SIZE / 2f;
                PoolMaster.current.BuildSplash(newPos);
                transform.position = newPos + Vector3.up * 0.5f;
                if (landingByZAxis == true) transform.rotation = Quaternion.identity;
                else transform.rotation = Quaternion.Euler(0, 90, 0);
                lineDrawer.enabled = false;
                Destroy(landingMarkObject);
                destructionTimer = 3;
                UIController.current.DeactivateLandButton();
            }
        }
    }

    private void OnDestroy()
    {
        if (GameMaster.sceneClearing) return;
        else
        {
            if (UIController.current != null)
            {
                UIController.current.mainCanvas.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(Click);
            }
        }
    }
}
