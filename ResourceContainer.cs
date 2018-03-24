using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceContainer {
	public readonly ResourceType type;
	public float volume{get; private set;}

	public ResourceContainer(ResourceType f_type, float f_volume) {
		type= f_type; 
		if (f_volume < 0) f_volume = 0;
		volume = f_volume;
	}
	public ResourceContainer() {
		type = ResourceType.Nothing;
		volume = 0;
	}

	public void Add(float f_volume) {if (f_volume > 0) volume += f_volume;}
	public float Get(float f_volume) {
		if (f_volume < 0) return 0;
		if (f_volume > volume) f_volume = volume;
		volume -= f_volume;
		return f_volume;
		}
}
