using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorageSerializer {
	public float[] standartResources;
}

public class Storage : MonoBehaviour {
	public float totalVolume = 0, maxVolume;
	List<ResourceContainer> customResources;
	List<StorageHouse> warehouses;
	public bool showStorage = false;
	public float[] standartResources{get;private set;}
	Rect myRect;
	const float MIN_VALUE_TO_SHOW = 0.001f;

	void Awake() {
		totalVolume = 0;
		maxVolume = 0;
		standartResources = new float[ResourceType.resourceTypesArray.Length];
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
	public float AddResource(ResourceType rtype, float count) {
		if (totalVolume == maxVolume || count == 0) return count;
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
						customResources[i] = new ResourceContainer(rtype, customResources[i].volume + loadedCount);
					}
				}
				if ( !myTypeFound ) {
					customResources.Add( new ResourceContainer(rtype, loadedCount) );
				}
			}
		}
		else 	standartResources[ rtype.ID ] += loadedCount;
		totalVolume += loadedCount;
		return (count - loadedCount);
	}
	/// <summary>
	/// Attention: container will be destroyed after resources transfer!
	/// </summary>
	/// <param name="rc">Rc.</param>
	public void AddResource(ResourceContainer rc) {
		AddResource(rc.type, rc.volume);
	}
	public void AddResources(List<ResourceContainer> resourcesList) {
		AddResources(resourcesList.ToArray());
	}
	public void AddResources(ResourceContainer[] resourcesList) {
		float freeSpace = maxVolume - totalVolume;
		if (freeSpace == 0) return;
		int idsCount = ResourceType.resourceTypesArray.Length;
		int i =0;
		while ( i < resourcesList.Length & freeSpace > 0) {
			float appliableVolume = resourcesList[i].volume;
			int id = resourcesList[i].type.ID;
			if (appliableVolume > freeSpace) appliableVolume = freeSpace;
			if (id > 0 & id < idsCount) {
				standartResources[id] += appliableVolume;
				freeSpace -= appliableVolume;
			}
			else {
				if (customResources != null) {
					bool found = false;
					for (int j = 0; j < customResources.Count; j++) {
						if (customResources[j].type.ID == id) {
							found = true;
							customResources[j] = new ResourceContainer(customResources[j].type, customResources[j].volume + appliableVolume);
							freeSpace -= appliableVolume;
							break;
						}
						if ( !found ) {
							customResources.Add(new ResourceContainer(resourcesList[i].type, appliableVolume));
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
		if (GameMaster.realMaster.weNeedNoResources) return count;
		if (totalVolume == 0 ) return 0;
		float gainedCount = 0;
		if (rtype.ID < 0 || rtype.ID > standartResources.Length) { // custom resource
			if ( customResources != null )  {
				for (int i = 0; i < customResources.Count; i++) {
					if (customResources[i].type == rtype) {
						if (customResources[i].volume <= count) {
							gainedCount = count; 
							customResources.RemoveAt(i);
							if (customResources.Count == 0) customResources = null;
						}						
						else {
							customResources[i] = new ResourceContainer(customResources[i].type, customResources[i].volume - count);
							gainedCount = count;
						}	
						break;
					}
				}
			}
		}
		else { //standart resource
			if (standartResources[rtype.ID] > count) {
				gainedCount = count;
				standartResources[rtype.ID] -= count;
			}
			else {
				gainedCount = standartResources[rtype.ID];
				standartResources[rtype.ID] = 0;
			}
		}
		return gainedCount;
	}

	/// <summary>
	/// standart resources only
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="val">Value.</param>

	public bool CheckSpendPossibility (ResourceContainer[] cost) {
		if (GameMaster.realMaster.weNeedNoResources) return true;
		if (cost == null || cost.Length == 0) return true;
		foreach (ResourceContainer rc in cost) {
			if (standartResources[rc.type.ID] < rc.volume) return false;
		}
		return true;
	}

	public bool CheckBuildPossibilityAndCollectIfPossible (ResourceContainer[] resourcesContain) {
		//TEST ZONE
		if (GameMaster.realMaster.weNeedNoResources) return true;
		//-----
		if (resourcesContain == null || resourcesContain.Length == 0) return true;

		List<int> customResourcesIndexes = new List<int>();
		foreach (ResourceContainer rc in resourcesContain ) {
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
		foreach (ResourceContainer rc in resourcesContain ) {
			if (rc.type.ID < 0 || rc.type.ID > standartResources.Length) { // custom resource
				int n = customResourcesIndexes[j];

				if (customResources[n].volume <= rc.volume) {
					customResources.RemoveAt(n);
					if (customResources.Count == 0) customResources = null;
				}
				else {
					customResources[n] = new ResourceContainer(customResources[n].type, customResources[n].volume - rc.volume);
				}
				j++;
			}
			else 	standartResources[rc.type.ID] -= rc.volume;
		}
		return true;
	}
	#region save-load system
	public StorageSerializer Save() {
		StorageSerializer ss = new StorageSerializer();
		ss.standartResources = standartResources;
		return ss;
	}
	public void Load(StorageSerializer ss) {
		standartResources = ss.standartResources;
	}
	#endregion
	void OLDOnGUI () {
		if (showStorage) {
			GUI.skin = GameMaster.mainGUISkin;
			float k = GameMaster.guiPiece;
			int positionsCount = 0;
			foreach ( float f in standartResources ) {
				if ( f > MIN_VALUE_TO_SHOW) positionsCount++;
			}
			positionsCount ++; // для total
			if (customResources != null) positionsCount += customResources.Count;

			myRect =new Rect(0,0,0,0);

			GUI.Box(myRect, GUIContent.none);
			Rect r_image = new Rect(myRect.x, myRect.y, k *0.75f, k*0.75f);
			Rect r_name = new Rect (r_image.x + r_image.width, myRect.y, myRect.width * 0.7f, r_image.height);
			Rect r_count = new Rect(myRect.x + r_name.width * 0.5f, myRect.y, myRect.width * 0.5f, r_name.height);
			int i = 0;
			if ( positionsCount != 0) {				
				for (; i < standartResources.Length; i++) {
					if (standartResources[i] > 0.001f) {
						ResourceType rt =  ResourceType.resourceTypesArray[i];
						GUI.DrawTexture(r_image, rt.icon, ScaleMode.ScaleToFit); r_image.y += r_image.height;
						GUI.Label(r_name, rt.name); r_name.y += r_image.height;
						if (standartResources[i] < 1000) GUI.Label(r_count, string.Format("{0:0.###}", standartResources[i]), PoolMaster.GUIStyle_RightOrientedLabel);
						else GUI.Label(r_count, string.Format("{0:0.#}", standartResources[i]), PoolMaster.GUIStyle_RightOrientedLabel);
						r_count.y += r_image.height;
					}
				}
			}
			if (customResources != null) {
				i = 0;
				for (; i < customResources.Count; i++) {
					if (customResources[i].volume > 0.001f) {
						GUI.DrawTexture(r_image, customResources[i].type.icon, ScaleMode.ScaleToFit); r_image.y += r_image.height;
						GUI.Label(r_name, customResources[i].type.name); r_name.y += r_image.height;
						GUI.Label(r_count, string.Format("{0:0.###}", customResources[i]), PoolMaster.GUIStyle_RightOrientedLabel);
						r_count.y += r_image.height;
					}
				}
			}
			GUI.Label(r_name, "Total:"); GUI.Label(r_count, ((int)totalVolume).ToString() + " / " + ((int)maxVolume).ToString(), PoolMaster.GUIStyle_RightOrientedLabel);
		}
	}
}
