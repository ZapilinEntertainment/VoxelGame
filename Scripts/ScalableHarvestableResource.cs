using UnityEngine;
using System.Collections.Generic;

public sealed class ScalableHarvestableResource : Structure {

	public ResourceType mainResource {get;private set;}
	public byte resourceCount { get; private set; }
    public const byte RESOURCE_STICK_RECT_SIZE = 2; // dependency : SurfaceRect.ScatterResources;
    public static readonly byte MAX_STICK_VOLUME = (byte)(CubeBlock.MAX_VOLUME / (SurfaceBlock.INNER_RESOLUTION / RESOURCE_STICK_RECT_SIZE * SurfaceBlock.INNER_RESOLUTION / RESOURCE_STICK_RECT_SIZE)),
        RESOURCES_PER_LEVEL = RESOURCE_STICK_RECT_SIZE * RESOURCE_STICK_RECT_SIZE;

    private static Dictionary<byte, Mesh> meshes = new Dictionary<byte, Mesh>();

    private static byte prevModelLevel = 0, prevModelLight = 255;
    private static int prevModelMaterialID = -1;
    private static Mesh prevModelMesh = null;

    public static ScalableHarvestableResource Create(ResourceType i_resource, byte count, SurfaceBlock surface, PixelPosByte pos)
    {
        GameObject g = new GameObject("ScalableHarvestableResource");
        var shr = g.AddComponent<ScalableHarvestableResource>();
        shr.ID = RESOURCE_STICK_ID;
        shr.PrepareStructure();
        shr.mainResource = i_resource;
        shr.resourceCount = count;
        shr.SetBasement(surface, pos);
        return shr;
    }

	override public void Prepare() {
		PrepareStructure();
		mainResource = ResourceType.Nothing; 
		resourceCount = 0;		
        //dependency : Create()
	}

    override protected void SetModel()
    {
        GameObject model = null;
        if (transform.childCount > 0) model = transform.GetChild(0).gameObject;
        Mesh m = null;
        byte level = (byte)(resourceCount / RESOURCES_PER_LEVEL);
        if (meshes.ContainsKey(level))
        {
            meshes.TryGetValue(level, out m);
        }
        else
        {            
            if (resourceCount == 0)
            {
                Annihilate(true, false, false);
                return;
            }
            else
            {
                m = new Mesh();
                float p = Block.QUAD_SIZE * RESOURCE_STICK_RECT_SIZE / (float)SurfaceBlock.INNER_RESOLUTION, x = p / 2f;
                float h = level * 1f / (MAX_STICK_VOLUME / RESOURCES_PER_LEVEL) ;
                var vertices = new Vector3[12]
                {
                    new Vector3(-x, 0, x), new Vector3(-x, h, x), new Vector3(x, h,x), new Vector3(x,0,x),
                    new Vector3(x,0,-x), new Vector3(x,h,-x),
                    new Vector3(-x, h, -x), new Vector3(-x,0,-x),
                    new Vector3(-x,h,x), new Vector3(x,h,x), new Vector3(x,h,-x), new Vector3(-x,h,-x)
                };
                var triangles = new int[30]
                {
                    3,2,1, 3,1,0,
                    4,5,2, 4,2,3,
                    7,6,5, 7,5,4,
                    0,1,6, 0,6,7,
                    11,8,9, 11,9,10
                };
                x = 1f;
                var uvs = new Vector2[12]
                {
                    new Vector2(x - 0.01f, 0.01f), new Vector2(x - 0.01f, x - 0.01f), new Vector2(0.01f,x - 0.01f), Vector2.one * 0.01f,
                    new Vector2(x - 0.01f,0.01f), new Vector2(x - 0.01f,x - 0.01f), 
                    new Vector2(0.01f,x - 0.01f), Vector2.one * 0.01f,
                    new Vector2(0,x - 0.01f ), new Vector2(x - 0.01f,x - 0.01f), new Vector2(x - 0.01f,0), Vector2.one * 0.01f
                };
                m.vertices = vertices;
                m.triangles = triangles;
                m.uv = uvs;                
                meshes.Add(level, m);
            }
        }

        MeshFilter mf; MeshRenderer mr;
        if (model != null)
        {
            mf = model.GetComponent<MeshFilter>();
            mr = model.GetComponent<MeshRenderer>();                      
        }
        else
        {
            model = new GameObject("resourceStick");
            model.transform.parent = transform;
            model.transform.localRotation = Quaternion.Euler(0, 0, 0);
            model.transform.localPosition = Vector3.zero;
            mf = model.AddComponent<MeshFilter>();
            mr = model.AddComponent<MeshRenderer>();
            mr.receiveShadows = PoolMaster.shadowCasting;
            mr.shadowCastingMode = PoolMaster.shadowCasting? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        byte light;
        if (basement != null) light = basement.myChunk.lightMap[basement.pos.x, basement.pos.y, basement.pos.z];
        else light = 255;

        if (prevModelMesh != null)
        {
            if (prevModelLight == light && prevModelLevel == level && prevModelMaterialID == mainResource.ID)
            {
                mf.sharedMesh = prevModelMesh;
                mr.sharedMaterial = PoolMaster.GetMaterial(mainResource.ID);
                return;
            }
        }
        
        mf.sharedMesh = m;
        prevModelMesh = PoolMaster.SetMaterialByID(ref mf, ref mr, mainResource.ID, light);
        prevModelLevel = level;
        prevModelLight = light;
        prevModelMaterialID = mainResource.ID;
    }

    public float AddResource( ResourceType type, float i_volume) {
        byte volume = (byte)i_volume;
        bool modelChanging = false;
		if (mainResource == ResourceType.Nothing) {
			mainResource = type;
            modelChanging = true;
		}
		else{
			if (type != mainResource) {
				return volume;
			}
		}
        byte addingVolume = volume;
		if (addingVolume > MAX_STICK_VOLUME - resourceCount) addingVolume = (byte)(MAX_STICK_VOLUME - resourceCount);
		if (addingVolume > 1)
        {
            resourceCount += addingVolume;
            modelChanging = true;
        }
        if (modelChanging) SetModel();
		return i_volume - addingVolume;
	}
	public void Harvest() {
		resourceCount -= (byte)GameMaster.realMaster.colonyController.storage.AddResource(mainResource,resourceCount);
		if (resourceCount == 0) Annihilate(true, false, false);
	}
    
	override public void Annihilate( bool clearFromSurface, bool returnResources, bool leaveRuins ) { // for pooling
        if (destroyed) return;
        else destroyed = true;
        if (!clearFromSurface) basement = null;
        else
        {
            basement.RemoveStructure(this);
        }
		if (returnResources) GameMaster.realMaster.colonyController.storage.AddResource(mainResource, resourceCount);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SaveStructureData();
        data.AddRange(System.BitConverter.GetBytes(mainResource.ID));
        data.Add(resourceCount);
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        var data = new byte[STRUCTURE_SERIALIZER_LENGTH + 5];
        fs.Read(data, 0, data.Length);
        Prepare();
        modelRotation = data[2];
        indestructible = (data[3] == 1);
        skinIndex = System.BitConverter.ToUInt32(data, 4);       
        //
        mainResource = ResourceType.GetResourceTypeById(System.BitConverter.ToInt32(data, STRUCTURE_SERIALIZER_LENGTH));
        resourceCount = data[STRUCTURE_SERIALIZER_LENGTH + 4];
        //
        SetBasement(sblock, new PixelPosByte(data[0], data[1]));
        hp = System.BitConverter.ToSingle(data, 8);
        maxHp = System.BitConverter.ToSingle(data, 12);
    }

    
    #endregion
}
