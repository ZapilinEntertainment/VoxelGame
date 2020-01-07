using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Knowledge
{
    public enum ResearchRoute : byte { Foundation = 0, CloudWhale, Engine, Pipes, Crystal, Monument, Blossom, Pollen }
    //dependence: 
    //uiController icons enum
    // labels in Localization - get challenge label

    private static Knowledge current;

    public readonly bool[] puzzlePins;
    public bool allRoutesUnblocked { get; private set; }
    public float[] routePoints { get; private set; }
    public byte[] puzzlePartsCount { get; private set; }
    public byte[] colorCodesArray{get; private set;}
    public float completeness { get; private set; }
    public int changesMarker { get; private set; }
    private KnowledgeTabUI observer;

    public const byte ROUTES_COUNT = 8, STEPS_COUNT = 7,
       WHITECOLOR_CODE = 0, REDCOLOR_CODE = 1, GREENCOLOR_CODE = 2, BLUECOLOR_CODE = 3, CYANCOLOR_CODE = 4, BLACKCOLOR_CODE = 5, NOCOLOR_CODE = 6;
    public static readonly Color[] colors = new Color[6]
    {
        Color.white, // 0 - white
        new Color(0.8f, 0.3f, 0.3f, 1f), // 1 - red
        new Color(0.43f, 0.8f, 0.3f), // 2 - green
        new Color(0.25f, 0.52f, 0.86f, 1f), // 3 - blue
        new Color(0.1f, 0.92f, 0.9f, 1f), // 4 - cyan
        new Color(0.2f, 0.17f, 0.17f, 1f), // 5 - black      
    };
    public static readonly float[] STEPVALUES = new float[7] { 1f, 10f, 25f, 50f, 75f, 100f, 128f };
    public readonly byte[,] routeButtonsIndexes = new byte[8, 7] {
        {36, 44, 43,51,52,60,59},
        {45, 53,46,54,62,55,63},
        {28,29,37,38,30,31,39 },
        {21,22, 13,14,15,6,7},
        {27,19,20,11,12,3,4 },
        {18, 10,17,9,1,8,0},
        {35,34,26,25,33,32,24 },
        {42,50,41,49,57,48,56 }
    };
    private readonly byte[] blockedCells = new byte[8]{ 2, 5, 16, 23, 40, 47, 58, 61 };

    public static Knowledge GetCurrent()
    {
        if (current == null) current = new Knowledge();
        return current;
    }

    private Knowledge()
    {
        puzzlePins = new bool[112]; // 7 * 8 + 8 * 7
        for (int i =0; i < puzzlePins.Length; i++)
        {
            puzzlePins[i] = Random.value > 0.55f ? true : false;
        }
        routePoints = new float[ROUTES_COUNT];
        completeness = 0f;
        puzzlePartsCount = new byte[6];
        colorCodesArray = new byte[64]; //filed with 0 - whitecolor
        foreach (byte b in blockedCells)
        {
            colorCodesArray[b] = BLACKCOLOR_CODE;
        }     
    }

    public byte GenerateCellColor(byte route, byte step)
    {
        float[] varieties; // r g b c
        switch ((ResearchRoute)route)
        {
            case ResearchRoute.Foundation:
                {
                    if (step == 1) varieties = new float[] { 3, 3, 3, 1 };
                    else
                    {
                        if (step == 6) varieties = new float[] { 0, 1, 3, 3 };
                        else varieties = new float[] { 1, 0.9f, 1, 0.1f };
                    }
                    break;
                }
            case ResearchRoute.CloudWhale:
                {
                    if (step < 4) varieties = new float[] { 1, 3, 6, 2 };
                    else varieties = new float[] { 0, 2, 6, 4 };
                    break;
                }
            case ResearchRoute.Engine:
                {
                    if (step < 3) varieties = new float[] { 0, 0, 1, 0 };
                    else varieties = new float[] { 1, 2, 12, 8 };
                    break;
                }
            case ResearchRoute.Pipes:
                {
                    if (step == 1) varieties = new float[] { 0, 0, 0, 1 };
                    else
                    {
                        if (step < 4) varieties = new float[] { 1, 1, 10, 3 };
                        else varieties = new float[] { 0, 0, 1, 1 };
                    }
                    break;
                }
            case ResearchRoute.Crystal:
                {
                    if (step == 1) varieties = new float[] { 0, 0, 0, 1 };
                    else
                    {
                        varieties = new float[] { 0.2f, 0.4f, 5, 5 }; ;
                    }
                    break;
                }
            case ResearchRoute.Monument:
                {
                    if (step < 5) varieties = new float[] { 1, 2, 5, 4 };
                    else varieties = new float[] { 0, 1, 5, 5 };
                    break;
                }
            case ResearchRoute.Blossom:
                {
                    varieties = new float[] { 1, 20, 5, 10 };
                    break;
                }
            case ResearchRoute.Pollen:
                {
                    if (step == 1) varieties = new float[] { 0, 10, 0, 1 };
                    else
                    {
                        varieties = new float[] { 1, 20, 10, 10 };
                    }
                    break;
                }
            default: varieties = new float[] { 1, 1, 1, 1 }; break;
        }

        float s = varieties[0] + varieties[1] + varieties[2] + varieties[3];
        float v = Random.value;
        byte col;
        if (v < (varieties[0] + varieties[1] + varieties[2]) / s)
        {
            if (v < varieties[0]) col = REDCOLOR_CODE; else col = GREENCOLOR_CODE;
        }
        else
        {
            if (v > varieties[3]) col = CYANCOLOR_CODE; else col = BLUECOLOR_CODE;
        }
        return col;
    }

    public void AddPuzzlePart(byte colorcode)
    {
        if (colorcode < puzzlePartsCount.Length && puzzlePartsCount[colorcode] < 255) puzzlePartsCount[colorcode]++;  
    }
    public void AddResearchPoints (ResearchRoute route, float pts)
    {
        byte routeIndex = (byte)route;
        float f = routePoints[routeIndex] + pts;
        float maxvalue = STEPVALUES[STEPS_COUNT - 1];
        if (f >= maxvalue)
        {
            routePoints[routeIndex] = maxvalue;
            for (byte step = 0; step < STEPS_COUNT; step++)
            {
                if (colorCodesArray[routeButtonsIndexes[routeIndex, step]] == WHITECOLOR_CODE)
                {
                    colorCodesArray[routeButtonsIndexes[routeIndex, step]] = GenerateCellColor(routeIndex, step);
                    changesMarker++;
                }                    
            }
        }
        else
        {
            byte step = 0;
            while (step < STEPS_COUNT && f >= STEPVALUES[step])
            {
                if ( colorCodesArray[routeButtonsIndexes[routeIndex, step]] == WHITECOLOR_CODE)
                {
                    colorCodesArray[routeButtonsIndexes[routeIndex, step]] = GenerateCellColor(routeIndex, step);
                    changesMarker++;
                }
                step++;
            }
            routePoints[routeIndex] = f;
        }
    }
    public bool UnblockButton(int i)
    {
        var colorcode = colorCodesArray[i];
        if (colorcode == NOCOLOR_CODE) return true;
        if (puzzlePartsCount[colorcode] > 0)
        {
            puzzlePartsCount[colorcode]--;
            colorCodesArray[i] = NOCOLOR_CODE;
            changesMarker++;
            return true;
        }
        else return false;
    }

    public void OpenResearchTab()
    {
        GameMaster.realMaster.environmentMaster.DisableDecorations();
        if (observer == null)
        {
            observer = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefs/knowledgeTab")).GetComponent<KnowledgeTabUI>();
            observer.Prepare(this);            
        }
        if (!observer.gameObject.activeSelf) observer.gameObject.SetActive(true);
        observer.Redraw();
    }

    public (byte,byte) CellIndexToRouteAndStep(int buttonIndex)
    {
        for (byte ri = 0; ri < ROUTES_COUNT; ri++)
        {
            for (byte si = 0; si < STEPS_COUNT;si++)
            {
                if (routeButtonsIndexes[ri, si] == buttonIndex) return (ri,si);
            }
        }
        return (255,255);
    }
}
