using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace data
{
    [CreateAssetMenu(fileName = "Item_", menuName = "GameData/Item")]
    public class ItemData : ScriptableObject
    {
       public List<Item> datas;

        public Item GetItemByID(ItemID idx)
        {
            return datas.FirstOrDefault(d => d.id == idx);
        }
    }

    [Serializable]
    public class Item
    {
        public ItemID id;
        public Sprite itemSprite;
        public Action<int> OnItemChanged;

        private const string Item_Source = "Storge";

        public int Value
        {
            get
            {
                string source = $"{Item_Source}_{id}";
                return PlayerPrefs.GetInt(source, 0);
            }
            set
            {
                if(id == ItemID.unlimited_Hearts)
                {
                    OnItemChanged?.Invoke(value);
                }
                else
                {
                    string source = $"{Item_Source}_{id}";

                    int count = PlayerPrefs.GetInt(source, 0);
                    count += value;

                    Debug.Log($"On Item Changed {id} : {value}");

                    PlayerPrefs.SetInt(source, count);

                    OnItemChanged?.Invoke(value);
                }
                
            }
        }
    }

    [Serializable]
    public enum ItemID
    {
        coin,
        heart,
        unlimited_Hearts,
        item1,
        item2,
        item3,
    }
}

