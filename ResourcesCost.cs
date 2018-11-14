using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesCost {
	public const int SHUTTLE_BUILD_COST_ID =  -2;

	public static ResourceContainer[] GetCost (int id) {
		ResourceContainer[] cost = new ResourceContainer[0];
		switch ( id ) {
		case SHUTTLE_BUILD_COST_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_S, 50), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 20),
				new ResourceContainer(ResourceType.Plastics, 100), new ResourceContainer(ResourceType.metal_E, 10)
			} ;
			break;
		case Structure.MINE_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.Plastics, 4)
			} ;
			break;
		case Structure.WIND_GENERATOR_1_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_E, 2)
			} ;
			break;
		case Structure.ORE_ENRICHER_2_ID:
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
		case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 140), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 25),
				new ResourceContainer(ResourceType.metal_N, 10), new ResourceContainer(ResourceType.Plastics, 15)
			} ;
			break;
		case Structure.FUEL_FACILITY_3_ID:
			cost  = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 15), new ResourceContainer(ResourceType.Concrete, 60)
			};
			break;
		case Structure.GRPH_ENRICHER_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 30),
				new ResourceContainer(ResourceType.metal_E, 16)
			};
			break;
		case Structure.XSTATION_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 75), new ResourceContainer(ResourceType.metal_K, 18), new ResourceContainer(ResourceType.Plastics, 40),
				new ResourceContainer(ResourceType.metal_E, 12), new ResourceContainer(ResourceType.metal_N, 3)
			};
			break;
		case Structure.CHEMICAL_FACTORY_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 35),
				new ResourceContainer(ResourceType.metal_E, 25)
			};
			break;		
		case Structure.DOCK_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.metal_E, 2), 
				new ResourceContainer(ResourceType.Concrete, 280)
			} ;
			break;		
		case Structure.STORAGE_1_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 1),
				new ResourceContainer(ResourceType.Plastics, 10)
			};
			break;
		case Structure.STORAGE_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 2),
				new ResourceContainer(ResourceType.Plastics, 25)
			};
			break;
		case Structure.STORAGE_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 25), new ResourceContainer(ResourceType.metal_K, 6), new ResourceContainer(ResourceType.metal_M, 4),
				new ResourceContainer(ResourceType.Plastics, 40)
			};
			break;
		case Structure.STORAGE_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 12),
				new ResourceContainer(ResourceType.Concrete, 270), new ResourceContainer(ResourceType.metal_M, 15)
			} ;
			break;
		case Structure.HOUSE_1_ID: 	
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 0.5f), new ResourceContainer(ResourceType.Plastics, 4), new ResourceContainer(ResourceType.Lumber, 20)
			};
			break;
		case Structure.HOUSE_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 2),
				new ResourceContainer(ResourceType.Plastics, 50)
			};
			break;
		case Structure.HOUSE_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 8),
				new ResourceContainer(ResourceType.Plastics, 100)
			};
			break;
		case Structure.HOUSE_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 420), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_E, 12),
				new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 10)
			} ;
			break;
		case Structure.ENERGY_CAPACITOR_1_ID: 
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_K, 1), new ResourceContainer(ResourceType.metal_E, 2), new ResourceContainer(ResourceType.Plastics, 8)
			};
			break;
		case Structure.ENERGY_CAPACITOR_2_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_E, 4), new ResourceContainer(ResourceType.Plastics, 10)
			} ;
			break;
		case Structure.ENERGY_CAPACITOR_3_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_E, 12),  new ResourceContainer(ResourceType.Plastics, 20)
			} ;
			break;
		case Structure.FARM_1_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Lumber, 50), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 2)
			} ;
			break;
		case Structure.FARM_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 6)
			} ;
			break;
		case Structure.FARM_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 13), new ResourceContainer(ResourceType.metal_M, 20)
			} ;
			break;
		case Structure.FARM_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 10)
			} ;
			break;
		case Structure.FARM_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
				new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 25)
			} ;
			break;
		case Structure.LUMBERMILL_1_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Lumber, 30), new ResourceContainer(ResourceType.metal_K, 5), new ResourceContainer(ResourceType.metal_M, 10)
			} ;
			break;
		case Structure.LUMBERMILL_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 6)
			}  ;
			break;
		case Structure.LUMBERMILL_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8)
			} ;
			break;
		case Structure.LUMBERMILL_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 16), new ResourceContainer(ResourceType.metal_E, 8)
			} ;
			break;
		case Structure.LUMBERMILL_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 16),
				new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 28)
			} ;
			break;
		case Structure.PLASTICS_FACTORY_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 20),
				new ResourceContainer(ResourceType.Concrete, 50)
			} ;
			break;
		case Structure.FOOD_FACTORY_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 40), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10),
				new ResourceContainer(ResourceType.Concrete, 50), new ResourceContainer(ResourceType.metal_E, 10)
			} ;
			break;
		case Structure.FOOD_FACTORY_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
				new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 30)
			} ;
			break;
		case Structure.SMELTERY_1_ID: cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Lumber, 50)
			} ;
			break;
		case Structure.SMELTERY_2_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8), new ResourceContainer(ResourceType.Plastics, 25),
				new ResourceContainer(ResourceType.Concrete, 40)
			} ;
			break;
		case Structure.SMELTERY_3_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Plastics, 50),
				new ResourceContainer(ResourceType.metal_E, 20)
			} ;
			break;
		case Structure.SMELTERY_5_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Plastics, 350), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 18),
				new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 40)
			} ;
			break;
		case Structure.HQ_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 6),
				new ResourceContainer(ResourceType.Plastics, 45), new ResourceContainer(ResourceType.metal_N, 4)
			};
			break;
		case Structure.HQ_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_E, 12),
				new ResourceContainer(ResourceType.Plastics, 60), new ResourceContainer(ResourceType.metal_N, 8)
			};
			break;
		case Structure.HQ_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 200), new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_E, 20),
				new ResourceContainer(ResourceType.Plastics, 250), new ResourceContainer(ResourceType.metal_N, 20)
			};
			break;
		case Structure.BIOGENERATOR_2_ID:
			cost = new ResourceContainer[] {
				new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 6),
				new ResourceContainer(ResourceType.metal_E, 4)
			};
			break;
		case Structure.HOSPITAL_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_E, 5)
			};
			break;
		case Structure.MINI_GRPH_REACTOR_3_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_M, 5), new ResourceContainer(ResourceType.metal_N, 12)
			};
			break;
		case Structure.GRPH_REACTOR_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 40), new ResourceContainer(ResourceType.metal_N, 30),
				new ResourceContainer(ResourceType.Concrete, 120), new ResourceContainer(ResourceType.Plastics, 100)
			};
			break;
		case Structure.MINERAL_POWERPLANT_2_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 20),
				new ResourceContainer(ResourceType.metal_E, 10)
			};
			break;
		case Structure.COLUMN_ID:
			cost = new ResourceContainer[]{new ResourceContainer(ResourceType.Concrete, 500), new ResourceContainer(ResourceType.metal_K, 120)};
			break;
		case Structure.SWITCH_TOWER_ID:
			cost = new ResourceContainer[]{new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.Plastics,10)};
			break;
		case Structure.SHUTTLE_HANGAR_4_ID:
			cost = new ResourceContainer[]{new ResourceContainer(ResourceType.Concrete, 300), new ResourceContainer(ResourceType.metal_K, 40), new ResourceContainer(ResourceType.Plastics, 250),
				new ResourceContainer(ResourceType.metal_M, 30)
			};
			break;
		case Structure.RECRUITING_CENTER_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 340), new ResourceContainer(ResourceType.metal_K, 80), new ResourceContainer(ResourceType.Plastics, 250)
			};
			break;
		case Structure.EXPEDITION_CORPUS_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 350), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.Plastics, 250),
				new ResourceContainer(ResourceType.metal_E, 20)
			};
			break;
		case Structure.QUANTUM_TRANSMITTER_4_ID:
			cost = new ResourceContainer[]{
				new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_S, 70),
				new ResourceContainer(ResourceType.metal_N, 20)
			};
			break;

		}
		return cost;
	}
}
