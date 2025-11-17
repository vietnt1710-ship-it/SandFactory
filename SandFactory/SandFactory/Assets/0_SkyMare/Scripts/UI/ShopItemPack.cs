using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using data;

public class ShopItemPack : ShopItem
{  
    public Image packName;
    public TMP_Text description;
    public TMP_Text price;
    public List<Item> items;


    public override void LoadPack()
    {
        if (packIcon != null)
        {
            packIcon.sprite = datas.mainSprite;
            packIcon.SetNativeSize();
        }
      
        if(packName != null)
        {
            packName.sprite = datas.nameSprite;
            packName.SetNativeSize();
        }
       

        if(description!= null)
        {
            description.text = datas.description;
        }
        price.text = $"{datas.price}$";

        for (int i = 0; i < datas.items.Count; i++)
        {
            var dataItem = datas.items[i];
            var uiItem = items[i];

            uiItem.value.text = dataItem.value.ToString() + (dataItem.item == data.ItemID.unlimited_Hearts ? "h" : "");
            if (uiItem.ic != null)
            {
                uiItem.ic.sprite = GameManger.I.datas.items.GetItemByID(dataItem.item).itemSprite;
                uiItem.ic.SetNativeSize();
            }
           
        }
    }
}
