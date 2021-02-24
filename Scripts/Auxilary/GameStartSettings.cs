public enum ScenarioType : byte { Unrecognised, Embedded}
public enum EmbeddedScenarioType : byte { Tutorial}

public sealed class GameStartSettings : MyObject
{
    private GameMode gameMode;
    private bool loadGame = false, loadTerrain = false;
    private string savename;
    private byte subIndex0 = 255, subIndex1 = 255;
    private Difficulty difficulty = Difficulty.Normal;
    private ChunkGenerationSettings chunkGenerationSettings;

    private GameStartSettings() { }
    public static GameStartSettings GetDefaultStartSettings()
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = GameMode.Survival;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetDefaultSettings();
        return cgs;
    }
    public static GameStartSettings GetEditorStartSettings(byte i_size)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = GameMode.Editor;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetGenerationSettings(ChunkGenerationMode.EditorPreset, i_size);
        return cgs;
    }
    /// <summary>
    /// starts new editor session and loads terrain preset
    /// </summary>
    public static GameStartSettings GetEditorStartSettings(string i_savename)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = GameMode.Editor;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetLoadingSettings(i_savename);
        return cgs;
    }
    public static GameStartSettings GetStartSettings(GameMode i_mode, ChunkGenerationSettings i_chunkSettings, Difficulty i_difficulty, StartFoundingType i_start)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = i_mode;
        cgs.chunkGenerationSettings = i_chunkSettings;
        cgs.difficulty = i_difficulty;
        cgs.subIndex0 = (byte)i_start;
        return cgs;
    }
    public static GameStartSettings GetModeChangingSettings(GameMode newMode, Difficulty i_difficulty, StartFoundingType i_foundingType)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = newMode;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetNoActionSettings();
        cgs.difficulty = i_difficulty;
        cgs.subIndex0 = (byte)i_foundingType;
        return cgs;
    }
    public static GameStartSettings GetLoadingSettings(GameMode i_gameMode, string i_savename)
    {
        var cgs = new GameStartSettings();
        cgs.gameMode = i_gameMode;
        cgs.loadGame = true;
        cgs.chunkGenerationSettings = ChunkGenerationSettings.GetNoActionSettings();
        cgs.savename = i_savename;
        return cgs;
    }
    public static GameStartSettings GetTutorialSettings()
    {
        var gss = new GameStartSettings();
        gss.gameMode = GameMode.Scenario;
        gss.subIndex0 = (byte)ScenarioType.Embedded;
        gss.subIndex1 = (byte)EmbeddedScenarioType.Tutorial;
        gss.loadTerrain = true;
        gss.savename = "tutorialTerrain";
        return gss;
    }

    public bool NeedLoading() { return loadGame; }
    public GameMode DefineGameMode()
    {
        return gameMode;
    }
    public Difficulty DefineDifficulty()
    {
        return difficulty;
    }
    public string GetSavename()
    {
        return savename ?? string.Empty;
    }
    public string GetSavenameFullpath()
    {
        if (savename != null)
        {
            switch (gameMode)
            {
                case GameMode.Survival:
                    if (chunkGenerationSettings.preparingActionMode != ChunkPreparingAction.Load)
                        return SaveSystemUI.GetGameSaveFullpath(savename);
                    else return SaveSystemUI.GetTerrainSaveFullpath(savename);
                case GameMode.Editor: return SaveSystemUI.GetTerrainSaveFullpath(savename);
                default: if (loadTerrain) return SaveSystemUI.GetTerrainSaveFullpath(savename);
                    else
                        return SaveSystemUI.GetGameSaveFullpath(savename);
            }
        }
        return string.Empty;
    }
    public StartFoundingType DefineFoundingType()
    {
        if (subIndex0 == 255) return StartFoundingType.Nothing;
        else
        {
            return (StartFoundingType)subIndex0;
        }
    }
    public ChunkGenerationSettings GetChunkGenerationSettings() { return chunkGenerationSettings ?? ChunkGenerationSettings.GetDefaultSettings(); }
    public ScenarioType GetScenarioType()
    {
        return subIndex0 == 255 ? ScenarioType.Unrecognised : (ScenarioType)subIndex0;
    }
    public byte GetSecondSubIndex() { return subIndex1; }

    protected override bool IsEqualNoCheck(object obj)
    {
        var gss = (GameStartSettings)obj;
        if (gameMode == gss.gameMode)
        {
            if (gameMode == GameMode.MainMenu) return true;
            else
            {
                if (loadGame && gss.loadGame) return chunkGenerationSettings == gss.chunkGenerationSettings && savename == gss.savename;
                else
                {
                    if (gss.loadGame != loadGame) return false;
                    else
                    {
                        switch (gameMode)
                        {
                            case GameMode.Survival:
                                return chunkGenerationSettings == gss.chunkGenerationSettings && difficulty == gss.difficulty && subIndex0 == gss.subIndex0;
                            case GameMode.Scenario:
                                return chunkGenerationSettings == gss.chunkGenerationSettings && subIndex0 == gss.subIndex0 && subIndex1 == gss.subIndex1 && difficulty == gss.difficulty && savename == gss.savename;
                            case GameMode.Editor:
                                return chunkGenerationSettings == gss.chunkGenerationSettings && savename == gss.savename;
                            default: return true;
                        }
                    }
                }
            }
        }
        else return false;
    }
}
