using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;
using UnityExtras;

public class SceneTrigger : MonoBehaviour
{
    [Serializable]
    public struct SceneWrapper
    {
        public SceneReference sceneReference;
        public LoadMode loadMode;
        public LoadSceneMode loadSceneMode;
    }

    public enum LoadMode
    {
        Load,
        LoadAsync,
        UnloadAsync
    }

    [Tag] public string playerTag;
    public List<SceneWrapper> scenes;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        foreach (var sceneWrapper in scenes)
        {
            switch (sceneWrapper.loadMode)
            {
                case LoadMode.Load:
                    SceneManager.LoadScene(sceneWrapper.sceneReference.path, sceneWrapper.loadSceneMode); 
                    break;
                case LoadMode.LoadAsync:
                    SceneManager.LoadSceneAsync(sceneWrapper.sceneReference.path, sceneWrapper.loadSceneMode);
                    break;
                case LoadMode.UnloadAsync:
                    SceneManager.UnloadSceneAsync(sceneWrapper.sceneReference.path);
                    break;
            }
        }
    }
}
