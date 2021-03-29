using UnityEngine;

public enum FullfillStatus : byte { Unknown, Full, Empty};
public enum WorkType : byte { Default, Digging, Pouring, Factory, Clearing, Gathering, BlockBuilding,
OpenFarming, HydroponicsFarming, OpenLumbering, HydroponicsLumbering, Mining, GearsUpgrading, Recruiting,
    ShuttleConstructing, ObservatoryFindCycle, AnchorBasement_Lift }
public abstract class GameConstants {
    public const float GEARS_LEVEL_TO_CREATE_BLOCK = 4, GEARS_LEVEL_TO_CHANGE_SURFACE_MATERIAL = 2.5f, GEARS_LOWER_LIMIT = 0.8f,
        GEARS_DAMAGE_COEFFICIENT = 0.00005f;

    public const byte HQ_LEVEL_TO_CREATE_BLOCK = 4, HQ_MAX_LEVEL = 6;

    public const int CELESTIAL_LAYER = 9;
    public const float START_HAPPINESS = 0.5f, LOW_HAPPINESS = 0.2f, HIGH_HAPPINESS = 0.9f,
        HAPPINESS_CHANGE_SPEED = 0.001f, 
        CAM_LOOK_SPEED = 10,
        START_BIRTHRATE_COEFFICIENT = 0.001f, 
        HIRE_COST_INCREASE = 0.1f, ENERGY_IN_CRYSTAL = 1000,
        FOOD_CONSUMPTION = 1,
        CLOUD_EMITTER_START_SPEED = 0.005f,
        STABILITY_CHANGE_SPEED = 0.002f,
        RSPACE_CONSUMING_VAL = 0.1f,
        LSECTOR_CONSUMING_VAL = 0.9f,

        OBSERVATORY_FIND_SPEED_CF = 10,

        RUINS_COEFFICIENT = 0.25f,
        GRAPHONIUM_CRITICAL_MASS = 10000f,
        ARTIFACT_FOUND_CHANCE = 0.05f,
        PER_DOCKED_SHIP_BASIC_REWARD = 5f,

        ASCENSION_VERYLOW = 0.1f, ASCENSION_LOW = 0.3f, ASCENSION_MEDIUM = 0.5f, ASCENSION_HIGH = 0.7f, ASCENSION_VERYHIGH = 0.9f,
        ASCENSION_STARTVALUE = 0f, ASCENSION_CHANGE_SPEED = 0.0005f
        ;
    

    public static float WORLD_CONSUMING_TIMER {
        get { return 15 * (2f - GameMaster.realMaster.GetDifficultyCoefficient()); }
    }

    public const string PP_GAMEID_PROPERTY = "NextGameID",PP_LOD_DISTANCE_PROPERTY = "LOD distance";
    public const string PP_BASE_SETTINGS_PROPERTY = "baseSettings";
    // int key - 32 values
    // 0 - lang
    // 1 - (0 -> first launch, 1 - not first)
    // 2 - tutorial first note available ( 0 - need not, 1 - no need)

    public const uint SAVE_SYSTEM_VERSION = 4;
    // 1 - 9.3.1 public alpha
    // 3 - 13    
    // 4 - 15+ the history book on a shelf is always repeating itself

    public static float GetUpperBorder() { return Chunk.chunkSize * 2; }
    public static float GetBottomBorder() { return Chunk.chunkSize * (-1); }

    public static float GetWorkComplexityCf(WorkType wt) // time in seconds when work_cf = 1 and factory_cf = 1
    {
        switch (wt)
        {
            case WorkType.AnchorBasement_Lift: return 15000f;
            case WorkType.Recruiting: return 10000f;
            case WorkType.ShuttleConstructing: return 8000f;
            case WorkType.ObservatoryFindCycle: return 3000f;         
            case WorkType.HydroponicsLumbering: return 120f;
            case WorkType.HydroponicsFarming: return 100f;
            
            case WorkType.GearsUpgrading: return 1600f;
            case WorkType.Digging: return 30f;
            case WorkType.Pouring: return 28f;
            case WorkType.Mining: return 25f;
            case WorkType.Clearing: return 22f;            
            case WorkType.BlockBuilding:                      
            case WorkType.Factory: return 20f;
            case WorkType.OpenLumbering: return 16f;
            case WorkType.OpenFarming: return 14f;
            default: return 1f;
        }
    }
    public static float GetMaxGearsCf(ColonyController c)
    {
        byte lvl = c.hq?.level ?? 0;
        if (lvl == 0) return GEARS_LOWER_LIMIT;
        else
        {
            if (lvl < 6) return lvl;
            else return 5;
        }
    }

    public static int GetEnvironmentLayerMask()
    {
        return UnityEngine.LayerMask.NameToLayer("Environment");
    }

    public static bool NeedTutorialNote()
    {
        int i = 0;
        if (PlayerPrefs.HasKey(PP_BASE_SETTINGS_PROPERTY)) { i = PlayerPrefs.GetInt(PP_BASE_SETTINGS_PROPERTY); }
        else PlayerPrefs.SetInt(PP_BASE_SETTINGS_PROPERTY, 0);
        return ((i & 4) == 0);
    }
    public static void DisableTutorialNote()
    {
        int i = 0;
        if (PlayerPrefs.HasKey(PP_BASE_SETTINGS_PROPERTY))
        {
            i = PlayerPrefs.GetInt(PP_BASE_SETTINGS_PROPERTY);
            if ((i & 4) == 0) { i += 4; PlayerPrefs.SetInt(PP_BASE_SETTINGS_PROPERTY, i); }
        }
        else PlayerPrefs.SetInt(PP_BASE_SETTINGS_PROPERTY, 4);
        
    }
}
