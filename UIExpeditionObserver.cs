using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIExpeditionObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private RawImage icon, connectionImage, crewPassButtonImage, destinationPointImage;
    [SerializeField] private InputField nameField;
    [SerializeField] private Text statusText, crewInfo, placeInfo, missionInfo, currentStepText;
    [SerializeField] private Image stepProgressBar;
    [SerializeField] private GameObject decisionButtonA, decisionButtonB, decisionButtonC;
#pragma warning restore 0649
    private bool subscribedToUpdate = false;
    private Expedition showingExpedition;

    public void SetPosition(Vector3 pos, SpriteAlignment alignment)
    {
        var rt = GetComponent<RectTransform>();
        Vector3 correctionVector = Vector3.zero;
        switch (alignment)
        {
            case SpriteAlignment.BottomRight: correctionVector = Vector3.left * rt.rect.width; break;
            case SpriteAlignment.RightCenter: correctionVector = new Vector3(-1f * rt.rect.width, -0.5f * rt.rect.height, 0f); break;
            case SpriteAlignment.TopRight: correctionVector = new Vector3(-1f * rt.rect.width, -1f * rt.rect.height, 0f); break;
            case SpriteAlignment.Center: correctionVector = new Vector3(-0.5f * rt.rect.width, -0.5f * rt.rect.height, 0f); break;
            case SpriteAlignment.TopCenter: correctionVector = new Vector3(-0.5f * rt.rect.width, -1f * rt.rect.height, 0f); break;
            case SpriteAlignment.BottomCenter: correctionVector = new Vector3(-0.5f * rt.rect.width, 0f, 0f); break;
            case SpriteAlignment.TopLeft: correctionVector = Vector3.down * rt.rect.height; break;
            case SpriteAlignment.LeftCenter: correctionVector = Vector3.down * rt.rect.height * 0.5f; break;
        }
        rt.position = pos + correctionVector;
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
            icon.texture = showingExpedition.icon;
            nameField.text = showingExpedition.name;

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
            StatusUpdate();
        }
    }

    public void StatusUpdate()
    {
        if (showingExpedition == null) gameObject.SetActive(false);
        else {
            bool connect = showingExpedition.hasConnection;
            connectionImage.uvRect = UIController.GetTextureUV(connect ? Icons.TaskCompleted : Icons.TaskFailed);
            statusText.text = connect ? Localization.GetExpeditionStatus(showingExpedition.stage) : Localization.GetPhrase(LocalizedPhrase.ConnectionLost);
            if (connect)
            {
                //fill stage text & stage bar
                currentStepText.text = Localization.GetWord(LocalizedWord.Step) + ' ' + showingExpedition.currentStep.ToString() + " / " + showingExpedition.mission.stepsCount.ToString();
                stepProgressBar.fillAmount = showingExpedition.progress;

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
