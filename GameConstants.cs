public abstract class GameConstants {
    public const float GEARS_LEVEL_TO_CREATE_BLOCK = 4, GEARS_LEVEL_TO_CHANGE_SURFACE_MATERIAL = 2.5f, GEARS_UP_LIMIT = 5, GEARS_LOWER_LIMIT = 0.8f;

    public const byte HQ_LEVEL_TO_CREATE_BLOCK = 4;


    public const int LIFEPOWER_PER_BLOCK = 130; // 200
    public const int LIFEPOWER_SPREAD_SPEED = 10;
    public const float START_HAPPINESS = 1, LIFE_DECAY_SPEED = 0.1f, CAM_LOOK_SPEED = 10,
        START_BIRTHRATE_COEFFICIENT = 0.001f, FACTORY_GEARS_DAMAGE_COEFFICIENT = 0.0001f,
        HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000,
        FOOD_CONSUMPTION = 1, STARVATION_TIME = 1,
        SHIP_ARRIVING_TIME = 300,
        CLOUD_EMITTER_START_SPEED = 0.005f;

    public const int MAX_LIFEPOWER_TRANSFER = 16;

    public static float GetShipArrivingTimer()
    {
        GameMaster gm = GameMaster.realMaster;
        return ((SHIP_ARRIVING_TIME / (gm.tradeVesselsTrafficCoefficient)) * (gm.colonyController.docksLevel + 1) / 2f);
    }
}
