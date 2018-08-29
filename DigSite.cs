using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigSite : Worksite {
	public bool dig = true;
	ResourceType mainResource;
	CubeBlock workObject;
	const int START_WORKERS_COUNT = 10, MAX_WORKERS = 60;


	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || (workObject.volume == CubeBlock.MAX_VOLUME && dig == false) || (workObject.volume ==0 && dig == true)) {
            StopWork();
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
					if (production == 0) { StopWork(); return;}
				}
			}
			workflow -= production;	
		actionLabel = Localization.GetActionLabel(LocalizationActionLabels.DigInProgress) + " ("+((int) ((1 -(float)workObject.volume / (float)CubeBlock.MAX_VOLUME) * 100)).ToString()+"%)";
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
                StopWork();
                return;
			}
			sign = Instantiate(Resources.Load<GameObject> ("Prefs/DigSign")).GetComponent<WorksiteSign>(); 
		}
		else 	sign = Instantiate(Resources.Load<GameObject>("Prefs/PourInSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.transform.position + Vector3.up * Block.QUAD_SIZE;
		mainResource = ResourceType.GetResourceTypeById(workObject.material_id);
        maxWorkers = MAX_WORKERS;
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
		GameMaster.colonyController.AddWorksite(this);
	}

    override public void StopWork()
    {
        if (workersCount > 0)
        {
            GameMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Destroy(sign.gameObject);
        if (workObject != null && workObject.excavatingStatus == 0) workObject.myChunk.AddBlock(new ChunkPos(workObject.pos.x, workObject.pos.y + 1, workObject.pos.z), BlockType.Surface, workObject.material_id, false);
        Destroy(this);
    }

    #region save-load system
    override public WorksiteSerializer Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
		WorksiteSerializer ws = GetWorksiteSerializer();
		ws.type = WorksiteType.DigSite;
		ws.workObjectPos = workObject.pos;
		ws.specificData = new byte[]{dig == true ? (byte)1 : (byte)0};
		return ws;
	}
	override public void Load(WorksiteSerializer ws) {
		LoadWorksiteData(ws);
		Set(GameMaster.mainChunk.GetBlock(ws.workObjectPos) as CubeBlock, ws.specificData[0] == 1);
	}
	#endregion
}
