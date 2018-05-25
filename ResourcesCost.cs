using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesCost {

	public static ResourceContainer[] GetCost (int id, byte level) {
		ResourceContainer[] cost = new ResourceContainer[0];
		switch ( id ) {
		case Structure.MINE_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_M, 4 * level), new ResourceContainer(ResourceType.metal_K, 10 * level), new ResourceContainer(ResourceType.Plastics, 4 * level)
			} ;
			break;
		case Structure.WIND_GENERATOR_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_E, 2)
			} ;
			break;
		case Structure.ORE_ENRICHER_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 25)
			};
			break;
		case Structure.ROLLING_SHOP_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 70), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 20),
				new ResourceContainer(ResourceType.Plastics, 70)
			};
			break;
		case Structure.QUANTUM_ENERGY_TRANSMITTER_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 140), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 25),
				new ResourceContainer(ResourceType.metal_N, 10), new ResourceContainer(ResourceType.Plastics, 15)
			} ;
			break;
		case Structure.FUEL_FACILITY_ID:
			cost  = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 15), new ResourceContainer(ResourceType.Concrete, 60)
			};
			break;
		case Structure.GRPH_ENRICHER_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 30),
				new ResourceContainer(ResourceType.metal_E, 16)
			};
			break;
		case Structure.XSTATION_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 75), new ResourceContainer(ResourceType.metal_K, 18), new ResourceContainer(ResourceType.Plastics, 40),
				new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 3)
			};
			break;
		case Structure.CHEMICAL_FACTORY_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 35),
				new ResourceContainer(ResourceType.metal_E, 25)
			};
			break;		
		case Structure.DOCK_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.metal_E, 2), 
				new ResourceContainer(ResourceType.Concrete, 100)
			} ;
			break;		
		case Structure.STORAGE_ID:
			switch ( level ) {
			case 1:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 1),
					new ResourceContainer(ResourceType.Plastics, 10)
				};
				break;
			case 2:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Concrete, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 2),
					new ResourceContainer(ResourceType.Plastics, 25)
				};
				break;
			case 3:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Concrete, 25), new ResourceContainer(ResourceType.metal_K, 6), new ResourceContainer(ResourceType.metal_M, 4),
					new ResourceContainer(ResourceType.Plastics, 40)
				};
				break;
			case 5:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 12),
					new ResourceContainer(ResourceType.Concrete, 270), new ResourceContainer(ResourceType.metal_M, 15)
				} ;
				break;
			}
			break;
		case Structure.HOUSE_ID:
			switch ( level ) {
			case 1: 	
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.Plastics, 4), new ResourceContainer(ResourceType.Lumber, 20)
				};
				break;
			case 2:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 2),
					new ResourceContainer(ResourceType.Plastics, 50)
				};
				break;
			case 3:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 8),
					new ResourceContainer(ResourceType.Plastics, 100)
				};
				break;
			case 5:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 420), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_E, 12),
					new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 10)
				} ;
				break;
			}
			break;
		case Structure.ENERGY_CAPACITOR_ID:
			switch (level) {
			case 1: 
				cost = new ResourceContainer[] {
					new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.metal_N, 2)
				};
				break;
			case 2:
				cost = new ResourceContainer[] {
					new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 4),
				} ;
				break;
			case 3:
				cost = new ResourceContainer[] {
					new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 5),
				} ;
				break;
			}
			break;
		case Structure.FARM_ID:
			switch (level) {
			case 1:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Lumber, 50), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 2)
				} ;
				break;
			case 2:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 6)
				} ;
				break;
			case 3:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 13), new ResourceContainer(ResourceType.metal_M, 20)
				} ;
				break;
			case 4:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 10)
				} ;
				break;
			case 5:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
					new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 25)
				} ;
				break;
			}
			break;
		case Structure.LUMBERMILL_ID:
			switch (level) {
			case 1:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Lumber, 30), new ResourceContainer(ResourceType.metal_K, 5), new ResourceContainer(ResourceType.metal_M, 10)
				} ;
				break;
			case 2:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 6)
				}  ;
				break;
			case 3:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8)
				} ;
				break;
			case 4:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 16), new ResourceContainer(ResourceType.metal_E, 8)
				} ;
				break;
			case 5:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 16),
					new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 28)
				} ;
				break;
			}
			break;
		case Structure.PLASTICS_FACTORY_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 20),
				new ResourceContainer(ResourceType.Concrete, 50)
			} ;
			break;
		case Structure.FOOD_FACTORY_ID:
			switch (level) {
			case 4:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 40), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10),
					new ResourceContainer(ResourceType.Concrete, 50), new ResourceContainer(ResourceType.metal_E, 10)
				} ;
				break;
			case 5:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
					new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 30)
				} ;
				break;
			}
			break;
		case Structure.SMELTERY_ID:
			switch (level) {
				case 1: cost = new ResourceContainer[] {
					new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Lumber, 50)
				} ;
				break;
			case 2:
				cost = new ResourceContainer[] {
					new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8), new ResourceContainer(ResourceType.Plastics, 25),
					new ResourceContainer(ResourceType.Concrete, 40)
				} ;
				break;
			case 3:
				cost = new ResourceContainer[] {
					new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Plastics, 50),
					new ResourceContainer(ResourceType.metal_E, 20)
				} ;
				break;
			case 5:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Plastics, 350), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 18),
					new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 40)
				} ;
				break;
			}
			break;
		case Structure.HQ_ID:
			switch (level) {
			case 2:
				cost = new ResourceContainer[]{
					new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 6),
					new ResourceContainer(ResourceType.Plastics, 45), new ResourceContainer(ResourceType.metal_N, 4)
				};
				break;
			}
			break;
		case Structure.BIOGENERATOR_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 6),
				new ResourceContainer(ResourceType.metal_E, 4)
			};
			break;
		case Structure.HOSPITAL_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_E, 5)
			};
			break;
		case Structure.MINI_GRPH_REACTOR_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_M, 5), new ResourceContainer(ResourceType.metal_N, 12)
			};
			break;
		case Structure.GRPH_REACTOR_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 40), new ResourceContainer(ResourceType.metal_N, 30),
				new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.Plastics, 100)
			};
			break;
		case Structure.MINERAL_POWERPLANT_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 20),
				new ResourceContainer(ResourceType.metal_E, 10)
			};
			break;
		}
		return cost;
	}
}


