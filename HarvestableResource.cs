using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HarvestableResourceSerializer
{
    public int mainResource_id;
    public float count;
}

public class HarvestableResource : Structure
{
    public ResourceType mainResource { get; protected set; }
    public float resourceCount;
    static Dictionary<ResourceType, short> materialBasedLods = new Dictionary<ResourceType, short>();
    static LODController modelController;

    override public void Prepare()
    { 
        PrepareStructure();
        maxHp = 10;
        innerPosition = SurfaceRect.one;
        isArtificial = false;

        mainResource = ResourceType.Nothing;
        hp = maxHp;
        resourceCount = 0;
        if (modelController == null) modelController = LODController.GetCurrent();
    }

    public void PrepareContainer(float i_maxhp, ResourceContainer rc, bool i_isArtificial, PixelPosByte size, GameObject i_model )
    {
        PrepareStructure();
        maxHp = i_maxhp;
        innerPosition = new SurfaceRect(0,0,size.x, size.y);
        isArtificial = i_isArtificial;
        model = i_model;
        resourceCount = rc.volume;
        mainResource = rc.type;
    }
    // выдаст модели для природных объектов

    protected override void SetModel()
    {
        if (model != null) Object.Destroy(model);
        int material_ID = mainResource.ID;
        bool modelIsSprite = false;
        switch (material_ID)
        {
            case ResourceType.STONE_ID:
            case ResourceType.METAL_K_ORE_ID:
            case ResourceType.METAL_S_ORE_ID:
            case ResourceType.METAL_P_ORE_ID:
            case ResourceType.METAL_N_ORE_ID:
            case ResourceType.METAL_E_ORE_ID:
            case ResourceType.METAL_M_ORE_ID:
                model = Object.Instantiate(Resources.Load<GameObject>("Prefs/boulderPref"));
                break;
            case ResourceType.MINERAL_F_ID:
            case ResourceType.MINERAL_L_ID:
                model = Object.Instantiate(Resources.Load<GameObject>("Prefs/pilePref"));
                break;
            case ResourceType.FOOD_ID:
                model = Object.Instantiate(Resources.Load<GameObject>("Prefs/berryBush"));
                modelIsSprite = true;
                break;
            default:
                model = Object.Instantiate(Resources.Load<GameObject>("Prefs/defaultContainer"));
                break;
        }
        if (!modelIsSprite)
        {
            Transform t = model.transform.GetChild(0);
            t.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(material_ID, t.GetComponent<MeshFilter>());

            short packIndex = -1;
            if (!materialBasedLods.TryGetValue(mainResource, out packIndex))
            {
                Vector3[] positions = new Vector3[] { new Vector3(0, 0.084f, -0.063f) };
                Vector3[] angles = new Vector3[] { new Vector3(45, 0, 0) };
                Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(model, positions, angles, 0.06f, Color.grey);
                Sprite[] lodSprites = new Sprite[1];

                lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, spritesAtlas.width, spritesAtlas.height), new Vector2(0.5f, 0.5f), 512);
                packIndex = LODController.AddSpritePack(lodSprites);
                materialBasedLods.Add(mainResource, packIndex);
            }
            modelController.AddObject(model.transform, ModelType.Boulder, packIndex);
        }
        else
        {
            GameMaster.realMaster.standartSpritesList.Add(model);
        }
    }

    public void SetResources(ResourceType resType, float f_count1)
    {
        ResourceType prevResType = mainResource;
        if (prevResType != ResourceType.Nothing & model != null) // замена модели
        {  
            SetModel();
            model.SetActive(visible);
        }
        mainResource = resType;
        if (prevResType != mainResource & model != null) Object.Destroy(model); 
        resourceCount = f_count1;
    }

    public void Harvest()
    {
        resourceCount = GameMaster.colonyController.storage.AddResource(mainResource, resourceCount);
        if (resourceCount <= 0) Annihilate(false); // может быть переполнение и не все соберется
    }

    #region save-load system
    override public StructureSerializer Save()
    {
        StructureSerializer ss = GetStructureSerializer();
        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
        {
            new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHarvestableResourceSerializer());
            ss.specificData = stream.ToArray();
        }
        return ss;
    }

    override public void Load(StructureSerializer ss, SurfaceBlock sblock)
    {
        LoadStructureData(ss, sblock);
        HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
        GameMaster.DeserializeByteArray<HarvestableResourceSerializer>(ss.specificData, ref hrs);
        SetResources(ResourceType.GetResourceTypeById(hrs.mainResource_id), hrs.count);
    }


    protected HarvestableResourceSerializer GetHarvestableResourceSerializer()
    {
        HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
        hrs.mainResource_id = mainResource.ID;
        hrs.count = resourceCount;
        return hrs;
    }
    #endregion
}
