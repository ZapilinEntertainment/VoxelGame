using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICrewObserver : MonoBehaviour
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


    public void SetPosition(Rect r, SpriteAlignment alignment)
    {
        var rt = GetComponent<RectTransform>();
        rt.position = r.position;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, r.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, r.height);
        Vector2 correctionVector = Vector2.zero;
        switch (alignment)
        {
            case SpriteAlignment.BottomRight: correctionVector = Vector2.left * rt.rect.width; break;
            case SpriteAlignment.RightCenter: correctionVector = new Vector2(-1f * rt.rect.width, -0.5f * rt.rect.height); break;
            case SpriteAlignment.TopRight: correctionVector = new Vector2(-1f * rt.rect.width, -1f * rt.rect.height);break;
            case SpriteAlignment.Center: correctionVector = new Vector2(-0.5f * rt.rect.width, -0.5f * rt.rect.height);break;
            case SpriteAlignment.TopCenter: correctionVector = new Vector2(-0.5f * rt.rect.width, -1f * rt.rect.height); break;
            case SpriteAlignment.BottomCenter: correctionVector = new Vector2(-0.5f * rt.rect.width, 0f);break;
            case SpriteAlignment.TopLeft:   correctionVector = Vector2.down * rt.rect.height;  break;
            case SpriteAlignment.LeftCenter: correctionVector = Vector2.down * rt.rect.height * 0.5f; break;
        }        
        rt.anchoredPosition += correctionVector;
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
