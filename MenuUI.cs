using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public class MenuUI : MonoBehaviour {
    int currentGraphicsLevel = 0, lastSelectedSaveButton = -1;
    [SerializeField] Image newGameButton, loadButton, optionsButton;
    [SerializeField] GameObject newGamePanel, optionsPanel, graphicsApplyButton;
    [SerializeField] Slider sizeSlider, roughnessSlider;
    [SerializeField] Dropdown difficultyDropdown, qualityDropdown;
    [SerializeField] Sprite overridingSprite;
    [SerializeField] Text sizeSliderVal, roughSliderVal;

    enum MenuSection { NoSelection, NewGame, Loading, Options }
    MenuSection currentSection = MenuSection.NoSelection;

    private void Start()
    {
        currentGraphicsLevel = QualitySettings.GetQualityLevel();
    }

    public void StartGame()
    {
        GameStartSettings gss = new GameStartSettings(true, (byte)sizeSlider.value, (Difficulty)difficultyDropdown.value, roughnessSlider.value);
        GameMaster.gss = gss;
        GameMaster.savename = string.Empty;
        SceneManager.LoadScene(1);
    }
    public void NGPanelButton()
    {
        if (currentSection == MenuSection.NewGame) {
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
            SaveSystemUI.Check(transform.root);
            SaveSystemUI.current.Activate(false);
        }
        
    }
    public void ExitButton()
    {
        Application.Quit();
    }

    public void QualityDropdownChanged() {
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
