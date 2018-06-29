using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GatherSiteSerializer {
	public float destructionTimer;
	public int bufer_resourceID; public float bufer_volume;
}

public class GatherSite : Worksite {
	float destructionTimer;
	SurfaceBlock workObject;
	const int START_WORKERS_COUNT = 5;
	ResourceContainer bufer = ResourceContainer.Empty;

	void Awake() {
		workersCount = 0;
		destructionTimer = GameMaster.LABOUR_TICK * 10;
	}

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null || workObject.surfaceObjects.Count == 0) {
			Destroy(this);
		}
		if (workersCount  > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if ( labourTimer <= 0 ) {
				if (workflow >= 1) {
					if (bufer.Equals( ResourceContainer.Empty)) {
						LabourResult();
						labourTimer = GameMaster.LABOUR_TICK;
					}
					else {
						Storage s = GameMaster.colonyController.storage;
						if (s.maxVolume -  s.totalVolume > bufer.volume) {
							s.AddResource(bufer);
							bufer = ResourceContainer.Empty;
						}
						destructionTimer = GameMaster.LABOUR_TICK * 10;
					}
				}
			}
		}
			
		destructionTimer -= Time.deltaTime * GameMaster.gameSpeed; 
		if (destructionTimer <=0) Destroy(this);
	}

void LabourResult() {
	int i = 0;
	bool resourcesFound = false;
	List<Structure> strs = workObject.surfaceObjects;
	while (i < strs.Count & bufer.Equals(ResourceContainer.Empty)) {
		if (strs[i]== null) { workObject.RequestAnnihilationAtIndex(i); continue;}
			if ( strs[i].id == Structure.PLANT_ID) {
				resourcesFound = true;
				Plant p = strs[i] as Plant;
				p.Harvest();
			}
			else {
				HarvestableResource hr = workObject.surfaceObjects[i].GetComponent<HarvestableResource>();
				if (hr == null) {i++; continue;}
				else {
					resourcesFound = true;
					if (workflow > hr.count1) {
						GameMaster.colonyController.storage.AddResource(hr.mainResource, hr.count1);
						workflow -= hr.count1;
						Destroy(hr.gameObject);
						break;
					}
					else {
						GameMaster.colonyController.storage.AddResource(hr.mainResource, hr.count1);
						hr.count1 -= Mathf.FloorToInt(workflow); workflow = 0;
						break;
					}
				}
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
			Destroy(this);
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
		bufer = new ResourceContainer(ResourceType.GetResourceTypeById(gss.bufer_resourceID), gss.bufer_volume);
	}

	protected GatherSiteSerializer GetGatherSiteSerializer() {
		GatherSiteSerializer gss = new GatherSiteSerializer();
		gss.destructionTimer = destructionTimer;
		if ( !bufer.Equals(ResourceContainer.Empty) ) {
			gss.bufer_resourceID = bufer.type.ID;
			gss.bufer_volume = bufer.volume;
		}
		else {
			gss. bufer_resourceID = 0;
		}
		return gss;
	}
	#endregion
}
