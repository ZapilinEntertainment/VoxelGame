using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIShuttleObserver : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private InputField nameField;
    [SerializeField] private Image hpbar;
    [SerializeField] private GameObject repairButton, hangarButton, closeButton, dissassembleButton;
    [SerializeField] private Text ownerInfo;
#pragma warning restore 0649
    private bool subscribedToUpdate = false;
    private float showingCondition = 0;
    private int lastDrawnShuttleHash = 0;
    private Shuttle showingShuttle;

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

    public void ShowShuttle(Shuttle s, bool useHangarButton, bool useCloseButton)
    {
        if (s == null) gameObject.SetActive(false);
        else
        {
            if (showingShuttle != s)
            {
                showingShuttle = s;
                hangarButton.SetActive(useHangarButton);
            }
            PrepareWindow();
            closeButton.SetActive(useCloseButton);
        }
    }

    private void PrepareWindow()
    {
        if (showingShuttle == null) gameObject.SetActive(false);
        else
        {
            nameField.text = showingShuttle.name;
            showingCondition = showingShuttle.condition;
            hpbar.fillAmount = showingCondition;
            if (showingShuttle.docked)
            {
                if (showingCondition != 1)
                {
                    repairButton.SetActive(true);
                    repairButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Repair) + ": " + showingShuttle.GetRepairCost().ToString();
                }
                else repairButton.SetActive(false);
                dissassembleButton.SetActive(true);
            }
            else
            {
                dissassembleButton.SetActive(false);
                repairButton.SetActive(false);
            }           


            if (showingShuttle.crew != null) ownerInfo.text = Localization.GetWord(LocalizedWord.Owner) + ":\"" + showingShuttle.crew.name + '"';
            else ownerInfo.text = Localization.GetPhrase(LocalizedPhrase.NoCrew);
            lastDrawnShuttleHash = Shuttle.listChangesMarkerValue;
        }
    }

    public void StatusUpdate()
    {
        if (showingCondition != showingShuttle.condition)
        {
            showingCondition = showingShuttle.condition;
            repairButton.SetActive(showingCondition != 1);
        }
        if (Shuttle.listChangesMarkerValue != lastDrawnShuttleHash)
        {
            if (showingShuttle.crew != null) ownerInfo.text = Localization.GetWord(LocalizedWord.Owner) + ":\"" + showingShuttle.crew.name + '"';
            else ownerInfo.text = Localization.GetPhrase(LocalizedPhrase.NoCrew);
            lastDrawnShuttleHash = Shuttle.listChangesMarkerValue;
        }
        if (showingShuttle.docked != dissassembleButton.activeSelf)
        {
            dissassembleButton.SetActive(showingShuttle.docked);
        }
    }

    //buttons
    public void NameChanged()
    {
        if (showingShuttle == null) gameObject.SetActive(false);
        else
        {
            showingShuttle.Rename(nameField.text);
            var ho = Hangar.hangarObserver;
            if (ho != null && ho.isActiveAndEnabled) ho.PrepareHangarWindow();
        }
    }
    public void RepairButton()
    {
        if (showingShuttle == null || showingShuttle.condition == 1) gameObject.SetActive(false);
        else
        {
            var colony = GameMaster.realMaster.colonyController;
            float cost = showingShuttle.GetRepairCost();
            if (colony.energyCrystalsCount >= cost)
            {
                colony.GetEnergyCrystals(cost);
                showingShuttle.SetCondition(1f);
            }
            else GameLogUI.NotEnoughMoneyAnnounce();
        }
    }
    public void PassToHangar()
    {
        if (showingShuttle == null) gameObject.SetActive(false);
        else
        {
            UIController.current.Select(showingShuttle.hangar);
        }
    }
    public void DeconstructButton()
    {
        //подтверждение?
        if (showingShuttle != null && showingShuttle.docked == true)
        {
            showingShuttle.Deconstruct();
            gameObject.SetActive(false);
        }
    }
    //

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
