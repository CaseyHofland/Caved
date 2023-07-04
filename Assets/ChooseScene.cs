using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;

public class ChooseScene : MonoBehaviour
{
    InventorySystem inventorySystem;
    RoadBlock _allSeen;
    public EmMovement _blockControls;
    public PlayableDirector _finalPlayableDirectorRemember;
    public PlayableDirector _finalplayableDirectorForget;

    public GameObject _ZimsThings;

    public GameObject _camRemember;
    public GameObject _camForget;

    public SceneReference _Remember;
    public SceneReference _Forget;

    public GameObject _decalRemember;
    public GameObject _decalForget;
    public Animator _crossfadePanel;

    // Start is called before the first frame update
    void Start()
    {
        inventorySystem = FindObjectOfType<InventorySystem>();

        if (inventorySystem._hurtCanTrigger)
        {
            _decalRemember.SetActive(true);
            Destroy(_decalForget);
        }
        else
        {
            _decalForget.SetActive(true);
            _ZimsThings.SetActive(false);
            Destroy(_decalRemember);
        }

        _blockControls = _blockControls.GetComponent<EmMovement>();
    }

    public void StartTheCutscene()
    {
        _blockControls.enabled = false; //!
        //_crossfadePanel.Play("A_CrossfadeStart");
        if (inventorySystem._traumaCanTrigger)
        {
            _camRemember.SetActive(true);
            _finalPlayableDirectorRemember.Play();
        }
        else
        {
            _camForget.SetActive(true);
            _finalplayableDirectorForget.Play();
        }
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

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Caved/Areas/S_FinaleSad");

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

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Caved/Areas/S_Peace");

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

    }
}
