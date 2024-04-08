
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Scripts.Databases.Items
{
    [CreateAssetMenu()]
    public class Item : ScriptableObject
    {
        [Header("Item Metadata")] 
        public string itemName;
        public string itemDescription;
        public ItemCategory itemCategory;
        
        [Header("Item Graphics")]
        public Sprite itemRender;
        
        [Header("Shop Metadata")]
        public int priceInShop;
        public bool itemOwned = false;
    }

    public class ItemComponent : MonoBehaviour
    {
        [Header("Item Metadata")] 
        public string itemName;
        public string itemDescription;
        public ItemCategory itemCategory;
        
        [Header("Item Graphics")]
        public Sprite itemRender;
        
        [Header("Shop Metadata")]
        public int priceInShop;
        public bool itemOwned = false;
    }

    public enum ItemCategory
    {
        Head = 0,
        Top = 1,
        Shoes = 2,
        Hair = 3,
        Bottom = 4,
        Accessory = 5,
        Banner = 6,
    }
}