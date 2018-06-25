using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorkBuildingSerializer {
	public BuildingSerializer buildingSerializer;
	public float workflow, workSpeed, workflowToProcees;
	public int workersCount;
}

public abstract class WorkBuilding : Building {
	public float workflow {get;protected set;} 
	public float workSpeed {get;protected set;}
	public float workflowToProcess{get; protected set;}
	public int maxWorkers = 8; // fixed by asset
	public int workersCount {get; protected set;} 
	const float WORKFLOW_GAIN = 1;
	public float workflowToProcess_setValue = 1;//fixed by asset

	override public void Prepare() {
		PrepareWorkbuilding();
	}
	protected void PrepareWorkbuilding() {
		PrepareBuilding();
		workersCount = 0;
		workflow = 0;
		workflowToProcess = workflowToProcess_setValue;
	}

	void Update() {
		if (GameMaster.gameSpeed == 0 || !isActive || !energySupplied) return;
		if (workersCount > 0) {
			workflow += workSpeed * Time.deltaTime * GameMaster.gameSpeed ;
			if (workflow >= workflowToProcess) {
				LabourResult();
			}
		}
	}

	protected virtual void LabourResult() {
		workflow = 0;
	}

	virtual public int AddWorkers (int x) {
		if (workersCount == maxWorkers) return 0;
		else {
			if (x > maxWorkers - workersCount) {
				x -= (maxWorkers - workersCount);
				workersCount = maxWorkers;
			}
			else {
				workersCount += x;
			}
			RecalculateWorkspeed();
			return x;
		}
	}

	public void FreeWorkers() {
		FreeWorkers(workersCount);
	}
	virtual public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	virtual protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Manufacturing);
	}

	#region save-load system
	override public StructureSerializer Save() {
		StructureSerializer ss = GetStructureSerializer();
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
		{
			new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, GetWorkBuildingSerializer());
			ss.specificData =  stream.ToArray();
		}
		return ss;
	}

	override public void Load(StructureSerializer ss, SurfaceBlock sblock) {
		LoadStructureData(ss, sblock);
		WorkBuildingSerializer wbs = new WorkBuildingSerializer();
		GameMaster.DeserializeByteArray<WorkBuildingSerializer>(ss.specificData, ref wbs);
		LoadWorkBuildingData(wbs);
	}
	protected void LoadWorkBuildingData (WorkBuildingSerializer wbs) {
		LoadBuildingData(wbs.buildingSerializer);
		workersCount = wbs.workersCount;
		workflow = wbs.workflow;
		workSpeed = wbs.workSpeed;
		workflowToProcess = wbs.workflowToProcees;
	}

	public WorkBuildingSerializer GetWorkBuildingSerializer() {
		WorkBuildingSerializer wbs = new WorkBuildingSerializer();
		wbs.buildingSerializer = GetBuildingSerializer();
		wbs.workflow = workflow;
		wbs.workSpeed = workSpeed;
		wbs.workflowToProcees = wbs.workflowToProcees;
		wbs.workersCount = workersCount;
		return wbs;
	}
		
	#endregion

	override protected float GUI_UpgradeButton( Rect rr) {
		GUI.DrawTexture(new Rect( rr.x, rr.y, rr.height, rr.height), PoolMaster.greenArrow_tx, ScaleMode.StretchToFill);
		if ( GUI.Button(new Rect (rr.x + rr.height, rr.y, rr.height * 4, rr.height), "Level up") ) {
			if ( GameMaster.colonyController.storage.CheckBuildPossibilityAndCollectIfPossible( requiredResources ) )
			{
				WorkBuilding upgraded = Structure.GetNewStructure(upgradedIndex) as WorkBuilding;
				PixelPosByte setPos = new PixelPosByte(innerPosition.x, innerPosition.z);
				byte bzero = (byte)0;
				if (upgraded.innerPosition.x_size == 16) setPos = new PixelPosByte(bzero, innerPosition.z);
				if (upgraded.innerPosition.z_size == 16) setPos = new PixelPosByte(setPos.x, bzero);
				int workers = workersCount;
				workersCount = 0;
				Quaternion originalRotation = transform.rotation;
				upgraded.SetBasement(basement, setPos);
				if ( !upgraded.isBasement & upgraded.randomRotation & (upgraded.rotate90only == rotate90only)) {
					upgraded.transform.localRotation = originalRotation;
				}
				upgraded.AddWorkers(workers);
			}
			else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
		}

		if ( requiredResources.Length > 0) {
			Storage storage = GameMaster.colonyController.storage;
			rr.y += rr.height;
			for (int i = 0; i < requiredResources.Length; i++) {
				if (requiredResources[i].volume > storage.standartResources[requiredResources[i].type.ID]) GUI.color = Color.red;
				GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
				GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
				GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), requiredResources[i].volume.ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
				rr.y += rr.height;
				GUI.color = Color.white;
			}
		}
		return rr.y;
	}

	protected void PrepareWorkbuildingForDestruction() {
		PrepareBuildingForDestruction();
		if (workersCount != 0) GameMaster.colonyController.AddWorkers(workersCount);
	}

	void OnDestroy() {
		PrepareWorkbuildingForDestruction();
	}
}
