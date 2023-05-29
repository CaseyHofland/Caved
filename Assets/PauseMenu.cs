using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    EmInput _menuNavigation;

    public Button _primaryButton;


    public static bool _gameIsPaused = false;
    private bool _showingMain;
    //private bool _buttonPressed = false;

    public GameObject _pauseMenuUI;
    public GameObject _mainPause;
    public GameObject _pauseSettings;

    private void Awake()
    {
        _menuNavigation = new EmInput();
        _pauseMenuUI.SetActive(false);
    }

    public void Resume()
    {
        _pauseMenuUI.SetActive(false);
        _showingMain = false;
        Time.timeScale = 1f;
        _gameIsPaused = false;
    }

    public void ShowPauseSettings()
    {
        _mainPause.SetActive(false);
        _pauseSettings.SetActive(true);
        _showingMain = false;
        //_settingsPrimaryButton.Select();
        //_mainActive = false;
        //_optionsActive = true;
    }

    public void BackToMain()
    {
        _mainPause.SetActive(true);
        _pauseSettings.SetActive(false);
        _showingMain = true;
        //_mainPrimaryButton.Select();
        //_mainActive = true;
        //_optionsActive = false;
    }

    public IEnumerator Pause()
    {
        _pauseMenuUI.SetActive(true);
        
        BackToMain();

        yield return new WaitForSeconds(.5f);

        _primaryButton.Select();
        Time.timeScale = 0f;
        _gameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("S_MainMenu");
    }

    void OnBack()
    {
        if (_gameIsPaused)
        {
            Resume();
        }
        else
        {
            StartCoroutine(Pause());
        }
    }
}
