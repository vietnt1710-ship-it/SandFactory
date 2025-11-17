using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace data
{
    [CreateAssetMenu(fileName = "_", menuName = "GameData/IAP")]
    public class IAPData : ScriptableObject
    {
        [Serializable]
        public struct ItemWithValue
        {
            public ItemID item;
            public int value;
        }
        public string packID;
        public bool isNoAdsPack = false;
        public string namePack;
        public Sprite nameSprite;
        public Sprite mainSprite;
        public string description;
        public float price;

        public List<ItemWithValue> items;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.name == $"IAP {namePack}") return;

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath) && !string.IsNullOrEmpty(namePack))
            {
                string desiredName = $"IAP {namePack}";
                string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                if (currentName != desiredName)
                {
                    string newPath = assetPath.Replace(currentName + ".asset", desiredName + ".asset");
                    string result = UnityEditor.AssetDatabase.RenameAsset(assetPath, desiredName);

                    if (!string.IsNullOrEmpty(result))
                    {
                        Debug.LogWarning($"Cannot rename asset: {result}");
                    }
                }
            }
        }
#endif
    }
}

