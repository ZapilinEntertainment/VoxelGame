public static class ResourcesCost
{
    public const int SHUTTLE_BUILD_COST_ID = -2, HQ_LVL2_COST_ID = -3, HQ_LVL3_COST_ID = -4, HQ_LVL4_COST_ID = -5, HQ_LVL5_COST_ID = -6, HQ_LVL6_COST_ID = -7 ;

    public static ResourceContainer[] GetCost(int id)
    {
        ResourceContainer[] cost = new ResourceContainer[0];
        switch (id)
        {
            case Structure.HEADQUARTERS_ID:
                cost = new ResourceContainer[]
                {
                    new ResourceContainer(ResourceType.metal_K, 10)
                };
                break;
            case HQ_LVL2_COST_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_E, 6),
                new ResourceContainer(ResourceType.Plastics, 45), new ResourceContainer(ResourceType.metal_N, 4)
            };
                break;
            case HQ_LVL3_COST_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 40), new ResourceContainer(ResourceType.metal_E, 12),
                new ResourceContainer(ResourceType.Plastics, 60), new ResourceContainer(ResourceType.metal_N, 8)
            };
                break;
            case HQ_LVL4_COST_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 200), new ResourceContainer(ResourceType.metal_K, 60), new ResourceContainer(ResourceType.metal_E, 20),
                new ResourceContainer(ResourceType.Plastics, 250), new ResourceContainer(ResourceType.metal_N, 20)
            };
                break;
            case HQ_LVL5_COST_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 400), new ResourceContainer(ResourceType.metal_K, 100), new ResourceContainer(ResourceType.metal_E, 60),
                new ResourceContainer(ResourceType.Plastics, 500), new ResourceContainer(ResourceType.Graphonium, 20)
            };
                break;
            case HQ_LVL6_COST_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 600), new ResourceContainer(ResourceType.metal_K, 200), new ResourceContainer(ResourceType.metal_E, 120),
                new ResourceContainer(ResourceType.Plastics, 750), new ResourceContainer(ResourceType.Graphonium, 100)
            };
                break;
            case SHUTTLE_BUILD_COST_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_S, 50), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 20),
                new ResourceContainer(ResourceType.Plastics, 100), new ResourceContainer(ResourceType.metal_E, 10)
            };
                break;
            case Structure.MINE_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.Plastics, 4)
            };
                break;
            case Structure.WIND_GENERATOR_1_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 4), new ResourceContainer(ResourceType.metal_E, 2)
            };
                break;
            case Structure.ORE_ENRICHER_2_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 60), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 25)
            };
                break;
            case Structure.WORKSHOP_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 70), new ResourceContainer(ResourceType.metal_K, 20), new ResourceContainer(ResourceType.metal_M, 20),
                new ResourceContainer(ResourceType.Plastics, 70)
            };
                break;
            case Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 140), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_E, 25),
                new ResourceContainer(ResourceType.metal_N, 10), new ResourceContainer(ResourceType.Plastics, 15)
            };
                break;
            case Structure.FUEL_FACILITY_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 120), new ResourceContainer(ResourceType.metal_M, 160), new ResourceContainer(ResourceType.Concrete, 300),
                new ResourceContainer(ResourceType.metal_E, 80), new ResourceContainer(ResourceType.Graphonium, 5)
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
            case Structure.DOCK_ID:
            case Structure.DOCK_2_ID:
            case Structure.DOCK_3_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 30), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.metal_E, 2),
                new ResourceContainer(ResourceType.Concrete, 300)
            };
                break;
            case Structure.STORAGE_1_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 200), new ResourceContainer(ResourceType.metal_K, 40), new ResourceContainer(ResourceType.metal_M, 10),
                new ResourceContainer(ResourceType.Plastics, 100)
            };
                break;
            case Structure.STORAGE_2_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 400), new ResourceContainer(ResourceType.metal_K, 70), new ResourceContainer(ResourceType.metal_M, 25),
                new ResourceContainer(ResourceType.Plastics, 200)
            };
                break;
            case Structure.STORAGE_BLOCK_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 12),
                new ResourceContainer(ResourceType.Concrete, 270), new ResourceContainer(ResourceType.metal_M, 15)
            };
                break;
            case Structure.HOUSE_BLOCK_ID:
                cost = new ResourceContainer[]
           {
                new ResourceContainer(ResourceType.Concrete, 1400f),
                new ResourceContainer(ResourceType.metal_K, 400f),
                new ResourceContainer(ResourceType.Plastics, 1000f),
                new ResourceContainer(ResourceType.metal_P, 350f),
                new ResourceContainer(ResourceType.metal_S, 200f)
           };
                break;
            case Structure.ENERGY_CAPACITOR_1_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_K, 60), new ResourceContainer(ResourceType.metal_E, 40), new ResourceContainer(ResourceType.metal_N, 10)
            };
                break;
            case Structure.ENERGY_CAPACITOR_2_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_K, 80), new ResourceContainer(ResourceType.metal_E, 80), new ResourceContainer(ResourceType.metal_N, 20)
            };
                break;
            case Structure.FARM_1_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Lumber, 50), new ResourceContainer(ResourceType.metal_K, 4), new ResourceContainer(ResourceType.metal_M, 2)
            };
                break;
            case Structure.FARM_2_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 6)
            };
                break;
            case Structure.FARM_3_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 13), new ResourceContainer(ResourceType.metal_M, 20)
            };
                break;
            case Structure.COVERED_FARM:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 60), new ResourceContainer(ResourceType.metal_E, 20),
                new ResourceContainer(ResourceType.Concrete, 280)
            };
                break;
            case Structure.FARM_BLOCK_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
                new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 25)
            };
                break;
            case Structure.LUMBERMILL_1_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Lumber, 30), new ResourceContainer(ResourceType.metal_K, 5), new ResourceContainer(ResourceType.metal_M, 10)
            };
                break;
            case Structure.LUMBERMILL_2_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 8), new ResourceContainer(ResourceType.metal_M, 6)
            };
                break;
            case Structure.LUMBERMILL_3_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 8)
            };
                break;
            case Structure.COVERED_LUMBERMILL:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 80), new ResourceContainer(ResourceType.metal_K, 16), new ResourceContainer(ResourceType.metal_E, 8)
            };
                break;
            case Structure.LUMBERMILL_BLOCK_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 16),
                new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 28)
            };
                break;
            case Structure.PLASTICS_FACTORY_3_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 20), new ResourceContainer(ResourceType.metal_K, 25), new ResourceContainer(ResourceType.metal_M, 20),
                new ResourceContainer(ResourceType.Concrete, 50)
            };
                break;
            case Structure.SUPPLIES_FACTORY_4_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 40), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10),
                new ResourceContainer(ResourceType.Concrete, 50), new ResourceContainer(ResourceType.metal_E, 10)
            };
                break;
            case Structure.SUPPLIES_FACTORY_5_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 400), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 20),
                new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 30)
            };
                break;
            case Structure.SMELTERY_1_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 10), new ResourceContainer(ResourceType.Lumber, 50)
            };
                break;
            case Structure.SMELTERY_2_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.metal_M, 16), new ResourceContainer(ResourceType.Plastics, 25),
                new ResourceContainer(ResourceType.Concrete, 40)
            };
                break;
            case Structure.SMELTERY_3_ID:
                cost = new ResourceContainer[] {
                new ResourceContainer(ResourceType.metal_K, 12), new ResourceContainer(ResourceType.metal_M, 20), new ResourceContainer(ResourceType.Plastics, 50),
                new ResourceContainer(ResourceType.metal_E, 20)
            };
                break;
            case Structure.SMELTERY_BLOCK_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 350), new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_E, 18),
                new ResourceContainer(ResourceType.Concrete, 250), new ResourceContainer(ResourceType.metal_M, 40)
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
                new ResourceContainer(ResourceType.metal_K, 15), new ResourceContainer(ResourceType.metal_M, 5), new ResourceContainer(ResourceType.Graphonium, 4)
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
                cost = new ResourceContainer[] { new ResourceContainer(ResourceType.Concrete, 500), new ResourceContainer(ResourceType.metal_K, 120) };
                break;
            case Structure.SWITCH_TOWER_ID:
                cost = new ResourceContainer[] { new ResourceContainer(ResourceType.Concrete, 10), new ResourceContainer(ResourceType.metal_K, 2), new ResourceContainer(ResourceType.Plastics, 10) };
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
                new ResourceContainer(ResourceType.metal_N, 10), new ResourceContainer(ResourceType.metal_E, 40)
            };
                break;
            case Structure.REACTOR_BLOCK_5_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 50), new ResourceContainer(ResourceType.metal_M, 120), new ResourceContainer(ResourceType.metal_N, 60),
                new ResourceContainer(ResourceType.Concrete, 1200), new ResourceContainer(ResourceType.Plastics, 250)
            };
                break;
            case Structure.FOUNDATION_BLOCK_5_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 300), new ResourceContainer(ResourceType.metal_M, 100), new ResourceContainer(ResourceType.metal_E, 50),
                new ResourceContainer(ResourceType.Concrete, 2000),
            };
                break;
            case Structure.CONNECT_TOWER_6_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 80), new ResourceContainer(ResourceType.metal_K, 200), new ResourceContainer(ResourceType.metal_S, 300),
                new ResourceContainer(ResourceType.metal_N, 20), new ResourceContainer(ResourceType.metal_E, 100)
            };
                break;        
            case Structure.HOTEL_BLOCK_6_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 600), new ResourceContainer(ResourceType.metal_K, 325), new ResourceContainer(ResourceType.metal_E, 80),
                new ResourceContainer(ResourceType.Concrete, 1200)
            };
                break;
            case Structure.HOUSING_MAST_6_ID:
                cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Plastics, 2100), new ResourceContainer(ResourceType.metal_K, 850), new ResourceContainer(ResourceType.metal_E, 180),
                new ResourceContainer(ResourceType.Concrete, 3200)
            };
                break;
            case Structure.DOCK_ADDON_1_ID:
                {
                    cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 60), new ResourceContainer(ResourceType.metal_M, 20), new ResourceContainer(ResourceType.metal_N, 10),
                new ResourceContainer(ResourceType.Concrete, 400)
            };
                }
                break;
            case Structure.DOCK_ADDON_2_ID:
                {
                    cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.metal_K, 120), new ResourceContainer(ResourceType.metal_M, 40), new ResourceContainer(ResourceType.metal_N, 20),
                new ResourceContainer(ResourceType.Concrete, 400)
                };
                }
                break;
            case Structure.OBSERVATORY_ID:
                {
                    cost = new ResourceContainer[]
                    {
                        new ResourceContainer(ResourceType.metal_S, 400), new ResourceContainer(ResourceType.Graphonium, 20),
                        new ResourceContainer(ResourceType.metal_K, 120), new ResourceContainer(ResourceType.metal_E, 40)
                    };
                }
                break;
            case Structure.ARTIFACTS_REPOSITORY_ID:
                {
                    cost = new ResourceContainer[]
                    {
                        new ResourceContainer(ResourceType.Stone, 550), new ResourceContainer(ResourceType.metal_K, 30),
                        new ResourceContainer(ResourceType.metal_E, 160)
                    };
                    break;
                }
            case Structure.MONUMENT_ID:
                {
                    cost = new ResourceContainer[]
                    {
                        new ResourceContainer(ResourceType.Concrete, 400), new ResourceContainer(ResourceType.Plastics, 400),
                        new ResourceContainer(ResourceType.metal_E, 40), new ResourceContainer(ResourceType.metal_N, 40)
                    };
                    break;
                }
            case Structure.SETTLEMENT_CENTER_ID:
                {
                    cost = new ResourceContainer[]
                    {
                        new ResourceContainer(ResourceType.Concrete, 50f), new ResourceContainer(ResourceType.metal_K, 10f)
                    };
                    break;
                }
            case Structure.SETTLEMENT_STRUCTURE_ID:
                {
                    cost = new ResourceContainer[]
                    {
                        new ResourceContainer(ResourceType.Concrete, 10f)
                    };
                    break;
                }
            case Structure.PSYCHOKINECTIC_GEN_ID:
                {
                    cost = new ResourceContainer[]{
                new ResourceContainer(ResourceType.Concrete, 100), new ResourceContainer(ResourceType.metal_K, 10), new ResourceContainer(ResourceType.Plastics, 40),
                new ResourceContainer(ResourceType.metal_E, 2)
            };
                    break;
                }
                case Structure.SCIENCE_LAB_ID:
                 cost = new ResourceContainer[]{
                  new ResourceContainer(ResourceType.Concrete, 600), new ResourceContainer(ResourceType.metal_K, 200), new ResourceContainer(ResourceType.Plastics, 250),
                 new ResourceContainer(ResourceType.metal_E, 50), new ResourceContainer(ResourceType.metal_N, 50)
                };
                 break;
        }
        return cost;
    }
    public static ResourceContainer[] GetAdditionalSettlementBuildingCost(byte level)
    {
        switch (level)
        {
            case 2:
                return new ResourceContainer[]
        {
                new ResourceContainer(ResourceType.Concrete, 50f),
                new ResourceContainer(ResourceType.metal_K, 10f)
        };
            default: return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 25f),
                new ResourceContainer(ResourceType.metal_K, 6f)
            };
        }
    }
    public static ResourceContainer[] GetSettlementUpgradeCost(byte level)
    {
        switch (level)
        {
            case 8:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 1000f),
                new ResourceContainer(ResourceType.metal_K, 200f),
                new ResourceContainer(ResourceType.Plastics, 2000f),
                new ResourceContainer(ResourceType.metal_P, 300f),
                new ResourceContainer(ResourceType.metal_S, 100f)
            };
            case 7:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 800f),
                new ResourceContainer(ResourceType.metal_K, 150f),
                new ResourceContainer(ResourceType.Plastics, 1000f),
                new ResourceContainer(ResourceType.metal_P, 240f),
                new ResourceContainer(ResourceType.metal_S, 80f)
            };
            case 6:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 550f),
                new ResourceContainer(ResourceType.metal_K, 110f),
                new ResourceContainer(ResourceType.Plastics, 800f),
                new ResourceContainer(ResourceType.metal_P, 200f),
                new ResourceContainer(ResourceType.metal_S, 50f)
            };
            case 5:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 500f),
                new ResourceContainer(ResourceType.metal_K, 100f),
                new ResourceContainer(ResourceType.Plastics, 400f),
                new ResourceContainer(ResourceType.metal_P, 160f)
            };
            case 4:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 400f),
                new ResourceContainer(ResourceType.metal_K, 70f),
                new ResourceContainer(ResourceType.Plastics, 250f),
                new ResourceContainer(ResourceType.metal_P, 120f)
            };
            case 3:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 250f),
                new ResourceContainer(ResourceType.metal_K, 60f),
                new ResourceContainer(ResourceType.Plastics, 150f),
                new ResourceContainer(ResourceType.metal_P, 60f)
            };
            case 2:
                return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 200f),
                new ResourceContainer(ResourceType.metal_K, 50f),
                new ResourceContainer(ResourceType.Plastics, 100f)
            };
            default: return new ResourceContainer[]
            {
                new ResourceContainer(ResourceType.Concrete, 100f),
                new ResourceContainer(ResourceType.metal_K, 30f),
                new ResourceContainer(ResourceType.Plastics, 50f)
            };
        }
    }
}