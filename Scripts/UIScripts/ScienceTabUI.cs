using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScienceTabUI : MonoBehaviour
{
    public enum ScienceTabPart : byte { UnexploredPosition, SelectedPosition, Position20, Position40, Position60, Position80, ExploredPosition}
    [SerializeField] private GameObject centerCrystal_basis;
    [SerializeField] private RawImage[] centerCrystal_effects, rays, techButtons;
    private static ScienceTabUI current;
    private int selectedResearchIndex = -1;

    public static void Initialize()
    {
        //current
    }

    public void SelectTech(int index)
    {
        if (selectedResearchIndex != -1 & selectedResearchIndex != index)
        {
            techButtons[selectedResearchIndex].uvRect = GetResearchPointRect(ScienceLab.GetTechIcon(index));
        }
        selectedResearchIndex = index;
        techButtons[selectedResearchIndex].uvRect = GetResearchPointRect(ScienceTabPart.SelectedPosition);
        //draw info
    }

    private void FullRedraw()
    {
        var rst = ScienceLab.researchStatus;
        int populationSecretIndex = (int)ScienceLab.Research.PopulationSecret,
            ascensionSecretIndex = (int)ScienceLab.Research.AscensionSecret,
            crystalSecretIndex = (int)ScienceLab.Research.CrystalSecret,
            lifepowerSecretIndex = (int)ScienceLab.Research.LifepowerSecret;
        bool basicResearchesReady =
             (rst[populationSecretIndex] == true) & (rst[ascensionSecretIndex] == true) &
             (rst[crystalSecretIndex] == true) & (rst[lifepowerSecretIndex] == true);

        int count = (int)ScienceLab.ResearchRoute.Total;
        Rect completedIconRect = GetResearchPointRect(ScienceTabPart.ExploredPosition),
            unexploredIconRect = GetResearchPointRect(ScienceTabPart.UnexploredPosition);
        if (basicResearchesReady)
        {
            for (int i = 0; i < count; i++)
            {
                rays[i].color = new Color(1f, 1f, 1f, 0.25f + 0.75f * ScienceLab.routePoints[i]);
                rays[i].gameObject.SetActive(true);
            }
            techButtons[populationSecretIndex].uvRect = completedIconRect;
            techButtons[ascensionSecretIndex].uvRect = completedIconRect;
            techButtons[crystalSecretIndex].uvRect = completedIconRect;
            techButtons[lifepowerSecretIndex].uvRect = completedIconRect;

            centerCrystal_basis.SetActive(true);
        }
        else
        {
            for (int i = 0; i < count; i++) rays[i].gameObject.SetActive(false);
            techButtons[populationSecretIndex].uvRect = rst[populationSecretIndex] == true ? completedIconRect : unexploredIconRect;
            techButtons[ascensionSecretIndex].uvRect = rst[populationSecretIndex] == true ? completedIconRect : unexploredIconRect;
            techButtons[crystalSecretIndex].uvRect = rst[populationSecretIndex] == true ? completedIconRect : unexploredIconRect;
            techButtons[lifepowerSecretIndex].uvRect = rst[populationSecretIndex] == true ? completedIconRect : unexploredIconRect;

            centerCrystal_basis.SetActive(false);
        }
        /*
        count = (int)ScienceLab.Research.Last;
        for (int i = 1; i < count; i++)
        {
            
        }
        */
    }

    private Rect GetResearchPointRect(ScienceTabPart stp)
    {
        float p = 0.125f;
        switch (stp)
        {
            case ScienceTabPart.SelectedPosition: return new Rect(7 * p, 0f, p, p);
            case ScienceTabPart.ExploredPosition: return new Rect(5 * p, 0f, p, p);
            case ScienceTabPart.Position80: return new Rect(4 * p, 0f, p, p);
            case ScienceTabPart.Position60: return new Rect(3 * p, 0f, p, p);
            case ScienceTabPart.Position40: return new Rect(2 * p, 0f, p, p);
            case ScienceTabPart.Position20: return new Rect(p, 0f, p, p);
            default: return new Rect(0f, 0f, p,p);
        }
    }
}
