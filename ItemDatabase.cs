using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "RoomBox/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;
}
