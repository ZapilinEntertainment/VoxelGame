using UnityEngine;

public class CleanSite : Worksite {
	public bool diggingMission {get;protected set;}
	SurfaceBlock workObject;
	const int START_WORKERS_COUNT = 10;

	override public void WorkUpdate () {
		if (workObject ==null) {
            StopWork();
			return;
		}
		if (workObject.surfaceObjects.Count == 0) {
			Chunk ch = workObject.myChunk;
			int x = workObject.pos.x, y = workObject.pos.y, z = workObject.pos.z;
            if (workObject.type != BlockType.Cave) ch.DeleteBlock(workObject.pos);
            else (workObject as CaveBlock).DestroySurface();
			if (diggingMission) {
				Block basement = ch.GetBlock(x, y - 1, z);
				if (basement == null || basement.type != BlockType.Cube) {
					FreeWorkers(workersCount); 
				}
				else {
					DigSite ds =  new DigSite();
					ds.Set(basement as CubeBlock, true);
					workersCount =  ds.AddWorkers(workersCount);
					if (workersCount > 0) FreeWorkers();
                    ds.ShowOnGUI();
				}
			}
            StopWork();
			return;
		}		
        else
        {
            workflow += workSpeed;
            Structure s = workObject.surfaceObjects[0];
            float workGained = 0;
            if (s.id == Structure.PLANT_ID)
            {
                workGained = s.hp;
                (s as Plant).Harvest();                
            }
            else
            {
                HarvestableResource hr = s as HarvestableResource;
                if (hr != null)
                {
                    workGained = hr.resourceCount;
                    hr.Harvest();
                }
                else
                {
                    s.ApplyDamage(workflow);
                    workGained = workflow;
                }
            }
            workflow -= workGained;
            actionLabel = Localization.GetActionLabel(LocalizationActionLabels.CleanInProgress) + " (" + workObject.surfaceObjects.Count.ToString() + ' ' + Localization.GetPhrase(LocalizedPhrase.ObjectsLeft) + ")";
        }		
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Clearing);
	}

	public void Set(SurfaceBlock block, bool f_diggingMission) {
		workObject = block;
        workObject.SetWorksite(this);
		if (block.grassland != null) block.grassland.Annihilation(true); 
		if (sign == null) sign = Object.Instantiate(Resources.Load<GameObject> ("Prefs/ClearSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.model.transform.position;
		diggingMission = f_diggingMission;
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this);
        if (!worksitesList.Contains(this)) worksitesList.Add(this);
        if (!subscribedToUpdate)
        {
            GameMaster.realMaster.labourUpdateEvent += WorkUpdate;
            subscribedToUpdate = true;
        }
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
            if (observer.observingWorksite == this)
            {
                observer.SelfShutOff();
            }
            showOnGUI = false;
        }
    }

    #region save-load mission
    override protected WorksiteSerializer Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
		WorksiteSerializer ws = GetWorksiteSerializer();
		ws.type = WorksiteType.CleanSite;
		ws.workObjectPos = workObject.pos;
		if (diggingMission) ws.specificData = new byte[1]{1};
		else ws.specificData = new byte[1]{0};
		return ws;
	}
	override protected void Load(WorksiteSerializer ws) {
		LoadWorksiteData(ws);
		Set(GameMaster.mainChunk.GetBlock(ws.workObjectPos) as SurfaceBlock, ws.specificData[0] == 1);
	}
	#endregion
			
}
