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
    
    [Header("Memory conditions")]
    [SerializeField] private int _traumaInt;
    public static int _memoryState = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

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

    public void OpenMemories()
    {
        if (_coroutine == null)
        {
            _target.SetActive(true);
            _coroutine = StartCoroutine(LoadMemoriesUI());
        }
    }

    void OnBack()
    {
        if(_showingMemories)
            _target.SetActive(false); _showingMemories = false;
    }

    void OnMemories()
    {
        if(!_showingMemories)
            OpenMemories();
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
