using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sinventory
{
    [Obsolete("Sinventory is bad & rigid, don't use it outside the A Dark Fairytale project.", false)]
    public static class PlayerInventory
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            itemCounts.Clear();
        }

        public static Dictionary<ItemType, int> itemCounts = new();

        //public static event Action<ItemType> itemAdded;
        //public static event Action<ItemType> itemRemoved;

        public static void AddItem(ItemType itemType)
        {
            itemCounts.TryAdd(itemType, 0);
            itemCounts[itemType]++;

            //itemAdded?.Invoke(itemType);
        }

        public static void RemoveItem(ItemType itemType)
        {
            if (!itemCounts.ContainsKey(itemType))
            {
                return;
            }

            if (--itemCounts[itemType] == 0)
            {
                itemCounts.Remove(itemType);
            }
            //itemRemoved?.Invoke(itemType);
        }

        public static void Craft(ItemType first, ItemType second)
        {
            if ((first | second) == (ItemType.Stick | ItemType.StoneTip)) 
            {
                RemoveItem(first);
                RemoveItem(second);

                AddItem(ItemType.Spear);
            }
        }
    }
}

