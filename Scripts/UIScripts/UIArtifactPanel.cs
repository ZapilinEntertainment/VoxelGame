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
    [SerializeField] private GameObject conservateButton, passButton, closeButton;
    private bool noArtifacts = false, descriptionOff = false, subscribedToUpdate = false;
    private int lastDrawnActionHash = 0;
    private Artifact chosenArtifact;

    private void RedrawWindow()
    {
        if (chosenArtifact == null)
        {
            if (!descriptionOff)
            {
                affectionIcon.transform.parent.gameObject.SetActive(false);
                nameField.gameObject.SetActive(false);
                status.enabled = false;
                conservateButton.SetActive(false);
                passButton.SetActive(false);
                description.text = noArtifacts ? Localization.GetPhrase(LocalizedPhrase.NoArtifacts) : string.Empty;
                descriptionOff = true;
            }
        }
        else
        {
            nameField.text = chosenArtifact.name;
            status.text = Localization.GetArtifactStatus(chosenArtifact.status);
            if (chosenArtifact.researched)
            {
                affectionIcon.sprite = Artifact.GetAffectionSprite(chosenArtifact.affectionType);                
                affectionIcon.enabled = true;
                // localization - write artifact info 
            }
            else
            {
                description.text = Localization.GetPhrase(LocalizedPhrase.NotResearched);
                affectionIcon.enabled = false;
            }
            mainIcon.texture = chosenArtifact.GetTexture();
            conservateButton.SetActive(chosenArtifact.status == Artifact.ArtifactStatus.UsingByCrew);
            if (chosenArtifact.status != Artifact.ArtifactStatus.Exists)
            {
                var ri = passButton.transform.GetChild(0).GetComponent<RawImage>();
                switch (chosenArtifact.status)
                {
                    case Artifact.ArtifactStatus.Researching:
                        // установка иконки иссл лаб
                        break;
                    case Artifact.ArtifactStatus.UsingByCrew:                        
                        ri.texture = UIController.current.iconsTexture;
                        ri.uvRect = UIController.GetTextureUV(Icons.CrewGoodIcon);
                        break;
                    case Artifact.ArtifactStatus.UsingInMonument:
                        ri.texture = UIController.current.buildingsIcons;
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
        lastDrawnActionHash = Artifact.actionsHash;
    }

    private void Update()
    {
        if (chosenArtifact != null)
        {
            if (chosenArtifact.status == Artifact.ArtifactStatus.UsingInMonument | chosenArtifact.status == Artifact.ArtifactStatus.UsingByCrew)
            {
                affectionIcon.transform.Rotate(Vector3.forward, chosenArtifact.frequency * 10f * Time.deltaTime);
            }
        }
    }

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
    public void ShowArtifact(Artifact a, bool useCloseButton)
    {
        chosenArtifact = a;
        RedrawWindow();
        closeButton.SetActive(useCloseButton);
    }

    // buttons
    public void NameChanged()
    {
        if (chosenArtifact != null)
        {
            chosenArtifact.ChangeName(nameField.text);
        }
        else RedrawWindow();
    }
    public void ConservateButton()
    {
        if (chosenArtifact != null)
        {
            if (chosenArtifact.owner != null)
            {
                chosenArtifact.owner.DropArtifact();
                // добавить в хранилище
                return;
            }
        }
        RedrawWindow();
    }
    public void PassButton()
    {
        if (chosenArtifact != null)
        {
            switch (chosenArtifact.status)
            {
                case Artifact.ArtifactStatus.Exists: RedrawWindow();break;
                case Artifact.ArtifactStatus.Researching:
                    //goto research center
                    break;
                case Artifact.ArtifactStatus.UsingByCrew:
                    if (chosenArtifact.owner != null)
                    {
                        var rt = GetComponent<RectTransform>();
                        var r = new Rect(rt.position + Vector3.right * rt.rect.width / 2f, new Vector2(rt.rect.height, rt.rect.height));
                        chosenArtifact.owner.ShowOnGUI(r, SpriteAlignment.LeftCenter, true);
                        gameObject.SetActive(false);
                    }
                    else RedrawWindow();
                    break;
                case Artifact.ArtifactStatus.UsingInMonument:
                    // goto monument
                    break;
            }
        }
        else RedrawWindow();
    }
  

    private void OnEnable()
    {
        if (!subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent += RedrawWindow;
            subscribedToUpdate = true;
        }
    }
    private void OnDisable()
    {
        if (subscribedToUpdate)
        {
            UIController.current.statusUpdateEvent -= RedrawWindow;
            subscribedToUpdate = false;
        }
    }
    private void OnDestroy()
    {
        if (!GameMaster.sceneClearing & subscribedToUpdate)
        {
            if (subscribedToUpdate)
            {
                UIController uc = UIController.current;
                if (uc != null) uc.statusUpdateEvent -= RedrawWindow;
                subscribedToUpdate = false;
            }
        }
    }
}
