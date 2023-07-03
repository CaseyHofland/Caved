using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompEscapeMenu : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private Button _btnRemumeGame;
    //[SerializeField] private Button _btnMemoriesGame;
    [SerializeField] private Button _btnSettingsGame;
    [SerializeField] private Button _btnExitGame;

    [Header("Pop up 2 choices")]
    [SerializeField] private GameObject _popUpTwoChoices;
    [SerializeField] private TMP_Text _txtPopUpTwo;
    [SerializeField] private Button _btnAccept;
    [SerializeField] private Button _btnDecline;

    [Header("Menus")]
    [SerializeField] private GameObject _menuSettings;
    //[SerializeField] private GameObject _memoryShow;

    private const string _exitGame = "Are you sure you want to exit the game?";

    //Handle exit game pop up
    private void HandleExitGame(string text)
    {
        _popUpTwoChoices.SetActive(true);
        _txtPopUpTwo.text = text;
        _btnAccept.onClick.RemoveAllListeners();
        _btnDecline.onClick.RemoveAllListeners();

        _btnAccept.onClick.AddListener(delegate {Application.Quit(); });
        _btnDecline.onClick.AddListener(delegate { _popUpTwoChoices.SetActive(false); });
    }

    //Handle all pop ups
    private void PopUp(string action)
    {
        switch(action)
        {
            case "Exit":
                HandleExitGame(_exitGame);
                break;

            default:
                break;
        }
    }

    //open settings
    private void Settings()
    {
        _menuSettings.SetActive(true);
    }

    //Resume game
    private void Resume()
    {
        gameObject.SetActive(false);
    }

    //show memories
    /*private void Memory()
    {
        _memoryShow.SetActive(true);
    }*/

    //Assign button Events
    private void AssignButtonEvents()
    {
        _btnRemumeGame.onClick.AddListener(delegate { Resume(); });
        _btnSettingsGame.onClick.AddListener(delegate { Settings(); });
        _btnExitGame.onClick.AddListener(delegate { PopUp("Exit"); });
        //_btnMemoriesGame.onClick.AddListener(delegate { Memory(); });
    }

    private void Start()
    {
        AssignButtonEvents();
    }

}
