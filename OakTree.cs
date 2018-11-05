using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OakTree : Plant
{
    float timer;
    private Transform modelTransform;

    private static List<OakTree> oaks;

    private static Sprite[] stageSprites;
    private static Mesh trunk_stage4, trunk_stage5, trunk_stage6, crones_stage4, crones_stage5, crones_stage6;
    static bool modelsContainerReady = false; // хранит заготовленные меши для деревьев
    private static GameObject modelsContainer;//
    private static List<GameObject> treeBlanks;
    private static LODController modelsLodController;
    private static short oak4spritesIndex = -1, oak5spritesIndex = -1, oak6spritesIndex = -1;
    private static float growSpeed, decaySpeed; // fixed by class
    public static int maxLifeTransfer { get; protected set; }  // fixed by class
    public const byte HARVESTABLE_STAGE = 4;
    const byte TRANSIT_STAGE = 3;
    //const int MAX_INACTIVE_BUFFERED = 25;
    // myRenderers : 0 -sprite, 1 - crone, 2 - trunk

    public const byte MAX_STAGE = 6;
    public const int CREATE_COST = 10, LUMBER = 100, FIRST_LIFEPOWER_TO_GROW = 10;

    static OakTree()
    {
        oaks = new List<OakTree>();
        maxLifeTransfer = 10;
        growSpeed = 0.1f;
        decaySpeed = growSpeed / 10f;
        FollowingCamera.main.cameraChangedEvent += CameraUpdate;
    }
    public static void ResetToDefaults_Static_OakTree()
    {
        if (modelsContainer == null) modelsContainerReady = false;
        oaks.Clear();
    }

    // тоже можно рассовать по методам
    override protected void SetModel()
    {
        // проверка на предыдущую модель не нужна
        if (!modelsContainerReady)
        {
            stageSprites = Resources.LoadAll<Sprite>("Textures/Plants/oakTree");
            modelsContainer = new GameObject("oakTreesContainer");

            treeBlanks = new List<GameObject>();
            modelsLodController = LODController.GetCurrent();
            GameObject trunkPref = Load3DModel(4);
            crones_stage4 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            trunk_stage4 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            ReturnModelToPool(trunkPref);

            trunkPref = Load3DModel(5);
            crones_stage5 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            trunk_stage5 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            ReturnModelToPool(trunkPref);

            trunkPref = Load3DModel(6);
            crones_stage6 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            trunk_stage6 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            ReturnModelToPool(trunkPref);

            modelsContainerReady = true;
        }
        GameObject model = null;
        if (stage > TRANSIT_STAGE)
        {
            if (treeBlanks.Count == 0) model = Load3DModel(stage);
            else {
                int last = treeBlanks.Count - 1;
                model = treeBlanks[last];
                treeBlanks.RemoveAt(last);
            }
            Transform modelTransform = model.transform;
            
            short packIndex = -1;
            switch (stage)
            {
                case 4:
                    modelTransform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage4;
                    modelTransform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage4;
                    packIndex = oak4spritesIndex;
                    break;
                case 5:
                    modelTransform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage5;
                    modelTransform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage5;
                    packIndex = oak5spritesIndex;
                    break;
                case 6:
                    modelTransform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage6;
                    modelTransform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage6;
                    packIndex = oak6spritesIndex;
                    break;
            }
            modelsLodController.ChangeModelSpritePack(modelTransform, ModelType.Tree, packIndex);
            model.transform.parent = transform;
            model.transform.localPosition = Vector3.zero;
        }
        else
        {
            modelTransform = new GameObject("sprite").transform;
            modelTransform.transform.parent = transform;
            modelTransform.localPosition = Vector3.zero;
            //Vector3 cpos = modelTransform.InverseTransformPoint(FollowingCamera.camPos); cpos.y = 0;
            // modelTransform.LookAt(cpos);
            SpriteRenderer sr = modelTransform.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = stageSprites[stage];
        }        
    }

    static GameObject Load3DModel(byte stage)
    {
        GameObject loadedModel = null;
        loadedModel = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-" + stage.ToString()));
        short modelSpritePack = 0;
        switch (stage)
        {
            case 4:
                if (oak4spritesIndex == -1)
                {
                    Vector3[] positions = new Vector3[] { new Vector3(0, 0.222f, -0.48f), new Vector3(0, 0.479f, -0.434f), new Vector3(0, 0.458f, -0.232f), new Vector3(0, 0.551f, -0.074f) };
                    Vector3[] angles = new Vector3[] { Vector3.zero, new Vector3(30, 0, 0), new Vector3(45, 0, 0), new Vector3(75, 0, 0) };
                    Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(loadedModel, positions, angles, 0.25f, Color.green);
                    Sprite[] lodSprites = new Sprite[4];
                    int size = spritesAtlas.width / 2;

                    lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[1] = Sprite.Create(spritesAtlas, new Rect(size, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[2] = Sprite.Create(spritesAtlas, new Rect(0, size, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[3] = Sprite.Create(spritesAtlas, new Rect(size, size, size, size), new Vector2(0.5f, 0.5f), 128);
                    oak4spritesIndex = LODController.AddSpritePack(lodSprites);
                }
                modelSpritePack = oak4spritesIndex;
                break;
            case 5:
                if (oak5spritesIndex == -1)
                {
                    Vector3[] positions = new Vector3[] { new Vector3(0, 0.222f, -0.48f), new Vector3(0, 0.479f, -0.434f), new Vector3(0, 0.458f, -0.232f), new Vector3(0, 0.551f, -0.074f) };
                    Vector3[] angles = new Vector3[] { Vector3.zero, new Vector3(30, 0, 0), new Vector3(45, 0, 0), new Vector3(75, 0, 0) };
                    Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(loadedModel, positions, angles, 0.25f, Color.green);
                    Sprite[] lodSprites = new Sprite[4];
                    int size = spritesAtlas.width / 2;

                    lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[1] = Sprite.Create(spritesAtlas, new Rect(size, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[2] = Sprite.Create(spritesAtlas, new Rect(0, size, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[3] = Sprite.Create(spritesAtlas, new Rect(size, size, size, size), new Vector2(0.5f, 0.5f), 128);
                    oak5spritesIndex = LODController.AddSpritePack(lodSprites);
                }
                modelSpritePack = oak5spritesIndex;
                break;
            case 6:
                if (oak6spritesIndex == -1)
                {
                    Vector3[] positions = new Vector3[] { new Vector3(0, 0.222f, -0.48f), new Vector3(0, 0.479f, -0.434f), new Vector3(0, 0.458f, -0.232f), new Vector3(0, 0.551f, -0.074f) };
                    Vector3[] angles = new Vector3[] { Vector3.zero, new Vector3(30, 0, 0), new Vector3(45, 0, 0), new Vector3(75, 0, 0) };
                    Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(loadedModel, positions, angles, 0.25f, Color.green);
                    Sprite[] lodSprites = new Sprite[4];
                    int size = spritesAtlas.width / 2;

                    lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[1] = Sprite.Create(spritesAtlas, new Rect(size, 0, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[2] = Sprite.Create(spritesAtlas, new Rect(0, size, size, size), new Vector2(0.5f, 0.5f), 128);
                    lodSprites[3] = Sprite.Create(spritesAtlas, new Rect(size, size, size, size), new Vector2(0.5f, 0.5f), 128);
                    oak6spritesIndex = LODController.AddSpritePack(lodSprites);
                }
                modelSpritePack = oak6spritesIndex;
                break;
        }
        modelsLodController.AddObject(loadedModel.transform.GetChild(2), ModelType.Tree, modelSpritePack);
        return loadedModel;
    }

    override public void ResetToDefaults()
    {
        lifepower = CREATE_COST;
        lifepowerToGrow = FIRST_LIFEPOWER_TO_GROW;
        stage = 0;
        growth = 0;
        hp = maxHp;
        SetStage(0);
    }

    override public void Prepare()
    {
        PrepareStructure();
        plant_ID = TREE_OAK_ID;
        innerPosition = SurfaceRect.one; isArtificial = false;
        lifepower = CREATE_COST;
        lifepowerToGrow = FIRST_LIFEPOWER_TO_GROW;
        growth = 0;
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        //#setStructureData
        basement = b;
        innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.size);
        if (transform.childCount == 0) SetModel();
        b.AddStructure(this);
            // isbasement check deleted
        //---
        if (!addedToClassList)
        {
            oaks.Add(this);
            addedToClassList = true;
            if (((existingPlantsMask >> TREE_OAK_ID) & 1) != 1)
            {
                int val = 1;
                val = val << TREE_OAK_ID;
                existingPlantsMask += val;
            }
        }
    }

    public static void UpdatePlants()
    {
        float t = GameMaster.LIFEPOWER_TICK;
        if (oaks.Count > 0)
        {
            int i = 0;
            float theoreticalGrowth;          
            while (i < oaks.Count)
            {
                OakTree oak = oaks[i];
                if (oak == null)
                {
                    oaks.RemoveAt(i);
                    continue;
                }
                else
                {
                    oak.timer -= t;
                    theoreticalGrowth = oak.lifepower / oak.lifepowerToGrow;
                    if (oak.growth < theoreticalGrowth)
                    {
                        oak.growth = Mathf.MoveTowards(oak.growth, theoreticalGrowth, growSpeed * t);
                    }
                    else
                    {
                        oak.lifepower -= decaySpeed * t;
                        if (oak.lifepower <= 0) oak.Dry();
                    }
                    if (oak.timer <= 0)
                    {
                        if (oak.growth >= 1 & oak.stage < MAX_STAGE)
                        {
                            byte nextStage = oak.stage;
                            nextStage++;
                            oak.SetStage(nextStage);
                        }
                        oak.timer = oak.stage;
                    }
                    i++;
                }
            }
        }
        else
        {
            if (((existingPlantsMask >> TREE_OAK_ID) & 1) != 0)
            {
                int val = 1;
                val = val << TREE_OAK_ID;
                existingPlantsMask -= val;
            }
        }
    }
    public static void CameraUpdate()
    {
        int count = oaks.Count;
        if (count > 0)
        {
            int i = 0;
            Vector3 camPos = FollowingCamera.camPos;
            Transform t = null;
            Vector3 cpos;
            while (i < count)
            {
                if (oaks[i] != null && oaks[i].stage <= TRANSIT_STAGE)
                {
                    t = oaks[i].transform.GetChild(0);
                    if (t != null)
                    {
                        cpos = Vector3.ProjectOnPlane(camPos - t.position, t.up);
                        t.forward = cpos.normalized;
                    }
                }
                i++;
            }
        }
    }

    override public int GetMaxLifeTransfer()
    {
        return maxLifeTransfer;
    }
    override public byte GetHarvestableStage()
    {
        return HARVESTABLE_STAGE;
    }

    #region lifepower operations
    public override void AddLifepowerAndCalculate(int life)
    {
        lifepower += life;
        byte nstage = 0;
        float lpg = FIRST_LIFEPOWER_TO_GROW;
        while (lifepower > lifepowerToGrow & nstage < MAX_STAGE)
        {
            nstage++;
            lpg = GetLifepowerLevelForStage(nstage);
        }
        lifepowerToGrow = lpg;
        SetStage(nstage);
    }
    public override int TakeLifepower(int life)
    {
        int lifeTransfer = life;
        if (life > lifepower) { if (lifepower >= 0) lifeTransfer = (int)lifepower; else lifeTransfer = 0; }
        lifepower -= lifeTransfer;
        return lifeTransfer;
    }
    override public void SetLifepower(float p)
    {
        lifepower = p;
    }
    override public void SetGrowth(float t)
    {
        growth = t;
    }
    override public void SetStage(byte newStage)
    {
        if (newStage == stage) return;
        if (transform.childCount != 0) {
            if (stage > TRANSIT_STAGE) ReturnModelToPool();
            else Destroy(transform.GetChild(0).gameObject);
        }
        stage = newStage;              
        SetModel();
        transform.GetChild(0).gameObject.SetActive(visible);        
        lifepowerToGrow = GetLifepowerLevelForStage(stage);
        growth = lifepower / lifepowerToGrow;
    }

    public static float GetLifepowerLevelForStage(byte st)
    {
        switch (st)
        {
            default:
            case 0: return FIRST_LIFEPOWER_TO_GROW; // 10
            case 1: return FIRST_LIFEPOWER_TO_GROW * 2; // 20
            case 2: return FIRST_LIFEPOWER_TO_GROW * 4;// 40;
            case 3: return FIRST_LIFEPOWER_TO_GROW * 8;// 80 - full 3d tree
            case 4: return FIRST_LIFEPOWER_TO_GROW * 16;// 160
            case 5: return FIRST_LIFEPOWER_TO_GROW * 32;// 320 - max 
        }
    }
    #endregion

    override public void Harvest()
    {
        if (destroyed) return;
        GameMaster.colonyController.storage.AddResource(ResourceType.Lumber, CountLumber());
        if (stage > TRANSIT_STAGE & transform.childCount != 0)
        {
            Transform model = transform.GetChild(0);
            model.transform.parent = null;
            FallingTree ft = model.gameObject.AddComponent<FallingTree>();
            ft.returnFunction = ReturnModelToPool;
        }
        Annihilate(false);
    }

    override public void Dry()
    {
        if (stage > 3)
        {
            GameObject pref = Load3DModel(stage);
            GameObject model3d = Instantiate(pref);
            ReturnModelToPool(pref);
            Transform t = model3d.transform.GetChild(1);            
            HarvestableResource hr = new GameObject("dead oak tree").AddComponent<HarvestableResource>();
            t.parent = hr.transform;
            t.gameObject.SetActive(true);
            Destroy(model3d);
            t.localPosition = Vector3.zero;
            
            MeshFilter mf = t.GetComponent<MeshFilter>();
            t.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.DeadLumber, mf, basement.illumination);
            
            hr.PrepareContainer(hp, new ResourceContainer(ResourceType.Lumber, CountLumber()), false, innerPosition.size, model3d);
            hr.SetModelRotation( modelRotation );
            hr.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
            // спрайтовый LOD?
        }
        else
        {
            Structure s = GetStructureByID(DRYED_PLANT_ID);
            s.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
            s.gameObject.AddComponent<MastSpriter>();
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
        if (x == visible) return;
        else
        {
            visible = x;
            if (transform.childCount != 0) transform.GetChild(0).gameObject.SetActive(visible);
        }
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        if (forced) { UnsetBasement(); }
        else
        {
            if (basement != null)
            {
                basement.RemoveStructure(this);
                basement.grassland.AddLifepower((int)(lifepower * GameMaster.lifepowerLossesPercent));
            }
        }
        if (addedToClassList)
        {
            int index = oaks.IndexOf(this);
            if (index > 0) oaks[index] = null;
            addedToClassList = false;
        }
        basement = null;
        if (transform.childCount != 0)
        {
            if (stage > TRANSIT_STAGE & modelTransform == null) ReturnModelToPool();
        }
        Destroy(gameObject);
    }

   private void ReturnModelToPool()
    {
        ReturnModelToPool( transform.GetChild(0).gameObject ); 
    }
    static void ReturnModelToPool(GameObject model)
    {
        // проверка на соответствие модели?
        model.transform.parent = modelsContainer.transform;
        treeBlanks.Add(model);
        model.SetActive(false);
        foreach (GameObject g in treeBlanks)
        {
            if (g == null) print("empty blank");
        }
    }
}

