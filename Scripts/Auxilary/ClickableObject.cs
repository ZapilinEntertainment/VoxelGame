using UnityEngine;
using System;

public sealed class ClickableObject : MonoBehaviour
{
    private Action clickAction, lostFocusAction;
    public const string CLICKABLE_TAG= "Clickable";

    private void Awake()
    {
        gameObject.tag = CLICKABLE_TAG;
    }

    public void AssignFunction(Action click, Action lostFocus)
    {
        clickAction = click;
        lostFocusAction = lostFocus;
    }

    public void Clicked()
    {
        clickAction?.Invoke();
    }
    public void LostFocus()
    {
        lostFocusAction?.Invoke();
    }
}
