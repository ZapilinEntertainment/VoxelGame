using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOptionsSubpanel : MonoBehaviour
{
    [SerializeField] private Transform buttonsHolder,savePanelSpace;
    [SerializeField] private GameObject optionsPanel;

    private enum MenuSection { NoSelection, Save, Load, Options, Advice }
    private MenuSection selectedMenuSection = MenuSection.NoSelection;
    private SaveSystemUI saveSystem;
    private const int SAVE_BTN_INDEX = 0, LOAD_BTN_INDEX = 1, OPTIONS_BTN_INDEX = 2, ADVICE_BTN_INDEX = 3, MAIN_MENU_BTN_INDEX = 4, EXIT_BTN_INDEX = 5;

    private void Start()
    {
        if (saveSystem == null) saveSystem = SaveSystemUI.Initialize(savePanelSpace);
        LocalizeTitles();
        if (optionsPanel.activeSelf) optionsPanel.SetActive(false);        
    }

    private void OnEnable()
    {
        var btn = buttonsHolder.GetChild(SAVE_BTN_INDEX);
        bool x = GameMaster.realMaster.CanWeSaveTheGame();
        btn.GetComponent<Button>().interactable = x;
        btn.GetChild(0).GetComponent<Text>().color = x ? Color.white : Color.gray; 
    }
    private void OnDisable()
    {
        SetMenuPanelSelection(MenuSection.NoSelection);
    }

    public void SaveButton()
    {
        saveSystem.Activate(true, false);
        SetMenuPanelSelection(MenuSection.Save);
    }
    public void LoadButton()
    {
        saveSystem.Activate(false, false);
        SetMenuPanelSelection(MenuSection.Load);
    }
    public void OptionsButton()
    {
        optionsPanel.SetActive(true);
        SetMenuPanelSelection(MenuSection.Options);
    }
    public void AdviceButton()
    {
        SetMenuPanelSelection(MenuSection.Advice);
    }
    public void ToMainMenu()
    {
        AnnouncementCanvasController.EnableDecisionWindow(
            Localization.GetPhrase(LocalizedPhrase.AskReturnToMainMenu),
            this.ReturnToMainMenu,
            Localization.GetWord(LocalizedWord.Yes),
            AnnouncementCanvasController.DisableDecisionPanel,
            Localization.GetWord(LocalizedWord.No),
            true
            );
    }
    private void ReturnToMainMenu()
    {
        if (GameMaster.realMaster.colonyController != null && GameMaster.realMaster.CanWeSaveTheGame()) GameMaster.realMaster.SaveGame("autosave");
        SetMenuPanelSelection(MenuSection.NoSelection);
        GameMaster.ReturnToMainMenu();
    }
    public void ExitButton()
    {
        AnnouncementCanvasController.EnableDecisionWindow(
            Localization.GetPhrase(LocalizedPhrase.AskExit),
            this.ExitGame,
            Localization.GetWord(LocalizedWord.Yes),
            AnnouncementCanvasController.DisableDecisionPanel,
            Localization.GetWord(LocalizedWord.No),
            true
            );
    }
    private void ExitGame()
    {
        if (GameMaster.realMaster.colonyController != null) GameMaster.realMaster.SaveGame("autosave");
        SetMenuPanelSelection(MenuSection.NoSelection);
        Application.Quit();
    }

    void SetMenuPanelSelection(MenuSection ms)
    {
        if (ms == selectedMenuSection) return;
        else
        {
            switch (selectedMenuSection)
            {
                case MenuSection.Save:
                    buttonsHolder.GetChild(SAVE_BTN_INDEX).GetComponent<Image>().overrideSprite = null;
                    saveSystem.CloseButton();
                    break;
                case MenuSection.Load:
                    buttonsHolder.GetChild(LOAD_BTN_INDEX).GetComponent<Image>().overrideSprite = null;
                    saveSystem.CloseButton();
                    break;
                case MenuSection.Options:
                    buttonsHolder.GetChild(OPTIONS_BTN_INDEX).GetComponent<Image>().overrideSprite = null;
                    optionsPanel.SetActive(false);
                    break;
            }
            selectedMenuSection = ms;
            switch (selectedMenuSection)
            {
                case MenuSection.Save: buttonsHolder.GetChild(SAVE_BTN_INDEX).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case MenuSection.Load: buttonsHolder.GetChild(LOAD_BTN_INDEX).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite; break;
                case MenuSection.Options:
                    buttonsHolder.GetChild(OPTIONS_BTN_INDEX).GetComponent<Image>().overrideSprite = PoolMaster.gui_overridingSprite;
                    break;
                case MenuSection.NoSelection:
                    break;
            }
        }
    }

    private void LocalizeTitles()
    {
        buttonsHolder.GetChild(SAVE_BTN_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Save);
        buttonsHolder.GetChild(LOAD_BTN_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Load);
        buttonsHolder.GetChild(OPTIONS_BTN_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Options);
        buttonsHolder.GetChild(ADVICE_BTN_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Advice);
        buttonsHolder.GetChild(MAIN_MENU_BTN_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.MainMenu);
        buttonsHolder.GetChild(EXIT_BTN_INDEX).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Exit);

        optionsPanel.GetComponent<GameSettingsUI>().LocalizeTitles();
    }
}
