using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour {
	public float totalVolume = 0, maxVolume;
	List<ResourceContainer> containers;
	List<StorageHouse> warehouses;
	public bool showStorage = false;

	void Awake() {
		totalVolume = 0;
		maxVolume = 0;
		containers = new List<ResourceContainer>();
		warehouses = new List<StorageHouse>();
	}

	public void AddWarehouse(StorageHouse sh) {
		if (sh == null) return;
		warehouses.Add(sh);
		RecalculateStorageVolume();
	}
	public void RemoveWarehouse(StorageHouse sh) {
		if (sh == null) return;
		int i = 0;
		while ( i < warehouses.Count) {
			if (warehouses[i] == sh) {warehouses.RemoveAt(i); break;}
			else i ++;
		}
		RecalculateStorageVolume();
	}
	public void RecalculateStorageVolume() {
		totalVolume = 0;
		if (warehouses.Count == 0) return;
		int i = 0;
		while ( i < warehouses.Count ) {
			if ( warehouses[i] == null) {warehouses.RemoveAt(i); continue;}
			else {
				if (warehouses[i].isActive)	totalVolume += warehouses[i].volume;
				i++;
			}
		}
	}

	public float AddResources(ResourceType rtype, float count) {
		if (totalVolume == maxVolume) return 0;
		float freeSpace = maxVolume - totalVolume;
		bool myTypeFound = false;
		int i =0;
		for (; i < containers.Count; i++) {
			if (containers[i].type != rtype)  continue;
			else myTypeFound = true;
			if (count > freeSpace) {
				containers[i].Add(freeSpace);
				count -= freeSpace;
				totalVolume = maxVolume;
			}
			else {
				containers[i].Add(count);
				totalVolume += count;
				count = 0;
			}
		}
		if ( !myTypeFound ) {
			if (count > freeSpace) {containers.Add(new ResourceContainer(rtype, freeSpace));totalVolume = maxVolume; count -= freeSpace;}
			else {containers.Add(new ResourceContainer(rtype, count)); totalVolume+= count; count = 0;}
		}
		return count;
	}

	public float GetResources(ResourceType rtype, float count) {
		if (totalVolume == 0) return 0;
		int i = 0;
		float gainedCount = 0;
		while ( i < containers.Count) {
			if (containers[i].volume == 0) {containers.RemoveAt(i); continue;}
			if (containers[i].type != rtype) {i++;continue;}
			gainedCount = containers[i].Get(count);
			totalVolume -= gainedCount;
			if (containers[i].volume == 0) containers.RemoveAt(i);
			break;
		}
		return gainedCount;
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
			GUI.Label(r_name, "Total:"); GUI.Label(r_count, ((int)totalVolume).ToString() + " / " + ((int)maxVolume).ToString(), GameMaster.mainGUISkin.customStyles[0]);
		}
	}
}
