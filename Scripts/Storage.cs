﻿using System.Collections.Generic;
using UnityEngine;

public sealed class Storage : MonoBehaviour {
    public float totalVolume { get; private set; }
	public float  maxVolume { get; private set; }
	public List<StorageHouse> warehouses { get; private set; }
    private float[] standartResources;
    public int operationsDone { get; private set; } // для UI

    private float announcementTimer;
	private Rect myRect;
	private const float MIN_VALUE_TO_SHOW = 0.001f;    

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
	public float AddResource(ResourceType rtype, float count) {
        if (count < 0)
        {
            Debug.Log("wrong operation: " + StackTraceUtility.ExtractStackTrace());
            return count;
        }
		if (totalVolume >= maxVolume )
        {
            Overloading();
            return count;
        }
        else
        {
            if (count == 0f) return 0f;
        }
		float loadedCount = count;
        if (maxVolume - totalVolume < loadedCount)
        {
            loadedCount = maxVolume - (float)totalVolume;
            Overloading();
        }
        if (rtype == ResourceType.FertileSoil) rtype = ResourceType.Dirt;
		standartResources[ rtype.ID ] += loadedCount;
		totalVolume += loadedCount;
        operationsDone++;
        if (count > loadedCount) return count - loadedCount;
        else return 0f;
	}
	/// <summary>
	/// Attention: container will be destroyed after resources transfer!
	/// </summary>
	public void AddResource(ResourceContainer rc) {
		AddResource(rc.type, rc.volume);
	}
	public void AddResources(List<ResourceContainer> resourcesList) {
		AddResources(resourcesList.ToArray());
	}
	public void AddResources(ResourceContainer[] resourcesList) {
		float freeSpace = maxVolume - (float)totalVolume;
        if (freeSpace == 0 & !GameMaster.loading)
        {
            Overloading();
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
    private void Overloading()
    {
        if (GameMaster.loading) return;
        if (announcementTimer <= 0)
        {
            AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.StorageOverloaded));
            if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.StorageOverload);
            announcementTimer = 10f;
        }
        else
        {
            // storage dumping
            var chunk = GameMaster.realMaster.mainChunk;
            ChunkPos cpos;
            bool secondTry = false;            
            if (warehouses.Count > 0) cpos = warehouses[Random.Range(0, warehouses.Count)].GetBlockPosition();
            else cpos = GameMaster.realMaster.colonyController.hq.GetBlockPosition();
            var sblock = chunk.GetNearestUnoccupiedSurface(cpos);
            SECOND_TRY:
            if (sblock != null)
            {
                int maxIndex = 0, sid = 0;
                float maxValue = 0;
                var bmaterials = ResourceType.blockMaterials;
                for (int j = 0; j < bmaterials.Length; j++)
                {
                    sid = bmaterials[j].ID;
                    if (standartResources[sid] > maxValue)
                    {
                        maxValue = standartResources[sid];
                        maxIndex = sid;
                    }
                }
                int dumpingVal = 1000;
                if (dumpingVal > maxValue) dumpingVal = (int)maxValue;

                dumpingVal -= sblock.FORCED_GetExtension().ScatterResources(SurfaceRect.full, ResourceType.GetResourceTypeById(maxIndex), dumpingVal);

                if (dumpingVal != 0)
                {
                    standartResources[maxIndex] -= dumpingVal;
                    totalVolume -= dumpingVal;
                }
                else
                {
                    if (!secondTry)
                    {
                        secondTry = true;
                        sblock = chunk.GetRandomSurface();
                        goto SECOND_TRY;
                    }
                }
            }
        }
    }

	public float GetResources(int rid, float count) {
		if (GameMaster.realMaster.weNeedNoResources) return count;
		if (totalVolume == 0 ) return 0;
		float gainedCount = 0;
			if (standartResources[rid] > count) {
				gainedCount = count;
				standartResources[rid] -= count;
			}
			else {
				gainedCount = standartResources[rid];
				standartResources[rid] = 0;
			}
        totalVolume -= gainedCount;
        operationsDone++;
        return gainedCount;
	}
    public float GetResources(ResourceType rtype, float count) { return GetResources(rtype.ID, count); }

    public bool TryGetResources(int rid, float count)
    {
        if (GameMaster.realMaster.weNeedNoResources) return true;
        else
        {
            if (totalVolume == 0) return false;
            else
            {
                if (standartResources[rid] < count) return false;
                else
                {
                    standartResources[rid] -= count;
                    totalVolume -= count;
                    operationsDone++;
                    return true;
                }
            }
        }
    }
    public bool TryGetResources(ResourceType rtype, float count)
    {
        return TryGetResources(rtype.ID, count);
    }
    public bool TryGetResources(ResourceContainer[] cost)
    {
        foreach (var rc in cost)
        {
            if (standartResources[rc.resourceID] < rc.volume) return false;
        }
        float f;
        foreach (var rc in cost)
        {
            f = rc.volume;
            standartResources[rc.resourceID] -= f;
            totalVolume -= f;
        }
        operationsDone++;
        return true;
    }
    public void GetResources(ResourceContainer[] cost)
    {
        if (cost == null || cost.Length == 0) return;
        else
        {
            foreach (ResourceContainer rc in cost)
            {
                int rid = rc.type.ID;
                if (standartResources[rid] < rc.volume)
                {
                    totalVolume -= standartResources[rid];
                    standartResources[rid] = 0;
                }
                else
                {
                    standartResources[rid] -= rc.volume;
                    totalVolume -= rc.volume;
                }
            }
            operationsDone++;
        }
    }
    
    public float GetResourceCount(ResourceType rt)
    {
        // no need in check
        return standartResources[rt.ID];
    }
    public float GetResourceCount(int id)
    {
        return standartResources[id];
    }
    public Dictionary<int, float> GetContent(float limit)
    {
        var d = new Dictionary<int,float>();
        float f;
        for (int i = 0; i < standartResources.Length; i++)
        {
            f = standartResources[i];
            if (f >= limit) d.Add(i, f);
        }
        return d;
    }
    /// <summary>
    /// Returns clone of storage array
    /// </summary>
    public float[] SYSTEM_GetResourcesArrayCopy()
    {
        var na = new float[standartResources.Length];
        standartResources.CopyTo(na, 0);
        return na;
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
	public void Save( System.IO.Stream fs) {
        if (standartResources == null) standartResources = new float[ResourceType.TYPES_COUNT];
        foreach (float f in standartResources)
        {
            fs.Write(System.BitConverter.GetBytes(f), 0, 4);
        }
	}
	public void Load(System.IO.Stream fs) {
        var val = new byte[4];
        if (standartResources == null) standartResources = new float[ResourceType.TYPES_COUNT];
        totalVolume = 0f;
        float f;
        for (int i = 0; i <ResourceType.TYPES_COUNT; i++)
        {
            fs.Read(val, 0, 4);
            f = System.BitConverter.ToSingle(val,0);
            if (f < 0)
            {
                Debug.Log("error in resource " + i.ToString() + ", count is " + f.ToString());
                f = 0;
            }
            standartResources[i] = f;
            totalVolume += f;
        }
	}
    #endregion
}
