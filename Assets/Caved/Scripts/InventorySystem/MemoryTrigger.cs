using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    private void Awake()
    {
        _inventoryManager = FindObjectOfType<InventorySystem>();
        _worldSpacePickup.SetActive(false);
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

        if (!_pickedUp && _memory != null && _triggerd && Input.GetKeyDown(KeyCode.F))
        {
            _inventoryManager.AddItemToSavedMemories(_memory.Id);
            _pickedUp = true;
            if (_event != null)
            {
                _event.Invoke();
            }
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            SceneManager.LoadScene(2);
        }
    }
}
