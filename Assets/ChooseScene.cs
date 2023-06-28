using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;

public class ChooseScene : MonoBehaviour
{
    InventorySystem inventorySystem;
    public SceneReference _Remember;
    public SceneReference _Forget;

    // Start is called before the first frame update
    void Start()
    {
        inventorySystem = FindObjectOfType<InventorySystem>();
    }

    void FinaleTriggerd(Scene scene)
    {
        if(inventorySystem.IsInTrauma())
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
