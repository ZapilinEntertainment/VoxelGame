public static class ResourcesCost {
	public static ResourceContainer[][] info{get; private set;}

	static ResourcesCost() {
		info = new ResourceContainer[Structure.TOTAL_STRUCTURES_COUNT][];
		info[Structure.ENERGY_CAPACITOR_1_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.metal_N, 2)
		} ;// energy capacitor lvl 1 
		info[Structure.HOUSE_1_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.Plastics, 4), new ResourceContainer(ResourceType.Lumber, 20)
		} ;// house lvl 1
		info[Structure.MINE_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.Plastics, 4)
		} ;// mine lvl 1
		info[Structure.SMELTERY_1_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Lumber, 50)
		} ;// smeltery lvl 1
		info[Structure.WIND_GENERATOR_1_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_E, 2)
		} ;// windGenerator lvl 1
		info[Structure.FARM_1_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Lumber, 50), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 2)
		} ;// farm lvl 1
		info[Structure.LUMBERMILL_1_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Lumber, 30), new ResourceContainer(ResourceType.metal_K, 5), new ResourceContainer(ResourceType.metal_M, 10)
		} ;// lumbermill lvl 1
		info[Structure.DOCK_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.metal_E, 2), 
			new ResourceContainer(ResourceType.Concrete, 100)
		} ;// dock lvl 1
		info[Structure.HQ_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 6),
			new ResourceContainer(ResourceType.Plastics, 45), new ResourceContainer(ResourceType.metal_N, 4)
		};//hq lvl 2
		info[Structure.STORAGE_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 2),
			new ResourceContainer(ResourceType.Plastics, 25)
		};//storage lvl 2
		info[Structure.HOUSE_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 2),
			new ResourceContainer(ResourceType.Plastics, 50)
		}; //house lvl 2
		info[Structure.HOSPITAL_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_E, 5)
		}; //hospital lvl 2
		info[14] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8), new ResourceContainer(ResourceType.Plastics, 25),
			new ResourceContainer(ResourceType.Concrete, 40)
		} ;// smeltery lvl 2
		info[Structure.BIOGENERATOR_2_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 6),
			new ResourceContainer(ResourceType.metal_E, 4)
		}; // biogenerator
		info[Structure.MINERAL_POWERPLANT_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 20),
			new ResourceContainer(ResourceType.metal_E, 10)
		};//mineral F powerplant
		info[Structure.ORE_ENRICHER_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 25)
		};//ore enricher lvl 2

		//info[] = new ResourceContainer[]{
		//	new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 5)
	//	}; // mine upgrade

		info[Structure.ROLLING_SHOP_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 70), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 20),
			new ResourceContainer(ResourceType.Plastics, 70)
		};// rolling shop
		info[Structure.HQ_3_ID] = new ResourceContainer[info[Structure.HQ_2_ID].Length];  // hq level 3
		{
			int origin = Structure.HQ_2_ID, newIndex = Structure.HQ_3_ID;
			for (int i = 0; i < info[origin].Length; i++) {
				info[newIndex][i] = new ResourceContainer( info[origin][i].type, info[origin][i].volume * GameMaster.upgradeCostIncrease);}
		}
		info[Structure.MINI_GRPH_REACTOR_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_M, 5), new ResourceContainer(ResourceType.metal_N, 12)
		}; // mini reactor level 3
		info[Structure.FUEL_FACILITY_3_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 15), new ResourceContainer(ResourceType.Concrete, 60)
		}; // fuel facility level 3
		info[Structure.ENERGY_CAPACITOR_2_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 4),
		} ;// energy capacitor lvl 2
		info[Structure.ENERGY_CAPACITOR_3_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 5),
		} ;// energy capacitor lvl 3
		info[Structure.FARM_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 6)
		} ;// farm lvl 2
		info[Structure.FARM_3_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 13), new ResourceContainer(ResourceType.metal_M, 20)
		} ;// farm lvl 3
		info[Structure.LUMBERMILL_2_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 6)
		} ;// lumbermill lvl 2
		info[Structure.LUMBERMILL_3_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8)
		} ;// lumbermill lvl 3
		info[Structure.STORAGE_1_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 1),
			new ResourceContainer(ResourceType.Plastics, 10)
		};//storage lvl 1
		info[Structure.STORAGE_3_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 25), new ResourceContainer(ResourceType.metal_K, 6), new ResourceContainer(ResourceType.metal_M, 4),
			new ResourceContainer(ResourceType.Plastics, 40)
		};//storage lvl 3
		info[Structure.FARM_4_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 10)
		} ;// farm lvl 4
		info[Structure.LUMBERMILL_4_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 16), new ResourceContainer(ResourceType.metal_E, 8)
		} ;// lumbermill lvl 4
		info[Structure.PLASTICS_FACTORY_4_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 20),
			new ResourceContainer(ResourceType.Concrete, 50)
		} ;// plastics factory lvl 4
		info[Structure.FOOD_FACTORY_4_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 40), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10),
			new ResourceContainer(ResourceType.Concrete, 50), new ResourceContainer(ResourceType.metal_E, 10)
		} ;// food factory lvl 4
		info[Structure.HQ_4_ID] = new ResourceContainer[info[Structure.HQ_3_ID].Length];  // hq level 4
		{
		int origin = Structure.HQ_3_ID, newIndex = Structure.HQ_4_ID;
			for (int i = 0; i < info[origin].Length; i++) {info[newIndex][i] = new ResourceContainer( info[origin][i].type, info[origin][i].volume * GameMaster.upgradeCostIncrease);}
		}
		info[Structure.GRPH_ENRICHER_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 30),
			new ResourceContainer(ResourceType.metal_E, 16)
		};// graphonium enricher lvl 3
		info[Structure.QUANTUM_ENERGY_TRANSMITTER_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 140), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 25),
			new ResourceContainer(ResourceType.metal_N, 10), new ResourceContainer(ResourceType.Plastics, 15)
		} ;// quantum energy transmitter
		info[Structure.FARM_5_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 25)
		} ;// farm block lvl 5
		info[Structure.LUMBERMILL_5_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 16),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 28)
		} ;// lumbermill block lvl 5
		info[Structure.SMELTERY_5_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 350), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 18),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 40)
		} ;// smeltery block lvl 5
		info[Structure.HOUSE_5_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 420), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_E, 12),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 10)
		} ;// residential block lvl 5
		info[Structure.FOOD_FACTORY_5_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
			new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 30)
		} ;// food factory block lvl 5
		info[Structure.STORAGE_5_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 12),
			new ResourceContainer(ResourceType.Concrete, 270), new ResourceContainer(ResourceType.metal_M, 15)
		} ;// storage block lvl 5
		info[Structure.CHEMICAL_FACTORY_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 35),
			new ResourceContainer(ResourceType.metal_E, 25)
		};// chemical factory lvl 4
		info[Structure.SMELTERY_3_ID] = new ResourceContainer[] {
			new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Plastics, 50),
			new ResourceContainer(ResourceType.metal_E, 20)
		} ;// smeltery lvl 3
		info[Structure.HOUSE_3_ID] = new ResourceContainer[]{
			new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 8),
			new ResourceContainer(ResourceType.Plastics, 100)
		}; //house lvl 3
	}
}
