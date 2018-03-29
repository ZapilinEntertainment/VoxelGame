using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Structure {
	public bool isActive {get;protected set;}
	public bool energySupplied {get;protected set;} // подключение, контролирующееся Colony Controller'ом
	public byte level = 0;
	public List<ResourceContainer> resourcesContain;
	public string resourcesContainSet = "";
	public float energySurplus = 0, energyCapacity = 0;
	protected bool connectedToPowerGrid = false; // подключение, контролирующееся игроком

	void Awake() {
		PrepareBuilding();
	}
	protected void PrepareBuilding() {
		PrepareStructure();
		isActive = false;
		energySupplied = false;
		if (resourcesContainSet.Length != 0) {
			resourcesContain = new List<ResourceContainer>();
			int x0 = 0;
			int x = resourcesContainSet.IndexOf(':', 0);
			int y0 = x + 1;
			int y = resourcesContainSet.IndexOf(';', 0);
			int length = resourcesContainSet.Length;
			while (x > 0 && y > 0) {
				ResourceType rt = ResourceType.Nothing; int count = 0;
				switch (resourcesContainSet.Substring(x0, x - x0)) {
				case "Lumber": rt = ResourceType.Lumber; break;
				case "Stone": rt = ResourceType.Stone; break;
				case "Dirt": rt = ResourceType.Dirt; break;
				case "Supplies": rt = ResourceType.Food; break;
				case "metalK_ore": rt = ResourceType.metal_K_ore; break;
				case "metalK": rt = ResourceType.metal_K; break;
				case "metalM_ore": rt = ResourceType.metal_M_ore; break;
				case "metalM": rt = ResourceType.metal_M; break;
				case "metalE_ore": rt = ResourceType.metal_E_ore; break;
				case "metalE": rt = ResourceType.metal_E; break;
				case "metalN_ore": rt = ResourceType.metal_N_ore; break;
				case "metalN": rt = ResourceType.metal_N; break;
				case "metalP_ore": rt = ResourceType.metal_P_ore; break;
				case "metalP": rt = ResourceType.metal_P; break;
				case "metalS_ore": rt = ResourceType.metal_S_ore; break;
				case "metalS": rt = ResourceType.metal_S; break;
				case "mineralF": rt = ResourceType.mineral_F; break;
				case "mineralL": rt = ResourceType.mineral_L; break;
				case "elasticMass": rt = ResourceType.ElasticMass; break;
				}
				count = int.Parse(resourcesContainSet.Substring(y0, y - y0 )); 
				resourcesContain.Add(new ResourceContainer(rt, count));

				x0 = y+1; 
				if ( y + 1 >= length) break;
				x = resourcesContainSet.IndexOf(':', y+1);	y0 =x + 1; 
				y = resourcesContainSet.IndexOf(';', y+1);
			}
		}
		resourcesContainSet = null;
	}

	override public void SetBasement(SurfaceBlock b, PixelPosByte pos) {
		if (b == null) return;
		SetBuildingData(b, pos);
	}

	protected void SetBuildingData(SurfaceBlock b, PixelPosByte pos) {
		SetStructureData(b,pos);
		isActive = true;
		if (energySurplus != 0 || energyCapacity >  0) {
			GameMaster.colonyController.AddToPowerGrid(this);
			connectedToPowerGrid = true;
		}
	}

	virtual public void SetActivationStatus(bool x) {
		isActive = x;
	}
	public void SetEnergySupply(bool x) {
		energySupplied = x;
	}

	public void DisconnectFromPowerGrid() {
		
	}

	protected void PrepareBuildingForDestruction() {
		if (basement != null) {
			basement.RemoveStructure(new SurfaceObject(innerPosition, this));
			basement.artificialStructures --;
		}
		if (connectedToPowerGrid) GameMaster.colonyController.DisconnectFromPowerGrid(this);
	}

	public void Demolish() {
		if (resourcesContain != null && resourcesContain.Count != 0 && GameMaster.demolitionLossesPercent != 1) {
			foreach (ResourceContainer rc in resourcesContain) {
				rc.Get(GameMaster.demolitionLossesPercent * rc.volume);
				GameMaster.colonyController.storage.AddResources(rc);
			}
		}
		Destroy(gameObject);
	}
	void OnDestroy() {
		PrepareBuildingForDestruction();
	}
}
