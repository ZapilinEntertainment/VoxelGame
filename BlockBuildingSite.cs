using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingSite : Worksite {
	SurfaceBlock workObject;
	const float buildingSpeed = 1;
	ResourceType rtype;
	const int START_WORKERS_COUNT = 20;

	void Update () {
		if (GameMaster.gameSpeed == 0) return;
		if (workObject ==null) {
			Destroy(this);
		}
		if (workersCount  > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			labourTimer -= Time.deltaTime * GameMaster.gameSpeed;
			if ( labourTimer <= 0 ) {
				LabourResult();
				labourTimer = GameMaster.LABOUR_TICK;
				workflow = 0;
			}
		}
	}

	void LabourResult() {
		float totalResources = 0;
		actionLabel = "";
		bool?[,] pillarsMap = new bool?[8,8];
		for (int a = 0; a < 8; a++) {
			for (int b = 0; b < 8; b++) {
				pillarsMap[a,b] = null;
			}
		}
		int placesToWork = 64;
		List<ScalableHarvestableResource> unfinishedPillars = new List<ScalableHarvestableResource>();

		if (workObject.cellsStatus != 0) {
			int i = 0;
			while ( i < workObject.surfaceObjects.Count ) {
				if (workObject.surfaceObjects[i] == null) {
					workObject.RequestAnnihilationAtIndex(i);
				}
				else {
					ScalableHarvestableResource shr = workObject.surfaceObjects[i] as ScalableHarvestableResource;
					if ( shr == null ) {
						workObject.surfaceObjects[i].Annihilate(false);
					}
					else {
						if (shr.count1 == ScalableHarvestableResource.MAX_VOLUME) {
							placesToWork --;
							pillarsMap[shr.innerPosition.x/2, shr.innerPosition.z/2] = true;
							totalResources += ScalableHarvestableResource.MAX_VOLUME;
						}
						else {
							pillarsMap[shr.innerPosition.x/2, shr.innerPosition.z/2] = false;
							totalResources += shr.count1;
							unfinishedPillars.Add(shr);
						}
					}
				}
				i++;
			}
		}

		if ( placesToWork == 0 ) {
			actionLabel = "Block completed";
			workObject.ClearSurface();
			workObject.myChunk.ReplaceBlock(workObject.pos, BlockType.Cube, rtype.ID, rtype.ID, false);
			Destroy(this);
		}
		else {
			int pos = (int)(placesToWork * Random.value);
			int n = 0;
			for (int a = 0; a < 8; a++) {
				for (int b = 0; b < 8; b++) {
					if ( pillarsMap[a,b] == true ) continue;
					else {
						if ( n == pos ) {
							ScalableHarvestableResource shr = null;
							float count = buildingSpeed * workflow;
							if (pillarsMap[a,b] == false) {
								foreach ( ScalableHarvestableResource fo in unfinishedPillars) {
									if (fo.innerPosition.x/2 == a & fo.innerPosition.z/2 == b) {
										shr = fo;
										break;
									}
								}
								if (count > ScalableHarvestableResource.MAX_VOLUME - shr.count1) count = ScalableHarvestableResource.MAX_VOLUME - shr.count1;
							}
							else {
								shr = Structure.GetNewStructure(Structure.RESOURCE_STICK_ID) as ScalableHarvestableResource;
								shr.SetBasement(workObject, new PixelPosByte(a * 2, b * 2));
							}
							count = GameMaster.colonyController.storage.GetResources(rtype, count);
							if (count == 0) actionLabel = Localization.announcement_notEnoughResources + ' ';
							else 
							{
								totalResources += count;
								shr.AddResource(rtype, count);
							}
							actionLabel += string.Format("{0:0.##}", totalResources / (float)CubeBlock.MAX_VOLUME * 100f)+ '%';	
							return;
						} 
						else 	n++;
					}
				}
			}
		}
	}

	protected override void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Pouring);
	}
	public void Set(SurfaceBlock block, ResourceType type) {
		workObject = block;
		rtype = type;
		actionLabel = Localization.structureName[Structure.RESOURCE_STICK_ID];
		GameMaster.colonyController.SendWorkers(START_WORKERS_COUNT, this, WorkersDestination.ForWorksite);
		GameMaster.colonyController.AddWorksite(this);

		if (sign == null) {
			sign = new GameObject().AddComponent<WorksiteSign>();
			BoxCollider bc =  sign.gameObject.AddComponent<BoxCollider>();
			bc.size = new Vector3(Block.QUAD_SIZE, 0.1f, Block.QUAD_SIZE);
			bc.center = Vector3.up * 0.05f;
			sign.worksite = this;
			sign.transform.position = workObject.transform.position;
		}
	}


	//---------SAVE   SYSTEM----------------
	override public WorksiteBasisSerializer Save() {
		if (workObject == null) {
			Destroy(this);
			return null;
		}
		WorksiteBasisSerializer wbs = new WorksiteBasisSerializer();
		wbs.type = WorksiteType.BlockBuildingSite;
		wbs.workObjectPos = workObject.pos;
		DigSiteSerializer dss = new DigSiteSerializer();
		dss.worksiteSerializer = GetWorksiteSerializer();
		dss.resourceTypeID = rtype.ID;
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, dss);
			wbs.data = stream.ToArray();
		}
		return wbs;
	}


	// --------------------------------------------------------
}
