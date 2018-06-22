using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TunnelBuildingSiteSerializer {
	public WorksiteSerializer worksiteSerializer;
	public byte signsMask;
}

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
	override public WorksiteBasisSerializer Save() {
		if (workObject == null) {
			Destroy(this);
			return null;
		}
		WorksiteBasisSerializer wbs = new WorksiteBasisSerializer();
		wbs.type = WorksiteType.TunnelBuildingSite;
		wbs.workObjectPos = workObject.pos;
		TunnelBuildingSiteSerializer tbss = new TunnelBuildingSiteSerializer();
		tbss.signsMask = signsMask;
		tbss.worksiteSerializer =GetWorksiteSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, tbss);
			wbs.data = stream.ToArray();
		}
		return wbs;
	}
	// --------------------------------------------------------
}
