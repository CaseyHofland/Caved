#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace UnityExtras.Naming.Editor
{
    public static class NamingValidatorSettings
    {
        private const string _packageName = "com.unityextras.namingvalidator";

        private static Settings? _instance;
        internal static Settings instance => _instance ??= new Settings(_packageName);

        public static UserSetting<List<NamingValidator>> namingValidators = new(instance, nameof(namingValidators), new());
        public static UserSetting<List<Object>> excludeFromNamingValidators = new(instance, nameof(excludeFromNamingValidators), new());

        [MenuItem("Assets/Obey Naming Validation")]
        private static void IncludeInNamingValidation()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj.GetType() == typeof(DefaultAsset))
                {
                    var directory = AssetDatabase.GetAssetPath(obj).Remove(0, "Assets/".Length);
                    namingValidators.value.RemoveAll(namingValidator => namingValidator.directory == directory);
                }
                else
                {
                    while (excludeFromNamingValidators.value.Remove(obj)) { }
                }
            }

            excludeFromNamingValidators.settings.Save();
        }

        [MenuItem("Assets/Ignore Naming Validation")]
        private static void ExcludeFromNamingValidation()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj.GetType() == typeof(DefaultAsset)) 
                {
                    var directory = AssetDatabase.GetAssetPath(obj).Remove(0, "Assets/".Length);
                    namingValidators.value.RemoveAll(namingValidator => namingValidator.directory == directory);
                    namingValidators.value.Add(NamingValidator.CreateEmpty(directory));
                }
                else
                {
                    while (excludeFromNamingValidators.value.Remove(obj)) { }
                    excludeFromNamingValidators.value.Add(obj);
                }
            }

            excludeFromNamingValidators.settings.Save();
        }
    }
}
