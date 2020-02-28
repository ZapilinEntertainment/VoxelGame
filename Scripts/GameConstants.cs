public enum FullfillStatus : byte { Unknown, Full, Empty};
public abstract class GameConstants {
    public const float GEARS_LEVEL_TO_CREATE_BLOCK = 4, GEARS_LEVEL_TO_CHANGE_SURFACE_MATERIAL = 2.5f, GEARS_UP_LIMIT = 5, GEARS_LOWER_LIMIT = 0.8f;

    public const byte HQ_LEVEL_TO_CREATE_BLOCK = 4, HQ_MAX_LEVEL = 6;

    public const int CELESTIAL_LAYER = 9;
    public const float START_HAPPINESS = 0.5f, LOW_HAPPINESS = 0.2f, HIGH_HAPPINESS = 0.9f,
        HAPPINESS_CHANGE_SPEED = 0.001f, 
        HEALTH_CHANGE_SPEED = 0.001f,
        CAM_LOOK_SPEED = 10,
        START_BIRTHRATE_COEFFICIENT = 0.001f,
        HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000,
        FOOD_CONSUMPTION = 1,
        SHIP_ARRIVING_TIME = 300,
        CLOUD_EMITTER_START_SPEED = 0.005f,
        STABILITY_CHANGE_SPEED = 0.002f,
        RSPACE_CONSUMING_VAL = 0.1f,
        LSECTOR_CONSUMING_VAL = 0.9f,

        OBSERVATORY_FIND_SPEED_CF = 10, DIGGING_SPEED = 0.5f, MINING_SPEED = 1f, POURING_SPEED = 0.5f, FACTORY_SPEED = 0.3f,
        CLEARING_SPEED = 5f, GATHERING_SPEED = 0.1f, MACHINE_CONSTRUCTING_SPEED = 1f, BLOCK_BUILDING_SPEED = 0.5f,
        HYDROPONICS_SPEED = 1f, OPEN_FARM_SPEED = 1f, EXPLORE_SPEED = 0.1f,

        RUINS_COEFFICIENT = 0.25f,
        GRAPHONIUM_CRITICAL_MASS = 10000f,
        ARTIFACT_FOUND_CHANCE = 0.05f
        ;

    public static float WORLD_CONSUMING_TIMER {
        get { return 15 * (2f - GameMaster.realMaster.GetDifficultyCoefficient()); }
    }
    public static float FACTORY_GEARS_DAMAGE_COEFFICIENT { get { return 0.000003f * GameMaster.realMaster.GetDifficultyCoefficient(); } }
    public static float WORKSITES_GEARS_DAMAGE_COEFFICIENT { get { return 0.000012f * GameMaster.realMaster.GetDifficultyCoefficient(); } }

    public const string BASE_SETTINGS_PLAYERPREF = "baseSettings";
    // int key - 32 values
    // 0 - lang
    // 1 - (0 -> first launch, 1 - not first)
    public const uint SAVE_SYSTEM_VERSION = 3;
    // 1 - 9.3.1 public alpha
    // 3 - 13    

    public static float GetShipArrivingTimer()
    {
        GameMaster gm = GameMaster.realMaster;
        return ((SHIP_ARRIVING_TIME / (gm.tradeVesselsTrafficCoefficient)) / (gm.colonyController.docksLevel + 1) / 2f);
    }

    public static float GetUpperBorder() { return Chunk.chunkSize * 2; }
    public static float GetBottomBorder() { return Chunk.chunkSize * (-1); }
}
