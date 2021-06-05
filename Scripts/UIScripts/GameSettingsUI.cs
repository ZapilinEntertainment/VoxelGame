using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameSettingsUI : MonoBehaviour, ILocalizable
{
    // вешается на объект с панелью

#pragma warning disable 0649
    [SerializeField] Dropdown qualityDropdown;
    [SerializeField] GameObject graphicsApplyButton;
    [SerializeField] Slider lodDistanceSlider;
    [SerializeField] Toggle touchscreenToggle;
    [SerializeField] private Text qualityDropdownLabel, lodDistanceLabel;
#pragma warning restore 0649
    private bool ignoreTouchscreenToggle = false, ignoreDistanceSliderChanging = false, localized = false;

    void Start()
    {
        if (!localized) LocalizeTitles();
    }

    void OnEnable()
    {
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


    public void Options_LODdistChanged()
    {
        if (!ignoreDistanceSliderChanging) LODController.SetLODdistance(lodDistanceSlider.value);
    }
    public void Options_QualityLevelChanged()
    {
        int v = qualityDropdown.value;
        graphicsApplyButton.SetActive(v != QualitySettings.GetQualityLevel());
    }
    public void Options_ApplyGraphicsChange()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        //FollowingCamera.cam.GetComponent<AmplifyOcclusionEffect>().enabled = qualityDropdown.value == 2;
        PoolMaster.ChangeQualityLevel(qualityDropdown.value);
        graphicsApplyButton.SetActive(false); // apply button
    }
    public void TouchSwitch()
    {
        if ( !ignoreTouchscreenToggle) FollowingCamera.SetTouchControl(touchscreenToggle.isOn);
    }

    public void LocalizeTitles()
    {
        Transform t = transform;
        lodDistanceLabel.text = Localization.GetPhrase(LocalizedPhrase.LODdistance);
        qualityDropdownLabel.text = Localization.GetPhrase(LocalizedPhrase.GraphicQuality);
        graphicsApplyButton.SetActive(false);
        graphicsApplyButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Apply);
        Localization.AddToLocalizeList(this);
        localized = true;
    }
    private void OnDestroy()
    {
        Localization.RemoveFromLocalizeList(this);
    }
}
