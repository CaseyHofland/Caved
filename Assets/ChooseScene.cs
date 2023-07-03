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

    public void StartTheCutscene()
    {
        _blockControls.enabled = false;
        if (inventorySystem._hurtCanTrigger)
            _finalPlayableDirectorRemember.Play();
        else
            _finalplayableDirectorForget.Play();
    }



    public void FinaleTriggerd()
    {
        StartCoroutine(LoadYourAsyncScene());
    }

    IEnumerator LoadYourAsyncScene()
    {
        if (inventorySystem._hurtCanTrigger)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Caved/Areas/FinaleSad");

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Caved/Areas/Peace");

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        
    }
}
