using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactPanel : MonoBehaviour {
    [SerializeField] private GameObject[] items;
    [SerializeField] private Image affectionIcon;
    [SerializeField] private RawImage mainIcon;
    [SerializeField] private InputField nameField;
    [SerializeField] private Text status, description;
    [SerializeField] private GameObject passButton, closeButton;
    private bool noArtifacts = false, descriptionOff = false, subscribedToUpdate = false;
    private int lastDrawnActionHash = 0;
    private Artifact observingArtifact;
    private MainCanvasController myCanvas;
    private static UIArtifactPanel _currentObserver;


    #region observer standart functions
    public static UIArtifactPanel GetObserver()
    {
        if (_currentObserver == null)
        {
            var mc = UIController.GetCurrent().GetMainCanvasController();
            _currentObserver = Instantiate(Resources.Load<GameObject>("UIPrefs/artifactPanel"), 
                mc.GetMainCanvasTransform()).GetComponent<UIArtifactPanel>();
            _currentObserver.myCanvas = mc;
        }
        return _currentObserver;
    }
    public static void Show(RectTransform parent, SpriteAlignment alignment, Artifact a, bool useCloseButton)
    {
        Show(parent, new Rect(Vector2.zero, parent.rect.size), alignment, a, useCloseButton);
    }
    public static void Show(RectTransform parent, Rect r, SpriteAlignment alignment, Artifact a, bool useCloseButton)
    {
        var co = GetObserver();
        if (!co.gameObject.activeSelf) co.gameObject.SetActive(true);
        co.SetPosition(parent, r, alignment, useCloseButton);
        co.ShowArtifact(a, useCloseButton);
    }
    public static void DisableObserver()
    {
        if (_currentObserver != null) _currentObserver.gameObject.SetActive(false);
    }
    public static void DestroyObserver()
    {
        if (_currentObserver != null) Destroy(_currentObserver.gameObject);
    }
    public static void Refresh()
    {
        if (_currentObserver != null) _currentObserver.RedrawWindow();
    }

    private void SetPosition(RectTransform parent, Rect r, SpriteAlignment alignment, bool useCloseButton)
    {
        closeButton.SetActive(useCloseButton);
        var rt = GetObserver().GetComponent<RectTransform>();
        UIController.PositionElement(rt, parent, alignment, r);
    }
    private void ShowArtifact(Artifact a, bool useCloseButton)
    {
        if (a == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            observingArtifact = a;
            RedrawWindow();
            closeButton.SetActive(useCloseButton);
        }
    }
    public void ClearInfo(Artifact a)
    {
        if (_currentObserver != null && _currentObserver.observingArtifact == a)
        {
            _currentObserver.gameObject.SetActive(false);
        }
    }
    #endregion

    private void RedrawWindow()
    {
        if (observingArtifact == null)
        {
            if (!descriptionOff)
            {
                affectionIcon.transform.parent.gameObject.SetActive(false);
                nameField.gameObject.SetActive(false);
                status.enabled = false;
                passButton.SetActive(false);
                description.text = noArtifacts ? Localization.GetPhrase(LocalizedPhrase.NoArtifacts) : string.Empty;
                descriptionOff = true;
            }
        }
        else
        {
            nameField.text = observingArtifact.name;
            status.text = Localization.GetArtifactStatus(observingArtifact.status);
            if (observingArtifact.researched)
            {
                affectionIcon.sprite = Artifact.GetAffectionSprite(observingArtifact.affectionPath);                
                affectionIcon.enabled = true;
                // localization - write artifact info 
            }
            else
            {
                description.text = Localization.GetPhrase(LocalizedPhrase.NotResearched);
                affectionIcon.enabled = false;
            }
            mainIcon.texture = observingArtifact.GetTexture();
            if (observingArtifact.status != Artifact.ArtifactStatus.Uncontrollable)
            {
                var ri = passButton.transform.GetChild(0).GetComponent<RawImage>();
                switch (observingArtifact.status)
                {
                    case Artifact.ArtifactStatus.Researching:
                        // установка иконки иссл лаб
                        break;
                    case Artifact.ArtifactStatus.UsingInMonument:
                        ri.texture = myCanvas.buildingsIcons;
                        ri.uvRect = Structure.GetTextureRect(Structure.MONUMENT_ID);
                        //иконка монумента
                        break;
                }
                passButton.SetActive(true);
            }
            else passButton.SetActive(false);

            if (descriptionOff)
            {
                affectionIcon.transform.parent.gameObject.SetActive(true);
                nameField.gameObject.SetActive(true);
                status.enabled = true;
                descriptionOff = false;
            }
        }
        lastDrawnActionHash = Artifact.listChangesMarkerValue;
    }
    private void Update()
    {
        if (observingArtifact != null)
        {
            if (observingArtifact.status == Artifact.ArtifactStatus.UsingInMonument)
            {
                affectionIcon.transform.Rotate(Vector3.forward, observingArtifact.frequency * 10f * Time.deltaTime);
            }
        }
    }

    // buttons
    public void NameChanged()
    {
        if (observingArtifact != null)
        {
            observingArtifact.ChangeName(nameField.text);
        }
        else RedrawWindow();
    }
  

    private void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            myCanvas.statusUpdateEvent += RedrawWindow;
            subscribedToUpdate = true;
        }
    }
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            myCanvas.statusUpdateEvent -= RedrawWindow;
            subscribedToUpdate = false;
        }
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            if (subscribedToUpdate)
            {
                if (myCanvas != null) myCanvas.statusUpdateEvent -= RedrawWindow;
                subscribedToUpdate = false;
            }
        }
    }
}
