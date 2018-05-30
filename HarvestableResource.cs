using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableResource : Structure {
	public ResourceType mainResource {get;protected set;}
	public float count1;


	override public void Prepare() {
		PrepareStructure();
		mainResource = ResourceType.Nothing; 
		hp = maxHp;
		count1 = 0;
	}

	public void SetResources(ResourceType resType, float f_count1) {
		mainResource = resType;
		count1 = f_count1;
		if (myRenderer != null) Destroy(myRenderer.gameObject);
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
			model.transform.localScale = Vector3.one * (1.2f + Random.value * 0.6f);
			myRenderer = model.transform.GetChild(0).GetComponent<MeshRenderer>();
			myRenderer.sharedMaterial = ResourceType.GetMaterialById(resType.ID);
		}
	}
}
