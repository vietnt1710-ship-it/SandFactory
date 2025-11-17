using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ShopItemCoin : ShopItem
{
    public Image adsIcon;
    public TMP_Text value;
    public TMP_Text price;


    public override void LoadPack()
    {
        packIcon.sprite = datas.mainSprite;
        packIcon.SetNativeSize();

        if (datas.price != 0)
        {
            adsIcon.gameObject.SetActive(false);
            price.gameObject.SetActive(true);
            price.text = $"{datas.price}";
        }
        else
        {
          
            adsIcon.gameObject.SetActive(true);
            price.gameObject.SetActive(false);
        }
        
        
        var dataItem = datas.items[0];

        value.text = dataItem.value.ToString() ;

    }
}
