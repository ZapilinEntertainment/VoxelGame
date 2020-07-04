using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class KnowledgeTabUI : MonoBehaviour
{
    [SerializeField] private GameObject zeroButton;
    [SerializeField] private Image unsufficientLabel, unsufficientLight;
    [SerializeField] private Transform holder, infoPanel;
    [SerializeField] private RectTransform unblockAnnouncePanel;
    [SerializeField] private RectTransform[] routeBackgrounds;
    private Knowledge knowledge;
    private GameObject[] buttons;
   
    private int lastChMarkerValue;
    private bool unsufficientMarkering = false, prepared = false;

    private Transform ascensionPanel { get { return infoPanel.GetChild(1); } }
    private Transform redpartsPanel { get { return infoPanel.GetChild(2); } }
    private Transform greenpartsPanel { get { return infoPanel.GetChild(3); } }
    private Transform bluepartsPanel { get { return infoPanel.GetChild(4); } }
    private Transform cyanpartsPanel { get { return infoPanel.GetChild(5); } }
    private Transform blackpartsPanel { get { return infoPanel.GetChild(6); } }
    private Transform whitepartsPanel { get { return infoPanel.GetChild(7); } }
    private GameObject[] endQuestButtons = new GameObject[8];

    private static Texture2D puzzleparts_tx, puzzleParts_origin_resized;
    private static int puzzleTexSize;

    private static readonly Rect plainSide = new Rect(0f, 0f, 0.5f, 0.5f), pinSide = new Rect(0f, 0.5f, 0.5f, 0.5f), cutSide = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
    private readonly Color unsufficientColor = new Color(0.67f, 0.06f, 0.06f, 1f), unvisibleColor = new Color(0f, 0f, 0f, 0f);
    private const float DISAPPEAR_SPEED = 1.5f;
    private const int PLAIN = 0, PIN = 1, CUT = 2;

    public static void PreparePartsTexture()
    {
        string name = Application.persistentDataPath + "/puzzlesAll.png";
        if (System.IO.File.Exists(name))
        {
            var bytes = System.IO.File.ReadAllBytes(name);
            puzzleparts_tx = new Texture2D(2, 2);
            puzzleparts_tx.LoadImage(bytes);
        }
        else
        {
            var origin = Resources.Load<Texture2D>("Textures/puzzleParts");
            int s = Screen.height / 2, originWidth = origin.width, s1, x = 0, y = 0;
            Color[] cls = origin.GetPixels(), ncls;
            while (originWidth > s)
            {
                s1 = originWidth / 2;
                if (s1 % 2 != 0) s1 -= 1;
                ncls = new Color[s1 * s1];
                for (x = 0; x < s1; x++)
                {
                    for (y = 0; y < s1; y++)
                    {
                        ncls[x * s1 + y] =
                            (cls[(2 * x) * originWidth + 2 * y]
                            + cls[(2 * x + 1) * originWidth + 2 * y]
                            + cls[(2 * x + 1) * originWidth + 2 * y + 1]
                            + cls[(2 * x) * originWidth + 2 * y + 1]) / 4f;
                    }
                }
                cls = ncls;
                originWidth = s1;
            }
            int ind;
            float alpha;
            for (x = 1; x < originWidth - 1; x++)
            {
                for (y = 1; y < originWidth - 1; y++)
                {
                    ind = x * originWidth + y;
                    alpha = cls[(x - 1) * originWidth + y].a + cls[(x + 1) * originWidth + y].a + cls[x * originWidth + y + 1].a + cls[x * originWidth + y - 1].a;
                    alpha /= 4f;
                    if (cls[ind].a < alpha) cls[ind].a = alpha;
                }
            }
            puzzleParts_origin_resized = new Texture2D(originWidth, originWidth, TextureFormat.ARGB32, false);
            puzzleParts_origin_resized.SetPixels(cls);
            puzzleParts_origin_resized.Apply();
            puzzleTexSize = originWidth / 2;
            //
            int fsize = puzzleTexSize * 10;
            puzzleparts_tx = new Texture2D(fsize, fsize);
            cls = new Color[fsize * fsize];
            int pinmask = 0, i, j, index, index2, upPin, downPin, rightPin, leftPin;

            Color[] clr = new Color[puzzleTexSize * puzzleTexSize];
            for (x = 0; x < 9; x++)
            {
                for (y = 0; y < 9; y++)
                {
                    pinmask = DecimalToTernary(x * 9 + y);
                    upPin = pinmask / 1000;
                    rightPin = (pinmask / 100) % 10;
                    downPin = (pinmask / 10) % 10;
                    leftPin = pinmask % 10;
                    clr = INLINE_GetDetail(upPin);
                    var nclr = INLINE_GetDetail(rightPin);
                    float a;
                    for (i = 0; i < puzzleTexSize; i++)
                    {
                        for (j = 0; j < puzzleTexSize; j++)
                        {
                            index = i * puzzleTexSize + j;
                            index2 = j * puzzleTexSize + i;
                            a = clr[index].a;
                            if (a < nclr[index2].a)
                            {
                                clr[index] = nclr[index2];
                            }
                        }
                    }
                    //

                    nclr = INLINE_GetDetail(downPin);
                    for (i = 0; i < puzzleTexSize; i++)
                    {
                        for (j = 0; j < puzzleTexSize; j++)
                        {
                            index = i * puzzleTexSize + j;
                            index2 = (puzzleTexSize - 1 - i) * puzzleTexSize + (puzzleTexSize - 1 - j);
                            a = clr[index].a;
                            if (a < nclr[index2].a)
                            {
                                clr[index] = nclr[index2];
                            }
                        }
                    }
                    //
                    nclr = INLINE_GetDetail(leftPin);
                    for (i = 0; i < puzzleTexSize; i++)
                    {
                        for (j = 0; j < puzzleTexSize; j++)
                        {
                            index = i * puzzleTexSize + j;
                            index2 = (puzzleTexSize - 1 - j) * puzzleTexSize + (puzzleTexSize - 1 - i);
                            a = clr[index].a;
                            if (a < nclr[index2].a)
                            {
                                clr[index] = nclr[index2];
                            }
                        }
                    }

                    for (int b = 0; b < puzzleTexSize; b++)
                    {
                        for (int c = 0; c < puzzleTexSize; c++)
                        {
                            cls[fsize * (x * puzzleTexSize + b) + puzzleTexSize * y + c] = clr[b * puzzleTexSize + c];
                        }
                    }
                }
            }

            //puzzleparts_tx = new Texture2D(puzzleTexSize, puzzleTexSize);
            puzzleparts_tx.SetPixels(cls);
            puzzleparts_tx.Apply();

            var data = puzzleparts_tx.EncodeToPNG();
            using (System.IO.FileStream fs = new System.IO.FileStream(name, System.IO.FileMode.Create))
            {
                fs.Write(data, 0, data.Length);
            }
        }
    }
    private static int DecimalToTernary(int x)
    {
        if (x >= 3)
        {
            var res = new List<int>();
            int n;
            while (x >= 3)
            {
                n = x / 3;
                res.Add(x % 3);
                x = n;
            }
            res.Add(x);
            n = 0;
            for(int i =0; i < res.Count; i++)
            {
                if (res[i] != 0)
                {
                    n += (int)(res[i] * Mathf.Pow(10, i));
                }
            }
            return n;
        }
        else return x;
    }
    public static int TernaryToDecimal (int x)
    {
        if (x < 3) return x;
        else
        {
            int result = 0, i =0;
            while (x >9 )
            {
                result += (int)((x % 10) * Mathf.Pow(3, i));
                x = x / 10;
                i++;
            }
            result += (int)(x * Mathf.Pow(3, i));
            return result;
        }
    }
    private static Color[] INLINE_GetDetail(int code)
    {
        int x0, y0;
        switch (code)
        {
            case PIN:
                x0 = (int)(pinSide.x * puzzleTexSize * 2);
                y0 = (int)(pinSide.y * puzzleTexSize * 2);
                break;
            case CUT:
                x0 = (int)(cutSide.x * puzzleTexSize * 2);
                y0 = (int)(cutSide.y * puzzleTexSize * 2);
                break;
            default:
                x0 = (int)(plainSide.x * puzzleTexSize * 2);
                y0 = (int)(plainSide.y * puzzleTexSize * 2);
                break;
        }
        int szx = x0 + puzzleTexSize, szy = y0 + puzzleTexSize;
        if (szx > puzzleParts_origin_resized.width) szx = puzzleParts_origin_resized.width;
        if (szy > puzzleParts_origin_resized.height) szy = puzzleParts_origin_resized.height;
        return puzzleParts_origin_resized.GetPixels(x0, y0, puzzleTexSize , puzzleTexSize);
    }
    private static Rect GetPuzzlePartRect(int pincode)
    {
        int x = TernaryToDecimal(pincode);
        int y = x % 9;
        x /= 9;
        return new Rect(y * 0.1f, x * 0.1f, 0.1f, 0.1f);
    }

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
            int row, column, pincode;
            RawImage ri;
            PreparePartsTexture();
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
               
                pincode = (row == 7 ? PLAIN : (pps[row * 15 + 7 + column] == true ? PIN : CUT)) * 1000 +
                    (column == 7 ? PLAIN : (pps[row * 15 + column] == true ? PIN : CUT)) * 100 +
                     (row == 0 ? PLAIN : (pps[(row - 1) * 15 + 7 + column] == true ? CUT : PIN)) * 10 +
                     (column == 0 ? PLAIN : (pps[row * 15 + column - 1] == false ? PIN : CUT));
                ri = rt.GetChild(0).GetComponent<RawImage>();
                ri.texture = puzzleparts_tx;
                ri.uvRect = GetPuzzlePartRect(pincode);
                int f_index = i;
                rt.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { this.Click(f_index); } );
                buttons[i] = rt.gameObject;                
            }
            // zero button:
            zeroButton.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { this.Click(0); });
            ri = zeroButton.transform.GetChild(0).GetComponent<RawImage>();
            pincode = (pps[0] == true ? PIN : CUT) * 1000 +
                    (pps[0] == true ? PIN : CUT) * 100 +
                     (PLAIN) * 10 +
                     PLAIN;
            //ri.texture = GetPuzzlePart(pincode);
            ri.uvRect = GetPuzzlePartRect(pincode);
            //
            GameObject blockTechMarker;
            int index;
            for (int i =0; i < Knowledge.ROUTES_COUNT; i++)
            {
                index = Knowledge.routeButtonsIndexes[i, Knowledge.STEPS_COUNT - 3];
                if (!knowledge.IsButtonUnblocked(index))
                {
                    rt = buttons[index].GetComponent<RectTransform>();
                    blockTechMarker = new GameObject();
                    blockTechMarker.transform.parent = rt;
                    rt = blockTechMarker.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.one * 0.4f;
                    rt.anchorMax = Vector2.one * 0.6f;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    ri = blockTechMarker.AddComponent<RawImage>();
                    ri.texture = UIController.current.iconsTexture;
                    ri.uvRect = UIController.GetIconUVRect(Icons.QuestBlockedIcon);
                }
                index = Knowledge.routeButtonsIndexes[i, Knowledge.STEPS_COUNT - 4];
                if (!knowledge.IsButtonUnblocked(index))
                {
                    rt = buttons[index].GetComponent<RectTransform>();
                    blockTechMarker = new GameObject();
                    blockTechMarker.transform.parent = rt;
                    rt = blockTechMarker.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.one * 0.4f;
                    rt.anchorMax = Vector2.one * 0.6f;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    ri = blockTechMarker.AddComponent<RawImage>();
                    ri.texture = UIController.current.iconsTexture;
                    ri.uvRect = UIController.GetIconUVRect(Icons.QuestBlockedIcon);
                    ri.raycastTarget = false;
                }
            }
            //
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

            unblockAnnouncePanel.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.NewBuildingUnblocked);

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
        int i = 0;
        for (; i < 64; i++)
        {
            code = carray[i];
            b = buttons[i];            
            if (code != Knowledge.NOCOLOR_CODE)
            {
                bt = buttons[i].transform;
                bt.GetChild(0).GetComponent<RawImage>().color = colors[code];
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

        var cca = knowledge.colorCodesArray;
        var ia = Knowledge.routeButtonsIndexes;
        for (i = 0; i< Knowledge.ROUTES_COUNT; i++)
        {                    
            if (knowledge.IsRouteUnblocked(i) && endQuestButtons[i] == null)
            {
                var g = new GameObject();
                RectTransform eqButton = g.AddComponent<RectTransform>();
                eqButton.transform.parent = routeBackgrounds[i].transform;
                eqButton.anchorMax = Vector2.zero;
                eqButton.anchorMin = Vector2.zero;
                eqButton.localPosition = Vector3.zero;
                float s = Screen.height / 10f;
                eqButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s);
                eqButton.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s);
                var ri = g.AddComponent<RawImage>();
                ri.texture = UIController.current.iconsTexture;
                ri.uvRect = UIController.GetIconUVRect(Icons.GuidingStar);
                var btn = g.AddComponent<Button>();
                byte index = (byte)i;
                btn.onClick.AddListener(() => GameLogUI.EnableDecisionWindow(
                    Localization.GetPhrase(LocalizedPhrase.Ask_StartFinalQuest),
                    () => QuestUI.current.StartEndQuest(index), Localization.GetWord(LocalizedWord.Yes),
                    GameLogUI.DisableDecisionPanel, Localization.GetWord(LocalizedWord.No)
                    ));
                endQuestButtons[i] = g;
            }
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
    public void UnblockAnnouncement(int b_id)
    {
        unblockAnnouncePanel.GetChild(1).GetComponent<RawImage>().uvRect = Structure.GetTextureRect(b_id);
        unblockAnnouncePanel.GetChild(2).GetComponent<Text>().text = Localization.GetStructureName(b_id);
        unblockAnnouncePanel.GetChild(3).gameObject.SetActive(false);
        unblockAnnouncePanel.GetChild(4).gameObject.SetActive(false);
        unblockAnnouncePanel.SetAsLastSibling();
        unblockAnnouncePanel.gameObject.SetActive(true);
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
