using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class KnowledgeTabUI : MonoBehaviour
{
    [SerializeField] private GameObject zeroButton;
    [SerializeField] private Image unsufficientLabel, unsufficientLight;
    [SerializeField] private Transform holder, infoPanel;
    private Knowledge knowledge;
    private GameObject[] buttons;
    private readonly Rect plainSide = new Rect(0f, 0f, 0.5f, 0.5f), pinSide = new Rect(0f, 0.5f, 0.5f, 0.5f), cutSide = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
    private int lastChMarkerValue;
    private bool unsufficientMarkering = false, prepared = false;

    private Transform ascensionPanel { get { return infoPanel.GetChild(1); } }
    private Transform redpartsPanel { get { return infoPanel.GetChild(2); } }
    private Transform greenpartsPanel { get { return infoPanel.GetChild(3); } }
    private Transform bluepartsPanel { get { return infoPanel.GetChild(4); } }
    private Transform cyanpartsPanel { get { return infoPanel.GetChild(5); } }
    private Transform blackpartsPanel { get { return infoPanel.GetChild(6); } }
    private Transform whitepartsPanel { get { return infoPanel.GetChild(7); } }

    private readonly Color unsufficientColor = new Color(0.67f, 0.06f, 0.06f, 1f), unvisibleColor = new Color(0f, 0f, 0f, 0f);
    private const float DISAPPEAR_SPEED = 1.5f;

    public void Prepare(Knowledge kn)
    {
        knowledge = kn;
        if (buttons == null) {
            buttons = new GameObject[64];
            buttons[0] = zeroButton;
            holder.gameObject.SetActive(false);
            infoPanel.gameObject.SetActive(false);

            RectTransform rt = holder.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.height);
            float xmin, ymin;
            var pps = knowledge.puzzlePins;
            int row, column;
            for (int i = 1; i < 64; i++)
            {
                row = i / 8;
                column = i % 8;

                rt = Instantiate(zeroButton, holder).GetComponent<RectTransform>();
                xmin = -0.0625f + column * 0.125f;
                ymin = -0.0625f + row * 0.125f;
                rt.anchorMin = new Vector2(xmin, ymin);
                rt.anchorMax = new Vector2(xmin + 0.25f, ymin + 0.25f);
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                
                //up
                rt.GetChild(0).GetComponent<RawImage>().uvRect = (row == 7 ? plainSide : 
                    (pps[row * 15 + 7 + column] == true ? pinSide : cutSide )
                    );
                //right
                rt.GetChild(1).GetComponent<RawImage>().uvRect = (column == 7 ? plainSide :
                    (pps[row * 15 + column] == true ? pinSide : cutSide)
                    );
                //down
                rt.GetChild(2).GetComponent<RawImage>().uvRect = (row == 0 ? plainSide :
                    (pps[(row - 1) * 15 + 7 + column] == true ? cutSide : pinSide)
                    );
                //left
                rt.GetChild(3).GetComponent<RawImage>().uvRect = (column == 0 ? plainSide :
                    (pps[row * 15 + column - 1] == false ? pinSide : cutSide)
                    );

                int f_index = i;
                rt.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate { this.Click(f_index); } );
                buttons[i] = rt.gameObject;                
            }
            zeroButton.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate { this.Click(0); });          
        
            ascensionPanel.GetChild(0).GetComponent<RawImage>().uvRect = UIController.GetIconUVRect(Icons.AscensionIcon);
            redpartsPanel.GetChild(0).GetComponent<RawImage>().color = Knowledge.colors[Knowledge.REDCOLOR_CODE];
            greenpartsPanel.GetChild(0).GetComponent<RawImage>().color = Knowledge.colors[Knowledge.GREENCOLOR_CODE];
            bluepartsPanel.GetChild(0).GetComponent<RawImage>().color = Knowledge.colors[Knowledge.BLUECOLOR_CODE];
            cyanpartsPanel.GetChild(0).GetComponent<RawImage>().color = Knowledge.colors[Knowledge.CYANCOLOR_CODE];
            blackpartsPanel.GetChild(0).GetChild(0).GetComponent<RawImage>().color = Knowledge.colors[Knowledge.BLACKCOLOR_CODE];
            whitepartsPanel.GetChild(0).GetComponent<RawImage>().color = Knowledge.colors[Knowledge.WHITECOLOR_CODE];

            unsufficientLabel.gameObject.SetActive(false);
            unsufficientLight.gameObject.SetActive(false);
            infoPanel.gameObject.SetActive(true);
            holder.gameObject.SetActive(true);

            Redraw();
        }
        infoPanel.GetChild(infoPanel.transform.childCount - 1).GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Return);
        prepared = true;
    }

    public void Redraw()
    {
        var carray = knowledge.colorCodesArray;
        var colors = Knowledge.colors;
        GameObject b;
        Transform bt;
        byte code;
        for (int i =0; i < 64; i++)
        {
            code = carray[i];
            b = buttons[i];            
            if (code != Knowledge.NOCOLOR_CODE)
            {
                bt = buttons[i].transform;
                bt.GetChild(0).GetComponent<RawImage>().color = colors[code];
                bt.GetChild(1).GetComponent<RawImage>().color = colors[code];
                bt.GetChild(2).GetComponent<RawImage>().color = colors[code];
                bt.GetChild(3).GetComponent<RawImage>().color = colors[code];
                if (!b.activeSelf) b.SetActive(true);
            }
            else
            {
                if (b.activeSelf) b.SetActive(false);
            }
        }

        var parts = knowledge.puzzlePartsCount;
        ascensionPanel.GetChild(1).GetComponent<Text>().text = ((int)(knowledge.completeness * 100f)).ToString() + '%';
        redpartsPanel.GetChild(1).GetComponent<Text>().text = parts[Knowledge.REDCOLOR_CODE].ToString();
        greenpartsPanel.GetChild(1).GetComponent<Text>().text = parts[Knowledge.GREENCOLOR_CODE].ToString();
        bluepartsPanel.GetChild(1).GetComponent<Text>().text = parts[Knowledge.BLUECOLOR_CODE].ToString();
        cyanpartsPanel.GetChild(1).GetComponent<Text>().text = parts[Knowledge.CYANCOLOR_CODE].ToString();
        var t = blackpartsPanel;
        var pc = parts[Knowledge.BLACKCOLOR_CODE];
        if (pc > 0)
        {
            t.GetChild(1).GetComponent<Text>().text = pc.ToString();
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
        }
        else
        {
            if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
        }
        t = whitepartsPanel;
        pc = parts[Knowledge.WHITECOLOR_CODE];
        if (pc > 0)
        {
            t.GetChild(1).GetComponent<Text>().text = pc.ToString();
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
        }
        else
        {
            if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
        }

        lastChMarkerValue = knowledge.changesMarker;
    }

    private void Update()
    {
        if (unsufficientMarkering)
        {
            Color col = Vector4.MoveTowards(unsufficientLight.color, unvisibleColor, DISAPPEAR_SPEED * Time.deltaTime);
            if (col == unvisibleColor)
            {
                unsufficientMarkering = false;
                unsufficientLabel.gameObject.SetActive(false);
                unsufficientLight.gameObject.SetActive(false);
            }
            else
            {
                unsufficientLight.color = col;
                unsufficientLabel.color = col;
            }
        }
        if (knowledge == null) return;
        if (lastChMarkerValue != knowledge.changesMarker)
        {
            Redraw();
        }

        //test
        if (Input.GetKeyDown("r")) knowledge.AddResearchPoints((Knowledge.ResearchRoute)Random.Range(0, 8), Random.value * 20f);
    }

    public void Click(int i)
    {
        if (knowledge.UnblockButton(i) == true)
        {
            if (unsufficientMarkering)
            {
                unsufficientMarkering = false;
                unsufficientLabel.gameObject.SetActive(false);
                unsufficientLight.gameObject.SetActive(false);
            }
            Redraw();
        }
        else
        {
            var code = knowledge.colorCodesArray[i];
            if (code != Knowledge.BLACKCOLOR_CODE & code != Knowledge.WHITECOLOR_CODE)
            {
                ShowUnsufficientParts(code);
            }
            else
            {
                if (
                    code == Knowledge.BLACKCOLOR_CODE & knowledge.allRoutesUnblocked |
                    code == Knowledge.WHITECOLOR_CODE & knowledge.puzzlePartsCount[Knowledge.WHITECOLOR_CODE] > 0
                    )
                    ShowUnsufficientParts(code);
            }
        }
    }

    private void ShowUnsufficientParts(byte colorcode)
    {
        switch (colorcode)
        {
            case Knowledge.REDCOLOR_CODE:
                unsufficientLight.transform.position = redpartsPanel.transform.position;                              
                break;
            case Knowledge.GREENCOLOR_CODE:
                unsufficientLight.transform.position = greenpartsPanel.transform.position;
                break;
            case Knowledge.BLUECOLOR_CODE:
                unsufficientLight.transform.position = bluepartsPanel.transform.position;
                break;
            case Knowledge.CYANCOLOR_CODE:
                unsufficientLight.transform.position = cyanpartsPanel.transform.position;
                break;
            case Knowledge.BLACKCOLOR_CODE:
                unsufficientLight.transform.position = blackpartsPanel.transform.position;
                break;
            case Knowledge.WHITECOLOR_CODE:
                unsufficientLight.transform.position = whitepartsPanel.transform.position;
                break;
            default: return;
        }
        unsufficientLight.color = unsufficientColor;
        unsufficientLight.gameObject.SetActive(true);
        unsufficientLabel.color = unsufficientColor;
        unsufficientLabel.gameObject.SetActive(true);
        unsufficientMarkering = true;
    }

    private void OnEnable()
    {
        if (knowledge == null)
        {            
            gameObject.SetActive(false);            
            return;
        }
        else
        {
            UIController.SetActivity(false);
            if (!prepared) Prepare(knowledge);
            else
            {
                if (lastChMarkerValue != knowledge.changesMarker) Redraw();
            }
        }
    }
    private void OnDisable()
    {
        GameMaster.realMaster.environmentMaster.EnableDecorations();
        UIController.SetActivity(true);
    }
}
