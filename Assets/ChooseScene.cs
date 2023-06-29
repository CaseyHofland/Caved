using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;

public class ChooseScene : MonoBehaviour
{
    InventorySystem inventorySystem;
    countingMemories _allSeen;
    public EmMovement _blockControls;
    PlayableDirector _finalPlayableDirectorRemember;
    PlayableDirector _finalplayableDirectorForget;
    public GameObject _ZimsThings;

    public SceneReference _Remember;
    public SceneReference _Forget;

    public GameObject _decalRemember;
    public GameObject _decalForget;

    // Start is called before the first frame update
    void Start()
    {
        inventorySystem = FindObjectOfType<InventorySystem>();

        if(inventorySystem._hurtCanTrigger)
        {
            _decalRemember.SetActive(true);
            Destroy(_decalForget);
        }
        else
        {
            _decalForget.SetActive(true);
            Destroy(_decalRemember);
        }

        _blockControls = gameObject.GetComponent<EmMovement>();
    }

    private void Update()
    {
        if(_allSeen._timeForCutscene)
            StartTheCutscene();
    }

    void StartTheCutscene()
    {
        _blockControls.enabled = false;
        if (inventorySystem._hurtCanTrigger)
            _finalPlayableDirectorRemember.Play();
        else
            _finalplayableDirectorForget.Play();
    }

    public void FinaleTriggerd()
    {
        if(inventorySystem._hurtCanTrigger)
        {
            SceneManager.LoadSceneAsync(_Remember, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(_Forget, LoadSceneMode.Single);
        }
    }

    /*void EndingRemember()
    {
        SceneManager.LoadSceneAsync(_Remember, LoadSceneMode.Single);
    }

    void EndingForget()
    {
        SceneManager.LoadSceneAsync(_Forget, LoadSceneMode.Single);
    }*/
}
