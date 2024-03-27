
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Scripts.Databases.Items
{
    public class ItemDatabase : MonoBehaviour
    {
        #region Private Fields
        
        [Header("Database Content")]
        [SerializeField] private List<Item> items;
        
        #endregion
        
        #region Public Methods

        public Item GetItemById(int itemId) => items[itemId];
        public Item GetItemByName(string itemName) => items.First((item) => item.itemName == itemName);

        public int GetIdByItem(Item item)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].itemName == item.itemName) return i;
            }
            return -1;
        }

        #endregion
    }
}