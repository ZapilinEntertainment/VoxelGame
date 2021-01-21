using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnginePanelUI : MonoBehaviour
{
    [SerializeField] private RawImage buttonUp, buttonDown, buttonLeft, buttonRight, centralButton;
    [SerializeField] private Text directionLabel, positionLabel;
    private Engine.ThrustDirection showingDirection = Engine.ThrustDirection.Offline;
    private GlobalMap gmap;
    private MapPoint cityPoint;
    private readonly Color enabledColor = Color.white, disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private void OnEnable()
    {
        gmap = GameMaster.realMaster.globalMap;
        cityPoint = gmap.cityPoint;       
        RedrawState(gmap.engineThrustDirection);
    }

    public void ButtonLeft() {
        gmap.ChangeThrustDirection(Engine.ThrustDirection.Clockwise);
        RedrawState(gmap.engineThrustDirection);
    }
    public void ButtonRight()
    {
        gmap.ChangeThrustDirection(Engine.ThrustDirection.CounterclockWise);
        RedrawState(gmap.engineThrustDirection);
    }
    public void ButtonUp() {
        gmap.ChangeThrustDirection(Engine.ThrustDirection.Inside);
        RedrawState(gmap.engineThrustDirection);
    }
    public void ButtonDown() {
        gmap.ChangeThrustDirection(Engine.ThrustDirection.Outward);
        RedrawState(gmap.engineThrustDirection);
    }
    public void PowerButton() {
        if (showingDirection == Engine.ThrustDirection.Offline) gmap.ChangeThrustDirection(Engine.ThrustDirection.StabilityManeuver);
        else gmap.ChangeThrustDirection(Engine.ThrustDirection.Offline);
        RedrawState(gmap.engineThrustDirection);
    }
    private void RedrawState(Engine.ThrustDirection nd)
    {
        if (nd != showingDirection)
        {
            switch (nd)
            {
                case Engine.ThrustDirection.Clockwise: buttonLeft.color = disabledColor; break;
                case Engine.ThrustDirection.CounterclockWise: buttonRight.color = disabledColor; break;
                case Engine.ThrustDirection.Inside: buttonUp.color = disabledColor; break;
                case Engine.ThrustDirection.Outward: buttonDown.color = disabledColor; break;
                case Engine.ThrustDirection.StabilityManeuver: centralButton.color = disabledColor; break;
            }
            showingDirection = nd;
        }
        switch (showingDirection)
        {
            case Engine.ThrustDirection.Clockwise: buttonLeft.color = enabledColor; break;
            case Engine.ThrustDirection.CounterclockWise: buttonRight.color = enabledColor; break;
            case Engine.ThrustDirection.Inside: buttonUp.color = enabledColor; break;
            case Engine.ThrustDirection.Outward: buttonDown.color = enabledColor; break;
            case Engine.ThrustDirection.StabilityManeuver: centralButton.color = enabledColor; break;
            case Engine.ThrustDirection.Offline: centralButton.color = disabledColor; break;
        }
        if (gmap.engineControlCenterBuilt)
        {
            buttonUp.transform.parent.gameObject.SetActive(true);
            buttonDown.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            buttonUp.transform.parent.gameObject.SetActive(false);
            buttonDown.transform.parent.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        directionLabel.text = gmap.cityFlyDirection.ToString();
        positionLabel.text = ((int)cityPoint.angle).ToString() + " ; " + string.Format("{0:0.##}", cityPoint.height);
    }
}
