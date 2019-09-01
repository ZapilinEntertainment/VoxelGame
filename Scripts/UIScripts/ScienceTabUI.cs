using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScienceTabUI : MonoBehaviour
{
    public enum ScienceTabPart : byte { UnexploredPosition, SelectedPosition, Position20, Position40, Position60, Position80, ExploredPosition}
    [SerializeField] private GameObject centerCrystal_basis;
    [SerializeField] private RawImage[] centerCrystal_effects, rays, techButtons;
    [SerializeField] private Text knowledgePtsInfo, ascensionLvlInfo;
    private static ScienceTabUI current;
    private int selectedResearchIndex = -1;
    private float basisEffect_cf1 = 0f, basisEffect_cf2 = 0f, basisEffect_cf3 = 0f;
    private const float BASIS_EFFECT_SPEED = 0.2f;

    public static void OpenResearchTab()
    {
        if (current == null)
        {
            current = Instantiate(Resources.Load<GameObject>("UIPrefs/scienceTab")).GetComponent<ScienceTabUI>();            
        }
        current.gameObject.SetActive(true);
        UIController.current.gameObject.SetActive(false);
        FollowingCamera.main.gameObject.SetActive(false);
        current.FullRedraw();
        current.transform.GetChild(1).gameObject.SetActive(true); // cam
    }
    public static void DestroyInterface()
    {
        if (current != null) Destroy(current);
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
    public void Close() {
        gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false); // cam
        UIController.current.gameObject.SetActive(true);
        FollowingCamera.main.gameObject.SetActive(true);
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

        techButtons[populationSecretIndex].uvRect = GetResearchPointRect(ScienceLab.GetTechIcon(populationSecretIndex));
        techButtons[ascensionSecretIndex].uvRect = GetResearchPointRect(ScienceLab.GetTechIcon(ascensionSecretIndex));
        techButtons[crystalSecretIndex].uvRect = GetResearchPointRect(ScienceLab.GetTechIcon(crystalSecretIndex));
        techButtons[lifepowerSecretIndex].uvRect = GetResearchPointRect(ScienceLab.GetTechIcon(lifepowerSecretIndex));

        if (basicResearchesReady)
        {
            for (int i = 0; i < count; i++)
            {
                rays[i].color = new Color(1f, 1f, 1f, 0.25f + 0.75f * ScienceLab.routePoints[i]);
                rays[i].gameObject.SetActive(true);
            }
            basisEffect_cf1 = 1f;
            basisEffect_cf2 = 1f;
            basisEffect_cf3 = 1f;
        }
        else
        {
            for (int i = 0; i < count; i++) rays[i].gameObject.SetActive(false);
            float sum = ScienceLab.routePoints[(int)ScienceLab.ResearchRoute.Population] + ScienceLab.routePoints[(int)ScienceLab.ResearchRoute.Pipe] 
                + ScienceLab.routePoints[(int)ScienceLab.ResearchRoute.Monument] + ScienceLab.routePoints[(int)ScienceLab.ResearchRoute.Butterfly];
            if (sum > 0.33f)
            {
                basisEffect_cf1 = 1f;
                if (sum > 0.66f)
                {
                    basisEffect_cf2 = 1f;
                    if (sum > 1f) basisEffect_cf3 = 1f;
                }
            }
        }

        ascensionLvlInfo.text = Localization.GetPhrase(LocalizedPhrase.AscensionLevel) + ":\n" + ((int)(GameMaster.realMaster.globalMap.ascension * 100 )).ToString() + '%';
        knowledgePtsInfo.text = Localization.GetPhrase(LocalizedPhrase.KnowledgePoints) + ":\n" + ScienceLab.knowledgePoints.ToString();
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

    private void Update()
    {
        centerCrystal_effects[0].color = new Color(1f, 1f, 1f, Mathf.PingPong(centerCrystal_effects[0].color.a, basisEffect_cf1));
        centerCrystal_effects[1].color = new Color(1f, 1f, 1f, Mathf.PingPong(centerCrystal_effects[1].color.a, basisEffect_cf1));
        centerCrystal_effects[2].color = new Color(1f, 1f, 1f, Mathf.PingPong(centerCrystal_effects[2].color.a, basisEffect_cf1));
    }
}
