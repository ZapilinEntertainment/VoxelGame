using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICrewObserver : UIObserver
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

    private int lastDrawState = 0;
    private Crew observingCrew;
    private event System.Action closingEvent;
    private static UICrewObserver _currentObserver;

    #region observer standart functions
    public static UICrewObserver GetObserver()
    {
        if (_currentObserver == null)
        {
            _currentObserver = Instantiate(Resources.Load<GameObject>("UIPrefs/crewPanel"), UIController.GetCurrent().GetCurrentCanvasTransform()).GetComponent<UICrewObserver>();
            _currentObserver.LocalizeTitles();
        }
        return _currentObserver;
    }
    public static void Show(RectTransform parent, SpriteAlignment alignment, Crew c, bool useCloseButton)
    {
        Show(parent, new Rect(Vector2.zero, parent.rect.size), alignment, c, useCloseButton);
    }
    public static void Show(RectTransform parent, Rect r, SpriteAlignment alignment, Crew c, bool useCloseButton)
    {
        var co = GetObserver();
        if (!co.gameObject.activeSelf) co.gameObject.SetActive(true);
        co.SetPosition(parent, r, alignment);
        co.ShowCrew(c, useCloseButton);
    }
    private void SetPosition(RectTransform parent, Rect r, SpriteAlignment alignment)
    {
        var rt = GetObserver().GetComponent<RectTransform>();
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
    #endregion
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
        bool hasFreePoints = observingCrew.freePoints > 0, notMaximum = observingCrew.persistence < Crew.MAX_ATTRIBUTE_VALUE;
        var b = t.GetChild(2);
        b.GetComponent<Button>().enabled = hasFreePoints & notMaximum;
        b.GetComponent<Image>().enabled = hasFreePoints & notMaximum;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.persistence.ToString();

        t = statsPanel.GetChild(1);
        b = t.GetChild(2);
        notMaximum = observingCrew.survivalSkills < Crew.MAX_ATTRIBUTE_VALUE;
        b.GetComponent<Button>().enabled = hasFreePoints & notMaximum;
        b.GetComponent<Image>().enabled = hasFreePoints & notMaximum;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.survivalSkills.ToString();

        t = statsPanel.GetChild(2);
        b = t.GetChild(2);
        notMaximum = observingCrew.perception < Crew.MAX_ATTRIBUTE_VALUE;
        b.GetComponent<Button>().enabled = hasFreePoints & notMaximum;
        b.GetComponent<Image>().enabled = hasFreePoints & notMaximum;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.perception.ToString();

        t = statsPanel.GetChild(3);
        b = t.GetChild(2);
        notMaximum = observingCrew.secretKnowledge < Crew.MAX_ATTRIBUTE_VALUE;
        b.GetComponent<Button>().enabled = hasFreePoints & notMaximum;
        b.GetComponent<Image>().enabled = hasFreePoints & notMaximum;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.secretKnowledge.ToString();

        t = statsPanel.GetChild(4);
        b = t.GetChild(2);
        notMaximum = observingCrew.intelligence < Crew.MAX_ATTRIBUTE_VALUE;
        b.GetComponent<Button>().enabled = hasFreePoints & notMaximum;
        b.GetComponent<Image>().enabled = hasFreePoints & notMaximum;
        b.GetChild(0).GetComponent<Text>().text = observingCrew.intelligence.ToString();

        t = statsPanel.GetChild(5);
        b = t.GetChild(2);
        notMaximum = observingCrew.techSkills < Crew.MAX_ATTRIBUTE_VALUE;
        b.GetComponent<Button>().enabled = hasFreePoints & notMaximum;
        b.GetComponent<Image>().enabled = hasFreePoints & notMaximum;
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
    override public void StatusUpdate()
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
            var exc = ExplorationPanelUI.current;
            if (exc != null && exc.isActiveAndEnabled) exc.ShowInList(observingCrew);
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
    public void AddPoint(int x)
    {
        switch (x)
        {
            case 0: observingCrew.ImprovePersistence(); break;
            case 1:  observingCrew.ImproveSurvivalSkill();break;
            case 2: observingCrew.ImprovePerception(); break;
            case 3: observingCrew.ImproveSecretKnowledge(); break;
            case 4: observingCrew.ImproveIntelligence();break;
            case 5: observingCrew.ImproveTechSkill(); break;
        }
        RedrawWindow();
    }
    //
    public void AddToClosingEvent(System.Action a)
    {
        closingEvent += a;
    }

    override public void LocalizeTitles()
    {
        statsPanel.GetChild(0).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Persistence);
        statsPanel.GetChild(1).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.SurvivalSkills);
        statsPanel.GetChild(2).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Perception);
        statsPanel.GetChild(3).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.SecretKnowledge);
        statsPanel.GetChild(4).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Intelligence);
        statsPanel.GetChild(5).GetChild(1).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.TechSkills);
        dismissButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);        
    }

    override protected void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            mycanvas.uicontroller.updateEvent += StatusUpdate;
            subscribedToUpdate = true;
        }
    }
    override  protected void OnDisable()
    {
        if (subscribedToUpdate)
        {
            var mc = mycanvas.uicontroller;
            if (mc != null )
            {
                mc.updateEvent -= StatusUpdate;
            }
            subscribedToUpdate = false;
        }
        if (closingEvent != null)
        {
            closingEvent.Invoke();
            closingEvent = null;
        }
    }
    override protected void OnDestroy()
    {
        if (!GameMaster.sceneClearing )
        {
            if (subscribedToUpdate)
            {
                var mc = mycanvas.uicontroller;
                if (mc != null)
                {
                    mc.updateEvent -= StatusUpdate;
                }
                subscribedToUpdate = false;
            }
            if (closingEvent != null)
            {
                closingEvent.Invoke();
                closingEvent = null;
            }
        }
    }
}
