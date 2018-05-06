using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesCost {
	public static ResourceContainer[][] info{get; private set;}
	public const int MINE_UPGRADE_INDEX = 18;

	static ResourcesCost() {
		info = new ResourceContainer[31][];
		info[0] = new ResourceContainer[0]; //empty
		info[1] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_E, 30),
			new ResourceContainer(ResourceType.metal_M, 50), new ResourceContainer(ResourceType.Food, 140)
		}; //start resources
		info[2] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.metal_N, 2)
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
		info[19] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 150), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 40)
		};// rolling shop
		info[20] = new ResourceContainer[info[10].Length];  // hq level 3
		for (int i = 0; i < info[10].Length; i++) {info[20][i] = new ResourceContainer( info[10][i].type, info[10][i].volume * GameMaster.upgradeCostIncrease);}
		info[21] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_M, 5), new ResourceContainer(ResourceType.metal_N, 12)
		}; // mini reactor level 3
		info[22] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 15), new ResourceContainer(ResourceType.Concrete, 60)
		}; // fuel facility level 3
		info[23] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 4),
		} ;// energy capacitor lvl 2
		info[24] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 5),
		} ;// energy capacitor lvl 3
		info[25] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.ElasticMass, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 6)
		} ;// farm lvl 2
		info[26] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.ElasticMass, 20), new ResourceContainer(ResourceType.metal_K, 13), new ResourceContainer(ResourceType.metal_M, 20)
		} ;// farm lvl 3
		info[27] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.ElasticMass, 20), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 6)
		} ;// lumbermill lvl 2
		info[28] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.ElasticMass, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8)
		} ;// lumbermill lvl 3
		info[29] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 1),
			new ResourceContainer(ResourceType.ElasticMass, 10)
		};//storage lvl 1
		info[30] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 25), new ResourceContainer(ResourceType.metal_K, 6), new ResourceContainer(ResourceType.metal_M, 4),
			new ResourceContainer(ResourceType.ElasticMass, 40)
		};//storage lvl 3
	}
}


