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

public class Storage : MonoBehaviour {
	public float volume = 0, maxVolume;
	List<ResourceContainer> containers;
	public bool showStorage = false;

	void Awake() {
		volume = 0;
		maxVolume = 0;
		containers = new List<ResourceContainer>();
	}

	public void AddVolume(float x) {
		if (x > 0) maxVolume += x;
	}
	public void ContractVolume(float x) {
		maxVolume -= x; if (maxVolume < 0) maxVolume = 0;
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
			if (containers[i].type != rtype) {i++;continue;}
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

	void OnGUI () {
		if (showStorage) {
			GUI.skin = GameMaster.mainGUISkin;
			float k = GameMaster.guiPiece;
			Rect r =new Rect(Screen.width - 8 *k, UI.current.upPanelHeight,4*k, k * 0.75f * (containers.Count+1));
			UI.current.serviceBoxRect = r;
			Rect r_name = new Rect (r.x, r.y, r.width * 0.7f, k*0.75f);
			Rect r_count = new Rect(r.x + r_name.width * 0.5f, r.y, r.width * 0.5f, r_name.height);
			if (containers.Count != 0 ) {
				foreach (ResourceContainer rc in containers) {
					GUI.Label(r_name, rc.type.name); 
					GUI.Label(r_count, ((int)rc.volume).ToString(), GameMaster.mainGUISkin.customStyles[0]);
					r_name.y += r_name.height; r_count.y += r_name.height;
				}
			}
			GUI.Label(r_name, "Total:"); GUI.Label(r_count, ((int)volume).ToString() + " / " + ((int)maxVolume).ToString(), GameMaster.mainGUISkin.customStyles[0]);
		}
	}
}
