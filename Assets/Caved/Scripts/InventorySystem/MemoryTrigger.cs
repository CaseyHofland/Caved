using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class MemoryTrigger : MonoBehaviour
{
    private bool _triggerd = false;

    InventorySystem _inventoryManager;
    countingMemories _seenMemoriesCheck;

    [SerializeField] InventoryItemSO _memory;
    [SerializeField] UnityEvent _event;
    bool _pickedUp;
    [SerializeField] private GameObject _worldSpacePickup;
    public CinemachineCamera _memoryCamera;
    public ParticleSystem _memorylights;
    public bool _lookClicked;
    
    EmInput _playerInputMemory;
    
    [SerializeField] private float _thinkingTime;
    private bool _isInRange;
    public GameObject _choices;

    public DecalProjector _projector;


    private void Awake()
    {
        _inventoryManager = FindObjectOfType<InventorySystem>();
        _worldSpacePickup.SetActive(false);
        _choices.SetActive(false);
        _seenMemoriesCheck = FindObjectOfType<countingMemories>();

        _memoryCamera.enabled = false;
        
        _playerInputMemory = new EmInput();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(_worldSpacePickup != null && !_pickedUp)
            {
                _worldSpacePickup.SetActive(true);
            }
            _triggerd = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (_worldSpacePickup != null && !_pickedUp)
            {
                _worldSpacePickup.SetActive(false);
            }
            _triggerd = false;
        }
    }

    private void Update()
    {
        if (_pickedUp)
        {
            _worldSpacePickup.SetActive(false);
        }
    }

    public void Remember()
    {
        if (!_pickedUp && _memory != null && _triggerd)
        {
            _inventoryManager.AddItemToSavedMemories(_memory.Id);
            _seenMemoriesCheck._count++;
            _inventoryManager.PositiveMemoriesScore += _memory.PositiveScore;
            _inventoryManager.NegativeMemoriesScore += _memory.NegativeScore;
            _pickedUp = true;
            if (_event != null)
            {
                _event.Invoke();
            }
        }
    }

    public void Forget()
    {
        _projector.enabled = false;
        _seenMemoriesCheck._count++;
        _memorylights.Stop();
        if (_event != null)
        {
            _event.Invoke();
        }
    }

    /*private void OnConfirm()
    {
        if(_triggerd && !_pickedUp)
            StartCoroutine(RememberChoice());
    }*/

    public void LookClicked()
    {
        Debug.Log(" Click");
        if (_triggerd && !_pickedUp)
            StartCoroutine(RememberChoice());
    }

    private IEnumerator RememberChoice()
    {
        if (_triggerd && !_pickedUp)
        {
            _lookClicked = true;
            _memoryCamera.enabled = true;

            yield return new WaitForSeconds(_thinkingTime);

            _choices.SetActive(true);
            _lookClicked = false;
        }
        else
        {
            yield break;
        }
    }



}
