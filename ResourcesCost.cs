using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesCost {
	public static ResourceContainer[][] info{get; private set;}

	static ResourcesCost() {
		info = new ResourceContainer[11][];
		info[0] = new ResourceContainer[0]; //empty
		info[1] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_E, 30),
			new ResourceContainer(ResourceType.metal_M, 50), new ResourceContainer(ResourceType.Food, 140)
		}; //start resources
		info[2] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_M, 2), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.metal_N, 2)
		} ;// energy capacitor lvl 1 
		info[3] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.ElasticMass, 4)
		} ;// house lvl 1
		info[4] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.ElasticMass, 4)
		} ;// mine lvl 1
		info[5] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 8), new ResourceContainer(ResourceType.ElasticMass, 6)
		} ;// smeltery lvl 1
		info[6] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_E, 2)
		} ;// windGenerator lvl 1
		info[7] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Lumber, 50), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 4)
		} ;// farm lvl 1
		info[8] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Lumber, 30), new ResourceContainer(ResourceType.metal_K, 5), new ResourceContainer(ResourceType.metal_M, 10)
		} ;// lumbermill lvl 1
		info[9] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_M, 30), new ResourceContainer(ResourceType.metal_E, 10), 
			new ResourceContainer(ResourceType.Concrete, 100)
		} ;// dock lvl 1
		info[10] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 200), new ResourceContainer(ResourceType.metal_K, 40), new ResourceContainer(ResourceType.metal_E, 10),
			new ResourceContainer(ResourceType.ElasticMass, 45), new ResourceContainer(ResourceType.metal_N, 4)
		};//hq lvl 2
	}
}


