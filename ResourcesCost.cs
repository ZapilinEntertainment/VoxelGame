using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesCost {
	public static ResourceContainer[][] info{get; private set;}
	public const int MINE_UPGRADE_INDEX = 18;

	static ResourcesCost() {
		info = new ResourceContainer[47][];
		info[0] = new ResourceContainer[0]; //empty
		info[1] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_E, 30),
			new ResourceContainer(ResourceType.metal_M, 50), new ResourceContainer(ResourceType.Food, 140)
		}; //start resources
		info[2] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.metal_N, 2)
		} ;// energy capacitor lvl 1 
		info[3] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.Plastics, 4), new ResourceContainer(ResourceType.Lumber, 20)
		} ;// house lvl 1
		info[4] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.Plastics, 4)
		} ;// mine lvl 1
		info[5] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Lumber, 50)
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
			new ResourceContainer(ResourceType.Plastics, 45), new ResourceContainer(ResourceType.metal_N, 4)
		};//hq lvl 2
		info[11] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 2),
			new ResourceContainer(ResourceType.Plastics, 25)
		};//storage lvl 2
		info[12] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 2),
			new ResourceContainer(ResourceType.Plastics, 50)
		}; //house lvl 2
		info[13] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_E, 5)
		}; //hospital lvl 2
		info[14] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8), new ResourceContainer(ResourceType.Plastics, 25),
			new ResourceContainer(ResourceType.Concrete, 40)
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
			new ResourceContainer(ResourceType.Concrete, 70), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 20),
			new ResourceContainer(ResourceType.Plastics, 70)
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
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 6)
		} ;// farm lvl 2
		info[26] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 13), new ResourceContainer(ResourceType.metal_M, 20)
		} ;// farm lvl 3
		info[27] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 6)
		} ;// lumbermill lvl 2
		info[28] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8)
		} ;// lumbermill lvl 3
		info[29] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 1),
			new ResourceContainer(ResourceType.Plastics, 10)
		};//storage lvl 1
		info[30] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 25), new ResourceContainer(ResourceType.metal_K, 6), new ResourceContainer(ResourceType.metal_M, 4),
			new ResourceContainer(ResourceType.Plastics, 40)
		};//storage lvl 3
		info[31] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 10)
		} ;// farm lvl 4
		info[32] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 16), new ResourceContainer(ResourceType.metal_E, 8)
		} ;// lumbermill lvl 4
		info[33] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 20),
			new ResourceContainer(ResourceType.Concrete, 50)
		} ;// plastics factory lvl 4
		info[34] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 40), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10),
			new ResourceContainer(ResourceType.Concrete, 50), new ResourceContainer(ResourceType.metal_E, 10)
		} ;// food factory lvl 4
		info[35] = new ResourceContainer[info[20].Length];  // hq level 4
		for (int i = 0; i < info[20].Length; i++) {info[35][i] = new ResourceContainer( info[20][i].type, info[20][i].volume * GameMaster.upgradeCostIncrease);}
		info[36] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 30),
			new ResourceContainer(ResourceType.metal_E, 16)
		};// graphonium enricher lvl 3
		info[37] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 140), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 25),
			new ResourceContainer(ResourceType.metal_N, 10), new ResourceContainer(ResourceType.Plastics, 15)
		} ;// quantum energy transmitter
		info[38] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 25)
		} ;// farm block lvl 5
		info[39] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 16),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 28)
		} ;// lumbermill block lvl 5
		info[40] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 350), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 18),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 40)
		} ;// smeltery block lvl 5
		info[41] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 420), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_E, 12),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 10)
		} ;// residential block lvl 5
		info[42] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 30)
		} ;// food factory block lvl 5
		info[43] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 12),
			new ResourceContainer(ResourceType.Concrete, 270), new ResourceContainer(ResourceType.metal_M, 15)
		} ;// storage block lvl 5
		info[44] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 35),
			new ResourceContainer(ResourceType.metal_E, 25)
		};// chemical factory lvl 4
		info[45] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Plastics, 50),
			new ResourceContainer(ResourceType.metal_E, 20)
		} ;// smeltery lvl 3
		info[46] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 8),
			new ResourceContainer(ResourceType.Plastics, 100)
		}; //house lvl 3
	}
}


