using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DigSite : MonoBehaviour {
	public const int MAX_WORKERS = 96;
	public int workersCount {get;private set;}
	public float workflow;
	public bool dig = true;
	CubeBlock workObject;
	GameObject sign;
	ResourceType mainResource;
	float labourTimer = 0;

	void Awake () {
		workersCount = 0;
	}

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || (workObject.volume == CubeBlock.MAX_VOLUME && dig == false) || (workObject.volume ==0 && dig == true)) {
			Destroy(this);
			return;
		}
		if (workersCount > 0) {
			if (dig) workflow += GameMaster.CalculateWorkflow(workersCount,WorkType.Digging);
			else workflow += GameMaster.CalculateWorkflow(workersCount,WorkType.Pouring);
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if (labourTimer <= 0) {LabourResult();labourTimer = GameMaster.LABOUR_TICK;}
		}
	}

	void LabourResult() {
		if (workflow > 1) {
			int x = (int) workflow;
			float production = x;
			if (dig) {
				production = workObject.Dig(x, true);
				GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
			}
			else {
				production = GameMaster.colonyController.storage.GetResources(mainResource, production);
				if (production != 0) {
					production = workObject.PourIn((int)production);
					if (production == 0) {Destroy(this);return;}
				}
			}
			workflow -= production;	
		}
	}

	public void Set(CubeBlock block, bool work_is_dig) {
		workObject = block;
		dig = work_is_dig;
		if (dig) {
			Block b = GameMaster.mainChunk.GetBlock(block.pos.x, block.pos.y, block.pos.z);
			if (b != null && b.type == BlockType.Surface) {
				CleanSite cs = b.gameObject.AddComponent<CleanSite>();
				cs.Set(b.gameObject.GetComponent<SurfaceBlock>(), true);
				Destroy(this);
				return;
			}
			sign = Instantiate(Resources.Load<GameObject> ("Prefs/DigSign")) as GameObject; 
			block.digStatus = -1;
		}
		else {
			sign = Instantiate(Resources.Load<GameObject>("Prefs/PourInSign"));
			block.digStatus = 1;
		}
		sign.transform.parent = workObject.transform;
		sign.transform.localPosition = Vector3.up * 0.5f;
		GameMaster.colonyController.digSites.Add(this);
		mainResource = ResourceType.GetResourceTypeByMaterialId(workObject.material_id);
	}

	public void AddWorkers (int x) {
		if (x > 0) workersCount += x;
	}

	void OnDestroy() {
		GameMaster.colonyController.AddWorkers(workersCount);
		if (sign != null) Destroy(sign);
		if (workObject != null) {
			if (dig && workObject.digStatus == -1) workObject.digStatus = 0;
			else if (!dig && workObject.digStatus == 1) workObject.digStatus = 0;
		} 
	}
}
