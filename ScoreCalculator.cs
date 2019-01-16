public class ScoreCalculator  {

    public double GetScore(GameMaster gm)
    {
        double score = 0;
        // подготовка
        #region arrays preparing
        double[] resourcesCosts = new double[ResourceType.resourceTypesArray.Length];
            double val = 0.000000001;
            resourcesCosts[ResourceType.SNOW_ID] = val;
            resourcesCosts[ResourceType.FOOD_ID] = val * 2;
            resourcesCosts[ResourceType.STONE_ID] = val * 4;
            resourcesCosts[ResourceType.LUMBER_ID] = val * 6;
            resourcesCosts[ResourceType.DIRT_ID] = val * 8;
            resourcesCosts[ResourceType.MINERAL_F_ID] = val * 9;
            resourcesCosts[ResourceType.MINERAL_L_ID] = val * 9;
            resourcesCosts[ResourceType.METAL_P_ORE_ID] = val * 10;
            resourcesCosts[ResourceType.METAL_K_ORE_ID] = val * 12;
            resourcesCosts[ResourceType.METAL_M_ORE_ID] = val * 14;
            resourcesCosts[ResourceType.METAL_E_ORE_ID] = val * 16;
            resourcesCosts[ResourceType.METAL_S_ORE_ID] = val * 20;
            resourcesCosts[ResourceType.METAL_N_ORE_ID] = val * 25;

            resourcesCosts[ResourceType.FERTILE_SOIL_ID] = resourcesCosts[ResourceType.DIRT_ID] * 2;
            resourcesCosts[ResourceType.CONCRETE_ID] = resourcesCosts[ResourceType.STONE_ID] * 2;
            resourcesCosts[ResourceType.METAL_P_ID] = resourcesCosts[ResourceType.METAL_P_ORE_ID] * 2;
            resourcesCosts[ResourceType.METAL_K_ID] = resourcesCosts[ResourceType.METAL_K_ORE_ID] * 2;
            resourcesCosts[ResourceType.METAL_M_ID] = resourcesCosts[ResourceType.METAL_M_ORE_ID] * 2.1;
            resourcesCosts[ResourceType.METAL_E_ID] = resourcesCosts[ResourceType.METAL_P_ORE_ID] * 2.2;
            resourcesCosts[ResourceType.METAL_S_ID] = resourcesCosts[ResourceType.METAL_P_ORE_ID] * 2.5;
            resourcesCosts[ResourceType.METAL_N_ID] = resourcesCosts[ResourceType.METAL_P_ORE_ID] * 4;
            resourcesCosts[ResourceType.PLASTICS_ID] = resourcesCosts[ResourceType.LUMBER_ID] * 4;
            resourcesCosts[ResourceType.FUEL_ID] = resourcesCosts[ResourceType.METAL_N_ID] * 1.2;
            resourcesCosts[ResourceType.GRAPHONIUM_ID] = resourcesCosts[ResourceType.METAL_N_ID] * 10;
            resourcesCosts[ResourceType.SUPPLIES_ID] = resourcesCosts[ResourceType.FOOD_ID] + resourcesCosts[ResourceType.PLASTICS_ID];

            val = 0.0001;
            double[] structuresCost = new double[Structure.TOTAL_STRUCTURES_COUNT];
            structuresCost[Structure.PLANT_ID] = 0.01;
            structuresCost[Structure.DRYED_PLANT_ID] = 0;
            structuresCost[Structure.RESOURCE_STICK_ID] = resourcesCosts[ResourceType.CONCRETE_ID];
            structuresCost[Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID] = 100;
            structuresCost[Structure.STORAGE_0_ID] = 10;
            structuresCost[Structure.COLUMN_ID] = 1;
            structuresCost[Structure.SWITCH_TOWER_ID] = 0;
            structuresCost[Structure.FOUNDATION_BLOCK_5_ID] = 2;
            structuresCost[Structure.CONNECT_TOWER_6_ID] = 10;

            structuresCost[Structure.LANDED_ZEPPELIN_ID] = val * 10;
            structuresCost[Structure.HOUSE_1_ID] = val;
            structuresCost[Structure.TREE_OF_LIFE_ID] = 1000;
            structuresCost[Structure.CONTAINER_ID] = val;
            structuresCost[Structure.MINE_ELEVATOR_ID] = 2 * val;
            structuresCost[Structure.TENT_ID] = 0;            
            structuresCost[Structure.DOCK_ID] = val * 100;
            structuresCost[Structure.ENERGY_CAPACITOR_1_ID] = 2 * val;
            structuresCost[Structure.FARM_1_ID] = val * 6;
            structuresCost[Structure.MINE_ID] = val * 8;
            structuresCost[Structure.SMELTERY_1_ID] = val * 8;
            structuresCost[Structure.WIND_GENERATOR_1_ID] = val * 4;
            structuresCost[Structure.BIOGENERATOR_2_ID] = val * 6;
            structuresCost[Structure.HOSPITAL_2_ID] = val * 25;
            structuresCost[Structure.MINERAL_POWERPLANT_2_ID] = val * 12;
            structuresCost[Structure.ORE_ENRICHER_2_ID] = val * 14;
            structuresCost[Structure.WORKSHOP_ID] = val * 16;
            structuresCost[Structure.MINI_GRPH_REACTOR_3_ID] = val * 20;
            structuresCost[Structure.FUEL_FACILITY_3_ID] = val * 18;
            structuresCost[Structure.GRPH_REACTOR_4_ID] = val * 50;
            structuresCost[Structure.XSTATION_3_ID] = val * 36;
            structuresCost[Structure.QUANTUM_TRANSMITTER_4_ID] = val * 25;
            structuresCost[Structure.SHUTTLE_HANGAR_4_ID] = val * 40;
            structuresCost[Structure.RECRUITING_CENTER_4_ID] = val * 45;
            structuresCost[Structure.EXPEDITION_CORPUS_4_ID] = val * 48;
            structuresCost[Structure.CONTROL_CENTER_6_ID] = val * 60;


            structuresCost[Structure.LIFESTONE_ID] = structuresCost[Structure.TREE_OF_LIFE_ID];
            structuresCost[Structure.HQ_2_ID] = structuresCost[Structure.LANDED_ZEPPELIN_ID] * 4;
            structuresCost[Structure.HQ_3_ID] = structuresCost[Structure.HQ_3_ID] * 4;
            structuresCost[Structure.HQ_4_ID] = structuresCost[Structure.HQ_3_ID] * 4;
            structuresCost[Structure.PLASTICS_FACTORY_3_ID] = structuresCost[Structure.FUEL_FACILITY_3_ID];
            structuresCost[Structure.SUPPLIES_FACTORY_4_ID] = structuresCost[Structure.FARM_4_ID];
            structuresCost[Structure.SUPPLIES_FACTORY_5_ID] = structuresCost[Structure.SUPPLIES_FACTORY_4_ID] * 4;
            structuresCost[Structure.GRPH_ENRICHER_3_ID] = structuresCost[Structure.WORKSHOP_ID] * 4;
            structuresCost[Structure.CHEMICAL_FACTORY_4_ID] = structuresCost[Structure.GRPH_ENRICHER_3_ID] * 4;
            structuresCost[Structure.STORAGE_2_ID] = structuresCost[Structure.STORAGE_1_ID] * 2;
            structuresCost[Structure.STORAGE_3_ID] = structuresCost[Structure.STORAGE_2_ID] * 2;
            structuresCost[Structure.STORAGE_5_ID] = structuresCost[Structure.STORAGE_3_ID] * 16;
            structuresCost[Structure.HOUSE_2_ID] = structuresCost[Structure.HOUSE_1_ID] * 3;
            structuresCost[Structure.HOUSE_3_ID] = structuresCost[Structure.HOUSE_2_ID] * 2;
            structuresCost[Structure.HOUSE_5_ID] = structuresCost[Structure.HOUSE_3_ID] * 16;
            structuresCost[Structure.HOTEL_BLOCK_6_ID] = structuresCost[Structure.HOUSE_5_ID] * 1.2;
            structuresCost[Structure.HOUSING_MAST_6_ID] = structuresCost[Structure.HOUSE_5_ID] * 3;
            structuresCost[Structure.ENERGY_CAPACITOR_2_ID] = structuresCost[Structure.ENERGY_CAPACITOR_1_ID] * 2;
            structuresCost[Structure.ENERGY_CAPACITOR_3_ID] = structuresCost[Structure.ENERGY_CAPACITOR_2_ID] * 2;
            structuresCost[Structure.FARM_2_ID] = structuresCost[Structure.FARM_1_ID] * 2;
            structuresCost[Structure.FARM_3_ID] = structuresCost[Structure.FARM_2_ID] * 2;
            structuresCost[Structure.FARM_4_ID] = structuresCost[Structure.FARM_3_ID] * 2;
            structuresCost[Structure.FARM_5_ID] = structuresCost[Structure.FARM_4_ID] * 4;
            structuresCost[Structure.LUMBERMILL_1_ID] = structuresCost[Structure.FARM_1_ID] * 0.85;
            structuresCost[Structure.LUMBERMILL_2_ID] = structuresCost[Structure.LUMBERMILL_1_ID] * 2;
            structuresCost[Structure.LUMBERMILL_3_ID] = structuresCost[Structure.LUMBERMILL_2_ID] * 2;
            structuresCost[Structure.LUMBERMILL_4_ID] = structuresCost[Structure.LUMBERMILL_3_ID] * 2;
            structuresCost[Structure.LUMBERMILL_5_ID] = structuresCost[Structure.LUMBERMILL_5_ID] * 4;
            structuresCost[Structure.SMELTERY_2_ID] = structuresCost[Structure.SMELTERY_1_ID] * 2;
            structuresCost[Structure.SMELTERY_3_ID] = structuresCost[Structure.SMELTERY_2_ID] * 2;
            structuresCost[Structure.SMELTERY_5_ID] = structuresCost[Structure.SMELTERY_3_ID] * 4;
            structuresCost[Structure.REACTOR_BLOCK_5_ID] = structuresCost[Structure.GRPH_REACTOR_4_ID] * 4;
            structuresCost[Structure.DOCK_ADDON_1_ID] = structuresCost[Structure.DOCK_ID] * 4;
            structuresCost[Structure.DOCK_ADDON_2_ID] = structuresCost[Structure.DOCK_ADDON_1_ID] * 4;
        structuresCost[Structure.DOCK_2_ID] = structuresCost[Structure.DOCK_ADDON_1_ID] * 2;
        structuresCost[Structure.DOCK_3_ID] = structuresCost[Structure.DOCK_ADDON_2_ID] * 2;
        #endregion

        Chunk c = gm.mainChunk;
        if (c != null)
        {
            foreach (Block b in c.blocks)
            {
                if (b == null) continue;
                else
                {
                    switch (b.type)
                    {
                        case BlockType.Shapeless:
                            // AWAITING
                            break;
                        case BlockType.Cube:
                            if (b.material_id > 0)
                            score += (b as CubeBlock).volume * CubeBlock.MAX_VOLUME * resourcesCosts[b.material_id];
                            break;
                        case BlockType.Surface:
                        case BlockType.Cave:
                            if (b.material_id > 0)
                                score += SurfaceBlock.INNER_RESOLUTION * SurfaceBlock.INNER_RESOLUTION * resourcesCosts[b.material_id];
                            SurfaceBlock sb = b as SurfaceBlock;
                            if (sb.cellsStatus != 0)
                            {
                                foreach (Structure s in sb.surfaceObjects)
                                {
                                    score += structuresCost[s.id];
                                }
                            }
                            break;
                    }
                }
            }
            score += c.lifePower;
        }

        ColonyController colony = gm.colonyController;
        if (colony != null)
        {
            score += colony.citizenCount;
            score += colony.energyCrystalsCount;
            if (colony.storage != null)
            {
                float[] res = colony.storage.standartResources;
                for (int i =0; i < res.Length; i++)
                {
                    score += resourcesCosts[i] * res[i];
                }
            }
        }
        score *= (int)GameMaster.difficulty / 3f;
        return score;
    }
}
