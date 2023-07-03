using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;
using UnityExtras;

public class NextScene : MonoBehaviour
{
    /*public SceneReference _scene;

    public void LoadNextScene()
    {
        SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single);
    }*/

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

    public void LoadNextScene()
    {
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
