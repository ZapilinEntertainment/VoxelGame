using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICrewObserver : MonoBehaviour, IObserverController<Crew>
{
#pragma warning disable 0649
    [SerializeField] private InputField nameField;
    [SerializeField] private Text levelText, membersButtonText, experienceText;
    [SerializeField] private Transform statsPanel;
    [SerializeField] private Image experienceBar, staminaBar;
    [SerializeField] private Button membersButton;
    [SerializeField] private GameObject dismissButton, closeButton;
    [SerializeField] private RawImage icon;
#pragma warning restore 0649

    private bool subscribedToUpdate = false;
    private int lastDrawState = 0;
    private Crew observingCrew;
    private static UICrewObserver _currentObserver;

    public static UICrewObserver GetCrewObserver()
    {
        if (_currentObserver == null)
        {
            _currentObserver = Instantiate(Resources.Load<GameObject>("UIPrefs/crewPanel"), UIController.current.mainCanvas).GetComponent<UICrewObserver>();
            _currentObserver.LocalizeTitles();
        }
        return _currentObserver;
    }
    public static void Show(RectTransform window, SpriteAlignment alignment, Crew c, bool useCloseButton)
    {
        Show(window, window.rect, alignment, c, useCloseButton);
    }
    public static void Show(RectTransform window, Rect r, SpriteAlignment alignment, Crew c, bool useCloseButton)
    {
        var co = GetCrewObserver();
        if (!co.gameObject.activeSelf) co.gameObject.SetActive(true);
        co.SetPosition(window, r, alignment);
        co.ShowCrew(c, useCloseButton);
    }

    private void SetPosition(RectTransform parent, Rect r, SpriteAlignment alignment)
    {
        var rt = GetCrewObserver().GetComponent<RectTransform>();
        UIController.PositionElement(rt, parent, alignment, r);
    }
    public void ShowCrew(Crew c, bool useCloseButton)
    {
        if (c == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            observingCrew = c;
            RedrawWindow();
            closeButton.SetActive(useCloseButton);
        }
    }

    public static void DisableObserver()
    {
        if (_currentObserver != null) _currentObserver.gameObject.SetActive(false);
    }
    public void ClearInfo(Crew c)
    {
        if (_currentObserver != null && _currentObserver.observingCrew == c)
        {
            _currentObserver.gameObject.SetActive(false);
        }
    }
    public static void DestroyObserver()
    {
        if (_currentObserver != null) Destroy(_currentObserver.gameObject);
    }

    public static void Refresh()
    {
        if (_currentObserver != null) _currentObserver.RedrawWindow();
    }
    public void RedrawWindow()
    {
        nameField.text = observingCrew.name;

        levelText.text = observingCrew.level.ToString();
        levelText.color = Color.Lerp(Color.white, Color.cyan, (float)observingCrew.level / 255f);
        int e = observingCrew.experience, ne = observingCrew.GetExperienceCap();
        experienceText.text = e.ToString() + " / " + ne.ToString();
        experienceBar.fillAmount = e / (float)ne;

        int m_count = observingCrew.membersCount;
        membersButtonText.text = m_count.ToString() + '/' + Crew.MAX_MEMBER_COUNT.ToString();

        staminaBar.fillAmount = observingCrew.stamina;
        //stats
        var t = statsPanel.GetChild(0);
        bool hasFreePoints = observingCrew.freePoints > 0;
        var b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints;
        b.GetComponent<Image>().enabled = hasFreePoints;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.persistence.ToString();

        t = statsPanel.GetChild(1);
        b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints;
        b.GetComponent<Image>().enabled = hasFreePoints;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.survivalSkills.ToString();

        t = statsPanel.GetChild(2);
        b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints;
        b.GetComponent<Image>().enabled = hasFreePoints;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.perception.ToString();

        t = statsPanel.GetChild(3);
        b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints;
        b.GetComponent<Image>().enabled = hasFreePoints;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.secretKnowledge.ToString();

        t = statsPanel.GetChild(4);
        b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints;
        b.GetComponent<Image>().enabled = hasFreePoints;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.intelligence.ToString();

        t = statsPanel.GetChild(5);
        b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints;
        b.GetComponent<Image>().enabled = hasFreePoints;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.techSkills.ToString();

        var fpp = statsPanel.GetChild(6);
        if (hasFreePoints)
        {
            fpp.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.FreeAttributePoints) + observingCrew.freePoints.ToString();
            fpp.gameObject.SetActive(true);
        }
        else
        {
            fpp.gameObject.SetActive(false);
        }
        t = statsPanel.GetChild(7);
        t.GetComponent<Text>().text = Localization.GetCrewInfo(observingCrew);
        //
        if (observingCrew.atHome)
        {
            if (!dismissButton.activeSelf) dismissButton.SetActive(true);
        }
        else
        {
            if (dismissButton.activeSelf) dismissButton.SetActive(false);
        }
        lastDrawState = observingCrew.changesMarkerValue ;
    }
    public void StatusUpdate()
    {

        if (observingCrew == null) gameObject.SetActive(false);
        else
        {
            if (lastDrawState != observingCrew.changesMarkerValue)
            {
                RedrawWindow();
            }
            staminaBar.fillAmount = observingCrew.stamina;
        }
    }

    //buttons
    public void NameChanged()
    {
        if (observingCrew == null) gameObject.SetActive(false);
        else
        {
            observingCrew.Rename(nameField.text);
            if (RecruitingCenter.rcenterObserver != null && RecruitingCenter.rcenterObserver.isActiveAndEnabled)
                RecruitingCenter.rcenterObserver.PrepareWindow();
        }
    }
    public void MembersButton()
    {
        if (observingCrew == null) gameObject.SetActive(false);
        else
        {
            if (RecruitingCenter.SelectAny()) gameObject.SetActive(false);
        }
    }
    public void DismissButton() // сделать подтверждение
    {
        if (observingCrew != null)
        {
            observingCrew.Dismiss();
            observingCrew = null;
            gameObject.SetActive(false);
        }
    }

    //

    public void LocalizeTitles()
    {
        statsPanel.GetChild(0).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Persistence);
        statsPanel.GetChild(1).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.SurvivalSkills);
        statsPanel.GetChild(2).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Perception);
        statsPanel.GetChild(3).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.SecretKnowledge);
        statsPanel.GetChild(4).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Intelligence);
        statsPanel.GetChild(5).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.TechSkills);
        dismissButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);        
    }

    private void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
    }
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            if (UIController.current != null)
            {
                UIController.current.statusUpdateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
        ExploringMinigameUI.ActivateIfEnabled();
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            if (UIController.current != null)
            {
                UIController.current.statusUpdateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
    }
}
