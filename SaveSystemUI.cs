using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveSystemUI : MonoBehaviour {
    public static SaveSystemUI current {get;private set;}
    public bool saveMode = false, ingame = false;

    private bool deleteSubmit = false, terrainsLoading = false;
    string[] saveNames;    
    [SerializeField] RectTransform exampleButton, saveNamesContainer; // fiti
    [SerializeField] Text saveLoadButtonText, deleteButtonText, submitButtonText, rejectButtonText, submitQuestionText, saveDateString; // fiti
    [SerializeField] GameObject submitWindow, inputFieldPanel;
    private int lastSelectedIndex = -1;

    public const string SAVE_FNAME_EXTENSION = "sav", TERRAIN_FNAME_EXTENSION = "itd"; // island terrain data
    private const int GAME_LEVEL_NUMBER = 1, EDITOR_LEVEL_NUMBER = 2;

    public static void Check(Transform canvas)
    {
        if (current == null)
        {
            GameObject mainContainer = Instantiate(Resources.Load<GameObject>("UIPrefs/SaveSystemModule"), canvas);
            current = mainContainer.GetComponent<SaveSystemUI>();
        }
    }

    private void Awake()
    {
        if (current != null & current != this) Destroy(current);
        current = this;
    }

    public void Activate(bool openSaveMode, bool i_terrainsLoading)
    {
        gameObject.SetActive(true);
       if (inputFieldPanel.activeSelf) inputFieldPanel.SetActive(false);
        if (submitWindow.activeSelf) submitWindow.SetActive(false);
        lastSelectedIndex = -1;
        saveLoadButtonText.transform.parent.GetComponent<Button>().interactable = false;
        deleteButtonText.transform.parent.GetComponent<Button>().interactable = false;
        deleteSubmit = false;
        saveDateString.enabled = false;

        saveMode = openSaveMode;
        terrainsLoading = i_terrainsLoading;
        RefreshSavesList();
    }

    void RefreshSavesList()
    {
        string directoryPath = terrainsLoading ? Application.persistentDataPath + "/Terrains" : Application.persistentDataPath + "/Saves";
        if (saveMode)
        {
            saveMode = true;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                saveNames = new string[0];
            }
            else
            {
                saveNames = Directory.GetFiles(directoryPath, "*." + (terrainsLoading ? TERRAIN_FNAME_EXTENSION : SAVE_FNAME_EXTENSION));
            }
            exampleButton.gameObject.SetActive(true);
            exampleButton.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.CreateNewSave);
            exampleButton.GetComponent<Button>().interactable = true;
            exampleButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                this.CreateNewSave();
            });
            saveLoadButtonText.text = Localization.GetWord(LocalizedWord.Save);
        }
        else
        {
            saveMode = false; // load mode enabled
            if (!Directory.Exists(directoryPath))
            {
                saveNames = new string[0];
            }
            else
            {
                saveNames = Directory.GetFiles(directoryPath, "*." + (terrainsLoading ? TERRAIN_FNAME_EXTENSION : SAVE_FNAME_EXTENSION));
            }
            if (saveNames.Length == 0)
            {
                exampleButton.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NoSavesFound);
                exampleButton.GetComponent<Button>().interactable = false;
                exampleButton.gameObject.SetActive(true);
            }
            else
            {
                exampleButton.gameObject.SetActive(false);
            }
            saveLoadButtonText.text = Localization.GetWord(LocalizedWord.Load);
        }

        int c = saveNamesContainer.childCount;
        if (saveNames.Length > 0)
        {
            int i = 0;
            for (; i < saveNames.Length; i++)
            {
                string s = saveNames[i];
                int lastSlashPos = s.LastIndexOf('\\'); // в редакторе так
                if (lastSlashPos == -1) lastSlashPos = s.LastIndexOf('/');
                saveNames[i] = s.Substring(lastSlashPos + 1, s.Length - lastSlashPos - 5); //  от последнего слеша до ".sav"
                Transform t;
                if (i + 1 < c)
                {
                    t = saveNamesContainer.GetChild(i + 1); // 0 - example
                }
                else 
                {
                    t = Instantiate(exampleButton, saveNamesContainer).transform;
                    t.transform.localPosition = exampleButton.localPosition + Vector3.down * (exampleButton.rect.height * (i + 1) + 16) ;                    
                }
                t.gameObject.SetActive(true);
                t.GetComponent<Button>().onClick.RemoveAllListeners(); // т.к. на example тоже может висеть listener
                t.GetChild(0).GetComponent<Text>().text = saveNames[i];
                int index = i ;
                t.GetComponent<Button>().onClick.AddListener(() =>
                {
                    this.SelectSave(index);
                });
            }
            if ( i < c)
            {
                i++;
                for (; i < c; i++) {
                    saveNamesContainer.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (c > 1)
            {
                for (int i = 1; i < c; i++)
                {
                    saveNamesContainer.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }

    public void CreateNewSave()
    {
        inputFieldPanel.SetActive(true);
        inputFieldPanel.GetComponent<InputField>().text = terrainsLoading ? "new terrain" : GameMaster.colonyController.cityName;
    }
    public void InputField_SaveGame()
    {
        if (terrainsLoading) GameMaster.realMaster.SaveTerrain(inputFieldPanel.GetComponent<InputField>().text);
        else GameMaster.realMaster.SaveGame(inputFieldPanel.GetComponent<InputField>().text);
        inputFieldPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SelectSave(int index) // индекс имени, индекс среди child будет index+1
    {
        Transform t;
        if (lastSelectedIndex != -1)
        {
            t = saveNamesContainer.GetChild(lastSelectedIndex +1);
            if (t != null)
            {
                t.GetComponent<Image>().color = Color.white;
                t.GetChild(0).GetComponent<Text>().color = Color.white;
            }
        }
        t = saveNamesContainer.GetChild(index +1);
        t.GetComponent<Image>().color = Color.black;
        t.GetChild(0).GetComponent<Text>().color = Color.cyan;
        lastSelectedIndex = index;
        saveLoadButtonText.transform.parent.GetComponent<Button>().interactable = true;
        deleteButtonText.transform.parent.GetComponent<Button>().interactable = true;
        string fullPath = terrainsLoading ? Application.persistentDataPath + "/Terrains/" + saveNames[index] + '.' + TERRAIN_FNAME_EXTENSION  : Application.persistentDataPath + "/Saves/" + saveNames[index] + '.' + SAVE_FNAME_EXTENSION;
        if (File.Exists(fullPath))
        {
            saveDateString.enabled = true;
            saveDateString.text = File.GetCreationTime(fullPath).ToString();
        }
        else
        {
            RefreshSavesList();
        }
    }

    public void SaveLoadButton()
    {
        if (lastSelectedIndex == -1) return;
        if (saveMode)
        {
            submitWindow.SetActive(true);
            submitQuestionText.text = Localization.GetWord(LocalizedWord.Rewrite) + '?';
            submitButtonText.text = Localization.GetWord(LocalizedWord.Yes);
            rejectButtonText.text = Localization.GetWord(LocalizedWord.Cancel);
            deleteSubmit = false;
        }
        else
        {
            if (terrainsLoading)
            {// ЗАГРУЗКА  УРОВНЕЙ  ДЛЯ  РЕДАКТОРА
                string fullPath = Application.persistentDataPath + "/Terrains/" + saveNames[lastSelectedIndex] + '.' + TERRAIN_FNAME_EXTENSION;
                if (ingame)
                {
                    if (GameMaster.realMaster.LoadTerrain(fullPath))  gameObject.SetActive(false);
                }
                else
                {
                    // теоретический сценарий, не должен использоваться
                    if (File.Exists(fullPath))
                    {
                        GameMaster.savename = saveNames[lastSelectedIndex];
                        GameStartSettings gss = new GameStartSettings(false);
                        GameMaster.gss = gss;
                        SceneManager.LoadScene(EDITOR_LEVEL_NUMBER);
                    }
                    else
                    {
                        saveNames[lastSelectedIndex] = "File not exist";
                    }
                }
            }
            else
            {// ЗАГРУЗКА  УРОВНЕЙ  ДЛЯ  ИГРЫ
                string fullPath = Application.persistentDataPath + "/Saves/" + saveNames[lastSelectedIndex] + '.' + SAVE_FNAME_EXTENSION;
                if (ingame)
                {
                    if (GameMaster.realMaster.LoadGame(fullPath))
                    {
                        gameObject.SetActive(false);
                        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GameLoaded));
                    }
                    else
                    {
                        string s = Localization.GetAnnouncementString(GameAnnouncements.LoadingFailed);
                        UIController.current.MakeAnnouncement(s);
                        saveNames[lastSelectedIndex] = s;
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        GameMaster.savename = saveNames[lastSelectedIndex];
                        GameStartSettings gss = new GameStartSettings(false);
                        GameMaster.gss = gss;
                        SceneManager.LoadScene(GAME_LEVEL_NUMBER);
                    }
                    else
                    {
                        saveNames[lastSelectedIndex] = "File not exist";
                    }
                }
            }
        }
    }

    public void SubmitButton() // for save option only
    {
        if (terrainsLoading)
        {// ПЕРЕЗАПИСЬ СОХРАНЕНИЯ ТЕРРЕЙНА
            if (deleteSubmit)
            {
                File.Delete(Application.persistentDataPath + "/Terrains/" + saveNames[lastSelectedIndex] + '.' + TERRAIN_FNAME_EXTENSION );
                Transform t = saveNamesContainer.GetChild(lastSelectedIndex + 1);
                t.GetComponent<Image>().color = Color.white;
                t.GetChild(0).GetComponent<Text>().color = Color.white;
                lastSelectedIndex = -1;
                RefreshSavesList();
            }
            else
            {
                if (lastSelectedIndex != -1)
                {
                   GameMaster.realMaster.SaveTerrain(saveNames[lastSelectedIndex]);
                }
            }
        }
        else
        {   // ПЕРЕЗАПИСЬ СОХРАНЕНИЯ ИГРЫ
            if (deleteSubmit)
            {
                File.Delete(terrainsLoading ? Application.persistentDataPath + "/Terrains/" + saveNames[lastSelectedIndex] + '.' + TERRAIN_FNAME_EXTENSION : Application.persistentDataPath + "/Saves/" + saveNames[lastSelectedIndex] + '.' + SAVE_FNAME_EXTENSION);
                Transform t = saveNamesContainer.GetChild(lastSelectedIndex + 1);
                t.GetComponent<Image>().color = Color.white;
                t.GetChild(0).GetComponent<Text>().color = Color.white;
                lastSelectedIndex = -1;
                RefreshSavesList();
            }
            else
            {
                if (lastSelectedIndex != -1)
                {
                    string s = saveNames[lastSelectedIndex];
                    if (GameMaster.realMaster.SaveGame(s))
                        UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GameSaved));
                    else UIController.current.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.SavingFailed));
                }
            }
        }
       submitWindow.SetActive(false);
    }

    public void DeleteButton()
    {
        if (lastSelectedIndex == -1 | lastSelectedIndex >= saveNames.Length) return;
        string path = terrainsLoading ? Application.persistentDataPath + "/Terrains/" + saveNames[lastSelectedIndex] + '.' + TERRAIN_FNAME_EXTENSION : Application.persistentDataPath + "/Saves/" + saveNames[lastSelectedIndex] + '.' + SAVE_FNAME_EXTENSION;
        if (File.Exists(path))
        {
            deleteSubmit = true;
            submitWindow.SetActive(true);
            submitQuestionText.text = Localization.GetWord(LocalizedWord.Delete) + '?';
            submitButtonText.text = Localization.GetWord(LocalizedWord.Yes);
            rejectButtonText.text = Localization.GetWord(LocalizedWord.Cancel);
        }
        else saveNames[lastSelectedIndex] = "File not exists";
    }

    public void CloseButton()
    {
        gameObject.SetActive(false);
    }

    public void LocalizeButtonTitles()
    {
        deleteButtonText.text = Localization.GetWord(LocalizedWord.Delete);
        inputFieldPanel.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Save);
        inputFieldPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);
    }
}
