using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResourceType {
	public float mass , toughness;
	public bool isBuildingMaterial;
	public static ResourceType Nothing, Lumber, Stone, Dirt,Food;

	public ResourceType(float f_mass, float f_toughness, bool useAsBuildingMaterial) {
		mass = f_mass;
		toughness = f_toughness;
		isBuildingMaterial = useAsBuildingMaterial;
	}

	static ResourceType() {
		Nothing = new ResourceType(0, 0,  false);
		Lumber = new ResourceType(0.5f, 5, true);
		Stone = new ResourceType(2.5f, 30,  true);
		Dirt = new ResourceType (1, 1, false);
		Food = new ResourceType(0.1f, 0.1f, false);
	}

	public static bool operator ==(ResourceType lhs, ResourceType rhs) {return lhs.Equals(rhs);}
	public static bool operator !=(ResourceType lhs, ResourceType rhs) {return !(lhs.Equals(rhs));}
}

public struct ResourceContainer {
	public ResourceType type;
	public int volume;

	public ResourceContainer(ResourceType f_type, int f_volume) {
		type= f_type; volume = f_volume;
	}
}

public class Storage : Structure {
	int volume = 0, maxVolume;
	List<ResourceContainer> containers;

	void Awake() {
		maxVolume = SurfaceBlock.INNER_RESOLUTION *SurfaceBlock.INNER_RESOLUTION *SurfaceBlock.INNER_RESOLUTION ;
		containers = new List<ResourceContainer>();
		isMainStructure = true;
		xsize_to_set = SurfaceBlock.INNER_RESOLUTION;
		zsize_to_set = SurfaceBlock.INNER_RESOLUTION;
	}

	public int AddResources(ResourceType rtype, int count) {
		if (volume == maxVolume) return 0;
		int freeSpace = maxVolume - volume;
		bool myTypeFound = false;
		int i =0;
		for (; i < containers.Count; i++) {
			if (containers[i].type != rtype)  continue;
			else myTypeFound = true;
			if (count > freeSpace) {
				containers[i] = new ResourceContainer(rtype, containers[i].volume + freeSpace);
				count -= freeSpace;
				volume = maxVolume;
			}
			else {
				containers[i] = new ResourceContainer(rtype, containers[i].volume + count);
				volume += count;
				count = 0;
			}
		}
		if ( !myTypeFound ) {
			if (count > freeSpace) {containers.Add(new ResourceContainer(rtype, freeSpace));volume = maxVolume; count -= freeSpace;}
			else {containers.Add(new ResourceContainer(rtype, count)); volume+= count; count = 0;}
		}
		return count;
	}

	public int GetResources(ResourceType rtype, int count) {
		if (volume == 0) return 0;
		int i = 0;
		while ( i < containers.Count && count > 0) {
			if (containers[i].type != rtype) continue;
			if (containers[i].volume < count) {
				volume -= containers[i].volume;
				count -= containers[i].volume;
				containers.RemoveAt(i);
				continue;
			}
			else {
				containers[i] = new ResourceContainer(rtype, containers[i].volume - count);
				volume -= count;
				count = 0;
				break;
			}
		}
		return count;
	}
}
