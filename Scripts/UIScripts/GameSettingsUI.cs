using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameSettingsUI : MonoBehaviour
{
    // вешается на объект с панелью

#pragma warning disable 0649
    [SerializeField] Dropdown qualityDropdown;
    [SerializeField] GameObject graphicsApplyButton;
    [SerializeField] Image settingsButton;
    [SerializeField] Slider lodDistanceSlider;
    [SerializeField] Toggle touchscreenToggle;
#pragma warning restore 0649
    private bool ignoreTouchscreenToggle = false, ignoreDistanceSliderChanging = false;

    void OnEnable()
    {
        settingsButton.overrideSprite = PoolMaster.gui_overridingSprite;
        Transform t = transform;
        ignoreDistanceSliderChanging = true;
        lodDistanceSlider.minValue = 0;
        lodDistanceSlider.maxValue = 1;
        lodDistanceSlider.value = LODController.lodCoefficient;
        ignoreDistanceSliderChanging = false;
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        graphicsApplyButton.gameObject.SetActive(false);

        ignoreTouchscreenToggle = true;
        touchscreenToggle.isOn = FollowingCamera.touchscreen;
        ignoreTouchscreenToggle = false;
    }

    private void OnDisable()
    {
        settingsButton.overrideSprite = null;
    }

    public void Options_LODdistChanged()
    {
        if (!ignoreDistanceSliderChanging) LODController.SetLODdistance(lodDistanceSlider.value);
    }
    public void Options_QualityLevelChanged()
    {
        int v = qualityDropdown.value;
        graphicsApplyButton.gameObject.SetActive(v != QualitySettings.GetQualityLevel());
    }
    public void Options_ApplyGraphicsChange()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        graphicsApplyButton.SetActive(false); // apply button
    }
    public void TouchSwitch()
    {
        if ( !ignoreTouchscreenToggle) FollowingCamera.SetTouchControl(touchscreenToggle.isOn);
    }

    public void LocalizeTitles()
    {
        Transform t = transform;
        lodDistanceSlider.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.LODdistance);
        qualityDropdown.transform.GetChild(0).GetComponent<Text>().text = Localization.GetPhrase(LocalizedPhrase.GraphicQuality);
        graphicsApplyButton.transform.GetChild(0).gameObject.SetActive(false);
    }
}
