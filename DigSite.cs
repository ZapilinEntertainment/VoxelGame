using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DigSite : Worksite {
	public bool dig = true;
	ResourceType mainResource;
	CubeBlock workObject;
	const int START_WORKERS_COUNT = 10;


	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || (workObject.volume == CubeBlock.MAX_VOLUME && dig == false) || (workObject.volume ==0 && dig == true)) {
			Destroy(this);
			return;
		}
		if (workersCount > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed;
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if ( labourTimer <= 0 ) {
				if (workflow >= 1) LabourResult();
				labourTimer = GameMaster.LABOUR_TICK;
			}
		}
	}

	void LabourResult() {
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
		actionLabel = Localization.ui_dig_in_progress + " ("+((int) (((float)workObject.volume / (float)CubeBlock.MAX_VOLUME) * 100)).ToString()+"%)";
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount,WorkType.Digging);
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
			sign = Instantiate(Resources.Load<GameObject> ("Prefs/DigSign")).GetComponent<WorksiteSign>(); 
		}
		else 	sign = Instantiate(Resources.Load<GameObject>("Prefs/PourInSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.transform.position + Vector3.up * Block.QUAD_SIZE;
		mainResource = ResourceType.GetResourceTypeById(workObject.material_id);
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
		GameMaster.colonyController.AddWorksite(this);
	}

	//---------SAVE   SYSTEM----------------
	public override string Save() {
		return '2' + SaveWorksite() + SaveDigSite();
	}
	protected string SaveDigSite() {
		string s = "";
		s += string.Format("{0:00}",workObject.pos.x) + string.Format("{0:00}",workObject.pos.y) + string.Format("{0:00}",workObject.pos.z); 
		if (dig) s += '1'; else s+='0';
		return s;
	}
	public override void Load(string s) {
		workersCount = int.Parse(s.Substring(1,3));
		workflow = int.Parse(s.Substring(4,4)) / 100f;
		labourTimer = int.Parse(s.Substring(8,4)) / 100f;
		// position
		workObject = GameMaster.mainChunk.GetBlock(int.Parse(s.Substring(12,2)), int.Parse(s.Substring(14,2)), int.Parse(s.Substring(16,2)) ) as CubeBlock;
		//dig site part
		dig = (s[18] == '1');
		if (dig) sign = Instantiate(Resources.Load<GameObject> ("Prefs/DigSign")).GetComponent<WorksiteSign>(); 
		else 	sign = Instantiate(Resources.Load<GameObject>("Prefs/PourInSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.transform.position + Vector3.up * Block.QUAD_SIZE;
		mainResource = ResourceType.GetResourceTypeById(workObject.material_id);
		GameMaster.colonyController.AddWorksite(this);
	}
	// --------------------------------------------------------
}
