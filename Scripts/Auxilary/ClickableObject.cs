using UnityEngine;
using System;

public sealed class ClickableObject : MonoBehaviour
{
    private Action action;
    public const string CLICKABLE_TAG= "Clickable";

    private void Awake()
    {
        gameObject.tag = CLICKABLE_TAG;
    }

    public void AssignFunction(Action a)
    {
        if (action == null) action = a;
        else action += a;
    }

    public void Clicked()
    {
        action?.Invoke();
    }
}
