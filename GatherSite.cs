using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GatherSiteSerializer {
	public float destructionTimer;
}

public class GatherSite : Worksite {
	float destructionTimer;
	SurfaceBlock workObject;
	const int START_WORKERS_COUNT = 5;

	void Awake() {
		workersCount = 0;
		destructionTimer = GameMaster.LABOUR_TICK * 10;
	}

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || workObject.surfaceObjects.Count == 0) {
            StopWork();
        }
		if (workersCount  > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if ( labourTimer <= 0 ) {
				if (workflow >= 1) {
					LabourResult();
					}
				}
			}
			
		destructionTimer -= Time.deltaTime * GameMaster.gameSpeed; 
		if (destructionTimer <=0) StopWork();
    }

void LabourResult() {
	int i = 0;
	bool resourcesFound = false;
	List<Structure> strs = workObject.surfaceObjects;
		while (i < strs.Count &  workflow > 0) {
			if (strs[i] == null ) {
				workObject.RequestAnnihilationAtIndex(i);
				continue;
			}
			else {
				switch (strs[i].id) {
				case Structure.PLANT_ID:
					Plant p = strs[i] as Plant;
					if (p!= null) {
						if (p.stage >= p.harvestableStage) {
							p.Harvest();
							resourcesFound = true;
							workflow --;
						}
					}
					break;
				case Structure.CONTAINER_ID:
					HarvestableResource hr = strs[i] as HarvestableResource;
					if (hr != null) {
						hr.Harvest();
						resourcesFound = true;
						workflow --;
					}
					break;
				case Structure.RESOURCE_STICK_ID:
					ScalableHarvestableResource shr = strs[i] as ScalableHarvestableResource;
					if (shr != null) {
						shr.Harvest();
						resourcesFound = true;
						workflow --;
					}
				break;
				}
				i++;
			}
		}
	if (resourcesFound) destructionTimer = GameMaster.LABOUR_TICK * 10;
}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Gathering);
	}
	public void Set(SurfaceBlock block) {
		workObject = block;
		sign = Instantiate(Resources.Load<GameObject> ("Prefs/GatherSign")).GetComponent<WorksiteSign>();
		sign.worksite = this;
		sign.transform.position = workObject.transform.position + Vector3.down /2f * Block.QUAD_SIZE;
		actionLabel = Localization.ui_gather_in_progress;
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
		GameMaster.colonyController.AddWorksite(this);
	}

	#region save-load system
	override public WorksiteSerializer Save() {
		if (workObject == null) {
            StopWork();
			return null;
		}
		WorksiteSerializer ws = GetWorksiteSerializer();
		ws.type = WorksiteType.GatherSite;
		ws.workObjectPos = workObject.pos;
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetGatherSiteSerializer());
			ws.specificData = stream.ToArray();
		}
		return ws;
	}
	override public void Load(WorksiteSerializer ws) {
		LoadWorksiteData(ws);
		Set(GameMaster.mainChunk.GetBlock(ws.workObjectPos) as SurfaceBlock);
		GatherSiteSerializer gss = new GatherSiteSerializer();
		GameMaster.DeserializeByteArray(ws.specificData, ref gss);
		destructionTimer = gss.destructionTimer;
	}

	protected GatherSiteSerializer GetGatherSiteSerializer() {
		GatherSiteSerializer gss = new GatherSiteSerializer();
		gss.destructionTimer = destructionTimer;
		return gss;
	}
	#endregion
}
