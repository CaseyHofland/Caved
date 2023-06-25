using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MBSO
{
    public static class SaveAndLoadSettings
    {
        //Save settings data
        public static void SaveSettings(SO_Settings set)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Application.persistentDataPath + "/settings.hjg";
            Debug.Log("Saving at: " + path);
            FileStream stream = new FileStream(path,FileMode.Create);
            SettingsFile data = new SettingsFile(set);
            formatter.Serialize(stream, data);
            stream.Close();
        }

        //Load settings data
        public static SettingsFile LoadSettings()
        {
            string path = Application.persistentDataPath + "/settings.hjg";
            if(File.Exists(path))
            {
                Debug.Log("Settings File Found: " + path);
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path,FileMode.Open);
                SettingsFile data = formatter.Deserialize(stream) as SettingsFile;
                stream.Close();
                return data;
            }
            else
            {
                Debug.Log("Settings File Not Found");
                return null;
            }
        }
    }
}

