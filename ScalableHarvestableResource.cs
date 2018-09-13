using UnityEngine;

public class ScalableHarvestableResource : Structure {
	public const float MAX_VOLUME = 64;
	public ResourceType mainResource {get;protected set;}
	public float resourceCount;

	override public void Prepare() {
		PrepareStructure();
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		resourceCount = 0;		
	}

    override protected void SetModel()
    {
        GameObject model = transform.GetChild(0).gameObject;
        if (model != null) Destroy(model);
        model = Instantiate(Resources.Load<GameObject>("Structures/resourcesStick")); 
        model.transform.parent = transform;
        model.transform.localRotation = Quaternion.Euler(0, 0, 0);
        model.transform.localPosition = Vector3.zero;
        if (resourceCount != 0)
        {
            Transform meshTransform = model.transform.GetChild(0);
            meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(mainResource.ID, meshTransform.GetComponent<MeshFilter>(),basement.illumination);
            model.transform.localScale = new Vector3(1, resourceCount / MAX_VOLUME, 1);
        }
    }

    override public void SetBasement(SurfaceBlock b, PixelPosByte pos)
    {
        if (b == null) return;
        SetStructureData(b, pos);
        transform.GetChild(0).localScale = new Vector3(1, 0, 1);
        //if (isBasement) basement.myChunk.chunkUpdateSubscribers_structures.Add(this);
    }

    public float AddResource( ResourceType type, float volume) {
		if (mainResource == ResourceType.Nothing) {
			mainResource = type;
            if (transform.childCount != 0)
            {
                Transform meshTransform = transform.GetChild(0).GetChild(0);
                meshTransform.GetComponent<MeshRenderer>().sharedMaterial = ResourceType.GetMaterialById(type.ID, meshTransform.GetComponent<MeshFilter>(), basement.illumination);
            }
		}
		else{
			if (type != mainResource) {
				return volume;
			}
		}
		float addingVolume = volume; 
		if (addingVolume > MAX_VOLUME - resourceCount) addingVolume = MAX_VOLUME - resourceCount;
		resourceCount += addingVolume;
		if (transform.childCount != 0) transform.GetChild(0).localScale = new Vector3(1, resourceCount/MAX_VOLUME,1);
		return volume - addingVolume;
	}
	public void Harvest() {
		resourceCount -= GameMaster.colonyController.storage.AddResource(mainResource,resourceCount);
		if (resourceCount == 0) Annihilate(false);
	}

	#region save-load system
	override public  StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHarvestableResourceSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
		GameMaster.DeserializeByteArray<HarvestableResourceSerializer>(ss.specificData, ref hrs);
		mainResource = ResourceType.GetResourceTypeById(hrs.mainResource_id);
		resourceCount = hrs.count;
	}

	protected HarvestableResourceSerializer GetHarvestableResourceSerializer() {
		HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
		hrs.mainResource_id = mainResource.ID;
		hrs.count = resourceCount;
		return hrs;
	}
	#endregion
	override public void Annihilate( bool forced ) { // for pooling
        if (destroyed) return;
        else destroyed = true;
        if (forced) basement = null;
		else GameMaster.colonyController.storage.AddResource(mainResource, resourceCount);
        Destroy(gameObject);
    }

}
