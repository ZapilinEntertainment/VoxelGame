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
    private int selectedResearchIndex = -1;
    private float basisEffect_cf1 = 0f, basisEffect_cf2 = 0f, basisEffect_cf3 = 0f;
    private ResearchStar researchStar;
    private const float BASIS_EFFECT_SPEED = 0.2f;

    public static ScienceTabUI CreateVisualizer(ResearchStar i_rs)
    {
        var rs = GameObject.Instantiate(Resources.Load<GameObject>("UIPrefs/scienceTab")).GetComponent<ScienceTabUI>();
        rs.researchStar = i_rs;
        return rs;
    }
    public void SelectTech(int index)
    {
        if (selectedResearchIndex != -1 & selectedResearchIndex != index)
        {
            techButtons[selectedResearchIndex].uvRect = GetResearchPointRect(researchStar.GetTechIcon(index));
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
    public void FullRedraw()
    {
        var rst = researchStar.researchStatus;
        int populationSecretIndex = (int)ResearchStar.Research.PopulationSecret,
            ascensionSecretIndex = (int)ResearchStar.Research.AscensionSecret,
            crystalSecretIndex = (int)ResearchStar.Research.CrystalSecret,
            lifepowerSecretIndex = (int)ResearchStar.Research.LifepowerSecret;
        bool basicResearchesReady =
             (rst[populationSecretIndex] == true) & (rst[ascensionSecretIndex] == true) &
             (rst[crystalSecretIndex] == true) & (rst[lifepowerSecretIndex] == true);

        int count = (int)ResearchStar.ResearchRoute.Total;
        Rect completedIconRect = GetResearchPointRect(ScienceTabPart.ExploredPosition),
            unexploredIconRect = GetResearchPointRect(ScienceTabPart.UnexploredPosition);

        techButtons[populationSecretIndex].uvRect = GetResearchPointRect(researchStar.GetTechIcon(populationSecretIndex));
        techButtons[ascensionSecretIndex].uvRect = GetResearchPointRect(researchStar.GetTechIcon(ascensionSecretIndex));
        techButtons[crystalSecretIndex].uvRect = GetResearchPointRect(researchStar.GetTechIcon(crystalSecretIndex));
        techButtons[lifepowerSecretIndex].uvRect = GetResearchPointRect(researchStar.GetTechIcon(lifepowerSecretIndex));

        if (basicResearchesReady)
        {
            for (int i = 0; i < count; i++)
            {
                rays[i].color = new Color(1f, 1f, 1f, 0.25f + 0.75f * researchStar.routePoints[i]);
                rays[i].gameObject.SetActive(true);
            }
            basisEffect_cf1 = 1f;
            basisEffect_cf2 = 1f;
            basisEffect_cf3 = 1f;
        }
        else
        {
            for (int i = 0; i < count; i++) rays[i].gameObject.SetActive(false);
            var rps = researchStar.routePoints;
            float sum = rps[(int)ResearchStar.ResearchRoute.Population] + rps[(int)ResearchStar.ResearchRoute.Pipe] 
                + rps[(int)ResearchStar.ResearchRoute.Monument] + rps[(int)ResearchStar.ResearchRoute.Butterfly];
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
        knowledgePtsInfo.text = Localization.GetPhrase(LocalizedPhrase.KnowledgePoints) + ":\n" + researchStar.knowledgePoints.ToString();
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
