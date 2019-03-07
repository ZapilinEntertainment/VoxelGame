using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public sealed class MenuUI : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Image newGameButton, loadButton, optionsButton, generateButtonImage, usePresetsButtonImage, editorButton, highscoresButton, authorsButton;
    [SerializeField] private GameObject newGamePanel, optionsPanel, graphicsApplyButton, generationPanel, terrainPresetsPanel, editorSettingPanel, 
        highscoresPanel, authorsPanel;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Dropdown difficultyDropdown, qualityDropdown, generationTypeDropdown, languageDropdown;
    [SerializeField] private Sprite overridingSprite;
    [SerializeField] private Text sizeSliderVal;
    [SerializeField] private Button gameStartButton;
    [SerializeField] private InputField editorSizeInputField;
#pragma warning restore 0649

    private bool optionsPrepared = false;

    enum MenuSection { NoSelection, NewGame, Loading, Options, Editor, Highscores, Authors }
    private MenuSection currentSection = MenuSection.NoSelection;
    private ChunkGenerationMode newGameGenMode;
    private SaveSystemUI saveSystem;
    private List<ChunkGenerationMode> availableGenerationModes;
    private string[] terrainSavenames;

    public void Start()
    {
        int k = 0;
        if (PlayerPrefs.HasKey(GameConstants.BASE_SETTINGS_PLAYERPREF)) k = PlayerPrefs.GetInt(GameConstants.BASE_SETTINGS_PLAYERPREF);

        if ( (k & 2) == 0) // first launch
        {
            LODController.SetLODdistance(0.5f);
            GameObject g = Instantiate(Resources.Load<GameObject>("UIPrefs/firstLaunchPanel"), transform);
            g.GetComponent<FirstLaunchUI>().menuScript = this;
            transform.GetChild(0).gameObject.SetActive(false);
            authorsButton.gameObject.SetActive(false);
        }
        else
        {
            if (saveSystem == null) saveSystem = SaveSystemUI.Initialize(transform.root);

            availableGenerationModes = new List<ChunkGenerationMode>() {
                ChunkGenerationMode.Standart, ChunkGenerationMode.Cube, ChunkGenerationMode.Peak
            };
            List<string> genModenames = new List<string>();
            foreach (ChunkGenerationMode cmode in availableGenerationModes) genModenames.Add(cmode.ToString());
            generationTypeDropdown.AddOptions(genModenames);
            generationTypeDropdown.value = 0;
            newGameGenMode = availableGenerationModes[0];
            LocalizeTitles();
            transform.GetChild(0).gameObject.SetActive(true);            
        }
        GameMaster.SetPause(false);
    }

    public void StartGame()
    {
        if (newGameGenMode == ChunkGenerationMode.TerrainLoading & GameMaster.savename == string.Empty) return;
        GameStartSettings gss = new GameStartSettings(newGameGenMode, (byte)sizeSlider.value, (Difficulty)difficultyDropdown.value);
        GameMaster.gameStartSettings = gss;
        GameMaster.ChangeScene(GameLevel.Playable);
    }
    public void NewGameButton()
    {
        if (currentSection == MenuSection.NewGame)    SwitchVisualSelection(MenuSection.NoSelection);
        else SwitchVisualSelection(MenuSection.NewGame);
    }
    public void OptionsButton()
    {
        if (currentSection == MenuSection.Options)  SwitchVisualSelection(MenuSection.NoSelection);
        else    SwitchVisualSelection(MenuSection.Options);
    }
    public void LoadPanelButton()
    {
        if (currentSection == MenuSection.Loading)  SwitchVisualSelection(MenuSection.NoSelection);
        else   SwitchVisualSelection(MenuSection.Loading);
    }
    public void EditorButton()
    {
        if (currentSection == MenuSection.Editor)    SwitchVisualSelection(MenuSection.NoSelection);
        else   SwitchVisualSelection(MenuSection.Editor);
    }
    public void HighscoresButton()
    {
        if (currentSection == MenuSection.Highscores) SwitchVisualSelection(MenuSection.NoSelection);
        else SwitchVisualSelection(MenuSection.Highscores);
    }
    public void AuthorsButton()
    {
        if (currentSection == MenuSection.Authors) SwitchVisualSelection(MenuSection.NoSelection);
        else SwitchVisualSelection(MenuSection.Authors);
    }
    public void ExitButton()
    {
        Application.Quit();
    }

    public void NG_GenerateButton()
    {
        newGameGenMode = availableGenerationModes[generationTypeDropdown.value];
        terrainPresetsPanel.SetActive(false);
        terrainSavenames = null;
        usePresetsButtonImage.overrideSprite = null;
        generationPanel.SetActive(true);
        generateButtonImage.overrideSprite = overridingSprite;
        gameStartButton.interactable = true;        

    }
    public void NG_UsePresetButton()
    {
        newGameGenMode = ChunkGenerationMode.TerrainLoading;
        GameMaster.SetSavename(string.Empty);
        generationPanel.SetActive(false);
        generateButtonImage.overrideSprite = null;
        gameStartButton.interactable = false;

        //preparing terrains list
        string directoryPath = SaveSystemUI.GetTerrainsPath();
        Transform contentHolder = terrainPresetsPanel.transform.GetChild(0).GetChild(0);
        RectTransform zeroButton = contentHolder.GetChild(0).GetComponent<RectTransform>();
        bool no_saves_found = true;
        if (Directory.Exists(directoryPath))
        {
            terrainSavenames = Directory.GetFiles(directoryPath, "*." + SaveSystemUI.TERRAIN_FNAME_EXTENSION);
            if (terrainSavenames.Length != 0)
            {
                no_saves_found = false;
                int c = contentHolder.childCount;
                int i = 0;
                zeroButton.GetComponent<Button>().interactable = true;
                for (; i < terrainSavenames.Length; i++)
                {
                    string s = terrainSavenames[i];
                    int lastSlashPos = s.LastIndexOf('\\'); // в редакторе так
                    if (lastSlashPos == -1) lastSlashPos = s.LastIndexOf('/');
                    terrainSavenames[i] = s.Substring(lastSlashPos + 1, s.Length - lastSlashPos - 5); //  от последнего слеша до ".sav"
                    Transform t;
                    if (i < c)
                    {
                        t = contentHolder.GetChild(i); // 0 - example
                    }
                    else
                    {
                        t = Instantiate(zeroButton, contentHolder).transform;
                        t.transform.localPosition = zeroButton.localPosition + Vector3.down * (zeroButton.rect.height * i + 16);
                    }
                    t.gameObject.SetActive(true);
                    t.GetComponent<Button>().onClick.RemoveAllListeners(); // т.к. на example тоже может висеть listener
                    t.GetChild(0).GetComponent<Text>().text = terrainSavenames[i];
                    int index = i;
                    t.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        this.NG_SelectTerrain(index);
                    });
                    t.GetComponent<Image>().overrideSprite = null;
                }
                if (i < c)
                {
                    i++;
                    for (; i < c; i++)
                    {
                        contentHolder.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }
        if (no_saves_found)
        {
            terrainSavenames = null;
            zeroButton.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoSavesFound);
            zeroButton.GetComponent<Button>().interactable = false;
            zeroButton.GetComponent<Image>().overrideSprite = null;
            zeroButton.gameObject.SetActive(true);
            if (contentHolder.childCount > 1)
            {
                while (contentHolder.childCount > 1)
                {
                    Destroy(contentHolder.GetChild(1));
                }
            }
        }
        terrainPresetsPanel.SetActive(true);
        usePresetsButtonImage.overrideSprite = overridingSprite;
    }
    public void NG_SelectTerrain(int index)
    {
       if (terrainSavenames == null || terrainSavenames.Length <= index)
        {
            GameMaster.SetSavename(string.Empty);
            gameStartButton.interactable = false;
        }
       else
        {
            GameMaster.SetSavename( terrainSavenames[index] );
            gameStartButton.interactable = true;
            Transform buttonsHolder = terrainPresetsPanel.transform.GetChild(0).GetChild(0);
            for (int i = 0; i < buttonsHolder.childCount; i++)
            {
                buttonsHolder.GetChild(i).GetComponent<Image>().overrideSprite = ((i == index) ? overridingSprite : null );
            }
        }
    }
    public void NG_SetGenMode()
    {
        newGameGenMode = availableGenerationModes[generationTypeDropdown.value];
    }

    public void Editor_InputFieldValueChanged()
    {
        int x = int.Parse(editorSizeInputField.text);
        if (x < 0)
        {
            x *= -1;
            editorSizeInputField.text = x.ToString();
        }
        if (x != Chunk.CHUNK_SIZE) Chunk.SetChunkSizeValue((byte)x);
    }
    public void Editor_SizePlusButton()
    {
        if (Chunk.CHUNK_SIZE < 99)
        {
            Chunk.SetChunkSizeValue((byte)(Chunk.CHUNK_SIZE + 1));
            editorSizeInputField.text = Chunk.CHUNK_SIZE.ToString();
        }
    }
    public void Editor_SizeMinusButton()
    {
        if (Chunk.CHUNK_SIZE > Chunk.MIN_CHUNK_SIZE) {
            Chunk.SetChunkSizeValue((byte)(Chunk.CHUNK_SIZE - 1));
            editorSizeInputField.text = Chunk.CHUNK_SIZE.ToString();
        }
    }
    public void Editor_Start()
    {
        GameMaster.ChangeScene(GameLevel.Editor);
    }

    public void QualityDropdownChanged()
    {
        graphicsApplyButton.SetActive(qualityDropdown.value != QualitySettings.GetQualityLevel());
    }
    public void ApplyGraphicsButton()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        graphicsApplyButton.SetActive(false);
    }
    public void Options_ChangeLanguage(int i)
    {
        Localization.ChangeLanguage((Language)i);
        int x = 0;
        if (PlayerPrefs.HasKey(GameConstants.BASE_SETTINGS_PLAYERPREF)) x = PlayerPrefs.GetInt(GameConstants.BASE_SETTINGS_PLAYERPREF);
        if (i == 1) { if ((x & 1) == 0) x += 1; }
        else
        {
            if ((x & 1) == 1) x -= 1;
        }
        PlayerPrefs.SetInt(GameConstants.BASE_SETTINGS_PLAYERPREF, x);
        PlayerPrefs.Save();

        transform.root.BroadcastMessage("LocalizeTitles", SendMessageOptions.DontRequireReceiver);
    }

    public void SizeValueChanged(float f)
    {
        sizeSliderVal.text = ((int)f).ToString();
    }

    void SwitchVisualSelection(MenuSection ms)
    {
        if (ms == currentSection) return;
        if (currentSection != MenuSection.NoSelection)
        {
            switch (currentSection)
            {
                case MenuSection.NewGame:
                    newGameButton.overrideSprite = null;
                    newGamePanel.SetActive(false);
                    break;
                case MenuSection.Loading:
                    loadButton.overrideSprite = null;
                    saveSystem.CloseButton();
                    break;
                case MenuSection.Options:
                    optionsButton.overrideSprite = null;
                    optionsPanel.SetActive(false);
                    break;
                case MenuSection.Editor:
                    editorButton.overrideSprite = null;
                    editorSettingPanel.SetActive(false);
                    break;
                case MenuSection.Highscores:
                    highscoresButton.overrideSprite = null;
                    highscoresPanel.SetActive(false);
                    break;
                case MenuSection.Authors:
                    authorsButton.overrideSprite = null;
                    authorsPanel.SetActive(false);
                    authorsPanel.transform.GetChild(0).GetComponent<Text>().text = string.Empty;
                    break;
            }
        }
        currentSection = ms;
        switch (currentSection)
        {
            case MenuSection.NewGame:
                newGameButton.overrideSprite = overridingSprite;
                newGamePanel.SetActive(true);
                NG_GenerateButton();
                break;
            case MenuSection.Loading:
                loadButton.overrideSprite = overridingSprite;
                saveSystem.Activate(false, false);
                break;
            case MenuSection.Options:
                if (!optionsPrepared)
                {
                    var options = new List<Dropdown.OptionData>();
                    foreach (string s in QualitySettings.names)
                    {
                        options.Add(new Dropdown.OptionData(s));
                    }
                    qualityDropdown.options = options;                    

                    options = new List<Dropdown.OptionData>() { new Dropdown.OptionData("English"), new Dropdown.OptionData("Русский") };
                    languageDropdown.options = options;

                    optionsPrepared = true;
                }
                qualityDropdown.value = QualitySettings.GetQualityLevel();
                graphicsApplyButton.SetActive(false);
                languageDropdown.value = Localization.currentLanguage == Language.English ? 0 : 1;
                optionsButton.overrideSprite = overridingSprite;
                optionsPanel.SetActive(true);
                break;
            case MenuSection.Editor:
                editorButton.overrideSprite = overridingSprite;
                editorSizeInputField.text = Chunk.CHUNK_SIZE.ToString();
                editorSettingPanel.SetActive(true);
                break;
            case MenuSection.Highscores:
                highscoresButton.overrideSprite = overridingSprite;
                RectTransform exampleItem = highscoresPanel.transform.GetChild(1).GetComponent<RectTransform>();
                var highscores = Highscore.GetHighscores();
                if (highscores == null)
                {
                    exampleItem.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoHighscores);
                    exampleItem.GetChild(1).GetComponent<Text>().text = string.Empty;
                    exampleItem.GetChild(2).GetComponent<RawImage>().enabled = false;
                    exampleItem.gameObject.SetActive(true);
                }
                else
                {
                    Highscore h = highscores[0];
                    exampleItem.GetChild(0).GetComponent<Text>().text = h.colonyName;
                    exampleItem.GetChild(1).GetComponent<Text>().text = ((int)h.score).ToString();
                    RawImage ri = exampleItem.GetChild(2).GetComponent<RawImage>();
                    ri.uvRect = GetEndGameIconRect(h.endType);
                    ri.enabled = true;
                    exampleItem.gameObject.SetActive(true);
                    if (highscores.Length > 1)
                    {
                        for (int i = 1; i < highscores.Length; i++)
                        {
                            RectTransform rt = Instantiate(exampleItem, exampleItem.parent);
                            rt.localPosition += Vector3.down * exampleItem.rect.height * (i - 1);
                            h = highscores[i];
                            rt.GetChild(0).GetComponent<Text>().text = h.colonyName;
                            rt.GetChild(1).GetComponent<Text>().text = ((int)h.score).ToString();
                            ri = rt.GetChild(2).GetComponent<RawImage>();
                            ri.uvRect = GetEndGameIconRect(h.endType);
                            ri.enabled = true;
                        }
                    }
                }
                highscoresPanel.SetActive(true);
                break;
            case MenuSection.Authors:
                authorsPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetCredits();
                authorsPanel.SetActive(true);
                authorsButton.overrideSprite = overridingSprite;
                break;
        }
    }

    private Rect GetEndGameIconRect(GameEndingType endType)
    {
        switch (endType)
        {
            case GameEndingType.ColonyLost: return new Rect(0.5f, 0.75f, 0.25f, 0.25f);
            case GameEndingType.TransportHubVictory: return new Rect(0.75f, 0.75f, 0.25f, 0.25f);            
            case GameEndingType.ConsumedByReal: return new Rect(0, 0.75f, 0.25f,0.25f);
            case GameEndingType.ConsumedByLastSector: return new Rect( 0.25f, 0.75f, 0.25f, 0.25f);
            default: return Rect.zero;
        }
    }

    private void LocalizeTitles()
    {
        newGameButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NewGame);
        generateButtonImage.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Generate);
        usePresetsButtonImage.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.UsePresets);
        Transform t = generationPanel.transform;
        t.GetChild(0).GetChild(4).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Size);
        t.GetChild(1).GetChild(3).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.GenerationType);
        difficultyDropdown.transform.GetChild(3).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Difficulty);
        newGamePanel.transform.GetChild(6).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Start);

        loadButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Load);

        optionsButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Options);
        optionsPanel.transform.GetChild(0).GetChild(3).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Language);
        optionsPanel.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Quality);
        optionsPanel.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Apply);

        editorButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Editor);
        editorSettingPanel.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Size);
        editorSettingPanel.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Start);

        highscoresButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Highscores);
        authorsButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Info);
        transform.GetChild(0).GetChild(6).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Exit);
    }
}
