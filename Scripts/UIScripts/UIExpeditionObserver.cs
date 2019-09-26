using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIExpeditionObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private RawImage  connectionImage, crewPassButtonImage, destinationPointImage;
    [SerializeField] private Text statusText, crewInfo, placeInfo, missionInfo, currentStepText, progressText;
    [SerializeField] private Image stepProgressBar;
    [SerializeField] private GameObject recallButton;
    [SerializeField] private Text[] logData;
#pragma warning restore 0649
    private bool subscribedToUpdate = false;
    private byte lastChangesMarkerValue = 0;
    private Expedition showingExpedition;

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
            case SpriteAlignment.TopRight: correctionVector = new Vector2(-1f * rt.rect.width, -1f * rt.rect.height); break;
            case SpriteAlignment.Center: correctionVector = new Vector2(-0.5f * rt.rect.width, -0.5f * rt.rect.height); break;
            case SpriteAlignment.TopCenter: correctionVector = new Vector2(-0.5f * rt.rect.width, -1f * rt.rect.height); break;
            case SpriteAlignment.BottomCenter: correctionVector = new Vector2(-0.5f * rt.rect.width, 0f); break;
            case SpriteAlignment.TopLeft: correctionVector = Vector2.down * rt.rect.height; break;
            case SpriteAlignment.LeftCenter: correctionVector = Vector2.down * rt.rect.height * 0.5f; break;
        }
        rt.anchoredPosition += correctionVector;
    }

    public void Show(Expedition e)
    {
        if (e == null) gameObject.SetActive(false);
        else
        {
            showingExpedition = e;
            RedrawWindow();
        }
    }
    private void RedrawWindow()
    {
        if (showingExpedition == null) gameObject.SetActive(false);
        else
        {
            crewInfo.text = showingExpedition.crew.name;
            showingExpedition.crew.DrawCrewIcon(crewPassButtonImage);

            var d = showingExpedition.destination;
            if (d != null)
            {
                placeInfo.text = Localization.GetMapPointTitle(d.type);
                destinationPointImage.uvRect = GlobalMapUI.GetMarkerRect(d.type);
                if (!destinationPointImage.isActiveAndEnabled)
                {
                    placeInfo.enabled = true;
                    destinationPointImage.transform.parent.gameObject.SetActive(true);
                }
            }
            else
            {
                if (destinationPointImage.isActiveAndEnabled)
                {
                    placeInfo.enabled = false;
                    destinationPointImage.transform.parent.gameObject.SetActive(false);
                }
            }

            bool connect = showingExpedition.hasConnection;
            connectionImage.uvRect = UIController.GetTextureUV(connect ? Icons.TaskCompleted : Icons.TaskFailed);
            statusText.text = connect ? Localization.GetExpeditionStatus(showingExpedition.stage) : Localization.GetPhrase(LocalizedPhrase.ConnectionLost);
            if (connect & (showingExpedition.stage == Expedition.ExpeditionStage.OnMission | showingExpedition.stage == Expedition.ExpeditionStage.LeavingMission))
            {
                //fill stage text & stage bar
                currentStepText.text = Localization.GetWord(LocalizedWord.Step) + ' ' + showingExpedition.currentStep.ToString() + " / " + showingExpedition.mission.stepsCount.ToString();
                float f = showingExpedition.progress / Expedition.ONE_STEP_WORKFLOW;
                stepProgressBar.fillAmount = f;
                progressText.text = ((int)(f * 100)).ToString() + '%';

                if (!currentStepText.enabled)
                {
                    currentStepText.enabled = true;
                    stepProgressBar.transform.parent.parent.gameObject.SetActive(true);
                }
            }
            else
            {
                currentStepText.enabled = false;
                stepProgressBar.transform.parent.parent.gameObject.SetActive(false);
            }
            if (showingExpedition.stage == Expedition.ExpeditionStage.OnMission | showingExpedition.stage == Expedition.ExpeditionStage.WayIn)
            {
                if (!recallButton.activeSelf) recallButton.SetActive(true);
            }
            else
            {
                if (recallButton.activeSelf) recallButton.SetActive(false);
            }

            lastChangesMarkerValue = showingExpedition.changesMarkerValue;
        }
    }

    public void StatusUpdate()
    {
        if (showingExpedition == null) gameObject.SetActive(false);
        else {
            if (lastChangesMarkerValue != showingExpedition.changesMarkerValue)
            {              
                if (showingExpedition.hasConnection && (showingExpedition.stage == Expedition.ExpeditionStage.OnMission | showingExpedition.stage == Expedition.ExpeditionStage.LeavingMission))
                {
                    //fill stage text & stage bar
                    currentStepText.text = Localization.GetWord(LocalizedWord.Step) + ' ' + showingExpedition.currentStep.ToString() + " / " + showingExpedition.mission.stepsCount.ToString();
                    float f = showingExpedition.progress / Expedition.ONE_STEP_WORKFLOW;
                    stepProgressBar.fillAmount = f;
                    progressText.text = ((int)(f * 100)).ToString() + '%';
                }
                lastChangesMarkerValue = showingExpedition.changesMarkerValue;
            }
        }
    }

    public void CrewButton()
    {
        if (showingExpedition == null || showingExpedition.stage == Expedition.ExpeditionStage.Dismissed) gameObject.SetActive(false);
        else
        {
            if (ExplorationPanelUI.current.isActiveAndEnabled)
            {
                ExplorationPanelUI.current.Show(showingExpedition.crew);
            }
            else
            {
                showingExpedition.crew.ShowOnGUI(new Rect(transform.position, GetComponent<RectTransform>().rect.size), SpriteAlignment.BottomLeft, true);
                gameObject.SetActive(false);
            }
        }
    }
    public void DestinationButton()
    {
        if (showingExpedition == null || showingExpedition.stage == Expedition.ExpeditionStage.Dismissed) gameObject.SetActive(false);
        else
        {
            var g = GameMaster.realMaster.globalMap;
            g.ShowOnGUI();
            g.observer.GetComponent<GlobalMapUI>().SelectPoint(showingExpedition.destination);
            gameObject.SetActive(false);
        }
    }
    public void RecallButton()
    {
        if (showingExpedition == null || showingExpedition.stage == Expedition.ExpeditionStage.Dismissed) gameObject.SetActive(false);
        else
        {
            showingExpedition.EndMission();
            StatusUpdate();
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
