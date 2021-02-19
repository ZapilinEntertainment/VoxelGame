using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndPanelController : MonoBehaviour
{
    [SerializeField] private Text endLabel, scoreLabel;
    [SerializeField] private Button continueGameButton, returnToMenuButton;
    [SerializeField] private RawImage backgroundImage;

    public void Prepare(GameEndingType endType, ulong score)
    {
        string reason = Localization.GetEndingTitle(endType);
        var rmaster = GameMaster.realMaster;
        switch (endType)
        {
            case GameEndingType.FoundationRoute:
                {
                    endLabel.text = reason;
                    scoreLabel.text = Localization.GetWord(LocalizedWord.Score) + ": " + ((int)score).ToString();
                    returnToMenuButton.onClick.AddListener(GameMaster.ReturnToMainMenu);
                    returnToMenuButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.MainMenu);
                    continueGameButton.onClick.AddListener(rmaster.ContinueGameAfterEnd);
                    continueGameButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Continue);
                    break;
                }
            case GameEndingType.ColonyLost:
            case GameEndingType.Default:
            case GameEndingType.ConsumedByReal:
            case GameEndingType.ConsumedByLastSector:
            default:
                {
                    endLabel.text = reason;
                    scoreLabel.text = Localization.GetWord(LocalizedWord.Score) + ": " + ((int)score).ToString();
                    backgroundImage.texture = Resources.Load<Texture>("Textures/gameover_texture");
                    continueGameButton.enabled = false;
                    returnToMenuButton.enabled = false;
                    var rt = returnToMenuButton.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
                    rt.offsetMin = new Vector2(rt.offsetMax.x, rt.offsetMin.y);
                    returnToMenuButton.onClick.AddListener(GameMaster.ReturnToMainMenu);
                    returnToMenuButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.MainMenu);
                    returnToMenuButton.enabled = true;
                    break;
                }
        }
    }
}
