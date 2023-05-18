using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityExtras;
using Input = UnityExtras.InputSystem.Input;

namespace Sinventory
{
    [Obsolete("Sinventory is bad & rigid, don't use it outside the A Dark Fairytale project.", false)]
    public class ItemTrigger : MonoBehaviour
    {
        public bool checkTag;
        [Tag] public string collisionTag;
        public Input pickupInput;
        public ItemType type;

        private void OnTriggerEnter(Collider other)
        {
            if (!checkTag || other.CompareTag(collisionTag))
            {
                pickupInput.action.performed += OnPickUp;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!checkTag || other.CompareTag(collisionTag))
            {
                pickupInput.action.performed -= OnPickUp;
            }
        }

        private void OnPickUp(InputAction.CallbackContext context) => PickUp();

        public void PickUp()
        {
            PlayerInventory.AddItem(type);
            Destroy(gameObject);
        }
    }
}
