using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sinventory
{
    [Obsolete("Sinventory is bad & rigid, don't use it outside the A Dark Fairytale project.", false)]
    public class UIInventory : MonoBehaviour
    {
        public UIItem uiItem;

        private void Update()
        {
            var uiItems = GetComponentsInChildren<UIItem>();

            foreach (var item in PlayerInventory.itemCounts)
            {
                foreach (var uiItem in uiItems)
                {

                }
            }
        }
    }
}