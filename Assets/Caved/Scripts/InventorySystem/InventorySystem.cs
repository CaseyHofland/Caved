using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    EmInput _memoryControls;
    
    [Header("Memory lists")]
    public List<InventoryItemSO> AllMemories = new List<InventoryItemSO>(); 
    public List<InventoryItemSO> SavedMemories = new List<InventoryItemSO>();
    public List<GameObject> SpawnedImages = new List<GameObject>();
    [Header("Memory count")]
    public int PositiveMemoriesScore;
    public int NegativeMemoriesScore;

    [Header("UI")]
    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _prefab;
    private Coroutine _coroutine;
    private bool _showingMemories;

    [Header("Menus")]
    [SerializeField] private GameObject _goEscapeMenu;
    [SerializeField] private GameObject _goSettingsMenu;
    [SerializeField] private GameObject _goMemoriesMenu;
    [SerializeField] private List<PopUpButton> _goPopUps;

    
    [Header("Memory conditions")]
    [SerializeField] private int _traumaInt;
    public static int _memoryState = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    //Collecting Memories
    void Start()
    {
        AllMemories = Resources.FindObjectsOfTypeAll<InventoryItemSO>().ToList();
        _memoryControls = new EmInput();
    }

    public bool IsInTrauma()
    {
        if(NegativeMemoriesScore > PositiveMemoriesScore)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddItemToSavedMemories(int id)
    {
        if (SavedMemories.Where(x => x.Id == id).Any())
            return;
        else
        {
            if(AllMemories.Where(x => x.Id == id).Any())
            {
                InventoryItemSO tempItem = AllMemories.Where(x => x.Id == id).FirstOrDefault();
                SavedMemories.Add(tempItem);
                _memoryState = _memoryState + 1;
            }
        }
    }


    //UI
    public void OpenMemories()
    {
        if (_coroutine == null)
        {
            _goMemoriesMenu.SetActive(true); //new
            _target.SetActive(true);
            _coroutine = StartCoroutine(LoadMemoriesUI());
        }
    }

    void OpenMenu()
    {
        OpenMemories();
    }

    void CloseMenu()
    {
        bool _popUp = false;

        foreach(PopUpButton item in _goPopUps)
        {
            if(item._popUp.activeSelf)
            {
                item._btnDecline.onClick.Invoke();
                _popUp = true;
            }
        }

        if(!_popUp)
        {
            if(_goSettingsMenu.activeSelf)
            {
                _goSettingsMenu.SetActive(false);
            }
            else if(_goEscapeMenu.activeSelf)
            {
                _goEscapeMenu.SetActive(false);
            }
            else
            {
                _goMemoriesMenu.SetActive(false);
            }
        }
    }


    public void OnBack()
    {
        if (_showingMemories)
        {
            _target.SetActive(false);
            _goMemoriesMenu.SetActive(false); //new
            _showingMemories = false;
        }
    }

    public void OnMemories() //OpenMenu
    {
        Debug.Log("CLICK");
        /*if(!_showingMemories)
            OpenMemories();
        else
        {
            _target.SetActive(false); 
            _showingMemories = false;
        }*/
        if (!_showingMemories)
            OpenMemories();
        else
        {
            CloseMenu();
        }

    }

    private IEnumerator LoadMemoriesUI()
    {
        _showingMemories = true;

        if (SavedMemories.Count > 0)
        {
            for (int i = 0; i < SavedMemories.Count; i++)
            {
                if(i < SpawnedImages.Count)
                {
                    SpawnedImages[i].GetComponent<Image>().sprite = SavedMemories[i].MemoryImage;
                }
                else
                {
                    GameObject temp = Instantiate(_prefab, _target.transform);
                    temp.GetComponent<Image>().sprite = SavedMemories[i].MemoryImage;
                    SpawnedImages.Add(temp);
                }
            }
            yield break;
        }
        else
            yield break;
    }
}

[Serializable]
public class PopUpButton
{
    public GameObject _popUp;
    public Button _btnDecline;
}
