using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResourceContainer {
	public ResourceType type;
	public float volume;

	public ResourceContainer(ResourceType f_type, float f_volume) {
		type= f_type; volume = f_volume;
	}
}

public class Storage : Structure {
	float volume = 0, maxVolume;
	List<ResourceContainer> containers;

	void Awake() {
		maxVolume = SurfaceBlock.INNER_RESOLUTION *SurfaceBlock.INNER_RESOLUTION *SurfaceBlock.INNER_RESOLUTION ;
		containers = new List<ResourceContainer>();
		isMainStructure = true;
		xsize_to_set = SurfaceBlock.INNER_RESOLUTION;
		zsize_to_set = SurfaceBlock.INNER_RESOLUTION;
	}

	public float AddResources(ResourceType rtype, float count) {
		if (volume == maxVolume) return 0;
		float freeSpace = maxVolume - volume;
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

	public float GetResources(ResourceType rtype, float count) {
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
