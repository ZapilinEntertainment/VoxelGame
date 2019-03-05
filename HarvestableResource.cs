using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContainerModelType : ushort { Default, Boulder, Pile, DeadOak4, DeadOak5, DeadOak6, BerryBush, DeadTreeOfLife, DeadLifestone }
// при изменении размерности - изменить сериализатор

public class HarvestableResource : Structure
{
    public const int CONTAINER_SERIALIZER_LENGTH = 10;

    public ResourceType mainResource { get; protected set; }
    public float resourceCount;
    private ContainerModelType model_id;

    public static HarvestableResource ConstructContainer(ContainerModelType i_modelType, ResourceType i_rtype, float i_count)
    {
        HarvestableResource hr = new GameObject().AddComponent<HarvestableResource>();
        hr.id = CONTAINER_ID;
        hr.PrepareStructure();
        hr.mainResource = i_rtype;
        hr.resourceCount = i_count;               
        GameObject model;

        bool createSpriteLOD = false;        
        LODRegisterInfo regInfo = new LODRegisterInfo(0, 0, 0);
        float height = 0;

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
                    byte c = (byte)(SurfaceBlock.INNER_RESOLUTION / 4);
                    hr.innerPosition = new SurfaceRect(c, c, (byte)(c + c));
                    hr.maxHp = LifeSource.MAX_HP * 0.9f;
                    break;
                }
            case ContainerModelType.DeadTreeOfLife:
                {
                    hr.gameObject.name = "dead Tree of Life";
                    model = Instantiate(Resources.Load<GameObject>("Lifeforms/deadTreeOfLife"));
                    byte c = (byte)(SurfaceBlock.INNER_RESOLUTION / 4);
                    hr.innerPosition = new SurfaceRect(c, c, (byte)(c+c));
                    hr.maxHp = LifeSource.MAX_HP * 0.9f ;
                    break;
                }
            case ContainerModelType.DeadOak4:
                hr.gameObject.name = "dead oak 4";
                model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-4_dead"));
                hr.innerPosition = SurfaceRect.one;
                hr.maxHp = 50;

                createSpriteLOD = true;
                regInfo = new LODRegisterInfo(LODController.CONTAINER_MODEL_ID, (int)ContainerModelType.DeadOak4, 0);
                height = 0.211f;
                break;
            case ContainerModelType.DeadOak5:
                hr.gameObject.name = "dead oak 5";
                model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-6_dead"));
                hr.innerPosition = SurfaceRect.one;
                hr.maxHp = 100;

                createSpriteLOD = true;
                regInfo = new LODRegisterInfo(LODController.CONTAINER_MODEL_ID, (int)ContainerModelType.DeadOak5, 0);
                height = 0.211f;
                break;
            case ContainerModelType.DeadOak6:
                hr.gameObject.name = "dead oak 6";
                model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-6_dead"));
                hr.innerPosition = SurfaceRect.one;
                hr.maxHp = 200;

