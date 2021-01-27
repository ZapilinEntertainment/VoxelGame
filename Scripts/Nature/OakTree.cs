﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class OakTree : Plant
{
    private static List<OakTree> oaks;

    private static Sprite[] startStageSprites;
    private static Transform blankModelsContainer;// хранит неиспользуемые модели
    private static List<GameObject> blankTrees_stage4, blankTrees_stage5, blankTrees_stage6;
    private static Sprite[] lodPack_stage4, lodPack_stage5, lodPack_stage6;

    private static bool modelsContainerReady = false, typeRegistered = false;
    private static float clearTimer = 0;

    public static readonly LODRegisterInfo oak4_lod_regInfo = new LODRegisterInfo(LODController.OAK_MODEL_ID, 4, 0);
    public static readonly LODRegisterInfo oak5_lod_regInfo = new LODRegisterInfo(LODController.OAK_MODEL_ID, 5, 0);
    public static readonly LODRegisterInfo oak6_lod_regInfo = new LODRegisterInfo(LODController.OAK_MODEL_ID, 6, 0);

    public const byte HARVESTABLE_STAGE = 4;
    private const byte TRANSIT_STAGE = 3;
    private const int MAX_INACTIVE_BUFFERED_STAGE4 = 12, MAX_INACTIVE_BUFFERED_STAGE5 = 6, MAX_INACTIVE_BUFFERED_STAGE6 = 3, CRITICAL_BUFFER_COUNT = 50;
    private const float CLEAR_BLANKS_TIME = 10f, PER_LEVEL_LIFEPOWER_SURPLUS = 0.6f;

    private GameObject modelHolder;
    private SpriteRenderer spriter;
    private enum OakDrawMode { NoDraw, DrawStartSprite, DrawModel, DrawLOD }
    private OakDrawMode drawmode;
    private byte lodNumber = 0;
    private bool addedToClassList = false;

    public const byte MAX_STAGE = 6;
    public const int LUMBER = 100, SPRITER_CHILDNUMBER = 0, MODEL_CHILDNUMBER = 1;
    private const float TREE_SPRITE_MAX_VISIBILITY = 8;

    static OakTree()
    {
        oaks = new List<OakTree>();
        AddToResetList(typeof(OakTree));
    }
    public static void ResetStaticData()
    {
        if (blankModelsContainer == null)   modelsContainerReady = false;
        oaks.Clear();
        typeRegistered = false;
    }

    // тоже можно рассовать по методам
    override protected void SetModel()
    {
        // проверка на предыдущую модель не нужна  - устанавливается через SetStage  
        if (!modelsContainerReady) // первая загрузка
        {
            startStageSprites = Resources.LoadAll<Sprite>("Textures/Plants/oakTree");
            blankModelsContainer = new GameObject("oakTreesContainer").transform;

            blankTrees_stage4 = new List<GameObject>();
            blankTrees_stage5 = new List<GameObject>();
            blankTrees_stage6 = new List<GameObject>();

            //stage 4 model
            GameObject model3d = LoadModel(4);
            
            // хоть это и первая загрузка, лучше всё равно проверить
            LODController currentLC = LODController.GetCurrent();
            int regIndex = currentLC.LOD_existanceCheck(oak4_lod_regInfo);
            if (regIndex == -1)
            {
                RenderPoint[] rpoints = new RenderPoint[] {
                new RenderPoint(new Vector3(0, 0.222f, -0.48f), Vector3.zero),
                 new RenderPoint(new Vector3(0, 0.479f, -0.434f), new Vector3(30, 0, 0)),
                  new RenderPoint(new Vector3(0, 0.458f, -0.232f), new Vector3(45, 0, 0)),
                   new RenderPoint(new Vector3(0, 0.551f, -0.074f), new Vector3(75, 0, 0))
            };
                regIndex = LODSpriteMaker.current.CreateLODPack(LODPackType.OneSide, model3d, rpoints, 32, 0.25f, Color.green, oak4_lod_regInfo);
            }         
             LODRegistrationTicket rticket = currentLC.registeredLODs[regIndex];
            lodPack_stage4 = new Sprite[4];
            lodPack_stage4[0] = rticket.sprites[0];
            lodPack_stage4[1] = rticket.sprites[1];
            lodPack_stage4[2] = rticket.sprites[2];
            lodPack_stage4[3] = rticket.sprites[3];


            GameObject fullModel = new GameObject("oak4");
            fullModel.SetActive(false);

            GameObject spriterCarrier = new GameObject("lodSpriter");
            SpriteRenderer sr = spriterCarrier.AddComponent<SpriteRenderer>();
            //сначала добавляется спрайт
            sr.sprite = lodPack_stage4[0];
            sr.sharedMaterial = !PoolMaster.useDefaultMaterials ? PoolMaster.billboardShadedMaterial: PoolMaster.billboardMaterial;
            if (PoolMaster.shadowCasting) sr.receiveShadows = true;
            spriterCarrier.transform.parent = fullModel.transform;
            spriterCarrier.transform.localPosition = Vector3.up * 0.211f;
            // потом модель
            model3d.transform.parent = fullModel.transform;
            model3d.transform.localPosition = Vector3.zero;
            model3d.transform.localRotation = Quaternion.identity;
            fullModel.transform.parent = blankModelsContainer;
            blankTrees_stage4.Add(fullModel);

            // stage 5 model     
            model3d = LoadModel(5);
            regIndex = currentLC.LOD_existanceCheck(oak5_lod_regInfo);
            if (regIndex == -1)
            {
                RenderPoint[] rpoints = new RenderPoint[] {
                new RenderPoint(new Vector3(0, 0.222f, -0.48f), Vector3.zero),
                 new RenderPoint(new Vector3(0, 0.479f, -0.434f), new Vector3(30, 0, 0)),
                  new RenderPoint(new Vector3(0, 0.458f, -0.232f), new Vector3(45, 0, 0)),
                   new RenderPoint(new Vector3(0, 0.551f, -0.074f), new Vector3(75, 0, 0))
            };
                regIndex = LODSpriteMaker.current.CreateLODPack(LODPackType.OneSide, model3d, rpoints, 32, 0.3f, Color.green, oak5_lod_regInfo);
            }
            rticket = currentLC.registeredLODs[regIndex];
            lodPack_stage5 = new Sprite[4];
            lodPack_stage5[0] = rticket.sprites[0];
            lodPack_stage5[1] = rticket.sprites[1];
            lodPack_stage5[2] = rticket.sprites[2];
            lodPack_stage5[3] = rticket.sprites[3];
            fullModel = new GameObject("oak5");
            fullModel.SetActive(false);
            spriterCarrier = new GameObject("lodSpriter");
            sr = spriterCarrier.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = !PoolMaster.useDefaultMaterials ? PoolMaster.billboardShadedMaterial : PoolMaster.billboardMaterial; ;
            if (PoolMaster.shadowCasting) sr.receiveShadows = true;
            sr.sprite = lodPack_stage5[0];
            spriterCarrier.transform.parent = fullModel.transform;
            spriterCarrier.transform.localPosition = Vector3.up * 0.239f;
            model3d.transform.parent = fullModel.transform;
            model3d.transform.localPosition = Vector3.zero;
            model3d.transform.localRotation = Quaternion.identity;
            fullModel.transform.parent = blankModelsContainer;
            blankTrees_stage5.Add(fullModel);

            //stage 6 model
            model3d = LoadModel(6);
            regIndex = currentLC.LOD_existanceCheck(oak6_lod_regInfo);
            if (regIndex == -1)
            {
                RenderPoint[] rpoints = new RenderPoint[] {
                new RenderPoint(new Vector3(0, 0.222f, -0.48f), Vector3.zero),
                 new RenderPoint(new Vector3(0, 0.479f, -0.434f), new Vector3(30, 0, 0)),
                  new RenderPoint(new Vector3(0, 0.458f, -0.232f), new Vector3(45, 0, 0)),
                   new RenderPoint(new Vector3(0, 0.551f, -0.074f), new Vector3(75, 0, 0))
            };
                regIndex = LODSpriteMaker.current.CreateLODPack(LODPackType.OneSide, model3d, rpoints, 64, 0.45f, Color.green, oak6_lod_regInfo);
            }
            rticket = currentLC.registeredLODs[regIndex];
            lodPack_stage6 = new Sprite[4];
            lodPack_stage6[0] = rticket.sprites[0];
            lodPack_stage6[1] = rticket.sprites[1];
            lodPack_stage6[2] = rticket.sprites[2];
            lodPack_stage6[3] = rticket.sprites[3];

            fullModel = new GameObject("oak6");
            fullModel.SetActive(false);
            spriterCarrier = new GameObject("lodSpriter");
            sr = spriterCarrier.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = !PoolMaster.useDefaultMaterials ? PoolMaster.billboardShadedMaterial : PoolMaster.billboardMaterial; ;
            if (PoolMaster.shadowCasting) sr.receiveShadows = true;
            sr.sprite = lodPack_stage6[0];
            spriterCarrier.transform.parent = fullModel.transform;
            spriterCarrier.transform.localPosition = Vector3.up * 0.21f;
            model3d.transform.parent = fullModel.transform;
            model3d.transform.localPosition = Vector3.zero;
            model3d.transform.localRotation = Quaternion.identity;
            fullModel.transform.parent = blankModelsContainer;
            blankTrees_stage6.Add(fullModel);
            //
            modelsContainerReady = true;
            FollowingCamera.main.cameraChangedEvent += CameraUpdate;
        }
        if (stage > TRANSIT_STAGE)
        {
            switch (stage)
            {
                case 4:
                    if (blankTrees_stage4.Count > 1)
                    {
                        int lastIndex = blankTrees_stage4.Count - 1;
                        modelHolder = blankTrees_stage4[lastIndex];
                        blankTrees_stage4.RemoveAt(lastIndex);
                        if (lastIndex > MAX_INACTIVE_BUFFERED_STAGE4)
                        {
                            Destroy(blankTrees_stage4[lastIndex - 1]);
                            blankTrees_stage4.RemoveAt(lastIndex - 1);
                        }
                    }
                    else
                    {
                        modelHolder = Instantiate(blankTrees_stage4[0], Vector3.zero, Quaternion.identity, transform);
                    }
                    break;
                case 5:
                    if (blankTrees_stage5.Count > 1)
                    {
                        int lastIndex = blankTrees_stage5.Count - 1;
                        modelHolder = blankTrees_stage5[lastIndex];
                        blankTrees_stage5.RemoveAt(lastIndex);
                        if (lastIndex > MAX_INACTIVE_BUFFERED_STAGE5)
                        {
                            Destroy(blankTrees_stage5[lastIndex - 1]);
                            blankTrees_stage5.RemoveAt(lastIndex - 1);
                        }
                    }
                    else modelHolder = Instantiate(blankTrees_stage5[0], Vector3.zero, Quaternion.identity, transform);
                    break;
                case 6:
                    if (blankTrees_stage6.Count > 1)
                    {
                        int lastIndex = blankTrees_stage6.Count - 1;
                        modelHolder = blankTrees_stage6[lastIndex];
                        blankTrees_stage6.RemoveAt(lastIndex);
                        if (lastIndex > MAX_INACTIVE_BUFFERED_STAGE6)
                        {
                            Destroy(blankTrees_stage6[lastIndex - 1]);
                            blankTrees_stage6.RemoveAt(lastIndex - 1);
                        }
                    }
                    else modelHolder = Instantiate(blankTrees_stage6[0], Vector3.zero, Quaternion.identity, transform);
                    break;
            }
            modelHolder.transform.parent = transform;
            modelHolder.transform.localPosition = Vector3.zero;
            modelHolder.transform.rotation = Quaternion.Euler(0, modelRotation * 90, 0);
            spriter = modelHolder.transform.GetChild(SPRITER_CHILDNUMBER).GetComponent<SpriteRenderer>();
            // # model draw mode check
            float dist = (transform.position - FollowingCamera.camPos).magnitude;
            if (dist < TREE_SPRITE_MAX_VISIBILITY * stage)
            {
                if (dist < LODController.lodCoefficient)
                {
                    modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(true); // model
                    spriter.enabled = false;  // lod sprite
                    drawmode = OakDrawMode.DrawModel;
                }
                else
                {
                    modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false); // model                    
                    // # setting lod (changed)
                    drawmode = OakDrawMode.DrawLOD;
                    byte spriteNumber = 0;
                    float angle = Vector3.Angle(Vector3.up, FollowingCamera.camPos - transform.position);
                    if (angle < 30)
                    {
                        if (angle < 10) spriteNumber = 3;
                        else spriteNumber = 2;
                    }
                    else
                    {
                        if (angle > 80) spriteNumber = 0;
                        else spriteNumber = 1;
                    }
                    switch (stage)
                    {
                        case 4: spriter.sprite = lodPack_stage4[spriteNumber]; break;
                        case 5: spriter.sprite = lodPack_stage5[spriteNumber]; break;
                        case 6: spriter.sprite = lodPack_stage6[spriteNumber]; break;
                    }
                    lodNumber = spriteNumber;
                    // eo setting lod
                    spriter.enabled = true; // lod sprite
                }
            }
            else
            {
                spriter.enabled = false;
                drawmode = OakDrawMode.NoDraw;
            }
            modelHolder.SetActive(true);
        }
        else
        {
            GameObject model = new GameObject("sprite");
            Transform modelTransform = model.transform;
            modelTransform.parent = transform;
            modelTransform.localPosition = Vector3.zero;
            //Vector3 cpos = modelTransform.InverseTransformPoint(FollowingCamera.camPos); cpos.y = 0;
            // modelTransform.LookAt(cpos);
            spriter = modelTransform.gameObject.AddComponent<SpriteRenderer>();
            spriter.sprite = startStageSprites[stage];
            spriter.sharedMaterial = PoolMaster.verticalWavingBillboardMaterial;
            spriter.enabled = ((transform.position - FollowingCamera.camPos).magnitude < TREE_SPRITE_MAX_VISIBILITY * stage);
            drawmode = OakDrawMode.DrawStartSprite;
        }
    }

    static GameObject LoadModel(byte stage)
    {
        GameObject g =  Instantiate(Resources.Load<GameObject>("Lifeforms/oak-" + stage.ToString()));
        if (!PoolMaster.useDefaultMaterials) PoolMaster.ReplaceMaterials(g);
        return g;
    }

    override public void Prepare()
    {
        PrepareStructure();
        surfaceRect = SurfaceRect.one; isArtificial = false;
        type = GetPlantType();
        stage = 1;
    }
    public static new PlantType GetPlantType()
    {
        return PlantType.OakTree;
    }
    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        basement = b;
        surfaceRect = new SurfaceRect(pos.x, pos.y, surfaceRect.size);
        if (spriter == null) SetModel();
        b.AddStructure(this);
        if (!addedToClassList)
        {
            oaks.Add(this);
            addedToClassList = true;
            if (!typeRegistered)
            {
                GameMaster.realMaster.mainChunk.GetNature().RegisterNewLifeform(type);
                typeRegistered = true;
            }
        }
    }

    public static void UpdatePlants()
    {
        if (oaks.Count == 0)
        {
            if (typeRegistered)
            {
                GameMaster.realMaster.mainChunk.GetNature().UnregisterLifeform(GetPlantType());
                typeRegistered = false;
            }
        }
        clearTimer -= Time.deltaTime;
        if (clearTimer <= 0)
        {
            if (blankTrees_stage4.Count > MAX_INACTIVE_BUFFERED_STAGE4) blankTrees_stage4.RemoveAt(blankTrees_stage4.Count - 1);
            if (blankTrees_stage5.Count > MAX_INACTIVE_BUFFERED_STAGE5) blankTrees_stage5.RemoveAt(blankTrees_stage5.Count - 1);
            if (blankTrees_stage6.Count > MAX_INACTIVE_BUFFERED_STAGE6) blankTrees_stage6.RemoveAt(blankTrees_stage6.Count - 1);
            clearTimer = CLEAR_BLANKS_TIME;
        }
    }
    public static void CameraUpdate()
    {
        int count = oaks.Count;
        if (count > 0)
        {
            int i = 0;
            Vector3 camPos = FollowingCamera.camPos;
            Transform t;
            Vector3 cpos;
            OakDrawMode newDrawMode = OakDrawMode.NoDraw;
            float dist, lodDist = LODController.lodCoefficient;

            if (!PoolMaster.shadowCasting)
            {
                while (i < count)
                {
                    OakTree oak = oaks[i];
                    if (oak == null) {
                        oaks.RemoveAt(i);
                        count--;
                        continue; }
                    else
                    {
                        if (!oak.isVisible) { i++; continue; }
                        dist = (oak.transform.position - camPos).magnitude;
                        if (oak.stage <= TRANSIT_STAGE)
                        {                            
                            if (dist > TREE_SPRITE_MAX_VISIBILITY * oak.stage)
                            {
                                if (oak.drawmode != OakDrawMode.NoDraw)
                                {
                                    oak.spriter.enabled = false;
                                    oak.drawmode = OakDrawMode.NoDraw;
                                }
                            }
                            else
                            {
                                if (oak.drawmode != OakDrawMode.DrawStartSprite)
                                {
                                    oak.drawmode = OakDrawMode.DrawStartSprite;
                                    oak.spriter.enabled = true;
                                }
                            }
                        }
                        else
                        {                        // # change model draw mode
                            float x = TREE_SPRITE_MAX_VISIBILITY + 3 * oak.stage;
                            x = dist / x;
                            if (x > lodDist)
                            {
                                if (x > 1) newDrawMode = OakDrawMode.NoDraw; else newDrawMode = OakDrawMode.DrawLOD;
                            }
                            else newDrawMode = OakDrawMode.DrawModel;
                            if (newDrawMode != oak.drawmode)
                            {
                                if (newDrawMode == OakDrawMode.NoDraw)
                                {
                                    oak.spriter.enabled = false;
                                    oak.modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                                }
                                else
                                {
                                    if (newDrawMode == OakDrawMode.DrawModel)
                                    {
                                        oak.spriter.enabled = false;
                                        oak.modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        oak.spriter.enabled = true;
                                        oak.modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                                    }
                                }
                                oak.drawmode = newDrawMode;
                            }
                            // # setting lod
                            if (oak.drawmode == OakDrawMode.DrawLOD)
                            {
                                byte spriteNumber = 0;
                                float angle = Vector3.Angle(Vector3.up, camPos - oak.transform.position);
                                if (angle < 30)
                                {
                                    if (angle < 10) spriteNumber = 3;
                                    else spriteNumber = 2;
                                }
                                else
                                {
                                    if (angle > 85) spriteNumber = 0;
                                    else spriteNumber = 1;
                                }
                                if (spriteNumber != oak.lodNumber)
                                {
                                    switch (oak.stage)
                                    {
                                        case 4: oak.spriter.sprite = lodPack_stage4[spriteNumber]; break;
                                        case 5: oak.spriter.sprite = lodPack_stage5[spriteNumber]; break;
                                        case 6: oak.spriter.sprite = lodPack_stage6[spriteNumber]; break;
                                    }
                                    oak.lodNumber = spriteNumber;
                                }
                            }
                            // eo setting lod
                        }
                        i++;
                    }
                }
            }
            else
            { // то же самое, только с разворотом на камеру
                while (i < count)
                {
                    OakTree oak = oaks[i];
                    if (oak == null) { oaks.RemoveAt(i); continue; }
                    else
                    {
                        if (!oak.isVisible) { i++; continue; }
                        dist = (oak.transform.position - camPos).magnitude;
                        if (oak.stage <= TRANSIT_STAGE)
                        {
                            if (dist > TREE_SPRITE_MAX_VISIBILITY * oak.stage)
                            {
                                if (oak.drawmode != OakDrawMode.NoDraw)
                                {
                                    oak.spriter.enabled = false;
                                    oak.drawmode = OakDrawMode.NoDraw;
                                }
                            }
                            else
                            {
                                if (oak.drawmode != OakDrawMode.DrawStartSprite)
                                {
                                    oak.drawmode = OakDrawMode.DrawStartSprite;
                                    oak.spriter.enabled = true;
                                }
                                t = oak.spriter.transform;
                                cpos = Vector3.ProjectOnPlane(camPos - t.position, t.up);
                                t.forward = cpos.normalized;
                            }
                        }
                        else
                        {                        // # change model draw mode
                            float x = TREE_SPRITE_MAX_VISIBILITY + 3 * oak.stage;
                            x = dist / x;
                            if (x > lodDist)
                            {
                                if (x > 1) newDrawMode = OakDrawMode.NoDraw; else newDrawMode = OakDrawMode.DrawLOD;
                            }
                            else newDrawMode = OakDrawMode.DrawModel;
                            if (newDrawMode != oak.drawmode)
                            {
                                if (newDrawMode == OakDrawMode.NoDraw)
                                {
                                    oak.spriter.enabled = false;
                                    oak.modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                                }
                                else
                                {
                                    if (newDrawMode == OakDrawMode.DrawModel)
                                    {
                                        oak.spriter.enabled = false;
                                        oak.modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        oak.spriter.enabled = true;
                                        oak.modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                                    }
                                }
                                oak.drawmode = newDrawMode;
                            }
                            // # setting lod
                            if (oak.drawmode == OakDrawMode.DrawLOD)
                            {
                                byte spriteNumber = 0;
                                float angle = Vector3.Angle(Vector3.up, camPos - oak.transform.position);
                                if (angle < 30)
                                {
                                    if (angle < 10) spriteNumber = 3;
                                    else spriteNumber = 2;
                                }
                                else
                                {
                                    if (angle > 85) spriteNumber = 0;
                                    else spriteNumber = 1;
                                }
                                if (spriteNumber != oak.lodNumber)
                                {
                                    switch (oak.stage)
                                    {
                                        case 4: oak.spriter.sprite = lodPack_stage4[spriteNumber]; break;
                                        case 5: oak.spriter.sprite = lodPack_stage5[spriteNumber]; break;
                                        case 6: oak.spriter.sprite = lodPack_stage6[spriteNumber]; break;
                                    }
                                    oak.lodNumber = spriteNumber;
                                }
                                oak.spriter.transform.forward =  oak.transform.position - camPos;
                            }
                            // eo setting lod
                        }
                        i++;
                    }
                }
            }
        }
    }

    override protected void SetStage(byte newStage)
    {
        if (destroyed || newStage == stage) return;
        if (transform.childCount != 0)
        {
            if (stage <= TRANSIT_STAGE )
            {
                if (newStage > TRANSIT_STAGE)
                {
                    Destroy(transform.GetChild(0).gameObject);
                    spriter = null;
                    modelHolder = null;
                    stage = newStage;
                    SetModel();
                }
                else
                {
                    stage = newStage;
                    spriter.sprite = startStageSprites[stage];
                    spriter.enabled = ((transform.position - FollowingCamera.camPos).magnitude < TREE_SPRITE_MAX_VISIBILITY * stage);
                    drawmode = OakDrawMode.DrawStartSprite;
                }
            }
            else
            {
                ReturnModelToPool();
                stage = newStage;
                SetModel();
            }
        }
        if (PoolMaster.qualityLevel > 0) PoolMaster.current.LifepowerSplash(transform.position, stage);
        SetVisibility(isVisible);
        Grassland g = basement.GetGrassland();
        if (!GameMaster.loading && g != null) g.needRecalculation = true; 
    }
    override public bool IsFullGrown()
    {
        return stage == MAX_STAGE;
    }
    override public float GetLifepowerSurplus()
    {
        return PER_LEVEL_LIFEPOWER_SURPLUS * stage;
    }
    public override int GetPlantComplexity()
    {
        return stage * stage * 10;
    }

    override public void Harvest(bool replenish)
    {
        if (destroyed) return;
        GameMaster.realMaster.colonyController.storage.AddResource(ResourceType.Lumber, CountLumber());
        if (stage > TRANSIT_STAGE & modelHolder != null)
        {
            modelHolder.transform.parent = null;
            FallingTree ft = modelHolder.gameObject.AddComponent<FallingTree>();
            ft.SetModelStage(stage);
            ft.returnFunction = ReturnModelToPool;
            modelHolder = null;
            spriter = null;
        }
        //if (!replenish) 
        Annihilate(true, false, false); // реплениш отключен, тк глючит - не успевает поставить спрайтер до обновления
        //else ResetToDefaults();
    }
    override public void Dry(bool sendMessageToGrassland)
    {
        if (!sendMessageToGrassland) basement?.RemoveStructure(this);
        if (stage > TRANSIT_STAGE)
        {
            ContainerModelType cmtype;
            if (stage == 4) cmtype = ContainerModelType.DeadOak4;
            else
            {
                if (stage == 5) cmtype = ContainerModelType.DeadOak5;
                else cmtype = ContainerModelType.DeadOak6;
            }
            HarvestableResource hr = HarvestableResource.ConstructContainer(cmtype, ResourceType.Lumber, CountLumber() * GameMaster.realMaster.environmentMaster.environmentalConditions);
            hr.SetModelRotation(modelRotation);
            hr.SetBasement(basement, new PixelPosByte(surfaceRect.x, surfaceRect.z));
            // спрайтовый LOD?
        }
        else
        {
            Structure s = GetStructureByID(DRYED_PLANT_ID);
            s.SetBasement(basement, new PixelPosByte(surfaceRect.x, surfaceRect.z));
            StructureTimer st = s.gameObject.AddComponent<StructureTimer>();
            st.timer = 5;
        }
    }

    float CountLumber()
    {
        switch (stage)
        {
            default: return LUMBER / 10f;
            case 4: return LUMBER / 4f;
            case 5: return LUMBER / 2f;
            case 6: return LUMBER;
        }
    }
    override public void SetVisibility(bool x)
    {
        if (destroyed | x == isVisible) return;
        else
        {
            isVisible = x;
            if (isVisible)
            {
                if (modelHolder != null)
                {                    
                    // # change model draw mode (changed)
                    float dist = (transform.position - FollowingCamera.camPos).magnitude;
                    if (dist > LODController.lodCoefficient)
                    {
                        if (dist > TREE_SPRITE_MAX_VISIBILITY * stage)
                        {
                            drawmode = OakDrawMode.NoDraw;
                            spriter.enabled = false;
                            modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                        }
                        else
                        {
                            drawmode = OakDrawMode.DrawLOD;
                            spriter.enabled = true;
                            modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                            // # setting lod(changed)
                            byte spriteNumber = 0;
                            float angle = Vector3.Angle(Vector3.up, FollowingCamera.camPos - transform.position);
                            if (angle < 30)
                            {
                                if (angle < 10) spriteNumber = 3;
                                else spriteNumber = 2;
                            }
                            else
                            {
                                if (angle > 80) spriteNumber = 0;
                                else spriteNumber = 1;
                            }
                            switch (stage)
                            {
                                case 4: spriter.sprite = lodPack_stage4[spriteNumber]; break;
                                case 5: spriter.sprite = lodPack_stage5[spriteNumber]; break;
                                case 6: spriter.sprite = lodPack_stage6[spriteNumber]; break;
                            }
                            lodNumber = spriteNumber;
                            // eo setting lod
                        }
                        modelHolder.SetActive(true);
                    }
                    else
                    {
                        drawmode = OakDrawMode.DrawModel;
                        spriter.enabled = false;
                        modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(true);
                    }
                }
                else spriter.enabled = isVisible;
            }
            else
            {
                if (modelHolder != null) modelHolder.SetActive(false);
                if (spriter != null) spriter.enabled = false;
            }
        }
    }

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        if (!clearFromSurface) basement = null;
        else
        {
            if (basement != null)
            {
                basement.RemoveStructure(this);
                //if (basement.grassland != null) basement.grassland.AddLifepower((int)(lifepower * GameMaster.realMaster.lifepowerLossesPercent));
            }
        }
        if (addedToClassList)
        {
            if (oaks.Count > 0)
            {
                int index = GetInstanceID();
                for (int i = 0; i < oaks.Count; i++)
                {
                    if (oaks[i].GetInstanceID() == index)
                    {
                        oaks.RemoveAt(i);
                        break;
                    }
                } 
            }           
            addedToClassList = false;
        }
        basement = null;
        ReturnModelToPool();
        Destroy(gameObject);
    }

    private void ReturnModelToPool()
    {
        if (stage <= TRANSIT_STAGE | modelHolder == null) return;
        modelHolder.transform.parent = blankModelsContainer;
        modelHolder.gameObject.SetActive(false);
        switch (stage)
        {
            case 4: if (blankTrees_stage4.Count < CRITICAL_BUFFER_COUNT) blankTrees_stage4.Add(modelHolder); else Destroy(modelHolder); break;
            case 5: if (blankTrees_stage5.Count < CRITICAL_BUFFER_COUNT) blankTrees_stage5.Add(modelHolder); else Destroy(modelHolder); break;
            case 6: if (blankTrees_stage6.Count < CRITICAL_BUFFER_COUNT) blankTrees_stage6.Add(modelHolder); else Destroy(modelHolder); break;
        }
        modelHolder = null;
        spriter = null;
        drawmode = OakDrawMode.NoDraw;
    }
    public static void ReturnModelToPool(GameObject model, byte stage)
    {
        switch (stage)
        {
            case 4: if (blankTrees_stage4.Count < CRITICAL_BUFFER_COUNT) blankTrees_stage4.Add(model); else Destroy(model); break;
            case 5: if (blankTrees_stage5.Count < CRITICAL_BUFFER_COUNT) blankTrees_stage5.Add(model); else Destroy(model); break;
            case 6: if (blankTrees_stage6.Count < CRITICAL_BUFFER_COUNT) blankTrees_stage6.Add(model); else Destroy(model); break;
            default: Destroy(model); return;
        }
        model.SetActive(false);
        model.transform.parent = blankModelsContainer;
    }
}

