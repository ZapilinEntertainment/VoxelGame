using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HarvestableResourceSerializer {
	public int mainResource_id;
	public float count;
}

public class HarvestableResource : Structure {
	public ResourceType mainResource {get;protected set;}
	public float count1;
    static Dictionary<ResourceType, short> materialBasedLods = new Dictionary<ResourceType, short>();
    static LODController modelController;

	override public void Prepare() {
		PrepareStructure();
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		count1 = 0;
        if (modelController == null) modelController = LODController.GetCurrent();
	}

	public void SetResources(ResourceType resType, float f_count1) {
        ResourceType prevResType = mainResource;
		mainResource = resType;
		count1 = f_count1;
		if (myRenderers!= null & myRenderers.Count> 0 ) {
			if (myRenderers[0] != null) Destroy(myRenderers[0]);
			myRenderers.RemoveAt(0);
		}
		GameObject model = null;
		switch (resType.ID) {
		case ResourceType.STONE_ID:
		case ResourceType.METAL_K_ORE_ID:
		case ResourceType.METAL_S_ORE_ID:
		case ResourceType.METAL_P_ORE_ID:
		case ResourceType.METAL_N_ORE_ID:
		case ResourceType.METAL_E_ORE_ID:
		case ResourceType.METAL_M_ORE_ID:
			model = Instantiate(Resources.Load<GameObject>("Prefs/boulderPref"));
			break;
		case ResourceType.MINERAL_F_ID:
		case ResourceType.MINERAL_L_ID:
			model = Instantiate(Resources.Load<GameObject>("Prefs/pilePref"));
			break;
		default:
			model = Instantiate(Resources.Load<GameObject>("Prefs/defaultContainer"));
			break;
		}
		if (model != null) {
			model.transform.parent = transform;
			model.transform.localPosition = Vector3.zero;
			model.transform.localRotation = Quaternion.Euler(0, Random.value * 360, 0);
			//model.transform.localScale = Vector3.one * (1.2f + Random.value * 0.6f);
			if (myRenderers == null) myRenderers = new List<Renderer>();
			myRenderers.Add( model.transform.GetChild(0).GetComponent<MeshRenderer>());
			myRenderers[0].sharedMaterial = ResourceType.GetMaterialById(resType.ID, myRenderers[0].GetComponent<MeshFilter>());

            short packIndex = -1;
            if (!materialBasedLods.TryGetValue(resType, out packIndex))
            {
                Vector3[] positions = new Vector3[] { new Vector3(0, 0.084f, -0.063f)};
                Vector3[] angles = new Vector3[] { new Vector3(45,0,0) };
                Texture2D spritesAtlas = LODSpriteMaker.current.MakeSpriteLODs(model, positions, angles, 0.06f, Color.grey);
                Sprite[] lodSprites = new Sprite[1];

                lodSprites[0] = Sprite.Create(spritesAtlas, new Rect(0, 0, spritesAtlas.width, spritesAtlas.height), new Vector2(0.5f, 0.5f), 512);
                packIndex = LODController.AddSpritePack(lodSprites);
                materialBasedLods.Add(resType, packIndex);
            }
            if (prevResType == ResourceType.Nothing)
            {                
                modelController.AddObject(model.transform, ModelType.Boulder, packIndex);
            }
            else
            {
                if (prevResType != resType)
                {
                    LODController.GetCurrent().ChangeModelSpritePack(transform, ModelType.Boulder, packIndex);
                }
            }
		}
	}

	public void Harvest() {
		count1 = GameMaster.colonyController.storage.AddResource(mainResource,count1);
		if (count1 <= 0) Annihilate(false);
	}

	#region save-load system
	override public  StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetHarvestableResourceSerializer());
			ss.specificData = stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
		GameMaster.DeserializeByteArray<HarvestableResourceSerializer>(ss.specificData, ref hrs);
		SetResources(ResourceType.GetResourceTypeById(hrs.mainResource_id), hrs.count);
	}


	protected HarvestableResourceSerializer GetHarvestableResourceSerializer() {
		HarvestableResourceSerializer hrs = new HarvestableResourceSerializer();
		hrs.mainResource_id = mainResource.ID;
		hrs.count = count1;
		return hrs;
	}
	#endregion
}
