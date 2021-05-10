using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ConditionWindowController : MonoBehaviour
{
    [SerializeField] private RawImage mainIcon;
    [SerializeField] private Transform conditionLinesHolder;
    [SerializeField] private Button actionButton;
    private bool prepared = false;
    private Text buttonText { get { return actionButton.transform.GetChild(0).GetComponent<Text>(); } }
    private (Text label, RawImage taskIcon)[] conditionsArray;
    private Quest conditionQuest;
    private Action clickAction;
    private int conditionSlots, activeSlotsCount;
    private static Rect completedRect, incompletedRect;
    private static bool rectsReady = false;

    private void Prepare()
    {
        conditionSlots = conditionLinesHolder.childCount;
        conditionsArray = new (Text, RawImage)[conditionSlots];
        Transform t;
        for (int i = 0;i < conditionSlots; i++)
        {
            t = conditionLinesHolder.GetChild(i);
            conditionsArray[i].label = t.GetChild(0).GetComponent<Text>();
            conditionsArray[i].taskIcon = t.GetChild(1).GetComponent<RawImage>();
        }
        if (!rectsReady)
        {
            completedRect = UIController.GetIconUVRect(Icons.TaskCompleted);
            incompletedRect = UIController.GetIconUVRect(Icons.TaskFrame);
            rectsReady = true;
        }
        buttonText.text = Localization.GetWord(LocalizedWord.Ready);
        prepared = true;
    }


    public void SetConditions(Quest i_conditionQuest, Action i_clickAction)
    {
        if (!prepared) Prepare();
        conditionQuest = i_conditionQuest;
        activeSlotsCount = conditionQuest.steps.Length;
        bool x = true;
        int completedSteps = 0;
        for (int i = 0; i< conditionSlots; i++)
        {
            x = (i < activeSlotsCount);
            if (x)
            {
                FillConditionInfo(i, ref completedSteps);
                conditionLinesHolder.GetChild(i).gameObject.SetActive(true);
            }
            else conditionLinesHolder.GetChild(i).gameObject.SetActive(false);
        }
        if (i_clickAction != null)
        {
            actionButton.interactable = completedSteps == activeSlotsCount;
            clickAction = i_clickAction;
            actionButton.gameObject.SetActive(true);
        }
        else actionButton.gameObject.SetActive(false);
    }
    public void Refresh()
    {
        int x = 0;
        for (int i = 0; i < activeSlotsCount; i++)
        {
            FillConditionInfo(i, ref x);
        }
        actionButton.interactable = x == activeSlotsCount;
    }
    private void FillConditionInfo(in int i, ref int completedSteps)
    {
        Text t = conditionsArray[i].label;
        bool x = conditionQuest.stepsFinished[i];
        t.text = conditionQuest.steps[i] + ' ' + conditionQuest.stepsAddInfo[i];
        t.color = x ? Color.grey : Color.white;
        conditionsArray[i].taskIcon.uvRect = x ? completedRect : incompletedRect;
        if (x) completedSteps++;
    }

    public void SetButtonText(string s)
    {
        buttonText.text = s;
    }

    public void SetMainIcon(Texture t, Rect r)
    {
        mainIcon.texture = t;
        mainIcon.uvRect = r;
    }

    public void ActionButton()
    {
        if (clickAction != null) clickAction.Invoke();
        else
        {
            Debug.Log("no action!");
        }
    }
}
