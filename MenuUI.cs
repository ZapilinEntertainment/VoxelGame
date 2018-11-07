using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public sealed class MenuUI : MonoBehaviour
{
    private int currentGraphicsLevel = 0;
#pragma warning disable 0649
    [SerializeField] private Image newGameButton, loadButton, optionsButton, generateButtonImage, usePresetsButtonImage;
    [SerializeField] private GameObject newGamePanel, optionsPanel, graphicsApplyButton, generationPanel, terrainPresetsPanel;
    [SerializeField] private Slider sizeSlider, roughnessSlider;
    [SerializeField] private Dropdown difficultyDropdown, qualityDropdown, generationTypeDropdown;
    [SerializeField] private Sprite overridingSprite;
    [SerializeField] private Text sizeSliderVal, roughSliderVal;
    [SerializeField] private Button gameStartButton;
#pragma warning restore 0649

    enum MenuSection { NoSelection, NewGame, Loading, Options }
    private MenuSection currentSection = MenuSection.NoSelection;
    private ChunkGenerationMode newGameGenMode;
    private List<ChunkGenerationMode> availableGenerationModes;
    private string[] terrainSavenames;

    private void Start()
    {
        currentGraphicsLevel = QualitySettings.GetQualityLevel();
        SaveSystemUI.Check(transform.root);

        availableGenerationModes = new List<ChunkGenerationMode>() { ChunkGenerationMode.Standart, ChunkGenerationMode.Pyramid };
        List<string> genModenames = new List<string>();
        foreach (ChunkGenerationMode cmode in availableGenerationModes) genModenames.Add(cmode.ToString());
        generationTypeDropdown.AddOptions(genModenames);
        generationTypeDropdown.value = 0;
        newGameGenMode = availableGenerationModes[0];

        Time.timeScale = 1;
    }

    public void StartGame()
    {
        if (newGameGenMode == ChunkGenerationMode.TerrainLoading & GameMaster.savename == string.Empty) return;
        GameStartSettings gss = new GameStartSettings(newGameGenMode, (byte)sizeSlider.value, (Difficulty)difficultyDropdown.value, roughnessSlider.value);
        GameMaster.gameStartSettings = gss;
        SceneManager.LoadScene(SaveSystemUI.GAME_LEVEL_NUMBER);
    }
    public void NGPanelButton()
    {
        if (currentSection == MenuSection.NewGame)
        {
            SwitchVisualSelection(MenuSection.NoSelection);
            newGamePanel.SetActive(false);
        }
        else
        {
            SwitchVisualSelection(MenuSection.NewGame);
            newGamePanel.SetActive(true);
        }
    }
    public void OptionsButton()
    {
        if (currentSection == MenuSection.Options)
        {
            SwitchVisualSelection(MenuSection.NoSelection);
            optionsPanel.SetActive(false);
        }
        else
        {
            SwitchVisualSelection(MenuSection.Options);
            optionsPanel.SetActive(true);
        }
    }
    public void LoadPanelButton()
    {
        if (currentSection == MenuSection.Loading)
        {
            SwitchVisualSelection(MenuSection.NoSelection);
            SaveSystemUI.current.gameObject.SetActive(false);
        }
        else
        {
            SwitchVisualSelection(MenuSection.Loading);
            SaveSystemUI.current.Activate(false, false);
        }

    }
    public void EditorButton()
    {
        SceneManager.LoadScene(SaveSystemUI.EDITOR_LEVEL_NUMBER);
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

    public void QualityDropdownChanged()
    {
        graphicsApplyButton.SetActive(qualityDropdown.value != currentGraphicsLevel);
    }
    public void ApplyGraphicsButton()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        graphicsApplyButton.SetActive(false);
    }
    public void SizeValueChanged(float f)
    {
        sizeSliderVal.text = ((int)f).ToString();
    }
    public void RoughnessValueChanged(float f)
    {
        roughSliderVal.text = string.Format("{0:0.###}", f);
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
                    SaveSystemUI.current.gameObject.SetActive(false);
                    break;
                case MenuSection.Options:
                    optionsButton.overrideSprite = null;
                    optionsPanel.SetActive(false);
                    break;
            }
        }
        currentSection = ms;
        switch (currentSection)
        {
            case MenuSection.NewGame:
                newGameButton.overrideSprite = overridingSprite;
                NG_GenerateButton();
                break;
            case MenuSection.Loading: loadButton.overrideSprite = overridingSprite; break;
            case MenuSection.Options: optionsButton.overrideSprite = overridingSprite; break;
        }
    }
}
