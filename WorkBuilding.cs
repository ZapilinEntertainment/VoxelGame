using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorkBuilding : Building {
	public float workflow {get;protected set;} 
	protected float workSpeed = 0;
	public float workflowToProcess{get; protected set;}
	public int maxWorkers = 8;
	public int workersCount {get; protected set;}
	const float WORKFLOW_GAIN = 1;
	public float workflowToProcess_setValue = 1;

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

	virtual public void FreeWorkers(int x) {
		if (x > workersCount) x = workersCount;
		workersCount -= x;
		GameMaster.colonyController.AddWorkers(x);
		RecalculateWorkspeed();
	}
	virtual protected void RecalculateWorkspeed() {
		workSpeed = GameMaster.CalculateWorkspeed(workersCount, WorkType.Manufacturing);
	}

	//---------------------                   SAVING       SYSTEM-------------------------------
	public override string Save() {
		return SaveStructureData() + SaveBuildingData() + SaveWorkBuildingData();
	}

	protected string SaveWorkBuildingData() {
		string s = "";
		s += string.Format("{0:d3}",(int)(workflow /workflowToProcess * 100));
		s += string.Format("{0:d3}", workersCount );
		return s;
	}

	public override void Load(string s_data, Chunk c, SurfaceBlock surface) {
		byte x = byte.Parse(s_data.Substring(0,2));
		byte z = byte.Parse(s_data.Substring(2,2));
		Prepare();
		SetBasement(surface, new PixelPosByte(x,z));
		//building class part
		workflow = int.Parse(s_data.Substring(12,3)) / 100f;
		AddWorkers(int.Parse(s_data.Substring(15,3)));
		SetActivationStatus(s_data[11] == '1');     
		//--
		transform.localRotation = Quaternion.Euler(0, 45 * int.Parse(s_data[7].ToString()), 0);
		hp = int.Parse(s_data.Substring(8,3)) / 100f * maxHp;
	}
	//---------------------------------------------------------------------------------------------------	

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
				upgraded.transform.localRotation = originalRotation;
				upgraded.AddWorkers(workers);
			}
			else UI.current.ChangeSystemInfoString(Localization.announcement_notEnoughResources);
		}
		if ( requiredResources.Length > 0) {
			rr.y += rr.height;
			for (int i = 0; i < requiredResources.Length; i++) {
				GUI.DrawTexture(new Rect(rr.x, rr.y, rr.height, rr.height), requiredResources[i].type.icon, ScaleMode.StretchToFill);
				GUI.Label(new Rect(rr.x +rr.height, rr.y, rr.height * 5, rr.height), requiredResources[i].type.name);
				GUI.Label(new Rect(rr.xMax - rr.height * 3, rr.y, rr.height * 3, rr.height), (requiredResources[i].volume * (1 - GameMaster.upgradeDiscount)).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
				rr.y += rr.height;
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
