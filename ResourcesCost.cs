﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesCost {
	public static ResourceContainer[][] info{get; private set;}
	public const int MINE_UPGRADE_INDEX = 18;

	static ResourcesCost() {
		info = new ResourceContainer[19][];
		info[0] = new ResourceContainer[0]; //empty
		info[1] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_E, 30),
			new ResourceContainer(ResourceType.metal_M, 50), new ResourceContainer(ResourceType.Food, 140)
		}; //start resources
		info[2] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_M, 2), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.metal_N, 2)
		} ;// energy capacitor lvl 1 
		info[3] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.ElasticMass, 4), new ResourceContainer(ResourceType.Lumber, 20)
		} ;// house lvl 1
		info[4] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.ElasticMass, 4)
		} ;// mine lvl 1
		info[5] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 8), new ResourceContainer(ResourceType.Lumber, 6)
		} ;// smeltery lvl 1
		info[6] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_E, 2)
		} ;// windGenerator lvl 1
		info[7] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Lumber, 50), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 2)
		} ;// farm lvl 1
		info[8] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Lumber, 30), new ResourceContainer(ResourceType.metal_K, 5), new ResourceContainer(ResourceType.metal_M, 10)
		} ;// lumbermill lvl 1
		info[9] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.metal_E, 2), 
			new ResourceContainer(ResourceType.Concrete, 100)
		} ;// dock lvl 1
		info[10] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 6),
			new ResourceContainer(ResourceType.ElasticMass, 45), new ResourceContainer(ResourceType.metal_N, 4)
		};//hq lvl 2
		info[11] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 2),
			new ResourceContainer(ResourceType.ElasticMass, 25)
		};//storage lvl 2
		info[12] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_E, 1)
		}; //house lvl 2
		info[13] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_E, 5)
		}; //hospital lvl 2
		info[14] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 14), new ResourceContainer(ResourceType.ElasticMass, 20),
			new ResourceContainer(ResourceType.Concrete, 60)
		} ;// smeltery lvl 2
		info[15] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 6),
			new ResourceContainer(ResourceType.metal_E, 4)
		}; // biogenerator
		info[16] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 20),
			new ResourceContainer(ResourceType.metal_E, 10)
		};//mineral F powerplant
		info[17] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 25)
		};//ore enricher lvl 2
		info[18] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 5)
		}; // mine upgrade
	}
}


