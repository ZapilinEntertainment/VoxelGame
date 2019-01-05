using System.Collections.Generic;
using UnityEngine;

public sealed class Storage : MonoBehaviour {
	public float totalVolume = 0, maxVolume, announcementTimer;
	public List<StorageHouse> warehouses { get; private set; }
	public bool showStorage = false;
	public float[] standartResources{get;private set;}
	Rect myRect;
	const float MIN_VALUE_TO_SHOW = 0.001f;
    public int operationsDone { get; private set; } // для UI

	void Awake() {
		totalVolume = 0;
		maxVolume = 0;
		standartResources = new float[ResourceType.resourceTypesArray.Length];
		warehouses = new List<StorageHouse>();
    }

    private void Update()
    {
        if (announcementTimer > 0) announcementTimer -= Time.deltaTime;
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
		if (totalVolume >= maxVolume | count == 0) return count;
		float loadedCount = count;
		if (maxVolume - totalVolume < loadedCount) loadedCount = maxVolume - totalVolume;
        if (rtype == ResourceType.FertileSoil) rtype = ResourceType.Dirt;
		standartResources[ rtype.ID ] += loadedCount;
		totalVolume += loadedCount;
        operationsDone++;
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
        if (freeSpace == 0)
        {
            if (announcementTimer <= 0)
            {
                UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.StorageOverloaded));
                announcementTimer = 5;
            }
            return;
        }
		int idsCount = ResourceType.resourceTypesArray.Length;
		int i =0;
		while ( i < resourcesList.Length & freeSpace > 0) {
			float appliableVolume = resourcesList[i].volume;
			int id = resourcesList[i].type.ID;
			if (appliableVolume > freeSpace) appliableVolume = freeSpace;
			standartResources[id] += appliableVolume;
			freeSpace -= appliableVolume;
			i++;
		}
        operationsDone++;
        totalVolume = maxVolume - freeSpace;
	}

	public float GetResources(ResourceType rtype, float count) {
		if (GameMaster.realMaster.weNeedNoResources) return count;
		if (totalVolume == 0 ) return 0;
		float gainedCount = 0;
			if (standartResources[rtype.ID] > count) {
				gainedCount = count;
				standartResources[rtype.ID] -= count;
			}
			else {
				gainedCount = standartResources[rtype.ID];
				standartResources[rtype.ID] = 0;
			}
        operationsDone++;
        return gainedCount;
	}

    public void GetResources(ResourceContainer[] cost)
    {
        if (cost == null || cost.Length == 0) return;
        else
        {
            foreach (ResourceContainer rc in cost)
            {
                int rid = rc.type.ID;
                if (standartResources[rid] < rc.volume) standartResources[rid] = 0;
                else standartResources[rid] -= rc.volume;
            }
            operationsDone++;
        }
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

		foreach (ResourceContainer rc in resourcesContain ) {
           if (standartResources[rc.type.ID] < rc.volume) return false;
		}
        GetResources(resourcesContain);
        return true;
	}
	#region save-load system
	public void Save( System.IO.FileStream fs) {
        if (standartResources == null) standartResources = new float[ResourceType.RTYPES_COUNT];
        foreach (float f in standartResources)
        {
            fs.Write(System.BitConverter.GetBytes(f), 0, 4);
        }
	}
	public void Load(System.IO.FileStream fs) {
        var val = new byte[4];
        if (standartResources == null) standartResources = new float[ResourceType.RTYPES_COUNT];
        totalVolume = 0;
        float f;
        for (int i = 0; i <ResourceType.RTYPES_COUNT; i++)
        {
            fs.Read(val, 0, 4);
            f = System.BitConverter.ToSingle(val,0);
            standartResources[i] = f;
            totalVolume += f;
        }
	}
	#endregion
}
