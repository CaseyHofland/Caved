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
    [SerializeField] private Animator _panel;

    public Button _btnNext;

    [SerializeField] private TMP_Text _txtNext;

    private bool _canClick;

    private int _ind = 0;


    // Start is called before the first frame update
    void Start()
    {
        _txtNext.text = "Next";
        //_line1.SetActive(true);
        _canClick = true;
    }

    private void Update()
    {
        Debug.Log(_ind);
    }

    public void StartExplanation()
    {
        _panel.Play("A_CrossfadeStart");
        _line1.SetActive(true);
        StartCoroutine(countingForClick());
        Debug.Log("Showing 1");
    }

    public void ShowNextLine()
    {
        _ind++;
        Debug.Log("Click");
        if(_canClick)
        {
            if (_ind == 1)
            {
                _line2.SetActive(true);
                StartCoroutine(countingForClick());
                Debug.Log("Showing 2");
            }
            else if (_ind == 2)
            {
                _line3.SetActive(true);
                StartCoroutine(countingForClick());
                Debug.Log("Showing 3");
            }
            else if (_ind == 3)
            {
                _txtNext.text = "Start";
                _line4.SetActive(true);
                StartCoroutine(countingForClick());
                Debug.Log("Showing 4");

            }
            if (_ind == 4)
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
        
        _panel.Play("A_FastDipEnd");

        Destroy(gameObject);
    }
}
