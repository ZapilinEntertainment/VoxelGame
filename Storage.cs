using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour {
	public float totalVolume = 0, maxVolume;
	List<ResourceContainer> customResources;
	List<StorageHouse> warehouses;
	public bool showStorage = false;
	public float[] standartResources;
	bool[] resourceInStock;
	int acceptableStandartTypesCount = 0;
	Rect myRect;

	void Awake() {
		totalVolume = 0;
		maxVolume = 0;
		standartResources = new float[ResourceType.resourceTypesArray.Length];
		resourceInStock = new bool[standartResources.Length];
		for (int i = 0; i < standartResources.Length; i++) {
			standartResources[i] = 0;
			resourceInStock[i] = false;
		}
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
		maxVolume = 0;
		if (warehouses.Count == 0) return;
		int i = 0;
		while ( i < warehouses.Count ) {
			if ( warehouses[i] == null) {warehouses.RemoveAt(i); continue;}
			else {
				if (warehouses[i].isActive)	maxVolume += warehouses[i].volume;
				i++;
			}
		}
	}
	/// <summary>
	/// returns the residual of sended value
	/// </summary>
	/// <returns>The resources.</returns>
	/// <param name="rtype">Rtype.</param>
	/// <param name="count">Count.</param>
	public float AddResources(ResourceType rtype, float count) {
		if (totalVolume == maxVolume || count == 0) return 0;
		float loadedCount = count;
		if (maxVolume - totalVolume < loadedCount) loadedCount = maxVolume - totalVolume;
		if (rtype.ID < 0 || rtype.ID >= standartResources.Length) { // custom resources
			if (customResources == null) {
				customResources = new List<ResourceContainer>();
				customResources.Add(new ResourceContainer (rtype, loadedCount));
			}
			else {
				bool myTypeFound = false;
				for (int i = 0 ; i < customResources.Count; i++ ) {
					if (customResources[i].type == rtype) {
						myTypeFound = true;
						customResources[i].Add(loadedCount);
					}
				}
				if ( !myTypeFound ) {
					customResources.Add( new ResourceContainer(rtype, loadedCount) );
				}
			}
		}
		else { //standart resources
			standartResources[rtype.ID] += loadedCount;
			if (resourceInStock[rtype.ID] == false ) {resourceInStock[rtype.ID] = true;acceptableStandartTypesCount++;}
		}
		totalVolume += loadedCount;
		return (count - loadedCount);
	}
	/// <summary>
	/// Attention: container will be destroyed after resources transfer!
	/// </summary>
	/// <param name="rc">Rc.</param>
	public void AddResources(ResourceContainer rc) {
		AddResources(rc.type, rc.volume);
	}
	public void AddResources(List<ResourceContainer> resourcesList) {
		float freeSpace = maxVolume - totalVolume;
		if (freeSpace == 0) return;
		int idsCount = ResourceType.resourceTypesArray.Length;
		int i =0;
		while ( i < resourcesList.Count && freeSpace > 0) {
			float appliableVolume = resourcesList[i].volume;
			int id = resourcesList[i].type.ID;
			if (appliableVolume > freeSpace) appliableVolume = freeSpace;
			if (id > 0 && id < idsCount) {
				standartResources[id] += appliableVolume;
				if (resourceInStock[id] == false) {resourceInStock[id] = true; acceptableStandartTypesCount++;}
				freeSpace -= appliableVolume;
			}
			else {
				if (customResources != null) {
					bool found = false;
					foreach (ResourceContainer customRc in customResources) {
						if (customRc.type.ID == id) {
							found = true;
							customRc.Add (appliableVolume);
							freeSpace -= appliableVolume;
							break;
						}
						if ( !found ) {
							customResources.Add(customRc);
							freeSpace -= appliableVolume;
						} 
					}
				}
			}
			i++;
		}
		totalVolume = maxVolume - freeSpace;
	}

	public float GetResources(ResourceType rtype, float count) {
		if (totalVolume == 0) return 0;
		float gainedCount = 0;
		if (rtype.ID < 0 || rtype.ID > standartResources.Length) { // custom resource
			if ( customResources != null )  {
				for (int i = 0; i < customResources.Count; i++) {
					if (customResources[i].type == rtype) {
						gainedCount = customResources[i].Get(count);
						if (customResources[i].volume == 0) {
							customResources.RemoveAt(i);
							if (customResources.Count == 0) customResources = null;
						}
						break;
					}
				}
			}
		}
		else { //standart resource
			if (standartResources[rtype.ID] >= count) {
				gainedCount = count;
				standartResources[rtype.ID] -= count;
			}
			else {
				gainedCount = standartResources[rtype.ID];
				standartResources[rtype.ID] = 0;
				if (resourceInStock[rtype.ID] == true) {resourceInStock[rtype.ID] = false; acceptableStandartTypesCount--; }
			}
		}
		return gainedCount;
	}

	public bool CheckBuildPossibilityAndCollectIfPossible (Building b) {
		//TEST ZONE
		if (GameMaster.realMaster.weNeedNoResources) return true;
		//-----
		if (b.resourcesContain == null || b.resourcesContain.Count == 0) return true;

		List<int> customResourcesIndexes = new List<int>();
		foreach (ResourceContainer rc in b.resourcesContain ) {
			if (rc.type.ID < 0 || rc.type.ID > standartResources.Length) { // custom resources
				if (customResources == null) return false;
				else {
					bool resourceFound = false;
					for ( int i = 0; i < customResources.Count; i++) {
						if (customResources[i].type == rc.type) {
							if (customResources[i].volume >= rc.volume) {
								customResourcesIndexes.Add(i);
							}
							else return false;
						}
					}
					if ( !resourceFound ) return false;
				}
			}
			else { // standart resources
				if (standartResources[rc.type.ID] < rc.volume ) return false;
			}
		}
		// getting resources:
		int j = 0;
		foreach (ResourceContainer rc in b.resourcesContain ) {
			if (rc.type.ID < 0 || rc.type.ID > standartResources.Length) { // custom resource
				customResources[customResourcesIndexes[j]].Get(rc.volume);
				if (customResources[customResourcesIndexes[j]].volume == 0 ) {
					customResources.RemoveAt(customResourcesIndexes[j]);
					if (customResources.Count == 0) customResources = null;
				}
				j++;
			}
			else {
				standartResources[rc.type.ID] -= rc.volume;
				if (standartResources[rc.type.ID] == 0) {
					if (resourceInStock[rc.type.ID] == true) {
						resourceInStock[rc.type.ID] = false; acceptableStandartTypesCount--;
					}
				}
			}
		}
		return true;
	}

	void OnGUI () {
		if (showStorage) {
			GUI.skin = GameMaster.mainGUISkin;
			float k = GameMaster.guiPiece;
			int positionsCount = acceptableStandartTypesCount + 1;
			if (customResources != null) positionsCount += customResources.Count;

			if (UI.current.mode != UIMode.View) myRect =new Rect(Screen.width - 16 *k, UI.current.upPanelBox.height, 8*k , k * 0.75f * positionsCount);
			else myRect =new Rect(Screen.width - 12 *k, UI.current.upPanelBox.height, 8*k , k * 0.75f * positionsCount);

			GUI.Box(myRect, GUIContent.none);
			Rect r_image = new Rect(myRect.x, myRect.y, k *0.75f, k*0.75f);
			Rect r_name = new Rect (r_image.x + r_image.width, myRect.y, myRect.width * 0.7f, r_image.height);
			Rect r_count = new Rect(myRect.x + r_name.width * 0.5f, myRect.y, myRect.width * 0.5f, r_name.height);
			int i = 0;
			if (acceptableStandartTypesCount != 0) {				
				for (; i < standartResources.Length; i++) {
					if (standartResources[i] > 0.01f) {
						ResourceType rt =  ResourceType.resourceTypesArray[i];
						GUI.DrawTexture(r_image, rt.icon, ScaleMode.ScaleToFit); r_image.y += r_image.height;
						GUI.Label(r_name, rt.name); r_name.y += r_image.height;
						GUI.Label(r_count, ((int)(standartResources[i] * 100) / 100f).ToString(), GameMaster.mainGUISkin.customStyles[(int)GUIStyles.RightOrientedLabel]);
						r_count.y += r_image.height;
					}
				}
			}
			if (customResources != null) {
				i = 0;
				for (; i < customResources.Count; i++) {
					if (customResources[i].volume > 0.01f) {
						GUI.DrawTexture(r_image, customResources[i].type.icon, ScaleMode.ScaleToFit); r_image.y += r_image.height;
						GUI.Label(r_name, customResources[i].type.name); r_name.y += r_image.height;
						GUI.Label(r_count, ((int)(customResources[i].volume * 100) / 100f).ToString(), GameMaster.mainGUISkin.customStyles[(int)GUIStyles.RightOrientedLabel]);
						r_count.y += r_image.height;
					}
				}
			}
			GUI.Label(r_name, "Total:"); GUI.Label(r_count, ((int)totalVolume).ToString() + " / " + ((int)maxVolume).ToString(), GameMaster.mainGUISkin.customStyles[0]);
		}
	}
}
