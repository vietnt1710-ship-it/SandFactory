using data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "GameData/Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Level Data")]
        public List<PicData> levels;

        [Header("Item")]
        public ItemData items;

        [Header("ShopPack")]
        public List<IAPData> goldPacks ;
    }
}

