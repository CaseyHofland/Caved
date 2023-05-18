#nullable enable
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using Object = UnityEngine.Object;

namespace UnityExtras.Naming.Editor
{
    public static class NamingValidatorSettingsProvider
    {
        private const string _settingsProviderPath = "Project/Naming Validator";

        private static readonly ReorderableList namingValidators = new(NamingValidatorSettings.namingValidators.value ??= new(), typeof(NamingValidator))
        {
            drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Naming Validators"),
            drawElementCallback = DrawNamingValidator,
        };

        private static readonly ReorderableList excludeFromNamingValidators = new(NamingValidatorSettings.excludeFromNamingValidators.value ??= new(), typeof(Object))
        {
            drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Exclude from Naming Validators"),
            drawElementCallback = DrawExcludeFromNamingValidator,
        };

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(_settingsProviderPath, SettingsScope.Project)
            {
                guiHandler = OnGUI,
            };
        }

        private static void OnGUI(string searchContext)
        {
            using var changeCheck = new EditorGUI.ChangeCheckScope();

            if (EditorGUILayout.LinkButton("Try out regex patterns!"))
            {
                Application.OpenURL("https://regexr.com/");
            }
            EditorGUILayout.Space();

            namingValidators.DoLayoutList();
            excludeFromNamingValidators.DoLayoutList();

            if (changeCheck.changed)
            {
                NamingValidatorSettings.namingValidators.SetValue(namingValidators.list);
                NamingValidatorSettings.excludeFromNamingValidators.SetValue(excludeFromNamingValidators.list);
                NamingValidatorSettings.instance.Save();
            }
        }

        private static void DrawNamingValidator(Rect rect, int index, bool isActive, bool isFocused)
        {
            const float buttonWidth = 36f;

            var namingValidator = NamingValidatorSettings.namingValidators.value[index];
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 64f;

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2f;

            var fullWidth = rect.width;
            rect.width = fullWidth * 0.4f;

            var ruleset = EditorGUI.ObjectField(rect, "Ruleset", namingValidator.ruleset, typeof(NamingRuleset), false) as NamingRuleset;
            namingValidator.ruleset = ruleset != null ? ruleset : NamingRuleset.emptyRuleset;

            // Draw the directory field and pass validation checks.
            {
                rect.x += (rect.width = fullWidth * 0.5f);
                rect.width -= (buttonWidth + 2f);

                namingValidator.directory = EditorGUI.DelayedTextField(rect, "Directory", namingValidator.directory) ?? string.Empty;
            }

            // Draw the choose directory button and pass validation checks.
            {
                rect.x += rect.width + 2f;
                rect.width = buttonWidth;
                if (GUI.Button(rect, EditorGUIUtility.IconContent("Folder Icon")))
                {
                    var folder = Application.dataPath + (AssetDatabase.IsValidFolder($"Assets/{namingValidator.directory}") ? $"/{namingValidator.directory}" : default);
                    folder = EditorUtility.OpenFolderPanel($"Choose the Naming Validator's Directory", folder, default);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        if (!folder.StartsWith(Application.dataPath + "/"))
                        {
                            throw new InvalidOperationException($"Trying to select a Naming Validator Directory outside of the current Unity Project. This is not allowed.");
                        }

                        namingValidator.directory = folder.Remove(0, Application.dataPath.Length + 1);
                    }
                }
            }

            // Make sure to use forward slashes for directory representation.
            namingValidator.directory = namingValidator.directory.Replace('\\', '/');

            EditorGUIUtility.labelWidth = labelWidth;
            NamingValidatorSettings.namingValidators.value[index] = namingValidator;
        }

        private static void DrawExcludeFromNamingValidator(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2f;

            var label = NamingValidatorSettings.excludeFromNamingValidators.value[index] ? NamingValidatorSettings.excludeFromNamingValidators.value[index].name : $"Element {index}";
            NamingValidatorSettings.excludeFromNamingValidators.value[index] =  EditorGUI.ObjectField(rect, label, NamingValidatorSettings.excludeFromNamingValidators.value[index], typeof(Object), false);
        }
    }
}
