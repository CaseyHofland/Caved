using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Screens")]
    public GameObject _startScreen;
    public GameObject _mainScreen;
    public GameObject _optionsScreen;
    public GameObject _controlsInfo;

    [Header("Conditions")]
    [SerializeField] private float _setupTime = 2.5f;
    [SerializeField] private bool _startScreenFinished;
    [SerializeField] private bool _readytoStart;
    private bool _mainActive;
    private bool _optionsActive;

    [Header("Opening Settings")]
    public Button _mainPrimaryButton;
    public Slider _settingsPrimaryButton;

    EmInput _menuNaviator;

    [Header("Animations")]
    public Animator _startScreenAnimations;


    private void Start()
    {
        _optionsScreen.SetActive(false);
        _mainScreen.SetActive(false);
        _controlsInfo.SetActive(false);
        _menuNaviator = new EmInput();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(_setupTime);

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowControls()
    {
        _optionsScreen.SetActive(false);
        _mainScreen.SetActive(false);
        _controlsInfo.SetActive(true);
        _readytoStart = true;

    }

    public void ShowOptions()
    {
        _mainScreen.SetActive(false);
        _optionsScreen.SetActive(true);
        _settingsPrimaryButton.Select();
        _mainActive = false;
        _optionsActive= true;
    }

    public void BackToMain()
    {
        _mainScreen.SetActive(true);
        _optionsScreen.SetActive(false);
        _mainPrimaryButton.Select();
        _mainActive = true;
        _optionsActive= false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }

    //TRANSITIONS
    private IEnumerator startToMain()
    {
        _startScreenAnimations.SetTrigger("isClicked");

        yield return new WaitForSeconds(2f);

        _startScreen.SetActive(false);

        BackToMain();
    }

    //INPUT
    void OnConfirm()
    {
        Debug.Log("Confirm pressed");
        if(!_startScreenFinished)
        {
            _startScreenFinished = true;
            StartCoroutine(startToMain());
        }
        else if(_readytoStart) 
        { 
            StartCoroutine(StartGame());  
        }
    }

    void OnBack()
    {
        if(_optionsActive)
        {
            BackToMain();
        }
        else if(_mainActive)
        {
            Debug.Log("Request quit");
        }
    }


}
