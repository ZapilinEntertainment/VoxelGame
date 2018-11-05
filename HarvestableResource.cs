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

    public void PrepareContainer(float i_maxhp, ResourceContainer rc, bool i_isArtificial, byte size, GameObject i_model )
    {
        PrepareStructure();
        maxHp = i_maxhp;
        innerPosition = new SurfaceRect(0,0,size);
        isArtificial = i_isArtificial;
        i_model.transform.parent = transform;
        i_model.transform.localPosition = Vector3.zero;
        // вращение не устанавливать 
        resourceCount = rc.volume;
        mainResource = rc.type;
    }

    protected override void SetModel()
    {
        if (transform.childCount != 0) Destroy(transform.GetChild(0).gameObject);
        int material_ID = mainResource.ID;
        bool modelIsSprite = false;
        GameObject model;
        switch (material_ID)
        {
            case ResourceType.STONE_ID:
            case ResourceType.METAL_K_ORE_ID:
            case ResourceType.METAL_S_ORE_ID:
            case ResourceType.METAL_P_ORE_ID:
            case ResourceType.METAL_N_ORE_ID:
            case ResourceType.METAL_E_ORE_ID:
            case ResourceType.METAL_M_ORE_ID:
                model = Instantiate(Resources.Load<GameObject>("Prefs/boulderPref"));
                break;
            case ResourceType.MINERAL_F_ID:
            case ResourceType.MINERAL_L_ID:
                model = Instantiate(Resources.Load<GameObject>("Prefs/pilePref"));
                break;
            case ResourceType.FOOD_ID:
                model = Instantiate(Resources.Load<GameObject>("Prefs/berryBush"));
                modelIsSprite = true;
                break;
            default:
                model = Instantiate(Resources.Load<GameObject>("Prefs/defaultContainer"));
                break;
        }
        model.transform.parent = transform;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(Vector3.zero);
        if (!modelIsSprite)
        {
            Transform meshTransform = model.transform.GetChild(0);
            meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(material_ID, meshTransform.GetComponent<MeshFilter>(), 255);

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
            //replaced by shader
            //FollowingCamera.main.AddSprite(model.transform);
            //haveSprite = true;
        }
    }

    public void SetResources(ResourceType resType, float f_count1)
    {
        ResourceType prevResType = mainResource;
        mainResource = resType;
        resourceCount = f_count1;
        if (prevResType != resType ) // замена модели
        {  
            SetModel();
            transform.GetChild(0).gameObject.SetActive(visible);
        }        
    }

    public void Harvest()
    {
        resourceCount = GameMaster.colonyController.storage.AddResource(mainResource, resourceCount);
        if (resourceCount <= 0) Annihilate(false); // может быть переполнение и не все соберется
    }

    override public void Annihilate(bool forced)
    {
        if (destroyed) return;
        else destroyed = true;
        PrepareStructureForDestruction(forced);
        basement = null;
        Destroy(gameObject);
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
        HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
        GameMaster.DeserializeByteArray<HarvestableResourceSerializer>(ss.specificData, ref hrs);
        SetResources(ResourceType.GetResourceTypeById(hrs.mainResource_id), hrs.count);
        LoadStructureData(ss, sblock);
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
