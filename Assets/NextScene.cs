using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;

public class NextScene : MonoBehaviour
{
    public SceneReference _scene;
    public void LoadNextScene()
    {
        SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Single);
    }
}