                createSpriteLOD = true;
                regInfo = new LODRegisterInfo(LODController.CONTAINER_MODEL_ID, (int)ContainerModelType.DeadOak6, 0);
                height = 0.211f;
                break;
            case ContainerModelType.Pile:
                {
                    hr.gameObject.name = "pile";
                    model = Instantiate(Resources.Load<GameObject>("Prefs/pilePref"));

                    createSpriteLOD = true;
                    regInfo = new LODRegisterInfo((int)ContainerModelType.Pile, 0, hr.mainResource.ID);
                    height = 0.047f;

                    Transform meshTransform = model.transform.GetChild(0);
                    meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(i_rtype.ID, meshTransform.GetComponent<MeshFilter>(), 255);
                    hr.maxHp = 30;
                    hr.innerPosition = SurfaceRect.one;
                    break;
                }
            case ContainerModelType.BerryBush:
                {
                    hr.gameObject.name = "berry bush";
                    model = Instantiate(Resources.Load<GameObject>("Prefs/berryBush"));
                    hr.maxHp = 10;
                    hr.innerPosition = SurfaceRect.one;
                    break;
                }
            case ContainerModelType.Boulder:
                {
                    hr.gameObject.name = "boulder";
                    model = Instantiate(Resources.Load<GameObject>("Prefs/boulderPref"));
                    Transform meshTransform = model.transform.GetChild(0);
                    meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(i_rtype.ID, meshTransform.GetComponent<MeshFilter>(), 255);
                   
                    regInfo = new LODRegisterInfo((int)ContainerModelType.Boulder, 0, hr.mainResource.ID);
                    createSpriteLOD = true;
                    height = 0.047f;

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
            sr.transform.localPosition = Vector3.up * height;
            sr.sharedMaterial = PoolMaster.billboardMaterial;
            if (PoolMaster.shadowCasting) sr.receiveShadows = true;
            LODController currentLC = LODController.GetCurrent();
            LODPackType lpackType = LODPackType.Point;
            int indexInRegistered = currentLC.LOD_existanceCheck(regInfo);
            float lodDistance = 6, visibilityDistance = 15;
      
            if (indexInRegistered == -1)
            {
                int resolution = 8;
                float size = 0.05f;
                Color backgroundColor = Color.gray;                
                RenderPoint[] renderpoints = new RenderPoint[] { };

                switch (hr.model_id)
                {
                    case ContainerModelType.Pile:
                        {                            
                            renderpoints = new RenderPoint[] { new RenderPoint(new Vector3(0, 0.084f, -0.063f), new Vector3(45, 0, 0)) };
                            break;
                        }
                    case ContainerModelType.Boulder:
                        {
                            renderpoints = new RenderPoint[] { new RenderPoint(new Vector3(0, 0.084f, -0.063f), new Vector3(45, 0, 0)) };
                            break;
                        }
                    case ContainerModelType.DeadOak4:
                        {
                            renderpoints = new RenderPoint[] {
                            new RenderPoint(new Vector3(0, 0.222f, -0.48f), Vector3.zero),
                             new RenderPoint(new Vector3(0, 0.479f, -0.434f), new Vector3(30, 0, 0)),
                              new RenderPoint(new Vector3(0, 0.458f, -0.232f), new Vector3(45, 0, 0)),
                               new RenderPoint(new Vector3(0, 0.551f, -0.074f), new Vector3(75, 0, 0))
                            };
                            size = 0.2f;
                            resolution = 32;
                            lpackType = LODPackType.OneSide;
                            lodDistance = 15;
                            visibilityDistance = 24;
                            break;
                        }
                    case ContainerModelType.DeadOak5:
                        {
                            renderpoints = new RenderPoint[] {
                            new RenderPoint(new Vector3(0, 0.222f, -0.48f), Vector3.zero),
                             new RenderPoint(new Vector3(0, 0.479f, -0.434f), new Vector3(30, 0, 0)),
                              new RenderPoint(new Vector3(0, 0.458f, -0.232f), new Vector3(45, 0, 0)),
                               new RenderPoint(new Vector3(0, 0.551f, -0.074f), new Vector3(75, 0, 0))
                            };
                            size = 0.25f;
                            resolution = 32;
                            lpackType = LODPackType.OneSide;
                            lodDistance = 18;
                            visibilityDistance = 28;
                            break;
                        }
                    case ContainerModelType.DeadOak6:
                        {
                            renderpoints = new RenderPoint[] {
                            new RenderPoint(new Vector3(0, 0.222f, -0.48f), Vector3.zero),
                             new RenderPoint(new Vector3(0, 0.479f, -0.434f), new Vector3(30, 0, 0)),
                              new RenderPoint(new Vector3(0, 0.458f, -0.232f), new Vector3(45, 0, 0)),
                               new RenderPoint(new Vector3(0, 0.551f, -0.074f), new Vector3(75, 0, 0))
                            };
                            size = 0.4f;
                            lodDistance = 21;
                            visibilityDistance = 32;
                            resolution = 64;
                            lpackType = LODPackType.OneSide;
                            break;
                        }
                }
                
                indexInRegistered = LODSpriteMaker.current.CreateLODPack(lpackType, model, renderpoints, resolution, size, backgroundColor, regInfo);
            }
            model.transform.parent = hr.transform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(Vector3.zero);
            LODController.GetCurrent().TakeCare(model.transform, indexInRegistered, lodDistance, visibilityDistance);
        }
        else
        {
            model.transform.parent = hr.transform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
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
        if (PoolMaster.useAdvancedMaterials) PoolMaster.ReplaceMaterials(model.gameObject);
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
    override public List<byte> Save()
    {
        List<byte> data = SerializeStructure();
        data.AddRange(SerializeHarvestableResource());
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + CONTAINER_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        int containerStartIndex = STRUCTURE_SERIALIZER_LENGTH;
        mainResource = ResourceType.GetResourceTypeById( System.BitConverter.ToInt32(data, containerStartIndex) );
        resourceCount = System.BitConverter.ToSingle(data, containerStartIndex + 4);

        SetModel();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);
        SetBasement(sblock, new PixelPosByte(data[0], data[1]));
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 812);
    }

    public static void LoadContainer(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + CONTAINER_SERIALIZER_LENGTH];
        fs.Read(data, 0, data.Length);
        int containerStartIndex = STRUCTURE_SERIALIZER_LENGTH;
        ushort modelId = System.BitConverter.ToUInt16(data, containerStartIndex + 8);
        ResourceType resType = ResourceType.GetResourceTypeById(System.BitConverter.ToInt32(data, containerStartIndex));
        float count = System.BitConverter.ToSingle(data, containerStartIndex + 4);
        HarvestableResource hr = ConstructContainer((ContainerModelType)modelId, resType, count);

        hr.modelRotation = data[2];
        hr.indestructible = (data[3] == 1);
        hr.SetBasement(sblock, new PixelPosByte(data[0], data[1]));
        hr.hp = System.BitConverter.ToSingle(data,4);
        hr.maxHp = System.BitConverter.ToSingle(data, 8);
    }


    protected List<byte> SerializeHarvestableResource()
    {
        var data = new List<byte>();
        data.AddRange(System.BitConverter.GetBytes(mainResource.ID));
        data.AddRange(System.BitConverter.GetBytes(resourceCount));
        data.AddRange(System.BitConverter.GetBytes((ushort)model_id));
        // SERIALIZER_LENGTH = 10
        return data;
    }
    #endregion
}
