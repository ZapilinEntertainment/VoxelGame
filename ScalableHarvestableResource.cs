using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableHarvestableResource : Structure {
	public const float MAX_VOLUME = 64;
	public ResourceType mainResource {get;protected set;}
	public float count1;

	override public void Prepare() {
		PrepareStructure();
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		count1 = 0;
		transform.localScale = new Vector3(1,0,1);
	}

	public float AddResource( ResourceType type, float volume) {
		if (mainResource == ResourceType.Nothing) {
			mainResource = type;
			myRenderer.sharedMaterial = type.material;
		}
		else{
			if (type != mainResource) {
				return volume;
			}
		}
		float addingVolume = volume; 
		if (addingVolume > MAX_VOLUME - count1) addingVolume = MAX_VOLUME - count1;
		count1 += addingVolume;
		transform.localScale = new Vector3(1,count1/MAX_VOLUME,1);
		return volume - addingVolume;
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
		count1 = hrs.count;
	}

	protected HarvestableResourceSerializer GetHarvestableResourceSerializer() {
		HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
		hrs.mainResource_id = mainResource.ID;
		hrs.count = count1;
		return hrs;
	}
	#endregion
	override public void Annihilate( bool forced ) { // for pooling
		if (forced) basement = null;
		else GameMaster.colonyController.storage.AddResource(mainResource, count1);
		Destroy(gameObject);
	}

}
