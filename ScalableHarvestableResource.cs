using UnityEngine;
using System.Collections.Generic;

public sealed class ScalableHarvestableResource : Structure {

    // переделать к обычному harvestable resource
    // 04.11 стоит ли?
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
        GameObject model = null;
        if (transform.childCount > 0) model = transform.GetChild(0).gameObject;
        if (model != null) Destroy(model);
        model = Instantiate(Resources.Load<GameObject>("Structures/resourceStick")); 
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
        SetModel();
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
		resourceCount -= GameMaster.realMaster.colonyController.storage.AddResource(mainResource,resourceCount);
		if (resourceCount == 0) Annihilate(false);
	}
    
	override public void Annihilate( bool forced ) { // for pooling
        if (destroyed) return;
        else destroyed = true;
        if (forced) basement = null;
		else GameMaster.realMaster.colonyController.storage.AddResource(mainResource, resourceCount);
        Destroy(gameObject);
    }

    #region save-load system
    override public List<byte> Save()
    {
        var data = SerializeStructure();
        data.AddRange(System.BitConverter.GetBytes(mainResource.ID));
        data.AddRange(System.BitConverter.GetBytes(resourceCount));
        return data;
    }

    override public void Load(System.IO.FileStream fs, SurfaceBlock sblock)
    {
        LoadStructureData(fs, sblock);
        var data = new byte[8];
        fs.Read(data, 0, data.Length);
        mainResource = ResourceType.GetResourceTypeById(System.BitConverter.ToInt32(data, 0));
        resourceCount = System.BitConverter.ToInt32(data, 4);
    }

    
    #endregion
}
