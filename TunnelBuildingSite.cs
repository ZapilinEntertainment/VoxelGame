using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelBuildingSite : Worksite {
	public byte signsMask = 0;
	CubeBlock workObject;
	const int START_WORKERS_COUNT = 10;


	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null ) {
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
		production = workObject.Dig(x, false);
		GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
		workflow -= production;	
		actionLabel = Localization.ui_dig_in_progress + " ("+((int) (((float)workObject.volume / (float)CubeBlock.MAX_VOLUME) * 100)).ToString()+"%)";
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount,WorkType.Digging);
	}

	public void Set(CubeBlock block) {
		workObject = block;
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
		GameMaster.colonyController.AddWorksite(this);
	}

	//---------SAVE   SYSTEM----------------
	public override string Save() {
		return SaveWorksite() + SaveTunnelBuildingSite();
	}
	protected string SaveTunnelBuildingSite() {
		string s = "";
		s += string.Format("{0:00}",workObject.pos.x) + string.Format("{0:00}",workObject.pos.y) + string.Format("{0:00}",workObject.pos.z); 
		s += string.Format("{0:00}", signsMask);
		return s;
	}
	public override void Load(string s) {
		workersCount = int.Parse(s.Substring(1,3));
		workflow = int.Parse(s.Substring(4,4)) / 100f;
		labourTimer = int.Parse(s.Substring(8,4)) / 100f;
		// position
		workObject = GameMaster.mainChunk.GetBlock(int.Parse(s.Substring(12,2)), int.Parse(s.Substring(14,2)), int.Parse(s.Substring(16,2)) ) as CubeBlock;
		signsMask = (byte)int.Parse(s.Substring(18,2));
		if (signsMask != 0) {
			WorksiteSign sign = null;
				if ((signsMask & 1) != 0) {
					sign = Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
					sign.transform.position = workObject.transform.position + Vector3.forward * Block.QUAD_SIZE / 2f;
					sign.worksite = this;
				}	
				if ((signsMask & 2 ) != 0) {
					sign = Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
					sign.transform.position = workObject.transform.position + Vector3.right * Block.QUAD_SIZE / 2f;
					sign.transform.rotation = Quaternion.Euler(0,90,0);
					sign.worksite = this;
				}
				if ((signsMask & 4 ) != 0) {
					sign = Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
					sign.transform.position = workObject.transform.position + Vector3.back * Block.QUAD_SIZE / 2f;
					sign.transform.rotation = Quaternion.Euler(0,180,0);
					sign.worksite = this;
				}
				if ((signsMask & 8) != 0) {
					sign = Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
					sign.transform.position =workObject.transform.position + Vector3.left * Block.QUAD_SIZE / 2f;
					sign.transform.rotation = Quaternion.Euler(0,-90,0);
					sign.worksite = this;
				}
		}
		GameMaster.colonyController.AddWorksite(this);
	}
	// --------------------------------------------------------
}
