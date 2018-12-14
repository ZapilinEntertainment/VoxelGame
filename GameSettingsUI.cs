using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameSettingsUI : MonoBehaviour
{
    // вешается на объект с панелью

#pragma warning disable 0649
    [SerializeField] Image settingsButton;
#pragma warning restore 0649

    private const int   OPTIONS_LOD_DISTANCE_SLIDER_INDEX = 0, OPTIONS_QUALITY_DROPDOWN_INDEX = 2;

    void OnEnable()
    {
        settingsButton.overrideSprite = PoolMaster.gui_overridingSprite;
        Transform t = transform;
        t.GetChild(OPTIONS_LOD_DISTANCE_SLIDER_INDEX).GetComponent<Slider>().value = LODController.lodCoefficient;
        t.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX).GetComponent<Dropdown>().value = QualitySettings.GetQualityLevel();
        t.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX + 2).gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        settingsButton.overrideSprite = null;
    }

    public void Options_LODdistChanged()
    {
        LODController.SetLODdistance(transform.GetChild(OPTIONS_LOD_DISTANCE_SLIDER_INDEX).GetComponent<Slider>().value);
    }
    public void Options_QualityLevelChanged()
    {
        int v = transform.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX).GetComponent<Dropdown>().value;
        transform.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX + 2).gameObject.SetActive(v != QualitySettings.GetQualityLevel());
    }
    public void Options_ApplyGraphicsChange()
    {
        QualitySettings.SetQualityLevel(transform.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX).GetComponent<Dropdown>().value);
        transform.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX + 2).gameObject.SetActive(false); // apply button
    }

    public void LocalizeTitles()
    {
        Transform t = transform;
        t.GetChild(OPTIONS_LOD_DISTANCE_SLIDER_INDEX + 1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.LODdistance);
        t.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX + 1).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.GraphicQuality);
        t.GetChild(OPTIONS_QUALITY_DROPDOWN_INDEX + 2).gameObject.SetActive(false);
    }
}
