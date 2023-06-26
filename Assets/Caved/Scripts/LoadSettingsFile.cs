using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBSO
{
    public static class LoadSettingsFile
    {
        public static SO_Settings settings;
        public static SO_Settings defaultSettings;

        public static void InitiateSettings()
        {
            SettingsFile data = SaveAndLoadSettings.LoadSettings();

            //File Exists
            if(data != null )
            {
                //Video settings
                settings.widthValue = data.screenWidth;
                settings.heightValue = data.screenHeight;
                settings.displayTypeValue = data.displayType;
                settings.brightnessValue = data.brightness;
            }
            //If the file does not exist. Find the default settings and assign them
            else
            {
                //Default video settings
                settings.widthValue = Screen.width;
                settings.heightValue = Screen.height;
                settings.displayTypeValue = Screen.fullScreenMode.ToString();
                settings.brightnessValue = defaultSettings.brightnessValue;
                SaveAndLoadSettings.SaveSettings(settings);
            }
        }

    }
}

