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
	public override string Save() {
		return SaveStructureData() + SaveScalableContainerData();
	}

	protected string SaveScalableContainerData() {
		string s = string.Format("{0:d3}", mainResource.ID);
		s += string.Format("{0:d4}", count1 / MAX_VOLUME * 1000);
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
		// container part:
		//print (int.Parse(s_data.Substring(11,3)));
		mainResource =  ResourceType.GetResourceTypeById(int.Parse(s_data.Substring(11,3)));
		count1 = int.Parse( s_data.Substring(14,4)) / 1000f * MAX_VOLUME;
		myRenderer.sharedMaterial = mainResource.material;
		transform.localScale = new Vector3(1, count1/MAX_VOLUME, 1);
	}
	// ------------------------------------------
	override public void Annihilate( bool forced ) { // for pooling
		if (forced) basement = null;
		else GameMaster.colonyController.storage.AddResource(mainResource, count1);
		Destroy(gameObject);
	}

}
