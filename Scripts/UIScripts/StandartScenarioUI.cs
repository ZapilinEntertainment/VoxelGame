using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class StandartScenarioUI : MonoBehaviour
{
    [SerializeField] private GameObject announcePanel, blockmask, specialButton;
    [SerializeField] private Text announceText,infoString0;
    [SerializeField] private RawImage icon;
    [SerializeField] private ConditionWindowController[] conditionWindows;
    private bool canvasEnabled = true, infoString0_enabled = false;
    private GameObject maincanvas_rightpanel;
    private Scenario workingScenario;
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
        specialButton.SetActive(false);
        infoString0.transform.parent.gameObject.SetActive(false); infoString0_enabled = false;
        foreach(var cw in conditionWindows)
        {
            cw.gameObject.SetActive(false);
        }
        
        var uic = UIController.GetCurrent();
        uic.AddSpecialCanvasToHolder(transform);
        mcc = uic.GetMainCanvasController();
        maincanvas_rightpanel = mcc.rightPanel;
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
    public ConditionWindowController ShowConditionPanel(int i,Quest conditionQuest, Action i_clickAction)
    {
        var cwo = conditionWindows[i];
        cwo.SetConditions(conditionQuest, i_clickAction);
        cwo.gameObject.SetActive(true);
        return cwo;
    }
    public void DisableConditionPanel(int index)
    {
        conditionWindows[index].gameObject.SetActive(false);
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
    public void ChangeAnnouncementText(string s)
    {
        announceText.text = s;
    }
    public void ScenarioEnds(Scenario s)
    {
        if (s == workingScenario) Destroy(gameObject);
    }

}
