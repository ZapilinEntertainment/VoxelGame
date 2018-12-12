using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HarvestableResourceSerializer
{
    public int mainResource_id;
    public float count;
    public ContainerModelType model_id;
}
public enum ContainerModelType : ushort { Default, Boulder, Pile, DeadOak4, DeadOak5, DeadOak6, BerryBush, DeadTreeOfLife, DeadLifestone }

public class HarvestableResource : Structure
{
    public ResourceType mainResource { get; protected set; }
    public float resourceCount;
    private ContainerModelType model_id;

    public static HarvestableResource ConstructContainer(ContainerModelType i_modelType, ResourceType i_rtype, float i_count)
    {
        HarvestableResource hr = new GameObject().AddComponent<HarvestableResource>();
        hr.PrepareStructure();
        hr.mainResource = i_rtype;
        hr.resourceCount = i_count;               
        GameObject model;
        bool createSpriteLOD = true;
        LODPackType lpackType = LODPackType.Point;
        hr.model_id = i_modelType;
        switch (hr.model_id)
        {
            case ContainerModelType.DeadLifestone:
                {
                    hr.gameObject.name = "dead Lifestone";
                    model = Instantiate(Resources.Load<GameObject>("Structures/LifeStone"));   
                    Destroy(model.transform.GetChild(0).gameObject);
                    Destroy(model.transform.GetChild(1).gameObject);
                    MeshRenderer[] mrrs = model.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrrs)
                    {
                        mr.sharedMaterial = PoolMaster.basic_material;
                    }
                    createSpriteLOD = false;
                    byte c = (byte)(SurfaceBlock.INNER_RESOLUTION / 4);
                    hr.innerPosition = new SurfaceRect(c, c, (byte)(c + c));
                    hr.maxHp = LifeSource.MAX_HP * 0.9f;
                    break;
                }
            case ContainerModelType.DeadTreeOfLife:
                {
                    hr.gameObject.name = "dead Tree of Life";
                    model = Instantiate(Resources.Load<GameObject>("Lifeforms/dead_treeOfLife"));
                    createSpriteLOD = false;
                    byte c = (byte)(SurfaceBlock.INNER_RESOLUTION / 4);
                    hr.innerPosition = new SurfaceRect(c, c, (byte)(c+c));
                    hr.maxHp = LifeSource.MAX_HP * 0.9f ;
                    break;
                }
            case ContainerModelType.DeadOak4:
                hr.gameObject.name = "dead oak 4";
                model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-4_dead"));
                lpackType = LODPackType.OneSide;
                hr.innerPosition = SurfaceRect.one;
                hr.maxHp = 50;
                break;
            case ContainerModelType.DeadOak5:
                hr.gameObject.name = "dead oak 5";
                model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-6_dead"));
                lpackType = LODPackType.OneSide;
                hr.innerPosition = SurfaceRect.one;
                hr.maxHp = 100;
                break;
            case ContainerModelType.DeadOak6:
                hr.gameObject.name = "dead oak 6";
                model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-6_dead"));
                lpackType = LODPackType.OneSide;
                hr.innerPosition = SurfaceRect.one;
                hr.maxHp = 200;
                break;
            case ContainerModelType.Pile:
                {
                    hr.gameObject.name = "pile";
                    model = Instantiate(Resources.Load<GameObject>("Prefs/pilePref"));
                    lpackType = LODPackType.Point;
                    Transform meshTransform = model.transform.GetChild(0);
                    meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(i_rtype.ID, meshTransform.GetComponent<MeshFilter>(), 255);
                    hr.maxHp = 30;
                    hr.innerPosition = SurfaceRect.one;
                    break;
                }
            case ContainerModelType.BerryBush:
                hr.gameObject.name = "berry bush";
                model = Instantiate(Resources.Load<GameObject>("Prefs/berryBush"));
                createSpriteLOD = false;
                hr.maxHp = 10;
                hr.innerPosition = SurfaceRect.one;
                break;
            case ContainerModelType.Boulder:
                {
                    hr.gameObject.name = "boulder";
                    model = Instantiate(Resources.Load<GameObject>("Prefs/boulderPref"));
                    Transform meshTransform = model.transform.GetChild(0);
                    meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(i_rtype.ID, meshTransform.GetComponent<MeshFilter>(), 255);
                    lpackType = LODPackType.Point;
                    hr.maxHp = 50;
                    hr.innerPosition = SurfaceRect.one;
                    break;
                }
            default:
                {
                    hr.gameObject.name = "default container";
                    model = Instantiate(Resources.Load<GameObject>("Prefs/defaultContainer"));
                    Transform meshTransform = model.transform.GetChild(0);
                    meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(i_rtype.ID, meshTransform.GetComponent<MeshFilter>(), 255);
                    hr.model_id = ContainerModelType.Default;
                    createSpriteLOD = false;
                    hr.maxHp = 10;
                    hr.innerPosition = SurfaceRect.one;
                    break;
                }
        }
        hr.hp = hr.maxHp;
        if (createSpriteLOD)
        {
            SpriteRenderer sr = new GameObject("lod").AddComponent<SpriteRenderer>();
            sr.transform.parent = model.transform;
            sr.transform.localPosition = Vector3.zero;
            sr.sharedMaterial = PoolMaster.billboardMaterial;
        }
        model.transform.parent = hr.transform;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(Vector3.zero);
        return hr;
    }

    override public void Prepare()
    {
        PrepareStructure();
        maxHp = 10;
        innerPosition = SurfaceRect.one;
        isArtificial = false;

        mainResource = ResourceType.Nothing;
        hp = maxHp;
        resourceCount = 0;
        model_id = ContainerModelType.Default;
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        //#setStructureData
        basement = b;
        innerPosition = new SurfaceRect(pos.x, pos.y, innerPosition.size);
        if (transform.childCount == 0) SetModel();
        b.AddStructure(this);
    }

    protected override void SetModel()
    {
        Transform model = Instantiate(Resources.Load<GameObject>("Prefs/defaultContainer")).transform;
        Transform meshTransform = model.transform.GetChild(0);
        meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(mainResource.ID, meshTransform.GetComponent<MeshFilter>(), 255);
        model_id = ContainerModelType.Default;
        model.parent = transform;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public void Harvest()
    {
        resourceCount = GameMaster.realMaster.colonyController.storage.AddResource(mainResource, resourceCount);
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
        mainResource = ResourceType.GetResourceTypeById(hrs.mainResource_id);
        resourceCount = hrs.count;
        SetModel();
        modelRotation = ss.modelRotation;
        indestructible = ss.indestructible;
        SetBasement(sblock, ss.pos);
        maxHp = ss.maxHp; hp = ss.maxHp;
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
