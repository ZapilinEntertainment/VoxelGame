using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OakTree : Plant
{
    float timer;
    static bool modelsContainerReady = false;
    GameObject model3d;

    private static OakTree[] oaks;
    private static List<OakTree> addToOaksList;
    private static List<int> removeFromOaksList;

    private static Sprite[] stageSprites;
    private static Mesh trunk_stage4, trunk_stage5, trunk_stage6, crones_stage4, crones_stage5, crones_stage6;
    private static GameObject modelsContainer;
    private static List<GameObject> treeBlanks;
    private static LODController modelsLodController;
    private static short oak4spritesIndex = -1, oak5spritesIndex = -1, oak6spritesIndex = -1;
    private static float growSpeed, decaySpeed; // fixed by class
    public static int maxLifeTransfer { get; protected set; }  // fixed by class
    public const byte HARVESTABLE_STAGE = 4;
    //const int MAX_INACTIVE_BUFFERED = 25;
    // myRenderers : 0 -sprite, 1 - crone, 2 - trunk

    public const byte MAX_STAGE = 6;
    public const int CREATE_COST = 10, LUMBER = 100, FIRST_LIFEPOWER_TO_GROW = 10;

    static OakTree()
    {
        oaks = new OakTree[0];
        addToOaksList = new List<OakTree>();
        removeFromOaksList = new List<int>();
        maxLifeTransfer = 10;
        growSpeed = 0.1f;
        decaySpeed = growSpeed;
    }
    public static void ResetToDefaults_Static_OakTree()
    {
        if (modelsContainer == null) modelsContainerReady = false;
    }

    override protected void SetModel()
    {
        if (!modelsContainerReady)
        {
            stageSprites = Resources.LoadAll<Sprite>("Textures/Plants/oakTree");
            modelsContainer = new GameObject("oakTreesContainer");

            treeBlanks = new List<GameObject>();
            modelsLodController = LODController.GetCurrent();
            GameObject trunkPref = LoadNewModel(4);
            crones_stage4 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            trunk_stage4 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            trunkPref.SetActive(false);

            trunkPref = LoadNewModel(5);
            crones_stage5 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            trunk_stage5 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            trunkPref.SetActive(false);

            trunkPref = LoadNewModel(6);
            crones_stage6 = trunkPref.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            trunk_stage6 = trunkPref.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            trunkPref.SetActive(false);

            modelsContainerReady = true;
        }
        if (stage > 3)
        {
            int i = 0;
            while (i < treeBlanks.Count)
            {
                model3d = treeBlanks[i];
                if (model3d == null)
                {
                    treeBlanks.RemoveAt(i);
                    continue;
                }
                else break;
            }
            if (model3d == null) model3d = LoadNewModel(stage);
            model3d.transform.parent = modelsContainer.transform;
            short packIndex = -1;
            switch (stage)
            {
                case 4:
                    model3d.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage4;
                    model3d.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage4;
                    packIndex = oak4spritesIndex;
                    break;
                case 5:
                    model3d.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage5;
                    model3d.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage5;
                    packIndex = oak5spritesIndex;
                    break;
                case 6:
                    model3d.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = crones_stage6;
                    model3d.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = trunk_stage6;
                    packIndex = oak6spritesIndex;
                    break;
            }
            modelsLodController.ChangeModelSpritePack(model3d.transform, ModelType.Tree, packIndex);            
            if (model != null) Object.Destroy(model);
        }
        else
        {
            model = new GameObject("Plant - OakTree");
            GameObject spriteSatellite = new GameObject("sprite");
            spriteSatellite.transform.parent = model.transform;
            GameMaster.realMaster.mastSpritesList.Add(spriteSatellite);
            spriteSatellite.AddComponent<SpriteRenderer>().sprite = stageSprites[stage];
            if (model3d != null) ReturnModel3DToPool();
        }

    }

    static GameObject LoadNewModel(byte stage)
    {
        GameObject loadedModel = null;
        loadedModel = Object.Instantiate(Resources.Load<GameObject>("Lifeforms/oak-" + stage.ToString()));
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
        loadedModel.transform.parent = modelsContainer.transform;
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
        SetStructureData(b, pos);
        if (stage > 3)
        {
            if (model3d == null) SetModel();
            model3d.transform.position = model.transform.position;
            model3d.transform.rotation = model.transform.rotation;
        }
        else
        {
            if (model == null) SetModel();
        }
        if (!addedToClassList)
        {
            addToOaksList.Add(this);
            addedToClassList = true;
            if (((existingPlantsMask >> TREE_OAK_ID) & 1) != 1)
            {
                int val = 1;
                val = val << TREE_OF_LIFE_ID;
                existingPlantsMask += val;
            }
        }
    }

    public static void UpdatePlants()
    {
        float t = GameMaster.LIFEPOWER_TICK;
        int removeCount = removeFromOaksList.Count, addCount = addToOaksList.Count;
        if (removeCount > 0 | addCount > 0)
        {
            if (removeCount > 0)
            {
                foreach (int index in removeFromOaksList) oaks[index] = null;
                removeFromOaksList.Clear();
            }
            OakTree[] newOaksArray = new OakTree[oaks.Length - removeCount + addCount];
            if (newOaksArray.Length != 0)
            {
                int i = 0;
                if (addCount > 0)
                {
                    for (; i < addCount; i++) newOaksArray[i] = addToOaksList[i];
                    addToOaksList.Clear();
                }
                if (oaks.Length > 0)
                {
                    for (int j = 0; j < oaks.Length; j++)
                    {
                        if (oaks[j] != null) newOaksArray[j + i] = oaks[j];
                    }
                }
            }
            oaks = newOaksArray;
        }
        if (oaks.Length > 0)
        {
            float theoreticalGrowth;
            foreach (OakTree oak in oaks)
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
                    if (oak.lifepower == 0) oak.Dry();
                }
                if (oak.timer <= 0)
                {
                    if (oak.growth >= 1 & oak.stage < MAX_STAGE)
                    {
                        byte nextStage = oak.stage;
                        nextStage++;
                        if (oak.CanGrowAround(nextStage)) oak.SetStage(nextStage);
                    }
                    oak.timer = oak.stage;
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
        stage = newStage;
        if (stage < 4)
        {
            if (model != null) model.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = stageSprites[stage];
            else SetModel();
            if (model3d != null) ReturnModel3DToPool();
        }
        else
        {
            if (model3d == null)
            {
                SetModel();
                model3d.transform.position = model.transform.position;
            }
        }
        if (visible)
        {
            if (model != null) model.SetActive(true);
            if (model3d != null) model.SetActive(true);
        }
        lifepowerToGrow = GetLifepowerLevelForStage(stage);
        growth = lifepower / lifepowerToGrow;
    }

    bool CanGrowAround(byte st)
    {
        //central cell : (rewrite if size % 2 == 0)
        SurfaceRect sr = new SurfaceRect((byte)(innerPosition.x + innerPosition.x_size / 2), (byte)(innerPosition.z + innerPosition.z_size / 2), innerPosition.x_size, innerPosition.z_size);
        switch (st)
        {
            case 0:
            case 1:
            case 2:
                sr = new SurfaceRect(sr.x, sr.z, 1, 1);
                break;
            case 3:
            case 4:
                if (sr.x - 1 < 0 | sr.z - 1 < 0) return false;
                sr = new SurfaceRect((byte)(sr.x - 1), (byte)(sr.z - 1), 3, 3);
                break;
            case 5:
                if (sr.x - 2 < 0 | sr.z - 2 < 0) return false;
                sr = new SurfaceRect((byte)(sr.x - 2), (byte)(sr.z - 2), 5, 5);
                break;
            case 6:
                if (sr.x - 3 < 0 | sr.z - 3 < 0) return false;
                sr = new SurfaceRect((byte)(sr.x - 3), (byte)(sr.z - 3), 7, 7);
                break;
        }
        if (sr == innerPosition) return true;
        if (sr.x + sr.x_size > SurfaceBlock.INNER_RESOLUTION | sr.z_size + sr.z > SurfaceBlock.INNER_RESOLUTION) return false;
        else
        {
            innerPosition = sr;
            return true;
        }
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
        GameMaster.colonyController.storage.AddResource(ResourceType.Lumber, CountLumber());
        if (model3d != null)
        {
            FallingTree ft = model3d.AddComponent<FallingTree>();
            ft.containList = treeBlanks;
            model3d = null;
        }
        Annihilate(false);
    }

    override public void Dry()
    {
        if (stage > 3)
        {
            GameObject pref = LoadNewModel(stage);
            GameObject deadTreeModel = Object.Instantiate(pref);
            ReturnModel3DToPool(pref);
            Object.Destroy(deadTreeModel.transform.GetChild(1).gameObject);
            deadTreeModel.transform.parent = deadTreeModel.transform;
            Transform t = deadTreeModel.transform.GetChild(0);
            MeshFilter mf = t.GetComponent<MeshFilter>();
            t.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetBasicMaterial(BasicMaterial.DeadLumber, mf);

            HarvestableResource hr = new HarvestableResource();
            hr.PrepareContainer(hp, new ResourceContainer(ResourceType.Lumber, CountLumber()), false, new PixelPosByte(innerPosition.x_size, innerPosition.z_size), deadTreeModel);
            hr.modelRotation = modelRotation;
            hr.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
        }
        else
        {
            Structure s = GetStructureByID(DRYED_PLANT_ID);
            s.SetBasement(basement, new PixelPosByte(innerPosition.x, innerPosition.z));
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
            if (model != null) model.SetActive(visible);
            if (model3d != null) model.SetActive(visible);
        }
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        //print("oak annihilation");
        if (model3d != null) ReturnModel3DToPool();
        if (forced) { UnsetBasement(); }
        PreparePlantForDestruction(forced);
        if (addedToClassList)
        {
            if (oaks.Length > 0)
            {
                int i = -1;
                foreach (OakTree oak in oaks)
                {
                    if (oak.personalNumber == personalNumber)
                    {
                        removeFromOaksList.Add(i);
                    }
                }
            }
            addedToClassList = false;
        }
    }

    void ReturnModel3DToPool()
    {
        treeBlanks.Add(model3d);
        model3d.SetActive(false);
        model3d = null;
    }
    void ReturnModel3DToPool(GameObject model)
    {
        treeBlanks.Add(model);
        model.SetActive(false);
    }
}

