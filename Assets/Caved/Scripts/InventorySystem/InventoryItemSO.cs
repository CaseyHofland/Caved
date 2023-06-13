using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MemoryObject", menuName = "Memories/New Memory", order = 1)]
public class InventoryItemSO : ScriptableObject
{
    public int Id;
    public Sprite MemoryImage;
}
