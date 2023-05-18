#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

public class PrototypeReferenceChecker : AssetModificationProcessor
{
    private const string menuName = "Tools/Check Prototype References/";
    private const string prototypeDirectory = "Prototype";
    private const string autoCheckPrototypeReferencesKey = nameof(autoCheckPrototypeReferencesKey);
    private const string autoCheckMenuName = menuName + "Auto Check";

    private static string[] OnWillSaveAssets(string[] paths)
    {
        var autoCheck = EditorPrefs.GetBool(autoCheckPrototypeReferencesKey, false);

        if (autoCheck)
        {
            CheckAssets();
            CheckOpenScenes();
        }

        return paths;
    }

    [InitializeOnLoadMethod]
    private static void SetAutoCheckStatus()
    {
        EditorApplication.delayCall += () =>
        {
            var autoCheck = EditorPrefs.GetBool(autoCheckPrototypeReferencesKey, false);
            Menu.SetChecked(autoCheckMenuName, autoCheck);
        };
    }

    [MenuItem(autoCheckMenuName, priority = 0)]
    private static void ToggleAutoCheckStatus()
    {
        var autoCheck = EditorPrefs.GetBool(autoCheckPrototypeReferencesKey, false);
        EditorPrefs.SetBool(autoCheckPrototypeReferencesKey, !autoCheck);

        Menu.SetChecked(autoCheckMenuName, !autoCheck);
    }

    #region Checks Prototype References
    [MenuItem(menuName + "Check Assets", priority = 1)]
    public static void CheckAssets()
    {
        // Get Prototype Objects
        var prototypeObjects = new List<Object>();
        GetPrototypeObjects(prototypeObjects);
        if (prototypeObjects.Count == 0)
        {
            return;
        }

        // Check Prefabs
        var prefabs = new List<Object>();
        GetPrefabs(prefabs);
        foreach (var prefab in prefabs)
        {
            CheckObject(prefab, prototypeObjects);
        }

        // Check Assets
        var assets = new List<Object>();
        GetAssets(assets);
        foreach (var asset in assets)
        {
            CheckObject(asset, prototypeObjects);
        }
    }

    [MenuItem(menuName + "Check Open Scenes", priority = 2)]
    public static void CheckOpenScenes()
    {
        // Get Prototype Objects
        var prototypeObjects = new List<Object>();
        GetPrototypeObjects(prototypeObjects);
        if (prototypeObjects.Count == 0)
        {
            return;
        }

        // Check Open Scenes
        var sceneObjects = new List<Object>();
        GetSceneObjects(sceneObjects);
        foreach (var sceneObject in sceneObjects)
        {
            if (sceneObject is GameObject gameObject)
            {
                var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
                var prefab = AssetDatabase.LoadMainAssetAtPath(prefabPath);
                if (prototypeObjects.Contains(prefab))
                {
                    Debug.LogError($"Prototype referenced in Scene {gameObject.scene.name}: {gameObject.name} to {AssetDatabase.GetAssetPath(prefab)}", gameObject);
                }
            }
            else
            {
                CheckObject(sceneObject, prototypeObjects);
            }
        }
    }

    private static void CheckObject(Object obj, IEnumerable<Object> prototypeObjects)
    {
        // Objects may be null in the case of e.g. missing scripts.
        if (obj == null)
        {
            return;
        }

        var serializedObject = new SerializedObject(obj);
        var property = serializedObject.GetIterator();

        while (property.NextVisible(true))
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                continue;
            }

            if (prototypeObjects.Contains(property.objectReferenceValue))
            {
                Debug.LogError($"Prototype referenced on {obj}: {property.displayName} to {AssetDatabase.GetAssetPath(property.objectReferenceValue)}", obj);
            }
        }
    }
    #endregion

    #region Get Objects
    private static void GetPrototypeObjects(List<Object> prototypeObjects)
    {
        prototypeObjects.Clear();

        var directories = Directory.GetDirectories(Application.dataPath, prototypeDirectory, SearchOption.AllDirectories);

        foreach (var directory in directories)
        {
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                // Meta files don't need to be loaded, we only want the actual Objects.
                if (file.EndsWith(".meta"))
                {
                    continue;
                }

                var relativeFile = file.Replace(Application.dataPath, "Assets");

                // Load certain asset types as main assets.
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(relativeFile);
                if (assetType == typeof(SceneAsset)
                    || assetType == typeof(GameObject))
                {
                    prototypeObjects.Add(AssetDatabase.LoadMainAssetAtPath(relativeFile));
                }
                else
                {
                    prototypeObjects.AddRange(AssetDatabase.LoadAllAssetsAtPath(relativeFile));
                }
            }
        }
    }

    private static void GetPrefabs(List<Object> prefabs)
    {
        prefabs.Clear();

        var prefabPaths = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        foreach (var prefabPath in prefabPaths)
        {
            var relativePrefabPath = prefabPath.Replace(Application.dataPath, "Assets");
            if (relativePrefabPath.Contains($"\\{prototypeDirectory}\\"))
            {
                continue;
            }

            prefabs.AddRange(AssetDatabase.LoadAllAssetsAtPath(relativePrefabPath));
        }
    }

    private static void GetAssets(List<Object> assets)
    {
        assets.Clear();

        var assetPaths = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
        foreach (var assetPath in assetPaths)
        {
            var relativeAssetPath = assetPath.Replace(Application.dataPath, "Assets");
            if (relativeAssetPath.Contains($"\\{prototypeDirectory}\\"))
            {
                continue;
            }

            assets.AddRange(AssetDatabase.LoadAllAssetsAtPath(relativeAssetPath));
        }
    }

    private static void GetSceneObjects(List<Object> sceneObjects)
    {
        sceneObjects.Clear();

        var scenes = new Scene[SceneManager.sceneCount];
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            scenes[i] = SceneManager.GetSceneAt(i);
        }

        foreach (var scene in scenes)
        {
            if (scene.path.Contains($"/{prototypeDirectory}/"))
            {
                continue;
            }

            foreach (var gameObject in scene.GetRootGameObjects())
            {
                AddSceneObjectsRecursive(gameObject);

                void AddSceneObjectsRecursive(GameObject gameObject)
                {
                    if (PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.NotAPrefab)
                    {
                        sceneObjects.AddRange(gameObject.GetComponents<Component>());
                    }
                    else
                    {
                        sceneObjects.Add(gameObject);
                    }

                    foreach (Transform child in gameObject.transform)
                    {
                        AddSceneObjectsRecursive(child.gameObject);
                    }
                }
            }
        }
    }
    #endregion
}
