public class ScoreCalculator
{
    public static double GetScore(GameMaster gm)
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

        val = 1;
        double[] structuresCost = new double[Structure.TOTAL_STRUCTURES_COUNT];
        structuresCost[Structure.PLANT_ID] = val * 0.01f;
        structuresCost[Structure.DRYED_PLANT_ID] = 0;
        structuresCost[Structure.RESOURCE_STICK_ID] = resourcesCosts[ResourceType.CONCRETE_ID];
        structuresCost[Structure.QUANTUM_ENERGY_TRANSMITTER_5_ID] = 100 * val;
        structuresCost[Structure.STORAGE_0_ID] = 10 * val;
        structuresCost[Structure.COLUMN_ID] = val;
        structuresCost[Structure.FOUNDATION_BLOCK_5_ID] = 2 * val;
        structuresCost[Structure.CONNECT_TOWER_6_ID] = 10 * val;

        structuresCost[Structure.HEADQUARTERS_ID] = 10 * val;
        structuresCost[Structure.TREE_OF_LIFE_ID] = 1000 * val;
        structuresCost[Structure.CONTAINER_ID] = val / 10f;
        structuresCost[Structure.MINE_ELEVATOR_ID] = val / 4f;
        structuresCost[Structure.TENT_ID] = 0;
        structuresCost[Structure.DOCK_ID] = val * 100;
        structuresCost[Structure.ENERGY_CAPACITOR_1_ID] = 2 * val;
        structuresCost[Structure.PSYCHOKINECTIC_GEN_ID] = 2 * val;
        structuresCost[Structure.MONUMENT_ID] = 4 * val;
        structuresCost[Structure.WIND_GENERATOR_1_ID] = val * 4;
        structuresCost[Structure.FARM_1_ID] = val * 6;
        structuresCost[Structure.BIOGENERATOR_2_ID] = val * 6;
        structuresCost[Structure.MINE_ID] = val * 8;
        structuresCost[Structure.SMELTERY_1_ID] = val * 8;             
        structuresCost[Structure.HOSPITAL_ID] = val * 25;
        structuresCost[Structure.MINERAL_POWERPLANT_2_ID] = val * 12;
        structuresCost[Structure.ORE_ENRICHER_2_ID] = val * 14;
        structuresCost[Structure.WORKSHOP_ID] = val * 16;
        structuresCost[Structure.MINI_GRPH_REACTOR_3_ID] = val * 20;
        structuresCost[Structure.FUEL_FACILITY_ID] = val * 18;
        structuresCost[Structure.GRPH_REACTOR_4_ID] = val * 50;
        structuresCost[Structure.XSTATION_3_ID] = val * 36;
        structuresCost[Structure.QUANTUM_TRANSMITTER_4_ID] = val * 25;
        structuresCost[Structure.SHUTTLE_HANGAR_4_ID] = val * 40;
        structuresCost[Structure.RECRUITING_CENTER_4_ID] = val * 45;
        structuresCost[Structure.EXPEDITION_CORPUS_4_ID] = val * 48;
        //structuresCost[Structure.CONTROL_CENTER_6_ID] = val * 60;
        structuresCost[Structure.ENGINE_ID] = val * 60;
        structuresCost[Structure.OBSERVATORY_ID] = 100 * val;
        structuresCost[Structure.ARTIFACTS_REPOSITORY_ID] = 16 * val;      

        // базируются на предыдущих
        structuresCost[Structure.LIFESTONE_ID] = structuresCost[Structure.TREE_OF_LIFE_ID];
        structuresCost[Structure.PLASTICS_FACTORY_3_ID] = structuresCost[Structure.FUEL_FACILITY_ID];
        structuresCost[Structure.SUPPLIES_FACTORY_4_ID] = structuresCost[Structure.COVERED_FARM];
        structuresCost[Structure.SUPPLIES_FACTORY_5_ID] = structuresCost[Structure.SUPPLIES_FACTORY_4_ID] * 4;
        structuresCost[Structure.GRPH_ENRICHER_3_ID] = structuresCost[Structure.WORKSHOP_ID] * 4;
        structuresCost[Structure.STORAGE_2_ID] = structuresCost[Structure.STORAGE_1_ID] * 2;
        structuresCost[Structure.SCIENCE_LAB_ID] = structuresCost[Structure.XSTATION_3_ID] * 2f;
       
