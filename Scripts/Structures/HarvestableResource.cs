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
    private PlaneBoundLODModel lodComponent;

    public static HarvestableResource ConstructContainer(ContainerModelType i_modelType, ResourceType i_rtype, float i_count)
    {
        HarvestableResource hr = new GameObject().AddComponent<HarvestableResource>();
        hr.ID = CONTAINER_ID;
        hr.PrepareStructure();
        hr.mainResource = i_rtype;
        hr.resourceCount = i_count;              
        hr.model_id = i_modelType;
        switch (hr.model_id)
        {
            case ContainerModelType.DeadLifestone:
                {
                    hr.gameObject.name = "dead Lifestone";
                    byte c = (byte)(PlaneExtension.INNER_RESOLUTION / 4);
                    hr.surfaceRect = new SurfaceRect(c, c, (byte)(c + c));
                    hr.maxHp = LifeSource.MAX_HP * 0.9f;
                    break;
                }
            case ContainerModelType.DeadTreeOfLife:
                {
                    hr.gameObject.name = "dead Tree of Life";
                    byte c = (byte)(PlaneExtension.INNER_RESOLUTION / 4);
                    hr.surfaceRect = new SurfaceRect(c, c, (byte)(c + c));
                    hr.maxHp = LifeSource.MAX_HP * 0.9f;
                    break;
                }
            case ContainerModelType.DeadOak4:
                {
                    hr.gameObject.name = "dead oak 4";
                    hr.surfaceRect = SurfaceRect.one;
                    hr.maxHp = 50;
                    break;
                }
            case ContainerModelType.DeadOak5:
                hr.gameObject.name = "dead oak 5";
                hr.surfaceRect = SurfaceRect.one;
                hr.maxHp = 100;
                break;
            case ContainerModelType.DeadOak6:
                hr.gameObject.name = "dead oak 6";
                hr.surfaceRect = SurfaceRect.one;
                hr.maxHp = 200;
                break;
            case ContainerModelType.Pile:
                {
                    hr.gameObject.name = "pile";
                    hr.maxHp = 30;
                    hr.surfaceRect = SurfaceRect.one;
                    break;
                }
            case ContainerModelType.BerryBush:
                {
                    hr.gameObject.name = "berry bush";
                    hr.maxHp = 10;
                    hr.surfaceRect = SurfaceRect.one;
                    break;
                }
            case ContainerModelType.Boulder:
                {
                    hr.gameObject.name = "boulder";
                    hr.maxHp = 50;
                    hr.surfaceRect = SurfaceRect.one;
                    break;
                }
            default:
                {
                    hr.gameObject.name = "default container";
                    hr.maxHp = 10;
                    hr.surfaceRect = SurfaceRect.one;
                    break;
                }
        }
        hr.hp = hr.maxHp;
        return hr;
    }

    override public void Prepare()
    {
        PrepareStructure();
        maxHp = 10;
        surfaceRect = SurfaceRect.one;
        isArtificial = false;

        mainResource = ResourceType.Nothing;
        hp = maxHp;
        resourceCount = 0;
        model_id = ContainerModelType.Default;
    }

    override public void SetBasement(Plane b, PixelPosByte pos)
    {
        if (b == null) return;
        //#setStructureData
        if (lodComponent != null && basement != null) lodComponent.ChangeBasement(b);
        basement = b;
        surfaceRect = new SurfaceRect(pos.x, pos.y, surfaceRect.size);
        b.AddStructure(this);
        SetModel();
    }

    protected override void SetModel()
    {
        if (transform.childCount == 0)
        {
            GameObject model;

            bool createSpriteLOD = false;
            LODRegisterInfo regInfo = new LODRegisterInfo(0, 0, 0);
            float height = 0;
            switch (model_id)
            {
                case ContainerModelType.DeadLifestone:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Structures/LifeStone"));
                        Destroy(model.transform.GetChild(0).gameObject);
                        Destroy(model.transform.GetChild(1).gameObject);
                        MeshRenderer[] mrrs = model.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in mrrs)
                        {
                            mr.sharedMaterial = PoolMaster.GetMaterial(MaterialType.Basic);
                        }
                        break;
                    }
                case ContainerModelType.DeadTreeOfLife:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Lifeforms/deadTreeOfLife"));
                        break;
                    }
                case ContainerModelType.DeadOak4:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-4_dead"));
                        createSpriteLOD = true;
                        regInfo = new LODRegisterInfo(LODController.CONTAINER_MODEL_ID, (int)ContainerModelType.DeadOak4, 0);
                        height = 0.211f;
                        break;
                    }
                case ContainerModelType.DeadOak5:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-6_dead"));
                        createSpriteLOD = true;
                        regInfo = new LODRegisterInfo(LODController.CONTAINER_MODEL_ID, (int)ContainerModelType.DeadOak5, 0);
                        height = 0.211f;
                        break;
                    }
                case ContainerModelType.DeadOak6:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Lifeforms/oak-6_dead"));
                        createSpriteLOD = true;
                        regInfo = new LODRegisterInfo(LODController.CONTAINER_MODEL_ID, (int)ContainerModelType.DeadOak6, 0);
                        height = 0.211f;
                        break;
                    }
                case ContainerModelType.Pile:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Prefs/pilePref"));

                        createSpriteLOD = true;
                        regInfo = new LODRegisterInfo((int)ContainerModelType.Pile, 0, mainResource.ID);
                        height = 0.047f;

                        Transform meshTransform = model.transform.GetChild(0);
                        var mf = meshTransform.GetComponent<MeshFilter>();
                        var mr = meshTransform.GetComponent<MeshRenderer>();
                        PoolMaster.SetMaterialByID(
                            ref mf,
                            ref mr,
                            mainResource.ID,
                            255
                            );
                        break;
                    }
                case ContainerModelType.BerryBush:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Prefs/berryBush"));
                        //if (PoolMaster.shadowCasting) PoolMaster.ReplaceMaterials(model, true);
                        break;
                    }
                case ContainerModelType.Boulder:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Prefs/boulderPref"));
                        Transform meshTransform = model.transform.GetChild(0);
                        var mf = meshTransform.GetComponent<MeshFilter>();
                        var mr = meshTransform.GetComponent<MeshRenderer>();
                        PoolMaster.SetMaterialByID(
                            ref mf,
                            ref mr,
                            mainResource.ID,
                            255
                            );

                        regInfo = new LODRegisterInfo((int)ContainerModelType.Boulder, 0, mainResource.ID);
                        createSpriteLOD = true;
                        height = 0.047f;
                        break;
                    }
                default:
                    {
                        model = Instantiate(Resources.Load<GameObject>("Prefs/defaultContainer"));
                        Transform meshTransform = model.transform.GetChild(0);
                        var mf = meshTransform.GetComponent<MeshFilter>();
                        var mr = meshTransform.GetComponent<MeshRenderer>();
                        PoolMaster.SetMaterialByID(
                            ref mf,
                            ref mr,
                            mainResource.ID,
                            255
                            );
                        break;
                    }
            }

            if (createSpriteLOD )
            {
                SpriteRenderer sr = new GameObject("lod").AddComponent<SpriteRenderer>();
                sr.transform.parent = model.transform;
                sr.transform.localPosition = Vector3.up * height;
                sr.sharedMaterial = !PoolMaster.useDefaultMaterials ? PoolMaster.billboardShadedMaterial : PoolMaster.billboardMaterial;
                if (PoolMaster.shadowCasting) sr.receiveShadows = true;
                LODController currentLC = LODController.GetCurrent();
                LODPackType lpackType = LODPackType.Point;
                int indexInRegistered = currentLC.LOD_existanceCheck(regInfo);

                if (indexInRegistered == -1)
                {
                    int resolution = 8;
                    float size = 0.05f;
                    Color backgroundColor = Color.gray;
                    RenderPoint[] renderpoints = new RenderPoint[] { };

                    switch (model_id)
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
                                resolution = 64;
                                lpackType = LODPackType.OneSide;
                                break;
                            }
                    }

                    indexInRegistered = LODSpriteMaker.current.CreateLODPack(lpackType, model, renderpoints, resolution, size, backgroundColor, regInfo);
                }
                model.transform.parent = transform;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.Euler(Vector3.zero);
                lodComponent = currentLC.SetInControl(basement, model.transform.GetChild(0).gameObject, sr, indexInRegistered);
            }
            else
            {
                model.transform.parent = transform;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    public void Harvest()
    {
        resourceCount = GameMaster.realMaster.colonyController.storage.AddResource(mainResource, resourceCount);
        if (resourceCount <= 0) Annihilate(StructureAnnihilationOrder.SystemDestruction); // может быть переполнение и не все соберется
    }

    override public void Annihilate(StructureAnnihilationOrder order)
    {
        if (destroyed) return;
        else destroyed = true;
        if (lodComponent != null)
        {
            lodComponent.PrepareToDestroy();
            lodComponent = null;
        }
        PrepareStructureForDestruction(order);
        basement = null;
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        List<byte> data = SaveStructureData();
        data.AddRange(SerializeHarvestableResource());
        return data;
    }

    override public void Load(System.IO.Stream fs, Plane sblock)
    {
        // не используется
        Debug.Log("Harvestable Resource - wrong call");
    }

    public static HarvestableResource LoadContainer(System.IO.Stream fs, Plane sblock)
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
        return hr;
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
