using UnityEngine;
using System.Collections.Generic;

public sealed class ScalableHarvestableResource : Structure {

	public ResourceType mainResource {get;private set;}
	public byte resourceCount;
    public static int MAX_VOLUME
    {
        get { return CubeBlock.MAX_VOLUME / SurfaceBlock.INNER_RESOLUTION / SurfaceBlock.INNER_RESOLUTION; }
    }

    private static Dictionary<byte, Mesh> meshes = new Dictionary<byte, Mesh>(); 

    public static void Create(ResourceType i_resource, byte count, SurfaceBlock surface, PixelPosByte pos)
    {
        GameObject g = new GameObject("ScalableHarvestableResource");
        var shr = g.AddComponent<ScalableHarvestableResource>();
        shr.mainResource = i_resource;
        shr.resourceCount = count;
        shr.SetBasement(surface, pos);
    }

	override public void Prepare() {
		PrepareStructure();
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		resourceCount = 0;		
	}

    override protected void SetModel()
    {
        GameObject model = null;
        if (transform.childCount > 0) model = transform.GetChild(0).gameObject;
        Mesh m = null;
        if (meshes.ContainsKey(resourceCount))
        {
            meshes.TryGetValue(resourceCount, out m);
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
                float p = Block.QUAD_SIZE / (float)SurfaceBlock.INNER_RESOLUTION, x = p / 2f;
                float h = resourceCount * p;
                var vertices = new Vector3[12]
                {
                    new Vector3(-x, 0, x), new Vector3(-x, h, x), new Vector3(x, h,x), new Vector3(x,0,x),
                    new Vector3(x,0,x), new Vector3(x,h,x),
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
                x = 0.25f * p;
                var uvs = new Vector2[12]
                {
                    new Vector2(x,0), new Vector2(x,x), new Vector2(0,x), new Vector2(0,0),
                    new Vector2(x,0), new Vector2(x,x), 
                    new Vector2(0,x), new Vector2(0,0),
                    new Vector2(0,x), new Vector2(x,x), new Vector2(x,0), new Vector2(0,0)
                };
                m.vertices = vertices;
                m.triangles = triangles;
                m.uv = uvs;
                meshes.Add(resourceCount, m);
            }
        }
        if (model != null)
        {
            PoolMaster.SetMeshUVs(ref m, mainResource.ID);
            model.GetComponent<MeshFilter>().mesh = m;
            //byte light;
            //if (basement != null) light = basement.myChunk.lightMap[basement.pos.x, basement.pos.y, basement.pos.z];
            //else light = 255;
            model.GetComponent<MeshRenderer>().sharedMaterial = PoolMaster.GetMaterial(mainResource.ID);
        }
        else
        {
            model = new GameObject("resourceStick");
            model.AddComponent<MeshFilter>().mesh = m;
            var mr = model.AddComponent<MeshRenderer>();
            mr.receiveShadows = PoolMaster.useAdvancedMaterials;
            mr.shadowCastingMode = PoolMaster.useAdvancedMaterials ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.sharedMaterial = PoolMaster.GetMaterial(mainResource.ID);
        }     
    }

    public float AddResource( ResourceType type, float volume) {
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
        float addingVolume = volume;
		if (addingVolume > MAX_VOLUME - resourceCount) addingVolume = MAX_VOLUME - resourceCount;
		if (addingVolume > 1)
        {
            resourceCount += (byte)addingVolume;
            modelChanging = true;
        }
        if (modelChanging) SetModel();
		return volume - addingVolume;
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
        var data = SerializeStructure();
        data.AddRange(System.BitConverter.GetBytes(mainResource.ID));
        data.Add(resourceCount);
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        LoadStructureData(fs, sblock);
        var data = new byte[8];
        fs.Read(data, 0, data.Length);
        mainResource = ResourceType.GetResourceTypeById(System.BitConverter.ToInt32(data, 0));
        resourceCount = data[4];
    }

    
    #endregion
}
