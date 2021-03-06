﻿using System.Collections;
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

    public Plane landingSurface { get; private set; }
    private GameObject landingMarkObject, landButton;
    private LineRenderer lineDrawer;
    private Transform landingCanvas;

    public static void CreateNew()
    {
        Instantiate(Resources.Load<GameObject>("Prefs/Ships/Zeppelin"));        
    }

    void Start() {
        float x = Random.value * 360;
        float cs = Chunk.chunkSize / 2;
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
        lineDrawer = GetComponent<LineRenderer>();

        if (current != null) Destroy(current);
        current = this;
        if (!PoolMaster.useDefaultMaterials) PoolMaster.ReplaceMaterials(gameObject);

        landingCanvas = Instantiate(Resources.Load<GameObject>("UIPrefs/landingCanvas")).transform;
        landingCanvas.GetComponentInChildren<UnityEngine.UI.Text>().text = Localization.GetWord(LocalizedWord.Land_verb);
        UIController.GetCurrent().AddSpecialCanvasToHolder(landingCanvas);
        landingCanvas.GetChild(0).GetComponent<UnityEngine.UI.Button>().onClick.AddListener(this.Raycasting);
        landButton = landingCanvas.GetChild(1).gameObject;
        landButton.SetActive(false);
        landButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(this.LandActionAccepted);

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
                    Structure hq = HeadQuarters.GetHQ(1), storage = null, setCenter = null;
                    //storage
                    if (landingByZAxis == true)
                    {
                        hq.SetModelRotation(0);
                        hq.SetBasement(landingSurface, PixelPosByte.zero);
                        var p2 = landingSurface.myChunk.GetSurfacePlane(landingSurface.pos.OneBlockForward());
                        storage = p2.CreateStructure(Structure.STORAGE_0_ID);
                        p2 = landingSurface.myChunk.GetSurfacePlane(landingSurface.pos.OneBlockBack());
                        setCenter = p2.CreateStructure(Structure.SETTLEMENT_CENTER_ID);
                    }
                    else
                    {
                        hq.SetModelRotation(2);
                        hq.SetBasement(landingSurface, PixelPosByte.zero);
                        var chunk = landingSurface.myChunk;
                        var npos = landingSurface.pos.OneBlockRight();
                        var p2 = chunk.GetSurfacePlane(npos);
                        if (p2 == null) p2 = chunk.GetBlock(npos).FORCED_GetPlane(Block.UP_FACE_INDEX);
                        storage = p2.CreateStructure(Structure.STORAGE_0_ID);
                        npos = landingSurface.pos.OneBlockLeft();
                        p2 = chunk.GetSurfacePlane(npos);
                        if (p2 == null) p2 = chunk.GetBlock(npos).FORCED_GetPlane(Block.UP_FACE_INDEX);
                        setCenter = p2.CreateStructure(Structure.SETTLEMENT_CENTER_ID);
                    }
                    var et = GameMaster.realMaster.eventTracker;
                    et.BuildingConstructed(hq);
                    if (storage != null) et.BuildingConstructed(storage);
                    if (setCenter != null) et.BuildingConstructed(setCenter);
                    
                    GameMaster.realMaster.SetStartResources();
                    PoolMaster.current.BuildSplash(transform.position);
                    if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.ColonyFounded);
                    FollowingCamera.main.SetLookPoint(hq.transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }

    public void Raycasting()
    {
        if (GameMaster.gameSpeed == 0 || landPointSet || destroyed) return;
        RaycastHit rh;
        if (Physics.Raycast(FollowingCamera.cam.ScreenPointToRay(Input.mousePosition), out rh))
        {
            GameObject collided = rh.collider.gameObject;
            if (collided.tag == Chunk.BLOCK_COLLIDER_TAG)
            {
                Chunk chunk = GameMaster.realMaster.mainChunk;
                ChunkRaycastHit crh = chunk.GetBlock(rh.point, rh.normal);                
                Block b = crh.block;

                if (b != null && crh.faceIndex == Block.UP_FACE_INDEX)
                {
                    landingSurface = null;
                    landingByZAxis = null;                    
                    Plane minusTwoBlock, minusOneBlock, plusOneBlock, plusTwoBlock;
                    int x = b.pos.x, y = b.pos.y, z = b.pos.z;
                    bool[] suitable = new bool[5];
                    suitable[2] = true;

                    bool Check(Plane p)
                    {
                        return p != null && p.isQuad && !p.isInvicible && p.mainStructure == null && !chunk.IsUnderOtherBlock(p);
                    }
                    // direction 0 - fwd    
                    {
                        
                        minusOneBlock = chunk.GetSurfacePlane(x, y, z - 1); suitable[1] = Check(minusOneBlock) ;
                        minusTwoBlock = chunk.GetSurfacePlane(x, y, z - 2); suitable[0] = Check(minusTwoBlock);
                        plusOneBlock = chunk.GetSurfacePlane(x, y, z + 1); suitable[3] = Check(plusOneBlock);
                        plusTwoBlock = chunk.GetSurfacePlane(x, y, z + 2); suitable[4] = Check(plusTwoBlock);
                        if (suitable[1])
                        {
                            if (suitable[0])
                            {
                                landingByZAxis = true;
                                landingSurface = minusOneBlock;
                                goto DRAW_LINE;
                            }
                            else
                            {
                                if (suitable[3])
                                {
                                    landingByZAxis = true;
                                    landingSurface = b.FORCED_GetPlane(crh.faceIndex);
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
                                    landingSurface = plusOneBlock as Plane;
                                    goto DRAW_LINE;
                                }
                            }
                        }
                    }
                    //direction 2 - right
                    {
                        minusOneBlock = chunk.GetSurfacePlane(x - 1, y, z); suitable[1] = Check(minusOneBlock);
                        minusTwoBlock = chunk.GetSurfacePlane(x - 2, y, z - 2); suitable[0] = Check(minusTwoBlock);
                        plusOneBlock = chunk.GetSurfacePlane(x + 1, y, z); suitable[3] = Check(plusOneBlock);
                        plusTwoBlock = chunk.GetSurfacePlane(x + 2, y, z + 2); suitable[4] = Check(plusTwoBlock);
                        if (suitable[1])
                        {
                            if (suitable[0])
                            {
                                landingByZAxis = false;
                                landingSurface = minusOneBlock as Plane;
                                goto DRAW_LINE;
                            }
                            else
                            {
                                if (suitable[3])
                                {
                                    landingByZAxis = false;
                                    landingSurface = b.FORCED_GetPlane(crh.faceIndex);
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
                                    landingSurface = plusOneBlock as Plane;
                                    goto DRAW_LINE;
                                }
                            }
                        }
                    }
                    if (landingSurface == null)
                    {
                        DropSelectedSurface();                      
                        return;
                    }
                    DRAW_LINE:
                    Vector3[] positions = new Vector3[4];
                    Vector3 lpos = landingSurface.GetCenterPosition();
                    float q = Block.QUAD_SIZE;
                    float h = lpos.y + 0.01f;
                    if (landingByZAxis == false)
                    {
                        positions[0] = new Vector3( lpos.x - 1.5f * q, h, lpos.z + 0.5f * q );
                        positions[1] = new Vector3(lpos.x + 1.5f * q, h, lpos.z + 0.5f * q);
                        positions[2] = new Vector3(lpos.x + 1.5f * q, h, lpos.z - 0.5f * q);
                        positions[3] = new Vector3(lpos.x - 1.5f * q, h, lpos.z - 0.5f * q);
                        //positions[4] = positions[0];
                    }
                    else
                    {
                        positions[0] = new Vector3(lpos.x - 0.5f * q, h, lpos.z + 1.5f * q);
                        positions[1] = new Vector3(lpos.x + 0.5f * q, h, lpos.z + 1.5f * q);
                        positions[2] = new Vector3(lpos.x + 0.5f * q, h, lpos.z - 1.5f * q);
                        positions[3] = new Vector3(lpos.x - 0.5f * q, h, lpos.z - 1.5f * q);
                        //positions[4] = positions[0];
                    }
                    lineDrawer.SetPositions(positions);
                    lineDrawer.enabled = true;
                    landingMarkObject.transform.position = lpos + Vector3.down * Block.QUAD_SIZE * 0.45f;
                    landingMarkObject.SetActive(true);
                    landButton.SetActive(true);
                }
            }
        }
    }
    public void DropSelectedSurface()
    {
        lineDrawer.enabled = false;
        landingMarkObject.SetActive(false);
        landButton.SetActive(false);
    }

    public void LandActionAccepted()
    {
        if (!landPointSet & !destroyed)
        {
            if (landingSurface != null)
            {
                landPointSet = true;
                Vector3 newPos = landingSurface.pos.ToWorldSpace();
                PoolMaster.current.BuildSplash(newPos);
                transform.position = newPos + Vector3.up;
                if (landingByZAxis == true) transform.rotation = Quaternion.identity;
                else transform.rotation = Quaternion.Euler(0, 90, 0);
                lineDrawer.enabled = false;
                Destroy(landingMarkObject);
                landButton.SetActive(false);
                destructionTimer = 3;
            }
        }
    }

    private void OnDestroy()
    {
       Destroy( landingCanvas.gameObject);
    }
}
