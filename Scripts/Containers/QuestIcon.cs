using UnityEngine;

public enum QuestIcon : byte { FoundationRouteIcon, PeopleIcon, TutorialIcon }
public static class QuestIconExtension
{
    public static void GetIconInfo(this QuestIcon iconType, ref Texture icon, ref Rect rect)
    {
        //use subIndex where stores construction info
        // 
        switch (iconType)
        {
            case QuestIcon.FoundationRouteIcon:
                {
                    icon = UIController.iconsTexture;
                    rect = UIController.GetIconUVRect(Icons.FoundationRoute);
                    break;
                }
            case QuestIcon.PeopleIcon:
                {
                    icon = UIController.iconsTexture;
                    rect = UIController.GetIconUVRect(Icons.Citizen);
                    break;
                }
            case QuestIcon.TutorialIcon:
                {
                    icon = UIController.iconsTexture;
                    rect = UIController.GetIconUVRect(Icons.SecretKnowledgeIcon);
                    break;
                }
            default:
                {
                    icon = GlobalMapCanvasController.GetMapMarkersTexture();
                    rect = GlobalMapCanvasController.GetMarkerRect(MapPointType.QuestMark);
                    break;
                }
        }
    }
}

