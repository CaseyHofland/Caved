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
    [SerializeField]
    InventoryItemSO _memory;
    [SerializeField]
    UnityEvent _event;
    bool _pickedUp;
    [SerializeField]
    private GameObject _worldSpacePickup;
    public CinemachineCamera _memoryCamera;
    
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
        if (_event != null)
        {
            _event.Invoke();
        }
    }

    private void OnConfirm()
    {
        if(_triggerd)
            StartCoroutine(RememberChoice());
    }

    private IEnumerator RememberChoice()
    {
        _memoryCamera.enabled = true;

        yield return new WaitForSeconds(_thinkingTime);

        _choices.SetActive(true);
    }



}
