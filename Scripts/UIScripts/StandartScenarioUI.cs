using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class StandartScenarioUI : MonoBehaviour, ILocalizable
{
    [SerializeField] private GameObject announcePanel, conditionPanel, blockmask, conditionLine1, conditionLine2, specialButton;
    [SerializeField] private Text announceText, condition0, condition1, condition2, infoString0;
    [SerializeField] private RawImage icon,conditionIcon, conditionCompleteMark0, conditionCompleteMark1, conditionCompleteMark2;
    [SerializeField] private Button conditionButton;
    private bool conditionWindowEnabled = false, canvasEnabled = true, infoString0_enabled = false;
    private GameObject maincanvas_rightpanel;
    private Scenario workingScenario;
    private Quest conditionQuest;
    private Canvas myCanvas;
    private MainCanvasController mcc;
    private Action specialButtonFunction;
    private static StandartScenarioUI current;

    public static StandartScenarioUI GetCurrent(Scenario requester)
    {
        if (current == null) current = Instantiate(Resources.Load<GameObject>("UIPrefs/scenarioCanvas")).GetComponent<StandartScenarioUI>();
        current.SetScenario(requester);
        return current;
    }

    private void Awake()
    {
        myCanvas = GetComponent<Canvas>(); canvasEnabled = myCanvas.enabled;
        announcePanel.SetActive(false);
        blockmask.SetActive(false);
        conditionPanel.SetActive(false); conditionWindowEnabled = false;
        specialButton.SetActive(false);
        infoString0.transform.parent.gameObject.SetActive(false); infoString0_enabled = false;
        
        LocalizeTitles();
        var uic = UIController.GetCurrent();
        uic.AddSpecialCanvasToHolder(transform);
        mcc = uic.GetMainCanvasController();
        maincanvas_rightpanel = mcc.rightPanel;
    }

    private void Update()
    {
        if (conditionWindowEnabled)
        {
            bool x = maincanvas_rightpanel.activeSelf;
            if (canvasEnabled != x)
            {
                x = !x;
                myCanvas.enabled = x;
                canvasEnabled = x;
            }
        }        
    }

    public void ShowAnnouncePanel() {
        blockmask.SetActive(true);
        announcePanel.SetActive(true);
        mcc.ChangeChosenObject(ChosenObjectType.None);
    }
    public void CloseAnnouncePanel() {
        announcePanel.SetActive(false);
        blockmask.SetActive(false);
    }
    // conditions
    public void ShowConditionPanel(Quest i_quest)
    {
        conditionQuest = i_quest;
        UpdateConditionInfo();        
        conditionWindowEnabled = true;
        conditionPanel.SetActive(conditionWindowEnabled);
        mcc.ChangeChosenObject(ChosenObjectType.None);
    }
    public void DisableConditionPanel()
    {
        conditionPanel.SetActive(false);
        conditionWindowEnabled = false;
    }

    public void UpdateConditionInfo()
    {
        condition0.text = conditionQuest.steps[0] + ' ' + conditionQuest.stepsAddInfo[0];
        int completed = 0, stcount = conditionQuest.steps.Length;
        bool finished = conditionQuest.stepsFinished[0];
        conditionCompleteMark0.enabled = finished;
        condition0.color = finished ? Color.grey : Color.white;
        if (finished) completed++;
        if (stcount != 1)
        {
            condition1.text = conditionQuest.steps[1] + ' ' + conditionQuest.stepsAddInfo[1];
            finished = conditionQuest.stepsFinished[1];
            conditionCompleteMark1.enabled = finished;
            condition1.color = finished ? Color.grey : Color.white;
            if (finished) completed++;
            if (!conditionLine1.activeSelf) conditionLine1.SetActive(true);
            if (stcount != 2)
            {
                condition2.text = conditionQuest.steps[2] + ' ' + conditionQuest.stepsAddInfo[2];
                finished = conditionQuest.stepsFinished[2];
                conditionCompleteMark2.enabled = finished;
                condition2.color = finished ? Color.grey : Color.white;
                if (finished) completed++;
                if (!conditionLine2.activeSelf) conditionLine2.SetActive(true);
            }
            else {
                if (conditionLine2.activeSelf) conditionLine2.SetActive(false);
            }
        }
        else
        {
            if (conditionLine1.activeSelf) conditionLine1.SetActive(false);
            if (conditionLine2.activeSelf) conditionLine2.SetActive(false);
        }

        bool x = completed == stcount;
        conditionButton.interactable = x;
        conditionButton.transform.GetChild(0).GetComponent<Text>().color = x ? Color.white : Color.grey;
    }
    public void ChangeConditionButtonLabel(string s)
    {
        conditionButton.transform.GetChild(0).GetComponent<Text>().text = s;
    }
    public void ConditionProceedButton()
    {
        workingScenario.UIConditionProceedButton();
    }
    // info string
    public void ShowInfoString(string s)
    {
        infoString0.text = s;
        if (!infoString0_enabled)
        {
            infoString0.transform.parent.gameObject.SetActive(true);
            infoString0_enabled = true;
        }
    }
    public void DisableInfoString()
    {
        if (infoString0_enabled)
        {
            infoString0.transform.parent.gameObject.SetActive(false);
            infoString0_enabled = false;
        }
    }
    // special button
    public void EnableSpecialButton(string s, Action i_action)
    {
        specialButton.transform.GetChild(0).GetComponent<Text>().text = s;
        specialButtonFunction = i_action;
        specialButton.SetActive(true);
    }
    public void SpecialButtonClick()
    {
        specialButtonFunction?.Invoke();
    }
    public void DisableSpecialButton()
    {
        specialButton.SetActive(false);
    }
    //

    private void SetScenario(Scenario s)
    {
        workingScenario = s;
    }
    public void OKButton()
    {
        workingScenario?.OKButton();
    }

    public void ChangeIcon(Texture t, Rect r)
    {
        icon.texture = t;
        icon.uvRect = r;
    }
    public void ChangeConditionIcon(Texture t, Rect r)
    {
        conditionIcon.texture = t;
        conditionIcon.uvRect = r;
    }
    public void ChangeAnnouncementText(string s)
    {
        announceText.text = s;
    }
    public void ScenarioEnds(Scenario s)
    {
        if (s == workingScenario) Destroy(gameObject);
    }

    //
    public void LocalizeTitles()
    {
        conditionButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Ready);
        Localization.AddToLocalizeList(this);
    }
    private void OnDestroy()
    {
        Localization.RemoveFromLocalizeList(this);
    }
}
