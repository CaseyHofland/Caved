using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public List<InventoryItemSO> AllMemories = new List<InventoryItemSO>(); 
    public List<InventoryItemSO> SavedMemories = new List<InventoryItemSO>();
    public List<GameObject> SpawnedImages = new List<GameObject>();

    [Header("UI")]
    [SerializeField]
    private GameObject _target;
    [SerializeField]
    private GameObject _prefab;
    private Coroutine _coroutine;
    // Start is called before the first frame update

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        AllMemories = Resources.FindObjectsOfTypeAll<InventoryItemSO>().ToList();
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
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            OpenMemories();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _target.SetActive(false);
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

    private IEnumerator LoadMemoriesUI()
    {
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
