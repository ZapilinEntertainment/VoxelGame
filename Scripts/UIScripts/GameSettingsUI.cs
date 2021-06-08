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
    [SerializeField] Slider lodDistanceSlider, cameraMoveSpeedSlider, cameraRotateSpeedSlider;
    [SerializeField] Toggle touchscreenToggle;
    [SerializeField] private Text qualityDropdownLabel, lodDistanceLabel, cameraMvLabel, cameraRtLabel;
#pragma warning restore 0649
    private bool ignoreCalls = false, localized = false, needToSaveSettings = false;

    void Start()
    {
        if (!localized) LocalizeTitles();
    }

    void OnEnable()
    {
        Transform t = transform;
        ignoreCalls = true;
        lodDistanceSlider.minValue = GameSettings.LOD_MIN_VAL;
        lodDistanceSlider.maxValue = GameSettings.LOD_MAX_VAL;
        var gset = GameSettings.GetSettings();
        lodDistanceSlider.value = gset.lodCoefficient;
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        graphicsApplyButton.gameObject.SetActive(false);

        touchscreenToggle.isOn = FollowingCamera.touchscreen;

        cameraMoveSpeedSlider.minValue = GameSettings.CAM_MV_MIN;
        cameraMoveSpeedSlider.maxValue = GameSettings.CAM_MV_MAX;
        cameraMoveSpeedSlider.value = gset.cameraMoveCf;
        cameraRotateSpeedSlider.minValue = GameSettings.CAM_RT_MIN;
        cameraRotateSpeedSlider.maxValue = GameSettings.CAM_RT_MAX;
        cameraRotateSpeedSlider.value = gset.cameraRotationCf;

        ignoreCalls = false;        
    }
    void OnDisable()
    {
        if (needToSaveSettings)
        {
            GameSettings.GetSettings().SaveSettings();
            needToSaveSettings = false;
        }
    }


    public void Options_LODdistChanged()
    {
        if (!ignoreCalls)
        {
            GameSettings.GetSettings().SetLodDistance(lodDistanceSlider.value);
            needToSaveSettings = true;
        }
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
        if ( !ignoreCalls) FollowingCamera.SetTouchControl(touchscreenToggle.isOn);
    }
    public void Options_CamMoveSpeedChanged(float x)
    {
        if (!ignoreCalls)
        {
            GameSettings.GetSettings().SetCameraMoveCf(x);
            needToSaveSettings = true;
        }
    }
    public void Options_CamRotSpeedChanged(float x)
    {
        if (!ignoreCalls)
        {
            GameSettings.GetSettings().SetCameraRotationCf(x);
            needToSaveSettings = true;
        }
    }

    public void LocalizeTitles()
    {
        Transform t = transform;
        lodDistanceLabel.text = Localization.GetPhrase(LocalizedPhrase.LODdistance);
        qualityDropdownLabel.text = Localization.GetPhrase(LocalizedPhrase.GraphicQuality);
        graphicsApplyButton.SetActive(false);
        graphicsApplyButton.transform.GetChild(0).GetComponent<Text>().text = Localization.GetWord(LocalizedWord.Apply);
        cameraMvLabel.text = Localization.GetPhrase(LocalizedPhrase.CameraMoveSpeed);
        cameraRtLabel.text = Localization.GetPhrase(LocalizedPhrase.CameraRotationSpeed);
        Localization.AddToLocalizeList(this);
        localized = true;
    }
    private void OnDestroy()
    {
        Localization.RemoveFromLocalizeList(this);
    }
}
