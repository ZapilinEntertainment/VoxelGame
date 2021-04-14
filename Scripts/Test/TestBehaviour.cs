using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestBehaviour : MonoBehaviour
{
    private RectTransform testWindow;
    [SerializeField] private RectTransform holder;
    [SerializeField] private SpriteAlignment alignment;
    [SerializeField] private Rect rect;

    private UIController uic;
    private MainCanvasController mcc;

    void Start()
    {
        uic = UIController.GetCurrent();
        mcc = uic.GetMainCanvasController();

        //testWindow = Instantiate(Resources.Load<RectTransform>("UIPrefs/ExpeditionPanel"), holder);
       // Destroy(testWindow.GetComponent<UIExpeditionObserver>());
        //SetPosition();
    }

    internal void SetPosition() {
        float pw = holder.rect.width, ph = holder.rect.height, sz = ph;
        if (pw < ph) sz = pw;
        rect = new Rect(0f, 0f, sz/2f, sz/2f);
        SetPosition(rect, alignment);
    }
    internal void SetPosition(Rect rect, SpriteAlignment alignment)
    {
        var rt = testWindow;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
        //       
        Vector2 anchor;
        switch (alignment)
        {
            case SpriteAlignment.BottomRight:
                anchor = Vector2.right;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.left * rect.width;
                break;
            case SpriteAlignment.BottomLeft:
                anchor = Vector2.zero;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.zero;
                break;
            case SpriteAlignment.RightCenter:
                anchor = new Vector2(1f, 0.5f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-1f * rect.width, -0.5f * rect.height);
                break;
            case SpriteAlignment.TopRight:
                anchor = Vector2.one;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-1f * rect.width, -1f * rect.height);
                break;
            case SpriteAlignment.Center:
                anchor = Vector2.one * 0.5f;
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-0.5f * rect.width, -0.5f * rect.height);
                break;
            case SpriteAlignment.TopCenter:
                anchor = new Vector2(0.5f, 1f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-0.5f * rect.width, -1f * rect.height);
                break;
            case SpriteAlignment.BottomCenter:
                anchor = new Vector2(0.5f, 0f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = new Vector2(-0.5f * rect.width, 0f);
                break;
            case SpriteAlignment.TopLeft:
                anchor = new Vector2(0f, 1f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.down * rect.height;
                break;
            case SpriteAlignment.LeftCenter:
                anchor = new Vector2(0f, 0.5f);
                rt.anchorMax = anchor;
                rt.anchorMin = anchor;
                rt.anchoredPosition = Vector2.down * rect.height * 0.5f;
                break;
        }        
    }

    private void Something(GameObject[] obj)
    {
        if (obj != null)
        {
            int i = 0;
            var rtypes = ResourceType.materialsForCovering;
            MeshFilter mf;
            MeshRenderer mr;
            while (i < obj.Length & i < rtypes.Length)
            {
                mf = obj[i].GetComponent<MeshFilter>();
                mr = obj[i].GetComponent<MeshRenderer>();
                PoolMaster.SetMaterialByID(ref mf, ref mr, rtypes[i].ID, 255);
                i++;
            }
        }
    }
}

[CustomEditor(typeof (TestBehaviour))]
public class TestBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TestBehaviour script = (TestBehaviour)target;
       // if (GUILayout.Button("Reposition")) script.SetPosition();
    }
}
