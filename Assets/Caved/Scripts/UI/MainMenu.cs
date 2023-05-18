using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Screens")]
    public GameObject _mainScreen;
    public GameObject _optionsScreen;

    [Header("Conditions")]
    [SerializeField] private float _setupTime = 2.5f;

    [Header("Opening Settings")]
    public Button _mainPrimaryButton;
    public Slider _settingsPrimaryButton;

    PlayerInput _menuNaviator;

    private void Start()
    {
        _optionsScreen.SetActive(false);
        _mainPrimaryButton.Select();
        _menuNaviator = new PlayerInput();
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowOptions()
    {
        _mainScreen.SetActive(false);
        _optionsScreen.SetActive(true);
        _settingsPrimaryButton.Select();
    }

    public void BackToMain()
    {
        _mainScreen.SetActive(true);
        _optionsScreen.SetActive(false);
        _mainPrimaryButton.Select();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }
}
