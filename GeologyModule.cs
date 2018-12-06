using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeologyModule : MonoBehaviour {
	public readonly float metalK_abundance, metalM_abundance, metalE_abundance, 
	metalN_abundance, metalP_abundance, metalS_abundance,
	mineralF_abundance, mineralL_abundance; // sum must be less than one!

	public GeologyModule() {
		metalK_abundance = 0.01f;
		metalM_abundance = 0.005f; metalE_abundance = 0.003f;
		metalN_abundance = 0.001f; metalP_abundance = 0.02f; metalS_abundance = 0.0045f;
		mineralF_abundance = 0.02f; mineralL_abundance = 0.02f; // sum must be less than one!
	}

	public void SpreadMinerals(SurfaceBlock surface) {
		if (surface == null) return;
		int maxObjectsCount = SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION / 2, positionsLeft;
		List<PixelPosByte> positions = surface.GetRandomCells(maxObjectsCount);
		if (positions.Count == 0) return;
		maxObjectsCount = positions.Count; 
		positionsLeft = maxObjectsCount;
		List<HarvestableResource> allBoulders = new List<HarvestableResource>();

		int containersCount = 0;
		if (Random.value < metalK_abundance * 10) {
			containersCount= (int)(maxObjectsCount * GameMaster.geologyModule.metalK_abundance * Random.value);
			if (containersCount != 0) {
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.metal_K_ore, 1+ Random.value * 100 * metalK_abundance);
					allBoulders.Add(hr);
					positionsLeft--; i++;
				}
			}
		}
		if (Random.value < metalM_abundance * 10 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.metalM_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.metal_M_ore, 1+ Random.value * 100 * metalM_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (Random.value < metalE_abundance * 10 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.metalE_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.metal_E_ore, 1+ Random.value * 100 * metalE_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (Random.value < metalN_abundance * 10 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.metalN_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr =Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.metal_N_ore, 1+ Random.value * 100 * metalN_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (Random.value < metalP_abundance * 10 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.metalP_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.metal_P_ore, 1+ Random.value * 100 * metalP_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (Random.value < metalS_abundance * 10 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.metalS_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr =Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.metal_S_ore, 1+ Random.value * 100 * metalS_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (Random.value < mineralF_abundance * 4 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.mineralF_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.mineral_F, 1+ Random.value * 100 * mineralF_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (Random.value < mineralL_abundance * 4 && positionsLeft > 0) {
			containersCount = (int)(maxObjectsCount * GameMaster.geologyModule.mineralL_abundance * Random.value);
			if (containersCount != 0) {						
				int i =0;
				while ( i < containersCount && positionsLeft > 0) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.mineral_L, 1+ Random.value * 100 * mineralL_abundance);
					allBoulders.Add(hr); positionsLeft--; i++;
				}
			}
		}
		if (allBoulders.Count > 0) {
			for ( int n = 0; n < allBoulders.Count; n++) {
				allBoulders[n].SetBasement(surface, positions[n]);
			}
		}
		maxObjectsCount = SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION - surface.surfaceObjects.Count;
        maxObjectsCount /= 2;
        maxObjectsCount = (int)(maxObjectsCount * Random.value);
		if (maxObjectsCount > 0) {
			int count = 0;
			if (surface.material_id == ResourceType.STONE_ID) count = (int)(maxObjectsCount * (0.05f + Random.value * 0.05f)); else count = (int)(Random.value * 0.08f);
			List<PixelPosByte> points = surface.GetRandomCells(count);
			if (points.Count > 0) {
				foreach (PixelPosByte p in points) {
					HarvestableResource hr = Structure.GetStructureByID(Structure.CONTAINER_ID) as HarvestableResource;
					hr.SetResources(ResourceType.Stone, 4 + Random.value * 10);
					hr.SetBasement(surface, p);
				}
			}
		}
	}

	public void CalculateOutput(float production, CubeBlock workObject, Storage storage) {
        if (workObject == null) return;
        ColonyController colony = GameMaster.realMaster.colonyController;
        if (workObject.naturalFossils > 0) {            
			float v = Random.value - GameMaster.LUCK_COEFFICIENT; 
			float m = 0;
			switch (workObject.material_id) {
			case ResourceType.STONE_ID :
				if (metalK_abundance >= v) {
					m= metalK_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.metal_K_ore, m); production -= m;
				}
				if (metalM_abundance >= v) {
					m= metalM_abundance * production * (Random.value +1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.metal_M_ore, m); production -= m;
				}
				if (metalE_abundance >= v) {
					m= metalE_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.metal_E_ore, m); production -= m;
				}
				if (metalN_abundance >= v) {
					m= metalN_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.metal_N_ore, m); production -= m;
				}
				if (metalP_abundance >= v) {
					m= metalP_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.metal_P_ore, m); production -= m;
				}
				if (metalS_abundance >= v) {
					m= metalS_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.metal_S_ore, m); production -= m;
				}
				if (mineralF_abundance >= v) {
					m= mineralF_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.mineral_F, m); production -= m;
				}
				if (mineralL_abundance >= v) {
					m= mineralL_abundance * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
					storage.AddResource(ResourceType.mineral_L, m); production -= m;
				}
				if (production > 0) {
					colony.storage.AddResource(ResourceType.Stone, production); 
				}
				break;
			case ResourceType.DIRT_ID:
				if (metalK_abundance >= v) {
					m= metalK_abundance/2f * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
                        colony.storage.AddResource(ResourceType.metal_K_ore, m); production -= m;
				}
				if (metalP_abundance >= v) {
					m= metalP_abundance/2f * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
                        colony.storage.AddResource(ResourceType.metal_P, m); production -= m;
				}
				if (mineralL_abundance >= v) {
					m= mineralL_abundance/2f * production * (Random.value + 1 + GameMaster.LUCK_COEFFICIENT);
                        colony.storage.AddResource(ResourceType.mineral_L, m); production -= m;
				}
				if (production > 0) {
                        colony.storage.AddResource(ResourceType.Dirt, production); 
				}
				break;
			default:
                    colony.storage.AddResource(ResourceType.GetResourceTypeById(workObject.material_id), production); 
				break;
			}
			workObject.naturalFossils -= production;
		}
		else { // no fossils
            colony.storage.AddResource(ResourceType.GetResourceTypeById(workObject.material_id), production); 
		}
	}
}
