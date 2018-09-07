using UnityEngine;

public class TunnelBuildingSite : Worksite {
	public byte signsMask = 0;
	CubeBlock workObject;
	const int START_WORKERS_COUNT = 10;


	override public void WorkUpdate () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null ) {
            StopWork();
            return;
		}
		if (workersCount > 0) {
			workflow += workSpeed;
			if (workflow >= 1) LabourResult();
			}
		}

	void LabourResult() {
		int x = (int) workflow;
		float production = x;
		production = workObject.Dig(x, false);
		GameMaster.geologyModule.CalculateOutput(production, workObject, GameMaster.colonyController.storage);
		workflow -= production;	
		actionLabel = Localization.GetActionLabel(LocalizationActionLabels.DigInProgress) + " ("+((int) (((float)workObject.volume / (float)CubeBlock.MAX_VOLUME) * 100)).ToString()+"%)";
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount,WorkType.Digging);
	}

	public void Set(CubeBlock block) {
		workObject = block;
        workObject.SetWorksite(this);
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate) {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }
    }

	public void CreateSign(byte side) {
		if ((signsMask & side) != 0) return;
		WorksiteSign sign = null;
		switch (side) {
		case 0:
				sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
				sign.transform.position = workObject.model.transform.position + Vector3.forward * Block.QUAD_SIZE / 2f;
				signsMask += 1;
			break;
		case 1:
				sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
			sign.transform.position = workObject.model.transform.position + Vector3.right * Block.QUAD_SIZE / 2f;
				sign.transform.rotation = Quaternion.Euler(0,90,0);
				signsMask += 2;
			break;
		case 2:
				sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
				sign.transform.position = workObject.model.transform.position + Vector3.back * Block.QUAD_SIZE / 2f;
				sign.transform.rotation = Quaternion.Euler(0,180,0);
				signsMask += 4;
			break;
		case 3:
				sign = Object.Instantiate(Resources.Load<GameObject>("Prefs/tunnelBuildingSign")).GetComponent<WorksiteSign>();
				sign.transform.position = workObject.model.transform.position + Vector3.left * Block.QUAD_SIZE / 2f;
				sign.transform.rotation = Quaternion.Euler(0,-90,0);
				signsMask += 8;
			break;
		}
		if (sign != null) sign.worksite = this;
	}

    override public void StopWork()
    {
        if (deleted) return;
        else deleted = true;
        if (workersCount > 0)
        {
            GameMaster.colonyController.AddWorkers(workersCount);
            workersCount = 0;
        }
        if (sign != null) Object.Destroy(sign.gameObject);
        if (worksitesList.Contains(this)) worksitesList.Remove(this);
        if (subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent -= WorkUpdate;
            subscribedToUpdate = false;
        }
        if (workObject != null)
        {
            if (workObject.worksite == this) workObject.ResetWorksite();
            workObject = null;
        }
        if (showOnGUI)
        {
            observer.SelfShutOff();
            showOnGUI = false;
        }
    }

    #region save-load system
    override protected WorksiteSerializer Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
		WorksiteSerializer ws = GetWorksiteSerializer();
		ws.type = WorksiteType.TunnelBuildingSite;
		ws.workObjectPos = workObject.pos;
		ws.specificData = new byte[1]{signsMask};
		return ws;
	}
	override protected void Load (WorksiteSerializer ws) {
		LoadWorksiteData(ws);
		Set(GameMaster.mainChunk.GetBlock(ws.workObjectPos) as CubeBlock);
		int smask = ws.specificData[0];
		if ((smask & 1) != 0) CreateSign(0);
		if ((smask & 2 )!= 0) CreateSign(1);
		if ((smask & 4 )!= 0) CreateSign(2);
		if ((smask & 8 )!= 0) CreateSign(3);
	}
	#endregion
}
