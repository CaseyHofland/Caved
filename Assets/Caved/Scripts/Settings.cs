using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MBSO;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Settings : MonoBehaviour
{
    public SO_Settings settings;
    public SO_Settings defaultSettings;
    public Volume _profile;

    [Header("GameObjects")]
    public GameObject _resPopUp;

    [Header("Buttons")]
    public Button _btnSave;
    public Button _btnApply;
    public Button _btnRevert;

    [Header("Text")]
    public TMP_Text _txtCountdown;

    [Header("Dropdowns")]
    public TMP_Dropdown _resolutionDimension;
    public TMP_Dropdown _display;

    [Header("Sliders")]
    public SliderSettings _brightnessSlider;

    //private variables
    private Resolution[] _storeResolutions;
    private FullScreenMode _screenMode;
    private int _prevScreenModeIndex;
    private int _prevResolutionIndex;
    private int _maximumPopUpTimer = 15;
    private int _countRes;

    #region Resolution and Display
    //Adds resolutions to dropdown based on the user's computer
    void AddResolutions(Resolution[] res)
    {
        _countRes = 0;

        //Display resolutions at the current screen refresh rate
        for(int i = 0; i < res.Length; i++)
        {
            if (res[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value && res[i].width > 800 && res[i].height > 800)
            {
                _storeResolutions[_countRes] = res[i];
                _countRes++;
            }
        }

        //Add the dropdown resolution as dropdown data
        for(int i = 0 ; i < _countRes ; i++)
        {
            _resolutionDimension.options.Add(new TMP_Dropdown.OptionData(ResolutionToString(_storeResolutions[i])));
        }
    }

    //Determines what screen mode we should use
    void ScreenOptions(string mode)
    {
        if(mode == "Exclusive Fullscreen")
        {
            _display.value = 0;
            _screenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if(mode == "Windowed")
        {
            _display.value = 1;
            _screenMode = FullScreenMode.Windowed;
        }
        else
        {
            _display.value = 2;
            _screenMode = FullScreenMode.FullScreenWindow;
        }

        settings.displayTypeValue = mode;
        _display.RefreshShownValue();
        //Screen.fullScreenMode = _screenMode;
    }
    
    //Converts resolution numbers into a string
    string ResolutionToString(Resolution screenRes)
    {
        return screenRes.width + "x" + screenRes.height;
    }
    #endregion

    void PopUpHandler(int index, TMP_Dropdown dropdown)
    {
        if (!_resPopUp.activeSelf)
        {


            _resPopUp.SetActive(true);
            _btnApply.onClick.RemoveAllListeners();
            _btnRevert.onClick.RemoveAllListeners();

            if (dropdown == _resolutionDimension)
            {
                SetScreen(index);
                _btnApply.onClick.AddListener(delegate
                {
                    _prevResolutionIndex = index;
                    ClosePopUp();
                });
                _btnRevert.onClick.AddListener(delegate
                {
                    SetScreen(_prevResolutionIndex);
                    ClosePopUp();
                });
            }
            else
            {
                SetDisplay(index);
                _btnApply.onClick.AddListener(delegate
                {
                    _prevScreenModeIndex = index;
                    ClosePopUp();
                });
                _btnRevert.onClick.AddListener(delegate
                {
                    SetDisplay(_prevScreenModeIndex);
                    ClosePopUp();
                });
            }

            StartCoroutine("Timer", dropdown);
        }
    }

    IEnumerator Timer(TMP_Dropdown dropdown)
    {
        int currentTimer = _maximumPopUpTimer;
        while(currentTimer >= 0)
        {
            _txtCountdown.text = currentTimer.ToString();
            yield return new WaitForSeconds(1);
            currentTimer--;

            if (currentTimer < 0)
            {
                if(dropdown == _resolutionDimension)
                {
                    SetScreen(_prevResolutionIndex);
                    ClosePopUp();
                }
                else
                {
                    SetDisplay(_prevScreenModeIndex);
                    ClosePopUp();
                }
            }
        }
    }

    void ClosePopUp()
    {
        _resPopUp.SetActive(false);
        StopCoroutine("Timer");
    }

    void SetScreen(int index)
    {
        Screen.SetResolution(_storeResolutions[index].width, _storeResolutions[index].height, _screenMode);
        settings.widthValue = _storeResolutions[index].width;
        settings.heightValue = _storeResolutions[index].height;
        _resolutionDimension.value = index;
        _resolutionDimension.RefreshShownValue();
        SaveAndLoadSettings.SaveSettings(settings);
    }

    void SetDisplay(int index)
    {
        ScreenOptions(_display.options[index].text);
        Screen.fullScreenMode = _screenMode;
        StartCoroutine("SetDisplayAtEnd");
        SaveAndLoadSettings.SaveSettings(settings);
    }

    //Sets the rsolution at the end of changing display type
    IEnumerator SetDisplayAtEnd()
    {
        yield return new WaitForFixedUpdate();
        Screen.SetResolution(settings.widthValue, settings.heightValue, _screenMode);
    }
    void LoadSettings()
    {
        for(int i=0 ; i < _storeResolutions.Length ; i++)
        {
            if (_storeResolutions[i].width == settings.widthValue && _storeResolutions[i].height == settings.heightValue)
                _resolutionDimension.value = i;
        }
        _resolutionDimension.RefreshShownValue();
        ScreenOptions(settings.displayTypeValue);
        Screen.SetResolution(settings.widthValue, settings.heightValue, _screenMode);

        _brightnessSlider._slider.value = settings.brightnessValue;
        Brightness(settings.brightnessValue);
    }

    void Brightness(float currentValue)
    {
        float _finalValue;
        _finalValue = ConvertValue(_brightnessSlider._slider.minValue, _brightnessSlider._slider.maxValue, _brightnessSlider._minSettingsValue, _brightnessSlider._maxSettingsValue, currentValue);
        _profile.GetComponent<ColorAdjustments>().postExposure.Override(_finalValue);
        settings.brightnessValue = currentValue;
        _brightnessSlider._txtSlider.text = Mathf.RoundToInt(currentValue).ToString();
    }

    private void Start()
    {
        Resolution[] resolutions = Screen.resolutions;
        Array.Reverse(resolutions);
        _storeResolutions = new Resolution[resolutions.Length];

        //ScreenInitialize();
        AddResolutions(resolutions);
        //ResolutionInitialize(_storeResolutions);

        LoadSettingsFile.settings = settings;
        LoadSettingsFile.defaultSettings = defaultSettings;
        LoadSettingsFile.InitiateSettings();
        LoadSettings();

        _resPopUp.SetActive(false);
        _prevResolutionIndex = _resolutionDimension.value;
        _prevScreenModeIndex = _display.value;

        //Resolutions and Display settings
        _display.onValueChanged.AddListener(delegate { PopUpHandler(_display.value, _display); });
        _resolutionDimension.onValueChanged.AddListener(delegate { PopUpHandler(_resolutionDimension.value, _resolutionDimension); });
        _brightnessSlider._slider.onValueChanged.AddListener(delegate { Brightness(_brightnessSlider._slider.value); });
        _btnSave.onClick.AddListener(delegate { SaveAndLoadSettings.SaveSettings(settings); });

    }

    #region Helper Functions
    float ConvertValue(float virtualMin, float virtualMax, float actualMin, float actualMax, float currentValue)
    {
        float ratio = (actualMax - actualMin) / (virtualMax-virtualMin);
        float returnValue = ((currentValue * ratio) - (virtualMin * ratio)) + actualMin;
        return returnValue;
    }
    #endregion
}
