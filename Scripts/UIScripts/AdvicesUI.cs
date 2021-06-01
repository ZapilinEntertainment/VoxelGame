using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class AdvicesUI : MonoBehaviour
{
    [SerializeField] private Text label, description, pagesNumber;
    private int currentAdviceIndex = 0, totalAdvicesCount = 0;
    private string[] lines;
    private bool textLoaded = false;

    private void LoadAdvices()
    {
        Localization.LoadLocalesData("advice", ref lines, false);
        totalAdvicesCount = lines.Length / 2;
        textLoaded = true;
    }

    public void PrepareAdvice()
    {
        if (!textLoaded) LoadAdvices();
        PrepareAdvice(Random.Range(0, totalAdvicesCount));
    }
    public void PrepareAdvice(int i)
    {
        if (!textLoaded) LoadAdvices();
        int i2 = i * 2;
        label.text = lines[i2];
        description.text = lines[i2 + 1];
        currentAdviceIndex = i;
        pagesNumber.text = currentAdviceIndex.ToString() + " / " + totalAdvicesCount.ToString();
    }

    public void NextAdvice()
    {
        currentAdviceIndex++;
        if (currentAdviceIndex >= totalAdvicesCount) currentAdviceIndex = 0;
        PrepareAdvice(currentAdviceIndex);
    }
    public void PreviousAdvice()
    {
        currentAdviceIndex--;
        if (currentAdviceIndex <0) currentAdviceIndex = totalAdvicesCount - 1;
        PrepareAdvice(currentAdviceIndex);
    }
}