        structuresCost[Structure.STORAGE_BLOCK_ID] = structuresCost[Structure.STORAGE_2_ID] * 4;
        structuresCost[Structure.HOTEL_BLOCK_6_ID] = structuresCost[Structure.HOUSE_BLOCK_ID] * 1.2;
        structuresCost[Structure.HOUSING_MAST_6_ID] = structuresCost[Structure.HOUSE_BLOCK_ID] * 3;
        structuresCost[Structure.ENERGY_CAPACITOR_2_ID] = structuresCost[Structure.ENERGY_CAPACITOR_1_ID] * 2;
        
        structuresCost[Structure.FARM_2_ID] = structuresCost[Structure.FARM_1_ID] * 2;
        structuresCost[Structure.FARM_3_ID] = structuresCost[Structure.FARM_2_ID] * 2;
        structuresCost[Structure.COVERED_FARM] = structuresCost[Structure.FARM_3_ID] * 2;
        structuresCost[Structure.FARM_BLOCK_ID] = structuresCost[Structure.COVERED_FARM] * 4;
        structuresCost[Structure.LUMBERMILL_1_ID] = structuresCost[Structure.FARM_1_ID] * 0.85;
        structuresCost[Structure.LUMBERMILL_2_ID] = structuresCost[Structure.LUMBERMILL_1_ID] * 2;
        structuresCost[Structure.LUMBERMILL_3_ID] = structuresCost[Structure.LUMBERMILL_2_ID] * 2;
        structuresCost[Structure.COVERED_LUMBERMILL] = structuresCost[Structure.LUMBERMILL_3_ID] * 2;
        structuresCost[Structure.LUMBERMILL_BLOCK_ID] = structuresCost[Structure.LUMBERMILL_BLOCK_ID] * 4;
        structuresCost[Structure.SMELTERY_2_ID] = structuresCost[Structure.SMELTERY_1_ID] * 2;
        structuresCost[Structure.SMELTERY_3_ID] = structuresCost[Structure.SMELTERY_2_ID] * 2;
        structuresCost[Structure.SMELTERY_BLOCK_ID] = structuresCost[Structure.SMELTERY_3_ID] * 4;
        structuresCost[Structure.REACTOR_BLOCK_5_ID] = structuresCost[Structure.GRPH_REACTOR_4_ID] * 4;
        structuresCost[Structure.DOCK_ADDON_1_ID] = structuresCost[Structure.DOCK_ID] * 4;
        structuresCost[Structure.DOCK_ADDON_2_ID] = structuresCost[Structure.DOCK_ADDON_1_ID] * 4;
        structuresCost[Structure.DOCK_2_ID] = structuresCost[Structure.DOCK_ADDON_1_ID] * 2;
        structuresCost[Structure.DOCK_3_ID] = structuresCost[Structure.DOCK_ADDON_2_ID] * 2;
        structuresCost[Structure.COMPOSTER_ID] = structuresCost[Structure.SMELTERY_2_ID];
        structuresCost[Structure.HOSPITAL_2_ID] = structuresCost[Structure.HOSPITAL_ID];
        structuresCost[Structure.CAPACITOR_MAST_ID] = structuresCost[Structure.ENERGY_CAPACITOR_2_ID] * 1.5f;
        structuresCost[Structure.ANCHOR_BASEMENT_ID] = 1000 * val;
        #endregion

        Chunk c = gm.mainChunk;
        
        if (c != null)
        {
            int r_id, n_id = ResourceType.Nothing.ID;
            foreach (var bd in c.blocks)
            {
                Block b = bd.Value;
                r_id = ResourceType.GetResourceTypeById(b.GetMaterialID()).ID;
                if (r_id != n_id)
                {
                    score += resourcesCosts[r_id] * b.GetVolume();
                }
            }
            score += c.lifePower;
        }
        var slist = UnityEngine.Object.FindObjectsOfType<Structure>();
        if (slist != null)
        {
            int ct = structuresCost.Length, id;
            foreach (var s in slist)
            {
                id = s.ID;
                if (id >= 0 & id < ct)
                {
                    score += structuresCost[id];
                }
            }
            slist = null;
        }

        ColonyController colony = gm.colonyController;
        if (colony != null)
        {
            score += colony.citizenCount;
            score += colony.energyCrystalsCount;
            if (colony.storage != null)
            {
                var res = colony.storage.SYSTEM_GetResourcesArrayCopy();
                for (int i = 0; i < res.Length; i++)
                {
                    score += resourcesCosts[i] * res[i];
                }
            }
            //посчитать settlements list
            //посчитать artifacts
            // посчитать команды
            // посчитать объекты карты
        }
        score *= (int)GameMaster.realMaster.difficulty / 3f;

        // + за артефакты

        return score;
    }
}