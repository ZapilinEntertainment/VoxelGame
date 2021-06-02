using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public sealed class SaveSystemUI : MonoBehaviour, ILocalizable, IListable
{
    public bool saveMode = false;
    private bool deleteSubmit = false, workWithTerrains, linkedToListController = false;
    private string[] savenames;
    private int[] IDs;
    private int selectedIndex = -1;
#pragma warning disable 0649
    [SerializeField] private Text saveLoadButtonText, deleteButtonText, submitButtonText, rejectButtonText, submitQuestionText, saveDateString; // fiti
    [SerializeField] private GameObject submitWindow, inputFieldBasis;
    [SerializeField] private InputField savenameField;
#pragma warning restore 0649
    [SerializeField] private ListController listController;

    public const string SAVE_FNAME_EXTENSION = "sav", TERRAIN_FNAME_EXTENSION = "itd"; // island terrain data    
    private const int NO_FILE_ID = -1, NEW_SAVE_ID = -2;

    public static SaveSystemUI Initialize(Transform basis)
    {
        GameObject g = Instantiate(Resources.Load<GameObject>("UIPrefs/SaveSystemModule"), basis);
        return g.GetComponent<SaveSystemUI>();
    }
    /// <summary>
    /// ATTENTION : returns without last slash : "Path/Folder" 
    /// </summary>
    public static string GetSavesPath() { return Application.persistentDataPath + "/Saves"; }
    /// <summary>
    /// ATTENTION : returns without last slash : "Path/Folder" 
    /// </summary>
    public static string GetTerrainsPath() { return Application.persistentDataPath + "/Terrains"; }

    public static string GetGameSaveFullpath(string savename)
    {
        return GetSavesPath() + '/' + savename + '.' + SAVE_FNAME_EXTENSION;
    }
    public static string GetTerrainSaveFullpath(string savename)
    {
        return GetTerrainsPath() + '/' + savename + '.' + TERRAIN_FNAME_EXTENSION;
    }

    private void Awake()
    {
        LocalizeTitles();
        gameObject.SetActive(false);
    }


    public void Activate(bool openSaveMode, bool i_terrainsLoading)
    {
        if (!linkedToListController)
        {
            listController.AssignSelectItemAction(this.SelectSave);
            linkedToListController = true;
        }
        gameObject.SetActive(true);
        if (inputFieldBasis.activeSelf) inputFieldBasis.SetActive(false);
        if (submitWindow.activeSelf) submitWindow.SetActive(false);        
        saveLoadButtonText.transform.parent.GetComponent<Button>().interactable = false;
        deleteButtonText.transform.parent.GetComponent<Button>().interactable = false;
        deleteSubmit = false;
        saveDateString.enabled = false;

        saveMode = openSaveMode;
        workWithTerrains = i_terrainsLoading;
        PrepareSavesList();
    }

    private void PrepareSavesList()
    {
        selectedIndex = -1;
        string directoryPath = workWithTerrains ? GetTerrainsPath() : GetSavesPath();

        string CutName(string s)
        {
            int lastSlashPos = s.LastIndexOf('\\'); // в редакторе так
            if (lastSlashPos == -1) lastSlashPos = s.LastIndexOf('/');
            return s.Substring(lastSlashPos + 1, s.Length - lastSlashPos - 5); //  от последнего слеша до ".sav"
        }

        if (saveMode)
        { //SAVING
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                savenames = new string[1] ;
                IDs = new int[1] ;               
            }
            else
            {
                var sn = Directory.GetFiles(directoryPath, "*." + (workWithTerrains ? TERRAIN_FNAME_EXTENSION : SAVE_FNAME_EXTENSION));
                int len = sn.Length;
                savenames = new string[len + 1];
                IDs = new int[len + 1];
                int j;
                for (int i = 0; i < sn.Length; i++)
                {
                    j = i + 1;
                    savenames[j] = CutName(sn[i]);
                    IDs[j] = j;
                }
                sn = null;
            }

            savenames[0] = Localization.GetPhrase(LocalizedPhrase.CreateNewSave);
            IDs[0] = NEW_SAVE_ID;            
            saveLoadButtonText.text = Localization.GetWord(LocalizedWord.Save);
        }
        else
        { // LOADING
            if (!Directory.Exists(directoryPath))
            {
                savenames = null;
                IDs = null;
            }
            else
            {
                savenames = Directory.GetFiles(directoryPath, "*." + (workWithTerrains ? TERRAIN_FNAME_EXTENSION : SAVE_FNAME_EXTENSION));
                int len = savenames.Length;
                if (len > 0)
                {
                    IDs = new int[len];
                    for (int i = 0; i < len; i++)
                    {
                        IDs[i] = i + 1;
                        savenames[i] = CutName(savenames[i]);
                    }
                }
            }
            if (savenames.Length == 0)
            {
                listController.ChangeEmptyLabelText(Localization.GetPhrase(LocalizedPhrase.NoSavesFound));
                savenames = null;
                IDs = null;
            }
            saveLoadButtonText.text = Localization.GetWord(LocalizedWord.Load);
        }

        listController.PrepareList(this);
   }
    public void SelectSave(int index) 
    {
        if (index < 0 | IDs.Length <= index) return;
        if (IDs[index] == NEW_SAVE_ID)
        {
            CreateNewSave();
            return;
        }
        else
        {
            saveLoadButtonText.transform.parent.GetComponent<Button>().interactable = true;
            deleteButtonText.transform.parent.GetComponent<Button>().interactable = true;
            var name = savenames[index];
            string fullPath = workWithTerrains ?
            GetTerrainSaveFullpath(name)
            :
            GetGameSaveFullpath(name);
            if (File.Exists(fullPath))
            {
                saveDateString.enabled = true;
                saveDateString.text = File.GetLastWriteTime(fullPath).ToString();
            }
            else PrepareSavesList();
        }
    }

    public void CreateNewSave()
    {
        listController.selectedButtonIndex = -1;
        inputFieldBasis.SetActive(true);
        savenameField.text = workWithTerrains ? "new terrain" : GameMaster.realMaster.colonyController.cityName;
    }
    public void InputField_SaveGame()
    {
        if (workWithTerrains) GameMaster.realMaster.SaveTerrain(savenameField.text);
        else GameMaster.realMaster.SaveGame(savenameField.text);
        inputFieldBasis.SetActive(false);
        gameObject.SetActive(false);
        PrepareSavesList();
    }   

    public void SaveLoadButton()
    {
        int sid = listController.selectedButtonIndex;
        if (sid == -1) return;
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
            bool ingame = GameMaster.realMaster != null;
            string name = savenames[sid];
            if (workWithTerrains)
            {// ЗАГРУЗКА  УРОВНЕЙ  ДЛЯ  РЕДАКТОРА
                string fullPath = GetTerrainSaveFullpath(name);
                if (ingame)
                {
                    if (GameMaster.realMaster.LoadTerrain(fullPath)) gameObject.SetActive(false);
                }
                else
                {
                    // теоретический сценарий, не должен использоваться
                    if (File.Exists(fullPath))
                    {
                        GameMaster.StartNewGame(
                            GameStartSettings.GetEditorStartSettings(name)
                            );
                    }
                    else
                    {
                        PrepareSavesList();
                    }
                }
            }
            else
            {// ЗАГРУЗКА  УРОВНЕЙ  ДЛЯ  ИГРЫ
                string fullPath = GetGameSaveFullpath(name);
                if (ingame)
                {
                    if (GameMaster.realMaster.LoadGame(fullPath))
                    {
                        gameObject.SetActive(false);
                        UIController.GetCurrent().GetMainCanvasController().ChangeActiveWindow(ActiveWindowMode.NoWindow);
                        AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GameLoaded));
                    }
                }
                else
                {
                    if (File.Exists(fullPath))
                    {
                        GameMaster.StartNewGame(
                            GameStartSettings.GetLoadingSettings(GameMode.Survival, name)
                            );
                    }
                    else
                    {
                        PrepareSavesList();
                    }
                }
            }
        }
    }

    public void SubmitButton() // for save option only
    {
        int si = listController.selectedButtonIndex;
        if (si < 0 || savenames == null || si >= savenames.Length) return;
        string name = savenames[listController.selectedButtonIndex];
        bool redrawList = false;
        if (workWithTerrains)
        {// ПЕРЕЗАПИСЬ СОХРАНЕНИЯ ТЕРРЕЙНА
            if (deleteSubmit)
            {
                File.Delete(GetTerrainSaveFullpath(name));
                redrawList = true;
            }
            else
            {
                GameMaster.realMaster.SaveTerrain(name);
                redrawList = true;
            }
        }
        else
        {   // ПЕРЕЗАПИСЬ СОХРАНЕНИЯ ИГРЫ
            if (deleteSubmit)
            {
                File.Delete( GetGameSaveFullpath(name));
                redrawList = true;
            }
            else
            {
                if (GameMaster.realMaster.SaveGame(name))
                {
                    AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.GameSaved));
                    redrawList = true;
                }
                else
                {
                    AnnouncementCanvasController.MakeAnnouncement(Localization.GetAnnouncementString(GameAnnouncements.SavingFailed));
                    if (GameMaster.soundEnabled) GameMaster.audiomaster.Notify(NotificationSound.SystemError);
                }
            }
        }
        submitWindow.SetActive(false);
        if (redrawList) PrepareSavesList();
    }

    public void DeleteButton()
    {
        if (listController.selectedButtonIndex < 0) return;
        string name = savenames[listController.selectedButtonIndex];
        string path = workWithTerrains ?
            GetTerrainSaveFullpath(name)
            :
            GetGameSaveFullpath(name);
        if (File.Exists(path))
        {
            deleteSubmit = true;
            submitWindow.SetActive(true);
            submitQuestionText.text = Localization.GetWord(LocalizedWord.Delete) + '?';
            submitButtonText.text = Localization.GetWord(LocalizedWord.Yes);
            rejectButtonText.text = Localization.GetWord(LocalizedWord.Cancel);
        }
        else PrepareSavesList();
    }

    public void CloseButton()
    {
        listController.selectedButtonIndex = -1;
        gameObject.SetActive(false);
    }

    public void LocalizeTitles()
    {
        deleteButtonText.text = Localization.GetWord(LocalizedWord.Delete);
        inputFieldBasis.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Save);
        inputFieldBasis.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Cancel);
        Localization.AddToLocalizeList(this);
    }
    private void OnDestroy()
    {
        Localization.RemoveFromLocalizeList(this);
    }

    #region IListable
    public string GetName(int index)
    {
        if (savenames != null && savenames.Length > index)
        {
            return savenames[index];
        }
        else return "no save";
    }
    public int GetItemsCount() { return savenames?.Length ?? 0; }
    public bool HaveSelectedObject() { return false; }
    public int GetID(int index) { if (IDs != null && IDs.Length > index) return IDs[index]; else return NO_FILE_ID; }
    #endregion
}
