using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZanaStoryUI : MonoBehaviour
{
    [SerializeField] private GameObject _line1;
    [SerializeField] private GameObject _line2;
    [SerializeField] private GameObject _line3;
    [SerializeField] private GameObject _line4;

    public Button _btnNext;

    [SerializeField] private TMP_Text _txtNext;

    private bool _canClick;

    private int _ind = 0;


    // Start is called before the first frame update
    void Start()
    {
        _txtNext.text = "Next";
        _line1.SetActive(true);
        _canClick = true;
    }

    public void ShowNextLine()
    {
        if(_canClick)
        {
            if (_ind == 0)
            {
                _line2.SetActive(true);
                StartCoroutine(countingForClick());
                _ind = 1;
                Debug.Log("Showing 2");
            }
            else if (_ind == 1)
            {
                _line3.SetActive(true);
                StartCoroutine(countingForClick());
                _ind = 2;
                Debug.Log("Showing 3");
            }
            else if (_ind == 2)
            {
                _txtNext.text = "Start";
                _line4.SetActive(true);
                StartCoroutine(countingForClick());
                _ind = 3;
                Debug.Log("Showing 4");

            }
            if (_ind == 3)
            {
                StartCoroutine(removingLines());
                Debug.Log("Closing");
            }

        }
        
    }

    IEnumerator countingForClick()
    {
        _canClick = false;
        _btnNext.interactable = false;
        yield return new WaitForSeconds(3);
        _canClick = true;
        _btnNext.interactable = true;
    }
    IEnumerator removingLines()
    {
        Debug.Log("Closing");
        
        _btnNext.enabled = false;

        yield return new WaitForEndOfFrame();

        Destroy(gameObject);
    }
}
