using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class StandartScenarioUI : MonoBehaviour, ILocalizable
{
    [SerializeField] private GameObject announcePanel, conditionPanel, blockmask, conditionLine1, conditionLine2;
    [SerializeField] private Text announceText, condition0, condition1, condition2;
    [SerializeField] private RawImage icon, conditionCompleteMark0, conditionCompleteMark1, conditionCompleteMark2;
    [SerializeField] private Button conditionButton;
    private Scenario workingScenario;
    private Quest conditionQuest;
    private static StandartScenarioUI current;

    public static StandartScenarioUI GetCurrent(Scenario requester)
    {
        if (current == null) current = Instantiate(Resources.Load<GameObject>("UIPrefs/scenarioCanvas")).GetComponent<StandartScenarioUI>();
        current.SetScenario(requester);
        return current;
    }

    private void Awake()
    {
        announcePanel.SetActive(false);
        blockmask.SetActive(false);
        LocalizeTitles();
    }
   
    public void ShowAnnouncePanel() {
        blockmask.SetActive(true);
        announcePanel.SetActive(true);
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
        conditionButton.interactable = false;
        conditionPanel.SetActive(true);
    }
    public void DisableConditionPanel()
    {
        conditionPanel.SetActive(false);
    }

    public void UpdateConditionInfo()
    {        
        condition0.text = conditionQuest.steps[0] + conditionQuest.stepsAddInfo[0];
        conditionCompleteMark0.enabled = conditionQuest.stepsFinished[0];
        var stcount = conditionQuest.steps.Length;
        if (stcount != 1)
        {
            condition1.text = conditionQuest.steps[1] + conditionQuest.stepsAddInfo[1];
            conditionCompleteMark1.enabled = conditionQuest.stepsFinished[1];
            if (!conditionLine1.activeSelf) conditionLine1.SetActive(true);
            if (stcount != 2)
            {
                condition2.text = conditionQuest.steps[2] + conditionQuest.stepsAddInfo[2];
                conditionCompleteMark2.enabled = conditionQuest.stepsFinished[2];
                if (!conditionLine2.activeSelf) conditionLine2.SetActive(true);
            }
            else {
                if (conditionLine1.activeSelf) conditionLine1.SetActive(false);
            }
        }
        else
        {
            if (conditionLine1.activeSelf) conditionLine1.SetActive(false);
            if (conditionLine2.activeSelf) conditionLine1.SetActive(false);
        }
    }
    public void ChangeConditionButtonLabel(string s)
    {
        conditionButton.transform.GetChild(0).GetComponent<Text>().text = s;
    }
    public void SetConditionButtonActivity(bool x) { conditionButton.interactable = x; }
    public void ConditionProceedButton()
    {
        workingScenario.UIConditionProceedButton();
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
