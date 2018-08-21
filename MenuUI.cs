using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

enum MenuSection { NoSelection, NewGame, Loading, Options}
public class MenuUI : MonoBehaviour {
    int currentGraphicsLevel = 0, lastSelectedSaveButton = -1;
    [SerializeField] Image newGameButton, loadButton, optionsButton;
    [SerializeField] GameObject newGamePanel, optionsPanel, loadingPanel, graphicsApplyButton, innerLoadButton;
    [SerializeField] Slider sizeSlider, roughnessSlider;
    [SerializeField] Dropdown difficultyDropdown, qualityDropdown;
    [SerializeField] Sprite overridingSprite;
    [SerializeField] Text sizeSliderVal, roughSliderVal, save_cityNameString, save_dateString;
    [SerializeField] RectTransform saveStringsContainer;

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
            loadingPanel.SetActive(false);
        }
        else
        {
            SwitchVisualSelection(MenuSection.Loading);
            loadingPanel.SetActive(true);
            string path = Application.persistentDataPath + "/Saves";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string[] saveNames = Directory.GetFiles(path, "*.sav");
            if (saveNames.Length > 0)
            {
                int i = 0;
                while (i < saveNames.Length)
                {
                    Transform t = saveStringsContainer.GetChild(i);
                    string nm = saveNames[i];
                    if (t == null)
                    {
                        Transform original = saveStringsContainer.GetChild(0);
                        t = Instantiate(original, saveStringsContainer);
                        t.transform.localPosition = original.transform.position + Vector3.down * i;
                        int index = i;
                        t.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            this.SelectSave(nm, index);
                        });                        
                    }
                    else
                    {
                        Button b = t.GetComponent<Button>();
                        b.onClick.RemoveAllListeners();
                        int index = i;
                        b.onClick.AddListener(() =>
                        {
                            this.SelectSave(nm, index);
                        });
                        t.gameObject.SetActive(true);
                        t.GetComponent<Image>().overrideSprite = null;
                    }
                    t.GetChild(0).GetComponent<Text>().text = nm;
                    i++;
                }
                if (i < saveStringsContainer.childCount)
                {
                    for (; i < saveStringsContainer.childCount; i++)
                    {
                        saveStringsContainer.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Transform t = saveStringsContainer.GetChild(0);
                t.gameObject.SetActive(true);
                t.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoSavesFound);
                if (saveStringsContainer.childCount > 1)
                {
                    for (int i = 1; i < saveStringsContainer.childCount; i++)
                    {
                        saveStringsContainer.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            innerLoadButton.SetActive(false);
        }
        
    }
    public void SelectSave(string nm, int index)
    {
        Transform t;
        if (lastSelectedSaveButton != -1)
        {
            t = saveStringsContainer.GetChild(lastSelectedSaveButton);
            if (t != null)
            {
                t.GetComponent<Image>().overrideSprite = null;
            }
        }
        t = saveStringsContainer.GetChild(index);
        t.GetComponent<Image>().overrideSprite = overridingSprite;
        save_cityNameString.text = "There must be the saved city name";
        save_dateString.text = File.GetCreationTime( Application.persistentDataPath + "/Saves/" + nm ).ToString();
        innerLoadButton.SetActive(true);
        lastSelectedSaveButton = index;
    }
    public void LoadSelectedSave()
    {
        Transform t = saveStringsContainer.GetChild(lastSelectedSaveButton);
        string fullPath = Application.persistentDataPath + "/Saves/" + t.GetChild(0).GetComponent<Text>();
        if (File.Exists(fullPath))
        {
            GameMaster.savename = fullPath;
            GameStartSettings gss = new GameStartSettings(false);
            GameMaster.gss = gss;
            SceneManager.LoadScene(1);
        }
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
                    loadingPanel.SetActive(false);
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
