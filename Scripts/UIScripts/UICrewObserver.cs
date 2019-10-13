using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UICrewObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown shuttlesDropdown, artifactsDropdown;
    [SerializeField] private InputField nameField;
    [SerializeField] private Text levelText, membersButtonText, experienceText, statsText, statusText;
    [SerializeField] private Image experienceBar, staminaBar;
    [SerializeField] private Button membersButton, shuttleButton, artifactButton;
    [SerializeField] private GameObject dismissButton, travelButton, closeButton;
    [SerializeField] private RawImage icon;
#pragma warning restore 0649

    private bool subscribedToUpdate = false;
    private int lastDrawState = 0, lastShuttlesState = 0, lastArtifactsState = 0;
    private List<int> shuttlesListIDs, artifactsIDs;
    private Crew showingCrew;


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
            showingCrew = c;
            PrepareShuttlesDropdown();
            PrepareArtifactsDropdown();
            RedrawWindow();
            closeButton.SetActive(useCloseButton);
        }
    }
    public void RedrawWindow()
    {
        nameField.text = showingCrew.name;
        statsText.text = Localization.GetCrewInfo(showingCrew);

        levelText.text = showingCrew.level.ToString();
        levelText.color = Color.Lerp(Color.white, Color.cyan, (float)showingCrew.level / 255f);
        int e = showingCrew.experience, ne = showingCrew.GetExperienceCap();
        experienceText.text = e.ToString() + " / " + ne.ToString();
        experienceBar.fillAmount = e / (float)ne;


        int m_count = showingCrew.membersCount;
        membersButtonText.text = m_count.ToString() + '/' + Crew.MAX_MEMBER_COUNT.ToString();
        membersButton.enabled = (m_count != Crew.MAX_MEMBER_COUNT) & showingCrew.status == Crew.CrewStatus.AtHome;

        shuttleButton.enabled = Shuttle.shuttlesList.Count > 0;
        var ri = shuttleButton.transform.GetChild(0).GetComponent<RawImage>();
        if (showingCrew.shuttle != null)
        {
            showingCrew.shuttle.DrawShuttleIcon(ri);
        }
        else
        {
            ri.texture = UIController.current.iconsTexture;
            ri.uvRect = UIController.GetIconUVRect(Icons.TaskFrame);
        }

        artifactButton.enabled = Artifact.artifactsList.Count > 0;
        ri = artifactButton.transform.GetChild(0).GetComponent<RawImage>();
        if (showingCrew.artifact != null)
        {
            ri.texture = showingCrew.artifact.GetTexture();
            ri.uvRect = new Rect(0f, 0f, 1f, 1f);
        }
        else
        {
            ri.texture = UIController.current.iconsTexture;
            ri.uvRect = UIController.GetIconUVRect(Icons.TaskFrame);
        }

        statusText.text = Localization.GetCrewStatus(showingCrew.status);
        dismissButton.SetActive(showingCrew.status == Crew.CrewStatus.AtHome);

        travelButton.SetActive(showingCrew.shuttle != null & showingCrew.status == Crew.CrewStatus.AtHome);
        staminaBar.fillAmount = showingCrew.stamina;

        lastDrawState = showingCrew.changesMarkerValue ;
    }

    public void StatusUpdate()
    {

        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            if (lastDrawState != showingCrew.changesMarkerValue)
            {
                RedrawWindow();
            }
            staminaBar.fillAmount = showingCrew.stamina;

            if (lastShuttlesState != Shuttle.listChangesMarkerValue) PrepareShuttlesDropdown();
            if (lastArtifactsState != Artifact.listChangesMarkerValue) PrepareArtifactsDropdown();
        }
    }

    //buttons
    public void NameChanged()
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            showingCrew.Rename(nameField.text);
            if (RecruitingCenter.rcenterObserver != null && RecruitingCenter.rcenterObserver.isActiveAndEnabled)
                RecruitingCenter.rcenterObserver.PrepareWindow();
        }
    }
    public void MembersButton()
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            if (RecruitingCenter.SelectAny()) gameObject.SetActive(false);
        }
    }
    public void ShuttleButton()
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            if (showingCrew.shuttle == null) RedrawWindow();
            else
            {
                if (UIController.current.currentActiveWindowMode == ActiveWindowMode.ExpeditionPanel)
                {
                    ExplorationPanelUI.current.Show(showingCrew.shuttle);
                }
                else
                {
                    var rt = statsText.GetComponent<RectTransform>();
                    showingCrew.shuttle.ShowOnGUI(true, new Rect(rt.position, rt.rect.size), SpriteAlignment.Center, true);
                }
            }
        }
    }
    public void ArtifactButton()
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            if (showingCrew.artifact != null)
            {
                if (UIController.current.currentActiveWindowMode == ActiveWindowMode.ExpeditionPanel)
                {
                    ExplorationPanelUI.current.Show(showingCrew.artifact);
                }
                else
                {
                    var rt = statsText.GetComponent<RectTransform>();
                    showingCrew.artifact.ShowOnGUI(new Rect(rt.position, rt.rect.size), SpriteAlignment.Center, true);
                }
            }
            else RedrawWindow();
        }
    }
    public void TravelButton() //  NOT COMPLETED
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            if (showingCrew.status == Crew.CrewStatus.AtHome)
            {
                //
            }
            else
            {
                RedrawWindow();
            }
        }
    }
    public void DismissButton() // сделать подтверждение
    {
        if (showingCrew != null)
        {
            showingCrew.Dismiss();
            showingCrew = null;
            gameObject.SetActive(false);
        }
    }

    public void SelectShuttle(int i)
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else {
            if (shuttlesListIDs[i] == -1)
            {
                showingCrew.SetShuttle(null);
                PrepareShuttlesDropdown();
            }
            else
            {
                var s = Shuttle.GetShuttle(shuttlesListIDs[i]);
                if (s != null)
                {
                    if (showingCrew.shuttle == null || showingCrew.shuttle != s)
                    {
                        showingCrew.SetShuttle(s);
                        PrepareShuttlesDropdown();
                    }
                }
                else PrepareShuttlesDropdown();
            }
        }
    }
    public void SelectArtifact(int i)
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            if (artifactsIDs[i] == -1)
            {
                showingCrew.DropArtifact();
                PrepareArtifactsDropdown();
            }
            else
            {
                var s = Artifact.GetArtifactByID(artifactsIDs[i]);
                if (showingCrew.artifact == null || showingCrew.artifact != s)
                {
                    showingCrew.SetArtifact(s);
                    PrepareArtifactsDropdown();
                }
            }
        }
    }
    //

    public void LocalizeTitles()
    {
        dismissButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Dismiss);
        travelButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.GoOnATrip);
    }

    private void PrepareShuttlesDropdown()
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {            
            var opts = new List<Dropdown.OptionData>();
            var shuttles = Shuttle.shuttlesList;
            char c = '"';
            shuttlesListIDs = new List<int>();
            opts.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoShuttle)));
            shuttlesListIDs.Add(-1);
            if (showingCrew.shuttle == null)
            { // без проверок на собственный шаттл                
                if (shuttles.Count > 0 )
                {
                    foreach (var s in shuttles)
                    {
                        if (s.crew == null)
                        {
                            opts.Add(new Dropdown.OptionData(c + s.name + c));
                            shuttlesListIDs.Add(s.ID);
                        }
                    }
                }
            }
            else
            {
                opts.Insert(0, new Dropdown.OptionData(c + showingCrew.shuttle.name + c));
                shuttlesListIDs.Insert(0, showingCrew.shuttle.ID);
                if (shuttles.Count > 0)
                {
                    foreach (var s in shuttles)
                    {
                        if (s != showingCrew.shuttle & s.crew == null)
                        {
                            opts.Add(new Dropdown.OptionData(c + s.name + c));
                            shuttlesListIDs.Add(s.ID);
                        }
                    }
                }
            }
            shuttlesDropdown.value = 0;
            shuttlesDropdown.options = opts;
            lastShuttlesState = Shuttle.listChangesMarkerValue;
            shuttlesDropdown.interactable = (showingCrew.status == Crew.CrewStatus.AtHome);
        }
    }
    private void PrepareArtifactsDropdown()
    {
        if (showingCrew == null) gameObject.SetActive(false);
        else
        {
            var opts = new List<Dropdown.OptionData>();
            var artifacts = Artifact.artifactsList;
            artifactsIDs = new List<int>();
            opts.Add(new Dropdown.OptionData(Localization.GetPhrase(LocalizedPhrase.NoArtifact)));
            artifactsIDs.Add(-1);
            if (showingCrew.artifact == null)
            { // без проверок на собственный артефакт             
                if (artifacts.Count > 0)
                {
                    foreach (var s in artifacts)
                    {
                        if (s.status == Artifact.ArtifactStatus.OnConservation)
                        {
                            opts.Add(new Dropdown.OptionData(s.name));
                            artifactsIDs.Add(s.ID);
                        }
                    }
                }
            }
            else
            {
                opts.Insert(0, new Dropdown.OptionData(showingCrew.artifact.name));
                shuttlesListIDs.Insert(0, showingCrew.artifact.ID);
                if (artifactsIDs.Count > 0)
                {
                    foreach (var s in artifacts)
                    {
                        if (s.status == Artifact.ArtifactStatus.OnConservation)
                        {
                            opts.Add(new Dropdown.OptionData(s.name));
                            artifactsIDs.Add(s.ID);
                        }
                    }
                }
            }
            artifactsDropdown.value = 0;
            artifactsDropdown.options = opts;
            lastArtifactsState = Artifact.listChangesMarkerValue;
            artifactsDropdown.interactable = (showingCrew.status == Crew.CrewStatus.AtHome);
        }
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
