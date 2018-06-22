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

	//--------SAVE  SYSTEM-------------
	public virtual byte[] Save() {
		HarvestableResourceSerializer hrs = GetHarvestableResourceSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, hrs);
			return stream.ToArray();
		}
	}

	protected HarvestableResourceSerializer GetHarvestableResourceSerializer() {
		HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
		hrs.structureSerializer = GetStructureSerializer();
		hrs.mainResource = mainResource;
		hrs.count = count1;
		return hrs;
	}
	// ------------------------------------------
	override public void Annihilate( bool forced ) { // for pooling
		if (forced) basement = null;
		else GameMaster.colonyController.storage.AddResource(mainResource, count1);
		Destroy(gameObject);
	}

}
