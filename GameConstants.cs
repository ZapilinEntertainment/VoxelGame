﻿public abstract class GameConstants {
    public const float GEARS_LEVEL_TO_CREATE_BLOCK = 4, GEARS_LEVEL_TO_CHANGE_SURFACE_MATERIAL = 2.5f, GEARS_UP_LIMIT = 5, GEARS_LOWER_LIMIT = 0.8f;

    public const byte HQ_LEVEL_TO_CREATE_BLOCK = 4, HQ_MAX_LEVEL = 7;


    public const int LIFEPOWER_PER_BLOCK = 130; // 200
    public const int LIFEPOWER_SPREAD_SPEED = 10 ;
    public const float START_HAPPINESS = 0.5f, HAPPINESS_CHANGE_SPEED = 0.001f,
        LIFE_DECAY_SPEED = 0.1f, CAM_LOOK_SPEED = 10,
        START_BIRTHRATE_COEFFICIENT = 0.001f,        
        HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000,
        FOOD_CONSUMPTION = 1, STARVATION_TIME = 1,
        SHIP_ARRIVING_TIME = 300,
        CLOUD_EMITTER_START_SPEED = 0.005f,
        RSPACE_CONSUMING_VAL = 0.125f,
        LSECTOR_CONSUMING_VAL = 0.9f        
        ;
    public static float WORLD_CONSUMING_TIMER { get { return 15 * (6 - (int)GameMaster.difficulty); } }
    public static float FACTORY_GEARS_DAMAGE_COEFFICIENT { get { return 0.0000005f * (int)GameMaster.difficulty; } }
    public static float WORKSITES_GEARS_DAMAGE_COEFFICIENT { get { return 0.000002f * (int)GameMaster.difficulty; } }
    public const int MAX_LIFEPOWER_TRANSFER = 16;

    public const string BASE_SETTINGS_PLAYERPREF = "baseSettings";
    // int key - 32 values
    // 0 - lang
    // 1 - (0 -> first launch, 1 - not first)
    public const uint SAVE_SYSTEM_VERSION = 1;
   

    public static float GetShipArrivingTimer()
    {
        GameMaster gm = GameMaster.realMaster;
        return ((SHIP_ARRIVING_TIME / (gm.tradeVesselsTrafficCoefficient)) / (gm.colonyController.docksLevel + 1) / 2f);
    }
}
