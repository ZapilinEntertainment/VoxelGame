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
    [SerializeField] private Image newGameButton, loadButton, optionsButton, generateButtonImage, loadPresetButtonImage, standartGenButtonImage, cubeGenButtonImage;
    [SerializeField] private GameObject newGamePanel, optionsPanel, graphicsApplyButton;
    [SerializeField] private Slider sizeSlider, roughnessSlider;
    [SerializeField] private Dropdown difficultyDropdown, qualityDropdown;
    [SerializeField] private Sprite overridingSprite;
    [SerializeField] private Text sizeSliderVal, roughSliderVal;
#pragma warning restore 0649

    enum MenuSection { NoSelection, NewGame, Loading, Options }
    private MenuSection currentSection = MenuSection.NoSelection;
    private ChunkGenerationMode newGameGenMode = ChunkGenerationMode.Standart;


    private void Start()
    {
        currentGraphicsLevel = QualitySettings.GetQualityLevel();
        SaveSystemUI.Check(transform.root);
    }

    public void StartGame()
    {
        GameStartSettings gss = new GameStartSettings(newGameGenMode, (byte)sizeSlider.value, (Difficulty)difficultyDropdown.value, roughnessSlider.value);
        GameMaster.gss = gss;
        GameMaster.savename = string.Empty;
        SceneManager.LoadScene(1);
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
            case MenuSection.NewGame: newGameButton.overrideSprite = overridingSprite; break;
            case MenuSection.Loading: loadButton.overrideSprite = overridingSprite; break;
            case MenuSection.Options: optionsButton.overrideSprite = overridingSprite; break;
        }
    }
}
