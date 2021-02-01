using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class OakTree : Plant
{
    private static Sprite[] startStageSprites;
    private static Transform blankModelsContainer;// хранит неиспользуемые модели
    private static List<GameObject> blankTrees_stage4, blankTrees_stage5, blankTrees_stage6;
    private static Sprite[] lodPack_stage4, lodPack_stage5, lodPack_stage6;

    private static bool modelsContainerReady = false, typeRegistered = false;
    private static int oaksCount = 0;

    public static readonly LODRegisterInfo oak4_lod_regInfo = new LODRegisterInfo(LODController.OAK_MODEL_ID, 4, 0);
    public static readonly LODRegisterInfo oak5_lod_regInfo = new LODRegisterInfo(LODController.OAK_MODEL_ID, 5, 0);
    public static readonly LODRegisterInfo oak6_lod_regInfo = new LODRegisterInfo(LODController.OAK_MODEL_ID, 6, 0);

    public const byte HARVESTABLE_STAGE = 4;
    private const byte TRANSIT_STAGE = 3;
    private const int MAX_INACTIVE_BUFFERED_STAGE4 = 12, MAX_INACTIVE_BUFFERED_STAGE5 = 6, MAX_INACTIVE_BUFFERED_STAGE6 = 3, CRITICAL_BUFFER_COUNT = 50;
    private const float PER_LEVEL_LIFEPOWER_SURPLUS = 0.6f;

    private GameObject modelHolder;
    private SpriteRenderer spriter;
    private enum OakDrawMode { NoDraw, DrawSaplingSprite, DrawModel, DrawLOD }
    private OakDrawMode drawmode;
    private byte lodNumber = 0;
    private bool subscribedToVisibilityEvent = false;

    public const byte MAX_STAGE = 6;
    public const int LUMBER = 100, SPRITER_CHILDNUMBER = 0, MODEL_CHILDNUMBER = 1;
    public const float COMPLEXITY = 10;

    static OakTree()
    {
        AddToResetList(typeof(OakTree));
    }
    public static void ResetStaticData()
    {
        if (blankModelsContainer == null)   modelsContainerReady = false;
        typeRegistered = false;
        oaksCount = 0;
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
            drawmode = OakDrawMode.DrawSaplingSprite;
        }
        RefreshVisibility();
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
        if (spriter == null && modelHolder == null) SetModel();
        b.AddStructure(this);        

        if (!typeRegistered)
        {
            GameMaster.realMaster.mainChunk.GetNature().RegisterNewLifeform(type);
            typeRegistered = true;
        }

        if (!subscribedToVisibilityEvent)
        {
            basement.visibilityChangedEvent += this.SetVisibility;
            subscribedToVisibilityEvent = true;
            oaksCount++;
        } 
    }

    public static void UpdatePool()
    {
        if (oaksCount == 0)
        {
            if (typeRegistered)
            {
                GameMaster.realMaster.mainChunk.GetNature().UnregisterLifeform(GetPlantType());
                typeRegistered = false;
            }
        }
            if (blankTrees_stage4.Count > MAX_INACTIVE_BUFFERED_STAGE4) blankTrees_stage4.RemoveAt(blankTrees_stage4.Count - 1);
            if (blankTrees_stage5.Count > MAX_INACTIVE_BUFFERED_STAGE5) blankTrees_stage5.RemoveAt(blankTrees_stage5.Count - 1);
            if (blankTrees_stage6.Count > MAX_INACTIVE_BUFFERED_STAGE6) blankTrees_stage6.RemoveAt(blankTrees_stage6.Count - 1);
    }

    protected override void INLINE_SetVisibility(VisibilityMode vm)
    {
        byte vmode = (byte)vm;
        if (stage <= TRANSIT_STAGE) // Спрайт
        {
            if (vmode >= (byte)VisibilityMode.SmallObjectsLOD)
            {
                if (drawmode != OakDrawMode.NoDraw)
                {
                    spriter.enabled = false;
                    drawmode = OakDrawMode.NoDraw;
                }
            }
            else
            {
                if (drawmode != OakDrawMode.DrawSaplingSprite)
                {
                    drawmode = OakDrawMode.DrawSaplingSprite;
                    spriter.enabled = true;
                }
            }
        }
        else
        {
            OakDrawMode newDrawMode = OakDrawMode.NoDraw;
            //3d model
            if (vmode >= (byte)VisibilityMode.MediumObjectsLOD)
            {
                if (vmode >= (byte)VisibilityMode.HugeObjectsLOD)
                {
                    newDrawMode = OakDrawMode.NoDraw;
                }
                else
                {
                    newDrawMode = OakDrawMode.DrawLOD;
                }
            }
            else newDrawMode = OakDrawMode.DrawModel;
            if (newDrawMode != drawmode)
            {
                if (newDrawMode == OakDrawMode.NoDraw)
                {
                    spriter.enabled = false;
                    modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                }
                else
                {
                    if (newDrawMode == OakDrawMode.DrawModel)
                    {
                        spriter.enabled = false;
                        modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(true);
                    }
                    else
                    {
                        spriter.enabled = true;
                        modelHolder.transform.GetChild(MODEL_CHILDNUMBER).gameObject.SetActive(false);
                    }
                }
                drawmode = newDrawMode;
            }
            // # setting lod
            if (drawmode == OakDrawMode.DrawLOD)
            {
                byte spriteNumber = 0;
                float angle = Vector3.Angle(Vector3.up, FollowingCamera.camPos - transform.position);
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
                if (spriteNumber != lodNumber)
                {
                    switch (stage)
                    {
                        case 4: spriter.sprite = lodPack_stage4[spriteNumber]; break;
                        case 5: spriter.sprite = lodPack_stage5[spriteNumber]; break;
                        case 6: spriter.sprite = lodPack_stage6[spriteNumber]; break;
                    }
                    lodNumber = spriteNumber;
                }
            }
            // eo setting lod
        }
        visibilityMode = vm;
    }
    private void RefreshVisibility()
    {
        INLINE_SetVisibility(basement?.visibilityMode ?? VisibilityMode.DrawAll);
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
                    drawmode = OakDrawMode.DrawSaplingSprite;
                    RefreshVisibility();
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
        RefreshVisibility();
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
    public override float GetPlantComplexity()
    {
        return COMPLEXITY;
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

    override public void Annihilate(bool clearFromSurface, bool returnResources, bool leaveRuins)
    {
        if (destroyed | GameMaster.sceneClearing) return;
        else destroyed = true;
        bool basementNotNull = basement != null;
        if (subscribedToVisibilityEvent)
        {
            if (basementNotNull) { basement.visibilityChangedEvent -= this.SetVisibility; }
            oaksCount--;
        }
        if (!clearFromSurface)
        {
            basement = null;
            basementNotNull = false;
        }
        else
        {
            if (basementNotNull)
            {
                basement.RemoveStructure(this);
                //if (basement.grassland != null) basement.grassland.AddLifepower((int)(lifepower * GameMaster.realMaster.lifepowerLossesPercent));
            }
        }
        if (basementNotNull) basement = null;
        ReturnModelToPool();
        Destroy(gameObject);
        UpdatePool();
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

