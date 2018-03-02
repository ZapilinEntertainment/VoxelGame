using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResourceCrate {
	ResourceType resourceType;
	int count;
	public ResourceCrate (ResourceType resType, int f_count) {
		resourceType = resType;
		count = f_count;
	}
}

public class Transport : MonoBehaviour {
	int passengersCapacity = 10, peopleCount, storageCapacity = 10;
	public List<ResourceCrate> storage {get; protected set;}
	// Use this for initialization
	void Awake () {
		storage = new List<ResourceCrate>();
	}
	
	public bool Load(ResourceCrate r) {
		if (storage.Count >= storageCapacity) return false;
		else {storage.Add(r); return true;}
	}
}
